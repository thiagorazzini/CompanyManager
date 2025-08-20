using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace CompanyManager.API.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        /// <summary>
        /// Gets the overall health status of the application
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealth()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Duration = healthReport.TotalDuration,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Tags = entry.Value.Tags
                })
            };

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        /// <summary>
        /// Gets detailed health information in JSON format
        /// </summary>
        /// <returns>Detailed health report</returns>
        [HttpGet("detailed")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetailedHealth()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Duration = healthReport.TotalDuration,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Tags = entry.Value.Tags,
                    Exception = entry.Value.Exception?.Message,
                    Data = entry.Value.Data
                })
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets a simple health status (useful for load balancers)
        /// </summary>
        /// <returns>Simple health status</returns>
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok(new { Status = "OK", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Gets readiness status (useful for Kubernetes readiness probes)
        /// </summary>
        /// <returns>Readiness status</returns>
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReadiness()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(registration => 
                registration.Tags.Contains("ready"));

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow }) 
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Not Ready", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Gets liveness status (useful for Kubernetes liveness probes)
        /// </summary>
        /// <returns>Liveness status</returns>
        [HttpGet("live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLiveness()
        {
            return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
        }
    }
}
