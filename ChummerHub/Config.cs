using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ChummerHub
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Config'
    public class Config
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Config'
    {
        internal static List<ApplicationUser> GetAdminUsers()
        {
            var a = new ApplicationUser()
            {
                Email = "archon.megalon@gmail.com",
                EmailConfirmed = true,
                LockoutEnabled = false,
                NormalizedEmail = "ARCHON.MEGALON@GMAIL.COM",
                NormalizedUserName = "archon",
                UserName = "Archon",
#pragma warning disable CS0612 // 'ApplicationUser.Groupname' is obsolete
                Groupname = "MyPlayGroup1",
#pragma warning restore CS0612 // 'ApplicationUser.Groupname' is obsolete
                Id = Guid.Parse("9FC744C1-FC22-4EDA-6A05-08D64B08AE81"),
            };
            var b = new ApplicationUser()
            {
                Email = "chummer5isalive@gmail.com",
                EmailConfirmed = true,
                LockoutEnabled = false,
                NormalizedEmail = "CHUMMER5ISALIVE@GMAIL.COM",
                NormalizedUserName = "chummer",
                UserName = "Chummer",
#pragma warning disable CS0612 // 'ApplicationUser.Groupname' is obsolete
                Groupname = "MyPlayGroup2",
#pragma warning restore CS0612 // 'ApplicationUser.Groupname' is obsolete
                Id = Guid.Parse("AFC744C1-FC22-4EDA-6A05-08D64B08AE81"),
            };
            var list = new List<ApplicationUser>() { a, b };
            return list;
        }
    }
}
