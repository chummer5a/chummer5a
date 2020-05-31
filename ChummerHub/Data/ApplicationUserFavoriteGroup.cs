using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup'
    public class ApplicationUserFavoriteGroup
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.Id'
        public Guid Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.FavoriteGuid'
        public Guid FavoriteGuid { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.FavoriteGuid'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.ApplicationUserFavoriteGroup()'
        public ApplicationUserFavoriteGroup()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationUserFavoriteGroup.ApplicationUserFavoriteGroup()'
        {
            FavoriteGuid = Guid.Empty;
        }
    }
}
