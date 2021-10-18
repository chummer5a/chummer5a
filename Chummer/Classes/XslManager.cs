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
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Xsl;

namespace Chummer
{
    /// <summary>
    /// Class for managing cached Xsl transforms and other stuff related to Xsl and Xslt files
    /// </summary>
    public static class XslManager
    {
        // Cache of compiled XSLTs to speed up repeated prints of the same character sheet
        private static readonly ConcurrentDictionary<string, Tuple<DateTime, XslCompiledTransform>> s_dicCompiledTransforms
            = new ConcurrentDictionary<string, Tuple<DateTime, XslCompiledTransform>>();

        /// <summary>
        /// Get the compiled Xsl Transform of an Xsl file. Will throw exceptions if anything goes awry.
        /// If we've already compiled the same Xsl Transform before, we'll fetch the cached version of that transform instead of repeating it.
        /// </summary>
        /// <param name="strXslFilePath">Absolute path to the Xsl file to be transformed.</param>
        /// <returns>The compiled Xsl transform of <paramref name="strXslFilePath"/>.</returns>
        public static XslCompiledTransform GetTransformForFile(string strXslFilePath)
        {
            if (!File.Exists(strXslFilePath))
                throw new FileNotFoundException(nameof(strXslFilePath));

            DateTime datLastWriteTimeUtc = File.GetLastWriteTimeUtc(strXslFilePath);

            XslCompiledTransform objReturn;

            if (!s_dicCompiledTransforms.TryGetValue(
                    strXslFilePath, out Tuple<DateTime, XslCompiledTransform> tupCachedData)
                || tupCachedData.Item1 <= datLastWriteTimeUtc)
            {
#if DEBUG
                objReturn = new XslCompiledTransform(true);
#else
                objReturn = new XslCompiledTransform();
#endif
                objReturn.Load(strXslFilePath);

                s_dicCompiledTransforms.TryRemove(strXslFilePath, out _);
                s_dicCompiledTransforms.TryAdd(
                    strXslFilePath, new Tuple<DateTime, XslCompiledTransform>(datLastWriteTimeUtc, objReturn));
            }
            else
                objReturn = tupCachedData.Item2;

            return objReturn;
        }
    }
}
