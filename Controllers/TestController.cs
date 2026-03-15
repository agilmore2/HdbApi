using Microsoft.AspNetCore.Mvc;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Test API Availability
        /// </summary>
        /// <remarks>
        /// Simple endpoint to verify API is running and accessible
        /// </remarks>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Test endpoint working" });
        }
    }
}