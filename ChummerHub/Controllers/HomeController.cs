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
