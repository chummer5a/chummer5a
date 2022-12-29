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
