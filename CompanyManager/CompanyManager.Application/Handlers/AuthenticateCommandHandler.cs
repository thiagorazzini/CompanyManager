using System;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Services;
using CompanyManager.Application.Validators;
using FluentValidation;
using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;

namespace CompanyManager.Application.Handlers
{
    public sealed class AuthenticateCommandHandler : IAuthenticateCommandHandler
    {
        private readonly IUserAccountRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IValidator<AuthenticateRequest> _validator;

        public AuthenticateCommandHandler(
            IUserAccountRepository users, 
            IPasswordHasher hasher, 
            ITokenService tokens,
            IValidator<AuthenticateRequest> validator)
        {
            _users = users;
            _hasher = hasher;
            _tokens = tokens;
            _validator = validator;
        }

        public async Task<AuthResult> Handle(AuthenticateCommand cmd, CancellationToken ct)
        {
            // 0) Validar o comando antes de processar
            var authenticateRequest = new AuthenticateRequest
            {
                Email = cmd.Email,
                Password = cmd.Password
            };

            var validationResult = await _validator.ValidateAsync(authenticateRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            // 1) Normalizar email
            var normalizedEmail = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
            
            // 2) Buscar usuário
            var user = await _users.FindByEmailAsync(normalizedEmail, ct);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            // 3) Verificar senha
            if (!_hasher.Verify(cmd.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            // 4) Gerar token
            var accessToken = _tokens.GenerateAccessToken(user);
            var expiresAt = _tokens.GetExpirationUtc();
            return new AuthResult(accessToken, expiresAt);
        }
    }
}
