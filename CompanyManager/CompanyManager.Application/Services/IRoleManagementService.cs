using CompanyManager.Domain.AccessControl;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for managing roles and role assignments
/// </summary>
public interface IRoleManagementService
{
    /// <summary>
    /// Gets or creates a role based on job title hierarchy level
    /// </summary>
    Task<Role> GetOrCreateRoleByHierarchyLevelAsync(int hierarchyLevel, CancellationToken cancellationToken);
    
    /// <summary>
    /// Maps hierarchy level to role name
    /// </summary>
    string GetRoleNameByHierarchyLevel(int hierarchyLevel);
}
