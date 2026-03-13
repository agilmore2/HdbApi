using Microsoft.AspNetCore.Mvc;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Test endpoint working" });
        }
    }
}