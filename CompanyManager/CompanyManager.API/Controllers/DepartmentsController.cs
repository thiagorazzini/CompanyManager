using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Queries;
using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/departments")]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ICreateDepartmentCommandHandler _createDepartmentHandler;
        private readonly IUpdateDepartmentCommandHandler _updateDepartmentHandler;
        private readonly IDeleteDepartmentCommandHandler _deleteDepartmentHandler;
        private readonly IGetDepartmentByIdQueryHandler _getByIdHandler;
        private readonly IListDepartmentsQueryHandler _listHandler;

        public DepartmentsController(
            ICreateDepartmentCommandHandler createDepartmentHandler,
            IUpdateDepartmentCommandHandler updateDepartmentHandler,
            IDeleteDepartmentCommandHandler deleteDepartmentHandler,
            IGetDepartmentByIdQueryHandler getByIdHandler,
            IListDepartmentsQueryHandler listHandler)
        {
            _createDepartmentHandler = createDepartmentHandler ?? throw new ArgumentNullException(nameof(createDepartmentHandler));
            _updateDepartmentHandler = updateDepartmentHandler ?? throw new ArgumentNullException(nameof(updateDepartmentHandler));
            _deleteDepartmentHandler = deleteDepartmentHandler ?? throw new ArgumentNullException(nameof(deleteDepartmentHandler));
            _getByIdHandler = getByIdHandler ?? throw new ArgumentNullException(nameof(getByIdHandler));
            _listHandler = listHandler ?? throw new ArgumentNullException(nameof(listHandler));
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

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving departments"));
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
        public async Task<IActionResult> GetDepartment(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var request = new GetDepartmentByIdRequest { Id = id };
                var result = await _getByIdHandler.Handle(request.Id, cancellationToken);

                if (result == null)
                {
                    return NotFound(new ErrorResponse("Department not found"));
                }

                var response = new DepartmentDetailDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving the department"));
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
        public async Task<IActionResult> CreateDepartment(
            [FromBody] CreateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

            try
            {
                var command = new CreateDepartmentCommand
                {
                    Name = request.Name,
                };

                var result = await _createDepartmentHandler.Handle(command, cancellationToken);

                var response = new DepartmentCreatedResponse
                {
                    Message = "Department created successfully",
                    DepartmentId = result
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = result }, response);
            }
            catch (ValidationException ex)
            {
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found"))
            {
                return NotFound(new ErrorResponse("Department not found"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new ErrorResponse("A department with this name already exists"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while creating the department"));
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
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

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

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found"))
            {
                return NotFound(new ErrorResponse("Department not found"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new ErrorResponse("A department with this name already exists"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while updating the department"));
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

                return Ok(response);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found"))
            {
                return NotFound(new ErrorResponse("Department not found"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cannot be deleted"))
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while deleting the department"));
            }
        }
    }
}
