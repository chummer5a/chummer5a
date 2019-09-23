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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace ChummerHub.Controllers.V1
{
    [Route("[action]/{Hash}")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    [ApiVersion("1.0")]
    [ControllerName("Chummer")]
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController'
    public class ChummerController : Controller
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController'
    {
        private readonly ILogger _logger;
        private TelemetryClient tc;
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
            _logger = logger;
            tc = telemetry;
        }

//        [HttpGet]
//        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
//        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.RedirectKeepVerb)]
//        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)(HttpStatusCode.PermanentRedirect))]
//        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
//        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerO")]
//        [EnableCors("AllowAllOrigins")]
//#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
//        public IActionResult O([FromRoute] string Hash)
//#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
//        {
//            try
//            {
//                if (String.IsNullOrEmpty(Hash))
//                    throw new ArgumentException("hash is empty: " + Hash);
//                var foundseq = (from a in _context.SINners where a.Hash == Hash select a).ToList();
//                if (!foundseq.Any())
//                {
//                    var nullseq = (from a in _context.SINners where String.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC" select a).ToList();
//                    foreach (var nullSinner in nullseq)
//                    {
//                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
//                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
//                        tc?.TrackTrace(tt);
//                    }
//                }
//                foundseq = (from a in _context.SINners where a.Hash == Hash select a).ToList();
//                _context.SaveChanges();
//                if (foundseq.Any())
//                {
//                    var sinner = foundseq.FirstOrDefault();

//                    string url = "chummer://plugin:SINners:Load:" + sinner.Id;
//                    sinner.LastDownload = DateTime.Now;
//                    _context.SaveChanges();
//                    string mypath = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
//                    //Response.HttpContext.Items.Add("Enviroment", mypath);
//                    Response.Headers.Add("Enviroment", mypath);
//                    return RedirectPreserveMethod(url);

//                }
//                else
//                    return NotFound("Could not find SINner with Hash " + Hash);
//            }
//            catch (Exception e)
//            {
//                tc.TrackException(e);
//                throw;
//            }
//        }

        [HttpGet]
        [EnableCors("AllowAllOrigins")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.RedirectKeepVerb)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)(HttpStatusCode.PermanentRedirect))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerO")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        public IActionResult O([FromRoute] string Hash)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerController.O(string)'
        {
            try
            {
                if (String.IsNullOrEmpty(Hash))
                    throw new ArgumentException("hash is empty: " + Hash);
                var foundseq = (from a in _context.SINners where a.Hash == Hash select a).ToList();
                if (!foundseq.Any())
                {
                    var nullseq = (from a in _context.SINners where String.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC" select a).ToList();
                    foreach (var nullSinner in nullseq)
                    {
                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                        tc?.TrackTrace(tt);
                    }
                }
                foundseq = (from a in _context.SINners where a.Hash == Hash select a).ToList();
                _context.SaveChanges();
#if DEBUG
                if (Debugger.IsAttached)
                    foundseq = (from a in _context.SINners select a).Take(1).ToList();
#endif 
                if (foundseq.Any())
                {
                    var sinner = foundseq.FirstOrDefault();
                    string transactionId = String.Format("{0:X}", Guid.NewGuid().ToString().GetHashCode());
                    string chummerUrl = "chummer://plugin:SINners:Load:" + sinner.Id + ":" + transactionId;
                    
                    string postbackUrl = "https://shadowsprawl.com/api/chummer/upload";
                    sinner.LastDownload = DateTime.Now;
                    _context.SaveChanges();
                    string mypath = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<html>");
                    sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                    sb.AppendFormat("<form name='form' action='{0}' method='post'>", postbackUrl);
                    sb.AppendFormat("<input type='hidden' name='guid' value='{0}'>", sinner?.Id);
                    sb.AppendFormat("<input type='hidden' name='Environment' value='{0}'>", mypath);
                    sb.AppendFormat("<input type='hidden' name='CharName' value='{0}'>", sinner?.Alias);
                    Uri escape = new Uri(sinner?.DownloadUrl);
                    string escapestr = $"{escape.Scheme}://{escape.Host}{escape.AbsolutePath}";
                    escapestr += Uri.EscapeDataString(escape.Query);
                    sb.AppendFormat("<input type='hidden' name='DownloadUrl' value='{0}'>", escapestr);
                    
                    string urlcallback = "https://shadowsprawl.com/character/status/" + transactionId;
                    string chummeruri = chummerUrl + ":" + Uri.EscapeDataString(urlcallback);
                    sb.AppendFormat("<input type='hidden' name='ChummerUrl' value='{0}'>", chummeruri);
                    sb.AppendFormat("<input type='hidden' name='TransactionId' value='{0}'>", transactionId);
                    sb.AppendFormat("<input type='hidden' name='StatusCallback' value='{0}'>", urlcallback);
                    sb.AppendFormat("<input type='hidden' name='UploadDateTime' value='{0}'>", sinner?.UploadDateTime);
                    // Other params go here
                    sb.Append("</form>");
                    sb.Append("</body>");
                    sb.Append("</html>");
                    tc.TrackTrace("Form generated: " + sb.ToString(), SeverityLevel.Information);
                    var contentresult = new ContentResult()
                    {
                        ContentType = "text/html",
                        StatusCode = (int) HttpStatusCode.OK,
                        Content = sb.ToString()
                    };
                    return contentresult;

                }
                return NotFound("Hash not found.");
            }
            catch (Exception e)
            {
                tc.TrackException(e);
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
                if (String.IsNullOrEmpty(Hash))
                    throw new ArgumentException("hash is empty: " + Hash);
                var foundseq = await (from a in _context.SINners where a.Hash == Hash select a).ToListAsync();
                if (!foundseq.Any())
                {
                    var nullseq = await (from a in _context.SINners
                                         where String.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC"
                                         select a).ToListAsync();
                    foreach (var nullSinner in nullseq)
                    {
                        string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                        TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                        tc?.TrackTrace(tt);
                    }
                }

                foundseq = await (from a in _context.SINners where a.Hash == Hash select a).ToListAsync();
                await _context.SaveChangesAsync();
                if (foundseq.Any())
                {
                    var sinner = foundseq.FirstOrDefault();
                    string url = "chummer://plugin:SINners:Load:" + sinner.Id;
                    sinner.LastDownload = DateTime.Now;
                    await _context.SaveChangesAsync();
                    //Redirect(string url);
                    //RedirectPermanent(string url);
                    //RedirectPermanentPreserveMethod(string url);
                    //RedirectPreserveMethod(string url);
                    return RedirectPreserveMethod(url);
                }
                else
                {
                    return NotFound("Could not find SINner with Hash " + Hash);
                }
            }
            catch (Exception e)
            {
                tc.TrackException(e);
                throw;
            }
        }

    }

}
