<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="MappDisableEditB.aspx.cs" Inherits="WebApp._edit.MappDisableEditB" %>

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
        $(function () {
            $(document).on("click", "input.addBtn", function () {
                if ($("#selectList tbody input[data-id='" + $(this).attr("data-id") + "']").length == 0) {
                    $("#selectList tbody").append(String.format("<tr><td>{0}</td><td><input type='button' value='移除' class='removeBtn' data-id='{1}' /></td></tr>", $(this).attr("data-text"), $(this).attr("data-id")));
                }
            });
            $(document).on("click", "input.addAll", function () {
                $("input.addBtn").each(function () {
                    if ($("#selectList tbody input[data-id='" + $(this).attr("data-id") + "']").length == 0) {
                        $("#selectList tbody").append(String.format("<tr><td>{0}</td><td><input type='button' value='移除' class='removeBtn' data-id='{1}' /></td></tr>", $(this).attr("data-text"), $(this).attr("data-id")));
                    }
                });
            });
            $(document).on("click", "input.removeBtn", function () {
                $(this).parent().parent().remove();
            });
            $(document).on("click", "input.removeAll", function () {
                $("#selectList tbody").empty();
            });
        });

        function checkData() {
            $("#<%=hidID.ClientID%>").val($("input.removeBtn").map(function () { return $(this).attr("data-id"); }).get().join(","));
            if ($("#<%=hidID.ClientID%>").val().length == 0) {
                alert("未選擇[MAPP設定]");
                return false;
            }
        }

        String.format = function () {
            var s = arguments[0];
            for (var i = 0; i < arguments.length - 1; i++) {
                var reg = new RegExp("\\{" + i + "\\}", "gm");
                s = s.replace(reg, arguments[i + 1]);
            }
            return s;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <div class="buttonArea">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hidID" runat="server" />
                    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
                    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnConfirm" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
        <div id="tabs">
            <ul class='etabs'>
                <li class='tab'><a href="#tabs-1">批次隔離</a></li>
            </ul>
            <div id="tabs-1" style="padding-top: 3px;">
                <div class="QueryArea">
                    <ul>
                        <li>分類：
                            <asp:DropDownList ID="ddlTypeQ" runat="server" DataTextField="MT_TYPE_NAME" DataValueField="MT_SEQ_ID" AppendDataBoundItems="true">
                                <asp:ListItem Text="全部" Value=""></asp:ListItem>
                            </asp:DropDownList>
                        </li>
                        <li>設定名稱：
                            <asp:TextBox ID="txtNameQ" runat="server"></asp:TextBox>
                        </li>
                        <li>
                            <asp:Button ID="btnQuery" runat="server" Text="查詢" OnClick="btnQuery_Click" />
                        </li>
                    </ul>
                </div>
                <table style="width: 100%;">
                    <tr>
                        <th class="label write" style="text-align: center; width: 25%;">符合查詢條件
                        </th>
                        <th class="label write" style="text-align: center; width: 25%;">已選擇設定
                        </th>
                        <th class="label write" style="text-align: center; width: 50%;">隔離
                        </th>
                    </tr>
                    <tr>
                        <td style="vertical-align: top;">
                            <div style="height: 85vh; overflow: auto;">
                                <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView" AutoGenerateColumns="False" AllowSorting="false" AllowPaging="false"
                                            GridLines="Vertical" Width="100%" ShowHeaderWhenEmpty="true">
                                            <Columns>
                                                <asp:TemplateField HeaderText="設定名稱" HeaderStyle-Width="10em">
                                                    <ItemTemplate>
                                                        <input type="text" class="lblTextBox" value='<%#Eval("MS_SYS_NAME") %>' disabled="disabled" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderStyle-Width="2em">
                                                    <HeaderTemplate>
                                                        <input type="button" value="全加入" class="addAll" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <input type="button" value="加入" class="addBtn" data-id='<%#Eval("MS_SEQ_ID") %>' data-text='<%#Eval("MS_SYS_NAME") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <EmptyDataTemplate>
                                                <div class="NoData">查無符合條件資料</div>
                                            </EmptyDataTemplate>
                                            <PagerTemplate></PagerTemplate>
                                        </asp:GridView>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btnQuery" EventName="Click" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </td>
                        <td style="vertical-align: top;">
                            <div style="height: 85vh; overflow: auto;">
                                <table id="selectList" class="MainGridView" style="width: 99%;">
                                    <thead>
                                        <tr>
                                            <th>設定名稱</th>
                                            <th style="width: 2em;">
                                                <input type="button" class="removeAll" value="全移除" /></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                    </tbody>
                                </table>
                            </div>
                        </td>
                        <td style="vertical-align: top;">
                            <table style="width: 99%;">
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
                                    <th class="label must">未解隔離通知<br />(每N分鐘)</th>
                                    <td>
                                        <asp:TextBox ID="txtMinites" runat="server" Width="5em" MaxLength="3" Text="60"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <th class="label write">逾期未解隔離<br />通知設定</th>
                                    <td>
                                        <asp:DropDownList ID="ddlDisableRemind" runat="server" DataTextField="MS_SYS_NAME" DataValueField="MS_SEQ_ID" Width="98%"></asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
