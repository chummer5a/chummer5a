using Microsoft.AspNetCore.Identity;
using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail'
    public class ResultAccountGetUserByEmail : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.MyApplicationUser'
        public ApplicationUser MyApplicationUser { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.MyApplicationUser'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail()'
        public ResultAccountGetUserByEmail()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail()'
        {
            MyApplicationUser = null;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(ApplicationUser)'
        public ResultAccountGetUserByEmail(ApplicationUser user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(ApplicationUser)'
        {
            MyApplicationUser = user;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(Exception)'
        public ResultAccountGetUserByEmail(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(Exception)'
        {

        }
    }
}
