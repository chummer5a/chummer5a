using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ChummerHub.Data;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChummerHub.Controllers.V1
{
    [Route("[action]/{Hash}")]
    [ApiController]
    [ControllerName("Chummer")]
    [AllowAnonymous]
    public class ChummerController : ControllerBase
    {
        private readonly ILogger _logger;
        private TelemetryClient tc;
        public ApplicationDbContext _context = null;

        public ChummerController(ApplicationDbContext context,
            ILogger<ChummerController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            TelemetryClient telemetry)
        {
            _context = context;
            _logger = logger;
            tc = telemetry;
        }


        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Redirect)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerOpen")]
        public async Task<ActionResult> Open([FromRoute] string Hash)
        {
            if (String.IsNullOrEmpty(Hash))
                throw new ArgumentException("hash is empty: " + Hash);
            var foundseq = await (from a in _context.SINners where a.Hash == Hash select a).ToListAsync();
            if (!foundseq.Any())
            {
                var nullseq = await (from a in _context.SINners where String.IsNullOrEmpty(a.Hash) || a.Hash == "25943ECC" select a).ToListAsync();
                foreach (var nullSinner in nullseq)
                {
                    string message = "Saving Hash for SINner " + nullSinner.Id + ": " + nullSinner.MyHash;
                    TraceTelemetry tt = new TraceTelemetry(message, SeverityLevel.Verbose);
                    tc?.TrackTrace(tt);
                }
            }
            foundseq = await (from a in _context.SINners where a.Hash == Hash select a).ToListAsync();
            await _context.SaveChangesAsync();
            foreach (var sinner in foundseq)
            {
                string url = "chummer://plugin:SINners:Load:" + sinner.Id;
                sinner.LastDownload = DateTime.Now;
                await _context.SaveChangesAsync();
                return Redirect(url);
            }
            return NotFound("Could not find SINner with Hash " + Hash);
        }
       
    }

}  
