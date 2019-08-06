using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetUserByEmail : ResultBase
    {
        public ApplicationUser MyApplicationUser { get; set; }

        public ResultAccountGetUserByEmail()
        {
            MyApplicationUser = null;
        }

        public ResultAccountGetUserByEmail(ApplicationUser user)
        {
            MyApplicationUser = user;
        }

        public ResultAccountGetUserByEmail(Exception e) : base(e)
        {
            
        }
    }
}
