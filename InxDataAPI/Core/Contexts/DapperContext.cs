using Microsoft.Data.SqlClient;
using System.Data;

namespace InxDataAPI.Core.Contexts
{
    public class DapperContext(IConfiguration configuration)
    {
        private readonly string ConnectionStringSQL = configuration.GetConnectionString("SQL") ?? string.Empty;

        // https://www.ithome.com.tw/news/130630
        // 微軟要以新釋出的Microsoft.Data.SqlClient取代舊有的System.Data.SqlClient，前者將同時支援.NET Core和.NET Framework
        public IDbConnection GetSqlConnection() => new SqlConnection(ConnectionStringSQL);
    }
}
