using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using News.BLL.DTO;
using News.BLL.Interfaces;
using News.Infrastructure.Common;
using News.Models;
using News.Infrastructure;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace News.Controllers
{
    [Route("User")]
    [Authorize]
    public class ProfileController : Controller
    {
        IUserService userService;
        IArticleService articleService;
        ICommentService commentService;

        IWebHostEnvironment _appEnvironment;
        public ProfileController(IArticleService ArServ, IUserService serv, IWebHostEnvironment appEnvironment, ICommentService coServ)
        {
            commentService = coServ;
            articleService = ArServ;
            this.userService = serv;
            this._appEnvironment = appEnvironment;
        }

        #region UserProfile
        [HttpGet]
        [Route("")]
        [Route("articles/page/{number}")]
        public async Task<IActionResult> Index(int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            var user = await userService.GetUserByNameAsync(User.Identity.Name, CancelTask.GetToken());
            var responce = await LinkSetForView(user.Data, (number - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            return View(responce);
        }
        #endregion

        #region OtherUserProfile
        [HttpGet]
        [Route("userid/{id}")]
        [Route("userid/{id}/articles/page/{number}")]
        public async Task<IActionResult> ViewProfile(int id, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            var user = await userService.GetUserByIdAsync(id, CancelTask.GetToken());
            var responce = await LinkSetForView(user.Data, (number - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            return View(responce);
        }
        #endregion

        #region EditUserProfile
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> ChangeProfile()
        {
            var user = await userService.GetUserByNameAsync(User.Identity.Name, CancelTask.GetToken());
            return View(new ChangeProfileViewModel()
            {
                Email = user.Data.Email,
                OldAvatarPath = user.Data.AvatarPath
            });
        }
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> ChangeProfile(ChangeProfileViewModel model)
        {
            CancellationToken token = CancelTask.GetToken();

            var user = await userService.GetUserByNameAsync(User.Identity.Name, CancelTask.GetToken());
            if (!string.IsNullOrEmpty(user.Data.Email))
            {
                user.Data.Email = model.Email;
            }
            if (model.NewAvatarPath != null)
            {
                string path = "/resources/UserAvatars/" + HashHelper.Hashing(model.NewAvatarPath.FileName);
                switch (model.NewAvatarPath.ContentType)
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
                    await model.NewAvatarPath.CopyToAsync(fileStream);
                }
                user.Data.AvatarPath = path;
            }

            if (model.NewPassword != null)
            {
                var checkPassword = await userService.CheckUserPasswordAsync(user.Data, model.OldPassword, token);
                if (checkPassword.Data)
                {
                    user.Data.Password = model.NewPassword;
                }
            }
            await userService.UpdateUserAsync(user.Data, token);

            return RedirectToAction("Edit", "User");
        }
        #endregion

        private async Task<ProfileViewModel> LinkSetForView(UserDTO user, int from, int to, CancellationToken token)
        {
            var response = await articleService.GetUserArticlesAreaAsync(user.Id, from, to, CancelTask.GetToken());
            List<ArticlePreviewViewModel> articles = new List<ArticlePreviewViewModel>();

            foreach (var item in response.Data)
            {
                var commets = await commentService.GetArticlesCommentsAsync(item.Id, token);
                item.Comments = commets.Data;

                var allComments = await commentService.GetArticlesCommentsAsync(item.Id, token);
                var countAllComments = await commentService.GetArticlesCommentsCountAsync(item.Id, token);

                articles.Add(new ArticlePreviewViewModel
                {
                    Article = item,
                    TextTag = ArticleTagText.GetArticleTagText(item),
                    CommentsCount = countAllComments.Data
                });
            }
            var ArticleCount = await articleService.GetArticlesAreaAsync(0, Config.NumberOfArticleViews * Config.NumberOfPageViews, CancelTask.GetToken());

            ArticlesViewModel responce = new ArticlesViewModel()
            {
                Articles = articles,
                ArticleCount = ArticleCount.Data.Count()
            };

            return new ProfileViewModel()
            {
                Articles = responce,
                User = user
            };
        }
    }
}
