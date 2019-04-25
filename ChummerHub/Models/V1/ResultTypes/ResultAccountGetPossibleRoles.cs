using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetPossibleRoles : ResultBase
    {
        public List<string> AllRoles { get; set; }

        public ResultAccountGetPossibleRoles(List<string> list)
        {
            AllRoles = list;
        }

        public ResultAccountGetPossibleRoles(Exception e) : base(e)
        {

        }
    }
}
