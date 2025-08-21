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
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds Infrastructure layer services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The configuration instance containing connection strings.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CompanyContext>(options =>
            {
                EntityFrameworkOptions.ConfigureSqlServer(options, configuration);

                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                EntityFrameworkOptions.ConfigureLogging(options, isDevelopment);
            });

                    services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IJobTitleRepository, JobTitleRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<IDatabaseInitializerService, DatabaseInitializerService>();

            return services;
        }
    }
}
