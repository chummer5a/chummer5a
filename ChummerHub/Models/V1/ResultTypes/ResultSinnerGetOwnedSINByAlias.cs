using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias'
    public class ResultSinnerGetOwnedSINByAlias : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.MySINners'
        public List<SINner> MySINners { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.MySINners'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias()'
        public ResultSinnerGetOwnedSINByAlias()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias()'
        {
            MySINners = new List<SINner>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(List<SINner>)'
        public ResultSinnerGetOwnedSINByAlias(List<SINner> list)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(List<SINner>)'
        {
            MySINners = list;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(Exception)'
        public ResultSinnerGetOwnedSINByAlias(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(Exception)'
        {

        }
    }
}
