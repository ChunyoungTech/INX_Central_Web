using Dapper;
using INX_InfluxDB.Contexts;
using INX_InfluxDB.Models;
using INX_InfluxDB_Service.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace INX_InfluxDB_Service.Services
{
    /// <summary>
    /// 從資料庫中，讀取中央給的資料
    /// </summary>
    public class SettingLoaderService
    {
        private DapperContext context;
        private IConfiguration configuration;
        private readonly string sql;
        private readonly string databaseName;
        private readonly string facID;
        List<TagData> tagDatas = new List<TagData>() { };
        List<SysSetting> sysSettings = new List<SysSetting>() { };
       
        public SettingLoaderService(IConfiguration _configuration, DapperContext _context)
        {
            context = _context;
            configuration = _configuration;
            sql = configuration.GetConnectionString("SQL1") ?? throw new InvalidOperationException("SQL connection string 未設定或為空值。");
            facID = configuration.GetValue<string>("FacID") ?? throw new InvalidOperationException("SQL connection string 未設定或為空值。");
            var builder = new SqlConnectionStringBuilder(sql);
            databaseName = builder.InitialCatalog;
        }

        public List<TagData> TagDatas 
        { 
            get { return tagDatas; } 
        }
        public List<SysSetting> SysSettings
        {
            get { return sysSettings; }
        }

        public void GetTagDatas()
        {
            Log.Information("取得TagDatas");

            try
            {
                // 假設 context.GetSqlConnection("SQL1") 能正確取得連線
                using var conn = context.GetSqlConnection("SQL1");

                // *** SQL 查詢修改 ***
                //string sql = @$"SELECT ID AS ID,Tag_Name AS TagName,sys_name AS System FROM [{databaseName}].[dbo].[TagData] WHERE TagFac = @facID;";
                string sql = $@"SELECT 
    t.ID AS ID,
    t.Tag_Name AS TagName,
    t.TagSource,
    t.TagSys,
    m.mesurement AS System,
    f.FacName
FROM [TagData] t
LEFT JOIN [IDBSysMapping] m
    ON t.TagSys = CAST(m.SeqID AS VARCHAR)
LEFT JOIN [IDBFacData] f
    ON m.IDBFacDataID = f.SeqID
WHERE FacName =  @facID;";

                // *** 使用原有的 TagData 類別進行映射 ***
                // 因為 SQL SELECT 的欄位現在對應 TagData 的屬性
                var result = conn.Query<TagData>(sql, new { facID = facID, databaseName = databaseName }).ToList(); // <--- 保持使用 TagData
                tagDatas = result;
                //讓 ShowTagDatas 異步在背景執行
                _ = Task.Run(async () =>
                {
                    try
                    {
                        //await ShowTagDatas(result);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "ShowTagDatas 發生例外");
                    }
                });

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get TagDatas");
            }
            Log.Information("已取得所有TagDatas");
        }
        Task ShowTagDatas(List<TagData> l)
        {
            Log.Information("=================== 顯示 TagDatas ==================");
            foreach (var tag in l)
            {
                Log.Information($"SeqID: {tag.ID}, TagName: {tag.TagName}, System: {tag.System}");
            }

            return Task.CompletedTask;
        }

        public void GetSysSettings()
        {
            try
            {
                using var conn = context.GetSqlConnection("SQL1");
                string sql = @"SELECT [ID], [Code], [Name], [Value] FROM [SysSetting]";
                var result = conn.Query<SysSetting>(sql, new { FacID = facID }).ToList();
                foreach (var tag in result)
                {
                    //Debug.WriteLine($"ID: {tag.ID}, Code: {tag.Code}, Name: {tag.Name}, Value: {tag.Value}");
                }
                sysSettings = result;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get SysSetting");
            }
        }

        /// <summary>
        /// 對應到資料庫設定名稱
        /// </summary>
        public static readonly Dictionary<SettingType, string> StatusDescriptions = new Dictionary<SettingType, string>
        {
            { SettingType.BatchSize, "BatchSize" },
            { SettingType.HistRes, "HistRes" },
            { SettingType.HistTZ, "HistTZ" }
        };
    }

    /*IDBUrl
    IDBToken
    IDBOrg
    IDBBucket
    BatchSize
    HistRes
    HistTZ*/
    public enum SettingType
    {
        IDBUrl,
        IDBToken,
        IDBOrg,
        IDBBucket,
        BatchSize,
        HistRes,
        HistTZ
    }

}
