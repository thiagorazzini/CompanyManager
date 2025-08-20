using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Domain.Entities;
using System.Security.Claims;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    internal sealed class StubTokenService : ITokenService
    {
        // Contadores usados nos testes
        public int GenerateAccessTokenCalls { get; private set; }
        public int GetExpirationUtcCalls { get; private set; }

        // Propriedades esperadas pelos testes
        public int Calls { get; private set; }   // alias para GenerateAccessTokenCalls
        public Guid LastUserId { get; private set; }
        public string LastGeneratedToken { get; private set; } = string.Empty;

        public void ResetCounters()
        {
            GenerateAccessTokenCalls = 0;
            GetExpirationUtcCalls = 0;
            Calls = 0;
            LastUserId = Guid.Empty;
            LastGeneratedToken = string.Empty;
        }

        public string GenerateAccessToken(UserAccount user, IEnumerable<Claim>? extraClaims = null)
        {
            GenerateAccessTokenCalls++;
            Calls++;                 // mantém compatível com tokens.Calls nos testes
            LastUserId = user.Id;    // os testes verificam este valor
            LastGeneratedToken = "stub.jwt.token"; // Token fixo esperado pelos testes
            return LastGeneratedToken;
        }

        public DateTime GetExpirationUtc()
        {
            GetExpirationUtcCalls++;
            return DateTime.UtcNow.AddMinutes(30);
        }
    }
}
