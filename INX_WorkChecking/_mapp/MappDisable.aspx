<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappDisable.aspx.cs" Inherits="WebApp._mapp.MappDisable" %>

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
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "14" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .9, Sub: "15" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .6, Sub: "16" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "19" },
            { reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "20" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <ul>
                    <li>MAPP設定：
                        <asp:DropDownList ID="ddlSetting" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP設定" data-filter-count="50" AppendDataBoundItems="true" DataTextField="Name" DataValueField="ID">
                            <asp:ListItem Text="全部" Value="0"></asp:ListItem>
                        </asp:DropDownList>
                    </li>
                </ul>
                <ul>
                    <li>分類：
                        <asp:DropDownList ID="ddlTypeQ" runat="server" CssClass="cyc-selectfilter" ToolTip="分類" data-filter-count="50" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" AppendDataBoundItems="true"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlTypeQ_SelectedIndexChanged">
                            <asp:ListItem Text="全部" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </li>
                    <li>隔離期間：<uc:ucDate ID="dteDateS" runat="server" />
                        ~<uc:ucDate ID="dteDateE" runat="server" />
                    </li>
                    <li>狀態：<asp:DropDownList ID="ddlStop" runat="server">
                        <asp:ListItem Text="全部" Value=""></asp:ListItem>
                        <asp:ListItem Text="隔離中" Value="1" Selected="true"></asp:ListItem>
                        <asp:ListItem Text="解隔離" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" /></li>
                    <li class="li-right">
                        <input type="button" class="extBtn" value="批次隔離" data-val='0' data-idx="4" data-height=".9" data-t="&nbsp;" />
                        <input type="button" class="extBtn" value="批次解隔離" data-val='0' data-idx="3" data-height=".9" data-t="&nbsp;" />
                    </li>
                </ul>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="fix-table">
                    <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView Grid100" AutoGenerateColumns="False" AllowSorting="true" AllowPaging="true"
                        GridLines="None" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                        <Columns>
                            <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-HorizontalAlign="Center">
                                <HeaderTemplate>
                                    <input type="button" class="extBtn" value="新增" data-val='0' data-idx="0" data-height=".8" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("MD_SEQ_ID") %>' data-idx="0" data-height=".8" style='<%#(int)Eval("MD_STOP_USER")==0 ? "": "display:none;" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="MAPP設定" SortExpression="MS_SYS_NAME" HeaderStyle-Width="10em">
                                <ItemTemplate>
                                    <input type="text" class="lblTextBox" value='<%#Eval("MS_SYS_NAME") %>' disabled="disabled" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="MD_DATE_START" HeaderText="隔離開始時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" SortExpression="MD_DATE_START" />
                            <asp:BoundField DataField="MD_DATE_END" HeaderText="隔離結束時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" SortExpression="MD_DATE_END" />
                            <asp:TemplateField HeaderText="隔離原因說明">
                                <ItemTemplate>
                                    <input type="text" class="lblTextBox" value='<%#Eval("MD_REASON") %>' disabled="disabled" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="MD_STOP_USER_NAME" HeaderText="解隔人員" HeaderStyle-Width="4em" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="MD_STOP_TIME" HeaderText="解隔時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                            <%--                        <asp:BoundField DataField="UPDATE_USER" HeaderText="更新人員" HeaderStyle-Width="4em" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="UPDATE_TIME" HeaderText="更新時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />--%>
                            <asp:TemplateField HeaderStyle-Width="3.5em" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="檢視記錄" data-val='<%#Eval("MD_SEQ_ID") %>' data-idx="1" data-height=".9" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderStyle-Width="3.5em" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <input type="button" class="extBtn" value="解隔離" data-val='<%#Eval("MD_SEQ_ID") %>' data-idx="2" style='<%#(int)Eval("MD_STOP_USER")==0 ? "": "display:none;" %>' />
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
