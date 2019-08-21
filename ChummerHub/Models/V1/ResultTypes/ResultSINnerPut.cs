using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut'
    public class ResultSINnerPut : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.MySINner'
        public SINner MySINner { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.MySINner'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut()'
        public ResultSINnerPut()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut()'
        {
            MySINner = new SINner();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut(SINner)'
        public ResultSINnerPut(SINner sin)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut(SINner)'
        {
            MySINner = sin;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut(Exception)'
        public ResultSINnerPut(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSINnerPut.ResultSINnerPut(Exception)'
        {

        }
    }
}
