$(function () {
    var app = $.url().param("app");
    if (app != undefined) {
        app = $(".masterMenu ul li[data-v='" + app + "']");
        if (app.length > 0) {
            app.addClass("hav").parent().show().siblings("ul").hide();
        }
    } else {
        $(".masterMenu ul").hide();
    }

    $(document).on("click", ".masterMenu h1", function () { $(this).next().slideToggle(); });
    $(".masterMenuShow").click(function () { $(".masterCont").animate({ left: "11.2rem" }, 100); switchMenu(); });
    $(".masterMenuHide").click(function () { $(".masterCont").animate({ left: ".2rem" }, 100); switchMenu(); });
});
function switchMenu() {
    $(".masterMenuShow,.masterMenuHide").toggle();
    $(".masterMenu").fadeToggle(100);//.slideToggle(100);
}
String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }
    return s;
}
function OpenLogin() {
    $("#OpenLogin").show().children("#ifmLogin").attr("src", "../loginInPage.aspx");
}
function CloseLogin() {
    $("#OpenLogin").hide().children("#ifmLogin").attr("src", "");
}