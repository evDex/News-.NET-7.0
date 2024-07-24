using System;

namespace News.BLL.DTO
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool State { get; set; }
        public DateTimeOffset Date { get; set; }
        public UserDTO UserUploaded { get; set; }
        public ArticleDTO InArticle { get; set; }
        public List<CommentDTO> ReaplyToComment { get; set; }
    }
}
