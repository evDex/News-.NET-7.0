using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.Infrastructure.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.BLL.Interfaces
{
    public interface IRoleService : IDisposable
    {
        Task<IBaseResponse<bool>> CreateRoleAsync(RoleDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> DeleteRoleByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<List<RoleDTO>>> GetRolesAsync(CancellationToken token);
        Task<IBaseResponse<List<RoleDTO>>> GetSortedRolesAreaAsync(RoleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token);
        Task<IBaseResponse<List<RoleDTO>>> GetSearchRolesAreaAsync(string search, int from, int to, CancellationToken token);

        Task<IBaseResponse<int>> GetSearchCountRolesAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<int>> GetCountRolesAsync(int from, int to, CancellationToken token);
    }
}
