using System;

namespace ChummerHub.Models.V1
{
    public class ResultSinnerGetSINById : ResultBase
    {
        public SINner MySINner { get; set; }

        public ResultSinnerGetSINById()
        {
            MySINner = new SINner();
        }

        public ResultSinnerGetSINById(SINner sinner)
        {
            MySINner = sinner;
        }

        public ResultSinnerGetSINById(Exception e) : base(e)
        {

        }
    }
}
