using System;
using System.Runtime.Serialization;
using ChummerHub.API;

namespace ChummerHub.Models.V1
{
    public class ResultBase
    {
        public HubException MyException { get; set; }
        public bool CallSuccess { get; set; }
        public string ErrorText { get; set; }
        //public Object PayLoad { get; set; }

        public ResultBase(Exception e)
        {
            if (e is HubException)
                MyException = e as HubException;
            else
            {
                MyException = new HubException(e.Message, e);
            }

            CallSuccess = false;
            ErrorText = e.Message;
        }

        public ResultBase()
        {
            CallSuccess = true;
        }

    }
}
