using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.BLL.Interfaces
{
    public interface IUserService : IDisposable
    {
        Task<IBaseResponse<ClaimsIdentity>> LoginAsync(UserDTO model, CancellationToken token);
        Task<IBaseResponse<ClaimsIdentity>> RegisterAsync(UserDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> CheckUserPasswordAsync(UserDTO model, string password, CancellationToken token);

        Task<IBaseResponse<bool>> UpdateUserAsync(UserDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> DeleteUserByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<UserDTO>> GetUserByIdAsync(int id, CancellationToken token);
        Task<IBaseResponse<UserDTO>> GetUserByNameAsync(string userName, CancellationToken token);

        Task<IBaseResponse<List<UserDTO>>> GetUsersAsync(CancellationToken token);
        Task<IBaseResponse<List<UserDTO>>> GetUsersAreaAsync(int from, int to, CancellationToken token);
        Task<IBaseResponse<List<UserDTO>>> GetSortedUsersAreaAsync(UserColumnName columnName, SortDirection direction, int from, int to, CancellationToken token);
        Task<IBaseResponse<List<UserDTO>>> GetSearchUsersAreaAsync(string search, int from, int to, CancellationToken token);

        Task<IBaseResponse<int>> GetSearchCountUserAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<int>> GetUsersCountAreaAsync(int from, int to, CancellationToken token);
    }
}
