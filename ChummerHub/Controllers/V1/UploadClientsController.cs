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

namespace ChummerHub.Controllers.V1
{
    [Route("api/v{api-version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("UploadClient")]
    [Authorize]
    public class UploadClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UploadClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET: api/UploadClients
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ClientGetTestClients")]
        public IEnumerable<UploadClient> GetSomeTestUploadClients()
        {
            return _context.UploadClients.Take(20);
        }

        //GET: api/UploadClients/5
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerOperation("ClientGet")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUploadClient([FromRoute] Guid id)
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
        public async Task<IActionResult> GetSINners([FromRoute] Guid id)
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
