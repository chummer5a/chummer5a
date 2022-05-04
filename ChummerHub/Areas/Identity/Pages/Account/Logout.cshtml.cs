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
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel'
    public class LogoutModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.LogoutModel(SignInManager<ApplicationUser>, ILogger<LogoutModel>)'
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.LogoutModel(SignInManager<ApplicationUser>, ILogger<LogoutModel>)'
        {
            _signInManager = signInManager;
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnGet()'
        public void OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnGet()'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnPost(string)'
        public async Task<IActionResult> OnPost(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnPost(string)'
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Page();
            }
        }
    }
}