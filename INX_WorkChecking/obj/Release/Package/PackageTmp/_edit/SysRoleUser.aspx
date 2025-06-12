<%@ Page Title="" Language="C#" MasterPageFile="~/_master/EditS.Master" AutoEventWireup="true" CodeBehind="SysRoleUser.aspx.cs" Inherits="WebApp._edit.SysRoleUser" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        select option:disabled{
            color:#BBB;
        }
    </style>
    <script type="text/javascript">
        var ID = $.url().param("pa");
        function closethis() { parent.jQuery.fancybox.close(); }

        function AddUser() {
            var s = $("#lstDepEmp option:selected");
            var a = $("#lstSelect");
            s.each(function () {
                if ($("#lstSelect option[value='" + $(this).val() + "']").length == 0) {
                    a.append("<option value='" + $(this).val() + "'>" + $(this).text() + "</option>");
                    $(this).prop("disabled", true);
                }
                $(this).prop("selected", false);
            });
        }

        function DelUser() {
            $("#lstSelect option:selected").each(function () {
                $(this).remove();
                $("#lstDepEmp option[value='" + $(this).val() + "']").prop("disabled", false);
            });
        }

        function GetEmpData(d) {
            $.ajax({
                type: "GET",
                url: "../_Query/GetDepUser.ashx?Limit=false&Dep=" + d,
                success: function (data) {
                    $("#lstDepEmp").empty();
                    var myarray = $.parseJSON(data);
                    $.each(myarray, function (i, item) {
                        
                        if ($("#lstSelect option[value='" + item.ID + "']").length == 0) {
                            $("#lstDepEmp").append("<option value='" + item.ID + "'>" + item.Name + "</option>");
                        } else {
                            $("#lstDepEmp").append("<option value='" + item.ID + "' disabled='true'>" + item.Name + "</option>");
                        }
                    });
                }
            });
        }

        $(function () {
            $.ajax({
                type: "GET",
                url: "../_Query/GetDepUser.ashx?Limit=false",
                success: function (data) {
                    $("#tree").tree({
                        //'autoOpen': 0,
                        'data': $.parseJSON(data)
                    });
                }
            });

            $('#tree').bind(
                'tree.click',
                function (event) {
                    var node = event.node;
                    var depNo = node.id;
                    GetEmpData(depNo);
                    //$('#tree').tree('toggle', node);
                }
            );

            $(document).on("change", "#txtCode,#txtName", function () {
                if ($(this).val().trim() == "") {
                    $("#txtCode").val("");
                    $("#txtName").val("");
                }
                $.get("../_Query/GetDepUser.ashx?Emp=" + $(this).val(), function (data) {
                    var user = JSON.parse(data);
                    if (user != null) {
                        $("#txtCode").val(user.ID);
                        $("#txtName").val(user.Name);
                    } else {
                        $("#txtCode").val("");
                        $("#txtName").val("");
                    }
                });
            });
            $(document).on("click", "#btnAdd", function () {
                if ($("#txtCode").val() != "" && $("#txtName").val() != "") {
                    if ($("#lstSelect option[value='" + $("#txtCode").val() + "']").length == 0) {
                        $("#lstSelect").append("<option value='" + $("#txtCode").val() + "'>" + $("#txtName").val() + "</option>");
                    }
                }
            });
        });

        function checkData() {
            $("#<%=hidSelect.ClientID%>").val($("#lstSelect option").map(function () { return this.value; }).get().join(","));
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="EditPanel" style="width: 100%;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="buttonArea">
                    <asp:Button ID="btnConfirm" runat="server" Text="儲存" OnClientClick="return checkData()" />
                    <input id="btnCancel" type="button" value="取消" onclick="closethis();" />
                </div>
                <div id="tabs">
                    <ul class='etabs'>
                        <li class='tab'><a href="#tabs-1">角色人員設定</a></li>
                    </ul>
                    <div id="tabs-1" style="padding-top: 3px;">
                        <table style="width: 100%">
                            <tr>
                                <td style="width: 40%;">部門</td>
                                <td style="width: 25%;">部門人員</td>
                                <td style="width: 10%;"></td>
                                <td style="width: 25%;">已選擇人員</td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;">
                                    <div id="tree" style="max-height: 450px; overflow: auto; padding-left: 10px;">
                                    </div>
                                </td>
                                <td style="vertical-align: top;">
                                    <select id="lstDepEmp" multiple="multiple" style="height:450px;width:100%;">
                                        <%--<asp:Literal ID="ltlDepEmp" runat="server"></asp:Literal>--%>
                                    </select>
                                </td>
                                <td style="text-align: center;">
                                    <input type="button" value="加入>>" onclick="AddUser();" /><br />
                                    <br />
                                    <input type="button" value="<<移除" onclick="DelUser();" />
                                </td>
                                <td style="vertical-align: top;">
                                    <asp:UpdatePanel ID="UpdatePanel2" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <select id="lstSelect" multiple="multiple" style="height:450px;width:100%;">
                                                <asp:Literal ID="ltlSelect" runat="server"></asp:Literal>
                                            </select>
                                            <asp:HiddenField ID="hidSelect" runat="server" />
                                        </ContentTemplate>
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="btnConfirm" EventName="Click" />
                                        </Triggers>
                                    </asp:UpdatePanel>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
