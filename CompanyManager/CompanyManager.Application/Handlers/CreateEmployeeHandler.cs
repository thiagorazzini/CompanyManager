using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
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
    private readonly IEmployeeRepository _employees;
    private readonly IUserAccountRepository _users;
    private readonly IDepartmentRepository _departments;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<CreateEmployeeHandler> _logger;

    public CreateEmployeeHandler(
        CreateEmployeeRequestValidator validator,
        IEmployeeRepository employees,
        IUserAccountRepository users,
        IDepartmentRepository departments,
        IPasswordHasher hasher,
        ILogger<CreateEmployeeHandler> logger)
    {
        _validator = validator;
        _employees = employees;
        _users = users;
        _departments = departments;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<CreateEmployeeResult> Handle(CreateEmployeeCommand command, CancellationToken ct, Guid? currentUserId = null)
    {
        _logger.LogInformation("Starting employee creation. Email: {Email}, Role: {RoleLevel}", 
            command.Email, command.RoleLevel);

        var request = new CreateEmployeeRequest
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            DocumentNumber = command.DocumentNumber,
            DateOfBirth = command.DateOfBirth,
            PhoneNumbers = command.Phones,
            JobTitle = command.JobTitle,
            DepartmentId = command.DepartmentId,
            ManagerId = command.ManagerId,
            Password = command.Password,
            RoleLevel = command.RoleLevel
        };

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for employee {Email}. Errors: {Errors}", 
                command.Email, string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(validation.Errors);
        }

        
        var userId = currentUserId ?? Guid.Empty; 

       
        var currentUser = await _users.GetByIdAsync(userId, ct);
        if (currentUser == null)
        {
            _logger.LogError("Current user not found. UserId: {CurrentUserId}", userId);
            throw new UnauthorizedAccessException("Current user not found.");
        }

        var targetRoleLevel = Enum.Parse<HierarchicalRole>(command.RoleLevel, true);
        if (!currentUser.CanCreateRole(targetRoleLevel))
        {
            _logger.LogWarning("Attempt to create employee with higher role. Current user role: {CurrentUserRole}, Requested role: {TargetRole}", 
                currentUser.GetHighestRoleLevel().GetDescription(), command.RoleLevel);
            throw new UnauthorizedAccessException($"You cannot create employees with role level '{command.RoleLevel}'. Your highest role level is '{currentUser.GetHighestRoleLevel().GetDescription()}'.");
        }

        if (!await _departments.ExistsAsync(command.DepartmentId, ct))
        {
            _logger.LogWarning("Department does not exist. DepartmentId: {DepartmentId}", command.DepartmentId);
            throw new ArgumentException("Department does not exist.", nameof(command.DepartmentId));
        }

        var normalizedEmail = (command.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await _employees.EmailExistsAsync(normalizedEmail, ct))
        {
            _logger.LogWarning("Email already in use: {Email}", normalizedEmail);
            throw new InvalidOperationException("Email already in use.");
        }

        var cpfForCheck = new DocumentNumber(command.DocumentNumber);
        if (await _employees.CpfExistsAsync(cpfForCheck.Digits, ct))
        {
            _logger.LogWarning("Document number already in use: {CPF}", cpfForCheck.Digits);
            throw new InvalidOperationException("Document number already in use.");
        }

        var email = new Email(command.Email!.Trim());
        var document = new DocumentNumber(command.DocumentNumber.Trim());

        var dobDate = DateTime.ParseExact(
            command.DateOfBirth.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        var dateOfBirth = new DateOfBirth(dobDate);
        var phones = command.Phones
            .Select(p => new PhoneNumber(p, defaultCountry: "BR"))
            .ToArray();

        var employee = Employee.Create(
            command.FirstName,
            command.LastName,
            email,
            document,
            dateOfBirth,
            phones,
            command.JobTitle,
            command.DepartmentId,
            command.ManagerId);

        await _employees.AddAsync(employee, ct);

        var account = UserAccount.Create(
            userName: normalizedEmail,
            passwordHash: _hasher.Hash(command.Password),
            employeeId: employee.Id);


        var role = new Role(command.JobTitle, targetRoleLevel);
        account.AddRole(role);

        await _users.AddAsync(account, ct);

        _logger.LogInformation("Employee and user account created successfully. EmployeeId: {EmployeeId}, Email: {Email}, Role: {Role}", 
            employee.Id, normalizedEmail, targetRoleLevel);

        return new CreateEmployeeResult { Id = employee.Id };
    }
}

public class CreateEmployeeResult
{
    public Guid Id { get; set; }
}