using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HdbController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<HdbController> _logger;

        public HdbController(Services.IDatabaseService databaseService, ILogger<HdbController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// List available HDB instances
        /// </summary>
        [HttpGet]
        public IActionResult GetHdbList()
        {
            var hdbs = new List<string>
            {
                "LCHDB - LC Production HDB",
                "UCHDB2 - UC Production HDB",
                "UCHDBT - UC Test HDB",
                "YAOHDB - YAO Production HDB",
                "ECOHDB - ECAO Production HDB",
                "LBOHDB - LBAO Production HDB",
                "KBOHDB - KBAO Production HDB",
                "PNHYD - PN Production Hydromet",
                "GPHYD - GP Production Hydromet"
            };

            return Ok(hdbs);
        }

        /// <summary>
        /// Test connection to HDB using api_hdb/api_user/api_pass headers
        /// </summary>
        [HttpGet("connect")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Uses the same header-based auth as other endpoints
                using var connection = await _databaseService.GetConnectionAsync(HttpContext);

                // Extract the user header for the response
                HttpContext.Request.Headers.TryGetValue("api_user", out var userValues);
                var username = userValues.FirstOrDefault();

                // Test the connection with a simple query
                var result = await connection.QueryAsync<dynamic>(
                    "select * from all_users where username = :username",
                    new { username = username?.ToUpper() });

                return Ok(new
                {
                    status = "connected",
                    user = username,
                    userInfo = result.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to HDB using header authentication");
                return BadRequest(new
                {
                    status = "connection_failed",
                    error = ex.Message
                });
            }
        }
    }
}