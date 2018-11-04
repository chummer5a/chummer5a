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
        public void LoadCharacter()
        {
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
            string strPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestFiles");
            DirectoryInfo objPathInfo = new DirectoryInfo(strPath);//Assuming Test is your Folder
            FileInfo[] aobjFiles = objPathInfo.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo objFileInfo in aobjFiles)
            {
                try
                {
                    Debug.WriteLine("Loading: " + objFileInfo.Name);
                    Character objLoopCharacter = MainForm.LoadCharacter(objFileInfo.FullName);
                    Assert.IsNotNull(objLoopCharacter);
                    Debug.WriteLine("Character loaded: " + objLoopCharacter.Name);
                    if (objLoopCharacter.Created)
                    {
                        frmCareer _ = new frmCareer(objLoopCharacter);
                        //SINnersUsercontrol sINnersUsercontrol = new SINnersUsercontrol(career);
                        //sINnersUsercontrol.UploadSINnerAsync();
                    }
                    else
                    {
                        frmCreate _ = new frmCreate(objLoopCharacter);
                    }
                    Debug.WriteLine("Test Form Created: " + objLoopCharacter.Name);
                }
                catch (AssertFailedException e)
                {
                    string strErrorMessage = "Could not load " + objFileInfo.FullName + "!";
                    strErrorMessage += Environment.NewLine + e.ToString();
                    Debug.WriteLine(strErrorMessage);
                    Console.WriteLine(strErrorMessage);
                }
                catch (Exception e)
                {
                    string strErrorMessage = "Exception while loading " + objFileInfo.FullName + ":";
                    strErrorMessage += Environment.NewLine + e.ToString();
                    Debug.WriteLine(strErrorMessage);
                    Console.WriteLine(strErrorMessage);
                    throw;
                }
            }
        }
    }
}
