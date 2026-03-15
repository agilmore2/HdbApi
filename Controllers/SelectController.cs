using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("select")]
    public class SelectController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<SelectController> _logger;

        public SelectController(Services.IDatabaseService databaseService, ILogger<SelectController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Execute SQL SELECT Query
        /// </summary>
        /// <remarks>
        /// Execute a read-only SQL SELECT statement against the HDB database
        /// </remarks>
        /// <param name="sqlStatement">SQL SELECT statement to execute</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string sqlStatement)
        {
            if (string.IsNullOrWhiteSpace(sqlStatement))
            {
                return BadRequest(new { error = "sqlStatement is required" });
            }

            // Enforce read-only usage
            var trimmed = sqlStatement.Trim();
            if (!trimmed.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Only SELECT statements are allowed." });
            }

            try
            {
                using var connection = await _databaseService.GetConnectionAsync(HttpContext);

                var result = await connection.QueryAsync<IDictionary<string, object?>>(trimmed);

                // Lowercase column names to match legacy behavior
                var normalized = result
                    .Select(row => row.ToDictionary(kv => kv.Key?.ToLowerInvariant() ?? string.Empty, kv => kv.Value))
                    .ToList();

                return Ok(normalized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing select statement");
                return StatusCode(500, new { error = "Query failed", details = ex.Message });
            }
        }
    }
}
