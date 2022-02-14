using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1.Examples
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample'
    public class UploadInfoObjectExample : IExamplesProvider<object>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.UploadInfoObjectExample()'
        public UploadInfoObjectExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.UploadInfoObjectExample()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.GetExamples()'
        public object GetExamples()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.GetExamples()'
        {
            return GetUploadInfoObjectExample();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.GetUploadInfoObjectExample()'
        public UploadInfoObject GetUploadInfoObjectExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UploadInfoObjectExample.GetUploadInfoObjectExample()'
        {
            var sin = new SINnerExample().GetSINnerExample();
            var id = Guid.NewGuid();
            sin.UploadClientId = id;
            UploadInfoObject info = new UploadInfoObject()
            {
                SINners = new List<SINner>()
                {
                    sin
                },
                UploadDateTime = DateTime.Now,
                Client = new UploadClient()
                {
                    Id = id,
                    ChummerVersion = System.Reflection.Assembly.GetAssembly(typeof(UploadInfoObjectExample))?.GetName().Version?.ToString(),
                }
            };
            return info;
        }
    }
}
