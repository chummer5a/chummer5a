using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Chummer;
using ChummerHub.Client.Backend;
using ChummerHub.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChummerHub.Client.Tests
{
    [TestClass]
    //[DeploymentItem("..\\..\\Chummer\\settings", "settings")]
    public class ChummerTest
    {
        //public static SINnersUsercontrol MySINnersUsercontrol = new SINnersUsercontrol();

        public static frmChummerMain MainForm;

        [TestMethod]
        public async Task LoadCharacter()
        {
            Properties.Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net/";
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (MainForm == null)
                MainForm = new frmChummerMain(true);
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
                        using (frmCareer career = new frmCareer(c))
                        {
                            career.Show();
                            SINnersUserControl sINnersUsercontrol = new SINnersUserControl();
                            var ce = sINnersUsercontrol.SetCharacterFrom(career);
                            await Utils.PostSINnerAsync(ce);
                            await Utils.UploadChummerFileAsync(ce);
                            career.Hide();
                            career.Dispose();
                        }
                    }
                    else
                    {
                        using (frmCreate create = new frmCreate(c))
                        {
                            create.Show();
                            SINnersUserControl sINnersUsercontrol = new SINnersUserControl();
                            var ce = sINnersUsercontrol.SetCharacterFrom(create);
                            await Utils.PostSINnerAsync(ce);
                            await Utils.UploadChummerFileAsync(ce);
                            create.Hide();
                            create.Dispose();
                        }
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
