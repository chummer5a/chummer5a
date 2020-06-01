using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel'
    public class Disable2faModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.Disable2faModel(UserManager<ApplicationUser>, ILogger<Disable2faModel>)'
        public Disable2faModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.Disable2faModel(UserManager<ApplicationUser>, ILogger<Disable2faModel>)'
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.OnGet()'
        public async Task<IActionResult> OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.OnGet()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");
            }

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Disable2faModel.OnPostAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}