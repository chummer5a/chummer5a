using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ChummerHub.API
{
    [DataContract]
    [Serializable]
    public class HubException : Exception
    {

        protected HubException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HubException()
        {
        }

        public HubException(string message)
            : base(message)
        {
        }

        public HubException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public override string ToString()
        {
            var strOut = base.ToString() + " Message: " + this.Message;
            return strOut;
        }

        

       


    }
}
