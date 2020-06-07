using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById'
    public class ResultGroupGetGroupById : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.MyGroup'
        public SINnerGroup MyGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.MyGroup'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById()'
        public ResultGroupGetGroupById()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById()'
        {
            MyGroup = new SINnerGroup();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(SINnerGroup)'
        public ResultGroupGetGroupById(SINnerGroup group)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(SINnerGroup)'
        {
            MyGroup = group;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(Exception)'
        public ResultGroupGetGroupById(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(Exception)'
        {

        }
    }
}
