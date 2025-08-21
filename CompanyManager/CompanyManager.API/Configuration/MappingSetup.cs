using Microsoft.Extensions.DependencyInjection;

namespace CompanyManager.Api.Configuration
{
    public static class MappingSetup
    {
        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => {}, typeof(CompanyManager.Application.DependencyInjection).Assembly);
            return services;
        }
    }
}
