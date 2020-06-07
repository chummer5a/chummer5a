using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel'
    public class ResetAuthenticatorModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel'
    {
        readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        ILogger<ResetAuthenticatorModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.ResetAuthenticatorModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<ResetAuthenticatorModel>)'
        public ResetAuthenticatorModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.ResetAuthenticatorModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<ResetAuthenticatorModel>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.OnGet()'
        public async Task<IActionResult> OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.OnGet()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetAuthenticatorModel.OnPostAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("./EnableAuthenticator");
        }
    }
}
