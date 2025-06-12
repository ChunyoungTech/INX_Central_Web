<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="SysRoleEdit.aspx.cs" Inherits="WebApp._edit.SysRoleEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[名稱]不可空白 \n";
            }
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
    </script>
    <style type="text/css">
        .td_full{width:100%;}
        .td_half{width:50%;}
        .td_label{display:inline-block;width:3em;background-color:#378A99; color:white;text-align:right; line-height:1.2em; padding:.1em;position:fixed; }
        .td_input{display:inline-block;width:100%;}
        .td_input div{margin-left:7.4em;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">群組資料</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">名稱</th>
                <td>
                    <asp:TextBox ID="txtName" runat="server" Width="90%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">單位權限</th>
                <td>
                    <asp:DropDownList ID="ddlLevelNo" runat="server">
                        <asp:ListItem Text="所屬" Value="0"></asp:ListItem>
                        <asp:ListItem Text="全部" Value="1"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label write">是否為預設群組</th>
                <td>
                    <asp:CheckBox ID="chkDefault" runat="server" />
                </td>
            </tr>
            <tr>
                <th class="label write">啟用</th>
                <td>
                    <asp:CheckBox ID="chkEnabled" runat="server" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
