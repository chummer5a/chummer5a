using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetRoles : ResultBase
    {
        public List<String> Roles { get; set; }
        public List<String> PossibleRoles { get; set; }

        public ResultAccountGetRoles()
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }

        public ResultAccountGetRoles(IList<string> roles)
        {
            PossibleRoles = new List<string>();
            if (roles != null)
                Roles = roles.ToList();
            else
                Roles = new List<string>();
        }

        public ResultAccountGetRoles(IList<string> roles, IList<string> possibleRoles)
        {
            PossibleRoles = new List<string>();
            Roles = new List<string>();
            if (roles != null)
                Roles = roles.ToList();
            if (possibleRoles != null)
                PossibleRoles = possibleRoles.ToList();
        }

        public ResultAccountGetRoles(Exception e) : base(e)
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }
    }
}
