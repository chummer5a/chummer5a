using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChummerHub.Controllers
{
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
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

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        
        public async Task<ActionResult<List<String>>> GetRoles()
        {
            try
            {
                //var user = _userManager.FindByEmailAsync(email).Result;
                var user = await _signInManager.UserManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);
                
                //var result = new JsonResult(roles);
                return Ok(roles.ToList());
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
            
        }

        


    }
}
