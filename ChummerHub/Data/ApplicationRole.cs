using Microsoft.AspNetCore.Identity;
using System;

namespace ChummerHub.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole'
    public class ApplicationRole : IdentityRole<Guid>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.MyRole'
        public string MyRole { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.MyRole'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.ApplicationRole()'
        public ApplicationRole()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.ApplicationRole()'
        {
            this.MyRole = "default";
            this.Name = "default";
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.ApplicationRole(string)'
        public ApplicationRole(string MyRole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationRole.ApplicationRole(string)'
        {
            this.MyRole = MyRole;
            this.Name = MyRole;
            this.Id = Guid.NewGuid();
        }
    }
}
