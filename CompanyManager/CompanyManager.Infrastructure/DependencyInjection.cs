using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure.Persistence;
using CompanyManager.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CompanyManager.Infrastructure
{
    /// <summary>
    /// Extension methods for registering Infrastructure layer services in the DI container.
    /// </summary>
    /// <remarks>
    /// This class provides a centralized way to register all Infrastructure layer services,
    /// including Entity Framework Core DbContext with SQL Server and repositories.
    /// </remarks>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds Infrastructure layer services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The configuration instance containing connection strings.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// Registers the following services:
        /// - Entity Framework Core DbContext with SQL Server
        /// - Repositories: IEmployeeRepository -> EmployeeRepository
        /// - SQL Server connection with retry policies and migrations
        /// - Database initializer service for seeding data
        /// </remarks>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register EF Core DbContext with SQL Server
            services.AddDbContext<CompanyContext>(options =>
            {
                EntityFrameworkOptions.ConfigureSqlServer(options, configuration);

                // Configurações de logging baseadas no ambiente
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                EntityFrameworkOptions.ConfigureLogging(options, isDevelopment);
            });

            // Register repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();

            // Register database initializer service
            services.AddScoped<IDatabaseInitializerService, DatabaseInitializerService>();

            // Optional: Add health check for database
            // services.AddHealthChecks().AddSqlServer(configuration.GetConnectionString("Default"));

            // Note: Database initialization is now available via IDatabaseInitializerService
            // Call it in Program.cs or use it as needed

            return services;
        }
    }
}
