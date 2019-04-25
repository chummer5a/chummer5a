using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetRoles : ResultBase
    {
        public List<String> Roles { get; set; }

        public ResultAccountGetRoles(IList<string> roles)
        {
            if (roles != null)
                Roles = roles.ToList();
            else
                Roles = new List<string>();
        }

        public ResultAccountGetRoles(Exception e) : base(e)
        {
            Roles = new List<string>();
        }
    }
}
