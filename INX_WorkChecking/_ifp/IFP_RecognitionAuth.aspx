<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IFP_RecognitionAuth.aspx.cs" Inherits="WebApp._ifp.IFP_RecognitionAuth" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>設備名稱：<asp:TextBox ID="txtDeviceName" runat="server"></asp:TextBox>
            </li>
            <li>記錄日期：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" /></li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
            <li><asp:CheckBox ID="chkNotUserEmpty" runat="server" Text="不含人員空白" /></li>
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
                        <asp:BoundField DataField="DeviceName" HeaderText="設備名稱" SortExpression="DeviceName" />
                        <asp:BoundField DataField="LogDateTime" HeaderText="記錄日期" SortExpression="LogDateTime" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" ItemStyle-Width="12em" />
                        <asp:BoundField DataField="FRUserID" HeaderText="人員代號" SortExpression="FRUserID" ItemStyle-Width="8em" />
                        <asp:BoundField DataField="UserName" HeaderText="人員姓名" SortExpression="UserName" ItemStyle-Width="8em" />
                        <asp:BoundField DataField="Code" HeaderText="回傳代碼" SortExpression="Code" />
                        <asp:BoundField DataField="LogContent" HeaderText="辨識結果" SortExpression="LogContent" />
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
