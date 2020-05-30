using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ChummerHub.Controllers.V1
{
    [ApiController]
    [EnableCors("AllowOrigin")]
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1.0")]
    [ControllerName("ChummerHelper")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper'
#pragma warning disable CS1587 // XML comment is not placed on a valid language element
    /// <summary>
    /// Collection of functions, that serve for helping the client
    /// with specific use-cases. These do not need to be related to an
    /// actual data-model.
    /// </summary>
    public class ChummerHelper : ControllerBase
#pragma warning restore CS1587 // XML comment is not placed on a valid language element
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper'
    {
        private readonly ApplicationDbContext _context;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper.ChummerHelper(ApplicationDbContext)'
        public ChummerHelper(ApplicationDbContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper.ChummerHelper(ApplicationDbContext)'
        {
            _context = context;
        }

        // GET: api/ChummerFiles
        [HttpGet]
        [AllowAnonymous]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ChummerHelperVersion")]
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper.GetVersion()'
        public async Task<ActionResult<ChummerHubVersion>> GetVersion()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ChummerHelper.GetVersion()'
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
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
