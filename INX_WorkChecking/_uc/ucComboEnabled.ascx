<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucComboEnabled.ascx.cs" Inherits="WebApp._uc.ucComboEnabled" %>
<div class="ucComboEnabled">
    <asp:DropDownList ID="ddlMain" runat="server"></asp:DropDownList>
    <asp:DropDownList ID="ddlEnabled" runat="server" style="display:none;"></asp:DropDownList>
    <asp:Label ID="lblEnabled" runat="server" Text="" ForeColor="Red" Font-Size="X-Small"></asp:Label>
</div>
