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
    [TestClass]
    public static class AssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Utils.IsUnitTest = true;
            Utils.IsUnitTestForUI = false;
        }
    }

    [TestClass]
    public class ChummerTest
    {
        public ChummerTest()
        {
            Utils.CreateSynchronizationContext();
            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty, "TestFiles");
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            foreach (DirectoryInfo objOldDir in objPathInfo.GetDirectories("TestRun-*"))
            {
                Utils.SafeDeleteDirectory(objOldDir.FullName);
            }
            TestPath = Path.Combine(strPath, "TestRun-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalSettings.InvariantCultureInfo));
            TestPathInfo = Directory.CreateDirectory(TestPath);
            TestFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            _lstCharacters = new List<Character>(TestFiles.Length);
        }

        private string TestPath { get; }
        private DirectoryInfo TestPathInfo { get; }

        private FileInfo[] TestFiles { get; }

        private readonly List<Character> _lstCharacters;

        private IEnumerable<Character> GetTestCharacters()
        {
            foreach (FileInfo objFileInfo in TestFiles)
            {
                Debug.WriteLine("Loading " + objFileInfo.Name);
                string strFile = objFileInfo.FullName;
                Character objLoopCharacter = _lstCharacters.Find(x => x.FileName == strFile);
                if (objLoopCharacter == null)
                {
                    objLoopCharacter = LoadCharacter(objFileInfo);
                    _lstCharacters.Add(objLoopCharacter);
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
                    lstCachingTasks.Add(Task.Run(() => CacheCommonFile(strLoopFile)));
                    if (++intCounter != Utils.MaxParallelBatchSize)
                        continue;
                    Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));
                    lstCachingTasks.Clear();
                    intCounter = 0;
                }

                Utils.RunWithoutThreadLock(() => Task.WhenAll(lstCachingTasks));

                async Task CacheCommonFile(string strFile)
                {
                    // Load default language data first for performance reasons
                    if (!GlobalSettings.Language.Equals(
                            GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        await XmlManager.LoadXPathAsync(strFile, null, GlobalSettings.DefaultLanguage);
                    }

                    await XmlManager.LoadXPathAsync(strFile);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test02_BasicStartup()
        {
            Debug.WriteLine("Unit test initialized for: Test02_BasicStartup()");
            ChummerMainForm frmOldMainForm = Program.MainForm;
            ChummerMainForm frmTestForm = null;
            // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
            try
            {
                frmTestForm = new ChummerMainForm(true, true)
                {
                    ShowInTaskbar =
                        false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactve)
                };
                Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                frmTestForm.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
#if DEBUG
                frmTestForm.SendToBack();
#endif
                while
                    (!frmTestForm
                        .IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                {
                    Utils.SafeSleep();
                }

                frmTestForm.Close();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                frmTestForm?.Dispose();
            }
            Program.MainForm = frmOldMainForm;
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test03_LoadCharacters()
        {
            Debug.WriteLine("Unit test initialized for: Test03_LoadCharacters()");
            List<Character> lstCharacters = new List<Character>();
            foreach (Character objCharacter in GetTestCharacters())
            {
                Debug.WriteLine("Finished loading " + objCharacter.FileName);
                lstCharacters.Add(objCharacter);
            }

            lstCharacters.Capacity = lstCharacters.Count; // Dummy command to make sure the test isn't optimized away.
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test04_SaveAsChum5lzThenLoad()
        {
            Debug.WriteLine("Unit test initialized for: Test04_SaveAsChum5lzThenLoad()");
            LzmaHelper.ChummerCompressionPreset eOldSetting = GlobalSettings.Chum5lzCompressionLevel;
            try
            {
                GlobalSettings.Chum5lzCompressionLevel = LzmaHelper.ChummerCompressionPreset.Fast;
                foreach (Character objCharacter in GetTestCharacters())
                {
                    string strFileName = Path.GetFileName(objCharacter.FileName)
                                         ?? LanguageManager.GetString("String_Unknown");
                    Debug.WriteLine("Checking " + strFileName);
                    string strDestination = Path.Combine(TestPathInfo.FullName, "(Compressed) " + strFileName);
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
            finally
            {
                GlobalSettings.Chum5lzCompressionLevel = eOldSetting;
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test05_LoadThenSaveIsDeterministic()
        {
            Debug.WriteLine("Unit test initialized for: Test05_LoadThenSaveIsDeterministic()");
            // Two separate loops because it's slightly faster this way
            List<string> lstBaseFileNames = new List<string>();
            foreach (Character objCharacterControl in GetTestCharacters())
            {
                string strFileName = Path.GetFileName(objCharacterControl.FileName) ??
                                     LanguageManager.GetString("String_Unknown");
                Debug.WriteLine("Saving Control for " + strFileName);
                // First Load-Save cycle
                string strDestinationControl = Path.Combine(TestPathInfo.FullName, "(Control) " + strFileName);
                SaveCharacter(objCharacterControl, strDestinationControl);
                lstBaseFileNames.Add(strFileName);
            }

            foreach (string strFileName in lstBaseFileNames)
            {
                Debug.WriteLine("Checking " + strFileName);
                // Second Load-Save cycle
                string strDestinationControl = Path.Combine(TestPathInfo.FullName, "(Control) " + strFileName);
                string strDestinationTest = Path.Combine(TestPathInfo.FullName, "(Test) " + strFileName);
                using (Character objCharacterTest = LoadCharacter(new FileInfo(strDestinationControl)))
                {
                    SaveCharacter(objCharacterTest, strDestinationTest);
                }

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
                            .CheckForIdentical()
                            .WithNodeFilter(x =>
                                x.Name !=
                                "mugshot") // image loading and unloading is not going to be deterministic due to compression algorithms
                            .WithNodeMatcher(
                                new DefaultNodeMatcher(
                                    ElementSelectors.Or(
                                        ElementSelectors.ByNameAndText,
                                        ElementSelectors.ByName)))
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
        }

        [TestMethod]
        public void Test06_LoadThenPrint()
        {
            Debug.WriteLine("Unit test initialized for: Test06_LoadThenPrint()");
            List<string> lstExportLanguages = new List<string>();
            foreach (string strFilePath in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "lang"), "*.xml"))
            {
                string strExportLanguage = Path.GetFileNameWithoutExtension(strFilePath);
                if (strExportLanguage.Contains("data"))
                    continue;
                if (strExportLanguage == GlobalSettings.DefaultLanguage)
                    lstExportLanguages.Insert(0, strExportLanguage);
                else
                    lstExportLanguages.Add(strExportLanguage);
            }
            Debug.WriteLine("Started pre-loading language files");
            foreach (string strExportLanguage in lstExportLanguages)
            {
                Debug.WriteLine("Pre-loading language file: " + strExportLanguage);
                LanguageManager.LoadLanguage(strExportLanguage);
            }
            Debug.WriteLine("Finished pre-loading language files");
            foreach (Character objCharacter in GetTestCharacters())
            {
                Debug.WriteLine("Checking " + (Path.GetFileName(objCharacter.FileName)
                                               ?? LanguageManager.GetString("String_Unknown")));
                foreach (string strExportLanguage in lstExportLanguages)
                {
                    DoAndSaveExport(objCharacter, strExportLanguage);
                }
            }
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
                    string strDummyFileName = Path.Combine(TestPathInfo.FullName,
                                                           "(UnitTest05Dummy) "
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
                                        (!blnFormClosed) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
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
            }
            Program.MainForm = frmOldMainForm;
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        private static Character LoadCharacter(FileInfo objFileInfo)
        {
            Character objCharacter = null;
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                objCharacter = new Character
                {
                    FileName = objFileInfo.FullName
                };
                bool blnSuccess = objCharacter.Load();
                Assert.IsTrue(blnSuccess);
                Debug.WriteLine("Character loaded: " + objCharacter.Name + ", " + objFileInfo.Name);
            }
            catch (AssertFailedException e)
            {
                if (objCharacter != null)
                {
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
        private static void SaveCharacter(Character objCharacter, string strPath)
        {
            Assert.IsNotNull(objCharacter);
            try
            {
                Debug.WriteLine("Saving: " + objCharacter.Name + ", " + Path.GetFileName(strPath));
                objCharacter.Save(strPath, false);
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
        private void DoAndSaveExport(Character objCharacter, string strExportLanguage)
        {
            Assert.IsNotNull(objCharacter);
            string strPath = Path.Combine(TestPathInfo.FullName, strExportLanguage + ' ' + Path.GetFileNameWithoutExtension(objCharacter.FileName) + ".xml");
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
