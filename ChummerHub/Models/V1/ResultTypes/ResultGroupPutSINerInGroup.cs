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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup'
    public class ResultGroupPutSINerInGroup : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.MySINner'
        public SINner MySINner { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.MySINner'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup()'
        public ResultGroupPutSINerInGroup()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup()'
        {
            MySINner = new SINner();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(SINner)'
        public ResultGroupPutSINerInGroup(SINner sin)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(SINner)'
        {
            MySINner = sin;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(Exception)'
        public ResultGroupPutSINerInGroup(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultGroupPutSINerInGroup.ResultGroupPutSINerInGroup(Exception)'
        {

        }
    }
}
