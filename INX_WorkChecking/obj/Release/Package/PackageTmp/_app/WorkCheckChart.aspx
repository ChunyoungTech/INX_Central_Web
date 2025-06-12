<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="WorkCheckChart.aspx.cs" Inherits="WebApp._app.WorkCheckChart" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .MainGridView span{
            color:blue;
            text-decoration:underline;
        }
            .MainGridView span:hover {
                cursor: pointer;
                color: red;
                /*font-weight: 600;*/
            }

        div.fix-table {
            width: 100%;
            max-height: calc(100vh - 3.2em - 20px);
            overflow: auto;
            margin:0; padding:0;
        }
        .MainGridView thead {
            position: sticky;
            top: 0;
            z-index: 2;
        }
        .MainGridView .td-right{
            text-align:right;
        }
    </style>
    <script src="../Scripts/chart.4.3.3.min.js"></script>
    <script src="../Scripts/chartjs-plugin-datalabels.min.js"></script>
    <script type="text/javascript">
        const colors = ['#3CB371', '#FF1493', '#9400D3', '#663399', '#E9967A', '#DC143C', '#FF4500', '#FFD700', '#008B8B', '#A0522D', '#4169E1', '#FFC0CB'];
        const options = {
            animation: false,
            responsive: true,
            maintainAspectRatio: false,
            aspectRatio: 3,
            plugins: {
                title: {
                    display: true, Align: 'start', text: '施工單統計', font: { size: 18 }
                },
                legend: { position: 'right', },
                datalabels: {
                    color: '#fff', anchor: 'end', backgroundColor: '#000'
                }
            }
        };

        var chart;
        $(function () {
            chart = new Chart(document.getElementById("pieChart"), {
                type: 'pie',
                data: {
                    labels: [],
                    datasets: [{ data: [], backgroundColor: colors, }],
                },
                options: options,
                plugins: [ChartDataLabels]
            });
        });
        function GetChart() {
            var dataC = JSON.parse($("#<%=hidChartValue.ClientID%>").val());
            if (dataC && dataC.length > 0) {
                var xList = dataC.filter((c) => c.Count > 0);
                if (xList.length) {
                    $(".div-chart").show();
                    chart.data.labels = xList.map(function (item) { return item.Name; });
                    chart.data.datasets[0].data = xList.map(function (item) { return item.Count; });
                    chart.update();
                    <%--$("#<%=GridView1.ClientID%> tbody").append("<tr><td colspan='7'></td>" + dataC.map(function (item) { return "<td style='text-align:right'>" + item.Count + "</td>"; }).join("") + "</tr>");--%>
                } else {
                    $(".div-chart").hide();
                }
            } else {
                $(".div-chart").hide();
            }
        }

        $(function () {
            $(document).on("click", '.MainGridView span.work-type', function () {
                let url = 'WorkCheckChartList.aspx?s=' + $("#<%=hidDateS.ClientID%>").val() + "&e=" + $("#<%=hidDateE.ClientID%>").val() + "&f=" + $("#<%=hidFac.ClientID%>").val() + "&v=" + $(this).attr("data-v") + "&t=" + $(this).attr("data-t");
                OpenWindow(url, '施工清單', .8, .9);
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>
                施工日期：<uc:ucDate ID="dteDateS" runat="server" />~<uc:ucDate ID="dteDateE" runat="server" />
            </li>
            <li>
                廠別：<asp:DropDownList ID="ddlFAC" runat="server"></asp:DropDownList>
            </li>
            <li>
                <asp:Button ID="btnQuery" runat="server" Text="查詢" OnClick="btnQuery_Click" />
            </li>
            <li class="li-right">
                <asp:Button ID="btnExport" runat="server" Text="匯出清冊" OnClick="btnExport_Click" />
            </li>
        </ul>
    </div>
    <div class="GridArea">

        <div style="display:flex;justify-content:center;width:100%;">
            <div class="div-chart" style="height:400px;width:700px;display:none;padding-bottom:10px;">
                <canvas id="pieChart" role="img"></canvas>
            </div>
        </div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="fix-table">
                    <table class="MainGridView Grid100">
                        <thead>
                            <tr>
                                <th><asp:Literal ID="ltlVendor" runat="server" Text="施工廠商"></asp:Literal></th>
                                <th>申請<br/>工單數</th>
                                <th>報到<br/>工單數</th>
                                <th>哨口<br/>報到人數</th>
                                <th>廠務<br/>報到人數</th>
                                <th>工單<br/>報到率</th>
                                <th>人數<br/>報到率</th>
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
                <%--<asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
                    GridLines="None" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
                        <asp:BoundField DataField="Vendor" HeaderText="施工廠商" />
                        <asp:TemplateField HeaderText="申請<br/>工單數" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='' class="work-type"><%#Eval("Cnt01")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Cnt02" HeaderText="報到<br/>工單數" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" HtmlEncode="false" />
                        <asp:BoundField DataField="AccCnt" HeaderText="哨口<br/>報到人數" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" HtmlEncode="false" />
                        <asp:BoundField DataField="ChkCnt" HeaderText="廠務<br/>報到人數" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" HtmlEncode="false" />
                        <asp:BoundField DataField="Rate01" HeaderText="工單<br/>報到率" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:P2}" HtmlEncode="false" />
                        <asp:BoundField DataField="Rate02" HeaderText="人數<br/>報到率" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:P2}" HtmlEncode="false" />
                        <asp:TemplateField HeaderText="一般作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T01' class="work-type"><%#Eval("T01")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="動火作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T02' class="work-type"><%#Eval("T02")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="送電、活線<br/>作業或活線<br/>接近作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T03' class="work-type"><%#Eval("T03")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="高架作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T04' class="work-type"><%#Eval("T04")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="吊掛作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T05' class="work-type"><%#Eval("T05")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="局限空間<br/>作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T06' class="work-type"><%#Eval("T06")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="路面開挖<br/>作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T07' class="work-type"><%#Eval("T07")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Inter-Lock<br/> by pass" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T08' class="work-type"><%#Eval("T08")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="安全防護<br/>系統中斷<br/>/隔離作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T09' class="work-type"><%#Eval("T09")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="危險管路拆<br/>卸鑽孔作業<br/>與化學品<br/>塗佈作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T10' class="work-type"><%#Eval("T10")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="開孔/防墬<br/>安全設施<br/>拆除作業" ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span data-v='<%#Eval("Vendor")%>' data-t='T11' class="work-type"><%#Eval("T11")%></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>--%>
                </div>
                <asp:HiddenField ID="hidChartValue" runat="server" />
                <asp:HiddenField ID="hidDateS" runat="server" />
                <asp:HiddenField ID="hidDateE" runat="server" />
                <asp:HiddenField ID="hidFac" runat="server" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnExport" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(GetChart);
        </script>
</asp:Content>
