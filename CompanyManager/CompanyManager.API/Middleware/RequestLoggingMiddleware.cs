using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CompanyManager.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var method = context.Request.Method;
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            _logger.LogInformation("Request started: {Method} {Path} - IP: {IP}, User-Agent: {UserAgent}", 
                method, requestPath, ipAddress, userAgent);

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                
                if (statusCode >= 400)
                {
                    _logger.LogWarning("Request failed: {Method} {Path} - Status: {StatusCode}, Duration: {Duration}ms", 
                        method, requestPath, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode}, Duration: {Duration}ms", 
                        method, requestPath, statusCode, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error in request: {Method} {Path} - Duration: {Duration}ms", 
                    method, requestPath, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
