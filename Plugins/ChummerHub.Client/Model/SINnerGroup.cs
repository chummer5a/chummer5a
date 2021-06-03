using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ChummerHub.Client.Sinners
{
    public partial class SINnerGroup
    {
        public string Password
        {
            set => PasswordHash = !string.IsNullOrEmpty(value) ? GetHashString(value) : null;
        }

        public SINnerGroup(SINnerSearchGroup searchGroup)
        {
            if (searchGroup != null)
            {

                Id = searchGroup.Id;
                Groupname = searchGroup.Groupname;
                IsPublic = searchGroup.IsPublic;
                MyParentGroupId = searchGroup.MyParentGroupId;
                Language = searchGroup.Language;
                MyAdminIdentityRole = searchGroup.MyAdminIdentityRole;
            }
        }
        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
