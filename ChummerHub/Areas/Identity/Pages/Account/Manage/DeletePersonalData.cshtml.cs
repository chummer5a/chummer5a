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