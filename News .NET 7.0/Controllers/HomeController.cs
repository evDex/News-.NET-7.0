using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using News.BLL.DTO;
using News.BLL.Interfaces;
using News.Infrastructure.Common;
using News.Models;
using News.Infrastructure;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace News.Controllers
{
    public class HomeController : Controller
    {
        IArticleService articleService;
        ICommentService commentService;
        public HomeController(IArticleService ArServ, ICommentService commentService)
        {
            articleService = ArServ;
            this.commentService = commentService;
        }
        [HttpGet]
        [Route("")]
        [Route("/page/{number}")]
        public async Task<IActionResult> Index(int number = 1)
        {
            CancellationToken token = CancelTask.GetToken();
            
            return View(await InitArticles(number, token));
        }

        public IActionResult PageSwitch(int countOfObject, int countOfObjectViews)
        {
            return PartialView("_PageSwitch", new PageSwitch { CountOfObject = countOfObject, CountOfObjectViews = countOfObjectViews });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<ArticlesViewModel> InitArticles(int numberOfPage, CancellationToken token)
        {
            var response = await articleService.GetArticlesAreaAsync((numberOfPage - 1) * Config.NumberOfArticleViews, Config.NumberOfArticleViews, token);

            ArticlesViewModel result = new ArticlesViewModel();
            List<ArticlePreviewViewModel> articles = new List<ArticlePreviewViewModel>();

            var articleCount = await articleService.GetArticlesAreaAsync(0, Config.NumberOfArticleViews * Config.NumberOfPageViews, token);

            foreach (var item in response.Data)
            {
                var countAllComments = await commentService.GetArticlesCommentsCountAsync(item.Id, token);
                articles.Add(new ArticlePreviewViewModel
                {
                    Article = item,
                    TextTag = ArticleTagText.GetArticleTagText(item),
                    CommentsCount = countAllComments.Data
                });
            }
            result.Articles = articles;
            result.ArticleCount = articleCount.Data.Count();

            return result;
        }
    }
}
