<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="ReportSetting.aspx.cs" Inherits="WebApp._report.ReportSetting" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "23" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "24" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>
                分類：<asp:DropDownList ID="ddlTypeQ" runat="server">
                    <asp:ListItem Text="全部" Value=""></asp:ListItem>
                    <asp:ListItem Text="地震" Value="E"></asp:ListItem>
                    <asp:ListItem Text="壓降" Value="P"></asp:ListItem>
                   </asp:DropDownList>
            </li>
            <li>
                廠區：<asp:DropDownList ID="ddlAreaQ" runat="server">
                    <asp:ListItem Text="全部" Value=""></asp:ListItem>
                    <asp:ListItem Text="南廠" Value="1"></asp:ListItem>
                    <asp:ListItem Text="北廠" Value="2"></asp:ListItem>
                   </asp:DropDownList>
            </li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="true" AllowPaging="true"
                    GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
                        <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-HorizontalAlign="Center">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" data-height=".6" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("ID") %>' data-idx="0" data-height=".6" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Code" HeaderText="報表代號" ItemStyle-HorizontalAlign="Center" SortExpression="Code" />
                        <asp:BoundField DataField="Name" HeaderText="報表名稱" ItemStyle-HorizontalAlign="Center" SortExpression="Name" />
                        <asp:TemplateField HeaderText="啟用" ItemStyle-HorizontalAlign="Center" SortExpression="IsEnabled" HeaderStyle-Width="3em">
                            <ItemTemplate>
                                <uc:YesNo ID="ucYesNo1" Value='<%#Eval("IsEnabled") %>' runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="欄位目錄" data-val='<%#Eval("ID") %>' data-idx="1" data-height=".9" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
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
