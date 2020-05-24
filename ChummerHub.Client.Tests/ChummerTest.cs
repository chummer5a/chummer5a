using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Chummer;
using ChummerHub.Client.Tests.Properties;
using ChummerHub.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils = ChummerHub.Client.Backend.Utils;

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
            Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net/";
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (MainForm == null)
                MainForm = new frmChummerMain(true);
            path = Path.Combine(path, "data");
            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                try
                {
                    Debug.WriteLine("Loading: " + file.Name);
                    Character c = await MainForm.LoadCharacter(file.FullName);
                    if (c == null)
                        continue;
                    Debug.WriteLine("Character loaded: " + c.Name);
                    if (c.Created)
                    {
                        using (frmCareer career = new frmCareer(c))
                        {
                            career.Show();
                            ucSINnersUserControl sINnersUsercontrol = new ucSINnersUserControl();
                            var ce = await sINnersUsercontrol.SetCharacterFrom(career);
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
                            ucSINnersUserControl sINnersUsercontrol = new ucSINnersUserControl();
                            var ce = await sINnersUsercontrol.SetCharacterFrom(create);
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
                    msg += Environment.NewLine + e;
                    Debug.Write(msg);
                    throw;
                }
            }
        }
    }
}
