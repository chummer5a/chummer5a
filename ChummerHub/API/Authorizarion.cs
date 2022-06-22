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
namespace ChummerHub.API
{
    public class Authorizarion
    {
        public class Constants
        {
            public static readonly string CreateOperationName = "Create";
            public static readonly string ReadOperationName = "Read";
            public static readonly string UpdateOperationName = "Update";
            public static readonly string DeleteOperationName = "Delete";
            public static readonly string ApproveOperationName = "Approve";
            public static readonly string RejectOperationName = "Reject";

            public static readonly string AdministratorsRole = "Administrator";
            public static readonly string ManagersRole = "Manager";
            public static readonly string UserRoleRegistered = "RegisteredUser";
            public static readonly string UserRoleConfirmed = "ConfirmedUser";
            public static readonly string UserRoleArchetype = "ArchetypeAdmin";


        }
    }
}
