using BaseCoreService.BL;
using BaseCoreService.Entities;
using BaseCoreService.Entities.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseCoreService.Controllers
{
    public class TestsController : BaseController
    {
        public TestsController(ITestBL productBL, IAuthBL authBL) : base(productBL, authBL)
        {
            CurrentType = typeof(Test);
            this._baseBL = productBL;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var response = new ServiceResponse();
            var validate = new List<ValidateResult>() { };
            //validate.Add(new ValidateResult() { AdditionInfo = new { QuantityAvailable = 3 }, ErrorMessage = "Số lượng mua vượt quá số lượng hàng trong kho" });
            //response.OnError(new ErrorResponse{ Data = validate });
            return Ok(response);
        }
    }
}
