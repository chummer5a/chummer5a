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
using System.Linq;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles'
    public class ResultAccountGetRoles : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.Roles'
        public List<String> Roles { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.Roles'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.PossibleRoles'
        public List<String> PossibleRoles { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.PossibleRoles'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles()'
        public ResultAccountGetRoles()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles()'
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(IList<string>)'
        public ResultAccountGetRoles(IList<string> roles)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(IList<string>)'
        {
            PossibleRoles = new List<string>();
            Roles = roles != null ? roles.ToList() : new List<string>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(IList<string>, IList<string>)'
        public ResultAccountGetRoles(IList<string> roles, IList<string> possibleRoles)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(IList<string>, IList<string>)'
        {
            PossibleRoles = new List<string>();
            Roles = new List<string>();
            if (roles != null)
                Roles = roles.ToList();
            if (possibleRoles != null)
                PossibleRoles = possibleRoles.ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(Exception)'
        public ResultAccountGetRoles(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetRoles.ResultAccountGetRoles(Exception)'
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }
    }
}
