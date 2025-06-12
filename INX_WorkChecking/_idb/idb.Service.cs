using idb.Data;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idb.InfluxDB
{
	public class Service : IDisposable
	{
        readonly InfluxDBClient oClient = null;
        readonly InfluxDBClientOptions oOptions = null;
        //const string Url = "http://chunyoung.asuscomm.com:58086";
        const string Org = "cy";
		const string Token = "je_gC0v3LsjupVay6gcnjzvaVcVpuoG8pu6As5IfLVNf-0p52bfNAq-5xXg2ECAzR7pmZx4np29frUoHOBbDaA==";

        public Service(IDBOptions options)
        {
            oOptions = new InfluxDBClientOptions(options.Url ?? cyc.Shared.SysQuery.GetAppSettingValue("InfluxDBURL"))
            {
                Bucket = options.Bucket,
                Org = options.Org ?? Org,
                Token = cyc.Global.IsDevelop ? Token : options.Token
            }; ;
            oClient = new InfluxDBClient(oOptions);
        }

        public List<TagValue> Query(IEnumerable<TagData> tagList, DateTime Time)
        {
            var TimeUtc = Time.ToUniversalTime();
            List<TagValue> oList = new List<TagValue>();
            try
            {
                if (true)
                {
                    var queryApi = oClient.GetQueryApi();

                    string query = $@"
from(bucket: ""{oOptions.Bucket}"")
|> range(start: time(v: ""{TimeUtc:o}""), stop: time(v: ""{TimeUtc.AddSeconds(1):o}""))
|> filter(fn: (r) => r._field == ""value"")
|> group(columns: [""_field"", ""tag_name""])
|> first()";

                    var result = queryApi.QueryAsync<TagValue>(query, oOptions.Org).Result;
                    if (result?.Count > 0)
                    {
                        oList.AddRange(from r in result
                                       join t in tagList on r.TagName equals t.ScadaName
                                       select new TagValue { TagName = t.IndName, Value = r.Value, Time = r.Time.ToLocalTime() });
                    }
                }

                //var Measurements = GetAllMeasurement();
                //if (Measurements?.Count > 0)
                //{
                //    var queryApi = oClient.GetQueryApi();

                //    foreach (var measurement in Measurements)
                //    {
                //        string query = $@"
                //    from(bucket: ""{oOptions.Bucket}"")
                //    |> range(start: -10d)
                //    |> filter(fn: (r) => r._measurement == ""{measurement}"")
                //    |> filter(fn: (r) => r._field == ""value"")
                //    |> group(columns: [""_field"", ""tag_name""])
                //    |> last()";

                //        var result = queryApi.QueryAsync<TagValue>(query, oOptions.Org).Result;
                //        if (result?.Count > 0)
                //        {
                //            oList.AddRange(from r in result
                //                           join t in tagList on r.TagName equals t.ScadaName
                //                           select new TagValue { TagName = t.IndName, Value = r.Value, Time = r.Time.ToLocalTime() });
                //        }
                //    }
                //}   

                //string query = $@"
                //from(bucket: ""{options.Bucket}"")
                //|> range(start: time(v: ""{TimeUtc:o}""), stop: time(v: ""{TimeUtc.AddSeconds(1):o}""))
                //|> filter(fn: (r) => r._measurement == ""tag_value_1"")
                //|> filter(fn: (r) => r._field == ""value"")
                //|> group(columns: [""_field"", ""tag_name""])
                //|> last()";

                //string query = $@"
                //    from(bucket: ""{oOptions.Bucket}"")
                //    |> range(start: -50d)
                //    |> filter(fn: (r) => r._measurement == ""tag_value"")
                //    |> filter(fn: (r) => r._field == ""value"")
                //    |> group(columns: [""_field"", ""tag_name""])
                //    |> last()";
                //var queryApi = oClient.GetQueryApi();
                //var result = queryApi.QueryAsync<TagValue>(query, oOptions.Org).Result;
                //if (result?.Count > 0)
                //{
                //    oList.AddRange(from r in result
                //                   join t in tagList on r.TagName equals t.ScadaName
                //                   select new TagValue { TagName = t.IndName, Value = r.Value, Time = r.Time.ToLocalTime() });
                //}
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog($"Query IDB Error: {ex.StackTrace} Options: {Newtonsoft.Json.JsonConvert.SerializeObject(oOptions)}"); }

            return oList;
        }

        public List<string> GetAllTagNames()
        {
            var tagNames = new List<string>();
            try
            {
                string query = $@"
                    import ""influxdata/influxdb/schema""
                    schema.tagValues(
                      bucket: ""{oOptions.Bucket}"",
                      tag: ""tag_name""
                    )";

                var queryApi = oClient.GetQueryApi();
                var tables = queryApi.QueryAsync(query, oOptions.Org).Result;
                foreach (var table in tables)
                {
                    foreach (var record in table.Records)
                    {
                        var value = record.GetValue() as string;
                        if (!string.IsNullOrEmpty(value) && value.Substring(0, 1) != "A")
                            tagNames.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                cyc.Log.WriteSysErrorLog($"GetAllTagNames Error: {ex.StackTrace} Options: {Newtonsoft.Json.JsonConvert.SerializeObject(oOptions)}");
            }
            return tagNames.Distinct().ToList();
        }

        public List<string> GetAllMeasurement()
        {
            var tagNames = new List<string>();
            try
            {
                string query = $@"
import ""influxdata/influxdb/schema""
schema.measurements(
  bucket: ""{oOptions.Bucket}""
)";
                var queryApi = oClient.GetQueryApi();
                var tables = queryApi.QueryAsync(query, oOptions.Org).Result;
                foreach (var table in tables)
                {
                    foreach (var record in table.Records)
                    {
                        var value = record.GetValue() as string;
                        if (!string.IsNullOrEmpty(value))
                            tagNames.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                cyc.Log.WriteSysErrorLog($"GetAllMeasurement Error: {ex.StackTrace} Options: {Newtonsoft.Json.JsonConvert.SerializeObject(oOptions)}");
            }
            return tagNames.Distinct().ToList();
        }

        public void Dispose()
        {
            oClient?.Dispose();
        }
    }
}