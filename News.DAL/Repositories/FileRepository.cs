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
    public class FileRepository : IGenericRepository<FileData>
    {
        private SqlConnection db;
        public FileRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<FileData> CreateAsync(FileData model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new FileData();
                }

                //Возможна ошибка с тем что не найдет Name,Path , явно указать
                var response = new FileData();
                response = await db.QueryFirstOrDefaultAsync<FileData>(@"
                    INSERT INTO [dbo].[FileData] (Name,Path,ArticleId,Rank) 
                    VALUES (@Name,@Path,@ArticleId,@Rank)",
                    new
                    {
                        Name = model.Name,
                        Path = model.Path,
                        ArticleId = model.UploadedArticle.Id,
                        Rank = model.Rank
                    });

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new FileData();
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
                    DELETE FROM [dbo].[FileData] 
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
        public async Task<bool> UpdateAsync(FileData model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var response = await db.QueryAsync<bool>(@"
                    UPDATE [dbo].[FileData] 
                    SET Name = @Name , 
                    Path = @Path , 
                    ArticleId = @ArticleId,
                    Rank = @Rank,
                    WHERE Id = @Id",
                    new
                    {
                        Name = model.Name,
                        Path = model.Path,
                        ArticleId = model.UploadedArticle.Id,
                        Rank = model.Rank,
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
        public async Task<FileData> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new FileData();
                }

                var response = new FileData();
                var file = await db.QueryAsync<FileData, Article, FileData>(@"
                    SELECT FileDatas.*,Articles.* 
                    FROM [dbo].[FileData] 
                    INNER JOIN [dbo].[Articles] ON FileDatas.ArticleId = Articles.Id 
                    WHERE FileDatas.Id = Id",
                (fileData, article) =>
                {
                    fileData.UploadedArticle = article;
                    return fileData;
                },
                new
                {
                    Id = id
                });
                response = file.FirstOrDefault();

                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new FileData();
            }
        }
        public async Task<IEnumerable<FileData>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<FileData>();
                }

                var response = await db.QueryAsync<FileData, Article, FileData>(@"
                    SELECT FileDatas.*,Articles.* 
                    FROM [dbo].[FileData] 
                    INNER JOIN [dbo].[Articles] ON FileDatas.ArticleId = Articles.Id ",
                (file_data, article) =>
                {
                    file_data.UploadedArticle = article;
                    return file_data;
                });

                if (response != null)
                {
                    return response;
                }

                return new List<FileData>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<FileData>();
            }
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
