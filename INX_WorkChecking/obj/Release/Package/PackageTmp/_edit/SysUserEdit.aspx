<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="SysUserEdit.aspx.cs" Inherits="WebApp._edit.SysUserEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtName.ClientID %>").val() == "" || $("#<%=txtCode.ClientID %>").val() == "") {
                msg += "所有欄位均不可空白 \n";
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
    <li class='tab'><a href="#tabs-1">人員</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">姓名</th>
                <td>
                    <asp:TextBox ID="txtName" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">人事部門</th>
                <td>
                    <uc:ucDept ID="ucDept" runat="server" isNoInclude="true" Width="95%" isShowAll="true" />
                </td>
            </tr>
            <tr>
                <th class="label must">帳號</th>
                <td>
                    <asp:TextBox ID="txtCode" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">密碼</th>
                <td>
                    <%--<asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Width="90%" MaxLength="20"></asp:TextBox>--%>
                    <asp:Button ID="btnDefault" runat="server" Text="回復預設密碼" OnClick="btnDefault_Click" Visible="false" />
                </td>
            </tr>
            <tr>
                <th class="label write">啟用</th>
                <td>
                    <asp:CheckBox ID="chkEnabled" runat="server" />
                </td>
            </tr>
            <tr>
                <th class="label write">部門主管</th>
                <td>
                    <asp:CheckBox ID="chkManager" runat="server" />
                </td>
            </tr>
            <tr>
                <th class="label write">權限部門</th>
                <td>
                    <uc:ucDept ID="ucDeptLevel" runat="server" isNoInclude="true" Width="95%" isShowAll="true" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
