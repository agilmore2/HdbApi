using System.Data;
using System.IO;
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
        private HashSet<string>? _allowedHdbs;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
        }

        private HashSet<string> GetAllowedHdbs()
        {
            if (_allowedHdbs == null)
            {
                _allowedHdbs = LoadAllowedHdbsFromFile();
            }
            return _allowedHdbs;
        }

        private HashSet<string> LoadAllowedHdbsFromFile()
        {
            try
            {
                var allowedHdbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                if (File.Exists("hostnames.txt"))
                {
                    var lines = File.ReadAllLines("hostnames.txt");
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Extract database name from format "DATABASENAME - Description"
                            var dbName = line.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim();
                            if (!string.IsNullOrEmpty(dbName))
                            {
                                allowedHdbs.Add(dbName.ToUpper());
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("hostnames.txt file not found, using default allowed databases");
                    // Fallback to hardcoded list if file doesn't exist
                    allowedHdbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                    { 
                        "LCHDB", "UCHDB2", "UCHDBT", "YAOHDB", "ECOHDB", 
                        "LBOHDB", "KBOHDB", "PNHYD", "GPHYD", "FREEPDB1" 
                    };
                }
                
                return allowedHdbs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allowed HDBs from hostnames.txt, using default list");
                // Fallback to hardcoded list on error
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { 
                    "LCHDB", "UCHDB2", "UCHDBT", "YAOHDB", "ECOHDB", 
                    "LBOHDB", "KBOHDB", "PNHYD", "GPHYD", "FREEPDB1" 
                };
            }
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
            // Validate hdb against allowed list loaded from hostnames.txt
            var allowedHdbs = GetAllowedHdbs();
            if (!allowedHdbs.Contains(hdb.ToUpper()))
            {
                throw new ArgumentException($"HDB '{hdb}' is not in the allowed list of databases");
            }

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
            try
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            catch
            {
                // ignore closing errors
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}