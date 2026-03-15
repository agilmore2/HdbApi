using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("sitedatatypes")]
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
        /// <remarks>
        /// Get metadata for available HDB site-datatype relationship(s)
        /// </remarks>
        /// <param name="sdi">Optional comma-separated HDB SiteDataType IDs to filter by (e.g., 1,2,3)</param>
        /// <param name="sid">Optional comma-separated HDB Site IDs to filter by (e.g., 1,2,3)</param>
        /// <param name="did">Optional comma-separated HDB DataType IDs to filter by (e.g., 1,2,3)</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? sdi = null, [FromQuery] string? sid = null, [FromQuery] string? did = null)
        {
            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var sql = "select SITE_DATATYPE_ID, SITE_ID, DATATYPE_ID from HDB_SITE_DATATYPE";

                var conditions = new List<string>();

                if (!string.IsNullOrEmpty(sdi))
                {
                    var ids = sdi.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (ids.Any())
                    {
                        var idList = string.Join(",", ids);
                        conditions.Add($"SITE_DATATYPE_ID in ({idList})");
                    }
                }

                if (!string.IsNullOrEmpty(sid))
                {
                    var ids = sid.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (ids.Any())
                    {
                        var idList = string.Join(",", ids);
                        conditions.Add($"SITE_ID in ({idList})");
                    }
                }

                if (!string.IsNullOrEmpty(did))
                {
                    var ids = did.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (ids.Any())
                    {
                        var idList = string.Join(",", ids);
                        conditions.Add($"DATATYPE_ID in ({idList})");
                    }
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

        /// <summary>
        /// Get SiteDataType(s)
        /// </summary>
        /// <remarks>
        /// Get metadata for available SiteDataType(s)
        /// </remarks>
        /// <param name="input">List of filters for SiteDataType request</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<SiteDataTypeQuery> input)
        {
            if (input == null)
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var sdi = input.Select(i => i.sdi).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                var sid = input.Select(i => i.sid).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                var did = input.Select(i => i.did).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                var sql = "select SITE_DATATYPE_ID, SITE_ID, DATATYPE_ID from HDB_SITE_DATATYPE";
                var conditions = new List<string>();

                if (sdi.Length > 0)
                {
                    var ids = string.Join(",", sdi);
                    conditions.Add($"SITE_DATATYPE_ID in ({ids})");
                }

                if (sid.Length > 0)
                {
                    var ids = string.Join(",", sid);
                    conditions.Add($"SITE_ID in ({ids})");
                }

                if (did.Length > 0)
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

        public class SiteDataTypeQuery
        {
            public string sdi { get; set; } = string.Empty;
            public string sid { get; set; } = string.Empty;
            public string did { get; set; } = string.Empty;
        }
    }
}