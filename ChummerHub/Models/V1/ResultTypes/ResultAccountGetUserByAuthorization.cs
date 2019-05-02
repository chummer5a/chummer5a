using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetUserByAuthorization : ResultBase
    {
        public ApplicationUser MyApplicationUser { get; set; }

        public ResultAccountGetUserByAuthorization()
        {
            MyApplicationUser = null;
        }

        public ResultAccountGetUserByAuthorization(ApplicationUser user)
        {
            MyApplicationUser = user;
        }

        public ResultAccountGetUserByAuthorization(Exception e) : base(e)
        {

        }
    }
}
