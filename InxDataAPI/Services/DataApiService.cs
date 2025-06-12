using Azure.Core;
using InxDataAPI.Models;
using Microsoft.Data.SqlClient;

public class DataApiService
{
    private readonly string _connectionString;

    public DataApiService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SQL");
    }
    public IDBDataAPIModel? GetTokenByValue(string apiName, string token, string clientIp)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand(@"
        SELECT TOP 1 * FROM IDBDataAPI
        WHERE APIName = @APIName
          AND Token = @Token
          AND (ClientIP = @ClientIP OR ClientIP = '0.0.0.0')
    ", conn);

        cmd.Parameters.AddWithValue("@APIName", apiName);
        cmd.Parameters.AddWithValue("@Token", token);
        cmd.Parameters.AddWithValue("@ClientIP", clientIp ?? "");

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new IDBDataAPIModel
            {
                SeqID = reader.GetInt32(reader.GetOrdinal("SeqID")),
                APIName = reader.GetString(reader.GetOrdinal("APIName")),
                Token = reader.GetString(reader.GetOrdinal("Token")),
                ClientIP = reader.GetString(reader.GetOrdinal("ClientIP")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                UserPWD = reader.GetString(reader.GetOrdinal("UserPWD")),
            };
        }
        return null;
    }
}
