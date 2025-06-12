using InxDataAPI.Models;
using Microsoft.AspNetCore.Mvc;
using InfluxDB.Client;
using InxDataAPI.Services;

[ApiController]
[Route("[controller]")]
public class InxDataAPIController : ControllerBase
{
    private readonly DataApiService _service;
    private readonly InxFluxDBService _influxService;

    public InxDataAPIController(DataApiService service, InxFluxDBService influxService)
    {
        _service = service;
        _influxService = influxService;
    }

    [HttpGet("Hello")]
    public IActionResult GetHello()
    {
        return Ok(new { Message = "Hello, this is a simple API!" });
    }

    [HttpGet("GetRandomValue")]
    public IActionResult GetRandomValue()
    {
        var randomValue = new Random().NextDouble() * 100;
        return Ok(new { Value = randomValue });
    }

    [HttpPost("Echo")]
    public IActionResult Echo([FromBody] DataRequest request)
    {
        return Ok(new { YouSent = request.Token });
    }

    [HttpPost("FAC8Data")]
    public async Task<IActionResult> FAC8Data([FromBody] DataRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"來自用戶 IP：{clientIp}");
        Console.WriteLine($"Token: {request.Token}");
        Console.WriteLine($"Client IP: {clientIp}");

        // ✅ 寫入 IP 紀錄 Log
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir); // 確保 Logs 資料夾存在
        var logFilePath = Path.Combine(logDir, "ip_log.txt");
        var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {clientIp}";
        await System.IO.File.AppendAllTextAsync(logFilePath, logLine + Environment.NewLine);

        if (request.TagNames == null || request.TagNames.Count == 0 || request.TagNames.Count > 50)
            return BadRequest("TagNames 必須 1~50 筆");

        var apiName = RouteData.Values["action"]?.ToString();
        var tokenRecord = _service.GetTokenByValue(apiName, request.Token, clientIp);

        if (tokenRecord == null)
            return Unauthorized("Token 驗證失敗或 IP 不允許");

        // 若改回用加密 Token 驗證請用以下方式：
        // string decrypted;
        // try { decrypted = AesHelper.Decrypt(request.Token); }
        // catch { return Unauthorized("Token 無效"); }
        // var parts = decrypted.Split('|');
        // if (parts.Length != 2) return Unauthorized("Token 格式錯誤");
        // var tokenRecord = _service.GetTokenRecord("FAC8DataAPI", parts[0], parts[1], clientIp);

        var timePoint = request.TimePoint;
        var result = await _influxService.GetTagValuesAsync(request.Factory, request.TagNames, request.TimePoint);

        Console.WriteLine($"呼叫 Influx: Bucket={request.Factory}, Tags={string.Join(",", request.TagNames)}, Time={request.TimePoint:O}");

        return Ok(result);
    }
    [HttpPost("InxDataAPI")]
    public async Task<IActionResult> InxDataAPI([FromBody] DataRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"來自用戶 IP：{clientIp}");
        Console.WriteLine($"Token: {request.Token}");
        Console.WriteLine($"Client IP: {clientIp}");

        // ✅ 寫入 IP 紀錄 Log
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir); // 確保 Logs 資料夾存在
        var logFilePath = Path.Combine(logDir, "ip_log.txt");
        var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {clientIp}";
        await System.IO.File.AppendAllTextAsync(logFilePath, logLine + Environment.NewLine);

        if (request.TagNames == null || request.TagNames.Count == 0 || request.TagNames.Count > 50)
            return BadRequest("TagNames 必須 1~50 筆");

        var apiName = RouteData.Values["action"]?.ToString();
        var tokenRecord = _service.GetTokenByValue(apiName, request.Token, clientIp);

        if (tokenRecord == null)
            return Unauthorized("Token 驗證失敗或 IP 不允許");

        // 若改回用加密 Token 驗證請用以下方式：
        // string decrypted;
        // try { decrypted = AesHelper.Decrypt(request.Token); }
        // catch { return Unauthorized("Token 無效"); }
        // var parts = decrypted.Split('|');
        // if (parts.Length != 2) return Unauthorized("Token 格式錯誤");
        // var tokenRecord = _service.GetTokenRecord("FAC8DataAPI", parts[0], parts[1], clientIp);

        var timePoint = request.TimePoint;
        var result = await _influxService.GetTagValuesAsync(request.Factory, request.TagNames, request.TimePoint);

        Console.WriteLine($"呼叫 Influx: Bucket={request.Factory}, Tags={string.Join(",", request.TagNames)}, Time={request.TimePoint:O}");

        return Ok(result);
    }
    [HttpPost("InxTokenSet")]
    public async Task<IActionResult> InxTokenSet([FromBody] DataRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"來自用戶 IP：{clientIp}");
        Console.WriteLine($"Token: {request.Token}");
        Console.WriteLine($"Client IP: {clientIp}");

        // ✅ 寫入 IP 紀錄 Log
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        Directory.CreateDirectory(logDir); // 確保 Logs 資料夾存在
        var logFilePath = Path.Combine(logDir, "ip_log.txt");
        var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {clientIp}";
        await System.IO.File.AppendAllTextAsync(logFilePath, logLine + Environment.NewLine);

        if (request.TagNames == null || request.TagNames.Count == 0 || request.TagNames.Count > 50)
            return BadRequest("TagNames 必須 1~50 筆");

        var apiName = RouteData.Values["action"]?.ToString();
        var tokenRecord = _service.GetTokenByValue(apiName, request.Token, clientIp);

        if (tokenRecord == null)
            return Unauthorized("Token 驗證失敗或 IP 不允許");

        // 若改回用加密 Token 驗證請用以下方式：
        // string decrypted;
        // try { decrypted = AesHelper.Decrypt(request.Token); }
        // catch { return Unauthorized("Token 無效"); }
        // var parts = decrypted.Split('|');
        // if (parts.Length != 2) return Unauthorized("Token 格式錯誤");
        // var tokenRecord = _service.GetTokenRecord("FAC8DataAPI", parts[0], parts[1], clientIp);

        var timePoint = request.TimePoint;
        var result = await _influxService.GetTagValuesAsync(request.Factory, request.TagNames, request.TimePoint);

        Console.WriteLine($"呼叫 Influx: Bucket={request.Factory}, Tags={string.Join(",", request.TagNames)}, Time={request.TimePoint:O}");

        return Ok(result);
    }
}
