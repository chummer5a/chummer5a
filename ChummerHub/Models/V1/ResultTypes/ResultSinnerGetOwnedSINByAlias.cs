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
using System.Collections.Generic;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias'
    public class ResultSinnerGetOwnedSINByAlias : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.MySINners'
        public List<SINner> MySINners { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.MySINners'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias()'
        public ResultSinnerGetOwnedSINByAlias()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias()'
        {
            MySINners = new List<SINner>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(List<SINner>)'
        public ResultSinnerGetOwnedSINByAlias(List<SINner> list)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(List<SINner>)'
        {
            MySINners = list;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(Exception)'
        public ResultSinnerGetOwnedSINByAlias(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultSinnerGetOwnedSINByAlias.ResultSinnerGetOwnedSINByAlias(Exception)'
        {

        }
    }
}
