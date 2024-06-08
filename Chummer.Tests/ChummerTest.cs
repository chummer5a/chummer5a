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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;

namespace Chummer.Tests
{
    public static class CommonTestData
    {
        static CommonTestData()
        {
            TestFilesBasePathInfo = new DirectoryInfo(TestFilesBasePath);//Assuming Test is your Folder
            foreach (DirectoryInfo objOldDir in TestFilesBasePathInfo.GetDirectories("TestRun-*"))
                Directory.Delete(objOldDir.FullName, true);
            TestPathInfo = Directory.CreateDirectory(Path.Combine(TestFilesBasePath,
                "TestRun-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalSettings.InvariantCultureInfo)));
            TestFileInfos = TestFilesBasePathInfo.GetFiles("*.chum5"); //Getting Text files
            Characters = new Character[TestFileInfos.Length];
        }

        private static string TestFilesBasePath { get; } =
            Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty, "TestFiles");

        private static DirectoryInfo TestFilesBasePathInfo { get; }

        public static DirectoryInfo TestPathInfo { get; }

        public static FileInfo[] TestFileInfos { get; }

        public static Character[] Characters { get; }
    }

    [TestClass]
    public class ChummerTest
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Utils.IsUnitTest = true;
            Utils.IsUnitTestForUI = false;
            Utils.CreateSynchronizationContext();
        }

        private static IEnumerable<Character> GetTestCharacters()
        {
            for (int i = 0; i < CommonTestData.TestFileInfos.Length; ++i)
            {
#if DEBUG
                FileInfo objFileInfo = CommonTestData.TestFileInfos[i];
                Debug.WriteLine("Loading " + objFileInfo.Name);
#endif
                Character objLoopCharacter = CommonTestData.Characters[i];
                if (objLoopCharacter == null)
                {
#if DEBUG
                    objLoopCharacter = LoadCharacter(objFileInfo);
#else
                    objLoopCharacter = LoadCharacter(CommonTestData.TestFileInfos[i]);
#endif
                    CommonTestData.Characters[i] = objLoopCharacter;
                }

                yield return objLoopCharacter;
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test00_ColorTest()
        {
            Debug.WriteLine("Unit test initialized for: Test00_ColorTest()");
            Color objColorLightGrayInDarkMode = ColorManager.GenerateDarkModeColor(Color.LightGray);
            float fltLightGrayLightness = Color.LightGray.GetBrightness();
            float fltLightGrayDarkModeLightness = objColorLightGrayInDarkMode.GetBrightness();
            Assert.IsTrue(fltLightGrayDarkModeLightness < fltLightGrayLightness);

            Color objColorBlackInvert = ColorManager.GenerateInverseDarkModeColor(Color.Black);
            Color objColorBlackInvertDark = ColorManager.GenerateDarkModeColor(objColorBlackInvert);
            float fltBlackHue = Color.Black.GetHue();
            float fltBlackInvertDarkHue = objColorBlackInvertDark.GetHue();
            Assert.IsTrue(Math.Abs(fltBlackInvertDarkHue - fltBlackHue) < 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
            Color objColorBlackInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorBlackInvertDark);
            Color objColorBlackInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorBlackInvertDarkInvert);
            Assert.IsTrue(objColorBlackInvertDark == objColorBlackInvertDarkInvertDark);

            Color objColorWhiteInvert = ColorManager.GenerateInverseDarkModeColor(Color.White);
            Color objColorWhiteInvertDark = ColorManager.GenerateDarkModeColor(objColorWhiteInvert);
            float fltWhiteHue = Color.White.GetHue();
            float fltWhiteInvertDarkHue = objColorWhiteInvertDark.GetHue();
            Assert.IsTrue(Math.Abs(fltWhiteInvertDarkHue - fltWhiteHue) < 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
            Color objColorWhiteInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorWhiteInvertDark);
            Color objColorWhiteInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorWhiteInvertDarkInvert);
            Assert.IsTrue(objColorWhiteInvertDark == objColorWhiteInvertDarkInvertDark);

            Color objColorRedInvert = ColorManager.GenerateInverseDarkModeColor(Color.Red);
            Color objColorRedInvertDark = ColorManager.GenerateDarkModeColor(objColorRedInvert);
            float fltRedHue = Color.Red.GetHue();
            float fltRedInvertDarkHue = objColorRedInvertDark.GetHue();
            Assert.IsTrue(Math.Abs(fltRedInvertDarkHue - fltRedHue) < 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
            Color objColorRedInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorRedInvertDark);
            Color objColorRedInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorRedInvertDarkInvert);
            Assert.IsTrue(objColorRedInvertDark == objColorRedInvertDarkInvertDark);

