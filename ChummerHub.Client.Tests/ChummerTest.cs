using System;
using System.Diagnostics;
using System.IO;
using Chummer;
using ChummerHub.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChummerHub.Client.Tests
{
    [TestClass]
    //[DeploymentItem("..\\..\\Chummer\\settings", "settings")]
    public class ChummerTest
    {
        //public static SINnersUsercontrol MySINnersUsercontrol = new SINnersUsercontrol();

        public static frmChummerMain MainForm = new frmChummerMain();

        [TestMethod]
        public void LoadCharacter()
        {
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            path = System.IO.Path.Combine(path, "data");
            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                try
                {
                    Debug.WriteLine("Loading: " + file.Name);
                    Character c = MainForm.LoadCharacter(file.FullName);
                    if (c == null)
                        continue;
                    Debug.WriteLine("Character loaded: " + c.Name);
                    if (c.Created)
                    {
                        frmCareer career = new frmCareer(c);
                        SINnersUserControl sINnersUsercontrol = new SINnersUserControl();
                        sINnersUsercontrol.SetCharacterFrom(career);
                        sINnersUsercontrol.PostSINnerAsync();
                    }
                }
                catch(Exception e)
                {
                    string msg = "Exception while loading " + file.FullName + ":";
                    msg += Environment.NewLine + e.ToString();
                    Debug.Write(msg);
                    throw;
                }
            }
        }
    }
}
