using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1.Examples
{
    public class UploadInfoObjectExample : IExamplesProvider
    {
        public UploadInfoObjectExample()
        {

        }

        public object GetExamples()
        {
            return GetUploadInfoObjectExample();
        }

        public UploadInfoObject GetUploadInfoObjectExample()
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
                    UploadClientId = id,
                    ChummerVersion = System.Reflection.Assembly.GetAssembly(typeof(UploadInfoObjectExample)).GetName().Version.ToString(),
                }
            };
            return info;
        }
    }
}
