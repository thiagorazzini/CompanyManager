using CompanyManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Infrastructure.Persistence
{
    public class DatabaseInitializerService : IDatabaseInitializerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializerService> _logger;

        public DatabaseInitializerService(IServiceProvider serviceProvider, ILogger<DatabaseInitializerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            const int maxRetries = 5;
            const int delaySeconds = 10;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CompanyContext>();
                    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                    _logger.LogInformation("Initializing database... Attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    await DatabaseInitializer.InitializeAsync(context, passwordHasher);
                    _logger.LogInformation("Database initialized successfully!");
                    return; // Sucesso, sair do loop
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error initializing database on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    
                    if (attempt == maxRetries)
                    {
                        _logger.LogError(ex, "Failed to initialize database after {MaxRetries} attempts", maxRetries);
                        throw; // Última tentativa falhou, re-throw
                    }
                    
                    _logger.LogInformation("Waiting {DelaySeconds} seconds before retry...", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CompanyContext>();

                _logger.LogInformation("Clearing database...");
                await DatabaseInitializer.ClearAsync(context);
                _logger.LogInformation("Database cleared successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing database");
                throw;
            }
        }
    }
}
