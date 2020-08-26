using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel'
    public class LoginWithRecoveryCodeModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.LoginWithRecoveryCodeModel(SignInManager<ApplicationUser>, ILogger<LoginWithRecoveryCodeModel>)'
        public LoginWithRecoveryCodeModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginWithRecoveryCodeModel> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.LoginWithRecoveryCodeModel(SignInManager<ApplicationUser>, ILogger<LoginWithRecoveryCodeModel>)'
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.ReturnUrl'
        public string ReturnUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.ReturnUrl'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.InputModel'
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.InputModel.RecoveryCode'
            public string RecoveryCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.InputModel.RecoveryCode'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.OnGetAsync(string)'
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.OnGetAsync(string)'
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.OnPostAsync(string)'
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWithRecoveryCodeModel.OnPostAsync(string)'
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return Page();
            }
        }
    }
}
