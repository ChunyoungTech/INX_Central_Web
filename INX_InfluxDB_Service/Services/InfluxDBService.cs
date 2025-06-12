using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using INX_InfluxDB.Models;
using Serilog;
using System.Collections.Generic;

namespace INX_InfluxDB.Services
{
    public class InfluxDBService(IConfiguration configuration, LogToSQLService logToSQLService)
    {
        private readonly string URL = configuration["InfluxDB:URL"] ?? string.Empty;
        //private readonly string Username = configuration["InfluxDB:Username"] ?? string.Empty;
        //private readonly string Password = configuration["InfluxDB:Password"] ?? string.Empty;
        private readonly string Token = configuration["InfluxDB:Token"] ?? string.Empty;
        private readonly string Org = configuration["InfluxDB:Org"] ?? string.Empty;
        private readonly string Bucket = configuration["InfluxDB:Bucket"] ?? string.Empty;
        private readonly int TimeZone = configuration.GetValue<int>("Historian.TimeZone");

        public async Task<DateTime> GetLastDateTime()
        {
            // 確保時間是秒為0
            var dateTime = DateTime.Now.AddMinutes(-1);
            dateTime = dateTime.Date.AddHours(dateTime.Hour).AddMinutes(dateTime.Minute);
            try
            {
                using var client = new InfluxDBClient(URL, Token);
                // TODO: 1h的檢查，可能無法補資料 (網路斷線可能超過1HR)
                // InfluxDB時間格式: 秒 (s), 分鐘(m), 小時(h), 天(d), 星期(w), 月(mo), 年(y)
                /*
                string query = @$"from(bucket: ""{Bucket}"")
                    |> range(start: -1h)
                    |> filter(fn: (r) => r._measurement == ""tag_value"")
                    |> last()";
                */
                string query = @$"from(bucket: ""{Bucket}"")
                    |> range(start: -1h)
                    |> last()";
                var tables = await client.GetQueryApi().QueryAsync(query, Org);
                // var tables = await client.GetQueryApi().QueryAsync<TagValue>(query, Org);
                var lastRecord = tables.SelectMany(table => table.Records).FirstOrDefault();
                if (lastRecord != null)
                {
                    var timeInDateTime = lastRecord.GetTimeInDateTime();
                    return dateTime;
                    return timeInDateTime.HasValue ? timeInDateTime.Value.AddHours(TimeZone) : dateTime;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to query TagValue");
                await logToSQLService.LogToSQLAsync($"Failed to query TagValue {ex}");
            }
            return dateTime;
        }

        public async void WriteMeasurement(IEnumerable<TagValue> tagValues)
        {
            try
            {
                using var client = new InfluxDBClient(URL, Token);
                using var writeApi = client.GetWriteApi();

                var points = tagValues.Select(tagValue =>
                           PointData
                               .Measurement("example")  
                               .Tag("tag_name", tagValue.TagName)
                               .Tag("quality", tagValue.Quality.ToString()) // Tag 一律是 string
                               .Field("value", tagValue.Value)
                               .Timestamp(tagValue.DateTime, WritePrecision.S) // 建議轉 UTC
                       ).ToList();

                // WritePrecision: 指定數據點時間戳的精確度
                // 預設是WritePrecision.Ns，但每分鐘記錄一次，不需要
                //writeApi.WriteMeasurements(tagValues.ToList(), WritePrecision.S, Bucket, Org);
                writeApi.WritePoints(points, Bucket, Org);
                Log.Information("資料寫入InfluxDB完成");
                await logToSQLService.LogToSQLAsync("資料寫入InfluxDB完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write TagValues");
                await logToSQLService.LogToSQLAsync($"Failed to write TagValues {ex}");
            }
        }

        /// <summary>
        /// 寫入至特定Msrmt
        /// </summary>
        /// <param name="tagValues"></param>
        /// <param name="measurement"></param>
        public async void WriteMeasurement(IEnumerable<TagValue> tagValues, Dictionary<string,string> _tagDatas)
        {
            try
            {
                using var client = new InfluxDBClient(URL, Token);
                using var writeApi = client.GetWriteApi();

                var points = tagValues
                    .Where(tagValue =>
                    {
                        bool exists = _tagDatas.ContainsKey(tagValue.TagName);
                        if (!exists)
                        {
                            Log.Warning($"TagName '{tagValue.TagName}' not found in _tagDatas");
                        }
                        return exists;
                    })
                    .Select(tagValue =>
                           PointData
                               .Measurement(_tagDatas[tagValue.TagName])
                               .Tag("tag_name", tagValue.TagName)
                               .Tag("quality", tagValue.Quality.ToString()) // Tag 一律是 string
                               .Field("value", tagValue.Value)
                               .Timestamp(tagValue.DateTime, WritePrecision.S) // 建議轉 UTC
                       ).ToList();

                // WritePrecision: 指定數據點時間戳的精確度
                // 預設是WritePrecision.Ns，但每分鐘記錄一次，不需要
                //writeApi.WriteMeasurements(tagValues.ToList(), WritePrecision.S, Bucket, Org);
                writeApi.WritePoints(points, Bucket, Org);
                Log.Information($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 資料寫入InfluxDB完成");
                await logToSQLService.LogToSQLAsync($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 資料寫入InfluxDB完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write TagValues");
                await logToSQLService.LogToSQLAsync($"Failed to write TagValues {ex}");
            }
        }


    }
}
