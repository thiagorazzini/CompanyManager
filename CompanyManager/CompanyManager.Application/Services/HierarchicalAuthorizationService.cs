using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for handling hierarchical authorization logic
/// </summary>
public class HierarchicalAuthorizationService : IHierarchicalAuthorizationService
{
    private readonly IUserAccountRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<HierarchicalAuthorizationService> _logger;

    public HierarchicalAuthorizationService(
        IUserAccountRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<HierarchicalAuthorizationService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> CanCreateEmployeeWithRoleAsync(Guid currentUserId, int jobTitleHierarchyLevel, CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
        {
            _logger.LogError("Current user ID not provided");
            throw new UnauthorizedAccessException("Current user ID not provided.");
        }

        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
        {
            _logger.LogError("Current user not found. UserId: {CurrentUserId}", currentUserId);
            throw new UnauthorizedAccessException("Current user not found.");
        }

        var currentUserRole = await _roleRepository.GetByIdAsync(currentUser.RoleId, cancellationToken);
        if (currentUserRole == null)
        {
            _logger.LogError("Current user role not found. RoleId: {RoleId}", currentUser.RoleId);
            throw new UnauthorizedAccessException("Current user role not found.");
        }

        var targetRoleLevel = ConvertJobTitleLevelToHierarchicalRole(jobTitleHierarchyLevel);
        var canCreate = currentUser.CanCreateRole(currentUserRole, targetRoleLevel);

        _logger.LogInformation("Hierarchical authorization check - User: {UserId}, Current role: {CurrentRole}, Target role: {TargetRole}, Can create: {CanCreate}",
            currentUserId, currentUserRole.Level, targetRoleLevel, canCreate);

        return canCreate;
    }

    public HierarchicalRole ConvertJobTitleLevelToHierarchicalRole(int hierarchyLevel)
    {
        return hierarchyLevel switch
        {
            999 => HierarchicalRole.SuperUser,
            1 => HierarchicalRole.Director,
            2 => HierarchicalRole.Manager,
            3 => HierarchicalRole.Senior,
            4 => HierarchicalRole.Pleno,
            5 => HierarchicalRole.Junior,
            _ => HierarchicalRole.Junior
        };
    }
}
