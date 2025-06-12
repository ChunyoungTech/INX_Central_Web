<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="WorkCheckinTemp.aspx.cs" Inherits="WebApp._app.WorkCheckinTemp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(function () {
            $(document).on("click", '.OpenBtn', function () {
                OpenWindow('../_edit/WorkCheckListTemp.aspx?pa=' + $(this).attr("data-val"), '報到查詢', .7, .8);
            });
        });
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
        </ul>
        <ul>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" OnClick="btnQuery_Click" />
            </li>
            <li class="li-right">
                <asp:CheckBox ID="chkAuto" runat="server" Text="自動更新" AutoPostBack="true"  />
                <asp:Button ID="btnExport" runat="server" Text="列印"  />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowDataBound="GridView1_RowDataBound" PageSize="1000">
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
                <asp:Timer ID="Timer1" runat="server"  Enabled="False">
                </asp:Timer>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
                <asp:PostBackTrigger ControlID="btnExport" />
                <asp:AsyncPostBackTrigger ControlID="chkAuto" EventName="CheckedChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
