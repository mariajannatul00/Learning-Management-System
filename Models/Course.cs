using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Web.Models
{
    public class Course : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced

        public double DurationHours { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsEnded => EndDate.HasValue && DateTime.UtcNow > EndDate.Value;

        public bool IsStarted => StartDate.HasValue && DateTime.UtcNow >= StartDate.Value;

        public int TotalLectures { get; set; } = 0;


        [MaxLength(500)]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string IntroVideoUrl { get; set; } = string.Empty;

        public double Rating { get; set; } = 5.0;

        public int TotalReviews { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;

        [ForeignKey("InstructorId")]
        public ApplicationUser? Instructor { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
