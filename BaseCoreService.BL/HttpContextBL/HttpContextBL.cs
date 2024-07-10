using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public class HttpContextBL: IHttpContextBL
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAcessor;

        public HttpContextBL(IConfiguration configuration, IHttpContextAccessor httpContextAcessor)
        {
            _configuration = configuration;
            _httpContextAcessor = httpContextAcessor;
        }

        public HttpContext? GetCurrentHttpContext()
        {
            var context = _httpContextAcessor.HttpContext;
            return context;
        }

        public IHeaderDictionary? GetCurrentRequestHeader()
        {
            var header = _httpContextAcessor?.HttpContext?.Request.Headers;
            return header;
        }

        public IHeaderDictionary? GetCurrentResponseHeader()
        {
            var header = _httpContextAcessor?.HttpContext?.Response.Headers;
            return header;
        }

        public string GetRequestHeaderValue(string key)
        {
            var header = GetCurrentRequestHeader();
            if (header != null)
            {
                if(header.TryGetValue(key, out var value))
                {
                    return value.ToString();
                }
            }
            return string.Empty;

        }

    }
}
