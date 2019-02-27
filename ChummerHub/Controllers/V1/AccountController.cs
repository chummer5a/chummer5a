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
        private RoleManager<ApplicationRole> _roleManager;

        public AccountController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                if (user.EmailConfirmed)
                {
                    await SeedData.EnsureRole(Program.MyHost.Services, user.Id, API.Authorizarion.Constants.UserRoleConfirmed, _roleManager, _userManager);
                }
                var roles = await _userManager.GetRolesAsync(user);
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

        [HttpPost]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PostSetUserRole")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ApplicationUser>> PostSetUserRole(string email, string userrole)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if(user == null)
                    return NotFound();
                user.PasswordHash = "";
                user.SecurityStamp = "";
                await SeedData.EnsureRole(Program.MyHost.Services, user.Id, userrole, _roleManager, _userManager);
                return Ok(user);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ResetDb")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<string>> GetDeleteAllSINnersDb()
        {
            try
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Administrator"))
                    return Unauthorized();
                var count = await _context.SINners.CountAsync();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    _context.UserRights.RemoveRange(_context.UserRights.ToList());
                    _context.SINnerComments.RemoveRange(_context.SINnerComments.ToList());
                    _context.Tags.RemoveRange(_context.Tags.ToList());
                    _context.SINnerVisibility.RemoveRange(_context.SINnerVisibility.ToList());
                    _context.SINnerMetaData.RemoveRange(_context.SINnerMetaData.ToList());
                    _context.SINners.RemoveRange(_context.SINners.ToList());
                    _context.UploadClients.RemoveRange(_context.UploadClients.ToList());
             
                    await _context.SaveChangesAsync();
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
                return Ok("Reseted " + count + " SINners");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("DeleteAndRecreate")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<string>> GetDeleteAndRecreateDb()
        {
            try
            {
#if DEBUG
                System.Diagnostics.Trace.TraceInformation("Users is NOT checked in Debug!");
#else
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if(user == null)
                    return Unauthorized();
                var roles = await _userManager.GetRolesAsync(user);
                if(!roles.Contains("Administrator"))
                    return Unauthorized();
#endif
                await _context.Database.EnsureDeletedAsync();
                Program.Seed();
                return Ok("Database recreated");
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SinnersByAuthorization")]
        [Authorize]
        public async Task<ActionResult<SINSearchResult>> GetSINnersByAuthorization()
        {
            SINSearchResult ret = new SINSearchResult();
            try
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if(user == null)
                {
                    ret.ErrorText = "Unauthorized";
                    return Unauthorized(ret);
                }
                //get all from visibility
                SINnersList list = new SINnersList();
                var userseq = (from a in _context.UserRights where a.EMail == user.NormalizedEmail && a.CanEdit == true select a).ToList();
                foreach(var ur in userseq)
                {
                    if (ur?.SINnerId == null) continue;
                    var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights)
                        .Include(b => b.MyGroup)
                        .FirstOrDefaultAsync(a => a.Id == ur.SINnerId);
                    if (sin != null)
                    {
                        SINnerList owndSINner = new SINnerList
                        {
                            SINner = sin
                        };
                        if(sin.MyGroup != null)
                        {
                            //add all members of his group
                            var members = await sin.MyGroup.GetGroupMembers(_context);
                            foreach(var member in members)
                            {
                                if((member.SINnerMetaData.Visibility.IsGroupVisible == true)
                                    || (member.SINnerMetaData.Visibility.IsPublic)
                                    )
                                {
                                    SINnerList memberlist = new SINnerList();
                                    memberlist.SINner = member;
                                    owndSINner.SINList.Add(memberlist);
                                }
                            }
                        }
                        list.MySINnersList.Add(owndSINner);
                    }
                }

                //get all that are viewable but NOT editable
                SINnerList viewlist = new SINnerList();
                viewlist.Header = "View";
                userseq = (from a in _context.UserRights where a.EMail == user.NormalizedEmail && a.CanEdit == false select a).ToList();
                foreach(var ur in userseq)
                {
                    if(ur?.SINnerId == null) continue;
                    var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == ur.SINnerId);
                    if(sin != null)
                    {
                        bool found = false;
                        foreach(var sinlist in list.MySINnersList)
                        {
                            if (sinlist.SINner == sin)
                            {
                                found = true;
                                break; 
                            }
                        }
                        if (!found)
                        {
                            SINnerList childviewlist = new SINnerList();
                            childviewlist.SINner = sin;
                            viewlist.SINList.Add(childviewlist);
                        }
                    }
                }
                list.MySINnersList.Add(viewlist);
                ret.SINLists.Add(list);
                return Ok(ret);
            }
            catch (Exception e)
            {
                ret.ErrorText = e.ToString();
                return BadRequest(ret);
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
