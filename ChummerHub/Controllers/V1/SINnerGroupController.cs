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
        /// <param name="uploadInfo"></param>
        /// <returns></returns>
        [HttpPost()]
        [SwaggerRequestExample(typeof(UploadInfoObject), typeof(UploadInfoObjectExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Accepted)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Conflict)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("PostGroup")]
        [Authorize]
        public async Task<IActionResult> PostGroup([FromBody] string Groupname, Guid SinnerId)
        {
            _logger.LogTrace("Post SINnerGroupInternal: " + Groupname + " (" + SinnerId + ").");
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
                    return new BadRequestObjectResult(new HubException(msg));
                }
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
                    SINnerGroup group = new SINnerGroup();
                    group.Groupname = Groupname;
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
                }
                switch(returncode)
                {
                    case HttpStatusCode.OK:
                        return Accepted("PostSINnerGroup", Groupname);
                    case HttpStatusCode.Created:
                        return CreatedAtAction("PostSINnerGroup", Groupname);
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
                HubException hue = new HubException("Exception in PostSINnerGroup: " + e.Message, e);
                return new BadRequestObjectResult(hue);
              
            }
        }

    }
}

