<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestIFP.aspx.cs" Inherits="WebApp.TestIFP" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="display:none">
            Device：<asp:TextBox ID="TextBox1" runat="server" Text="aaa"></asp:TextBox><br />
            Message：<asp:TextBox ID="TextBox2" runat="server" Text="GGG"></asp:TextBox><br />
            <asp:Button ID="Button1" runat="server" Text="Send" OnClick="Button1_Click" />
        </div>
        <div style="display:none">
            Device：<asp:TextBox ID="TextBox3" runat="server" Text="bbb"></asp:TextBox><br />
            Message：<asp:TextBox ID="TextBox4" runat="server" Text="QQQ"></asp:TextBox><br />
            <asp:Button ID="Button4" runat="server" Text="Send" OnClick="Button4_Click" />
        </div>
        <br />
        <div>
            <asp:Button ID="Button5" runat="server" Text="Show" OnClick="Button5_Click" />
            <asp:Button ID="Button6" runat="server" Text="Send" OnClick="Button6_Click" />
            <asp:Button ID="Button2" runat="server" Text="List" OnClick="Button2_Click" />
            <asp:Button ID="Button3" runat="server" Text="Clear" OnClick="Button3_Click" /><br />
            <asp:ListBox ID="ListBox1" runat="server" Width="300px" Height="600px"></asp:ListBox>
        </div>
    </form>
</body>
</html>
