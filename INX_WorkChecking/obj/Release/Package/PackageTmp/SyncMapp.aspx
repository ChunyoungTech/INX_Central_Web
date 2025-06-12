<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SyncMapp.aspx.cs" Inherits="WebApp.SyncMapp" %>

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
            var chat = $.connection.syncMappHub;

            chat.client.addMessage = function (msg) {
                $('#messageDiv').children(":gt(48)").fadeOut(500, function () { $(this).remove(); });
                var inO = $("<li class='list-group-item list-group-item-info' style='display:none'>" + msg + "</li>");
                inO.prependTo($('#messageDiv')).slideDown(500);
            };

            $.connection.hub.start().done(function () {
                $('#Button1').click(function () {
                    chat.server.sendMessage($('#TextBox1').val());
                    $('#TextBox1').val("");
                });
            });

            $("#btnClear").click(function () { $('#messageDiv').empty(); });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <nav class="navbar bg-dark text-light">

                <a class="navbar-brand" href="#"><img src="_img/logo.jpg" alt="Logo" style="height: 50px;" /></a>

                <h1 class="">MAPP訊息同步作業</h1>

                <div class="form-inline">
                    <input type="text" id="TextBox1" class="form-control" placeholder="Message" style="display:none" />
                    <input type="button" id="Button1" value="Send" class="btn btn-warning" style="display:none" />
                    <input type="button" id="btnClear" value="CLEAR" class="btn btn-primary" />
                </div>

            </nav>
            <ul id="messageDiv" class="list-group">
            </ul>
        </div>
    </form>
</body>
</html>
