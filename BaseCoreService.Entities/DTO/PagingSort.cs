using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.DTO
{
    public class PagingSort
    {
        public string Selector { get; set; }

        public bool Random { get; set; }

        public bool Desc { get; set; }
    }
}
