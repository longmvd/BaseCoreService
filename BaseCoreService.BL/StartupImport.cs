using BaseCoreService.DL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public static class BLStartupImport
    {
        public static IServiceCollection AddBusinessLayerService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IHttpContextBL, HttpContextBL>();
            services.AddScoped<ServiceCollection>();
            services.AddScoped<IBaseBL, BaseBL>();
            services.AddScoped<IUserDL, UserDL>();
            services.AddScoped<IUserBL, UserBL>();
            services.AddScoped<IRoleDL, RoleDL>();
            services.AddScoped<IRoleBL, RoleBL>();
            services.AddScoped<ITestBL, TestBL>();

            return services;
        }

        public static void Intit(IServiceCollection services, IConfiguration configuration)
        {
            services.AddBusinessLayerService();
        }
    }
}
