<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.Master" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="WebApp._app.home" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        div.back {
            background-image: url('../_img/login3.jpg');
            background-repeat: no-repeat; background-position:center;
            height: 90vh;
            /*width: 100%;*/
/*            display: flex;
            align-items: center;
            justify-content: center;*/
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="back">
        <%--<img src="../_img/login.jpg" style="width:100%" />--%>
    </div>
    
</asp:Content>
