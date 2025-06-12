<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="ReportCategory.aspx.cs" Inherits="WebApp._report.ReportCategory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
        .td-center{
            text-align:center;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "23" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>報表：<asp:DropDownList ID="ddlReport" runat="server" DataTextField="Name" DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="ddlReport_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>類別一：<asp:DropDownList ID="ddlLevel01" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLevel01_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li style="display: none;">
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
                </ul>
            </div>
            <div class="GridArea">
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="true" AllowPaging="true"
                    GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
                        <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-CssClass="td-center">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0,<%=ReportID %>' data-idx="0" data-height=".6" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("ID")%>,<%=ReportID %>' data-idx="0" data-height=".6" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FAC" HeaderText="廠別" ItemStyle-CssClass="td-center" SortExpression="FAC" />
                        <asp:BoundField DataField="Level01" HeaderText="類別一" SortExpression="Level01" />
                        <asp:BoundField DataField="Level02" HeaderText="類別二" SortExpression="Level02" />
                        <asp:BoundField DataField="Level03" HeaderText="類別三" SortExpression="Level03" />
                        <asp:BoundField DataField="SeqNo" HeaderText="排序" ItemStyle-CssClass="td-center" SortExpression="SeqNo" />
                        <asp:BoundField DataField="DataType" HeaderText="資料類型" ItemStyle-CssClass="td-center" SortExpression="DataType" />
                        <asp:BoundField DataField="YearS" HeaderText="起始年度" ItemStyle-CssClass="td-center" />
                        <asp:BoundField DataField="YearE" HeaderText="結束年度" ItemStyle-CssClass="td-center" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </div>
<%--            <div>
                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
                <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
            </div>--%>
        </ContentTemplate>
<%--        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlReport" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlLevel01" EventName="SelectedIndexChanged" />
        </Triggers>--%>
    </asp:UpdatePanel>
</asp:Content>

