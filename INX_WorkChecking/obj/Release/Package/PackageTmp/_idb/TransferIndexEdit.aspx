<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="TransferIndexEdit.aspx.cs" Inherits="WebApp._idb.TransferIndexEdit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function checkData() {
            var msg = "";
            if ($("#<%=txt_ind_tagname.ClientID %>").val() == "") {
                msg += "[ind_tagname]不可空白 \n";
            }
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">TransferIndex</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">ind_no</th>
                <td>
                    <asp:TextBox ID="txt_ind_no" runat="server" Width="95%" MaxLength="10"></asp:TextBox>
                </td>
                <th class="label write">ind_source_note</th>
                <td>
                    <asp:TextBox ID="txt_ind_source_note" runat="server" Width="95%" MaxLength="40"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">scada_tagname</th>
                <td colspan="3">
                    <asp:TextBox ID="txt_scada_tagname" runat="server" Width="98%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">ind_tagname</th>
                <td colspan="3">
                    <asp:TextBox ID="txt_ind_tagname" runat="server" Width="98%" MaxLength="40"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">ind_fac</th>
                <td>
                    <asp:TextBox ID="txt_ind_fac" runat="server" Width="95%" MaxLength="6"></asp:TextBox>
                </td>
                <th class="label write">ind_fab</th>
                <td>
                    <asp:TextBox ID="txt_ind_fab" runat="server" Width="95%" MaxLength="6"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">ind_System</th>
                <td>
                    <asp:TextBox ID="txt_ind_System" runat="server" Width="95%" MaxLength="6"></asp:TextBox>
                </td>
                <th class="label write">ind_unit</th>
                <td>
                    <asp:TextBox ID="txt_ind_unit" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">ind_eqp_group</th>
                <td>
                    <asp:TextBox ID="txt_ind_eqp_group" runat="server" Width="95%" MaxLength="50"></asp:TextBox>
                </td>
                <th class="label write">ind_section</th>
                <td>
                    <asp:TextBox ID="txt_ind_section" runat="server" Width="95%" MaxLength="30"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">ind_Common</th>
                <td colspan="3">
                    <asp:TextBox ID="txt_ind_Common" runat="server" Width="98%" MaxLength="70"></asp:TextBox>
                </td>
            </tr>

            <tr>
                <th class="label write">ind_level</th>
                <td>
                    <asp:TextBox ID="txt_ind_level" runat="server" Width="95%" MaxLength="1"></asp:TextBox>
                </td>
                <th class="label write">ind_priority</th>
                <td>
                    <asp:TextBox ID="txt_ind_priority" runat="server" Width="95%" MaxLength="3"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">ind_col_index</th>
                <td>
                    <asp:TextBox ID="txt_ind_col_index" runat="server" Width="95%" MaxLength="20"></asp:TextBox>
                </td>
                <th class="label write">ind_row_index</th>
                <td>
                    <asp:TextBox ID="txt_ind_row_index" runat="server" Width="95%" MaxLength="30"></asp:TextBox>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
