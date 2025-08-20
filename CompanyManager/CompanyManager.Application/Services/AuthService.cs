#nullable enable

using CompanyManager.Application.Interfaces;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;


namespace CompanyManager.Application.Services
{
    /// <summary>
    /// Service responsible for employee authentication and credential validation.
    /// </summary>
    /// <remarks>
    /// This service validates employee credentials by retrieving the employee from
    /// the repository using their email and then verifying the provided password
    /// against the stored hash using the password hasher service.
    /// </remarks>
    public sealed class AuthService : IAuthService
    {
        public string GenerateJwtToken(Employee employee)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
