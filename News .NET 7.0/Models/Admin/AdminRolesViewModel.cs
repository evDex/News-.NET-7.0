using News.BLL.DTO;
using News.Infrastructure;

namespace News.Models
{
    public class AdminRolesViewModel
    {
        public List<RoleDTO> Roles { get; set; }
        public int RolesCount { get; set; }
        public int NumberOfRolesViews { get; set; } = Config.NumberOfTableRows;
    }
}
