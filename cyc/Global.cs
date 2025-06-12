using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYCloud
{
    public static class Global
    {
        //MAPP設定
        public static cyc.Data.SysCacheObj<CYCloud.Mapp.Data.MappSetting> MappSetting { get; private set; } = new cyc.Data.SysCacheObj<CYCloud.Mapp.Data.MappSetting>("select * from MappSetting");
        //MAPP分類
        public static cyc.Data.SysCacheObj<CYCloud.Mapp.Data.MappSettingType> MappSettingType { get; private set; } = new cyc.Data.SysCacheObj<CYCloud.Mapp.Data.MappSettingType>("select * from MappSettingType order by MT_SORT_NUM");
        //地震降壓設定
        public static cyc.Data.SysCacheObj<CYCloud.MappEV.Data.MappEVSetting> MappEV { get; private set; } = new cyc.Data.SysCacheObj<CYCloud.MappEV.Data.MappEVSetting>(@"
select A.ID,A.Code,A.Name,A.Type,A.FacArea,A.IsTop,A.NormalID,A.DisableID,A.MappSubject,A.MappContent
,B.MS_SYS_NAME as NormalCode,ISNULL(C.MS_SYS_NAME,'') as DisableCode,CimWebApi,CimMethod,CimParaData,CimEnable,CimLevel,CimGroup
from MappEV A inner join MappSetting B on A.NormalID=B.MS_SEQ_ID left join MappSetting C on A.DisableID=C.MS_SEQ_ID");
        //報表設定
        public static cyc.Data.SysCacheObj<ReportData> ReportSetting { get; private set; } = new cyc.Data.SysCacheObj<ReportData>("select * from ReportData");

        //辨識系統 預設目前已存在資料
        //public static cyc.Data.SysCacheObj<RecognitionAuth> RecognitionAuth { get; set; } = new cyc.Data.SysCacheObj<RecognitionAuth>("select * from IFP_RecognitionAuth where LogDateTime>@Date", new { Date = DateTime.Now.AddMinutes(-10) });

//        public static void Close()
//        {
//            if (MappEVHighList != null && MappEVHighList.Any())
//            {
//                using (var oDB = new cyc.DB.SqlDapperConn())
//                {
//                    foreach (var oData in MappEVHighList)
//                    {
//                        oDB.Execute(@"
//delete from MappEVTemp where [Type]=@Type and FacArea=@FacArea;
//insert into MappEVTemp ([Type],FacArea,RawData,UpdateTime) values (@Type,@FacArea,@RawData,getdate())"
//, new { oData.Type, oData.FacArea, RawData = Newtonsoft.Json.JsonConvert.SerializeObject(oData) });
//                        if (oData.List != null) oData.List.Clear();
//                    }
//                }
//            }
//        }

        public static class AutoSignal
        {
            //發送訂閱
            public static event EventHandler<string> RaiseSendMappEvent;//發送 Mapp 訊息

            public static event EventHandler<string> RaiseSyncMappEvent;//同步 Mapp 內外網資料

            public static event EventHandler<string> RaiseSyncDataEvent;//同步 TagValues

            public static event EventHandler<string> RaiseTagDataChangeEvent;//發布 TagData 修改

            public static void DoSendMappPublish(string sMsg)
            {
                try
                { RaiseSendMappEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
                catch (Exception ex) { DoErrorHandle(ex); }
            }

            public static void DoSyncMappPublish(string sMsg)
            {
                try
                { RaiseSyncMappEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
                catch (Exception ex) { DoErrorHandle(ex); }
            }

            public static void DoSyncDataPublish(string sMsg)
            {
                try
                { RaiseSyncDataEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
                catch (Exception ex) { DoErrorHandle(ex); }
            }

            public static void DoTagDataChangePublish(string sMsg)
            {
                try
                { RaiseTagDataChangeEvent?.Invoke(null, DateTime.Now.ToString() + "--" + sMsg); }
                catch (Exception ex) { DoErrorHandle(ex); }
            }

            private static void DoErrorHandle(Exception ex)
            {
                //Do Something
            }
        }
    }
}
