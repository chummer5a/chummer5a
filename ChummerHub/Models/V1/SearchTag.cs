using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag'
    public class SearchTag : Tag
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag'
    {
        //[Key]
        //public Guid Id { get; set; }
        //public string sTagName { get; set; }
        //public string sTagValue { get; set; }

        //public Guid? sParentTagId { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchTags'
        public List<SearchTag> SearchTags { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchTags'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchOpterator'
        public TagOperatorEnum SearchOpterator { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchOpterator'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum'
        public enum TagOperatorEnum
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum'
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.bigger'
            @bigger,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.bigger'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.smaller'
            @smaller,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.smaller'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.equal'
            @equal,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.equal'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.contains'
            @contains,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.contains'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.notnull'
            @notnull,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.notnull'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.exists'
            @exists
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.TagOperatorEnum.exists'
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchTag()'
        public SearchTag()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SearchTag.SearchTag()'
        {
            SearchTags = new List<SearchTag>();
        }
    }
}
