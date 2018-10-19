using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
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
                SINnerMetaData = new SINnerMetaData()
                {
                    SINnerMetaDataId = Guid.NewGuid(),
                    Tags = new List<Tag>()
                    {
                        new Tag()
                        {
                            TagId = parentTagGuid,
                            TagName = "TestTag",
                            TagValue = "TestTagValue",
                            TagType = Tag.TagValueEnum.@string,
                            ParentTagId = Guid.Empty,
                            IsUserGenerated = true,
                            Tags = new List<Tag>()
                            {
                                new Tag()
                                {
                                    TagId = Guid.NewGuid(),
                                    TagName = "TestChildTag",
                                    TagValue = "TestChildTagValue",
                                    TagType = Tag.TagValueEnum.@string,
                                    ParentTagId = parentTagGuid,
                                    IsUserGenerated = false
                                }
                            }
                        }
                    }
                }
            };
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
