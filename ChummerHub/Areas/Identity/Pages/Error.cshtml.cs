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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ChummerHub.Areas.Identity.Pages
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel'
    public class ErrorModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.RequestId'
        public string RequestId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.RequestId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.ShowRequestId'
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.ShowRequestId'

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.OnGet()'
        public void OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.OnGet()'
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}