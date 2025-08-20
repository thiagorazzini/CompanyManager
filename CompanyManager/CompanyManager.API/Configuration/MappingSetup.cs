using Microsoft.Extensions.DependencyInjection;

namespace CompanyManager.Api.Configuration
{
    public static class MappingSetup
    {
        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                // Configure AutoMapper profiles here if needed
            }, AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
