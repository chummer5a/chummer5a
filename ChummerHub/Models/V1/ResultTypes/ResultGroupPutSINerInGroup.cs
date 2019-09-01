using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup'
    public class ResultGroupPutSINerInGroup : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.MySINner'
        public SINner MySINner { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.MySINner'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup()'
        public ResultGroupPutSINerInGroup()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup()'
        {
            MySINner = new SINner();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(SINner)'
        public ResultGroupPutSINerInGroup(SINner sin)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(SINner)'
        {
            MySINner = sin;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(Exception)'
        public ResultGroupPutSINerInGroup(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(Exception)'
        {

        }
    }
}
