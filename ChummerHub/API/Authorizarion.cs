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
    public static class Authorization
    {
        public static class Constants
        {
            public const string CreateOperationName = "Create";
            public const string ReadOperationName = "Read";
            public const string UpdateOperationName = "Update";
            public const string DeleteOperationName = "Delete";
            public const string ApproveOperationName = "Approve";
            public const string RejectOperationName = "Reject";
                   
            public const string UserRoleAdmin = "Administrator";
            //public const string ManagersRole = "Manager";
            public const string UserRoleRegistered = "RegisteredUser";
            public const string UserRoleConfirmed = "ConfirmedUser";
            public const string UserRoleArchetypeAdmin = "ArchetypeAdmin";
            public const string UserRolePublicAccess = "PublicAccess";

        }
    }
}
