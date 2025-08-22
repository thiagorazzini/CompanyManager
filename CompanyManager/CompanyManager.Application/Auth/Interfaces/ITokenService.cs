using CompanyManager.Domain.Entities;
using System.Security.Claims;

namespace CompanyManager.Application.Auth.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserAccount user, IEnumerable<Claim>? extraClaims = null);
        Task<string> GenerateAccessTokenAsync(UserAccount user, CancellationToken cancellationToken = default);
        DateTime GetExpirationUtc();
    }
}
