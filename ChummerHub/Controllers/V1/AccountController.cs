using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChummerHub.Controllers
{
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AccountController : Controller
    {

        private UserManager<ApplicationUser> _userManager = null;
        private SignInManager<ApplicationUser> _signInManager = null;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        //}

        [HttpGet]
        public IActionResult Login(string email, string password)
        {
#if DEBUG
#else
            _signInManager.SignOutAsync().RunSynchronously();
#endif
            var user = _userManager.FindByEmailAsync(email).Result;
            var result = _signInManager.PasswordSignInAsync(email, password, true, true).Result;
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
            //return new JsonResult(result);
        }

       

    }
}
