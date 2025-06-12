using cyc.Data;
using idb.Data;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace idb.Job
{
    public class LoaderDB : cyc.Auto.AutoJob
    {
        public const string JobKey = "ImportLoaderDB";
        public const string JobName = "LoaderDB資料匯入";
        const int Minute = 12;
        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                var oLog = new cyc.Data.FileLog();
                try
                {
                    DateTime tTime = Convert.ToDateTime($"{DateTime.Now:yyyy-MM-dd HH:mm}");
                    if (tTime.Minute % 30 == Minute)
                    {
                        oLog.AddLog($"[{JobKey}]開始");
                        using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                        {
                            int iCount = oDB.Execute(@"
insert into IFMTransferData
select C.tf_data_source,c.tf_data_gen_time,C.tf_tagname,C.tf_value,C.tf_ack_flag,C.tf_sn,CAST(C.created as datetime2)as created
from TagData A inner join IFMTransferIndex B on A.Tag_Name=B.scada_tagname
inner join FAC_Loader.dbo.Transfer_Table_Temp C on B.ind_fac=C.fac and B.ind_tagname=C.tf_tagname
where A.TagSource='LoaderDB' and C.tf_data_gen_time between @TimeS and @TimeE", new { TimeS = tTime.AddMilliseconds(-Minute), TimeE = tTime });
                            oLog.AddLog($"[{JobKey}]匯入資料筆數:{iCount}");
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    oLog.AddLog($"[{JobKey}]發生錯誤:{ex.Message}");
                    cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}"); 
                    oResult.Error(ex.Message); 
                }
                finally 
                {
                    cyc.Auto.Manager.CloseExclusive(JobKey, oResult);
                    oLog.AddLog($"[{JobKey}]結束");
                    cyc.Log.WriteFileLog(oLog.ToString(), JobName);
                }
            }
        }
    }

    public class InfluxDB : cyc.Auto.AutoJob
    {
        const int Minute = 5;
        public const string JobKey = "ImportInfluxDB";
        public const string JobName = "InfluxDB資料匯入";
        protected override void Run()
        {
            if (cyc.Auto.Manager.GetExclusive(JobKey))
            {
                var oLog = new cyc.Data.FileLog();
                try
                {
                    oLog.AddLog($"[{JobKey}]開始");
                    DateTime tTime = Convert.ToDateTime($"{DateTime.Now:yyyy-MM-dd HH:mm}");

                    List<TagData> tagList = null;
                    List<FacData> facList = null;
                    DataTable oTable = null;
                    using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                    {
                        var xData = oDB.QueryMultiple(@"
select RTRIM(B.scada_tagname) as ScadaName,RTRIM(B.ind_tagname) as IndName,RTRIM(B.ind_fac) as FacName,C.mesurement as Mesurement
from TagData A 
inner join IFMTransferIndex B on A.Tag_Name=B.scada_tagname
inner join IDBSysMapping C on A.TagSys=C.SeqID
inner join IDBFacData D on C.IDBFacDataID=D.SeqID and B.ind_fac=D.FacName
where A.TagSource='IntouchView' or A.TagSource='I/O'
group by B.scada_tagname,B.ind_tagname,B.ind_fac,C.Mesurement
;
select * from IDBFacData");
                        if (!oResult.Success)
                            oLog.AddLog($"[{JobKey}]查詢資料庫失敗");

                        if (xData != null)
                        {
                            tagList = xData.Read<TagData>().ToList();
                            facList = xData.Read<FacData>().ToList();
                            oLog.AddLog($"[{JobKey}]，TagData:{tagList.Count}筆，FacData:{facList.Count}筆");

                            oTable = oDB.QueryDataTable(@"select * from IFMTransferData where 1=0");
                        }
                    }

                    if (tagList?.Count > 0 && facList?.Count > 0 & oTable != null)
                    {
                        foreach (var tagGroup in tagList.GroupBy(p => p.FacName))
                        {
                            var fac = facList.FirstOrDefault(f => f.FacName == tagGroup.Key.Trim());
                            if (fac != null)
                            {
                                oLog.AddLog($"[{JobKey}]，查詢InfluxDB bucket:{fac.BucketName}");

                                var oOptions = new IDBOptions { Bucket = fac.BucketName, Token = fac.BucketToken };
                                using (var oService = new idb.InfluxDB.Service(oOptions))
                                {
                                    var vList = oService.Query(tagGroup, tTime.AddMinutes(-5));
                                    if (vList != null)
                                    {
                                        oLog.AddLog($"[{JobKey}]，取得資料數:{vList.Count}筆");
                                        foreach (var v in vList)
                                            oTable.Rows.Add(new object[] { "Real_Time_Data", v.Time.ToString("yyyy/MM/dd HH:mm:ss"), v.TagName, v.Value, "N", null, v.Time });
                                    }
                                }
                            }
                        }

                        if (oTable != null && oTable.Rows.Count > 0)
                        {
                            oLog.AddLog($"[{JobKey}]，總計InfluxDB資料數:{oTable.Rows.Count}筆");
                            using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                            {
                                oDB.BulkCopy(oTable, "dbo.IFMTransferData");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    oLog.AddLog($"[{JobKey}]發生錯誤:{ex.Message}");
                    cyc.Log.WriteSysErrorLog($"{JobName}:{ex.Message}");
                    oResult.Error(ex.Message); 
                }
                finally 
                { 
                    cyc.Auto.Manager.CloseExclusive(JobKey, oResult);
                    oLog.AddLog($"[{JobKey}]結束");
                    cyc.Log.WriteFileLog(oLog.ToString(), JobName);
                }
            }
        }
    }

    public static class Shared
    {

        public static void TestInfluxDB(DateTime tTime)
        {
            ExeResult oResult = new ExeResult();
            try
            {
                List<TagData> tagList = null;
                List<FacData> facList = null;
                DataTable oTable = null;
                using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                {
                    var xData = oDB.QueryMultiple(@"
select RTRIM(B.scada_tagname) as ScadaName,RTRIM(B.ind_tagname) as IndName,RTRIM(B.ind_fac) as FacName,C.mesurement as Mesurement
from TagData A 
inner join IFMTransferIndex B on A.Tag_Name=B.scada_tagname
inner join IDBSysMapping C on A.TagSys=C.SeqID
inner join IDBFacData D on C.IDBFacDataID=D.SeqID and B.ind_fac=D.FacName
where A.TagSource='IntouchView' or A.TagSource='I/O'
group by B.scada_tagname,B.ind_tagname,B.ind_fac,C.mesurement
;
select * from IDBFacData");
                    if (xData != null)
                    {
                        tagList = xData.Read<TagData>().ToList();
                        facList = xData.Read<FacData>().ToList();

                        oTable = oDB.QueryDataTable(@"select * from IFMTransferData where 1=0");
                    }
                }

                if (tagList?.Count > 0 && facList?.Count > 0 & oTable != null)
                {
                    foreach (var tagGroup in tagList.GroupBy(p => p.FacName))
                    {
                        var fac = facList.FirstOrDefault(f => f.FacName == tagGroup.Key.Trim());
                        if (fac != null)
                        {
                            var oOptions = new IDBOptions { Bucket = fac.BucketName, Token = fac.BucketToken };
                            using (var oService = new idb.InfluxDB.Service(oOptions))
                            {
                                var vList = oService.Query(tagGroup, tTime.AddMinutes(-5));
                                if (vList != null && vList.Count > 0)
                                {
                                    foreach (var v in vList)
                                        oTable.Rows.Add(new object[] { "Real_Time_Data", v.Time.ToString("yyyy/MM/dd HH:mm:ss"), v.TagName, v.Value, "N", null, v.Time });
                                }
                            }
                        }
                    }

                    if (oTable != null && oTable.Rows.Count > 0)
                    {
                        using (var oDB = new cyc.DB.SqlDapperConn(oResult))
                        {
                            oDB.BulkCopy(oTable, "dbo.IFMTransferData");
                        }
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog($"TestInfluxDB:{ex.Message}"); oResult.Error(ex.Message); }
            //finally { cyc.Auto.Manager.CloseExclusive(JobKey, oResult); }
        }
    }
}