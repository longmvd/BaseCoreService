using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.Enums
{
    public enum ValidateType
    {
        Required = 1,
        Unique = 2,
        MaxLength = 3,
        MinLength = 4,
        Email = 5,
        InValid = 6,


    }
}
