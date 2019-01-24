using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1.Examples
{  

    public class SINnerSearchExample : IExamplesProvider
    {
        public SINnerSearchExample()
        {

        }

        public object GetExamples()
        {
            return GetSINnerSearchExample();
        }

        public SearchTag GetSINnerSearchExample()
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
