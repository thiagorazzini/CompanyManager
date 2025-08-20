using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CompanyManager.Application.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.Secret))
                throw new ArgumentException("JWT secret is required.", nameof(options));

            var keyBytes = Encoding.UTF8.GetBytes(_options.Secret);
            var key = new SymmetricSecurityKey(keyBytes);
            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateAccessToken(UserAccount user, IEnumerable<Claim>? extraClaims = null)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", user.UserName),
                new Claim("sstamp", user.SecurityStamp.ToString())
            };

            // Adicionar claims extras se fornecidos
            if (extraClaims != null)
                claims.AddRange(extraClaims);

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: _credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public DateTime GetExpirationUtc()
        {
            return DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        }
    }
}
