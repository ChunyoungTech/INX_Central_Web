<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MappEVTest.aspx.cs" Inherits="WebApp.MappEVTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title></title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <script src="Scripts/jquery-3.7.1.min.js"></script>
    <script src="Scripts/jquery.signalR-2.4.3.min.js"></script>
    <script src="<%= ResolveUrl("~/signalr/hubs") %>"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script type="text/javascript">
        $(function () {
            var chat = $.connection.sendMappHub;

            chat.client.addMessage = function (msg) {
                $('#messageDiv').children(":gt(48)").fadeOut(500, function () { $(this).remove(); });
                var inO = $("<li class='list-group-item list-group-item-info'>" + msg + "</li>");
                inO.prependTo($('#messageDiv')).slideDown(500);
            };

            $.connection.hub.start().done(function () {
                $('#Button1').click(function () {
                    chat.server.sendMessage($('#TextBox1').val());
                    $('#TextBox1').val("");
                });
            });

            $("#btnClear").click(function () { $('#messageDiv').empty(); });


            $("#btnSend").click(function () {
                if ($("#selFac").val() != "" && $("#selType").val() != "") {
                    var xData = { FacName: $("#selFac").val(), Type: $("#selType").val(), Value1: $("#txtInput1").val(), Value2: $("#txtInput2").val(), Value3: $("#txtInput3").val(), Value4: $("#txtInput4").val(), Value5: $("#txtInput5").val(), InputSource: $("#txtInputS").val() };
                    $.post("_api/TestMappEV.ashx", JSON.stringify(xData));
                }
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <nav class="navbar bg-dark text-light">

                <a class="navbar-brand" href="#"><img src="_img/logo.jpg" alt="Logo" style="height: 50px;" /></a>

                <h1 class="">地震壓降MAPP測試</h1>

                <div class="form-inline">
                    <input type="text" id="TextBox1" class="form-control" placeholder="Message" style="display:none" />
                    <input type="button" id="Button1" value="Send" class="btn btn-warning" style="display:none" />
                    <input type="button" id="btnClear" value="CLEAR" class="btn btn-primary" />
                </div>
            </nav>

            <div class="form">
                <div class="form-row">
                    <div class="col-6">
                        <label for="selFac" class="col-form-label">廠區：</label>
                        <select id="selFac" class="form-control">
                            <option value="FAC01">FAC1</option>
                            <option value="FAC02">FAC2</option>
                            <option value="FAC03">FAC3</option>
                            <option value="FAC04">FAC4</option>
                            <option value="FAC05">FAC5</option>
                            <option value="FAC06">FAC6</option>
                            <option value="FAC07">FAC7</option>
                            <option value="FAC08">FAC8</option>
                            <option value="FACL">FACL</option>
                            <option value="FACT1">FACT1</option>
                            <option value="FACT2">FACT2</option>
                            <option value="FACT3">FACT3</option>
                        </select>
                    </div>
                    <div class="col-6">
                        <label for="selType" class="col-form-label">分類：</label>
                        <select id="selType" class="form-control">
                            <option value="E">地震</option>
                            <option value="P">壓降</option>
                        </select>
                    </div>
                </div>
                <div class="form-row">
                    <div class="col-4">
                        <label for="txtInput1" class="col-form-label">值1：</label>
                        <input type="text" id="txtInput1" class="form-control" maxlength="10" />
                    </div>
                    <div class="col-4">
                        <label for="txtInput2" class="col-form-label">值2：</label>
                        <input type="text" id="txtInput2" class="form-control" maxlength="10" />
                    </div>
                    <div class="col-4">
                        <label for="txtInput3" class="col-form-label">值3：</label>
                        <input type="text" id="txtInput3" class="form-control" maxlength="10" />
                    </div>
                </div>
                <div class="form-row">
                    <div class="col-4">
                        <label for="txtInput4" class="col-form-label">值4：</label>
                        <input type="text" id="txtInput4" class="form-control" maxlength="10" />
                    </div>
                    <div class="col-4">
                        <label for="txtInput5" class="col-form-label">值5：</label>
                        <input type="text" id="txtInput5" class="form-control" maxlength="10" />
                    </div>
                    <div class="col-4">
                        <label for="txtInputS" class="col-form-label">來源：</label>
                        <input type="text" id="txtInputS" class="form-control" maxlength="10" />
                    </div>
                </div>
                <div class="form-row p-2">
                    <div class="col-4"></div>
                    <div class="col-4">
                        <input type="button" class="btn btn-info form-control" id="btnSend" value="Send" />
                    </div>
                    <div class="col-4"></div>
                </div>
            </div>

            <ul id="messageDiv" class="list-group">
            </ul>
        </div>
    </form>
</body>
</html>
