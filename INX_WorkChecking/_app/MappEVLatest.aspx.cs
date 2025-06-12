using CYCloud;
using cyc.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CYCloud.MappEV;
using CYCloud.MappEV.Data;
using static Dapper.SqlMapper;
using NPOI.SS.Formula.Functions;

namespace WebApp._app
{
    public partial class MappEVLatest : BasePageGrid
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Panel1.Visible = false;
        }
        protected override System.Data.DataTable QuerySourceData(int idx)
        {
            return null;
        }

        protected override GridPageOption SetPageSetting()
        {
            return new GridPageOption() { GridOption = new GridOption[] { new GridOption() } };
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            GridView1.Visible = false;
            GridView2.Visible = false;
            btnSend.Enabled = false;

            var xData = dDB.QueryMultiple(@"
        select Top 1 * from MappEVLatest where FacArea=@FacArea and Type=@Type
        ;
        select Code as FacName,Type,1 as IsDisable from MappEV where FacArea=@FacArea and Type=@Type and IsTop=0 order by Code
        ", new { FacArea = ddlAreaQ.SelectedValue, Type = ddlTypeQ.SelectedValue });

            if (oResult.Success)
            {
                var oData = xData.ReadFirstOrDefault<CYCloud.MappEV.Data.MappEVLatest>();
                var sList = xData.Read<MappEVInputEx>();

                if (sList.Count() > 0)
                {
                    if (oData != null && oData.UpdateTime.Date == DateTime.Today)
                    {
                        List<MappEVInput> qList = null;
                        try
                        {
                            qList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MappEVInput>>(oData.List);
                        }
                        catch { }

                        if (qList == null) { qList = new List<MappEVInput>(); }

                        if (qList.Any())
                        {
                            foreach (var s in sList)
                            {
                                var qData = qList.FirstOrDefault(p => p.FacName == s.FacName && !p.IsDisable);
                                if (qData != null)
                                {
                                    s.DateStr = qData.InputTime.ToString("yyyy-MM-dd");
                                    s.TimeStr = qData.InputTime.ToString("HH:mm:ss");
                                    s.Value1 = qData.Value1;
                                    s.Value2 = qData.Value2;
                                    s.Value3 = qData.Value3;
                                    s.Value4 = qData.Value4;
                                    s.Value5 = qData.Value5;
                                    s.IsDisable = qData.IsDisable;
                                }
                            }
                        }
                    }
                    hidData.Value = ddlTypeQ.SelectedValue + "," + ddlAreaQ.SelectedValue;
                    btnSend.Enabled = true;
                }
                if (ddlTypeQ.SelectedValue == "E")
                {
                    GridView1.DataSource = sList;
                    GridView1.DataBind();
                    GridView1.Visible = true;
                }
                else
                {
                    GridView2.DataSource = sList;
                    GridView2.DataBind();
                    GridView2.Visible = true;
                }
            }
            if (!oResult.Success) { ShowResult("", false, false); }
        }
//        protected void btnQuery_Click(object sender, EventArgs e)
//        {
//            GridView1.Visible = false;
//            GridView2.Visible = false;
//            btnSend.Enabled = false;

//            var xData = dDB.QueryMultiple(@"
//select top 1 FacArea,Type,List as ListJson,SendTime,ResetTime,UpdateTime from MappEVHigh where FacArea=@Area and Type=@Type
//;
//select Code as FacName,Type,1 as IsDisable from MappEV where FacArea=@Area and Type=@Type and IsTop=0 order by Code
//", new { Area = ddlAreaQ.SelectedValue, Type = ddlTypeQ.SelectedValue });

//            if (oResult.Success)
//            {
//                var oData = xData.ReadFirstOrDefault<CYCloud.MappEV.Data.MappEVHigh>();
//                var sList = xData.Read<MappEVInputEx>();
//                if (sList.Count() > 0)
//                {
//                    if (oData != null && oData.UpdateTime.Date == DateTime.Today)
//                    {
//                        try
//                        {
//                            oData.List = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MappEVInput>>(oData.ListJson);
//                        }
//                        catch { }

//                        foreach (var s in sList)
//                        {
//                            var qData = oData.List.FirstOrDefault(p => p.FacName == s.FacName && !p.IsDisable);
//                            if (qData != null)
//                            {
//                                s.DateStr = qData.InputTime.ToString("yyyy-MM-dd");
//                                s.TimeStr = qData.InputTime.ToString("HH:mm:ss");
//                                s.Value1 = qData.Value1;
//                                s.Value2 = qData.Value2;
//                                s.Value3 = qData.Value3;
//                                s.Value4 = qData.Value4;
//                                s.Value5 = qData.Value5;
//                                s.IsDisable = qData.IsDisable;
//                            }
//                        }
//                    }
//                    hidData.Value = ddlTypeQ.SelectedValue + "," + ddlAreaQ.SelectedValue;
//                    btnSend.Enabled = true;
//                }
//                if (ddlTypeQ.SelectedValue == "E")
//                {
//                    GridView1.DataSource = sList;
//                    GridView1.DataBind();
//                    GridView1.Visible = true;
//                }
//                else
//                {
//                    GridView2.DataSource = sList;
//                    GridView2.DataBind();
//                    GridView2.Visible = true;
//                }
//            }
//            if (!oResult.Success) { ShowResult("", false, false); }
//        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            TmpData tData = Preview();
            if (!oResult.Success)
                ShowResult("", false, false);
            else if (tData.Message != null)
            {
                dDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT,MM_TYPE,UPDATE_USER) values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT,@MM_TYPE,@UPDATE_USER)", tData.Message);
                if (oResult.Success && tData.Setting != null && tData.HighData != null)
                {
                    //更新最新資料->補發查詢
                    //Shared.UpdateMappEVLatest(tData.HighData, dDB);
                    dDB.Execute(@"delete from MappEVLatest where FacArea=@FacArea and Type=@Type;
insert into MappEVLatest (FacArea,Type,List,UpdateTime) values (@FacArea,@Type,@List,getdate())"
, new { tData.HighData.FacArea, tData.HighData.Type, List = Newtonsoft.Json.JsonConvert.SerializeObject(tData.HighData.List) });

                    //系統CIM啟用，高階CIM啟用，各廠比重加總>0
                    if (Shared.DoCimWebApi && tData.Setting.CimEnable && !string.IsNullOrEmpty(tData.Setting.CimWebApi) && !string.IsNullOrEmpty(tData.Setting.CimParaData))
                    {
                        //20240716 CIM 高階 分組
                        try
                        {
                            var cList = tData.HighData.List.Where(p => !p.IsDisable && p.Value1 != "NA").GroupBy(p => p.CimGroup).Where(p => p.Sum(q => q.CimLevel) > 0);
                            foreach (var cData in cList)
                            {
                                var qData = Shared.GetFirstValue(tData.Setting.Type, cData);
                                if (qData != null)
                                {
                                    CimWebApi.Enqueue(new CimTask
                                    {
                                        MappEVID = tData.Setting.ID,
                                        WebApi = tData.Setting.CimWebApi,
                                        Method = tData.Setting.CimMethod,
                                        ParaData = Shared.ConvertMappContent(tData.Setting.CimParaDataX, qData),
                                        IsHigh = true
                                    });
                                }
                            }
                            //CimWebApi
                            System.Threading.Tasks.Task.Run(() => CimWebApi.Execute());
                        }
                        catch (Exception ex) { cyc.Log.WriteSysErrorLog($"MappEV處理高階彙整CIM：{ex.Message}"); }
                    }

                    ////系統CIM啟用，高階CIM啟用，各廠比重加總>0
                    //if (Shared.DoCimWebApi && tData.Setting.CimEnable && !string.IsNullOrEmpty(tData.Setting.CimWebApi) && !string.IsNullOrEmpty(tData.Setting.CimParaData) && tData.HighData.List.Sum(p => p.CimLevel) > 0)
                    //{
                    //    var first = tData.HighData.List.FirstOrDefault(p => p.Value1 != "NA");
                    //    if (first != null)
                    //    {
                    //        CYCloud.Global.CimWebApiTaskQueue.Enqueue(new CimTask
                    //        {
                    //            MappEVID = tData.Setting.ID,
                    //            WebApi = tData.Setting.CimWebApi,
                    //            Method = tData.Setting.CimMethod,
                    //            ParaData = Shared.ConvertMappContent(tData.Setting.CimParaDataX, first)
                    //        });

                    //        //CimWebApi
                    //        System.Threading.Tasks.Task.Run(() => CimWebApi.Execute());
                    //    }
                    //}
                }
                ShowResult("已加入發送排程", false, false);
            }
        }

        private TmpData Preview()
        {
            TmpData tData = new TmpData();
            string[] sKey = hidData.Value.Split(',');
            if (sKey.Length == 2)
            {
                //高階設定
                tData.Setting = CYCloud.Global.MappEV.List.FirstOrDefault(p => p.Type == sKey[0] && p.FacArea == sKey[1] && p.IsTop);
                if (tData.Setting != null)
                {
                    tData.HighData = new MappEVHigh { Type = sKey[0], FacArea = sKey[1], List = GetList(sKey[0]) };

                    if (oResult.Success)
                    {
                        if (tData.HighData.List.Count > 0)
                        {
                            //轉換Template
                            Shared.ConvertMappTemplate(tData.Setting);

                            string sNewLine = "\n";
                            //第1筆 非Null的資料
                            //var oFirst = tData.HighData.List.FirstOrDefault(p => p.Value1 != "NA");
                            //20250312修改，地震最大值、壓降最小值
                            var oFirst = Shared.GetFirstValue(tData.HighData.Type, tData.HighData.List.Where(p => p.Value1 != "NA"));
                            if (oFirst != null)
                            {
                                //查詢正式群組是否有隔離
                                //20230320 地震壓降不設定隔離群組，一律改在隔離設定做，隔離處置僅高階彙整
                                bool bDisable = dDB.QueryOne<int>("select count(1) from MappDisable where MS_SEQ_ID=@ID and MD_STOP_TIME is null and @Time between MD_DATE_START and MD_DATE_END", new { ID = tData.Setting.NormalID, Time = DateTime.Now }) > 0;
                                //tData.Message = new MappEVMessage { MS_SYS_NAME = bDisable ? tData.Setting.DisableCode : tData.Setting.NormalCode, MM_TYPE = 'M', UPDATE_USER = bUser.User.ID };
                                tData.Message = new MappEVMessage { MS_SYS_NAME = tData.Setting.NormalCode, MM_TYPE = 'M', UPDATE_USER = bUser.User.ID };
                                //MAPP主旨
                                tData.Message.MM_SUBJECT = CYCloud.MappEV.Shared.ConvertMappContent(tData.Setting.MappSubjectX, oFirst);
                                //MAPP內容 - 標頭+內容+結尾
                                var sCont = (tData.Setting.MappContentX ?? "").Split(new string[] { "~@~" }, StringSplitOptions.None);
                                if (sCont.Length == 1)
                                    tData.Message.MM_TEXT_CONTENT = string.Join(sNewLine, tData.HighData.List.Select(p => CYCloud.MappEV.Shared.ConvertMappContent(sCont[0], p, p.IsDisable)));
                                else if (sCont.Length == 3)
                                {
                                    tData.Message.MM_TEXT_CONTENT = string.Format("{0}{1}{2}",
                                        sCont[0].Length > 0 ? CYCloud.MappEV.Shared.ConvertMappContent(sCont[0], oFirst) + sNewLine : "",
                                        string.Join(sNewLine, tData.HighData.List.Select(p => CYCloud.MappEV.Shared.ConvertMappContent(sCont[1], p, p.IsDisable))),
                                        sCont[2].Length > 0 ? sNewLine + CYCloud.MappEV.Shared.ConvertMappContent(sCont[2], oFirst) : "");
                                }
                            }
                            else
                                oResult.Error("無需發送資料");
                        }
                        else
                            oResult.Error("無需發送資料");
                    }
                }
                else
                    oResult.Error("查無高階發送設定");
            }
            return tData;
            //return oMessage;
        }

        private List<MappEVInput> GetList(string sType)
        {
            List<string> mList = new List<string>();

            var oList = new List<MappEVInput>();
            if (sType == "E")
            {
                foreach (GridViewRow row in GridView1.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var oData = new MappEVInput { FacName = row.Cells[0].Text, Type = sType };
                        TextBox txtDate = (TextBox)row.FindControl("txtDate");
                        TextBox txtTime = (TextBox)row.FindControl("txtTime");
                        TextBox txtValue1 = (TextBox)row.FindControl("txtValue1");
                        TextBox txtValue2 = (TextBox)row.FindControl("txtValue2");
                        TextBox txtValue3 = (TextBox)row.FindControl("txtValue3");

                        if (txtDate.Text != "0000-00-00")
                        {
                            if (DateTime.TryParse(txtDate.Text + " " + txtTime.Text, out DateTime dTime))
                            {
                                oData.InputTime = dTime;
                                oData.InputSource = txtSource.Text;
                                oData.Value1 = txtValue1.Text;
                                oData.Value2 = txtValue2.Text;
                                oData.Value3 = txtValue3.Text;
                                if (oData.Value1 != "NA" && oData.Value3 != "NA" && double.TryParse(oData.Value1, out double iGal) && double.TryParse(oData.Value3, out _))
                                {
                                    if (iGal < 2.5)
                                        oData.Value2 = "1";
                                    else if (iGal < 8)
                                        oData.Value2 = "2";
                                    else if (iGal < 25)
                                        oData.Value2 = "3";
                                    else if (iGal < 80)
                                        oData.Value2 = "4";
                                    else if (iGal < 400)
                                        oData.Value2 = "5";
                                    else
                                        oData.Value2 = ">6";

                                    txtValue2.Text = oData.Value2;

                                    var oSetting = CYCloud.Global.MappEV.List.FirstOrDefault(p => p.Type == sType && p.Code == oData.FacName);
                                    if (oSetting != null) { oData.CimLevel = oSetting.CimLevel; }
                                }
                                else
                                    mList.Add(string.Format("{0}-[地震最大gal數]或[地震持續時間]格式錯誤", oData.FacName));
                            }
                            else
                                mList.Add(string.Format("{0}-[觸發日期]或[觸發時間]格式錯誤", oData.FacName));
                        }
                        else
                            oData.IsDisable = true;

                        oList.Add(oData);
                    }
                }
            }
            else
            {
                foreach (GridViewRow row in GridView2.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var oData = new MappEVInput { FacName = row.Cells[0].Text, Type = sType };
                        TextBox txtDate = (TextBox)row.FindControl("txtDate");
                        TextBox txtTime = (TextBox)row.FindControl("txtTime");
                        TextBox txtValue1 = (TextBox)row.FindControl("txtValue1");
                        TextBox txtValue2 = (TextBox)row.FindControl("txtValue2");
                        TextBox txtValue3 = (TextBox)row.FindControl("txtValue3");
                        TextBox txtValue4 = (TextBox)row.FindControl("txtValue4");
                        TextBox txtValue5 = (TextBox)row.FindControl("txtValue5");

                        if (txtDate.Text != "0000-00-00")
                        {
                            if (DateTime.TryParse(txtDate.Text + " " + txtTime.Text, out DateTime dTime))
                            {
                                oData.InputTime = dTime;
                                oData.InputSource = txtSource.Text;
                                oData.Value1 = txtValue1.Text;
                                oData.Value2 = txtValue2.Text;
                                oData.Value3 = txtValue3.Text;
                                oData.Value4 = txtValue4.Text;
                                oData.Value5 = txtValue5.Text;

                                if (oData.Value1 != "NA" && oData.Value5 != "NA" && double.TryParse(oData.Value1, out double iValue1) && double.TryParse(oData.Value5, out double iValue5))
                                {
                                    if (iValue1 >= 90 || iValue5 <= 50)
                                        oData.Value4 = "A";
                                    else if (iValue1 > 80 && iValue5 > 1000)
                                        oData.Value4 = "D";
                                    else
                                    {
                                        if (iValue1 >= 50 && iValue1 < 70 && iValue5 >= 50 && iValue5 < 200)
                                            oData.Value4 = "B";
                                        if (iValue1 >= 80 && iValue1 < 90 && iValue5 >= 50 && iValue5 < 200)
                                            oData.Value4 = "B";
                                        if (iValue1 >= 50 && iValue1 < 90 && iValue5 >= 50 && iValue5 < 200)
                                            oData.Value4 = "B";
                                        if (iValue1 >= 70 && iValue1 < 90 && iValue5 >= 200 && iValue5 < 600)
                                            oData.Value4 = "B";
                                        if (iValue1 >= 80 && iValue1 < 90 && iValue5 >= 600 && iValue5 < 10000)
                                            oData.Value4 = "B";

                                        if (iValue1 >= 0 && iValue1 < 40 && iValue5 >= 50 && iValue5 < 1000)
                                            oData.Value4 = "C";
                                        if (iValue1 >= 50 && iValue1 < 80 && iValue5 >= 200 && iValue5 < 1000)
                                            oData.Value4 = "C";
                                        if (iValue1 >= 70 && iValue1 < 80 && iValue5 >= 600 && iValue5 < 10000)
                                            oData.Value4 = "C";
                                    }
                                    txtValue4.Text = oData.Value4;

                                    var oSetting = CYCloud.Global.MappEV.List.FirstOrDefault(p => p.Type == sType && p.Code == oData.FacName);
                                    if (oSetting != null) { oData.CimLevel = oSetting.CimLevel; }
                                }
                                else
                                    mList.Add(string.Format("{0}-[壓降剩餘電量(%)]或[持續秒數]格式錯誤", oData.FacName));
                            }
                            else
                                mList.Add(string.Format("{0}-[觸發日期]或[觸發時間]格式錯誤", oData.FacName));
                        }
                        oList.Add(oData);
                    }
                }
            }

            if (mList.Count > 0)
                oResult.Error(string.Join(";", mList));

            return oList;
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {

            GridView1.Visible = false;
            GridView2.Visible = false;
            btnSend.Enabled = false;

            var sList = dDB.QueryList<MappEVInputEx>(@"select Code as FacName,Type,FacArea from MappEV where FacArea=@FacArea and Type=@Type and IsTop=0 order by Code", 
                new { FacArea = ddlAreaQ.SelectedValue, Type = ddlTypeQ.SelectedValue });

            if (sList != null)
            {
                if (ddlTypeQ.SelectedValue == "E")
                {
                    GridView1.DataSource = sList;
                    GridView1.DataBind();
                    GridView1.Visible = true;
                }
                else
                {
                    GridView2.DataSource = sList;
                    GridView2.DataBind();
                    GridView2.Visible = true;
                }

                hidData.Value = ddlTypeQ.SelectedValue + "," + ddlAreaQ.SelectedValue;
                btnSend.Enabled = sList.Count() > 0;
            }
        }

        class MappEVInputEx : MappEVInput
        {
            public string DateStr { get; set; } = "0000-00-00";
            public string TimeStr { get; set; } = "00:00:00";
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {
            TmpData tData = Preview();
            if (!oResult.Success)
                ShowResult("", false, false);
            else if (tData.Message != null)
            {
                txtPreview.Text = tData.Message.MM_TEXT_CONTENT;
                Panel1.Visible = true;
            }
        }

        class TmpData
        {
            public CYCloud.MappEV.Data.MappEVHigh HighData { get; set; }
            public CYCloud.MappEV.Data.MappEVSetting Setting { get; set; }
            public CYCloud.MappEV.Data.MappEVMessage Message { get; set; }
        }
    }
}