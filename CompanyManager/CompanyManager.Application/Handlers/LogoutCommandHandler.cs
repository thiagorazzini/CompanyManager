using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Handlers
{
    public sealed class LogoutCommandHandler : ILogoutCommandHandler
    {
        private readonly IUserAccountRepository _userRepository;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IUserAccountRepository userRepository,
            ILogger<LogoutCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                _logger.LogWarning("No refresh token provided for logout");
                return;
            }

            try
            {

                var email = command.RefreshToken?.Trim().ToLowerInvariant();
                
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var userAccount = await _userRepository.FindByEmailAsync(email, cancellationToken);
                    
                    if (userAccount != null)
                    {

                        userAccount.SetPasswordHash(userAccount.PasswordHash);
                        await _userRepository.UpdateAsync(userAccount, cancellationToken);
                        
                        _logger.LogInformation("User session invalidated for: {Email}", email);
                    }
                    else
                    {
                        _logger.LogWarning("User account not found for email: {Email}", email);
                    }
                }
                
                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while invalidating refresh token");
                throw;
            }
        }
    }
}
