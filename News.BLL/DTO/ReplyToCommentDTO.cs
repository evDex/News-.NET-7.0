namespace News.BLL.DTO
{
    public class ReplyToCommentDTO
    {
        public int Id { get; set; }
        public CommentDTO Comment { get; set; }
        public CommentDTO ReplyComment { get; set; }
    }
}
