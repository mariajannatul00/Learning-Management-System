using Microsoft.AspNetCore.Identity;
using System;

namespace LMS.Web.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, string? description = null) : base(roleName)
        {
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
