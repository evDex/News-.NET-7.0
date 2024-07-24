using System.ComponentModel.DataAnnotations;

namespace News.DAL.Entities
{
    public class FileData
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Rank { get; set; }
        public Article UploadedArticle { get; set; }
    }
}
