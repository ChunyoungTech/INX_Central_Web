<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login2.aspx.cs" Inherits="WebApp.login2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>廠務智慧雲端管理平台</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <style type="text/css">
        .main {
            z-index: 109;
            position: fixed;
            background-color: rgba(255,255,255,0.9);
            width: 30em;
            left: 25vw;
            bottom: 20vh;
        }
        div.back1 {
            display: inline-block;
            position: fixed;
            background-image: url('_img/login001-2.png');
            background-repeat: no-repeat;
            z-index: 102;
            height: 15vh;
            width: 100vw;
            min-width: 400px;
            float: left;
        }
        div.back2 {
            display: inline-block;
            position: fixed;
            background-image: url('_img/login002-1.png');
            background-repeat: no-repeat;
            z-index: 101;
            height: 100vh;
            width: 100vw;
            bottom: 0;
            right: 0;
            float: right;
            /*background-size: 85vh;*/
            background-size: 100vh;
            background-position-x: right;
            background-position-y: bottom;
        }
        div.back3 {
            display: inline-block;
            position: fixed;
            background-image: url('_img/login003.png');
            background-repeat: no-repeat;
            background-position-y: top;
            height: 90vh;
            z-index: 100;
            width: 100vw;
            min-width: 430px;
            top: 120px;
            left: 0;
        }

        @media (min-width:1400px) {
/*            div.back2 {
                background-size: 90vh;
                height: 95vh;
                width: 85vw;
            }*/
        }
        @media (max-width:850px){
            .main{
                width:60vw;
                left:20vw;
                bottom: 15vh;
            }
/*            div.back2 {
                background-size: 95vw;
                height: 95vh;
                width: 95vw;
            }*/
            div.back2 {
                background-position-x: left;
                background-size: 90vh;
            }
        }

        @media(max-width:430px) {
            .main {
                width: 90vw;
                left: 5vw;
                bottom: 10vh;
            }
            div.back1 {
                background-size: 100vw;
            }
        }
    </style>
    <script type="text/javascript" src="Scripts/jquery-3.7.1.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
</head>
<body>
    <div style="height:100vh;width:100vw;">
        <div class="back1"></div>
        <div class="back3"></div>
        <div class="back2"></div>
        <div class="main card shadow rounded">
            <div class="card-header">
                <h2 class="form-signin-heading">群創光電</h2>
                <h3 class="form-signin-heading text-primary float-right">廠務智慧雲端管理平台</h3>
            </div>
            <div class="card-body">
                <div>
                    <form id="form1" runat="server">
                        <div class="form-group mt-3">
                            <div class="input-group mb-3">
                                <span class="input-group-text bg-info text-light">&nbsp;&nbsp;&nbsp;User ID：</span>
                                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-group mt-3">
                            <div class="input-group mb-3">
                                <span class="input-group-text bg-info text-light rounded-pill">Password：</span>
                                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-group float-right mt-3">
                            <asp:Button ID="Button1" runat="server" Text="登入" CssClass="btn btn-primary" />
                        </div>
                        <div class="panel-warning text-danger text-center mt-2">
                            <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
