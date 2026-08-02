using LMS.Web.Models;
using LMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.Web.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
        Task<UserProfileViewModel?> GetUserProfileAsync(string userId);
        Task<IdentityResult> UpdateProfileAsync(UserProfileViewModel model, string? avatarPath);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);
    }
}
