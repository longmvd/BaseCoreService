using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL.MongoDB
{
    public static class StartupImport
    {
        public static IServiceCollection AddMongoService(this IServiceCollection service)
        {
            service.AddScoped(typeof(IMongoDBService<>), typeof(MongoDBService<>));
            return service;
        }
    }
}
