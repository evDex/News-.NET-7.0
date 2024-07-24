using Azure;
using Dapper;
using Microsoft.Data.SqlClient;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using News.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.DAL.Repositories
{
    public class RoleRepository : IGenericRepository<Role>
    {
        private SqlConnection db;
        public RoleRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<Role> CreateAsync(Role model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Role();
                }

                var response = new Role();
                response = await db.QueryFirstOrDefaultAsync<Role>(@"
                    INSERT INTO [dbo].[Roles]
                    (Name) VALUES(@Name)",
                    new
                    {
                        Name = model.Name
                    });

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Role();
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
                    DELETE FROM [dbo].[Roles] 
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
        public async Task<bool> UpdateAsync(Role model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }
                var response = await db.QueryAsync<bool>(@"
                    UPDATE [dbo].[Roles] 
                    SET Name = @Name
                    WHERE Id = @Id",
                    new
                    {
                        Name = model.Name,
                        Id = model.Id
                    });

                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return await Task.FromResult(false);
            }
        }
        public async Task<Role> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Role();
                }

                var response = new Role();
                response = await db.QueryFirstOrDefaultAsync<Role>(@"
                    SELECT * 
                    FROM [dbo].[Roles] 
                    WHERE Id = @Id",
                    new
                    {
                        Id = id
                    });

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Role();
            }
        }
        public async Task<IEnumerable<Role>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Role>();
                }

                var roles = await db.QueryAsync<Role>(@"
                    SELECT * 
                    FROM [dbo].[Roles]");
                var response = new List<Role>();
                response = roles.ToList();

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Role>();
            }
        }
        public async Task<IEnumerable<Role>> GetSortedCollectionArea(RoleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Role>();
                }

                var sql = "SELECT * " +
                    "FROM [dbo].[Roles] ";
                
                switch (columnName)
                {
                    default:
                    case RoleColumnName.Id:
                        sql += "ORDER BY Roles.Id ";
                        break;
                    case RoleColumnName.Name:
                        sql += "ORDER BY Roles.Name ";
                        break;
                }
                switch(direction)
                {
                    default:
                    case SortDirection.Ascending:
                        sql += "ASC ";
                        break;
                    case SortDirection.Descending:
                        sql += "DESC ";
                        break;
                }
                sql += $"OFFSET {from} ROWS FETCH NEXT {to} ROWS ONLY ";
                var roles = await db.QueryAsync<Role>(sql);
                var response = new List<Role>();
                response = roles.ToList();

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Role>();
            }
        }
        public async Task<Role> GetElementByNameAsync(string name, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Role();
                }
                var response = new Role();
                response = await db.QueryFirstOrDefaultAsync<Role>(@"
                    SELECT *
                    FROM [dbo].[Roles] 
                    WHERE Name = @Name",
                    new
                    {
                        Name = name
                    });

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Role();
            }
        }
        public async Task<IEnumerable<Role>> GetSearchCollectionArea(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Role>();
                }
                search = $"%{search}%";
                var roles = await db.QueryAsync<Role>(@"
                    SELECT * 
                    FROM [dbo].[Roles]
                    WHERE Roles.Name LIKE @Search", new
                {
                    Search = search
                });
                var responce = roles.ToList();

                return responce;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Role>();
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
                    FROM [dbo].[Roles]
                    WHERE Roles.Name LIKE @Search) AS [Count]", new
                {
                    Search = search
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
                    FROM [dbo].[Roles]
					ORDER BY Roles.Id
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
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
