using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using ChummerHub.Models.V1;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace ChummerHub.Models
{
    public class SINnerUploadAble
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid? Id { get; set; }

        public string DownloadUrl { get; set; }

        public DateTime? UploadDateTime { get; set; }

        public DateTime LastChange { get; set; }

        public string FileName
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
        public string GoogleDriveFileId { get; set; }


        public SINnerUploadAble()
        {
            this.DownloadUrl = "";
        }

    }
}
