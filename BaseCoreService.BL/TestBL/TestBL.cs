using BaseCoreService.DL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.BL
{
    public class TestBL : BaseBL, ITestBL
    {
        public TestBL(ITestDL productDL, ServiceCollection serviceCollection) : base(productDL, serviceCollection)
        {
            this._baseDL = productDL;
        }
    }
}
