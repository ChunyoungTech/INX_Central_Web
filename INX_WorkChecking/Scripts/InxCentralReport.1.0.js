$(function () {
    $(document).on("change", ".txt-input-number", function () {
        if ($(this).val().trim() != "" && isNaN($(this).val())) {
            $(this).addClass("value-invalid");
        } else {
            $(this).removeClass("value-invalid");

            if ($(this).val() != $(this).attr("data-old")) {
                $(this).addClass("value-changed");
            } else {
                $(this).removeClass("value-changed");
            }
        }
    });
    $(document).on("change", ".txt-input-string", function () {
        if ($(this).val() != $(this).attr("data-old")) {
            $(this).addClass("value-changed");
        } else {
            $(this).removeClass("value-changed");
        }
    });
});

function UpdateProcess(x, y, z, q) {
    $(document).on("click", x, function (e) {
        //$(this).hide();
        e.preventDefault();
        if ($("input.value-invalid").length > 0) {
            alert("輸入值有誤");
            return;
        } else if ($("input.value-changed").length == 0) {
            alert("無異動資料");
            return;
        } else {
            var oList = new Array();
            $("input.value-changed").each(function () {
                var pp = $(this).parentsUntil("tbody", "tr");
                oList.push({ V: $(this).val(), M: $(this).attr("data-idx"), C: pp.attr("data-key"), Y: pp.attr("data-y") });
            });

            $.ajax({
                url: "InxCentralReportUpdate.ashx",
                type: "post",
                data: JSON.stringify({ XA: $(y).val(), XL: oList, XT: q }),
                success: function (data) {
                    console.log(data);
                    var result = JSON.parse(data);
                    if (result.Success) {
                        alert("更新成功" + result.Message);
                        $(z).trigger("click");
                    } else {
                        alert(result.Message);
                    }
                },
                complete: function () {
                    //$(this).show();
                }
            });
        }
    });
}