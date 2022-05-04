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
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel'
    public class ResetPasswordModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.ResetPasswordModel(UserManager<ApplicationUser>)'
        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.ResetPasswordModel(UserManager<ApplicationUser>)'
        {
            _userManager = userManager;
        }

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.Input'
        public InputModel Input { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.Input'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel'
        public class InputModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel'
        {
            [Required]
            [EmailAddress]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Email'
            public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Email'

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Password'
            public string Password { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Password'

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.ConfirmPassword'
            public string ConfirmPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.ConfirmPassword'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Code'
            public string Code { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.InputModel.Code'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.OnGet(string)'
        public IActionResult OnGet(string code = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.OnGet(string)'
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = code
                };
                return Page();
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResetPasswordModel.OnPostAsync()'
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
