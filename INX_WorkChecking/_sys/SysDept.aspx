<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysDept.aspx.cs" Inherits="WebApp._sys.SysDept" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "2" }];
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
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>部門：<uc:ucDept ID="ddlDeptQ" runat="server" isShowAll="true" />
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" />
                    </li>
                    <li class="li-right">
                        <asp:Button ID="btnReset" runat="server" Text="重新載入" OnClick="btnReset_Click" />
                        <asp:Button ID="btnExport" runat="server" Text="匯出" Visible="false" />
                    </li>
                </ul>
            </div>
            <div class="GridArea">
<%--                <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <div class="fix-table">
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView" AllowPaging="true"
                        GridLines="None" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <HeaderTemplate>
                                    <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ID" HeaderText="序號" SortExpression="ID" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em" />
                            <asp:BoundField DataField="Code" HeaderText="部門代號" SortExpression="Code" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="Name" HeaderText="部門名稱" SortExpression="NameAll" />
                            <asp:BoundField DataField="NameAll" HeaderText="部門階層" SortExpression="NameAll" />
                            <asp:BoundField DataField="LevelNo" HeaderText="部門層級" SortExpression="LevelNo" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em" />
                        </Columns>
                        <PagerTemplate>
                        </PagerTemplate>
                        <EmptyDataTemplate>
                            <div class="NoData">查無符合條件資料</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
