using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.Web.ViewModels.Account
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Bio { get; set; }

        public string? Designation { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Profile Avatar")]
        public IFormFile? AvatarFile { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }

        public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();
    }
}
