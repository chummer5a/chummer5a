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
                if (this is SINner meAsSINner)
                {
                    return meAsSINner.Id.ToString() + ".chum" + meAsSINner.EditionNumber + "z";
                }
                else if (this is SINnerGroupSetting meAsSINnerGroupSetting)
                {
                    return "GroupSetting_" + meAsSINnerGroupSetting.Id.ToString() + ".chumGroupz";
                }
                else
                    return this.Id + ".unknown";
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
            this.DownloadUrl = "";
        }

    }
}
