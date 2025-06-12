<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappEVLatest.aspx.cs" Inherits="WebApp._mapp.MappEVLatest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
        .MainGridView input{
            text-align:center;
        }
    </style>
    <script type="text/javascript">
<%--        var extOpt = [
            { reCtl: "#<%=lbRefresh.ClientID%>", reHid: "#hidRefresh", Width: .6, Sub: "18" }];
        InitExt(extOpt);--%>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <ul>
                    <li>分類：<asp:DropDownList ID="ddlTypeQ" runat="server">
                        <asp:ListItem Text="地震" Value="E"></asp:ListItem>
                        <asp:ListItem Text="壓降" Value="P"></asp:ListItem>
                    </asp:DropDownList>
                    </li>
                    <li>廠區：<asp:DropDownList ID="ddlAreaQ" runat="server">
                        <asp:ListItem Text="南廠" Value="1"></asp:ListItem>
                        <asp:ListItem Text="北廠" Value="2"></asp:ListItem>
                    </asp:DropDownList>
                    </li>
                    <li>
                        <asp:Button ID="btnQuery" runat="server" Text="查詢" OnClick="btnQuery_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="清除" OnClick="btnClear_Click" />
                    </li>
                    <li style="float: right;">
                        <asp:Button ID="btnPreview" runat="server" Text="預覽" OnClick="btnPreview_Click" />
                        <asp:Button ID="btnSend" runat="server" Text="發送" OnClick="btnSend_Click" Enabled="false" />
                    </li>
                </ul>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div style="padding:0.5em;margin:0.5em;">
                    資料來源：
                    <asp:TextBox ID="txtSource" runat="server" Text="SCADA"></asp:TextBox>
                    <asp:HiddenField ID="hidData" runat="server" />
                </div>
                <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="false" AllowSorting="false" AllowPaging="false"
                    GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false" Visible="false">
                    <Columns>
                        <asp:BoundField DataField="FacName" HeaderText="廠別" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="5em" />
                        <asp:TemplateField HeaderText="觸發日期" HeaderStyle-Width="8em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtDate" runat="server" Text='<%#Eval("DateStr") %>' ></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="觸發時間" HeaderStyle-Width="8em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtTime" runat="server" Text='<%#Eval("TimeStr") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="地震最大gal數" HeaderStyle-Width="6em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue1" runat="server" Text='<%#Eval("Value1") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="地震級數" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue2" runat="server" Text='<%#Eval("Value2") %>' ReadOnly="true"></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="地震持續時間" HeaderStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue3" runat="server" Text='<%#Eval("Value3") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="NoData">查無符合條件資料</div></EmptyDataTemplate>
                </asp:GridView>
                <asp:GridView ID="GridView2" runat="server" CssClass="MainGridView" AutoGenerateColumns="false" AllowSorting="false" AllowPaging="false"
                    GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false" Visible="false">
                    <Columns>
                        <asp:BoundField DataField="FacName" HeaderText="廠別" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="5em" />
                        <asp:TemplateField HeaderText="觸發日期" HeaderStyle-Width="8em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtDate" runat="server" Text='<%#Eval("DateStr") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="觸發時間" HeaderStyle-Width="8em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtTime" runat="server" Text='<%#Eval("TimeStr") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="壓降剩餘電量(%)" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue1" runat="server" Text='<%#Eval("Value1") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="壓降前用電量" HeaderStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue2" runat="server" Text='<%#Eval("Value2") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="壓降後用電量" HeaderStyle-Width="5em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue3" runat="server" Text='<%#Eval("Value3") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="落點區域" HeaderStyle-Width="6em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue4" runat="server" Text='<%#Eval("Value4") %>' ReadOnly="true"></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="持續秒數" HeaderStyle-Width="4em">
                            <ItemTemplate>
                                <asp:TextBox ID="txtValue5" runat="server" Text='<%#Eval("Value5") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="NoData">查無符合條件資料</div></EmptyDataTemplate>
                </asp:GridView>
                <asp:Panel ID="Panel1" runat="server">
                    MAPP訊息預覽：<br />
                    <asp:TextBox ID="txtPreview" runat="server" TextMode="MultiLine" Rows="12" Width="60%" ReadOnly="true"></asp:TextBox>
                </asp:Panel>
                
                <%--<uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />--%>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnPreview" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
