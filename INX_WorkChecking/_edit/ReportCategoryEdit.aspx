<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="ReportCategoryEdit.aspx.cs" Inherits="WebApp._edit.ReportCategoryEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtLevel01.ClientID %>").val() == "") {
                msg += "[類別一]不可空白 \n";
            }
            if ($("#<%=txtFAC.ClientID %>").val() == "") {
                msg += "[廠別]不可空白 \n";
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
    <li class='tab'><a href="#tabs-1">報表資料設定</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">所屬報表</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlReportID" runat="server" DataTextField="Name" DataValueField="ID" Width="95%"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">廠別</th>
                <td colspan="3">
                    <asp:TextBox ID="txtFAC" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">類別一</th>
                <td colspan="3">
                    <asp:TextBox ID="txtLevel01" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">類別二</th>
                <td colspan="3">
                    <asp:TextBox ID="txtLevel02" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">類別三</th>
                <td colspan="3">
                    <asp:TextBox ID="txtLevel03" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">資料型態</th>
                <td>
                    <asp:DropDownList ID="ddlDataType" runat="server" Width="90%">
                        <asp:ListItem Text="數值" Value="i"></asp:ListItem>
                        <asp:ListItem Text="文字" Value="s"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <th class="label must">排序</th>
                <td>
                    <asp:TextBox ID="txtSeqNo" runat="server" Width="90%" MaxLength="4"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">起始年度</th>
                <td>
                    <asp:TextBox ID="txtYearS" runat="server" Width="90%" MaxLength="4" Text="2000"></asp:TextBox>
                </td>
                <th class="label must">結束年度</th>
                <td>
                    <asp:TextBox ID="txtYearE" runat="server" Width="90%" MaxLength="4" Text="2999"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">合計</th>
                <td colspan="3">
                    <ul style="list-style:none;padding:1em;">
                        <li><asp:CheckBox ID="chkAddSUM" runat="server" Text="新增合計欄位" /></li>
                        <li><asp:CheckBox ID="chkExtSUM" runat="server" Text="可編輯資料" /></li>
                        <li>欄位名稱：<asp:TextBox ID="txtTitleSUM" runat="server" Width="90%" MaxLength="10"></asp:TextBox></li>
                        <li><asp:CheckBox ID="chkIsSUM" runat="server" Text="為合計值列" /></li>
                    </ul>
                </td>
            </tr>
            <tr>
                <th class="label write">平均</th>
                <td colspan="3">
                    <ul style="list-style:none;padding:1em;">
                        <li><asp:CheckBox ID="chkAddAVG" runat="server" Text="新增平均欄位" /></li>
                        <li><asp:CheckBox ID="chkExtAVG" runat="server" Text="可編輯資料" /></li>
                        <li>欄位名稱：<asp:TextBox ID="txtTitleAVG" runat="server" Width="90%" MaxLength="10"></asp:TextBox></li>
                        <li><asp:CheckBox ID="chkIsAVG" runat="server" Text="為平均值列" /></li>
                    </ul>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
