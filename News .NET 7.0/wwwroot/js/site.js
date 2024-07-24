$(function () {
    $("#search-main-form").on("submit", function (e) {
        e.preventDefault();
        var inputText = document.getElementById("search-input").value;
        window.location.assign('/article/search/' + inputText + '/');
    });
})
$(function () {
    $.ajaxSetup({ cache: false });
    $(".loginModal").click(function (e) {

        e.preventDefault();
        $.get(this.href, function (data) {
            $('#dialogContent').html(data);
            $('#modDialog').modal('show');
        });
    });
})
$(function () {
    $.ajaxSetup({ cache: false });
    $(".registerModal").click(function (e) {

        e.preventDefault();
        $.get(this.href, function (data) {
            $('#dialogContent').html(data);
            $('#modDialog').modal('show');
        });
    });
})