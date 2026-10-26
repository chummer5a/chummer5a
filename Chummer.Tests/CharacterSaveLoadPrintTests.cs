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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;

namespace Chummer.Tests
{
    [TestClass]
    public class CharacterSaveLoadPrintTests
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Tests saving a given character.
        /// </summary>
        private static void SaveCharacter(Character objCharacter, string strPath, LzmaHelper.ChummerCompressionPreset eCompressionForChum5Lz = LzmaHelper.ChummerCompressionPreset.Fastest, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Assert.IsNotNull(objCharacter);
            try
            {
                Debug.WriteLine("Saving: " + objCharacter.Name + ", " + Path.GetFileName(strPath));
                objCharacter.Save(strPath, false, false, eCompressionForChum5Lz, token);
                Debug.WriteLine("Character saved: " + objCharacter.Name + " to " + Path.GetFileName(strPath));
            }
            catch (AssertFailedException e)
            {
                e = e.Demystify();
                string strErrorMessage = "Could not save " + Path.GetFileName(strPath) + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                e = e.Demystify();
                string strErrorMessage = "Could not save to " + strPath + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (Exception e)
            {
                e = e.Demystify();
                string strErrorMessage = "Exception while saving " + Path.GetFileName(strPath) + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
        }

        /// <summary>
        /// Tests exporting a given character.
        /// </summary>
        private static void DoAndSaveExport(Character objCharacter, string strExportLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Assert.IsNotNull(objCharacter);
            string strPath = Path.Combine(UnitTestUtils.CommonTestData.TestPathInfo.FullName, strExportLanguage + " " + Path.GetFileNameWithoutExtension(objCharacter.FileName) + ".xml");
            try
            {
                Debug.WriteLine("Exporting: " + objCharacter.Name + " to " + Path.GetFileName(strPath));
                CultureInfo objExportCultureInfo;
                try
                {
                    objExportCultureInfo = new CultureInfo(strExportLanguage);
                }
                catch (CultureNotFoundException)
                {
                    objExportCultureInfo = CultureInfo.InvariantCulture;
                }

                XmlDocument xmlDocument
                    = Utils.SafelyRunSynchronously(
                        t => objCharacter.GenerateExportXml(objExportCultureInfo,
                                                             strExportLanguage, t), token); // Need this wrapper to make unit test work
                using (FileStream objFileStream
                       = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    xmlDocument.Save(objFileStream);
                Debug.WriteLine("Character exported: " + objCharacter.Name + " to " + Path.GetFileName(strPath));
            }
            catch (AssertFailedException e)
            {
                e = e.Demystify();
                string strErrorMessage = "Could not export " + Path.GetFileName(objCharacter.FileName) + " in "
                                         + strExportLanguage + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                e = e.Demystify();
                string strErrorMessage = "Could not export to " + strPath + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
            catch (Exception e)
            {
                e = e.Demystify();
                string strErrorMessage = "Exception while exporting " + Path.GetFileName(objCharacter.FileName) + " in "
                                         + strExportLanguage + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
                throw e;
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod, Timeout(5 * 60 * 1000, CooperativeCancellation = true)]
        public void LoadCharacters()
        {
            try
            {
                List<Character> lstCharacters = new List<Character>(UnitTestUtils.CommonTestData.TestFileInfos.Length);
                foreach (Character objCharacter in UnitTestUtils.GetTestCharacters(TestContext.CancellationToken))
                {
                    Debug.WriteLine("Finished loading " + objCharacter.FileName);
                    lstCharacters.Add(objCharacter);
                }

                lstCharacters.Capacity = lstCharacters.Count; // Dummy command to make sure the test isn't optimized away.
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                Assert.Fail(ex.Message);
                throw ex;
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod, Timeout(5 * 60 * 1000, CooperativeCancellation = true)]
        public void LoadCharactersThenSaveAsChum5lz()
        {
            try
            {
                using (new FetchSafelyFromSafeObjectPool<XmlDocument>(Utils.XmlDocumentPool, out XmlDocument objXmlDocument))
                {
                    foreach (Character objCharacter in UnitTestUtils.GetTestCharacters(TestContext.CancellationToken))
                    {
                        string strFileName = Path.GetFileName(objCharacter.FileName) ?? "Unknown";
                        Debug.WriteLine("Checking " + strFileName);
                        string strDestination =
                            Path.Combine(UnitTestUtils.CommonTestData.TestPathInfo.FullName, "(Compressed) " + strFileName);
                        if (!strDestination.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                        {
                            if (strDestination.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                                strDestination += "lz";
                            else
                                strDestination += ".chum5lz";
                        }

                        SaveCharacter(objCharacter, strDestination, token: TestContext.CancellationToken);
                        // If our compression is malformed, we should run into a parse error when we try to load the XML data (don't load the full character because it's unnecessary)
                        objXmlDocument.LoadStandardFromLzmaCompressed(strDestination, token: TestContext.CancellationToken);
                    }
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

        [TestMethod, Timeout(5 * 60 * 1000, CooperativeCancellation = true)]
        public void LoadCharactersThenSaveIsDeterministic()
        {
            try
            {
                DefaultNodeMatcher objDiffNodeMatcher = new DefaultNodeMatcher(ElementSelectors.ByNameAndText);
                foreach (Character objCharacterControl in UnitTestUtils.GetTestCharacters(TestContext.CancellationToken))
                {
                    string strFileName = (Path.GetFileName(objCharacterControl.FileName) ?? "Unknown").CleanForFileName();
                    Debug.WriteLine("Saving Control for " + strFileName);
                    // First Load-Save cycle
                    string strDestinationControl = Path.Combine(UnitTestUtils.CommonTestData.TestPathInfo.FullName, "(Control) " + strFileName);
                    SaveCharacter(objCharacterControl, strDestinationControl, token: TestContext.CancellationToken);
                    Debug.WriteLine("Checking " + strFileName);
                    // Second Load-Save cycle
                    string strDestinationTest = Path.Combine(UnitTestUtils.CommonTestData.TestPathInfo.FullName, "(Test) " + strFileName);
                    Character objCharacterTest = UnitTestUtils.LoadCharacter(new FileInfo(strDestinationControl), token: TestContext.CancellationToken);
#if MEMORYTESTING
                    try
                    {
#endif
                    SaveCharacter(objCharacterTest, strDestinationTest, token: TestContext.CancellationToken);

                    try
                    {
                        // Check to see that character after first load cycle is consistent with character after second
                        using (FileStream controlFileStream =
                               File.Open(strDestinationControl, FileMode.Open, FileAccess.Read))
                        using (FileStream testFileStream =
                               File.Open(strDestinationTest, FileMode.Open, FileAccess.Read))
                        {
                            Diff myDiff = DiffBuilder
                                .Compare(controlFileStream)
                                .WithTest(testFileStream)
                                .CheckForSimilar()
                                .WithNodeFilter(x =>
                                    // image loading and unloading is not going to be deterministic due to compression algorithms
                                    x.Name != "mugshot"
                                    // Improvements list's order can be nondeterministic because improvements that get (re)generated on character load happen in a parallelized way
                                    && x.Name != "improvement")
                                .WithNodeMatcher(objDiffNodeMatcher)
                                // Improvements list's order can be nondeterministic because improvements that get (re)generated on character load happen in a parallelized way
                                .WithDifferenceEvaluator((x, y) =>
                                    string.Equals(x.ControlDetails.Target?.Name, "improvements", StringComparison.OrdinalIgnoreCase)
                                        ? EvaluateNodeWithChildrenIgnoringOrder(x, y)
                                        : DifferenceEvaluators.Default(x, y))
                                .IgnoreWhitespace()
                                .Build();
                            foreach (Difference diff in myDiff.Differences)
                            {
                                Console.WriteLine(diff.Comparison);
                                Console.WriteLine();
                            }

                            Assert.IsFalse(myDiff.HasDifferences(), myDiff.ToString());
                        }
                    }
                    catch (XmlSchemaException e)
                    {
                        e = e.Demystify();
                        Assert.Fail("Unexpected validation failure: " + e.Message);
                        throw e;
                    }
#if MEMORYTESTING
                    }
                    finally
                    {
                        objCharacterTest.Dispose();
                    }
#endif
                }

                ComparisonResult EvaluateNodeWithChildrenIgnoringOrder(Comparison objComparison, ComparisonResult eOutcome)
                {
                    if (eOutcome == ComparisonResult.EQUAL)
                        return eOutcome;
                    // First check for true equality, in which case we don't need to mess around with checking in an out-of-order way
                    if (DifferenceEvaluators.Default(objComparison, eOutcome) == ComparisonResult.EQUAL)
                        return ComparisonResult.EQUAL;
                    XmlNode xmlParentControl = objComparison.ControlDetails.Target;
                    XmlNode xmlParentTest = objComparison.TestDetails.Target;
                    if (!string.Equals(xmlParentControl.Name, xmlParentTest.Name, StringComparison.OrdinalIgnoreCase))
                        return ComparisonResult.DIFFERENT;
                    int intTestChildCount = xmlParentTest.ChildNodes.Count;
                    if (xmlParentControl.ChildNodes.Count != intTestChildCount)
                        return ComparisonResult.DIFFERENT;
                    List<XmlNode> lstXmlChildrenTest =
                        new List<XmlNode>(intTestChildCount);
                    foreach (XmlNode xmlChildTest in xmlParentTest.ChildNodes)
                        lstXmlChildrenTest.Add(xmlChildTest);
                    foreach (XmlNode xmlLoopChildControl in xmlParentControl.ChildNodes)
                    {
                        bool blnFoundMatch = false;
                        for (int i = 0; i < lstXmlChildrenTest.Count; ++i)
                        {
                            XmlNode xmlLoopChildTest = lstXmlChildrenTest[i];
                            Diff objLoopDiff = DiffBuilder
                                .Compare(xmlLoopChildControl)
                                .WithTest(xmlLoopChildTest)
                                .CheckForSimilar()
                                .WithNodeMatcher(objDiffNodeMatcher)
                                .IgnoreWhitespace()
                                .Build();
                            if (!objLoopDiff.HasDifferences())
                            {
                                blnFoundMatch = true;
                                lstXmlChildrenTest.RemoveAt(i);
                                break;
                            }
                        }

                        if (!blnFoundMatch)
                            return ComparisonResult.DIFFERENT;
                    }
                    // Because we already checked to make sure the number of children is the same, if every control child matches to a test child, we must be similar
                    return ComparisonResult.SIMILAR;
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

        [TestMethod, Timeout(5 * 60 * 1000, CooperativeCancellation = true)]
        public void LoadCharactersThenPrint()
        {
            try
            {
                List<string> lstExportLanguages = new List<string>(11);
                foreach (string strFilePath in Directory.EnumerateFiles(Utils.GetLanguageFolderPath, "*.xml"))
                {
                    string strExportLanguage = Path.GetFileNameWithoutExtension(strFilePath);
                    if (!strExportLanguage.Contains("data") && strExportLanguage != GlobalSettings.DefaultLanguage)
                        lstExportLanguages.Add(strExportLanguage);
                }

                lstExportLanguages.Sort();

                Debug.WriteLine("Started pre-loading language files");
                Debug.WriteLine("Pre-loading language file: " + GlobalSettings.DefaultLanguage);
                LanguageManager.LoadLanguage(GlobalSettings.DefaultLanguage, TestContext.CancellationToken);
                foreach (string strExportLanguage in lstExportLanguages)
                {
                    Debug.WriteLine("Pre-loading language file: " + strExportLanguage);
                    LanguageManager.LoadLanguage(strExportLanguage, TestContext.CancellationToken);
                }
                Debug.WriteLine("Finished pre-loading language files");
                int intLanguageIndex = 0;
                foreach (Character objCharacter in UnitTestUtils.GetTestCharacters(TestContext.CancellationToken))
                {
                    Debug.WriteLine("Checking " + (Path.GetFileName(objCharacter.FileName) ?? "Unknown"));
                    // Always try to export in English because this will cover most export code
                    DoAndSaveExport(objCharacter, GlobalSettings.DefaultLanguage, TestContext.CancellationToken);
                    // Rotate through languages instead of testing every one for every character to save on execution time
                    DoAndSaveExport(objCharacter, lstExportLanguages[intLanguageIndex++ % lstExportLanguages.Count], TestContext.CancellationToken);
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
