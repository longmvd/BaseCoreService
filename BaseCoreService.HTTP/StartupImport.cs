using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BaseCoreService.HTTP
{
    public static class StartupImport
    {
        public static IServiceCollection AddHTTPRequest(this IServiceCollection collection)
        {
            collection.AddTransient<IHttpRequest, HttpRequest>();
            return collection;
        }
    }
}
