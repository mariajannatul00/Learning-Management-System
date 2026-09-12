using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Web.Models
{
    public class LessonProgress : BaseEntity
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson? Lesson { get; set; }

        public bool IsCompleted { get; set; } = true;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
