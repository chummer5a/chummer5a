using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    [DataContract]
    [Serializable]
    public class ResultSinnerPostSIN : ResultBase
    {
        public List<SINner> MySINners { get; set; }

        public ResultSinnerPostSIN(List<SINner> list)
        {
            MySINners = list;
        }

        public ResultSinnerPostSIN(Exception e) : base(e)
        {

        }
    }
}
