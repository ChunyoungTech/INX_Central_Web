using System.Text.Json.Serialization;

namespace InxDataAPI.Models
{
    public class DataResponse
    {
        public string TagName { get; set; }
        public DateTime Time { get; set; }
        public object Value { get; set; }
        public object Quality { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Message { get; set; }
    }
}
