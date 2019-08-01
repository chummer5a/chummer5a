using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChummerHub.Data
{
    public class ApplicationUserFavoriteGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid FavoriteGuid { get; set; }

        public ApplicationUserFavoriteGroup()
        {
            FavoriteGuid = Guid.Empty;
        }
    }
}
