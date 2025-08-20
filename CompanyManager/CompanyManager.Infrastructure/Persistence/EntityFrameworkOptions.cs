using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace CompanyManager.Infrastructure.Persistence
{
    public static class EntityFrameworkOptions
    {
        public static void ConfigureSqlServer(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Default' not found.");
            }
            
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(CompanyContext).Assembly.FullName);
                
                // Configurações de performance
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                // Configurações de consulta
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                
                // Configurações de timeout
                sqlOptions.CommandTimeout(60);
                
                // Configurações de pool de conexões
                sqlOptions.MaxBatchSize(100);
                
                // Configurações de segurança
                sqlOptions.EnableRetryOnFailure();
            });
        }



        public static void ConfigureLogging(DbContextOptionsBuilder options, bool enableSensitiveDataLogging = false)
        {
            options.EnableSensitiveDataLogging(enableSensitiveDataLogging);
            options.EnableDetailedErrors();
        }
    }
}
