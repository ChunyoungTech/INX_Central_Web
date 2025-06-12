<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="WorkCheckInList.aspx.cs" Inherits="WebApp._edit.WorkCheckInList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .tagSelect {
            list-style: none;
            width: 99%;
            background-color: #FFF;
        }

            .tagSelect li {
                padding: .15em 0 .15em .2em;
                background-color: #E3EAEB;
            }

                .tagSelect li:nth-child(2n+1) {
                    background-color: #FFF;
                }

            .tagSelect input {
                margin-right: .2em;
            }
    </style>
    <script type="text/javascript">
        $(function () {
            $(document).on("click", "#btnAllAdd", function () {
                $(".btnInsert:enabled").each(function () {
                    if ($(".tagSelect li[data-id='" + $(this).attr("data-id") + "']").length == 0) {
                        if ($(".tagSelect li .btnRemove[data-code='" + $(this).attr("data-code") + "']").length == 0) {
                            $(".tagSelect").append("<li data-id='" + $(this).attr("data-id") + "'><input type='button' class='btnRemove' value='移除' data-code='" + $(this).attr("data-code") + "' />" + $(this).attr("data-name") + "-" + $(this).attr("data-supplier") + "</li>");
                            $(".btnInsert[data-code='" + $(this).attr("data-code") + "']").prop("disabled", true);
                        }
                    }
                });
            });
            $(document).on("click", ".btnInsert", function () {
                if ($(".tagSelect li[data-id='" + $(this).attr("data-id") + "']").length == 0) {
                    if ($(".tagSelect li .btnRemove[data-code='" + $(this).attr("data-code") + "']").length == 0) {
                        $(".tagSelect").append("<li data-id='" + $(this).attr("data-id") + "'><input type='button' class='btnRemove' value='移除' data-code='" + $(this).attr("data-code") + "' />" + $(this).attr("data-name") + "-" + $(this).attr("data-supplier") + "</li>");
                        $(".btnInsert[data-code='" + $(this).attr("data-code") + "']").prop("disabled", true);
                    } else {
                        alert("此人員已選取");
                    }
                } else {
                    alert("此人員已選取");
                }
            });
            $(document).on("click", ".btnRemove", function () {
                $(".btnInsert[data-code='" + $(this).attr("data-code") + "']").prop("disabled", false);
                $(this).parent().remove();
            });

            checkEnabled();
        });

        function checkData() {
            $("#<%=hidValue.ClientID%>").val($(".tagSelect li").map(function () { return $(this).attr("data-id") }).get().join(','));
            return true;
        }
        function checkEnabled() {
            $(".btnRemove").each(function () {
                $(".btnInsert[data-code='" + $(this).attr("data-code") + "']").prop("disabled", true);
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="buttonArea">
                    <asp:HiddenField ID="hidValue" runat="server" />
                    <asp:Button ID="btnConfirm" runat="server" Text="確定" OnClientClick="return checkData()" />
                    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div id="tabs">
            <ul class='etabs'>
                <li class='tab'><a href="#tabs-1">選擇簽到人員</a></li>
            </ul>
            <div id="tabs-1" style="padding-top: 3px;">
                <table style="width: 100%;">
                    <tr>
                        <th class="label write" style="width: 60%; text-align: center;">辨識系統登錄資料</th>
                        <th class="label write" style="width: 40%; text-align: center;">簽到人員</th>
                    </tr>
                    <tr>
                        <td valign="top">
                            <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <div style="height: 30em; overflow: auto;">
                                        廠商：<asp:DropDownList ID="ddlSupplier" runat="server" OnSelectedIndexChanged="ddlSupplier_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>

                                        <asp:GridView ID="GridView1" runat="server" CssClass="MainGridView Grid100" AutoGenerateColumns="False" AllowSorting="False" AllowPaging="False"
                                            GridLines="Vertical">
                                            <Columns>
                                                <asp:BoundField DataField="FRUserID" HeaderText="證號" />
                                                <asp:BoundField DataField="FRUserName" HeaderText="姓名" />
                                                <asp:BoundField DataField="SupplierName" HeaderText="廠商" />
                                                <asp:BoundField DataField="LogDateTime" HeaderText="登錄時間" DataFormatString="{0:yyyy/MM/dd HH:mm}" HeaderStyle-Width="9em" />
                                                <asp:TemplateField HeaderStyle-Width="2em" ItemStyle-HorizontalAlign="Center">
                                                    <HeaderTemplate>
                                                        <input type="button" id="btnAllAdd" value="全加入" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <input type="button" class="btnInsert" value="加入" data-id='<%#Eval("IFP_RecognitionAuth_ID") %>' data-code='<%#Eval("FRUserID") %>' data-name='<%#Eval("FRUserName") %>' data-supplier='<%#Eval("SupplierName") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <EmptyDataTemplate>
                                                <div class="NoData">查無符合條件資料</div>
                                            </EmptyDataTemplate>
                                            <PagerTemplate></PagerTemplate>
                                        </asp:GridView>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                        <td valign="top">
                            <div style="height: 30em; overflow: auto;">
                                <ul class="tagSelect">
                                    <asp:Literal ID="ltlCont" runat="server"></asp:Literal>
                                </ul>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(checkEnabled);
    </script>
</asp:Content>
