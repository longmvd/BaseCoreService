using BaseCoreService.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.DTO
{
    public class ValidateResult
    {
        /// <summary>
        /// ID of error record
        /// </summary>
        public object ID;
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public object AdditionInfo { get; set; }
        public ValidateType ValidateType { get; set; }
    }
}
