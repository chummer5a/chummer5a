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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                    return BadRequest(new HubException(msg));
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
                if (myGroup.MyParentGroup != null)
                {
                    myGroup.MyParentGroup.PasswordHash = "";
                    myGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }
                myGroup.PasswordHash = "";
                myGroup.MyGroups = RemovePWHashRecursive(myGroup.MyGroups);
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
                return BadRequest(hue);
            }

        }

       

        /// <summary>
            /// Store the new group
            /// </summary>
            /// <param name="mygroup"></param>
            /// <param name="SinnerId"></param>
            /// <returns></returns>
            [HttpPost()]
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK, "")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted, "Group existed", typeof(SINnerGroup))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created, "Group created", typeof(SINnerGroup))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest, "an error occured", typeof(HubException))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict, "an error occured", typeof(HubException))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PostGroup")]
        [Authorize]
        public async Task<IActionResult> PostGroup([FromBody] SINnerGroup mygroup, Guid SinnerId)
        {
            _logger.LogTrace("Post SINnerGroupInternal: " + mygroup?.Groupname + " (" + SinnerId + ").");
            ApplicationUser user = null;
            //SINner sinner = null;
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
                    return BadRequest(new HubException(msg));
                }

                if (mygroup == null)
                {
                    return BadRequest(new HubException("group == null."));
                }
                if (String.IsNullOrEmpty(mygroup?.Groupname))
                {
                    return BadRequest(new HubException("Groupname may not be empty."));
                }

                if (SinnerId == Guid.Empty)
                {
                    return BadRequest(new HubException("SinnerId may not be empty."));
                }

                SINnerGroup parentGroup = null;
                
                var returncode = HttpStatusCode.OK;
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sinnerseq = await (from a in _context.SINners.Include(b => b.SINnerMetaData.Visibility.UserRights) where a.Id == SinnerId select a).ToListAsync();
                if (!sinnerseq.Any())
                {
                    string msg = "Please upload SINner prior to adding him/her to a group!";
                    return BadRequest(new HubException(msg));
                }
                foreach(var sinner in sinnerseq)
                {
                    if(sinner.SINnerMetaData.Visibility.UserRights.Any() == false)
                    {
                        return BadRequest(new HubException("Sinner  " + sinner.Id + ": Visibility contains no entries!"));
                    }

                    if(sinner.LastChange == null)
                    {
                        return BadRequest(new HubException("Sinner  " + sinner.Id + ": LastChange not set!"));
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
                        return BadRequest(new HubException(msg));
                    }

                    List<SINnerGroup> groupfoundseq;
                    if (mygroup.Id == null || mygroup.Id == Guid.Empty)
                    {
                        groupfoundseq = await (from a in _context.SINnerGroups
                            where a.Id == mygroup.Id
                            select a).Take(1).ToListAsync();
                    }
                    else
                    {
                        groupfoundseq = await (from a in _context.SINnerGroups
                            where a.Groupname == mygroup.Groupname
                                  && a.Language == mygroup.Language
                            select a).Take(1).ToListAsync();
                    }

                    SINnerGroup storegroup = null;
                    if ((groupfoundseq.Any()))
                    {
                        storegroup = groupfoundseq.FirstOrDefault();
                        user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                        var roles = await _userManager.GetRolesAsync(user);
                        if (!roles.Contains("GroupAdmin") || roles.Contains(storegroup?.MyAdminIdentityRole))
                        {
                            string msg = "A group with the name " + mygroup.Groupname + " already exists and user is not GroupAdmin or "+ storegroup?.MyAdminIdentityRole + "!";
                            return BadRequest(new HubException(msg));
                        }
                        
                    }
                    
                    if (storegroup == null)
                    {
                        if (mygroup.Id == null || mygroup.Id == Guid.Empty)
                        {
                            mygroup.Id = Guid.NewGuid();
                            mygroup.GroupCreatorUserName = user.UserName;
                        }
                        mygroup.MyParentGroup = parentGroup;
                        parentGroup?.MyGroups.Add(mygroup);
                        _context.SINnerGroups.Add(mygroup);
                        returncode = HttpStatusCode.Created;
                    }
                    else
                    {
                        returncode = HttpStatusCode.Accepted;
                        if ((String.IsNullOrEmpty(mygroup.MyAdminIdentityRole))
                            && (!String.IsNullOrEmpty(storegroup.MyAdminIdentityRole)))
                        {
                            mygroup.MyAdminIdentityRole = storegroup.MyAdminIdentityRole;
                        }
                        _context.Entry(storegroup).CurrentValues.SetValues(mygroup);
                    }

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
                    case HttpStatusCode.Accepted:
                        return Accepted("PostGroup", mygroup);
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostGroup", mygroup);
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
                    telemetry.Properties.Add("Groupname", mygroup?.Groupname?.ToString());
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in PostGroup: " + e.Message, e);
                return BadRequest(hue);
              
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK, "SINner joined the group", typeof(SINner))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest, "an error occured", typeof(HubException))]
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
                    return BadRequest(new HubException(msg));
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in PutSINerInGroup: " + e.Message, e);
                return BadRequest(hue);
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

                var groupset = await (from a in context.SINnerGroups
                        .Include(a => a.MySettings)
                        .Include(a => a.MyParentGroup)
                        .Include(a => a.MyParentGroup.MyGroups)
                        .Include(a => a.MyGroups)
                        .ThenInclude(a => a.MyGroups)
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
                        .Include(a => a.MyGroup.MyParentGroup)
                        .Include(a => a.MyGroup.MyParentGroup.MyGroups)
                        .Include(a => a.MyGroup.MyGroups)
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
                sin.MyGroup.MyGroups = RemovePWHashRecursive(sin.MyGroup.MyGroups);
                if (sin.MyGroup.MyParentGroup != null)
                {
                    sin.MyGroup.MyParentGroup.PasswordHash = "";
                    sin.MyGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }

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
                throw hue;
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
                    return BadRequest(new HubException(msg));
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
                group.MyGroups = RemovePWHashRecursive(group.MyGroups);
                group.PasswordHash = null;
                if (group.MyParentGroup != null)
                {
                    group.MyParentGroup.PasswordHash = "";
                    group.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }

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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in GetGroupById: " + e.Message, e);
                return BadRequest(hue);

            }
        }

        /// <summary>
        /// Search for public Groups (without authorization)
        /// </summary>
        /// <param name="Groupname"></param>
        /// <param name="Language"></param>
        /// <returns>SINSearchGroupResult</returns>
        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetPublicGroup")]
        [AllowAnonymous]
        public async Task<ActionResult<SINSearchGroupResult>> GetPublicGroup(string Groupname, string Language)
        {
            _logger.LogTrace("GetPublicGroup: " + Groupname + ".");
            try
            {
                return await GetSearchGroupsInternal(Groupname, null, null, Language);
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return BadRequest(hue);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK, "Groups found", typeof(SINSearchGroupResult))]
        [ProducesResponseType(typeof(SINSearchGroupResult), StatusCodes.Status200OK )]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest, "an error occured", typeof(HubException))]
        [ProducesResponseType(typeof(HubException), StatusCodes.Status400BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound, "Group not found")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetSearchGroups")]
        [Authorize]
        public async Task<ActionResult<SINSearchGroupResult>> GetSearchGroups(string Groupname, string UsernameOrEmail, string SINnerName, string Language)
        {
            _logger.LogTrace("GetSearchGroups: " + Groupname + "/" + UsernameOrEmail + "/" + SINnerName + ".");
            try
            {
                return await GetSearchGroupsInternal(Groupname, UsernameOrEmail, SINnerName, Language);
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return BadRequest(hue);
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in DeleteLeaveGroup: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        /// <summary>
        /// Delete a Group (recursive - only Admins can do that)
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpDelete()]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("DeleteGroup")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteGroup(Guid groupid)
        {
            _logger.LogTrace("DeleteLeaveGroup: " + groupid +  ".");
            try
            {
                var group =  await DeleteGroupInternal(groupid);
                _context.SINnerGroups.Remove(group);
                await _context.SaveChangesAsync();
                return Ok(true);

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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in DeleteLeaveGroup: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private async Task<SINnerGroup> DeleteGroupInternal(Guid? groupid)
        {
            if ((groupid == null) || (groupid == Guid.Empty))
                throw new ArgumentNullException(nameof(groupid));
            var groupbyidseq = await(from a in _context.SINnerGroups
                    .Include(a => a.MyGroups)
                where a.Id == groupid
                select a).Take(1).ToListAsync();
            if (!groupbyidseq.Any())
                return null;

            var mygroup = groupbyidseq.FirstOrDefault();

            ApplicationUser user = await _signInManager.UserManager.GetUserAsync(User);
            if (user == null)
                throw new NoUserRightException("Could not verify ApplicationUser!");

            bool candelete = false;
            if (mygroup.IsPublic == false)
            {
                if (mygroup.GroupCreatorUserName != user.UserName)
                {
                    throw new NoUserRightException("Only " + mygroup.GroupCreatorUserName + " can delete this group.");
                }
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);
                if ((roles.Contains("GroupAdmin")) || roles.Contains(mygroup.MyAdminIdentityRole))
                {
                    candelete = true;
                }
                else
                {
                    throw new NoUserRightException("Group is public - can only be deleted by GroupAdmins or " + mygroup.MyAdminIdentityRole + ".");
                }
            }

            var members = (from a in _context.SINners where a.MyGroup == mygroup select a).ToList();
            foreach (var member in members)
            {
                member.MyGroup = null;
                _context.Entry(member).CurrentValues.SetValues(member);
            }

            await _context.SaveChangesAsync();
            foreach (var childgroup in mygroup.MyGroups)
            {
                var result = await DeleteGroupInternal(childgroup.Id);
                if (result == null)
                {
                    throw new ArgumentException("Could not delete (child-)group " + childgroup.Groupname + " (Id: " +
                                                childgroup.Id + ").");
                }
                _context.SINnerGroups.Remove(result);
            }
            await _context.SaveChangesAsync();
            return mygroup;
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

        private async Task<ActionResult<SINSearchGroupResult>> GetSearchGroupsInternal(string Groupname, string UsernameOrEmail, string sINnerName, string language)
        {
            ApplicationUser user = null;
            try
            {
                if (User != null)
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

                    return BadRequest(new HubException(msg));
                }

                SINSearchGroupResult result = new SINSearchGroupResult();

                List<Guid?> groupfoundseq = new List<Guid?>();
                if (!String.IsNullOrEmpty(Groupname))
                {
                    groupfoundseq = await (from a in _context.SINnerGroups
                                           where a.Groupname.ToLowerInvariant().Contains(Groupname.ToLowerInvariant())
                                           && (a.Language == language || String.IsNullOrEmpty(language))
                                           select a.Id).ToListAsync();
                    if (!groupfoundseq.Any())
                    {
                        return NotFound();
                    }
                }
                else if (String.IsNullOrEmpty(UsernameOrEmail) && String.IsNullOrEmpty(sINnerName))
                {
                    groupfoundseq = await (from a in _context.SINnerGroups
                                           where a.IsPublic == true && a.MyParentGroupId == null
                                           select a.Id).ToListAsync();
                    if (!groupfoundseq.Any())
                    {
                        return NotFound();
                    }
                }
                

                foreach (var groupid in groupfoundseq)
                {
                    var ssg = await GetSinSearchGroupResultById(groupid, user);
                    result.SINGroups.Add(ssg);
                }

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
                         
                            SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember
                            {
                                MySINner = sin
                            };
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return BadRequest(hue);

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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetGroupMembers")]
        [Authorize]
        public async Task<ActionResult<SINSearchGroupResult>> GetGroupMembers(Guid groupid)
        {
            _logger.LogTrace("GetGroupMembers: " + groupid + ".");

            ApplicationUser user = null;
            
            try
            {
                if (User != null)
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

                    return BadRequest(new HubException(msg));
                }

                SINSearchGroupResult result = new SINSearchGroupResult();
                var range = await GetSinSearchGroupResultById(groupid, user);
                result.SINGroups.Add(range);
                result.SINGroups = RemovePWHashRecursive(result.SINGroups);
                Ok(result);
            }
            catch (Exception e)
            {
                try
                {
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in GetSearchGroups: " + e.Message, e);
                return BadRequest(hue);
            }

            return BadRequest();
        }

        private static List<SINnerSearchGroup> RemovePWHashRecursive(List<SINnerSearchGroup> sINGroups)
        {
            if (sINGroups == null)
                return new List<SINnerSearchGroup>();
            foreach(var group in sINGroups)
            {
                group.PasswordHash = "";
                group.MyGroups = RemovePWHashRecursive(group.MyGroups);
            }
            return sINGroups;
        }

        private static List<SINnerGroup> RemovePWHashRecursive(List<SINnerGroup> sINGroups)
        {
            if (sINGroups == null)
                return new List<SINnerGroup>();
            foreach (var group in sINGroups)
            {
                group.PasswordHash = "";
                group.MyGroups = RemovePWHashRecursive(group.MyGroups);
            }
            return sINGroups;
        }

        private async Task<SINnerSearchGroup> GetSinSearchGroupResultById(Guid? groupid, ApplicationUser askingUser)
        {
            if ((groupid == null) || (groupid == Guid.Empty))
                throw new ArgumentNullException(nameof(groupid));
            ApplicationUser user = null;
            if (User != null)
                user = await _signInManager.UserManager.GetUserAsync(User);
            SINnerSearchGroup ssg = null;
            var groupbyidseq = await (from a in _context.SINnerGroups
                    .Include(a => a.MyParentGroup)
                    .Include(a => a.MyGroups)
                    .Include(a => a.MySettings)
                    .Include(a => a.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    where a.Id == groupid
                select a).Take(1).ToListAsync();
            
            foreach (var group in groupbyidseq)
            {
                if (group.MyGroups == null)
                    group.MyGroups = new List<SINnerGroup>();
                ssg = new SINnerSearchGroup(group);

                var members = await ssg.GetGroupMembers(_context);
                foreach (var member in members)
                {
                    member.MyGroup = null;
                    SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember
                    {
                        MySINner = member
                    };
                    ssg.MyMembers.Add(ssgm);
                }

                foreach (var child in group.MyGroups)
                {
                    bool okToShow = false;
                    if ((child.IsPublic == false) && user == null)
                    {
                        continue;
                    }
                    else if (child.IsPublic == false && user != null)
                    {
                        //check if the user has the right to see this group
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains(child.MyAdminIdentityRole) == true)
                        {
                            okToShow = true;
                        }
                        else
                        {
                            //check if the user has a chummer that is part of the group
                            //maybe later
                        }
                    }
                    else if (child.IsPublic == true)
                    {
                        okToShow = true;
                    }

                    if (okToShow)
                    {
                        var childresult = await GetSinSearchGroupResultById(child.Id, user);
                        ssg.MySINSearchGroups.Add(childresult);
                    }
                    
                }
            }
            ssg.MyGroups = RemovePWHashRecursive(ssg.MyGroups);
            return ssg;
        }
    }
}

