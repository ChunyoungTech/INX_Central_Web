<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysUser.aspx.cs" Inherits="WebApp._sys.SysUser" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "3" }];
        InitExt(extOpt);
    </script>
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 100px);
            overflow: auto;
            margin:0; padding:0;
            /*transform: translateY(100%);*/
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
            <li>部門：<uc:ucDept ID="ddlDeptQ" runat="server" isShowAll="true" /></li>
            <li>帳號：<asp:TextBox ID="txtCodeQ" runat="server" Width="5em" MaxLength="10"></asp:TextBox></li>
            <li>姓名：<asp:TextBox ID="txtNameQ" runat="server" Width="5em" MaxLength="10"></asp:TextBox></li>
            <li>啟用：
                <asp:DropDownList ID="ddlEnabled" runat="server">
                    <asp:ListItem Text="全部" Value="" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="是" Value="1"></asp:ListItem>
                    <asp:ListItem Text="否" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
            <li class="li-right">
                <asp:Button ID="btnExport" runat="server" Text="匯出" Visible="false" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                    <div class="fix-table">
                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                            GridLines="None" ShowHeaderWhenEmpty="true" AllowPaging="true" PagerSettings-Visible="false">
                            <Columns>
                                <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                    <HeaderTemplate>
                                        <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ID" HeaderText="序號" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em" />
                                <asp:BoundField DataField="Code" HeaderText="帳號" HeaderStyle-Width="10em" />
                                <asp:BoundField DataField="Name" HeaderText="姓名" HeaderStyle-Width="8em" />
                                <asp:BoundField DataField="DeptName" HeaderText="人事部門" SortExpression="DeptName" />
                                <asp:BoundField DataField="DeptLevel" HeaderText="權限部門" SortExpression="DeptLevel" />
                                <asp:TemplateField HeaderText="啟用" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                    <ItemTemplate>
                                        <uc:YesNo ID="ucYesNo2" Value='<%#Eval("Enabled")%>' runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
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
                <asp:PostBackTrigger ControlID="btnExport" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
