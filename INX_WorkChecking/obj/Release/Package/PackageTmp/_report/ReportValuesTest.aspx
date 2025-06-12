<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="ReportValuesTest.aspx.cs" Inherits="WebApp._report.ReportValuesTest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    Message：<asp:Label ID="Label1" runat="server" Text=""></asp:Label>
    <table class="MainGridView">
        <tr>
            <td>
                FactoryExpenseAnalysis
            </td>
            <td>
                <asp:Button ID="Button1" runat="server" Text="Create" OnClick="Button1_Click" />
            </td>
            <td>
                <asp:Button ID="Button2" runat="server" Text="Clear" OnClick="Button2_Click" />
            </td>
        </tr>
    </table>
</asp:Content>
