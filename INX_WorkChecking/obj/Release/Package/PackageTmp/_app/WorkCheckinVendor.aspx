<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Vendor.Master" AutoEventWireup="true" CodeBehind="WorkCheckinVendor.aspx.cs" Inherits="WebApp._app.WorkCheckinVendor" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "11" }];
        InitExt(extOpt);

        $(document).on('click', '.OpenWindowTitle a', function () { $.fancybox.close(); });
        function OpenListWindow(con_number) {
            $.fancybox.open(
                {
                    title: "<div class='OpenWindowTitle'><span>報到查詢</span><a href='#'><img src='../_img/window_off.png' style='height:1.5em;float:right;' /><a/></div>",
                    type: "iframe",
                    href: '../_edit/WorkCheckListTemp.aspx?pa=' + con_number,
                    width: 1200,
                    minHeight: $(window).height() * .8,
                    closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
                    beforeShow: function () {
                        this.wrap.tinyDraggable();
                    }
                }, { helpers: { overlay: { closeClick: true, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
            );
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>施工日期：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li>施工單號&廠商：<asp:TextBox ID="txtNumber" runat="server"></asp:TextBox>
            </li>
            <li>
                <asp:CheckBox ID="chkCheckIn" runat="server" Text="只查詢已簽到" />
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
<%--            <li class="li-right">
                <asp:CheckBox ID="chkAuto" runat="server" Text="自動更新" AutoPostBack="true"  />
                <asp:Button ID="btnExport" runat="server" Text="列印"  />
            </li>--%>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="con_date" HeaderText="施工日期" SortExpression="con_date" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-Width="6em" />
                        <asp:BoundField DataField="con_number" HeaderText="施工單號" SortExpression="con_number" ItemStyle-Width="5em" />
                        <asp:BoundField DataField="fac_name" HeaderText="施工廠別" SortExpression="fac_name" />
                        <asp:BoundField DataField="vendor_name" HeaderText="施工廠商" SortExpression="vendor_name" />
                        <asp:BoundField DataField="哨口進廠時間" HeaderText="哨口進廠時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="哨口進廠人數" HeaderText="哨口進廠人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="checkin_time" HeaderText="刷臉簽到時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="checkin_count" HeaderText="刷臉簽到人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="checkout_time" HeaderText="刷臉簽退時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="checkout_count" HeaderText="刷臉簽退人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="哨口出廠時間" HeaderText="哨口出廠時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="哨口出廠人數" HeaderText="哨口出廠人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />

                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <input type="button" value="報到查詢" data-val='<%# Eval("con_number") %>' data-idx="0" onclick ="OpenListWindow(<%# Eval("con_number")%>)" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerTemplate>
                    </PagerTemplate>
                    <EmptyDataTemplate>
                        <div class="NoData">
                            查無符合條件資料
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>