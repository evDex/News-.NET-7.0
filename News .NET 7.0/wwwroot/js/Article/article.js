$(".card").on('click', function (event) {
    window.location.assign('/Article/' + $(this).attr('data-article'));
});