using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1.Examples
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample'
    public class SINnerSearchExample : IExamplesProvider<object>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.SINnerSearchExample()'
        public SINnerSearchExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.SINnerSearchExample()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetExamples()'
        public object GetExamples()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetExamples()'
        {
            return GetSINnerSearchExample();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetSINnerSearchExample()'
        public SearchTag GetSINnerSearchExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetSINnerSearchExample()'
        {
            Guid parentTagGuid = Guid.NewGuid();
            var sin = new SearchTag
            {
                TagName = "Reflection",
                TagValue = "",
                SearchOpterator = SearchTag.TagOperatorEnum.notnull,
                Tags = new List<Tag>()
                {
                    new SearchTag ()
                    {
                         Tags = new List<Tag>(),
                         TagName = "AttributeSection",
                         SearchOpterator = SearchTag.TagOperatorEnum.exists
                    }
                }

            };
            return sin;
        }
    }

}
