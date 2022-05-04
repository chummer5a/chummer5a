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
using ChummerHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChummerHub.Controllers
{
    [Authorize]
    //[EnableCors("AllowOrigin")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController'
    public class HomeController : Controller
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController'
    {
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Index()'
        public IActionResult Index()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Index()'
        {
            return View();
        }

        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController.About()'
        public IActionResult About()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController.About()'
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Contact()'
        public IActionResult Contact()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Contact()'
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Privacy()'
        public IActionResult Privacy()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Privacy()'
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Error()'
        public IActionResult Error()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HomeController.Error()'
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public RedirectResult RedirectToChummer(string args)
        //{
        //    if (String.IsNullOrEmpty(args))
        //        throw new ArgumentException("url unknown: " + args);
        //    return Redirect(args);
        //}
    }
}
