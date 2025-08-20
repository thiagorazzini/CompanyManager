using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly CreateEmployeeHandler _createEmployeeHandler;
        private readonly IUpdateEmployeeCommandHandler _updateEmployeeHandler;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            CreateEmployeeHandler createEmployeeHandler,
            IUpdateEmployeeCommandHandler updateEmployeeHandler,
            ILogger<EmployeesController> logger)
        {
            _createEmployeeHandler = createEmployeeHandler ?? throw new ArgumentNullException(nameof(createEmployeeHandler));
            _updateEmployeeHandler = updateEmployeeHandler ?? throw new ArgumentNullException(nameof(updateEmployeeHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new employee
        /// </summary>
        /// <param name="request">Employee creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created employee response</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateEmployeeResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new employee: {Email}", request.Email);
            
            try
            {
                // TODO: Obter o ID do usuário atual do contexto de autenticação
                // Por enquanto, usando um GUID vazio como placeholder
                var currentUserId = Guid.Empty; // Isso deve vir do contexto de autenticação
                
                var employeeId = await _createEmployeeHandler.Handle(request, currentUserId, cancellationToken);
                
                _logger.LogInformation("Employee created successfully. ID: {EmployeeId}, Email: {Email}", 
                    employeeId, request.Email);
                               
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = employeeId },
                    new { id = employeeId, message = "Employee created successfully" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed when creating employee: {Email}. Errors: {Errors}", 
                    request.Email, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating employee: {Email}", request.Email);
                return StatusCode(500, new { error = "An error occurred while creating the employee" });
            }
        }

        /// <summary>
        /// Gets an employee by ID (stub implementation)
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Employee information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            // Stub implementation - returns basic info without database access
            var result = new { id };
            return Task.FromResult<IActionResult>(Ok(result));
        }

        /// <summary>
        /// Updates an existing employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="request">Employee update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated employee information</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployee(
            Guid id,
            [FromBody] UpdateEmployeeRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating employee ID: {EmployeeId}, Email: {Email}", id, request.Email);
            
            try
            {
                // Garantir que o ID da URL seja usado
                request.Id = id;

                var command = new UpdateEmployeeCommand
                {
                    Id = request.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    DocumentNumber = request.DocumentNumber,
                    Phones = request.PhoneNumbers?.ToArray(),
                    JobTitle = request.JobTitle,
                    DepartmentId = request.DepartmentId,
                    ManagerId = request.ManagerId
                };

                await _updateEmployeeHandler.Handle(command, cancellationToken);
                
                _logger.LogInformation("Employee updated successfully. ID: {EmployeeId}", id);
                               
                return Ok(new { message = "Employee updated successfully", id });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed when updating employee ID: {EmployeeId}. Errors: {Errors}", 
                    id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Employee not found"))
            {
                _logger.LogWarning("Employee not found for update. ID: {EmployeeId}", id);
                return NotFound(new { error = "Employee not found" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Department does not exist"))
            {
                _logger.LogWarning("Department does not exist for employee ID: {EmployeeId}. DepartmentId: {DepartmentId}", 
                    id, request.DepartmentId);
                return BadRequest(new { error = "Department does not exist" });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already in use"))
            {
                _logger.LogWarning("Invalid operation when updating employee ID: {EmployeeId}. Error: {Error}", 
                    id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when updating employee ID: {EmployeeId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the employee" });
            }
        }
    }
}
