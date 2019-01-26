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
    [ControllerName("SINner")]
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
        [Authorize]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDownloadFile")]
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, type: typeof(FileResult))]
        //[ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(FileContentResult), 200)]
        [Produces(@"application/json", @"application/octet-stream")]
        public async Task<FileResult> GetDownloadFile([FromRoute] Guid sinnerid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("ModelState is invalid!");
                }

                var chummerFile = await _context.SINners.FindAsync(sinnerid);

                if (chummerFile == null)
                {
                    throw new ArgumentException("Could not find id " + sinnerid.ToString());
                }
                if (String.IsNullOrEmpty(chummerFile.DownloadUrl))
                {
                    string msg = "Chummer " + chummerFile.Id + " does not have a valid DownloadUrl!";
                    throw new ArgumentException(msg);
                }
                //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), chummerFile.Id.ToString() + ".chum5z");
                var stream = await MyHttpClient.GetStreamAsync(new Uri(chummerFile.DownloadUrl));
                string downloadname = chummerFile.Id.ToString() + ".chum5z";
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                if(user == null)
                {
                    throw new NoUserRightException("User not found!");
                }
                try
                {
                    var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("GetDownloadFile");
                    telemetry.Properties.Add("User", user.Email);
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

               
                //using (var client = new WebClient())
                //{
                //    client.DownloadFile(new Uri(chummerFile.DownloadUrl), path);
                //}
                //if (!System.IO.File.Exists(path))
                //{
                //    string msg = "No file downloaded from " + chummerFile.DownloadUrl;
                //    return BadRequest(msg);
                //}
                
                ////var res = new FileStreamResult(new MemoryStream(path), "application/octet-stream");
                ////var res = new FileResult(downloadname, path, "application/octet-stream");
                ////var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                ////var reader = new StreamReader(stream, true);
                //return PhysicalFile(path, "application/octet-stream", downloadname);
                ////return File(path, "application/octet-stream");
                ////return new ObjectResult(reader.BaseStream);
            }
            catch (Exception e)
            {
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
                HubException hue = new HubException("Exception in GetSINner: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerGet")]
        [Authorize]
        public async Task<ActionResult<SINner>> GetById([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a=> a.Id == id);
                if (sin == null)
                {
                    return NotFound();
                }
               
                if (sin.SINnerMetaData.Visibility.IsPublic == true)
                    return Ok(sin);

                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return BadRequest("Could not find user: " + User.Identity.Name);
                }
                var list = (from a in sin.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a);
                if (list.Any())
                    return Ok(sin);
                if (sin.SINnerMetaData.Visibility.IsGroupVisible)
                {
                    if (sin.SINnerMetaData.Visibility.Groupname?.ToUpperInvariant()
                        == user.Groupname?.ToUpperInvariant())
                    {
                        return Ok(sin);
                    }
                }

                return BadRequest("not authorized");
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in GetSINnerfile: " + e.Message, e);
                return BadRequest(hue);
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
        public async Task<IActionResult> Put([FromRoute] Guid id, IFormFile uploadedFile)
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
                //_logger.LogError("Could not store file on GDrive: " + e.ToString());
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
                    return new BadRequestObjectResult(new HubException(msg));
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

                    if(String.IsNullOrEmpty(sinner.JsonSummary))
                        return BadRequest("sinner " + sinner.Id +": JsonSummary == null");

                    if (sinner.SINnerMetaData.Visibility.UserRights.Any() == false)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": Visibility contains no entries!");
                    }

                    if (sinner.LastChange == null)
                    {
                        return BadRequest("Sinner  " + sinner.Id + ": LastChange not set!");
                    }
                    if (sinner.SINnerMetaData.Visibility.Id == null)
                    {
                        sinner.SINnerMetaData.Visibility.Id = Guid.NewGuid();
                    }
                    var olduserrights = await (from a in _context.UserRights where a.SINnerId == sinner.Id select a).ToListAsync();
                    _context.UserRights.RemoveRange(olduserrights);
                    foreach (var ur in sinner.SINnerMetaData.Visibility.UserRights)
                    {
                        ur.Id = Guid.NewGuid();
                        ur.SINnerId = sinner.Id;
                        _context.UserRights.Add(ur);
                    }

                    foreach(var tag in sinner.SINnerMetaData.Tags)
                    {
                        tag.SetSinnerIdRecursive(sinner.Id);
                    }

                    sinner.UploadClientId = uploadInfo.Client.Id;
                    SINner dbsinner = await CheckIfUpdateSINnerFile(sinner.Id.Value, user);
                    if (dbsinner != null)
                    {
                        _context.SINners.Attach(dbsinner);
                        if (String.IsNullOrEmpty(sinner.GoogleDriveFileId))
                            sinner.GoogleDriveFileId = dbsinner.GoogleDriveFileId;
                        if(String.IsNullOrEmpty(sinner.DownloadUrl))
                            sinner.DownloadUrl = dbsinner.DownloadUrl;
                        //_context.Entry(dbsinner.SINnerMetaData.Visibility).State = EntityState.Modified;
                        _context.Entry(dbsinner.SINnerMetaData.Visibility).CurrentValues.SetValues(sinner.SINnerMetaData.Visibility);

                        //_context.Tags.RemoveRange(dbsinner.AllTags);
                        _context.Entry(dbsinner).State = EntityState.Modified;
                        _context.Entry(dbsinner).CurrentValues.SetValues(sinner);
                        string msg = "Sinner " + sinner.Id + " updated: " + _context.Entry(dbsinner).State.ToString();
                        msg += Environment.NewLine + Environment.NewLine + "LastChange: " + dbsinner.LastChange;
                        _logger.LogError(msg);
                        List<Tag> taglist = sinner.SINnerMetaData.Tags;
                        UpdateEntityEntries(taglist);
                    }
                    else
                    {
                        returncode = HttpStatusCode.Created;
                        _context.SINners.Add(sinner);
                    }
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
                            DbUpdateConcurrencyExceptionHandler(entry);
                        }
                        else if (entry.Entity is Tag)
                        {
                            DbUpdateConcurrencyExceptionHandler(entry);
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
                        telemetry.Metrics.Add("TagCount", (double)sinner?.AllTags?.Count);
                        tc.TrackException(telemetry);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                    HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                    var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = hue.Message };
                    return Conflict(msg);
                }
                List<Guid> myids = (from a in uploadInfo.SINners select a.Id.Value).ToList();
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted("PostSINnerFile", myids);
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostSINnerFile", myids);
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
                    telemetry.Metrics.Add("TagCount", (double)sinner?.AllTags?.Count);
                    tc.TrackException(telemetry);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private void DbUpdateConcurrencyExceptionHandler(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var proposedValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();
            string msg = "";
            foreach(var property in proposedValues.Properties)
            {
                Object proposedValue = null;
                Object databaseValue = null;
                if(proposedValues?.Properties?.Contains(property) == true)
                    proposedValue = proposedValues[property];
                if(databaseValues?.Properties?.Contains(property) == true)
                    databaseValue = databaseValues[property];

                msg += Environment.NewLine + "property: " + property + Environment.NewLine;
                msg += "\tproposedValue: " + proposedValue + Environment.NewLine;
                msg += "\tdatabaseValue: " + databaseValue + Environment.NewLine;
                _logger.LogError(msg);
                // TODO: decide which value should be written to database
                // proposedValues[property] = <value to be saved>;
            }
            throw new NotSupportedException(
               "Don't know how to handle concurrency conflicts for "
               + entry.Metadata.Name + ": " + msg);
            // Refresh original values to bypass next concurrency check
            entry.OriginalValues.SetValues(databaseValues);
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
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created, Type = typeof(SINner))]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] UploadInfoObject uploadInfo)
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
                var dbsinner = await _context.SINners.FirstOrDefaultAsync(e => e.Id == id);
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
                    throw new ChummerHub.NoUserRightException(user.UserName, dbsinner.Id);
                }
                return null;
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in SINnerFileExists: " + e.Message, e);
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
