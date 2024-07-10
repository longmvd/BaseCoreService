using BaseCoreService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.DL
{
    public interface IRoleDL : IBaseDL
    {
        IEnumerable<User> GetUserByRoleName(string roleName);
    }
}
