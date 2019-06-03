using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
    public class ResultSinnerGetSINnerVisibilityById : ResultBase
    {
        public List<SINnerUserRight> UserRights { get; set; }

        public ResultSinnerGetSINnerVisibilityById()
        {
            UserRights = new List<SINnerUserRight>();
        }

        public ResultSinnerGetSINnerVisibilityById(List<SINnerUserRight> list)
        {
            UserRights = list;
        }

        public ResultSinnerGetSINnerVisibilityById(Exception e) : base(e)
        {

        }
    }
}
