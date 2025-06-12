<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappEVSetting.aspx.cs" Inherits="WebApp._mapp.MappEVSetting" %>
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
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "18" }];
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
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="true" AllowPaging="true"
                    GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
                        <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-HorizontalAlign="Center">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" data-height=".9" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("ID") %>' data-idx="0" data-height=".9" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Code" HeaderText="代號" ItemStyle-HorizontalAlign="Center" SortExpression="Code" />
                        <asp:BoundField DataField="Name" HeaderText="名稱" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="TypeName" HeaderText="分類" ItemStyle-HorizontalAlign="Center" SortExpression="TypeName" />
                        <asp:BoundField DataField="AreaName" HeaderText="廠區" ItemStyle-HorizontalAlign="Center" SortExpression="AreaName" />
                        <asp:TemplateField HeaderText="高階" ItemStyle-HorizontalAlign="Center" SortExpression="IsTop" HeaderStyle-Width="3em">
                            <ItemTemplate>
                                <uc:YesNo ID="ucYesNo1" ValueOrginal='<%#Eval("IsTop") %>' runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="NormalCode" HeaderText="正式發送設定" />
                        <asp:BoundField DataField="DisableCode" HeaderText="隔離發送設定" />
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
