<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestWorkCheck.aspx.cs" Inherits="WebApp._test.TestWorkCheck" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <style>
        div.main {
            padding:.5rem;
            margin:.5rem;
            border: .1rem solid #000;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>

                <div class="main">
                    <h3>一、自動產生一筆工單</h3>
                    日期：<uc:ucDate ID="dteCreate" runat="server" />
                    廠別：<asp:DropDownList ID="ddlFactory" runat="server">
                        <asp:ListItem Text="FAC6" Value="6"></asp:ListItem>
                        <asp:ListItem Text="FAC8" Value="8"></asp:ListItem>
                       </asp:DropDownList>
                    <asp:Button ID="btnCreate" runat="server" Text="RUN" OnClick="btnCreate_Click" />
                    =>產出工單號：<asp:TextBox ID="txtCreate" runat="server"></asp:TextBox>
                    <div>
                        說明：
                        <ul>
                            <li>根據輸入日期與廠別。產生一筆工單資料</li>
                            <li>工單資料，為2024/3/11同一廠別的工單複製，僅修改日期及工單號</li>
                        </ul>
                    </div>
                </div>
                <div class="main">
                    <h3>二、產生一筆工單報到資料 (AccessList2)</h3>
                    工單：<asp:TextBox ID="txtConNumberIn" runat="server"></asp:TextBox>
                    <asp:Button ID="btnCheckin" runat="server" Text="RUN" OnClick="btnCheckin_Click" />
                    ，Result: <asp:TextBox ID="txtCheckin" runat="server"></asp:TextBox>
                    <div>
                        說明：
                        <ul>
                            <li>將步驟一，產生的工單號複製至[工單]欄位</li>
                            <li>報到人員為下列資料隨機產生</li>
                            <li>
                                <ol>
                                    <li>A100100100,測試一</li>
                                    <li>B200200200,測試二</li>
                                    <li>C123123123,測試三</li>
                                    <li>D222333444,測試四</li>
                                    <li>E123456789,測試五</li>
                                </ol>
                            </li>
                            <li>每一筆資料最多只會報到一次</li>
                        </ul>
                    </div>
                </div>
                <div class="main">
                    <h3>三、產生一筆工單報退資料 (AccessList2)</h3>
                    工單：<asp:TextBox ID="txtConNumberOut" runat="server"></asp:TextBox>
                    <asp:Button ID="btnCheckout" runat="server" Text="RUN" OnClick="btnCheckout_Click" />
                    ，Result: <asp:TextBox ID="txtCheckout" runat="server"></asp:TextBox>
                    <div>
                        說明：
                        <ul>
                            <li>將步驟一，產生的工單號複製至[工單]欄位</li>
                            <li>報退人員為相同工單號報到人員隨機產生</li>
                            <li>每一筆資料最多只會報退一次</li>
                            <li>報到與報退時間，要相隔10分鐘以上，自動報到退功能才會納入</li>
                        </ul>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
