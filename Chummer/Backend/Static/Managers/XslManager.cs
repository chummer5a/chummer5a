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
using System.IO;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Chummer
{
    /// <summary>
    /// Class for managing cached Xsl transforms and other stuff related to Xsl and Xslt files
    /// </summary>
    public static class XslManager
    {
        // Cache of compiled XSLTs to speed up repeated prints of the same character sheet
        private static readonly LockingDictionary<string, Tuple<DateTime, XslCompiledTransform>> s_dicCompiledTransforms
            = new LockingDictionary<string, Tuple<DateTime, XslCompiledTransform>>();

        /// <summary>
        /// Get the compiled Xsl Transform of an Xsl file. Will throw exceptions if anything goes awry.
        /// If we've already compiled the same Xsl Transform before, we'll fetch the cached version of that transform instead of repeating it.
        /// </summary>
        /// <param name="strXslFilePath">Absolute path to the Xsl file to be transformed.</param>
        /// <returns>The compiled Xsl transform of <paramref name="strXslFilePath"/>.</returns>
        public static XslCompiledTransform GetTransformForFile(string strXslFilePath)
        {
            return GetTransformForFileCoreAsync(true, strXslFilePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the compiled Xsl Transform of an Xsl file. Will throw exceptions if anything goes awry.
        /// If we've already compiled the same Xsl Transform before, we'll fetch the cached version of that transform instead of repeating it.
        /// </summary>
        /// <param name="strXslFilePath">Absolute path to the Xsl file to be transformed.</param>
        /// <returns>The compiled Xsl transform of <paramref name="strXslFilePath"/>.</returns>
        public static Task<XslCompiledTransform> GetTransformForFileAsync(string strXslFilePath)
        {
            return GetTransformForFileCoreAsync(false, strXslFilePath);
        }

        /// <summary>
        /// Get the compiled Xsl Transform of an Xsl file. Will throw exceptions if anything goes awry.
        /// If we've already compiled the same Xsl Transform before, we'll fetch the cached version of that transform instead of repeating it.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strXslFilePath">Absolute path to the Xsl file to be transformed.</param>
        /// <returns>The compiled Xsl transform of <paramref name="strXslFilePath"/>.</returns>
        private static async Task<XslCompiledTransform> GetTransformForFileCoreAsync(bool blnSync, string strXslFilePath)
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
                if (blnSync)
                {
                    objReturn.Load(strXslFilePath);
                    // ReSharper disable once MethodHasAsyncOverload
                    s_dicCompiledTransforms.Remove(strXslFilePath);
                    // ReSharper disable once MethodHasAsyncOverload
                    s_dicCompiledTransforms.TryAdd(
                        strXslFilePath, new Tuple<DateTime, XslCompiledTransform>(datLastWriteTimeUtc, objReturn));
                }
                else
                {
                    await Task.Run(() => objReturn.Load(strXslFilePath));
                    await s_dicCompiledTransforms.RemoveAsync(strXslFilePath);
                    await s_dicCompiledTransforms.TryAddAsync(
                        strXslFilePath, new Tuple<DateTime, XslCompiledTransform>(datLastWriteTimeUtc, objReturn));
                }
            }
            else
                objReturn = tupCachedData.Item2;

            return objReturn;
        }
    }
}
