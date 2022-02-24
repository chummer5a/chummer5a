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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
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
        }
    }

    [TestClass]
    public class ChummerTest
    {
        public ChummerTest()
        {
            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            foreach (DirectoryInfo objOldDir in objPathInfo.GetDirectories("TestRun-*"))
            {
                Directory.Delete(objOldDir.FullName, true);
            }
            TestPath = Path.Combine(strPath, "TestRun-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalSettings.InvariantCultureInfo));
            TestPathInfo = Directory.CreateDirectory(TestPath);
            TestFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
        }

        private string TestPath { get; }
        private DirectoryInfo TestPathInfo { get; }

        private FileInfo[] TestFiles { get; }

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
        public void Test01_BasicStartup()
        {
            Debug.WriteLine("Unit test initialized for: Test01_BasicStartup()");
            ChummerMainForm frmOldMainForm = Program.MainForm;
            ChummerMainForm frmTestForm = null;
            // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
            try
            {
                frmTestForm = new ChummerMainForm(true)
                {
                    WindowState = FormWindowState.Minimized,
                    ShowInTaskbar =
                        false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactve)
                };
                Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                frmTestForm.Show(); // Show the main form so that we know the UI can load in properly
                while
                    (!frmTestForm
                        .IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                {
                    Utils.SafeSleep(true);
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
        public void Test02_LoadThenSave()
        {
            Debug.WriteLine("Unit test initialized for: Test02_LoadThenSave()");
            foreach (FileInfo objFileInfo in TestFiles)
            {
                string strDestination = Path.Combine(TestPathInfo.FullName, objFileInfo.Name);
                using (Character objCharacter = LoadCharacter(objFileInfo))
                {
                    SaveCharacter(objCharacter, strDestination);
                    using (Character _ = LoadCharacter(new FileInfo(strDestination)))
                    {
                        // Assert on failed load will already happen inside LoadCharacter
                    }
                }
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test03_LoadThenSaveIsDeterministic()
        {
            Debug.WriteLine("Unit test initialized for: Test03_LoadThenSaveIsDeterministic()");
            foreach (FileInfo objBaseFileInfo in TestFiles)
            {
                // First Load-Save cycle
                string strDestinationControl = Path.Combine(TestPathInfo.FullName, "(Control) " + objBaseFileInfo.Name);
                using (Character objCharacterControl = LoadCharacter(objBaseFileInfo))
                {
                    SaveCharacter(objCharacterControl, strDestinationControl);
                    // Second Load-Save cycle
                    string strDestinationTest = Path.Combine(TestPathInfo.FullName, "(Test) " + objBaseFileInfo.Name);
                    using (Character objCharacterTest = LoadCharacter(new FileInfo(strDestinationControl)))
                    {
                        SaveCharacter(objCharacterTest, strDestinationTest);
                        // Check to see that character after first load cycle is consistent with character after second
                        using (FileStream controlFileStream =
                            File.Open(strDestinationControl, FileMode.Open, FileAccess.Read))
                        {
                            using (FileStream testFileStream =
                                File.Open(strDestinationTest, FileMode.Open, FileAccess.Read))
                            {
                                try
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
                                catch (XmlSchemaException e)
                                {
                                    Assert.Fail("Unexpected validation failure: " + e.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test04_LoadThenPrint()
        {
            Debug.WriteLine("Unit test initialized for: Test04_LoadThenPrint()");
            foreach (FileInfo objFileInfo in TestFiles)
            {
                using (Character objCharacter = LoadCharacter(objFileInfo))
                {
                    string strLanguageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
                    foreach (string strFilePath in Directory.GetFiles(strLanguageDirectoryPath, "*.xml"))
                    {
                        string strExportLanguage = Path.GetFileNameWithoutExtension(strFilePath);
                        if (strExportLanguage.Contains("data"))
                            continue;
                        CultureInfo objExportCultureInfo = new CultureInfo(strExportLanguage);
                        string strDestination = Path.Combine(TestPathInfo.FullName, strExportLanguage + ' ' + Path.GetFileNameWithoutExtension(objFileInfo.Name) + ".xml");
                        // ReSharper disable once AccessToDisposedClosure
                        XmlDocument xmlCharacter = Utils.RunWithoutThreadLock(() => objCharacter.GenerateExportXml(objExportCultureInfo, strExportLanguage));
                        xmlCharacter.Save(strDestination);
                    }
                }
            }
        }

        // Test methods have a number in their name so that by default they execute in the order of fastest to slowest
        [TestMethod]
        public void Test05_LoadCharacterForms()
        {
            Debug.WriteLine("Unit test initialized for: Test05_LoadCharacterForms()");
            ChummerMainForm frmOldMainForm = Program.MainForm;
            ChummerMainForm frmTestForm = null;
            // Try-finally pattern necessary in order prevent weird exceptions from disposal of MdiChildren
            try
            {
                frmTestForm = new ChummerMainForm(true)
                {
                    WindowState = FormWindowState.Minimized,
                    ShowInTaskbar =
                        false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used, but that forces the test to be interactive)
                };
                Program.MainForm = frmTestForm; // Set program Main form to Unit test version
                frmTestForm.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would actually display it).
                while (!frmTestForm.IsFinishedLoading) // Hacky, but necessary to get xUnit to play nice because it can't deal well with the dreaded WinForms + async combo
                {
                    Utils.SafeSleep(true);
                }
                foreach (FileInfo objFileInfo in TestFiles)
                {
                    using (Character objCharacter = LoadCharacter(objFileInfo))
                    {
                        try
                        {
                            using (CharacterShared frmCharacterForm = objCharacter.Created
                                ? (CharacterShared)new CharacterCareer(objCharacter)
                                : new CharacterCreate(objCharacter))
                            {
                                frmCharacterForm.MdiParent = frmTestForm;
                                frmCharacterForm.ShowInTaskbar = false;
                                frmCharacterForm.WindowState = FormWindowState.Minimized;
                                frmCharacterForm.Show();
                            }
                        }
                        catch (Exception e)
                        {
                            string strErrorMessage = "Exception while loading form for " + objFileInfo.FullName + ":";
                            strErrorMessage += Environment.NewLine + e;
                            Debug.WriteLine(strErrorMessage);
                            Console.WriteLine(strErrorMessage);
                            Assert.Fail(strErrorMessage);
                        }
                    }
                }
                frmTestForm.Close();
            }
            finally
            {
                frmTestForm?.Dispose();
            }
            Program.MainForm = frmOldMainForm;
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        private static Character LoadCharacter(FileInfo objFileInfo)
        {
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
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
                Debug.WriteLine("Character loaded: " + objCharacter.Name);
            }
            catch (AssertFailedException e)
            {
                objCharacter?.Dispose();
                objCharacter = null;
                string strErrorMessage = "Could not load " + objFileInfo.FullName + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (Exception e)
            {
                objCharacter?.Dispose();
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ":";
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
        private static void SaveCharacter(Character c, string path)
        {
            Debug.WriteLine("Unit test initialized for: SaveCharacter()");
            Assert.IsNotNull(c);
            try
            {
                c.Save(path, false);
            }
            catch (AssertFailedException e)
            {
                string strErrorMessage = "Could not load " + c.FileName + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (InvalidOperationException e)
            {
                string strErrorMessage = "Could not save to " + path + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
            catch (Exception e)
            {
                string strErrorMessage = "Exception while loading " + c.FileName + ":";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                Assert.Fail(strErrorMessage);
            }
        }
    }
}
