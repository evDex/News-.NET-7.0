using System.Collections.Generic;

namespace News.BLL.DTO
{
    public class HashTagDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ArticleDTO> Articles { get; set; }
    }
}
