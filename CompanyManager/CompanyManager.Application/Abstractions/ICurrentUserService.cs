using CompanyManager.Domain.Entities;
using CompanyManager.Domain.AccessControl;

namespace CompanyManager.Application.Abstractions
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current authenticated user's ID
        /// </summary>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Gets the current authenticated user's account
        /// </summary>
        Task<UserAccount?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current authenticated user's employee information
        /// </summary>
        Task<Employee?> GetCurrentEmployeeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current authenticated user's job title information
        /// </summary>
        Task<JobTitle?> GetCurrentJobTitleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current authenticated user's role information
        /// </summary>
        Task<Role?> GetCurrentRoleAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        bool IsAuthenticated();
    }
}
