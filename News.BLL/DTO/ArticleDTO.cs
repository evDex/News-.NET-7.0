using System;
using System.Collections.Generic;

namespace News.BLL.DTO
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int NumberOfViews { get; set; }
        public bool State { get; set; }
        public DateTimeOffset Date { get; set; }
        public UserDTO UploadedUser { get; set; }
        public List<HashTagDTO> HashTags{ get; set; }
        public List<FileDTO> Files { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }
}
