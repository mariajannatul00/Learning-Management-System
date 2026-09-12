using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Web.Models
{
    public class Course : BaseEntity, IValidatableObject
    {
        [Required]
        [MaxLength(200)]
        [MinLength(3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [MinLength(5)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 9999999999.99, ErrorMessage = "Regular price must be greater than 0.")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 9999999999.99, ErrorMessage = "Discount price must be greater than 0 if provided.")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced

        [Range(0.5, 10000, ErrorMessage = "Duration must be at least 0.5 hours.")]
        public double DurationHours { get; set; } = 0;

        [Required]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsEnded => EndDate.HasValue && DateTime.UtcNow > EndDate.Value;

        public bool IsStarted => StartDate.HasValue && DateTime.UtcNow >= StartDate.Value;

        [Range(1, int.MaxValue, ErrorMessage = "Total lectures must be a positive number.")]
        public int TotalLectures { get; set; } = 0;


        [Required]
        [MaxLength(500)]
        [Url(ErrorMessage = "Thumbnail URL must be a valid URL.")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Url(ErrorMessage = "Intro video URL must be a valid URL.")]
        public string IntroVideoUrl { get; set; } = string.Empty;

        public double Rating { get; set; } = 5.0;

        public int TotalReviews { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;

        [ForeignKey("InstructorId")]
        public ApplicationUser? Instructor { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DiscountPrice.HasValue && DiscountPrice.Value > Price)
            {
                yield return new ValidationResult(
                    "Discount price must be lower than or equal to the regular price.",
                    new[] { nameof(DiscountPrice) });
            }

            if (StartDate.HasValue && StartDate.Value.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Course start date cannot be in the past.",
                    new[] { nameof(StartDate) });
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value.Date < StartDate.Value.Date)
            {
                yield return new ValidationResult(
                    "Course end date cannot be before the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}
