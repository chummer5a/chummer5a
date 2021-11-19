using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using SINnerGroup = ChummerHub.Models.V1.SINnerGroup;

//using Swashbuckle.AspNetCore.Filters;

namespace ChummerHub.Controllers.V1
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [ControllerName("SINGroup")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController'
    public class SINnerGroupController : ControllerBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController'
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TelemetryClient tc;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.SINnerGroupController(ApplicationDbContext, ILogger<SINnerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
        public SINnerGroupController(ApplicationDbContext context,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.SINnerGroupController(ApplicationDbContext, ILogger<SINnerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
            ILogger<SINnerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, TelemetryClient telemetry)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            tc = telemetry;
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.~SINnerGroupController()'
        ~SINnerGroupController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.~SINnerGroupController()'
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
#pragma warning disable CS1573 // Parameter 'isPublicVisible' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'adminIdentityRole' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'parentGroupId' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
        public async Task<ActionResult<ResultGroupPutGroupInGroup>> PutGroupInGroup(Guid GroupId, string groupname, Guid? parentGroupId, string adminIdentityRole, bool isPublicVisible)
#pragma warning restore CS1573 // Parameter 'parentGroupId' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'adminIdentityRole' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'isPublicVisible' has no matching param tag in the XML comment for 'SINnerGroupController.PutGroupInGroup(Guid, string, Guid?, string, bool)' (but other parameters do)
        {
            ResultGroupPutGroupInGroup res;
            _logger.LogTrace("PutGroupInGroup called with GroupId: " + GroupId + " and ParentGroupId: " + parentGroupId + " - adminIdentityRole: " + adminIdentityRole + ".");
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
                            msg += Environment.NewLine + "\t" + singleerr;
                        }

                    }

                    var e = new HubException(msg);
                    res = new ResultGroupPutGroupInGroup(e);
                    return BadRequest(res);
                }

                SINnerGroup myGroup = await _context.SINnerGroups.FirstOrDefaultAsync(a => a.Id == GroupId);
                if (myGroup == null)
                {
                    var e = new ArgumentException("Group with Id " + GroupId.ToString() + " not found.");
                    res = new ResultGroupPutGroupInGroup(e);
                    return NotFound(res);
                }

                SINnerGroup returnGroup = myGroup;
                bool onlyFavremoval = false;
                SINnerGroup parentGroup = null;
                if (parentGroupId != null)
                {
                    if (parentGroupId == Guid.Empty)
                    {
                        //only make this group a favorite group of the user and return
                        if (user.FavoriteGroups.All(a => a.FavoriteGuid != GroupId))
                        {
                            user.FavoriteGroups.Add(new ApplicationUserFavoriteGroup
                            {
                                FavoriteGuid = GroupId
                            });
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        parentGroup = await _context.SINnerGroups.FirstOrDefaultAsync(a => a.Id == parentGroupId);
                        if (parentGroup == null)
                        {
                            var e = new ArgumentException("Parentgroup with Id " + parentGroupId?.ToString() +
                                                          " not found.");
                            res = new ResultGroupPutGroupInGroup(e);
                            return NotFound(res);
                        }

                        returnGroup = parentGroup;
                    }
                }
                else
                {
                    if (user.FavoriteGroups.Any(a => a.FavoriteGuid == GroupId))
                    {
                        var removefav = user.FavoriteGroups.FirstOrDefault(a => a.FavoriteGuid == GroupId);
                        if (removefav != null)
                        {
                            user.FavoriteGroups.Remove(removefav);
                            onlyFavremoval = true;
                        }

                    }
                }

                myGroup.Groupname = groupname;
                myGroup.IsPublic = isPublicVisible;
                myGroup.MyAdminIdentityRole = adminIdentityRole;
                if (!onlyFavremoval)
                {
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
                }

                await _context.SaveChangesAsync();

                returnGroup = await _context.SINnerGroups.Include(a => a.MyGroups)
                    .FirstOrDefaultAsync(b => b.Id == returnGroup.Id);


                if (returnGroup.MyParentGroup != null)
                {
                    returnGroup.MyParentGroup.PasswordHash = string.Empty;
                    returnGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }
                returnGroup.PasswordHash = string.Empty;
                if (returnGroup.MyGroups == null)
                    returnGroup.MyGroups = new List<SINnerGroup>();
                else
                    RemovePWHashRecursive(returnGroup.MyGroups);
                res = new ResultGroupPutGroupInGroup(returnGroup);
                var logmessage = JsonConvert.SerializeObject(res, Formatting.Indented);
                logmessage = "PutGroupInGroup returns Object ResultGroupPutGroupInGroup: " + Environment.NewLine +
                             logmessage;
                _logger.LogDebug(logmessage);
                return Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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
                dbgroup = await _context.SINnerGroups.Include(a => a.MySettings).FirstOrDefaultAsync(a => a.Id == id);
                if (dbgroup == null)
                {
                    var e = new ArgumentException("Group with Id " + id + " not found!");
                    res = new ResultGroupPutSetting(e);
                    return NotFound(res);
                }
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);

                if (user == null)
                {
                    var e = new NoUserRightException("User not found!");
                    res = new ResultGroupPutSetting(e);
                    return NotFound(res);
                }

                dbgroup.MySettings.DownloadUrl = Startup.GDrive.StoreXmlInCloud(dbgroup.MySettings, uploadedFile);
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PutStoreXmlInCloud");
                    telemetry.Properties.Add("User", user.Email);
                    telemetry.Properties.Add("SINnerGroupId", dbgroup.Id.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName);
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
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerGroupId", dbgroup?.Id.ToString() ?? string.Empty);
                    telemetry.Properties.Add("FileName", uploadedFile.FileName);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(statusCode: (int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(statusCode: (int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(statusCode: (int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(statusCode: (int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(summary: "GroupPostGroup")]
        [Authorize]
        public async Task<ActionResult<ResultGroupPostGroup>> PostGroup([FromBody] SINnerGroup mygroup, Guid? SinnerId)
        {
            _logger.LogTrace(message: "Post SINnerGroupInternal: " + mygroup?.Groupname + " (" + SinnerId + ").");
            ApplicationUser user = null;
            var returncode = HttpStatusCode.OK;
            //SINner sinner = null;
            try
            {
                ResultGroupPostGroup res;
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Select(selector: x => x.Value.Errors)
                            .Where(predicate: y => y.Count > 0)
                            .ToList();
                        string msg = "ModelState is invalid: ";
                        foreach (var err in errors)
                        {
                            foreach (var singleerr in err)
                            {
                                msg += Environment.NewLine + "\t" + singleerr;
                            }

                        }
                        res = new ResultGroupPostGroup(e: new HubException(message: msg));
                        return BadRequest(error: res);
                    }

                    if (mygroup == null)
                    {
                        res = new ResultGroupPostGroup(e: new HubException(message: "group == null."));
                        return BadRequest(error: res);
                    }

                    if (string.IsNullOrEmpty(value: mygroup.Groupname))
                    {
                        res = new ResultGroupPostGroup(e: new HubException(message: "Groupname may not be empty."));
                        return BadRequest(error: res);
                    }

                    SINnerGroup parentGroup = null;


                    user = await _signInManager.UserManager.FindByNameAsync(userName: User.Identity?.Name ?? string.Empty);
                    SINnerGroup storegroup = mygroup.Id != null && mygroup.Id != Guid.Empty
                        ? await _context.SINnerGroups.FirstOrDefaultAsync(a => a.Id == mygroup.Id)
                        : await _context.SINnerGroups.FirstOrDefaultAsync(a => a.Groupname == mygroup.Groupname && a.Language == mygroup.Language);

                    if (storegroup != null)
                    {
                        user = await _signInManager.UserManager.FindByNameAsync(userName: User.Identity?.Name ?? string.Empty);
                        var roles = await _userManager.GetRolesAsync(user: user);
                        if (!roles.Contains(item: "GroupAdmin") || roles.Contains(item: storegroup.MyAdminIdentityRole))
                        {

                            string msg = "A group with the name " + mygroup.Groupname +
                                         " already exists! (Multiple groups with the same name can only be created by Admins, because they should know what the do)";
                            res = new ResultGroupPostGroup(e: new HubException(message: msg));
                            return BadRequest(error: res);
                        }
                        returncode = HttpStatusCode.Accepted;
                        if (string.IsNullOrEmpty(value: mygroup.MyAdminIdentityRole)
                            && !string.IsNullOrEmpty(value: storegroup.MyAdminIdentityRole))
                        {
                            mygroup.MyAdminIdentityRole = storegroup.MyAdminIdentityRole;
                        }

                        _context.Entry(entity: storegroup).CurrentValues.SetValues(obj: mygroup);
                    }
                    else
                    {
                        if (mygroup.Id == null || mygroup.Id == Guid.Empty)
                        {
                            mygroup.Id = Guid.NewGuid();
                            mygroup.GroupCreatorUserName = user.UserName;
                        }

                        mygroup.MyParentGroup = null; //parentGroup;
                        //parentGroup?.MyGroups.Add(item: mygroup);
                        await _context.SINnerGroups.AddAsync(entity: mygroup);
                        returncode = HttpStatusCode.Created;
                    }

                    if (mygroup.Id != null)
                    {
                        if (user.FavoriteGroups.All(predicate: a => a.FavoriteGuid != mygroup.Id.Value))
                            user.FavoriteGroups.Add(item: new ApplicationUserFavoriteGroup()
                            {
                                FavoriteGuid = mygroup.Id.Value
                            });
                    }

                    if (SinnerId != null)
                    {
                        var sinnerseq =
                            await _context.SINners.Include(navigationPropertyPath: b => b.SINnerMetaData.Visibility.UserRights).Where(a => a.Id == SinnerId).ToListAsync();
                        if (sinnerseq.Count == 0)
                        {
                            string msg = "Please upload SINner prior to adding him/her to a group!";
                            res = new ResultGroupPostGroup(e: new HubException(message: msg));
                            return BadRequest(error: res);
                        }

                        foreach (var sinner in sinnerseq)
                        {
                            if (sinner.SINnerMetaData.Visibility.UserRights.Count == 0)
                            {
                                res = new ResultGroupPostGroup(e: new HubException(message: "Sinner  " + sinner.Id + ": Visibility contains no entries!"));
                                return BadRequest(error: res);
                            }
                            if (sinner.SINnerMetaData.Visibility.Id == null)
                            {
                                sinner.SINnerMetaData.Visibility.Id = Guid.NewGuid();
                            }

                            bool found = false;
                            foreach (var sinur in sinner.SINnerMetaData.Visibility.UserRights)
                            {
                                if (sinur.EMail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (sinur.CanEdit)
                                        found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                string msg = "Sinner " + sinner.Id + " is not editable for user " + user.UserName + ".";

                                res = new ResultGroupPostGroup(e: new HubException(message: msg));
                                return BadRequest(error: res);
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
                                Utils.DbUpdateExceptionHandler(entry: entry, logger: _logger);
                            }
                            catch (Exception e)
                            {
                                res = new ResultGroupPostGroup(e: e);
                                return BadRequest(error: res);
                            }
                        }
                        else
                        {
                            var e = new NotSupportedException(
                                message: "Don't know how to handle concurrency conflicts for "
                                         + entry.Metadata.Name);
                            res = new ResultGroupPostGroup(e: e);
                            return BadRequest(error: res);
                        }
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                        Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                            new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(exception: e);
                        telemetry.Properties.Add(key: "User", value: user?.Email);
                        tc.TrackException(telemetry: telemetry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(message: ex.ToString());
                    }

                    var re = new ResultGroupPostGroup(e: e)
                    {
                        ErrorText = "A group \"" + mygroup.Groupname + "\" for language \"" + mygroup.Language + "\" already exists!"
                    };
                    return BadRequest(error: re);
                }
                res = new ResultGroupPostGroup(mygroup);
                switch (returncode)
                {
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.Created:
                        return Accepted(uri: "PostGroup", value: res);
                    default:
                        return Ok(value: res);
                }
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(exception: e);
                    telemetry.Properties.Add(key: "User", value: user?.Email);
                    telemetry.Properties.Add(key: "Groupname", value: mygroup?.Groupname);
                    tc.TrackException(telemetry: telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: ex.ToString());
                }
                var re = new ResultGroupPostGroup(e: e);
                return BadRequest(error: re);
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
                            msg += Environment.NewLine + "\t" + singleerr;
                        }

                    }

                    res = new ResultGroupPutSINerInGroup(new HubException(msg));
                    return BadRequest(res);
                }
                var roles = await _userManager.GetRolesAsync(user);
                var sin = await PutSiNerInGroupInternal(GroupId, SinnerId, user, _context, _logger, pwhash, roles, tc);
                res = new ResultGroupPutSINerInGroup(sin);
                return Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("GroupId", GroupId.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultGroupPutSINerInGroup(e)
                {
                    ErrorText = e.Message
                };
                return BadRequest(res);
            }

        }

        internal static async Task<SINner> PutSiNerInGroupInternal(Guid? GroupId, Guid? SinnerId, ApplicationUser user, ApplicationDbContext context, ILogger logger, string pwhash, IList<string> userroles, TelemetryClient tc)
        {
            try
            {
                SINnerGroup MyTargetGroup = null;

                if (SinnerId == Guid.Empty || SinnerId == null)
                {
                    throw new ArgumentNullException(nameof(SinnerId), "SinnerId may not be empty.");
                }
                if (GroupId == Guid.Empty)
                {
                    if (user.FavoriteGroups.All(a => a.FavoriteGuid != SinnerId.Value))
                        user.FavoriteGroups.Add(new ApplicationUserFavoriteGroup()
                        {
                            FavoriteGuid = SinnerId.Value
                        });
                }
                else if (GroupId == null)
                {
                    user.FavoriteGroups.RemoveAll(a => a.FavoriteGuid == SinnerId);
                }
                else
                {
                    var targetgroup = await context.SINnerGroups.Include(a => a.MySettings)
                        .Include(a => a.MyParentGroup)
                        .Include(a => a.MyParentGroup.MyGroups)
                        .Include(a => a.MyGroups)
                        .ThenInclude(a => a.MyGroups)
                        .FirstOrDefaultAsync(a => a.Id == GroupId);

                    MyTargetGroup = targetgroup ?? throw new ArgumentException("GroupId not found", nameof(GroupId));

                    if (!string.IsNullOrEmpty(MyTargetGroup.PasswordHash)
                        && MyTargetGroup.PasswordHash != pwhash)
                    {
                        throw new NoUserRightException("PW is wrong!");
                    }
                    if (!string.IsNullOrEmpty(MyTargetGroup.MyAdminIdentityRole))
                    {
                        if (!userroles.Contains(MyTargetGroup.MyAdminIdentityRole))
                        {
                            throw new NoUserRightException("User " + user?.UserName + " has not the role " +
                                                           MyTargetGroup.MyAdminIdentityRole + ".");
                        }
                    }

                    if (MyTargetGroup.Id != null && user != null)
                    {
                        if (user.FavoriteGroups.All(a => a.FavoriteGuid != MyTargetGroup.Id.Value))
                            user.FavoriteGroups.Add(new ApplicationUserFavoriteGroup()
                            {
                                FavoriteGuid = MyTargetGroup.Id.Value
                            });
                    }
                }
                if (user != null)
                    user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();


                SINner sin = await context.SINners.Include(a => a.MyGroup)
                    .Include(a => a.MyGroup.MyParentGroup)
                    .Include(a => a.MyGroup.MyParentGroup.MyGroups)
                    .Include(a => a.MyGroup.MyGroups)
                    .Include(a => a.SINnerMetaData)
                    //.Include(a => a.MyExtendedAttributes)
                    .Include(a => a.SINnerMetaData.Visibility)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .FirstOrDefaultAsync(a => a.Id == SinnerId);
                if (sin == null)
                    throw new ArgumentException("SinnerId not found", nameof(SinnerId));
                if (string.IsNullOrEmpty(sin.DownloadUrl))
                    throw new ArgumentException("Sinner " + sin.Alias + " does not have a DownloadURL!");
                //if (String.IsNullOrEmpty(sin.MyExtendedAttributes?.JsonSummary))
                //    throw new ArgumentException("Sinner " + sin.Alias + " does not have a valid JsonSummary!");
                sin.MyGroup = MyTargetGroup;

                await context.SaveChangesAsync();
                if (sin.MyGroup != null)
                {
                    if (sin.MyGroup.MyGroups == null)
                        sin.MyGroup.MyGroups = new List<SINnerGroup>();
                    else
                        RemovePWHashRecursive(sin.MyGroup.MyGroups);
                    if (sin.MyGroup.MyParentGroup != null)
                    {
                        sin.MyGroup.MyParentGroup.PasswordHash = string.Empty;
                        sin.MyGroup.MyParentGroup.MyGroups = new List<SINnerGroup>();
                    }
                }

                return sin;
            }
            catch (Exception e)
            {
                try
                {
                    if (tc == null)
                        tc = new TelemetryClient();
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
                            msg += Environment.NewLine + "\t" + singleerr;
                        }
                    }
                    res = new ResultGroupGetGroupById(new HubException(msg));
                    return BadRequest(res);
                }

                var group = await _context.SINnerGroups
                    .Include(a => a.MySettings)
                    .Include(a => a.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .FirstOrDefaultAsync(a => a.Id == groupid);
                if (group == null)
                {
                    var e = new ArgumentException("Could not find group with id " + groupid + ".");
                    res = new ResultGroupGetGroupById(e);
                    return NotFound(res);
                }

                if (group.MyGroups == null)
                    group.MyGroups = new List<SINnerGroup>();
                else
                    RemovePWHashRecursive(group.MyGroups);
                group.PasswordHash = string.Empty;
                if (group.MyParentGroup != null)
                {
                    group.MyParentGroup.PasswordHash = string.Empty;
                    group.MyParentGroup.MyGroups = new List<SINnerGroup>();
                }
                res = new ResultGroupGetGroupById(group);
                return Ok(res);

            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("groupid", groupid.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
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
            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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

#pragma warning disable CS1572 // XML comment has a param tag for 'UsernameOrEmail', but there is no parameter by that name
#pragma warning disable CS1572 // XML comment has a param tag for 'SINnerName', but there is no parameter by that name
        /// <summary>
        /// Search for Groups
        /// </summary>
        /// <param name="Groupname"></param>
        /// <param name="UsernameOrEmail"></param>
        /// <param name="SINnerName"></param>
        /// <returns></returns>
        [HttpGet()]
#pragma warning restore CS1572 // XML comment has a param tag for 'SINnerName', but there is no parameter by that name
#pragma warning restore CS1572 // XML comment has a param tag for 'UsernameOrEmail', but there is no parameter by that name
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupGetGroupmembers")]
        [AllowAnonymous]
#pragma warning disable CS1573 // Parameter 'email' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'Language' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'password' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetGroupmembers(string Groupname, string Language, string email, string password)
#pragma warning restore CS1573 // Parameter 'password' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'Language' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'email' has no matching param tag in the XML comment for 'SINnerGroupController.GetGroupmembers(string, string, string, string)' (but other parameters do)
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
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.GetHash(string)'
        public static byte[] GetHash(string inputString)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.GetHash(string)'
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.GetHashString(string)'
        public static string GetHashString(string inputString)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupController.GetHashString(string)'
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
                if (!string.IsNullOrEmpty(Groupname))
                {
                    string strGroupNameUpper = Groupname.ToUpperInvariant();
                    groupfoundseq = await _context.SINnerGroups.Where(a => a.Groupname.ToUpperInvariant().Contains(strGroupNameUpper)
                                                                           && (a.Language == language || string.IsNullOrEmpty(language)))
                        .Select(a => a.Id).ToListAsync();
                    if (groupfoundseq.Count == 0)
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

                if (result.SINGroups == null)
                    result.SINGroups = new List<SINnerSearchGroup>();
                else
                    RemovePWHashRecursive(result.SINGroups);

                return result;

            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("Groupname", Groupname);
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
#pragma warning disable CS1573 // Parameter 'Language' has no matching param tag in the XML comment for 'SINnerGroupController.GetSearchGroups(string, string, string, string)' (but other parameters do)
        public async Task<ActionResult<ResultGroupGetSearchGroups>> GetSearchGroups(string Groupname, string UsernameOrEmail, string SINnerName, string Language)
#pragma warning restore CS1573 // Parameter 'Language' has no matching param tag in the XML comment for 'SINnerGroupController.GetSearchGroups(string, string, string, string)' (but other parameters do)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
            ResultGroupGetSearchGroups res = null;
            var user = await _signInManager.UserManager.GetUserAsync(User);
            _logger.LogTrace("GetSearchGroups: " + Groupname + "/" + UsernameOrEmail + "/" + SINnerName + ".");
            try
            {
                var r = await GetSearchGroupsInternal(Groupname, UsernameOrEmail, SINnerName, Language);
                res = new ResultGroupGetSearchGroups(r);
                string teststring = JsonConvert.SerializeObject(res);
                var returnObj = JsonConvert.DeserializeObject<ResultGroupGetSearchGroups>(teststring);
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
            catch (Exception e)
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
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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
            _logger.LogTrace("DeleteLeaveGroup: " + groupid + ".");
            try
            {
                var group = await DeleteGroupInternal(groupid);
                _context.SINnerGroups.Remove(group);
                await _context.SaveChangesAsync();
                return Ok(true);

            }
            catch (Exception e)
            {
                try
                {
                    var user = await _signInManager.UserManager.GetUserAsync(User);
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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
            if (groupid == null || groupid == Guid.Empty)
                throw new ArgumentNullException(nameof(groupid));
            var mygroup = await _context.SINnerGroups
                .Include(a => a.MyGroups).FirstOrDefaultAsync(a => a.Id == groupid);
            if (mygroup == null)
                return null;

            ApplicationUser user = await _signInManager.UserManager.GetUserAsync(User);
            if (user == null)
                throw new NoUserRightException("Could not verify ApplicationUser!");
            bool candelete = false;
            var members = await _context.SINners.Where(a => a.MyGroup == mygroup).ToListAsync();
            if (mygroup.IsPublic == false)
            {
                if (mygroup.GroupCreatorUserName?.ToUpperInvariant() != user.NormalizedEmail
                    && !string.IsNullOrEmpty(mygroup.GroupCreatorUserName))
                {
                    if (members.Count > 2)
                    {
                        //if there is only one member left, it's a pointless group anyway
                        throw new NoUserRightException("Only " + mygroup.GroupCreatorUserName +
                                                       " can delete this group.");
                    }
                }
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("GroupAdmin") || roles.Contains(mygroup.MyAdminIdentityRole))
                {
                    candelete = true;
                }
                else
                {
                    throw new NoUserRightException("Group is public - can only be deleted by GroupAdmins or " + mygroup.MyAdminIdentityRole + ".");
                }
            }

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
            if (groupid == default || groupid == Guid.Empty)
                throw new ArgumentNullException(nameof(groupid));
            if (sinnerid == default || sinnerid == Guid.Empty)
                throw new ArgumentNullException(nameof(sinnerid));

            var group = await _context.SINnerGroups.Include(a => a.MyGroups).FirstOrDefaultAsync(a => a.Id == groupid);

            if (group == null)
                return NotFound(groupid);

            var members = await group.GetGroupMembers(_context, false);

            var sinner = await _context.SINners.Include(a => a.MyGroup).FirstOrDefaultAsync(a => a.Id == sinnerid);
            if (sinner == null)
                return NotFound(sinnerid);

            sinner.MyGroup = null;

            if (members.Count < 2 && members.Contains(sinner))
            {
                if (group.MyGroups == null || group.MyGroups.Count == 0)
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
                        IsolationLevel = IsolationLevel.ReadUncommitted

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
                                msg += Environment.NewLine + "\t" + singleerr;
                            }
                        }

                        throw new HubException(msg);
                    }

                    SINSearchGroupResult result = new SINSearchGroupResult();
                    if (user != null)
                    {
                        SINnerSearchGroup ssgFavs = new SINnerSearchGroup
                        {
                            Id = Guid.Empty,
                            Groupname = "Favorites"
                        };
                        var favlist = user.FavoriteGroups.Select(a => a.FavoriteGuid).ToHashSet();
                        var favgrouplist = await _context.SINnerGroups.Where(a => a.Id != null && favlist.Contains(a.Id.Value))
                            .ToListAsync();
                        foreach (var favgroup in favgrouplist)
                        {
                            var ssgsinglefav = new SINnerSearchGroup(favgroup, user);
                            ssgFavs.MySINSearchGroups.Add(ssgsinglefav);
                        }

                        result.SINGroups.Add(ssgFavs);
                    }

                    List<Guid?> groupfoundseq = new List<Guid?>();
                    if (!string.IsNullOrEmpty(Groupname))
                    {
                        string strGroupNameUpper = Groupname;
                        groupfoundseq = await _context.SINnerGroups
                            .Where(a => a.Groupname != null
                                        && a.Groupname.Contains(strGroupNameUpper)
                                        && (a.Language == language || string.IsNullOrEmpty(language)))
                            .Select(a => a.Id).ToListAsync();
                        if (groupfoundseq.Count == 0)
                        {
                            throw new ArgumentException("No group found with the given parameter: " + Groupname);
                        }
                    }
                    else if (string.IsNullOrEmpty(UsernameOrEmail) && string.IsNullOrEmpty(sINnerName))
                    {
                        groupfoundseq = await _context.SINnerGroups.Where(a => a.IsPublic && a.MyParentGroupId == null).Select(a => a.Id).ToListAsync();
                        if (groupfoundseq.Count == 0)
                        {
                            throw new ArgumentException("No group found with the given parameter IsPublic");
                        }
                    }

                    foreach (var groupid in groupfoundseq)
                    {
                        var ssg = await GetSinSearchGroupResultById(groupid, user);
                        result.SINGroups.Add(ssg);
                    }

                    if (!string.IsNullOrEmpty(UsernameOrEmail))
                    {
                        List<SINner> byUser = new List<SINner>();
                        ApplicationUser bynameuser = await _userManager.FindByNameAsync(UsernameOrEmail);

                        if (bynameuser != null)
                        {
                            var usersinners = await SINner.GetSINnersFromUser(bynameuser, _context, true);
                            byUser.AddRange(usersinners);
                        }

                        ApplicationUser byemailuser = await _userManager.FindByEmailAsync(UsernameOrEmail);
                        if (byemailuser != null && byemailuser != bynameuser)
                        {
                            var usersinners = await SINner.GetSINnersFromUser(byemailuser, _context, true);
                            byUser.AddRange(usersinners);
                        }


                        foreach (var sin in byUser)
                        {
                            if (sin.MyGroup != null)
                            {
                                SINnerSearchGroup ssg = result.SINGroups.FirstOrDefault(a => sin.MyGroup.Groupname.Equals(a.Groupname, StringComparison.Ordinal))
                                                        ?? new SINnerSearchGroup(sin.MyGroup, user);

                                SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember(user, sin);
                                if (byemailuser != null)
                                    ssgm.Username = byemailuser.UserName;
                                if (bynameuser != null)
                                    ssgm.Username = bynameuser.UserName;
                                ssg.MyMembers.Add(ssgm);
                            }
                        }
                    }

                    if (result.SINGroups == null)
                        result.SINGroups = new List<SINnerSearchGroup>();
                    else
                        RemovePWHashRecursive(result.SINGroups);
                    if (user != null)
                    {
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
                            SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember(user, ownedSin);
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
                    }
                    if (result.SINGroups == null)
                        result.SINGroups = new List<SINnerSearchGroup>();
                    else
                        RemovePWHashRecursive(result.SINGroups);
                    t.Complete();
                    return result;
                }
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("Groupname", Groupname);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
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
                    string msg = "ModelState is invalid: ";
                    foreach (var err in ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0))
                    {
                        foreach (var singleerr in err)
                        {
                            msg += Environment.NewLine + "\t" + singleerr;
                        }
                    }
                    res = new ResultGroupGetSearchGroups(new HubException(msg));
                    return BadRequest(res);
                }

                SINSearchGroupResult result = new SINSearchGroupResult();
                var range = await GetSinSearchGroupResultById(groupid, user);
                result.SINGroups.Add(range);
                if (result.SINGroups == null)
                    result.SINGroups = new List<SINnerSearchGroup>();
                else
                    RemovePWHashRecursive(result.SINGroups);
                res = new ResultGroupGetSearchGroups(result);
                Ok(res);
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
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

        private static void RemovePWHashRecursive(IEnumerable<SINnerSearchGroup> sINGroups)
        {
            foreach (var group in sINGroups)
            {
                if (!string.IsNullOrEmpty(group.PasswordHash))
                {
                    group.HasPassword = true;
                    group.PasswordHash = string.Empty;
                }
                if (group.MyGroups == null)
                    group.MyGroups = new List<SINnerGroup>();
                else
                    RemovePWHashRecursive(group.MyGroups);
            }
        }

        private static void RemovePWHashRecursive(IEnumerable<SINnerGroup> sINGroups)
        {
            foreach (var group in sINGroups)
            {
                group.PasswordHash = string.Empty;
                if (group.MyGroups == null)
                    group.MyGroups = new List<SINnerGroup>();
                else
                    RemovePWHashRecursive(group.MyGroups);
            }
        }

        private async Task<SINnerSearchGroup> GetSinSearchGroupResultById(Guid? groupid, ApplicationUser askingUser,
            bool addTags = false)
        {
            if (groupid == null || groupid == Guid.Empty)
                throw new ArgumentNullException(nameof(groupid));
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted

                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                ApplicationUser user = null;
                if (User != null)
                    user = await _signInManager.UserManager.GetUserAsync(User);
                SINnerSearchGroup ssg = null;
                var group = await _context.SINnerGroups
                    //.Include(a => a.MyParentGroup)
                    .Include(a => a.MySettings)
                    .Include(a => a.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .ThenInclude(b => b.MyGroups)
                    .FirstOrDefaultAsync(a => a.Id == groupid);
                if (group != null)
                {
                    if (group.MyGroups == null)
                        group.MyGroups = new List<SINnerGroup>();
                    ssg = new SINnerSearchGroup(group, user);

                    var members = await ssg.GetGroupMembers(_context, addTags);
                    foreach (var member in members)
                    {
                        if (member == null)
                            continue;
                        if (member.SINnerMetaData?.Visibility?.IsGroupVisible == false)
                        {
                            if (member.SINnerMetaData?.Visibility.UserRights.Any(a => string.IsNullOrEmpty(a.EMail)) == false)
                            {
                                if (user == null || member.SINnerMetaData?.Visibility.UserRights.Any(a =>
                                        a.EMail?.ToUpperInvariant() == user.NormalizedEmail) == false)
                                {
                                    //dont show this guy!
                                    continue;
                                }
                            }
                        }
                        member.MyGroup = null;
                        SINnerSearchGroupMember ssgm = new SINnerSearchGroupMember(user, member);
                        ssg.MyMembers.Add(ssgm);
                    }

                    foreach (var child in group.MyGroups)
                    {
                        bool okToShow = false;
                        if (!child.IsPublic)
                        {
                            if (user == null)
                                continue;
                            //check if the user has the right to see this group
                            var roles = await _userManager.GetRolesAsync(user);
                            if (roles.Contains(child.MyAdminIdentityRole))
                            {
                                okToShow = true;
                            }
                        }
                        else
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

                if (ssg != null)
                {
                    if (ssg.MyGroups == null)
                        ssg.MyGroups = new List<SINnerGroup>();
                    else
                        RemovePWHashRecursive(ssg.MyGroups);
                }

                t.Complete();
                return ssg;
            }
        }
    }
}

