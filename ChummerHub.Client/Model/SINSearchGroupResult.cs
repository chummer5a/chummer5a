using System.Collections.Generic;

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
