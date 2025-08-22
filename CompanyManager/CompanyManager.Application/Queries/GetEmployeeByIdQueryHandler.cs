using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Queries;

public sealed class GetEmployeeByIdQueryHandler : IGetEmployeeByIdQueryHandler
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

    public GetEmployeeByIdQueryHandler(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<GetEmployeeByIdQueryHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeByIdResponse?> Handle(GetEmployeeByIdRequest request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee ID provided: {Id}", request.Id);
            return null;
        }

        _logger.LogInformation("Retrieving employee with ID: {EmployeeId}", request.Id);

        var employee = await _employeeRepository.GetByIdWithJobTitleAsync(request.Id, cancellationToken);
        if (employee == null)
        {
            _logger.LogInformation("Employee not found with ID: {EmployeeId}", request.Id);
            return null;
        }


        var department = await _departmentRepository.GetByIdAsync(employee.DepartmentId, cancellationToken);
        
        var response = new GetEmployeeByIdResponse
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email.Value,
            DocumentNumber = employee.DocumentNumber.Raw,
            DateOfBirth = employee.DateOfBirth.BirthDate.ToString("yyyy-MM-dd"),
            PhoneNumbers = employee.Phones.Select(p => p.PhoneNumber.Raw).ToList(),
            JobTitleId = employee.JobTitle?.Id ?? Guid.Empty,
            JobTitleName = employee.JobTitle?.Name ?? string.Empty,
            DepartmentId = employee.DepartmentId,
            DepartmentName = department?.Name ?? string.Empty,
            Roles = new List<string>(), // Roles serão preenchidas pelo controller
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };

        _logger.LogInformation("Successfully retrieved employee: {EmployeeId}, Email: {Email}", 
            employee.Id, employee.Email.Value);

        return response;
    }
}
