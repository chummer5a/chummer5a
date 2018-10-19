//using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class SINner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid? SINnerId { get; set; }
        public string Base64EncodedXmlFile { get; set; }

        public DateTime? UploadDateTime { get; set; }

        public ChummerUploadClient ChummerUploadClient { get; set; }

        public SINnerMetaData SINnerMetaData { get; set; }

        public SINner()
        {
            SINnerId = Guid.NewGuid();
            this.ChummerUploadClient = new ChummerUploadClient();
            this.UploadDateTime = DateTime.Now;
            this.SINnerMetaData = new SINnerMetaData();
        }


    }

    
}
