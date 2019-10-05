using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel'
    public class SetPasswordModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.SetPasswordModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>)'
        public SetPasswordModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.SetPasswordModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.Input'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel'
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel.NewPassword'
            public string NewPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel.NewPassword'

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel.ConfirmPassword'
            public string ConfirmPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.InputModel.ConfirmPassword'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.OnGetAsync()'
        public async Task<IActionResult> OnGetAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.OnGetAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword");
            }

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SetPasswordModel.OnPostAsync()'
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

            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your password has been set.";

            return RedirectToPage();
        }
    }
}
