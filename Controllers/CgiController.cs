using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using HdbApi.Models;
using System.Text.Json;
using System.Text;

namespace HdbApi.Controllers
{
    [ApiController]
    [Route("cgi")]
    public class CgiController : ControllerBase
    {
        private readonly Services.IDatabaseService _databaseService;
        private readonly ILogger<CgiController> _logger;

        public CgiController(Services.IDatabaseService databaseService, ILogger<CgiController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Query Time-Series Data (Legacy CGI Program)
        /// </summary>
        /// <remarks>
        /// Calls the stored procedure used by the legacy CGI program for backwards compatibility
        /// </remarks>
        /// <param name="svr">HDB instance name</param>
        /// <param name="sdi">Comma-separated list of SDIs (enter one per line or comma-separated)</param>
        /// <param name="tstp">Interval table {INSTANT, HOUR, DAY, MONTH, YEAR, WY}</param>
        /// <param name="t1">Start date (allowed formats: yyyy-MM-dd, yyyy-MM-ddTHH:mm:ss, etc.)</param>
        /// <param name="t2">End date (allowed formats: yyyy-MM-dd, yyyy-MM-ddTHH:mm:ss, etc.)</param>
        /// <param name="table">Optional - HDB Table {R, M, B}</param>
        /// <param name="mrid">Optional - Model Run ID if table=M</param>
        /// <param name="format">Output format {json, html, 1, 2, 3, 4, 5, 6, 7, 8, 88, 9, 99}</param>
        [HttpGet]
        [Produces("application/json", "text/json", "text/html")]
        public async Task<IActionResult> GetCgiData(
            [FromQuery] string svr,
            [FromQuery] string sdi,
            [FromQuery] string? t1,
            [FromQuery] string? t2,
            [FromQuery] string tstp = "dy",
            [FromQuery] CgiTableType table = CgiTableType.R,
            [FromQuery] string mrid = "0",
            [FromQuery] string format = "json")
        {
            if (string.IsNullOrEmpty(svr) || string.IsNullOrEmpty(sdi) || string.IsNullOrEmpty(t1) || string.IsNullOrEmpty(t2))
            {
                return BadRequest(new { error = "Required parameters: svr, sdi, t1, t2" });
            }

            IDbConnection? db = null;

            try
            {
                // For now, we'll use the database service to get connection
                // In the future, this could be enhanced to support different HDB instances
                db = await _databaseService.GetConnectionAsync(HttpContext);

                // Parse and validate dates
                if (!DateTime.TryParse(t1, out DateTime t1Date) || !DateTime.TryParse(t2, out DateTime t2Date))
                {
                    return BadRequest(new { error = "Invalid date format for t1 or t2" });
                }

                // Snap dates based on timestep
                (t1Date, t2Date) = SnapDatesToTimeStep(t1Date, t2Date, tstp);

                // Build query parameters
                var parameters = BuildQueryParameters(sdi, t1Date, t2Date, table, mrid, tstp);

                // Execute query and get data
                var data = await GetCgiJsonData(db, parameters, sdi, t1Date, t2Date, table, mrid, tstp);

                // Return based on format
                if (format.ToLower() == "json")
                {
                    return Ok(data);
                }
                else if (format.ToLower() == "html" || format == "2" || format == "4")
                {
                    var result = BuildHtmlOutput(data, format);
                    return Content(result, "text/html");
                }
                else
                {
                    // For other formats, return text output
                    var result = BuildTextOutput(data, format);
                    return Content(result, "text/plain");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CGI request for SVR {Svr}", svr);
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

        private (DateTime t1, DateTime t2) SnapDatesToTimeStep(DateTime t1, DateTime t2, string tstp)
        {
            var normalized = tstp.ToLower();
            switch (normalized)
            {
                case "in":
                case "instant":
                case "hr":
                case "hour":
                    return (t1, t2.AddDays(1)); // Include the entire last day for hourly/instant data like old cgi
                case "dy":
                case "day":
                    return (
                        new DateTime(t1.Year, t1.Month, t1.Day, 0, 0, 0),
                        new DateTime(t2.Year, t2.Month, t2.Day, 0, 0, 0)
                    );
                case "mn":
                case "month":
                    return (
                        new DateTime(t1.Year, t1.Month, 1, 0, 0, 0),
                        new DateTime(t2.Year, t2.Month, 1, 0, 0, 0)
                    );
                case "yr":
                case "year":
                    return (
                        new DateTime(t1.Year, 1, 1, 0, 0, 0),
                        new DateTime(t2.Year, 1, 1, 0, 0, 0)
                    );
                case "wy":
                    return (
                        new DateTime(t1.Year - 1, 10, 1, 0, 0, 0),
                        new DateTime(t2.Year, 1, 9, 30, 0, 0)
                    );
                default:
                    return (t1, t2);
            }
        }

        private string GetTableSuffix(string tstp)
        {
            var normalized = tstp.ToLower();
            return normalized switch
            {
                "in" or "instant" => "INSTANT",
                "hr" or "hour" => "HOUR",
                "dy" or "day" => "DAY",
                "mn" or "month" => "MONTH",
                "yr" or "year" => "YEAR",
                "wy" => "WY",
                _ => "DAY"
            };
        }

        private DynamicParameters BuildQueryParameters(string sdi, DateTime t1, DateTime t2, CgiTableType table, string mrid, string tstp)
        {
            var parameters = new DynamicParameters();
            parameters.Add("sdi", sdi);
            parameters.Add("t1", t1);
            parameters.Add("t2", t2);

            if (table == CgiTableType.M && !string.IsNullOrEmpty(mrid) && mrid != "0")
            {
                parameters.Add("mrid", mrid);
            }

            return parameters;
        }

        private async Task<CgiModel.HdbCgiJson> GetJsonData(IDbConnection db, DynamicParameters parameters, string sdi, DateTime t1, DateTime t2, CgiTableType table, string mrid, string tstp)
        {
            // Build table name
            var tableSuffix = GetTableSuffix(tstp);
            var tableName = $"{table.ToString().ToUpper()}_{tableSuffix}";

            // Build SQL query
            var sql = $"select START_DATE_TIME as HDB_DATETIME, TO_CHAR(VALUE) as VALUE, SITE_DATATYPE_ID " +
                     $"from {tableName} " +
                     $"where SITE_DATATYPE_ID in ({sdi}) " +
                     $"and START_DATE_TIME between :t1 and :t2";

if (table == CgiTableType.M && !string.IsNullOrEmpty(mrid) && mrid != "0")
            {
                sql += " and MODEL_RUN_ID = :mrid";
            }

            sql += " order by SITE_DATATYPE_ID, START_DATE_TIME";

            var dataResults = await db.QueryAsync(sql, parameters);

            // Get site/datatype info
            var sdiList = sdi.Split(',').Select(x => x.Trim()).ToList();
            var sdiParams = string.Join(",", sdiList.Select((x, i) => $":sdi{i}"));
            var infoParams = new DynamicParameters();
            for (int i = 0; i < sdiList.Count; i++)
            {
                infoParams.Add($"sdi{i}", sdiList[i]);
            }

            var infoSql = @"
                select distinct A.SITE_DATATYPE_ID, B.SITE_NAME, C.DATATYPE_NAME, D.UNIT_COMMON_NAME,
                       B.LAT, B.LONGI, B.ELEVATION, B.DB_SITE_CODE as DB
                from HDB_SITE_DATATYPE A
                join HDB_SITE B on A.SITE_ID = B.SITE_ID
                join HDB_DATATYPE C on A.DATATYPE_ID = C.DATATYPE_ID
                join HDB_UNIT D on C.UNIT_ID = D.UNIT_ID
                where A.SITE_DATATYPE_ID in (" + sdiParams + ")";
            
            var infoResults = await db.QueryAsync(infoSql, infoParams);

            // Build JSON response
            var jsonOut = new CgiModel.HdbCgiJson
            {
                QueryDate = DateTime.Now.ToString("G"),
                StartDate = t1.ToString("G"),
                EndDate = t2.ToString("G"),
                TimeStep = tableSuffix,
                DataSource = table == CgiTableType.M ? "Modeled" : "Observed",
                Series = new List<CgiModel.Sites>()
            };

            foreach (var info in infoResults)
            {
                var site = new CgiModel.Sites
                {
                    SDI = info.SITE_DATATYPE_ID.ToString(),
                    SiteName = info.SITE_NAME,
                    DataTypeName = info.DATATYPE_NAME,
                    DataTypeUnit = info.UNIT_COMMON_NAME,
                    Latitude = info.LAT?.ToString(),
                    Longitude = info.LONGI?.ToString(),
                    Elevation = info.ELEVATION?.ToString(),
                    DB = info.DB,
                    MRID = table == CgiTableType.M ? mrid : null,
                    Data = new List<CgiModel.Data>()
                };

                var siteData = dataResults.Where(d => d.SITE_DATATYPE_ID.ToString() == site.SDI).ToList();
                foreach (var dataPoint in siteData)
                {
                    site.Data.Add(new CgiModel.Data
                    {
                        t = DateTime.Parse(dataPoint.HDB_DATETIME.ToString()).ToString("G"),
                        v = dataPoint.VALUE
                    });
                }

                jsonOut.Series.Add(site);
            }

            return jsonOut;
        }

        private async Task<string> GetHtmlData(IDbConnection db, DynamicParameters parameters, string sdi, DateTime t1, DateTime t2, CgiTableType table, string mrid, string tstp, string format)
        {
            var jsonData = await GetJsonData(db, parameters, sdi, t1, t2, table, mrid, tstp);
            return BuildHtmlOutput(jsonData, format);
        }

        private async Task<string> GetTextData(IDbConnection db, DynamicParameters parameters, string sdi, DateTime t1, DateTime t2, CgiTableType table, string mrid, string tstp, string format)
        {
            var jsonData = await GetJsonData(db, parameters, sdi, t1, t2, table, mrid, tstp);
            return BuildTextOutput(jsonData, format);
        }

        private string BuildHtmlOutput(CgiModel.HdbCgiJson data, string format)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><title>HDB CGI Data</title></head><body>");

            bool hasPreamble = format == "2" || format.ToLower() == "html";
            if (hasPreamble)
            {
                html.AppendLine($"<h1>HDB Time Series Data</h1>");
                html.AppendLine($"<p>Query Date: {data.QueryDate}</p>");
                html.AppendLine($"<p>Start Date: {data.StartDate}</p>");
                html.AppendLine($"<p>End Date: {data.EndDate}</p>");
                html.AppendLine($"<p>Time Step: {data.TimeStep}</p>");
                html.AppendLine($"<p>Data Source: {data.DataSource}</p>");
            }

            if (data.Series != null)
            {
                foreach (var series in data.Series)
                {
                    if (hasPreamble)
                    {
                        html.AppendLine($"<h2>SDI: {series.SDI} - {series.SiteName}</h2>");
                        html.AppendLine($"<p>Data Type: {series.DataTypeName} ({series.DataTypeUnit})</p>");
                        html.AppendLine($"<p>Location: {series.Latitude}, {series.Longitude} (Elevation: {series.Elevation})</p>");
                    }

                    if (series.Data != null && series.Data.Any())
                    {
                        html.AppendLine("<table border='1'><tr><th>DateTime</th><th>Value</th></tr>");
                        foreach (var point in series.Data)
                        {
                            html.AppendLine($"<tr><td>{point.t}</td><td>{point.v}</td></tr>");
                        }
                        html.AppendLine("</table>");
                    }
                    else
                    {
                        html.AppendLine("<p>No data available</p>");
                    }
                }
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }
        private async Task<CgiModel.HdbCgiJson> GetCgiJsonData(IDbConnection db, DynamicParameters parameters, string sdi, DateTime t1, DateTime t2, CgiTableType table, string mrid, string tstp)
        {
            // TODO: Implement database query to get data
            // For now, return empty
            return new CgiModel.HdbCgiJson
            {
                QueryDate = DateTime.Now.ToString(),
                StartDate = t1.ToString(),
                EndDate = t2.ToString(),
                TimeStep = tstp,
                DataSource = table == CgiTableType.M ? "Modeled" : "Observed",
                Series = new List<CgiModel.Sites>() // Empty for now
            };
        }
        private List<string> BuildTxtArray(CgiModel.HdbCgiJson data)
        {
            var txt = new List<string>();
            txt.Add("USBR Hydrologic Database (HDB) System Data Access");
            txt.Add(" ");
            txt.Add("The Bureau of Reclamation makes efforts to maintain the accuracy of data found ");
            txt.Add("in the HDB system databases but the data is largely unverified and should be ");
            txt.Add("considered preliminary and subject to change.  Data and services are provided ");
            txt.Add("with the express understanding that the United States Government makes no ");
            txt.Add("warranties, expressed or implied, concerning the accuracy, completeness, ");
            txt.Add("usability, or suitability for any particular purpose of the information or data ");
            txt.Add("obtained by access to this computer system. The United States shall be under no ");
            txt.Add("liability whatsoever to any individual or group entity by reason of any use made ");
            txt.Add("thereof. ");
            txt.Add(" ");
            if (data.Series != null)
            {
                foreach (var series in data.Series)
                {
                    txt.Add($"SDI {series.SDI}: {series.SiteName?.ToUpper()} - {series.DataTypeName?.ToUpper()} in {series.DataTypeUnit?.ToUpper()}");
                }
            }
            txt.Add("BEGIN DATA");
            // header
            string headLine = "DATETIME";
            if (data.Series != null)
            {
                foreach (var series in data.Series)
                {
                    headLine += $", SDI_{series.SDI}";
                }
            }
            txt.Add(headLine);
            // data
            // To build rows, need to group by datetime
            var dataByTime = new Dictionary<string, Dictionary<string, string>>();
            if (data.Series != null)
            {
                foreach (var series in data.Series)
                {
                    if (series.Data != null)
                    {
                        foreach (var point in series.Data)
                        {
                            if (!dataByTime.ContainsKey(point.t ?? ""))
                            {
                                dataByTime[point.t ?? ""] = new Dictionary<string, string>();
                            }
                            dataByTime[point.t ?? ""][$"SDI_{series.SDI}"] = point.v ?? "";
                        }
                    }
                }
            }
            foreach (var kvp in dataByTime.OrderBy(k => k.Key))
            {
                string row = kvp.Key;
                if (data.Series != null)
                {
                    foreach (var series in data.Series)
                    {
                        string val = kvp.Value.ContainsKey($"SDI_{series.SDI}") ? kvp.Value[$"SDI_{series.SDI}"] : "";
                        if (val == "") val = "NaN";
                        row += $", {val}";
                    }
                }
                txt.Add(row);
            }
            txt.Add("END DATA");
            return txt;
        }

        private string FormatOutput(string[] outFile, string outFormat)
        {
            var htmlOut = new List<string>();
            bool isCSV = false, hasPreamble = false, isAmChart = false, isDyGraph = false, isHdbWebSeriesQuery = false, isJson = false;
            if (outFormat == "1" || outFormat == "3" || outFormat == "5")
            { isCSV = true; }
            if (outFormat == "1" || outFormat == "2")
            { hasPreamble = true; }
            if (outFormat == "8" || outFormat == "88")
            { isHdbWebSeriesQuery = true; }
            if (outFormat == "9" || outFormat.ToLower() == "graph")
            { isDyGraph = true; }
            if (outFormat == "99")
            { isAmChart = true; }
            if (outFormat.ToLower() == "csv")
            {
                hasPreamble = false;
                isCSV = true;
            }
            if (outFormat.ToLower() == "html")
            {
                hasPreamble = false;
            }
            if (outFormat.ToLower() == "json")
            {
                isJson = true;
            }

            int startOfDataRow = Array.IndexOf(outFile, "BEGIN DATA");

            // format == 99 or format == graph
            if (isAmChart || isDyGraph)
            { 
                // Placeholder for dyGraphs
                htmlOut.Add("<html><body><p>DyGraphs chart would be here</p></body></html>");
            }
            // format == 8
            else if (isHdbWebSeriesQuery)
            {
                if (outFormat == "8")
                {
                    htmlOut.Add("<PRE>");
                    for (int i = startOfDataRow + 2; i < outFile.Length - 1; i++)
                    { htmlOut.Add(outFile[i] + "\r\n"); }
                }
                else
                {
                    var headerString = "Date,";
                    for (int i = 12; i < startOfDataRow; i++)
                    { headerString += outFile[i].Replace(",", " ") + ","; }
                    htmlOut.Add(headerString.TrimEnd(',') + "\n");
                    for (int i = startOfDataRow + 2; i < outFile.Length - 1; i++)
                    { htmlOut.Add(outFile[i] + "\n"); }
                }
            }
            // format == json
            else if (isJson)
            {
                // Placeholder
                htmlOut.Add("{}");
            }
            // format == 1, 2, 3, 4
            else
            {
                htmlOut.Add("<HTML>");
                htmlOut.Add("<HEAD>");
                htmlOut.Add("<TITLE>Bureau of Reclamation HDB Data</TITLE>");
                htmlOut.Add("</HEAD>");
                htmlOut.Add("<BODY>");

                // Add preamble
                if (hasPreamble)
                {
                    htmlOut.Add("<PRE>");
                    htmlOut.Add("<B>" + outFile[0] + "</B>");
                    for (int i = 1; i <= startOfDataRow - 1; i++)
                    {
                        htmlOut.Add("<BR>" + outFile[i]);
                    }
                    htmlOut.Add("</PRE>");
                    htmlOut.Add("<p>");
                }
                // Add data
                for (int i = startOfDataRow; i < outFile.Length; i++)
                {
                    if (isCSV && hasPreamble)
                    {
                        if (i == startOfDataRow)
                        { htmlOut.Add("<PRE>"); }
                        htmlOut.Add("<BR>" + outFile[i]);
                    }
                    else if (isCSV && !hasPreamble)
                    {
                        if (i == startOfDataRow)
                        {
                            htmlOut.Add("<PRE>");
                            i++;
                            htmlOut.Add(outFile[i]);
                        }
                        else if (i == outFile.Length - 1)
                        { }
                        else
                        { htmlOut.Add("<BR>" + outFile[i]); }
                    }
                    else
                    {
                        if (i == startOfDataRow)
                        {
                            htmlOut.Add("<TABLE BORDER=1>");
                            i++;
                            htmlOut.Add("<TR><TH>" + outFile[i].Replace(",", "</TH><TH>") + "</TH></TR>");
                        }
                        else if (i == outFile.Length - 1)
                        { }
                        else
                        { htmlOut.Add("<TR><TD>" + outFile[i].Replace(",", "</TD><TD>") + "</TD></TR>"); }
                    }
                }
                // Add final lines
                if (isCSV)
                { htmlOut.Add("</PRE>"); }
                else
                { htmlOut.Add("</TABLE>"); }
                htmlOut.Add("</BODY></HTML>");
            }
            return string.Join("", htmlOut);
        }
        private string BuildTextOutput(CgiModel.HdbCgiJson data, string format)
        {
            var txt = BuildTxtArray(data);
            return FormatOutput(txt.ToArray(), format);
        }
        private string OldBuildTextOutput(CgiModel.HdbCgiJson data, string format)
        {
            var text = new StringBuilder();
            switch (format.ToLower())
            {
                case "1":
                    // Tab-separated values
                    text.AppendLine($"HDB Time Series Data - Query Date: {data.QueryDate}");
                    text.AppendLine($"Start Date: {data.StartDate}, End Date: {data.EndDate}, Time Step: {data.TimeStep}, Data Source: {data.DataSource}");
                    text.AppendLine();
                    if (data.Series != null)
                    {
                        foreach (var series in data.Series)
                        {
                            text.AppendLine($"SDI: {series.SDI}");
                            text.AppendLine("DateTime\tValue");
                            if (series.Data != null)
                            {
                                foreach (var point in series.Data)
                                {
                                    text.AppendLine($"{point.t}\t{point.v}");
                                }
                            }
                            text.AppendLine();
                        }
                    }
                    break;
                case "88":
                    // Pure CSV for DyGraph generation
                    if (data.Series != null)
                    {
                        foreach (var series in data.Series)
                        {
                            text.AppendLine($"SDI: {series.SDI}");
                            text.AppendLine("Date,Value");
                            if (series.Data != null)
                            {
                                foreach (var point in series.Data)
                                {
                                    text.AppendLine($"{point.t},{point.v}");
                                }
                            }
                            text.AppendLine();
                        }
                    }
                    break;
                case "8":
                    // Pisces HdbWebSeries Query
                    text.AppendLine("<PRE>");
                    if (data.Series != null)
                    {
                        foreach (var series in data.Series)
                        {
                            if (series.Data != null)
                            {
                                foreach (var point in series.Data)
                                {
                                    text.AppendLine($"{point.t}\t{point.v}");
                                }
                            }
                        }
                    }
                    text.AppendLine("</PRE>");
                    break;
                case "9":
                    // DyGraphs
                    text.AppendLine($"HDB Time Series Data - Query Date: {data.QueryDate}");
                    text.AppendLine($"Start Date: {data.StartDate}, End Date: {data.EndDate}, Time Step: {data.TimeStep}, Data Source: {data.DataSource}");
                    text.AppendLine();
                    // For chart, perhaps JSON or specific format
                    // For now, same as default
                    goto default;
                default:
                    // Generic text output
                    text.AppendLine($"HDB Time Series Data - Query Date: {data.QueryDate}");
                    text.AppendLine($"Start Date: {data.StartDate}, End Date: {data.EndDate}, Time Step: {data.TimeStep}, Data Source: {data.DataSource}");
                    text.AppendLine();
                    if (data.Series != null)
                    {
                        foreach (var series in data.Series)
                        {
                            text.AppendLine($"SDI: {series.SDI} - {series.SiteName}");
                            text.AppendLine($"Data Type: {series.DataTypeName} ({series.DataTypeUnit})");
                            text.AppendLine($"Location: {series.Latitude}, {series.Longitude} (Elevation: {series.Elevation})");
                            text.AppendLine("DateTime\tValue");
                            if (series.Data != null)
                            {
                                foreach (var point in series.Data)
                                {
                                    text.AppendLine($"{point.t}\t{point.v}");
                                }
                            }
                            text.AppendLine();
                        }
                    }
                    break;
            }
            return text.ToString();
        }
    }



    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum CgiTableType
    {
        R,
        M,
        B
    }
}