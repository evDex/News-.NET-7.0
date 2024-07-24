using News.BLL.DTO;
using News.BLL.Infrastructure;
using News.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace News.BLL.Interfaces
{
    public interface IArticleService : IDisposable
    {
        Task<IBaseResponse<bool>> CreateArticleAsync(ArticleDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> UpdateArticleAsync(ArticleDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> DeleteArticleByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<ArticleDTO>> GetArticleByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<List<ArticleDTO>>> GetArticlesAsync(CancellationToken token);
        Task<IBaseResponse<List<ArticleDTO>>> GetArticlesAreaAsync(int from,int to, CancellationToken token);
        Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesAreaAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesByUserNameAreaAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<List<ArticleDTO>>> GetSearchArticlesByHashTagAreaAsync(string search, int from, int to, CancellationToken token);

        Task<IBaseResponse<List<ArticleDTO>>> GetSortedArticlesAreaAsync(ArticleColumnName columnName, SortDirection direction, int from, int to, CancellationToken token);
        Task<IBaseResponse<List<ArticleDTO>>> GetUserArticlesAreaAsync(int UserID, int from, int to, CancellationToken token);

        Task<IBaseResponse<int>> GetArticlesCountAreaAsync(int from, int to, CancellationToken token);
        Task<IBaseResponse<int>> GetArticlesSearchCountAreaAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<int>> GetArticlesSearchByUserNameCountAreaAsync(string search, int from, int to, CancellationToken token);
        Task<IBaseResponse<int>> GetArticlesSearchByHashTagCountAreaAsync(string search, int from, int to, CancellationToken token);
    }
}
