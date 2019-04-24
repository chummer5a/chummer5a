using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    [DataContract]
    [Serializable]
    public class ResultGroupPostGroup : ResultBase
    {
        public SINnerGroup MyGroup { get; set; }

        public ResultGroupPostGroup(SINnerGroup group)
        {
            MyGroup = group;
        }

        public ResultGroupPostGroup(Exception e) : base(e)
        {

        }
    }
}
