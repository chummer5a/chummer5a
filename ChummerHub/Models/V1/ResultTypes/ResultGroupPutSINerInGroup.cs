using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultGroupPutSINerInGroup : ResultBase
    {
        public SINner MySINner { get; set; }

        public ResultGroupPutSINerInGroup()
        {
            MySINner = new SINner();
        }

        public ResultGroupPutSINerInGroup(SINner sin)
        {
            MySINner = sin;
        }

        public ResultGroupPutSINerInGroup(Exception e) : base(e)
        {

        }
    }
}
