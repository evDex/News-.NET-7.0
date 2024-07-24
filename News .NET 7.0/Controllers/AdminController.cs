using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.BLL.DTO;
using News.BLL.Interfaces;
using News.BLL.Services;
using News.DAL.Entities;
using News.Infrastructure;
using News.Infrastructure.Common;
using News.Infrastructure.Enums;
using News.Models;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace News.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin,ChiefAdmin")]
    public class AdminController : Controller
    {
        IArticleService articleService;
        IUserService userService;
        IRoleService roleService;
        ICommentService commentService;
        public AdminController(IArticleService articleService, IUserService userService, IRoleService roleService, ICommentService commentService)
        {
            this.articleService = articleService;
            this.userService = userService;
            this.roleService = roleService;
            this.commentService = commentService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region AdminPanelOfArticles
        [HttpGet]
        [Route("Articles/{column:maxlength(16)?}/{orderby:maxlength(10)?}")]
        [Route("Articles/{column:maxlength(16)?}/{orderby:maxlength(10)?}/page/{number}")]
        public async Task<IActionResult> Articles(string? column, string? orderby, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();

            var result = await InitAdminArticles(column, orderby, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);

            return View(result);
        }
        [HttpGet]
        [Route("Articles/search/{search}")]
        [Route("Articles/search/{search}/page/{number}")]
        public async Task<IActionResult> Articles(string search, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();

            var result = new ModelCollectionSorted<ArticlesViewModel>()
            {
                OptionOrderBy = "ascending"
            };
            var articlesPreview = new List<ArticlePreviewViewModel>();
            var articles = await articleService.GetSearchArticlesAreaAsync(search, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);

            
            result = await InitAnArticleComments(result, articles.Data, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);
            var articleCount = await articleService.GetArticlesSearchCountAreaAsync(search, (number - 1), Config.NumberOfTableRows * Config.NumberOfCommentsView, token);
            result.SortedCollection.ArticleCount = articleCount.Data;
            result.LinkRequest = $"/Admin/Articles/search/{search}/page/";

            return View(result);
        }
        #endregion

        #region AdminPanelOfUsers
        [HttpGet]
        [Route("Users/{column:maxlength(16)?}/{orderby:maxlength(10)?}")]
        [Route("Users/{column:maxlength(16)?}/{orderby:maxlength(10)?}/page/{number}")]
        public async Task<IActionResult> Users(string? column, string? orderby, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();

            var responceUsers = await userService.GetUsersAreaAsync((number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);

            return View(await InitUsers(column, orderby, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token));
        }
        [HttpGet]
        [Route("Users/search/{search}")]
        [Route("Users/search/{search}/page/{number}")]
        public async Task<IActionResult> Users(string search, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            var responceUsers = await userService.GetSearchUsersAreaAsync(search, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);

            var result = new ModelCollectionSorted<AdminUserRolesViewModel>()
            {
                OptionOrderBy = "ascending"
            };
            var usersCount = await userService.GetSearchCountUserAsync(search, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows * 7, token);
            var responceRoles = await roleService.GetRolesAsync(token);
            var model = new AdminUserRolesViewModel()
            {
                Users = responceUsers.Data,
                Roles = responceRoles.Data,
                UsersCount = usersCount.Data
            };

            result.SortedCollection = model;
            result.LinkRequest = $"/Admin/Users/search/{search}/page/";

            return View(result);
        }
        #endregion

        #region AdminPanelOfRoles
        [HttpGet]
        [Route("Roles/{column:maxlength(10)?}/{orderby:maxlength(10)?}")]
        [Route("Roles/{column:maxlength(10)?}/{orderby:maxlength(10)?}/page/{number}")]
        public async Task<IActionResult> Roles(string? column, string? orderby, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            var response = await roleService.GetRolesAsync(token);
            var result = new ModelCollectionSorted<AdminRolesViewModel>()
            {
                OptionOrderBy = orderby != null ? orderby : "ascending"
            };
            RoleColumnName columnName;
            SortDirection sortBy = orderby == "descending" ? SortDirection.Descending : SortDirection.Ascending;
            switch (column)
            {
                default:
                case "Id":
                    result.ColumnNameOrderBy = "Id";
                    columnName = RoleColumnName.Id;
                    break;
                case "Name":
                    result.ColumnNameOrderBy = "Name";
                    columnName = RoleColumnName.Name;
                    break;
            }
            var roles = await roleService.GetSortedRolesAreaAsync(columnName, sortBy, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);
            var rolesCount = await roleService.GetCountRolesAsync((number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows * Config.NumberOfPageViews, token);
            var model = new AdminRolesViewModel()
            {
                Roles = roles.Data,
                RolesCount = rolesCount.Data
            };
            result.SortedCollection = model;

            return View(result);
        }
        [HttpGet]
        [Route("Roles/search/{search}")]
        [Route("Roles/search/{search}/page/{number}")]
        public async Task<IActionResult> Roles(string search, int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            var responceRoles = await roleService.GetSearchRolesAreaAsync(search, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows, token);

            var result = new ModelCollectionSorted<AdminRolesViewModel>()
            {
                OptionOrderBy = "ascending"
            };
            var rolesCount = await roleService.GetSearchCountRolesAsync(search, (number - 1) * Config.NumberOfTableRows, Config.NumberOfTableRows * 7, token);
            var model = new AdminRolesViewModel()
            {
                Roles = responceRoles.Data,
                RolesCount = rolesCount.Data
            };

            result.SortedCollection = model;
            result.LinkRequest = $"/Admin/Roles/search/{search}/page/";

            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> EditUserRoles(EditUserRoles model)
        {
            CancellationToken token = CancelTask.GetToken();

            var response = await userService.GetUserByIdAsync(model.UserId ,token);
            var user = response.Data;
            var newRoles = new List<RoleDTO>();
            foreach (var role in model.RoleNames)
            {
                newRoles.Add(new RoleDTO()
                {
                    Name = role
                });
            }
            user.Roles = newRoles;
            await userService.UpdateUserAsync(user, token);
            return RedirectToAction("Index", "Home");
        }
        #endregion
        private async Task<ModelCollectionSorted<AdminUserRolesViewModel>> InitUsers(string? column, string? orderby, int from, int to, CancellationToken token)
        {
            var responceRoles = await roleService.GetRolesAsync(token);
            var result = new ModelCollectionSorted<AdminUserRolesViewModel>()
            {
                OptionOrderBy = orderby != null ? orderby : "ascending"
            };
            UserColumnName columnName;
            SortDirection sortBy = orderby == "descending" ? SortDirection.Descending : SortDirection.Ascending;
            switch (column)
            {
                default:
                case "Id":
                    result.ColumnNameOrderBy = "Id";
                    columnName = UserColumnName.Id;
                    break;
                case "UserName":
                    result.ColumnNameOrderBy = "UserName";
                    columnName = UserColumnName.UserName;
                    break;
                case "Email":
                    result.ColumnNameOrderBy = "Email";
                    columnName = UserColumnName.Email;
                    break;
                case "Articles":
                    result.ColumnNameOrderBy = "Articles";
                    columnName = UserColumnName.Articles;
                    break;
            }
            var users = await userService.GetSortedUsersAreaAsync(columnName, sortBy, from, to, token);
            var usersCount = await userService.GetUsersCountAreaAsync(from, Config.NumberOfTableRows * Config.NumberOfCommentsView, token);
            var model = new AdminUserRolesViewModel()
            {
                Users = users.Data,
                Roles = responceRoles.Data,
                UsersCount = usersCount.Data
            };

            result.SortedCollection = model;
            result.LinkRequest = $"/Admin/Users/{result.ColumnNameOrderBy}/{result.OptionOrderBy}/page/";

            return result;
        }
        private async Task<ModelCollectionSorted<ArticlesViewModel>> InitAdminArticles(string? column, string? orderby, int from, int to, CancellationToken token)
        {
            SortDirection sortBy = orderby == "descending" ? SortDirection.Descending : SortDirection.Ascending;
            ArticleColumnName columnName;
            var result = new ModelCollectionSorted<ArticlesViewModel>()
            {
                OptionOrderBy = orderby != null ? orderby : "ascending"
            };
            switch (column)
            {
                default:
                case "Id":
                    columnName = ArticleColumnName.Id;
                    result.ColumnNameOrderBy = "Id";
                    break;
                case "Name":
                    columnName = ArticleColumnName.Name;
                    result.ColumnNameOrderBy = "Name";
                    break;
                case "UserName":
                    columnName = ArticleColumnName.UserName;
                    result.ColumnNameOrderBy = "UserName";
                    break;
                case "NumberOfViews":
                    columnName = ArticleColumnName.NumberOfViews;
                    result.ColumnNameOrderBy = "NumberOfViews";
                    break;
                case "Comments":
                    columnName = ArticleColumnName.NumberOfComments;
                    result.ColumnNameOrderBy = "Comments";
                    break;
                case "Date":
                    columnName = ArticleColumnName.Date;
                    result.ColumnNameOrderBy = "Date";
                    break;
                case "State":
                    columnName = ArticleColumnName.State;
                    result.ColumnNameOrderBy = "State";
                    break;
            }

            var articles = await articleService.GetSortedArticlesAreaAsync(columnName, sortBy, from, to, token);

            result = await InitAnArticleComments(result, articles.Data, from, to, token);
            result.LinkRequest = $"/Admin/Articles/{result.ColumnNameOrderBy}/{result.OptionOrderBy}/page/";
            return result;
        }
        private async Task<ModelCollectionSorted<ArticlesViewModel>> InitAnArticleComments(ModelCollectionSorted<ArticlesViewModel> model, List<ArticleDTO> articles, int from, int to, CancellationToken token)
        {
            var articlesPreview = new List<ArticlePreviewViewModel>();
            foreach (var item in articles)
            {
                var comments = await commentService.GetArticlesCommentsAsync(item.Id, token);
                item.Comments = comments.Data;
                var countAllComments = await commentService.GetArticlesCommentsCountAsync(item.Id, token);
                articlesPreview.Add(new ArticlePreviewViewModel
                {
                    Article = item,
                    CommentsCount = countAllComments.Data
                });
            }
            var articleCount = await articleService.GetArticlesCountAreaAsync(0, Config.NumberOfTableRows * Config.NumberOfCommentsView, token);
            model.SortedCollection = new ArticlesViewModel
            {
                Articles = articlesPreview,
                ArticleCount = articleCount.Data,
                NumberOfArticleViews = Config.NumberOfArticleViews
            };

            return model;
        }
    }
}