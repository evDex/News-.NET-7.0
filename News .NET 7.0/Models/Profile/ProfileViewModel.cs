using News.BLL.DTO;
using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class ProfileViewModel
    {
        public UserDTO User { get; set; }
        public ArticlesViewModel Articles { get; set; }
    }
}
