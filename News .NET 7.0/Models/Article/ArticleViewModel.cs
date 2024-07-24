using News.BLL.DTO;
using News.Infrastructure;
using System.Collections.Generic;

namespace News.Models
{
    public class ArticleViewModel
    {
        public UserDTO User { get; set; }
        public ArticleDTO Article { get; set; }
        public List<TagTextView> TextTag { get; set; }
        public CommentDTO AddComment { get; set; }
        public ReplyToCommentDTO ReplyComment { get; set; }
        public int NumberOfCommentsView { get; set; } = Config.NumberOfCommentsView;
        public int CommentsCount { get; set; }
        public int CommentsCountOfPage { get; set; }
    }
    public class ArticlePreviewViewModel
    {
        public ArticleDTO Article { get; set; }
        public List<TagTextView> TextTag { get; set; }
        public int CommentsCount { get; set; }
    }
    public class ArticlesViewModel
    {
        public List<ArticlePreviewViewModel> Articles { get; set; }
        public int ArticleCount { get; set; }
        public int NumberOfArticleViews { get; set; } = Config.NumberOfArticleViews;
    }
    public class ArticlesSearchViewModel : ArticlesViewModel
    {
        public string SearchRequest { get; set; } = "";
    }
}
