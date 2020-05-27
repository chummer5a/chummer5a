using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1.Examples
{


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample'
    public class SINnerExample : IExamplesProvider
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample'
    {
        /// <summary>
        /// Class to enalbe Swagger to generate an Example
        /// </summary>
        public SINnerExample()
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample.GetExamples()'
        public object GetExamples()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample.GetExamples()'
        {
            return GetSINnerExample();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample.GetSINnerExample()'
        public SINner GetSINnerExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample.GetSINnerExample()'
        {
            Guid parentTagGuid = Guid.NewGuid();
            Guid childTagGuid = Guid.NewGuid();
            var sin = new SINner
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerListExample'
    public class SINnerListExample : IExamplesProvider
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerListExample'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerListExample.GetExamples()'
        public object GetExamples()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerListExample.GetExamples()'
        {
            var list = new List<SINner>() { new SINnerExample().GetSINnerExample() };
            return list;
        }
    }
}
