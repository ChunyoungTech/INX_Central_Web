<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="WorkCheckListTemp.aspx.cs" Inherits="WebApp._edit.WorkCheckListTemp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .GridYellow tr:nth-child(odd) td {
            background-color: transparent;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab' style="display:none;"><a href="#tabs-1"></a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100 GridYellow"
        GridLines="Vertical" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true">
        <RowStyle BackColor="#fff3b0" />
        <AlternatingRowStyle BackColor="#ffe66d" />
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="姓名" />
            <asp:BoundField DataField="Supplier" HeaderText="廠商" />
            <asp:BoundField DataField="Date" HeaderText="哨口入廠時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
        </Columns>
        <PagerTemplate>
        </PagerTemplate>
        <EmptyDataTemplate>
            <div class="NoData">
                查無符合條件資料      
            </div>
        </EmptyDataTemplate>
    </asp:GridView>
    <br />
    <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
        GridLines="Vertical" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true">
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="姓名" />
            <asp:BoundField DataField="Supplier" HeaderText="廠商" />
            <asp:BoundField DataField="Date" HeaderText="廠務報到時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
        </Columns>
        <PagerTemplate>
        </PagerTemplate>
        <EmptyDataTemplate>
            <div class="NoData">
                查無符合條件資料      
            </div>
        </EmptyDataTemplate>
    </asp:GridView>
    <br />
    <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100"
        GridLines="Vertical" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true">
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="姓名" />
            <asp:BoundField DataField="Supplier" HeaderText="廠商" />
            <asp:BoundField DataField="Date" HeaderText="廠務報退時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
        </Columns>
        <PagerTemplate>
        </PagerTemplate>
        <EmptyDataTemplate>
            <div class="NoData">
                查無符合條件資料      
            </div>
        </EmptyDataTemplate>
    </asp:GridView>
    <br />
    <asp:GridView ID="GridView4" runat="server" AutoGenerateColumns="false" CssClass="MainGridView Grid100 GridYellow"
        GridLines="Vertical" AllowSorting="false" AllowPaging="false" ShowHeaderWhenEmpty="true">
        <RowStyle BackColor="#fff3b0" />
        <AlternatingRowStyle BackColor="#ffe66d" />
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="姓名" />
            <asp:BoundField DataField="Supplier" HeaderText="廠商" />
            <asp:BoundField DataField="Date" HeaderText="哨口離廠時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
        </Columns>
        <PagerTemplate>
        </PagerTemplate>
        <EmptyDataTemplate>
            <div class="NoData">
                查無符合條件資料      
            </div>
        </EmptyDataTemplate>
    </asp:GridView>
    </div>
</asp:Content>
