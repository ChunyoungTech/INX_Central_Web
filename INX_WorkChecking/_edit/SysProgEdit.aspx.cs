using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;
using cyc.Page;

namespace WebApp._edit
{
    public partial class SysProgEdit : cyc.Page.BasePageSub
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!IsPostBack)
            {
                this.ddlDir.DataSource = cyc.Global.SysDir.List;
                this.ddlDir.DataBind();
            }
        }

        #region #繼承
        protected override SubPageOption SetPageOption() => new SubPageOption()
        {
            CheckOpen = "open.aspx;SysProgEdit.aspx",
            Confirm = btnConfirm,
            Parameter = "pa"
        };
        protected override void LoadData()
        {
            this.hidID.Value = Request.QueryString["pa"].ToString();
            var prog = cyc.Global.SysProg.List.FirstOrDefault(p => p.ID == Convert.ToInt16(Request.QueryString["pa"]));
            if (prog != null)
            {
                txtName.Text = prog.Name;
                ddlDir.SelectedValue = prog.DirID.ToString();
                chkEnabled.Checked = prog.Enabled;
                txtSeq.Text = prog.Seq.ToString();
            }
            //else
            //{
            //    oResult.Error("查無資料");
            //}
        }
        protected override void SaveCheck()
        {
            string sMsg = "";
            if (txtName.Text.Trim().Length == 0)
                sMsg += "[功能名稱]不可空白;";
            if (!cyc.Shared.Check.IsInteger(txtSeq.Text.Trim()))
                sMsg += "[排序]必須是數字且不可空白;";
            if (sMsg.Length > 0)
                oResult.Error(sMsg);
        }
        protected override void SaveData()
        {
            var prog = new ProgEdit()
            {
                ID = Convert.ToInt32(this.hidID.Value),
                Name = txtName.Text.Trim(),
                Enabled = chkEnabled.Checked,
                DirID = Convert.ToInt32(ddlDir.SelectedItem.Value),
                DirName = ddlDir.SelectedItem.Text,
                Seq = Convert.ToInt32(txtSeq.Text),
                UpdUser = bUser.User.ID
            };

            using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
            {
                try
                {
                    oDB.Execute("update SysProg set Name=@Name,DirID=@DirID,Enabled=@Enabled,Seq=@Seq,u_user=@UpdUser,u_date=getdate() where id=@ID", prog);

                    var data = cyc.Global.SysProg.List.FirstOrDefault(p => p.ID == Convert.ToInt32(this.hidID.Value));
                    if (data != null)
                    {
                        prog.Folder = data.Folder;
                        prog.Path = data.Path;
                        cyc.Data.Shared.CopyObjectValues<cyc.Data.SysProg>(prog, data);
                    }
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex.Message + ":" + ex.StackTrace, oResult); }
            }
        }
        #endregion

        class ProgEdit : cyc.Data.SysProg
        {
            public int UpdUser { get; set; }
            //public DateTime UpdDate { get; set; }
        }
    }
}