using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ChummerHub.Controllers.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation'
    public class FileUploadOperation : IOperationFilter
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation.FileUploadOperation()'
        public FileUploadOperation()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation.FileUploadOperation()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation.Apply(Operation, OperationFilterContext)'
        public void Apply(Operation operation, OperationFilterContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileUploadOperation.Apply(Operation, OperationFilterContext)'
        {
            if (operation.OperationId.ToLower() == "ApiV1SINnerByIdPut".ToLower())
            {
                operation.Parameters.RemoveAt(1);
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "uploadedFile",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });
                operation.Consumes.Add("multipart/form-data");
            }
        }
    }
}
