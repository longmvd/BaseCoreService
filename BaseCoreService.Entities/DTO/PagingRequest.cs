using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.DTO
{
    public class PagingRequest
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public bool? Desc { get; set; }

        public string? CustomFilter { get; set; }

        public object? CustomParams { get; set; }

        public string? Columns { get; set; }

        public string? Filter { get; set; }

        public QuickSearch? QuickSearch { get; set; }

        public string? Sort { get; set; }

        public bool? UseSp { get; set; } = false;

    }
    public class QuickSearch
    {
        public string? SearchValue { get; set; }

        public string? Columns { get; set; }
    }
}
