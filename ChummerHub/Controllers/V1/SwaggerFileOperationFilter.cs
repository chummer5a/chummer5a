using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Controllers.V1
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var anyFileStreamResult = context.ApiDescription.SupportedResponseTypes
                .Any(x => x.Type == typeof(FileStreamResult));

            if (anyFileStreamResult)
            {
                operation.Produces = new[] { "application/octet-stream" };
                operation.Responses["200"].Schema = new Schema { Type = "file" };
            }
        }
    }
}
