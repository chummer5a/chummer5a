using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ChummerHub.Models
{
    [DataContract]
    public class HubCallResult
    {
        [DataMember(Name = "Success")]
        bool Success { get; set; }

        [DataMember(Name = "Message")]
        public String MyExceptionMessage { get; set; }
    }
}
