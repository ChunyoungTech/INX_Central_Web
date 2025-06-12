<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="DeptTagsSet.aspx.cs" Inherits="WebApp._alarm.DeptTagsSet" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "40" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "39" }];
        InitExt(extOpt);

        function deleteItem(id) {
            if (!confirm("刪除確認？")) return;
            $.ajax({
                type: "POST",
                url: "../_alarm/DeptTagsSet.aspx/DeleteItem",
                data: JSON.stringify({ id: id, App: <%=Request.QueryString["app"]%> }),
                contentType: "application/json; charset=utf-8",
                //async: false,
                //cache: false,
                dataType: "json",
                success: function (data) {
                    if (data.d != true) {
                        alert("刪除失敗")
                    }
                    $("#ContentPlaceHolder1_ContentPlaceHolder1_btnQuery").click();
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>部門：<uc:ucDept ID="ddlDeptQ" runat="server" isShowAll="false" isNoInclude="true" />
            </li>
            <li>資料點名稱：<asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
            </li>
            <li>資料點類型：<asp:DropDownList ID="ddlTypeQ" runat="server">
                <asp:ListItem Text="" Value=""></asp:ListItem>
                <asp:ListItem Text="DI" Value="DI"></asp:ListItem>
                <asp:ListItem Text="AI" Value="AI"></asp:ListItem>
            </asp:DropDownList>
            </li>
        </ul>
        <ul>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
            <li class="li-right">
                <%--<input type="button" class="extBtn" value="整批匯入" data-val='0' data-idx="1" />--%>
                <asp:Button ID="btnExport" runat="server" Visible="false" Text="匯出" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                            <HeaderTemplate>
                                <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="序號" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="MS_SYS_NAME" HeaderText="MAPP群組" SortExpression="MS_SYS_NAME" />
                        <asp:BoundField DataField="Tag_Name" HeaderText="資料點名稱" SortExpression="Tag_Name" />
                        <asp:BoundField DataField="Unit" HeaderText="資料點單位" SortExpression="Unit" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Tag_Type" HeaderText="資料點類型" SortExpression="Tag_Type" ItemStyle-HorizontalAlign="Center" />


                        <asp:BoundField DataField="ALL_Enable" HeaderText="全部啟用" SortExpression="ALL_Enable" />
                        <asp:BoundField DataField="HIHI_Enable" HeaderText="HIHI啟用" SortExpression="HIHI_Enable" />
                        <asp:BoundField DataField="HI_Enable" HeaderText="HI啟用" SortExpression="HI_Enable" />
                        <asp:BoundField DataField="LO_Enable" HeaderText="LO啟用" SortExpression="LO_Enable" />
                        <asp:BoundField DataField="LOLO_Enable" HeaderText="LOLO啟用" SortExpression="LOLO_Enable" />

                        <asp:BoundField DataField="HiHi" HeaderText="HIHI警報值" SortExpression="HiHi" />
                        <asp:BoundField DataField="Hi" HeaderText="HI警報值" SortExpression="Hi" />
                        <asp:BoundField DataField="Lo" HeaderText="LO警報值" SortExpression="Lo" />
                        <asp:BoundField DataField="LoLo" HeaderText="LOLO警報值" SortExpression="LoLo" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                            <HeaderTemplate>
                                刪除
                            </HeaderTemplate>
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="刪除" onclick="deleteItem(<%# Eval("ID") %>)" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerTemplate>
                    </PagerTemplate>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnExport" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
