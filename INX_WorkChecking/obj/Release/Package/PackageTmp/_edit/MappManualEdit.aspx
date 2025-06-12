<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappManualEdit.aspx.cs" Inherits="WebApp._edit.MappManualEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
<%--            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[填充口名稱]不可空白 \n";
            }--%>
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定發送" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">MApp手動發送</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label write">廠別</th>
                <td>
                    <asp:TextBox ID="txtPlant" runat="server" Width="6em" MaxLength="5"></asp:TextBox>
                </td>
                <th class="label write">秒數</th>
                <td>
                    <asp:TextBox ID="txtSecond" runat="server" Width="6em" MaxLength="2"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">Type</th>
                <td>
                    <asp:TextBox ID="txtType" runat="server" Width="98%" MaxLength="3"></asp:TextBox>
                </td>
                <th class="label write">Provider</th>
                <td>
                    <asp:TextBox ID="txtProvider" runat="server" Width="98%" MaxLength="10"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">發送值1</th>
                <td colspan="3">
                    <asp:TextBox ID="txtValue1" runat="server" Width="98%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">發送值2</th>
                <td colspan="3">
                    <asp:TextBox ID="txtValue2" runat="server" Width="98%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">發送值3</th>
                <td colspan="3">
                    <asp:TextBox ID="txtValue3" runat="server" Width="98%"></asp:TextBox>
                </td>
            </tr>
            <tr <%=ViewState["ID"].ToString() == "0" ? "style='display:none;'" : "" %>>
                <th class="label read">已發送</th>
                <td>
                    <asp:Label ID="lblAck_Flag" runat="server" Text=""></asp:Label>
                </td>
                <th class="label read">新增日期時間</th>
                <td>
                    <asp:Label ID="lblDate" runat="server" Text=""></asp:Label>
                </td>
<%--                <th class="label read">新增時間</th>
                <td>
                    <asp:Label ID="lblTime" runat="server" Text=""></asp:Label>
                </td>--%>
            </tr>
        </table>
    </div>
</asp:Content>
