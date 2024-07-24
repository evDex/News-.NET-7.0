using News.BLL.DTO;
using News.Infrastructure;

namespace News.Models
{
    public class AdminUserRolesViewModel
    {
        public List<UserDTO> Users { get; set; }
        public List<RoleDTO> Roles { get; set; }
        public int NumberOfUsersView { get; set; } = 9;
        public int UsersCount { get; set; }
    }
}
