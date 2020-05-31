using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization'
    public class ResultAccountGetSinnersByAuthorization : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.MySINSearchGroupResult'
        public SINSearchGroupResult MySINSearchGroupResult { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.MySINSearchGroupResult'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization()'
        public ResultAccountGetSinnersByAuthorization()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization()'
        {
            MySINSearchGroupResult = new SINSearchGroupResult();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization(SINSearchGroupResult)'
        public ResultAccountGetSinnersByAuthorization(SINSearchGroupResult ssg)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization(SINSearchGroupResult)'
        {
            MySINSearchGroupResult = ssg;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization(Exception)'
        public ResultAccountGetSinnersByAuthorization(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetSinnersByAuthorization.ResultAccountGetSinnersByAuthorization(Exception)'
        {

        }
    }
}
