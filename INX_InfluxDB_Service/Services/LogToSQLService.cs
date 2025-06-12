using Dapper;
using INX_InfluxDB.Contexts;
using Serilog;

namespace INX_InfluxDB.Services 
{
    public class LogToSQLService
    {
        private  DapperContext context;
        private IConfiguration configuration;
        public LogToSQLService(DapperContext _context, IConfiguration _configuration)
        {
            this.context = _context;
            this.configuration = _configuration;
        }

        public async Task LogToSQLAsync(string logMessage)
        {
            await SaveLogToDatabaseAsync(logMessage);
        }

        private async Task SaveLogToDatabaseAsync(string logMessage)
        {
            string sql = "INSERT INTO IDBLocalLog (Message, Fac, DT) VALUES (@Message, @Fac, @DT)";
            var parameters = new
            {
                Message = logMessage,
                Fac = configuration.GetValue<string>("FacID"),
                DT = DateTime.Now
            };

            try
            {
                using var conn = context.GetSqlConnection("SQL1");
                await conn.ExecuteAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Log ¼g¤J¥¢±Ñ");
            }
        }
    }
}

