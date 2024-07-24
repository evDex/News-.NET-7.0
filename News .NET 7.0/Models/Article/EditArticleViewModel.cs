using News.BLL.DTO;


namespace News.Models
{
    public class EditArticleViewModel
    {
        public ArticleDTO Article { get; set; }
        public string ArticleId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string? HashTagsString { get; set; }
        public List<IFormFile> UploadFiles { get; set; }
        public List<string> FilesRank { get; set; }
        public bool State { get; set; } = false;
        public List<TagTextView> TextTag { get; set; }
    }
    //public class EditArticleViewModel
    //{
    //    public string ArticleId { get; set; }
    //    public string Title { get; set; }
    //    public string? HashTagsString { get; set; }
    //    public List<IFormFile> UploadFiles { get; set; }
    //    public List<FileDTO>? Files { get; set; }
    //    public List<string> FilesRank { get; set; }
    //    public UserDTO UploadedUser { get; set; }
    //    public bool State { get; set; } = false;
    //    public List<TagTextView> TextTag { get; set; }

    //}
}
