using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
    public class ResultSinnerGetSINnerVisibilityById : ResultBase
    {
        public List<SINerUserRight> UserRights { get; set; }

        public ResultSinnerGetSINnerVisibilityById()
        {
            UserRights = new List<SINerUserRight>();
        }

        public ResultSinnerGetSINnerVisibilityById(List<SINerUserRight> list)
        {
            UserRights = list;
        }

        public ResultSinnerGetSINnerVisibilityById(Exception e) : base(e)
        {

        }
    }
}
