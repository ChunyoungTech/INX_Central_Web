<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestInfluxDB.aspx.cs" Inherits="WebApp._test.TestInfluxDB" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtTime" runat="server"></asp:TextBox>
            <asp:Button ID="btnRun" runat="server" Text="Run" OnClick="btnRun_Click" /><br />
            <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>
        </div>

        <div style="padding-top:1em;">
            <asp:Button ID="btnTagName" runat="server" Text="TagName" OnClick="btnTagName_Click" /><br />
            <asp:TextBox ID="TextBox1" runat="server" TextMode="MultiLine" Rows="10" Width="99%"></asp:TextBox>
        </div>

        <div style="padding-top:1em;">
            <asp:Button ID="btnMeasurement" runat="server" Text="Measurement" OnClick="btnMeasurement_Click" /><br />
            <asp:TextBox ID="txtMeasurement" runat="server" TextMode="MultiLine" Rows="10" Width="99%"></asp:TextBox>
        </div>
    </form>
</body>
</html>
