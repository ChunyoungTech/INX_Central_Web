<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucPager.ascx.cs" Inherits="WebApp._uc.ucPager" %>
<asp:Panel ID="Panel1" runat="server" Visible="false">
<div style="display: flex; flex-wrap: wrap; justify-content: center; padding-top: .3em;">
    <asp:Label ID="lblCurrentPage" runat="server" Text="" Visible="false"></asp:Label>
    <div style="display: flex; align-items: center; flex-wrap: wrap;">
        <asp:ImageButton ID="ImageButton1" runat="server" AlternateText="最前頁" ImageUrl="~/_img/First.gif" Height="1.3em" OnClick="ImageButton1_Click" />
        <asp:ImageButton ID="ImageButton2" runat="server" AlternateText="上一頁" ImageUrl="~/_img/Prev.gif" Height="1.3em" OnClick="ImageButton2_Click" />
        <asp:ImageButton ID="ImageButton3" runat="server" AlternateText="下一頁" ImageUrl="~/_img/Next.gif" Height="1.3em" OnClick="ImageButton3_Click" />
        <asp:ImageButton ID="ImageButton4" runat="server" AlternateText="最末頁" ImageUrl="~/_img/Last.gif" Height="1.3em" OnClick="ImageButton4_Click" />
    </div>
    <div style="display: flex; align-items: center; flex-wrap: wrap;">
        ，
        <asp:ImageButton ID="ImageButton5" runat="server" AlternateText="前往" ImageUrl="~/_img/go.gif" Height="1.3em" OnClick="ImageButton5_Click" />
        第 <asp:TextBox ID="txtToGoN" runat="server" Width="3em" style="text-align:center;"></asp:TextBox> / 
        <asp:Label ID="lblTotalPageN" runat="server" Text=""></asp:Label> 頁
        <%--<asp:Button ID="btnToGo" runat="server" Text="Go" />--%>
        ，每頁
        <asp:DropDownList ID="ddlPageSizeN" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSizeN_SelectedIndexChanged">
            <asp:ListItem>10</asp:ListItem>
            <asp:ListItem>20</asp:ListItem>
            <asp:ListItem>30</asp:ListItem>
            <asp:ListItem>40</asp:ListItem>
            <asp:ListItem>50</asp:ListItem>
        </asp:DropDownList>筆，共<asp:Label ID="lblTotalCntQ" runat="server" Text=""></asp:Label>筆
    </div>
    <asp:HiddenField ID="hidGridView" runat="server" />
</div>
</asp:Panel>