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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser entity specifics if needed
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
        }
    }
}
