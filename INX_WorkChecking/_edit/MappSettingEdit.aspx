<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappSettingEdit.aspx.cs" Inherits="WebApp._edit.MappSettingEdit" %>

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

        function checkDelete() {
            if ($("#<%=hidKey.ClientID%>").val().length != 0 || confirm('同時會刪除相關聯資料，確定要刪除此筆資料？')) {
                return reConfirm(function () { $("#<%=btnDelete.ClientID%>").trigger("click");});
            } else {
                return false;
            }
        }

        function reConfirm(callback) {
            if ($("#<%=hidKey.ClientID%>").val().length == 0) {
                ReLogin(function (k) {
                    if (k.length > 0) {
                        $("#<%=hidKey.ClientID%>").val(k);
                        callback();
                        $("#<%=btnDelete.ClientID%>").trigger("click");
                    }
                });
                return false;
            }
        }

    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <%--<asp:HiddenField ID="hidID" runat="server" />--%><asp:HiddenField ID="hidKey" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">MAPP發送設定</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">設定名稱</th>
                <td colspan="2">
                    <asp:TextBox ID="txtName" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">設定說明</th>
                <td colspan="2">
                    <asp:TextBox ID="txtDesc" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">設定分類</th>
                <td colspan="2">
                    <asp:DropDownList ID="ddlType" runat="server" DataTextField="Name" DataValueField="ID" Width="95%"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">發送類別</th>
                <td colspan="2">
                    <asp:DropDownList ID="ddlSendType" runat="server" Width="95%">
                        <asp:ListItem Text="團隊" Value="1"></asp:ListItem>
                        <asp:ListItem Text="交談室" Value="2"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">Team+ 帳號</th>
                <td colspan="2">
                    <asp:TextBox ID="txtAccount" runat="server" Width="95%" MaxLength="25"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">團隊編號</th>
                <td colspan="2">
                    <asp:TextBox ID="txtTeamSN" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">API KEY</th>
                <td colspan="2">
                    <asp:TextBox ID="txtApiKey" runat="server" Width="95%" MaxLength="250"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">使用部門</th>
                <td colspan="2">
                    <uc:ucDept ID="ucDept" runat="server" isShowTop="true" isShowAll="false" isNoInclude="true" />
                </td>
            </tr>
<%--            <tr>
                <th class="label write">隔離轉發設定</th>
                <td colspan="2">
                    <asp:DropDownList ID="ddlTransID" runat="server" Width="95%" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>--%>
            <tr>
                <th class="label write">預設逾時未解隔<br/>通知設定</th>
                <td colspan="2">
                    <asp:RadioButtonList ID="rblDefaultRemind" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Text="是" Value="1"></asp:ListItem>
                        <asp:ListItem Text="否" Value="0" Selected="True"></asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
            <tr>
                <th class="label write">是否停用</th>
                <td>
                    <asp:RadioButtonList ID="rblStop" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Text="是" Value="Y"></asp:ListItem>
                        <asp:ListItem Text="否" Value="N" Selected="True"></asp:ListItem>
                    </asp:RadioButtonList>
                </td>
                <td style="text-align:right;">
                    <asp:Button ID="btnDelete" runat="server" Text="刪除" BackColor="Red" OnClientClick="return checkDelete()" Visible="false" OnClick="btnDelete_Click" />
                </td>
            </tr>
            <tr>
                <th class="label read">最後更新人員</th>
                <td colspan="2">
                    <asp:Label ID="lblUpdateUser" runat="server" Text=""></asp:Label>
                    <asp:Label ID="lblUpdateTime" runat="server" Text=""></asp:Label>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
