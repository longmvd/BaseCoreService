using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Cache.Interface
{
    public interface ICacheService
    {
        Task<T> GetValue<T>(string key);

        Task SetValue(string key, object value, int expirationTime);
    }
}
