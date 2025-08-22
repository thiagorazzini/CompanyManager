using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CompanyManager.Application.Auth
{
    public sealed class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;
        private readonly IUserAccountRepository _userRepository;

        public TokenService(IOptions<JwtOptions> options, IUserAccountRepository userRepository)
        {
            _options = options.Value;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

            if (string.IsNullOrWhiteSpace(_options.Secret))
                throw new ArgumentException("JWT secret is required.", nameof(options));

            var keyBytes = Encoding.UTF8.GetBytes(_options.Secret);
            var key = new SymmetricSecurityKey(keyBytes);
            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public async Task<string> GenerateAccessTokenAsync(UserAccount user, CancellationToken cancellationToken = default)
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

            // Adicionar permissões do usuário
            var permissions = await _userRepository.GetAllPermissionsAsync(user.Id, cancellationToken);
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    if (!string.IsNullOrEmpty(permission))
                    {
                        claims.Add(new Claim("perm", permission));
                    }
                }
            }

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
