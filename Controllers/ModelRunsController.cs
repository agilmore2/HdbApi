using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModelRunsController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<ModelRunsController> _logger;

        private const string DefaultIdType = "model_run_id";

        public ModelRunsController(Services.IDatabaseService databaseService, ILogger<ModelRunsController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get Model Run(s)
        /// </summary>
        /// <param name="idtype">Optional ID category to query. "model_run_id" (default) or "model_id".</param>
        /// <param name="id">Optional IDs to filter by (model_run_id or model_id depending on idtype).</param>
        /// <param name="modelrunname">Optional model run name filter (case-insensitive partial match).</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? idtype = null, [FromQuery] int[]? id = null, [FromQuery] string? modelrunname = null)
        {
            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var effectiveIdType = string.IsNullOrWhiteSpace(idtype) ? DefaultIdType : idtype.ToLower();
                if (effectiveIdType != "model_run_id" && effectiveIdType != "model_id")
                {
                    effectiveIdType = DefaultIdType;
                }

                var sql = "select b.model_run_id, b.model_run_name, b.date_time_loaded, b.run_date, " +
                          "b.user_name, b.cmmnt as model_run_cmmnt, A.model_id, A.model_name, A.cmmnt as model_cmmnt " +
                          "from hdb_model A, ref_model_run B where A.model_id = b.model_id";

                if (id != null && id.Length > 0)
                {
                    var ids = string.Join(",", id);
                    sql += $" and b.{effectiveIdType} in ({ids})";
                }

                if (!string.IsNullOrWhiteSpace(modelrunname))
                {
                    var escaped = modelrunname.Replace("'", "''").ToLower();
                    sql += $" and lower(b.model_run_name) like '%{escaped}%'";
                }

                sql += " order by b.date_time_loaded desc";

                var results = await db.QueryAsync<ModelRunDto>(sql);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model runs");
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
