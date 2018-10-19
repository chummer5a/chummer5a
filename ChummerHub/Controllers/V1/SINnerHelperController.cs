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

namespace ChummerHub.Controllers.V1
{
    [ApiController]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ControllerName("SINnerHelper")]
    [AllowAnonymous]
    public class SINnerHelperController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SINnerHelperController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public bool SINnerExists(Guid? id)
        {
            return _context.SINners.Any(e => e.SINnerId == id);
        }
    }
}
