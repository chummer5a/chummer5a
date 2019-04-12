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

        // GET: api/ChummerFiles
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerListExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerTestSINners")]
        public IEnumerable<SINner> Get()
        {
            try
            {
                _logger.LogTrace("Getting SINner");
                var result = _context.SINners.OrderByDescending(a => a.UploadDateTime).Take(20);
                result = _context.SINners.Include(sinner => sinner.SINnerMetaData)
                    .ThenInclude(meta => meta.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .OrderByDescending(a => a.UploadDateTime).Take(20);
                return result;
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;
                HubException hue = new HubException("Exception in GetSINner: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NoContent)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetSINnerGroupFromSINerById")]
        [AllowAnonymous]
        public async Task<ActionResult<SINnerGroup>> GetSINnerGroupFromSINerById([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!_context.SINners.Any(a => a.Id == id))
                {
                    return NotFound("SINner with id " + id + " does not exist.");
                }

                var groupseq = await (from a in _context.SINners.Include(a => a.MyGroup)
                        .Include(b => b.MyGroup.MySettings)
                    where a.Id == id
                    select a.MyGroup).ToListAsync();

                if (!groupseq.Any())
                    return NoContent();
                else
                {
                    return Ok(groupseq.FirstOrDefault());
                }

            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;
                HubException hue = new HubException("Exception in GetSINById: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetSINById")]
        [AllowAnonymous]
        public async Task<ActionResult<SINner>> GetSINById([FromRoute] Guid id)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
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
                if (sin == null)
                {
                    return NotFound("SINner with id " + id + " does not exist.");
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
                    throw new ArgumentException("User " + user?.UserName + " or public is not allowed to download " + id.ToString());
                }
                

                if(sin.SINnerMetaData.Visibility.IsPublic == true)
                    return Ok(sin);

              
                var list = (from a in sin.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a);
                if(list.Any())
                    return Ok(sin);
                
                throw new NoUserRightException("SINner is not viewable for public or groupmembers.");
                
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;
                HubException hue = new HubException("Exception in GetSINById: " + e.Message, e);
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
        public async Task<IActionResult> PutSIN([FromRoute] Guid id, IFormFile uploadedFile)
        {
            ApplicationUser user = null;
            SINner dbsinner = null;
            try
            {
                var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);
                if (sin == null)
                {
                    return NotFound("Sinner with Id " + id + " not found!");
                }
                user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                dbsinner = await CheckIfUpdateSINnerFile(id, user);
                if (dbsinner == null)
                {
                    return Conflict("CheckIfUpdateSINnerFile");
                }
                sin.GoogleDriveFileId = dbsinner.GoogleDriveFileId;
                if(user == null)
                {
                    throw new NoUserRightException("User not found!");
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
                    return Conflict(e);
                }

                return Ok(sin.DownloadUrl);
            }
            catch(NoUserRightException e)
            {
                return BadRequest(e);
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
                if (e is HubException)
                    return BadRequest(e);
                HubException hue = new HubException("Exception in PutSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

       

        private async Task<IActionResult> PostSINnerInternal(UploadInfoObject uploadInfo)
        {
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
                    return BadRequest(new HubException(msg));
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

                    if(String.IsNullOrEmpty(sinner.MyExtendedAttributes.JsonSummary))
                        return BadRequest("sinner " + sinner.Id +": JsonSummary == null");

                    if (sinner.SINnerMetaData.Visibility.UserRights.Any() == false)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": Visibility contains no entries!");
                    }

                    if (sinner.LastChange == null)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": LastChange not set!");
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
                        var olduserrights = oldsinner.SINnerMetaData.Visibility.UserRights.ToList();
                        bool canedit = false;
                        foreach(var oldright in olduserrights)
                        {
                            if((oldright.EMail.ToLowerInvariant() == user.Email.ToLowerInvariant()
                                && (oldright.CanEdit == true)))
                            {
                                canedit = true;
                                break;
                            }
                        }
                        if(!canedit)
                        {
                            string msg = "SINner " + sinner.Id + " is not editable for user " + user.Email + ".";
                            throw new NoUserRightException(msg);
                        }
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
                        var alltags = await dbsinner.GetTagsForSinnerFlat(_context);
                        _context.Tags.RemoveRange(alltags);
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
                        if (e is HubException)
                            return Conflict(e);

                        HubException hue = new HubException("Exception in PostSINnerFile: " + e.ToString(), e);
                        //var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = e.Message };
                        return Conflict(hue);
                    }
                }
                
                List<Guid> myids = (from a in uploadInfo.SINners select a.Id.Value).ToList();
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted("PostSIN", myids);
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostSIN", myids);
                    default:
                        break;
                }
                return BadRequest();
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
                if (e is HubException)
                    return BadRequest(e);

                HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                return BadRequest(hue);
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerUpload")]
        [Authorize]
        public async Task<IActionResult> PostSIN([FromBody] UploadInfoObject uploadInfo)
        {
            
            return await PostSINnerInternal(uploadInfo);
        }

        // DELETE: api/ChummerFiles/5
        [HttpDelete("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDelete")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var sinner = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);

                if (sinner == null)
                {
                    return NotFound();
                }
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var dbsinner = await CheckIfUpdateSINnerFile(id, user);
                if (dbsinner == null)
                {
                    return BadRequest("not authorized");
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

                return Ok("deleted");
            }
            catch (Exception e)
            {
                if (e is HubException)
                    return BadRequest(e);

                HubException hue = new HubException("Exception in DeleteSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private async Task<SINner> CheckIfUpdateSINnerFile(Guid id, ApplicationUser user)
        {
            try
            {
                bool admin = false;
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    if (role.ToUpperInvariant() == "Administrator".ToUpperInvariant())
                    {
                        admin = true;
                        break;
                    }
                }
                var dbsinner = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(b => b.MyGroup)
                    .FirstOrDefaultAsync(e => e.Id == id);
                if (dbsinner != null)
                { 
                    var editseq = (from a in dbsinner.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a).ToList();
                    foreach(var edit in editseq)
                    {
                        if (edit.CanEdit == true)
                            return dbsinner;
                    }
                    if (admin)
                        return dbsinner;
                    if (dbsinner.MyGroup != null)
                    {
                        if (!String.IsNullOrEmpty(dbsinner.MyGroup.MyAdminIdentityRole))
                        {
                            var localadmins = await _userManager.GetUsersInRoleAsync(dbsinner.MyGroup.MyAdminIdentityRole);
                            if (localadmins.Contains(user))
                                return dbsinner;
                        }
                    }
                    throw new ChummerHub.NoUserRightException(user.UserName, dbsinner.Id);
                }
                return null;
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;

                HubException hue = new HubException("Exception in CheckIfUpdateSINnerFile: " + e.Message, e);
                throw hue;
            }
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
