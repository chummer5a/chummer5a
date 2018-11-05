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


        // POST: api/ChummerFiles
        [HttpPost()]
        [AllowAnonymous]
        [Route("upload")]
        //[SwaggerRequestExample(typeof(SINner), typeof(SINnerExample))]
        //[Swashbuckle.AspNetCore.Annotations.SwaggerResponse((int)HttpStatusCode.Created, Type = typeof(SINner))]

        public void PostFile(IFormFile uploadedFile)
        {
            using (var fileStream = System.IO.File.Create("temp"))
            {
                using (var stream = uploadedFile.OpenReadStream())
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

            }
        }
    }

}
