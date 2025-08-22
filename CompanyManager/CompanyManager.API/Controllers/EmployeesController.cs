using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Queries;
using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    [Produces("application/json")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly CreateEmployeeHandler _createEmployeeHandler;
        private readonly IUpdateEmployeeCommandHandler _updateEmployeeHandler;
        private readonly IListEmployeesQueryHandler _listEmployeesHandler;
        private readonly IGetEmployeeByIdQueryHandler _getEmployeeByIdHandler;
        private readonly IDeleteEmployeeCommandHandler _deleteEmployeeHandler;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            CreateEmployeeHandler createEmployeeHandler,
            IUpdateEmployeeCommandHandler updateEmployeeHandler,
            IListEmployeesQueryHandler listEmployeesHandler,
            IGetEmployeeByIdQueryHandler getEmployeeByIdHandler,
            IDeleteEmployeeCommandHandler deleteEmployeeHandler,
            ILogger<EmployeesController> logger)
        {
            _createEmployeeHandler = createEmployeeHandler ?? throw new ArgumentNullException(nameof(createEmployeeHandler));
            _updateEmployeeHandler = updateEmployeeHandler ?? throw new ArgumentNullException(nameof(updateEmployeeHandler));
            _listEmployeesHandler = listEmployeesHandler ?? throw new ArgumentNullException(nameof(listEmployeesHandler));
            _getEmployeeByIdHandler = getEmployeeByIdHandler ?? throw new ArgumentNullException(nameof(getEmployeeByIdHandler));
            _deleteEmployeeHandler = deleteEmployeeHandler ?? throw new ArgumentNullException(nameof(deleteEmployeeHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lists employees with pagination and filtering
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="nameContains">Filter by name (optional)</param>
        /// <param name="departmentId">Filter by department ID (optional)</param>
        /// <param name="jobTitleId">Filter by job title ID (optional)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of employees</returns>
        [HttpGet]
        [ProducesResponseType(typeof(EmployeeListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployees(
            CancellationToken cancellationToken,
            [FromQuery] string? nameContains = null,
            [FromQuery] Guid? departmentId = null,
            [FromQuery] Guid? jobTitleId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("Invalid pagination parameters: Page={Page}, PageSize={PageSize}", page, pageSize);
                    return BadRequest(new ErrorResponse("Invalid pagination parameters. Page must be 1 or greater, and page size must be between 1 and 100."));
                }

                var request = new ListEmployeesRequest
                {
                    NameContains = nameContains,
                    DepartmentId = departmentId,
                    JobTitleId = jobTitleId,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _listEmployeesHandler.Handle(request, cancellationToken);

                var response = new EmployeeListResponse
                {
                    Items = result.Items.Select(e => new EmployeeSummaryDto
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.Email.Value,
                        JobTitle = e.JobTitle?.Name ?? string.Empty,
                        DepartmentId = e.DepartmentId,
                        DepartmentName = e.Department?.Name ?? string.Empty,
                        CreatedAt = e.CreatedAt
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total,
                        TotalPages = (int)Math.Ceiling((double)result.Total / pageSize)
                    }
                };

                _logger.LogInformation("Employees list retrieved successfully. Total: {Total}, Page: {Page}, PageSize: {PageSize}", 
                    result.Total, page, pageSize);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Employees list retrieval operation cancelled");
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving employees list");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Gets an employee by ID
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Employee information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployee(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving employee with ID: {EmployeeId}", id);

                var request = new GetEmployeeByIdRequest { Id = id };
                var result = await _getEmployeeByIdHandler.Handle(request, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Employee not found with ID: {EmployeeId}", id);
                    return NotFound(new ErrorResponse("Employee not found. Please check the ID and try again."));
                }

                var response = new EmployeeDetailDto
                {
                    Id = result.Id,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    Email = result.Email,
                    DocumentNumber = result.DocumentNumber,
                    DateOfBirth = result.DateOfBirth,
                    PhoneNumbers = result.PhoneNumbers,
                    JobTitleId = result.JobTitleId,
                    JobTitleName = result.JobTitleName,
                    DepartmentId = result.DepartmentId,
                    DepartmentName = result.DepartmentName,
                    Roles = result.Roles,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt
                };

                _logger.LogInformation("Successfully retrieved employee: {EmployeeId}, Email: {Email}", 
                    result.Id, result.Email);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Employee retrieval operation cancelled for ID: {EmployeeId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving employee with ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Creates a new employee
        /// </summary>
        /// <param name="request">Employee creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created employee information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeCreatedResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Employee creation attempt with null request body");
                return BadRequest(new ErrorResponse("Employee data is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Employee creation attempt with empty email");
                return BadRequest(new ErrorResponse("Employee email is required"));
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                _logger.LogWarning("Employee creation attempt with empty first name");
                return BadRequest(new ErrorResponse("Employee first name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                _logger.LogWarning("Employee creation attempt with empty last name");
                return BadRequest(new ErrorResponse("Employee last name is required"));
            }

            _logger.LogInformation("Creating employee with email: {Email}", request.Email);
            
            try
            {
                var command = new CreateEmployeeCommand
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    DocumentNumber = request.DocumentNumber,
                    DateOfBirth = request.DateOfBirth,
                    Phones = request.PhoneNumbers?.ToList() ?? new List<string>(),
                    JobTitleId = request.JobTitleId,
                    DepartmentId = request.DepartmentId,
                    Password = request.Password
                };

                // Obter o ID do usuário atual para validação hierárquica
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
                {
                    _logger.LogError("Unable to extract user ID from JWT token");
                    return Unauthorized(new ErrorResponse("Invalid authentication token. Please log in again."));
                }
                
                var result = await _createEmployeeHandler.Handle(command, cancellationToken, userId);
                
                _logger.LogInformation("Employee created successfully. ID: {EmployeeId}, Email: {Email}", 
                    result.Id, request.Email);
                               
                var response = new EmployeeCreatedResponse
                {
                    Message = "Employee created successfully",
                    EmployeeId = result.Id
                };

                return CreatedAtAction(nameof(GetEmployee), new { id = result.Id }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed when creating employee: {Email}. Errors: {Errors}", 
                    request.Email, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                
                return BadRequest(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee creation failed - email already exists: {Email}", request.Email);
                return Conflict(new ErrorResponse("An employee with this email already exists. Please use a different email address."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("hierarchical", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee creation failed - hierarchical validation failed: {Email}, Error: {Error}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("You cannot create an employee with a job title equal to or higher than your current level."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("department", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee creation failed - department validation failed: {Email}, Error: {Error}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("The specified department does not exist or is not active."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("job title", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee creation failed - job title validation failed: {Email}, Error: {Error}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("The specified job title does not exist or is not active."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Employee creation operation cancelled for email: {Email}", request.Email);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating employee: {Email}", request.Email);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Updates an existing employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="request">Employee update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeUpdatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployee(
            Guid id,
            [FromBody] UpdateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Employee update attempt with null request body for ID: {EmployeeId}", id);
                return BadRequest(new ErrorResponse("Employee update data is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Employee update attempt with empty email for ID: {EmployeeId}", id);
                return BadRequest(new ErrorResponse("Employee email is required"));
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                _logger.LogWarning("Employee update attempt with empty first name for ID: {EmployeeId}", id);
                return BadRequest(new ErrorResponse("Employee first name is required"));
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                _logger.LogWarning("Employee update attempt with empty last name for ID: {EmployeeId}", id);
                return BadRequest(new ErrorResponse("Employee last name is required"));
            }

            _logger.LogInformation("Updating employee ID: {EmployeeId}, Email: {Email}", id, request.Email);
            
            try
            {
                var command = new UpdateEmployeeCommand
                {
                    Id = id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    DocumentNumber = request.DocumentNumber ?? string.Empty,
                    Phones = request.PhoneNumbers?.ToList() ?? new List<string>(),
                    JobTitleId = request.JobTitleId,
                    DepartmentId = request.DepartmentId,
                    Password = request.Password
                };

                // Obter o ID do usuário atual para validação hierárquica
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
                {
                    _logger.LogError("Unable to extract user ID from JWT token for employee update ID: {EmployeeId}", id);
                    return Unauthorized(new ErrorResponse("Invalid authentication token. Please log in again."));
                }

                await _updateEmployeeHandler.Handle(command, cancellationToken, userId);
                
                _logger.LogInformation("Employee updated successfully. ID: {EmployeeId}", id);
                               
                var response = new EmployeeUpdatedResponse
                {
                    Message = "Employee updated successfully"
                };

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed when updating employee ID: {EmployeeId}. Errors: {Errors}", 
                    id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Employee not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee update failed - employee not found. ID: {EmployeeId}", id);
                return NotFound(new ErrorResponse("Employee not found. Please check the ID and try again."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department does not exist", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee update failed - department does not exist. ID: {EmployeeId}, DepartmentId: {DepartmentId}", 
                    id, request.DepartmentId);
                return BadRequest(new ErrorResponse("The specified department does not exist. Please choose a valid department."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Job title does not exist", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee update failed - job title does not exist. ID: {EmployeeId}, JobTitleId: {JobTitleId}", 
                    id, request.JobTitleId);
                return BadRequest(new ErrorResponse("The specified job title does not exist. Please choose a valid job title."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee update failed - email already in use. ID: {EmployeeId}, Email: {Email}, Error: {Error}", 
                    id, request.Email, ex.Message);
                return Conflict(new ErrorResponse("This email is already in use by another employee. Please choose a different email address."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("hierarchical", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee update failed - hierarchical validation failed. ID: {EmployeeId}, Error: {Error}", 
                    id, ex.Message);
                return BadRequest(new ErrorResponse("You cannot assign a job title equal to or higher than your current level."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Employee update operation cancelled for ID: {EmployeeId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when updating employee ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Deletes an employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeDeletedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting employee with ID: {EmployeeId}", id);

                var command = new DeleteEmployeeCommand { Id = id };
                await _deleteEmployeeHandler.Handle(command, cancellationToken);

                _logger.LogInformation("Employee deleted successfully. ID: {EmployeeId}", id);

                var response = new EmployeeDeletedResponse
                {
                    Message = "Employee deleted successfully"
                };

                return Ok(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Employee not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee deletion failed - employee not found. ID: {EmployeeId}", id);
                return NotFound(new ErrorResponse("Employee not found. Please check the ID and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be deleted", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Employee deletion failed - cannot be deleted. ID: {EmployeeId}, Error: {Error}", 
                    id, ex.Message);
                
                if (ex.Message.Contains("active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ErrorResponse("This employee cannot be deleted because they are currently active. Please deactivate them first."));
                }
                else if (ex.Message.Contains("in use", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ErrorResponse("This employee cannot be deleted because they are currently in use by the system. Please contact support for assistance."));
                }
                else
                {
                    return BadRequest(new ErrorResponse(ex.Message));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Employee deletion operation cancelled for ID: {EmployeeId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when deleting employee ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }
    }
}
