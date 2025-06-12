<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestAutoCheckIn.aspx.cs" Inherits="WebApp._test.TestAutoCheckIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="margin:1em;">
            日期：<asp:TextBox ID="TextBox1" runat="server" Width="8em"></asp:TextBox>
            <br />
            <asp:Button ID="Button1" runat="server" Text="CheckIn" OnClick="Button1_Click" />
            <asp:Button ID="Button2" runat="server" Text="CheckOut" OnClick="Button2_Click" />
        </div>
        <div style="margin:1em;">
            日期：<asp:TextBox ID="TextBox2" runat="server" Width="8em"></asp:TextBox>
            <br />
            <asp:Button ID="Button3" runat="server" Text="每小時" OnClick="Button3_Click" />
        </div>
        <div style="margin:1em;">
            日期：<asp:TextBox ID="TextBox3" runat="server" Width="8em"></asp:TextBox>
            <br />
            <asp:Button ID="Button4" runat="server" Text="施工管理統計表發送" OnClick="Button4_Click" />
        </div>
    </form>
</body>
</html>
