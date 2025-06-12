<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucDate.ascx.cs" Inherits="WebApp._uc.ucDate" %>
<asp:TextBox ID="txtDate" runat="server" Width="7em" style="width:7em;text-align:center;" OnTextChanged="txtDate_TextChanged" CssClass="cycDate"></asp:TextBox>
<asp:Label ID="lblDate" runat="server" Text="" Visible="false"></asp:Label>