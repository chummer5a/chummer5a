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
        /// <summary>
        /// Class to enalbe Swagger to generate an Example
        /// </summary>
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
                 Id = Guid.NewGuid(),
                 UploadClientId = Guid.NewGuid()
            };
            sin.SINnerMetaData = new SINnerMetaData()
            {
                Id = Guid.NewGuid(),
                Tags = new List<Tag>(),
                Visibility = new SINnerVisibility()
                {
                    IsGroupVisible = true,
                    UserRights = new List<SINnerUserRight>()
                    {
                         new SINnerUserRight(sin.Id.Value)
                         {
                              EMail = "archon.megalon@gmail.com",
                              CanEdit = true
                         }
                    }
                }
              
            };
            var parenttag = new Tag(sin, null)
            {
                Id = parentTagGuid,
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
                    Id = childTagGuid,
                    TagName = "TestChildTag",
                    TagValue = "TestChildTagValue",
                    TagType = Tag.TagValueEnum.@string,
                    ParentTagId = parentTagGuid,
                    IsUserGenerated = false
                }
            };
            sin.SINnerMetaData.Tags.Add(parenttag);
            //sin.MyExtendedAttributes.JsonSummary = "{}";
            sin.LastChange = DateTime.Now;
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
