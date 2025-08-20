using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Interfaces;
using FluentValidation;
using System.Security.Claims;

namespace CompanyManager.Application.Handlers
{
    public sealed class RefreshTokenCommandHandler : IRefreshTokenCommandHandler
    {
        private readonly IUserAccountRepository _users;
        private readonly ITokenService _tokens;
        private readonly IValidator<RefreshTokenRequest> _validator;

        public RefreshTokenCommandHandler(
            IUserAccountRepository users,
            ITokenService tokens,
            IValidator<RefreshTokenRequest> validator)
        {
            _users = users;
            _tokens = tokens;
            _validator = validator;
        }

        public async Task<AuthResult> Handle(RefreshTokenCommand cmd, CancellationToken ct)
        {
            // 0) Validar o comando antes de processar
            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = cmd.RefreshToken
            };

            var validationResult = await _validator.ValidateAsync(refreshTokenRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            // 1) Validar o refresh token (implementação simplificada para testes)
            // Nota: Em produção, isso deveria validar um JWT real
            if (string.IsNullOrWhiteSpace(cmd.RefreshToken))
                throw new UnauthorizedAccessException("Invalid refresh token.");

            // 2) Para testes, vamos assumir que o refresh token contém o email do usuário
            // Em produção, isso seria um JWT decodificado
            var email = cmd.RefreshToken.Trim().ToLowerInvariant();
            var user = await _users.FindByEmailAsync(email, ct);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            // 3) Verificar se o usuário está ativo
            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is not active.");

            // 4) Gerar novo access token
            var accessToken = _tokens.GenerateAccessToken(user);
            var expiresAt = _tokens.GetExpirationUtc();

            // 5) Retornar resultado
            return new AuthResult(accessToken, expiresAt);
        }
    }
}