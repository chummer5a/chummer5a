using ChummerHub.API;
using System;
using System.Runtime.Serialization;

namespace ChummerHub
{
    [DataContract]
    [Serializable]
    public class NoUserRightException : HubException
    {
        private string userName;
        private Guid? id;

        public string UserName => userName;

        public Guid? SINnerId => id;

        public NoUserRightException()
        {
        }

        public NoUserRightException(string message) : base(message)
        {
        }

        public NoUserRightException(string userName, Guid? id) : base(message: "User " + userName + " may not edit SINner with Id " + id + "!")
        {
            this.userName = userName;
            this.id = id;
        }

        public NoUserRightException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoUserRightException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
