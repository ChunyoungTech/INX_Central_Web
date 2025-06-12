using cyc.Data;
using cyc.Page;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static NPOI.HSSF.Util.HSSFColor;

namespace WebApp._idb
{
    public partial class TransferIndexEdit : BasePageSub
    {
        int iID = 0;
//        protected override void OnInit(EventArgs e)
//        {
//            base.OnInit(e);
//            if (!IsPostBack)
//            {
//                ddlFac.DataSource = dDB.QueryList<BaseObj>(@"
//select distinct C.SeqID as ID,C.FacName as Code from (
//	select distinct ID3 from View_SysDeptLevel where ID1=@ID or ID2=@ID or ID3=@ID or ID4=@ID or ID5=@ID
//)A inner join View_SysDeptLevel B on A.ID3=B.ID inner join IDBFacData C on B.Code=C.FacName", new { ID = bUser.User.DeptLevel });
//                ddlFac.DataBind();
//            }
//        }

        protected override void OnLoad(EventArgs e)
        {
            iID = Convert.ToInt32(ViewState["ID"] ?? Request.QueryString["pa"]);
            base.OnLoad(e);
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;TransferIndex.aspx",
            Confirm = btnConfirm,
            Parameter = "pa",
            AppID = new int[] { 32 },
            SubID = new int[] { 27 },
        };
        protected override void LoadData()
        {
            if (iID != 0)
            {
                var oData = dDB.QueryOne<TagData>("select * from IFMTransferIndex where SEQ_ID=@ID", new { ID = iID });
                if (oData != null)
                {
                    txt_ind_no.Text = oData.ind_no.ToString();
                    txt_ind_source_note.Text = oData.ind_source_note;
                    txt_scada_tagname.Text = oData.scada_tagname;
                    txt_ind_tagname.Text = oData.ind_tagname;
                    txt_ind_fac.Text = oData.ind_fac;
                    txt_ind_fab.Text = oData.ind_fab;
                    txt_ind_System.Text = oData.ind_System;
                    txt_ind_unit.Text = oData.ind_unit;
                    txt_ind_eqp_group.Text = oData.ind_eqp_group;
                    txt_ind_section.Text = oData.ind_section;
                    txt_ind_Common.Text = oData.ind_Common;
                    txt_ind_level.Text = oData.ind_level;
                    txt_ind_priority.Text = oData.ind_priority;
                    txt_ind_col_index.Text = oData.ind_col_index;
                    txt_ind_row_index.Text = oData.ind_row_index;
                }
                else
                    oResult.Error("查無資料");
            }
        }
        protected override void SaveCheck()
        {
            string sMsg = "";
            if (string.IsNullOrWhiteSpace(txt_ind_tagname.Text))
                sMsg += "[ind_tagname]不可空白;";
            else if (dDB.QueryOne<BaseObj>("select SEQ_ID as ID from IFMTransferIndex where ind_tagname=@Name and scada_tagname=@scada_tagname and ind_fac=@ind_fac and SEQ_ID<>@ID", new { Name = txt_ind_tagname.Text, ID = iID }) != null)
                sMsg += "[ind_tagname]已重複;";
            else if (dDB.QueryOne<BaseObj>("select top 1 ID from TagData where Tag_Name=@Name", new { Name = txt_scada_tagname.Text }) == null)
                sMsg += "[scada_tagname]不存在於[TagData資料表];";
            //else if (dDB.QueryOne<BaseObj>("select top 1 ID from TagData where Tag_Name=@Name", new { Name = txt_ind_tagname.Text }) == null)
            //    sMsg += "[ind_tagname]不存在於[TagData資料表];";

            //if (string.IsNullOrEmpty(ddlSys.SelectedValue))
            //    sMsg += "[系統別]不可空白";
            if (!string.IsNullOrWhiteSpace(txt_ind_no.Text) && !cyc.Shared.Check.IsInteger(txt_ind_no.Text.Trim()))
                sMsg += "[ind_no]必須是整數;";
            //if (!string.IsNullOrWhiteSpace(txtHiLimit.Text) && !cyc.Shared.Check.IsNumeric(txtHiLimit.Text.Trim()))
            //    sMsg += "[HI警報值]必須是數值;";
            //if (!string.IsNullOrWhiteSpace(txtLoLimit.Text) && !cyc.Shared.Check.IsNumeric(txtLoLimit.Text.Trim()))
            //    sMsg += "[LO警報值]必須是數值;";
            //if (!string.IsNullOrWhiteSpace(txtLoLoLimit.Text) && !cyc.Shared.Check.IsNumeric(txtLoLoLimit.Text.Trim()))
            //    sMsg += "[LOLO警報值]必須是數值;";
            if (sMsg.Length > 0)
                oResult.Error(sMsg);
        }
        protected override void SaveData()
        {
            var oData = new TagData
            {
                SEQ_ID = iID,
                ind_no = Convert.ToInt32(txt_ind_no.Text),
                ind_source_note = txt_ind_source_note.Text,
                scada_tagname = txt_scada_tagname.Text,
                ind_tagname = txt_ind_tagname.Text,
                ind_fac = txt_ind_fac.Text,
                ind_fab = txt_ind_fab.Text,
                ind_System = txt_ind_System.Text,
                ind_unit = txt_ind_unit.Text,
                ind_eqp_group = txt_ind_eqp_group.Text,
                ind_section = txt_ind_section.Text,
                ind_Common = txt_ind_Common.Text,
                ind_level = txt_ind_level.Text,
                ind_priority = txt_ind_priority.Text,
                ind_col_index = txt_ind_col_index.Text,
                ind_row_index = txt_ind_row_index.Text
            };
            oData.SEQ_ID = dDB.Execute(cyc.DB.Shared.GetEditSQL("IFMTransferIndex", "ind_no,ind_source_note,scada_tagname,ind_tagname,ind_fac,ind_fab,ind_System,ind_unit,ind_eqp_group,ind_section,ind_Common,ind_level,ind_priority,ind_col_index,ind_row_index;;SEQ_ID", iID == 0), oData, iID);
        }
        #endregion

        class TagData
        {
            public int SEQ_ID { get; set; }
            public int ind_no { get; set; }
            public string ind_source_note { get; set; }
            public string scada_tagname { get; set; }
            public string ind_tagname { get; set; }
            public string ind_fac { get; set; }
            public string ind_fab { get; set; }
            public string ind_System { get; set; }
            public string ind_unit { get; set; }
            public string ind_eqp_group { get; set; }
            public string ind_section { get; set; }
            public string ind_Common { get; set; }
            public string ind_level { get; set; }
            public string ind_priority { get; set; }
            public string ind_col_index { get; set; }
            public string ind_row_index { get; set; }
        }
    }
}