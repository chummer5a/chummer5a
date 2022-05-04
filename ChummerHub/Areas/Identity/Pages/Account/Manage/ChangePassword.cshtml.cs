/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel'
    public class ChangePasswordModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.ChangePasswordModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<ChangePasswordModel>)'
        public ChangePasswordModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.ChangePasswordModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<ChangePasswordModel>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.Input'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel'
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.OldPassword'
            public string OldPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.OldPassword'

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.NewPassword'
            public string NewPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.NewPassword'

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.ConfirmPassword'
            public string ConfirmPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.InputModel.ConfirmPassword'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.OnGetAsync()'
        public async Task<IActionResult> OnGetAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.OnGetAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChangePasswordModel.OnPostAsync()'
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

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToPage();
        }
    }
}
