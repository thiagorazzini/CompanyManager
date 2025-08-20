using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("departments")]
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
        /// Creates a new department
        /// </summary>
        /// <param name="request">Department creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created department information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDepartment(
            [FromBody] CreateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            try
            {
                var command = new CreateDepartmentCommand
                {
                    Name = request.Name
                };

                var departmentId = await _createDepartmentHandler.Handle(command, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = departmentId },
                    new { id = departmentId, name = request.Name, message = "Department created successfully" });
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while creating the department" });
            }
        }

        /// <summary>
        /// Updates an existing department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="request">Department update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated department information</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDepartment(
            Guid id,
            [FromBody] UpdateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            try
            {
                var command = new UpdateDepartmentCommand
                {
                    Id = id, // Use the URL ID directly
                    NewName = request.NewName
                };

                await _updateDepartmentHandler.Handle(command, cancellationToken);

                return Ok(new { id, name = request.NewName, message = "Department updated successfully" });
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found"))
            {
                return NotFound(new { error = "Department not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while updating the department" });
            }
        }

        /// <summary>
        /// Gets a department by ID
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Department information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var department = await _getByIdHandler.Handle(id, cancellationToken);

                if (department == null)
                    return NotFound(new { error = "Department not found" });

                return Ok(new
                {
                    id = department.Id,
                    name = department.Name,
                    description = department.Description,
                    isActive = department.IsActive,
                    createdAt = department.CreatedAt,
                    updatedAt = department.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while retrieving the department" });
            }
        }

        /// <summary>
        /// Lists all departments with optional filtering and pagination
        /// </summary>
        /// <param name="nameContains">Filter by department name (optional)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of departments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListDepartments(
            [FromQuery] string? nameContains = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new ListDepartmentsRequest
                {
                    NameContains = nameContains,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _listHandler.Handle(request, cancellationToken);

                return Ok(new
                {
                    items = result.Items.Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        description = d.Description,
                        isActive = d.IsActive,
                        createdAt = d.CreatedAt,
                        updatedAt = d.UpdatedAt
                    }),
                    total = result.Total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)result.Total / pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while retrieving departments" });
            }
        }

        /// <summary>
        /// Deletes a department by ID
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteDepartmentCommand { Id = id };
                await _deleteDepartmentHandler.Handle(command, cancellationToken);

                return Ok(new { message = "Department deleted successfully" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department not found"))
            {
                return NotFound(new { error = "Department not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while deleting the department" });
            }
        }
    }
}
