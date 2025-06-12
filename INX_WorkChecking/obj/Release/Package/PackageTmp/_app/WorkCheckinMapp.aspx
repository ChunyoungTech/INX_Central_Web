<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="WorkCheckinMapp.aspx.cs" Inherits="WebApp._app.WorkCheckinMapp" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .lblTextbox{
            width:99%;
            border:none;
            background-color:transparent;
            color:#000;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "7" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "8" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "9" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .7, Sub: "10" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>施工日期：<uc:ucDate ID="dteDateS" runat="server" />
                ~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li>
                廠別：
                <asp:DropDownList ID="ddlFAC" runat="server">
                </asp:DropDownList>
            </li>
            <li>
                施工單號&廠商：<asp:TextBox ID="txtNumber" runat="server"></asp:TextBox>
            </li>
            <li>
                身份證號或姓名：<asp:TextBox ID="txtName" runat="server"></asp:TextBox>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
            <li class="li-right">
                <asp:Button ID="btnExport" runat="server" Text="匯出" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:BoundField DataField="EV_DATE" HeaderText="違規時間" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="11em" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" />
                        <asp:BoundField DataField="SHORT_NAME" HeaderText="廠商名稱" SortExpression="SHORT_NAME" ItemStyle-Width="9em" />
                        <asp:BoundField DataField="APPLY_PK" HeaderText="工單號碼" SortExpression="APPLY_PK" ItemStyle-Width="9em" />
                        <asp:BoundField DataField="ID" HeaderText="違規人員" ItemStyle-Width="7em" />
                        <asp:BoundField DataField="P_NAME" HeaderText="姓名" ItemStyle-Width="5em" />
                        <%--<asp:BoundField DataField="ERROR_CODE" HeaderText="違規事由" />--%>
                        <asp:TemplateField HeaderText="違規事由">
                            <ItemTemplate>
                                <asp:TextBox ID="txtError" runat="server" CssClass="lblTextbox" Text='<%#Eval("ERROR_CODE") %>' Enabled="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerTemplate>
                    </PagerTemplate>
                    <EmptyDataTemplate>
                        <div class="NoData">
                            查無符合條件資料
                        </div>
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
