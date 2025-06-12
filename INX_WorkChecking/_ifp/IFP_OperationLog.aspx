<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IFP_OperationLog.aspx.cs" Inherits="WebApp._ifp.IFP_OperationLog" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>填充口：
                <asp:DropDownList ID="ddlMaterial" runat="server" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
            </li>
            <li>記錄日期：
                <uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
            <li style="float:right;"><asp:Button ID="btnExport" runat="server" Text="匯出CSV" OnClick="btnExport_Click" /></li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:BoundField DataField="PortName" HeaderText="充填口名稱" SortExpression="PortName" />
                        <asp:BoundField DataField="Operation_Time" HeaderText="操作時間" SortExpression="Operation_Time" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" />
                        <asp:BoundField DataField="Operation_Log" HeaderText="操作內容" SortExpression="Operation_Log" />
                        <asp:BoundField DataField="UserName" HeaderText="使用者名稱" SortExpression="UserName" />
                        <asp:BoundField DataField="SCADA_USER" HeaderText="圖控操作人員" SortExpression="SCADA_USER" />
                        <%--<asp:BoundField DataField="Code" HeaderText="回傳代碼" SortExpression="Code" />--%>
                    </Columns>
                    <PagerTemplate></PagerTemplate>
                    <EmptyDataTemplate><div class="NoData">查無符合條件資料</div></EmptyDataTemplate>
                </asp:GridView>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnExport" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
