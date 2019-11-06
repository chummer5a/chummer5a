using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup'
    public class ResultGroupPostGroup : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.MyGroup'
        public SINnerGroup MyGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.MyGroup'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup()'
        public ResultGroupPostGroup()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup()'
        {
            MyGroup = new SINnerGroup();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup(SINnerGroup)'
        public ResultGroupPostGroup(SINnerGroup group)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup(SINnerGroup)'
        {
            MyGroup = group;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup(Exception)'
        public ResultGroupPostGroup(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPostGroup.ResultGroupPostGroup(Exception)'
        {

        }
    }
}
