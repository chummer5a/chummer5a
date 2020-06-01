using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById'
    public class ResultSinnerGetSINnerGroupFromSINerById : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.MySINnerGroup'
        public SINnerGroup MySINnerGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.MySINnerGroup'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById()'
        public ResultSinnerGetSINnerGroupFromSINerById()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById()'
        {
            MySINnerGroup = new SINnerGroup();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById(SINnerGroup)'
        public ResultSinnerGetSINnerGroupFromSINerById(SINnerGroup group)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById(SINnerGroup)'
        {
            MySINnerGroup = group;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById(Exception)'
        public ResultSinnerGetSINnerGroupFromSINerById(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerGroupFromSINerById.ResultSinnerGetSINnerGroupFromSINerById(Exception)'
        {

        }
    }
}
