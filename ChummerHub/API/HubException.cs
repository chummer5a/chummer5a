using System;
using System.Runtime.Serialization;

namespace ChummerHub.API
{
    [DataContract]
    [Serializable]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException'
    public class HubException : Exception
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException'
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(SerializationInfo, StreamingContext)'
        protected HubException(SerializationInfo info, StreamingContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(SerializationInfo, StreamingContext)'
            : base(info, context)
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException()'
        public HubException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException()'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(string)'
        public HubException(string message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(string)'
            : base(message)
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(string, Exception)'
        public HubException(string message, Exception inner)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException.HubException(string, Exception)'
            : base(message, inner)
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HubException.ToString()'
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HubException.ToString()'
        {
            var strOut = base.ToString() + " Message: " + this.Message;
            return strOut;
        }






    }
}
