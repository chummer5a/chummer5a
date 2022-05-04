/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
