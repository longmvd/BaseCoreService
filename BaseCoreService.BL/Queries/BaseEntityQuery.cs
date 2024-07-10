using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public static class BaseEntityQuery
    {
        public static string BaseBL_GetAll = "SELECT * FROM {0};";
        public static string BaseBL_GetPaging = "SELECT {0} FROM {1} WHERE {2};";
    }
}
