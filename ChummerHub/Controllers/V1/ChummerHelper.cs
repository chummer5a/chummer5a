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
