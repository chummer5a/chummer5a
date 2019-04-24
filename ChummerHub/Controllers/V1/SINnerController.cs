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
    [ControllerName("SIN")]
    [Authorize]
    public class SINnerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private SignInManager<ApplicationUser> _signInManager = null;
        private UserManager<ApplicationUser> _userManager = null;

        public SINnerController(ApplicationDbContext context,
            ILogger<SINnerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }


        private System.Net.Http.HttpClient MyHttpClient { get; } = new System.Net.Http.HttpClient();

        ~SINnerController()
        {
            MyHttpClient?.Dispose();
        }

        // GET: api/ChummerFiles/5
        //[Route("download")]
        //[SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        /// <summary>
        /// Returns the Chummer-Save-File
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{sinnerid}")]
        [AllowAnonymous]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDownloadFile")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
        [Produces(@"application/json", @"application/octet-stream")]
        public async Task<FileResult> GetDownloadFile([FromRoute] Guid sinnerid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("ModelState is invalid!");
                }

                var sinnerseq = await (from a in _context.SINners
                                .Include(a => a.MyGroup)
                                .Include(a => a.SINnerMetaData.Visibility.UserRights)
                                where a.Id == sinnerid select a).ToListAsync();
                if (!sinnerseq.Any())
                {
                    throw new ArgumentException("Could not find id " + sinnerid.ToString());
                }
                ApplicationUser user = null;
                if (!String.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var chummerFile = sinnerseq.FirstOrDefault();
                if (chummerFile == null)
                {
                    throw new ArgumentException("Could not find id " + sinnerid.ToString());
                }
                if (String.IsNullOrEmpty(chummerFile.DownloadUrl))
                {
                    string msg = "Chummer " + chummerFile.Id + " does not have a valid DownloadUrl!";
                    throw new ArgumentException(msg);
                }
                bool oktoDownload = false;
                if ((!oktoDownload) && (chummerFile.SINnerMetaData.Visibility.IsPublic == true))
                {
                    oktoDownload = true;
                }
                if ((!oktoDownload) && (chummerFile.MyGroup != null && chummerFile.MyGroup.IsPublic == true))
                {
                    oktoDownload = true;
                }
                if ((!oktoDownload) && (user != null && chummerFile.SINnerMetaData.Visibility.UserRights.Any(a => a.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())))
                {
                    oktoDownload = true;
                }
                if (!oktoDownload)
                {
                    throw new ArgumentException("User " + user?.UserName + " or public is not allowed to download " + sinnerid.ToString());
                }
                //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), chummerFile.Id.ToString() + ".chum5z");
                var stream = await MyHttpClient.GetStreamAsync(new Uri(chummerFile.DownloadUrl));
                string downloadname = chummerFile.Id.ToString() + ".chum5z";
                
                if(user == null)
                {
                    throw new NoUserRightException("User not found!");
                }
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("GetDownloadFile");
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerId", sinnerid.ToString());
                    telemetry.Metrics.Add("FileSize", stream.Length);
                    tc.TrackEvent(telemetry);
                }
                catch(Exception e)
                {
                    _logger.LogError(e.ToString());
                }
                return new FileStreamResult(stream, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"))
                {
                    FileDownloadName = downloadname
                };

            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;
                HubException hue = new HubException("Exception in GetDownloadFile: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NoContent)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SinnerGetSINnerGroupFromSINerById")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultSinnerGetSINnerGroupFromSINerById>> GetSINnerGroupFromSINerById([FromRoute] Guid id)
        {
            ResultSinnerGetSINnerGroupFromSINerById res;
            try
            {
                
                if (!_context.SINners.Any(a => a.Id == id))
                {
                    var e = new ArgumentException("SINner with id " + id + " does not exist.");
                    res = new ResultSinnerGetSINnerGroupFromSINerById(e);
                    return NotFound(res);
                }

                var groupseq = await (from a in _context.SINners.Include(a => a.MyGroup)
                        .Include(b => b.MyGroup.MySettings)
                    where a.Id == id
                    select a.MyGroup).ToListAsync();

                
                if (!groupseq.Any())
                {
                    return NoContent();
                }
                else
                {
                    res = new ResultSinnerGetSINnerGroupFromSINerById(groupseq.FirstOrDefault());
                    return Ok(res);
                }

            }
            catch (Exception e)
            {
                res = new ResultSinnerGetSINnerGroupFromSINerById(e);
                return BadRequest(res);
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SinnerGetSINById")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultSinnerGetSINById>> GetSINById([FromRoute] Guid id)
        {
            ResultSinnerGetSINById res; 
            try
            {
                
                ApplicationUser user = null;
                if (!String.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sin = await _context.SINners
                    .Include(a => a.MyExtendedAttributes)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(a => a.MyGroup)
                    .Include(b => b.MyGroup.MySettings)
                    .Where(a => a.Id == id).Take(1)
                    .FirstOrDefaultAsync(a => a.Id == id);
                res = new ResultSinnerGetSINById(sin);
                if (sin == null)
                {
                   
                    return NotFound(res);
                }
                bool oktoDownload = false;
                if ((!oktoDownload) && (sin.SINnerMetaData.Visibility.IsPublic == true))
                {
                    oktoDownload = true;
                }
                if ((!oktoDownload) && (sin.MyGroup != null && sin.MyGroup.IsPublic == true))
                {
                    oktoDownload = true;
                }
                if ((!oktoDownload) && (user != null && sin.SINnerMetaData.Visibility.UserRights.Any(a => a.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())))
                {
                    oktoDownload = true;
                }
                if (!oktoDownload)
                {
                    var e =  new ArgumentException("User " + user?.UserName + " or public is not allowed to download " + id.ToString());
                    res = new ResultSinnerGetSINById(e);
                    return BadRequest(res);
                }
                
                res = new ResultSinnerGetSINById(sin);
                if(sin.SINnerMetaData.Visibility.IsPublic == true)
                    return Ok(res);

              
                var list = (from a in sin.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a);
                if(list.Any())
                    return Ok(res);
                
                var e1 =  new NoUserRightException("SINner is not viewable for public or groupmembers.");
                res = new ResultSinnerGetSINById(e1);
                return BadRequest(res);

            }
            catch (Exception e)
            {
                res = new ResultSinnerGetSINById(e);
                return BadRequest(res);
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}", Name = "SinnerGetOwnedSINByAlias")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SinnerGetOwnedSINByAlias")]
        [Authorize]
        public async Task<ActionResult<ResultSinnerGetOwnedSINByAlias>> GetOwnedSINByAlias([FromRoute] string id)
        {
            ResultSinnerGetOwnedSINByAlias res;
            try
            {
               
                ApplicationUser user = null;
                if (!String.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sinseq = await _context.SINners
                    .Include(a => a.MyExtendedAttributes)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(a => a.MyGroup)
                    .Include(b => b.MyGroup.MySettings)
                    .Where(a => a.Alias == id).ToListAsync();
                if (!sinseq.Any())
                {
                    var e = new ArgumentException("SINner with Alias " + id + " does not exist.");
                    res = new ResultSinnerGetOwnedSINByAlias(e);
                    return NotFound(res);
                }
                List<SINner> download = new List<SINner>();
                foreach (var sin in sinseq)
                {
                    if ((user != null &&
                                            sin.SINnerMetaData.Visibility.UserRights.Any(a =>
                                                a.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())))
                    {
                        download.Add(sin);
                    }
                }
                res = new ResultSinnerGetOwnedSINByAlias(download);
                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultSinnerGetOwnedSINByAlias(e);
                return BadRequest(res);
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}", Name = "GetOwnedSINByAlias")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetOwnedSINByAlias")]
        [Authorize]
        public async Task<ActionResult<List<SINner>>> GetOwnedSINByAlias([FromRoute] string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApplicationUser user = null;
                if (!String.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sinseq = await _context.SINners
                    .Include(a => a.MyExtendedAttributes)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(a => a.MyGroup)
                    .Include(b => b.MyGroup.MySettings)
                    .Where(a => a.Alias == id).ToListAsync();
                if (!sinseq.Any())
                {
                       return NotFound("SINner with Alias " + id + " does not exist.");
                }
                List<SINner> download = new List<SINner>();
                foreach (var sin in sinseq)
                {
                    if ((user != null &&
                                            sin.SINnerMetaData.Visibility.UserRights.Any(a =>
                                                a.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())))
                    {
                        download.Add(sin);
                    }
                }

                return Ok(download);
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;
                HubException hue = new HubException("Exception in GetOwnedSINByAlias: " + e.Message, e);
                throw hue;
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerPut")]
        [Authorize]
        public async Task<ActionResult<ResultSINnerPut>> PutSIN([FromRoute] Guid id, IFormFile uploadedFile)
        {
            ResultSINnerPut res;
            ApplicationUser user = null;
            SINner dbsinner = null;
            try
            {
                var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);
                if (sin == null)
                {
                    var e = new ArgumentException("Sinner with Id " + id + " not found!");
                    res = new ResultSINnerPut(e);
                    return NotFound(res);
                }
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                dbsinner = await CheckIfUpdateSINnerFile(id, user);
                if (dbsinner == null)
                {
                    var e = new ArgumentException("You may not edit this (existing) sinner!");
                    res = new ResultSINnerPut(e);
                    return Conflict(res);
                }
                sin.GoogleDriveFileId = dbsinner.GoogleDriveFileId;
                if(user == null)
                {
                    var e =  new NoUserRightException("User not found!");
                    res = new ResultSINnerPut(e);
                    return NotFound(res);
                }
                
                sin.DownloadUrl = Startup.GDrive.StoreXmlInCloud(sin, uploadedFile);
                _context.Entry(dbsinner).CurrentValues.SetValues(sin);
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PutStoreXmlInCloud");
                    telemetry.Properties.Add("User", user.Email);
                    telemetry.Properties.Add("SINnerId", sin.Id.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName?.ToString());
                    telemetry.Metrics.Add("FileSize", uploadedFile.Length);
                    tc.TrackEvent(telemetry);
                }
                catch(Exception e)
                {
                    _logger.LogError(e.ToString());
                }
                try
                {
                    int x = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    res = new ResultSINnerPut(e);
                    return Conflict(res);
                }

                res = new ResultSINnerPut(sin);
                return Ok(res);
            }
            catch(NoUserRightException e)
            {
                res = new ResultSINnerPut(e);
                return BadRequest(res);
            }
            catch (Exception e)
            {
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerId", dbsinner?.Id?.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName?.ToString());
                    telemetry.Metrics.Add("FileSize", uploadedFile.Length);
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultSINnerPut(e);
                return BadRequest(res);
            }
        }

       

        private async Task<ActionResult<ResultSinnerPostSIN>> PostSINnerInternal(UploadInfoObject uploadInfo)
        {
            ResultSinnerPostSIN res;
            _logger.LogTrace("Post SINnerInternalt: " + uploadInfo + ".");
            ApplicationUser user = null;
            SINner sinner = null;
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
                    var e = new HubException(msg);
                    res = new ResultSinnerPostSIN(e);
                    return BadRequest(res);
                }
                if (uploadInfo.UploadDateTime == null)
                    uploadInfo.UploadDateTime = DateTime.Now;
                if (uploadInfo.Client != null)
                {
                    if (!UploadClientExists(uploadInfo.Client.Id))
                    {
                        _context.UploadClients.Add(uploadInfo.Client);
                    }
                    else
                    {
                        _context.UploadClients.Attach(uploadInfo.Client);
                        _context.Entry(uploadInfo.Client).State = EntityState.Modified;
                        _context.Entry(uploadInfo.Client).CurrentValues.SetValues(uploadInfo.Client);
                    }
                }
                var returncode = HttpStatusCode.OK;
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                foreach (var tempsinner in uploadInfo.SINners)
                {
                    sinner = tempsinner;
                    if (sinner.Id.ToString() == "string")
                        sinner.Id = Guid.Empty;

                    if (String.IsNullOrEmpty(sinner.MyExtendedAttributes.JsonSummary))
                    {
                        var e = new ArgumentException("sinner " + sinner.Id + ": JsonSummary == null");
                        res = new ResultSinnerPostSIN(e);
                        return BadRequest(res);
                    }
                        

                    //check for own visibility
                    if (!sinner.SINnerMetaData.Visibility.UserRights.Any(a => a.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant() && a.CanEdit == true))
                    {
                        var addme = new SINerUserRight()
                        {
                            CanEdit = true,
                            EMail = user.Email,
                            SINnerId = sinner.Id
                        };
                        sinner.SINnerMetaData.Visibility.UserRights.Add(addme);
                    }

                    if (sinner.LastChange == null)
                    {
                        var e = new ArgumentException("Sinner  " + sinner.Id + ": LastChange not set!");
                        res = new ResultSinnerPostSIN(e);
                        return BadRequest(res);
                    }
                    if ((sinner.SINnerMetaData.Visibility.Id == null)
                        || (sinner.SINnerMetaData.Visibility.Id == Guid.Empty))
                    {
                        sinner.SINnerMetaData.Visibility.Id = Guid.NewGuid();
                    }
                    var oldsinner = (from a in _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights)
                                                        .Include(b => b.MyGroup)
                                     where a.Id == sinner.Id
                                     select a).FirstOrDefault();
                    if(oldsinner != null)
                    {
                        var canedit = await CheckIfUpdateSINnerFile(oldsinner, user);
                        if (canedit == null)
                        {
                            string msg = "SINner " + sinner.Id + " is not editable for user " + user.Email + ".";
                            var e = new NoUserRightException(msg);
                            res = new ResultSinnerPostSIN(e);
                            return BadRequest(res);

                        }
                        var olduserrights = oldsinner.SINnerMetaData.Visibility.UserRights.ToList();
                        oldsinner.SINnerMetaData.Visibility.UserRights.Clear();
                        _context.UserRights.RemoveRange(olduserrights);
                        bool userfound = false;
                        foreach(var ur in sinner.SINnerMetaData.Visibility.UserRights)
                        {
                            if (ur.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())
                            {
                                ur.CanEdit = true;
                                userfound = true;
                            }
                            ur.Id = Guid.NewGuid();
                            ur.SINnerId = sinner.Id;
                            _context.UserRights.Add(ur);
                        }
                        if (!userfound)
                        {
                            SINerUserRight ownUser = new SINerUserRight();
                            ownUser.Id = Guid.NewGuid();
                            ownUser.SINnerId = sinner.Id;
                            ownUser.CanEdit = true;
                            ownUser.EMail = user.Email;
                            sinner.SINnerMetaData.Visibility.UserRights.Add(ownUser);
                            _context.UserRights.Add(ownUser);                                
                        }
                    }
                    else
                    {
                        var ownuserfound = false;
                        var list = sinner.SINnerMetaData.Visibility.UserRights.ToList();
                        foreach (var ur in list)
                        {
                            ur.SINnerId = sinner.Id;
                            if (ur.EMail.ToLowerInvariant() == "delete.this.and.add@your.mail".ToLowerInvariant())
                                sinner.SINnerMetaData.Visibility.UserRights.Remove(ur);
                            if (ur.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant())
                                ownuserfound = true;
                        }
                        if (!ownuserfound)
                        {
                            SINerUserRight ownright = new SINerUserRight();
                            ownright.CanEdit = true;
                            ownright.EMail = user.Email;
                            ownright.SINnerId = sinner.Id;
                            ownright.Id = Guid.NewGuid();
                            sinner.SINnerMetaData.Visibility.UserRights.Add(ownright);
                        }
                    }
               
                    foreach(var tag in sinner.SINnerMetaData.Tags)
                    {
                        tag.SetSinnerIdRecursive(sinner.Id);
                    }

                    sinner.UploadClientId = uploadInfo.Client.Id;
                    SINner dbsinner = await CheckIfUpdateSINnerFile(sinner.Id.Value, user);
                    SINnerGroup oldgroup = null;
                    if (dbsinner != null)
                    {
                        oldgroup = dbsinner.MyGroup;
                        _context.SINners.Attach(dbsinner);
                        if (String.IsNullOrEmpty(sinner.GoogleDriveFileId))
                            sinner.GoogleDriveFileId = dbsinner.GoogleDriveFileId;
                        if(String.IsNullOrEmpty(sinner.DownloadUrl))
                            sinner.DownloadUrl = dbsinner.DownloadUrl;
                        
                        _context.UserRights.RemoveRange(dbsinner.SINnerMetaData.Visibility.UserRights);
                        _context.SINnerVisibility.Remove(dbsinner.SINnerMetaData.Visibility);
                        var alltags = await _context.Tags.Where(a => a.SINnerId == dbsinner.Id).Select(a => a.Id).ToListAsync();
                        foreach (var id in alltags)
                        {
                            var tag = from a in _context.Tags where a.Id == id select a;
                            if (tag.Any())
                            {
                                _context.Tags.Remove(tag.FirstOrDefault());
                            }
                        }
                        _context.SINnerMetaData.Remove(dbsinner.SINnerMetaData);
                        _context.SINners.Remove(dbsinner);
                        dbsinner.SINnerMetaData.Visibility.UserRights.Clear();
                        dbsinner.SINnerMetaData.Visibility = null;
                        dbsinner.SINnerMetaData.Tags = null;
                        dbsinner.SINnerMetaData = null;
                        
                        await _context.SaveChangesAsync();
                        
                        await _context.SINners.AddAsync(sinner);
                        string msg = "Sinner " + sinner.Id + " updated: " + _context.Entry(dbsinner).State.ToString();
                        msg += Environment.NewLine + Environment.NewLine + "LastChange: " + dbsinner.LastChange;
                        _logger.LogError(msg);
                        List<Tag> taglist = sinner.SINnerMetaData.Tags;
                        UpdateEntityEntries(taglist);
                    }
                    else
                    {
                        returncode = HttpStatusCode.Created;
                        sinner.MyGroup = null;
                        _context.SINners.Add(sinner);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (oldgroup != null)
                        {
                            var roles = await _userManager.GetRolesAsync(user);
                            await SINnerGroupController.PutSiNerInGroupInternal(oldgroup.Id.Value, sinner.Id.Value, user, _context,
                                _logger, oldgroup.PasswordHash, roles);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        foreach(var entry in ex.Entries)
                        {
                            if(entry.Entity is SINner || entry.Entity is Tag)
                            {
                                try
                                {
                                    Utils.DbUpdateConcurrencyExceptionHandler(entry, _logger);
                                }
                                catch (Exception e)
                                {
                                    res = new ResultSinnerPostSIN(e);
                                    return BadRequest(res);
                                }
                            }
                            else
                            {
                                var e = new NotSupportedException(
                                    "Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                                res = new ResultSinnerPostSIN(e);
                                return BadRequest(res);
                            }
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        res = new ResultSinnerPostSIN(ex);
                        return BadRequest(res);
                    }
                    catch (Exception e)
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
                        res = new ResultSinnerPostSIN(e);
                        return Conflict(res);
                    }
                }
                
                List<Guid> myids = (from a in uploadInfo.SINners select a.Id.Value).ToList();
                List<SINner> sinlist = new List<SINner>();
                foreach (var id in myids)
                {
                    var sin = from a in _context.SINners where a.Id == id select a;
                    if (sin.Any())
                        sinlist.Add(sin.FirstOrDefault());
                }
                res = new ResultSinnerPostSIN(sinlist);
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted(res);
                    case HttpStatusCode.Created:
                        return Created("SINnerPostSIN", res);
                    default:
                        return Ok(res);
                }
            }
            catch (Exception e)
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
                res = new ResultSinnerPostSIN(e);
                return BadRequest(res);
            }
        }

      

        private void UpdateEntityEntries(List<Tag> taglist)
        {
            foreach (var item in taglist)
            {
                if(!_context.Tags.Contains(item))
                    _context.Tags.Add(item);
                //_context.Tags.Attach(item);
                //_context.Entry(item).State = EntityState.Modified;
                //_context.Entry(item).CurrentValues.SetValues(item);
                UpdateEntityEntries(item.Tags);
            }
        }

        // POST: api/ChummerFiles
        /// <summary>
        /// Store the MetaData for chummerfiles (to get a Id).
        /// This Id can be used to store the actual file with PUT afterwards.
        /// Alternativly, the DownloadUrl can be set directly from the Client.
        /// </summary>
        /// <param name="uploadInfo"></param>
        /// <returns></returns>
        [HttpPost()]
        [SwaggerRequestExample(typeof(UploadInfoObject), typeof(UploadInfoObjectExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerPostSIN")]
        [Authorize]
        public async Task<ActionResult<ResultSinnerPostSIN>> PostSIN([FromBody] UploadInfoObject uploadInfo)
        {
            return await PostSINnerInternal(uploadInfo);
        }

        // DELETE: api/ChummerFiles/5
        [HttpDelete("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDelete")]
        [Authorize]
        public async Task<ActionResult<ResultSinnerDelete>> Delete([FromRoute] Guid id)
        {
            ResultSinnerDelete res;
            try
            {
                var sinner = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);

                if (sinner == null)
                {
                    var e = new ArgumentException("Sinner " + id + " not found.");
                    res = new ResultSinnerDelete(e);
                    return NotFound(res);
                }
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var dbsinner = await CheckIfUpdateSINnerFile(id, user);
                if (dbsinner == null)
                {
                    var e = new ArgumentException("not authorized");
                    res = new ResultSinnerDelete(e);
                    return BadRequest(res);
                }
                var olduserrights = await (from a in _context.UserRights where a.SINnerId == sinner.Id select a).ToListAsync();
                _context.UserRights.RemoveRange(olduserrights);
                var oldtags = await (from a in _context.Tags where a.SINnerId == sinner.Id select a).ToListAsync();
                _context.Tags.RemoveRange(oldtags);
                var oldsinners = await (from a in _context.SINners where a.Id == sinner.Id select a).ToListAsync();
                foreach(var oldsin in oldsinners)
                {
                    if(_context.SINnerVisibility.Contains(oldsin.SINnerMetaData.Visibility))
                        _context.SINnerVisibility.Remove(oldsin.SINnerMetaData.Visibility);
                    if(_context.SINnerMetaData.Contains(oldsin.SINnerMetaData))
                        _context.SINnerMetaData.Remove(oldsin.SINnerMetaData);
                }

                _context.SINners.RemoveRange(oldsinners);
                await _context.SaveChangesAsync();
                res = new ResultSinnerDelete(true);
                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultSinnerDelete(e);
                return BadRequest(res);
            }
        }

        private async Task<SINner> CheckIfUpdateSINnerFile(Guid id, ApplicationUser user)
        {
            try
            {
                var dbsinner = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(b => b.MyGroup)
                    .FirstOrDefaultAsync(e => e.Id == id);
                return await CheckIfUpdateSINnerFile(dbsinner, user);
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;

                HubException hue = new HubException("Exception in CheckIfUpdateSINnerFile: " + e.Message, e);
                throw hue;
            }
        }

        private async Task<SINner> CheckIfUpdateSINnerFile(SINner dbsinner, ApplicationUser user)
        {
            if (dbsinner != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    if (role.ToUpperInvariant() == "Administrator".ToUpperInvariant())
                    {
                        return dbsinner;
                    }
                }
                var editseq = (from a in dbsinner.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a).ToList();
                foreach (var edit in editseq)
                {
                    if (edit.CanEdit == true)
                        return dbsinner;
                }
                if (dbsinner.MyGroup != null)
                {
                    if (!String.IsNullOrEmpty(dbsinner.MyGroup.MyAdminIdentityRole))
                    {
                        var localadmins = await _userManager.GetUsersInRoleAsync(dbsinner.MyGroup.MyAdminIdentityRole);
                        if (localadmins.Contains(user))
                            return dbsinner;
                    }
                    if (!String.IsNullOrEmpty(dbsinner.MyGroup.GroupCreatorUserName))
                    {
                        if (dbsinner.MyGroup.GroupCreatorUserName == user.UserName)
                            return dbsinner;
                    }
                }
                throw new ChummerHub.NoUserRightException(user.UserName, dbsinner.Id);
            }
            return null;
        }

        private bool UploadClientExists(Guid id)
        {
            try
            {
                return _context.UploadClients.Any(e => e.Id == id);
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in UploadClientExists: " + e.Message, e);
                throw hue;
            }
        }
    }
}
