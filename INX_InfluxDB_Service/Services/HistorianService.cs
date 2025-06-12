using Dapper;
using INX_InfluxDB.Contexts;
using INX_InfluxDB.Models;
using INX_InfluxDB_Service.Services;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Collections.Concurrent;

namespace INX_InfluxDB.Services
{
    public class HistorianService(IConfiguration configuration, DapperContext context, SettingLoaderService settingLoaderService)
    {
        private readonly string sBatchSize = SettingLoaderService.StatusDescriptions[SettingType.BatchSize];
        private readonly string sHistRes = SettingLoaderService.StatusDescriptions[SettingType.HistRes];
        //直接讀 APPSettings 資料
        private readonly int BatchSize = configuration.GetValue<int>("Batch.Size");
        private readonly int Resolution = configuration.GetValue<int>("Historian.Resolution");
        private readonly string SQL = configuration.GetConnectionString("SQL");
        /// <summary>
        /// 轉換成分鐘
        /// </summary>
        private readonly int TimeZone = configuration.GetValue<int>("Historian.TimeZone") * -1 * 60;
        private const string HistoryTable = "History";

        public IEnumerable<TagValue> GetTagValues(IEnumerable<string> tagNames, DateTime dateTime)
        {
            // dateTime是最後一筆
            var startDateTime = dateTime.AddSeconds(5);
            var endDateTime = DateTime.Now.AddSeconds(-30);
            endDateTime = endDateTime.Date.AddHours(endDateTime.Hour).AddMinutes(endDateTime.Minute).AddSeconds(endDateTime.Second - endDateTime.Second % 5);
            try
            {

                 var conn = context.GetSqlConnection();
                // 要抓 QualityDetail,然後要跟科勝要對應QualityString的意思。
                // wwCycleCount要能開放設定。這是要抓的時間內要分割為幾筆，預設是99，這要能外部控制是否加入這個條件。如要1秒1個是設61
                // History.TagName能開放用like
                string sql = $@"SELECT History.TagName, DateTime = DateAdd(mi,@TimeZone,DateTime), Value, Quality
                    FROM {HistoryTable}
                    WHERE History.TagName IN @tagNames
                    AND wwRetrievalMode = 'Cyclic'
                    AND wwResolution = @Resolution
                    AND wwQualityRule = 'Extended'
                    AND wwVersion = 'Latest'
                    AND DateTime >= @startDateTime
                    AND DateTime <= @endDateTime";
                List<TagValue> list = [];
                // 計算總批次數：使用 Math.Ceiling 確保不遺漏最後一批不足 BatchSize 的資料。"index <= batchCount" 可能多跑一次空查詢
                int batchCount = (int)Math.Ceiling(tagNames.Count() / (double)BatchSize);
                for (int index = 0; index < batchCount; index++)
                {
                    var batch = tagNames.Skip(index * BatchSize).Take(BatchSize);
                    if (!batch.Any()) continue; // 保險起見也可加這行
                    var result = conn.Query<TagValue>(sql, new { Resolution, TimeZone, tagNames = batch, startDateTime, endDateTime });
                    list.AddRange(result);
                }
                
                return list;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get TagValues");
                return [];
            }
        }
        public IEnumerable<TagValue> GetTagValues_2(IEnumerable<string> tagNames, DateTime dateTime)
        {
            // dateTime是最後一筆
            var startDateTime = dateTime.AddSeconds(5);
            var endDateTime = DateTime.Now.AddSeconds(-30);
            endDateTime = endDateTime.Date.AddHours(endDateTime.Hour).AddMinutes(endDateTime.Minute).AddSeconds(endDateTime.Second - endDateTime.Second % 5);
            try
            {
                var builder = new SqlConnectionStringBuilder(SQL);
                string databaseName = builder.InitialCatalog;

                using var conn = context.GetSqlConnection();

                // Step 1: 先取得聚合的 TagName 字串
            //    string getWhereInSql = $@"
            //SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), '''' + [TagName] + ''''), ',　')
            //FROM [{databaseName}].[dbo].[TagData]";
            //    var whereInClause = conn.ExecuteScalar<string>(getWhereInSql);
                var timeZone = TimeZone;

                // Step 2: 組合動態 SQL 查詢
                string dynamicSql = $@"
            SELECT 
                [History].[TagName],
                DateAdd(mi, {timeZone}, [History].[DateTime]) AS [DateTime],
                [History].[Value],
                [History].[Quality]
            FROM [Runtime].[dbo].[History] AS [History]
            WHERE [History].[TagName] IN ({string.Join(",", tagNames.Select(tn => $"'{tn}'"))})
              AND [History].[wwRetrievalMode] = 'Cyclic'
              AND [History].[wwResolution] = 1000
              AND [History].[wwQualityRule] = 'Extended'
              AND [History].[wwVersion] = 'Latest'
              AND [History].[DateTime] >= '{startDateTime:yyyy-MM-dd HH:mm:ss}'
              AND [History].[DateTime] <= '{endDateTime:yyyy-MM-dd HH:mm:ss}'";

                // Step 3: 執行 SQL 查詢
                var result = conn.Query<TagValue>(dynamicSql).ToList();
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get TagValues");
                return [];
            }
        }

