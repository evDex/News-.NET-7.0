using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.BLL.DTO;
using News.BLL.Interfaces;
using News.DAL.Entities;
using News.Infrastructure.Common;
using News.Models;
using News.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;
using News.BLL.Infrastructure;

namespace News.Controllers
{
    [Route("Article")]
    public class ArticleController : Controller
    {
        IArticleService articleService;
        ICommentService commentService;
        IUserService userService;

        IWebHostEnvironment _appEnvironment;
        public ArticleController(IArticleService articleService, ICommentService commentService, IUserService userService, IWebHostEnvironment appEnvironment)
        {
            this.articleService = articleService;
            this.commentService = commentService;
            this.userService = userService;
            this._appEnvironment = appEnvironment;
        }

        #region ArticleView
        [HttpGet]
        [Route("{id}")]
        [Route("{id}/comments/page/{number}")]
        public async Task<IActionResult> Index(int id = 1, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();

            var response = await articleService.GetArticleByIdAsync(id, token);
            var commets = await commentService.GetArticlesAreaCommentsAsync(id, (number - 1) * Config.NumberOfCommentsView, Config.NumberOfCommentsView, token);
            var allComments = await commentService.GetArticlesCommentsAsync(id, token);
            var countAllComments = await commentService.GetArticlesCommentsCountAsync(id, token);
            response.Data.Comments = commets.Data;

            response.Data.NumberOfViews++;
            await articleService.UpdateArticleAsync(response.Data, token);

            return View(new ArticleViewModel { Article = response.Data, TextTag = ArticleTagText.GetArticleTagText(response.Data), CommentsCount = countAllComments.Data, CommentsCountOfPage = allComments.Data.Count() });
        }
        [HttpPost]
        public async Task<IActionResult> CreateComment(ArticleViewModel model, string returnUrl)
        {
            CancellationToken token = CancelTask.GetToken();

            var user = await userService.GetUserByNameAsync(User.Identity.Name, token);
            CommentDTO newComment = new CommentDTO()
            {
                Text = model.AddComment.Text,
                Date = DateTimeOffset.UtcNow,
                UserUploaded = user.Data,
                InArticle = model.Article
            };

            var response = await commentService.CreateCommentAsync(newComment, token);
            ViewBag.returnUrl = returnUrl;
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> CreateReplyComment(ArticleViewModel model, string returnUrl)
        {
            CancellationToken token = CancelTask.GetToken();

            var user = await userService.GetUserByNameAsync(User.Identity.Name, token);
            var comment = await commentService.GetCommentByIdAsync(model.ReplyComment.Id, token);
            var article = await articleService.GetArticleByIdAsync(model.Article.Id, token);
            CommentDTO newComment = new CommentDTO()
            {
                Text = model.AddComment.Text,
                Date = DateTimeOffset.UtcNow,
                UserUploaded = user.Data,
                InArticle = article.Data
            };

            var response = await commentService.CreateReplyCommentAsync(newComment, model.ReplyComment.Id, token);

            if (Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
        #endregion

        #region SearchArticles
        [HttpGet]
        [Route("search/{search}")]
        [Route("search/{search}/page/{number}")]
        public async Task<IActionResult> SearchArticles(string search, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            switch (search[0])
            {
                default:
                    return View(await SearchArticleByName(search, number, token));
                case '@':
                    return View(await SearchArticleByUserName(search.Substring(1), number, token));
                case '!':
                    return View(await SearchArticleByHashTag(search.Substring(1), number, token));
            }
        }
        private async Task<ArticlesSearchViewModel> SearchArticleByName(string search, int number, CancellationToken token)
        {
            var response = await articleService.GetSearchArticlesAreaAsync(search, (number - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            var result = await InitPreviewArticles(search, response, token);

            var articleCount = await articleService.GetArticlesSearchCountAreaAsync(search, 0, Config.NumberOfArticleViews * Config.NumberOfPageViews, token);
            result.ArticleCount = articleCount.Data;
            result.SearchRequest = search;

            return result;
        }
        private async Task<ArticlesSearchViewModel> SearchArticleByUserName(string search, int number, CancellationToken token)
        {
            var response = await articleService.GetSearchArticlesByUserNameAreaAsync(search, (number - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            var result = await InitPreviewArticles(search, response, token);

            var articleCount = await articleService.GetArticlesSearchByUserNameCountAreaAsync(search, 0, Config.NumberOfArticleViews * Config.NumberOfPageViews, token);
            result.ArticleCount = articleCount.Data;
            result.SearchRequest = $"@{search}";

            return result;
        }
        private async Task<ArticlesSearchViewModel> SearchArticleByHashTag(string search, int number, CancellationToken token)
        {
            var response = await articleService.GetSearchArticlesByHashTagAreaAsync(search, (number - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            var result = await InitPreviewArticles(search, response, token);

            var articleCount = await articleService.GetArticlesSearchByHashTagCountAreaAsync(search, 0, Config.NumberOfArticleViews * Config.NumberOfPageViews, token);
            result.ArticleCount = articleCount.Data;
            result.SearchRequest = $"!{search}";

            return result;
        }
        private async Task<ArticlesSearchViewModel> InitPreviewArticles(string search, IBaseResponse<List<ArticleDTO>> response, CancellationToken token)
        {
            ArticlesSearchViewModel result = new ArticlesSearchViewModel();
            List<ArticlePreviewViewModel> articles = new List<ArticlePreviewViewModel>();
           
            foreach (var item in response.Data)
            {
                var comments = await commentService.GetArticlesCommentsAsync(item.Id, token);
                item.Comments = comments.Data;
                var countAllComments = await commentService.GetArticlesCommentsCountAsync(item.Id, token);
                articles.Add(new ArticlePreviewViewModel
                {
                    Article = item,
                    TextTag = ArticleTagText.GetArticleTagText(item),
                    CommentsCount = countAllComments.Data
                });
            }
            result.Articles = articles;
            return result;
        }
        #endregion

        #region CreateArticle
        [HttpGet]
        [Route("Add")]
        public IActionResult CreateArticle()
        {
            return View();
        }
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> CreateArticle(CreateArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                CancellationToken token = CancelTask.GetToken();

                var user = await userService.GetUserByNameAsync(User.Identity.Name, token);

                string[] hashTags = new string[0];
                if (!System.String.IsNullOrEmpty(model.HashTagsString))
                {
                    hashTags = model.HashTagsString.Split(new char[] { ',' });
                }

                List<FileDTO> newFiles = new List<FileDTO>();
                for (int i = 0; i < model.Files.Count; i++)
                {
                    string path = "/resources/ArticleFiles/" + HashHelper.Hashing(model.Files[i].FileName);
                    switch (model.Files[i].ContentType)
                    {
                        case "image/jpeg":
                            path += ".jpg";
                            break;
                        case "image/png":
                            path += ".png";
                            break;
                        case "image/gif":
                            path += ".gif";
                            break;
                    }

                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await model.Files[i].CopyToAsync(fileStream);
                    }
                    newFiles.Add(new FileDTO()
                    {
                        Name = model.Files[i].FileName,
                        Path = path,
                        Rank = model.FilesRank[i]
                    });
                }

                List<HashTagDTO> newHashTags = new List<HashTagDTO>();
                foreach (var item in hashTags)
                {
                    newHashTags.Add(new HashTagDTO()
                    {
                        Name = item
                    });
                }

                var newArticle = new ArticleDTO()
                {
                    Name = model.Title,
                    Text = model.Text,
                    Date = DateTimeOffset.UtcNow,
                    UploadedUser = new UserDTO()
                    {
                        Id = user.Data.Id,
                        UserName = user.Data.UserName
                    },
                    HashTags = newHashTags,
                    Files = newFiles
                };
                await articleService.CreateArticleAsync(newArticle, token);
            }
            return RedirectToAction("Index", "Profile");
        }
        #endregion

        #region EditArticle
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditArticle(int id)
        {
            CancellationToken token = CancelTask.GetToken();
            var responce = await articleService.GetArticleByIdAsync(id, token);
            var article = responce.Data;
            var user = await userService.GetUserByIdAsync(article.UploadedUser.Id, token);

            var text = article.Text.Split("[image]").ToList();
            return PartialView(new EditArticleViewModel()
            {
                Article = article,
                HashTagsString = System.String.Join(",", article.HashTags.Select(a => a.Name)),
                TextTag = ArticleTagText.GetArticleTagText(article)
            });
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> EditArticle(EditArticleViewModel model)
        {
            //string message = "";
            CancellationToken token = CancelTask.GetToken();
            var responce = await articleService.GetArticleByIdAsync(Int32.Parse(model.ArticleId), token);
            var article = responce.Data;

            if (!System.String.IsNullOrEmpty(model.HashTagsString))
            {
                var checkHashTagsString = model.HashTagsString ?? " ";
                var hashTags = checkHashTagsString.Split(new char[] { ',' });
                List<HashTagDTO> newHashTags = new List<HashTagDTO>();

                foreach (var item in hashTags)
                {
                    newHashTags.Add(new HashTagDTO()
                    {
                        Name = item,
                    });
                }
                article.HashTags = newHashTags;
            }
            if (model.UploadFiles != null)
            {
                foreach (var item in article.Files)
                {
                    DeleteFile.DeleteFileAt(item.Path);
                }
                List<FileDTO> newFiles = new List<FileDTO>();
                for (int i = 0; i < model.UploadFiles.Count; i++)
                {
                    string path = "/resources/ArticleFiles/" + HashHelper.Hashing(model.UploadFiles[i].FileName);
                    switch (model.UploadFiles[i].ContentType)
                    {
                        case "image/jpeg":
                            path += ".jpg";
                            break;
                        case "image/png":
                            path += ".png";
                            break;
                    }
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await model.UploadFiles[i].CopyToAsync(fileStream);
                    }
                    newFiles.Add(new FileDTO()
                    {
                        Name = model.UploadFiles[i].FileName,
                        Path = path,
                        Rank = model.FilesRank[i],
                    });
                }
                article.Files = newFiles;
            }

            if (!System.String.IsNullOrEmpty(model.Title))
            {
                article.Name = model.Title;
            }
            if (!System.String.IsNullOrEmpty(model.Text))
            {
                article.Text = model.Text;
            }
            article.State = model.State;

            await articleService.UpdateArticleAsync(article, token);

            //message = string.Format("<h3>Статья : {0} изменена </h3>>", model.Title);
            //return Json(new
            //{
            //    Status = "success",
            //    Message = message
            //});
            return RedirectToAction("Index", "Profile");
        }
        #endregion

        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            if (id > -1)
            {
                await articleService.DeleteArticleByIdAsync(id, CancelTask.GetToken());
            }

            return RedirectToAction("Index", "Profile");
        }
    }
}
