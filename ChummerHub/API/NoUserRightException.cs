/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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

        public NoUserRightException(string userName, Guid? id) : base(message: "User " + userName + " may not edit SINner with Id " + id + "!")
        {
            this.userName = userName;
            this.id = id;
        }

        public NoUserRightException(string userName, Guid? id, string message) : base("User " + userName + " may not edit SINner with Id " + id + "!" + Environment.NewLine + message)
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
