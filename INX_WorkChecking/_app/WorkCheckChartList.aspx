<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="WorkCheckChartList.aspx.cs" Inherits="WebApp._app.WorkCheckChartList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 10px);
            overflow: auto;
            margin: 0;
            padding: 0;
        }

        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }

        .MainGridView span.con_number {
            color: blue;
            text-decoration: underline;
        }

            .MainGridView span.con_number:hover {
                cursor: pointer;
                color: red;
                /*font-weight: 600;*/
            }

        .MainGridView .td-right {
            text-align: right;
        }

        .MainGridView .td-center {
            text-align: center;
        }
    </style>
    <script type="text/javascript">
        $(function () {
            $(document).on("click", '.MainGridView span.con_number', function () {
                OpenWindow('../_edit/WorkCheckView.aspx?pa=' + $(this).text(), '工單明細', .6, .8);
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="fix-table">
        <table class="MainGridView Grid100">
            <thead>
                <tr>
                    <th>工單號</th>
                    <th>工單日期</th>
                    <th>施工廠別</th>
                    <th>施工廠商</th>
                    <th>哨口<br/>報到人數</th>
                    <th>廠務<br/>報到人數</th>
                    <th>一般作業</th>
                    <th>動火作業</th>
                    <th>送電、活線<br/>作業或活線<br/>接近作業</th>
                    <th>高架作業</th>
                    <th>吊掛作業</th>
                    <th>局限空間<br/>作業</th>
                    <th>路面開挖<br/>作業</th>
                    <th>Inter-Lock<br/> by pass</th>
                    <th>安全防護<br/>系統中斷<br/>/隔離作業</th>
                    <th>危險管路拆<br/>卸鑽孔作業<br/>與化學品<br/>塗佈作業</th>
                    <th>開孔/防墬<br/>安全設施<br/>拆除作業</th>
                </tr>
            </thead>
            <tbody>
                <asp:Literal ID="ltlContent" runat="server"></asp:Literal>
            </tbody>
        </table>

        <%--<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100" OnDataBound="GridView1_DataBound"
            GridLines="None" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
            <Columns>
                <asp:TemplateField HeaderText="工單號">
                    <ItemTemplate>
                        <span class="con_number"><%#Eval("con_number")%></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="con_date" HeaderText="工單日期" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="Fac" HeaderText="施工廠別" />
                <asp:BoundField DataField="Vendor" HeaderText="施工廠商" />
                <asp:BoundField DataField="AccCnt" HeaderText="哨口<br/>報到人數" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" HtmlEncode="false" />
                <asp:BoundField DataField="ChkCnt" HeaderText="廠務<br/>報到人數" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" HtmlEncode="false" />
                <asp:TemplateField HeaderText="一般作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T01") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="動火作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T02") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="送電、活線<br/>作業或活線<br/>接近作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T03") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="高架作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T04") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="吊掛作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T05") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="局限空間<br/>作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T06") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="路面開挖<br/>作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T07") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Inter-Lock<br/> by pass" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T08") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="安全防護<br/>系統中斷<br/>/隔離作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T09") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="危險管路拆<br/>卸鑽孔作業<br/>與化學品<br/>塗佈作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T10") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="開孔/防墬<br/>安全設施<br/>拆除作業" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <uc:YesNo ID="ucYes" runat="server" Yes="V" No="" ValueInt='<%#Eval("T11") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="NoData">查無符合條件資料</div>
            </EmptyDataTemplate>
        </asp:GridView>--%>
    </div>
</asp:Content>
