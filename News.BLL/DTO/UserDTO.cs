using System.Collections.Generic;

namespace News.BLL.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string AvatarPath { get; set; }
        public string Email { get; set; }
        public List<RoleDTO> Roles { get; set; }
        public List<ArticleDTO> Articles { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }
}
