using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.AccessControl;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CompanyManager.Application.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAccountRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobTitleRepository _jobTitleRepository;
        private readonly IRoleRepository _roleRepository;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IUserAccountRepository userRepository,
            IEmployeeRepository employeeRepository,
            IJobTitleRepository jobTitleRepository,
            IRoleRepository roleRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _jobTitleRepository = jobTitleRepository ?? throw new ArgumentNullException(nameof(jobTitleRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
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

        public async Task<JobTitle?> GetCurrentJobTitleAsync(CancellationToken cancellationToken = default)
        {
            var userAccount = await GetCurrentUserAsync(cancellationToken);
            if (userAccount == null)
                return null;

            return await _jobTitleRepository.GetByIdAsync(userAccount.JobTitleId, cancellationToken);
        }

        public async Task<Role?> GetCurrentRoleAsync(CancellationToken cancellationToken = default)
        {
            var userAccount = await GetCurrentUserAsync(cancellationToken);
            if (userAccount == null)
                return null;

            return await _roleRepository.GetByIdAsync(userAccount.RoleId, cancellationToken);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
