<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysLicense.aspx.cs" Inherits="WebApp._sys.SysLicense" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div style="width: 97%; border: .1em solid gray; margin: .2em 0; padding: .5em;">
                    MAC ADDRESS：<asp:DropDownList ID="ddlMacList" runat="server"></asp:DropDownList>
                </div>
                <div style="width: 97%; border: .1em solid gray; margin: .2em 0; padding: .5em;">
                    密碼：<asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Width="10em"></asp:TextBox>
                </div>
                <div style="width: 97%; border: .1em solid gray; margin: .2em 0; padding: .5em;">
                    授權數量：<asp:TextBox ID="txtQty" runat="server" Width="8em" MaxLength="6"></asp:TextBox>
                </div>
                <div style="width: 97%; border: .1em solid gray; margin: .2em 0; padding: .5em;">
                    序號：<asp:TextBox ID="txtLicense" runat="server" Width="30em"></asp:TextBox>
                </div>
                <div style="width: 97%; border: .1em solid gray; margin: .5em 0; padding: .5em;">
                    <asp:Button ID="btnRun" runat="server" Text="1.產生序號" OnClick="btnRun_Click" />
                    <asp:Button ID="btnUpdate" runat="server" Text="2.更新至SysSetting" OnClick="btnUpdate_Click" />
                    <asp:Button ID="btnReload" runat="server" Text="3.重新套用序號" OnClick="btnReload_Click" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
