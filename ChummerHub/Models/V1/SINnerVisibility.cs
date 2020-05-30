using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility'
    public class SINnerVisibility
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.IsPublic'
        public bool IsPublic { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.IsPublic'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.IsGroupVisible'
        public bool IsGroupVisible { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.IsGroupVisible'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.UserRights'
        public List<SINnerUserRight> UserRights { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.UserRights'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.SINnerVisibility()'
        public SINnerVisibility()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerVisibility.SINnerVisibility()'
        {
            UserRights = new List<SINnerUserRight>();

        }
    }
}
