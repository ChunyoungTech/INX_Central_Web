<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="TagDataEdit.aspx.cs" Inherits="WebApp._idb.TagDataEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[點位名稱]不可空白 \n";
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
    <li class='tab'><a href="#tabs-1">資料點</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">點位名稱</th>
                <td colspan="3">
                    <asp:TextBox ID="txtName" runat="server" Width="98%" MaxLength="200"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">點位描述</th>
                <td colspan="3">
                    <asp:TextBox ID="txtDesc" runat="server" Width="98%" MaxLength="200"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">單位</th>
                <td>
                    <asp:TextBox ID="txtUnit" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
                <th class="label write">類型</th>
                <td>
                    <asp:DropDownList ID="ddlType" runat="server" Width="95%">
                        <asp:ListItem Text="AI" Value="AI"></asp:ListItem>
                        <asp:ListItem Text="DI" Value="DI"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">廠別</th>
                <td>
                    <asp:DropDownList ID="ddlFac" runat="server" Width="95%" DataTextField="Code" DataValueField="ID" OnSelectedIndexChanged="ddlFac_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                </td>
                <th class="label must">系統別</th>
                <td>
                    <asp:DropDownList ID="ddlSys" runat="server" Width="95%" DataTextField="Code" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label write">HIHI警報值</th>
                <td>
                    <asp:TextBox ID="txtHiHiLimit" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
                <th class="label write">HI警報值</th>
                <td>
                    <asp:TextBox ID="txtHiLimit" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">LOLO警報值</th>
                <td>
                    <asp:TextBox ID="txtLoLoLimit" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
                <th class="label write">LO警報值</th>
                <td>
                    <asp:TextBox ID="txtLoLimit" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
