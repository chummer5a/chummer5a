/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1.Examples
{


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerExample'
    public class SINnerExample : IExamplesProvider<object>
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
    public class SINnerListExample : IExamplesProvider<object>
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
