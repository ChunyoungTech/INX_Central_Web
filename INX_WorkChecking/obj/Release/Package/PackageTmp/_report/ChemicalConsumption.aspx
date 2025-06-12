<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="ChemicalConsumption.aspx.cs" Inherits="WebApp._report.ChemicalConsumption" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Content/InxCentralReport.1.0.css" rel="stylesheet" />
    <script src="../Scripts/freeze-table.min.js"></script>
    <script src="InxCentralReport.1.0.js"></script>
    <script>
        $(function () {
            UpdateProcess("#<%=btnUpdate.ClientID%>", "#<%=hidAuth.ClientID%>", "#<%=btnQuery.ClientID%>", "<%=Option.TableName%>");
        });
        function FixTable() {
            $(".fix-table").freezeTable({
                //freezeHead: true,
                freezeColumn: true,
                //scrollable: true,
                columnNum: 1,
            });
            $(".fix-table2").freezeTable({
                //freezeHead: true,
                //freezeColumn: true,
                scrollable: true,
                //columnNum: 2,
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <h3 style="padding-left: 1em;"><%=Option.ReportName %></h3>
        <ul>
<%--            <li>類別：<asp:DropDownList ID="ddlLevel01" runat="server"></asp:DropDownList>
                <asp:DropDownList ID="ddlLevel02" runat="server" Visible="false"></asp:DropDownList>
            </li>
            <li>廠別：<asp:DropDownList ID="ddlFAC" runat="server"></asp:DropDownList>
            </li>--%>
            <li>年度：<asp:TextBox ID="txtYear" runat="server" MaxLength="4" Width="4em" TextMode="Number"></asp:TextBox>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" />
            </li>
            <li class="li-right">
                <asp:Button ID="btnUpdate" runat="server" Text="更新" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><asp:HiddenField ID="hidAuth" runat="server" />

                <asp:Literal ID="ltlContent01" runat="server"></asp:Literal>

                <asp:Literal ID="ltlContent02" runat="server"></asp:Literal>

            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FixTable);
    </script>
</asp:Content>
