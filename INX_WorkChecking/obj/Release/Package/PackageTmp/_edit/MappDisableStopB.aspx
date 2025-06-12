<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="MappDisableStopB.aspx.cs" Inherits="WebApp._edit.MappDisableStopB" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <script type="text/javascript">
        function checkData() {
            $("#<%=hidSelect.ClientID%>").val(checkboxSelectValue(".chkSelect"));
            if ($("#<%=hidSelect.ClientID%>").val() == "") {
                alert("未勾選[解隔離]項目");
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
                    <%--<asp:LinkButton ID="lbRefresh" runat="server" /><input type="hidden" id="hidRefresh" value="" />--%>
                    <asp:Button ID="btnConfirm" runat="server" Text="解隔離" OnClientClick="return checkData()" />
                    <input id="btnCancel" type="button" value="關閉" onclick="parent.CloseAndReload(1, 0);" />
                </div>
                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">批次解隔離</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <div class="QueryArea">
                            <ul>
                                <li>分類：
                                    <asp:DropDownList ID="ddlTypeQ" runat="server" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeQ_SelectedIndexChanged">
                                        <asp:ListItem Text="全部" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </li>
                                <li>設定名稱：
                                    <asp:DropDownList ID="ddlNameQ" runat="server" DataTextField="MS_SYS_NAME" DataValueField="MS_SEQ_ID" AppendDataBoundItems="true" AutoPostBack="true" OnSelectedIndexChanged="ddlNameQ_SelectedIndexChanged">
                                        <asp:ListItem Text="全部" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </li>
                            </ul>
                        </div>
                        <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="False" AllowPaging="true"
                            GridLines="Vertical" Width="100%" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:TemplateField HeaderText="設定名稱" HeaderStyle-Width="10em">
                                    <ItemTemplate>
                                        <input type="text" class="lblTextBox" value='<%#Eval("MS_SYS_NAME") %>' disabled="disabled" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="MD_DATE_START" HeaderText="隔離開始時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                                <asp:BoundField DataField="MD_DATE_END" HeaderText="隔離結束時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" ItemStyle-HorizontalAlign="Center" />
                                <asp:TemplateField HeaderText="隔離原因說明">
                                    <ItemTemplate>
                                        <input type="text" class="lblTextBox" value='<%#Eval("MD_REASON") %>' disabled="disabled" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderStyle-Width="2em" ItemStyle-HorizontalAlign="Center">
                                    <HeaderTemplate>
                                        <input type="checkbox" class="chkAll" data-s=".chkSelect" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <input type="checkbox" class="chkSelect" value='<%#Eval("MD_SEQ_ID") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="NoData">查無符合條件資料</div>
                            </EmptyDataTemplate>
                            <PagerTemplate></PagerTemplate>
                        </asp:GridView>
                        <uc:Pager ID="ucPager" runat="server" TargetID="GridView1" />
                    </div>
                </div>
                <asp:HiddenField ID="hidSelect" runat="server" />
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlTypeQ" EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger ControlID="ddlNameQ" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
