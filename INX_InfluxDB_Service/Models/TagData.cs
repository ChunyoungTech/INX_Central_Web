using InfluxDB.Client.Core;

namespace INX_InfluxDB.Models
{
    public class TagData
    {
        public int ID { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string System { get; set; } = string.Empty;
    }
}
