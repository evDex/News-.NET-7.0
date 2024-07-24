using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace News.DAL.Entities
{
    public class HashTag
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
