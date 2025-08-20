#nullable enable

using CompanyManager.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace CompanyManager.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for employee authentication services.
    /// </summary>
    /// <remarks>
    /// This service is responsible for validating employee credentials by comparing
    /// provided email and password against stored employee data. The actual
    /// implementation handles password hashing and comparison details.
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Validates employee credentials asynchronously.
        /// </summary>
        /// <param name="email">The email address of the employee to authenticate.</param>
        /// <param name="password">The plain text password to validate.</param>
        /// <param name="ct">Cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// <see langword="true"/> if the credentials are valid; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="email"/> or <paramref name="password"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="email"/> or <paramref name="password"/> is empty or whitespace.
        /// </exception>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when the operation is cancelled via the <paramref name="ct"/> token.
        /// </exception>
        Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
        string GenerateJwtToken(Employee employee);
    }
}
