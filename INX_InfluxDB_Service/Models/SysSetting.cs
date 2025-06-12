using InfluxDB.Client.Core;

namespace INX_InfluxDB.Models
{
    public class SysSetting
    {
        public int ID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

