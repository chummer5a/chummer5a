using ChummerHub.API;
using System;
using System.Runtime.Serialization;

namespace ChummerHub
{
    [DataContract]
    [Serializable]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException'
    public class NoUserRightException : HubException
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException'
    {
        private readonly string userName;
        private Guid? id;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.UserName'
        public string UserName => userName;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.UserName'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.SINnerId'
        public Guid? SINnerId => id;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.SINnerId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException()'
        public NoUserRightException()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException()'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string)'
        public NoUserRightException(string message) : base(message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string)'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string, Guid?)'
        public NoUserRightException(string userName, Guid? id) : base(message: "User " + userName + " may not edit SINner with Id " + id + "!")
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string, Guid?)'
        {
            this.userName = userName;
            this.id = id;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string, Exception)'
        public NoUserRightException(string message, Exception innerException) : base(message, innerException)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(string, Exception)'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(SerializationInfo, StreamingContext)'
        protected NoUserRightException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoUserRightException.NoUserRightException(SerializationInfo, StreamingContext)'
        {
        }
    }
}
