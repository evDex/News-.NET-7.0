let fileRank = 0;
var currentMenuVisible = null;
$(document).ready(function () {
    document.getElementById('title-input-img').addEventListener('change', function (event) {
        SetAndChangeImgArticle(event.target, $('#blah'));
    });
    document.getElementById("title-img").addEventListener('contextmenu', function (event) {
        var input = document.getElementById('title-input-img');
        var img = document.getElementById('blah');
        event.preventDefault();
        CreateContextMenuImg(event, input, img, ['edit', 'clear']);
    });
    document.addEventListener('click', function () {
        ClosetheOpenedMenu();
    });
    
    AddTextArea();
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
    menuElement.classList.add('div-context__menu');

    const menuListElement = document.createElement('ul');
    menuListElement.classList = 'menu';
    for (var element of menuOptionArray) {
        var listElement = document.createElement('li');
        var a = document.createElement('a');
        a.href = '#';

        var i = document.createElement('i');
        i.ariaHidden = 'true';
        var text;
        switch (element) {
            case 'copy':
                listElement.className = "copy";
                i.classList = 'fa fa-copy';
                text = document.createTextNode('Скопировать текст');
                listElement.addEventListener('click', function () {
                    CopyText(event.target);
                });
                break;
            case 'paste':
                listElement.className = "paste";
                i.classList = 'fa fa-paste';
                text = document.createTextNode('Вставить текст');
                listElement.addEventListener('click', function () {
                    PasteText(event.target);
                });
                break;
            case 'trash':
                listElement.className = "trash";
                i.classList = 'fa fa-trash';
                text = document.createTextNode('Удалить текстовое поле');
                listElement.addEventListener('click', function () {
                    DeleteDiv(event.target.parentNode);
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
    menuElement.style.left = event.pageX + "px";
    menuElement.style.top = event.pageY + "px";
    currentMenuVisible = menuElement;
}
function CopyText(input) {
    input.select();
    input.setSelectionRange(0, 99999);

    navigator.clipboard.writeText(input.value);
}
function PasteText(input) {
    navigator.clipboard.readText().then((clipText) => (input.value += clipText));
}
function CreateContextMenuImg(event, inputFile, img, menuOptionArray) {
    ClosetheOpenedMenu();

    const menuElement = document.createElement('div');
    menuElement.classList.add('div-context__menu');

    const menuListElement = document.createElement('ul');
    menuListElement.classList = 'menu';

    for (var element of menuOptionArray) {
        var listElement = document.createElement('li');
        var a = document.createElement('a');
        a.href = '#';

        var i = document.createElement('i');
        i.ariaHidden = 'true';

        var text = "";
        switch (element) { 
            case 'edit':
                listElement.className = "edit";
                i.classList = 'fa fa-edit';
                text = document.createTextNode('Поменять изображение');
                listElement.addEventListener('click', function () {
                    ChangeTextImg(inputFile);
                });
                break;
            case 'clear':
                listElement.className = "clear";
                i.classList = 'fa fa-times';
                text = document.createTextNode('Очитить текущее изображение');
                listElement.addEventListener('click', function () {
                    ClearTextImg(inputFile, img);
                });
                break;
            case 'trash':
                listElement.className = "trash";
                i.classList = 'fa fa-trash';
                text = document.createTextNode('Удалить поле с изображением');
                listElement.addEventListener('click', function () {
                    DeleteDiv(event.target.parentNode);
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
    menuElement.style.left = event.pageX + "px";
    menuElement.style.top = event.pageY + "px";

    currentMenuVisible = menuElement;
}
function ChangeTextImg(inputFile) {
    inputFile.click();
}
function ClearTextImg(inputFile, img) {
    inputFile.value = "";
    img.src = '';
}
function DeleteDiv(div) {
    div.remove();
}
function AddTextArea() {
    var content = document.getElementById('text');
    var div = document.createElement('div');
    $(div).data("div-type", "text");

    var textArea = document.createElement('textarea');
    textArea.className = "textarea-article__text";
    textArea.cols = "50";
    textArea.rows = "1";
    textArea.placeholder = "Введите текст статьи.";
    textArea.oninput = function () {
        textArea.style.height = (textArea.scrollHeight) + "px";
    };
    div.addEventListener('contextmenu', function (event) { 
        if (currentMenuVisible === null) {
            event.preventDefault();
            CreateContextMenuText(event, ['copy', 'paste', 'trash']);
        }
    });
    
    div.appendChild(textArea);

    content.appendChild(div);
}
function AddTextImg() {
    var div = document.getElementById('text');

    var divImg = document.createElement('div');
    divImg.className = "div-add__article__img";
    $(divImg).data("div-type", "image");
    divImg.title = "Поле с изображением.\nНажмите сюда ЛКМ, а после Ctrl+v чтобы вставить изображение из буфера.";

    var articleImg = document.createElement('img');
    articleImg.className = "img-article";
    articleImg.tabIndex = fileRank;
    articleImg.src = "";

    divImg.appendChild(articleImg);
    articleImg.onpaste = function (event) {
        console.log("paste");
        console.log(event);
        var items = (event.clipboardData || event.originalEvent.clipboardData).items;
        var blob = items[0].getAsFile();
        if (blob !== null) {
            event.preventDefault();
            var reader = new FileReader();
            reader.onload = function (event) {
                articleImg.src = event.target.result;
            }

            reader.readAsDataURL(blob);
        }
    };
    var inputRankImg = document.createElement('input');
    inputRankImg.className = "input-file__rank";
    inputRankImg.type = "hidden";
    inputRankImg.value = fileRank++;
    divImg.appendChild(inputRankImg);

    var inputPathImgEdit = document.createElement('input');
    inputPathImgEdit.className = 'input-title__img';
    inputPathImgEdit.type = "file";
    inputPathImgEdit.style.display = "none";
    inputPathImgEdit.addEventListener("change", function () {
        SetAndChangeImgArticle(inputPathImgEdit, articleImg);
    });

    divImg.addEventListener('contextmenu', function (event) {
        if (currentMenuVisible === null) {
            event.preventDefault();
            CreateContextMenuImg(event, inputPathImgEdit, articleImg, ['edit', 'clear', 'trash']);
        }
    });

    divImg.appendChild(inputPathImgEdit);
    div.appendChild(divImg);
}
function SetInputFileImg(input) {
    input.click();
}
function DeleteArticleFileImg(divHover, inputFiles, img) {
    $(divHover).data('isImg', false);
    inputFiles.value = "";
    img.src = '';
}
function SetAndChangeImgArticle(inputFiles, img) {
    if (inputFiles.files && inputFiles.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(img).attr('src', e.target.result);
            $(img).css('visibility', 'visible');
        };

        reader.readAsDataURL(inputFiles.files[0]);
    }
}
