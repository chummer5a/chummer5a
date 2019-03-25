using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using ChummerHub.API;

namespace ChummerHub.Controllers.V1
{
    [ApiController]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1.0")]
    [ControllerName("ChummerHelper")]
    [Authorize]
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

        // GET: api/ChummerFiles
        [HttpGet]
        [AllowAnonymous]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerHelperVersion")]
        public async Task<ActionResult<ChummerHubVersion>> GetVersion()
        {
            try
            {
                return Ok(new ChummerHubVersion());
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in GetVersion: " + e.Message, e);
                return StatusCode(500, hue);
            }
        }



    }

}
