using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace News.Models
{
    public class CreateArticleViewModel
    {
        public string Title { get; set; }
        public string? HashTagsString { get; set; }
        public string Text { get; set; }
        public List<IFormFile> Files { get; set; }
        public List<string> FilesRank { get; set; }
    }
}
