using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;

namespace WebApp._test
{
    public partial class TestDataBase : System.Web.UI.Page
    {
        cyc.Data.ExeResult oResult = new cyc.Data.ExeResult();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GetConnection();
            }
        }

        private void GetConnection()
        {
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings[i];
                ddlConnection.Items.Add(new ListItem(cs.Name, cs.ConnectionString));
            }

            if (ddlConnection.Items.Count > 0)
            {  
                ddlConnection.SelectedIndex = 0;
                GetDataTable();
            }
        }

        private void GetDataTable()
        {
            if (ddlConnection.SelectedIndex >= 0)
            {
                GridView1.DataSource = null;
                GridView1.DataBind();

                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, ddlConnection.SelectedValue))
                {
                    var dList = oDB.QueryList<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_TYPE='BASE TABLE'");
                    if (oResult.Success)
                    {
                        ddlDataTable.DataSource = dList;
                        ddlDataTable.DataBind();
                        ddlDataTable.Items.Insert(0, "");
                    }
                }
            }
        }

        protected void ddlDataTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlConnection.SelectedIndex >= 0 && !string.IsNullOrEmpty(ddlDataTable.SelectedValue))
            {
                using (cyc.DB.SqlDapperConn oDB = new cyc.DB.SqlDapperConn(oResult, ddlConnection.SelectedValue))
                {
                    var xTable = oDB.QueryDataTable($"select * from [{ddlDataTable.SelectedValue}] where 1=0");
                    if (oResult.Success)
                    {
                        List<ColumnData> oList = new List<ColumnData>();
                        foreach (DataColumn oData in xTable.Columns)
                        {
                            oList.Add(new ColumnData { Name = oData.ColumnName, DataType = oData.DataType, TypeName = oData.DataType.Name, AllowDBNull = oData.AllowDBNull, MaxLength = oData.MaxLength, AutoIncrement = oData.AutoIncrement, DefaultValue = oData.DefaultValue.ToString() });
                        }
                        GridView1.DataSource = oList;
                        GridView1.DataBind();
                    }
                }
            }
        }

        protected void ddlConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetDataTable();
        }
    }

    class ColumnData
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public string TypeName { get; set; }
        public int MaxLength { get; set; }
        public bool AllowDBNull { get; set; }
        public bool AutoIncrement { get; set; }
        public string DefaultValue { get; set; }
    }
}