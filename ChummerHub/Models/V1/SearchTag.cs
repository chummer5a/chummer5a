using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SearchTag : Tag
    {
        //[Key]
        //public Guid Id { get; set; }
        //public string sTagName { get; set; }
        //public string sTagValue { get; set; }

        //public Guid? sParentTagId { get; set; }

        public List<SearchTag> SearchTags { get; set; }

        public TagOperatorEnum SearchOpterator { get; set; }

        public enum TagOperatorEnum
        {
            @bigger,
            @smaller,
            @equal,
            @contains,
            @notnull,
            @exists
        }

        public SearchTag()
        {
            SearchTags = new List<SearchTag>();
        }
    }
}
