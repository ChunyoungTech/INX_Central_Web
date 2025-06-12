using Microsoft.Data.SqlClient;
using System.Data;
using Serilog;

namespace INX_InfluxDB.Contexts
{
    /// <summary>
    /// for dapper
    /// </summary>
    public class DapperContext(IConfiguration _configuration)
    {
        private readonly string ConnectionStringSQL = _configuration.GetConnectionString("SQL") ?? string.Empty;

        // https://www.ithome.com.tw/news/130630
        // 微軟要以新釋出的Microsoft.Data.SqlClient取代舊有的System.Data.SqlClient，前者將同時支援.NET Core和.NET Framework
        //public IDbConnection GetSqlConnection() => new SqlConnection(ConnectionStringSQL);
        public IDbConnection GetSqlConnection()
        {
            try
            {
                return new SqlConnection(ConnectionStringSQL);
            }
            catch (ArgumentException ex)
            {            
                Log.Error(ex, $"在 GetSqlConnection() 中，SQL 連接字串格式錯誤: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {         
                Log.Error(ex, $"在 GetSqlConnection() 中，建立 SQL 連線時發生未知錯誤: {ex.Message}");
                return null;
            }
        }


       public IDbConnection GetSqlConnection(string name = "SQL")
    {
        string connStr = string.Empty;
        try
        {
            connStr = _configuration.GetConnectionString(name) ?? string.Empty;
            return new SqlConnection(connStr);
        }
        catch (ArgumentException ex)
        {
            Log.Error(ex, $"在 GetSqlConnection('{name}') 中，連接字串格式錯誤: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"在 GetSqlConnection('{name}') 中，建立連線時發生未知錯誤: {ex.Message}");
            return null;
        }
    }
    }
}