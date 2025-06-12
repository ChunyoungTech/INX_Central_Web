using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace InxDataAPI.Core.Contexts
{
    public class InfluxDBContext
    {
        private readonly string URL;
        private readonly string Token;
        private readonly string Bucket;
        private readonly string Org;

        private readonly object _clientLock = new();
        private const int CheckMinutes = 10;
        private readonly Dictionary<int, Lazy<InfluxDBClient>> ClientDict = [];
        private readonly Dictionary<int, DateTime> LastCreateTimeDict = [];

        public InfluxDBContext(IConfiguration configuration)
        {
            URL = configuration["InfluxDB:URL"] ?? string.Empty;
            Token = configuration["InfluxDB:Token"] ?? string.Empty;
            Bucket = configuration["InfluxDB:Bucket"] ?? string.Empty;
            Org = configuration["InfluxDB:Org"] ?? string.Empty;
            WarmUpInfluxDB();
        }

        private void WarmUpInfluxDB(int seconds = default)
        {
            // 先建立連線，避免第一次查詢時耗時過長
            string dummyQuery = @$"from(bucket: ""{Bucket}"")
                |> range(start: -1m)
                |> limit(n: 1)";
            try
            {
                GetInfluxDBClient(seconds).GetQueryApiSync().QuerySync(dummyQuery, Org);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Token為長期存活的API Token，直到手動撤銷才會失效 (或是設定期限);
        /// <para>
        /// 不建議使用: Username/Password登入 實際上是一種相容模式 (用於與InfluxDB v1兼容的方式)
        /// 內部會轉成短期的 session 或 JWT token，具有有限的生命週期 (參考influxdb的config的session-length，預設60分鐘)
        /// </para>
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private InfluxDBClient CreateClient(int seconds = default)
        {
            // timeout預設10秒 (偶爾會有超時的BUG，改用retry解決這個問題)
            // 若有查詢大量資料的需求，或query時間過長，可考慮增加timeout時間 (30秒、60秒、120秒...)
            // https://github.com/influxdata/influxdb-client-csharp/tree/master/Client#client-connection-string
            return new(seconds == default ? URL : $"{URL}?timeout={TimeSpan.FromSeconds(seconds).TotalMilliseconds}", Token);
        }

        /// <summary>
        /// 取得 InfluxDBClient
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private InfluxDBClient GetInfluxDBClient(int seconds = default)
        {
            lock (_clientLock)
            {
                if (LastCreateTimeDict.ContainsKey(seconds) == false)
                {
                    ClientDict[seconds] = new Lazy<InfluxDBClient>(() => CreateClient(seconds));
                    LastCreateTimeDict[seconds] = DateTime.Now;
                }
                else
                {
                    // 每10分鐘檢查1次
                    if ((DateTime.Now - LastCreateTimeDict[seconds]).TotalMinutes > CheckMinutes)
                    {
                        string dummyQuery = @$"from(bucket: ""{Bucket}"")
                            |> range(start: -1m)
                            |> limit(n: 1)";
                        bool healthFail = false;
                        try
                        {
                            ClientDict[seconds].Value.GetQueryApiSync().QuerySync(dummyQuery, Org);
                        }
                        catch (Exception ex)
                        {
                            healthFail = true;
                        }
                        if (healthFail)
                        {
                            // 若有健康檢查失敗，則 Dispose 舊的 Client 並重新建立
                            if (ClientDict.TryGetValue(seconds, out var lazyClient) && lazyClient.IsValueCreated)
                                lazyClient.Value.Dispose();
                            ClientDict[seconds] = new Lazy<InfluxDBClient>(() => CreateClient(seconds));
                            WarmUpInfluxDB();
                        }
                        LastCreateTimeDict[seconds] = DateTime.Now;
                    }
                }
                return ClientDict[seconds].Value;
            }
        }

        public QueryApi GetQueryApi(int seconds = default) => GetInfluxDBClient(seconds).GetQueryApi();

        public WriteApiAsync GetWriteApi(int seconds = default) => GetInfluxDBClient(seconds).GetWriteApiAsync();

        public DeleteApi GetDeleteApi(int seconds = default) => GetInfluxDBClient(seconds).GetDeleteApi();

        public void Dispose()
        {
            lock (_clientLock)
            {
                foreach (var client in ClientDict.Values)
                {
                    if (client.IsValueCreated)
                        client.Value.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
