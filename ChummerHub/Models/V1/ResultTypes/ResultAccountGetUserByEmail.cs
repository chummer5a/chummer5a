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
using Microsoft.AspNetCore.Identity;
using System;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail'
    public class ResultAccountGetUserByEmail : ResultBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.MyApplicationUser'
        public ApplicationUser MyApplicationUser { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.MyApplicationUser'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail()'
        public ResultAccountGetUserByEmail()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail()'
        {
            MyApplicationUser = null;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(ApplicationUser)'
        public ResultAccountGetUserByEmail(ApplicationUser user)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(ApplicationUser)'
        {
            MyApplicationUser = user;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(Exception)'
        public ResultAccountGetUserByEmail(Exception e) : base(e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ResultAccountGetUserByEmail.ResultAccountGetUserByEmail(Exception)'
        {

        }
    }
}
