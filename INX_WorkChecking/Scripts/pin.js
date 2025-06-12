(window.jQuery || document.write('<script src="../_js/jquery-3.3.1.min.js"><\/script>'));
($.fancybox || document.write('<script src="../Scripts/jquery.fancybox.js"><\/script>'));

var gWidth = $(window).width();
var gopt;
function initBase(o) {
    gopt = o;
    $.extend(jQuery.fancybox.defaults, { parent: $('form:first'), type: "iframe", scrollOutside: false, closeBtn: false, autoSize: true, padding: 5 });
    var ohelpers = { overlay: { closeClick: false, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } };
    if (o != undefined) {
        function reloadGrid() {
            if ($(o.hr).val().length > 0) {
                var lb = $(o.re);
                if (lb.size() > 0) {
                    var exp = /javascript:__doPostBack\(\'([\w\d\$]+)\'\,\'(.*)\'\)/;
                    if (exp.test(lb.attr('href')) == true) {
                        var ar = exp.exec(lb.attr('href'));
                        var ctl = ar[1];
                        var param = ar[2];
                        __doPostBack(ctl, param);
                    }
                } else if (o.ref != undefined) {
                    o.ref();
                }
                $(o.hr).val("");
            }
        }

        function OpenWindow(x) {
            if (!(x.attr("data-w") == undefined || x.attr("data-x") == undefined)) {
                $.fancybox.open(
                    {
                        title: "<div class='OpenWindowTitle'><p>" + (x.attr("data-t") == undefined ? x.val() : x.attr("data-t")) + "<a href='#'><img src='../_img/window_off.png' style='height:1.5em;float:right;' /><a/></p></div>",
                        href: "../Edit/" + x.attr("data-w") + ".aspx?pa=" + x.attr("data-x"),
                        width: (x.attr("data-width") == undefined ? o.ed.w * gWidth : x.attr("data-width")),
                        minHeight: $(window).height() * .5,
                        beforeClose: reloadGrid,
                        beforeShow: function () {
                            $(".MainGridView .gr_select").removeClass("gr_select");
                            x.parentsUntil(".MainGridView tbody").addClass("gr_select");
                            //var gr = x.parentsUntil(".MainGridView tbody");
                            //if (gr.length > 0) { gr.addClass("gr_select"); }
                            this.wrap.tinyDraggable();
                        }
                    }, { helpers: ohelpers }
                )
            }
        }

        function WaitOperate(x) {
            if (x.attr("data-w") == undefined || x.attr("data-x") == undefined) { return false; }
            if (x.attr("data-confirm") != undefined) { if (!confirm(x.attr("data-confirm"))) { return false; } }
            var pa = x.attr("data-w").split("/");
            if (pa.length != 2) { return false; }
            $.ajax({
                type: "POST",
                url: "../_Query/" + pa[0] + ".aspx/" + pa[1],
                contentType: "application/json",
                data: JSON.stringify({ "data": x.attr("data-x") }),
                success: function (rtn) {
                    msg = JSON.parse(rtn.d);
                    if (!msg.Success) {
                        alert("訊息：" + msg.Message);
                    } else {
                        alert(msg.Message);
                        $(o.hr).val("1");
                        reloadGrid();
                    }
                },
                error: function () { alert("訊息：非同步作業失敗"); }
            });
        }

        $(document).on('click', ':button.exBtn,:submit.exBtn', function (e) { OpenWindow($(this)); e.preventDefault(); });
        $(document).on('click', ':button.opBtn,:submit.opBtn', function () { WaitOperate($(this)); });
        $(document).on('click', '.OpenWindowTitle a', function () { $.fancybox.close(); });
    }
}

$(function () {
    $(document).on("click", ":checkbox.checkAll", function () {
        if ($(this).attr("data-s") != undefined) {
            var chk = $(this).prop("checked");
            $($(this).attr("data-s")).each(function () {
                if (!$(this).prop("disabled")) { $(this).prop("checked", chk); }
            });
        }
    });
    $(window).resize(function () {
        gWidth = $(window).width();
    });
});

function checkboxSelect(x) {
    return $(x).map(function () {
        if (this.checked)
            return this.value;
    }).get().join();
}

function showMessage(s)
{
    var sa = s.replace(/;/g, "\n");
    alert(sa);
}

function CloseAndReload(c, r) {
    if (c) {
        $.fancybox.close();
    }
    if (r) {
        var exp = /javascript:__doPostBack\(\'([\w\d\$]+)\'\,\'(.*)\'\)/;
        if (exp.test($(gopt.re).attr('href')) == true) {
            var ar = exp.exec($(gopt.re).attr('href'));
            var ctl = ar[1];
            var param = ar[2];
            __doPostBack(ctl, param);
        }
    }
}

function ClientAlert(msg) {
    msg = msg.replace(/;/g, "\n");
    alert(msg);
}

function ReLogin(callback) {
    $.fancybox.open(
        {
            //title: "<div class='OpenWindowTitle'><span>確認</span></div>",
            type: "iframe",
            href: '../_edit/ReConfirm.aspx',
            width: 400,
            closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
            beforeClose: function () {
                var x = $('.fancybox-iframe').contents().find("#hidConfirm");
                if (x && x.val().length > 0) {
                    callback(x.val());
                }
            }
        }, { helpers: { overlay: { closeClick: false, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
    );
}
