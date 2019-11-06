using Microsoft.AspNetCore.Identity;
using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization'
    public class ResultAccountGetUserByAuthorization : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.MyApplicationUser'
        public ApplicationUser MyApplicationUser { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.MyApplicationUser'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization()'
        public ResultAccountGetUserByAuthorization()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization()'
        {
            MyApplicationUser = null;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization(ApplicationUser)'
        public ResultAccountGetUserByAuthorization(ApplicationUser user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization(ApplicationUser)'
        {
            MyApplicationUser = user;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization(Exception)'
        public ResultAccountGetUserByAuthorization(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByAuthorization.ResultAccountGetUserByAuthorization(Exception)'
        {

        }
    }
}
