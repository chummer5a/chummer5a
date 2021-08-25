using System;

namespace ChummerHub.Models.V1
{
    public class ResultAccountGetSinnersByAuthorization : ResultBase
    {
        public string UserEmail { get; set; }

        public SINSearchGroupResult MySINSearchGroupResult { get; set; }

        public ResultAccountGetSinnersByAuthorization()
        {
            MySINSearchGroupResult = new SINSearchGroupResult();
        }

        public ResultAccountGetSinnersByAuthorization(SINSearchGroupResult ssg)
        {
            MySINSearchGroupResult = ssg;
        }

        public ResultAccountGetSinnersByAuthorization(Exception e) : base(e)
        {

        }
    }
}
