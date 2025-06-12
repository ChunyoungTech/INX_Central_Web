<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappSendLog.aspx.cs" Inherits="WebApp._mapp.MappSendLog" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
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
            /*position:absolute;*/
        }

        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
    </style>
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "13" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "19" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <ul>
                    <li>MAPP設定：
                        <asp:DropDownList ID="ddlNameQ" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP設定" data-filter-count="50" AppendDataBoundItems="true" DataTextField="NameA" DataValueField="Name">
                            <asp:ListItem Text="全部" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </li>
                </ul>
                <ul>
                    <li>分類：
                        <asp:DropDownList ID="ddlTypeQ" runat="server" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" AppendDataBoundItems="true"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlTypeQ_SelectedIndexChanged">
                            <asp:ListItem Text="全部" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </li>
                    <li>發送日期：
                        <uc:ucDate ID="dteDateS" runat="server" />
                        ~<uc:ucDate ID="dteDateE" runat="server" />
                    </li>
                    <li style="display: none;">設定部門：
                        <uc:ucDept ID="ddlDeptQ" runat="server" />
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
                </ul>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="fix-table">
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                        GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:BoundField DataField="MS_SYS_NAME" HeaderText="MAPP設定" SortExpression="MS_SYS_NAME" />
                            <asp:BoundField DataField="MM_SUBJECT" HeaderText="訊息主旨" />
                            <asp:BoundField DataField="UPDATE_TIME" HeaderText="產生時間" SortExpression="UPDATE_TIME" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" HeaderStyle-Width="10em" />
                            <asp:BoundField DataField="ML_SEND_TIME" HeaderText="發送時間" SortExpression="ML_SEND_TIME" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" HeaderStyle-Width="10em" />
                            <asp:BoundField DataField="ML_IS_SUCCESS" HeaderText="是否成功" SortExpression="ML_IS_SUCCESS" HeaderStyle-Width="2em" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="MS_SYS_DEPT_NAME" HeaderText="設定部門" />
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="4em">
                                <HeaderTemplate>
                                    <input type="button" class="extBtn" value="新增訊息" data-val='0' data-idx="0" data-height=".8" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="訊息內容" data-val='<%# Eval("MM_SEQ_ID") %>' data-idx="0" data-height=".8" />
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
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
