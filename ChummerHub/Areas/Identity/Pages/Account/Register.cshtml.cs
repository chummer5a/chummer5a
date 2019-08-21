using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel'
    public class RegisterModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.RegisterModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<RegisterModel>, IEmailSender)'
        public RegisterModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.RegisterModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<RegisterModel>, IEmailSender)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.ReturnUrl'
        public string ReturnUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.ReturnUrl'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel'
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.Email'
            public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.Email'

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.Password'
            public string Password { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.Password'

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.ConfirmPassword'
            public string ConfirmPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.InputModel.ConfirmPassword'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.OnGet(string)'
        public void OnGet(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.OnGet(string)'
        {
            ReturnUrl = returnUrl;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.OnPostAsync(string)'
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterModel.OnPostAsync(string)'
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var result1 = await _userManager.AddToRoleAsync(user, ChummerHub.API.Authorizarion.Constants.UserRoleRegistered);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
