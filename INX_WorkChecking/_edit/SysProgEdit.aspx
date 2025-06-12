<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="SysProgEdit.aspx.cs" Inherits="WebApp._edit.SysProgEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[功能名稱]不可空白 \n";
            }
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">系統功能</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label write">啟用功能</th>
                <td>
                    <asp:CheckBox ID="chkEnabled" runat="server" />
                </td>
            </tr>
            <tr>
                <th class="label must">功能名稱</th>
                <td>
                    <asp:TextBox ID="txtName" runat="server" Width="90%" MaxLength="20"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">分類</th>
                <td>
                    <asp:DropDownList ID="ddlDir" runat="server" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">排序</th>
                <td>
                    <asp:TextBox ID="txtSeq" runat="server" Text="99"></asp:TextBox>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
