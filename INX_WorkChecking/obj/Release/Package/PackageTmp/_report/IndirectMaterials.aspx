<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="IndirectMaterials.aspx.cs" Inherits="WebApp._report.IndirectMaterials" %>
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
                scrollable: true,
                columnNum: 2
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <h3 style="padding-left:1em;">間材(扣除非廠務)</h3>
        <ul>
            <li>
                廠別：<asp:DropDownList ID="ddlFAC" runat="server"></asp:DropDownList>
            </li>
            <li>
                年度：<asp:TextBox ID="txtYear" runat="server" MaxLength="4" Width="4em" TextMode="Number"></asp:TextBox>
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
                <asp:HiddenField ID="hidAuth" runat="server" />

                <div class="fix-table">
                    <table class="MainGridView Grid100">
                        <thead>
                            <asp:Literal ID="ltlContent01" runat="server"></asp:Literal>
                        </thead>
                        <tbody>
                            <asp:Literal ID="ltlContent02" runat="server"></asp:Literal>
                        </tbody>
                    </table>
                </div>

                <%--<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView" PagerSettings-Visible="false"
                    GridLines="Both" AllowSorting="true" AllowPaging="false" ShowHeaderWhenEmpty="true">
                    <Columns>
                        <asp:BoundField DataField="FAC" HeaderText="廠別" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em" />
                        <asp:BoundField DataField="Level01" HeaderText="費用" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:HiddenField ID="hidCategoryID" runat="server" Value='<%#Eval("CategoryID") %>' />
                                <asp:TextBox ID="txtMonth01" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month01") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth02" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month02") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth03" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month03") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth04" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month04") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth05" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month05") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth06" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month06") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth07" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month07") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth08" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month08") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth09" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month09") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth10" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month10") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth11" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month11") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtMonth12" runat="server" Width="5em" style="text-align:right" MaxLength="10" Text='<%#Eval("Month12") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerTemplate>
                    </PagerTemplate>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>--%>
                <%--<uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />--%>
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
