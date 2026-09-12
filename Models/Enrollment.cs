using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Web.Models
{
    public class Enrollment : BaseEntity
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; }

        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankTranId { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Completed";

        // Course Drop Request Fields
        [MaxLength(50)]
        public string DropStatus { get; set; } = "None"; // "None", "Pending", "Approved", "Rejected"

        public DateTime? DropRequestedAt { get; set; }

        [MaxLength(500)]
        public string? DropReason { get; set; }

        public DateTime? DropApprovedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;

        public int DeductionPercentage { get; set; } = 0;
    }
}

