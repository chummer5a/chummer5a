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
using System.Windows.Forms;
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
        private static frmChummerMain _frmMainForm;
        public static frmChummerMain MainForm
        {
            get
            {
                if (_frmMainForm == null)
                {
                    try
                    {
                        _frmMainForm = new frmChummerMain(true)
                        {
                            WindowState = FormWindowState.Minimized,
                            ShowInTaskbar = false // This lets the form be "shown" in unit tests (to actually have it show, ShowDialog() needs to be used)
                        };
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                        Console.WriteLine(e);
                    }
                }
                Assert.IsNotNull(_frmMainForm);
                return _frmMainForm;
            }
        }

        [TestMethod]
        public void LoadThenSave()
        {
            Debug.WriteLine("Unit test initialized for: LoadThenSave()");

            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            string strTestPath = Path.Combine(strPath, nameof(LoadThenSave) + '-' + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalOptions.InvariantCultureInfo));
            DirectoryInfo objTestPath = Directory.CreateDirectory(strTestPath);
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            FileInfo[] aobjFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo objFileInfo in aobjFiles)
            {
                string strDestination = Path.Combine(objTestPath.FullName, objFileInfo.Name);
                using (Character objCharacter = LoadCharacter(objFileInfo))
                    SaveCharacter(objCharacter, strDestination);
                using (Character _ = LoadCharacter(new FileInfo(strDestination)))
                { // Assert on failed load will already happen inside LoadCharacter
                }
            }
            objTestPath.Delete(true);
        }

        [TestMethod]
        public void LoadThenSaveIsDeterministic()
        {
            Debug.WriteLine("Unit test initialized for: LoadThenSaveIsDeterministic()");

            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            string strTestPath = Path.Combine(strPath, nameof(LoadThenSaveIsDeterministic) + '-' + DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalOptions.InvariantCultureInfo));
            DirectoryInfo objTestPath = Directory.CreateDirectory(strTestPath);
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            FileInfo[] aobjFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo objBaseFileInfo in aobjFiles)
            {
                // First Load-Save cycle
                string strDestinationControl = Path.Combine(objTestPath.FullName, "(Control) " + objBaseFileInfo.Name);
                using (Character objCharacter = LoadCharacter(objBaseFileInfo))
                    SaveCharacter(objCharacter, strDestinationControl);
                // Second Load-Save cycle
                string strDestinationTest = Path.Combine(objTestPath.FullName, "(Test) " + objBaseFileInfo.Name);
                using (Character objCharacter = LoadCharacter(new FileInfo(strDestinationControl)))
                    SaveCharacter(objCharacter, strDestinationTest);
                // Check to see that character after first load cycle is consistent with character after second
                using (FileStream controlFileStream = File.Open(strDestinationControl, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream testFileStream = File.Open(strDestinationTest, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            Diff myDiff = DiffBuilder
                                .Compare(controlFileStream)
                                .WithTest(testFileStream)
                                .CheckForSimilar()
                                .WithNodeMatcher(new DefaultNodeMatcher(ElementSelectors.Or(ElementSelectors.ByNameAndText,
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
            objTestPath.Delete(true);
        }

        [TestMethod]
        public void LoadCharacterForms()
        {
            Debug.WriteLine("Unit test initialized for: LoadCharacterForms()");
            frmChummerMain frmOldMainForm = Program.MainForm;
            Program.MainForm = MainForm; // Set program Main form to Unit test version
            MainForm.Show(); // We don't actually want to display the main form, so Show() is used (ShowDialog() would acutally display it).
            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            FileInfo[] aobjFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo objFileInfo in aobjFiles)
            {
                using (Character objCharacter = LoadCharacter(objFileInfo))
                {
                    try
                    {
                        using (CharacterShared frmCharacterForm = objCharacter.Created ? (CharacterShared) new frmCareer(objCharacter) : new frmCreate(objCharacter))
                        {
                            frmCharacterForm.MdiParent = MainForm;
                            frmCharacterForm.WindowState = FormWindowState.Minimized;
                            frmCharacterForm.Show();
                            frmCharacterForm.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        string strErrorMessage = "Exception while loading form for " + objFileInfo.FullName + ":";
                        strErrorMessage += Environment.NewLine + e;
                        Debug.WriteLine(strErrorMessage);
                        Console.WriteLine(strErrorMessage);
                        throw;
                    }
                }
            }
            MainForm.Close();
            Program.MainForm = frmOldMainForm;
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        private Character LoadCharacter(FileInfo objFileInfo)
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
                Assert.IsTrue(objCharacter.Load().Result);
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
            }
            catch (Exception e)
            {
                objCharacter?.Dispose();
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ":";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                throw;
            }

            return objCharacter;
        }

        /// <summary>
        /// Tests saving a given character.
        /// </summary>
        private void SaveCharacter(Character c, string path)
        {
            Debug.WriteLine("Unit test initialized for: SaveCharacter()");
            Assert.IsNotNull(c);
            try
            {
                c.Save(path);
            }
            catch (AssertFailedException e)
            {
                string strErrorMessage = "Could not load " + c.FileName + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
            }
            catch (InvalidOperationException e)
            {
                string strErrorMessage = "Could not save to " + path + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
            }
            catch (Exception e)
            {
                string strErrorMessage = "Exception while loading " + c.FileName + ":";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                throw;
            }
        }
    }
}
