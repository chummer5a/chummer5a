using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.IO;
using ChummerHub.API;
using ChummerHub.Models.V1.Examples;

namespace ChummerHub.Controllers.V1
{
    [ApiController]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1.0")]
    [ControllerName("ChummerHelper")]
    [AllowAnonymous]
    /// <summary>
    /// Collection of functions, that serve for helping the client
    /// with specific use-cases. These do not need to be related to an
    /// actual data-model.
    /// </summary>
    public class ChummerHelper : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChummerHelper(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> GetDownloadFile([FromRoute] Guid sinnerid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chummerFile = await _context.SINners.FindAsync(sinnerid);

                if (chummerFile == null)
                {
                    return NotFound();
                }
                if (String.IsNullOrEmpty(chummerFile.DownloadUrl))
                {
                    string msg = "Chummer " + chummerFile.SINnerId + " does not have a valid DownloadUrl!";
                    throw new ArgumentException(msg);
                }
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), chummerFile.SINnerId.ToString() + ".chum5z");
                using (var client = new WebClient())
                {
                    client.DownloadFile(new Uri(chummerFile.DownloadUrl), path);
                }
                if (!System.IO.File.Exists(path))
                {
                    throw new ArgumentException("No file downloaded from " + chummerFile.DownloadUrl);
                }
                var res = new FileResult(chummerFile.SINnerId.ToString() + ".chum5z", path, "application/octet-stream");
                
                return res;
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in GetSINnerfile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        
       
    }

}
