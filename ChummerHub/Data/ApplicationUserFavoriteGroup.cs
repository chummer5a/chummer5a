/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
