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
﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel'
    public class ExternalLoginsModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.ExternalLoginsModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>)'
        public ExternalLoginsModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.ExternalLoginsModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.CurrentLogins'
        public IList<UserLoginInfo> CurrentLogins { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.CurrentLogins'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OtherLogins'
        public IList<AuthenticationScheme> OtherLogins { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OtherLogins'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.ShowRemoveButton'
        public bool ShowRemoveButton { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.ShowRemoveButton'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnGetAsync()'
        public async Task<IActionResult> OnGetAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnGetAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CurrentLogins = await _userManager.GetLoginsAsync(user);
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnPostRemoveLoginAsync(string, string)'
        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnPostRemoveLoginAsync(string, string)'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                throw new InvalidOperationException($"Unexpected error occurred removing external login for user with ID '{userId}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToPage();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnPostLinkLoginAsync(string)'
        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnPostLinkLoginAsync(string)'
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnGetLinkLoginCallbackAsync()'
        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ExternalLoginsModel.OnGetLinkLoginCallbackAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }
    }
}
