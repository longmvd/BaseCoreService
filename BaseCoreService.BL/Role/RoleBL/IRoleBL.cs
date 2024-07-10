using BaseCoreService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public interface IRoleBL : IBaseBL
    {
        Task<List<Role>>GetRolesByUserID(Guid userID);
    }
}
