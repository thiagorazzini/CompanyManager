using System;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Services;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using FluentValidation;
using System.Linq;

namespace CompanyManager.Application.Handlers
{
    public sealed class ChangePasswordCommandHandler : IChangePasswordCommandHandler
    {
        private readonly IUserAccountRepository _users;
        private readonly PasswordHasher _hasher;
        private readonly IValidator<ChangePasswordRequest> _validator;

        public ChangePasswordCommandHandler(
            IUserAccountRepository users, 
            PasswordHasher hasher,
            IValidator<ChangePasswordRequest> validator)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task Handle(ChangePasswordCommand cmd, CancellationToken ct)
        {
            await ValidateRequest(cmd, ct);
            await ValidateUserCredentials(cmd, ct);
            ValidateNewPasswordRequirements(cmd);
            await UpdateUserPassword(cmd, ct);
        }

        private async Task ValidateRequest(ChangePasswordCommand cmd, CancellationToken ct)
        {
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = cmd.Email,
                CurrentPassword = cmd.CurrentPassword,
                NewPassword = cmd.NewPassword,
                ConfirmNewPassword = cmd.ConfirmNewPassword
            };

            var validationResult = await _validator.ValidateAsync(changePasswordRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }
        }

        private async Task ValidateUserCredentials(ChangePasswordCommand cmd, CancellationToken ct)
        {
            var normalizedEmail = NormalizeEmail(cmd.Email);
            var user = await FindUserByEmail(normalizedEmail, ct);
            ValidateCurrentPassword(cmd.CurrentPassword, user.PasswordHash);
        }

        private static string NormalizeEmail(string email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        private async Task<UserAccount> FindUserByEmail(string email, CancellationToken ct)
        {
            var user = await _users.FindByEmailAsync(email, ct);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            return user;
        }

        private void ValidateCurrentPassword(string currentPassword, string storedHash)
        {
            if (!_hasher.Verify(currentPassword ?? string.Empty, storedHash))
                throw new UnauthorizedAccessException("Invalid credentials.");
        }

        private void ValidateNewPasswordRequirements(ChangePasswordCommand cmd)
        {
            if (string.Equals(cmd.NewPassword, cmd.CurrentPassword, StringComparison.Ordinal))
                throw new ArgumentException("New password must be different from the current one.", nameof(cmd.NewPassword));
        }

        private async Task UpdateUserPassword(ChangePasswordCommand cmd, CancellationToken ct)
        {
            var normalizedEmail = NormalizeEmail(cmd.Email);
            var user = await FindUserByEmail(normalizedEmail, ct);
            
            var newHash = _hasher.Hash(cmd.NewPassword);
            user.ChangePassword(newHash);

            await _users.UpdateAsync(user, ct);
        }
    }
}
