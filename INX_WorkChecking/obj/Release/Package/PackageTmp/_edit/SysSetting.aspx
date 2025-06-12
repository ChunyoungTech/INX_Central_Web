<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="SysSetting.aspx.cs" Inherits="WebApp._edit.SysSetting" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%--<script type="text/javascript" src="../_js/jquery.tiny-draggable.min.js"></script>--%>
    <script type="text/javascript">
<%--        var extOpt = [
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .8, Sub: "49" }];
        InitExt(extOpt);--%>
        function checkData() {

        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="buttonArea">
                    <asp:Button ID="btnConfirm" runat="server" Text="儲存" OnClientClick="return checkData()" />
                    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
                </div>
                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">系統參數</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <table style="width: 100%;">
                            <tr>
                                <th class="label read">參數代碼</th>
                                <td>
                                    <asp:Label ID="lblCode" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <th class="label must">參數名稱</th>
                                <td>
                                    <asp:TextBox ID="txtName" runat="server" MaxLength="20" Width="95%"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="label must">設定值</th>
                                <td>
                                    <asp:TextBox ID="txtValue" runat="server" MaxLength="200" Width="95%"></asp:TextBox>
                                    <asp:TextBox ID="txtValue2" TextMode="Password" runat="server" MaxLength="200" Width="95%" Visible="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="label read">說明</th>
                                <td>
                                    <asp:Label ID="lblMemo" runat="server" Text=""></asp:Label>
                                    <asp:HiddenField ID="hidType" runat="server" />
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
