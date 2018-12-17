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
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

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
