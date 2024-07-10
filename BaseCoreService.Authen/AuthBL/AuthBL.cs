
using BaseCoreService.Common;
using BaseCoreService.DL;
using BaseCoreService.Entities;
using BaseCoreService.Entities.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public class AuthBL : IAuthBL
    {
        private IConfiguration _configuration;

        private IServiceScopeFactory _scopeFactory;

        private readonly IBaseDL DL;

        public AuthBL(IConfiguration configuration, IServiceScopeFactory scopeFactory, IBaseDL baseDL)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            DL = baseDL;

        }

        private string GetStringQuery(string query)
        {
            return Utils.GetStringQuery(query, "Queries/AuthenQueries.json");
        }

        public async Task<ServiceResponse> Login(LoginRequest request)
        {
            var response = new ServiceResponse();
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.OnError(new ErrorResponse());
            }
            else
            {
                var sql = GetStringQuery("AuthBL_Login");
                var user = (await DL.QueryAsyncUsingCommandText<User>(sql, new Dictionary<string, object>()
                {
                    {"@UserName", request.UserName},
                    {"password", HashPassword( request.Password)}
                })).FirstOrDefault();

                if (user == null)
                {
                    response.OnError(new ErrorResponse() { ErrorMessage = "User name or password is invalid" });
                }
                else
                {
                    sql = GetStringQuery("AuthBL_GetUserRoles");
                    user.Roles = await DL.QueryAsyncUsingCommandText<Role>(sql, new Dictionary<string, object>() { { "@UserID", user.UserID } });
                    var token = BuildToken(_configuration["Jwt:Key"], "", null, user);
                    response.OnSuccess(new { Token = token });

                }

            }
            return response;
        }

        public Task<ServiceResponse> Logout()
        {
            throw new NotImplementedException();
        }

        public string BuildToken(string key, string issuer, IEnumerable<string> audience, User user)
        {
            var claims = new List<Claim>
        {
            new Claim("UserID", user.UserID.ToString()),
            new Claim("Email", user.Email),
            new Claim("FullName", user.FullName),
            new Claim("Roles", string.Join(",", user.Roles?.Select(role => role.RoleCode)))


        };
            if (audience != null)
            {
                claims.AddRange(audience.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
                expires: DateTime.Now.AddDays(int.Parse(_configuration["Jwt:Expired"])), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                }, out _);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object GetInfoFromToken(string token)
        {
            if (ValidateToken(token))
            {
                var decoded = new JwtSecurityToken(jwtEncodedString: token);
                var rawData = decoded.Payload;
                //var user = 
                return rawData;
            }
            return null;

        }

        public T GetInfoFromToken<T>(string token)
        {
            var res = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(GetInfoFromToken(token)));
            return res;

        }

        public string GenerateWorkerToken()
        {
            var token = BuildToken(_configuration["Jwt:Key"], "", null, new User()
            {
                UserID = Guid.NewGuid(),
                Email = "",
                FullName = "WorkerService",
                Roles = [new Role() { RoleCode = "ADMIN" }]
            });
            return token;
        }
    }
}
