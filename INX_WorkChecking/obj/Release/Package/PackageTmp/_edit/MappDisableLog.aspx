<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="MappDisableLog.aspx.cs" Inherits="WebApp._edit.MappDisableLog" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%--<link href="../Content/grid.1.1.css" rel="stylesheet" />--%>
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <script type="text/javascript" src="../Scripts/jquery.tiny-draggable.min.js"></script>
    <script type="text/javascript">
<%--        var extOpt = [
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .8, Sub: "59" }];
        InitExt(extOpt);--%>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="buttonArea">
<%--                    <asp:HiddenField ID="hidID" runat="server" />
                    <asp:LinkButton ID="lbRefresh" runat="server" /><input type="hidden" id="hidRefresh" value="" />--%>
                    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
                </div>

                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">隔離設定修改記錄</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <table style="width: 100%;">
                            <tr style="display:none;">
                                <th class="label must">MAPP類別</th>
                                <td>
                                    <asp:Label ID="lblName" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" valign="top">
                                    <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="False" AllowPaging="true"
                                        GridLines="Vertical" Width="100%" ShowHeaderWhenEmpty="true">
                                        <Columns>
                                            <asp:TemplateField HeaderText="隔離MAPP設定">
                                                <ItemTemplate>
                                                    <input type="text" class="lblTextBox" value='<%#Eval("MS_SYS_NAME") %>' disabled="disabled" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="MD_DATE_START" HeaderText="隔離開始時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                                            <asp:BoundField DataField="MD_DATE_END" HeaderText="隔離結束時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                                            <asp:TemplateField HeaderText="隔離原因說明">
                                                <ItemTemplate>
                                                    <input type="text" class="lblTextBox" value='<%#Eval("MD_REASON") %>' disabled="disabled" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="MD_REMIND_MIN" HeaderText="逾時未解隔<br/>通知頻率" HeaderStyle-Width="4em" ItemStyle-HorizontalAlign="Center" HtmlEncode="false" />
                                            <asp:BoundField DataField="MD_REMIND_SETTING" HeaderText="逾時未解隔<br/>通知設定" HtmlEncode="false" />
                                            <asp:BoundField DataField="MD_TRANS_NAME" HeaderText="隔離轉發" />
                                            <asp:BoundField DataField="UPDATE_USER" HeaderText="更新人員" HeaderStyle-Width="4em" ItemStyle-HorizontalAlign="Center" />
                                            <asp:BoundField DataField="UPDATE_TIME" HeaderText="更新時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="NoData">查無符合條件資料</div>
                                        </EmptyDataTemplate>
                                        <PagerTemplate></PagerTemplate>
                                    </asp:GridView>
                                    <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>

