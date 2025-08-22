using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Handlers
{
    /// <summary>
    /// Handles user logout operations for JWT-based authentication
    /// </summary>
    public sealed class LogoutCommandHandler : ILogoutCommandHandler
    {
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the logout command - simply logs the action as tokens are stateless JWT
        /// </summary>
        public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                _logger.LogInformation("User logged out successfully");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                throw;
            }
        }
    }
}
