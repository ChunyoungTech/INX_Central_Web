<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDataBase.aspx.cs" Inherits="WebApp._test.TestDataBase" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div>
                        連線：<asp:DropDownList ID="ddlConnection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlConnection_SelectedIndexChanged"></asp:DropDownList>
                        資料表：<asp:DropDownList ID="ddlDataTable" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDataTable_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                    <div>
                        <asp:GridView ID="GridView1" runat="server">

                        </asp:GridView>
                    </div>
                    <div>
                        <asp:Literal ID="ltlGrid" runat="server"></asp:Literal>
                    </div>
                    <div>
                        <asp:Literal ID="ltlEdit" runat="server"></asp:Literal>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
