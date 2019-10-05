using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById'
    public class ResultSinnerGetSINById : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.MySINner'
        public SINner MySINner { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.MySINner'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById()'
        public ResultSinnerGetSINById()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById()'
        {
            MySINner = new SINner();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById(SINner)'
        public ResultSinnerGetSINById(SINner sinner)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById(SINner)'
        {
            MySINner = sinner;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById(Exception)'
        public ResultSinnerGetSINById(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINById.ResultSinnerGetSINById(Exception)'
        {

        }
    }
}
