<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="WorkCheckTest.aspx.cs" Inherits="WebApp._test.WorkCheckTest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(function () {
            $(document).on("click", '.OpenBtn', function () {
                $.fancybox.open(
                    {
                        title: "<div class='OpenWindowTitle'><span>報到查詢</span></div>",
                        type: "iframe",
                        href: '../_edit/WorkCheckListTemp.aspx?pa=' + $(this).attr("data-val"),
                        width: $(window).width() * .7,
                        minHeight: $(window).height() * .8,
                        minWidth: "450",
                        closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
                        beforeShow: function () {
                            this.wrap.tinyDraggable();
                        }
                    }, { helpers: { overlay: { closeClick: true, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
                );
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>本功能僅限於測試環境使用</li>
        </ul>
        <ul>
            <li>工單日期：<uc:ucDate ID="dteCreate" runat="server" />
            </li>
            <li>廠別：<asp:DropDownList ID="ddlFactory" runat="server">
                <asp:ListItem Text="FAC6" Value="6"></asp:ListItem>
                <asp:ListItem Text="FAC8" Value="8"></asp:ListItem>
            </asp:DropDownList>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
            <li class="li-right">
                <asp:Button ID="btnCreate" runat="server" Text="產生新工單" OnClick="btnCreate_Click" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowCommand="GridView1_RowCommand">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <asp:Button ID="Button1" runat="server" Text="新增報到" CommandName="Checkin" CommandArgument='<%# Eval("con_number") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <asp:Button ID="Button2" runat="server" Text="新增報退" CommandName="Checkout" CommandArgument='<%# Eval("con_number") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="con_date" HeaderText="施工日期" SortExpression="con_date" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-Width="6em" />
                        <asp:BoundField DataField="con_number" HeaderText="施工單號" SortExpression="con_number" ItemStyle-Width="5em" />
                        <asp:BoundField DataField="fac_name" HeaderText="施工廠別" SortExpression="fac_name" />
                        <asp:BoundField DataField="vendor_name" HeaderText="施工廠商" SortExpression="vendor_name" />
                        <asp:BoundField DataField="哨口進廠人數" HeaderText="哨口進廠人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="checkin_count" HeaderText="廠務報到人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="checkout_count" HeaderText="廠務報退人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:BoundField DataField="哨口出廠人數" HeaderText="哨口出廠人數" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <input type="button" class="OpenBtn" value="報到查詢" data-val='<%# Eval("con_number") %>' />
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
                <asp:AsyncPostBackTrigger ControlID="btnCreate" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
