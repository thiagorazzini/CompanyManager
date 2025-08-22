using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Application.Services;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.Domain.AccessControl;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace CompanyManager.Application.Handlers;

public sealed class CreateEmployeeHandler
{
    private readonly CreateEmployeeRequestValidator _validator;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserAccountRepository _userRepository;
    private readonly IJobTitleRepository _jobTitleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHierarchicalAuthorizationService _authorizationService;
    private readonly IRoleManagementService _roleManagementService;
    private readonly IEmployeeValidationService _validationService;
    private readonly ILogger<CreateEmployeeHandler> _logger;

    public CreateEmployeeHandler(
        CreateEmployeeRequestValidator validator,
        IEmployeeRepository employeeRepository,
        IUserAccountRepository userRepository,
        IJobTitleRepository jobTitleRepository,
        IPasswordHasher passwordHasher,
        IHierarchicalAuthorizationService authorizationService,
        IRoleManagementService roleManagementService,
        IEmployeeValidationService validationService,
        ILogger<CreateEmployeeHandler> logger)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jobTitleRepository = jobTitleRepository ?? throw new ArgumentNullException(nameof(jobTitleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _roleManagementService = roleManagementService ?? throw new ArgumentNullException(nameof(roleManagementService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateEmployeeResult> Handle(CreateEmployeeCommand command, CancellationToken ct, Guid? currentUserId = null)
    {
        try
    {
        _logger.LogInformation("Starting employee creation. Email: {Email}", command.Email);

            await ValidateInputAsync(command, ct);
            
            var userId = currentUserId ?? throw new UnauthorizedAccessException("Current user ID not provided.");
            
            var jobTitle = await GetJobTitleAsync(command.JobTitleId, ct);
            
            var canCreate = await _authorizationService.CanCreateEmployeeWithRoleAsync(userId, jobTitle.HierarchyLevel, ct);
            if (!canCreate)
            {
                var targetRoleLevel = _authorizationService.ConvertJobTitleLevelToHierarchicalRole(jobTitle.HierarchyLevel);
                _logger.LogWarning("User {CurrentUserId} cannot create employees with role level '{TargetRoleLevel}'", 
                    userId, targetRoleLevel);
                throw new UnauthorizedAccessException($"You cannot create employees with role level '{targetRoleLevel}'.");
            }

            await ValidateBusinessRulesAsync(command, ct);

            var employee = await CreateEmployeeAsync(command, ct);
            var role = await _roleManagementService.GetOrCreateRoleByHierarchyLevelAsync(jobTitle.HierarchyLevel, ct);
            var userAccount = await CreateUserAccountAsync(command, employee, role, ct);

            _logger.LogInformation("Employee and user account created successfully. EmployeeId: {EmployeeId}, Email: {Email}, Role: {Role}", 
                employee.Id, command.Email, role.Name);

            return new CreateEmployeeResult { Id = employee.Id };
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Validation error in value objects: {Message}", ex.Message);
            throw new ValidationException($"Validation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating employee: {Message}", ex.Message);
            throw;
        }
    }

    private Task ValidateInputAsync(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var request = new CreateEmployeeRequest
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            DocumentNumber = command.DocumentNumber,
            DateOfBirth = command.DateOfBirth,
            PhoneNumbers = command.Phones,
            JobTitleId = command.JobTitleId,
            DepartmentId = command.DepartmentId,
            Password = command.Password
        };

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for employee {Email}. Errors: {Errors}", 
                command.Email, string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(validation.Errors);
        }

        return Task.CompletedTask;
    }

    private async Task<JobTitle> GetJobTitleAsync(Guid jobTitleId, CancellationToken cancellationToken)
    {
        var jobTitle = await _jobTitleRepository.GetByIdAsync(jobTitleId, cancellationToken);
        if (jobTitle == null)
        {
            _logger.LogWarning("Job title not found. JobTitleId: {JobTitleId}", jobTitleId);
            throw new ArgumentException("Job title not found.", nameof(jobTitleId));
        }
        return jobTitle;
    }

    private async Task ValidateBusinessRulesAsync(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        await _validationService.ValidateEmailNotInUseAsync(command.Email, cancellationToken);
        await _validationService.ValidateDocumentNotInUseAsync(command.DocumentNumber, cancellationToken);
        await _validationService.ValidateDepartmentExistsAsync(command.DepartmentId, cancellationToken);
        await _validationService.ValidateJobTitleExistsAsync(command.JobTitleId, cancellationToken);
    }

    private async Task<Employee> CreateEmployeeAsync(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var document = new DocumentNumber(command.DocumentNumber.Trim());
        var dobDate = DateTime.ParseExact(
                command.DateOfBirth.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var dateOfBirth = new DateOfBirth(dobDate);
        var email = new Email(command.Email.Trim());
        
        var employee = Employee.Create(
            command.FirstName,
            command.LastName,
            email,
            document,
            dateOfBirth,
            command.Phones,
            command.JobTitleId,
            command.DepartmentId);

        await _employeeRepository.AddAsync(employee, cancellationToken);
        return employee;
    }

    private async Task<UserAccount> CreateUserAccountAsync(CreateEmployeeCommand command, Employee employee, Role role, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        
            var account = UserAccount.Create(
                userName: normalizedEmail,
            passwordHash: _passwordHasher.Hash(command.Password),
                employeeId: employee.Id,
            roleId: role.Id,
            jobTitleId: command.JobTitleId);
                
        await _userRepository.AddAsync(account, cancellationToken);
        return account;
}
}

public class CreateEmployeeResult
{
    public Guid Id { get; set; }
}
