<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPatrol.aspx.cs" Inherits="WebApp._test.TestPatrol" %>

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
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:DropDownList ID="ddlSetting" runat="server" DataTextField="Name" DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="ddlSetting_SelectedIndexChanged"></asp:DropDownList><br />
                    <asp:DropDownList ID="ddlPlace" runat="server" DataTextField="Name" DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="ddlPlace_SelectedIndexChanged"></asp:DropDownList><br />
                    <asp:DropDownList ID="ddlEquip" runat="server" DataTextField="Name" DataValueField="ID"></asp:DropDownList><br />

                    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox><br />
                    <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </form>
</body>
</html>
