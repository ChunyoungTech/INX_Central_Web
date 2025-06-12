<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysRole.aspx.cs" Inherits="WebApp._sys.SysRole" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 100px);
            overflow: auto;
            margin:0; padding:0;
            /*position:absolute;*/
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "4" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .5, Sub: "5" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "6" }
        ];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="QueryArea">
        <ul>
            <li style="display:none;">
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
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" AllowPaging="true" AllowSorting="true" CssClass="MainGridView" 
                        GridLines="None" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <HeaderTemplate>
                                    <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("ID")%>' data-idx="0" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ID" HeaderText="序號" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em" />
                            <asp:BoundField DataField="Name" HeaderText="名稱" SortExpression="Name" />
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderText="預設群組" HeaderStyle-Width="4em">
                                <ItemTemplate>
                                    <uc:YesNo ID="ynDefault" runat="server" Value='<%#Eval("IsDefault")%>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                <ItemTemplate>
                                    <input id="btnProg" type="button" class="extBtn" value="功能" data-val='<%# Eval("ID") %>' data-t="設定" data-idx="2" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                                <ItemTemplate>
                                    <input id="btnUser" type="button" class="extBtn" value="使用者" data-val='<%# Eval("ID") %>' data-width="650" data-t="設定" data-idx="1" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="NoData">查無符合條件資料</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>
