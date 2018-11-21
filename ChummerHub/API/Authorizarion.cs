using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            public static readonly string RegisteredUserRole = "RegisteredUser";
            
        }
    }
}
