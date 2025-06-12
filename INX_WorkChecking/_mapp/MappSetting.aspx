<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappSetting.aspx.cs" Inherits="WebApp._mapp.MappSetting" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 100px);
            overflow: auto;
            margin:0; padding:0;
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "12" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>
                部門：<uc:ucDept ID="ucDeptQ" runat="server" />
            </li>
            <li>
                分類：
                <asp:DropDownList ID="ddlTypeQ" runat="server" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" AppendDataBoundItems="true">
                    <asp:ListItem Text="全部" Value=""></asp:ListItem>
                </asp:DropDownList>
            </li>
            <li>
                設定名稱：<asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
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
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                        GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <HeaderTemplate>
                                    <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("MS_SEQ_ID") %>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="MS_SYS_NAME" HeaderText="設定名稱" SortExpression="MS_SYS_NAME" />
                            <asp:BoundField DataField="MS_SYS_DESC" HeaderText="設定說明" />
                            <asp:BoundField DataField="MT_TYPE_NAME" HeaderText="設定分類" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="MS_MAPP_TEAM_SN" HeaderText="團隊編號" SortExpression="MS_MAPP_TEAM_SN" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="MS_SYS_DEPT_NAME" HeaderText="使用部門" SortExpression="MS_SYS_DEPT_NAME" />
                            <asp:BoundField DataField="MS_SYS_STOP" HeaderText="停用" SortExpression="MS_SYS_STOP" HeaderStyle-Width="2em" ItemStyle-HorizontalAlign="Center" />
                            <%--<asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="4em">
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="發送記錄" data-val='<%# Eval("MS_SEQ_ID") %>' data-idx="1" data-height=".8" />
                            </ItemTemplate>
                        </asp:TemplateField>--%>
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
                <asp:AsyncPostBackTrigger ControlID="btnReInit" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
