using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeriesController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<SeriesController> _logger;

        public SeriesController(Services.IDatabaseService databaseService, ILogger<SeriesController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Query Time-Series Data
        /// </summary>
        /// <param name="sdi">Site Datatype ID</param>
        /// <param name="t1">Start Date in YYYY-MM-DD HH:MM format</param>
        /// <param name="t2">End Date in YYYY-MM-DD HH:MM format</param>
        /// <param name="interval">Time interval (hour, day, month, etc.)</param>
        /// <param name="table">Data table type (R for observed, M for modeled)</param>
        /// <param name="mrid">Model Run ID (required if table=M)</param>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string sdi,
            [FromQuery] DateTime t1,
            [FromQuery] DateTime t2,
            [FromQuery] string interval = "day",
            [FromQuery] string table = "R",
            [FromQuery] int? mrid = null)
        {
            if (string.IsNullOrEmpty(sdi))
            {
                return BadRequest(new { error = "Site Datatype ID (sdi) is required" });
            }

            if (table.ToUpper() == "M" && !mrid.HasValue)
            {
                return BadRequest(new { error = "Model Run ID (mrid) is required when table=M" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // Build table name
                var tableName = $"{table.ToUpper()}_{interval.ToUpper()}";

                // Build SQL query
                var sql = $"select START_DATE_TIME as DATETIME, cast(VALUE as varchar(20)) as VALUE " +
                         $"from {tableName} " +
                         $"where SITE_DATATYPE_ID = :sdi " +
                         $"and START_DATE_TIME between :t1 and :t2";

                if (table.ToUpper() == "M" && mrid.HasValue)
                {
                    sql += " and MODEL_RUN_ID = :mrid";
                }

                sql += " order by START_DATE_TIME";

                var parameters = new { sdi = sdi, t1 = t1, t2 = t2, mrid = mrid };

                var results = await db.QueryAsync<TimeSeriesPointDto>(sql, parameters);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving time series data for SDI {Sdi}", sdi);
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