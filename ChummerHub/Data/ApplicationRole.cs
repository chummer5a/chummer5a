using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub.Data
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string MyRole { get; set; }
    }
}
