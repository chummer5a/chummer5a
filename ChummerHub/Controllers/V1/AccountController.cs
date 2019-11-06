using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Transactions;

namespace ChummerHub.Controllers
{
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController'
    public class AccountController : Controller
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController'
    {

        private UserManager<ApplicationUser> _userManager = null;
        private SignInManager<ApplicationUser> _signInManager = null;
        private ApplicationDbContext _context;
        private RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger _logger;
        private TelemetryClient tc;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.AccountController(ApplicationDbContext, ILogger<AccountController>, UserManager<ApplicationUser>, SignInManager<ApplicationUser>, RoleManager<ApplicationRole>, TelemetryClient)'
        public AccountController(ApplicationDbContext context,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.AccountController(ApplicationDbContext, ILogger<AccountController>, UserManager<ApplicationUser>, SignInManager<ApplicationUser>, RoleManager<ApplicationRole>, TelemetryClient)'
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            TelemetryClient telemetry)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            tc = telemetry;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetPossibleRoles")]
        [Authorize]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetPossibleRoles()'
        public async Task<ActionResult<ResultAccountGetPossibleRoles>> GetPossibleRoles()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetPossibleRoles()'
        {
            ResultAccountGetPossibleRoles res;
            try
            {
                var roles = await _context.Roles.ToListAsync();
                var list = (from a in roles select a.Name).ToList();
                res = new ResultAccountGetPossibleRoles(list);
                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultAccountGetPossibleRoles(e);
                return BadRequest(res);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetRoles")]
        [Authorize]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetRoles()'
        public async Task<ActionResult<ResultAccountGetRoles>> GetRoles()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetRoles()'
        {
            ResultAccountGetRoles res;
            try
            {
                //var user = _userManager.FindByEmailAsync(email).Result;
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if (user.EmailConfirmed)
                {
                    await SeedData.EnsureRole(Program.MyHost.Services, user.Id, API.Authorizarion.Constants.UserRoleConfirmed, _roleManager, _userManager);
                }
                var roles = await _userManager.GetRolesAsync(user);
                var possibleRoles = await _context.Roles.ToListAsync();
                var list = (from a in possibleRoles select a.Name).ToList();
                res = new ResultAccountGetRoles(roles, list);

                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultAccountGetRoles(e);
                return BadRequest(res);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetUserByEmail")]
        [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetUserByEmail(string)'
        public async Task<ActionResult<ResultAccountGetUserByEmail>> GetUserByEmail(string email)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetUserByEmail(string)'
        {
            ResultAccountGetUserByEmail res;
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                res = new ResultAccountGetUserByEmail(user);
                if (user == null)
                    return NotFound(res);
                user.PasswordHash = "";
                user.SecurityStamp = "";
                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultAccountGetUserByEmail(e);
                return BadRequest(res);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetAddSqlDbUser")]
        [Authorize(Roles = "Administrator")]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAddSqlDbUser(string, string, string, string)'
        public async Task<ActionResult<string>> GetAddSqlDbUser(string username, string password, string start_ip_address, string end_ip_address)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetAddSqlDbUser(string, string, string, string)'
        {
            string result = "";
            try
            {
                if (String.IsNullOrEmpty(username))
                    throw new ArgumentNullException(nameof(username));
                if (String.IsNullOrEmpty(password))
                    throw new ArgumentNullException(nameof(password));

                IPAddress startaddress = null;
                if (!String.IsNullOrEmpty(start_ip_address))
                {
                    startaddress = IPAddress.Parse(start_ip_address);
                }
                IPAddress endaddress = null;
                if (!String.IsNullOrEmpty(end_ip_address))
                {
                    endaddress = IPAddress.Parse(end_ip_address);
                }
                if (String.IsNullOrEmpty(Startup.ConnectionStringToMasterSqlDb))
                {
                    throw new ArgumentNullException("Startup.ConnectionStringToMasterSqlDB");
                }


                try
                {
                    string cmd = "CREATE LOGIN " + username + " WITH password = '" + password + "';";
                    using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringToMasterSqlDb))
                    {
                        await masterConnection.OpenAsync();
                        using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                        {
                            dbcmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException e)
                {
                    result += e.Message + Environment.NewLine + Environment.NewLine;
                }
                //create the user in the master DB
                try
                {
                    string cmd = "CREATE USER " + username + " FROM LOGIN " + username + ";";
                    using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringToMasterSqlDb))
                    {
                        await masterConnection.OpenAsync();
                        using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                        {
                            dbcmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException e)
                {
                    result += e.Message + Environment.NewLine + Environment.NewLine;
                }
                //create the user in the sinner_db as well!
                try
                {
                    string cmd = "CREATE USER " + username + " FROM LOGIN " + username + ";";
                    using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringSinnersDb))
                    {
                        await masterConnection.OpenAsync();
                        using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                        {
                            dbcmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    result += e.Message + Environment.NewLine + Environment.NewLine;
                }
                try
                {
                    string cmd = "ALTER ROLE dbmanager ADD MEMBER " + username + ";";
                    using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringSinnersDb))
                    {
                        await masterConnection.OpenAsync();
                        using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                        {
                            dbcmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    bool worked = false;
                    try
                    {
                        string cmd = "EXEC sp_addrolemember 'db_owner', '" + username + "';";
                        using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringSinnersDb))
                        {
                            await masterConnection.OpenAsync();
                            using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                            {
                                dbcmd.ExecuteNonQuery();
                            }
                        }
                        worked = true;
                    }
                    catch (Exception e1)
                    {
                        result += e1.ToString() + Environment.NewLine + Environment.NewLine;
                    }
                    if (worked)
                    {
                        result += "User added!" + Environment.NewLine + Environment.NewLine;
                    }
                    else
                    {
                        result += e.Message + Environment.NewLine + Environment.NewLine;
                    }
                }
                try
                {
                    string cmd = "EXEC sp_set_database_firewall_rule N'Allow " +
                                 username + "', '" + startaddress + "', '" + endaddress + "';";
                    using (SqlConnection masterConnection = new SqlConnection(Startup.ConnectionStringSinnersDb))
                    {
                        await masterConnection.OpenAsync();
                        using (SqlCommand dbcmd = new SqlCommand(cmd, masterConnection))
                        {
                            dbcmd.ExecuteNonQuery();
                        }

                        result += "Firewallrule added: " + startaddress + " - " + endaddress + Environment.NewLine +
                                  Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    result += e.Message + Environment.NewLine + Environment.NewLine;
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                result += Environment.NewLine + e;
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException(result, e);
                return BadRequest(hue);
            }
        }

        [HttpPost]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PostSetUserRole")]
        [Authorize(Roles = "Administrator")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.PostSetUserRole(string, string)'
        public async Task<ActionResult<ApplicationUser>> PostSetUserRole(string email, string userrole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.PostSetUserRole(string, string)'
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return NotFound();
                await SeedData.EnsureRole(Program.MyHost.Services, user.Id, userrole, _roleManager, _userManager);
                user.PasswordHash = "";
                user.SecurityStamp = "";
                return Ok(user);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException(e.Message, e);
                return BadRequest(hue);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetUserByAuthorization")]
        [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetUserByAuthorization()'
        public async Task<ActionResult<ResultAccountGetUserByAuthorization>> GetUserByAuthorization()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetUserByAuthorization()'
        {
            ResultAccountGetUserByAuthorization res;
            try
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                res = new ResultAccountGetUserByAuthorization(user);
                if (user == null)
                    return NotFound(res);

                user.PasswordHash = "";
                user.SecurityStamp = "";
                return Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                res = new ResultAccountGetUserByAuthorization(e);
                return BadRequest(res);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ResetDb")]
        [Authorize(Roles = "Administrator")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetDeleteAllSINnersDb()'
        public async Task<ActionResult<string>> GetDeleteAllSINnersDb()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetDeleteAllSINnersDb()'
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
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException(e.Message, e);
                return BadRequest(hue);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("DeleteAndRecreate")]
        [Authorize(Roles = "Administrator")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetDeleteAndRecreateDb()'
        public async Task<ActionResult<string>> GetDeleteAndRecreateDb()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetDeleteAndRecreateDb()'
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
                Startup.Seed(null);
                return Ok("Database recreated");
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException(e.Message, e);
                return BadRequest(hue);
            }
        }

        /// <summary>
        /// Search for Sinners for one user
        /// </summary>
        /// <returns>SINSearchGroupResult</returns>
        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status200OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(StatusCodes.Status400BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetSinnersByAuthorization")]
        [Authorize]
        public async Task<ActionResult<ResultAccountGetSinnersByAuthorization>> GetSINnersByAuthorization()
        {
            try
            {
                var res = await GetSINnersByAuthorizationInternal();
                return res;
            }
            catch (Exception e)
            {
                ResultAccountGetSinnersByAuthorization error = new ResultAccountGetSinnersByAuthorization(e);
                return error;
            }
        }

        private async Task<ActionResult<ResultAccountGetSinnersByAuthorization>> GetSINnersByAuthorizationInternal()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            ResultAccountGetSinnersByAuthorization res = null;

            SINSearchGroupResult ret = new SINSearchGroupResult();
            res = new ResultAccountGetSinnersByAuthorization(ret);
            SINnerGroup sg = new SINnerGroup();
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                var e = new AuthenticationException("User is not authenticated.");
                res = new ResultAccountGetSinnersByAuthorization(e)
                {
                    ErrorText = "Unauthorized"
                };
                return BadRequest(res);
            }
            user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();

            SINnerSearchGroup ssg = new SINnerSearchGroup(sg, user)
            {
                MyMembers = new List<SINnerSearchGroupMember>()
            };
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                
                try
                {
                    

                    var roles = await _userManager.GetRolesAsync(user);
                    ret.Roles = roles.ToList();
                    ssg.Groupname = user.UserName;
                    ssg.Id = Guid.Empty;
                  
                 
                    var worklist = (from a in user.FavoriteGroups select a.FavoriteGuid).ToList();
                    var groupworklist = await (from a in _context.SINnerGroups
                            .Include(a => a.MyGroups)
                            .ThenInclude(b => b.MyGroups)
                            .ThenInclude(c => c.MyGroups)
                            .ThenInclude(d => d.MyGroups)
                                               where (a.Id != null && worklist.Contains(a.Id.Value) == true)
                        select a).ToListAsync();
                    ssg.MySINSearchGroups = await RecursiveBuildGroupMembers(groupworklist, user);
                    var memberworklist = await (from a in _context.SINners
                                .Include(a => a.MyGroup)
                                .Include(a => a.SINnerMetaData.Visibility)
                            where (a.Id != null && worklist.Contains(a.Id.Value) == true)
                            select a
                        ).ToListAsync();
                    foreach (var member in memberworklist)
                    {
                        
                            if (member.SINnerMetaData?.Visibility?.IsGroupVisible == false)
                            {
                                if (member.SINnerMetaData?.Visibility.UserRights.Any(a =>
                                        String.IsNullOrEmpty(a.EMail) == false) == true)
                                {
                                    if (member.SINnerMetaData?.Visibility.UserRights.Any(a =>
                                            a.EMail?.ToUpperInvariant() == user.NormalizedEmail) == false)
                                    {
                                        //dont show this guy!
                                        continue;
                                    }
                                }
                            }
                        
                        member.LastDownload = DateTime.Now;
                        if (member.MyGroup == null)
                            member.MyGroup = new SINnerGroup();
                        if (member.MyGroup.MyGroups == null)
                            member.MyGroup.MyGroups = new List<SINnerGroup>();
                        SINnerSearchGroupMember sinssgGroupMember = new SINnerSearchGroupMember(user, member)
                        {
                            MySINner = member
                        };
                        ssg.MyMembers.Add(sinssgGroupMember);
                    }

                    await _context.SaveChangesAsync();
                    ret.SINGroups.Add(ssg);
                    res = new ResultAccountGetSinnersByAuthorization(ret);
                    return Ok(res);
                }
                catch (Exception e)
                {
                    try
                    {
                        user = await _signInManager.UserManager.GetUserAsync(User);
                        ExceptionTelemetry et = new ExceptionTelemetry(e);
                        et.Properties.Add("user", User.Identity.Name);
                        tc.TrackException(et);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex.ToString());
                    }

                    res = new ResultAccountGetSinnersByAuthorization(e);
                    return BadRequest(res);
                }
                finally
                {
                    Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry("GetSINnersByAuthorization", DateTimeOffset.Now, sw.Elapsed, "Azure", res?.CallSuccess ?? false, res?.ErrorText);
                    tc?.TrackAvailability(telemetry);
                }
            }
        }

        private async Task<List<SINnerSearchGroup>> RecursiveBuildGroupMembers(List<SINnerGroup> groupworklist, ApplicationUser user)
        {
            List<SINnerSearchGroup> addlist = new List<SINnerSearchGroup>();
            foreach (var singroup in groupworklist)
            {
                if (singroup == null)
                    continue;
                SINnerSearchGroup ssgFromSIN;
                if (addlist.Any(a => a.Id != null && a.Id == singroup.Id))
                {
                    ssgFromSIN = addlist.FirstOrDefault(a => a.Id != null && a.Id == singroup.Id);
                }
                else
                {
                    if (singroup.Id == null)
                    {
                        _context.SINnerGroups.Remove(singroup);
                        continue;
                    }
                    ssgFromSIN = new SINnerSearchGroup(singroup, user);
                    addlist.Add(ssgFromSIN);
                    //for all groups in this group
                    ssgFromSIN.MySINSearchGroups = await RecursiveBuildGroupMembers(singroup.MyGroups, user);
                }

                //add all members of his group
                var members = await singroup.GetGroupMembers(_context, false);
                foreach (var member in members)
                {
                    if (singroup.IsPublic != true)
                    {
                        if (member.SINnerMetaData?.Visibility?.IsGroupVisible == false)
                        {
                            if (member.SINnerMetaData?.Visibility.UserRights.Any(a =>
                                    String.IsNullOrEmpty(a.EMail) == false) == true)
                            {
                                if (member.SINnerMetaData?.Visibility.UserRights.Any(a =>
                                        a.EMail?.ToUpperInvariant() == user.NormalizedEmail) == false)
                                {
                                    //dont show this guy!
                                    continue;
                                }
                            }
                        }
                    }

                    member.LastDownload = DateTime.Now;
                    member.MyGroup = singroup;
                    member.MyGroup.MyGroups = new List<SINnerGroup>();
                    SINnerSearchGroupMember sinssgGroupMember = new SINnerSearchGroupMember(user, member)
                    {
                        MySINner = member
                    };
                    //check if it is already added:
                    var groupseq = from a in ssgFromSIN.MyMembers where a.MySINner == member select a;
                    if (groupseq.Any())
                        continue;
                    ssgFromSIN.MyMembers.Add(sinssgGroupMember);
                }

                singroup.PasswordHash = "";
                
                singroup.MyGroups = new List<SINnerGroup>();
            }

            return addlist;
        }


        // GET: api/ChummerFiles
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NoContent)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountGetSinnerAsAdmin")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetSinnerAsAdmin()'
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetSinnerAsAdmin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.GetSinnerAsAdmin()'
        {
            ResultAccountGetSinnersByAuthorization res;

            SINSearchGroupResult ret = new SINSearchGroupResult();
            res = new ResultAccountGetSinnersByAuthorization(ret);
            SINnerGroup sg = new SINnerGroup();
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                var e = new AuthenticationException("User is not authenticated.");
                res = new ResultAccountGetSinnersByAuthorization(e)
                {
                    ErrorText = "Unauthorized"
                };
                return BadRequest(res);
            }
            user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();

            SINnerSearchGroup ssg = new SINnerSearchGroup(sg, user)
            {
                MyMembers = new List<SINnerSearchGroupMember>()
            };
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                ret.Roles = roles.ToList();
                ssg.Groupname = user.Email;
                ssg.Id = Guid.Empty;
                //get all from visibility
                List<SINner> mySinners = await _context.SINners.Include(a => a.MyGroup)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .OrderByDescending(a => a.UploadDateTime)
                    .Take(200)
                    .ToListAsync();
                foreach (var sin in mySinners)
                {
                    SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember(user, sin);
                    ssg.MyMembers.Add(ssgm);
                    if (sin.MyGroup != null)
                    {
                        SINnerSearchGroup ssgFromSIN;
                        if (ssg.MySINSearchGroups.Any(a => a.Id == sin.MyGroup.Id))
                        {
                            ssgFromSIN = ssg.MySINSearchGroups.FirstOrDefault(a => a.Id == sin.MyGroup.Id);
                        }
                        else
                        {
                            ssgFromSIN = new SINnerSearchGroup(sin.MyGroup, user);
                            ssg.MySINSearchGroups.Add(ssgFromSIN);
                        }
                        //add all members of his group
                        var members = await sin.MyGroup.GetGroupMembers(_context, false);
                        foreach (var member in members)
                        {
                            member.MyGroup = sin.MyGroup;
                            member.MyGroup.MyGroups = new List<SINnerGroup>();
                            SINnerSearchGroupMember sinssgGroupMember = new SINnerSearchGroupMember(user, member);
                            ssgFromSIN.MyMembers.Add(sinssgGroupMember);
                        }
                        sin.MyGroup.PasswordHash = "";
                        sin.MyGroup.MyGroups = new List<SINnerGroup>();

                    }
                }

                ret.SINGroups.Add(ssg);
                res = new ResultAccountGetSinnersByAuthorization(ret);
                return Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                res = new ResultAccountGetSinnersByAuthorization(e);
                return BadRequest(res);
            }
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("AccountLogout")]
        [Authorize]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountController.Logout()'
        public async Task<ActionResult<bool>> Logout()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountController.Logout()'
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
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    ExceptionTelemetry et = new ExceptionTelemetry(e);
                    et.Properties.Add("user", User.Identity.Name);
                    tc.TrackException(et);
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                        _logger.LogError(ex.ToString());
                }
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException(e.Message, e);
                return BadRequest(hue);
            }
        }






    }
}
