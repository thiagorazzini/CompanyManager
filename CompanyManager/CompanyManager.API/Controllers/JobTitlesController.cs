using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Common;
using CompanyManager.API.Models;
using Microsoft.Extensions.Logging;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class JobTitlesController : ControllerBase
    {
        private readonly IListJobTitlesQueryHandler _listJobTitlesQueryHandler;
        private readonly IGetJobTitleByIdQueryHandler _getJobTitleByIdQueryHandler;
        private readonly ILogger<JobTitlesController> _logger;

        public JobTitlesController(
            IListJobTitlesQueryHandler listJobTitlesQueryHandler,
            IGetJobTitleByIdQueryHandler getJobTitleByIdQueryHandler,
            ILogger<JobTitlesController> logger)
        {
            _listJobTitlesQueryHandler = listJobTitlesQueryHandler ?? throw new ArgumentNullException(nameof(listJobTitlesQueryHandler));
            _getJobTitleByIdQueryHandler = getJobTitleByIdQueryHandler ?? throw new ArgumentNullException(nameof(getJobTitleByIdQueryHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lists all available job titles
        /// </summary>
        /// <param name="request">Job titles list request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of job titles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PageResult<Application.DTOs.JobTitleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PageResult<Application.DTOs.JobTitleResponse>>> List(
            [FromQuery] ListJobTitlesRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Job titles list attempt with null request");
                    return BadRequest(new ErrorResponse("Job titles list parameters are required"));
                }

                var result = await _listJobTitlesQueryHandler.Handle(request, cancellationToken);

                _logger.LogInformation("Job titles list retrieved successfully. Total: {Total}, Page: {Page}, PageSize: {PageSize}", 
                    result.Total, result.Page, result.PageSize);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job titles list retrieval operation cancelled");
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Job titles list retrieval failed - invalid parameters: {Message}", ex.Message);
                return BadRequest(new ErrorResponse("Invalid job titles list parameters. Please check your input and try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving job titles list");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Gets a specific job title by ID
        /// </summary>
        /// <param name="id">Job title ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Job title information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Application.DTOs.JobTitleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Application.DTOs.JobTitleResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving job title with ID: {JobTitleId}", id);

                var request = new GetJobTitleByIdRequest { Id = id };
                var result = await _getJobTitleByIdQueryHandler.Handle(request, cancellationToken);
                
                if (result == null)
                {
                    _logger.LogWarning("Job title not found with ID: {JobTitleId}", id);
                    return NotFound(new ErrorResponse("Job title not found. Please check the ID and try again."));
                }

                _logger.LogInformation("Job title retrieved successfully. ID: {JobTitleId}, Name: {Name}", 
                    result.Id, result.Name);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job title retrieval operation cancelled for ID: {JobTitleId}", id);
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Job title retrieval failed - invalid ID format: {JobTitleId}, Message: {Message}", 
                    id, ex.Message);
                return BadRequest(new ErrorResponse("Invalid job title ID format. Please check the ID and try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving job title with ID: {JobTitleId}", id);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Lists job titles available for user creation (based on current user's hierarchy)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available job titles</returns>
        [HttpGet("available-for-creation")]
        [ProducesResponseType(typeof(IEnumerable<Application.DTOs.JobTitleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Application.DTOs.JobTitleResponse>>> GetAvailableForCreation(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving job titles available for creation");

                // TODO: Implement hierarchical permission logic
                // For now, returns all active job titles
                var request = new ListJobTitlesRequest { IsActive = true };
                var result = await _listJobTitlesQueryHandler.Handle(request, cancellationToken);

                _logger.LogInformation("Job titles available for creation retrieved successfully. Count: {Count}", 
                    result.Items.Count());

                return Ok(result.Items);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job titles available for creation retrieval operation cancelled");
                return StatusCode(499, new ErrorResponse("Request was cancelled. Please try again."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Job titles available for creation retrieval failed - invalid parameters: {Message}", ex.Message);
                return BadRequest(new ErrorResponse("Invalid parameters for retrieving available job titles. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving job titles available for creation");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }
    }
}
