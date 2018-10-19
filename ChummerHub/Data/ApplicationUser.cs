using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// A way for a GM to search for all the characters of "his" group
        /// </summary>
        [PersonalData]
        public string Groupname { get; set; }
        
    }
}
