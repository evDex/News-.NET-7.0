using Dapper;
using News.DAL.Entities;
using News.Infrastructure.Common;
using System.Collections.Generic;
using System.Linq;

namespace News.DAL.Dapper
{
    public class NewsDbInitialize
    {
        public async static void InitData()
        {
            using (var db = NewsEntities.Connection())
            {
                db.Open();
                #region InitRoles
                var checkRoles = await db.QueryFirstOrDefaultAsync<Role>(@"
                    SELECT TOP 1 * 
                    FROM [dbo].[Roles]");
                if (checkRoles == null)
                {
                    var initRoles = new List<Role>();
                    initRoles.Add(new Role()
                    {
                        Name = "ChiefAdmin"
                    });
                    initRoles.Add(new Role()
                    {
                        Name = "Admin"
                    });
                    initRoles.Add(new Role()
                    {
                        Name = "User"
                    });
                    foreach (var item in initRoles)
                    {
                        await db.QueryFirstOrDefaultAsync<Role>(@"
                                INSERT INTO [dbo].[Roles]
                                (Name) VALUES(@Name)", item);
                    }
                }
                #endregion

                #region InitUsers
                var checkUsers = await db.QueryFirstOrDefaultAsync<User>(@"
                    SELECT TOP 1 * 
                    FROM [dbo].[Users]");
                if (checkUsers == null && checkRoles == null)
                {
                    var initUsers = new List<User>();
                    initUsers.Add(new User()
                    {
                        UserName = "admin",
                        Email = "admin@gmail.com",
                        PasswordHash = HashHelper.Hashing("123456"),
                        Roles = new List<Role>()
                    {
                        new Role()
                        {
                            Name = "ChiefAdmin"
                        }
                    }
                    });
                    #region InitUsersRoles
                    foreach (var item in initUsers)
                    {
                        await db.QueryFirstOrDefaultAsync<User>(@"
                                INSERT INTO [dbo].[Users] (AvatarPath,UserName,Email,PasswordHash) 
                                VALUES(@AvatarPath,@UserName,@Email,@PasswordHash)",
                              item);
                    }
                    foreach (var itemUser in initUsers)
                    {
                        var user = await db.QueryFirstOrDefaultAsync<User>(@"
                        SELECT *
                        FROM [dbo].[Users]
                        WHERE UserName = @UserName",
                        new
                        {
                            UserName = itemUser.UserName
                        });
                        foreach (var itemRole in itemUser.Roles)
                        {
                            var role = db.QueryFirstOrDefault<Role>(@"
                            SELECT *
                            FROM [dbo].[Roles]
                            WHERE Name = @Name",
                                itemRole);
                            await db.ExecuteAsync(@"
                                    INSERT INTO [dbo].[UserRoles] (UserId,RoleId) 
                                    VALUES (@UserId,@RoleId)",
                            new
                            {
                                UserId = user.Id,
                                RoleId = role.Id
                            });
                        }
                    }
                    #endregion
                }
                #endregion
            }
        }
    }
}
