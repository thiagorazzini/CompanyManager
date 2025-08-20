using CompanyManager.Domain.Entities;
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
            var acc = UserAccount.Create(
                userName: "USER@Example.com",
                passwordHash: "hash123",
                employeeId: Guid.NewGuid());

            acc.UserName.Should().Be("user@example.com");
            acc.PasswordHash.Should().Be("hash123");
            acc.IsActive.Should().BeTrue();
            acc.AccessFailedCount.Should().Be(0);
            acc.SecurityStamp.Should().NotBe(Guid.Empty);
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
                acc.RecordFailedLoginAttempt(maxAttempts: 5, lockoutFor: TimeSpan.FromMinutes(15));

            acc.IsLockedOut.Should().BeTrue();

            acc.UnlockNow();
            acc.IsLockedOut.Should().BeFalse();
            acc.AccessFailedCount.Should().Be(0);
        }

        [Fact(DisplayName = "Should reset failure count after successful login")]
        public void ResetFailuresOnSuccess()
        {
            var acc = FakeUser();
            acc.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15));
            acc.ResetFailuresAfterSuccessfulLogin();
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

        private static UserAccount FakeUser() =>
            UserAccount.Create("user@example.com", "hash", Guid.NewGuid());
    }
}
