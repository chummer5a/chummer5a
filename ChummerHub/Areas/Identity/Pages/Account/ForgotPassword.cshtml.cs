using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel'
    public class ForgotPasswordModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.ForgotPasswordModel(UserManager<ApplicationUser>, IEmailSender)'
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.ForgotPasswordModel(UserManager<ApplicationUser>, IEmailSender)'
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.InputModel'
        {
            [Required]
            [EmailAddress]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.InputModel.Email'
            public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.InputModel.Email'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ForgotPasswordModel.OnPostAsync()'
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
