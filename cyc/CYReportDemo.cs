using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.SS.UserModel;
using System.Threading.Tasks;

namespace CYCloud.Demo
{
    //[DisallowConcurrentExecutionAttribute()]
    public class AutoBatch : cyc.Auto.AutoJob //IJob
    {
        public static bool IsRunning { get; private set; } = false;
        protected override void Run()
        {
            cyc.Auto.Manager.Update("DemoDoAuto");
            if (!IsRunning)
            {
                IsRunning = true;

                DateTime dDate = DateTime.Today;
                //執行報表產出
                using (ReportCreate oCreate = new ReportCreate())
                {
                    oCreate.Execute(dDate.AddDays(-1));
                }
                IsRunning = false;
            }
        }
    }

    public class ReportCreate : IDisposable
    {
        cyc.DB.SqlDapperConn bDB = null;
        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();

        public ReportCreate()
        {
            bDB = new cyc.DB.SqlDapperConn();
        }

        public cyc.Data.ExeResult Execute(DateTime bDate, bool isAuto = true)
        {
            try
            {
                List<string> oError = new List<string>();
                TimeSpan nowTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

                if (isAuto)
                {
                    foreach (var rItem in Global.ReportSetting.List.Where(p => p.auto_create_time == nowTime && p.stop_flag != 'Y'))
                    {
                        oResult.Reset();

                        if (rItem.report_type == 1)
                            RunForDate(rItem, bDate);
                        else if (rItem.report_type == 2)
                            RunForMonth(rItem, bDate);

                        if (!oResult.Success) { oError.Add(oResult.Message); }

                        bDB.Execute(@"
update ReportData set last_exec_status=@Status,last_exec_time=@Time where ID=@ID;
insert into ReportDataExecLog (report_data_id,exec_time,exec_status,file_name) values (@ID,@Time,@Status,@File)"
        , new
        {
            Status = oResult.Success ? "成功" : (oResult.Message.Length > 50 ? oResult.Message.Substring(0, 50) : oResult.Message),
            Time = DateTime.Now,
            ID = rItem.ID,
            File = oResult.Success ? oResult.Message : ""
        });
                    }

                    foreach (var rItem in Global.ReportSetting.List.Where(p => p.auto_create_time2 == nowTime && p.stop_flag != 'Y'))
                    {
                        oResult.Reset();

                        if (rItem.report_type == 1)
                            RunForDate(rItem, bDate.AddDays(1));
                        else if (rItem.report_type == 2)
                            RunForMonth(rItem, bDate.AddDays(1));

                        if (!oResult.Success) { oError.Add(oResult.Message); }

                        bDB.Execute(@"
update ReportData set last_exec_status=@Status,last_exec_time=@Time where ID=@ID;
insert into ReportDataExecLog (report_data_id,exec_time,exec_status,file_name) values (@ID,@Time,@Status,@File)"
        , new
        {
            Status = oResult.Success ? "成功" : (oResult.Message.Length > 50 ? oResult.Message.Substring(0, 50) : oResult.Message),
            Time = DateTime.Now,
            ID = rItem.ID,
            File = oResult.Success ? oResult.Message : ""
        });
                    }

                }
                else
                {
                    foreach (var rItem in Global.ReportSetting.List.Where(p => p.stop_flag != 'Y'))
                    {
                        oResult.Reset();

                        if (rItem.report_type == 1)
                            RunForDate(rItem, bDate);
                        else if (rItem.report_type == 2)
                            RunForMonth(rItem, bDate);

                        if (!oResult.Success) { oError.Add(oResult.Message); }

                        bDB.Execute(@"
update ReportData set last_exec_status=@Status,last_exec_time=@Time where ID=@ID;
insert into ReportDataExecLog (report_data_id,exec_time,exec_status,file_name) values (@ID,@Time,@Status,@File)"
        , new
        {
            Status = oResult.Success ? "成功" : (oResult.Message.Length > 50 ? oResult.Message.Substring(0, 50) : oResult.Message),
            Time = DateTime.Now,
            ID = rItem.ID,
            File = oResult.Success ? oResult.Message : ""
        });
                    }
                }

                if (oError.Count > 0) { oResult.Error(string.Join(";", oError)); }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }
            return oResult;
        }

