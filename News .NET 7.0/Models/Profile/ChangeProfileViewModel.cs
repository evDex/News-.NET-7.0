using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class ChangeProfileViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string OldAvatarPath { get; set; }
        [Display(Name = "Аватар")]
        public IFormFile NewAvatarPath { get; set; }

        [Display(Name = "Старый пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Display(Name = "Новый пароль")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmNewPassword { get; set; }
    }
}
