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
    [Route("api/v1/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly ICreateDepartmentCommandHandler _createDepartmentHandler;
        private readonly IUpdateDepartmentCommandHandler _updateDepartmentHandler;
        private readonly IDeleteDepartmentCommandHandler _deleteDepartmentHandler;
        private readonly IGetDepartmentByIdQueryHandler _getByIdHandler;
        private readonly IListDepartmentsQueryHandler _listHandler;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(
            ICreateDepartmentCommandHandler createDepartmentHandler,
            IUpdateDepartmentCommandHandler updateDepartmentHandler,
            IDeleteDepartmentCommandHandler deleteDepartmentHandler,
            IGetDepartmentByIdQueryHandler getByIdHandler,
            IListDepartmentsQueryHandler listHandler,
            ILogger<DepartmentsController> logger)
        {
            _createDepartmentHandler = createDepartmentHandler ?? throw new ArgumentNullException(nameof(createDepartmentHandler));
            _updateDepartmentHandler = updateDepartmentHandler ?? throw new ArgumentNullException(nameof(updateDepartmentHandler));
            _deleteDepartmentHandler = deleteDepartmentHandler ?? throw new ArgumentNullException(nameof(deleteDepartmentHandler));
            _getByIdHandler = getByIdHandler ?? throw new ArgumentNullException(nameof(getByIdHandler));
            _listHandler = listHandler ?? throw new ArgumentNullException(nameof(listHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a paginated list of departments
        /// </summary>
        /// <param name="nameContains">Filter by department name (optional)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of departments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(DepartmentListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] string? nameContains = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var request = new ListDepartmentsRequest
                {
                    NameContains = nameContains,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _listHandler.Handle(request, cancellationToken);

                var response = new DepartmentListResponse
                {
                    Items = result.Items.Select(d => new DepartmentSummaryDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        CreatedAt = d.CreatedAt
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total,
                        TotalPages = (int)Math.Ceiling((double)result.Total / pageSize)
                    }
                };

                _logger.LogInformation("Departments list retrieved successfully. Total: {Total}, Page: {Page}, PageSize: {PageSize}", 
                    result.Total, page, pageSize);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Departments list retrieval operation cancelled");
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving departments list");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Retrieves a department by ID
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Department details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DepartmentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDepartment(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var request = new GetDepartmentByIdRequest { Id = id };
                var result = await _getByIdHandler.Handle(request.Id, cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Department not found with ID: {DepartmentId}", id);
                    return NotFound(new ErrorResponse("Department not found. Please check the ID and try again."));
                }

                var response = new DepartmentDetailDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt
                };

                _logger.LogInformation("Department retrieved successfully. ID: {DepartmentId}, Name: {Name}", 
                    result.Id, result.Name);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Department retrieval operation cancelled for ID: {DepartmentId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving department with ID: {DepartmentId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Creates a new department
        /// </summary>
        /// <param name="request">Department creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created department information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentCreatedResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDepartment(
            [FromBody] CreateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Department creation attempt with null request body");
                return BadRequest(new ErrorResponse("Department data is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Department creation attempt with empty name");
                return BadRequest(new ErrorResponse("Department name is required"));
            }

            try
            {
                var command = new CreateDepartmentCommand
                {
                    Name = request.Name,
                    Description = request.Description,
                };

                var result = await _createDepartmentHandler.Handle(command, cancellationToken);

                var response = new DepartmentCreatedResponse
                {
                    Message = "Department created successfully",
                    DepartmentId = result
                };

                _logger.LogInformation("Department created successfully. ID: {DepartmentId}, Name: {Name}", 
                    result, request.Name);

                return CreatedAtAction(nameof(GetDepartment), new { id = result }, response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Department creation validation failed: {Errors}", 
                    string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department creation failed - department not found: {Message}", ex.Message);
                return NotFound(new ErrorResponse("Department not found. Please check your input and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department creation failed - name already exists: {Name}", request.Name);
                return Conflict(new ErrorResponse("A department with this name already exists. Please choose a different name."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department creation failed - invalid data: {Message}", ex.Message);
                return BadRequest(new ErrorResponse("Invalid department data provided. Please check your input and try again."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Department creation operation cancelled for name: {Name}", request.Name);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating department with name: {Name}", request.Name);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Updates an existing department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="request">Department update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(DepartmentUpdatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Department update attempt with null request body for ID: {DepartmentId}", id);
                return BadRequest(new ErrorResponse("Department update data is required"));
            }

            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                _logger.LogWarning("Department update attempt with empty name for ID: {DepartmentId}", id);
                return BadRequest(new ErrorResponse("New department name is required"));
            }

            try
            {
                var command = new UpdateDepartmentCommand
                {
                    Id = id,
                    NewName = request.NewName,
                };

                await _updateDepartmentHandler.Handle(command, cancellationToken);

                var response = new DepartmentUpdatedResponse
                {
                    Message = "Department updated successfully"
                };

                _logger.LogInformation("Department updated successfully. ID: {DepartmentId}, New Name: {NewName}", 
                    id, request.NewName);

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Department update validation failed for ID {DepartmentId}: {Errors}", 
                    id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department update failed - department not found. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                return NotFound(new ErrorResponse("Department not found. Please check the ID and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department update failed - name already exists. ID: {DepartmentId}, New Name: {NewName}", 
                    id, request.NewName);
                return Conflict(new ErrorResponse("A department with this name already exists. Please choose a different name."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department update failed - invalid data. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                return BadRequest(new ErrorResponse("Invalid department data provided. Please check your input and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be updated", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department update failed - cannot be updated. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                return BadRequest(new ErrorResponse("This department cannot be updated. It may be in use by employees or other system components."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Department update operation cancelled for ID: {DepartmentId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating department with ID: {DepartmentId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Deletes a department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(DepartmentDeletedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteDepartmentCommand { Id = id };
                await _deleteDepartmentHandler.Handle(command, cancellationToken);

                var response = new DepartmentDeletedResponse
                {
                    Message = "Department deleted successfully"
                };

                _logger.LogInformation("Department deleted successfully. ID: {DepartmentId}", id);

                return Ok(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department deletion failed - department not found. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                return NotFound(new ErrorResponse("Department not found. Please check the ID and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be deleted", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department deletion failed - cannot be deleted. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                
                if (ex.Message.Contains("employees", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ErrorResponse("This department cannot be deleted because it has employees assigned to it. Please reassign or remove all employees first."));
                }
                else if (ex.Message.Contains("active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ErrorResponse("This department cannot be deleted because it is currently active. Please deactivate it first."));
                }
                else
                {
                    return BadRequest(new ErrorResponse(ex.Message));
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("in use", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Department deletion failed - department in use. ID: {DepartmentId}, Message: {Message}", 
                    id, ex.Message);
                return BadRequest(new ErrorResponse("This department cannot be deleted because it is currently in use by the system. Please contact support for assistance."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Department deletion operation cancelled for ID: {DepartmentId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting department with ID: {DepartmentId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }
    }
}
