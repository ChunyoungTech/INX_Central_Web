using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using cyc.Page;

namespace WebApp._alarm
{
    public partial class MappSetting : BasePage
    {
        [WebMethod(EnableSession = true)]
        public static bool SaveSetting(int mappId, int logMappId, string logSendTime)
        {
            try
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn())
                {
                    cyc.DB.SqlDBPara bPara = new cyc.DB.SqlDBPara
                    {
                        Command = $@"
UPDATE OPC_Server
SET MAppGroupId =@MAppGroupId,
logMAppGroupId =@logMAppGroupId,
logTime =@logTime
"
                    };
                    bPara.Parameter.Clear();
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("MAppGroupId", mappId));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("logMAppGroupId", logMappId));
                    bPara.Parameter.Add(new System.Data.SqlClient.SqlParameter("logTime", logSendTime));
                    oDB.QueryDataTable(bPara);

                }

            }
            catch (Exception e)
            {
                cyc.Log.WriteSysErrorLog($"儲存MAPP警報設定錯誤：{e.Message} {e.StackTrace}");
                return false;
            }
            return true;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bPara.Command = "select MS_SEQ_ID as ID,MS_SYS_NAME as NAME,MS_SYS_DESC+case when MS_SYS_STOP='Y' then '(停用)' else '' end as Name,MS_SYS_NAME+case when MS_SYS_STOP='Y' then '(停用)' else '' end as Name2 from MappSetting";
                using (DataTable oDT = dDB.QueryDataTable(bPara))
                {
                    ddlCP_MAPP_TYPE.DataSource = oDT;
                    ddlCP_MAPP_TYPE.DataValueField = "ID";
                    ddlCP_MAPP_TYPE.DataTextField = "NAME";
                    ddlCP_MAPP_TYPE.DataBind();

                    log_MAPP_TYPE.DataSource = oDT;
                    log_MAPP_TYPE.DataValueField = "ID";
                    log_MAPP_TYPE.DataTextField = "NAME";
                    log_MAPP_TYPE.DataBind();
                }
            }

            bPara.Command = "SELECT TOP 1  * FROM opc_server";
            using (DataTable oDT = dDB.QueryDataTable(bPara))
            {

                if (ddlCP_MAPP_TYPE.Items.FindByValue(oDT.Rows[0]["MAppGroupId"].ToString()) != null) { ddlCP_MAPP_TYPE.SelectedValue = oDT.Rows[0]["MAppGroupId"].ToString(); }
                if (log_MAPP_TYPE.Items.FindByValue(oDT.Rows[0]["logMAppGroupId"].ToString()) != null) { log_MAPP_TYPE.SelectedValue = oDT.Rows[0]["logMAppGroupId"].ToString(); }
                logSendTime.Text = oDT.Rows[0]["logTime"].ToString().Substring(0, 5);
            }

        }
    }
}