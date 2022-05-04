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
