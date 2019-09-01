using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups'
    public class ResultGroupGetSearchGroups : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.MySearchGroupResult'
        public SINSearchGroupResult MySearchGroupResult { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.MySearchGroupResult'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups()'
        public ResultGroupGetSearchGroups()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups()'
        {
            MySearchGroupResult = new SINSearchGroupResult();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups(SINSearchGroupResult)'
        public ResultGroupGetSearchGroups(SINSearchGroupResult ssg)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups(SINSearchGroupResult)'
        {
            MySearchGroupResult = ssg;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups(Exception)'
        public ResultGroupGetSearchGroups(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetSearchGroups.ResultGroupGetSearchGroups(Exception)'
        {

        }

    }
}
