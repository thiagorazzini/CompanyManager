using CompanyManager.Domain.Interfaces;
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

    public async Task<Guid> Handle(CreateEmployeeRequest request, Guid currentUserId, CancellationToken ct)
    {
        _logger.LogInformation("Starting employee creation. Email: {Email}, Role: {RoleLevel}", 
            request.Email, request.RoleLevel);

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for employee {Email}. Errors: {Errors}", 
                request.Email, string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(validation.Errors);
        }

        // Validar permissões hierárquicas
        var currentUser = await _users.GetByIdAsync(currentUserId, ct);
        if (currentUser == null)
        {
            _logger.LogError("Current user not found. UserId: {CurrentUserId}", currentUserId);
            throw new UnauthorizedAccessException("Current user not found.");
        }

        var targetRoleLevel = Enum.Parse<HierarchicalRole>(request.RoleLevel, true);
        if (!currentUser.CanCreateRole(targetRoleLevel))
        {
            _logger.LogWarning("Attempt to create employee with higher role. Current user role: {CurrentUserRole}, Requested role: {TargetRole}", 
                currentUser.GetHighestRoleLevel().GetDescription(), request.RoleLevel);
            throw new UnauthorizedAccessException($"You cannot create employees with role level '{request.RoleLevel}'. Your highest role level is '{currentUser.GetHighestRoleLevel().GetDescription()}'.");
        }

        if (!await _departments.ExistsAsync(request.DepartmentId, ct))
        {
            _logger.LogWarning("Department does not exist. DepartmentId: {DepartmentId}", request.DepartmentId);
            throw new ArgumentException("Department does not exist.", nameof(request.DepartmentId));
        }

        var normalizedEmail = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await _employees.EmailExistsAsync(normalizedEmail, ct))
        {
            _logger.LogWarning("Email already in use: {Email}", normalizedEmail);
            throw new InvalidOperationException("Email already in use.");
        }

        var cpfForCheck = new DocumentNumber(request.DocumentNumber);
        if (await _employees.CpfExistsAsync(cpfForCheck.Digits, ct))
        {
            _logger.LogWarning("Document number already in use: {CPF}", cpfForCheck.Digits);
            throw new InvalidOperationException("Document number already in use.");
        }

        var email = new Email(request.Email!.Trim());
        var document = new DocumentNumber(request.DocumentNumber.Trim());

        var dobDate = DateTime.ParseExact(
            request.DateOfBirth.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        var dateOfBirth = new DateOfBirth(dobDate);
        var phones = request.PhoneNumbers
            .Select(p => new PhoneNumber(p, defaultCountry: "BR"))
            .ToArray();


        var employee = Employee.Create(
            request.FirstName,
            request.LastName,
            email,
            document,
            dateOfBirth,
            phones,
            request.JobTitle,
            request.DepartmentId,
            request.ManagerId);

        await _employees.AddAsync(employee, ct);

        var account = UserAccount.Create(
            userName: normalizedEmail,
            passwordHash: _hasher.Hash(request.Password),
            employeeId: employee.Id);

        // Criar e atribuir o role apropriado
        var role = new Role(request.JobTitle, targetRoleLevel);
        account.AddRole(role);

        await _users.AddAsync(account, ct);

        _logger.LogInformation("Employee and user account created successfully. EmployeeId: {EmployeeId}, Email: {Email}, Role: {Role}", 
            employee.Id, normalizedEmail, targetRoleLevel);

        return employee.Id;
    }
}