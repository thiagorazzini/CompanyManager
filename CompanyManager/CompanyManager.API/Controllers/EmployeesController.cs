using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Queries;
using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/employees")]
    [Produces("application/json")]
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
        /// <param name="jobTitle">Filter by job title (optional)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of employees</returns>
        [HttpGet]
        [ProducesResponseType(typeof(EmployeeListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEmployees(
            CancellationToken cancellationToken,
            [FromQuery] string? nameContains = null,
            [FromQuery] Guid? departmentId = null,
            [FromQuery] string? jobTitle = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new ErrorResponse("Invalid pagination parameters"));
                }

                var request = new ListEmployeesRequest
                {
                    NameContains = nameContains,
                    DepartmentId = departmentId,
                    JobTitle = jobTitle,
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
                        JobTitle = e.JobTitle,
                        DepartmentId = e.DepartmentId,
                        DepartmentName = null, // TODO: Get department name from department repository
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees list");
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving employees"));
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
                    return NotFound(new ErrorResponse("Employee not found"));
                }

                var response = new EmployeeDetailDto
                {
                    Id = result.Id,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    Email = result.Email,
                    DocumentNumber = result.DocumentNumber,
                    DateOfBirth = result.DateOfBirth,
                    PhoneNumbers = result.PhoneNumbers.ToArray(),
                    JobTitle = result.JobTitle,
                    DepartmentId = result.DepartmentId,
                    ManagerId = result.ManagerId,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt
                };

                _logger.LogInformation("Successfully retrieved employee: {EmployeeId}, Email: {Email}", 
                    result.Id, result.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving the employee"));
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
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

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
                    Phones = request.PhoneNumbers?.ToArray() ?? Array.Empty<string>(),
                    JobTitle = request.JobTitle,
                    DepartmentId = request.DepartmentId,
                    ManagerId = request.ManagerId
                };

                var result = await _createEmployeeHandler.Handle(command, cancellationToken);
                
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
                
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                
                return BadRequest(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning("Employee already exists: {Email}", request.Email);
                return Conflict(new ErrorResponse("An employee with this email already exists"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating employee: {Email}", request.Email);
                return StatusCode(500, new ErrorResponse("An error occurred while creating the employee"));
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
        public async Task<IActionResult> UpdateEmployee(
            Guid id,
            [FromBody] UpdateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

            _logger.LogInformation("Updating employee ID: {EmployeeId}, Email: {Email}", id, request.Email ?? "null");
            
            try
            {
                var command = new UpdateEmployeeCommand
                {
                    Id = id,
                    FirstName = request.FirstName ?? string.Empty,
                    LastName = request.LastName ?? string.Empty,
                    Email = request.Email ?? string.Empty,
                    DocumentNumber = request.DocumentNumber ?? string.Empty,
                    Phones = request.PhoneNumbers?.ToArray() ?? Array.Empty<string>(),
                    JobTitle = request.JobTitle ?? string.Empty,
                    DepartmentId = request.DepartmentId,
                    ManagerId = request.ManagerId
                };

                await _updateEmployeeHandler.Handle(command, cancellationToken);
                
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
                
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Employee not found"))
            {
                _logger.LogWarning("Employee not found for update. ID: {EmployeeId}", id);
                return NotFound(new ErrorResponse("Employee not found"));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department does not exist"))
            {
                _logger.LogWarning("Department does not exist for employee ID: {EmployeeId}. DepartmentId: {DepartmentId}", 
                    id, request.DepartmentId);
                return BadRequest(new ErrorResponse("Department does not exist"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already in use"))
            {
                _logger.LogWarning("Conflict when updating employee ID: {EmployeeId}. Error: {Error}", 
                    id, ex.Message);
                return Conflict(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when updating employee ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("An error occurred while updating the employee"));
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
            catch (ArgumentException ex) when (ex.Message.Contains("Employee not found"))
            {
                _logger.LogWarning("Employee not found for deletion. ID: {EmployeeId}", id);
                return NotFound(new ErrorResponse("Employee not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when deleting employee ID: {EmployeeId}", id);
                return StatusCode(500, new ErrorResponse("An error occurred while deleting the employee"));
            }
        }
    }
}
