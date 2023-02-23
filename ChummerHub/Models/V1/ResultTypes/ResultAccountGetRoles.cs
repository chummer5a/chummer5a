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
    public class ResultAccountGetRoles : ResultBase
    {
        public List<String> Roles { get; set; }
        public List<String> PossibleRoles { get; set; }

        public ResultAccountGetRoles()
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }

        public ResultAccountGetRoles(IList<string> roles)
        {
            PossibleRoles = new List<string>();
            Roles = roles != null ? roles.ToList() : new List<string>();
        }

        public ResultAccountGetRoles(IList<string> roles, IList<string> possibleRoles)
        {
            PossibleRoles = new List<string>();
            Roles = new List<string>();
            if (roles != null)
                Roles = roles.ToList();
            if (possibleRoles != null)
                PossibleRoles = possibleRoles.ToList();
        }

        public ResultAccountGetRoles(Exception e) : base(e)
        {
            Roles = new List<string>();
            PossibleRoles = new List<string>();
        }
    }
}
