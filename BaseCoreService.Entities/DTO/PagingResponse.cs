using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.DTO
{
    public class PagingResponse
    {
        public object? PageData { get; set; }
        public int Total { get; set; }
        public PagingResponse() { }

        public PagingResponse(object pageData, int total)
        {
            PageData = pageData;
            Total = total;
        }
    }
}
