using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Authen.Policies
{
    public class AdminRequirement : AuthorizationHandler<AdminRequirement>, IAuthorizationRequirement
    {
        IConfiguration _configuration;
        public AdminRequirement(IConfiguration configuration) { _configuration = configuration; }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var ignoreAuthen = bool.Parse(_configuration["AppSettings:DisabledAuthen"] ?? "false");
            if (ignoreAuthen == true)
            {
                context.Succeed(requirement);
                return;
            }
            if (context.User?.Claims?.Count() > 0)
            {
                var claims = context.User.Claims.FirstOrDefault(x => x.Type == "Roles");
                if (claims != null)
                {
                    var roleValue = claims.Value;
                    var roles = roleValue.Split(',');
                    foreach (var role in roles)
                    {
                        if (!string.IsNullOrEmpty(role) && role.ToUpper() == "ADMIN")
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }

                }
            }
            else
            {
                context.Fail();
            }
            return;
        }
    }
}
