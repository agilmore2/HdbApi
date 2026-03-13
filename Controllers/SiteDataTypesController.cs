using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteDataTypesController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<SiteDataTypesController> _logger;

        public SiteDataTypesController(Services.IDatabaseService databaseService, ILogger<SiteDataTypesController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get SiteDataType(s)
        /// </summary>
        /// <param name="sdi">Optional HDB SiteDataType IDs to filter by</param>
        /// <param name="sid">Optional HDB Site IDs to filter by</param>
        /// <param name="did">Optional HDB DataType IDs to filter by</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string[]? sdi = null, [FromQuery] string[]? sid = null, [FromQuery] string[]? did = null)
        {
            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var sql = "select SITE_DATATYPE_ID, SITE_ID, DATATYPE_ID from HDB_SITE_DATATYPE";

                var conditions = new List<string>();

                if (sdi != null && sdi.Length > 0)
                {
                    var ids = string.Join(",", sdi);
                    conditions.Add($"SITE_DATATYPE_ID in ({ids})");
                }

                if (sid != null && sid.Length > 0)
                {
                    var ids = string.Join(",", sid);
                    conditions.Add($"SITE_ID in ({ids})");
                }

                if (did != null && did.Length > 0)
                {
                    var ids = string.Join(",", did);
                    conditions.Add($"DATATYPE_ID in ({ids})");
                }

                if (conditions.Any())
                {
                    sql += " where " + string.Join(" and ", conditions);
                }

                sql += " order by SITE_DATATYPE_ID";

                var results = await db.QueryAsync<SiteDataTypeDto>(sql);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving site datatypes");
                return StatusCode(500, new { error = "Database error", details = ex.Message });
            }
            finally
            {
                if (db != null)
                {
                    _databaseService.CloseConnection(db);
                }
            }
        }
    }
}