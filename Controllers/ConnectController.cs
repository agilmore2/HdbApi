using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("connect")]
    public class ConnectController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<ConnectController> _logger;

        public ConnectController(Services.IDatabaseService databaseService, ILogger<ConnectController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Test Database Connection
        /// </summary>
        /// <remarks>
        /// Test connection to HDB database and return user information
        /// </remarks>
        /// <param name="hdb">HDB instance to connect to</param>
        /// <param name="username">Username for HDB authentication</param>
        /// <param name="password">Password for HDB authentication</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string hdb, [FromQuery] string username, [FromQuery] string password)
        {
            try
            {
                // Create connection using the provided parameters
                using var connection = await _databaseService.GetConnectionAsync(hdb, username, password);

                // Query user info like the old API
                var rawResult = await connection.QueryAsync<IDictionary<string, object?>>(
                    "select * from all_users where username = :username",
                    new { username = username.ToUpper() });

                // Normalize column names to lowercase (avoid legacy uppercase/underscore issues)
                var normalized = rawResult
                    .Select(row => row.ToDictionary(kv => kv.Key?.ToLowerInvariant() ?? string.Empty, kv => kv.Value))
                    .ToList();

                return Ok(normalized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to HDB {Hdb} as user {Username}", hdb, username);
                return BadRequest(new
                {
                    error = "Connection failed",
                    details = ex.Message
                });
            }
        }
    }
}