using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseCoreService.DL
{
    public static class DLStartupImport
    {
        public static IServiceCollection AddDataLayerService(this IServiceCollection services)
        {
            services.AddScoped<IBaseDL, BaseDL>();
            services.AddScoped<ITestDL, TestDL>();
            return services;
        }

        public static IConfiguration AddConfiguartion(this IConfiguration configuration)
        {
            DatabaseContext.ConnectionString = configuration.GetConnectionString("MySql");
            return configuration;
        }

        public static void Intit(IServiceCollection services, IConfiguration configuration) {
            services.AddDataLayerService();
            configuration.AddConfiguartion();
        }
    }
}