        private void RunForDate(CYCloud.ReportData rItem, DateTime bDate)
        {
            string sFileName = "";

            var xList = bDB.QueryMultiple(@"
select A.*,B.Tag_Name as Name from ReportTags A inner join TagData B on A.tag_data_id=B.ID where A.report_data_id=@ID order by A.sort;
select value_time from ReportDataTime where report_data_id=@ID order by sort;", new { rItem.ID });

            var tagList = xList.Read<ReportTag>().ToList();
            var timeList = xList.Read<ReportTime>().ToList();

            if (tagList.Count > 0 && timeList.Count > 0)
            {
                //                var xListV = bDB.oConn.QueryMultiple(@"
                //select tag_id as TagID,tag_value as TagValue,value_datetime as ValueDateTime from TagValues 
                //where tag_id in (select tag_data_id from ReportTags where report_data_id=@ID) 
                //and value_datetime in (select @Date+value_time from ReportDataTime where report_data_id=@ID);

                //select tag_value as TagValue,value_datetime as ValueDateTime,tag_value_max as TagValueMax,tag_value_min as TagValueMin
                //,max_datetime as ValueDateTimeMax,min_datetime as ValueDateTimeMin
                //from TagExtValues where value_datetime = @Date and tag_id in (select tag_data_id from ReportTags where report_data_id = @ID); "
                //, new { ID = rItem.ID, Date = bDate });

                //                var vList = xListV.Read<ReportTagValue>();
                //                var eList = xListV.Read<ReportTagValueExt>();
                var aList = bDB.QueryMultiple(@"
select tag_id as TagID,tag_value as TagValue,value_datetime as ValueDateTime from TagValues 
where tag_id in (select tag_data_id from ReportTags where report_data_id=@ID) 
and value_datetime in (select @Date+value_time from ReportDataTime where report_data_id=@ID)
;
select tag_id as TagID,tag_value as TagValue,value_datetime as ValueDateTime from TagValues 
where tag_id in (select tag_data_id from ReportTags where report_data_id=@ID) 
and value_datetime in (select @Date2+value_time from ReportDataTime where report_data_id=@ID)
;
select tag_id as TagID,tag_value as TagValue,value_datetime as ValueDateTime,tag_value_max as TagValueMax,tag_value_min as TagValueMin
,max_datetime as ValueDateTimeMax,min_datetime as ValueDateTimeMin
from TagExtValues where value_datetime=@Date and tag_id in (select tag_data_id from ReportTags where report_data_id=@ID)
"
, new { ID = rItem.ID, Date = bDate, Date2 = bDate.AddDays(-1) });

                var vList = aList.Read<ReportTagValue>();//當日
                var vList2 = aList.Read<ReportTagValue>();//前日
                var eList = aList.Read<ReportTagValueExt>();//最大 最小 平均

                if (vList.Count() > 0 || vList2.Count() > 0 || eList.Count() > 0)
                //if (vList.Count() > 0 || eList.Count() > 0)
                {
                    string sSysStorePath = cyc.Shared.SysQuery.GetAppSettingValue("ReportStorePath");
                    string sUsrStorePath = cyc.Shared.SysQuery.GetSysSettingValue("ReportPath");

                    string sTemplateFile = (cyc.Shared.SysQuery.GetSysSettingValue("ReportTemp") + @"\" + rItem.save_pate).Replace(@"\\", @"\");
                    if (!File.Exists(sTemplateFile))
                    {
                        oResult.Error("範本檔案不存在");
                    }
                    else if (!Directory.Exists(sUsrStorePath))
                    {
                        oResult.Error("報表存檔路徑不存在");
                    }
                    else
                    {
                        sFileName = rItem.report_name + DateTime.Now.ToString("yyyyMMddHHmmss") + "日報" + Path.GetExtension(sTemplateFile);

                        try
                        {
                            //開啟範本檔
                            IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(sTemplateFile);

                            if (wk != null)
                            //using (FileStream fs = File.Open(sTemplateFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                //wk = new HSSFWorkbook(fs);
                                //頁籤
                                //ISheet sheet = wk.GetSheet("rawData");
                                //if (sheet == null) { sheet = wk.CreateSheet("rawData"); }
                                //wk.SetSheetOrder("rawData", 0);
                                ////表頭
                                //IRow rowH = sheet.CreateRow(0);
                                //rowH.CreateCell(2).SetCellValue(rItem.report_name);
                                //rowH.CreateCell(7).SetCellValue(bDate.ToString("yyyy/MM/dd"));
                                //rowH = sheet.CreateRow(1);
                                //rowH.CreateCell(0).SetCellValue("DateTime");

                                ////各時間點-當日
                                //string sDate = bDate.ToString("yyyy/MM/dd");
                                //for (int idx = 0; idx < timeList.Count; idx++)
                                //    rowH.CreateCell(idx + 1).SetCellValue(sDate + " " + timeList[idx].value_time.ToString(@"hh\:mm"));
                                ////各時間點-前日
                                //sDate = bDate.AddDays(-1).ToString("yyyy/MM/dd");
                                //for (int idx = 0; idx < timeList.Count; idx++)
                                //    rowH.CreateCell(timeList.Count + idx + 1).SetCellValue(sDate + " " + timeList[idx].value_time.ToString(@"hh\:mm"));

                                //var vGroup = vList.ToLookup(p => p.TagID);
                                //var vGroup2 = vList2.ToLookup(p => p.TagID);

                                //string[,] oArray = new string[tagList.Count, timeList.Count * 2 + 1];

                                //Parallel.ForEach(tagList, (tag) =>
                                //{
                                //    int idxD = tagList.IndexOf(tag);
                                //    oArray[idxD, 0] = tag.Name;

                                //    var vItem = vGroup.FirstOrDefault(p => p.Key == tag.tag_data_id);
                                //    if (vItem != null)
                                //    {
                                //        for (int idxT = 0; idxT < timeList.Count; idxT++)
                                //        {
                                //            var v = vItem.FirstOrDefault(p => p.ValueDateTime.TimeOfDay == timeList[idxT].value_time);
                                //            if (v != null) { oArray[idxD, idxT + 1] = v.TagValue; }
                                //        }
                                //    }

                                //    var vItem2 = vGroup2.FirstOrDefault(p => p.Key == tag.tag_data_id);
                                //    if (vItem2 != null)
                                //    {
                                //        for (int idxT = 0; idxT < timeList.Count; idxT++)
                                //        {
                                //            var v = vItem2.FirstOrDefault(p => p.ValueDateTime.TimeOfDay == timeList[idxT].value_time);
                                //            if (v != null) { oArray[idxD, timeList.Count + idxT + 1] = v.TagValue; }
                                //        }
                                //    }
                                //});

                                //for (int i = 0; i <= oArray.GetUpperBound(0); i++)
                                //{
                                //    rowH = sheet.CreateRow(i + 2);
                                //    rowH.CreateCell(0).SetCellValue(oArray[i, 0] ?? "");
                                //    for (int j = 1; j <= oArray.GetUpperBound(1); j++)
                                //    {
                                //        if (oArray[i, j] != null)
                                //        {
                                //            double xValue = 0;
                                //            if (double.TryParse(oArray[i, j], out xValue))
                                //                rowH.CreateCell(j).SetCellValue(Math.Round(xValue, 2, MidpointRounding.AwayFromZero));
                                //            else
                                //                rowH.CreateCell(j).SetCellValue(oArray[i, j]);
                                //        }
                                //    }
                                //}

                                //#region Max Min Avg

                                //ISheet sheet2 = wk.GetSheet("rawData2");
                                //if (sheet2 == null) { sheet2 = wk.CreateSheet("rawData2"); }
                                //wk.SetSheetOrder("rawData2", 1);

                                //rowH = sheet2.CreateRow(0);
                                //rowH.CreateCell(0).SetCellValue("日期");
                                //rowH.CreateCell(1).SetCellValue(bDate.ToString("yyyy/MM/dd"));
                                //rowH.CreateCell(2).SetCellValue("最大值");
                                ////sheet2.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 2, 3));
                                //rowH.CreateCell(4).SetCellValue("最小值");
                                ////sheet2.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 4, 5));

                                //rowH = sheet2.CreateRow(1);
                                //rowH.CreateCell(0).SetCellValue("TAG");
                                //rowH.CreateCell(1).SetCellValue("平均值");
                                //rowH.CreateCell(2).SetCellValue("發生時間");
                                //rowH.CreateCell(3).SetCellValue("數值");
                                //rowH.CreateCell(4).SetCellValue("發生時間");
                                //rowH.CreateCell(5).SetCellValue("數值");

                                //for (int idx1 = 0; idx1 < tagList.Count; idx1++)
                                //{
                                //    IRow row = sheet2.CreateRow(idx1 + 2);
                                //    row.CreateCell(0).SetCellValue(tagList[idx1].Name);

                                //    var qItem = eList.FirstOrDefault(p => p.TagID == tagList[idx1].tag_data_id);
                                //    if (qItem != null)
                                //    {
                                //        row.CreateCell(1).SetCellValue(qItem.TagValue ?? "");
                                //        if (qItem.TagValueMax != null && qItem.ValueDateTimeMax != null)
                                //        {
                                //            row.CreateCell(2).SetCellValue(((DateTime)qItem.ValueDateTimeMax).ToString("HH:mm:ss"));
                                //            row.CreateCell(3).SetCellValue(qItem.TagValueMax);

                                //        }
                                //        if (qItem.TagValueMin != null && qItem.ValueDateTimeMin != null)
                                //        {
                                //            row.CreateCell(4).SetCellValue(((DateTime)qItem.ValueDateTimeMin).ToString("HH:mm:ss"));
                                //            row.CreateCell(5).SetCellValue(qItem.TagValueMin);
                                //        }
                                //    }
                                //    else
                                //    {
                                //        System.Threading.Thread.Sleep(1);
                                //    }
                                //}

                                //#endregion

                                //#region Sheet Formula
                                ////int iSheet = (wk.GetSheet("rawData2") == null ? 1 : 2);
                                ////bool bStop = false;
                                ////do
                                ////{
                                ////    try
                                ////    {
                                ////        ISheet sheetQ = wk.GetSheetAt(iSheet);
                                ////        sheetQ.ForceFormulaRecalculation = true;
                                ////        iSheet++;
                                ////    }
                                ////    catch { bStop = true; }
                                ////} while (!bStop);
                                //for (int isheet = (wk.GetSheet("rawData2") == null ? 1 : 2); isheet < wk.NumberOfSheets; isheet++)
                                //{
                                //    try { wk.GetSheetAt(isheet).ForceFormulaRecalculation = true; }
                                //    catch { continue; }
                                //}
                                //#endregion

                                //儲存報表檔案-系統指定目錄
                                string sSavePath = (sSysStorePath + @"\" + rItem.ID.ToString() + @"\").Replace(@"\\", @"\");
                                if (!Directory.Exists(sSavePath)) { Directory.CreateDirectory(sSavePath); }
                                using (FileStream fileStream = File.Open(sSavePath + sFileName, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    wk.Write(fileStream);
                                    fileStream.Close();
                                }
                                //儲存報表檔案-使用者自訂目錄
                                string userSavePath = (sUsrStorePath + @"\").Replace(@"\\", @"\");
                                if (!Directory.Exists(sSavePath)) { Directory.CreateDirectory(sSavePath); }
                                File.Copy(sSavePath + sFileName, userSavePath + sFileName);
                                //using (FileStream fileStream = File.Open(sSavePath + sFileName, FileMode.Create, FileAccess.ReadWrite))
                                //{
                                //    wk.Write(fileStream);
                                //    fileStream.Close();
                                //}


                                //fs.Close();
                                wk = null;
                                oResult.Message = sFileName;//執行成功，將檔名放入Message
                            }
                            else
                            {
                                oResult.Error("範本檔案格式有誤");
                            }
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
                    }
                }
                else
                {
                    oResult.Error("查無報表資料點指定時間值");
                }
            }
            else
            {
                oResult.Error("報表未設定資料點或記錄時間");
            }
        }

        private void RunForMonth(CYCloud.ReportData rItem, DateTime bDate)
        {
            //上個月最後一天 到 本月最後一天
            DateTime dDateS = new DateTime(bDate.Year, bDate.Month, 1).AddDays(-1);
            DateTime dDateE = dDateS.AddDays(1).AddMonths(1);
            //DateTime dDateE = bDate.Date.AddDays(1);
            string sFileName = "";

            var xList = bDB.QueryMultiple(@"
select A.*,B.Tag_Name as Name from ReportTags A inner join TagData B on A.tag_data_id=B.ID where A.report_data_id=@ID order by A.sort;
select top 1 value_time from ReportDataTime where report_data_id=@ID order by sort;", new { ID = rItem.ID });

            var tagList = xList.Read<ReportTag>().ToList();
            var timeList = xList.Read<ReportTime>().ToList();

            if (tagList.Count == 0 || timeList.Count == 0)
            {
                oResult.Error("報表未設定資料點或記錄時間");
            }
            else
            {
                var vList = bDB.QueryList<ReportTagValue>(@"
select tag_id as TagID,tag_value as TagValue,value_datetime as ValueDateTime from TagValues 
where tag_id in (select tag_data_id from ReportTags where report_data_id=@ID) and value_datetime between @DateS and @DateE"
, new { ID = rItem.ID, DateS = dDateS, DateE = dDateE }).Where(p => p.ValueDateTime.TimeOfDay == timeList[0].value_time).OrderBy(p => p.ValueDateTime);

                if (vList.Count() == 0)
                {
                    oResult.Error("查無報表資料點指定時間值");
                }
                else
                {
                    string sSysStorePath = cyc.Shared.SysQuery.GetAppSettingValue("ReportStorePath");
                    string sUsrStorePath = cyc.Shared.SysQuery.GetSysSettingValue("ReportPath");

                    string sTemplateFile = (cyc.Shared.SysQuery.GetSysSettingValue("ReportTemp") + @"\" + rItem.save_pate).Replace(@"\\", @"\");
                    if (!File.Exists(sTemplateFile))
                    {
                        oResult.Error("範本檔案不存在");
                    }
                    else if (!Directory.Exists(sUsrStorePath))
                    {
                        oResult.Error("報表存檔路徑不存在");
                    }
                    else
                    {
                        sFileName = rItem.report_name + DateTime.Now.ToString("yyyyMMddHHmmss") + "月報" + Path.GetExtension(sTemplateFile);

                        try
                        {

                            //開啟範本檔
                            IWorkbook wk = cyc.Shared.NPOI.GetWorkbook(sTemplateFile);
                            if (wk != null)
                            {
                                //wk = new HSSFWorkbook(fs);

                                //各時間點
                                ISheet sheet = wk.GetSheet("rawData");
                                if (sheet == null) { sheet = wk.CreateSheet("rawData"); }
                                wk.SetSheetOrder("rawData", 0);

                                IRow rowH = sheet.CreateRow(0);
                                int idxD = 0;
                                DateTime dDateT = dDateS;
                                while (dDateT < dDateE)
                                {
                                    rowH.CreateCell(idxD + 1).SetCellValue(dDateS.AddDays(idxD).ToString("yyyy/MM/dd"));
                                    idxD++;
                                    dDateT = dDateT.AddDays(1);
                                }

                                var vGroup = vList.GroupBy(p => p.TagID);

                                string[,] oArray = new string[tagList.Count, (dDateE - dDateS).Days + 1];
                                Parallel.ForEach(tagList, (tag) =>
                                {
                                    int idxT = tagList.IndexOf(tag);
                                    oArray[idxT, 0] = tag.Name;

                                    var vItem = vGroup.FirstOrDefault(p => p.Key == tag.tag_data_id);
                                    if (vItem != null)
                                    {
                                        int idxQ = 1;
                                        DateTime dDateQ = dDateS;
                                        while (dDateQ < dDateE)
                                        {
                                            var v = vItem.FirstOrDefault(p => p.ValueDateTime.Date == dDateQ);
                                            if (v != null) { oArray[idxT, idxQ] = v.TagValue; }
                                            idxQ++;
                                            dDateQ = dDateQ.AddDays(1);
                                        }
                                    }
                                });
                                for (int i = 0; i <= oArray.GetUpperBound(0); i++)
                                {
                                    rowH = sheet.CreateRow(i + 1);
                                    rowH.CreateCell(0).SetCellValue(oArray[i, 0] ?? "");
                                    for (int j = 1; j <= oArray.GetUpperBound(1); j++)
                                    {
                                        if (oArray[i, j] != null)
                                        {
                                            if (double.TryParse(oArray[i, j], out double xValue))
                                                rowH.CreateCell(j).SetCellValue(Math.Round(xValue, 2, MidpointRounding.AwayFromZero));
                                            else
                                                rowH.CreateCell(j).SetCellValue(oArray[i, j]);
                                        }
                                    }
                                }

                                #region SheetFormula
                                for (int isheet = (wk.GetSheet("rawData2") == null ? 1 : 2); isheet < wk.NumberOfSheets; isheet++)
                                {
                                    try { wk.GetSheetAt(isheet).ForceFormulaRecalculation = true; }
                                    catch { continue; }
                                }
                                #endregion

                                //儲存報表檔案-系統指定目錄
                                string sSavePath = (sSysStorePath + @"\" + rItem.ID.ToString() + @"\").Replace(@"\\", @"\");
                                if (!Directory.Exists(sSavePath)) { Directory.CreateDirectory(sSavePath); }
                                using (FileStream fileStream = File.Open(sSavePath + sFileName, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    wk.Write(fileStream);
                                    fileStream.Close();
                                }

                                //儲存報表檔案-使用者自訂目錄
                                string userSavePath = (sUsrStorePath + @"\").Replace(@"\\", @"\");
                                if (!Directory.Exists(sSavePath)) { Directory.CreateDirectory(sSavePath); }
                                File.Copy(sSavePath + sFileName, userSavePath + sFileName);
                                //using (FileStream fileStream = File.Open(sSavePath + sFileName, FileMode.Create, FileAccess.ReadWrite))
                                //{
                                //    wk.Write(fileStream);
                                //    fileStream.Close();
                                //}


                                //fs.Close();
                                wk = null;

                                oResult.Message = sFileName;//執行成功，將檔名放入Message
                            }
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
                    }
                }
            }
        }

        public void Dispose()
        {
            bDB.Dispose();
        }
    }

    public static class SyncReportValue
    {
        public static void DoSunc(DateTime dDate, string sType)
        {
            cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
            DateTime TimeS = DateTime.Now;
            try
            {
                if (sType == "TagValues")
                {
                    using (ReportValueSync oSync = new ReportValueSync())
                    {
                        oResult = oSync.Run(dDate);
                    }
                }
                else
                {
                    using (ReportValueExtSync oSyncExt = new ReportValueExtSync(dDate))
                    {
                        oResult = oSyncExt.RunSync();
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
            finally { WriteLog(sType, TimeS, DateTime.Now, oResult); }
        }

        public static void WriteLog(string sType, DateTime TimeS, DateTime TimeE, cyc.Data.ExeResult oResult)
        {
            try
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                {
                    oDB.Execute("insert into TagValueSyncLog (SyncData,StartTime,FinishTime,IsSuccess,Message) values (@sType,@TimeS,@TimeE,@Success,@Message)",
                        new { sType, TimeS, TimeE, oResult.Success, oResult.Message });
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); }
        }
    }

    public class ReportValueSync : IDisposable
    {
        cyc.DB.SqlDapperConn mDB = null;
        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
        StringBuilder oStr = new StringBuilder("");

        public ReportValueSync()
        {
            //oDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Other);
            mDB = new cyc.DB.SqlDapperConn();
        }

        public cyc.Data.ExeResult Run(DateTime bDate)
        {
            //cyc.Log.WriteSysErrorLog(bDate.ToString("yyyyMMdd") + "TagValues執行開始");
            Global.AutoSignal.DoSyncDataPublish("TagValues同步開始(" + bDate.ToString("yyyyMMdd") + ")");

            var xData = mDB.QueryList<ReportTagValue>(@"
select D.value_time as ValueTime,C.Tag_Name as TagName,C.ID as TagID,E.tag_value as TagValue
,@Date+D.value_time as ValueDateTime from ReportData A
inner join ReportTags B on A.ID=B.report_data_id
inner join TagData C on B.tag_data_id=C.ID
inner join ReportDataTime D on A.ID=D.report_data_id
left join TagValues E on C.ID=E.tag_id and E.value_datetime=@Date+D.value_time
where A.stop_flag<>'Y'
group by D.value_time,C.Tag_Name,C.ID,E.tag_value", new { Date = bDate });

            //依時間分組
            var tGroup = xData.GroupBy(p => p.ValueTime);

            foreach (var grp in tGroup)
            {
                DateTime dDate = bDate.AddTicks(grp.Key.Ticks);
                if (grp.Count() > 0)
                {
                    oStr.AppendFormat("{0}查詢Tag數：{1}筆，", dDate.ToString("yyyy/MM/dd HH:mm"), grp.Count());

                    //var hList = QueryHistory("'" + string.Join("','", grp.Select(p => p.TagName)) + "'", dDate);
                    var hList = QueryHistory(grp.Select(p => p.TagName), dDate);
                    oStr.AppendFormat("來源取得：{0}筆，", hList.Count);

                    if (hList != null && hList.Count > 0)
                    {
                        var insList = from lsH in hList
                                      join lsG in grp on new { lsH.TagName, lsH.ValueDateTime } equals new { lsG.TagName, lsG.ValueDateTime }
                                      where lsG.TagValue == null && lsH.TagValue != null
                                      select new ReportTagValue { TagID = lsG.TagID, TagValue = lsH.TagValue, ValueDateTime = lsH.ValueDateTime };

                        var updList = from lsH in hList
                                      join lsG in grp on new { lsH.TagName, lsH.ValueDateTime } equals new { lsG.TagName, lsG.ValueDateTime }
                                      where lsG.TagValue != lsH.TagValue
                                      select new ReportTagValue { TagID = lsG.TagID, TagValue = lsH.TagValue, ValueDateTime = lsH.ValueDateTime };
                        try
                        {
                            if (insList.Count() > 0)
                            {
                                mDB.Execute("insert into TagValues (tag_id,tag_value,value_datetime) values (@TagID,@TagValue,@ValueDateTime)", insList);
                            }
                            if (updList.Count() > 0)
                            {
                                mDB.Execute("update TagValues set tag_value=@TagValue where tag_id=@TagID and value_datetime=@ValueDateTime", updList);
                            }
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }
                        finally
                        {
                            if (oResult.Success)
                                oStr.AppendFormat("新增：{0}筆，更新：{1}筆；", insList.Count(), updList.Count());
                            else
                                oStr.AppendFormat("錯誤；{0}；", oResult.Message);
                        }
                    }
                }
            }
            //cyc.Log.WriteSysErrorLog(bDate.ToString("yyyyMMdd") + "TagValues執行結束");
            Global.AutoSignal.DoSyncDataPublish("TagValues同步結束(" + bDate.ToString("yyyyMMdd") + ")");
            cyc.Log.WriteSysErrorLog("TagValues同步結束(" + bDate.ToString("yyyyMMdd") + ")");

            oResult.Message = oStr.ToString();
            return oResult;
        }

        private List<ReportTagValue> QueryHistory(IEnumerable<string> tagList, DateTime dDate)
        {
            string sSQL = @"
SET NOCOUNT ON
DECLARE @StartDate DateTime
DECLARE @EndDate DateTime
SET @StartDate = '{1}'
SET @EndDate = '{2}'
SET NOCOUNT OFF
SELECT * FROM (
SELECT TagName,DateTime as ValueDateTime,vValue as TagValue,StartDateTime
FROM History
WHERE TagName IN ('{0}')
AND wwRetrievalMode = 'Cyclic'
AND wwCycleCount = 1
AND wwQualityRule = 'Optimistic'
AND wwVersion = 'Latest'
AND DateTime = @EndDate
 ) temp WHERE temp.StartDateTime >= @StartDate";

            List<ReportTagValue> rList = new List<ReportTagValue>();

            using (var xDB = new cyc.DB.SqlDapperConn(null, cyc.DB.ConnString.Runtime))
            {
                try
                {
                    int iStart = 0, iEnd = tagList.Count();

                    while (iStart < iEnd)
                    {
                        rList.AddRange(xDB.QueryList<ReportTagValue>(string.Format(sSQL, string.Join("','", tagList.Skip(iStart).Take(500)), dDate.Date.ToString("yyyyMMdd HH:mm:ss"), dDate.ToString("yyyyMMdd HH:mm:ss"))));
                        iStart += 500;
                    }

                    //return rList;
                    //return xDB.oConn.Query<ReportTagValue>(string.Format(sSQL, sTags, dDate.Date.ToString("yyyyMMdd HH:mm:ss"), dDate.ToString("yyyyMMdd HH:mm:ss"))).ToList();
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            }
            return rList;
        }

        #region OLD
        //        private List<ReportValueExt> QueryHistoryExt(string sTags, DateTime dDate)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();

        //            string sSQL1 = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'min' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ({0})
        //AND wwRetrievalMode='Min'
        //AND wwResolution=2073600000
        //AND wwQualityRule='Good'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            string sSQL2 = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'max' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ({0})
        //AND wwRetrievalMode='Max'
        //AND wwResolution=2073600000
        //AND wwQualityRule='Good'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            string sSQL3 = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'avg' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ({0})
        //AND wwRetrievalMode='Average'
        //AND wwCycleCount=1
        //AND wwQualityRule='Extended'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            using (var xDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Runtime))
        //            {
        //                try
        //                {
        //                    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(sSQL1, sTags, dDate.Date.ToString("yyyyMMdd HH:mm:ss"), dDate.AddDays(1).ToString("yyyyMMdd HH:mm:ss"))).ToList());

        //                    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(sSQL2, sTags, dDate.Date.ToString("yyyyMMdd HH:mm:ss"), dDate.AddDays(1).ToString("yyyyMMdd HH:mm:ss"))).ToList());

        //                    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(sSQL3, sTags, dDate.Date.ToString("yyyyMMdd HH:mm:ss"), dDate.AddDays(1).ToString("yyyyMMdd HH:mm:ss"))).ToList());
        //                }
        //                catch (Exception ex)
        //                {
        //                    cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //                    return null;
        //                }
        //            }
        //            return oList;
        //        }
        #endregion

        public void Dispose()
        {
            //oDB.Dispose();
            mDB.Dispose();
        }
    }

    public class ReportValueExtSync : IDisposable
    {
        cyc.DB.SqlDapperConn mDB = null;
        //cyc.DB.SqlDBConn xDB = null;
        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
        string sDateS = "", sDateE = "";
        DateTime bDate;
        StringBuilder oStr = new StringBuilder("");
        //int insCount = 0, updCount = 0;

        public ReportValueExtSync(DateTime xDate)
        {
            bDate = xDate;
            sDateS = xDate.Date.ToString("yyyyMMdd HH:mm:ss");
            sDateE = xDate.Date.AddDays(1).ToString("yyyyMMdd HH:mm:ss");
            mDB = new cyc.DB.SqlDapperConn();
            //xDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Runtime);
        }

        public cyc.Data.ExeResult RunSync(bool isTest = false)
        {
            Run(isTest);
            return oResult;
        }

        public void Run(bool isTest = false)
        {
            try
            {
                //cyc.Log.WriteSysErrorLog(bDate.ToString("yyyyMMdd") + "TagValuesExt執行開始");
                Global.AutoSignal.DoSyncDataPublish("TagExtValues同步開始(" + bDate.ToString("yyyyMMdd") + ")");

                //取得現有資料
                var vList = mDB.QueryList<ReportTagValueExt>(@"
                select C.ID as TagID,C.Tag_Name as TagName,D.tag_value as TagValue,D.value_datetime as ValueDateTime
                ,D.tag_value_max as TagValueMax,D.tag_value_min as TagValueMin,D.max_datetime as ValueDateTimeMax,D.min_datetime as ValueDateTimeMin
                from ReportData A
                inner join ReportTags B on A.ID=B.report_data_id
                inner join TagData C on B.tag_data_id=C.ID
                left join TagExtValues D on C.ID=D.tag_id and D.value_datetime=@Date
                where A.stop_flag<>'Y'", new { Date = bDate });

                //依名稱分組
                var tList = vList.GroupBy(p => p.TagName).Select(p => p.Key);

                oStr.AppendFormat("{0}查詢Tag數：{1}筆，", bDate.ToString("yyyy/MM/dd"), tList.Count());

                ////取得新資料
                var eList = QueryHistoryExt();

                oStr.AppendFormat("來源取得：{0}筆，", eList.Count());

                if (eList != null && eList.Count() > 0)
                {
                    //資料彙整
                    var hList = new List<ReportTagValueExt>();
                    foreach (var sTag in tList)
                    {
                        var exList = eList.Where(p => p.TagName == sTag);
                        if (exList != null && exList.Count() > 0)
                        {
                            var hItem = new ReportTagValueExt() { TagName = sTag, ValueDateTime = bDate };
                            foreach (var ex in exList)
                            {
                                switch (ex.TagValueType)
                                {
                                    case "avg":
                                        hItem.TagValue = ex.TagValue;
                                        break;
                                    case "max":
                                        hItem.TagValueMax = ex.TagValue;
                                        hItem.ValueDateTimeMax = ex.ValueDateTime;
                                        break;
                                    case "min":
                                        hItem.TagValueMin = ex.TagValue;
                                        hItem.ValueDateTimeMin = ex.ValueDateTime;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            hList.Add(hItem);
                        }
                    }

                    oStr.AppendFormat("比對後：{0}筆，", hList.Count());

                    //比對新舊資料
                    var insList = from lsH in hList
                                  join lsV in vList on lsH.TagName equals lsV.TagName
                                  where lsV.ValueDateTime == new DateTime()
                                  select new ReportTagValueExt
                                  {
                                      TagID = lsV.TagID,
                                      TagValue = lsH.TagValue,
                                      ValueDateTime = lsH.ValueDateTime,
                                      TagValueMax = lsH.TagValueMax,
                                      TagValueMin = lsH.TagValueMin,
                                      ValueDateTimeMax = lsH.ValueDateTimeMax,
                                      ValueDateTimeMin = lsH.ValueDateTimeMin
                                  };

                    var updList = from lsH in hList
                                  join lsV in vList on new { lsH.TagName, lsH.ValueDateTime } equals new { lsV.TagName, lsV.ValueDateTime }
                                  where lsV.TagValue != lsH.TagValue || lsV.TagValueMax != lsH.TagValueMax || lsV.TagValueMin != lsH.TagValueMin || lsV.ValueDateTimeMax != lsH.ValueDateTimeMax || lsV.ValueDateTimeMin != lsH.ValueDateTimeMin
                                  select new ReportTagValueExt
                                  {
                                      TagID = lsV.TagID,
                                      TagValue = lsH.TagValue,
                                      ValueDateTime = lsH.ValueDateTime,
                                      TagValueMax = lsH.TagValueMax,
                                      TagValueMin = lsH.TagValueMin,
                                      ValueDateTimeMax = lsH.ValueDateTimeMax,
                                      ValueDateTimeMin = lsH.ValueDateTimeMin
                                  };


                    oStr.AppendFormat("新增：{0}筆，更新：{1}筆；", insList.Count(), updList.Count());

                    if (insList.Count() > 0)
                    {
                        mDB.Connection.Execute(@"
                insert into TagExtValues (tag_id,tag_value,value_datetime,tag_value_max,tag_value_min,max_datetime,min_datetime)
                values (@TagID,@TagValue,@ValueDateTime,@TagValueMax,@TagValueMin,@ValueDateTimeMax,@ValueDateTimeMin)", insList, commandTimeout: 120);
                        //insCount = insList.Count();
                    }
                    if (updList.Count() > 0)
                    {
                        mDB.Connection.Execute(@"
                update TagExtValues set tag_value=@TagValue,tag_value_max=@TagValueMax,tag_value_min=@TagValueMin
                ,max_datetime=@ValueDateTimeMax,min_datetime=@ValueDateTimeMin where tag_id=@TagID and value_datetime=@ValueDateTime", updList, commandTimeout: 120);
                        //updCount = updList.Count();
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            if (!oResult.Success)
                oStr.AppendFormat("執行錯誤；{0}；", oResult.Message);

            //cyc.Log.WriteSysErrorLog(bDate.ToString("yyyyMMdd") + "TagValuesExt執行結束");
            //if (oResult.Success) { oResult.Message = string.Format("新增：{0}筆，更新{1}筆", insCount, updCount); }
            Global.AutoSignal.DoSyncDataPublish("TagExtValues同步結束(" + bDate.ToString("yyyyMMdd") + ")");


            oResult.Message = oStr.ToString();
        }

        #region QueryHistoryExt

        private IEnumerable<ReportValueExt> QueryHistoryExt()
        {
            return mDB.QueryList<ReportValueExt>(@"
select TagName,TagValue,ValueDateTime,TagType as TagValueType 
from TagExtValueTemp where ValueDateTime between @DateS and @DateE"
                , new { DateS = bDate, DateE = bDate.AddDays(1).AddMilliseconds(-1) });
        }

        //        private List<ReportValueExt> QueryHistoryExt(IEnumerable<string> tagList)
        //        {
        //            string[] strSQL = new string[3];

        //            strSQL[0] = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'min' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ('{0}')
        //AND wwRetrievalMode='Min'
        //AND wwCycleCount=1
        //AND wwQualityRule='Good'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            strSQL[1] = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'max' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ('{0}')
        //AND wwRetrievalMode='Max'
        //AND wwCycleCount=1
        //AND wwQualityRule='Good'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            strSQL[2] = @"
        //SET NOCOUNT ON
        //DECLARE @StartDate DateTime
        //DECLARE @EndDate DateTime
        //SET @StartDate='{1}'
        //SET @EndDate='{2}'
        //SET NOCOUNT OFF
        //SELECT *,'avg' as TagValueType FROM (
        //SELECT TagName,DateTime as ValueDateTime,Value as TagValue,StartDateTime
        //FROM History
        //WHERE History.TagName IN ('{0}')
        //AND wwRetrievalMode='Average'
        //AND wwCycleCount=2
        //AND wwQualityRule='Good'
        //AND wwVersion='Latest'
        //AND DateTime>=@StartDate
        //AND DateTime<@EndDate) temp WHERE temp.StartDateTime>=@StartDate";

        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                int iStart = 0, iEnd = tagList.Count(), iCount = 1;
        //                try
        //                {
        //                    iCount = Convert.ToInt32(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"));
        //                }
        //                catch { }

        //                while (iStart < iEnd)
        //                {
        //                    string sTags = string.Join("','", tagList.Skip(iStart).Take(iCount));

        //                    for (int idx = 0; idx < 3; idx++)
        //                    {
        //                        try
        //                        {
        //                            cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}：", iStart, iEnd) + string.Format(strSQL[idx], sTags, sDateS, sDateE));
        //                            if (xDB.oConn.State == System.Data.ConnectionState.Closed) { xDB.oConn.Open(); }
        //                            var qList = xDB.oConn.Query<ReportValueExt>(string.Format(strSQL[idx], sTags, sDateS, sDateE), commandTimeout: 120);
        //                            if (qList != null && qList.Count() > 0) { oList.AddRange(qList); }
        //                            System.Threading.Thread.Sleep(10);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            cyc.Log.WriteSysErrorLog(ex.Message);
        //                        }
        //                    }
        //                    //try
        //                    //{
        //                    //    //cyc.Log.WriteSysErrorLog(string.Format(minSQL, sTags, sDateS, sDateE));
        //                    //    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(minSQL, sTags, sDateS, sDateE), commandTimeout: 120).ToList());
        //                    //}
        //                    //catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + string.Format(minSQL, sTags, sDateS, sDateE)); }
        //                    //try
        //                    //{
        //                    //    //cyc.Log.WriteSysErrorLog(string.Format(maxSQL, sTags, sDateS, sDateE));
        //                    //    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(maxSQL, sTags, sDateS, sDateE), commandTimeout: 120).ToList());
        //                    //}
        //                    //catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + string.Format(maxSQL, sTags, sDateS, sDateE)); }
        //                    //try
        //                    //{
        //                    //    //cyc.Log.WriteSysErrorLog(string.Format(avgSQL, sTags, sDateS, sDateE));
        //                    //    oList.AddRange(xDB.oConn.Query<ReportValueExt>(string.Format(avgSQL, sTags, sDateS, sDateE), commandTimeout: 120).ToList());
        //                    //}
        //                    //catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + string.Format(avgSQL, sTags, sDateS, sDateE)); }

        //                    iStart += iCount;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //                //return null;
        //            }

        //            return oList;
        //        }

        //        private List<ReportValueExt> QueryHistoryExtAll(IEnumerable<string> tagList)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                int iStart = 0, iEnd = tagList.Count(), iCount = 10;
        //                int.TryParse(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"), out iCount);

        //                while (iStart < iEnd)
        //                {
        //                    int iTryCnt = 1;
        //                    bool bSuccess = false;
        //                    string sTags = string.Join(",", tagList.Skip(iStart).Take(iCount));

        //                    do
        //                    {
        //                        try
        //                        {
        //                            cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}-第{2}次: ", iStart, iEnd, iTryCnt) + sTags);

        //                            var qList = xDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtAll", new { Date = bDate, sTags = sTags }, commandTimeout: 120, commandType: System.Data.CommandType.StoredProcedure);
        //                            if (qList != null && qList.Count() > 0) { oList.AddRange(qList); }
        //                            bSuccess = true;
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}: ", iStart, iEnd) + ex.Message);

        //                            //xDB.Dispose();
        //                            //xDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Runtime);
        //                            iTryCnt++;
        //                        }
        //                    }
        //                    while (iTryCnt < 6 && !bSuccess);

        //                    iStart += iCount;
        //                }
        //                //}
        //            }
        //            catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message, oResult); }
        //            return oList;
        //        }

        //        private List<ReportValueExt> QueryHistoryExtAll2(IEnumerable<string> tagList)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                using (cyc.DB.SqlDBConn oDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Other))
        //                {
        //                    //全部一次執行
        //                    //oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtAll", new { Date = bDate, sTags = string.Join(",", tagList) }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                    //分段執行
        //                    int iStart = 0, iEnd = tagList.Count(), iCount = 100;
        //                    try
        //                    {
        //                        iCount = Convert.ToInt32(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"));
        //                    }
        //                    catch { }
        //                    while (iStart < iEnd)
        //                    {
        //                        string sTags = string.Join(",", tagList.Skip(iStart).Take(iCount));
        //                        cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}: ", iStart, iEnd) + sTags);
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMin", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMax", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtAvg", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        iStart += iCount;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //            }
        //            return oList;
        //        }

        //        private List<ReportValueExt> QueryHistoryExtMin(IEnumerable<string> tagList)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                using (cyc.DB.SqlDBConn oDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Other))
        //                {
        //                    //全部一次執行
        //                    //oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMin", new { Date = bDate, sTags = string.Join(",", tagList) }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                    //分段執行
        //                    int iStart = 0, iEnd = tagList.Count(), iCount = 100;
        //                    try
        //                    {
        //                        iCount = Convert.ToInt32(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"));
        //                    }
        //                    catch { }
        //                    while (iStart < iEnd)
        //                    {
        //                        string sTags = string.Join(",", tagList.Skip(iStart).Take(iCount));
        //                        cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}: ", iStart, iEnd) + sTags);
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMin", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        iStart += iCount;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //            }
        //            return oList;
        //        }

        //        private List<ReportValueExt> QueryHistoryExtMax(IEnumerable<string> tagList)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                using (cyc.DB.SqlDBConn oDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Other))
        //                {
        //                    //全部一次執行
        //                    //oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMax", new { Date = bDate, sTags = string.Join(",", tagList) }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                    //分段執行
        //                    int iStart = 0, iEnd = tagList.Count(), iCount = 100;
        //                    try
        //                    {
        //                        iCount = Convert.ToInt32(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"));
        //                    }
        //                    catch { }
        //                    while (iStart < iEnd)
        //                    {
        //                        string sTags = string.Join(",", tagList.Skip(iStart).Take(iCount));
        //                        cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}: ", iStart, iEnd) + sTags);
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtMax", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        iStart += iCount;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //            }
        //            return oList;
        //        }

        //        private List<ReportValueExt> QueryHistoryExtAvg(IEnumerable<string> tagList)
        //        {
        //            List<ReportValueExt> oList = new List<ReportValueExt>();
        //            try
        //            {
        //                using (cyc.DB.SqlDBConn oDB = new cyc.DB.SqlDBConn(cyc.DB.ConnString.Other))
        //                {
        //                    //全部一次執行
        //                    //oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtAvg", new { Date = bDate, sTags = string.Join(",", tagList) }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                    //分段執行
        //                    int iStart = 0, iEnd = tagList.Count(), iCount = 100;
        //                    try
        //                    {
        //                        iCount = Convert.ToInt32(cyc.Shared.SysQuery.GetAppSettingValue("SyncValueExtCount"));
        //                    }
        //                    catch { }
        //                    while (iStart < iEnd)
        //                    {
        //                        string sTags = string.Join(",", tagList.Skip(iStart).Take(iCount));
        //                        cyc.Log.WriteSysErrorLog(string.Format("{0}/{1}: ", iStart, iEnd) + sTags);
        //                        oList.AddRange(oDB.oConn.Query<ReportValueExt>("sp_GetTagValuesExtAvg", new { Date = bDate, sTags = sTags }, commandTimeout: 300, commandType: System.Data.CommandType.StoredProcedure));
        //                        iStart += iCount;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                cyc.Log.WriteSysErrorLog(ex.Message, oResult);
        //            }
        //            return oList;
        //        }

        #endregion

        public void Dispose()
        {
            mDB.Dispose();
            //xDB.Dispose();
        }
    }

    #region 類別定義
    public class ReportTagValue
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public TimeSpan ValueTime { get; set; }
        public string TagValue { get; set; }
        public DateTime ValueDateTime { get; set; }
    }

    public class ReportTagValueExt : ReportTagValue
    {
        public string TagValueMax { get; set; }
        public string TagValueMin { get; set; }
        public DateTime? ValueDateTimeMax { get; set; }
        public DateTime? ValueDateTimeMin { get; set; }
    }

    public class ReportValueExt : ReportTagValue
    {
        public string TagValueType { get; set; }
    }
    #endregion
}
