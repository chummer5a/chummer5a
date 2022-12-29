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
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("UserRight {EMail}: {CanEdit}")]
    public class SINnerUserRight
    {
        public SINnerUserRight()
        {
        }

        public SINnerUserRight(Guid sinnerId)
        {
            SINnerId = sinnerId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Guid? SINnerId { get; set; }

        private string _email;

        [MaxLength(64)]
        public string EMail
        {
            get => _email;
            set
            {
                _email = null;
                _email = value?.ToUpperInvariant();
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.CanEdit'
        public bool CanEdit { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUserRight.CanEdit'
    }
}
