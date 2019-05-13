using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupPutGroupInGroup")]
        [Authorize(Roles = "Administrator,GroupAdmin")]
        public async Task<ActionResult<ResultGroupPutGroupInGroup>> PutGroupInGroup(Guid GroupId, string groupname, Guid? parentGroupId, string adminIdentityRole, bool isPublicVisible)
        {
            ResultGroupPutGroupInGroup res;
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

                    var e = new HubException(msg);
                    res = new ResultGroupPutGroupInGroup(e);
                    return BadRequest(res);
                }

                SINnerGroup parentGroup = null;
                if (parentGroupId != null)
                {
                    var getParentseq = (from a in _context.SINnerGroups.Include(a => a.MyGroups)
                        where a.Id == parentGroupId
                        select a).Take(1);
                    if (!getParentseq.Any())
                    {
                        var e = new ArgumentException("Parentgroup with Id " + parentGroupId?.ToString() + " not found.");
                        res = new ResultGroupPutGroupInGroup(e);
                        return NotFound(res);
                    }
                    parentGroup = getParentseq.FirstOrDefault();
                }

                SINnerGroup myGroup = null;
                var getGroupseq = (from a in _context.SINnerGroups
                                where a.Id == GroupId
                                   select a).Take(1);
                if (!getGroupseq.Any())
                {
                    var e = new ArgumentException("Group with Id " + parentGroupId.ToString() + " not found.");
                    res = new ResultGroupPutGroupInGroup(e);
                    return NotFound(res);
                }

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
                else
                {
                    myGroup.MyParentGroupId = null;
                }
                
                await _context.SaveChangesAsync();
                if (myGroup.MyParentGroup != null)
                {
                    myGroup.MyParentGroup.PasswordHash = "";
                    myGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }
                myGroup.PasswordHash = "";
                myGroup.MyGroups = RemovePWHashRecursive(myGroup.MyGroups);
                res = new ResultGroupPutGroupInGroup(myGroup);
                return Ok(res);
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
                res = new ResultGroupPutGroupInGroup(e);
                return BadRequest(res);
            }

        }

        // PUT: api/ChummerFiles/5
        /// <summary>
        /// The Xml or Zip File can be uploaded (knowing the previously stored Id)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PutGroupSetting")]
        [Authorize]
        public async Task<ActionResult<ResultGroupPutSetting>> PutGroupSetting([FromRoute] Guid id, IFormFile uploadedFile)
        {
            ResultGroupPutSetting res;
            ApplicationUser user = null;
            SINnerGroup dbgroup = null;
            try
            {
                var group = await _context.SINnerGroups.Include(a => a.MySettings).FirstOrDefaultAsync(a => a.Id == id);
                if (group == null)
                {
                    var e = new ArgumentException("Group with Id " + id + " not found!");
                    res = new ResultGroupPutSetting(e);
                    return NotFound(res);
                }
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);

                if (user == null)
                {
                    var e = new NoUserRightException("User not found!");
                    res = new ResultGroupPutSetting(e);
                    return NotFound(res);
                }

                dbgroup.MySettings.DownloadUrl = Startup.GDrive.StoreXmlInCloud(dbgroup.MySettings, uploadedFile);
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PutStoreXmlInCloud");
                    telemetry.Properties.Add("User", user.Email);
                    telemetry.Properties.Add("SINnerGroupId", dbgroup.Id.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName?.ToString());
                    telemetry.Metrics.Add("FileSize", uploadedFile.Length);
                    tc.TrackEvent(telemetry);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
                try
                {
                    int x = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    res = new ResultGroupPutSetting(e);
                    return Conflict(res);
                }

                res = new ResultGroupPutSetting(dbgroup);
                return Ok(res);
            }
            catch (NoUserRightException e)
            {
                res = new ResultGroupPutSetting(e);
                return BadRequest(res);
            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerGroupId", dbgroup.Id.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName?.ToString());
                    telemetry.Metrics.Add("FileSize", uploadedFile.Length);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultGroupPutSetting(e);
                return BadRequest(res);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupPostGroup")]
        [Authorize]
        public async Task<ActionResult<ResultGroupPostGroup>> PostGroup([FromBody] SINnerGroup mygroup, Guid? SinnerId)
        {
            ResultGroupPostGroup res;
            _logger.LogTrace("Post SINnerGroupInternal: " + mygroup?.Groupname + " (" + SinnerId + ").");
            ApplicationUser user = null;
            var returncode = HttpStatusCode.OK;
            //SINner sinner = null;
            try
            {
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
                        res = new ResultGroupPostGroup(new HubException(msg));
                        return BadRequest(res);
                    }

                    if (mygroup == null)
                    {
                        res = new ResultGroupPostGroup(new HubException("group == null."));
                        return BadRequest(res);
                    }

                    if (String.IsNullOrEmpty(mygroup?.Groupname))
                    {
                        res = new ResultGroupPostGroup(new HubException("Groupname may not be empty."));
                        return BadRequest(res);
                    }

                    SINnerGroup parentGroup = null;

                    
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
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
                            string msg = "A group with the name " + mygroup.Groupname +
                                         " already exists and user is not GroupAdmin or " +
                                         storegroup?.MyAdminIdentityRole + "!";

                            res = new ResultGroupPostGroup(new HubException(msg));
                            return BadRequest(res);
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

                    if (SinnerId != null)
                    {
                        var sinnerseq =
                            await (from a in _context.SINners.Include(b => b.SINnerMetaData.Visibility.UserRights)
                                where a.Id == SinnerId
                                select a).ToListAsync();
                        if (!sinnerseq.Any())
                        {
                            string msg = "Please upload SINner prior to adding him/her to a group!";
                            res = new ResultGroupPostGroup(new HubException(msg));
                            return BadRequest(res);
                        }

                        foreach (var sinner in sinnerseq)
                        {
                            if (sinner.SINnerMetaData.Visibility.UserRights.Any() == false)
                            {
                                res = new ResultGroupPostGroup(new HubException("Sinner  " + sinner.Id + ": Visibility contains no entries!"));
                                return BadRequest(res);
                            }
                            if (sinner.SINnerMetaData.Visibility.Id == null)
                            {
                                sinner.SINnerMetaData.Visibility.Id = Guid.NewGuid();
                            }

                            bool found = false;
                            foreach (var sinur in sinner.SINnerMetaData.Visibility.UserRights)
                            {
                                if (sinur.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())
                                {
                                    if (sinur.CanEdit == true)
                                        found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                string msg = "Sinner " + sinner.Id + " is not editable for user " + user.UserName + ".";

                                res = new ResultGroupPostGroup(new HubException(msg));
                                return BadRequest(res);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is SINner || entry.Entity is Tag || entry.Entity is SINnerGroup)
                        {
                            try
                            {
                                Utils.DbUpdateConcurrencyExceptionHandler(entry, _logger);
                            }
                            catch (Exception e)
                            {
                                res = new ResultGroupPostGroup(e);
                                return BadRequest(res);
                            }
                        }
                        else
                        {
                            var e = new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                            res = new ResultGroupPostGroup(e);
                            return BadRequest(res);
                        }
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                        Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                            new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                        telemetry.Properties.Add("User", user?.Email);
                        tc.TrackException(telemetry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }

                    var re = new ResultGroupPostGroup(e);
                    re.ErrorText = "A group \"" + mygroup.Groupname + "\" for language \"" + mygroup.Language +"\" already exists!";
                    return BadRequest(re);
                }
            res = new ResultGroupPostGroup(mygroup);
            switch (returncode)
            {
                case HttpStatusCode.Accepted:
                    return Accepted("PostGroup", res);
                case HttpStatusCode.Created:
                    return CreatedAtAction("PostGroup", res);
                default:
                    return Ok(res);
                    break;
            }
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
            var re = new ResultGroupPostGroup(e);
            return BadRequest(re);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupPutSINerInGroup")]
        [Authorize]
        public async Task<ActionResult<ResultGroupPutSINerInGroup>> PutSINerInGroup(Guid? GroupId, Guid? SinnerId, string pwhash)
        {
            ResultGroupPutSINerInGroup res;
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

                    res = new ResultGroupPutSINerInGroup(new HubException(msg));
                    return BadRequest(res);
                }
                var roles = await _userManager.GetRolesAsync(user);
                var sin = await PutSiNerInGroupInternal(GroupId, SinnerId, user, _context, _logger, pwhash, roles);
                res = new ResultGroupPutSINerInGroup(sin);
                return Ok(res);
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
                res = new ResultGroupPutSINerInGroup(e);
                res.ErrorText = e.Message;
                return BadRequest(res);
            }

        }

        internal static async Task<SINner> PutSiNerInGroupInternal(Guid? GroupId, Guid? SinnerId, ApplicationUser user, ApplicationDbContext context, ILogger logger, string pwhash, IList<string> userroles)
        {
            try
            {
                SINnerGroup MyTargetGroup = null;
                if (GroupId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(GroupId), "GroupId may not be empty.");
                }

                if (SinnerId == Guid.Empty)
                {
                    throw new ArgumentNullException(nameof(SinnerId), "SinnerId may not be empty.");
                }

                if (GroupId != null)
                {
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

                    MyTargetGroup = groupset.FirstOrDefault();

                    if ((!String.IsNullOrEmpty(MyTargetGroup.PasswordHash))
                        && (MyTargetGroup.PasswordHash != pwhash))
                    {
                        throw new NoUserRightException("PW is wrong!");
                    }

                    if (!String.IsNullOrEmpty(MyTargetGroup.MyAdminIdentityRole))
                    {
                        if (!userroles.Contains(MyTargetGroup.MyAdminIdentityRole))
                        {
                            throw new NoUserRightException("User " + user.UserName + " has not the role " +
                                                           MyTargetGroup.MyAdminIdentityRole + ".");
                        }
                    }
                }

                var sinnerseq = await (from a in context.SINners
                        .Include(a => a.MyGroup)
                        .Include(a => a.MyGroup.MyParentGroup)
                        .Include(a => a.MyGroup.MyParentGroup.MyGroups)
                        .Include(a => a.MyGroup.MyGroups)
                        .Include(a => a.SINnerMetaData)
                        //.Include(a => a.MyExtendedAttributes)
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
                    {
                        if (String.IsNullOrEmpty(sin.DownloadUrl))
                            throw new ArgumentException("Sinner " + sin.Alias + " does not have a DownloadURL!");
                        //if (String.IsNullOrEmpty(sin.MyExtendedAttributes?.JsonSummary))
                        //    throw new ArgumentException("Sinner " + sin.Alias + " does not have a valid JsonSummary!");
                        sin.MyGroup = MyTargetGroup;
                    }
                }

                await context.SaveChangesAsync();
                if (sin?.MyGroup != null)
                {
                    sin.MyGroup.MyGroups = RemovePWHashRecursive(sin.MyGroup.MyGroups);
                    if (sin.MyGroup.MyParentGroup != null)
                    {
                        sin.MyGroup.MyParentGroup.PasswordHash = "";
                        sin.MyGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                    }
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupGetGroupById")]
        [Authorize]
        public async Task<ActionResult<ResultGroupGetGroupById>> GetGroupById(Guid groupid)
        {
            ResultGroupGetGroupById res;
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
                    res = new ResultGroupGetGroupById(new HubException(msg));
                    return BadRequest(res);
                }

                var groupfoundseq = await (from a in _context.SINnerGroups
                        .Include(a => a.MySettings)
                        .Include(a => a.MyGroups)
                        .ThenInclude(b => b.MyGroups)
                        .ThenInclude(b => b.MyGroups)
                    where a.Id == groupid select a).ToListAsync();

                if (!groupfoundseq.Any())
                {
                    var e = new ArgumentException("Could not find group with id " + groupid + ".");
                    res = new ResultGroupGetGroupById(e);
                    return NotFound(res);
                }
                    

                var group = groupfoundseq.FirstOrDefault();
                group.MyGroups = RemovePWHashRecursive(group.MyGroups);
                group.PasswordHash = null;
                if (group.MyParentGroup != null)
                {
                    group.MyParentGroup.PasswordHash = "";
                    group.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }
                res = new ResultGroupGetGroupById(group);
                return Ok(res);

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
                res = new ResultGroupGetGroupById(e);
                return BadRequest(res);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupGetPublicGroup")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetPublicGroup(string Groupname, string Language)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            ResultGroupGetSearchGroups res = null;
            _logger.LogTrace("GetPublicGroup: " + Groupname + ".");
            try
            {
                var ssg = await GetSearchGroupsInternal(Groupname, null, null, Language);
                res = new ResultGroupGetSearchGroups(ssg);
                return Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);

                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                        new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                res = new ResultGroupGetSearchGroups(e);
                return BadRequest(res);
            }
            finally
            {
                Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry telemetry =
                    new Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry("GetPublicGroup",
                        DateTimeOffset.Now, sw.Elapsed, "Azure", res?.CallSuccess ?? false, res?.ErrorText);
                tc.TrackAvailability(telemetry);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupGetGroupmembers")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetGroupmembers(string Groupname, string Language, string email, string password)
        {
            ResultGroupGetSearchGroups res;
            _logger.LogTrace("GetGroupmembers: " + Groupname + "/" + Language + "/" + email + ".");
            try
            {
                var r = await GetGroupmembersInternal(Groupname, Language, email, password);
                res = new ResultGroupGetSearchGroups(r);
                return Ok(res);
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
                res = new ResultGroupGetSearchGroups(e);
                return BadRequest(res);
            }
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private async Task<SINSearchGroupResult> GetGroupmembersInternal(string Groupname, string language, string email, string password)
        {
            try
            {
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
                        throw new ArgumentException("No group found with the given parameter: " + Groupname);
                    }
                }

                var user = await _userManager.FindByEmailAsync(email);
                foreach (var groupid in groupfoundseq)
                {
                    //check for the password
                    var group = await _context.SINnerGroups.FindAsync(groupid);
                    if (group == null)
                        continue;
                    if (group.HasPassword)
                    {
                        string pwhash = GetHashString(password);
                        if (!group.PasswordHash.Equals(password))
                        {
                            throw new ArgumentException("Wrong password provided for group: " + Groupname);
                        }
                    }
                    var ssg = await GetSinSearchGroupResultById(groupid, user, true);
                    result.SINGroups.Add(ssg);
                }

                result.SINGroups = RemovePWHashRecursive(result.SINGroups);

                return result;

            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("Groupname", Groupname?.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                throw;
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupGetSearchGroups")]
        [Authorize]
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetSearchGroups(string Groupname, string UsernameOrEmail, string SINnerName, string Language)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            ResultGroupGetSearchGroups res = null;
            var user = await _signInManager.UserManager.GetUserAsync(User);
            _logger.LogTrace("GetSearchGroups: " + Groupname + "/" + UsernameOrEmail + "/" + SINnerName + ".");
            string teststring = "not set";
            try
            {
                var r =  await GetSearchGroupsInternal(Groupname, UsernameOrEmail, SINnerName, Language);
                res = new ResultGroupGetSearchGroups(r);
                teststring = Newtonsoft.Json.JsonConvert.SerializeObject(res);
                var returnObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResultGroupGetSearchGroups>(teststring);
                try
                {
                    
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("GroupGetSearchGroups");
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("JSON", teststring);
                    telemetry.Properties.Add("Object", res.ToString());
                    tc.TrackEvent(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                return Ok(res);
            }
            catch(Exception e)
            {
                try
                {
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultGroupGetSearchGroups(e);
                return BadRequest(res);
            }
            finally
            {
                Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry telemetry =
                    new Microsoft.ApplicationInsights.DataContracts.AvailabilityTelemetry("GetSearchGroups",
                        DateTimeOffset.Now, sw.Elapsed, "Azure", res?.CallSuccess ?? false, res?.ErrorText);
                tc.TrackAvailability(telemetry);
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

            var members = await group.GetGroupMembers(_context, false);

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

        private async Task<SINSearchGroupResult> GetSearchGroupsInternal(string Groupname, string UsernameOrEmail, string sINnerName, string language)
        {
            ApplicationUser user = null;
            try
            {
                using (var t = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted

                    }, TransactionScopeAsyncFlowOption.Enabled))
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

                        throw new HubException(msg);
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
                            throw new ArgumentException("No group found with the given parameter: " + Groupname);
                        }
                    }
                    else if (String.IsNullOrEmpty(UsernameOrEmail) && String.IsNullOrEmpty(sINnerName))
                    {
                        groupfoundseq = await (from a in _context.SINnerGroups
                            where a.IsPublic == true && a.MyParentGroupId == null
                            select a.Id).ToListAsync();
                        if (!groupfoundseq.Any())
                        {
                            throw new ArgumentException("No group found with the given parameter IsPublic");
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

                    //now add owned SINners
                    SINnerSearchGroup ownedGroup = new SINnerSearchGroup()
                    {
                        Groupname = "My Data (virtual Group)",
                        Description = "This isn't a group, but only a list of all the Chummers you \"own\"."
                                      + Environment.NewLine +
                                      "You can't delete this fictional group or remove your Chummers from here."
                                      + Environment.NewLine +
                                      "But you can drag'n'drop from here to have a Chummer of yours join another group."
                    };
                    result.SINGroups.Add(ownedGroup);
                    List<SINner> mySinners;
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains("SeeAllSInners"))
                        mySinners = await SINner.GetSINnersFromUser(user, _context, true);
                    else
                    {
                        mySinners = await _context.SINners.Include(a => a.MyGroup)
                            .Include(a => a.SINnerMetaData.Visibility.UserRights)
                            //.Include(a => a.MyExtendedAttributes)
                            .Include(a => a.SINnerMetaData)
                            .Include(a => a.SINnerMetaData.Visibility)
                            .ToListAsync();
                    }

                    foreach (var ownedSin in mySinners)
                    {
                        SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember
                        {
                            MySINner = ownedSin,
                            Username = user.UserName
                        };
                        ownedGroup.MyMembers.Add(ssgm);
                    }

                    //add the users own groups - always!
                    var userownedgroupsseq = await SINner.GetSINnersFromUser(user, _context, true);
                    List<Guid?> usergroups = new List<Guid?>();
                    foreach (var sin in userownedgroupsseq)
                    {
                        if (sin.MyGroup != null)
                        {
                            if (!usergroups.Contains(sin.MyGroup.Id))
                                usergroups.Add(sin.MyGroup.Id);
                        }
                    }

                    foreach (var groupid in usergroups)
                    {
                        var ssg = await GetSinSearchGroupResultById(groupid, user);
                        ownedGroup.MySINSearchGroups.Add(ssg);
                    }

                    result.SINGroups = RemovePWHashRecursive(result.SINGroups);
                    t.Complete();
                    return result;
                }

               

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
                throw;
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetGroupmembersById")]
        [Authorize]
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetGroupmembersById(Guid groupid)
        {
            ResultGroupGetSearchGroups res;
            _logger.LogTrace("GetGroupmembersById: " + groupid + ".");

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
                    res = new ResultGroupGetSearchGroups(new HubException(msg));
                    return BadRequest(res);
                }

                SINSearchGroupResult result = new SINSearchGroupResult();
                var range = await GetSinSearchGroupResultById(groupid, user);
                result.SINGroups.Add(range);
                result.SINGroups = RemovePWHashRecursive(result.SINGroups);
                res = new ResultGroupGetSearchGroups(result);
                Ok(res);
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
                res = new ResultGroupGetSearchGroups(e);
                return BadRequest(res);
            }

            return NotFound(res);
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

        private async Task<SINnerSearchGroup> GetSinSearchGroupResultById(Guid? groupid, ApplicationUser askingUser,
            bool addTags = false)
        {
            if ((groupid == null) || (groupid == Guid.Empty))
                throw new ArgumentNullException(nameof(groupid));
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted

                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                ApplicationUser user = null;
                if (User != null)
                    user = await _signInManager.UserManager.GetUserAsync(User);
                SINnerSearchGroup ssg = null;
                var groupbyidseq = await (from a in _context.SINnerGroups
                        //.Include(a => a.MyParentGroup)
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

                    var members = await ssg.GetGroupMembers(_context, addTags);
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
                t.Complete();
                return ssg;
                
            }
            
        }
    }
}

