using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using ChummerHub.Models.V1.Examples;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

//using Swashbuckle.AspNetCore.Filters;

namespace ChummerHub.Controllers.V1
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [ControllerName("SINner")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController'
    public class SINnerController : ControllerBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController'
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TelemetryClient tc;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.SINnerController(ApplicationDbContext, ILogger<SINnerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
        public SINnerController(ApplicationDbContext context,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.SINnerController(ApplicationDbContext, ILogger<SINnerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
            ILogger<SINnerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetry)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            tc = telemetry;
        }


        private System.Net.Http.HttpClient MyHttpClient { get; } = new System.Net.Http.HttpClient();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.~SINnerController()'
        ~SINnerController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.~SINnerController()'
        {
            MyHttpClient?.Dispose();
        }

        // GET: api/ChummerFiles/5
        //[Route("download")]
        //[SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
#pragma warning disable CS1572 // XML comment has a param tag for 'id', but there is no parameter by that name
        /// <summary>
        /// Returns the Chummer-Save-File
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{sinnerid}")]
#pragma warning restore CS1572 // XML comment has a param tag for 'id', but there is no parameter by that name
        [AllowAnonymous]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDownloadFile")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
        [EnableCors("AllowAllOrigins")]
        [Produces(@"application/json", @"application/octet-stream")]
#pragma warning disable CS1573 // Parameter 'sinnerid' has no matching param tag in the XML comment for 'SINnerController.GetDownloadFile(Guid)' (but other parameters do)
        public async Task<FileResult> GetDownloadFile([FromRoute] Guid sinnerid)
#pragma warning restore CS1573 // Parameter 'sinnerid' has no matching param tag in the XML comment for 'SINnerController.GetDownloadFile(Guid)' (but other parameters do)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("ModelState is invalid!");
                }

                var chummerFile = await _context.SINners
                    .Include(a => a.MyGroup)
                    .Include(a => a.SINnerMetaData.Visibility)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights).Where(a => a.Id == sinnerid).FirstOrDefaultAsync();
                if (chummerFile == null)
                {
                    throw new ArgumentException("Could not find id " + sinnerid.ToString());
                }
                if (string.IsNullOrEmpty(chummerFile.DownloadUrl))
                {
                    string msg = "Chummer " + chummerFile.Id + " does not have a valid DownloadUrl!";
                    throw new ArgumentException(msg);
                }
                ApplicationUser user = null;
                if (!string.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                bool oktoDownload = chummerFile.SINnerMetaData.Visibility.IsPublic
                                    || chummerFile.MyGroup != null && chummerFile.MyGroup.IsPublic
                                    || chummerFile.SINnerMetaData.Visibility.UserRights.Any(a => a.EMail == null)
                                    || !string.IsNullOrEmpty(user?.Email) && chummerFile.SINnerMetaData.Visibility.UserRights.Any(a => user.Email.Equals(a.EMail, StringComparison.OrdinalIgnoreCase));

                if (!oktoDownload)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("SeeAllSINners"))
                        oktoDownload = true;
                }
                if (!oktoDownload)
                {
                    throw new ArgumentException("User " + user?.UserName + " or public is not allowed to download " + sinnerid.ToString() + " (Codepoint 2).");
                }

                //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), chummerFile.Id.ToString() + ".chum5z");
                var stream = await MyHttpClient.GetStreamAsync(new Uri(chummerFile.DownloadUrl));
                string downloadname = chummerFile.Id.ToString() + ".chum5z";
                try
                {
                    chummerFile.LastDownload = DateTime.Now;
                    await _context.SaveChangesAsync();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("GetDownloadFile");
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerId", sinnerid.ToString());
                    telemetry.Metrics.Add("FileSize", stream.Length);
                    tc.TrackEvent(telemetry);
                }
                catch (Exception e)
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINnerGroupFromSINerById(Guid)'
        public async Task<ActionResult<ResultSinnerGetSINnerGroupFromSINerById>> GetSINnerGroupFromSINerById([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINnerGroupFromSINerById(Guid)'
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

                var group = await _context.SINners
                    .Include(a => a.MyGroup)
                    .Include(b => b.MyGroup.MySettings).FirstOrDefaultAsync(a => a.Id == id);


                if (group == null)
                {
                    return NoContent();
                }
                else
                {
                    res = new ResultSinnerGetSINnerGroupFromSINerById(group.MyGroup);
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINById(Guid)'
        public async Task<ActionResult<ResultSinnerGetSINById>> GetSINById([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINById(Guid)'
        {
            ResultSinnerGetSINById res;
            try
            {

                ApplicationUser user = null;
                if (!string.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sin = await _context.SINners.AsNoTracking()
                    .Include(a => a.SINnerMetaData.Visibility)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights).AsNoTracking()
                    .Include(a => a.MyGroup).AsNoTracking()
                    .Include(b => b.MyGroup.MySettings).AsNoTracking()
                    .Where(a => a.Id == id).Take(1)
                    .FirstOrDefaultAsync(a => a.Id == id);
                res = new ResultSinnerGetSINById(sin);
                if (sin == null)
                {
                    return NotFound(res);
                }
                bool oktoDownload = sin.SINnerMetaData.Visibility.IsPublic
                                    || (sin.MyGroup != null && sin.MyGroup.IsPublic)
                                    || (user != null
                                        && sin.SINnerMetaData.Visibility.UserRights.Any(a => user.Email.Equals(a.EMail, StringComparison.OrdinalIgnoreCase))
                                    || sin.SINnerMetaData.Visibility.UserRights.Count == 0);
                if (!oktoDownload)
                {
                    var e = new ArgumentException("User " + user?.UserName + " or public is not allowed to download " + id + " (Codepoint 1).");
                    res = new ResultSinnerGetSINById(e);
                    return BadRequest(res);
                }
                sin.LastDownload = DateTime.Now;
                await _context.SaveChangesAsync();
                res = new ResultSinnerGetSINById(sin);
                if (sin.SINnerMetaData.Visibility.IsPublic)
                    return Ok(res);


                if (user != null
                    && (sin.SINnerMetaData.Visibility.UserRights.Any(a => user.NormalizedEmail.Equals(a.EMail, StringComparison.OrdinalIgnoreCase))
                        || (sin.SINnerMetaData.Visibility.UserRights.Count == 0))
                    )
                    return Ok(res);

                var e1 = new NoUserRightException("SINner is not viewable for public or groupmembers.");
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
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetSINnerVisibilityById")]
        [ProducesResponseType(typeof(ResultSinnerGetSINnerVisibilityById), (int)HttpStatusCode.OK)]
        [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINnerVisibilityById(Guid)'
        public async Task<ActionResult<ResultSinnerGetSINnerVisibilityById>> GetSINnerVisibilityById([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetSINnerVisibilityById(Guid)'
        {
            ResultSinnerGetSINnerVisibilityById res;
            try
            {

                ApplicationUser user = null;
                if (!string.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var list = await _context.UserRights
                    .Where(a => a.SINnerId == id)
                    .ToListAsync();
                res = new ResultSinnerGetSINnerVisibilityById(list);
                if (list == null || list.Count == 0)
                {
                    return NotFound(res);
                }
                return Ok(res);
            }
            catch (Exception e)
            {
                res = new ResultSinnerGetSINnerVisibilityById(e);
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
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetOwnedSINByAlias(string)'
        public async Task<ActionResult<ResultSinnerGetOwnedSINByAlias>> GetOwnedSINByAlias([FromRoute] string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.GetOwnedSINByAlias(string)'
        {
            ResultSinnerGetOwnedSINByAlias res;
            try
            {

                ApplicationUser user = null;
                if (!string.IsNullOrEmpty(User?.Identity?.Name))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var sinseq = await _context.SINners
                    //.Include(a => a.MyExtendedAttributes)
                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(a => a.MyGroup)
                    .Include(b => b.MyGroup.MySettings)
                    .Where(a => a.Alias == id).ToListAsync();
                if (sinseq.Count == 0)
                {
                    var e = new ArgumentException("SINner with Alias " + id + " does not exist.");
                    res = new ResultSinnerGetOwnedSINByAlias(e);
                    return NotFound(res);
                }
                List<SINner> download = new List<SINner>();
                foreach (var sin in sinseq)
                {
                    if (sin.SINnerMetaData.Visibility.UserRights.Any(a => a.EMail == null && a.CanEdit))
                        download.Add(sin);
                    else if (user != null
                             && sin.SINnerMetaData.Visibility.UserRights.Any(a => user.Email.Equals(a.EMail, StringComparison.OrdinalIgnoreCase)))
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
        [AllowAnonymous]
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
                if ((User?.Identity != null) && (User.Identity.IsAuthenticated))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                dbsinner = await CheckIfUpdateSINnerFile(id, user);
                if (dbsinner == null)
                {
                    var e = new NoUserRightException("You may not edit this (existing) sinner with id " + id + ".");
                    res = new ResultSINnerPut(e);
                    return Conflict(res);
                }
                sin.GoogleDriveFileId = dbsinner.GoogleDriveFileId;

                //if(user == null)
                //{
                //    var e =  new NoUserRightException("User not found!");
                //    res = new ResultSINnerPut(e);
                //    return NotFound(res);
                //}

                sin.DownloadUrl = Startup.GDrive.StoreXmlInCloud(sin, uploadedFile);
                dbsinner.DownloadUrl = sin.DownloadUrl;
                //_context.Entry(dbsinner).CurrentValues.SetValues(sin);
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.EventTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.EventTelemetry("PutStoreXmlInCloud");
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("LastChange", sin?.LastChange.ToString());
                    telemetry.Properties.Add("SINnerId", sin.Id.ToString());
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
                    dbsinner.LastChange = sin.LastChange;
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
            catch (NoUserRightException e)
            {
                res = new ResultSINnerPut(e);
                return BadRequest(res);
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerId", dbsinner?.Id?.ToString());
                    telemetry.Properties.Add("FileName", uploadedFile.FileName);
                    telemetry.Metrics.Add("FileSize", uploadedFile.Length);
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultSINnerPut(e);
                return BadRequest(res);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SINnerId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GetThumbnailById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetThumbnailById(Guid? SINnerId, int? index)
        {
            var temppath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            string filename = SINnerId != null ? SINnerId.Value.ToString() : string.Empty;
            filename += Guid.NewGuid().ToString() + ".zip";
            string filepath = Path.Combine(temppath, filename);
            try
            {
                if (index == null)
                    index = 0;
                var sinnerseq = await _context.SINners.Where(a => a.Id == SINnerId).ToListAsync();
                if (sinnerseq.Count == 0)
                    return NotFound("SINner " + SINnerId + " not found!");
                var net = new WebClient();

                if (System.IO.File.Exists(filepath))
                {
                    FileInfo fi = new FileInfo(filepath);
                    if (fi.CreationTimeUtc < DateTime.UtcNow - TimeSpan.FromHours(1))
                        System.IO.File.Delete(filepath);
                }

                if (!System.IO.File.Exists(filename))
                {
                    Uri downloadUri = new Uri(sinnerseq.FirstOrDefault()?.DownloadUrl ?? string.Empty);
                    net.DownloadFile(downloadUri, filename);
                }

                if (!System.IO.File.Exists(filename))
                {
                    return NotFound("Could not download sinner " + SINnerId + " from "
                                    + (sinnerseq.FirstOrDefault()?.DownloadUrl ?? string.Empty) + ".");
                }

                using (var file = System.IO.File.OpenRead(filename))
                {
                    using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            using (var stream = entry.Open())
                            {
                                try
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.Load(stream);
                                    var node = doc.SelectSingleNode(
                                        "/*[local-name()='character']/*[local-name()='mugshots']");
                                    if (node == null)
                                        return NotFound("SINner found but he/she has no MugShot saved.");
                                    if (node.ChildNodes.Count <= index.Value)
                                        return NotFound("SINner found but he/she has only " + node.ChildNodes.Count +
                                                        "  Mugshots.");
                                    var mugchild = node.ChildNodes.Item(index.Value);
                                    if (mugchild != null)
                                    {
                                        //now we need to get the value of the Childnode
                                        if (mugchild.FirstChild != null)
                                        {
                                            byte[] bytes = Convert.FromBase64String(mugchild.FirstChild.Value);
                                            return File(bytes, "image/jpeg");
                                        }
                                        else
                                        {
                                            throw new ArgumentNullException(nameof(index),
                                                "The FirstChild-Node with this index seems to be null!");
                                        }
                                    }
                                    else
                                    {
                                        throw new ArgumentNullException(nameof(index),
                                            "Childnode with this index seems to be null!");
                                    }
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                                        Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                                            new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                                        telemetry.Properties.Add("SINnerId", SINnerId?.ToString());
                                        tc.TrackException(telemetry);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                }

                return NotFound("Could not find the mugshots-token.");
            }
            catch (Exception e)
            {
                try
                {
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                        new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("SINnerId", SINnerId?.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                return BadRequest(e);
            }
            finally
            {
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
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
                            msg += Environment.NewLine + "\t" + singleerr;
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
                        await _context.UploadClients.AddAsync(uploadInfo.Client);
                    }
                    else
                    {
                        _context.UploadClients.Attach(uploadInfo.Client);
                        _context.Entry(uploadInfo.Client).State = EntityState.Modified;
                        _context.Entry(uploadInfo.Client).CurrentValues.SetValues(uploadInfo.Client);
                    }
                }
                var returncode = HttpStatusCode.OK;
                if ((User != null) && (User.Identity.IsAuthenticated))
                    user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                foreach (var tempsinner in uploadInfo.SINners)
                {
                    sinner = tempsinner;
                    if (sinner.Id.ToString() == "string")
                        sinner.Id = Guid.Empty;

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
                    foreach (var tag in sinner.SINnerMetaData.Tags)
                    {
                        if (tag == null)
                            continue;
                        tag.TagValueFloat = null;
                        if (float.TryParse(tag.TagValue, out float result))
                        {
                            tag.TagValueFloat = result;
                        }
                        if (tag.TagValueFloat != null && float.IsNaN(tag.TagValueFloat.Value))
                            tag.TagValueFloat = null;
                    }

                    Guid? guiSinnerId = sinner.Id; // Separate definition prevents funny business with async
                    var oldsinner = await _context.SINners.AsNoTracking()
                        .Include(a => a.SINnerMetaData).AsNoTracking()
                        .Include(a => a.SINnerMetaData.Visibility).AsNoTracking()
                        .Include(a => a.SINnerMetaData.Visibility.UserRights).AsNoTracking()
                        .Include(b => b.MyGroup).AsNoTracking()
                        .Where(a => a.Id == guiSinnerId).FirstOrDefaultAsync();
                    SINnerGroup oldgroup = null;
                    if (oldsinner != null)
                    {
                        var canedit = await CheckIfUpdateSINnerFile(oldsinner.Id.Value, user);
                        if (canedit == null)
                        {
                            string msg = "SINner " + sinner.Id + " is not editable for user " + user?.Email + ".";
                            if (user == null)
                                msg = "SINner " + sinner.Id + " is not editable for anonymous users.";
                            var e = new NoUserRightException(msg);
                            res = new ResultSinnerPostSIN(e);
                            return BadRequest(res);

                        }
                        oldgroup = oldsinner.MyGroup;
                        oldsinner.MyGroup = null;
                        var olduserrights = oldsinner.SINnerMetaData.Visibility.UserRights.ToList();
                        oldsinner.SINnerMetaData.Visibility.UserRights.Clear();
                        _context.UserRights.RemoveRange(olduserrights);
                                                //check if ANY visibility-data was uploaded
                        if (sinner.SINnerMetaData.Visibility.UserRights.Count > 0)
                        {
                            bool userfound = false;
                            foreach (var ur in sinner.SINnerMetaData.Visibility.UserRights)
                            {
                                if (ur.EMail.Equals(user?.Email, StringComparison.OrdinalIgnoreCase))
                                {
                                    ur.CanEdit = true;
                                    userfound = true;
                                }

                                ur.Id = Guid.NewGuid();
                                ur.SINnerId = sinner.Id;
                                await _context.UserRights.AddAsync(ur);
                            }
                            if (!userfound)
                            {
                                SINnerUserRight ownUser = new SINnerUserRight
                                {
                                    Id = Guid.NewGuid(),
                                    SINnerId = sinner.Id,
                                    CanEdit = true,
                                    EMail = user?.Email
                                };
                                sinner.SINnerMetaData.Visibility.UserRights.Add(ownUser);
                                await _context.UserRights.AddAsync(ownUser);
                            }
                        }
                        else
                        {
                            //no userrights where uploaded.
                            sinner.SINnerMetaData.Visibility.UserRights = olduserrights;
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
                            if (ur.EMail.ToLowerInvariant() == user?.Email.ToLowerInvariant())
                                ownuserfound = true;
                        }
                        if (!ownuserfound)
                        {
                            SINnerUserRight ownright = new SINnerUserRight
                            {
                                CanEdit = true,
                                EMail = user?.Email,
                                SINnerId = sinner.Id,
                                Id = Guid.NewGuid()
                            };
                            sinner.SINnerMetaData.Visibility.UserRights.Add(ownright);
                        }
                    }

                    sinner.SINnerMetaData.Tags.RemoveAll(a => a == null);
                    foreach (Tag tag in sinner.SINnerMetaData.Tags)
                    {
                        tag?.SetSinnerIdRecursive(sinner.Id);
                    }

                    sinner.UploadClientId = uploadInfo.Client.Id;
                    SINner dbsinner = await CheckIfUpdateSINnerFile(sinner.Id.Value, user, true);
                    
                    if (dbsinner != null)
                    {
                        if (oldgroup == null && dbsinner.MyGroup != null)
                            oldgroup = dbsinner.MyGroup;
                        if (string.IsNullOrEmpty(sinner.GoogleDriveFileId))
                            sinner.GoogleDriveFileId = dbsinner.GoogleDriveFileId;
                        if (string.IsNullOrEmpty(sinner.DownloadUrl))
                            sinner.DownloadUrl = dbsinner.DownloadUrl;
                        
                        sinner.MyGroup = oldgroup;


                        var alltags = _context.Tags.Where(a => a != null && a.SINnerId == dbsinner.Id).ToArray();

                        _context.Tags.RemoveRange(alltags);
                        _context.UserRights.RemoveRange(dbsinner.SINnerMetaData.Visibility.UserRights);
                        _context.SINnerVisibility.Remove(dbsinner.SINnerMetaData.Visibility);
                        _context.SINnerMetaData.Remove(dbsinner.SINnerMetaData);
                        _context.SINners.Remove(dbsinner);
                        if (oldgroup != null)
                            _context.Entry(oldgroup).State = EntityState.Detached;

                        dbsinner.SINnerMetaData.Visibility.UserRights.Clear();
                        dbsinner.SINnerMetaData.Visibility.UserRights = null;
                        dbsinner.SINnerMetaData.Visibility = null;
                        dbsinner.SINnerMetaData.Tags = null;
                        dbsinner.SINnerMetaData = null;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            foreach (var entry in ex.Entries)
                            {
                                if (entry.Entity is SINner
                                    || entry.Entity is Tag
                                    || entry.Entity is SINnerGroup
                                    || entry.Entity is SINnerUserRight
                                    || entry.Entity is SINnerMetaData)
                                {
                                    try
                                    {
                                        Utils.DbUpdateExceptionHandler(entry, _logger);
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
                                        "(Codepoint 1) Don't know how to handle concurrency conflicts for "
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

                        try
                        {
                            await _context.SINners.AddAsync(sinner);
                        }
                        catch(InvalidOperationException e)
                        {
                            var hub = new HubException("InvalidOperationException while adding sinner to context: " + e.Message, e);
                            hub.Data.Add("oldsinner", oldsinner?.Id);
                            hub.Data.Add("oldgroup", oldgroup?.Id);
                            _logger.LogError(e, e.Message);
                            throw hub;
                        }
                        string msg = "Sinner " + sinner.Id + " updated: " + _context.Entry(dbsinner).State.ToString();
                        msg += Environment.NewLine + Environment.NewLine + "LastChange: " + dbsinner.LastChange;
                        _logger.LogInformation(msg);
                        UpdateEntityEntries(sinner.SINnerMetaData.Tags);
                    }
                    else
                    {
                        returncode = HttpStatusCode.Created;
                        sinner.MyGroup = null;
                        await _context.SINners.AddAsync(sinner);
                    }

                    if (sinner.MyGroup?.Id != null && sinner.MyGroup?.Id != Guid.Empty)
                    {
                        if (user.FavoriteGroups.All(a => a.FavoriteGuid != sinner.MyGroup.Id))
                        {
                            user.FavoriteGroups.Add(new ApplicationUserFavoriteGroup
                            {
                                FavoriteGuid = sinner.MyGroup.Id.Value
                            });
                        }
                    }

                    if (user != null)
                        user.FavoriteGroups = user.FavoriteGroups.GroupBy(a => a.FavoriteGuid).Select(b => b.First()).ToList();

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (oldgroup != null)
                        {
                            var roles = await _userManager.GetRolesAsync(user);
                            await SINnerGroupController.PutSiNerInGroupInternal(oldgroup.Id.Value, sinner.Id.Value,
                                user, _context,
                                _logger, oldgroup.PasswordHash, roles, tc);
                        }
                        await _context.SaveChangesAsync();
                        if (oldsinner == null && user.FavoriteGroups != null)
                        {
                            if (user.FavoriteGroups.All(a => a.FavoriteGuid != sinner.Id))
                            {
                                user.FavoriteGroups.Add(new ApplicationUserFavoriteGroup
                                {
                                    FavoriteGuid = sinner.Id.Value
                                });
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is SINner || entry.Entity is Tag || entry.Entity is SINnerMetaData)
                            {
                                try
                                {
                                    Utils.DbUpdateExceptionHandler(entry, _logger);
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
                                    "(Codepoint 4) Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                                res = new ResultSinnerPostSIN(e);
                                return BadRequest(res);
                            }
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        res = new ResultSinnerPostSIN(ex);
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is SINner || entry.Entity is Tag || entry.Entity is SINnerMetaData)
                            {
                                try
                                {
                                    Utils.DbUpdateExceptionHandler(entry, _logger);
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
                                    "(Codepoing 5) Don't know how to handle concurrency conflicts for "
                                    + entry.Metadata.Name);
                                res = new ResultSinnerPostSIN(e);
                                return BadRequest(res);
                            }
                        }
                        try
                        {
                            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                            Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                                new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                            telemetry.Properties.Add("User", user?.Email);
                            telemetry.Properties.Add("SINnerId", sinner?.Id?.ToString());
                            tc.TrackException(telemetry);
                        }
                        catch (Exception ex1)
                        {
                            _logger.LogError(ex1.ToString());
                        }

                        return BadRequest(res);
                    }
                    catch (SqlException ex)
                    {
                        res = new ResultSinnerPostSIN(ex);
                        try
                        {
                            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                            Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry =
                                new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                            telemetry.Properties.Add("User", user?.Email);
                            telemetry.Properties.Add("SINnerId", sinner?.Id?.ToString());
                            telemetry.Properties.Add("Procedure", ex.Procedure);
                            StringBuilder allerrors = new StringBuilder();
                            foreach (var error in ex.Errors)
                            {
                                allerrors.AppendLine(error.ToString());
                            }
                            telemetry.Properties.Add("Errors", allerrors.ToString());
                            tc.TrackException(telemetry);
                        }
                        catch (Exception ex1)
                        {
                            _logger.LogError(ex1.ToString());
                        }

                        return BadRequest(res);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                            Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                            telemetry.Properties.Add("User", user?.Email);
                            telemetry.Properties.Add("SINnerId", sinner?.Id?.ToString());
                            tc.TrackException(telemetry);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                        res = new ResultSinnerPostSIN(e);
                        return Conflict(res);
                    }
                }

                List<SINner> sinlist = new List<SINner>();
                foreach (var id in uploadInfo.SINners.Where(a => a.Id != null).Select(a => a.Id.Value))
                {
                    var sin = _context.SINners.FirstOrDefault(a => a.Id == id);
                    if (sin != null)
                        sinlist.Add(sin);
                }
                res = new ResultSinnerPostSIN(sinlist);
                switch (returncode)
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
                    //var tc = new Microsoft.ApplicationInsights.TelemetryClient();
                    Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(e);
                    telemetry.Properties.Add("User", user?.Email);
                    telemetry.Properties.Add("SINnerId", sinner?.Id?.ToString());
                    tc.TrackException(telemetry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                res = new ResultSinnerPostSIN(e);
                return BadRequest(res);
            }
        }



        private void UpdateEntityEntries(IEnumerable<Tag> taglist)
        {
            foreach (var item in taglist)
            {
                if (!_context.Tags.Contains(item))
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
        /// Alternatively, the DownloadUrl can be set directly from the Client.
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
        [AllowAnonymous]
        public async Task<ActionResult<ResultSinnerPostSIN>> PostSIN([FromBody] UploadInfoObject uploadInfo)
        {
            try
            {
                return await PostSINnerInternal(uploadInfo);
            }
            catch (Exception e)
            {
                int i = 0;
                if (uploadInfo?.SINners != null)
                {
                    foreach (var sin in uploadInfo.SINners)
                    {
                        e.Data.Add("SINner_" + i++, sin.Id);
                        e.Data.Add("InstallationId", uploadInfo.Client.InstallationId);
                    }
                }

                tc.TrackException(e);
                throw;
            }

        }

        // DELETE: api/ChummerFiles/5
        [HttpDelete("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerDelete")]
        [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.Delete(Guid)'
        public async Task<ActionResult<ResultSinnerDelete>> Delete([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerController.Delete(Guid)'
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
                var olduserrights = await _context.UserRights.Where(a => a.SINnerId == sinner.Id).ToListAsync();
                _context.UserRights.RemoveRange(olduserrights);
                var oldtags = await _context.Tags.Where(a => a.SINnerId == sinner.Id).ToListAsync();
                _context.Tags.RemoveRange(oldtags);
                var oldsinners = await _context.SINners.Where(a => a.Id == sinner.Id).ToListAsync();
                foreach (var oldsin in oldsinners)
                {
                    if (_context.SINnerVisibility.Contains(oldsin.SINnerMetaData.Visibility))
                        _context.SINnerVisibility.Remove(oldsin.SINnerMetaData.Visibility);
                    if (_context.SINnerMetaData.Contains(oldsin.SINnerMetaData))
                        _context.SINnerMetaData.Remove(oldsin.SINnerMetaData);
                    //if (_context.SINnerExtendedMetaData.Contains(oldsin.MyExtendedAttributes))
                    //    _context.SINnerExtendedMetaData.Remove(oldsin.MyExtendedAttributes);
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

        private async Task<SINner> CheckIfUpdateSINnerFile(Guid id, ApplicationUser user, bool allincludes = false)
        {
            try
            {
                return await CheckIfUpdateSINnerFileInternal(id, user, allincludes);
            }
            catch (Exception e)
            {
                if (e is HubException)
                    throw;

                HubException hue = new HubException("Exception in CheckIfUpdateSINnerFile: " + e.Message, e);
                throw hue;
            }
        }

        private async Task<SINner> CheckIfUpdateSINnerFileInternal(Guid id, ApplicationUser user, bool allincludes)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "Id is empty!");
            }

            SINner dbsinner = null;
            if (allincludes)
            {
                dbsinner = await _context.SINners.AsNoTracking()
                    .Include(a => a.SINnerMetaData).AsNoTracking()
                    .Include(a => a.SINnerMetaData.Visibility).AsNoTracking()
                    .Include(a => a.SINnerMetaData.Visibility.UserRights).AsNoTracking()
                    .Include(a => a.MyGroup).AsNoTracking().Where(a => a.Id == id).FirstOrDefaultAsync();
            }
            else
            {
                dbsinner = await _context.SINners.AsNoTracking()
                    .Include(a => a.MyGroup).AsNoTracking().Where(a => a.Id == id).FirstOrDefaultAsync();
            }

            if (dbsinner == null)
                return null;
            string normEmail = user?.NormalizedEmail;
            string userName = user?.UserName;
            var ur = _context.UserRights.AsNoTracking().Where(a => a.SINnerId == id).ToList();
            if (ur.Any(a => ((!string.IsNullOrEmpty(a.EMail)
                              && a.EMail.ToUpperInvariant() == normEmail)
                             || a.EMail == null)
                            && a.CanEdit))
            {
                return dbsinner;
            }
            if (ur.Count == 0)
                return dbsinner;
            if (dbsinner.MyGroup != null)
            {
                if (!string.IsNullOrEmpty(dbsinner.MyGroup.MyAdminIdentityRole))
                {
                    var localadmins = await _userManager.GetUsersInRoleAsync(dbsinner.MyGroup.MyAdminIdentityRole);
                    if (localadmins.Contains(user))
                        return dbsinner;
                }
                if (!string.IsNullOrEmpty(dbsinner.MyGroup.GroupCreatorUserName))
                {
                    if (dbsinner.MyGroup.GroupCreatorUserName == userName)
                        return dbsinner;
                }
            }
            List<SINnerUserRight> ur2 = null;
            if (!allincludes && (dbsinner.SINnerMetaData?.Visibility?.UserRights == null))
            {
                dbsinner = await _context.SINners.AsNoTracking()
                    .Include(a => a.SINnerMetaData).AsNoTracking()
                    .Include(a => a.SINnerMetaData.Visibility).AsNoTracking()
                    .Include(a => a.SINnerMetaData.Visibility.UserRights).AsNoTracking()
                    //.Include(a => a.MyExtendedAttributes)
                    .Include(a => a.MyGroup).AsNoTracking().Where(a => a.Id == id).FirstOrDefaultAsync();
                ur2 = _context.UserRights.AsNoTracking().Where(a => a.SINnerId == id).ToList();
                if (ur2.Any(a => ((!string.IsNullOrEmpty(a.EMail)
                                  && a.EMail.ToUpperInvariant() == normEmail)
                                 || a.EMail == null)
                                && a.CanEdit))
                {
                    return dbsinner;
                }
                if (ur2.Count == 0)
                    return dbsinner;
            }
            
            throw new NoUserRightException(userName, dbsinner.Id, "ur2.Count == " + ur2?.Count);
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
