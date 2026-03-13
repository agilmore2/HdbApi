using System.Data;
using Oracle.ManagedDataAccess.Client;
using Dapper;

namespace HdbApi.Services
{
    public interface IDatabaseService
    {
        Task<IDbConnection> GetConnectionAsync(HttpContext context);
        Task<IDbConnection> GetConnectionAsync(string hdb, string username, string password);
        void CloseConnection(IDbConnection connection);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
        }

        public async Task<IDbConnection> GetConnectionAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("api_hdb", out var hdbValues) ||
                !context.Request.Headers.TryGetValue("api_user", out var userValues) ||
                !context.Request.Headers.TryGetValue("api_pass", out var passValues))
            {
                throw new KeyNotFoundException("HTTP Request Header Keys missing. Required headers: api_hdb, api_user, api_pass");
            }

            var hdb = hdbValues.FirstOrDefault();
            var username = userValues.FirstOrDefault();
            var password = passValues.FirstOrDefault();

            if (string.IsNullOrEmpty(hdb) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("HDB connection parameters cannot be null or empty");
            }

            return await GetConnectionAsync(hdb, username, password);
        }

        public async Task<IDbConnection> GetConnectionAsync(string hdb, string username, string password)
        {
            try
            {
                _logger.LogInformation("Connecting to HDB: {Hdb} as user: {Username}", hdb, username);

                // Special handling for hydromet databases
                if (hdb.ToLower() == "pnhyd" || hdb.ToLower() == "gphyd")
                {
                    // For hydromet, we might not need a database connection
                    // or we might need a different connection approach
                    throw new NotImplementedException("Hydromet database connections not yet implemented");
                }

                string connectionString = $"Data Source={hdb};User Id={username};Password={password};";

                // Add connection pooling for app_user and dba users
                if (username.ToLower() == "app_user" || username.ToLower().Contains("dba"))
                {
                    connectionString += "Min Pool Size=5;Max Pool Size=100;Connection Lifetime=120;Connection Timeout=15;" +
                                       "Incr Pool Size=5;Decr Pool Size=5";
                }

                var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to database {Hdb} as {Username}", hdb, username);
                throw;
            }
        }

        public void CloseConnection(IDbConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}