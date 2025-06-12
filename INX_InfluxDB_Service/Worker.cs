using INX_InfluxDB.Models;
using INX_InfluxDB.Services;
using INX_InfluxDB_Service.Services;
using Serilog;
using System.Diagnostics;

namespace INX_InfluxDB
{
    public class Worker(HistorianService historianService, InfluxDBService influxDBService, SettingLoaderService settingLoaderService, LogToSQLService logToSQLService) : BackgroundService
    {
        private const string EXECUTE_NAME = "取得Historian資料至InfluxDB";
        private Dictionary<string, string> _tagDatas = new Dictionary<string, string>();
    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        settingLoaderService.GetTagDatas();
    //        settingLoaderService.GetSysSettings();
    //        _tagDatas = settingLoaderService.TagDatas
    //.GroupBy(tag => tag.TagName)
    //.ToDictionary(g => g.Key, g => g.First().System);
    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            //顯示目前版本
    //            var version = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
    //            Log.Information($"Ver.{version} 開始 {EXECUTE_NAME}...");
    //            await logToSQLService.LogToSQLAsync($"Ver.{version} 開始 {EXECUTE_NAME}...");

                
    //            var dateTime = await influxDBService.GetLastDateTime();
    //            Log.Information($"取得InfluxDB最後時間: {dateTime}");
    //            await logToSQLService.LogToSQLAsync($"取得InfluxDB最後時間: {dateTime}");
    //            // TODO: 測試用
    //            //var tagNames = Enumerable.Range(1, 10000).Select(data => $"A{data}");
    //            //var tagNames = new List<string>() { 
    //            //    "F86F10F_EXHA_PT410_I6_F56", "F86F10F_EXHV_PT610_I8_F53", "F86F10F_HVAP_PT_I12_I45_PLC", 
    //            //    "F86F30A_EXHA_PT430_K3_F14", "F86F30D_HVAP_PT_I1_F10_PLC", "F86F30E_EXHA_PT430_I4_F24", 
    //            //    "F86F50C_EXHC_PT750_L5_F43", "F86F50C_EXHC_PT750_L5_F44", "F86F50E_EXHC_PT750A_I3_F24",
    //            //    "F86F50E_EXHC_PT750B_I3_F24"
    //            //};
                
    //            var tagNames = settingLoaderService.TagDatas.Select(tag => tag.TagName);
    //            Log.Information($"已取得  {tagNames.Count()} 筆 tagNames");
    //            //await logToSQLService.LogToSQLAsync($"已取得  {tagNames.Count()} 筆 tagNames");

    //            //讀取 SQL內 InfluxDB資料庫 內的 TagData資料表 TagName欄位 的所有值，量太大，可嘗試用異步取得
    //            Log.Information($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 開始讀取Historian資料");
    //            await logToSQLService.LogToSQLAsync($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 開始讀取Historian資料");

    //            //讀取 TagValues 數值
    //            //var tagValues = historianService.GetTagValues(tagNames, dateTime);
    //                //修改SQL語法，不用batch，以加快讀取速度
    //            //var tagValues = historianService.GetTagValues_2(tagNames, dateTime);
    //                //異步方法，待測試
    //            var tagValues = await historianService.GetTagValuesAsync(tagNames, dateTime);

    //            Log.Information($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 取得Historian資料，筆數: {tagValues.Count()}");
    //            await logToSQLService.LogToSQLAsync($"------------------ {DateTime.Now.ToString("HH:mm:ss.fff")} 取得Historian資料，筆數: {tagValues.Count()}");

    //            //寫入InfluxDB，未分類Measurement
    //            //influxDBService.WriteMeasurement(tagValues);
    //            //寫入InfluxDB，有用System欄位分類Measurement
    //            influxDBService.WriteMeasurement(tagValues, _tagDatas);

    //            Log.Information($"{EXECUTE_NAME} 完成...");
    //            await logToSQLService.LogToSQLAsync($"{EXECUTE_NAME} 完成...");
    //            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    //        }
    //    }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            settingLoaderService.GetTagDatas();
            settingLoaderService.GetSysSettings();
            _tagDatas = settingLoaderService.TagDatas
                .GroupBy(tag => tag.TagName)
                .ToDictionary(g => g.Key, g => g.First().System);

            while (!stoppingToken.IsCancellationRequested)
            {
                var loopStart = DateTime.Now;

                try
                {
                    var version = FileVersionInfo
                        .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
                        .FileVersion;

                    Log.Information($"Ver.{version} 開始 {EXECUTE_NAME}...");
                    await logToSQLService.LogToSQLAsync($"Ver.{version} 開始 {EXECUTE_NAME}...");

                    var dateTime = await influxDBService.GetLastDateTime();
                    Log.Information($"取得InfluxDB最後時間: {dateTime}");
                    await logToSQLService.LogToSQLAsync($"取得InfluxDB最後時間: {dateTime}");

                    var tagNames = settingLoaderService.TagDatas.Select(tag => tag.TagName);
                    Log.Information($"已取得 {tagNames.Count()} 筆 tagNames");

                    Log.Information($"------------------ {DateTime.Now:HH:mm:ss.fff} 開始讀取Historian資料");
                    await logToSQLService.LogToSQLAsync($"------------------ {DateTime.Now:HH:mm:ss.fff} 開始讀取Historian資料");

                    var tagValues = await historianService.GetTagValuesAsync(tagNames, dateTime);

                    Log.Information($"------------------ {DateTime.Now:HH:mm:ss.fff} 取得Historian資料，筆數: {tagValues.Count()}");
                    await logToSQLService.LogToSQLAsync($"------------------ {DateTime.Now:HH:mm:ss.fff} 取得Historian資料，筆數: {tagValues.Count()}");

                    influxDBService.WriteMeasurement(tagValues, _tagDatas);

                    Log.Information($"{EXECUTE_NAME} 完成...");
                    await logToSQLService.LogToSQLAsync($"{EXECUTE_NAME} 完成...");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{EXECUTE_NAME} 發生錯誤！");
                    await logToSQLService.LogToSQLAsync($"{EXECUTE_NAME} 發生錯誤：{ex.Message}");
                }

                // ✅ 控制每次執行週期為一分鐘
                var elapsed = DateTime.Now - loopStart;
                var delay = TimeSpan.FromMinutes(1) - elapsed;

                if (delay > TimeSpan.Zero)
                {
                    Log.Information($"等待 {delay.TotalSeconds:F1} 秒進入下一輪...");
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }
    }
}
