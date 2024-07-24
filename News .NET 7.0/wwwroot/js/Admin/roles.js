document.getElementById('id-th').addEventListener('click', function (event) {
    SortedRows('Id');
});
document.getElementById('role-name-th').addEventListener('click', function (event) {
    SortedRows('Name');
});
function SortedRows(column) {
    var dataTable = $('#admin-table').data('sort');
    var nameAscending = column + '_ascending';
    var nameDescending = column + '_descending';

    if (dataTable === nameAscending || dataTable === nameDescending) {
        if (dataTable === nameAscending) {
            window.location.assign('/Admin/Roles/' + column + '/descending');
        }
        else if (dataTable === nameDescending) {
            window.location.assign('/Admin/Roles/' + column + '/ascending');
        }
    }
    else {
        $('#admin-table').data('sort', nameAscending);
        window.location.assign('/Admin/Roles/' + column + '/ascending');
    }
}
$("#search-main-form-admin-articles").on("submit", function (e) {
    e.preventDefault();
    var inputText = document.getElementById("search-admin-input").value;
    window.location.assign('/admin/roles/search/' + inputText + '/');
});