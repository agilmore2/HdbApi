using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;
using System.Text.Json.Serialization;
using Oracle.ManagedDataAccess.Client;

namespace HdbApi.Controllers
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SeriesInterval { instant, hour, day, month, year }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SeriesTableType { R, M }

    [ApiController]
    [Route("series")]
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
        /// <remarks>
        /// Gets Time-Series Data given certain input filters
        /// </remarks>
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
            [FromQuery] SeriesInterval interval = SeriesInterval.day,
            [FromQuery] SeriesTableType table = SeriesTableType.R,
            [FromQuery] int? mrid = null)
        {
            if (string.IsNullOrEmpty(sdi))
            {
                return BadRequest(new { error = "Site Datatype ID (sdi) is required" });
            }

            if (table == SeriesTableType.M && !mrid.HasValue)
            {
                return BadRequest(new { error = "Model Run ID (mrid) is required when table=M" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // Build table name
                var tableName = $"{table.ToString().ToUpper()}_{interval.ToString().ToUpper()}";

                // Build SQL query
                var sql = $"select START_DATE_TIME as DATETIME, cast(VALUE as varchar(20)) as VALUE " +
                         $"from {tableName} " +
                         $"where SITE_DATATYPE_ID = :sdi " +
                         $"and START_DATE_TIME between :t1 and :t2";

                if (table == SeriesTableType.M && mrid.HasValue)
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

        /// <summary>
        /// Write Observed Data
        /// </summary>
        /// <remarks>
        /// Write Time-Series Data points for Observed Data
        /// </remarks>
        [HttpPost("r-write")]
        public async Task<IActionResult> WriteObservedData([FromBody] List<Models.PointModel.ObservedPoint> input)
        {
            if (input == null || !input.Any())
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var hdbProcessor = new App_Code.HdbCommands();
                
                foreach (Models.PointModel.ObservedPoint point in input)
                {
                    if (point.loading_application_id < 1)
                    {
                        point.loading_application_id = -99;
                    }
                    if (point.computation_id < 1)
                    {
                        point.computation_id = -99;
                    }
                    if (point.data_flags == null)
                    {
                        point.data_flags = "";
                    }
                    var result = hdbProcessor.modify_r_base_raw(db, point.site_datatype_id, point.interval, point.start_date_time, point.value, point.overwrite_flag, point.validation, point.do_update_y_or_n, point.loading_application_id, point.computation_id, point.data_flags);
                }

                return Ok(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing observed series data");
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
        /// Write Modeled Data
        /// </summary>
        /// <remarks>
        /// Write Time-Series Data points for Modeled Data
        /// </remarks>
        [HttpPost("m-write")]
        public async Task<IActionResult> WriteModeledData([FromBody] List<Models.PointModel.ModeledPoint> input)
        {
            if (input == null || !input.Any())
            {
                return BadRequest(new { error = "Invalid request data" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var hdbProcessor = new App_Code.HdbCommands();
                
                foreach (Models.PointModel.ModeledPoint point in input)
                {
                    var result = hdbProcessor.modify_m_table_raw(db, point.model_run_id, point.site_datatype_id, point.start_date_time, point.value, point.interval, point.do_update_y_or_n);
                }

                return Ok(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing modeled series data");
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
        /// Delete Observed Data
        /// </summary>
        /// <remarks>
        /// Delete observed time-series data points for a given site-datatype
        /// </remarks>
        /// <param name="sdi">Site Datatype ID</param>
        /// <param name="interval">Time interval (instant, hour, day, etc.)</param>
        /// <param name="startDate">Optional start date for deletion range</param>
        /// <param name="endDate">Optional end date for deletion range</param>
        [HttpDelete("r-delete")]
        public async Task<IActionResult> DeleteObservedData([FromQuery] string sdi, [FromQuery] SeriesInterval interval = SeriesInterval.instant, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(sdi))
            {
                return BadRequest(new { error = "Site Datatype ID (sdi) is required" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // First, select the data points to delete
                var selectSql = "SELECT start_date_time FROM r_base WHERE site_datatype_id = :sdi";
                var parameters = new DynamicParameters();
                parameters.Add("sdi", sdi);

                if (startDate.HasValue && endDate.HasValue)
                {
                    selectSql += " AND start_date_time BETWEEN :startDate AND :endDate";
                    parameters.Add("startDate", startDate.Value);
                    parameters.Add("endDate", endDate.Value);
                }
                else if (startDate.HasValue)
                {
                    selectSql += " AND start_date_time >= :startDate";
                    parameters.Add("startDate", startDate.Value);
                }
                else if (endDate.HasValue)
                {
                    selectSql += " AND start_date_time <= :endDate";
                    parameters.Add("endDate", endDate.Value);
                }

                var dataPoints = await db.QueryAsync<DateTime>(selectSql, parameters);

                // Delete each point using the HdbCommands class
                var hdbProcessor = new App_Code.HdbCommands();
                int deletedCount = 0;
                foreach (var dateTime in dataPoints)
                {
                    hdbProcessor.delete_from_hdb(db, decimal.Parse(sdi), dateTime, interval.ToString());
                    deletedCount++;
                }

                return Ok(new { status = "success", message = $"Deleted {deletedCount} data points" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting observed series data for SDI {Sdi}", sdi);
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
        /// Delete Modeled Data
        /// </summary>
        /// <remarks>
        /// Delete modeled time-series data points for a given site-datatype and model run
        /// </remarks>
        /// <param name="sdi">Site Datatype ID</param>
        /// <param name="mrid">Model Run ID</param>
        /// <param name="interval">Time interval (instant, hour, day, etc.)</param>
        /// <param name="startDate">Optional start date for deletion range</param>
        /// <param name="endDate">Optional end date for deletion range</param>
        [HttpDelete("m-delete")]
        public async Task<IActionResult> DeleteModeledData([FromQuery] string sdi, [FromQuery] int mrid, [FromQuery] SeriesInterval interval = SeriesInterval.instant, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(sdi))
            {
                return BadRequest(new { error = "Site Datatype ID (sdi) is required" });
            }

            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // First, select the data points to delete
                var tableName = $"M_{interval.ToString().ToUpper()}";
                var selectSql = $"SELECT start_date_time FROM {tableName} WHERE site_datatype_id = :sdi AND model_run_id = :mrid";
                var parameters = new DynamicParameters();
                parameters.Add("sdi", sdi);
                parameters.Add("mrid", mrid);

                if (startDate.HasValue && endDate.HasValue)
                {
                    selectSql += " AND start_date_time BETWEEN :startDate AND :endDate";
                    parameters.Add("startDate", startDate.Value);
                    parameters.Add("endDate", endDate.Value);
                }
                else if (startDate.HasValue)
                {
                    selectSql += " AND start_date_time >= :startDate";
                    parameters.Add("startDate", startDate.Value);
                }
                else if (endDate.HasValue)
                {
                    selectSql += " AND start_date_time <= :endDate";
                    parameters.Add("endDate", endDate.Value);
                }

                var dataPoints = await db.QueryAsync<DateTime>(selectSql, parameters);

                // Delete each point using the HdbCommands class
                var hdbProcessor = new App_Code.HdbCommands();
                int deletedCount = 0;
                foreach (var dateTime in dataPoints)
                {
                    hdbProcessor.delete_from_hdb(db, decimal.Parse(sdi), dateTime, interval.ToString(), mrid);
                    deletedCount++;
                }

                return Ok(new { status = "success", message = $"Deleted {deletedCount} modeled data points" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting modeled series data for SDI {Sdi}", sdi);
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

    /// <summary>
    /// Request model for writing time series data
    /// </summary>
    public class SeriesWriteRequest
    {
        /// <summary>
        /// Site Datatype ID
        /// </summary>
        [JsonPropertyName("sdi")]
        public string? Sdi { get; set; }

        /// <summary>
        /// Model Run ID (required for modeled data)
        /// </summary>
        [JsonPropertyName("mrid")]
        public int? Mrid { get; set; }

        /// <summary>
        /// Whether to overwrite existing data
        /// </summary>
        [JsonPropertyName("overwrite")]
        public bool Overwrite { get; set; } = false;

        /// <summary>
        /// List of data points to write
        /// </summary>
        [JsonPropertyName("data")]
        public List<SeriesDataPoint>? Data { get; set; }
    }

    /// <summary>
    /// Data point for time series
    /// </summary>
    public class SeriesDataPoint
    {
        /// <summary>
        /// Date and time of the data point
        /// </summary>
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Value of the data point
        /// </summary>
        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}
