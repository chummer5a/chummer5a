using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel'
    public class LoginModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.LoginModel(SignInManager<ApplicationUser>, ILogger<LoginModel>)'
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.LoginModel(SignInManager<ApplicationUser>, ILogger<LoginModel>)'
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ExternalLogins'
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ExternalLogins'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ReturnUrl'
        public string ReturnUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ReturnUrl'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ErrorMessage'
        public string ErrorMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.ErrorMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel'
        {
            [Required]
            [EmailAddress]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.Email'
            public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.Email'

            [Required]
            [DataType(DataType.Password)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.Password'
            public string Password { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.Password'

            [Display(Name = "Remember me?")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.RememberMe'
            public bool RememberMe { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.InputModel.RememberMe'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.OnGetAsync(string)'
        public async Task OnGetAsync(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.OnGetAsync(string)'
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.OnPostAsync(string)'
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LoginModel.OnPostAsync(string)'
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
                var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
