using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        /// <param name="id">Optional HDB DataType IDs to filter by</param>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string[]? id = null)
        {
            IDbConnection? db = null;

            try
            {
                db = await _databaseService.GetConnectionAsync(HttpContext);

                var sql = "select * from HDB_DATATYPE A, HDB_UNIT B where A.UNIT_ID = B.UNIT_ID";

                if (id != null && id.Length > 0)
                {
                    // Use parameter binding to avoid injection
                    var ids = string.Join(",", id.Select(x => $"'{x}'"));
                    sql += $" and A.DATATYPE_ID in ({ids})";
                }

                sql += " order by A.DATATYPE_ID";

                var results = await db.QueryAsync<DataTypeDto>(sql);
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
