<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysSetting.aspx.cs" Inherits="WebApp._sys.SysSetting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 100px);
            overflow: auto;
            margin: 0;
            padding: 0;
            /*position:absolute;*/
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "17" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li style="display: none;">
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
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3em">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ID" HeaderText="序號" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="3em" />
                            <asp:BoundField DataField="Code" HeaderText="參數代碼" />
                            <asp:BoundField DataField="Name" HeaderText="參數名稱" />
                            <asp:BoundField DataField="Value" HeaderText="設定值" />
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
                <asp:AsyncPostBackTrigger ControlID="btnReInit" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
