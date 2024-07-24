using System.ComponentModel.DataAnnotations;

namespace News.DAL.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
