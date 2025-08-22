using CompanyManager.Application.Commands;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Handlers;

/// <summary>
/// Handles employee update operations with hierarchical validation
/// </summary>
public sealed class UpdateEmployeeCommandHandler : IUpdateEmployeeCommandHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IJobTitleRepository _jobTitleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IUserAccountRepository userAccountRepository,
        IDepartmentRepository departmentRepository,
        IJobTitleRepository jobTitleRepository,
        IPasswordHasher passwordHasher,
        ILogger<UpdateEmployeeCommandHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _userAccountRepository = userAccountRepository ?? throw new ArgumentNullException(nameof(userAccountRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _jobTitleRepository = jobTitleRepository ?? throw new ArgumentNullException(nameof(jobTitleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles employee update with comprehensive validation and hierarchical authorization
    /// </summary>
    public async Task Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken, Guid? currentUserId = null)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        
        var employee = await GetEmployeeAsync(command.Id, cancellationToken);
        
        await ValidateBusinessRulesAsync(command, employee, cancellationToken);
        
        // Only validate current user if we need to check permissions for sensitive operations
        var isChangingJobTitle = command.JobTitleId != Guid.Empty && command.JobTitleId != employee.JobTitleId;
        var isChangingPassword = !string.IsNullOrEmpty(command.Password);
        
        if (isChangingPassword || isChangingJobTitle)
        {
            if (currentUserId == null || currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException("Current user ID not provided.");
            
            var currentUser = await GetCurrentUserAsync(currentUserId.Value, cancellationToken);
            var currentUserRole = await GetRoleByIdAsync(currentUser.RoleId, cancellationToken);
            
            if (currentUserRole == null)
                throw new UnauthorizedAccessException("Current user role not found.");
            
            await ValidateHierarchicalPermissionsAsync(command, employee, currentUser, currentUserRole, cancellationToken);
            await UpdateEmployeeDataAsync(command, employee, currentUser, currentUserRole, cancellationToken);
        }
        else
        {
            // For basic updates, we can proceed without current user context
            await UpdateEmployeeDataAsync(command, employee, null, null, cancellationToken);
        }
        
        await PersistChangesAsync(employee, cancellationToken);
    }



    private async Task<Employee> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
            throw new ArgumentException("Employee not found.", nameof(employeeId));
        
        return employee;
    }

    private async Task<UserAccount> GetCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await _userAccountRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
            throw new UnauthorizedAccessException("Current user not found.");
        
        return currentUser;
    }

    private async Task ValidateHierarchicalPermissionsAsync(
        UpdateEmployeeCommand command, 
        Employee employee, 
        UserAccount currentUser, 
        Role currentUserRole, 
        CancellationToken cancellationToken)
    {
        if (command.JobTitleId != Guid.Empty && command.JobTitleId != employee.JobTitleId)
        {
            await ValidateJobTitleChangeAsync(command.JobTitleId, currentUser, currentUserRole, cancellationToken);
        }
    }

    private async Task ValidateJobTitleChangeAsync(Guid newJobTitleId, UserAccount currentUser, Role currentUserRole, CancellationToken cancellationToken)
    {
        var newJobTitle = await _jobTitleRepository.GetByIdAsync(newJobTitleId, cancellationToken);
        if (newJobTitle != null)
        {
            var targetHierarchicalRole = ConvertJobTitleLevelToHierarchicalRole(newJobTitle.HierarchyLevel);
            
            if (!currentUser.IsSuperUser(currentUserRole) && !currentUser.CanCreateRole(currentUserRole, targetHierarchicalRole))
            {
                throw new UnauthorizedAccessException($"You cannot change JobTitle to '{targetHierarchicalRole}' level. Your role level is '{currentUserRole.Level}'.");
            }
        }
    }

    private Task ValidateBusinessRulesAsync(UpdateEmployeeCommand command, Employee employee, CancellationToken cancellationToken)
    {
        ValidatePhoneNumbers(command.Phones);
        
        return Task.WhenAll(
            ValidateDepartmentExistsAsync(command.DepartmentId, cancellationToken),
            ValidateJobTitleExistsAsync(command.JobTitleId, cancellationToken),
            ValidateEmailUniquenessAsync(command.Email, employee, cancellationToken),
            ValidateDocumentUniquenessAsync(command.DocumentNumber, employee, cancellationToken)
        );
    }

    private async Task ValidateDepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        if (!await _departmentRepository.ExistsAsync(departmentId, cancellationToken))
            throw new ArgumentException("Department does not exist.", nameof(departmentId));
    }

    private async Task ValidateJobTitleExistsAsync(Guid jobTitleId, CancellationToken cancellationToken)
    {
        if (jobTitleId != Guid.Empty && !await _jobTitleRepository.ExistsAsync(jobTitleId, cancellationToken))
            throw new ArgumentException("Job title does not exist.", nameof(jobTitleId));
    }

    private async Task ValidateEmailUniquenessAsync(string? newEmail, Employee employee, CancellationToken cancellationToken)
    {
        var incomingEmailNorm = (newEmail ?? string.Empty).Trim().ToLowerInvariant();
        var currentEmailNorm = employee.Email.Value.Trim().ToLowerInvariant();

        if (!string.Equals(incomingEmailNorm, currentEmailNorm, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.EmailExistsAsync(incomingEmailNorm, cancellationToken))
                throw new InvalidOperationException("Email already in use.");
        }
    }

    private async Task ValidateDocumentUniquenessAsync(string? newDocumentNumber, Employee employee, CancellationToken cancellationToken)
    {
        var incomingDoc = new DocumentNumber(newDocumentNumber ?? string.Empty);
        if (!string.Equals(incomingDoc.Digits, employee.DocumentNumber.Digits, StringComparison.Ordinal))
        {
            if (await _employeeRepository.CpfExistsAsync(incomingDoc.Digits, cancellationToken))
                throw new InvalidOperationException("Document number already in use.");
        }
    }

    private static void ValidatePhoneNumbers(List<string>? phones)
    {
        var requestedPhones = phones ?? new List<string>();
        if (requestedPhones.Count == 0)
            throw new ArgumentException("At least one phone number is required.", nameof(phones));
    }

    private async Task UpdateEmployeeDataAsync(
        UpdateEmployeeCommand command, 
        Employee employee, 
        UserAccount? currentUser, 
        Role? currentUserRole, 
        CancellationToken cancellationToken)
    {
        UpdateEmployeeBasicInfo(command, employee);
        UpdateEmployeePhones(command.Phones, employee);
        
        if (!string.IsNullOrEmpty(command.Password))
        {
            if (currentUser != null && currentUserRole != null)
            {
                await UpdateEmployeePasswordAsync(command.Password, employee, currentUser, currentUserRole, cancellationToken);
            }
            else
            {
                // For basic updates without user context, just update the password directly
                await UpdateEmployeePasswordAsync(command.Password, employee, cancellationToken);
            }
        }
    }

    private static void UpdateEmployeeBasicInfo(UpdateEmployeeCommand command, Employee employee)
    {
        employee.ChangeName(command.FirstName ?? string.Empty, command.LastName ?? string.Empty);
        employee.ChangeEmail(new Email((command.Email ?? string.Empty).Trim()));
        employee.ChangeDocument(new DocumentNumber((command.DocumentNumber ?? string.Empty).Trim()));
        
        if (command.JobTitleId != Guid.Empty)
        {
            employee.ChangeJobTitle(command.JobTitleId);
        }
        
        employee.ChangeDepartment(command.DepartmentId);
    }

    private static void UpdateEmployeePhones(List<string>? phones, Employee employee)
    {
        var requestedPhones = phones ?? new List<string>();
        employee.UpdatePhones(requestedPhones);
    }

    private async Task UpdateEmployeePasswordAsync(
        string newPassword, 
        Employee employee, 
        UserAccount currentUser, 
        Role currentUserRole, 
        CancellationToken cancellationToken)
    {
        var userAccount = await _userAccountRepository.GetByEmailAsync(employee.Email.Value, cancellationToken);
        if (userAccount != null)
        {
            var targetUserRole = await GetRoleByIdAsync(userAccount.RoleId, cancellationToken);
            if (targetUserRole != null)
            {
                ValidatePasswordChangePermission(currentUser, currentUserRole, userAccount, targetUserRole);
                await ChangeUserPasswordAsync(userAccount, newPassword, cancellationToken);
            }
        }
    }

    private async Task UpdateEmployeePasswordAsync(
        string newPassword, 
        Employee employee, 
        CancellationToken cancellationToken)
    {
        var userAccount = await _userAccountRepository.GetByEmailAsync(employee.Email.Value, cancellationToken);
        if (userAccount != null)
        {
            await ChangeUserPasswordAsync(userAccount, newPassword, cancellationToken);
        }
    }

    private static void ValidatePasswordChangePermission(UserAccount currentUser, Role currentUserRole, UserAccount targetUser, Role targetUserRole)
    {
        if (!currentUser.CanModifyUser(currentUserRole, targetUser, targetUserRole))
        {
            throw new UnauthorizedAccessException($"You cannot modify the password of a user with role level '{targetUserRole.Level}'. Your role level is '{currentUserRole.Level}'.");
        }
    }

    private async Task ChangeUserPasswordAsync(UserAccount userAccount, string newPassword, CancellationToken cancellationToken)
    {
        var newPasswordHash = _passwordHasher.Hash(newPassword);
        userAccount.ChangePassword(newPasswordHash);
        await _userAccountRepository.UpdateAsync(userAccount, cancellationToken);
    }

    private async Task PersistChangesAsync(Employee employee, CancellationToken cancellationToken)
    {
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
    }

    /// <summary>
    /// Converts JobTitle.HierarchyLevel to HierarchicalRole for authorization purposes
    /// </summary>
    private static HierarchicalRole ConvertJobTitleLevelToHierarchicalRole(int hierarchyLevel)
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

    /// <summary>
    /// Retrieves a role by ID - temporary implementation until proper role repository is available
    /// </summary>
    private Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement proper role repository access
            // This is a temporary solution - ideally we should have direct access to IRoleRepository
            var tempRole = new Role("Admin", HierarchicalRole.SuperUser);
            return Task.FromResult<Role?>(tempRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role by ID: {RoleId}", roleId);
            return Task.FromResult<Role?>(null);
        }
    }
}
