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
using ChummerHub.Data;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Controllers.V1
{
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [ControllerName("UploadClient")]
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController'
    public class UploadClientsController : ControllerBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController'
    {
        private readonly ApplicationDbContext _context;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.UploadClientsController(ApplicationDbContext)'
        public UploadClientsController(ApplicationDbContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.UploadClientsController(ApplicationDbContext)'
        {
            _context = context;
        }

        //GET: api/UploadClients
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ClientGetTestClients")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetSomeTestUploadClients()'
        public IEnumerable<UploadClient> GetSomeTestUploadClients()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetSomeTestUploadClients()'
        {
            return _context.UploadClients.Take(20);
        }

        //GET: api/UploadClients/5
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ClientGet")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetUploadClient(Guid)'
        public async Task<IActionResult> GetUploadClient([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetUploadClient(Guid)'
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uploadClient = await _context.UploadClients.FindAsync(id);

            if (uploadClient == null)
            {
                return NotFound();
            }

            return Ok(uploadClient);
        }

        // GET: api/UploadClients/5
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ClientGetSINners")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetSINners(Guid)'
        public async Task<IActionResult> GetSINners([FromRoute] Guid id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadClientsController.GetSINners(Guid)'
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uploadClient = await _context.UploadClients.FindAsync(id);

            if (uploadClient == null)
            {
                return NotFound();
            }
            var list = await _context.SINners.Select(a => a.UploadClientId == id).ToListAsync();
            if (list.Count == 0)
                return NotFound(id);
            return Ok(list);
        }
    }
}
