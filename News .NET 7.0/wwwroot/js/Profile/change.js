document.getElementById('input-avatar-img').addEventListener('change', function () {
    console.log("change");
    var img = document.getElementById('img-avatar-user');
    var input = document.getElementById('input-avatar-img');
    ChangeAvatarImg(input, img);
});

function ChangeAvatarImg(inputPathImgEdit, avatarImg) {
    if (inputPathImgEdit.files && inputPathImgEdit.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $(avatarImg).attr('src', e.target.result);
        };

        reader.readAsDataURL(inputPathImgEdit.files[0]);
    }
}
