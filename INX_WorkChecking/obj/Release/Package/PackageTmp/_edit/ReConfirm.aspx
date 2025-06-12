<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="ReConfirm.aspx.cs" Inherits="WebApp._edit.ReConfirm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(function () {

            if ($("#<%=txtID.ClientID%>").val().length > 0) {
                $("#<%=txtPWD.ClientID%>").focus();
            } else {
                $("#<%=txtID.ClientID%>").focus();
            }

            $(document).on("click", "#btnConfirm", function (e) {
                e.preventDefault();
                if ($("#<%=txtID.ClientID%>").val() == "") {
                    $("#<%=txtID.ClientID%>").focus();
                    alert("所有欄位均不可空白");
                } else if ($("#<%=txtPWD.ClientID%>").val() == "") {
                    $("#<%=txtPWD.ClientID%>").focus();
                    alert("所有欄位均不可空白");
                } else {
                    $.ajax({
                        type: "post",
                        url: "ReConfirm.aspx/ReLoginCheck?app=" + $.url().param("app"),
                        data: JSON.stringify({ oData: { ID: $("#<%=txtID.ClientID%>").val(), PWD: $("#<%=txtPWD.ClientID%>").val() } }),
                        dataType: "json",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            var rcv = data.d;
                            if (rcv.Success && rcv.Message.length > 0) {
                                $("#hidConfirm").val(rcv.Message);
                                //callback(rcv.Message);
                                parent.$.fancybox.close();
                            } else {
                                alert(rcv.Message);
                            }
                        }, error: function (data) {
                            alert(data.responseText);
                        }
                    });
                    $("#<%=txtPWD.ClientID%>").val("").focus();
                }
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">帳號</th>
                <td>
                    <%--<input type="text" id="txtID" style="width:95%" />--%>
                    <asp:TextBox ID="txtID" runat="server" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">密碼</th>
                <td>
                    <%--<input type="password" id="txtPWD" style="width:95%" />--%>
                    <asp:TextBox ID="txtPWD" runat="server" TextMode="Password" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" style="text-align:center">
                    <input id="btnConfirm" type="submit" value="確定" />
                    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
                    <input id="hidConfirm" type="hidden" value="" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
