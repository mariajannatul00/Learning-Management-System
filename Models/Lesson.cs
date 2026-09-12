using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Web.Models
{
    public class Lesson : BaseEntity
    {
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public Module? Module { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public int DurationMinutes { get; set; } = 10;

        [MaxLength(500)]
        public string VideoUrl { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string ContentSummary { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 1;

        public bool IsFreePreview { get; set; } = false;
    }
}
