using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetRoles")]
        [Authorize]

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

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountByEmail")]
        [Authorize]
        public async Task<ActionResult<ApplicationUser>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return NotFound();
                user.PasswordHash = "";
                user.SecurityStamp = "";
                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountByAuthorization")]
        [Authorize]
        public async Task<ActionResult<ApplicationUser>> GetUserByAuthorization()
        {
            try
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                
                if (user == null)
                    return NotFound();
                user.PasswordHash = "";
                user.SecurityStamp = "";
                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SinnersByAuthorization")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Models.V1.SINner>>> GetSINnersByAuthorization()
        {
            try
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var userseq = (from a in _context.UserRights where a.EMail == user.NormalizedEmail select a).ToList();
                List<SINner> result = new List<SINner>();
                foreach(var ur in userseq)
                {
                    if (ur?.SINnerId == null) continue;
                    var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == ur.SINnerId);

                    //var sin = await _context.SINners.FindAsync(ur.SINnerId);
                    if (sin != null)
                    {
                        
                        //sin.SINnerMetaData.Tags = await (from a in _context.Tags.Include(b => b.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 .ThenInclude(t => t.Tags)
                        //                                 where a.SINnerId == sin.Id select a).ToListAsync();
                        //sin.SINnerMetaData.Visibility.UserRights = await (from a in _context.UserRights where a.SINnerId == sin.Id select a).ToListAsync();

                        result.Add(sin);
                    }
                }
                //var sinnersseq = (from a in _context.SINners
                //                 where a.SINnerMetaData.Visibility != null
                //                        && a.SINnerMetaData.Visibility.UserRights.Any(b => userseq.Contains(b)) select a).ToList();
                //foreach(var sin in sinnersseq)
                //{
                //    await _context.Entry(sin).Reference(i => i.SINnerMetaData).LoadAsync();
                //    //await _context.Entry(sin.SINnerMetaData).Reference(i => i.Visibility).LoadAsync();
                //    //sin.SINnerMetaData.Tags = (from a in _context.Tags where a.SINnerId == sin.Id select a).ToList();
                //    //await _context.Entry(sin.SINnerMetaData).Collection(i => i.Tags).LoadAsync();
                //    //await _context.Entry(sin.SINnerMetaData.Visibility).Collection(i => i.UserRights).LoadAsync();

                //}
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountLogout")]
        [Authorize]

        public async Task<ActionResult<bool>> Logout()
        {
            try
            {
                //var user = _userManager.FindByEmailAsync(email).Result;
                //var user = await _signInManager.UserManager.GetUserAsync(User);
                await _signInManager.SignOutAsync();
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }






    }
}
