using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    [DataContract]
    [Serializable]
    public class ResultSinnerGetSINById : ResultBase
    {
        public SINner MySINner { get; set; }

        public ResultSinnerGetSINById(SINner sinner)
        {
            MySINner = sinner;
        }

        public ResultSinnerGetSINById(Exception e) : base(e)
        {

        }
    }
}
