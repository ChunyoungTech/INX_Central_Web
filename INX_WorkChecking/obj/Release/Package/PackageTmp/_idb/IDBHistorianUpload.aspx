<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IDBHistorianUpload.aspx.cs" Inherits="WebApp._idb.IDBHistorianUpload" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Scripts/jquery.multiselect.css" rel="stylesheet" />
    <script src="../Scripts/jquery.multiselect.js"></script>
    <script>
        $(function () {
            $(document).on("click", "#chkItemAll", function () {
                $(".chkItem input").prop("checked", $(this).prop("checked"));
            });
            CreateDefault();
        });

        function CreateMutliSelect() {
            $('select[multiple]').multiselect({
                selectAll: true,
                texts: {
                    placeholder: '請選擇',
                    //search: 'Search States',
                    selectAll: '全選',
                    unselectAll: '取消全選',
                    selectedOptions: "筆資料已選擇",
                }
            });
        }

        function CreateDefault() {
            CreateMutliSelect();
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
<%--                <div style="padding:.3rem .5rem">
                    廠別：<asp:ListBox ID="lstFactory" runat="server" DataTextField="Code" DataValueField="ID" SelectionMode="Multiple" AutoPostBack="true" OnSelectedIndexChanged="lstFactory_SelectedIndexChanged"></asp:ListBox>
                </div>
                <div style="padding:.3rem .5rem">
                    系統別：<asp:ListBox ID="lstSystem" runat="server" SelectionMode="Multiple"></asp:ListBox>
                </div>--%>
                <ul>
                    <li>廠別：<asp:DropDownList ID="ddlFactory" runat="server" DataTextField="Code" DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="ddlFactory_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                    <li>系統別：<asp:DropDownList ID="ddlSystem" runat="server"></asp:DropDownList>
                    </li>
                    <li>TagName：<asp:TextBox ID="txtTagName" runat="server"></asp:TextBox>
                    </li>
                    <li><asp:CheckBox ID="chkOnly" runat="server" Text="只顯示異常點位" /></li>
                    <li style="flex-grow:1;display:flex;justify-content:end;">
                        <asp:Button ID="btnExport" runat="server" Text="匯出" BackColor="white" ForeColor="black" OnClick="btnExport_Click" />
                    </li>
                </ul>
                <ul>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" />
                    </li>
                    <li style="flex-grow:1;display:flex;justify-content:end;">
                        <asp:Button ID="btnUpdate" runat="server" Text="儲存" BackColor="Red" ForeColor="White" OnClick="btnUpdate_Click" />
                    </li>
                </ul>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="GridArea">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100" PagerSettings-Visible="false"
                    GridLines="None" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="FacName" HeaderText="廠別" SortExpression="FacName" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="SysName" HeaderText="系統別" SortExpression="SysName" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="TagName" HeaderText="點位名稱" SortExpression="TagName" />
                        <asp:BoundField DataField="TagDesc" HeaderText="點位描述" />
                        <asp:BoundField DataField="LastTime" HeaderText="資料時間" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" ItemStyle-HorizontalAlign="Center" />
                        <asp:TemplateField HeaderStyle-Width="2.5em" ItemStyle-HorizontalAlign="Center">
                            <HeaderTemplate>
                                上傳<br /><input type="checkbox" id="chkItemAll" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="chkEnabled" CssClass="chkItem" runat="server" Checked='<%#Eval("Enabled") %>' />
                                <asp:Label ID="lblSeqID" runat="server" Text='<%#Eval("SeqID") %>' Visible="false"></asp:Label>
                                <asp:Label ID="lblTagID" runat="server" Text='<%#Eval("TagID") %>' Visible="false"></asp:Label>
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
            <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnExport" />
        </Triggers>
    </asp:UpdatePanel>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(CreateDefault);
    </script>
</asp:Content>
