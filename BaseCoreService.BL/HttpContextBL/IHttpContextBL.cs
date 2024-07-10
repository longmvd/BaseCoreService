using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public interface IHttpContextBL
    {
        public HttpContext? GetCurrentHttpContext();

        public IHeaderDictionary? GetCurrentRequestHeader();

        public IHeaderDictionary? GetCurrentResponseHeader();

        public string GetRequestHeaderValue(string key);

    }
}
