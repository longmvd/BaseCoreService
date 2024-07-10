using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public class ServiceCollection
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private IHttpContextBL _httpContextBL;
        
        public ServiceCollection(IServiceProvider serviceProvider, IConfiguration configuration, IHttpContextBL httpContextBL)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpContextBL = httpContextBL;
        }

        public IServiceProvider ServiceProvider => _serviceProvider;

        public IConfiguration Configuration => _configuration;

        public IHttpContextBL HttpContextBL => _httpContextBL;

    }
}
