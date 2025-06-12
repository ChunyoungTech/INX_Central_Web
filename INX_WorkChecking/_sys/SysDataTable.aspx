<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysDataTable.aspx.cs" Inherits="WebApp._sys.SysDataTable" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>
                        連線：<asp:DropDownList ID="ddlConnection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlConnection_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>
                        資料表：<asp:DropDownList ID="ddlDataTable" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDataTable_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" Visible="false" />
                    </li>
                    <li class="li-right">
                        <asp:Button ID="btnCreate" runat="server" Text="產出" />
                    </li>
                </ul>
            </div>
            <div class="GridArea">
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                    GridLines="Vertical" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
<%--                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>--%>
                        <asp:BoundField DataField="Name" HeaderText="欄位名稱" />
                        <asp:BoundField DataField="TypeName" HeaderText="資料類型"/>
                        <asp:BoundField DataField="MaxLength" HeaderText="最大長度" ItemStyle-HorizontalAlign="Center"/>
                        <asp:TemplateField HeaderText="允許NULL" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="ucYesNo1" Value='<%#Eval("AllowDBNull") %>' runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="自動遞增" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="ucYesNo2" Value='<%#Eval("AutoIncrement") %>' runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DefaultValue" HeaderText="預設資料" />
                    </Columns>
                    <PagerTemplate>
                    </PagerTemplate>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <%--<uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />--%>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
