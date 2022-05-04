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
using ChummerHub.Models.V1;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ChummerHub.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble'
    public class SINnerUploadAble
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.DownloadUrl'
        public string DownloadUrl { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.DownloadUrl'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.UploadDateTime'
        public DateTime? UploadDateTime { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.UploadDateTime'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.LastChange'
        public DateTime LastChange { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.LastChange'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.FileName'
        public string FileName
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.FileName'
        {
            get
            {
                switch (this)
                {
                    case SINner meAsSINner:
                        return meAsSINner.Id.ToString() + ".chum" + meAsSINner.EditionNumber + "z";
                    case SINnerGroupSetting meAsSINnerGroupSetting:
                        return "GroupSetting_" + meAsSINnerGroupSetting.Id.ToString() + ".chumGroupz";
                    default:
                        return this.Id + ".unknown";
                }
            }
        }

        [JsonIgnore]
        [XmlIgnore]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.GoogleDriveFileId'
        public string GoogleDriveFileId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.GoogleDriveFileId'


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.SINnerUploadAble()'
        public SINnerUploadAble()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerUploadAble.SINnerUploadAble()'
        {
            this.DownloadUrl = string.Empty;
        }

    }
}
