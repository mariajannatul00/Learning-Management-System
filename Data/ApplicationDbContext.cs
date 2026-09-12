using LMS.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser entity specifics
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                b.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                b.Property(u => u.ProfilePictureUrl).HasMaxLength(255);
                b.Property(u => u.Bio).HasMaxLength(500);
                b.Property(u => u.Designation).HasMaxLength(100);
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.Property(r => r.Description).HasMaxLength(255);
            });

            // Configure Course relationships
            builder.Entity<Course>(b =>
            {
                b.HasOne(c => c.Category)
                 .WithMany(cat => cat.Courses)
                 .HasForeignKey(c => c.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(c => c.Instructor)
                 .WithMany()
                 .HasForeignKey(c => c.InstructorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Enrollment relationships
            builder.Entity<Enrollment>(b =>
            {
                b.HasOne(e => e.Student)
                 .WithMany()
                 .HasForeignKey(e => e.StudentId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Course)
                 .WithMany(c => c.Enrollments)
                 .HasForeignKey(e => e.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Module & Lesson relationships
            builder.Entity<Module>(b =>
            {
                b.HasOne(m => m.Course)
                 .WithMany()
                 .HasForeignKey(m => m.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Lesson>(b =>
            {
                b.HasOne(l => l.Module)
                 .WithMany(m => m.Lessons)
                 .HasForeignKey(l => l.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<LessonProgress>(b =>
            {
                b.HasOne(lp => lp.Student)
                 .WithMany()
                 .HasForeignKey(lp => lp.StudentId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(lp => lp.Lesson)
                 .WithMany()
                 .HasForeignKey(lp => lp.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
