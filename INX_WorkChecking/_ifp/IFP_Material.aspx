<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IFP_Material.aspx.cs" Inherits="WebApp._ifp.IFP_Material" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "25" },
            { reCtl: "#<%=lblReType.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "29" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>
                分類：<uc:ucDept ID="ddlDeptQ" runat="server" isShowAll="false" isNoInclude="false" />
                <asp:DropDownList ID="ddlTypeQ" runat="server" Visible="false"></asp:DropDownList>
                <input type="button" class="extBtn" value="分類設定" data-val='0' data-idx="1" style="display:none;" />
                <asp:LinkButton ID="lblReType" runat="server" />
            </li>
            <li>名稱：<asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
            </li>
            <li><asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Name" HeaderText="原料名稱" SortExpression="Name" />
                        <asp:BoundField DataField="Code" HeaderText="原料代號" SortExpression="Code" />
                        <asp:BoundField DataField="TypeName" HeaderText="原料分類" SortExpression="TypeName" />
                        <asp:BoundField DataField="Supplier" HeaderText="供應商" SortExpression="Supplier" />
                        <asp:BoundField DataField="PortNo" HeaderText="填充口" SortExpression="PortNo" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="FaceDevice" HeaderText="辨識設備" SortExpression="FaceDevice" />
                    </Columns>
                    <PagerTemplate></PagerTemplate>
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
