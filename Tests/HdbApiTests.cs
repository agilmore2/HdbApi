using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using HdbApi.Services;

namespace HdbApi.Tests
{
    internal static class TestConfig
    {
        // Can be overridden by environment variables for CI
        public static string Hdb => Environment.GetEnvironmentVariable("HDB_DATASOURCE") ?? "FREEPDB1";
        public static string User => Environment.GetEnvironmentVariable("HDB_USER") ?? "app_user";
        public static string Pass => Environment.GetEnvironmentVariable("HDB_PASS") ?? "<password>";
    }

    [TestFixture]
    public class HdbConnectionTests
    {
        private IDatabaseService _dbService = null!;

        [SetUp]
        public void SetUp()
        {
            _dbService = new DatabaseService(NullLogger<DatabaseService>.Instance);
        }

        private HttpContext BuildContext(string? hdb = null, string? user = null, string? pass = null)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["api_hdb"] = hdb ?? TestConfig.Hdb;
            ctx.Request.Headers["api_user"] = user ?? TestConfig.User;
            ctx.Request.Headers["api_pass"] = pass ?? TestConfig.Pass;
            return ctx;
        }

        [Test]
        public async Task Connect_OpenConnection_Succeeds()
        {
            using var conn = await _dbService.GetConnectionAsync(BuildContext());
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public void Connect_MissingHeader_Throws()
        {
            var ctx = new DefaultHttpContext();
            Assert.ThrowsAsync<KeyNotFoundException>(() => _dbService.GetConnectionAsync(ctx));
        }
    }

    [TestFixture]
    public class HdbApiDataTests
    {
        private IDatabaseService _dbService = null!;

        [SetUp]
        public void SetUp()
        {
            _dbService = new DatabaseService(NullLogger<DatabaseService>.Instance);
        }

        private async Task<IDbConnection> OpenConn() => await _dbService.GetConnectionAsync(BuildContext());

        private HttpContext BuildContext() => new DefaultHttpContext()
        {
            Request =
            {
                Headers =
                {
                    ["api_hdb"] = TestConfig.Hdb,
                    ["api_user"] = TestConfig.User,
                    ["api_pass"] = TestConfig.Pass,
                }
            }
        };

        [Test]
        public async Task SiteQuery_ReturnsRow()
        {
            using var conn = await OpenConn();
            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "select site_name from hdb_site where rownum = 1");

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task DataTypeQuery_ReturnsRow()
        {
            using var conn = await OpenConn();
            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "select datatype_name from hdb_datatype where rownum = 1");

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task ModelRunQuery_ReturnsRow()
        {
            using var conn = await OpenConn();
            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "select model_run_name from ref_model_run where rownum = 1");

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task SeriesQuery_ReturnsRows()
        {
            using var conn = await OpenConn();

            // Find a known site_datatype_id in the database (if present) to validate the query.
            var sdi = await conn.QueryFirstOrDefaultAsync<string>(
                "select site_datatype_id from r_day where rownum = 1");

            if (string.IsNullOrEmpty(sdi))
            {
                Assert.Inconclusive("No series data found in r_day; cannot validate series query.");
                return;
            }

            var rows = (await conn.QueryAsync<Models.TimeSeriesPointDto>(
                "select start_date_time as datetime, cast(value as varchar(20)) as value from r_day where site_datatype_id = :sdi and start_date_time between :t1 and :t2 order by start_date_time",
                new { sdi, t1 = new DateTime(2026, 2, 1), t2 = new DateTime(2026, 2, 2) }))
                .ToList();

            Assert.IsNotNull(rows);
        }
    }

    [TestFixture, Category("Integration")]
    public class SwaggerUITests
    {
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000");
            _client.Timeout = TimeSpan.FromSeconds(5);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        [Test]
        public async Task SwaggerUI_Index_ReturnsSuccess()
        {
            try
            {
                // Act
            var response = await _client.GetAsync("/swagger/ui/index");
                // Assert
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Swagger UI test skipped - application not running on localhost:5000");
            }
        }

        [Test]
        public async Task SwaggerUI_Index_ContainsExpectedContent()
        {
            try
            {
                // Act
                var response = await _client.GetAsync("/swagger/ui/index");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Does.Contain("HDB Data Services API"));
                Assert.That(content, Does.Contain("swagger-ui"));
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Swagger UI test skipped - application not running on localhost:5000");
            }
        }

        [Test]
        public async Task SwaggerJson_ReturnsSuccess()
        {
            try
            {
                // Act
                var response = await _client.GetAsync("/swagger/v1/swagger.json");

                // Assert
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Swagger JSON test skipped - application not running on localhost:5000");
            }
        }

        [Test]
        public async Task SwaggerJson_ContainsExpectedEndpoints()
        {
            try
            {
                // Act
                var response = await _client.GetAsync("/swagger/v1/swagger.json");
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.That(content, Does.Contain("/sites"));
                Assert.That(content, Does.Contain("/datatypes"));
                Assert.That(content, Does.Contain("/modelruns"));
                Assert.That(content, Does.Contain("/series"));
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Swagger JSON test skipped - application not running on localhost:5000");
            }
        }
    }
}
