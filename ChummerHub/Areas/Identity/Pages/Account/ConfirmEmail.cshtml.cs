using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel'
    public class ConfirmEmailModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.ConfirmEmailModel(UserManager<ApplicationUser>)'
        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.ConfirmEmailModel(UserManager<ApplicationUser>)'
        {
            _userManager = userManager;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.OnGetAsync(string, string)'
        public async Task<IActionResult> OnGetAsync(string userId, string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.OnGetAsync(string, string)'
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }

            return Page();
        }
    }
}
