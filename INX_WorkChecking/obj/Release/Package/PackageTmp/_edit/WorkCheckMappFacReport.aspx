<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="WorkCheckMappFacReport.aspx.cs" Inherits="WebApp._edit.WorkCheckMappFacReport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
    <script type="text/javascript">
        function checkData() {
            var msg = "";
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
    <li class='tab'><a href="#tabs-1">部門</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">廠別</th>
                <td>
                    <asp:DropDownList ID="ddlFacCode" runat="server" Width="98%"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">MAPP設定</th>
                <td>
                    <asp:DropDownList ID="ddlMappSetting" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP發送設定" data-filter-count="50" Width="98%" DataTextField="Name" DataValueField="Code"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">啟用</th>
                <td>
                    <asp:DropDownList ID="ddlEnabled" runat="server" Width="98%">
                        <asp:ListItem Text="是" Value="1"></asp:ListItem>
                        <asp:ListItem Text="否" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
