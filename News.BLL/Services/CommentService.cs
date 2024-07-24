using AutoMapper;
using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.BLL.Interfaces;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace News.BLL.Services
{
    public class CommentService : ICommentService
    {
        IUnitOfWork Database { get; set; }
        ExecuteTransaction Transaction;
        public CommentService(IUnitOfWork uow)
        {
            Database = uow;
            Transaction = new ExecuteTransaction(uow);
        }
        public async Task<IBaseResponse<bool>> CreateCommentAsync(CommentDTO model, CancellationToken token)
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

                    var user = await Database.Users.GetElementByIdAsync(model.UserUploaded.Id, token);

                    var article = await Database.Articles.GetElementByIdAsync(model.InArticle.Id, token);

                    Comment comment = new Comment
                    {
                        Text = model.Text,
                        State = model.State,
                        Date = DateTimeOffset.UtcNow,
                        UploadedUser = user,
                        UploadedArticle = article
                    };

                    await Database.Comments.CreateAsync(comment, token);

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[CreateCommentAsync] : {ex.Message}";
                    return response;
                }
            },model,baseResponse);
            return baseResponse; 
        }
        public async Task<IBaseResponse<bool>> DeleteCommentByIdAsync(int id, CancellationToken token)
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

                    var comment = await Database.Comments.GetElementByIdAsync(id, token);
                    if (comment != null)
                    {
                        await Database.Comments.DeleteAsync(id, token);
                        response.Description = "Коментарий удален";
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }
                    response.Description = "Коментарий не найден";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[DeleteCommentByIdAsync] : {ex.Message}";
                    return response;
                }
            }, id, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<CommentDTO>> GetCommentByIdAsync(int id, CancellationToken token)
        {
            var baseResponse = new BaseResponse<CommentDTO>()
            {
                Data = new CommentDTO()
            };

            await Transaction.ExecuteTransationAsync(async (model, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var comment = await Database.Comments.GetElementByIdAsync(id, token);
                    if (comment != null)
                    {
                        response.StatusCode = StatusCode.OK;
                        return response;
                    }
                    response.Description = "Коментарий не найден";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[DeleteByIdComent] : {ex.Message}";
                    return response;
                }
            }, id, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> CreateReplyCommentAsync(CommentDTO model, int commentId, CancellationToken token)
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
                        return new BaseResponse<bool>();
                    }

                    var user = await Database.Users.GetElementByIdAsync(model.UserUploaded.Id, token);

                    var article = await Database.Articles.GetElementByIdAsync(model.InArticle.Id, token);

                    Comment comment = new Comment
                    {
                        Text = model.Text,
                        State = model.State,
                        Date = DateTimeOffset.UtcNow,
                        UploadedUser = user,
                        UploadedArticle = article
                    };

                    await Database.Comments.CreateReplyCommentAsync(comment, commentId, token);

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[CreateReplyCommentAsync] : {ex.Message}";
                    return response;
                }
            }, model, baseResponse);
            return baseResponse;
        }
        public async Task<IBaseResponse<List<CommentDTO>>> GetArticlesCommentsAsync(int articleId, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<CommentDTO>>() 
            { 
                Data = new List<CommentDTO>() 
            };

            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var commets = await Database.Comments.GetAllCollectionInArticleAsync(articleId, token);

                    if (commets != null)
                    {
                        response.Data = GetCommenstDTO(commets.ToList());
                        response.StatusCode = StatusCode.OK;
                        return baseResponse;
                    }

                    response.Description = "Коментариев нет";
                    return baseResponse;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesCommentsAsync] : {ex.Message}";
                    return response;
                }
            }, articleId, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<CommentDTO>>> GetArticlesAreaCommentsAsync(int articleId, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<CommentDTO>>()
            {
                Data = new List<CommentDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var commets = await Database.Comments.GetAreaCollectionInArticleAsync(articleId, from, to, token);

                    if (commets != null)
                    {
                        response.Data = GetCommenstDTO(commets.ToList());
                        response.StatusCode = StatusCode.OK;
                        return baseResponse;
                    }

                    response.Description = "Коментариев нет";
                    return baseResponse;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesAreaCommentsAsync] : {ex.Message}";
                    return response;
                }
            }, articleId, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetArticlesCommentsCountAsync(int articleId, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };

            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var commets = await Database.Comments.GetArticlesCommentsCountAsync(articleId, token);

                    if (commets != null)
                    {
                        response.Data = commets;
                        response.StatusCode = StatusCode.OK;
                        return baseResponse;
                    }

                    response.Description = "Коментариев нет";
                    return baseResponse;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesCommentsCountAsync] : {ex.Message}";
                    return response;
                }
            }, articleId, baseResponse);

            return baseResponse;
        }
        private List<CommentDTO> GetCommenstDTO(List<Comment> articleComments)
        {
            var result = new List<CommentDTO>();
            result = articleComments.Select(a => new CommentDTO
            {
                Id = a.Id,
                Text = a.Text,
                Date = a.Date,
                State = a.State,
                UserUploaded = new UserDTO
                {
                    Id = a.UploadedUser.Id,
                    UserName = a.UploadedUser.UserName,
                    AvatarPath = a.UploadedUser.AvatarPath,
                }
            }).ToList();

            foreach (var comment in result)
            {
                List<CommentDTO> replyToComment = new List<CommentDTO>();
                foreach (var replyComments in articleComments.Where(i => i.Id == comment.Id).Select(a => a.ReplyToComment))
                {
                    foreach (var replyComment in replyComments)
                    {
                        replyToComment.Add(new CommentDTO
                        {
                            Id = replyComment.Id,
                            Text = replyComment.Text,
                            Date = replyComment.Date,
                            UserUploaded = new UserDTO
                            {
                                Id = replyComment.UploadedUser.Id,
                                UserName = replyComment.UploadedUser.UserName,
                                AvatarPath = replyComment.UploadedUser.AvatarPath,
                            },
                        });
                    }
                }
                comment.ReaplyToComment = replyToComment;
            }
            return result;
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
