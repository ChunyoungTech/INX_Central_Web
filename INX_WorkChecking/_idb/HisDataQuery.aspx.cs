using cyc.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp._idb
{
    public partial class HisDataQuery : BasePageGrid
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string xPath = cyc.Shared.SysQuery.GetSysSettingValue("HisDataQuery");
            if (string.IsNullOrEmpty(xPath))
                lblMessage.Text = "[中央歷史資料查詢]路徑尚未設定";
            else
                Response.Redirect($"{xPath}?LoginToken={bUser.User.LoginToken}");
        }

        protected override DataTable QuerySourceData(int idx) => null;

        protected override GridPageOption SetPageSetting() => new GridPageOption { CheckOpen = "" };
    }
}