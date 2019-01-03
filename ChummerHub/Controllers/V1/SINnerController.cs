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


        private static System.Net.Http.HttpClient MyHttpClient { get; } = new System.Net.Http.HttpClient();

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
        public async Task<IActionResult> GetById([FromRoute] Guid id)
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
                //sin.SINnerMetaData.Tags = await (from a in _context.Tags.Include(b => b.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                                 .ThenInclude(t => t.Tags)
                //                        where a.SINnerId == sin.Id
                //                        select a).ToListAsync();

                if (sin.SINnerMetaData.Visibility.IsPublic == true)
                    return Ok(sin);

                //sin.SINnerMetaData.Visibility.UserRights = await (from a in _context.UserRights where a.SINnerId == sin.Id select a).ToListAsync();
                
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
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NoContent)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerPut")]

        [Authorize]
        public async Task<IActionResult> Put([FromRoute] Guid id, IFormFile uploadedFile)
        {
            try
            {
                var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);
                if (sin == null)
                {
                    return NotFound();
                }
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var check = await CheckIfUpdateSINnerFile(id, user);
                if (!check)
                {
                    return Conflict("CheckIfUpdateSINnerFile");
                }
                sin.DownloadUrl = Startup.GDrive.StoreXmlInCloud(sin, uploadedFile);
                _context.Entry(sin).CurrentValues.SetValues(sin);
                
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
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in PutSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

       

        private async Task<IActionResult> PostSINnerInternal(UploadInfoObject uploadInfo)
        {
            _logger.LogTrace("Post SINnerInternalt: " + uploadInfo + ".");
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
                        _context.Entry(uploadInfo.Client).CurrentValues.SetValues(uploadInfo.Client);
                    }
                }
                var returncode = HttpStatusCode.OK;
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                foreach (var sinner in uploadInfo.SINners)
                {

                    if (sinner.Id.ToString() == "string")
                        sinner.Id = Guid.Empty;

                    foreach (var ur in sinner.SINnerMetaData.Visibility.UserRights)
                    {
                        ur.Id = Guid.NewGuid();
                        ur.SINnerId = sinner.Id;
                    }

                    foreach (var tag in sinner.SINnerMetaData.Tags)
                        tag.SINnerId = sinner.Id;

                    sinner.UploadClientId = uploadInfo.Client.Id;
                    var check = await CheckIfUpdateSINnerFile(sinner.Id.Value, user);
                    if (check)
                    {
                        _context.Entry(sinner).CurrentValues.SetValues(sinner);
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
                catch(Exception e)
                {
                    HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                    var msg = new System.Net.Http.HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = hue.Message };
                    return Conflict(msg);
                }
                List<Guid> myids = (from a in uploadInfo.SINners select a.Id.Value).ToList();
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted("PostSINnerFile", new { Ids = myids });
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostSINnerFile", new { Ids = myids });
                    default:
                        break;
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private void UpdateEntityEntries(List<Tag> taglist)
        {
            foreach (var item in taglist)
            {
                _context.Entry(item).CurrentValues.SetValues(item);
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
        [Authorize]
        [SwaggerRequestExample(typeof(UploadInfoObject), typeof(UploadInfoObjectExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SINnerUpload")]
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created, Type = typeof(SINner))]

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
                var sin = await _context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights).FirstOrDefaultAsync(a => a.Id == id);

                if (sin == null)
                {
                    return NotFound();
                }
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                var check = await CheckIfUpdateSINnerFile(id, user);
                if (!check)
                {
                    return BadRequest("not authorized");
                }

                _context.SINners.Remove(sin);
                _context.UserRights.RemoveRange(sin.SINnerMetaData.Visibility.UserRights);
                await _context.SaveChangesAsync();

                return Ok("deleted");
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in DeleteSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private async Task<bool> CheckIfUpdateSINnerFile(Guid id, ApplicationUser user)
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
                var existingseq = _context.SINners.Where(e => e.Id == id);
                foreach(var existing in existingseq)
                {
                    var editseq = (from a in existing.SINnerMetaData.Visibility.UserRights where a.EMail.ToUpperInvariant() == user.NormalizedEmail select a).ToList();
                    foreach(var edit in editseq)
                    {
                        if (edit.CanEdit == true)
                            return true;
                    }
                    if (admin)
                        return true;
                }
                
                
                return false;
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
