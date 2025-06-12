<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysProg.aspx.cs" Inherits="WebApp._sys.SysProg" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "1" }];
        InitExt(extOpt);
    </script>
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 100px);
            overflow: auto;
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>分類：<asp:DropDownList ID="ddlDirQ" runat="server" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
            </li>
            <li>啟用：<asp:DropDownList ID="ddlEnabledQ" runat="server">
                    <asp:ListItem Text="" Value=""></asp:ListItem>
                    <asp:ListItem Text="是" Value="true"></asp:ListItem>
                    <asp:ListItem Text="否" Value="false"></asp:ListItem>
                   </asp:DropDownList>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
                <asp:Button ID="btnExport" runat="server" Text="匯出" Visible="false" />
            </li>
            <li class="li-right">
                <asp:Button ID="btnReInit" runat="server" Text="重新載入" OnClick="btnReInit_Click" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="fix-table">
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                        GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ID" HeaderText="序號" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em" />
                            <asp:BoundField DataField="Name" HeaderText="功能名稱" />
                            <asp:BoundField DataField="DirName" HeaderText="分類" SortExpression="DirName" />
                            <asp:TemplateField HeaderText="啟用" ItemStyle-HorizontalAlign="Center" SortExpression="Enabled" HeaderStyle-Width="3em">
                                <ItemTemplate>
                                    <uc:YesNo ID="ucYesNo1" Value='<%#Eval("Enabled") %>' runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Seq" HeaderText="排序" SortExpression="Seq" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em" />
                        </Columns>
                        <PagerTemplate>
                        </PagerTemplate>
                        <EmptyDataTemplate>
                            <div class="NoData">查無符合條件資料</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnReInit" EventName="Click" />
<%--                <asp:PostBackTrigger ControlID="btnExport" />--%>
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
