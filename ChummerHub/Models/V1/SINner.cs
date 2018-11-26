//using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
    public class SINner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid? Id { get; set; }

        [NotMapped]
        public string DownloadUrl { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string DownloadUrlInternal { get; set; }

        public DateTime? UploadDateTime { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public Guid UploadClientId { get; set; }

        public SINnerMetaData SINnerMetaData { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string GoogleDriveFileId { get; set; }

        public SINner()
        {
            Id = Guid.NewGuid();
            this.SINnerMetaData = new SINnerMetaData();
        }
    }
}
