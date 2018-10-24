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
