using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Email")]
        [Required (ErrorMessage = "Не указан Email")]
        public string Email { get; set; }
        [Display(Name = "Пароль")]
        [Required (ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }
}
