/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using ChummerHub.API;
using ChummerHub.Data;
using ChummerHub.Models.V1;
using ChummerHub.Models.V1.Examples;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    [Authorize(Roles = API.Authorization.Constants.UserRolePublicAccess, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
    public class SINSearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public SINSearchController(ApplicationDbContext context, ILogger<SINSearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ChummerFiles
        [HttpGet()]
        [Authorize(Roles = API.Authorization.Constants.UserRolePublicAccess, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(SINnerListExample))]
        [SwaggerRequestExample(typeof(SearchTag), typeof(SINnerSearchExample))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.OK)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.NotFound)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SearchGetSINners")]
        public IEnumerable<SINner> Search(SearchTag searchTag)
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
        [Authorize(Roles = API.Authorization.Constants.UserRoleAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("SearchAdminGetSINnerIds")]
        public async Task<IEnumerable<SINner>> AdminGetSINners()
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
