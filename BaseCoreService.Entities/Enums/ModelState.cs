using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.Enums
{
    public enum ModelState
    {
        None = 0,
        Insert = 1,
        Update = 2,
        Delete = 3,
        Dupplicate = 4,
        Restore = 5,
    }
}
