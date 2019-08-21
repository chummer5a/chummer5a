using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel'
    public partial class IndexModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.IndexModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, IEmailSender)'
        public IndexModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.IndexModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, IEmailSender)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.Username'
        public string Username { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.Username'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.IsEmailConfirmed'
        public bool IsEmailConfirmed { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.IsEmailConfirmed'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.StatusMessage'

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel'
        {
            [Required]
            [EmailAddress]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel.Email'
            public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel.Email'

            [Phone]
            [Display(Name = "Phone number")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel.PhoneNumber'
            public string PhoneNumber { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.InputModel.PhoneNumber'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnGetAsync()'
        public async Task<IActionResult> OnGetAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnGetAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnPostAsync()'
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnPostSendVerificationEmailAsync()'
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'IndexModel.OnPostSendVerificationEmailAsync()'
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "SINners from ChummerHub - confirm your email",
                $"Please confirm your account to access the online SINners database from ChummerHub by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email. Since a free sendmail-provider is used, this may take a while.";
            return RedirectToPage();
        }
    }
}
