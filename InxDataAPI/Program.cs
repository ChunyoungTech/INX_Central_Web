using InxDataAPI.Core.Contexts;
using InxDataAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataApiService>();  // 注入資料庫服務
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<InfluxDBContext>();

builder.Services.AddScoped<InxFluxDBService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
