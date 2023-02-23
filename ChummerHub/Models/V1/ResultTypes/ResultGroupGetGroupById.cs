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

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById'
    public class ResultGroupGetGroupById : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.MyGroup'
        public SINnerGroup MyGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.MyGroup'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById()'
        public ResultGroupGetGroupById()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById()'
        {
            MyGroup = new SINnerGroup();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(SINnerGroup)'
        public ResultGroupGetGroupById(SINnerGroup group)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(SINnerGroup)'
        {
            MyGroup = group;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(Exception)'
        public ResultGroupGetGroupById(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupGetGroupById.ResultGroupGetGroupById(Exception)'
        {

        }
    }
}
