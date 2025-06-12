<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="SysTasks.aspx.cs" Inherits="WebApp._sys.SysTasks" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div>
        <asp:Literal ID="ltlSysTask" runat="server"></asp:Literal>
    </div>
    <div class="QueryArea">
        <ul>
            <li class="li-right">
                <asp:LinkButton ID="lbRefresh" runat="server" Text="Refresh" />
            </li>
        </ul>
    </div>
    <div class="GridArea">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" AllowPaging="false"
                    AllowSorting="true" CssClass="MainGridView Grid100" GridLines="Vertical" ShowHeaderWhenEmpty="true" PagerSettings-Visible="false">
                    <Columns>
                        <asp:BoundField DataField="Key" HeaderText="Key" HeaderStyle-Width="15em" />
                        <asp:BoundField DataField="Name" HeaderText="名稱" HeaderStyle-Width="15em" />
                        <asp:BoundField DataField="LastCall" HeaderText="最近啟動時間" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="10em" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                        <asp:BoundField DataField="LastExec" HeaderText="最近完成時間" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="10em" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                        <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderText="成功" HeaderStyle-Width="3em">
                            <ItemTemplate>
                                <uc:YesNo ID="ynSuccess" runat="server" Value='<%# Eval("Success") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Message" HeaderText="訊息" />
                    </Columns>
                    <PagerSettings Visible="false" />
                    <EmptyDataTemplate>
                        <div class="NoData">查無符合條件資料</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="lbRefresh" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
