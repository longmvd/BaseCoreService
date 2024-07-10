using BaseCoreService.Entities;
using BaseCoreService.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public interface IAuthBL
    {
        Task<ServiceResponse> Login(LoginRequest request);

        Task<ServiceResponse> Logout();

        bool ValidateToken(string token);

        object GetInfoFromToken(string token);
        T GetInfoFromToken<T>(string token);

        string BuildToken(string key, string issuer, IEnumerable<string> audience, User userName);
    }
}
