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
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            path = System.IO.Path.Combine(path, "TestFiles");
            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                try
                {
                    Debug.WriteLine("Loading: " + file.Name);
                    Character c = MainForm.LoadCharacter(file.FullName);
                    Assert.IsNotNull(c);
                    Debug.WriteLine("Character loaded: " + c.Name);
                    if (c.Created)
                    {
                        frmCareer career = new frmCareer(c);
                        //SINnersUsercontrol sINnersUsercontrol = new SINnersUsercontrol(career);
                        //sINnersUsercontrol.UploadSINnerAsync();
                    }
                }
                catch (AssertFailedException e)
                {
                    string msg = "Could not load " + file.FullName + "!";
                    msg += Environment.NewLine + e.ToString();
                    Debug.WriteLine(msg);
                    Console.WriteLine(msg);
                }
                catch (Exception e)
                {
                    string msg = "Exception while loading " + file.FullName + ":";
                    msg += Environment.NewLine + e.ToString();
                    Debug.WriteLine(msg);
                    Console.WriteLine(msg);
                    throw;
                }
            }
        }
    }
}
