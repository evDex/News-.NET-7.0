document.getElementById('id-th').addEventListener('click', function (event) {
    SortedRows('Id');
});
document.getElementById('user-name-th').addEventListener('click', function (event) {
    SortedRows('UserName');
});
document.getElementById('email-th').addEventListener('click', function (event) {
    SortedRows('Email');
});
document.getElementById('articles-th').addEventListener('click', function (event) {
    SortedRows('Articles');
});
function SortedRows(column) {
    var initialUrl = '/Admin/Users/';
    var dataTable = $('#admin-table').data('sort');
    var nameAscending = column + '_ascending';
    var nameDescending = column + '_descending';

    if (dataTable === nameAscending || dataTable === nameDescending) {
        if (dataTable === nameAscending) {
            window.location.assign(initialUrl + column + '/descending');
        }
        else if (dataTable === nameDescending) {
            window.location.assign(initialUrl + column + '/ascending');
        }
    }
    else {
        $('#admin-table').data('sort', nameAscending);
        window.location.assign(initialUrl + column + '/ascending');
    }
}
function getSelectValues(select) {
    var result = [];
    var options = select && select.options;
    var opt;

    for (var i = 0, iLen = options.length; i < iLen; i++) {
        opt = options[i];

        if (opt.selected) {
            result.push(opt.value || opt.text);
        }
    }
    return result;
}
$("#search-main-form-admin-articles").on("submit", function (e) {
    e.preventDefault();
    var inputText = document.getElementById("search-admin-input").value;
    window.location.assign('/admin/users/search/' + inputText + '/');
});
