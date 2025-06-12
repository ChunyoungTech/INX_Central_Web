<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappDisableEdit.aspx.cs" Inherits="WebApp._edit.MappDisableEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .input-memo{font-size:0.8em;color:red;}
    </style>
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
    <script type="text/javascript">
        function checkData() {

        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">MAPP隔離</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label must">設定分類</th>
                <td>
                    <asp:DropDownList ID="ddlType" runat="server" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" Width="98%" AutoPostBack="true" OnSelectedIndexChanged="ddlType_SelectedIndexChanged"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">隔離MAPP設定</th>
                <td>
                    <asp:DropDownList ID="ddlSetting" runat="server" CssClass="cyc-selectfilter" ToolTip="隔離MAPP設定" data-filter-count="50" DataTextField="Name" DataValueField="ID" Width="98%"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">隔離原因</th>
                <td>
                    <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" Width="98%" MaxLength="250"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label must">隔離開始時間</th>
                <td>
                    <uc:ucDate ID="dteDateS" runat="server" />
                    <asp:DropDownList ID="ddlTimeHour" runat="server"></asp:DropDownList>點
                    <asp:DropDownList ID="ddlTimeMinute" runat="server"></asp:DropDownList>分
                </td>
            </tr>
            <tr>
                <th class="label must">隔離結束時間</th>
                <td>
                    <uc:ucDate ID="dteDateE" runat="server" />
                    <asp:DropDownList ID="ddlTimeHour2" runat="server"></asp:DropDownList>點
                    <asp:DropDownList ID="ddlTimeMinute2" runat="server"></asp:DropDownList>分
                </td>
            </tr>
            <tr>
                <th class="label write">隔離轉發設定</th>
                <td>
                    <asp:DropDownList ID="ddlTransID" runat="server" CssClass="cyc-selectfilter" ToolTip="隔離轉發設定" data-filter-count="50" DataTextField="Name" DataValueField="ID" Width="98%"></asp:DropDownList>
                    <div class="input-memo">做隔離設定時，請增加轉發群組，如果有選，如果有選MAPP發送功能就會轉發，不選就不轉發，符合地震壓降要隔離高階又要發送原群組的需求。地震壓降不設定隔離群組，一律改在隔離設定做</div>
                </td>
            </tr>
            <tr>
                <th class="label must">逾期未解隔離<br />通知頻率</th>
                <td>
                    每<asp:TextBox ID="txtMinites" runat="server" Width="4em" MaxLength="3" Text="60" style="text-align:center;"></asp:TextBox>分鐘
                </td>
            </tr>
            <tr>
                <th class="label write">逾期未解隔離<br />通知設定</th>
                <td>
                    <asp:DropDownList ID="ddlDisableRemind" runat="server" CssClass="cyc-selectfilter" ToolTip="逾期未解隔離通知設定" data-filter-count="50" DataTextField="Name" DataValueField="ID" Width="98%"></asp:DropDownList>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
