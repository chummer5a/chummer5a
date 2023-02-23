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

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment'
    public class SINnerComment
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.SINnerId'
        public Guid? SINnerId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.SINnerId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.MyDateTime'
        public DateTime MyDateTime { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.MyDateTime'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Comment'
        public string Comment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Comment'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Email'
        public string Email { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Email'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Downloads'
        public int Downloads { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerComment.Downloads'
    }
}
