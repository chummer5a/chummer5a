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
            byte[] achrHash = GetHash(inputString);
            StringBuilder sb = new StringBuilder(achrHash.Length);
            foreach (byte b in achrHash)
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
