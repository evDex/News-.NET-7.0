var currentMenuVisible = null;

$(document).ready(function () {
    var comments = document.getElementsByClassName('ViewComments__Title-Menu');

    document.addEventListener('click', function (event) {
        ClosetheOpenedMenu();
    });

    for (var i = 0; i < comments.length; i++) {
        comments[i].addEventListener('click', function (event) {
            event.preventDefault();
            event.stopPropagation();
            CreateContextMenuText(event, ['reply']);
            clickDocument = true;
        });
    }
});

function ClosetheOpenedMenu() {
    if (currentMenuVisible !== null) {
        CloseContextMenu(currentMenuVisible);
    }
}

function CloseContextMenu(menu) {
    menu.style.left = '0px';
    menu.style.top = '0px';
    document.body.removeChild(menu);
    currentMenuVisible = null;
}

function CreateContextMenuText(event, menuOptionArray) {
    ClosetheOpenedMenu();

    const menuElement = document.createElement('div');
    menuElement.classList.add('ContextMenu__Reply');

    const menuListElement = document.createElement('ul');
    menuListElement.classList = 'ContextMenu';
    for (var element of menuOptionArray) {
        var listElement = document.createElement('li');
        var a = document.createElement('a');
        /*a.href = '#';*/

        var i = document.createElement('i');
        i.ariaHidden = 'true';
        var text;
        switch (element) {
            case 'reply':
                listElement.className = "Reply";
                i.classList = 'fa-solid fa-pen';
                text = document.createTextNode('Ответить');
                listElement.addEventListener('click', function () {
                    /*$(event.target).data("comment");*/
                    var div = event.target.closest(".ViewComments__Div");
                    CreateReplyArea(div, div.dataset.comment);
                    /*alert(event.target.closest(".ViewComments__Div").dataset.comment);*/
                    /*alert(event.target.closest("div").dataset.comment);*/
                });
                break;
        }
        a.appendChild(i);
        a.appendChild(text);
        listElement.appendChild(a);
        menuListElement.appendChild(listElement);
    }
    menuElement.appendChild(menuListElement);
    document.body.appendChild(menuElement);
    menuElement.style.display = 'block';
    menuElement.style.left = event.pageX - 140 + "px";
    menuElement.style.top = event.pageY + "px";
    currentMenuVisible = menuElement;
}



