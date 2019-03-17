using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using ChummerHub.API;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ChummerHub.Models.V1.Examples;
using ChummerHub.Services.GoogleDrive;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using Microsoft.AspNetCore.Identity;

//using Swashbuckle.AspNetCore.Filters;

namespace ChummerHub.Controllers.V1
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("SINGroup")]
    [Authorize]
    public class SINnerGroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private SignInManager<ApplicationUser> _signInManager = null;
        private UserManager<ApplicationUser> _userManager = null;

        public SINnerGroupController(ApplicationDbContext context,
            ILogger<SINnerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }


        ~SINnerGroupController()
        {
       
        }

        /// <summary>
        /// Store the new group
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="groupname">In case you want to rename the group</param>
        /// <returns></returns>
        [HttpPut()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PutGroupInGroup")]
        [Authorize(Roles = "Administrator,GroupAdmin")]
        public async Task<ActionResult<SINner>> PutGroupInGroup(Guid GroupId, string groupname, Guid? parentGroupId, string adminIdentityRole, bool isPublicVisible)
        {
            _logger.LogTrace("PutGroupInGroup: " + GroupId + " (" + parentGroupId + ", " + adminIdentityRole +").");
            ApplicationUser user = null;
            try
            {
                user = await _signInManager.UserManager.GetUserAsync(User);
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach (var err in errors)
                    {
                        foreach (var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }

                    }

                    return new BadRequestObjectResult(new HubException(msg));
                }

                SINnerGroup parentGroup = null;
                if (parentGroupId != null)
                {
                    var getParentseq = (from a in _context.SINnerGroups.Include(a => a.MyGroups)
                        where a.Id == parentGroupId
                        select a).Take(1);
                    if (!getParentseq.Any())
                        return NotFound("Parentgroup with Id " + parentGroupId.ToString() + " not found.");
                    parentGroup = getParentseq.FirstOrDefault();
                }

                SINnerGroup myGroup = null;
                var getGroupseq = (from a in _context.SINnerGroups
                                where a.Id == GroupId
                                   select a).Take(1);
                if (!getGroupseq.Any())
                    return NotFound("Group with Id " + parentGroupId.ToString() + " not found.");
                myGroup = getGroupseq.FirstOrDefault();
                myGroup.Groupname = groupname;
                myGroup.IsPublic = isPublicVisible;
                myGroup.MyAdminIdentityRole = adminIdentityRole;
                myGroup.MyParentGroup = parentGroup;
                if (parentGroup != null)
                {
                    if (parentGroup.MyGroups == null)
                        parentGroup.MyGroups = new List<SINnerGroup>();
                    if (!parentGroup.MyGroups.Contains(myGroup))
                        parentGroup.MyGroups.Add(myGroup);
                }
                
                await _context.SaveChangesAsync();
                return Ok(myGroup);
            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("GroupId", GroupId.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in PutSINerInGroup: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }

        }


        /// <summary>
        /// Store the new group
        /// </summary>
        /// <param name="Groupname"></param>
        /// <param name="SinnerId"></param>
        /// <param name="language"></param>
        /// <param name="pwhash"></param>
        /// <returns></returns>
        [HttpPost()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PostGroup")]
        [Authorize]
        public async Task<IActionResult> PostGroup([FromBody] string Groupname, Guid SinnerId, string language, string pwhash )
        {
            _logger.LogTrace("Post SINnerGroupInternal: " + Groupname + " (" + SinnerId + ").");
            ApplicationUser user = null;
            //SINner sinner = null;
            SINnerGroup group = null;
            try
            {
                if(!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                                               .Where(y => y.Count > 0)
                                               .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach(var err in errors)
                    {
                        foreach(var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }

                    }
                    return new BadRequestObjectResult(new HubException(msg));
                }

                if (String.IsNullOrEmpty(Groupname))
                {
                    return BadRequest("Groupname may not be empty.");
                }

                if (SinnerId == Guid.Empty)
                {
                    return BadRequest("SinnerId may not be empty.");
                }

                SINnerGroup parentGroup = null;
                
                var returncode = HttpStatusCode.OK;
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sinnerseq = await (from a in _context.SINners.Include(b => b.SINnerMetaData.Visibility.UserRights) where a.Id == SinnerId select a).ToListAsync();
                if (!sinnerseq.Any())
                {
                    string msg = "Please upload SINner prior to adding him/her to a group!";
                    return new BadRequestObjectResult(new HubException(msg));
                }
                foreach(var sinner in sinnerseq)
                {
                    if(sinner.SINnerMetaData.Visibility.UserRights.Any() == false)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": Visibility contains no entries!");
                    }

                    if(sinner.LastChange == null)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": LastChange not set!");
                    }
                    if(sinner.SINnerMetaData.Visibility.Id == null)
                    {
                        sinner.SINnerMetaData.Visibility.Id = Guid.NewGuid();
                    }
                    bool found = false;
                    foreach(var sinur in sinner.SINnerMetaData.Visibility.UserRights)
                    {
                        if(sinur.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())
                        {
                            if(sinur.CanEdit == true)
                                found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        string msg = "Sinner " + sinner.Id + " is not editable for user " + user.UserName + ".";
                        return new BadRequestObjectResult(new HubException(msg));
                    }

                    var groupfoundseq = await (from a in _context.SINnerGroups where a.Groupname == Groupname select a).ToListAsync();
                    if(groupfoundseq.Any())
                    {
                        string msg = "A group with the name " + Groupname + " already exists!";
                        return new BadRequestObjectResult(new HubException(msg));
                    }
                    group = new SINnerGroup
                    {
                        Id = Guid.NewGuid(),
                        Groupname = Groupname,
                        MyParentGroup = parentGroup,
                        Language = language,
                        PasswordHash = pwhash
                    };
                    parentGroup?.MyGroups.Add(group);
                    _context.SINnerGroups.Add(group);
                    returncode = HttpStatusCode.Created;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }

                    catch(DbUpdateConcurrencyException ex)
                    {
                        foreach(var entry in ex.Entries)
                        {
                            if(entry.Entity is SINner)
                            {
                                Utils.DbUpdateConcurrencyExceptionHandler(entry, _logger);
                            }
                            else if(entry.Entity is Tag)
                            {
                                Utils.DbUpdateConcurrencyExceptionHandler(entry, _logger);
                            }
                            else if (entry.Entity is SINnerGroup)
                            {
                                Utils.DbUpdateConcurrencyExceptionHandler(entry, _logger);
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    "Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        try
                        {
                            var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                            Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                            telemetry.Properties.Add("User", user?.Email);
                            telemetry.Properties.Add("SINnerId", sinner?.Id?.ToString());
                      
                            
                            tc.TrackException(telemetry);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                        HubException hue = new HubException("Exception in PostGroup: " + e.ToString(), e);
                        var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = e.Message };
                        return Conflict(hue);
                    }
                }
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted("PostGroup", group.Id);
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostGroup", group.Id);
                    default:
                        break;
                }
                return BadRequest();
            }
            catch(Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("Groupname", Groupname?.ToString());
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in PostGroup: " + e.Message, e);
                return new BadRequestObjectResult(hue);
              
            }
        }


        /// <summary>
        /// Store the new group
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="SinnerId"></param>
        /// <param name="pwhash"></param>
        /// <returns></returns>
        [HttpPut()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PutSINerInGroup")]
        [Authorize]
        public async Task<ActionResult<SINner>> PutSINerInGroup(Guid GroupId, Guid SinnerId, string pwhash)
        {
            _logger.LogTrace("PutSINerInGroup: " + GroupId + " (" + SinnerId + ").");
            ApplicationUser user = null;
            try
            {
                user = await _signInManager.UserManager.GetUserAsync(User);
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach (var err in errors)
                    {
                        foreach (var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }

                    }

                    return new BadRequestObjectResult(new HubException(msg));
                }
                var roles = await _userManager.GetRolesAsync(user);
                var sin = await PutSiNerInGroupInternal(GroupId, SinnerId, user, _context, _logger, pwhash, roles);
                return Ok(sin);
            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("GroupId", GroupId.ToString());
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in PutSINerInGroup: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }

        }

        internal static async Task<ActionResult<SINner>> PutSiNerInGroupInternal(Guid GroupId, Guid SinnerId, ApplicationUser user, ApplicationDbContext context, ILogger logger, string pwhash, IList<string> userroles)
        {
            try
            {
                if (GroupId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(GroupId), "GroupId may not be empty.");
                }

                if (SinnerId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(SinnerId), "SinnerId may not be empty.");
                }

                var groupset = await (from a in context.SINnerGroups.Include(a => a.MySettings)
                    where a.Id == GroupId
                    select a).ToListAsync();
                if (!groupset.Any())
                {
                    throw new ArgumentException("GroupId not found", nameof(GroupId));
                }

                var group = groupset.FirstOrDefault();

                if ((!String.IsNullOrEmpty(group.PasswordHash))
                    && (group.PasswordHash != pwhash))
                {
                    throw new NoUserRightException("PW is wrong!");
                }

                if (!String.IsNullOrEmpty(group.MyAdminIdentityRole))
                {
                    if (!userroles.Contains(group.MyAdminIdentityRole))
                    {
                        throw new NoUserRightException("User " + user.UserName + " has not the role " + group.MyAdminIdentityRole + ".");
                    }
                }

                var sinnerseq = await (from a in context.SINners
                        .Include(a => a.MyGroup)
                        .Include(a => a.SINnerMetaData)
                        .Include(a => a.SINnerMetaData.Visibility)
                        .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    where a.Id == SinnerId
                    select a).ToListAsync();
                SINner sin = null;
                if (!sinnerseq.Any())
                {
                    throw new ArgumentException("SinnerId not found", nameof(SinnerId));
                }
                else
                {
                    sin = sinnerseq.FirstOrDefault();
                    if (sin != null)
                        sin.MyGroup = group;
                }

                await context.SaveChangesAsync();
                return sin;
            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                        new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("GroupId", GroupId.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex.ToString());
                }

                HubException hue = new HubException("Exception in PutSiNerInGroupInternal: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }
        }

        /// <summary>
        /// Search for Groups
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetGroupById")]
        [Authorize]
        public async Task<ActionResult<SINnerGroup>> GetGroupById(Guid groupid)
        {
            _logger.LogTrace("GetById: " + groupid + ".");
           
            try
            {
                if(!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                                               .Where(y => y.Count > 0)
                                               .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach(var err in errors)
                    {
                        foreach(var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }
                    }
                    return new BadRequestObjectResult(new HubException(msg));
                }

                var groupfoundseq = await (from a in _context.SINnerGroups
                        .Include(a => a.MySettings)
                        .Include(a => a.MyGroups)
                        .ThenInclude(b => b.MyGroups)
                        .ThenInclude(b => b.MyGroups)
                    where a.Id == groupid select a).ToListAsync();

                if (!groupfoundseq.Any())
                    return NotFound(groupid);

                var group = groupfoundseq.FirstOrDefault();
                group.PasswordHash = null;
                return Ok(group);

            }
            catch(Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("groupid", groupid.ToString());
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in GetGroupById: " + e.Message, e);
                return new BadRequestObjectResult(hue);

            }
        }




        /// <summary>
        /// Search for Groups
        /// </summary>
        /// <param name="Groupname"></param>
        /// <param name="UsernameOrEmail"></param>
        /// <param name="SINnerName"></param>
        /// <returns></returns>
        [HttpGet()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetSearchGroups")]
        [Authorize]
        public async Task<ActionResult<SINSearchGroupResult>> GetSearchGroups(string Groupname, string UsernameOrEmail, string SINnerName)
        {
            _logger.LogTrace("GetSearchGroups: " + Groupname + "/" + UsernameOrEmail + "/" + SINnerName + ".");
            try
            {
                return await GetSearchGroupsInternal(Groupname, UsernameOrEmail, SINnerName);
            }
            catch(Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }
        }

        /// <summary>
        /// Remove a sinner from a group. If this sinner is the last member of it's group, the group will be deleted as well!
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="sinnerid"></param>
        /// <returns></returns>
        [HttpDelete()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("DeleteLeaveGroup")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteLeaveGroup(Guid groupid, Guid sinnerid)
        {
            _logger.LogTrace("DeleteLeaveGroup: " + groupid + "/" + sinnerid + ".");
            try
            {
                return await DeleteLeaveGroupInternal(groupid, sinnerid);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in DeleteLeaveGroup: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }
        }

        private async Task<ActionResult<bool>> DeleteLeaveGroupInternal(Guid groupid, Guid sinnerid)
        {
            if ((groupid == null) || (groupid == Guid.Empty))
                throw new ArgumentNullException(nameof(groupid));
            if ((sinnerid == null) || (sinnerid == Guid.Empty))
                throw new ArgumentNullException(nameof(sinnerid));

            var groupbyidseq = await(from a in _context.SINnerGroups
                   .Include(a => a.MyGroups)
                                     where a.Id == groupid
                                     select a).Take(1).ToListAsync();

            if (!groupbyidseq.Any())
                return NotFound(groupid);

            var group = groupbyidseq.FirstOrDefault();

            var members = await group.GetGroupMembers(_context);

            var sinnerseq = await (from a in _context.SINners.Include(a => a.MyGroup)
                                   where a.Id == sinnerid
                                   select a).Take(1).ToListAsync();
            if (!sinnerseq.Any())
                return NotFound(sinnerid);

            var sinner = sinnerseq.FirstOrDefault();

            sinner.MyGroup = null;

            if ((members.Count < 2) && (members.Contains(sinner)))
            {
                if ((group.MyGroups == null) || (!group.MyGroups.Any()))
                {
                    //delete group
                    _context.SINnerGroups.Remove(group);
                }
            }
            await _context.SaveChangesAsync();
            return true;
            

        }

        private async Task<ActionResult<SINSearchGroupResult>> GetSearchGroupsInternal(string Groupname, string UsernameOrEmail, string sINnerName)
        {
            ApplicationUser user = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                                               .Where(y => y.Count > 0)
                                               .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach (var err in errors)
                    {
                        foreach (var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }
                    }
                    return new BadRequestObjectResult(new HubException(msg));
                }
                SINSearchGroupResult result = new SINSearchGroupResult();
                var groupfoundseq = await(from a in _context.SINnerGroups
                                          where a.Groupname == Groupname
                                          select a).ToListAsync();
                if (!groupfoundseq.Any())
                {
                    return NotFound(result);
                }
                var groupid = groupfoundseq.FirstOrDefault().Id;

                var range = await GetSinSearchGroupResultById(groupid);
                result.SINGroups.Add(range);

                if (!String.IsNullOrEmpty(UsernameOrEmail))
                {
                    List<SINner> byUser = new List<SINner>();
                    ApplicationUser bynameuser = await _userManager.FindByNameAsync(UsernameOrEmail);

                    if (bynameuser != null)
                    {
                        var usersinners = await SINner.GetSINnersFromUser(bynameuser, _context, true);
                        byUser.AddRange(usersinners);
                    }

                    ApplicationUser byemailuser = await _userManager.FindByEmailAsync(UsernameOrEmail);
                    if ((byemailuser != null) && (byemailuser != bynameuser))
                    {
                        var usersinners = await SINner.GetSINnersFromUser(byemailuser, _context, true);
                        byUser.AddRange(usersinners);
                    }


                    foreach (var sin in byUser)
                    {
                        if (sin.MyGroup != null)
                        {
                            SINnerSearchGroup ssg = null;
                            var foundseq = (from a in result.SINGroups
                                            where a.Groupname?.ToLowerInvariant() == sin.MyGroup?.Groupname.ToLowerInvariant()
                                            select a).ToList();
                            if (foundseq.Any())
                            {
                                ssg = foundseq.FirstOrDefault();
                            }

                            if (ssg == null)
                                ssg = new SINnerSearchGroup(sin.MyGroup);
                            ssg.Id = sin.MyGroup?.Id;
                            SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember();
                            ssgm.MySINner = sin;
                            if (byemailuser != null)
                                ssgm.Username = byemailuser?.UserName;
                            if (bynameuser != null)
                                ssgm.Username = bynameuser?.UserName;
                            ssg.MyMembers.Add(ssgm);
                        }
                    }
                }
                result.SINGroups = RemovePWHashRecursive(result.SINGroups);

                return Ok(result);

            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("Groupname", Groupname?.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return new BadRequestObjectResult(hue);

            }
        }

        /// <summary>
        /// Search for all members and subgroups of a group
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int) HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int) HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetGroupMembers")]
        [Authorize]
        public async Task<ActionResult<SINSearchGroupResult>> GetGroupMembers(Guid groupid)
        {
            _logger.LogTrace("GetGroupMembers: " + groupid + ".");


            
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();
                    string msg = "ModelState is invalid: ";
                    foreach (var err in errors)
                    {
                        foreach (var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr.ToString();
                        }
                    }

                    return new BadRequestObjectResult(new HubException(msg));
                }

                SINSearchGroupResult result = new SINSearchGroupResult();
                var range = await GetSinSearchGroupResultById(groupid);
                result.SINGroups.Add(range);
                result.SINGroups = RemovePWHashRecursive(result.SINGroups);
                Ok(result);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("groupid", groupid.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return new BadRequestObjectResult(hue);
            }

            return BadRequest();
        }

        private List<SINnerSearchGroup> RemovePWHashRecursive(List<SINnerSearchGroup> sINGroups)
        {
            throw new NotImplementedException();
        }

        private async Task<SINnerSearchGroup> GetSinSearchGroupResultById(Guid? groupid)
        {
            if ((groupid == null) || (groupid == Guid.Empty))
                throw new ArgumentNullException(nameof(groupid));
            SINnerSearchGroup ssg = null;
            var groupbyidseq = await (from a in _context.SINnerGroups
                    .Include(a => a.MySettings)
                    .Include(a => a.MyGroups)
                where a.Id == groupid
                select a).Take(1).ToListAsync();
            
            foreach (var group in groupbyidseq)
            {
                ssg = new SINnerSearchGroup(group);
                ssg.Id = group.Id;
                var members = await ssg.GetGroupMembers(_context);
                foreach (var member in members)
                {
                    SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember
                    {
                        MySINner = member
                    };
                    ssg.MyMembers.Add(ssgm);
                }

                foreach (var child in group.MyGroups)
                {
                    var childresult = await GetSinSearchGroupResultById(child.Id);
                    ssg.MySINSearchGroup = childresult;
                }
            }

            return ssg;
        }
    }
}

