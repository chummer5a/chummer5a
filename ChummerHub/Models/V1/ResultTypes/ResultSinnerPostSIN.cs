using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN'
    public class ResultSinnerPostSIN : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.MySINners'
        public List<SINner> MySINners { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.MySINners'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN()'
        public ResultSinnerPostSIN()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN()'
        {
            MySINners = new List<SINner>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN(List<SINner>)'
        public ResultSinnerPostSIN(List<SINner> list)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN(List<SINner>)'
        {
            MySINners = list;
            if (list != null)
            {
                foreach(var sinner in list)
                {
                    if (sinner.MyGroup == null)
                    {
                        sinner.MyGroup = new SINnerGroup();
                    }
                }
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN(Exception)'
        public ResultSinnerPostSIN(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerPostSIN.ResultSinnerPostSIN(Exception)'
        {

        }
    }
}
