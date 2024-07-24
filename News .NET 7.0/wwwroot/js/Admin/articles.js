document.getElementById('id-th').addEventListener('click', function (event) {
    SortedRows('Id');
});
document.getElementById('article-name-th').addEventListener('click', function (event) {
    SortedRows('Name');
});
document.getElementById('user-name-th').addEventListener('click', function (event) {
    SortedRows('UserName');
});
document.getElementById('number-of-views-th').addEventListener('click', function (event) {
    SortedRows('NumberOfViews');
});
document.getElementById('comments-th').addEventListener('click', function (event) {
    SortedRows('Comments');
});
document.getElementById('date-th').addEventListener('click', function (event) {
    SortedRows('Date');
});
document.getElementById('state-th').addEventListener('click', function (event) {
    SortedRows('State');
});
function SortedRows(column) {
    var initialUrl = '/Admin/Articles/';
    var dataTable = $('#admin-table').data('sort');
    var dataPage= $('#admin-table').data('page');
    var nameAscending = column + '_ascending';
    var nameDescending = column + '_descending';

    if (dataTable === nameAscending || dataTable === nameDescending) {
        if (dataTable === nameAscending) {
            window.location.href = initialUrl + column + '/descending/page/' + dataPage;
           /* window.location.assign(initialUrl + column + '/descending/page/' + dataPage);*/
        }
        else if (dataTable === nameDescending) {
            window.location.href = initialUrl + column + '/ascending/page/' + dataPage;
            /*window.location.assign(initialUrl + column + '/ascending/page/' + dataPage);*/
        }
    }
    else {
        $('#admin-table').data('sort', nameAscending);
        /*window.location.assign(initialUrl + column + '/ascending/page/' + dataPage);*/
        window.location.href = initialUrl + column + '/ascending/page/' + dataPage;
    }
}
$("#search-main-form-admin-articles").on("submit", function (e) {
    e.preventDefault();
    var inputText = document.getElementById("search-admin-input").value;
    window.location.assign('/admin/articles/search/' + inputText + '/');
});