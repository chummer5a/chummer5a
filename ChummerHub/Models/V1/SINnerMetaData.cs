using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData'
    public class SINnerMetaData
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Id'
        public Guid Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Visibility'
        public SINnerVisibility Visibility { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Visibility'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Tags'
        public List<Tag> Tags { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.Tags'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.SINnerMetaData()'
        public SINnerMetaData()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerMetaData.SINnerMetaData()'
        {
            this.Tags = new List<Tag>();
            Visibility = new SINnerVisibility();
        }

    }
}
