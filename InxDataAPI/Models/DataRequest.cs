namespace InxDataAPI.Models
{
    public class DataRequest
    {
        public string Token { get; set; }
        public string Factory { get; set; } // 例如 "8廠" 或 "T6"
        public List<string> TagNames { get; set; } // 最多 50 個
        public DateTime TimePoint { get; set; }
    }
}
