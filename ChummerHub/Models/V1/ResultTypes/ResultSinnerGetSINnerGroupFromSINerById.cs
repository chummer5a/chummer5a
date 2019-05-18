using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultSinnerGetSINnerGroupFromSINerById : ResultBase
    {
        public SINnerGroup MySINnerGroup { get; set; }

        public ResultSinnerGetSINnerGroupFromSINerById()
        {
            MySINnerGroup = new SINnerGroup();
        }

        public ResultSinnerGetSINnerGroupFromSINerById(SINnerGroup group)
        {
            MySINnerGroup = group;
        }

        public ResultSinnerGetSINnerGroupFromSINerById(Exception e) : base(e)
        {

        }
    }
}
