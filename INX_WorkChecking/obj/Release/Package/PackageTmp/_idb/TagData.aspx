<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="TagData.aspx.cs" Inherits="WebApp._idb.TagData" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
/*         .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }

       div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 150px);
            overflow: auto;
            margin: 0;
            padding: 0;
        }

        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }*/
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "26", Dir: "_idb", Width: .5 }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>廠別：<asp:DropDownList ID="ddlFactory" runat="server" DataTextField="Code" DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="ddlFactory_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>系統別：<asp:DropDownList ID="ddlSystem" runat="server"></asp:DropDownList>
                    </li>
                    <li>TagName：<asp:TextBox ID="txtTagName" runat="server"></asp:TextBox>
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" />
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
                                <input type="button" class="extBtn" value="編輯" data-val='<%# Eval("ID") %>' data-idx="0" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="序號" SortExpression="ID" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Tag_Name" HeaderText="資料點名稱" SortExpression="Tag_Name" />
                        <asp:BoundField DataField="Tag_Desc" HeaderText="資料點描述" />
                        <asp:BoundField DataField="FacName" HeaderText="廠別" SortExpression="FacName" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="SysName" HeaderText="系統別" SortExpression="SysName" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Unit" HeaderText="資料點單位" SortExpression="Unit" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="Tag_Type" HeaderText="資料點類型" SortExpression="Tag_Type" ItemStyle-HorizontalAlign="Center" />

<%--                        <asp:BoundField DataField="HiHi_Limit" HeaderText="HIHI警報值" ItemStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="Hi_Limit" HeaderText="HI警報值" ItemStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="Lo_Limit" HeaderText="LO警報值" ItemStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="LoLo_Limit" HeaderText="LOLO警報值" ItemStyle-HorizontalAlign="Right" />--%>
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
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
