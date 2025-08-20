using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CompanyManager.Application.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IUserAccountRepository userRepository,
            IEmployeeRepository employeeRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        public Guid? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }

        public async Task<UserAccount?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return null;

            return await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        }

        public async Task<Employee?> GetCurrentEmployeeAsync(CancellationToken cancellationToken = default)
        {
            var userAccount = await GetCurrentUserAsync(cancellationToken);
            if (userAccount == null)
                return null;

            return await _employeeRepository.GetByIdAsync(userAccount.EmployeeId, cancellationToken);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
