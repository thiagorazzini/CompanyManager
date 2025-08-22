using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for validating employee creation requirements
/// </summary>
public class EmployeeValidationService : IEmployeeValidationService
{
    private readonly IUserAccountRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IJobTitleRepository _jobTitleRepository;
    private readonly ILogger<EmployeeValidationService> _logger;

    public EmployeeValidationService(
        IUserAccountRepository userRepository,
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IJobTitleRepository jobTitleRepository,
        ILogger<EmployeeValidationService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _jobTitleRepository = jobTitleRepository ?? throw new ArgumentNullException(nameof(jobTitleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ValidateEmailNotInUseAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        
        if (existingUser != null)
        {
            _logger.LogWarning("Email already in use: {Email}", normalizedEmail);
            throw new ArgumentException($"Email '{email}' is already in use.");
        }
    }

    public async Task ValidateDocumentNotInUseAsync(string documentNumber, CancellationToken cancellationToken)
    {
        var normalizedDocument = documentNumber.Trim();
        var existingEmployee = await _employeeRepository.ExistsByDocumentAsync(normalizedDocument, cancellationToken);
        
        if (existingEmployee)
        {
            _logger.LogWarning("Document number already in use: {DocumentNumber}", normalizedDocument);
            throw new ArgumentException($"Document number '{documentNumber}' is already in use by another employee.");
        }
    }

    public async Task ValidateDepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        
        if (department == null)
        {
            _logger.LogWarning("Department not found. DepartmentId: {DepartmentId}", departmentId);
            throw new ArgumentException("Department not found.", nameof(departmentId));
        }
    }

    public async Task ValidateJobTitleExistsAsync(Guid jobTitleId, CancellationToken cancellationToken)
    {
        var jobTitle = await _jobTitleRepository.GetByIdAsync(jobTitleId, cancellationToken);
        
        if (jobTitle == null)
        {
            _logger.LogWarning("Job title not found. JobTitleId: {JobTitleId}", jobTitleId);
            throw new ArgumentException("Job title not found.", nameof(jobTitleId));
        }
    }
}
