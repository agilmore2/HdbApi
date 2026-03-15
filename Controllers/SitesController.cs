using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("sites")]
    public class SitesController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<SitesController> _logger;

        public SitesController(Services.IDatabaseService databaseService, ILogger<SitesController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get Site(s)
        /// </summary>
        /// <remarks>
        /// Get metadata for available HDB site(s)
        /// </remarks>
        /// <param name="id">Optional comma-separated HDB Site IDs to filter by (e.g., 1,2,3)</param>
        [HttpGet]
        public async Task<IActionResult> GetSites([FromQuery] string? id = null)
        {
            IDbConnection? db = null;

            try
            {
                // Get database connection from headers
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // Build SQL query (matches legacy API column order)
                var sql = @"select A.SITE_ID, A.SITE_NAME, A.SITE_COMMON_NAME, A.DESCRIPTION, A.ELEVATION, A.LAT, A.LONGI, A.DB_SITE_CODE, " +
                          "A.OBJECTTYPE_ID, B.OBJECTTYPE_NAME, A.BASIN_ID, A.HYDROLOGIC_UNIT, A.RIVER_MILE, A.SEGMENT_NO, A.STATE_ID, C.STATE_CODE, " +
                          "A.USGS_ID, A.NWS_CODE, A.SHEF_CODE, A.SCS_ID, A.PARENT_OBJECTTYPE_ID, A.PARENT_SITE_ID " +
                          "from HDB_SITE A, HDB_OBJECTTYPE B, HDB_STATE C " +
                          "where A.OBJECTTYPE_ID = B.OBJECTTYPE_ID " +
                          "and A.STATE_ID = C.STATE_ID";

                if (!string.IsNullOrEmpty(id))
                {
                    var ids = id.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (ids.Any())
                    {
                        var idList = string.Join(",", ids.Select(x => $"'{x}'"));
                        sql += $" and A.SITE_ID in ({idList})";
                    }
                }

                sql += " order by A.SITE_ID";

                var sites = await db.QueryAsync<SiteDto>(sql);

                return Ok(sites);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sites");
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