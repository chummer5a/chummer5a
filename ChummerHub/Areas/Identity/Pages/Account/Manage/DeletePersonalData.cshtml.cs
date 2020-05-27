using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel'
    public class DeletePersonalDataModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.DeletePersonalDataModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<DeletePersonalDataModel>)'
        public DeletePersonalDataModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.DeletePersonalDataModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<DeletePersonalDataModel>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.InputModel'
        {
            [Required]
            [DataType(DataType.Password)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.InputModel.Password'
            public string Password { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.InputModel.Password'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.RequirePassword'
        public bool RequirePassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.RequirePassword'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.OnGet()'
        public async Task<IActionResult> OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.OnGet()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeletePersonalDataModel.OnPostAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Password not correct.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}