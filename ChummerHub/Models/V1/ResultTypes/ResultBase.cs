using ChummerHub.API;
using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase'
    public class ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.MyException'
        public HubException MyException { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.MyException'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.CallSuccess'
        public bool CallSuccess { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.CallSuccess'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ErrorText'
        public string ErrorText { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ErrorText'
        //public Object PayLoad { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ResultBase(Exception)'
        public ResultBase(Exception e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ResultBase(Exception)'
        {
            if (e is HubException exception)
                MyException = exception;
            else
            {
                MyException = new HubException(e.Message, e);
            }

            CallSuccess = false;
            ErrorText = e.Message;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ResultBase()'
        public ResultBase()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultBase.ResultBase()'
        {
            CallSuccess = true;
        }

    }
}
