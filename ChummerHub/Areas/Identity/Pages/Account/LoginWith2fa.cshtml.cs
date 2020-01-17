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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel'
    public class LoginWith2faModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWith2faModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.LoginWith2faModel(SignInManager<ApplicationUser>, ILogger<LoginWith2faModel>)'
        public LoginWith2faModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginWith2faModel> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.LoginWith2faModel(SignInManager<ApplicationUser>, ILogger<LoginWith2faModel>)'
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.RememberMe'
        public bool RememberMe { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.RememberMe'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.ReturnUrl'
        public string ReturnUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.ReturnUrl'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel'
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel.TwoFactorCode'
            public string TwoFactorCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel.TwoFactorCode'

            [Display(Name = "Remember this machine")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel.RememberMachine'
            public bool RememberMachine { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.InputModel.RememberMachine'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.OnGetAsync(bool, string)'
        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.OnGetAsync(bool, string)'
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.OnPostAsync(bool, string)'
        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginWith2faModel.OnPostAsync(bool, string)'
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
