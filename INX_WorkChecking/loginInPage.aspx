<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loginInPage.aspx.cs" Inherits="WebApp.loginInPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>廠務智慧雲端管理平台</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <style type="text/css">
        .main {
            width: 22em;
        }
    </style>
    <script type="text/javascript" src="Scripts/jquery-3.7.1.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#btnClose").click(function () { CloseParent(); });
        });
        function CloseParent() {
            window.parent.CloseLogin();
        }
    </script>
</head>
<body>
    <div style="height:100vh;width:100vw;display:flex;justify-content:center;align-items:center;">
        <div class="main card shadow rounded" style="z-index:109;position:fixed;background-color: rgba(255,255,255,0.8);">
            <div class="card-header">
                <h2 class="form-signin-heading">群創光電</h2>
                <div class="d-flex justify-content-end">
                    <h3 class="form-signin-heading text-primary">廠務智慧雲端管理平台</h3>
                </div>
            </div>
            <div class="card-body">
                <div>
                    <form id="form1" runat="server">
                        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                        <div class="form-group mt-3">
                            <div class="input-group mb-3">
                                <span class="input-group-text bg-info text-light">&nbsp;&nbsp;&nbsp;User ID：</span>
                                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
                        <div class="form-group mt-3">
                            <div class="input-group mb-3">
                                <span class="input-group-text bg-info text-light">Password：</span>
                                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mt-3 d-flex justify-content-evenly">
                            <asp:Button ID="btnConfirm" runat="server" Text="登入" CssClass="btn btn-primary" />
                            <input type="button" id="btnClose" value="關閉" class="btn btn-secondary" />
                        </div>
                        <div class="form-group">
                            <div class="panel-warning text-danger text-center mt-3">
                                <asp:Label ID="lblMessage" runat="server" Text="尚未登入或逾時登出"></asp:Label>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
