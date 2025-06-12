using InfluxDB.Client.Core;

namespace INX_InfluxDB.Models
{
    /// <summary>
    /// 產生
    /// SELECT [tf_data_source],[tf_data_gen_time] ,[tf_tagname],[tf_value],[tf_ack_flag],[tf_sn],[created],[fac]FROM [FAC_Loader].[dbo].[Transfer_Table_Temp]
    /// SQL 的結構資料
    /// </summary>
    public class TransferTableData
    {
        //產生 SELECT [tf_data_source],[tf_data_gen_time] ,[tf_tagname],[tf_value],[tf_ack_flag],[tf_sn],[created],[fac]FROM [FAC_Loader].[dbo].[Transfer_Table_Temp]   的結構
        public string tf_data_source { get; set; } = string.Empty;
        public string tf_data_gen_time { get; set; }
        public string tf_tagname { get; set; } = string.Empty;
        public string tf_value { get; set; }
        public string tf_ack_flag { get; set; }
        public string tf_sn { get; set; }
        public string created { get; set; }
        public string fac { get; set; } = string.Empty;

    }
}
