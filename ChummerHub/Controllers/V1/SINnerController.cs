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
//using Swashbuckle.AspNetCore.Filters;

namespace ChummerHub.Controllers.V1
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("SINner")]
    [AllowAnonymous]
    public class SINnerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public SINnerController(ApplicationDbContext context, ILogger<SINnerController> logger)
        {
            
            _context = context;
            _logger = logger;
        }

        // GET: api/ChummerFiles
        [HttpGet]
        [AllowAnonymous]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerListExample))]
        public IEnumerable<SINner> GetSINnerFile()
        {
            try
            {
                _logger.LogInformation("Getting GetSINnerFile");
                var result = _context.SINners.OrderByDescending(a => a.UploadDateTime).Take(20);
                result = _context.SINners.Include(sinner => sinner.SINnerMetaData)
                    .ThenInclude(meta => meta.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .OrderByDescending(a => a.UploadDateTime).Take(20);
                return result;
            }
            catch(Exception e)
            {
                HubException hue = new HubException("Exception in GetSINnerfile: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles/5
        [HttpGet("{id}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerExample))]
        public async Task<IActionResult> GetSINnerFile([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chummerFile = await _context.SINners.FindAsync(id);

                if (chummerFile == null)
                {
                    return NotFound();
                }

                return Ok(chummerFile);
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in GetSINnerfile: " + e.Message, e);
                return BadRequest(hue);
            }
           
        }

        // PUT: api/ChummerFiles/5
        [HttpPut("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NoContent)]

        public async Task<IActionResult> PutSINnerFile([FromRoute] Guid id, [FromBody] SINner chummerFile)
        {
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
                    //throw new HubException(msg);
                    return BadRequest(new HubException(msg));
                }

                if (id != chummerFile.SINnerId)
                {
                    return BadRequest();
                }

                _context.Entry(chummerFile).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SINnerFileExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in PutSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        // POST: api/ChummerFiles
        [HttpPost()]
        [AllowAnonymous]
        [SwaggerRequestExample(typeof(SINner), typeof(SINnerExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created, Type = typeof(SINner))]

        public async Task<IActionResult> PostSINnerFile([FromBody] SINner chummerFile)
        {
            
            return await PostSINnerInternal(chummerFile);
        }

       
        private async Task<IActionResult> PostSINnerInternal(SINner chummerFile)
        {
            _logger.LogInformation("Post SINnerInternalt: " + chummerFile + ".");
            try
            {
                if (!ModelState.IsValid)
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
                    //throw new HubException(msg);
                    return BadRequest(new HubException(msg));
                }

                if (chummerFile.SINnerId.ToString() == "string")
                    chummerFile.SINnerId = Guid.Empty;
                if (chummerFile.UploadDateTime == null)
                    chummerFile.UploadDateTime = DateTime.Now;
               
                _context.SINners.Add(chummerFile);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetSINnerFile", new { id = chummerFile.SINnerId }, chummerFile);
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in PostSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        // DELETE: api/ChummerFiles/5
        [HttpDelete("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]

        public async Task<IActionResult> DeleteSINnerFile([FromRoute] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chummerFile = await _context.SINners.FindAsync(id);
                if (chummerFile == null)
                {
                    return NotFound();
                }

                _context.SINners.Remove(chummerFile);
                await _context.SaveChangesAsync();

                return Ok(chummerFile);
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in DeleteSINnerFile: " + e.Message, e);
                return BadRequest(hue);
            }
        }

        private bool SINnerFileExists(Guid id)
        {
            try
            {
                return _context.SINners.Any(e => e.SINnerId == id);
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in SINnerFileExists: " + e.Message, e);
                throw hue;
            }
        }
    }
}
