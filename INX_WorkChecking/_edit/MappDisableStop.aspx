<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="MappDisableStop.aspx.cs" Inherits="WebApp._edit.MappDisableStop" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .validmsg {
            font-size: small;
            color: red;
            margin-top: -.3em;
            line-height: .5em;
            display: block;
        }
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <script type="text/javascript">
        function checkData() {
            if (!Page_ClientValidate()) {
                return false;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="buttonArea">
                    <asp:HiddenField ID="hidID" runat="server" />
                    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" ValidationGroup="btnConfirm" />
                    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
                </div>
                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">MAPP隔離設定</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <table style="width: 100%;">
                            <tr>
                                <th class="label read">MAPP類別</th>
                                <td>
                                    <asp:Label ID="lblSetting" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <th class="label read">隔離原因</th>
                                <td>
                                    <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="lblTextBox" Enabled="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="label read">隔離開始時間</th>
                                <td>
                                    <asp:TextBox ID="txtDateS" runat="server" CssClass="lblTextBox" Enabled="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="label read">隔離結束時間</th>
                                <td>
                                    <asp:TextBox ID="txtDateE" runat="server" CssClass="lblTextBox" Enabled="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="label read">解隔離人員</th>
                                <td>
                                    <asp:Label ID="lblStopUser" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
