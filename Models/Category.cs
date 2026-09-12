using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.Web.Models
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string IconClass { get; set; } = "fa-solid fa-folder";

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
