<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappManual.aspx.cs" Inherits="WebApp._app.MappManual" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "38" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>發送日期：
                <uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增訊息" data-val='0' data-idx="0" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="檢視訊息" data-val='<%# Eval("SEQ_ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="MApp_Plant" HeaderText="廠別" />
                        <asp:BoundField DataField="MApp_Date" HeaderText="日期" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="7em" SortExpression="MApp_Date" />
                        <asp:BoundField DataField="MApp_Time" HeaderText="時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5.5em" />
                        <asp:BoundField DataField="MApp_Value1" HeaderText="發送值1" />
                        <asp:BoundField DataField="MApp_Value2" HeaderText="發送值2" />
                        <asp:BoundField DataField="MApp_Value3" HeaderText="發送值3" />
                        <asp:BoundField DataField="MApp_Provider" HeaderText="Provider" />
                        <asp:BoundField DataField="Ack_Flag" HeaderText="已發送" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3em" SortExpression="Ack_Flag" />
                        <%--<asp:TemplateField HeaderText="已發送" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3em">
                            <ItemTemplate>
                                <uc:YesNo ID="ynAckFlag" runat="server" Value='<%#Eval("MApp_Ack_Flag")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>--%>
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
