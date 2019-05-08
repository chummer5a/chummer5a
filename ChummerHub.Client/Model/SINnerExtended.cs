using System;
using SINners.Models;

namespace SINners.Models
{
    public partial class SINnerExtended
    {
        public SINnerExtended(SINner sinner)
        {
            this.SiNnerId = sinner.Id;
            this.Id = Guid.NewGuid();
        }
    }
}
