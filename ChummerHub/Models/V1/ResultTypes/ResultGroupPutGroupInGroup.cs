using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    [DataContract]
    [Serializable]
    public class ResultGroupPutGroupInGroup : ResultBase
    {
        public SINnerGroup MyGroup { get; set; }

        public ResultGroupPutGroupInGroup(SINnerGroup group)
        {
            MyGroup = group;
        }

        public ResultGroupPutGroupInGroup(Exception e) : base(e)
        {

        }
    }
}
