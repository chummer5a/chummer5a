using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using ChummerHub.Models.V1.Examples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChummerHub.Controllers.V1
{
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [ControllerName("SINSearch")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController'
    public class SINSearchController : ControllerBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController'
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.SINSearchController(ApplicationDbContext, ILogger<SINSearchController>)'
        public SINSearchController(ApplicationDbContext context, ILogger<SINSearchController> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.SINSearchController(ApplicationDbContext, ILogger<SINSearchController>)'
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ChummerFiles
        [HttpGet()]
        [Authorize]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerListExample))]
        [SwaggerRequestExample(typeof(SearchTag), typeof(SINnerSearchExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SearchGetSINners")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.Search(SearchTag)'
        public IEnumerable<SINner> Search(SearchTag searchTag)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.Search(SearchTag)'
        {
            try
            {
                _logger.LogTrace("Searching SINnerFile");
                var result = _context.SINners.Include(sinner => sinner.SINnerMetaData)
                    .ThenInclude(meta => meta.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .OrderByDescending(a => a.UploadDateTime).Take(20);
                return result;
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in SearchSINnerFile: " + e.Message, e);
                throw hue;
            }
        }

        // GET: api/ChummerFiles
        [HttpGet()]
        [Authorize(Roles = "Administrator")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SearchAdminGetSINnerIds")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.AdminGetSINners()'
        public async Task<IEnumerable<SINner>> AdminGetSINners()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINSearchController.AdminGetSINners()'
        {
            try
            {
                _logger.LogTrace("AdminGetIds");
                var result = await _context.SINners.Include(sinner => sinner.SINnerMetaData)
                    .ThenInclude(meta => meta.Visibility)
                    .ThenInclude(vis => vis.UserRights)
                    .Include(meta => meta.SINnerMetaData)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags)
                    .ThenInclude(tag => tag.Tags).ToListAsync();
                return result;
            }
            catch (Exception e)
            {
                HubException hue = new HubException("Exception in AdminGetIds: " + e.Message, e);
                throw hue;
            }
        }
    }

}
