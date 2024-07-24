using Dapper;
using Microsoft.Data.SqlClient;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace News.DAL.Repositories
{
    public class CommentRepository : IGenericRepository<Comment>
    {
        private SqlConnection db;
        public CommentRepository(SqlConnection connection)
        {
            this.db = connection;
        }
        public async Task<Comment> CreateAsync(Comment model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Comment();
                }

                //Возможна ошибка с тем что не найдет text,явно указать
                var response = await db.QueryFirstOrDefaultAsync<Comment>(@"
                    INSERT INTO [dbo].[Comments] (Text,UserId,ArticleId,Date) 
                    VALUES(@Text,@UserId,@ArticleId,@Date)",
                    new
                    {
                        Text = model.Text,
                        UserId = model.UploadedUser.Id,
                        ArticleId = model.UploadedArticle.Id,
                        Date = model.Date
                    });

                if (response != null)
                {
                    return response;
                }
                return new Comment();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Comment();
            }
        }
        public async Task<Comment> CreateReplyCommentAsync(Comment model,int commentId, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Comment();
                }

                //Возможна ошибка с тем что не найдет text,явно указать
                await db.QueryAsync<Comment>(@"
                    INSERT INTO [dbo].[Comments] (Text,UserId,ArticleId,Date) 
                    VALUES(@Text,@UserId,@ArticleId,@Date)",
                    new
                    {
                        Text = model.Text,
                        UserId = model.UploadedUser.Id,
                        ArticleId = model.UploadedArticle.Id,
                        Date = model.Date
                    });
                var response = await db.QueryFirstOrDefaultAsync<Comment>(@"
                    SELECT *
                    FROM [dbo].[Comments]
                    WHERE Id = @@IDENTITY");

                await db.QueryAsync<Comment>(@"
                    INSERT INTO [dbo].[ReplyToComments] (ReplyToCommentId, CommentId) 
                    VALUES(@ReplyToCommentId, @CommentId)",
                    new
                    {
                        ReplyToCommentId = commentId,
                        CommentId = response.Id
                    });

                if (response != null)
                {
                    return response;
                }
                return new Comment();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Comment();
            }
        }
        public async Task<Comment> CreateInArticle(Comment model,int replyToCommenId, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Comment();
                }

                await db.QueryAsync<Comment>(@"
                    INSERT INTO [dbo].[Comments] (Text,UserId,ArticleId,ReplyToCommentId) 
                    VALUES(@Text,@UserId,@ArticleId)",
                    new
                    {
                        Text = model.Text,
                        UserId = model.UploadedUser.Id,
                        ArticleId = model.UploadedArticle.Id
                    });
                var response = await db.QueryFirstAsync<Comment>(@"
                    SELECT *
                    FROM [dbo].[Comments]
                    WHERE Id = @@IDENTITY");
                if (response != null)
                {
                    //CommentId : ид комантария с ответом / ReplyToCommentId : ид коментария к котому адресован ответ  
                    await db.QueryFirstOrDefaultAsync<Comment>(@"
                    INSERT INTO [dbo].[ReplyToComments] (CommentId,ReplyToCommentId) 
                    VALUES(@CommentId,@ReplyToCommentId)",
                        new
                        {
                            CommentId = response.Id,
                            ReplyToCommentId = replyToCommenId
                        });
                    return response;
                }

                return new Comment();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Comment();
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
                    DELETE FROM [dbo].[Comments] 
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
        public async Task<bool> UpdateAsync(Comment model, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return await Task.FromResult(false);
                }

                var response = await db.QueryAsync<bool>(@"
                    UPDATE [dbo].[Comments] 
                    SET Text = @Text,
                    State = @State, 
                    Data = @Data
                    WHERE Id = @Id",
                    new
                    {
                        Text = model.Text,
                        State = model.State,
                        Date = model.Date,
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
        public async Task<Comment> GetElementByIdAsync(int id, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new Comment();
                }

                //возможна ошибка инициализации user и article
                var response = await db.QueryAsync<Comment, User, Article, Comment>(@"
                    SELECT Comments.*,Users.*,Articles.* 
                    FROM [dbo].[Comments] 
                    INNER JOIN [dbo].[Users] ON Users.Id = Comments.UserId 
                    INNER JOIN [dbo].[Articles] ON Comments.ArticleId = Articles.Id 
                    WHERE Comments.Id = Id", 
                (comment, user, article) =>
                {
                    comment.UploadedUser = user;
                    comment.UploadedArticle = article;
                    return comment;
                }, new { Id = id });

                if (response != null)
                {
                    return response.FirstOrDefault();
                }
                return new Comment();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new Comment();
            }
        }
        public async Task<IEnumerable<Comment>> GetAllCollectionAsync(CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Comment>();
                }
                //возможна ошибка инициализации user и article
                var response = await db.QueryAsync<Comment, User, Article, Comment>(@"
                    SELECT Comments.*,Users.*,Articles.* 
                    FROM [dbo].[Comments] 
                    INNER JOIN [dbo].[Users] ON Users.Id = Comments.UserId 
                    INNER JOIN [dbo].[Articles] ON Comments.ArticleId = Articles.Id", 
                (comment, user, article) =>
                {
                    comment.UploadedUser = user;
                    comment.UploadedArticle = article;
                    return comment;
                });

                if (response != null)
                {
                    return response;
                }
                return new List<Comment>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Comment>();
            }
        }
        public async Task<IEnumerable<Comment>> GetAreaCollectionAsync(int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Comment>();
                }
                //возможна ошибка инициализации user и article
                var response = await db.QueryAsync<Comment, User, Article, Comment>(@"
                    SELECT Comments.*,Users.*,Articles.* 
                    FROM [dbo].[Comments] 
                    INNER JOIN [dbo].[Users] ON Users.Id = Comments.UserId 
                    INNER JOIN [dbo].[Articles] ON Comments.ArticleId = Articles.Id
                    ORDER BY Comments.Id
                    OFFSET @From ROWS FETCH NEXT @To ROWS ONLY",
                (comment, user, article) =>
                {
                    comment.UploadedUser = user;
                    comment.UploadedArticle = article;
                    return comment;
                }, new
                {
                    From = from,
                    To = to
                });

                if (response != null)
                {
                    return response;
                }
                return new List<Comment>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Comment>();
            }
        }
        public async Task<IEnumerable<Comment>> GetAllCollectionInArticleAsync(int articleId, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Comment>();
                }
                //возможна ошибка инициализации user и article
                var response = await db.QueryAsync<Comment, Article, Comment>(@"
                        SELECT Comments.*, Articles.*
                        FROM [dbo].[Comments]
						INNER JOIN [dbo].[Articles] ON Comments.ArticleId = Articles.Id
						WHERE Comments.ArticleId = @ArticleId AND Comments.Id NOT IN (SELECT Comments.Id
                        FROM [dbo].[Comments]
                        INNER JOIN [dbo].[ReplyToComments] ON Comments.Id = ReplyToComments.CommentId)",
                        /**/
                        (comment, article) =>
                        {
                            comment.UploadedArticle = article;
                            return comment;
                        },
                new
                {
                    ArticleId = articleId
                });
                if(response != null)
                {
                    return await InitCommentsAsync(response, token);
                }
                return new List<Comment>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Comment>();
            }
        }
        public async Task<int> GetArticlesCommentsCountAsync(int articleId, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return 0;
                }
                //возможна ошибка инициализации user и article
                var count = await db.QueryFirstOrDefaultAsync<int>(@"
                        SELECT COUNT(*)
                        FROM (SELECT * FROM Comments
					    WHERE Comments.ArticleId = @ArticleId) as [Count]",
                new
                {
                    ArticleId = articleId
                });

                return count;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<Comment>> GetAreaCollectionInArticleAsync(int articleId, int from, int to, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Comment>();
                }
                //возможна ошибка инициализации user и article
                var response = await db.QueryAsync<Comment, Article, Comment>(@"
                        SELECT Comments.*, Articles.*
                        FROM [dbo].[Comments]
                        INNER JOIN [dbo].[Articles] ON Comments.ArticleId = Articles.Id
                        WHERE Comments.ArticleId = @ArticleId AND Comments.Id NOT IN (SELECT Comments.Id
                        FROM [dbo].[Comments]
                        INNER JOIN [dbo].[ReplyToComments] ON Comments.Id = ReplyToComments.CommentId)
                        ORDER BY Comments.Id
                        OFFSET @From ROWS FETCH NEXT @To ROWS ONLY", 
                (comment, article) =>
                {
                    comment.UploadedArticle = article;
                    return comment;
                }, new
                {
                    ArticleId = articleId,
                    From = from,
                    To = to
                });
                if(response != null)
                {
                    return await InitCommentsAsync(response, token);
                }
                return new List<Comment>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Comment>();
            }
        }
        private async Task<IEnumerable<Comment>> InitCommentsAsync(IEnumerable<Comment> comments, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                    return new List<Comment>();
                }

                var response = comments.ToList();
                foreach (var item in response)
                {
                    var answers = await db.QueryAsync<Comment>(@"
                            SELECT Comments.*
                            FROM [dbo].[Comments]
                            WHERE Comments.ArticleId = @ArticleId AND Id IN (SELECT ReplyToComments.CommentId
                            FROM [dbo].[ReplyToComments]
                            WHERE ReplyToComments.ReplyToCommentId = @ReplyToCommentsId)",
                        new
                        {
                            ArticleId = item.UploadedArticle.Id,
                            ReplyToCommentsId = item.Id
                        });
                    foreach (var reply in answers.ToList())
                    {
                        var replyUser = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            INNER JOIN [dbo].[Comments] ON Comments.UserId = Users.Id
                            WHERE Comments.Id = @Id",
                        new
                        {
                            Id = reply.Id
                        });
                        reply.UploadedUser = replyUser;
                    }

                    var user = await db.QueryFirstOrDefaultAsync<User>(@"
                            SELECT Users.*
                            FROM [dbo].[Users]
                            INNER JOIN [dbo].[Comments] ON Comments.UserId = Users.Id
                            WHERE Comments.Id = @Id",
                        new
                        {
                            Id = item.Id
                        });
                    var article = await db.QueryFirstOrDefaultAsync<Article>(@"
                            SELECT *
                            FROM [dbo].[Articles]
                            WHERE Id = @Id",
                        new
                        {
                            Id = item.UploadedArticle.Id
                        });
                    item.UploadedUser = user;
                    item.UploadedArticle = article;
                    item.ReplyToComment = answers.ToList();
                }

                if (response != null)
                {
                    return response;
                }
                return new List<Comment>();
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new List<Comment>();
            }
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}
