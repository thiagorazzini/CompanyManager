using CompanyManager.Application.Auth;
using CompanyManager.Application.Services;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CompanyManager.UnitTest.Application.Services
{
    public sealed class TokenServiceTest
    {
        private static IOptions<JwtOptions> Options(
            string issuer = "cm-tests",
            string audience = "cm-client",
            string secret = "unit-test-access-secret-0123456789ABCDEF",
            int accessMinutes = 15,
            int skewSeconds = 0
        )
        {
            return Microsoft.Extensions.Options.Options.Create(new JwtOptions
            {
                Issuer = issuer,
                Audience = audience,
                Secret = secret,
                AccessTokenMinutes = accessMinutes,
                ClockSkewSeconds = skewSeconds
            });
        }

        private static TokenValidationParameters Validation(string issuer, string audience, string secret, int skewSeconds = 0)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(skewSeconds)
            };
        }

        private static UserAccount NewUser(string email)
        {
            // Ajuste para sua factory real se necessário
            return UserAccount.Create(
                userName: email.Trim().ToLowerInvariant(),
                passwordHash: "x",
                employeeId: Guid.NewGuid(),
                roleId: Guid.NewGuid(),
                jobTitleId: Guid.NewGuid()
            );
        }

        [Fact(DisplayName = "Should create JWT with expected claims (sub, name, sstamp, iss, aud)")]
        public void Should_Create_Jwt_With_Claims()
        {
            var opts = Options();
            var mockUserRepo = new Mock<IUserAccountRepository>();
            var svc = new TokenService(opts, mockUserRepo.Object);

            var user = NewUser("john.doe@acme.com");
            var token = svc.GenerateAccessToken(user);

            // valida assinatura/claims
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(
                token,
                Validation(opts.Value.Issuer, opts.Value.Audience, opts.Value.Secret, opts.Value.ClockSkewSeconds),
                out var validated);

            var jwt = (JwtSecurityToken)validated;

            jwt.Claims.First(c => c.Type == "sub").Value.Should().Be(user.Id.ToString());
            jwt.Claims.First(c => c.Type == "name").Value.Should().Be(user.UserName);
            jwt.Claims.First(c => c.Type == "sstamp").Value.Should().Be(user.SecurityStamp.ToString());
            jwt.Issuer.Should().Be(opts.Value.Issuer);
            jwt.Audiences.Should().Contain(opts.Value.Audience);
        }

        [Fact(DisplayName = "Should set ExpiresAt according to options")]
        public void Should_Set_ExpiresAt()
        {
            var opts = Options(accessMinutes: 20, skewSeconds: 0);
            var mockUserRepo = new Mock<IUserAccountRepository>();
            var svc = new TokenService(opts, mockUserRepo.Object);

            var user = NewUser("alice@acme.com");
            var before = DateTime.UtcNow;
            var expiresAt = svc.GetExpirationUtc();
            var after = DateTime.UtcNow;

            var expectedMin = before.AddMinutes(20).AddSeconds(-2);
            var expectedMax = after.AddMinutes(20).AddSeconds(2);

            expiresAt.Should().BeOnOrAfter(expectedMin);
            expiresAt.Should().BeOnOrBefore(expectedMax);
        }

        [Fact(DisplayName = "Should reject validation with wrong signing key")]
        public void Should_Reject_With_Wrong_Key()
        {
            var opts = Options(secret: "unit-test-access-secret-AAAAAAAAAAAAAAAAAAAAAA");
            var mockUserRepo = new Mock<IUserAccountRepository>();
            var svc = new TokenService(opts, mockUserRepo.Object);

            var user = NewUser("bob@acme.com");
            var token = svc.GenerateAccessToken(user);

            var handler = new JwtSecurityTokenHandler();

            Action act = () => handler.ValidateToken(
                token,
                Validation(opts.Value.Issuer, opts.Value.Audience, "WRONG-SECRET-0529982247252345"),
                out _);

            act.Should().Throw<SecurityTokenException>();
        }
    }
}
