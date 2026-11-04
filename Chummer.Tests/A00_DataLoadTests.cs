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
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chummer.Tests
{
    [TestClass]
    public class A00_DataLoadTests // Weird name is to make sure the tests methods herein are always the first ones to run, since data content ends up being used by a lot of other tests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void LoadAllBasicDataFiles()
        {
            try
            {
                // Attempt to cache all XML files that are used the most.
                Utils.RunWithoutThreadLock(() => ParallelExtensions.ForEachAsync(Utils.BasicDataFileNames, CacheCommonFileAsync, TestContext.CancellationToken), token: TestContext.CancellationToken);
                async Task CacheCommonFileAsync(string strFile)
                {
                    // Load default language data first for performance reasons
                    if (!GlobalSettings.Language.Equals(
                            GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage, token: TestContext.CancellationToken).ConfigureAwait(false);
                    }

                    await XmlManager.LoadXPathAsync(strFile, token: TestContext.CancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }
    }
}
