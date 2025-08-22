using System;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using FluentValidation;
using System.Linq;

namespace CompanyManager.Application.Handlers
{
    /// <summary>
    /// Handles password change operations by validating credentials and updating user passwords
    /// </summary>
    public sealed class ChangePasswordCommandHandler : IChangePasswordCommandHandler
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<ChangePasswordRequest> _validator;

        public ChangePasswordCommandHandler(
            IUserAccountRepository userRepository, 
            IPasswordHasher passwordHasher,
            IValidator<ChangePasswordRequest> validator)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Handles the password change command by orchestrating validation and update operations
        /// </summary>
        public async Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            await ValidateRequestAsync(command, cancellationToken);
            await ValidateUserCredentialsAsync(command, cancellationToken);
            ValidateNewPasswordRequirements(command);
            await UpdateUserPasswordAsync(command, cancellationToken);
        }

        /// <summary>
        /// Validates the change password request using FluentValidation
        /// </summary>
        private async Task ValidateRequestAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = command.Email,
                CurrentPassword = command.CurrentPassword,
                NewPassword = command.NewPassword,
                ConfirmNewPassword = command.ConfirmNewPassword
            };

            var validationResult = await _validator.ValidateAsync(changePasswordRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }
        }

        /// <summary>
        /// Validates user credentials by checking email and current password
        /// </summary>
        private async Task ValidateUserCredentialsAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var normalizedEmail = NormalizeEmail(command.Email);
            var user = await FindUserByEmailAsync(normalizedEmail, cancellationToken);
            ValidateCurrentPassword(command.CurrentPassword, user.PasswordHash);
        }

        /// <summary>
        /// Normalizes email by trimming whitespace and converting to lowercase
        /// </summary>
        private static string NormalizeEmail(string email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        /// <summary>
        /// Finds user by email and throws exception if not found
        /// </summary>
        private async Task<UserAccount> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            return user;
        }

        /// <summary>
        /// Validates current password against stored hash
        /// </summary>
        private void ValidateCurrentPassword(string currentPassword, string storedHash)
        {
            if (!_passwordHasher.Verify(currentPassword ?? string.Empty, storedHash))
                throw new UnauthorizedAccessException("Invalid credentials.");
        }

        /// <summary>
        /// Validates that new password is different from current password
        /// </summary>
        private void ValidateNewPasswordRequirements(ChangePasswordCommand command)
        {
            if (string.Equals(command.NewPassword, command.CurrentPassword, StringComparison.Ordinal))
                throw new ArgumentException("New password must be different from the current one.", nameof(command.NewPassword));
        }

        /// <summary>
        /// Updates user password with new hash and persists changes
        /// </summary>
        private async Task UpdateUserPasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var normalizedEmail = NormalizeEmail(command.Email);
            var user = await FindUserByEmailAsync(normalizedEmail, cancellationToken);
            
            var newHash = _passwordHasher.Hash(command.NewPassword);
            user.ChangePassword(newHash);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }
}
