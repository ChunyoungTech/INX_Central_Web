<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IFP_MaterialOrder.aspx.cs" Inherits="WebApp._ifp.IFP_MaterialOrder" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "27" },
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "28" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>原料名稱：<asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
            </li>
            <li>預計填充日期：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" /></li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Material" HeaderText="原料名稱" SortExpression="Material" />
                        <asp:BoundField DataField="Supplier" HeaderText="供應商" SortExpression="Supplier" />
                        <asp:BoundField DataField="OrderDate" HeaderText="填寫日期" SortExpression="OrderDate" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="UserName" HeaderText="填寫人員" SortExpression="UserName" />
                        <%--<asp:BoundField DataField="FillingPort" HeaderText="填充口" SortExpression="FillingPort" />--%>
                        <asp:BoundField DataField="EDate" HeaderText="預計填充日" SortExpression="EstimateDate" />
                        <%--<asp:BoundField DataField="FDate" HeaderText="填充完成時間" SortExpression="FillingDate" />--%>
                        <asp:BoundField DataField="FillingDate" HeaderText="填充完成時間" SortExpression="FillingDate" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                        <asp:BoundField DataField="IsCancel" HeaderText="已取消" ItemStyle-HorizontalAlign="Center" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="填充回報" data-val='<%# Eval("ID") %>' data-idx="1" <%# (Eval("IsCancel").ToString()=="是" ? "disabled":"")%> />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerTemplate></PagerTemplate>
                    <EmptyDataTemplate><div class="NoData">查無符合條件資料</div></EmptyDataTemplate>
                </asp:GridView>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
