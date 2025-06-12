<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="MappSetting.aspx.cs" Inherits="WebApp._alarm.MappSetting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function saveSetting() {
            var mappId = $("#ContentPlaceHolder1_ContentPlaceHolder1_ddlCP_MAPP_TYPE")[0].value;
            var logMappId = $("#ContentPlaceHolder1_ContentPlaceHolder1_log_MAPP_TYPE")[0].value;
            var logSendTime = $("#ContentPlaceHolder1_ContentPlaceHolder1_logSendTime")[0].value;
            var isValid = /^([0-1][0-9]|2[0-3]):([0-5][0-9])$/.test(logSendTime);

            if (!isValid) {
                alert("時間格式不符，範例 13:13");
                return;
            }

            $.ajax({
                type: "POST",
                url: "../_alarm/MappSetting.aspx/SaveSetting",
                data: JSON.stringify({ mappId: mappId, logMappId: logMappId, logSendTime: logSendTime }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    if (data.d != true) {
                        alert("儲存設定失敗")
                    } else {
                        alert("儲存設定成功")
                    }
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="QueryArea">
        <input type="button" onclick="saveSetting()" value="儲存" />
    </div>
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">斷線警報群組</th>
                <td>
                    <asp:DropDownList ID="ddlCP_MAPP_TYPE" runat="server" AppendDataBoundItems="true" DataTextField="Name" DataValueField="ID">
                        <asp:ListItem Text="" Value=""></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">變更紀錄通知群組</th>
                <td>
                    <asp:DropDownList ID="log_MAPP_TYPE" runat="server" AppendDataBoundItems="true" DataTextField="Name" DataValueField="ID">
                        <asp:ListItem Text="" Value=""></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">變更紀錄通知時間</th>
                <td>
                    <asp:TextBox runat="server" ID="logSendTime"></asp:TextBox></td>
            </tr>
        </table>
    </div>
</asp:Content>
