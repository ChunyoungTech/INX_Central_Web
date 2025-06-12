using CYCloud.MappEV.Data;
using pin.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._mapp
{
    public partial class MappEVLatest : BasePageGridMulti
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

        protected override GridPageSetting SetPageSetting()
        {
            return new GridPageSetting() { Option = new GridOption[] { new GridOption { AutoBind = false, Grid = null, Pager = null, Query = null, Refresh = null } } };
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            GridView1.Visible = false;
            GridView2.Visible = false;
            btnSend.Enabled = false;
            txtSource.Text = "";

            var xData = bDB.QueryMultiple(@"
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
                                txtSource.Text = qData.InputSource;
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
                //else
                //    oResult.Error("查無資料");
            }
            if (!oResult.Success) { ShowResult("", false, false); }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            MappEVMessage oMessage = Preview();
            if (!oResult.Success)
                ShowResult("", false, false);
            else if (oMessage != null)
            {
                bDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT,MM_TYPE,UPDATE_USER) values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT,@MM_TYPE,@UPDATE_USER)", oMessage);
                ShowResult("已加入發送排程", false, false);
            }
        }

        private MappEVMessage Preview()
        {
            MappEVMessage oMessage = null;
            string[] sKey = hidData.Value.Split(',');
            if (sKey.Length == 2)
            {
                //高階設定
                var hSetting = CYCloud.Global.MappEV.List.FirstOrDefault(p => p.Type == sKey[0] && p.FacArea == sKey[1] && p.IsTop);
                if (hSetting != null)
                {
                    var oList = GetList(sKey[0]);

                    if (oResult.Success)
                    {
                        if (oList.Count > 0)
                        {
                            //第1筆 非Null的資料
                            var first = oList.FirstOrDefault(p => p.Value1 != "NA");
                            if (first != null)
                            {
                                //查詢正式群組是否有隔離
                                bool bDisable = bDB.QueryOne<int>("select count(1) from MappDisable where MS_SEQ_ID=@ID and MD_STOP_TIME is null and @Time between MD_DATE_START and MD_DATE_END", new { ID = hSetting.NormalID, Time = DateTime.Now }) > 0;
                                oMessage = new MappEVMessage { MS_SYS_NAME = bDisable ? hSetting.DisableCode : hSetting.NormalCode, MM_TYPE = 'M', UPDATE_USER = bUser.User.ID };
                                //MAPP主旨
                                oMessage.MM_SUBJECT = CYCloud.MappEV.Shared.ConvertMappContent(hSetting.MappSubject, first);
                                //MAPP內容 - 標頭+內容+結尾
                                var sCont = (hSetting.MappContent ?? "").Split(new string[] { "~@~" }, StringSplitOptions.None);
                                if (sCont.Length == 1)
                                    oMessage.MM_TEXT_CONTENT = string.Join(System.Environment.NewLine, oList.Select(p => CYCloud.MappEV.Shared.ConvertMappContent(sCont[0], p, p.IsDisable)));
                                else if (sCont.Length == 3)
                                {
                                    oMessage.MM_TEXT_CONTENT = string.Format("{0}{1}{2}",
                                        sCont[0].Length > 0 ? CYCloud.MappEV.Shared.ConvertMappContent(sCont[0], first) + System.Environment.NewLine : "",
                                        string.Join(System.Environment.NewLine, oList.Select(p => CYCloud.MappEV.Shared.ConvertMappContent(sCont[1], p, p.IsDisable))),
                                        sCont[2].Length > 0 ? System.Environment.NewLine + CYCloud.MappEV.Shared.ConvertMappContent(sCont[2], first) : "");
                                }

                                //bDB.Execute("insert into MappMessage (MS_SYS_NAME,MM_CONTENT_TYPE,MM_TEXT_CONTENT,MM_SUBJECT,MM_TYPE,UPDATE_USER) values (@MS_SYS_NAME,@MM_CONTENT_TYPE,@MM_TEXT_CONTENT,@MM_SUBJECT,@MM_TYPE,@UPDATE_USER)", oMessage);
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
            return oMessage;
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
                                }
                                else
                                    mList.Add(string.Format("{0}-[地震最大gal數]或[地震持續時間]格式錯誤", oData.FacName));
                            }
                            else
                                mList.Add(string.Format("{0}-[觸發日期]或[觸發時間]格式錯誤", oData.FacName));
                        }

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

            var sList = bDB.QueryList<MappEVInputEx>(@"select Code as FacName,Type,FacArea from MappEV where FacArea=@FacArea and Type=@Type and IsTop=0 order by Code",
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
            MappEVMessage oMessage = Preview();
            if (!oResult.Success)
                ShowResult("", false, false);
            else if (oMessage != null)
            {
                txtPreview.Text = oMessage.MM_TEXT_CONTENT;
                Panel1.Visible = true;
            }
        }
    }
}