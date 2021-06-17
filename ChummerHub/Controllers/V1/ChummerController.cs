using ChummerHub.Data;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using ChummerHub.Models.V1;
using Newtonsoft.Json;

namespace ChummerHub.Controllers.V1
{
    [Route("[action]/{Hash}")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [ControllerName("Chummer")]
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController'
    public class ChummerController : Controller
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController'
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient tc;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController._context'
        public ApplicationDbContext _context = null;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController._context'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.ChummerController(ApplicationDbContext, ILogger<ChummerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
        public ChummerController(ApplicationDbContext context,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.ChummerController(ApplicationDbContext, ILogger<ChummerController>, SignInManager<ApplicationUser>, UserManager<ApplicationUser>, TelemetryClient)'
            ILogger<ChummerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetry)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            tc = telemetry;
        }

        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.RedirectKeepVerb)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)(HttpStatusCode.PermanentRedirect))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerO")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        public IActionResult O([FromRoute] string Hash, string open)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        {
            try
            {
                if (string.IsNullOrEmpty(Hash))
                    throw new ArgumentException("hash is empty: " + Hash);
                if (!_context.SINners.Any(a => a.Hash == Hash))
                {
                    foreach (var nullSinner in _context.SINners.Where(a => string.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC"))
                    {
                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                        tc?.TrackTrace(tt);
                    }
                }
                var sinner = _context.SINners.FirstOrDefault(a => a.Hash == Hash);
                _context.SaveChanges();
#if DEBUG
                if (Debugger.IsAttached)
                    sinner = _context.SINners.FirstOrDefault();
#endif
                if (sinner != null)
                {
                    string transactionId = $"{Guid.NewGuid().ToString().GetHashCode():X}";
                    string chummerUrl = "chummer://plugin:SINners:Load:" + sinner.Id + ":" + transactionId;
                    string postbackUrl = "https://shadowsprawl.com/api/chummer/upload";
                    sinner.LastDownload = DateTime.Now;
                    _context.SaveChanges();
                    string mypath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    StringBuilder sb = new StringBuilder("<html>")
                        .AppendFormat(@"<body onload='document.forms[""form""].submit()'>")
                        .AppendFormat("<form name='form' action='{0}' method='post'>", postbackUrl)
                        .AppendFormat("<input type='hidden' name='guid' value='{0}'>", sinner.Id)
                        .AppendFormat("<input type='hidden' name='Environment' value='{0}'>", mypath)
                        .AppendFormat("<input type='hidden' name='CharName' value='{0}'>", sinner.Alias);
                    Uri escape = new Uri(sinner.DownloadUrl);
                    string escapestr = $"{escape.Scheme}://{escape.Host}{escape.AbsolutePath}";
                    escapestr += Uri.EscapeDataString(escape.Query);
                    sb.AppendFormat("<input type='hidden' name='DownloadUrl' value='{0}'>", escapestr);

                    string urlcallback = "https://shadowsprawl.com/character/status/" + transactionId;
                    string chummeruri = chummerUrl + ":" + Uri.EscapeDataString(urlcallback);
                    sb.AppendFormat("<input type='hidden' name='ChummerUrl' value='{0}'>", chummeruri)
                        .AppendFormat("<input type='hidden' name='TransactionId' value='{0}'>", transactionId)
                        .AppendFormat("<input type='hidden' name='StatusCallback' value='{0}'>", urlcallback)
                        .AppendFormat("<input type='hidden' name='UploadDateTime' value='{0}'>", sinner.UploadDateTime)
                        .AppendFormat("<input type='hidden' name='OpenChummer' value='{0}'>", open);
                    // Other params go here
                    sb.Append("</form></body></html>");
                    string strBody = sb.ToString();
                    var contentresult = new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = strBody
                    };
                    if (tc != null)
                    {
                        tc.TrackTrace("Form generated: " + strBody, SeverityLevel.Information);
                        string msg = "Redirecting Hash " + Hash + " to Shadowsprawl with there parameters: "
                                     + Environment.NewLine + Environment.NewLine;
                        msg += contentresult;
                        TraceTelemetry tt = new TraceTelemetry(msg, SeverityLevel.Verbose);
                        tc.TrackTrace(tt);
                    }
                    return contentresult;
                }
                return NotFound("Hash not found.");
            }
            catch (Exception e)
            {
                tc?.TrackException(e);
                throw;
            }
        }

        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.RedirectKeepVerb)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)(HttpStatusCode.PermanentRedirect))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("GroupO")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        public IActionResult G([FromRoute] string Hash, string open)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        {
            try
            {
                if (string.IsNullOrEmpty(Hash))
                    throw new ArgumentException("hash is empty: " + Hash);
                if (!_context.SINnerGroups.Include(a => a.MyGroups).Any(a => a.Hash == Hash))
                {
                    foreach (var nullSinner in _context.SINnerGroups.Where(a => string.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC"))
                    {
                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                        tc?.TrackTrace(tt);
                    }
                }
                var sgi = _context.SINnerGroups.FirstOrDefault(a => a.Hash == Hash);
                _context.SaveChanges();
#if DEBUG
                if (Debugger.IsAttached)
                    sgi = _context.SINnerGroups.FirstOrDefault();
#endif
                if (sgi != null)
                {
                    var user = _signInManager.UserManager.GetUserAsync(User).Result;
                    SINnerSearchGroup sg = new SINnerSearchGroup(sgi, user);
                    string transactionId = $"{Guid.NewGuid().ToString().GetHashCode():X}";
                    string chummerUrl = "chummer://plugin:SINners:Load:" + sg.Id + ":" + transactionId;

                    string postbackUrl = "https://shadowsprawl.com/api/chummer/upload";
                    _context.SaveChanges();
                    string mypath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    StringBuilder sb = new StringBuilder("<html>")
                        .AppendFormat(@"<body onload='document.forms[""form""].submit()'>")
                        .AppendFormat("<form name='form' action='{0}' method='post'>", postbackUrl)
                        .AppendFormat("<input type='hidden' name='guid' value='{0}'>", sg.Id)
                        .AppendFormat("<input type='hidden' name='Environment' value='{0}'>", mypath)
                        .AppendFormat("<input type='hidden' name='GroupName' value='{0}'>", sg.Groupname)
                        .AppendFormat("<input type='hidden' name='HasPassword' value='{0}'>", sg.HasPassword)
                        .AppendFormat("<input type='hidden' name='Description' value='{0}'>", sg.Description)
                        .AppendFormat("<input type='hidden' name='IsPublic' value='{0}'>", sg.IsPublic)
                        .AppendFormat("<input type='hidden' name='Language' value='{0}'>", sg.Language);
                    string urlcallback = "https://shadowsprawl.com/character/status/" + transactionId;
                    string chummeruri = chummerUrl + ":" + Uri.EscapeDataString(urlcallback);
                    sb.AppendFormat("<input type='hidden' name='ChummerUrl' value='{0}'>", chummeruri)
                        .AppendFormat("<input type='hidden' name='TransactionId' value='{0}'>", transactionId)
                        .AppendFormat("<input type='hidden' name='StatusCallback' value='{0}'>", urlcallback)
                        .AppendFormat("<input type='hidden' name='OpenChummer' value='{0}'>", open);

                    // Other params go here
                    var members = sg.GetGroupMembers(_context, false).Result;
                    var json = JsonConvert.SerializeObject(members, Formatting.Indented);
                    sb.AppendFormat("<input type='hidden' name='Members' value='{0}'>", json);

                    var jsonsubgroups = JsonConvert.SerializeObject(sg.MyGroups);
                    sb.AppendFormat("<input type='hidden' name='SubGroups' value='{0}'>", jsonsubgroups);

                    sb.Append("</form></body></html>");
                    string strBody = sb.ToString();
                    var contentresult = new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = strBody
                    };
                    if (tc != null)
                    {
                        tc.TrackTrace("Form generated: " + strBody, SeverityLevel.Information);
                        string msg = "Redirecting Hash " + Hash + " to Shadowsprawl with there parameters: "
                                     + Environment.NewLine + Environment.NewLine;
                        msg += contentresult;
                        TraceTelemetry tt = new TraceTelemetry(msg, SeverityLevel.Verbose);
                        tc.TrackTrace(tt);
                    }
                    return contentresult;
                }
                return NotFound("Hash not found.");
            }
            catch (Exception e)
            {
                tc?.TrackException(e);
                throw;
            }
        }


        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerOpen")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.Open(string)'
        public async Task<ActionResult> Open([FromRoute] string Hash)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.Open(string)'
        {
            try
            {
                if (string.IsNullOrEmpty(Hash))
                    throw new ArgumentException("hash is empty: " + Hash);
                if (!_context.SINners.Any(a => a.Hash == Hash))
                {
                    foreach (var nullSinner in _context.SINners.Where(a => string.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC"))
                    {
                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                        tc?.TrackTrace(tt);
                    }
                }

                var sinner = await _context.SINners.FirstOrDefaultAsync(a => a.Hash == Hash);
                await _context.SaveChangesAsync();
                if (sinner != null)
                {
                    string url = "chummer://plugin:SINners:Load:" + sinner.Id;
                    sinner.LastDownload = DateTime.Now;
                    await _context.SaveChangesAsync();
                    //Redirect(string url);
                    //RedirectPermanent(string url);
                    //RedirectPermanentPreserveMethod(string url);
                    //RedirectPreserveMethod(string url);
                    return RedirectPreserveMethod(url);
                }

                return NotFound("Could not find SINner with Hash " + Hash);
            }
            catch (Exception e)
            {
                tc?.TrackException(e);
                throw;
            }
        }
    }
}
