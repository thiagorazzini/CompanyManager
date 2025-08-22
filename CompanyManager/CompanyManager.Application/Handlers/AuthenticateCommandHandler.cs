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
using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;

namespace CompanyManager.Application.Handlers
{
    /// <summary>
    /// Handles user authentication by validating credentials and generating access tokens
    /// </summary>
    public sealed class AuthenticateCommandHandler : IAuthenticateCommandHandler
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IValidator<AuthenticateRequest> _validator;

        public AuthenticateCommandHandler(
            IUserAccountRepository userRepository, 
            IPasswordHasher passwordHasher, 
            ITokenService tokenService,
            IValidator<AuthenticateRequest> validator)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Authenticates user credentials and returns authentication result
        /// </summary>
        public async Task<AuthResult> Handle(AuthenticateCommand command, CancellationToken cancellationToken)
        {
            await ValidateInputAsync(command, cancellationToken);
            
            var normalizedEmail = NormalizeEmail(command.Email);
            var user = await GetUserAsync(normalizedEmail, cancellationToken);
            
            ValidatePassword(command.Password, user.PasswordHash);
            
            var accessToken = _tokenService.GenerateAccessToken(user);
            var expiresAt = _tokenService.GetExpirationUtc();
            
            return new AuthResult(accessToken, expiresAt, user.Id, user.UserName);
        }

        private async Task ValidateInputAsync(AuthenticateCommand command, CancellationToken cancellationToken)
        {
            var authenticateRequest = new AuthenticateRequest
            {
                Email = command.Email,
                Password = command.Password
            };

            var validationResult = await _validator.ValidateAsync(authenticateRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private async Task<UserAccount> GetUserAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");
            
            return user;
        }

        private void ValidatePassword(string password, string passwordHash)
        {
            if (!_passwordHasher.Verify(password, passwordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");
        }
    }
}
