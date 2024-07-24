namespace News.Models
{
    public class CreateReplyComment
    {
        public int ArticleId { get; set; }
        public int CommentId { get; set; }
        public string? Text { get; set; }
    }
}
