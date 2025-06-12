<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Edit.Master" AutoEventWireup="true" CodeBehind="MappMessage.aspx.cs" Inherits="WebApp._edit.MappMessage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lblTextBox {
            border: none;
            background-color: transparent;
            width: 99%;
            color: #000;
        }
    </style>
    <link href="../Content/cyc-select-filter.css" rel="stylesheet" />
    <script src="../Scripts/cyc-select-filter.js"></script>
    <script type="text/javascript">
        function checkData() {
            var type = $("#<%=ddlMM_CONTENT_TYPE.ClientID%>").val();
            var msg = "";
            if ($("#<%=ddlMS_SYS_NAME.ClientID%>").val() == "") {
                msg += "[MAPP設定]不可空白 \n";
            }
            if (type == "1" || type == "4") {
                if ($("#<%=txtMM_TEXT_CONTENT.ClientID %>").val() == "") {
                    msg += "[訊息內容]不可空白 \n";
                }
            } else {
                if ($(".file_upload").val() == "") {
                    msg += "[傳送檔案]不可空白 \n";
                }
            }

            if (msg.length > 0) {
                alert(msg);
            } else {
                if (type == "1" || type == "4") {
                    $("#<%=btnConfirm.ClientID%>").trigger("click");
                } else {
                    var upload = document.getElementById("<%=fileMM_FILE_SHOW_NAME.ClientID%>");
                    if (upload.files.length > 0) {
                        var form = new FormData();
                        form.append("file", upload.files[0]);
                        $.ajax({
                            type: "POST",
                            data: form,
                            cache: false,
                            contentType: false,
                            processData: false,
                            url: "../_api/UploadMappFile.ashx",
                            success: function (data) {
                                var result = $.parseJSON(data);
                                if (!result.Success) {
                                    alert("[上傳檔案]執行錯誤：" + result.Message);
                                } else {
                                    $("#<%=hidMM_FILE_SHOW_NAME.ClientID%>").val(result.Message);
                                    $("#<%=btnConfirm.ClientID%>").trigger("click");
                                }
                                upload.value = "";
                            },
                            error: function () { alert('[上傳檔案]執行錯誤'); upload.value = ""; }
                        });
                    }
                }
            }
            return false;
        }
        $(function () {
            CreateDefault();

            $(document).on("change", "#<%=ddlMM_CONTENT_TYPE.ClientID%>", function () { ChangeSendType(); });

            $(document).on("change", ".file_upload", function (e) {
                var ff = e.target.files[0];
                if (ff && ff.size > 10 * 1024 * 1024) {
                    alert("檔案大小超過限制");
                    $(this).val("");
                }
            });
        });

        function CreateDefault() {
            ChangeSendType();
        }
        function ChangeSendType() {
            var type = $("#<%=ddlMM_CONTENT_TYPE.ClientID%> option:selected").val();
            $(".send-type").hide();
            if (type == "1" || type == "3") {
                $(".send-text").show();
            }
            if (type == "2" || type == "3")
            {
                $(".send-file").show();
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ButtonArea" runat="server">
    <asp:Button ID="btnConfirm" runat="server" Text="發送" Style="display: none;" Visible="false" />
    <asp:Button ID="btnConfirm2" runat="server" Text="發送" OnClientClick="return checkData()" Visible="false" />
    <input id="btnCancel" type="button" value="取消" onclick="parent.CloseAndReload(1, 0);" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="TabTitle" runat="server">
    <li class='tab'><a href="#tabs-1">MAPP發送訊息</a></li>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="TabContent" runat="server">
    <div id="tabs-1" style="padding-top: 3px;">
        <table style="width: 100%;">
            <tr>
                <th class="label write" colspan="4" style="text-align: center; border: .1em solid #378A99; background-color: #FFF; color: #378A99;">訊息資訊</th>
            </tr>
            <tr>
                <th class="label must">MAPP設定</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlMS_SYS_NAME" runat="server" CssClass="cyc-selectfilter" ToolTip="MAPP設定" data-filter-count="50" style="width:99%;" DataTextField="Name" DataValueField="Code"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">訊息類別</th>
                <td colspan="3">
                    <asp:DropDownList ID="ddlMM_CONTENT_TYPE" runat="server" style="width:99%;">
                        <asp:ListItem Text="文字" Value="1" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="檔案、圖片(不含文字)" Value="2"></asp:ListItem>
                        <asp:ListItem Text="檔案、圖片(含文字)" Value="3"></asp:ListItem>
                        <%--<asp:ListItem Text="聊天室" Value="4"></asp:ListItem>--%>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <th class="label must">訊息主旨</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMM_SUBJECT" runat="server" Width="99%" MaxLength="50"></asp:TextBox>
                </td>
            </tr>
            <tr class="send-type send-text">
                <th class="label must">訊息內容</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMM_TEXT_CONTENT" runat="server" TextMode="MultiLine" Rows="8" Width="99%" MaxLength="500"></asp:TextBox>
                </td>
            </tr>
            <%--            <tr class="send-type send-file" style="display:none;">
                <th class="label must">多媒體內容</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMM_MEDIA_CONTENT" runat="server" TextMode="MultiLine" Rows="3" Width="99%" MaxLength="250"></asp:TextBox>
                </td>
            </tr>--%>
            <tr class="send-type send-file" style="display: none;">
                <th class="label must">傳送檔案</th>
                <td colspan="3">
                    <asp:TextBox ID="txtMM_FILE_SHOW_NAME" runat="server" CssClass="lblTextBox" Width="99%" MaxLength="50" Enabled="false"></asp:TextBox>
                    <asp:FileUpload ID="fileMM_FILE_SHOW_NAME" runat="server" CssClass="file_upload" Visible="false" />
                    <asp:HiddenField ID="hidMM_FILE_SHOW_NAME" runat="server" />
                </td>
            </tr>
            <tr style="<%=(Request.QueryString["pa"] == "0" ? "display:none;": "") %>">
                <th class="label read">發送狀態</th>
                <td>
                    <asp:DropDownList ID="ddlMM_SENDED_FLAG" runat="server" Enabled="false" Style="width:10em;">
                        <asp:ListItem Text="未發送" Value="N" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="發送中" Value="P"></asp:ListItem>
                        <asp:ListItem Text="已發送" Value="Y"></asp:ListItem>
                        <asp:ListItem Text="已隔離" Value="D"></asp:ListItem>
                        <asp:ListItem Text="停用" Value="S"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <th class="label read">發送類別</th>
                <td>
                    <asp:DropDownList ID="ddlMM_TYPE" runat="server" Enabled="false" Style="width:10em;">
                        <asp:ListItem Text="自動" Value="A"></asp:ListItem>
                        <asp:ListItem Text="人工" Value="M" Selected="True"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr style="<%=(Request.QueryString["pa"] == "0" ? "display:none;": "") %>">
                <th class="label read">隔離轉發</th>
                <td colspan="3">
                    <asp:Label ID="lblTransName" runat="server" Text=""></asp:Label>
                </td>
            </tr>
<%--            <tr>
                <td colspan="2" style="width:50%;"></td>
                <td colspan="2" style="width:50%;"></td>
            </tr>--%>
        </table>
        <table style="width: 100%; <%=(Request.QueryString["pa"] == "0" ? "display:none;": "") %>">
            <tr>
                <th class="label write" colspan="4" style="text-align: center; border: .1em solid #378A99; background-color: #FFF; color: #378A99;">發送回復資訊</th>
            </tr>
            <tr>
                <th class="label read">發送時間</th>
                <td colspan="3">
                    <asp:Label ID="lblML_SEND_TIME" runat="server" Text=""></asp:Label>
                    <%--<asp:Button ID="btnSend" runat="server" Text="重新發送" Visible="false" OnClientClick="return checkData()" OnClick="btnSend_Click" />--%>
                </td>
            </tr>
            <tr>
                <th class="label read">是否成功</th>
                <td>
                    <asp:Label ID="lblML_IS_SUCCESS" runat="server" Text="" Width="99%"></asp:Label>
                </td>
                <th class="label read">錯誤代碼</th>
                <td>
                    <asp:Label ID="lblML_ERROR_CODE" runat="server" Text="" Width="99%"></asp:Label>
                </td>
            </tr>
            <tr>
                <th class="label read">回復訊息</th>
                <td colspan="3">
                    <asp:Label ID="lblML_DESCRIPTION" runat="server" Text=""></asp:Label>
                </td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(CreateDefault);
    </script>
</asp:Content>
