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
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel'
    public class TwoFactorAuthenticationModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel'
    {
        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.TwoFactorAuthenticationModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<TwoFactorAuthenticationModel>)'
        public TwoFactorAuthenticationModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.TwoFactorAuthenticationModel(UserManager<ApplicationUser>, SignInManager<ApplicationUser>, ILogger<TwoFactorAuthenticationModel>)'
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.HasAuthenticator'
        public bool HasAuthenticator { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.HasAuthenticator'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.RecoveryCodesLeft'
        public int RecoveryCodesLeft { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.RecoveryCodesLeft'

        [BindProperty]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.Is2faEnabled'
        public bool Is2faEnabled { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.Is2faEnabled'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.IsMachineRemembered'
        public bool IsMachineRemembered { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.IsMachineRemembered'

        [TempData]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.StatusMessage'
        public string StatusMessage { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.StatusMessage'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.OnGet()'
        public async Task<IActionResult> OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.OnGet()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return Page();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.OnPost()'
        public async Task<IActionResult> OnPost()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TwoFactorAuthenticationModel.OnPost()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }
    }
}