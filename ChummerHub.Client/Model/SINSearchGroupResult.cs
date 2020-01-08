using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SINners.Models
{
    public partial class SINSearchGroupResult
    {
        public SINSearchGroupResult(SINnerGroup myGroup)
        {
            this.SinGroups = new List<SINnerSearchGroup>();
            SINnerSearchGroup ssg = new SINnerSearchGroup(myGroup);
            SinGroups.Add(ssg);
        }
    }
}
