using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        /// <summary>
        /// Gets overall health status
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Health status information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthCheckInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken: cancellationToken);

            var response = new HealthCheckInfo
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Version = "1.0.0"
            };

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        /// <summary>
        /// Gets detailed health status with individual check results
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Detailed health status information</returns>
        [HttpGet("detailed")]
        [ProducesResponseType(typeof(DetailedHealthCheckInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetDetailedHealth(CancellationToken cancellationToken)
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken: cancellationToken);

            var response = new DetailedHealthCheckInfo
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Version = "1.0.0",
                Data = healthReport.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => (object)new
                    {
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
        /// Simple ping endpoint for load balancer health checks
        /// </summary>
        /// <returns>Ping response</returns>
        [HttpGet("ping")]
        [ProducesResponseType(typeof(PingResponse), StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            var response = new PingResponse
            {
                Message = "pong",
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets readiness status (useful for Kubernetes readiness probes)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Readiness status</returns>
        [HttpGet("ready")]
        [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReadiness(CancellationToken cancellationToken)
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(registration => 
                registration.Tags.Contains("ready"), cancellationToken: cancellationToken);

                            var response = new ReadinessResponse
                {
                    Status = healthReport.Status == HealthStatus.Healthy ? "Ready" : "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Checks = new Dictionary<string, object>
                    {
                        ["Total"] = healthReport.Entries.Count,
                        ["Healthy"] = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                        ["Unhealthy"] = healthReport.Entries.Count(e => e.Value.Status != HealthStatus.Healthy)
                    }
                };

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        /// <summary>
        /// Gets liveness status (useful for Kubernetes liveness probes)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Liveness status</returns>
        [HttpGet("live")]
        [ProducesResponseType(typeof(LivenessResponse), StatusCodes.Status200OK)]
        public IActionResult GetLiveness(CancellationToken cancellationToken)
        {
                            var response = new LivenessResponse
                {
                    Status = "Alive",
                    Timestamp = DateTime.UtcNow
                };

            return Ok(response);
        }

        /// <summary>
        /// Gets startup status (useful for Kubernetes startup probes)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Startup status</returns>
        [HttpGet("startup")]
        [ProducesResponseType(typeof(StartupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(StartupResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetStartup(CancellationToken cancellationToken)
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(registration => 
                registration.Tags.Contains("startup"), cancellationToken: cancellationToken);

            var response = new StartupResponse
            {
                Status = healthReport.Status == HealthStatus.Healthy ? "Started" : "Starting",
                Timestamp = DateTime.UtcNow,
                StartupTime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
            };

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        [HttpGet("database/clear")]
        public async Task<IActionResult> ClearDatabase()
        {
            try
            {
                var dbInitializer = HttpContext.RequestServices.GetRequiredService<IDatabaseInitializerService>();
                await dbInitializer.ClearAsync();
                return Ok(new { message = "Database cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("database/init")]
        public async Task<IActionResult> InitializeDatabase()
        {
            try
            {
                var dbInitializer = HttpContext.RequestServices.GetRequiredService<IDatabaseInitializerService>();
                await dbInitializer.InitializeAsync();
                return Ok(new { message = "Database initialized successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
