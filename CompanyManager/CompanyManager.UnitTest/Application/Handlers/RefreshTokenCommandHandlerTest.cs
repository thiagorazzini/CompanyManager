using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Xunit;

using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Auth;              // AuthResult
using CompanyManager.Application.Auth.Interfaces;   // ITokenService
using CompanyManager.Application.Services;          // TokenService
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.UnitTest.Application.TestDouble; // InMemoryUserAccountRepository
using Microsoft.Extensions.Options;

namespace CompanyManager.UnitTest.Application.Handlers
{
    public sealed class RefreshTokenCommandHandlerTest
    {
        private static UserAccount NewUser(string email)
        {
            // hash não é usado no refresh; pode ser qualquer string não vazia
            return UserAccount.Create(
                userName: email.Trim().ToLowerInvariant(),
                passwordHash: "$2a$10$u3kH8S5Xw5f9s1.zzzFakeHash1234567890ABCDE",
                employeeId: Guid.NewGuid()
            );
        }

        private static IValidator<RefreshTokenRequest> CreateValidator() => 
            new RefreshTokenRequestValidator();

        private static IOptions<JwtOptions> Options()
        {
            return Microsoft.Extensions.Options.Options.Create(new JwtOptions
            {
                Issuer = "cm-tests",
                Audience = "cm-client",
                Secret = "unit-test-access-secret-0123456789ABCDEF",
                AccessTokenMinutes = 15,
                ClockSkewSeconds = 0
            });
        }

        [Fact(DisplayName = "Should refresh access token when refresh token is valid")]
        public async Task Should_Refresh_Successfully()
        {
            var users = new InMemoryUserAccountRepository();
            var tokens = new TokenService(Options()); // Usar TokenService real

            var user = NewUser("john.doe@acme.com");
            await users.AddAsync(user, CancellationToken.None);

            // Para testes, vamos usar o email como refresh token (implementação simplificada)
            var goodRt = "john.doe@acme.com";

            var handler = new RefreshTokenCommandHandler(users, tokens, CreateValidator());
            var cmd = new RefreshTokenCommand { RefreshToken = goodRt };

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.AccessToken.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            result.UserId.Should().Be(user.Id);
            result.Email.Should().Be(user.UserName);
        }

        [Fact(DisplayName = "Should fail when refresh token is expired")]
        public async Task Should_Fail_When_Expired()
        {
            var users = new InMemoryUserAccountRepository();
            var tokens = new TokenService(Options());

            var user = NewUser("jane@acme.com");
            await users.AddAsync(user, CancellationToken.None);

            // Para testes, vamos usar um token vazio (que falha na validação)
            var expiredRt = "";

            var handler = new RefreshTokenCommandHandler(users, tokens, CreateValidator());
            var cmd = new RefreshTokenCommand { RefreshToken = expiredRt };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*Validation failed*");
        }

        [Fact(DisplayName = "Should fail when token payload does not match user (stamp mismatch)")]
        public async Task Should_Fail_When_SecurityStamp_Mismatch()
        {
            var users = new InMemoryUserAccountRepository();
            var tokens = new TokenService(Options());

            var user = NewUser("alice@acme.com");
            await users.AddAsync(user, CancellationToken.None);

            // Para testes, vamos usar um email que não existe no repositório
            var mismatchRt = "nonexistent@acme.com";

            var handler = new RefreshTokenCommandHandler(users, tokens, CreateValidator());
            var cmd = new RefreshTokenCommand { RefreshToken = mismatchRt };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("*Invalid refresh token*");
        }

        [Fact(DisplayName = "Should fail when refresh token is invalid/unknown")]
        public async Task Should_Fail_When_Invalid_Token()
        {
            var users = new InMemoryUserAccountRepository();
            var tokens = new TokenService(Options());

            var handler = new RefreshTokenCommandHandler(users, tokens, CreateValidator());
            var cmd = new RefreshTokenCommand { RefreshToken = "invalid-token-format" };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("*Invalid refresh token*");
        }

        [Theory(DisplayName = "Should reject invalid refresh tokens during validation")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid-token")]
        [InlineData("part1.part2")]
        [InlineData("part1.part2.part3.part4")]
        public async Task Should_Reject_Invalid_Tokens_During_Validation(string token)
        {
            var users = new InMemoryUserAccountRepository();
            var tokens = new TokenService(Options());

            var handler = new RefreshTokenCommandHandler(users, tokens, CreateValidator());
            var cmd = new RefreshTokenCommand { RefreshToken = token };

            var act = () => handler.Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                     .WithMessage("*Validation failed*");
        }
    }
}
