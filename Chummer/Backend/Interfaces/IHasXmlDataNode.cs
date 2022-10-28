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

using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public interface IHasXmlDataNode
    {
        /// <summary>
        /// Get a faster, read-only version of this item's Xml data node in a particular language.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strLanguage">Language to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default);

        /// <summary>
        /// Get a slower, writable version of this item's Xml data node in a particular language.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strLanguage">Language to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default);
    }

    public static class HasXmlDataNode
    {
        /// <summary>
        /// Get a faster, read-only version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static XPathNavigator GetNodeXPath(this IHasXmlDataNode objThis, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => objThis.GetNodeXPathCoreAsync(true, GlobalSettings.DefaultLanguage, token));
        }

        /// <summary>
        /// Get a faster, read-only version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static XPathNavigator GetNodeXPath(this IHasXmlDataNode objThis, string strLanguage, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => objThis.GetNodeXPathCoreAsync(true, strLanguage, token));
        }

        /// <summary>
        /// Get a faster, read-only version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static Task<XPathNavigator> GetNodeXPathAsync(this IHasXmlDataNode objThis, CancellationToken token = default)
        {
            return objThis.GetNodeXPathCoreAsync(false, GlobalSettings.DefaultLanguage, token);
        }

        /// <summary>
        /// Get a faster, read-only version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static Task<XPathNavigator> GetNodeXPathAsync(this IHasXmlDataNode objThis, string strLanguage, CancellationToken token = default)
        {
            return objThis.GetNodeXPathCoreAsync(false, strLanguage, token);
        }

        /// <summary>
        /// Get a slower, writable version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static XmlNode GetNode(this IHasXmlDataNode objThis, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => objThis.GetNodeCoreAsync(true, GlobalSettings.DefaultLanguage, token));
        }

        /// <summary>
        /// Get a slower, writable version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static XmlNode GetNode(this IHasXmlDataNode objThis, string strLanguage, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => objThis.GetNodeCoreAsync(true, strLanguage, token));
        }

        /// <summary>
        /// Get a slower, writable version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static Task<XmlNode> GetNodeAsync(this IHasXmlDataNode objThis, CancellationToken token = default)
        {
            return objThis.GetNodeCoreAsync(false, GlobalSettings.DefaultLanguage, token);
        }

        /// <summary>
        /// Get a slower, writable version of this item's Xml data node in the default Chummer language (English).
        /// </summary>
        public static Task<XmlNode> GetNodeAsync(this IHasXmlDataNode objThis, string strLanguage, CancellationToken token = default)
        {
            return objThis.GetNodeCoreAsync(false, strLanguage, token);
        }
    }
}
