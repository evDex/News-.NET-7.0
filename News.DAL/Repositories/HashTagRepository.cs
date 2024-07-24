using Azure;
using Dapper;
using Microsoft.Data.SqlClient;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace News.DAL.Repositories
{
    public class HashTagRepository : IGenericRepository<HashTag>
    {
        private SqlConnection db;
        public HashTagRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<HashTag> CreateAsync(HashTag model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new HashTag();
                }

                var response = new HashTag();
                await db.QueryFirstOrDefaultAsync<HashTag>(@"
                    INSERT INTO [dbo].[HashTags] (Name) 
                    VALUES(@Name)", 
                    new
                    {
                        Name = model.Name
                    });
                response = await db.QueryFirstOrDefaultAsync<HashTag>(@"
                    SELECT *
                    FROM [dbo].[HastTags]
                    WHERE Id = @@IDENTITY");

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new HashTag();
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
                    DELETE FROM [dbo].[HashTags] 
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
        public async Task<bool> UpdateAsync(HashTag model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var response = await db.QueryAsync<bool>(@"
                    UPDATE [dbo].[HashTags] 
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
        public async Task<HashTag> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new HashTag();
                }

                var response = new HashTag();
                response = await db.QueryFirstOrDefaultAsync<HashTag>(@"
                    SELECT * 
                    FROM [dbo].[HashTags] 
                    WHERE Id = @Id", 
                    new 
                    { 
                        Id = id 
                    });

                if (response == null)
                {
                    await InitializeLinksHashTag(response);
                }

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new HashTag();
            }
        }
        public async Task<IEnumerable<HashTag>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<HashTag>();
                }

                var hashTags = await db.QueryAsync<HashTag>(@"
                    SELECT * 
                    FROM [dbo].[HashTags]");

                var response = hashTags.ToList();
                if (response == null)
                {
                    for (int i = 0; i < response.Count(); i++)
                    {
                        await InitializeLinksHashTag(response[i]);
                    }
                    return response;
                }

                return new List<HashTag>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<HashTag>();
            }
        }
        private async Task<HashTag> InitializeLinksHashTag(HashTag hashTag)
        {
            if (hashTag == null)
            {
                var articles = await db.QueryAsync<Article>(@"
                        SELECT Articles.* 
                        FROM [dbo].[Articles]
                        INNER JOIN [dbo].[ArticleHashTags] ON Articles.Id = ArticleHashTags.Article.Id
                        WHERE ArticleHashTags.HashTagId = @Id",
                        new
                        {
                            Id = hashTag.Id
                        });

                hashTag.Articles = articles.ToList();
            }
            return hashTag;
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
