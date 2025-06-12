<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="TransferIndex.aspx.cs" Inherits="WebApp._idb.TransferIndex" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "27", Dir: "_idb", Width: .5 }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>廠別：<asp:DropDownList ID="ddlFactory" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFactory_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>系統別：<asp:DropDownList ID="ddlSystem" runat="server"></asp:DropDownList>
                    </li>
                    <li>點位名稱：<asp:TextBox ID="txtTagName" runat="server"></asp:TextBox>
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" />
                    </li>
                    <li style="flex-grow:1;display:flex;justify-content:end;">
                        <asp:Button ID="btnExport" runat="server" Text="匯出" BackColor="white" ForeColor="black" OnClick="btnExport_Click"/>
                    </li>
                </ul>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="GridArea">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100" PagerSettings-Visible="false"
                    GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="2.5em">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("SEQ_ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ind_no" HeaderText="ind_no" SortExpression="ind_no" />
                        <asp:BoundField DataField="ind_source_note" HeaderText="ind_source_note" />
                        <asp:BoundField DataField="scada_tagname" HeaderText="scada_tagname" SortExpression="scada_tagname" />
                        <asp:BoundField DataField="ind_tagname" HeaderText="ind_tagname" SortExpression="ind_tagname" />
                        <asp:BoundField DataField="ind_fac" HeaderText="ind_fac" SortExpression="ind_fac" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_fab" HeaderText="ind_fab" SortExpression="ind_fab" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_System" HeaderText="ind_System" SortExpression="ind_System" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_unit" HeaderText="ind_unit" SortExpression="ind_unit" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_eqp_group" HeaderText="ind_eqp_group" SortExpression="ind_eqp_group" />
                        <asp:BoundField DataField="ind_section" HeaderText="ind_section" SortExpression="ind_section" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_Common" HeaderText="ind_Common" SortExpression="ind_Common" />
                        <asp:BoundField DataField="ind_level" HeaderText="ind_level" SortExpression="ind_level" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_priority" HeaderText="ind_priority" SortExpression="ind_priority" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="ind_col_index" HeaderText="ind_col_index" SortExpression="ind_col_index" />
                        <asp:BoundField DataField="ind_row_index" HeaderText="ind_row_index" SortExpression="ind_row_index" />
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
            <asp:PostBackTrigger ControlID="btnExport" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
