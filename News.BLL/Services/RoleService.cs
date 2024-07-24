using AutoMapper;
using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.BLL.Interfaces;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using News.Infrastructure.Enums;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Helpers;

namespace News.BLL.Services
{
    public class RoleService : IRoleService
    {
        IUnitOfWork Database { get; set; }
        ExecuteTransaction Transaction;
        public RoleService(IUnitOfWork uow)
        {
            Database = uow;
            Transaction = new ExecuteTransaction(uow);
        }
        public async Task<IBaseResponse<bool>> CreateRoleAsync(RoleDTO model, CancellationToken token)
        {
            var baseResponse = new BaseResponse<bool>() 
            { 
                Data = false
            };

            await Transaction.ExecuteTransationAsync(async (model, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var role = new Role()
                    {
                        Name = model.Name
                    };
                    var roles = await Database.Roles.GetAllCollectionAsync(token);
                    if (!roles.Contains(role))
                    {
                        await Database.Roles.CreateAsync(role, token);
                        response.StatusCode = StatusCode.OK;
                        response.Description = "Роль создана";
                    }

                    response.Description = "Роль уже есть";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[CreateRoleAsync] : {ex.Message}";
                    return response;
                }
                
            }, model, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> DeleteRoleByIdAsync(int id, CancellationToken token)
        {
            var baseResponse = new BaseResponse<bool>() 
            { 
                Data = false
            };

            await Transaction.ExecuteTransationAsync(async (model, response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var role = await Database.Roles.GetElementByIdAsync(id, token);
                    if (role != null)
                    {
                        await Database.Roles.DeleteAsync(id, token);
                        response.StatusCode = StatusCode.OK;
                        response.Description = "Роль удалена";
                    }

                    response.Description = "Роль не найдена";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[DeleteRoleByIdAsync] : {ex.Message}";
                    return response;
                }
            }, id, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<RoleDTO>>> GetRolesAsync(CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<RoleDTO>>()
            {
                Data = new List<RoleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleDTO>()).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<Role>, List<RoleDTO>>(await Database.Roles.GetAllCollectionAsync(token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetRolesAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<RoleDTO>>> GetSortedRolesAreaAsync(RoleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<RoleDTO>>()
            {
                Data = new List<RoleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleDTO>()).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<Role>, List<RoleDTO>>(await Database.Roles.GetSortedCollectionArea(columnName, direction, from, to, token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSortedRolesAreaAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<RoleDTO>>> GetSearchRolesAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<RoleDTO>>()
            {
                Data = new List<RoleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleDTO>()).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<Role>, List<RoleDTO>>(await Database.Roles.GetSearchCollectionArea(search, from, to, token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchRolesAreaAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetSearchCountRolesAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };

            await Transaction.ExecuteTransationAsync(async (response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    response.Data = await Database.Roles.GetSearchCountAsync(search, from, to, token);
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchCountRolesAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetCountRolesAsync(int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };

            await Transaction.ExecuteTransationAsync(async (response) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    response.Data = await Database.Roles.GetCountAreaAsync(from, to, token);
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetCountRolesAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
