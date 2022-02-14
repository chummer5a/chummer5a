using System.Collections.Generic;

namespace ChummerHub.Client.Sinners
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
