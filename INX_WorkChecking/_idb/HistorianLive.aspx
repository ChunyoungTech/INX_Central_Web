<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="HistorianLive.aspx.cs" Inherits="WebApp._idb.HistorianLive" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>廠別：<asp:DropDownList ID="ddlFactory" runat="server"></asp:DropDownList></li>
                    <li>TagName：<asp:TextBox ID="txtTagName" runat="server"></asp:TextBox></li>
                    <li>Quality：<asp:TextBox ID="txtQuality" runat="server"></asp:TextBox></li>
                    <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
                </ul>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="GridArea">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100" PagerSettings-Visible="false"
                    GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:BoundField DataField="DateTime" HeaderText="資料時間" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="TagName" HeaderText="TagName" />
                        <asp:BoundField DataField="Value" HeaderText="Value" ItemStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="vValue" HeaderText="vValue" />
                        <asp:BoundField DataField="Quality" HeaderText="Quality" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="QualityDetail" HeaderText="QualityDetail" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="OPCQuality" HeaderText="OPCQuality" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="wwTagKey" HeaderText="wwTagKey" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="wwRetrievalMode" HeaderText="wwRetrievalMode" />
                        <asp:BoundField DataField="wwTimeDeadband" HeaderText="wwTimeDeadband" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="wwValueDeadband" HeaderText="wwValueDeadband" ItemStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="wwTimeZone" HeaderText="wwTimeZone" />
                        <asp:BoundField DataField="wwParameters" HeaderText="wwParameters" />
                        <asp:BoundField DataField="SourceTag" HeaderText="SourceTag" />
                        <asp:BoundField DataField="SourceServer" HeaderText="SourceServer" />
                        <asp:BoundField DataField="wwValueSelector" HeaderText="wwValueSelector" />
                        <asp:BoundField DataField="wwExpression" HeaderText="wwExpression" />
                        <asp:BoundField DataField="wwUnit" HeaderText="wwUnit" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
