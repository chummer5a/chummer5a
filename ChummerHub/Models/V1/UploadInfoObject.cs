using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Models.V1
{
    public class UploadInfoObject
    {
        public DateTime? UploadDateTime { get; set; }

        public UploadClient Client { get; set; }

        public IEnumerable<SINner> SINners { get; set; }

        public String Groupname { get; set; }
    }
}
