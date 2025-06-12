<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="WorkCheckView.aspx.cs" Inherits="WebApp._edit.WorkCheckView" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 100%;
            color: #000;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:HiddenField ID="hidID" runat="server" />
    <asp:Button ID="btnConfirm" runat="server" Text="儲存" Visible="false" />
    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">工單明細</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr style="display:none;">
                <th class="label write">工單備註</th>
                <td colspan="3">
                    <asp:TextBox ID="txtRemark" runat="server" Width="99%" MaxLength="255"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">工程編號</th>
                <td>
                    <asp:TextBox ID="labPROJECT" runat="server" CssClass="lblTextBox" Enabled="false"></asp:TextBox>
                </td>
                <th class="label write">工單號碼</th>
                <td>
                    <asp:TextBox ID="lblNumber" runat="server" CssClass="lblTextBox" Enabled="false"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th class="label write">施工日期</th>
                <td>
                    <asp:Label ID="lblDate" runat="server"></asp:Label>
                </td>
                <th class="label write">施工時間</th>
                <td>
                    <asp:Label ID="labTime" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">施工廠別</th>
                <td>
                    <asp:Label ID="lblFAC" runat="server"></asp:Label>
                </td>
                <th class="label write">無塵室名稱</th>
                <td>
                    <asp:Label ID="lblFAB" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">主要區域</th>
                <td>
                    <asp:Label ID="lblMainArea" runat="server"></asp:Label>
                </td>
                <th class="label write">次要區域</th>
                <td>
                    <asp:Label ID="lblSecondArea" runat="server"></asp:Label>
                </td>
            </tr>

            <tr>
                <th class="label write">廠商名稱</th>
                <td colspan="3">
                    <asp:Label ID="lblVendor" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別1</th>
                <td colspan="3">
                    <asp:Label ID="lblType1" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別2</th>
                <td colspan="3">
                    <asp:Label ID="lblType2" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別3</th>
                <td colspan="3">
                    <asp:Label ID="lblType3" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別4</th>
                <td colspan="3">
                    <asp:Label ID="lblType4" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別5</th>
                <td colspan="3">
                    <asp:Label ID="lblType5" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別6</th>
                <td colspan="3">
                    <asp:Label ID="lblType6" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">作業類別7</th>
                <td colspan="3">
                    <asp:Label ID="lblType7" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">施作內容</th>
                <td colspan="3">
                    <asp:Label ID="lblContent" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">群創工程師</th>
                <td colspan="3">
                    <asp:Label ID="lblEngineer" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label write">施工負責人</th>
                <td>
                    <asp:Label ID="lblVendorPE" runat="server"></asp:Label>
                </td>
                <th class="label write">施工代理人</th>
                <td>
                    <asp:Label ID="labSAFE_NAME" runat="server"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
