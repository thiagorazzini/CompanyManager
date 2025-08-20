using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using CompanyManager.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManager.API.Controllers
{
    /// <summary>
    /// Controller for managing job titles
    /// </summary>
    [ApiController]
    [Route("api/v1/job-titles")]
    [Produces("application/json")]
    public class JobTitlesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<JobTitlesController> _logger;

        public JobTitlesController(
            IEmployeeRepository employeeRepository,
            ILogger<JobTitlesController> logger)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all available job titles in the system
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available job titles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(JobTitlesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobTitles(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all available job titles");

                var jobTitles = await _employeeRepository.GetDistinctJobTitlesAsync(cancellationToken);
                var jobTitlesList = jobTitles.ToList();

                var response = new JobTitlesResponse
                {
                    JobTitles = jobTitlesList,
                    Total = jobTitlesList.Count,
                    Message = "Job titles retrieved successfully"
                };

                _logger.LogInformation("Retrieved {Count} job titles", jobTitlesList.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving job titles");
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving job titles"));
            }
        }

        /// <summary>
        /// Gets detailed information about a specific job title
        /// </summary>
        /// <param name="jobTitle">The job title to get information about</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Job title information with employee count</returns>
        [HttpGet("{jobTitle}")]
        [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobTitleInfo(string jobTitle, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobTitle))
                {
                    return BadRequest(new ErrorResponse("Job title cannot be null or empty"));
                }

                _logger.LogInformation("Retrieving information for job title: {JobTitle}", jobTitle);

                // Get employees with this job title
                var employees = await _employeeRepository.GetByJobTitleAsync(jobTitle, cancellationToken);
                var employeeCount = employees.Count();

                if (employeeCount == 0)
                {
                    return NotFound(new ErrorResponse($"No employees found with job title: {jobTitle}"));
                }

                var response = new JobTitleResponse
                {
                    JobTitle = jobTitle,
                    EmployeeCount = employeeCount,
                    Message = $"Found {employeeCount} employee(s) with job title: {jobTitle}"
                };

                _logger.LogInformation("Retrieved information for job title: {JobTitle}, Employee count: {Count}", 
                    jobTitle, employeeCount);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving job title information for: {JobTitle}", jobTitle);
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving job title information"));
            }
        }

        /// <summary>
        /// Searches for job titles that contain the specified text
        /// </summary>
        /// <param name="searchTerm">Text to search for in job titles</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Matching job titles</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(JobTitlesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchJobTitles(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ErrorResponse("Search term cannot be null or empty"));
                }

                _logger.LogInformation("Searching for job titles containing: {SearchTerm}", searchTerm);

                var allJobTitles = await _employeeRepository.GetDistinctJobTitlesAsync(cancellationToken);
                var matchingJobTitles = allJobTitles
                    .Where(jt => jt.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(jt => jt)
                    .ToList();

                var response = new JobTitlesResponse
                {
                    JobTitles = matchingJobTitles,
                    Total = matchingJobTitles.Count,
                    Message = $"Found {matchingJobTitles.Count} job title(s) matching '{searchTerm}'"
                };

                _logger.LogInformation("Search completed for term: {SearchTerm}, Found: {Count} matches", 
                    searchTerm, matchingJobTitles.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching job titles for term: {SearchTerm}", searchTerm);
                return StatusCode(500, new ErrorResponse("An error occurred while searching job titles"));
            }
        }
    }
}
