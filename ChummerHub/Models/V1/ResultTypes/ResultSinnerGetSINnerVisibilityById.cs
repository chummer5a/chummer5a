using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById'
    public class ResultSinnerGetSINnerVisibilityById : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.UserRights'
        public List<SINnerUserRight> UserRights { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.UserRights'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById()'
        public ResultSinnerGetSINnerVisibilityById()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById()'
        {
            UserRights = new List<SINnerUserRight>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById(List<SINnerUserRight>)'
        public ResultSinnerGetSINnerVisibilityById(List<SINnerUserRight> list)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById(List<SINnerUserRight>)'
        {
            UserRights = list;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById(Exception)'
        public ResultSinnerGetSINnerVisibilityById(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetSINnerVisibilityById.ResultSinnerGetSINnerVisibilityById(Exception)'
        {

        }
    }
}