            Color objColorChocolateInvert = ColorManager.GenerateInverseDarkModeColor(Color.Chocolate);
            Color objColorChocolateInvertDark = ColorManager.GenerateDarkModeColor(objColorChocolateInvert);
            float fltChocolateHue = Color.Chocolate.GetHue();
            float fltChocolateInvertDarkHue = objColorChocolateInvertDark.GetHue();
            Assert.IsTrue(Math.Abs(fltChocolateInvertDarkHue - fltChocolateHue) < 0.1f / 360.0f); // Only care if we're off by more than 0.1 degrees
            Color objColorChocolateInvertDarkInvert = ColorManager.GenerateInverseDarkModeColor(objColorChocolateInvertDark);
            Color objColorChocolateInvertDarkInvertDark = ColorManager.GenerateDarkModeColor(objColorChocolateInvertDarkInvert);
            Assert.IsTrue(objColorChocolateInvertDark == objColorChocolateInvertDarkInvertDark);
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test01_LoadContent()
        {
            Debug.WriteLine("Unit test initialized for: Test01_LoadContent()");
            try
            {
                // Attempt to cache all XML files that are used the most.
                List<Task> lstCachingTasks = new List<Task>(Utils.MaxParallelBatchSize);
                int intCounter = 0;
                foreach (string strLoopFile in Utils.BasicDataFileNames)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    lstCachingTasks.Add(Task.Run(() => CacheCommonFileAsync(strLoopFile)));
                    if (++intCounter != Utils.MaxParallelBatchSize)
                        continue;
                    Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));
                    lstCachingTasks.Clear();
                    intCounter = 0;
                }

                Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));

                async Task CacheCommonFileAsync(string strFile)
                {
                    // Load default language data first for performance reasons
                    if (!GlobalSettings.Language.Equals(
                            GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage).ConfigureAwait(false);
                    }

                    await XmlManager.LoadXPathAsync(strFile).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test02_LoadCharacters()
        {
            Debug.WriteLine("Unit test initialized for: Test03_LoadCharacters()");
            List<Character> lstCharacters = new List<Character>(CommonTestData.TestFileInfos.Length);
            foreach (Character objCharacter in GetTestCharacters())
            {
                Debug.WriteLine("Finished loading " + objCharacter.FileName);
                lstCharacters.Add(objCharacter);
            }

            lstCharacters.Capacity = lstCharacters.Count; // Dummy command to make sure the test isn't optimized away.
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test03_SaveAsChum5lz()
        {
            Debug.WriteLine("Unit test initialized for: Test04_SaveAsChum5lz()");
            foreach (Character objCharacter in GetTestCharacters())
            {
                string strFileName = Path.GetFileName(objCharacter.FileName)
                                     ?? LanguageManager.GetString("String_Unknown");
                Debug.WriteLine("Checking " + strFileName);
                string strDestination =
                    Path.Combine(CommonTestData.TestPathInfo.FullName, "(Compressed) " + strFileName);
                if (!strDestination.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase))
                {
                    if (strDestination.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                        strDestination += "lz";
                    else
                        strDestination += ".chum5lz";
                }

                SaveCharacter(objCharacter, strDestination);
                // If our compression is malformed, we should run into a parse error when we try to load the XML data (don't load the full character because it's unnecessary)
                XmlDocument objXmlDocument = new XmlDocument { XmlResolver = null };
                objXmlDocument.LoadStandardFromLzmaCompressed(strDestination);
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test04_LoadThenSaveIsDeterministic()
        {
            Debug.WriteLine("Unit test initialized for: Test05_LoadThenSaveIsDeterministic()");
            DefaultNodeMatcher objDiffNodeMatcher = new DefaultNodeMatcher(ElementSelectors.ByNameAndText);
            foreach (Character objCharacterControl in GetTestCharacters())
            {
                string strFileName = Path.GetFileName(objCharacterControl.FileName) ??
                                     LanguageManager.GetString("String_Unknown");
                Debug.WriteLine("Saving Control for " + strFileName);
                // First Load-Save cycle
                string strDestinationControl = Path.Combine(CommonTestData.TestPathInfo.FullName, "(Control) " + strFileName);
                SaveCharacter(objCharacterControl, strDestinationControl);
                Debug.WriteLine("Checking " + strFileName);
                // Second Load-Save cycle
                string strDestinationTest = Path.Combine(CommonTestData.TestPathInfo.FullName, "(Test) " + strFileName);
                Character objCharacterTest = LoadCharacter(new FileInfo(strDestinationControl)); // No using here because we value execution time much more than memory usage
                SaveCharacter(objCharacterTest, strDestinationTest);

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
                    Assert.Fail("Unexpected validation failure: " + e.Message);
                }
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

        [TestMethod]
        public void Test05_LoadThenPrint()
        {
            Debug.WriteLine("Unit test initialized for: Test06_LoadThenPrint()");
            List<string> lstExportLanguages = new List<string>();
            foreach (string strFilePath in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "lang"), "*.xml"))
            {
                string strExportLanguage = Path.GetFileNameWithoutExtension(strFilePath);
                if (!strExportLanguage.Contains("data") && strExportLanguage != GlobalSettings.DefaultLanguage)
                    lstExportLanguages.Add(strExportLanguage);
            }

            lstExportLanguages.Sort();

            Debug.WriteLine("Started pre-loading language files");
            Debug.WriteLine("Pre-loading language file: " + GlobalSettings.DefaultLanguage);
            LanguageManager.LoadLanguage(GlobalSettings.DefaultLanguage);
            foreach (string strExportLanguage in lstExportLanguages)
            {
                Debug.WriteLine("Pre-loading language file: " + strExportLanguage);
                LanguageManager.LoadLanguage(strExportLanguage);
            }
            Debug.WriteLine("Finished pre-loading language files");
            int intLanguageIndex = 0;
            foreach (Character objCharacter in GetTestCharacters())
            {
                Debug.WriteLine("Checking " + (Path.GetFileName(objCharacter.FileName)
                                               ?? LanguageManager.GetString("String_Unknown")));
                // Always try to export in English because this will cover most export code
                DoAndSaveExport(objCharacter, GlobalSettings.DefaultLanguage);
                // Rotate through languages instead of testing every one for every character to save on execution time
                DoAndSaveExport(objCharacter, lstExportLanguages[intLanguageIndex++ % lstExportLanguages.Count]);
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test06_BasicStartup()
        {
            Debug.WriteLine("Unit test initialized for: Test02_BasicStartup()");
            ChummerMainForm frmOldMainForm = Program.MainForm;
            ChummerMainForm frmTestForm = null;
            // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
            try
            {
                Utils.IsUnitTestForUI = true;
                frmTestForm = frmOldMainForm.DoThreadSafeFunc(() => new ChummerMainForm(true, true)
                {
                    ShowInTaskbar =
                        false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactve)
                });
                Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                frmTestForm.DoThreadSafe(x =>
                {
                    x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                    x.SendToBack();
#endif
                });
                while
                    (!frmTestForm
                        .IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                {
                    Utils.SafeSleep();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                try
                {
                    frmTestForm?.DoThreadSafe(x => x.Close());
                }
                catch (Exception e)
                {
                    string strErrorMessage = "Encountered (non-fatal) exception while disposing of main form." + Environment.NewLine
                        + e.Message;
                    Debug.WriteLine(strErrorMessage);
                    Console.WriteLine(strErrorMessage);
                    Utils.BreakIfDebug();
                }
                Utils.IsUnitTestForUI = false;
            }
            Program.MainForm = frmOldMainForm;
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test07_LoadCharacterForms()
        {
            Debug.WriteLine("Unit test initialized for: Test07_LoadCharacterForms()");
            ChummerMainForm frmOldMainForm = Program.MainForm;
            ChummerMainForm frmTestForm = null;
            // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
            try
            {
                Utils.IsUnitTestForUI = true;
                frmTestForm = frmOldMainForm.DoThreadSafeFunc(() => new ChummerMainForm(true, true)
                {
                    ShowInTaskbar =
                        false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactive)
                });
                Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                frmTestForm.DoThreadSafe(x =>
                {
                    x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                    x.SendToBack();
#endif
                });
                while (!frmTestForm.IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                {
                    Utils.SafeSleep();
                }
                Debug.WriteLine("Main form loaded");
                foreach (Character objCharacter in GetTestCharacters())
                {
                    string strFileName = Path.GetFileName(objCharacter.FileName) ?? LanguageManager.GetString("String_Unknown");
                    Debug.WriteLine("Checking " + strFileName);
                    string strDummyFileName = Path.Combine(CommonTestData.TestPathInfo.FullName,
                                                           "(UnitTest07Dummy) "
                                                           + Path.GetFileNameWithoutExtension(objCharacter.FileName)
                                                           + ".txt");
                    using (File.Create(strDummyFileName, byte.MaxValue,
                                       FileOptions
                                           .DeleteOnClose)) // Create this so that we can track how far along the Unit Test is even if we don't have a debugger attached
                    {
                        try
                        {
                            bool blnFormClosed = false;
                            // ReSharper disable once AccessToDisposedClosure
                            CharacterShared frmCharacterForm = Program.MainForm.DoThreadSafeFunc(
                                () => objCharacter.Created
                                    // ReSharper disable once AccessToDisposedClosure
                                    ? (CharacterShared) new CharacterCareer(objCharacter)
                                    // ReSharper disable once AccessToDisposedClosure
                                    : new CharacterCreate(objCharacter));
                            try
                            {
                                frmCharacterForm.DoThreadSafe(x =>
                                {
                                    x.FormClosed += (sender, args) => blnFormClosed = true;
                                    x.MdiParent = frmTestForm;
                                    x.ShowInTaskbar = false;
                                    x.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                                    x.SendToBack();
#endif
                                });
                                while
                                    (!frmCharacterForm
                                        .IsFinishedInitializing) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                                {
                                    Utils.SafeSleep();
                                }
                            }
                            finally
                            {
                                try
                                {
                                    frmCharacterForm.DoThreadSafe(x => x.Close());
                                    while
                                        (!blnFormClosed && !frmCharacterForm.IsDisposed) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                                    {
                                        Utils.SafeSleep();
                                    }
                                }
                                catch (ApplicationException e)
                                {
                                    string strErrorMessage
                                        = "Encountered (non-fatal) exception while disposing of character form."
                                          + Environment.NewLine
                                          + e.Message;
                                    Debug.WriteLine(strErrorMessage);
                                    Console.WriteLine(strErrorMessage);
                                    Program.OpenCharacters.Remove(objCharacter);
                                }
                                catch (InvalidOperationException e)
                                {
                                    string strErrorMessage
                                        = "Encountered (non-fatal) exception while disposing of character form."
                                          + Environment.NewLine
                                          + e.Message;
                                    Debug.WriteLine(strErrorMessage);
                                    Console.WriteLine(strErrorMessage);
                                    Program.OpenCharacters.Remove(objCharacter);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            string strErrorMessage
                                = "Exception while loading form for " + strFileName + ":";
                            strErrorMessage += Environment.NewLine + e;
                            Debug.WriteLine(strErrorMessage);
                            Console.WriteLine(strErrorMessage);
                            Assert.Fail(strErrorMessage);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    frmTestForm?.DoThreadSafe(x => x.Close());
                }
                catch (Exception e)
                {
                    string strErrorMessage = "Encountered (non-fatal) exception while disposing of main form." + Environment.NewLine
                        + e.Message;
                    Debug.WriteLine(strErrorMessage);
                    Console.WriteLine(strErrorMessage);
                    Utils.BreakIfDebug();
                }
                Utils.IsUnitTestForUI = false;
            }
            Program.MainForm = frmOldMainForm;
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        private static Character LoadCharacter(FileInfo objFileInfo, Character objExistingCharacter = null)
        {
            Character objCharacter = objExistingCharacter;
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                if (objExistingCharacter != null)
                    objCharacter.ResetCharacter();
                else
                    objCharacter = new Character();
                objCharacter.FileName = objFileInfo.FullName;

                bool blnSuccess = objCharacter.Load();
                Assert.IsTrue(blnSuccess);
                Debug.WriteLine("Character loaded: " + objCharacter.Name + ", " + objFileInfo.Name);
            }
            catch (AssertFailedException e)
            {
                if (objCharacter != null)
                {
                    if (objExistingCharacter == null)
                        objCharacter.Dispose();
                    objCharacter = null;
                }
                string strErrorMessage = "Could not load " + objFileInfo.FullName + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (Exception e)
            {
                if (objCharacter != null)
                {
                    if (objExistingCharacter == null)
                        objCharacter.Dispose();
                    objCharacter = null;
                }
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }

            return objCharacter;
        }

        /// <summary>
        /// Tests saving a given character.
        /// </summary>
        private static void SaveCharacter(Character objCharacter, string strPath, LzmaHelper.ChummerCompressionPreset eCompressionForChum5Lz = LzmaHelper.ChummerCompressionPreset.Fast)
        {
            Assert.IsNotNull(objCharacter);
            try
            {
                Debug.WriteLine("Saving: " + objCharacter.Name + ", " + Path.GetFileName(strPath));
                objCharacter.Save(strPath, false, false, eCompressionForChum5Lz);
                Debug.WriteLine("Character saved: " + objCharacter.Name + " to " + Path.GetFileName(strPath));
            }
            catch (AssertFailedException e)
            {
                string strErrorMessage = "Could not save " + Path.GetFileName(strPath) + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (InvalidOperationException e)
            {
                string strErrorMessage = "Could not save to " + strPath + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (Exception e)
            {
                string strErrorMessage = "Exception while saving " + Path.GetFileName(strPath) + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
        }

        /// <summary>
        /// Tests exporting a given character.
        /// </summary>
        private static void DoAndSaveExport(Character objCharacter, string strExportLanguage)
        {
            Assert.IsNotNull(objCharacter);
            string strPath = Path.Combine(CommonTestData.TestPathInfo.FullName, strExportLanguage + ' ' + Path.GetFileNameWithoutExtension(objCharacter.FileName) + ".xml");
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
                        () => objCharacter.GenerateExportXml(objExportCultureInfo,
                                                             strExportLanguage)); // Need this wrapper to make unit test work
                using (FileStream objFileStream
                       = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    xmlDocument.Save(objFileStream);
                Debug.WriteLine("Character exported: " + objCharacter.Name + " to " + Path.GetFileName(strPath));
            }
            catch (AssertFailedException e)
            {
                string strErrorMessage = "Could not export " + Path.GetFileName(objCharacter.FileName) + " in "
                                         + strExportLanguage + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (InvalidOperationException e)
            {
                string strErrorMessage = "Could not export to " + strPath + '!';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (Exception e)
            {
                string strErrorMessage = "Exception while exporting " + Path.GetFileName(objCharacter.FileName) + " in "
                                         + strExportLanguage + ':';
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
        }
    }
}
