using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
   
    public class UploadClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid UploadClientId { get; set; }

        public string ChummerVersion { get; set; }

        public string ClientSecret { get; set; }

        public UploadClient()
        {
            this.UploadClientId = Guid.Empty;
            this.ChummerVersion = new Version().ToString();
            this.ClientSecret = "";
        }

        public string UserEmail { get; set; }

        
    }
}
