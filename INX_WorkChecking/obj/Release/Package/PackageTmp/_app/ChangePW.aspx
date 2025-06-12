<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Main.Master" AutoEventWireup="true" CodeBehind="ChangePW.aspx.cs" Inherits="WebApp._app.ChangePW" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        #changePW {
            padding-top: 3em;
        }

            #changePW th {
                height: 2em;
                width: 30%;
                background-color:#1C5E55;
                color:white;
                text-align: right;
            }
    </style>
    <script type="text/javascript">
        function checkData() {
            if ($("#<%=txtPWOLD.ClientID%>").val() == "" || $("#<%=txtPWNEW.ClientID%>").val() == "" || $("#<%=txtPWNEW2.ClientID%>").val() == "") {
                alert("所有欄位均不可空白");
                return false;
            } else if ($("#<%=txtPWNEW.ClientID%>").val() != $("#<%=txtPWNEW2.ClientID%>").val()) {
                alert("[新密碼]與[確認新密碼]必須相同");
                return false;
            }
        }
        function ClientAlert(msg) {
            msg = msg.replace(";", "\n");
            alert(msg);
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="changePW">
        <table style="margin: 0 auto; width: 35em;">
            <tr>
                <th>原密碼</th>
                <td>
                    <asp:TextBox ID="txtPWOLD" TextMode="Password" runat="server" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>新密碼</th>
                <td>
                    <asp:TextBox ID="txtPWNEW" TextMode="Password" runat="server" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>確認新密碼</th>
                <td>
                    <asp:TextBox ID="txtPWNEW2" TextMode="Password" runat="server" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="center">
                    <asp:Button ID="btnConfirm" runat="server" Text="確定修改" OnClientClick="return checkData()" OnClick="btnConfirm_Click" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
