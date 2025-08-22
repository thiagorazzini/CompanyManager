using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for handling hierarchical authorization logic
/// </summary>
public interface IHierarchicalAuthorizationService
{
    /// <summary>
    /// Validates if current user can create employees with specified role level
    /// </summary>
    Task<bool> CanCreateEmployeeWithRoleAsync(Guid currentUserId, int jobTitleHierarchyLevel, CancellationToken cancellationToken);
    
    /// <summary>
    /// Converts job title hierarchy level to hierarchical role
    /// </summary>
    HierarchicalRole ConvertJobTitleLevelToHierarchicalRole(int hierarchyLevel);
}
