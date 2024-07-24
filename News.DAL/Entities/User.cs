using System.Collections.Generic;

namespace News.DAL.Entities
{
    public class User 
    {
        public int Id { get; set; }
        public string AvatarPath { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<Role> Roles { get; set; }
        public ICollection<Article> Articles { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
