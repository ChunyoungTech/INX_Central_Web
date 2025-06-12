<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="MappEVExtList.aspx.cs" Inherits="WebApp._edit.MappEVExtList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%--<link href="../Content/grid.1.1.css" rel="stylesheet" />--%>
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <script type="text/javascript">
        var extOpt = [
            { reCtl: "#<%=btnQuery.ClientID%>", reHid: "#hidRefresh", Width: .9, Sub: "25" }];
        InitExt(extOpt);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="buttonArea">
                    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
                    <asp:Button ID="btnQuery" runat="server" Text="" style="display:none;" />
                </div>
                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">地震壓降MAPP加發</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="false" AllowSorting="false" AllowPaging="false"
                            GridLines="Vertical" Width="100%" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                            <Columns>
                                <asp:TemplateField HeaderStyle-Width="3em" ItemStyle-HorizontalAlign="Center">
                                    <HeaderTemplate>
                                        <input type="button" class="extBtn" value="新增" data-val='0,<%=iID %>' data-idx="0" data-height=".9" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <input type="button" class="extBtn" value="編輯" data-val='<%#Eval("ID")%>,<%=iID %>' data-idx="0" data-height=".9" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Name" HeaderText="地震壓降設定名稱" />
                                <asp:BoundField DataField="TypeName" HeaderText="分類" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="AreaName" HeaderText="廠區" ItemStyle-HorizontalAlign="Center" />
                                <asp:TemplateField HeaderText="高階" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                    <ItemTemplate>
                                        <uc:YesNo ID="ucYesNo" Value='<%#Eval("IsTop") %>' runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="NormalCode" HeaderText="MAPP加發設定" />
                                <asp:TemplateField HeaderText="CIM<br/>啟用" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                    <ItemTemplate>
                                        <uc:YesNo ID="ucYesNo2" Value='<%#Eval("CimEnable") %>' runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="CIM<br/>分組" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3em">
                                    <ItemTemplate>
                                        <%#Eval("CIMGroup")%>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="NoData">查無符合條件資料</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                                    <%--<uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />--%>
<%--                        <table style="width: 100%;">
                            <tr>
                                <th class="label must">地震壓降設定</th>
                                <td>
                                    <asp:Label ID="lblName" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" valign="top">

                                </td>
                            </tr>
                        </table>--%>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
