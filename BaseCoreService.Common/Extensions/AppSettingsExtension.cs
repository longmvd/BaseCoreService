using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Common
{
    public static class AppSettingsExtension
    {
        public static string GetAppSettings(this IConfiguration configuration, string key)
        {
            var res = configuration[$"AppSettings:{key}"] ?? "";
            return res;
        }
    }
}
