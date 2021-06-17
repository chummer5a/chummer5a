using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel'
    public class EnableAuthenticatorModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.EnableAuthenticatorModel(UserManager<ApplicationUser>, ILogger<EnableAuthenticatorModel>, UrlEncoder)'
        public EnableAuthenticatorModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.EnableAuthenticatorModel(UserManager<ApplicationUser>, ILogger<EnableAuthenticatorModel>, UrlEncoder)'
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.SharedKey'
        public string SharedKey { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.SharedKey'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.AuthenticatorUri'
        public string AuthenticatorUri { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.AuthenticatorUri'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.RecoveryCodes'
        public string[] RecoveryCodes { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.RecoveryCodes'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.StatusMessage'

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.InputModel'
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.InputModel.Code'
            public string Code { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.InputModel.Code'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.OnGetAsync()'
        public async Task<IActionResult> OnGetAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.OnGetAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EnableAuthenticatorModel.OnPostAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hypens
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4) + ' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("ChummerHub"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
