using Dapper;
using InfluxDB.Client.Api.Domain;
using INX_InfluxDB.Contexts;
using INX_InfluxDB.Models;
using INX_InfluxDB.Services;
using INX_InfluxDB_Service.Services;
using Microsoft.Data.SqlClient;
using Serilog;
using System;
using System.Diagnostics;

namespace INX_InfluxDB
{
    /// <summary>
    /// 上傳 Transfer_Table 資料至 FAC_Loader 的背景服務
    /// </summary>
    public class LoaderUploadWorker : BackgroundService
    {
        private DapperContext context;
        private IConfiguration configuration;
        SettingLoaderService settingLoaderService;
        LogToSQLService logToSQLService;
        private readonly string facID;
        private readonly string LoaderDBName;
        List<TagData> tagDatas = new List<TagData>() { };

        LoaderUploadWorkerLog privateLog = new LoaderUploadWorkerLog();
        string GetTransferTableDataTags = "";
        public LoaderUploadWorker(IConfiguration configuration, DapperContext context, SettingLoaderService settingLoaderService, LogToSQLService logToSQLService)
        {
            try {
                this.configuration = configuration;
                this.settingLoaderService = settingLoaderService;
                this.logToSQLService = logToSQLService;
                this.context = context;
                facID = configuration.GetValue<string>("FacID") ?? throw new InvalidOperationException("SQL connection string 未設定或為空值。");
                LoaderDBName = configuration.GetValue<string>("LoaderDBName");
                if (string.IsNullOrEmpty(LoaderDBName))
                {
                    LoaderDBName = "FAC_Loader_TEST";
                    privateLog.WarningLog($"SQL connection string 未設定或為空值。已套用預設值 {LoaderDBName}。");
                }
            }
            catch (Exception ex)
            {
                privateLog.ErrorLog(ex, "LoaderUploadWorker 建構函式發生錯誤");
                throw;
            }
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 執行資料上傳邏輯
                    await DoTask(stoppingToken);

                    // 等待 30 分鐘
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    // 記錄錯誤日誌
                    privateLog.ErrorLog(ex, $"執行 {EXECUTE_NAME} 時發生錯誤");
                }
            }
        }

        private const string EXECUTE_NAME = "取得 Transfer_Table 資料至 中央 Loader DB";
        private bool _isRunning = false;

        protected async Task DoTask(CancellationToken stoppingToken)
        {
            if (_isRunning) return; // 避免重複執行
            _isRunning = true;

            try
            {
                privateLog.InfoLog($"開始 {EXECUTE_NAME}...");
                await logToSQLService.LogToSQLAsync($"開始 {EXECUTE_NAME}...");
                GetTagDatas();
                var tagNames = tagDatas.Select(tag => tag.TagName);
                DateTime dateTime = DateTime.Now;
                dateTime = GetLastDateTime().AddSeconds(1);
                privateLog.InfoLog($"取得 Transfer_Table_Temp 最後時間: {dateTime}");
                //dateTime = new DateTime(2025, 05, 12, 0, 0, 0); // 測試用
                //如果 dateTime 小於3天前  從3天前0點開始
                if (dateTime < DateTime.Now.AddDays(-3).Date)
                {
                    dateTime = DateTime.Now.AddDays(-3).Date;
                    privateLog.InfoLog($"最後時間小於3天前，將使用 {dateTime} 作為起始時間。");
                }
                var tagValues = GetTransferTableData(tagNames, dateTime);
                WriteTransferTableData(tagValues);
            }
            catch (Exception ex)
            {
                privateLog.ErrorLog(ex, $"執行 {EXECUTE_NAME} 時發生錯誤");
                await logToSQLService.LogToSQLAsync($"執行 {EXECUTE_NAME} 時發生錯誤: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// 取得 Transfer_Table_Temp 最後一筆資料的時間
        /// </summary>
        /// <returns></returns>
        DateTime GetLastDateTime() {
            DateTime lastDateTime = DateTime.Now.AddMinutes(-30);
            using var conn = context.GetSqlConnection("SQL1");
            conn.Open();
            string query = $@"SELECT TOP 1 [tf_data_gen_time]
                 FROM [{LoaderDBName}].[dbo].[Transfer_Table_Temp]
                 WHERE [fac] = @FAC
                 ORDER BY [tf_data_gen_time] DESC;";
            try
            {
                // 查詢最後一筆資料的時間
                var result = conn.QuerySingleOrDefault<DateTime?>(query, new { FAC = facID });
                if (result.HasValue)
                {
                    lastDateTime = result.Value;
                }
                else
                {
                    privateLog.WarningLog($"未找到任何符合條件的資料，將使用 {lastDateTime} 作為最後時間。");
                }
            }
            catch (Exception ex)
            {
                privateLog.ErrorLog(ex, $"取得最後時間時發生錯誤，將使用 {lastDateTime} 作為最後時間。");
            }


            return lastDateTime; 
        }

        /// <summary>
        /// 取得要上傳的點位 TagName
        /// 條件為：在 IDBHistorianUpload 中有設定為上傳 (Enable = '1')，
        /// 且 TagData 的 TagSource 為 'LoaderDB'。
        /// </summary>
        void GetTagDatas()
        {
            using var conn = context.GetSqlConnection("SQL1");

            string sql = $@"SELECT 
    t.ID,
    t.Tag_Name,
	i.ind_tagname AS TagName,
    h.Enable,
    t.TagSys,
    t.TagSource,
    m.mesurement AS System,
    f.FacName
FROM [dbo].[TagData] t
LEFT JOIN [IDBHistorianUpload] h
    ON t.ID = h.TagID
LEFT JOIN [IDBSysMapping] m
    ON t.TagSys = CAST(m.SeqID AS VARCHAR)
LEFT JOIN [IDBFacData] f
    ON m.IDBFacDataID = f.SeqID
LEFT JOIN IFMTransferIndex i
	ON t.Tag_Name = i.scada_tagname
WHERE FacName = @FacName AND Enable = '1'　AND TagSource = 'LoaderDB'";

            // *** 使用原有的 TagData 類別進行映射 ***
            // 因為 SQL SELECT 的欄位現在對應 TagData 的屬性
            var result = conn.Query<TagData>(sql, new {FacName = facID  }).ToList(); // <--- 保持使用 TagData
            tagDatas = result;
        }

        /// <summary>
        /// 取得各廠的[Transfer_Table]連線資訊，連線資訊寫在[INX_WorkCheckingDB2].[dbo].[SysSetting]裡。
        /// </summary>
        string GetLoaderSQLConnSettings()
        {
            using var conn = context.GetSqlConnection("SQL1");
            //利用 settingLoaderService.GetSysSettingsSql Code = facID + "LoaderSQLConn"  取得對應的 SQL語法(Value)
            string sql = $@"SELECT [ID], [Code], [Name], [Value] FROM [SysSetting] WHERE Code = @Code";
            var result = conn.Query<SysSetting>(sql, new { Code = facID + "LoaderSQLConn" }).ToList();
            return result[0].Value;
        }

        /// <summary>
        /// 取得 Transfer_Table 資料
        /// </summary>
        private IEnumerable<TransferTableData> GetTransferTableData(IEnumerable<string> tagNames, DateTime dateTime) {

            string connStr = GetLoaderSQLConnSettings();
            var sqlConn = new SqlConnection(connStr);
            
            string sql = @"SELECT [tf_data_source]
      ,[tf_data_gen_time]
      ,[tf_tagname]
      ,[tf_value]
      ,[tf_ack_flag]
      ,[tf_sn]
      ,[created]
  FROM [Transfer_Table]
  WHERE [tf_tagname] IN @tagNames
  AND [tf_data_gen_time] >= @startDateTime
  AND [tf_data_gen_time] <= @endDateTime";

            //dateTime是最後一筆 
            var startDateTime = dateTime.ToString("yyyy/MM/dd HH:mm:ss");
            var endDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //endDateTime = endDateTime.Date.AddHours(endDateTime.Hour).AddMinutes(endDateTime.Minute).AddSeconds(endDateTime.Second - endDateTime.Second % 5);

            List<TransferTableData> list = [];
            // 計算總批次數：使用 Math.Ceiling 確保不遺漏最後一批不足 BatchSize 的資料。"index <= batchCount" 可能多跑一次空查詢
            int batchCount = (int)Math.Ceiling(tagNames.Count() / (double)100);
            GetTransferTableDataTags = "";
            for (int index = 0; index < batchCount; index++)
            {
                var batch = tagNames.Skip(index * 100).Take(100);
                if (!batch.Any()) continue; // 保險起見也可加這行
                var result = sqlConn.Query<TransferTableData>(sql, new
                {
                    startDateTime,
                    endDateTime,
                    tagNames = batch
                });
                RecNoValueTag(result);

                list.AddRange(result);
            }

            privateLog.InfoLog($"{GetTransferTableDataTags}");
            privateLog.InfoLog($"找到需要上傳的 {list.Count} 筆資料");
            return list;
        }

        /// <summary>
        /// 把 transferTableData 資料存進 192.168.66.3 的 FAC_Loader 資料庫
        /// </summary>
        /// <param name="transferTableData"></param>
        private void WriteTransferTableData(IEnumerable<TransferTableData> transferTableData)
        {
            if (transferTableData == null || !transferTableData.Any())
                return; // 如果沒有資料，直接返回

            DateTime fakedateTime = DateTime.Now;
            foreach (var data in transferTableData)
            {
                try
                {
                    if (configuration.GetValue<string>("Version") == "0.0.0")
                    {
                        fakedateTime = fakedateTime.AddSeconds(-1);
                        data.tf_data_gen_time = fakedateTime.ToString("yyyy/MM/dd HH:mm:ss"); // 測試用
                        privateLog.InfoLog($"測試資料 {data.tf_data_gen_time}");
                    }
                    data.fac = facID;
                }
                catch (Exception ex)
                {
                    privateLog.ErrorLog(ex, "處理 transferTableData 時發生錯誤，資料: {@data}", data);
                }
            }

            try
            {
                using var conn = context.GetSqlConnection("SQL1");
                conn.Open();

                // SQL Insert 語法
                string insertQuery = $@"
        INSERT INTO [{LoaderDBName}].[dbo].[Transfer_Table_Temp](
        [tf_data_source]
        ,[tf_data_gen_time]
        ,[tf_tagname]
        ,[tf_value]
        ,[tf_ack_flag]
        ,[tf_sn]
        ,[created]
        ,[fac]
        )VALUES(
        @tf_data_source,
        @tf_data_gen_time, 
        @tf_tagname, 
        @tf_value, 
        @tf_ack_flag, 
        @tf_sn, 
        @created, 
        @fac)";

                // 批次處理：將資料分成小批次以減少單次插入的負擔
                const int batchSize = 100; // 每次插入資料筆數
                var batches = transferTableData
                    .Select((data, index) => new { data, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.data));

                foreach (var batch in batches)
                {
                    try
                    {
                        // 批次插入資料
                        conn.Execute(insertQuery, batch);
                    }
                    catch (Exception ex)
                    {
                        privateLog.ErrorLog(ex, "批次插入資料時發生錯誤");
                    }
                }
            }
            catch (Exception ex)
            {
                privateLog.ErrorLog(ex, "寫入 TransferTableData 時發生錯誤");
            }
            finally {
                privateLog.InfoLog($"資料寫入[{LoaderDBName}].[dbo].[Transfer_Table_Temp]結束");
            }
        }


        void RecNoValueTag(IEnumerable<TransferTableData> result) {

            foreach (var data in result)
            {
                //data.tf_tagname 加進tagList 中 
                //Trim一下data.tf_tagname
                GetTransferTableDataTags += $"{data.tf_tagname.Trim()},";
            }
            privateLog.InfoLog($"{GetTransferTableDataTags}");
        }
    }

    class LoaderUploadWorkerLog {
        private readonly Serilog.ILogger _extraLogger;
        public LoaderUploadWorkerLog()
        {
            _extraLogger = new LoggerConfiguration()
                .WriteTo.Console() // ✅小黑會顯示
                .WriteTo.File("Logs/Log_Extra.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }


        public void InfoLog(string s) {
            _extraLogger.Information($"--- LoaderUploadWorker - {s}");
        }
        public void WarningLog(string s)
        {
            _extraLogger.Warning($"--- LoaderUploadWorker - {s}");
        }
        public void ErrorLog(Exception ex, string s, TransferTableData t = null)
        {
            _extraLogger.Error(ex, $"--- LoaderUploadWorker - {s}",t);
        }
    }
}
