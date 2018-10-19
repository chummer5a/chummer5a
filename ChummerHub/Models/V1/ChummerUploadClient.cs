using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace ChummerHub.Models.V1
{
   
    public class ChummerUploadClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid UploadClientId { get; set; }

        public string ChummerVersion { get; set; }

        public string ClientSecret { get; set; }

        public ChummerUploadClient()
        {
            this.UploadClientId = Guid.Empty;
            this.ChummerVersion = new Version().ToString();
            this.ClientSecret = "";
        }

        public string UserEmail { get; set; }
    }
}
