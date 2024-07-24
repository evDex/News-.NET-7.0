namespace News.BLL.DTO
{
    public class FileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Rank { get; set; }
        public ArticleDTO InArticle{ get; set; }
    }
}
