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
            if (!list.Any())
                return NotFound(id);
            return Ok(list);
        }
    }
}
