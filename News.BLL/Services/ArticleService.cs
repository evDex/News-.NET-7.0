using Azure;
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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.BLL.Services
{
    public class ArticleService : IArticleService
    {
        IUnitOfWork Database { get; set; }
        ExecuteTransaction Transaction;
        public ArticleService(IUnitOfWork uow)
        {
            Database = uow;
            Transaction = new ExecuteTransaction(uow);
        }
        public async Task<IBaseResponse<bool>> CreateArticleAsync(ArticleDTO model,CancellationToken token)
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
                    var user = await Database.Users.GetElementByIdAsync(model.UploadedUser.Id, token);
                    var newarticle = new Article
                    {
                        Name = model.Name,
                        Text = model.Text,
                        UploadedUser = user,
                        HashTags = model.HashTags.Select(a => new HashTag { Name = a.Name }).ToList(),
                        Files = model.Files.Select(a => new FileData { Name = a.Name, Path = a.Path,Rank = a.Rank }).ToList()
                    };

                    await Database.Articles.CreateAsync(newarticle, token);

                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[CreateArticle] : {ex.Message}";
                    return response;
                }
            },model,baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> UpdateArticleAsync(ArticleDTO model, CancellationToken token)
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

                    var newArticle = new Article()
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Text = model.Text,
                        NumberOfViews = model.NumberOfViews,
                        State = model.State,
                        HashTags = model.HashTags.Select(a =>
                        new HashTag
                        {
                            Name = a.Name
                        }).ToList(),
                        Date = model.Date,
                        Files = model.Files.Select(a => new FileData { Name = a.Name, Path = a.Path, Rank = a.Rank }).ToList()
                    };

                    await Database.Articles.UpdateAsync(newArticle, token);
                    response.StatusCode = StatusCode.OK;
                    response.Description = "Статья изменена";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[UpdateArticle] : {ex.Message}";
                    return response;
                }
            },model,baseResponse);
                
            return baseResponse;
        }
        public async Task<IBaseResponse<bool>> DeleteArticleByIdAsync(int id, CancellationToken token)
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

                    await Database.Articles.DeleteAsync(id, token);

                    response.StatusCode = StatusCode.OK;
                    response.Description = "Статья удалена";
                    return response;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[DeleteByIdArticle] : {ex.Message}";
                    return response;
                }
            },id,baseResponse);
                
            return baseResponse;
        }
        public async Task<IBaseResponse<ArticleDTO>> GetArticleByIdAsync(int articleID, CancellationToken token)
        {
            var baseResponse = new BaseResponse<ArticleDTO>() 
            { 
                Data = new ArticleDTO()
            };

            await Transaction.ExecuteTransationAsync(async (id, response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var article = await Database.Articles.GetElementByIdAsync(articleID, token);

                    if (article != null)
                    {
                        response.Data = InitArticleDTO(article);
                        response.StatusCode = StatusCode.OK;
                    }

                    response.Description = "Статья не найдена";
                    return baseResponse;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetByIdArticle] : {ex.Message}";
                    return response;
                }
            },articleID,baseResponse);
                
            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetArticlesAsync(CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetAllCollectionAsync(token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesAsync] : {ex.Message}";
                    return response;
                }
                return response;
            },baseResponse);
                
            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetArticlesAreaAsync(int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetAreaCollectionAsync(from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;

                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        
        public async Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetSearchCollectionArea(search, from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchArticlesAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesByUserNameAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetSearchCollectionArticlesByUserNameAreaAsync(search, from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchArticlesByUserNameAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesByHashTagAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetSearchCollectionArticlesByHashTagsAreaAsync(search, from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSearchArticlesByHashTagAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetSortedArticlesAreaAsync(ArticleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetSortedCollectionArea(columnName, direction, from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetSortedArticlesAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<List<ArticleDTO>>> GetUserArticlesAreaAsync(int userID, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<List<ArticleDTO>>()
            {
                Data = new List<ArticleDTO>()
            };

            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }

                    var articles = await Database.Articles.GetAreaCollectionInUserAsync(userID, from, to, token);
                    foreach (var item in articles)
                    {
                        response.Data.Add(InitArticleDTO(item));
                    }

                    response.StatusCode = StatusCode.OK;

                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetUserArticlesAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetArticlesCountAreaAsync(int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var count = await Database.Articles.GetCountAreaAsync(from, to, token);
                    baseResponse.Data = count;

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesCountAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetArticlesSearchCountAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var count = await Database.Articles.GetArticlesSearchCountAreaAsync(search, from, to, token);
                    baseResponse.Data = count;

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesCountAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetArticlesSearchByUserNameCountAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var count = await Database.Articles.GetArticlesSearchByUserNameCountAreaAsync(search, from, to, token);
                    baseResponse.Data = count;

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesSearchByUserNameCountAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        public async Task<IBaseResponse<int>> GetArticlesSearchByHashTagCountAreaAsync(string search, int from, int to, CancellationToken token)
        {
            var baseResponse = new BaseResponse<int>()
            {
                Data = 0
            };
            await Transaction.ExecuteTransationAsync(async (response) => {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        StaticLogger.LogDebug(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | Task stoped");
                        return response;
                    }
                    var count = await Database.Articles.GetArticlesSearchByHashTagCountAreaAsync(search, from, to, token);
                    baseResponse.Data = count;

                    response.StatusCode = StatusCode.OK;
                }
                catch (Exception ex)
                {
                    StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                    response.Description = $"[GetArticlesSearchByHashTagCountAreaAsync] : {ex.Message}";
                    return response;
                }
                return response;
            }, baseResponse);

            return baseResponse;
        }
        private ArticleDTO InitArticleDTO(Article article)
        {
            if(article != null)
            {
                var responce = new ArticleDTO
                {
                    Id = article.Id,
                    Name = article.Name,
                    Text = article.Text,
                    NumberOfViews = article.NumberOfViews,
                    State = article.State,
                    Date = article.Date,
                    UploadedUser = new UserDTO
                    {
                        Id = article.UploadedUser.Id,
                        UserName = article.UploadedUser.UserName,
                        AvatarPath = article.UploadedUser.AvatarPath,
                    },
                    HashTags = article.HashTags.Select(a =>
                        new HashTagDTO
                        {
                            Id = a.Id,
                            Name = a.Name
                        }).ToList(),
                    Files = article.Files.Select(a =>
                        new FileDTO
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Path = a.Path,
                            Rank = a.Rank
                        }).ToList(),

                };
                return responce;
            }
            return new ArticleDTO();
        }
        //private ArticleDTO GetArticleDTO(Article article)
        //{
        //    var responce = InitArticleDTO(article);
        //    responce.Comments = GetCommenstDTO(article.Comments.ToList());
        //    return responce;
        //}
        //private async Task<ArticleDTO> GetArticleDTOAsync(int from, int to, Article article)
        //{
        //    var responce = InitArticleDTO(article);
        //    var commetsArtilce = await Database.Comments.GetAreaCollectionInArticleAsync(article.Id, from, to, CancelTask.GetToken());
        //    responce.Comments = GetCommenstDTO(commetsArtilce.ToList());
        //    return responce;
        //}
        //private List<CommentDTO> GetCommenstDTO(List<Comment> articleComments)
        //{
        //    var result = new List<CommentDTO>();
        //    result = articleComments.Select(a => new CommentDTO
        //    {
        //        Id = a.Id,
        //        Text = a.Text,
        //        Date = a.Date,
        //        State = a.State,
        //        UserUploaded = new UserDTO
        //        {
        //            Id = a.UploadedUser.Id,
        //            UserName = a.UploadedUser.UserName,
        //            AvatarPath = a.UploadedUser.AvatarPath,
        //        }
        //    }).ToList();

        //    foreach (var comment in result)
        //    {
        //        List<CommentDTO> replyToComment = new List<CommentDTO>();
        //        foreach (var replyComments in articleComments.Where(i => i.Id == comment.Id).Select(a => a.ReplyToComment))
        //        {
        //            foreach (var replyComment in replyComments)
        //            {
        //                replyToComment.Add(new CommentDTO
        //                {
        //                    Id = replyComment.Id,
        //                    Text = replyComment.Text,
        //                    Date = replyComment.Date,
        //                    UserUploaded = new UserDTO
        //                    {
        //                        Id = replyComment.UploadedUser.Id,
        //                        UserName = replyComment.UploadedUser.UserName,
        //                        AvatarPath = replyComment.UploadedUser.AvatarPath,
        //                    },
        //                });
        //            }
        //        }
        //        comment.ReaplyToComment = replyToComment;
        //    }
        //    return result;
        //}
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
