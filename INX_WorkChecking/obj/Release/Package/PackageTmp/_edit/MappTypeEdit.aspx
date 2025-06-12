<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappTypeEdit.aspx.cs" Inherits="WebApp._edit.MappTypeEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[設定名稱]不可空白 \n";
            }
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">MAPP設定分類</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">分類名稱</th>
                <td>
                    <asp:TextBox ID="txtName" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">分類排序</th>
                <td>
                    <asp:TextBox ID="txtSort" runat="server" Width="95%" MaxLength="3"></asp:TextBox>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
