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

using System.Text;

namespace Translator
{
    public static class StringExtensions
    {
        /// <summary>
        /// Clean an XPath string.
        /// </summary>
        /// <param name="strSearch">String to clean.</param>
        public static string CleanXPath(this string strSearch)
        {
            int intQuotePos = strSearch.IndexOf('"');
            if (intQuotePos == -1)
            {
                return '\"' + strSearch + '\"';
            }

            StringBuilder objReturn = new StringBuilder("concat(\"");
            for (; intQuotePos != -1; intQuotePos = strSearch.IndexOf('"'))
            {
                string strSubstring = strSearch.Substring(0, intQuotePos);
                objReturn.Append(strSubstring);
                objReturn.Append("\", '\"', \"");
                strSearch = strSearch.Substring(intQuotePos + 1);
            }
            objReturn.Append(strSearch);
            objReturn.Append("\")");
            return objReturn.ToString();
        }
    }
}
