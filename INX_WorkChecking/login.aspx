<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="WebApp.login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>廠務智慧雲端管理平台</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <style type="text/css">

        .main {
            width: 30em;
            left: 20vw;
            bottom: 20vh;
        }

        @media (max-width: 512px) {
            body {
                font-size: 14px;
            }

            .main {
                width: 80vw;
                left: 5vw;
                bottom: 10vh;
            }
        }

        @media (min-width: 513px) and (max-width: 768px) {
            body {
                font-size: 15px;
            }

            .main {
                width: 60vw;
                left: 15vw;
                bottom: 15vh;
            }
        }

        @media (min-width: 769px) and (max-width:1200px) {
            body {
                font-size: 16px;
            }
        }

        .back1 {
            background-image: url('_img/login001-2.png');
            background-repeat: no-repeat;
            height: 15vh;
            z-index: 101;
            display: inline-block;
            width: 100vw;
            /*min-width: 394px;*/
            min-width: 400px;
            float: left;
        }

        .back2 {
            background-image: url('_img/login002-1.png');
            background-repeat: no-repeat;
            height: 85vh;
            z-index: 100;
            display: inline-block;
            position: fixed;
            bottom: 0;
            right: 0;
            width: 60vw;
            float: right;
        }

        .back3 {
            background-image: url('_img/login003.png');
            background-repeat: no-repeat;
            background-position-y: top;
            height: 90vh;
            z-index: 88;
            display: inline-block;
            width: 100vw;
            min-width: 430px;
            position: fixed;
            top: 120px;
            left: 0;
        }

/*        @media (max-width: 1280px) {

        }*/

        @media (min-width:1201px) and (max-width: 1920px) {
                .back1 {
                    /*width:45vw;*/
                }

                .back2 {
                    background-position-x: right;
                    background-position-y: top;
                    height: 90vh;
                    width: 70vw;
                    background-size: 70%;
                }

                .back3 {
                    /*width: 45vw;*/
                }

                .main {
                    left: 25vw;
                    bottom: 25vh;
                }
            }

            @media (min-width:1921px) {
                .back1 {
                    background-position-x: unset;
                    background-position-y: unset;
                    background-size: 30%;
                }

                .back2 {
                    background-position-x: unset;
                    background-position-y: unset;
                    background-size: 80%;
                    height: 95vh;
                    width: 75vw;
                }

                .back3 {
                    background-position-x: unset;
                    background-position-y: unset;
                    background-size: 60%;
                    /*width: 40vw;*/
                }
                .main {
                    left: 25vw;
                    bottom: 25vh;
                }
            }
    </style>
    <script type="text/javascript" src="Scripts/jquery-3.7.1.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
</head>
<body>
    <div style="height:100vh;width:100vw;">
        <div class="back1"></div>
        <div class="back2"></div>
        <div class="back3"></div>
        <div class="main card shadow rounded" style="z-index:109;position:fixed; min-width:350px;background-color: rgba(255,255,255,0.8);">
            <div class="card-header">
                <h2 class="form-signin-heading">群創光電</h2>
                <div class="d-flex justify-content-end">
                    <h3 class="form-signin-heading text-primary">廠務智慧雲端管理平台</h3>
                </div>
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
                                <span class="input-group-text bg-info text-light">Password：</span>
                                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                        </div>

                        <div class="d-flex mt-3 justify-content-end">
                            <asp:Button ID="btnConfirm" runat="server" Text="登入" CssClass="btn btn-primary" />
                        </div>
                        <div class="panel-warning text-danger text-center mt-2">
                            <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
                        </div>
                        <div class="panel-warning text-danger text-center mt-2">
                            <asp:Label ID="lblCheck" runat="server" Text=""></asp:Label>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>

</body>
</html>
