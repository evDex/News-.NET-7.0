using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace News.DAL.Entities
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int NumberOfViews { get; set; }
        public bool State { get; set; }
        public DateTimeOffset Date { get; set; }
        public User UploadedUser { get; set; }
        public ICollection<HashTag> HashTags { get; set; } = new HashSet<HashTag>();
        public ICollection<FileData> Files { get; set; } = new HashSet<FileData>();
        public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
