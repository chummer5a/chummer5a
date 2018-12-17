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
                sTagName = "Reflection",
                sTagValue = "",
                sSearchOpterator = SearchTag.TagOperatorEnum.notnull,
                sTags = new List<SearchTag>()
                {
                    new SearchTag ()
                    {
                         sTags = new List<SearchTag>(),
                         sTagName = "AttributeSection",

                    }
                }

            };
            return sin;
        }
    }
   
}
