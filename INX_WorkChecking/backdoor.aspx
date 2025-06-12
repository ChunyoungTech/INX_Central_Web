<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="backdoor.aspx.cs" Inherits="WebApp.backdoor" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>廠務智慧雲端系統</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <script type="text/javascript" src="Scripts/jquery-3.6.0.min.js"></script>
    <script type="text/javascript" src="Scripts/bootstrap.min.js"></script>
    <style>
        .main {
            padding: 1em;
            display: flex;
            flex-direction: column;
            flex-wrap: wrap;
            border-radius: 1em;
            box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
        }

        @media (min-width: 768px) {
            .main {
                width: 25em;
            }
        }
    </style>
</head>
<body>

    <div style="height: 95vh; display: flex; align-items: center; justify-content: center;">

        <div class="main">

            <form id="form1" runat="server">
                <div class="form-group">
                    標題
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    首頁字串
                        <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group float-right">
                </div>
                <div class="panel-warning text-danger text-center;">
                    <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
                </div>
                <table style="width: 100%;">

                    <tr>
                        <th class="label write">LOGO上傳</th>
                        <td colspan="3">
                            <asp:FileUpload ID="FileUpload2" runat="server" Width="90%" />
                            <br />
                            目前檔案：<asp:Label ID="Label2" runat="server" Text="" ForeColor="Blue"></asp:Label>
                            <asp:HiddenField ID="HiddenField1" runat="server" />
                        </td>
                    </tr>

                </table>
            </form>

        </div>

    </div>
</body>
</html>
