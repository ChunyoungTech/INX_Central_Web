<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SysTest.aspx.cs" Inherits="WebApp._sys.SysTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="TextBox1" runat="server" Text="1000"></asp:TextBox>
            <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" /><br />
            <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
        </div>


        <div>
            <strong>測試Cache</strong>
            <asp:Button ID="btnCache" runat="server" Text="Cache Clear" OnClick="btnCache_Click" />
        </div>
    </form>
</body>
</html>
