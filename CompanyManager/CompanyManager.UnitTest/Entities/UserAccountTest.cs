using CompanyManager.Domain.Entities;
using CompanyManager.Domain.AccessControl;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.UnitTest.Entities
{
    public class UserAccountTest
    {
        [Fact(DisplayName = "Should create active account with normalized username and password hash")]
        public void Create()
        {
            var roleId = Guid.NewGuid();
            var acc = UserAccount.Create(
                userName: "USER@Example.com",
                passwordHash: "hash123",
                employeeId: Guid.NewGuid(),
                roleId: roleId);

            acc.UserName.Should().Be("user@example.com");
            acc.PasswordHash.Should().Be("hash123");
            acc.IsActive.Should().BeTrue();
            acc.AccessFailedCount.Should().Be(0);
            acc.SecurityStamp.Should().NotBe(Guid.Empty);
            acc.RoleId.Should().Be(roleId);
        }

        [Fact(DisplayName = "Should change password updating PasswordChangedAt and SecurityStamp")]
        public void ChangePassword()
        {
            var acc = FakeUser();
            var beforeStamp = acc.SecurityStamp;
            var beforeDate = acc.PasswordChangedAt;

            acc.SetPasswordHash("new-hash");

            acc.PasswordHash.Should().Be("new-hash");
            acc.SecurityStamp.Should().NotBe(beforeStamp);
            acc.PasswordChangedAt.Should().BeAfter(beforeDate);
        }

        [Fact(DisplayName = "Should lockout after N failed attempts and unlock afterwards")]
        public void Lockout()
        {
            var acc = FakeUser();
            for (var i = 0; i < 5; i++)
                acc.IncrementAccessFailedCount();

            acc.AccessFailedCount.Should().Be(5);

            acc.Lockout(DateTime.UtcNow.AddMinutes(15));
            acc.IsLockedOut.Should().BeTrue();

            acc.Unlock();
            acc.IsLockedOut.Should().BeFalse();
            acc.AccessFailedCount.Should().Be(0);
        }

        [Fact(DisplayName = "Should reset failure count after successful login")]
        public void ResetFailuresOnSuccess()
        {
            var acc = FakeUser();
            acc.IncrementAccessFailedCount();
            acc.ResetAccessFailedCount();
            acc.AccessFailedCount.Should().Be(0);
        }

        [Fact(DisplayName = "Should activate and deactivate account")]
        public void ActivateDeactivate()
        {
            var acc = FakeUser();
            acc.Deactivate();
            acc.IsActive.Should().BeFalse();
            acc.Activate();
            acc.IsActive.Should().BeTrue();
        }

        [Fact(DisplayName = "Should enable and disable 2FA")]
        public void TwoFactor()
        {
            var acc = FakeUser();
            acc.EnableTwoFactor("encrypted-secret");
            acc.TwoFactorEnabled.Should().BeTrue();
            acc.DisableTwoFactor();
            acc.TwoFactorEnabled.Should().BeFalse();
        }

        [Fact(DisplayName = "Should set role")]
        public void SetRole()
        {
            var acc = FakeUser();
            var newRoleId = Guid.NewGuid();
            
            acc.SetRole(newRoleId);
            acc.RoleId.Should().Be(newRoleId);
        }

        [Fact(DisplayName = "Should validate permissions with role")]
        public void ValidatePermissionsWithRole()
        {
            var acc = FakeUser();
            var role = new Role("Manager", HierarchicalRole.Manager);
            
            acc.HasPermission(role, "employees:read").Should().BeTrue();
            acc.HasPermission(role, "departments:write").Should().BeTrue();
            acc.HasPermission(role, "invalid:permission").Should().BeFalse();
        }

        [Fact(DisplayName = "Should validate SuperUser permissions")]
        public void ValidateSuperUserPermissions()
        {
            var acc = FakeUser();
            var superUserRole = new Role("SuperUser", HierarchicalRole.SuperUser);
            
            acc.IsSuperUser(superUserRole).Should().BeTrue();
            acc.HasPermission(superUserRole, "any:permission").Should().BeTrue();
        }

        [Fact(DisplayName = "Should validate role level")]
        public void ValidateRoleLevel()
        {
            var acc = FakeUser();
            var role = new Role("Senior", HierarchicalRole.Senior);
            
            acc.GetRoleLevel(role).Should().Be(HierarchicalRole.Senior);
        }

        [Fact(DisplayName = "Should validate role creation permissions")]
        public void ValidateRoleCreationPermissions()
        {
            var acc = FakeUser();
            var managerRole = new Role("Manager", HierarchicalRole.Manager);
            var juniorRole = new Role("Junior", HierarchicalRole.Junior);
            
            acc.CanCreateRole(managerRole, HierarchicalRole.Junior).Should().BeTrue();
            acc.CanCreateRole(managerRole, HierarchicalRole.Pleno).Should().BeTrue();
            acc.CanCreateRole(managerRole, HierarchicalRole.Senior).Should().BeTrue();
            acc.CanCreateRole(managerRole, HierarchicalRole.Manager).Should().BeTrue();
            acc.CanCreateRole(managerRole, HierarchicalRole.Director).Should().BeFalse();
        }

        private static UserAccount FakeUser() =>
            UserAccount.Create("user@example.com", "hash", Guid.NewGuid(), Guid.NewGuid());
    }
}
