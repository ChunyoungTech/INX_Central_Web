<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="TagAlarmStatus.aspx.cs" 
Inherits="WebApp._alarm.TagAlarmStatus" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var extOpt = [{ reCtl: "#<%=btnQuery.ClientID%>", Width: .8, Sub: "41" }];
        InitExt(extOpt);

        function stopAlarm() {
            $.ajax({
                type: "GET",
                url: "<%=cyc.Shared.SysQuery.GetAppSettingValue("stopAlarmUpdater")%>",
                success: function (data) {
                    alert(data.message);
                }
            });
        }
        function startAlarm() {
            $.ajax({
                type: "GET",
                url: "<%=cyc.Shared.SysQuery.GetAppSettingValue("startAlarmUpdater")%>",
                success: function (data) {
                    alert(data.message);
                }
            });
        }
        setInterval(function () { $("#ContentPlaceHolder1_ContentPlaceHolder1_btnQuery").click() }, 60000);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <ul>
            <li>部門：<uc:ucDept ID="ddlDeptQ" runat="server" isShowAll="false" isNoInclude="true" />
            </li>
            <li>資料點名稱：<asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
            </li>
            <li class="li-right">
                <input type="button" class="extBtn" value="停止警報伺服器" onclick="stopAlarm()">
                <input type="button" class="extBtn" value="啟動警報伺服器" onclick="startAlarm()">
            </li>
            <asp:Button ID="btnQuery" runat="server" Text="查詢" />
        </ul>
        <ul>
            <li>
                <asp:CheckBoxList ID="chkType" runat="server" RepeatDirection="Horizontal" RepeatColumns="5">
                    <asp:ListItem Text="全部禁能" Value="1" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="HIHI禁能" Value="2" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="HI禁能" Value="3" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="LO禁能" Value="4" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="LOLO禁能" Value="5" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="HIHI現況值" Value="6" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="HI現況值" Value="7" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="LO現況值" Value="8" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="LOLO現況值" Value="9" Selected="False"></asp:ListItem>
                    <asp:ListItem Text="通訊異常" Value="10" Selected="False"></asp:ListItem>
                </asp:CheckBoxList>
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
<%--                <asp:LinkButton ID="lbRefresh" runat="server" />
                <input type="hidden" id="hidRefresh" value="" /><input type="hidden" id="hidGuid" value="" />--%>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView"
                    GridLines="Vertical" AllowSorting="true" AllowPaging="true" ShowHeaderWhenEmpty="true" OnDataBound="GridView1_DataBound" OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="Tag_Name" HeaderText="資料點名稱" SortExpression="Tag_Name" />
                        
<%--                        <asp:TemplateField HeaderText="全部啟用" SortExpression="ALL_Enable" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="yn001" runat="server" Value='<%#Eval("tall") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="HIHI啟用" SortExpression="HIHI_Enable" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="yn002" runat="server" Value='<%#Eval("thihi") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="HI啟用" SortExpression="HI_Enable" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="yn003" runat="server" Value='<%#Eval("thi") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="LO啟用" SortExpression="LO_Enable" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="yn004" runat="server" Value='<%#Eval("tlo") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="LOLO啟用" SortExpression="LOLO_Enable" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <uc:YesNo ID="yn005" runat="server" Value='<%#Eval("tlolo") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>--%>

                        <asp:BoundField DataField="tall" HeaderText="全部禁能" SortExpression="tall" />
                        <asp:BoundField DataField="thihi" HeaderText="HIHI禁能" SortExpression="thihi" />
                        <asp:BoundField DataField="thi" HeaderText="HI禁能" SortExpression="thi" />
                        <asp:BoundField DataField="tlo" HeaderText="LO禁能" SortExpression="tlo" />
                        <asp:BoundField DataField="tlolo" HeaderText="LOLO禁能" SortExpression="tlolo" />

                        <asp:BoundField DataField="HiHi_Limit" HeaderText="HIHI現況值" SortExpression="HiHi_Limit" />
                        <asp:BoundField DataField="Hi_Limit" HeaderText="HI現況值" SortExpression="Hi_Limit" />
                        <asp:BoundField DataField="Lo_Limit" HeaderText="LO現況值" SortExpression="Lo_Limit" />
                        <asp:BoundField DataField="LoLo_Limit" HeaderText="LOLO現況值" SortExpression="LoLo_Limit" />
                        <asp:BoundField DataField="u_date" HeaderText="更新時間" SortExpression="u_date" DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" />
                        <asp:BoundField DataField="quality" HeaderText="通訊品質" SortExpression="quality" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <input type="button" class="extBtn" value="變更紀錄" data-val='<%# Eval("tag_data_id") %>' data-idx="0" />
                            </ItemTemplate>
                            <HeaderStyle Width="4em" />
                            <ItemStyle HorizontalAlign="Center" />
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
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
