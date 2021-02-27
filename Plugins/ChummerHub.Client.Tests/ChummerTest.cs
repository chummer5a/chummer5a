using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                    catch (Exception e)
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
        public async Task LoadCharacter()
        {
            Settings.Default.SINnerUrl = "https://chummer-stable.azurewebsites.net/";
            Debug.WriteLine("Unit test initialized for: LoadCharacter()");
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            path = Path.Combine(path, "data");
            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.chum5"); //Getting Text files
            foreach (FileInfo file in Files)
            {
                try
                {
                    Debug.WriteLine("Loading: " + file.Name);
                    using (Character c = await MainForm.LoadCharacter(file.FullName))
                    {
                        if (c == null)
                            continue;
                        Debug.WriteLine("Character loaded: " + c.Name);
                        using (CharacterShared frmCharacterForm = c.Created ? (CharacterShared)new frmCareer(c) : new frmCreate(c))
                        {
                            frmCharacterForm.MdiParent = MainForm;
                            frmCharacterForm.WindowState = FormWindowState.Minimized;
                            frmCharacterForm.Show();
                            using (ucSINnersUserControl sINnersUsercontrol = new ucSINnersUserControl())
                            {
                                var ce = await sINnersUsercontrol.SetCharacterFrom(frmCharacterForm);
                                await Utils.PostSINnerAsync(ce);
                                await Utils.UploadChummerFileAsync(ce);
                            }
                            frmCharacterForm.Close();
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
