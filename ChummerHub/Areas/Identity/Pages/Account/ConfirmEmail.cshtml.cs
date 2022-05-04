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
using System;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel'
    public class ConfirmEmailModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.ConfirmEmailModel(UserManager<ApplicationUser>)'
        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.ConfirmEmailModel(UserManager<ApplicationUser>)'
        {
            _userManager = userManager;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.OnGetAsync(string, string)'
        public async Task<IActionResult> OnGetAsync(string userId, string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ConfirmEmailModel.OnGetAsync(string, string)'
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }

            return Page();
        }
    }
}
