using Microsoft.AspNetCore.Mvc;

namespace Orsync.Controllers
{
    public class TestController : ControllerBase
    {
   
            [HttpGet]
  
        [Route("api/[controller]")]
        public IActionResult Get() => Ok("Hello World");
      
    }
}
