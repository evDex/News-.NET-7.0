using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace News.DAL.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool State { get; set; }
        public DateTimeOffset Date { get; set; }
        public User UploadedUser { get; set; }
        public Article UploadedArticle { get; set; }
        public ICollection<Comment> ReplyToComment { get; set; } = new List<Comment>();
    }
}
