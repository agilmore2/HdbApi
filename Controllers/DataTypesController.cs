using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("datatypes")]
    public class DataTypesController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<DataTypesController> _logger;

        public DataTypesController(Services.IDatabaseService databaseService, ILogger<DataTypesController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get DataType(s)
        /// </summary>
        /// <remarks>
        /// Get metadata for available HDB datatype(s)
        /// </remarks>
        /// <param name="id">Optional comma-separated HDB DataType IDs to filter by (e.g., 1,2,3)</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? id = null)
        {
            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var sql = "select * from HDB_DATATYPE A, HDB_UNIT B where A.UNIT_ID = B.UNIT_ID";

                if (!string.IsNullOrEmpty(id))
                {
                    var ids = id.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (ids.Any())
                    {
                        if (!ids.All(x => long.TryParse(x, out _)))
                            return BadRequest(new { error = "id must be integers" });
                        sql += $" and A.DATATYPE_ID in ({string.Join(",", ids)})";
                    }
                }
                sql += " order by A.DATATYPE_ID";

                var results = (await db.QueryAsync<DataTypeDto>(sql)).ToList();

                // Legacy behavior: return 0 for agen_id instead of null
                results.ForEach(r =>
                {
                    if (!r.AGEN_ID.HasValue)
                    {
                        r.AGEN_ID = 0;
                    }
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving datatypes");
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
