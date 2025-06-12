<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="TransferData.aspx.cs" Inherits="WebApp._idb.TransferData" %>
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

        function CreateDefault() {

        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="QueryArea">
                <ul>
                    <li>
                        日期區間：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
                    </li>
                    <li>
                        TagName：<asp:TextBox ID="txtTagName" runat="server"></asp:TextBox>
                    </li>
                </ul>
                <ul>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" />
                    </li>
                    <li style="flex-grow:1;display:flex;justify-content:end;">
                        <asp:Button ID="btnExport" runat="server" Text="匯出" BackColor="white" ForeColor="black" OnClick="btnExport_Click" />
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
                        <asp:BoundField DataField="tf_data_source" HeaderText="資料型態" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="tf_tagname" HeaderText="資料點名稱" SortExpression="tf_tagname" />
                        <asp:BoundField DataField="tf_value" HeaderText="數值" />
                        <asp:BoundField DataField="tf_ack_flag" HeaderText="ACK_FLAG" />
                        <asp:BoundField DataField="tf_sn" HeaderText="SN" />
                        <asp:BoundField DataField="created" HeaderText="產生時間" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" ItemStyle-HorizontalAlign="Center" />
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
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(CreateDefault);
    </script>
</asp:Content>
