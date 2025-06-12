<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappEVSetting.aspx.cs" Inherits="WebApp._edit.MappEVSetting" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
    <script type="text/javascript">
        $(function () {
            CreateDefault();
            $(document).on("change", "#<%=rblTop.ClientID%> input[type=radio]", function () {
                CreateDefault();
            });
        });
        function checkData() {
            var msg = "";
            if ($("#<%=txtCode.ClientID %>").val() == "") {
                msg += "[廠別代號]不可空白 \n";
            }
            if ($("#<%=txtName.ClientID %>").val() == "") {
                msg += "[設定名稱]不可空白 \n";
            }
            if (msg.length > 0) {
                alert(msg);
                return false;
            }
        }
        function CreateDefault() {
            if ($("#<%=rblTop.ClientID%> input[type=radio]:checked").val() == 'Y') {
                $(".top-cont").show();
            } else {
                $(".top-cont").hide();
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <%--<asp:HiddenField ID="hidID" runat="server" />--%><asp:HiddenField ID="hidKey" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">地震壓降MAPP設定</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">分類</th>
                <td>
                    <asp:DropDownList ID="ddlType" runat="server" Width="10em">
                        <asp:ListItem Text="地震" Value="E"></asp:ListItem>
                        <asp:ListItem Text="壓降" Value="P"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <th class="label must">廠區</th>
                <td>
                    <asp:DropDownList ID="ddlArea" runat="server" Width="10em">
                        <asp:ListItem Text="南廠" Value="1"></asp:ListItem>
                        <asp:ListItem Text="北廠" Value="2"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">廠別代號</th>
                <td>
                    <asp:TextBox ID="txtCode" runat="server" Width="10em" MaxLength="10"></asp:TextBox>
                </td>
                <th class="label must">是否為高階</th>
                <td>
                    <asp:RadioButtonList ID="rblTop" runat="server" RepeatDirection="Horizontal" Width="10em" RepeatLayout="Flow">
                        <asp:ListItem Text="否" Value="N" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="是" Value="Y"></asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
            <tr>
                <th class="label must">設定名稱</th>
                <td colspan="3">
                    <asp:TextBox ID="txtName" runat="server" Width="98%" MaxLength="25"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">MAPP發送設定</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlNormalID" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP發送設定" data-filter-count="50" Width="98%" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>
<%--            <tr>
                <th class="label must">正式發送設定</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlNormalID" runat="server" Width="98%" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label write">隔離發送設定</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlDisableID" runat="server" Width="98%" DataTextField="Name" DataValueField="ID"></asp:DropDownList>
                </td>
            </tr>--%>
            <tr>
                <th class="label must">發送主旨</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMappSubject" runat="server" Width="98%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr class="top-cont">
                <th class="label write">發送抬頭</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMappContentH" runat="server" TextMode="MultiLine" Rows="2" Width="98%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">發送內容</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMappContent" runat="server" TextMode="MultiLine" Rows="6" Width="98%" MaxLength="400"></asp:TextBox>
                </td>
            </tr>
            <tr class="top-cont">
                <th class="label write">發送結尾</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMappContentF" runat="server" TextMode="MultiLine" Rows="2" Width="98%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">CIM啟用</th>
                <td>
                    <asp:DropDownList ID="ddlCimEnable" runat="server" Width="10em">
                        <asp:ListItem Text="否" Value="0"></asp:ListItem>
                        <asp:ListItem Text="是" Value="1"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <th class="label write">CIM彙整比重</th>
                <td>
                    <asp:DropDownList ID="ddlCimLevel" runat="server" Width="10em">
                        <asp:ListItem Text="1" Value="1"></asp:ListItem>
                        <asp:ListItem Text="0" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label write">CIM方法</th>
                <td>
                    <asp:DropDownList ID="ddlCimMethod" runat="server" Width="10em">
                        <asp:ListItem Text="" Value=""></asp:ListItem>
                        <asp:ListItem Text="POST" Value="POST"></asp:ListItem>
                        <asp:ListItem Text="GET" Value="GET"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <th class="label write">CIM彙整分組</th>
                <td>
                    <asp:DropDownList ID="ddlCimGroup" runat="server" Width="10em">
                        <asp:ListItem Text="0" Value="0"></asp:ListItem>
                        <asp:ListItem Text="1" Value="1"></asp:ListItem>
                        <asp:ListItem Text="2" Value="2"></asp:ListItem>
                        <asp:ListItem Text="3" Value="3"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr >
                <th class="label write">CIM網址</th>
                <td colspan="3">
                    <asp:TextBox ID="txtCimWebApi" runat="server" Width="98%" MaxLength="200"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">CIM參數</th>
                <td colspan="3">
                    <asp:TextBox ID="txtCimParaData" runat="server" Width="98%" MaxLength="200"></asp:TextBox>
                </td>
            </tr>

            <tr>
                <th class="label read">說明</th>
                <td colspan="3">
                    <ul style="list-style:decimal;margin-left:1em;">
                        <li>[地震]可使用標籤：<asp:Label ID="lblTemplate1" runat="server" Text=""></asp:Label></li>
                        <li>[壓降]可使用標籤：<asp:Label ID="lblTemplate2" runat="server" Text=""></asp:Label></li>
                        <li>[發送主旨]、[發送內容]、[發送抬頭]、[發送結尾]、[CIM參數]，均可使用</li>
                        <li>[CIM參數]如有多個，請使用 '&' 分隔，例：scale="2"&occurTime={發報日期} {發報時間}</li>
                        <li>[CIM比重]為高階發送時參考，若各廠彙整比重加總>0時，才會發送高階CIM</li>
                    </ul>
                </td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(CreateDefault);
    </script>
</asp:Content>
