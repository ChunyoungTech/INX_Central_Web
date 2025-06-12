<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappTestSend.aspx.cs" Inherits="WebApp._mapp.MappTestSend" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table style="width:99%">
                    <tr>
                        <td style="text-align:right;">MAPP API(團隊)：</td>
                        <td>
                            <asp:TextBox ID="txtMappApi01" runat="server" Width="50em"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">MAPP API(交談室)：</td>
                        <td>
                            <asp:TextBox ID="txtMappApi02" runat="server" Width="50em"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">發送群組：</td>
                        <td>
                            <asp:DropDownList ID="ddlSetting" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP設定" data-filter-count="50" DataTextField="Name" DataValueField="ID" Width="98%"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">訊息主題：</td>
                        <td>
                            <asp:TextBox ID="txtSubject" runat="server" Width="98%"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">訊息內容：</td>
                        <td>
                            <asp:TextBox ID="txtContent" runat="server" Width="98%"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;"></td>
                        <td>
                            <asp:Button ID="btnSend" runat="server" Text="測試發送" OnClick="btnSend_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align:right;">Message：</td>
                        <td>
                            <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>

    </div>
</asp:Content>
