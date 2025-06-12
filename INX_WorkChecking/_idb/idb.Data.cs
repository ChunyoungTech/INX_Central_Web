using InfluxDB.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idb.Data
{
    public class IDBOptions
    {
        public string Url { get; set; }
        public string Org { get; set; }
        public string Bucket { get; set; }
        public string Token { get; set; }
    }

    public class TagValue
    {
        [Column("tag_id", IsTag = true)]
        public int TagID { get; set; }

        [Column("tag_name", IsTag = true)]
        public string TagName { get; set; }

        /// <summary>
        /// 注意: Timestamp為UTC，存入之前要先轉換時區
        /// </summary>
        [Column(IsTimestamp = true)]
        public DateTime Time { get; set; }

        /// <summary>
        /// 注意: 若IsTag(索引 unikey)與timestamp重複，則新的數據會覆蓋舊的那筆數據
        /// https://docs.influxdata.com/influxdb/v2/write-data/best-practices/duplicate-points/
        /// </summary>
        [Column("value")]
        public double Value { get; set; }
    }
    public class TagData
    {
        public int ID { get; set; }
        public string ScadaName { get; set; }
        public string IndName { get; set; }
        public string FacName { get; set; }
        public string Mesurement { get; set; }
    }

    public class FacData
    {
        public int SeqID { get; set; }
        public string FacName { get; set; }
        public string BucketName { get; set; }
        public string BucketToken { get; set; }
    }
}