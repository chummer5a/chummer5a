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
using System.IO;
using System.Threading;
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
                GC.KeepAlive(GlobalSettings.SafeXmlReaderSettings); // Do this here so we fire the constructor for GlobalSettings separately
                // Attempt to cache all XML files that are used the most.
                Utils.RunWithoutThreadLock(t => ParallelExtensions.ForEachAsync(Utils.BasicDataFileNames, CacheCommonFileAsync, t), token: TestContext.CancellationToken);
                async Task CacheCommonFileAsync(string strFile, CancellationToken token)
                {
                    await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false);
                    if (string.Equals(strFile, "settings.xml", Utils.PathStringComparison))
                    {
                        _ = await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                throw;
            }
#if MEMORYTESTING
            finally
            {
                TestContext.CancellationTokenSource.Dispose();
            }
#endif
        }

        [TestMethod]
        public void LoadAllLanguageDataFiles()
        {
            try
            {
                // Attempt to load all languages to make sure they work properly
                // Start with the default language, just in case.
                Assert.IsTrue(LanguageManager.LoadLanguage(GlobalSettings.DefaultLanguage, TestContext.CancellationToken));
                Assert.IsTrue(string.IsNullOrEmpty(LanguageManager.ManagerErrorMessage));
                foreach (string strFilePath in Directory.EnumerateFiles(Utils.GetLanguageFolderPath, "*.xml"))
                {
                    if (strFilePath.EndsWith("_data.xml", Utils.PathStringComparison))
                        continue;
                    string strExportLanguage = Path.GetFileNameWithoutExtension(strFilePath);
                    if (strExportLanguage != GlobalSettings.DefaultLanguage)
                        Assert.IsTrue(LanguageManager.LoadLanguage(strExportLanguage, TestContext.CancellationToken));
                }
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                throw;
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
