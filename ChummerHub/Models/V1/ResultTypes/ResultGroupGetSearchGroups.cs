using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChummerHub.Models.V1
{
    public class ResultGroupGetSearchGroups : ResultBase
    {
        public SINSearchGroupResult MySearchGroupResult { get; set; }

        public ResultGroupGetSearchGroups(SINSearchGroupResult ssg)
        {
            MySearchGroupResult = ssg;
        }

        public ResultGroupGetSearchGroups(Exception e) : base(e)
        {

        }

    }
}
