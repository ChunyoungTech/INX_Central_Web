using InxDataAPI.Core.Contexts;
using InxDataAPI.Models;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;

namespace InxDataAPI.Services;

public class InxFluxDBService
{
    private readonly InfluxDBContext _context;
    private readonly IConfiguration _config;
    private readonly string _org;

    public InxFluxDBService(InfluxDBContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        _org = config["InfluxDB:Org"]!;
    }

    public async Task<List<DataResponse>> GetTagValuesAsync(string bucket, IEnumerable<string> tagNames, DateTime timePoint)
    {
        var queryApi = _context.GetQueryApi();
        var results = new List<DataResponse>();
        var timeUtc = timePoint.ToUniversalTime();

        foreach (var tag in tagNames)
        {
            var flux = $@"
from(bucket: ""{bucket}"")
  |> range(start: time(v: ""{timeUtc:O}""), stop: time(v: ""{timeUtc.AddSeconds(1):O}""))
  |> filter(fn: (r) => r[""tag_name""] == ""{tag}"")
  |> filter(fn: (r) => r._field == ""value"" or r._field == ""quality"")
  |> group(columns: [""_field"", ""tag_name""])
  |> last()";

            // 寫入 Flux 語法 Log
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logDir);
            var fluxLogPath = Path.Combine(logDir, "flux_log.txt");
            await System.IO.File.AppendAllTextAsync(fluxLogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Query:\n{flux}\n");

            try
            {
                var tables = await queryApi.QueryAsync(flux, _org);
                var records = tables.SelectMany(t => t.Records).ToList();

                var valueRecord = records.FirstOrDefault(r => r.GetField() == "value");
                var qualityRecord = records.FirstOrDefault(r => r.GetField() == "quality");

                var result = new DataResponse
                {
                    TagName = tag,
                    Time = timePoint,
                    Value = valueRecord?.GetValue() ?? "",
                    Quality = qualityRecord?.GetValue() ?? ""
                };

                results.Add(result);

                // ✅ 寫入查詢結果 Log
                var resultLogPath = Path.Combine(logDir, "flux_result.txt");
                var resultLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Tag: {tag}, Time: {timePoint:O}, Value: {result.Value}, Quality: {result.Quality}";
                await System.IO.File.AppendAllTextAsync(resultLogPath, resultLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                results.Add(new DataResponse
                {
                    TagName = tag,
                    Time = timePoint,
                    Value = "",
                    Quality = ""
                });

                var errorLogPath = Path.Combine(logDir, "flux_result.txt");
                var errorMsg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR - Tag: {tag}, Message: {ex.Message}\n";
                await System.IO.File.AppendAllTextAsync(errorLogPath, errorMsg);
            }
        }

        return results;
    }
}
