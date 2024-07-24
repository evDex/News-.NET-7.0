using Dapper;
using Microsoft.Data.SqlClient;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using News.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.DAL.Repositories
{
    public class UserRepository : IGenericRepository<User>
    {
        private SqlConnection db;
        public UserRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<User> CreateAsync(User model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new User();
                }

                await db.QueryFirstOrDefaultAsync<User>(@"
                    INSERT INTO [dbo].[Users] (AvatarPath,UserName,Email,PasswordHash) 
                    VALUES(@AvatarPath,@UserName,@Email,@PasswordHash)",
                    new
                    {
                        AvatarPath = model.AvatarPath,
                        UserName = model.UserName,
                        Email = model.Email,
                        PasswordHash = model.PasswordHash
                    });
                var response = new User();
                response = await db.QueryFirstOrDefaultAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users] 
                    WHERE UserName= @UserName",
                    new
                    {
                        UserName = model.UserName
                    });

                foreach (var item in model.Roles)
                {
                    await db.QueryFirstOrDefaultAsync<Role>(@"
                        IF NOT EXISTS (SELECT * FROM [dbo].[HashTags] WHERE Name = @Name)  
                        INSERT INTO [dbo].[Roles] (Name) 
                        VALUES (@Name)",
                        new
                        {
                            Name = item.Name
                        });
                    var role = db.QueryFirstOrDefault<Role>(@"
                        SELECT *
                        FROM [dbo].[Roles]
                        WHERE Name = @Name",
                        new
                        {
                            Name = item.Name
                        });

                    await db.ExecuteAsync(@"
                        INSERT INTO [dbo].[UserRoles] (UserId,RoleId) 
                        VALUES (@UserId,@RoleId)",
                        new 
                        { 
                            UserId = response.Id, 
                            RoleId = role.Id 
                        });
                }

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new User();
            }
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var response = await db.QueryAsync<bool>(@"
                    DELETE FROM [dbo].[Users] 
                    WHERE Id=@Id",
                   new
                   {
                       Id = id
                   });

                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return await Task.FromResult(false);
            }
        }
        public async Task<bool> UpdateAsync(User model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var user = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            WHERE Users.Id = @Id",
                            new
                            {
                                Id = model.Id
                            });
                if(user != null)
                {
                    await db.QueryAsync(@"
                    UPDATE [dbo].[Users] 
                    SET AvatarPath = @AvatarPath,
                    UserName = @UserName,
                    Email = @Email,
                    PasswordHash = @PasswordHash
                    WHERE Id = @Id",
                    new
                    {
                        AvatarPath = model.AvatarPath,
                        UserName = model.UserName,
                        Email = model.Email,
                        PasswordHash = model.PasswordHash,
                        Id = model.Id
                    });
                }
                

                if(model.Roles != null && user != null)
                {
                    var roles = await db.QueryAsync<Role>(@"
                        SELECT Roles.*
                        FROM [dbo].[Roles]
                        INNER JOIN [dbo].[UserRoles] ON Roles.Id = UserRoles.RoleId
                        WHERE UserRoles.UserId = @Id",
                        new
                        {
                            Id = user.Id
                        });
                    foreach (var item in roles)
                    {
                        if(!model.Roles.Contains(item))
                        {
                            await db.ExecuteAsync(@"
                                    DELETE FROM [dbo].[UserRoles] 
                                    WHERE UserId = @UserId AND RoleId = @RoleId",
                                    new
                                    {
                                        UserId = user.Id,
                                        RoleId = item.Id
                                    });
                        }
                    }
                    foreach (var item in model.Roles)
                    {
                        if(!roles.Contains(item))
                        {
                            await db.ExecuteAsync(@"
                                    INSERT INTO [dbo].[UserRoles] (UserId,RoleId)
                                    VALUES (@UserId,@RoleId)",
                                    new
                                    {
                                        UserId = user.Id,
                                        RoleId = item.Id
                                    });
                        }
                    }
                    return await Task.FromResult(true);
                }
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return await Task.FromResult(false);
            }
        }
        private async Task<User> InitializeLinksUserAsync(User user)
        {
            if(user != null)
            {
                var userRoles = await db.QueryAsync<Role>(@"
                        SELECT Roles.* 
                        FROM [dbo].[Roles] 
                        INNER JOIN [dbo].[UserRoles] ON UserRoles.RoleId = Roles.Id 
                        WHERE UserRoles.UserId = @Id",
                    new
                    {
                        Id = user.Id
                    });
                var userArticles = await db.QueryAsync<Article>(@"
                    SELECT *
                    FROM [dbo].[Articles]
                    WHERE UserId = @Id",
                    new
                    {
                        Id = user.Id
                    });
                foreach (var item in userArticles)
                {
                    var userArticlesFiles = await db.QueryAsync<FileData>(@"
                        SELECT *
                        FROM [dbo].[FileData]
                        WHERE ArticleId = @Id",
                        new
                        {
                            Id = item.Id
                        });
                    item.Files = userArticlesFiles.ToList();
                }
                var userComments = await db.QueryAsync<Comment>(@"
                    SELECT *
                    FROM [dbo].[Comments]
                    WHERE UserId = @Id",
                    new
                    {
                        Id = user.Id
                    });
                user.Roles = userRoles.ToList();
                user.Articles = userArticles.ToList();
                user.Comments = userComments.ToList();
            }

            return user;
        }
        public async Task<User> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new User();
                }

                var response = new User();
                response = await db.QueryFirstOrDefaultAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users] 
                    WHERE Id = @Id", 
                    new 
                    { 
                        Id = id 
                    });

                await InitializeLinksUserAsync(response);
                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new User();
            }        
        }
        public async Task<IEnumerable<User>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if(token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<User>();
                }

                var users = await db.QueryAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users]");

                var response = users.ToList();

                if(response != null)
                {
                    for (int i = 0; i < response.Count(); i++)
                    {
                        await InitializeLinksUserAsync(response[i]);
                    }
                    return response;
                }
                return new List<User>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<User>();
            }
        }
        public async Task<IEnumerable<User>> GetSearchCollectionArea(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<User>();
                }
                search = $"%{search}%";
                var users = await db.QueryAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users]
                    WHERE Users.UserName LIKE @Search
                    ORDER BY Users.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY", new
                {
                    Search = search,
                    From = from,
                    To = to
                });

                var response = users.ToList();

                if (response != null)
                {
                    for (int i = 0; i < response.Count(); i++)
                    {
                        await InitializeLinksUserAsync(response[i]);
                    }
                    return response;
                }
                return new List<User>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<User>();
            }
        }
        public async Task<int> GetSearchCountAsync(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return 0;
                }
                search = $"%{search}%";
                var count = await db.QueryFirstOrDefaultAsync<int>(@"
                    SELECT COUNT(*)
                    FROM (SELECT * 
                    FROM [dbo].[Users]
                    WHERE Users.UserName LIKE @Search
                    ORDER BY Users.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) AS [Count]", new
                {
                    Search = search,
                    From = from,
                    To = to
                });
                
                return count;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return 0;
            }
        }
        public async Task<int> GetCountAreaAsync(int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return 0;
                }
                var count = await db.QueryFirstOrDefaultAsync<int>(@"
                    SELECT COUNT(*)
                    FROM (SELECT * 
                    FROM [dbo].[Users]
                    ORDER BY Users.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) AS [Count]", new
                {
                    From = from,
                    To = to
                });

                return count;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<User>> GetSortedCollectionArea(UserColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<User>();
                }
                var responce = new List<User>();

                string sql = "SELECT Users.Id,Users.AvatarPath, Users.UserName, Users.Email, Users.PasswordHash, COUNT(Articles.Id) as CountOfArticles " +
                    "FROM Users " +
                    "INNER JOIN Articles ON Articles.UserId = Users.Id " +
                    "GROUP BY Users.Id,Users.AvatarPath, Users.UserName, Users.Email, Users.PasswordHash ";
                switch (columnName)
                {
                    default:
                    case UserColumnName.Id:
                        sql += "ORDER BY Users.Id ";
                        break;
                    case UserColumnName.UserName:
                        sql += "ORDER BY Users.UserName ";
                        break;
                    case UserColumnName.Email:
                        sql += "ORDER BY Users.UserName ";
                        break;
                    case UserColumnName.Articles:
                        sql += "ORDER BY CountOfArticles ";
                        break;
                }

                switch (direction)
                {
                    default:
                    case SortDirection.Ascending:
                        sql += "ASC ";
                        break;
                    case SortDirection.Descending:
                        sql += "DESC ";
                        break;
                }
                sql += $"OFFSET {from} ROWS FETCH NEXT {to} ROWS ONLY";

                var users = await db.QueryAsync<User>(sql);
                
                foreach (var item in users)
                {
                    responce.Add(await InitializeLinksUserAsync(item));
                }

                return responce;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<User>();
            }
        }
        public async Task<IEnumerable<User>> GetAreaCollectionAsync(int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<User>();
                }

                var users = await db.QueryAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users]
                    ORDER BY Users.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY", 
                    new
                    {
                        From = from,
                        To = to
                    });
                var response = users.ToList();

                if (response != null)
                {
                    for (int i = 0; i < response.Count(); i++)
                    {
                        await InitializeLinksUserAsync(response[i]);
                    }
                    return response;
                }
                return new List<User>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<User>();
            }
        }
        public async Task<User> GetElementByNameAsync(string name, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new User();
                }

                var response = await db.QueryFirstOrDefaultAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users] 
                    WHERE UserName = @Name", 
                    new 
                    { 
                        Name = name
                    });
                if(response != null)
                {
                    await InitializeLinksUserAsync(response);
                    return response;
                }

                return new User();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new User();
            }
        }
        public async Task<User> GetElementByEmailAsync(string email, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new User();
                }

                var response = await db.QueryFirstOrDefaultAsync<User>(@"
                    SELECT * 
                    FROM [dbo].[Users] 
                    WHERE Email = @Email", 
                    new 
                    { 
                        Email = email 
                    });
                if (response != null)
                {
                    await InitializeLinksUserAsync(response);
                    return response;
                }

                return new User();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new User();
            }
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
