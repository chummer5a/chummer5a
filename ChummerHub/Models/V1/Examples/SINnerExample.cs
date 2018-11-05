using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ChummerHub.Models.V1.Examples
{
    public class SINnerExample : IExamplesProvider
    {
        public SINnerExample()
        {

        }

        public object GetExamples()
        {
            return GetSINnerExample();
        }

        public SINner GetSINnerExample()
        {
            Guid parentTagGuid = Guid.NewGuid();
            Guid childTagGuid = Guid.NewGuid();
            var sin =  new SINner
            {
                SINnerId = Guid.NewGuid(),
                ChummerUploadClient = new ChummerUploadClient()
                {
                    UploadClientId = Guid.Empty,
                    ChummerVersion = new Version().ToString(),
                    ClientSecret = "42"
                },
                UploadDateTime = DateTime.Now,                
            };
            sin.SINnerMetaData = new SINnerMetaData()
            {
                SINnerMetaDataId = Guid.NewGuid(),
                Tags = new List<Tag>(),
              
            };
            var parenttag = new Tag(sin, null)
            {
                TagId = parentTagGuid,
                TagName = "TestTag",
                TagValue = "TestTagValue",
                TagType = Tag.TagValueEnum.@string,
                ParentTagId = Guid.Empty,
                IsUserGenerated = true
            };
            parenttag.Tags = new List<Tag>()
            {
                new Tag(sin, parenttag)
                {
                    TagId = childTagGuid,
                    TagName = "TestChildTag",
                    TagValue = "TestChildTagValue",
                    TagType = Tag.TagValueEnum.@string,
                    ParentTagId = parentTagGuid,
                    IsUserGenerated = false
                }
            };
            sin.SINnerMetaData.Tags.Add(parenttag);

            //try
            //{
            //    var assembly = typeof(SINnerExample).Assembly;
            //    using (Stream resource = assembly.GetManifestResourceStream("ChummerHub.Models.V1.Examples.B64Barrett.txt"))
            //    {
            //        using (StreamReader reader = new StreamReader(resource))
            //        {
            //            string b64 = reader.ReadToEnd();
            //            sin.Base64EncodedXmlFile = b64;
            //        }

            //    }
            //}
            //catch (Exception e)
            //{
            //    sin.Base64EncodedXmlFile = e.ToString();
            //}

            return sin;
        }
    }
    public class SINnerListExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var list = new List<SINner>() { new SINnerExample().GetSINnerExample() };
            return list;
        }
    }
}
