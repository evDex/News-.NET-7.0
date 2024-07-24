using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public enum Allowed
    {
        [Display(Name = "Одобренно")]
        Approved = 1,
        [Display(Name = "Не одобренно")]
        NotApproved = 0
    }
}
