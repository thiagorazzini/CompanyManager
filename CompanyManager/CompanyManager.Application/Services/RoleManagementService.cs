using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for managing roles and role assignments
/// </summary>
public class RoleManagementService : IRoleManagementService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(
        IRoleRepository roleRepository,
        ILogger<RoleManagementService> logger)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Role> GetOrCreateRoleByHierarchyLevelAsync(int hierarchyLevel, CancellationToken cancellationToken)
    {
        var roleName = GetRoleNameByHierarchyLevel(hierarchyLevel);
        
        _logger.LogInformation("Looking for existing role: {RoleName}", roleName);
        
        var existingRole = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        
        if (existingRole != null)
        {
            _logger.LogInformation("Role '{RoleName}' found (ID: {RoleId}, Level: {Level})", 
                roleName, existingRole.Id, existingRole.Level);
            return existingRole;
        }
        
        _logger.LogWarning("Role '{RoleName}' not found. Creating new role.", roleName);
        
        var hierarchicalRole = GetHierarchicalRoleByName(roleName);
        var newRole = new Role(roleName, hierarchicalRole);
        
        _logger.LogInformation("Saving new role '{RoleName}' to database", roleName);
        var savedRole = await _roleRepository.AddAsync(newRole, cancellationToken);
        
        if (savedRole == null || savedRole.Id == Guid.Empty)
        {
            _logger.LogError("Failed to save role '{RoleName}' to database", roleName);
            throw new InvalidOperationException($"Could not create role '{roleName}'");
        }
        
        _logger.LogInformation("Role '{RoleName}' saved successfully (ID: {RoleId})", roleName, savedRole.Id);
        
        return savedRole;
    }

    public string GetRoleNameByHierarchyLevel(int hierarchyLevel)
    {
        return hierarchyLevel switch
        {
            999 => "SuperUser",
            1 => "Director",
            2 => "Manager",
            3 => "Senior",
            4 => "Pleno",
            5 => "Junior",
            _ => "Junior"
        };
    }

    private static HierarchicalRole GetHierarchicalRoleByName(string roleName)
    {
        return roleName switch
        {
            "SuperUser" => HierarchicalRole.SuperUser,
            "Director" => HierarchicalRole.Director,
            "Manager" => HierarchicalRole.Manager,
            "Senior" => HierarchicalRole.Senior,
            "Pleno" => HierarchicalRole.Pleno,
            "Junior" => HierarchicalRole.Junior,
            _ => HierarchicalRole.Junior
        };
    }
}
