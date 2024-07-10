
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseCoreService.BL;
using BaseCoreService.Common;
using BaseCoreService.DL;
using BaseCoreService.Entities;
using Microsoft.Extensions.Configuration;

namespace BaseCoreService.BL
{
    public class RoleBL : BaseBL, IRoleBL
    {

        public RoleBL(IRoleDL roleDL, ServiceCollection serviceCollection) : base(roleDL, serviceCollection)
        {
            this._baseDL = roleDL;
        }

        public async Task<List<Role>> GetRolesByUserID(Guid userID)
        {
            var sql = Utils.GetStringQuery("RoleBL_GetRolesByUserID", "Queries/AuthenQueries.json");
            var res = await QueryAsyncUsingCommandText(typeof(Role), sql, new Dictionary<string, object>() { { "@UserID", userID } });
            var roles = res.Cast<Role>().ToList();
            return roles;

        }
    }
}
