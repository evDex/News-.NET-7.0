using News.BLL.DTO;
using News.BLL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace News.BLL.Interfaces
{
    public interface ICommentService : IDisposable
    {
        Task<IBaseResponse<bool>> CreateCommentAsync(CommentDTO model, CancellationToken token);
        Task<IBaseResponse<bool>> DeleteCommentByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<CommentDTO>> GetCommentByIdAsync(int id, CancellationToken token);

        Task<IBaseResponse<bool>> CreateReplyCommentAsync(CommentDTO model, int commentId, CancellationToken token);

        Task<IBaseResponse<List<CommentDTO>>> GetArticlesCommentsAsync(int articleId, CancellationToken token);
        Task<IBaseResponse<List<CommentDTO>>> GetArticlesAreaCommentsAsync(int articleId, int from, int to, CancellationToken token);

        Task<IBaseResponse<int>> GetArticlesCommentsCountAsync(int articleId, CancellationToken token);
    }
}
