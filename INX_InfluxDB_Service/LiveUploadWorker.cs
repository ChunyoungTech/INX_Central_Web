using Dapper;
using INX_InfluxDB.Contexts;
using INX_InfluxDB.Models;
using INX_InfluxDB.Services;
using INX_InfluxDB_Service.Services;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;

namespace INX_InfluxDB
{
    /// <summary>
    /// 上傳 Transfer_Table 資料至 FAC_Loader 的背景服務
    /// </summary>
    public class LiveUploadWorker : BackgroundService
    {
        private DapperContext context;
        private IConfiguration configuration;
        SettingLoaderService settingLoaderService;
        LogToSQLService logToSQLService;
        private readonly string facID;
        private readonly string CentralLiveServer;
        private readonly string LoaderDBName;
        private readonly string LiveTableName;
        List<TagData> tagDatas = new List<TagData>() { };

        LiveUploadWorkerLog privateLog = new LiveUploadWorkerLog();

        public LiveUploadWorker(IConfiguration configuration, DapperContext context, SettingLoaderService settingLoaderService, LogToSQLService logToSQLService)
        {
            try {
                this.configuration = configuration;
                this.settingLoaderService = settingLoaderService;
                this.logToSQLService = logToSQLService;
                this.context = context;
                facID = configuration.GetValue<string>("FacID") ?? throw new InvalidOperationException("SQL connection string 未設定或為空值。");
                var connectionString = configuration.GetConnectionString("SQL1");
                CentralLiveServer = connectionString
                    .Split("Data Source=")[1]
                    .Split(';')[0];
                LoaderDBName = configuration.GetValue<string>("LoaderDBName");
                LiveTableName = configuration.GetValue<string>("LiveTableName");
                if (string.IsNullOrEmpty(LoaderDBName))
                {
                    LoaderDBName = "FAC_Loader_TEST";
                    privateLog.WarningLog($"SQL connection string 未設定或為空值。已套用預設值 {LoaderDBName}。");
                    logToSQLService.LogToSQLAsync($"SQL connection string 未設定或為空值。已套用預設值 {LoaderDBName}。");
                }
                if (string.IsNullOrEmpty(LiveTableName))
                {
                    privateLog.WarningLog($"LiveTableName 未設定或為空值。 無法使用 {EXECUTE_NAME} ");
                    logToSQLService.LogToSQLAsync($"LiveTableName 未設定或為空值。 無法使用 {EXECUTE_NAME} ");
                }
            }
            catch (Exception ex)
            {
                privateLog.ErrorLog(ex, "LoaderUploadWorker 建構函式發生錯誤");
                logToSQLService.LogToSQLAsync($"LoaderUploadWorker 建構函式發生錯誤 {ex}");
                throw;
            }
            
        }

        private const string EXECUTE_NAME = "每分鐘同步 FAC8_Historian_Live";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoTask(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // 每分鐘執行一次
                }
                catch (Exception ex)
                {
                    privateLog.ErrorLog(ex, $"執行 {EXECUTE_NAME} 時發生錯誤");
                    await logToSQLService.LogToSQLAsync($"執行 {EXECUTE_NAME} 時發生錯誤 {ex}");
                }
            }
        }

        private bool _isRunning = false;

        protected async Task DoTask(CancellationToken stoppingToken)
        {
            if (_isRunning) return;
            _isRunning = true;

            try
            {
                privateLog.InfoLog($"開始 {EXECUTE_NAME}...");
                await logToSQLService.LogToSQLAsync($"開始 {EXECUTE_NAME}...");

                using (var conn = context.GetSqlConnection("SQL")) // 這裡已經是 SqlConnection 或 IDbConnection
                {
                    conn.Open();
                    var sql = $@"
            EXEC ('TRUNCATE TABLE [{LoaderDBName}].[dbo].[{LiveTableName}]') AT [{CentralLiveServer}];

            INSERT INTO  [{CentralLiveServer}].[{LoaderDBName}].[dbo].[{LiveTableName}]
            (
                [DateTime], [TagName], [Value], [vValue], [Quality], [QualityDetail],
                [OPCQuality], [wwTagKey], [wwRetrievalMode], [wwTimeDeadband],
                [wwValueDeadband], [wwTimeZone], [wwParameters], [SourceTag],
                [SourceServer], [wwValueSelector], [wwExpression], [wwUnit]
            )
            SELECT
                [DateTime], [TagName], [Value], [vValue], [Quality], [QualityDetail],
                [OPCQuality], [wwTagKey], [wwRetrievalMode], [wwTimeDeadband],
                [wwValueDeadband], [wwTimeZone], [wwParameters], [SourceTag],
                [SourceServer], [wwValueSelector], [wwExpression], [wwUnit]
            FROM [Runtime].[dbo].[Live];
                ";

                    using (var cmd = new SqlCommand(sql, (SqlConnection)conn)) // 確保 cast 成 SqlConnection
                    {
                        cmd.CommandTimeout = 60;
                        await cmd.ExecuteNonQueryAsync(stoppingToken);
                    }

                    await logToSQLService.LogToSQLAsync($"執行 {EXECUTE_NAME} 成功");
                    privateLog.InfoLog($"執行 {EXECUTE_NAME} 成功");
                }            
            }
            catch (Exception ex)
            {
                await logToSQLService.LogToSQLAsync($"外層錯誤：{EXECUTE_NAME} {ex}");
                privateLog.ErrorLog(ex, $"外層錯誤：{EXECUTE_NAME}");
            }
            finally
            {
                _isRunning = false;
            }
        }



        /// <summary>
        /// 取得各廠的[Transfer_Table]連線資訊，連線資訊寫在[INX_WorkCheckingDB2].[dbo].[SysSetting]裡。
        /// </summary>
        string GetRuntimeConnSettings()
        {
            using var conn = context.GetSqlConnection("SQL1");
            //利用 settingLoaderService.GetSysSettingsSql Code = facID + "LoaderSQLConn"  取得對應的 SQL語法(Value)
            string sql = $@"SELECT [ID], [Code], [Name], [Value] FROM [SysSetting] WHERE Code = @Code";
            var result = conn.Query<SysSetting>(sql, new { Code = facID + "RuntimeSQLConn" }).ToList();
            return result[0].Value;
        }
    }

    class LiveUploadWorkerLog {
        private readonly Serilog.ILogger _extraLogger;
        public LiveUploadWorkerLog()
        {
            _extraLogger = new LoggerConfiguration()
                .WriteTo.Console() // ✅小黑會顯示
                .WriteTo.File("Logs/Log_Live.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }


        public void InfoLog(string s) {
            _extraLogger.Information($"*** LiveUploadWorker - {s}");
        }
        public void WarningLog(string s)
        {
            _extraLogger.Warning($"*** LiveUploadWorker - {s}");
        }
        public void ErrorLog(Exception ex, string s)
        {
            _extraLogger.Error(ex, $"*** LiveUploadWorker - {s}");
        }
    }
}
