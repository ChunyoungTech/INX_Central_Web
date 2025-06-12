<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ucDept.ascx.cs" Inherits="WebApp._uc.ucDept" %>
<div style="display: inline">
    <asp:DropDownList ID="ddlDept" runat="server" OnSelectedIndexChanged="ddlDept_SelectedIndexChanged">
    </asp:DropDownList>
    <asp:CheckBox ID="chkInclude" runat="server" Text="含所屬" Checked="true" OnCheckedChanged="chkInclude_CheckedChanged" />
</div>

