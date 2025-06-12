$(function () {
    $(document).on("click", "select.cyc-selectfilter", function (e) {
/*        e.preventDefault();*/
        $(".cyc-popup-back").remove();
        $(".cyc-select-filter").remove();

        var select = $(this);
        var filter_count = select.attr("data-filter-count");
        if (!filter_count || (filter_count && !isNaN(filter_count) && select.children("option").length > filter_count)) {
            var position = select.position();
            $(".select-filter-active").removeClass("select-filter-active");
            select.addClass("select-filter-active");

            select.parents("body").append('<div class="cyc-popup-back"></div>');
            select.parents("body").append("<div class='cyc-select-filter'></div>");
            /*select.parents("body").append("<div class='cyc-select-filter' style='top:" + position.top + "px;left:" + position.left + "px;width:" + select.width() + "px;'></div>");*/
            $("div.cyc-select-filter").append(String.format("<div>{0}</div><input type='text' /><ul>{1}</ul>", select.attr("title"), select.children("option").map(function () { return String.format("<li data-val='{1}'>{0}</li>", $(this).text(), $(this).attr("value")); }).get().join("")));
            $("div.cyc-select-filter input").focus();
        }
    });
    $(document).on("click", ".cyc-popup-back", function () {
        $(".cyc-select-filter").hide();
        $(this).hide();
    });

    $(document).on("click", "div.cyc-select-filter li", function () {
        var value = $(this).attr("data-val");
        $(".select-filter-active").val(value).trigger("change");
        $(".cyc-popup-back").trigger("click");
    });
    $(document).on("keyup", "div.cyc-select-filter input", function () {
        var filter = $(this).val().toUpperCase();
        $("div.cyc-select-filter ul li").each(function () {
            if ($(this).text().toUpperCase().indexOf(filter) > -1) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });
});