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
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public static frmChummerMain _mainForm;
        public static frmChummerMain MainForm
        {
            get
            {
                if (_mainForm == null)
                {
                    try
                    {
                        _mainForm = new frmChummerMain(true);
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                        Console.WriteLine(e);
                    }
                }
                Assert.IsNotNull(_mainForm);

                return _mainForm;
            }
        }

        [TestMethod]
        public void LoadThenSave()
        {
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");

            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            string strTestPath = Path.Combine(strPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", GlobalOptions.InvariantCultureInfo));
            DirectoryInfo objTestPath = Directory.CreateDirectory(strTestPath);
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            FileInfo[] aobjFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo objFileInfo in aobjFiles)
            {
                string destination = Path.Combine(objTestPath.FullName, objFileInfo.Name);
                Character c = LoadCharacter(objFileInfo);
                SaveCharacter(c, destination);
                c = new Character
                {
                    FileName = destination
                };
                Assert.IsTrue(c.Load().Result);
            }
            objTestPath.Delete(true);
        }

        /// <summary>
        /// Validate that a given list of Characters can be successfully loaded.
        /// </summary>
        public Character LoadCharacter(FileInfo objFileInfo)
        {
            Character c = new Character();
            try
            {
                Debug.WriteLine("Loading: " + objFileInfo.Name);
                Character objCharacter = new Character
                {
                    FileName = objFileInfo.FullName
                };
                Assert.IsTrue(objCharacter.Load().Result);
                Debug.WriteLine("Character loaded: " + c.Name);
                /*
                if (c.Created)
                {
                    frmCareer _ = new frmCareer(c);
                    //SINnersUsercontrol sINnersUsercontrol = new SINnersUsercontrol(career);
                    //sINnersUsercontrol.UploadSINnerAsync();
                }
                else
                {
                    frmCreate _ = new frmCreate(c);
                }
                Debug.WriteLine("Test Form Created: " + c.Name);
                */
            }
            catch (AssertFailedException e)
            {
                string strErrorMessage = "Could not load " + objFileInfo.FullName + "!";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
            }
            catch (Exception e)
            {
                string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ":";
                strErrorMessage += Environment.NewLine + e;
                Debug.WriteLine(strErrorMessage);
                Console.WriteLine(strErrorMessage);
                throw;
            }

            return c;
        }

        /// <summary>
        /// Tests saving a given character.
        /// </summary>
        public void SaveCharacter(Character c, string path)
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
