using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("hdb")]
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
        /// List Available HDB Instances
        /// </summary>
        /// <remarks>
        /// Get list of available HDB database instances that can be connected to
        /// </remarks>
        [HttpGet]
        public IActionResult GetHdbList()
        {
            try
            {
                var hdbs = System.IO.File.ReadAllLines("hostnames.txt").ToList();
                return Ok(hdbs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading hostnames.txt");
                return StatusCode(500, new { error = "Unable to read HDB list" });
            }
        }

    }
}