        /// <summary>
        /// 測試中，暫時無法使用
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// 
        public async Task<IEnumerable<TagValue>> GetTagValuesAsync(IEnumerable<string> tagNames, DateTime dateTime)
        {
            //var startDateTime = dateTime.AddSeconds(5);
            //var endDateTime = DateTime.Now.AddSeconds(-30);
            //endDateTime = endDateTime.Date
            //    .AddHours(endDateTime.Hour)
            //    .AddMinutes(endDateTime.Minute)
            //    .AddSeconds(endDateTime.Second - endDateTime.Second % 5);

            // ✅ 取得 5 分鐘前的整分鐘作為 start
            var startDateTime = DateTime.Now.AddMinutes(-5);
            startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day,
                                          startDateTime.Hour, startDateTime.Minute, 0);
            var endDateTime = startDateTime.AddMinutes(1);
            Log.Information($"開始讀取Historian: {startDateTime} ~ {endDateTime} 資料");

            var allResults = new ConcurrentBag<TagValue>(); // thread-safe 收集結果
            int batchCount = (int)Math.Ceiling(tagNames.Count() / (double)BatchSize);

            int maxConcurrency = 15;
            using var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = tagNames
            .Select((tag, i) => new { tag, index = i / BatchSize })
            .GroupBy(x => x.index)
            .Select(g => g.Select(x => x.tag).ToList())
            .Select(async batch =>
            {
                await semaphore.WaitAsync();
                try
                {
                    using var conn = context.GetSqlConnection();
                    var sql = $@"SELECT History.TagName, DateTime = DateAdd(mi,@TimeZone,DateTime), Value, Quality
                        FROM {HistoryTable}
                        WHERE History.TagName IN @tagNames
                        AND wwRetrievalMode = 'Cyclic'
                        AND wwResolution = @Resolution
                        AND wwQualityRule = 'Extended'
                        AND wwVersion = 'Latest'
                        AND DateTime >= @startDateTime
                        AND DateTime <= @endDateTime";
                    //顯示正在讀取的 tagNames 區間，觀察多線程有沒有漏掉
                    //Log.Information($"正在讀取 {batch.FirstOrDefault()} - {batch.LastOrDefault()}  之間資料");
                    
                    var result = await conn.QueryAsync<TagValue>(sql, new
                    {
                        Resolution,
                        TimeZone,
                        tagNames = batch,
                        startDateTime,
                        endDateTime
                    });

                    foreach (var r in result)
                        allResults.Add(r);
                }
                finally
                {
                    semaphore.Release();
                }
            })
            .ToList();
            await Task.WhenAll(tasks);
            return allResults;
        }



    }
}
