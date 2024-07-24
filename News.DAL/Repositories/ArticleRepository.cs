using Azure;
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
    public class ArticleRepository : IGenericRepository<Article>
    {
        private SqlConnection db;
        public ArticleRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<Article> CreateAsync(Article model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Article();
                }

                await db.QueryAsync<Article>(@"
                    INSERT INTO [dbo].[Articles] (Name,UserId,Text) 
                    VALUES(@Name,@UserId,@Text)",
                    new
                    {
                        Name = model.Name,
                        UserId = model.UploadedUser.Id,
                        Text = model.Text
                    });
                var response = await db.QueryFirstOrDefaultAsync<Article>(@"
                    SELECT *
                    FROM [dbo].[Articles]
                    WHERE Id = @@IDENTITY");
                if (response != null)
                {
                    foreach (var item in model.HashTags)
                    {
                        await db.QueryFirstOrDefaultAsync<HashTag>(@"
                        IF NOT EXISTS (SELECT * FROM [dbo].[HashTags] WHERE Name = @Name) 
                        INSERT INTO [dbo].[HashTags] (Name) 
                        VALUES(@Name)",
                           new
                           {
                               Name = item.Name
                           });
                        var hashTag = db.QueryFirstOrDefault<HashTag>(@"
                            SELECT * 
                            FROM [dbo].[HashTags] 
                            WHERE Name = @Name",
                            new
                            {
                                Name = item.Name
                            });
                        await db.ExecuteAsync(@"
                        INSERT INTO [dbo].[ArticleHashTags] 
                        VALUES(@ArticleId,@HashTagId)",
                            new
                            {
                                ArticleId = response.Id,
                                HashTagId = hashTag.Id
                            });
                    }

                    foreach (var item in model.Files)
                    {
                        await db.QueryFirstOrDefaultAsync<FileData>(@"
                        IF NOT EXISTS (SELECT * FROM [dbo].[FileData] WHERE Name = @Name AND ArticleId = @ArticleId) 
                        INSERT INTO [dbo].[FileData] (Name,Path,ArticleId,Rank)
                        VALUES(@Name,@Path,@ArticleId,@Rank)",
                        new
                        {
                            Name = item.Name,
                            Path = item.Path,
                            ArticleId = response.Id,
                            Rank = item.Rank
                        });
                    }
                    return response;
                }
                return new Article();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Article();
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
                    DELETE FROM [dbo].[Articles] 
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
        public async Task<bool> UpdateAsync(Article model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var response = await db.QueryAsync<bool>(@"
                    UPDATE [dbo].[Articles] 
                    SET Name = @Name,
                    Text = @Text,
                    NumberOfViews = @NumberOfViews,
                    State = @State,
                    Date = @Date
                    WHERE Id = @Id",
                    new
                    {
                        Name = model.Name,
                        Text = model.Text,
                        NumberOfViews = model.NumberOfViews,
                        State = model.State,
                        Date = model.Date,
                        Id = model.Id
                    });

                var article = await db.QueryFirstOrDefaultAsync<Article>(@"
                    SELECT *
                    FROM [dbo].[Articles]
                    WHERE Id = @Id",
                    new
                    {
                        Id = model.Id
                    });

                if (model.HashTags != null && article != null)
                {
                    var hashTags = await db.QueryAsync<HashTag>(@"
                        SELECT HashTags.* 
                        FROM [dbo].[HashTags]
                        INNER JOIN [dbo].[ArticleHashTags] ON HashTags.Id = ArticleHashTags.HashTagId
                        WHERE ArticleHashTags.ArticleId = @Id",
                        new
                        {
                            Id = article.Id
                        });
                    foreach (var item in hashTags)
                    {
                        if (!model.HashTags.Contains(item))
                        {
                            await db.ExecuteAsync(@"
                                  DELETE FROM [dbo].[HashTags]
                                  WHERE Id = @Id",
                                  new
                                  {
                                      Id = item.Id
                                  });
                            await db.ExecuteAsync(@"
                                  DELETE FROM [dbo].[ArticleHashTags]
                                  WHERE HashTagId = @HashTagId",
                                  new
                                  {
                                      HashTagId = item.Id
                                  });
                        }
                    }
                    foreach (var item in model.HashTags)
                    {
                        if (!hashTags.Contains(item))
                        {
                            await db.ExecuteAsync(@"
                                  IF NOT EXISTS (SELECT * FROM [dbo].[HashTags] WHERE Name = @Name)
                                  INSERT INTO [dbo].[HashTags] (Name) 
                                  VALUES (@Name)",
                                   new
                                   {
                                       Name = item.Name
                                   });
                            var hashTag = await db.QueryFirstOrDefaultAsync<HashTag>(@"
                                SELECT HashTags.* 
                                FROM [dbo].[HashTags]
                                WHERE Name = @Name",
                                new
                                {
                                    Name = item.Name
                                });

                            await db.ExecuteAsync(@"
                                  INSERT INTO [dbo].[ArticleHashTags] (ArticleId,HashTagId)
                                  VALUES(@ArticleId , @HashTagId)",
                                   new
                                   {
                                       ArticleId = article.Id,
                                       HashTagId = hashTag.Id
                                   });
                        }
                    }
                }
                if (model.Files != null && response != null)
                {
                    var fileData = await db.QueryAsync<FileData>(@"
                        SELECT * 
                        FROM [dbo].[FileData] 
                        WHERE ArticleId = @ArticleId",
                        new
                        {
                            ArticleId = article.Id
                        });
                    foreach (var item in fileData)
                    {
                        if (!model.Files.Contains(item))
                        {
                            await db.ExecuteAsync(@"
                                  DELETE FROM [dbo].[FileData]
                                  WHERE Id = @Id",
                                  new
                                  {
                                      Id = item.Id
                                  });
                        }
                    }
                    foreach (var item in model.Files)
                    {
                        if (!fileData.Contains(item))
                        {
                            await db.QueryFirstOrDefaultAsync<FileData>(@"
                                IF NOT EXISTS (SELECT * FROM [dbo].[FileData] WHERE Name = @Name AND ArticleId = @ArticleId) 
                                INSERT INTO [dbo].[FileData] (Name,Path,ArticleId,Rank)
                                VALUES(@Name,@Path,@ArticleId,@Rank)",
                                new
                                {
                                    Name = item.Name,
                                    Path = item.Path,
                                    ArticleId = article.Id,
                                    Rank = item.Rank
                                });
                        }
                    }
                    return response.FirstOrDefault();
                }
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return await Task.FromResult(false);
            }
        }
        public async Task<Article> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Article();
                }

                var response = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id 
                    WHERE  Articles.Id = @Id",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    Id = id
                });
                if (response != null)
                {
                    var hashTags = await db.QueryAsync<HashTag>(@"
                        SELECT HashTags.* 
                        FROM [dbo].[HashTags] 
                        INNER JOIN [dbo].[ArticleHashTags] ON HashTags.Id  = ArticleHashTags.HashTagId
                        WHERE ArticleHashTags.ArticleId = @Id", new
                    {
                        Id = id
                    });
                    response.First().HashTags = hashTags.ToList();

                    var fileData = await db.QueryAsync<FileData>(@"
                        SELECT *
                        FROM [dbo].[FileData]
                        WHERE ArticleId = @ArticleId", new
                    {
                        ArticleId = response.First().Id
                    });
                    response.First().Files = fileData.ToList();

                    var comments = await db.QueryAsync<Comment>(@"
                        SELECT Comments.*
                        FROM [dbo].[Comments]
                        WHERE Comments.ArticleId = @ArticleId AND Id NOT IN (SELECT Comments.Id
                        FROM [dbo].[Comments]
                        INNER JOIN [dbo].[ReplyToComments] ON Comments.Id = ReplyToComments.CommentId)", new
                    {
                        ArticleId = response.First().Id
                    });

                    foreach (var item in comments)
                    {
                        var answers = await db.QueryAsync<Comment>(@"
                            SELECT Comments.*
                            FROM [dbo].[Comments]
                            WHERE Comments.ArticleId = @ArticleId AND Id IN (SELECT ReplyToComments.CommentId
                            FROM [dbo].[ReplyToComments]
                            WHERE ReplyToComments.ReplyToCommentId = @ReplyToCommentsId)", new
                        {
                            ArticleId = response.First().Id,
                            ReplyToCommentsId = item.Id
                        });
                        foreach (var reply in answers.ToList())
                        {
                            var replyUser = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            INNER JOIN [dbo].[Comments] ON Comments.UserId = Users.Id
                            WHERE Comments.Id = @Id", new
                            {
                                Id = reply.Id
                            });
                            reply.UploadedUser = replyUser;
                        }


                        var user = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            INNER JOIN [dbo].[Comments] ON Comments.UserId = Users.Id
                            WHERE Comments.Id = @Id", new
                        {
                            Id = item.Id
                        });
                        var article = await db.QueryFirstOrDefaultAsync<Article>(@"
                            SELECT *
                            FROM [dbo].[Articles]
                            WHERE Id = @Id", new
                        {
                            Id = response.First().Id
                        });

                        item.UploadedUser = user;
                        item.UploadedArticle = article;
                        item.ReplyToComment = answers.ToList();
                    }

                    response.First().Comments = comments.ToList();
                    return response.FirstOrDefault();
                }
                return new Article();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Article();
            }
        }
        public async Task<IEnumerable<Article>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }

                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetAreaCollectionInUserAsync(int userID, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }

                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    WHERE Users.Id = @UserID
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    UserID = userID,
                    From = from,
                    To = to
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetSortedCollectionArea(ArticleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }
                string sql = String.Empty;
                foreach (var item in GetSqlSortedRequest(columnName, direction, from, to))
                {
                    sql += item;
                }

                var articles = await db.QueryAsync<Article, User, Article>(sql, (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetSearchCollectionArea(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }
                search = $"%{search}%";
                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    WHERE Articles.Name LIKE @Search
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    Search = search,
                    From = from,
                    To = to
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetSearchCollectionArticlesByUserNameAreaAsync(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }
                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    WHERE Users.UserName LIKE @Search
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    Search = search,
                    From = from,
                    To = to
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetSearchCollectionArticlesByHashTagsAreaAsync(string search, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }
                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    INNER JOIN [dbo].[ArticleHashTags] ON Articles.Id = ArticleHashTags.ArticleId
					INNER JOIN [dbo].[HashTags] ON HashTags.Id = ArticleHashTags.HashTagId
                    WHERE HashTags.Name LIKE @Search
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    Search = search,
                    From = from,
                    To = to
                });

                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public async Task<IEnumerable<Article>> GetAreaCollectionAsync(int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }

                var articles = await db.QueryAsync<Article, User, Article>(@"
                    SELECT Articles.*,Users.* 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (article, user) =>
                {
                    article.UploadedUser = user;
                    return article;
                }, new
                {
                    From = from,
                    To = to
                });
                var response = await InitArticlesAsync(articles, token);
                if (response != null)
                {
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
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
                //SELECT COUNT(*)
                //FROM(SELECT * FROM Articles ORDER BY Articles.Id OFFSET 0 ROWS FETCH NEXT 15 ROWS ONLY) as [Count]
                var count = await db.QueryFirstOrDefaultAsync<int>(@"
                    SELECT COUNT(*)
                    FROM (SELECT * FROM Articles ORDER BY Articles.Id OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) as [Count]", new
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
        public async Task<int> GetArticlesSearchCountAreaAsync(string search, int from, int to, CancellationToken token)
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
                    FROM (SELECT * FROM Articles 
                    WHERE Articles.Name LIKE @Search
                    ORDER BY Articles.Id 
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) as [Count]", new
                {
                    From = from,
                    To = to,
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
        public async Task<int> GetArticlesSearchByUserNameCountAreaAsync(string search, int from, int to, CancellationToken token)
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
                    FROM (SELECT Articles.Id 
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    WHERE Users.UserName LIKE @Search
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) as [Count]", new
                {
                    From = from,
                    To = to,
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
        public async Task<int> GetArticlesSearchByHashTagCountAreaAsync(string search, int from, int to, CancellationToken token)
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
                    FROM (SELECT Articles.*
                    FROM [dbo].[Articles] 
                    INNER JOIN [dbo].[Users] ON Articles.UserId = Users.Id
                    INNER JOIN [dbo].[ArticleHashTags] ON Articles.Id = ArticleHashTags.ArticleId
					INNER JOIN [dbo].[HashTags] ON HashTags.Id = ArticleHashTags.HashTagId
                    WHERE HashTags.Name LIKE @Search
                    ORDER BY Articles.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY) as [Count]", new
                {
                    From = from,
                    To = to,
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
        private List<string> GetSqlSortedRequest(ArticleColumnName columnName, SortDirection direction, int from, int to)
        {
            List<string> request = new List<string>();
            request.Add("SELECT A.Id, A.UserId, A.Name, A.Text, A.State, A.Date, A.NumberOfViews, U.Id, U.UserName, U.Email, COUNT(C.Id) as CountOfComments ");
            request.Add("FROM [dbo].[Articles] A ");
            request.Add("INNER JOIN [dbo].[Users] U ON A.UserId = U.Id ");
            request.Add("LEFT JOIN [dbo].[Comments] C ON A.Id = C.ArticleId  ");
            request.Add("GROUP BY A.Id, A.UserId, A.Name, A.Text, A.State, A.Date, A.NumberOfViews, U.Id, U.UserName, U.Email ");
            switch (columnName)
            {
                default:
                case ArticleColumnName.Id:
                    request.Add("ORDER BY A.Id ");
                    //sql += " ORDER BY A.Id";
                    break;
                case ArticleColumnName.Name:
                    request.Add("ORDER BY A.Name ");
                    //sql += " ORDER BY A.Name";
                    break;
                case ArticleColumnName.UserName:
                    request.Add("ORDER BY U.UserName ");
                    //sql += " ORDER BY U.UserName";
                    break;
                case ArticleColumnName.NumberOfViews:
                    request.Add("ORDER BY A.NumberOfViews ");
                    //sql += " ORDER BY A.NumberOfViews";
                    break;
                case ArticleColumnName.NumberOfComments:
                    request.Add("ORDER BY CountOfComments ");
                    //sql += " ORDER BY CountOfComments";
                    break;
                case ArticleColumnName.Date:
                    request.Add("ORDER BY A.Date ");
                    //sql += " ORDER BY A.Date";
                    break;
                case ArticleColumnName.State:
                    request.Add("ORDER BY A.State ");
                    //sql += " ORDER BY A.State";
                    break;
            }
            switch (direction)
            {
                default:
                case SortDirection.Ascending:
                    request.Add("ASC ");
                    //sql += " ASC";
                    break;
                case SortDirection.Descending:
                    request.Add("DESC ");
                    //sql += " DESC";
                    break;
            }
            request.Add($"OFFSET {from} ROWS FETCH NEXT {to} ROWS ONLY ");
            //sql += $" OFFSET {from} ROWS FETCH NEXT {to} ROWS ONLY";
            return request;
        }
        private async Task<IEnumerable<Article>> InitArticlesAsync(IEnumerable<Article> articles, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Article>();
                }
                var response = articles.ToList();
                if (response != null)
                {
                    for (int i = 0; i < response.Count(); i++)
                    {
                        var hashTags = await db.QueryAsync<HashTag>(@"
                            SELECT HashTags.* 
                            FROM [dbo].[HashTags] 
                            INNER JOIN [dbo].[ArticleHashTags] ON HashTags.Id = ArticleHashTags.HashTagId
                            WHERE ArticleHashTags.ArticleId = @ArticleId",
                            new
                            {
                                ArticleId = response[i].Id
                            });

                        response[i].HashTags = hashTags.ToList();
                    }
                    for (int i = 0; i < response.Count(); i++)
                    {
                        var fileData = await db.QueryAsync<FileData>(@"
                            SELECT * 
                            FROM [dbo].[FileData] 
                            WHERE ArticleId = @ArticleId", new
                        {
                            ArticleId = response[i].Id
                        });
                        response[i].Files = fileData.ToList();
                    }
                    for (int i = 0; i < response.Count(); i++)
                    {
                        var comments = await db.QueryAsync<Comment>(@"
                            SELECT *
                            FROM [dbo].[Comments]
                            WHERE ArticleId = @ArticleId ", new
                        {
                            ArticleId = response[i].Id
                        });
                        response[i].Comments = comments.ToList();

                        foreach (var item in comments)
                        {
                            //var answers = await db.QueryAsync<Comment>(@"
                            //    SELECT Comments.*
                            //    FROM [dbo].[Comments]
                            //    INNER JOIN [dbo].[ReplyToComments] ON Comments.Id = ReplyToComments.CommentId
                            //    WHERE Comments.ArticleId = @ArticleId AND ReplyToComments.ReplyToCommentId = @ReplyToCommentsId",
                            //    new
                            //    {
                            //        ArticleId = response[i].Id,
                            //        ReplyToCommentsId = item.Id
                            //    });
                            var user = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            INNER JOIN [dbo].[Comments] ON Comments.UserId = Users.Id
                            WHERE Comments.Id = @Id", new
                            {
                                Id = item.Id
                            });
                            var article = await db.QueryFirstOrDefaultAsync<Article>(@"
                            SELECT *
                            FROM [dbo].[Articles]
                            WHERE Id = @Id", new
                            {
                                Id = response.First().Id
                            });

                            item.UploadedUser = user;
                            item.UploadedArticle = article;
                        }
                    }
                    return response;
                }
                return new List<Article>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Article>();
            }
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
