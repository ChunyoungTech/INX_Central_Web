using InfluxDB.Client;
using INX_InfluxDB;
using INX_InfluxDB.Contexts;
using INX_InfluxDB.Services;
using INX_InfluxDB_Service.Services;
using Microsoft.Data.SqlClient;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSerilog((hostingContext, loggerConfig) =>
    {
        // ✅ 這裡統一設定 Serilog
        loggerConfig
            .MinimumLevel.Debug()
            .WriteTo.Console() // ✅小黑會顯示
            .ReadFrom.Configuration(hostingContext.Configuration); // ✅允許讀 appsettings.json
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<DapperContext>();
        services.AddSingleton<HistorianService>();
        services.AddSingleton<InfluxDBService>();
        services.AddSingleton<LogToSQLService>();
        services.AddSingleton<SettingLoaderService>();
        services.AddHostedService<Worker>();
        services.AddHostedService<LoaderUploadWorker>();
        services.AddHostedService<LiveUploadWorker>();
    })
    .Build();


try
{


    Log.Information("Starting up");


    // 在啟動背景服務之前進行 SQL 連接檢查和重試
    var configuration = host.Services.GetRequiredService<IConfiguration>();
    var connectionStringSQL = configuration.GetConnectionString("SQL") ?? string.Empty;
    await CheckDatabaseConnectionAndRetryAsync(connectionStringSQL);
    
    var connectionStringSQL1 = configuration.GetConnectionString("SQL1") ?? string.Empty;
    await CheckDatabaseConnectionAndRetryAsync(connectionStringSQL1);
   
    var url = configuration["InfluxDB:URL"] ?? string.Empty;
    var token = configuration["InfluxDB:Token"] ?? string.Empty;
    await CheckInfluxDBConnectionAndRetryAsync(url, token);

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "發生錯誤，請通知管理員");
}
finally
{
    Log.CloseAndFlush();
}

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
            // https://docs.microsoft.com/en-us/dotnet/core/extensions/windows-service
            .UseWindowsService()
            .ConfigureServices((hostContext, services) =>
            {
                #region 資料庫連線

                // Context
                services.AddSingleton<DapperContext>();

                #endregion 資料庫連線

                services.AddSingleton<HistorianService>();
                services.AddSingleton<InfluxDBService>();
                services.AddSingleton<LogToSQLService>();
                services.AddSingleton<SettingLoaderService>();
                services.AddHostedService<Worker>();
            })
            .UseSerilog((hostingContext, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(hostingContext.Configuration)
            );

async Task CheckDatabaseConnectionAndRetryAsync(string connectionStringSQL)
{
    int retryCount = 0;
    while (true)
    {
        try
        {
            using var connection = new SqlConnection(connectionStringSQL);
            await connection.OpenAsync();
            string _SQL = connectionStringSQL
                    .Split("Data Source=")[1]
                    .Split(';')[0];
            Log.Information($"成功連線到MSSQL {_SQL}");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            Log.Warning($"第 {retryCount} 次 資料庫連線失敗：{ex.Message}");
            Log.Error($"無法連線到MSSQL: {ex.Message}。30秒後重試...");
            await Task.Delay(TimeSpan.FromMinutes(0.5));
            Log.Error($"無法連線到MSSQL: {ex.Message}。1分鐘後重試...");
        }
    }
}

async Task CheckInfluxDBConnectionAndRetryAsync(string url, string token)
{
    while (true)
    {
        try
        {
            using var client = new InfluxDBClient(url, token);
            if (await client.PingAsync())
            {
                Log.Information("成功連線到InfluxDB...");
                break;
            }
            else
            {
                Log.Error($"無法連線到InfluxDB，1分鐘後重試...");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"無法連線到InfluxDB: {ex.Message}。1分鐘後重試...");
        }

        // 等待 1 分鐘後重試
        await Task.Delay(TimeSpan.FromMinutes(1));
    }
}