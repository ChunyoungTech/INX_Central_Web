<%@ Page Title="" Language="C#" MasterPageFile="~/_master/Grid.master" AutoEventWireup="true" CodeBehind="PatrolTest.aspx.cs" Inherits="WebApp._test.PatrolTest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .care-main {
            display: flex;
            width: 99.5%;
            /*min-height: calc((100vh - 3rem) / 4);*/
            /*min-height: 10rem;*/
            margin: .1rem .1rem;
            flex-shrink:1;
            flex-grow:1;
        }
        .care-dept{
            display:flex;
            flex-direction:column;
            width:9rem;
            flex-grow:0;
            flex-shrink:0;
            background-color:#CCC;
            justify-content:center;
            align-items:center;
            font-size:1.5rem;
            font-weight:600;
            color:#fff;
        }
        .care-status {
            width:2rem;
            height:2rem;
            border:.3rem solid #FFF;
            border-radius:50%;
            background-color:green;
            margin:.5rem;
        }
        .care-status-alarm {
            background-color: red !important;
            /*border:.3rem solid yellow !important;*/
            cursor:pointer;
            animation-name: alarm-border;
            animation-duration: 2s;
            animation-iteration-count: infinite;
        }
        @keyframes alarm-border {
            0%   {border-color: yellow}
            50%  {border-color: white;}
            100% {border-color: yellow}
        }

        .care-time {
            display:flex;
            text-align:center;
            align-items:center;
            font-size:.8rem;
            background: rgba(76, 175, 80, 0.2);
            cursor:pointer;
            border-radius:20%;
        }
        .care-time:hover {
            background: rgba(76, 175, 80, 0.3);
        }

        .care-detail{
            display:flex;
            flex-grow:1;
            background-color:#DDD;
            justify-content:start;
            flex-wrap:wrap;
            /*padding:.5rem;*/
        }

        .care-place-main {
            display: flex;
            flex-wrap: wrap;
            width: 8.2rem;
            height: 6rem;
            /*margin: .1rem;*/
            align-self: center;
            align-items: end;
            /*justify-content: center;*/
        }

        .care-place {
            display:flex;
            width:5.1rem;
            height:5.1rem;
            align-self:center;
            justify-content:center;
            align-items:center;
            font-size:.7rem;
            border: .2rem solid #fff;
            cursor:pointer;
            background-color:#CCC;
            margin: 0 .3rem;
            padding:.3rem;
        }
        .care-place:hover{
            background-color:#DDD;
        }

        .care-path {
            display:flex;
            width:3rem;
            height:3rem;
            /*margin:.1rem;*/
            align-self:center;
            background-image: url('img/care-path-white.svg');
            background-repeat: no-repeat;
            background-size: 3rem;
            background-position: center center;
        }

        .care-place-name {
            padding-left:.5rem;
            margin-top: -0.5rem;
            font-size:.75rem;
            width:7rem;
        }

        .care-place img {
            height: 95%;
            cursor:pointer;
/*            border-radius:50%;
            border: .5rem solid green;*/
        }
    </style>
    <script type="text/javascript">
        var colors = ["#2a9d8f", "#e9c46a", "#f4a261", "#e76f51"];
        $(function () {
            $.getJSON("../_api/GetPatrolSetting.ashx", function (list) {
                var divX = $("#divCurrent");
                divX.empty();
                if (list) {
                    divX.append(list.map(function (item, idx) {
                        return String.format("<div class='care-main' data-id='{0}' data-code='{1}' data-name='{2}' data-img='{5}-0.png' data-start='0'><div class='care-dept' style='background-color:{3}'>{2}<div class='care-status'></div></div><div class='care-detail'>{4}</div></div>",
                            item.ID,
                            item.Code,
                            item.Name,
                            colors[idx % colors.length],
                            item.Places.map(function (p, i) {
                                return String.format("<div class='care-place' data-id='{0}' data-code='{1}' data-name='{2}'>{2}</div>",
                                    p.ID, p.Code, p.Name);
                            }).join(""),
                            idx + 1
                        );
                    }).join(""));
                }
            });

            $(document).on("click", ".care-place", function () {
                $.getJSON("../_api/SetPatrolRecord.ashx?pa=" + $(this).attr("data-id"), function () { });
            });
            $(document).on("click", ".care-status", function () {
                $.getJSON("../_api/SetPatrolAlarm.ashx?pa=" + $(this).parents(".care-main").attr("data-id"), function () { });
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="divCurrent" style="display:flex; flex-direction:column;height:calc(100vh - 3.5rem);">

    </div>
</asp:Content>
