<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="WorkCheckin.aspx.cs" Inherits="WebApp._app.WorkCheckin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 150px);
            overflow: auto;
            margin: 0;
            padding: 0;
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "7" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "8" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "9" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "10" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>
                施工日期：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li>
                廠別：<asp:DropDownList ID="ddlFAC" runat="server"></asp:DropDownList>
            </li>
            <li>施工單號&廠商：<asp:TextBox ID="txtNumber" runat="server"></asp:TextBox>
            </li>
        </ul>
        <ul>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
            <li>
                <asp:CheckBox ID="chkCheckIn" runat="server" Text="只查詢已簽到" />
            </li>
            <li class="li-right">
                <asp:CheckBox ID="chkAuto" runat="server" Text="自動更新" AutoPostBack="true" OnCheckedChanged="chkAuto_CheckedChanged" />
                <asp:Button ID="btnExport" runat="server" Text="列印" OnClick="btnExport_Click" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="fix-table">
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100" PagerSettings-Visible="false"
                        GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowDataBound="GridView1_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="con_date" HeaderText="施工日期" SortExpression="con_date" ItemStyle-HorizontalAlign="Center" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-Width="6em" />
                            <asp:BoundField DataField="con_number" HeaderText="施工單號" SortExpression="con_number" ItemStyle-Width="5em" />
                            <asp:BoundField DataField="fac_name" HeaderText="施工廠別" SortExpression="fac_name" />
                            <asp:BoundField DataField="vendor_name" HeaderText="施工廠商" SortExpression="vendor_name" />
                            <asp:BoundField DataField="checkin_time" HeaderText="廠務報到時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                            <asp:BoundField DataField="checkin_count" HeaderText="廠務報到人員" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                            <asp:BoundField DataField="checkout_time" HeaderText="廠務報退時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                            <asp:BoundField DataField="checkout_count" HeaderText="廠務報退人員" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                            <asp:BoundField DataField="哨口進廠時間" HeaderText="哨口進廠時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                            <asp:BoundField DataField="哨口進廠人數" HeaderText="哨口進廠人員" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                            <asp:BoundField DataField="哨口出廠時間" HeaderText="哨口出廠時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="8em" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                            <asp:BoundField DataField="哨口出廠人數" HeaderText="哨口出廠人員" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3.5em" />
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="工單明細" data-val='<%# Eval("con_number") %>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="簽到" data-t='工單簽到(<%#Eval("con_number") %>)' data-val='<%# Eval("con_number") %>' data-idx="1" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="簽退" data-t='工單簽退(<%#Eval("con_number") %>)' data-val='<%# Eval("con_number") %>' data-idx="2" <%#Convert.ToBoolean(Eval("CheckOut")) ? "" : "disabled" %> />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="報到查詢" data-val='<%# Eval("con_number") %>' data-idx="3" data-height=".8" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="NoData">查無符合條件資料</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <asp:Timer ID="Timer1" runat="server" OnTick="Timer1_Tick" Enabled="False">
                </asp:Timer>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
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
