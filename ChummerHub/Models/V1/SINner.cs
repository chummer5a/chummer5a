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

        public Guid? SINnerId { get; set; }

        public string DownloadUrl { get; set; }

        public DateTime? UploadDateTime { get; set; }

        public ChummerUploadClient ChummerUploadClient { get; set; }

        public SINnerMetaData SINnerMetaData { get; set; }

        public string GoogleDriveFileId { get; set; }

        public SINner()
        {
            SINnerId = Guid.NewGuid();
            this.ChummerUploadClient = new ChummerUploadClient();
            this.UploadDateTime = DateTime.Now;
            this.SINnerMetaData = new SINnerMetaData();
        }
    }
}
