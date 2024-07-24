using Autofac;
using AutoMapper;
using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.BLL.Interfaces;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using News.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.BLL.Services
{
    public class UserService : IUserService
    {
        IUnitOfWork Database { get; set; }
        ExecuteTransaction Transaction;
        public UserService(IUnitOfWork uow)
        {
            Database = uow;
            Transaction = new ExecuteTransaction(uow);
        }
        public async Task<IBaseResponse<ClaimsIdentity>> LoginAsync(UserDTO model, CancellationToken token)
        {
            var baseResponse = new BaseResponse<ClaimsIdentity>() 
            { 
                Data = new ClaimsIdentity()
            };

            await Transaction.ExecuteTransationAsync(async (model, response) => 
            {
                try
                {
                    StackTrace stackTrace = new StackTrace();
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    StaticLogger.LogError(MethodBase.GetCurrentMethod().DeclaringType, $" | {stackTrace.GetFrame(1).GetMethod().Name} | Start");

                    var user = await Database.Users.GetElementByEmailAsync(model.Email, token);
                    if (user != null)
                    {
                        var hashPassword = HashHelper.Hashing(model.Password);
                        if (hashPassword == user.PasswordHash)
                        {
                            response.Data = Authenticate(user);
                            response.StatusCode = StatusCode.OK;
                            return response;
                        }
                    }

                    response.Description = "Не верный логин или пароль";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[LoginUser] : {ex.Message}";
                    return response;
                }
            },model,baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<ClaimsIdentity>> RegisterAsync(UserDTO model, CancellationToken token)
        {
            var baseResponse = new BaseResponse<ClaimsIdentity>()
            {
                Data = new ClaimsIdentity()
            };

            await Transaction.ExecuteTransationAsync(async (model, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var foundUserEmail = await Database.Users.GetElementByEmailAsync(model.Email, token);
                    var foundUserName = await Database.Users.GetElementByNameAsync(model.UserName, token);

                    if (foundUserEmail == null && foundUserName == null)
                    {
                        var roles = new List<Role>();
                        roles.Add(new Role { Name = "User" });
                        var user = new User
                        {
                            UserName = model.UserName,
                            Email = model.Email,
                            AvatarPath = model.AvatarPath,
                            PasswordHash = HashHelper.Hashing(model.Password),
                            Roles = roles
                        };

                        await Database.Users.CreateAsync(user, token);
                        
                        response.Data = Authenticate(user);
                        response.StatusCode = StatusCode.OK;
                        response.Description = "Вы успешно зарегистрированы!";
                        return response;
                    }
                    response.Description = "Такой пользователь уже существует";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[RegisterAsync] : {ex.Message}";
                    return response;
                }
            }, model, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> CheckUserPasswordAsync(UserDTO model, string inputPassword, CancellationToken token)
        {
            var baseResponse = new BaseResponse<bool>()
            {
                Data = false
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

                    var user = await Database.Users.GetElementByEmailAsync(model.Email, token);
                    if (user.PasswordHash == HashHelper.Hashing(inputPassword))
                    {
                        response.StatusCode = StatusCode.OK;
                        response.Data = true;
                        return response;
                    }

                    response.Data = false;
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[CheckUserPasswordAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> UpdateUserAsync(UserDTO model, CancellationToken token)
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

                    var currentUser = await Database.Users.GetElementByIdAsync(model.Id, token);
                    if (currentUser != null)
                    {
                        currentUser.UserName = model.UserName;
                        currentUser.AvatarPath = model.AvatarPath;
                        currentUser.Email = model.Email;
                        if(currentUser.PasswordHash != model.Password)
                        {
                            currentUser.PasswordHash = HashHelper.Hashing(model.Password);
                        }

                        var roles = new List<Role>();
                        foreach (var item in model.Roles)
                        {
                            roles.Add(await Database.Roles.GetElementByNameAsync(item.Name, token));
                        }  
                        currentUser.Roles = roles;

                        await Database.Users.UpdateAsync(currentUser, token);

                        response.Description = "Пользователь обновлен";
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[UpdateUserAsync] : {ex.Message}";
                    return response;
                }
            },model, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> DeleteUserByIdAsync(int id, CancellationToken token)
        {
            var baseResponse = new BaseResponse<bool>()
            {
                Data = false
            };

            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    User user = await Database.Users.GetElementByIdAsync(id, token);
                    if (user != null)
                    {
                        await Database.Users.DeleteAsync(user.Id, token);

                        response.Description = "Пользователь удален";
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }
                    response.Description = "Пользователь не найден";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[DeleteUserByIdAsync] : {ex.Message}";
                    return response;
                }
            }, id, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<UserDTO>> GetUserByIdAsync(int id, CancellationToken token)
        {
            var baseResponse = new BaseResponse<UserDTO>()
            {
                Data = new UserDTO()
            };
            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var user = await Database.Users.GetElementByIdAsync(id, token);
                    if (user != null)
                    {
                        response.Data = GetUserDTO(user);
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }
                    response.Description = "Пользователь не найден";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(MethodBase.GetCurrentMethod().DeclaringType, " | {UserService} [GetByIdAsync] | " + ex.Message);
                    response.Description = $"[GetUser] : {ex.Message}";
                    return response;
                }
            }, id, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<UserDTO>> GetUserByNameAsync(string userName, CancellationToken token)
        {
            var baseResponse = new BaseResponse<UserDTO>()
            {
                Data = new UserDTO()
            };

            await Transaction.ExecuteTransationAsync(async (userName, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var user = await Database.Users.GetElementByNameAsync(userName, token);
                    if (user != null)
                    {
                        response.Data = GetUserDTO(user);
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }
                    response.Description = "Пользователь не найден";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(MethodBase.GetCurrentMethod().DeclaringType, " | {UserService} [GetByNameAsync] | " + ex.Message);
                    response.Description = $"[GetUser] : {ex.Message}";
                    return response;
                }
            }, userName, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<List<UserDTO>>> GetUsersAsync(CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<UserDTO>>()
            {
                Data = new List<UserDTO>()
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
                    //возможна ошибка с выоборкой ролей
                    //var users = new List<User>();
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()
                                    .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(a => new RoleDTO { Id = a.Id,Name=a.Name})))
                                    .ForMember(dest => dest.Articles, opt => opt.MapFrom(src => src.Articles.Select(a => new ArticleDTO
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Text = a.Text,
                                        NumberOfViews = a.NumberOfViews,
                                        State = a.State,
                                        Date = a.Date
                                    })))
                                    .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Select(a => new CommentDTO
                                    {
                                        Id = a.Id,
                                        Text = a.Text,
                                        Date = a.Date
                                    })))
                                    ).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<User>, List<UserDTO>>(await Database.Users.GetAllCollectionAsync(token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetUsers] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<UserDTO>>> GetUsersAreaAsync(int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<UserDTO>>()
            {
                Data = new List<UserDTO>()
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
                    //возможна ошибка с выоборкой ролей
                    //var users = new List<User>();
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()
                                    .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(a => new RoleDTO { Id = a.Id, Name = a.Name })))
                                    .ForMember(dest => dest.Articles, opt => opt.MapFrom(src => src.Articles.Select(a => new ArticleDTO
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Text = a.Text,
                                        NumberOfViews = a.NumberOfViews,
                                        State = a.State,
                                        Date = a.Date
                                    })))
                                    .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Select(a => new CommentDTO
                                    {
                                        Id = a.Id,
                                        Text = a.Text,
                                        Date = a.Date
                                    })))
                                    ).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<User>, List<UserDTO>>(await Database.Users.GetAreaCollectionAsync(from, to, token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetUsers] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<UserDTO>>> GetSortedUsersAreaAsync(UserColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<UserDTO>>()
            {
                Data = new List<UserDTO>()
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
                    //возможна ошибка с выоборкой ролей
                    //var users = new List<User>();
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()
                                    .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(a => new RoleDTO { Id = a.Id, Name = a.Name })))
                                    .ForMember(dest => dest.Articles, opt => opt.MapFrom(src => src.Articles.Select(a => new ArticleDTO
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Text = a.Text,
                                        NumberOfViews = a.NumberOfViews,
                                        State = a.State,
                                        Date = a.Date
                                    })))
                                    .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Select(a => new CommentDTO
                                    {
                                        Id = a.Id,
                                        Text = a.Text,
                                        Date = a.Date
                                    })))
                                    ).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<User>, List<UserDTO>>(await Database.Users.GetSortedCollectionArea(columnName, direction, from, to, token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSortedUsersAreaAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<UserDTO>>> GetSearchUsersAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<UserDTO>>()
            {
                Data = new List<UserDTO>()
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
                    //возможна ошибка с выоборкой ролей
                    //var users = new List<User>();
                    var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()
                                    .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                                    .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(a => new RoleDTO { Id = a.Id, Name = a.Name })))
                                    .ForMember(dest => dest.Articles, opt => opt.MapFrom(src => src.Articles.Select(a => new ArticleDTO
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Text = a.Text,
                                        NumberOfViews = a.NumberOfViews,
                                        State = a.State,
                                        Date = a.Date
                                    })))
                                    .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Select(a => new CommentDTO
                                    {
                                        Id = a.Id,
                                        Text = a.Text,
                                        Date = a.Date
                                    })))
                                    ).CreateMapper();
                    response.Data = mapper.Map<IEnumerable<User>, List<UserDTO>>(await Database.Users.GetSearchCollectionArea(search, from, to, token));
                    response.StatusCode = StatusCode.OK;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchUsersAreaAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetSearchCountUserAsync(string search, int from, int to, CancellationToken token)
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

                    var count = await Database.Users.GetSearchCountAsync(search, from, to, token);
                    response.StatusCode = StatusCode.OK;
                    response.Data = count;
                    
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchCountUserAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetUsersCountAreaAsync(int from, int to, CancellationToken token)
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

                    var count = await Database.Users.GetCountAreaAsync(from, to, token);
                    response.StatusCode = StatusCode.OK;
                    response.Data = count;

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetUsersCountAreaAsync] : {ex.Message}";
                    return response;
                }
            }, baseResponse);
            return baseResponse;
        }
        private UserDTO GetUserDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Password = user.PasswordHash,
                UserName = user.UserName,
                AvatarPath = user.AvatarPath,
                Email = user.Email,
                Roles = user.Roles.Select(a =>
                        new RoleDTO
                        {
                            Id = a.Id,
                            Name = a.Name
                        }).ToList(),
                Articles = user.Articles.Select( a => 
                            new ArticleDTO
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Text = a.Text,
                                NumberOfViews = a.NumberOfViews,
                                State = a.State,
                                Date = a.Date,
                                HashTags = a.HashTags.Select(c => 
                                            new HashTagDTO 
                                            { 
                                                Id = c.Id,
                                                Name = c.Name
                                            }).ToList(),
                                Comments = a.Comments.Select(c => 
                                    new CommentDTO
                                    {
                                        Id = c.Id,
                                        Text = c.Text,
                                        State = c.State,
                                        Date = c.Date,
                                        UserUploaded = GetUserDTO(c.UploadedUser),
                                    }
                                ).ToList(),
                                Files = a.Files.Select( c => 
                                            new FileDTO
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Path = c.Path,
                                                Rank = c.Rank
                                            }).ToList()
                            }
                ).ToList(),
            };
        }
        private ClaimsIdentity Authenticate(User user)
        {
            ClaimsIdentity identity = new ClaimsIdentity();

            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName)
            };

            foreach (var item in user.Roles)
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, item.Name));
            }

            return new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
