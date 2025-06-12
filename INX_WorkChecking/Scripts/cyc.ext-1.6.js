//(window.jQuery || document.write('<script src="../Scripts/jquery-3.7.1.min.js"><\/script>'));
//($.fancybox || document.write('<script src="../Scripts/jquery.fancybox.js"><\/script>'));
//($.tinyDraggable || document.write('<script src="../Scripts/jquery.tiny-draggable.min.js"><\/script>'));

var gWidth = $(window).width(), gHeight = $(window).height(), gOpt, gIdx;

function InitExt(o) {
    gOpt = o;
    
    var ohelpers = { overlay: { closeClick: false, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } };
    $.extend(jQuery.fancybox.defaults, { parent: $('form:first'), type: "iframe", scrollOutside: false, closeBtn: false, autoSize: true, padding: 5 });
    $(document).on("click", ".extBtn", function (e) { OpenExtWindow($(this)); e.preventDefault(); });

    function OpenExtWindow(x) {
        if (!(x.attr("data-val") == undefined || x.attr("data-idx") == undefined || gOpt == undefined)) {
            gIdx = parseInt(x.attr("data-idx"));
            if (gIdx != isNaN) {
                if (gOpt[gIdx] != null) {
                    $.fancybox.open(
                        {
                            title: "<div class='OpenWindowTitle'><span>" + (x.attr("data-t") == undefined ? x.val() : x.attr("data-t")) + "</span><a href='#'><img src='../_img/window_off.png' style='height:1.5em;float:right;' /><a/></div>",
                            href: "../" + (gOpt[gIdx].Dir == undefined ? "_edit" : gOpt[gIdx].Dir) + "/open.aspx?pa=" + x.attr("data-val") + "&app=" + (x.attr("data-app") == undefined ? $.url().param("app") : x.attr("data-app")) + "&sub=" + gOpt[gIdx].Sub,
                            width: (x.attr("data-width") == undefined ? gOpt[gIdx].Width * gWidth : x.attr("data-width")),
                            minHeight: (x.attr("data-height") == undefined ? $(window).height() * .5 : $(window).height() * x.attr("data-height")),
                            minWidth: "450",
                            beforeShow: function () {
                                //$(".MainGridView .gr_select").removeClass("gr_select");
                                //x.parentsUntil(".MainGridView tbody").addClass("gr_select");
                                this.wrap.tinyDraggable();
                            }
                        }, { helpers: ohelpers }
                    );
                }
            }
        }
    }
}

$(function () {
    $(document).on('click', '.OpenWindowTitle a', function () { $.fancybox.close(); });
    $(document).on('click', '.chkAll[data-s]', function () {
        $($(this).attr("data-s") + " :checkbox:enabled").prop("checked", $(this).prop('checked'));
        $($(this).attr("data-s") + ":checkbox:enabled").prop("checked", $(this).prop('checked'));
    });
    $(window).resize(function () {
        gWidth = $(window).width();
    });
    DatePicker();
});

function OpenWindow(url, title, width, height) {
    $.fancybox.open(
        {
            title: "<div class='OpenWindowTitle'><span>" + title + "</span><a href='#'><img src='../_img/window_off.png' style='height:1.5em;float:right;' /><a/></div>",
            type: "iframe",
            href: url,
            width: $(window).width() * width,
            minHeight: $(window).height() * height,
            minWidth: "450",
            closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
            beforeShow: function () {
                this.wrap.tinyDraggable();
            }
        }, { helpers: { overlay: { closeClick: true, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
    );
}

$.datepicker.setDefaults({ changeYear: true, changeMonth: true });

function DatePicker() {
    $(".cycDate").datepicker($.datepicker.regional['zh-TW']);
}

function checkboxSelectValue(x) {
    return $(x).map(function () {
        if (this.checked)
            return this.value;
    }).get().join();
}

function CloseAndReload(c, r) {
    if (c) {
        $.fancybox.close();
    }
    if (r) {
        $(gOpt[gIdx].reCtl).trigger('click');
        //var exp = /javascript:__doPostBack\(\'([\w\d\$]+)\'\,\'(.*)\'\)/;
        //if (exp.test($(gOpt[gIdx].reCtl).attr('href')) == true) {
        //    var ar = exp.exec($(gOpt[gIdx].reCtl).attr('href'));
        //    var ctl = ar[1];
        //    var param = ar[2];
        //    __doPostBack(ctl, param);
        //}
    }
}

function ClientAlert(msg) {
    msg = msg.replace(/;/g, "\n");
    alert(msg);
}

function ReLogin(callback) {
    $.fancybox.open(
        {
            title: "<div class='OpenWindowTitle'><span>再確認</span></div>",
            type: "iframe",
            href: '../_edit/ReConfirm.aspx?app=' + $.url().param("app"),
            width: 500,
            closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
            beforeShow: function () {
                this.wrap.tinyDraggable();
            },
            beforeClose: function () {
                if (callback) {
                    var x = $('.fancybox-iframe').contents().find("#hidConfirm");
                    if (x && x.val().length > 0) {
                        callback(x.val());
                    }
                }
            }
        }, { helpers: { overlay: { closeClick: false, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
    );
}

function ChangePWD() {
    $.fancybox.open(
        {
            title: "<div class='OpenWindowTitle'><span>修改密碼</span></div>",
            type: "iframe",
            href: '../_edit/ChangePWD.aspx',
            width: 500,
            closeBtn: false, autoSize: true, padding: 5, scrollOutside: false,
            beforeShow: function () {
                this.wrap.tinyDraggable();
            }
        }, { helpers: { overlay: { closeClick: false, locked: false, css: { 'background-color': 'Gray', 'opacity': 0.3 } }, title: { type: 'inside', position: 'top' } } }
    );
}
