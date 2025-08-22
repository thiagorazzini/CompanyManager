using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Services;

/// <summary>
/// Service for validating employee creation requirements
/// </summary>
public interface IEmployeeValidationService
{
    /// <summary>
    /// Validates that email is not already in use
    /// </summary>
    Task ValidateEmailNotInUseAsync(string email, CancellationToken cancellationToken);
    
    /// <summary>
    /// Validates that document number is not already in use
    /// </summary>
    Task ValidateDocumentNotInUseAsync(string documentNumber, CancellationToken cancellationToken);
    
    /// <summary>
    /// Validates that department exists
    /// </summary>
    Task ValidateDepartmentExistsAsync(Guid departmentId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Validates that job title exists
    /// </summary>
    Task ValidateJobTitleExistsAsync(Guid jobTitleId, CancellationToken cancellationToken);
}
