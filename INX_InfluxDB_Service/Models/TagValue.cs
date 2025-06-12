using InfluxDB.Client.Core;

namespace INX_InfluxDB.Models
{
    /// <summary>
    /// http://chunyoung.asuscomm.com:58086/orgs/cb806e45604f937a/load-data/client-libraries/csharp
    /// Option 3: Use POCO and corresponding class to write data
    /// InfluxDB 命名規則: 使用小寫字母和下劃線分隔詞語。例如：cpu_load, memory_usage
    /// </summary>
    [Measurement("tag_value")]
    public class TagValue
    {
        [Column("tag_name", IsTag = true)]
        public string TagName { get; set; } = string.Empty;
        /// <summary>
        /// 注意: Timestamp為UTC，存入之前要先轉換時區
        /// </summary>
        [Column(IsTimestamp = true)]
        public DateTime DateTime { get; set; }
        [Column("value")]
        public double Value { get; set; }
        [Column("quality", IsTag = true)]
        public int Quality { get; set; }
    }
}
