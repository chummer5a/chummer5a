using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace ChummerHub.Controllers.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SwaggerFileOperationFilter'
    public class SwaggerFileOperationFilter : IOperationFilter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SwaggerFileOperationFilter'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SwaggerFileOperationFilter.Apply(Operation, OperationFilterContext)'
        public void Apply(Operation operation, OperationFilterContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SwaggerFileOperationFilter.Apply(Operation, OperationFilterContext)'
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
