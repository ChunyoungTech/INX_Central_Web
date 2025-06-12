<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="ChangePWD.aspx.cs" Inherits="WebApp._edit.ChangePWD" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        $(function () {

            $("#<%=txtPwdOld.ClientID%>").focus();

            $(document).on("click", "#btnConfirm", function (e) {
                e.preventDefault();
                if ($("#<%=txtPwdOld.ClientID%>").val() == "" || $("#<%=txtPwdNew.ClientID%>").val() == "" || $("#<%=txtPwdNew2.ClientID%>").val() == "") {
                    alert("[所有欄位]均不可空白");
                } else if ($("#<%=txtPwdNew.ClientID%>").val() != $("#<%=txtPwdNew2.ClientID%>").val()) {
                    alert("[新密碼]與[確認新密碼]必須相同");
                } else if ($("#<%=txtPwdNew.ClientID%>").val() == $("#<%=txtPwdOld.ClientID%>").val()) {
                    alert("[新密碼]與[原密碼]不可相同");
                } else {
                    $.ajax({
                        type: "post",
                        url: "ChangePWD.aspx/Change",
                        data: JSON.stringify({ oData: { O: $("#<%=txtPwdOld.ClientID%>").val(), N: $("#<%=txtPwdNew.ClientID%>").val(), N2: $("#<%=txtPwdNew2.ClientID%>").val() } }),
                        dataType: "json",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            var rcv = data.d;
                            if (rcv.Success) {
                                alert("密碼修改完成");
                                parent.$.fancybox.close();
                            } else {
                                alert(rcv.Message);
                            }
                        }, error: function (data) {
                            alert(data.responseText);
                        }
                    });
                    $("#<%=txtPwdOld.ClientID%>").val("").focus();
                }
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">原密碼</th>
                <td>
                    <asp:TextBox ID="txtPwdOld" runat="server" TextMode="Password" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">新密碼</th>
                <td>
                    <asp:TextBox ID="txtPwdNew" runat="server" TextMode="Password" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">確認新密碼</th>
                <td>
                    <asp:TextBox ID="txtPwdNew2" runat="server" TextMode="Password" Width="95%"></asp:TextBox>
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
