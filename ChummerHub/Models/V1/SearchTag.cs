using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SearchTag
    {
        [Key]
        public Guid Id { get; set; }
        public string sTagName { get; set; }
        public string sTagValue { get; set; }

        public Guid? sParentTagId { get; set; }

        public List<SearchTag> sTags { get; set; }

        public TagOperatorEnum sSearchOpterator { get; set; }

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
            Id = Guid.NewGuid();
            this.sTagName = "";
            this.sTagValue = "";
            this.sTags = new List<SearchTag>();
        }
    }
}
