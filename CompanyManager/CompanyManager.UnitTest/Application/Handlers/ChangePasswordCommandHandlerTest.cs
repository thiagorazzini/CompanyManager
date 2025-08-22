using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Xunit;

using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Services;           // PasswordHasher
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryUserAccountRepository

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class ChangePasswordCommandHandlerTest
    {
        private static UserAccount NewUser(string email, string rawPassword)
        {
            var hasher = new PasswordHasher();
            var hash = hasher.Hash(rawPassword);

            return UserAccount.Create(
                userName: email.Trim().ToLowerInvariant(),
                passwordHash: hash,
                employeeId: Guid.NewGuid(),
                roleId: Guid.NewGuid(),
                jobTitleId: Guid.NewGuid()
            );
        }

        private static IValidator<ChangePasswordRequest> CreateValidator() => 
            new ChangePasswordRequestValidator();

        [Fact(DisplayName = "Should change password, rotate stamp and set UpdatedAt")]
        public async Task Should_Change_Password_Successfully()
        {
            var repo = new InMemoryUserAccountRepository();
            var user = NewUser("john.doe@acme.com", "OldPass123!");
            await repo.AddAsync(user, CancellationToken.None);

            var handler = new ChangePasswordCommandHandler(repo, new PasswordHasher(), CreateValidator());

            var beforeHash = user.PasswordHash;
            var beforeStamp = user.SecurityStamp;
            var beforeUpdatedAt = user.UpdatedAt;

            var cmd = new ChangePasswordCommand
            {
                Email = "  John.Doe@Acme.com  ",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };

            await handler.Handle(cmd, CancellationToken.None);

            var saved = await repo.FindByEmailAsync("john.doe@acme.com", CancellationToken.None);
            saved.Should().NotBeNull();

            var hasher = new PasswordHasher();
            hasher.Verify("NewStrongPass456!", saved!.PasswordHash).Should().BeTrue();
            hasher.Verify("OldPass123!", saved.PasswordHash).Should().BeFalse();

            saved.SecurityStamp.Should().NotBe(beforeStamp);
            saved.UpdatedAt.Should().NotBe(beforeUpdatedAt);
            saved.PasswordHash.Should().NotBe(beforeHash);
        }

        [Fact(DisplayName = "Should fail when user not found (do not leak info)")]
        public async Task Should_Fail_When_User_Not_Found()
        {
            var repo = new InMemoryUserAccountRepository();
            var handler = new ChangePasswordCommandHandler(repo, new PasswordHasher(), CreateValidator());

            var cmd = new ChangePasswordCommand
            {
                Email = "missing@acme.com",
                CurrentPassword = "ValidPass123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("*Invalid credentials*");
        }

        [Fact(DisplayName = "Should fail when current password is wrong")]
        public async Task Should_Fail_When_Current_Wrong()
        {
            var repo = new InMemoryUserAccountRepository();
            var user = NewUser("jane@acme.com", "CorrectPass123!");
            await repo.AddAsync(user, CancellationToken.None);

            var handler = new ChangePasswordCommandHandler(repo, new PasswordHasher(), CreateValidator());

            var cmd = new ChangePasswordCommand
            {
                Email = "jane@acme.com",
                CurrentPassword = "WRONGPASS123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("*Invalid credentials*");
        }

        [Theory(DisplayName = "Should reject invalid requests during validation")]
        [InlineData("", "ValidPass123!", "NewStrongPass456!", "NewStrongPass456!")] // email vazio
        [InlineData("invalid-email", "ValidPass123!", "NewStrongPass456!", "NewStrongPass456!")] // email inválido
        [InlineData("john@acme.com", "", "NewStrongPass456!", "NewStrongPass456!")] // senha atual vazia
        [InlineData("john@acme.com", "ValidPass123!", "weak", "weak")] // nova senha fraca
        [InlineData("john@acme.com", "ValidPass123!", "NewStrongPass456!", "DifferentPass789!")] // confirmação não confere
        public async Task Should_Reject_Invalid_Requests_During_Validation(
            string email, string currentPassword, string newPassword, string confirmPassword)
        {
            var repo = new InMemoryUserAccountRepository();
            var handler = new ChangePasswordCommandHandler(repo, new PasswordHasher(), CreateValidator());

            var cmd = new ChangePasswordCommand
            {
                Email = email,
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmPassword
            };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*Validation failed*");
        }
    }
}
