using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CYCloud.FFURelated
{
    public class Logger
    {
        public void Log(string message)
        {
            if (cyc.Shared.SysQuery.GetAppSettingValue("ffuDebug") == "1")
            {
                string logPath = cyc.Shared.SysQuery.GetAppSettingValue("logPath");
                using (StreamWriter w = File.AppendText($"{logPath}/logs.txt"))
                {
                    w.WriteLineAsync(message);
                }
            }
        }
    }
    public class FFUPerformanceUpload : cyc.Auto.AutoJob //IJob
    {
        static bool IsRunning = false;
        protected override void Run()
        {
            cyc.Auto.Manager.Update("UploadFFUPerformance");
            if (!IsRunning)
            {
                IsRunning = true;

                cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                try
                {
                    UploadFFUPerformance();
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }

                cyc.Auto.Manager.Update("UploadFFUPerformance", oResult);
                IsRunning = false;
            }
        }
        public static void UploadFFUPerformance()
        {

            using (cyc.DB.SqlDapperConn mainDB = new cyc.DB.SqlDapperConn())
            using (cyc.DB.SqlDapperConn cimDB = new cyc.DB.SqlDapperConn(null, cyc.DB.ConnString.CIM))
            {
                //upload performance
                cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                {
                    Command = @"select * from vPerformance where 1=1"
                };
                bPara.Parameter.Clear();
                DataTable vPerformances = mainDB.QueryDataTable(bPara);

                foreach (DataRow vPerformance in vPerformances.Rows)
                {
                    bPara = new cyc.DB.SqlDBPara
                    {
                        Command = $@"INSERT INTO data_report ([SHOP_ID]
                                          ,[Report_Time]
                                          ,[Tag_name]
                                          ,[Type]
                                          ,[Value1]
                                          ,[Value1_DataGroup_Name]
                                          ,[Value1_DataGroup_Type]
                                          ,[Value2]
                                          ,[Value2_DataGroup_Name]
                                          ,[Value2_DataGroup_Type]
                                          ,[Value3]
                                          ,[Value3_DataGroup_Name]
                                          ,[Value3_DataGroup_Type]
                                          ,[Value4]
                                          ,[Value4_DataGroup_Name]
                                          ,[Value4_DataGroup_Type])
                                    VALUES (@SHOP_ID
                                           ,@Report_Time
                                           ,@Tag_name
                                           ,@Type
                                           ,@Value1
                                           ,@Value1_DataGroup_Name
                                           ,@Value1_DataGroup_Type
                                           ,@Value2
                                           ,@Value2_DataGroup_Name
                                           ,@Value2_DataGroup_Type
                                           ,@Value3
                                           ,@Value3_DataGroup_Name
                                           ,@Value3_DataGroup_Type
                                           ,@Value4
                                           ,@Value4_DataGroup_Name
                                           ,@Value4_DataGroup_Type)"
                    };
                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("SHOP_ID", vPerformance["SHOP_ID"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Report_Time", vPerformance["Report_Time"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Tag_name", vPerformance["Tag_name"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Type", vPerformance["Type"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value1", vPerformance["Value1"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value1_DataGroup_Name", vPerformance["Value1_DataGroup_Name"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value1_DataGroup_Type", vPerformance["Value1_DataGroup_Type"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value2", vPerformance["Value2"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value2_DataGroup_Name", vPerformance["Value2_DataGroup_Name"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value2_DataGroup_Type", vPerformance["Value2_DataGroup_Type"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value3", vPerformance["Value3"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value3_DataGroup_Name", vPerformance["Value3_DataGroup_Name"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value3_DataGroup_Type", vPerformance["Value3_DataGroup_Type"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value4", vPerformance["Value4"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value4_DataGroup_Name", vPerformance["Value4_DataGroup_Name"]));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("Value4_DataGroup_Type", vPerformance["Value4_DataGroup_Type"]));
                    cimDB.QueryDataTable(bPara);
                }

                //insert performance record
                FFUPerformanceHistory.InsertFFUPerformanceHsitory();
                FFUHistoryInsert.InsertFFUhsitory();
            }
        }
    }

    public class FFUPerformanceUpdate : cyc.Auto.AutoJob //IJob
    {
        static bool IsRunning = false;
        protected override void Run()
        {
            cyc.Auto.Manager.Update("UpdateFFUPerformance");
            if (!IsRunning)
            {
                IsRunning = true;

                cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                try
                {
                    UpdateFFUPerformance();
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }

                IsRunning = false;
                cyc.Auto.Manager.Update("UpdateFFUPerformance", oResult);
            }
        }
        public static void UpdateFFUPerformance()
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                {
                    Command = @"select * from FFU_Equipment where 1=1"
                };
                bPara.Parameter.Clear();
                DataTable dt = oDB.QueryDataTable(bPara);
                string equipmentName = "";
                int total = 0;
                int alarms = 0;
                int maintains = 0;
                int maintains_alarms = 0;
                int old_total = 0;
                int old_alarms = 0;
                int old_maintains = 0;
                int old_maintains_alarms = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    // get total, alarm, maintain and alarm with maintain
                    equipmentName = dr["ffu_equipment_name"].ToString();

                    bPara = new cyc.DB.SqlDBPara
                    {
                        Command = @"
select   count(1) as total
        ,count(case when (maintain='V') then 1 else null end) as  maintains
        ,count(case when (alarm!='0') then 1 else null end) as  alarms
        ,count(case when (maintain='V' and alarm!='0') then 1 else null end) as  maintains_alarms
from FFU 
where ffu_equipment_name=@equipmentName
and alarm!='255'
"
                    };
                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("equipmentName", equipmentName));
                    DataRow count = oDB.QueryDataTable(bPara).Rows[0];
                    total = Convert.ToInt32(count["total"]);
                    alarms = Convert.ToInt32(count["alarms"]);
                    maintains = Convert.ToInt32(count["maintains"]);
                    maintains_alarms = Convert.ToInt32(count["maintains_alarms"]);

                    //get old ffu data
                    string oldFFUPerformanceTable = cyc.Shared.SysQuery.GetAppSettingValue("oldFFUPerformanceTable");

                    bPara = new cyc.DB.SqlDBPara
                    {
                        Command = $@"select   *
                                    from {oldFFUPerformanceTable} 
                                    where EQP_ID=@equipmentName AND merge_flag = 1"
                    };
                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("equipmentName", equipmentName));
                    DataRowCollection oldDataRows = oDB.QueryDataTable(bPara).Rows;
                    if (oldDataRows.Count == 0)
                    {
                        old_total = 0;
                        old_alarms = 0;
                        old_maintains = 0;
                        old_maintains_alarms = 0;
                    }
                    else
                    {
                        DataRow oldData = oldDataRows[0];
                        old_total = Convert.ToInt32(oldData["total"] ?? 0);
                        old_alarms = Convert.ToInt32(oldData["alarms"] ?? 0);
                        old_maintains = Convert.ToInt32(oldData["maintains"] ?? 0);
                        old_maintains_alarms = Convert.ToInt32(oldData["maintains_alarms"] ?? 0);
                    }

                    total += old_total;
                    alarms += old_alarms;
                    maintains += old_maintains;
                    maintains_alarms += old_maintains_alarms;

                    if (total == 0)
                    {
                        bPara = new cyc.DB.SqlDBPara
                        {
                            Command = $@"UPDATE [dbo].[FFU_Equipment]
                                        SET [update_time] = GETDATE()
                                        ,   ffu_total = {total}
                                        ,   ffu_alarm = {alarms}
                                        ,   ffu_maintain = {maintains}
                                        ,   ffu_maintain_alarm = {maintains_alarms}
                                        ,   performance = 100
                                        ,   performance_report = 100
                                        WHERE ffu_equipment_name=@equipmentName"
                        };
                    }
                    else
                    {
                        //update performance
                        bPara = new cyc.DB.SqlDBPara
                        {
                            Command = $@"UPDATE [dbo].[FFU_Equipment]
                                        SET [update_time] = GETDATE()
                                        ,   ffu_total = {total}
                                        ,   ffu_alarm = {alarms}
                                        ,   ffu_maintain = {maintains}
                                        ,   ffu_maintain_alarm = {maintains_alarms}
                                        ,   performance = ROUND( {(double)((total - alarms) * 100 / total)},2)
                                        ,   performance_report = ROUND( {(double)((total - (alarms - maintains_alarms)) * 100 / total)},2)
                                        WHERE ffu_equipment_name=@equipmentName"
                        };
                    }


                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("equipmentName", equipmentName));
                    oDB.QueryDataTable(bPara);
                }



                //TODO: performance alert
            }
        }
    }

    public class FFUHistoryInsert : cyc.Auto.AutoJob //IJob
    {
        static bool IsRunning = false;
        protected override void Run()
        {
            cyc.Auto.Manager.Update("FFUHistoryInsert");
            if (!IsRunning)
            {
                IsRunning = true;

                cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();
                try
                {
                    InsertFFUhsitory();
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace); oResult.Error(ex.Message); }

                cyc.Auto.Manager.Update("FFUHistoryInsert", oResult);
                IsRunning = false;
            }
        }
        public static void InsertFFUhsitory()
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                {
                    Command = @"insert into ffu_history(
                                         [fab]
                                        ,[ffu_id]
                                        ,[gateway]
                                        ,[running]
                                        ,[speed_setting]
                                        ,[speed]
                                        ,[alarm]
                                        ,[maintain]
                                        ,[update_time]
                                        ,[ffu_equipment_name]
                                        ,[record_time]
                                        ,[alert_range]
                                )
                                SELECT 
                                         FFU.[fab]
                                        ,FFU.[ffu_id]
                                        ,FFU.[gateway]
                                        ,FFU.[running]
                                        ,FFU.[speed_setting]
                                        ,FFU.[speed]
                                        ,FFU.[alarm]
                                        ,FFU.[maintain]
                                        ,FFU.[update_time]
                                        ,FFU.[ffu_equipment_name]
                                        ,GETDATE() as [record_time]
                                        ,FFU_Gateway.[alert_range]

                                FROM [dbo].[FFU] 
                                left join FFU_Gateway 
                                  on FFU_Gateway.ffu_gateway_id=ffu.[gateway]"
                };
                bPara.Parameter.Clear();
                oDB.QueryDataTable(bPara);
            }
        }
    }

    public class FFUPerformanceHistory
    {
        public static void InsertFFUPerformanceHsitory()
        {
            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                {
                    Command = @"insert into FFU_Performance_History(
                                                [ffu_equipment_id]
                                                ,[update_time]
                                                ,[fab]
                                                ,[ffu_equipment_name]
                                                ,[ffu_total]
                                                ,[ffu_alarm]
                                                ,[ffu_maintain]
                                                ,[ffu_maintain_alarm]
                                                ,[performance]
                                                ,[performance_report]
                                                ,[record_time]
                                                ,[performance_alert]
                                        )
                                        select [ffu_equipment_id]
                                                ,[update_time]
                                                ,[fab]
                                                ,[ffu_equipment_name]
                                                ,[ffu_total]
                                                ,[ffu_alarm]
                                                ,[ffu_maintain]
                                                ,[ffu_maintain_alarm]
                                                ,[performance]
                                                ,[performance_report]
                                                ,GETDATE() as record_time
                                                ,[performance_alert]  
                                        from FFU_Equipment"
                };
                bPara.Parameter.Clear();
                oDB.QueryDataTable(bPara);
            }
        }
    }
}