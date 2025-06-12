<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="PatrolEdit.aspx.cs" Inherits="WebApp.PatrolEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {

        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
<%--    <asp:Button ID="btnConfirm" runat="server" Text="確定啟動" OnClientClick="return checkData()" />--%>
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">巡檢啟動</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label write">巡檢日期</th>
                <td>
                    <uc:ucDate ID="dteDate" runat="server" Enabled="false" />
                </td>
            </tr>
            <tr>
                <th class="label write">巡檢設定</th>
                <td>
                    <asp:Label ID="lblSetting" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">巡檢人員</th>
                <td>
                    <asp:Label ID="lblPatrolUser" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">啟動人員</th>
                <td>
                    <asp:Label ID="lblStartUser" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">啟動時間</th>
                <td>
                    <asp:Label ID="lblTimeStart" runat="server" Text=""></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">結束時間</th>
                <td>
                    <asp:Label ID="lblTimeEnd" runat="server" Text=""></asp:Label>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
