using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using Microsoft.Rest;
using Newtonsoft.Json;
using ChummerHub.Client.Sinners;
using NLog;
using Utils = ChummerHub.Client.Backend.Utils;

//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersOptions : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private bool? LoginStatus;

        private static SINnerVisibility _SINnerVisibility;
        public static SINnerVisibility SINnerVisibility
        {
            get
            {
                if (_SINnerVisibility != null)
                    return _SINnerVisibility;
                Settings.Default.Reload();
                var ur = new SINnerUserRight
                {
                    EMail = !string.IsNullOrEmpty(Settings.Default.UserEmail)
                        ? Settings.Default.UserEmail
                        : "delete.this.and.add@your.mail",
                    CanEdit = true,
                    Id = Guid.NewGuid()
                };
                if (string.IsNullOrEmpty(Settings.Default.SINnerVisibility))
                {
                    return _SINnerVisibility = new SINnerVisibility
                    {
                        Id = Guid.NewGuid(),
                        IsPublic = Settings.Default.VisibilityIsPublic,
                        UserRights = new List<SINnerUserRight>
                        {
                            ur
                        }
                    };
                }
                try
                {
                    return _SINnerVisibility = JsonConvert.DeserializeObject<SINnerVisibility>(Settings.Default.SINnerVisibility);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
                return _SINnerVisibility = new SINnerVisibility
                {
                    Id = Guid.NewGuid(),
                    IsPublic = Settings.Default.VisibilityIsPublic,
                    UserRights = new List<SINnerUserRight>
                    {
                        ur
                    }
                };
            }
            set
            {
                _SINnerVisibility = value;
                if (value == null)
                    Settings.Default.SINnerVisibility = null;
            }
        }

        public ucSINnersOptions()
        {
            InitializeComponent();
            InitializeMe();
        }

        public static bool UploadOnSave
        {
            get => Settings.Default.UploadOnSave;
            set
            {
                Settings.Default.UploadOnSave = value;
                Settings.Default.Save();
            }
        }

        public static Task<T> StartSTATask<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private bool IsLoading;

        private void InitializeMe()
        {
            if (IsLoading)
                return;
            IsLoading = true;
            string tip = "Milestone builds always user sinners." + Environment.NewLine + "Nightly builds always user sinners-beta.";
            cbSINnerUrl.SetToolTip(tip);
            cbSINnerUrl.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
            Settings.Default.Reload();
            if (string.IsNullOrEmpty(Settings.Default.TempDownloadPath))
            {
                Settings.Default.TempDownloadPath = Path.GetTempPath();
                Settings.Default.Save();
            }
            tbTempDownloadPath.Text = Settings.Default.TempDownloadPath;
            tbTempDownloadPath.SetToolTip("Where should chummer download the temporary files from the WebService?");
            var client = StaticUtils.GetClient();
            if (client == null)
            {
                return;
            }
            var sinnerurl = client.BaseUrl.ToString();
            Settings.Default.SINnerUrls.Clear();
            Settings.Default.Save();
            Settings.Default.SINnerUrls.Add("https://chummer-stable.azurewebsites.net/");
            Settings.Default.SINnerUrls.Add("https://chummer-beta.azurewebsites.net/");
            Settings.Default.Save();
            cbSINnerUrl.DataSource = Settings.Default.SINnerUrls;
            cbSINnerUrl.SelectedItem = sinnerurl;
            if (cbSINnerUrl.SelectedItem == null)
                cbSINnerUrl.SelectedItem = Settings.Default.SINnerUrls[0];

            cbVisibilityIsPublic.Checked = Settings.Default.VisibilityIsPublic;
            cbIgnoreWarnings.Checked = Settings.Default.IgnoreWarningsOnOpening;
            cbOpenChummerFromSharedLinks.Checked = Settings.Default.OpenChummerFromSharedLinks;
            cbSINnerUrl.Enabled = true;
            rbListUserMode.SelectedIndex = Settings.Default.UserModeRegistered ? 1 : 0;
            cbVisibilityIsPublic.BindingContext = new BindingContext();
            if (StaticUtils.UserRoles?.Count == 0)
            {
                _ = StartSTATask(
                    async () =>
                    {
                        var roles = await GetRolesStatus(this).ConfigureAwait(true);
                        UpdateDisplay();
                        if (roles.Count == 0)
                            ShowWebBrowser();
                    });
            }
            else
            {
                LoginStatus = true;
                UpdateDisplay();
            }
            cbUploadOnSave.Checked = UploadOnSave;
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
            //AddShieldToButton(bRegisterUriScheme);
            cbVisibilityIsPublic.CheckedChanged += cbVisibilityIsPublic_CheckedChanged;
            cbUploadOnSave.CheckedChanged += cbUploadOnSave_CheckedChanged;
            rbListUserMode.SelectedIndexChanged += RbListUserMode_SelectedIndexChanged;
            cbIgnoreWarnings.CheckedChanged += CbIgnoreWarningsOnCheckedChanged;
            cbOpenChummerFromSharedLinks.CheckedChanged += CbOpenChummerFromSharedLinksOnCheckedChanged;
        }

        private void CbIgnoreWarningsOnCheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void CbOpenChummerFromSharedLinksOnCheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }


        //[DllImport("user32.dll")]
        //public static extern int SendMessage(IntPtr hWnd,
        //    uint Msg, int wParam, int lParam);

        //// Make the button display the UAC shield.
        //public static void AddShieldToButton(Button btn)
        //{
        //    const Int32 BCM_SETSHIELD = 0x160C;

        //    // Give the button the flat style and make it
        //    // display the UAC shield.
        //    btn.FlatStyle = System.Windows.Forms.FlatStyle.System;
        //    SendMessage(btn.Handle, BCM_SETSHIELD, 0, 1);
        //}

        ~ucSINnersOptions()
        {
            Settings.Default.Save();
        }

        private async void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Settings.Default.Save();
            var client = StaticUtils.GetClient();
            if (client != null)
                StaticUtils.GetClient(true);
            bLogin.Text = "Logout";
            //this.cbRoles.DataSource = null;
            LoginStatus = false;
            InitializeMe();
        }

        public async void UpdateDisplay()
        {
            tlpOptions.Enabled = Settings.Default.UserModeRegistered;
            var mail = await GetUserEmail().ConfigureAwait(true);
            this.DoThreadSafe(() =>
            {
                try
                {
                    Settings.Default.Reload();
                    tbTempDownloadPath.Text = Settings.Default.TempDownloadPath;

                    if (!string.IsNullOrEmpty(mail))
                    {
                        lUsername.Text = mail;
                        //also, since we are logged in in now, refresh the frmCharacterRoster!
                        PluginHandler.MainForm?.DoThreadSafe(() =>
                        {
                            PluginHandler.MainForm.CharacterRoster.LoadCharacters();
                        });
                        bLogin.Text = "Logout";
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        cbRoles.DataSource = bs;
                        HideWebBrowser();
                    }
                    else
                    {
                        bLogin.Text = "Login";
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        cbRoles.DataSource = bs;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            });
        }

        public async Task<string> GetUserEmail()
        {
            using (new CursorWait(this, true))
            {
                try
                {
                    var client = StaticUtils.GetClient();
                    if (client == null)
                        return null;
                    string strEmail;
                    var result = await client.GetUserByAuthorizationAsync().ConfigureAwait(true);
                    if (result ==  null)
                    {
                        LoginStatus = false;
                        return null;
                    }
                    strEmail = result.MyApplicationUser.Email;
                    if (!string.IsNullOrEmpty(strEmail))
                    {
                        Settings.Default.UserEmail = strEmail;
                        Settings.Default.Save();
                    }
                    return strEmail;
                }
                catch (SerializationException)
                {
                    LoginStatus = false;
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            }
            return null;
        }

        private void bLogin_ClickAsync(object sender, EventArgs e)
        {
            if (bLogin.Text == "Logout")
            {
                StaticUtils.AuthorizationCookieContainer = null;
                LoginStatus = false;
                bLogin.Text = "Login";
                _ = StartSTATask(
                      async () =>
                      {
                          try
                          {
                              var client = StaticUtils.GetClient();
                              var signout = await client.LogoutAsync().ConfigureAwait(true);
                              {
                                  if (!signout)
                                  {
                                      var roles = GetRolesStatus(this).Result;
                                  }
                                  else
                                  {
                                      StaticUtils.UserRoles.Clear();
                                  }
                              }
                              UpdateDisplay();
                          }
                          catch(Exception ex)
                          {
                              Log.Warn(ex);
                          }
                      });
            }
            else
            {
                Settings.Default.SINnerUrl = cbSINnerUrl.SelectedItem?.ToString();
                Settings.Default.Save();
                ShowWebBrowser();
            }
        }

        private void HideWebBrowser()
        {
            frmWebBrowser?.Hide();
        }

        private frmWebBrowser frmWebBrowser;

        private void ShowWebBrowser()
        {
            try
            {
                if (frmWebBrowser == null)
                {
                    frmWebBrowser = new frmWebBrowser();
                }
                if(frmWebBrowser.InvokeRequired)
                {
                    Invoke((Action)(() =>
                    {
                        frmWebBrowser.ShowDialog(Program.MainForm);
                        _ = StartSTATask(
                        async () =>
                        {
                            var roles = await GetRolesStatus(this).ConfigureAwait(true);
                            UpdateDisplay();
                        });
                    })
                    );
                }
                else
                {
                    frmWebBrowser.ShowDialog(Program.MainForm);
                    _ = StartSTATask(
                           async () =>
                           {
                               var roles = await GetRolesStatus(this).ConfigureAwait(true);
                               UpdateDisplay();
                           });
                }

                ResumeLayout(false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }

        private async Task<IList<string>> GetRolesStatus(Control sender)
        {
            ResultAccountGetRoles myresult = null;
            try
            {
                using (new CursorWait(sender, true))
                {
                    var client = StaticUtils.GetClient();
                    if (client == null)
                        return StaticUtils.UserRoles;
                    myresult = await client.GetRolesAsync().ConfigureAwait(true);
                    await Utils.HandleError(myresult).ConfigureAwait(true);
                    var myresultbody = myresult;
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        if (myresultbody?.CallSuccess == true)
                        {
                            StaticUtils.UserRoles = myresultbody.Roles.ToList();
                            if (StaticUtils.UserRoles != null && StaticUtils.UserRoles.Count > 0)
                            {
                                LoginStatus = true;
                            }

                            StaticUtils.PossibleRoles = myresultbody.PossibleRoles.ToList();
                        }

                        //bBackup.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        //bRestore.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        cbRoles.DataSource = bs;
                    });
                }

                return StaticUtils.UserRoles;
            }
            catch (SerializationException)
            {
                LoginStatus = false;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                Log.Info(ex);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            finally
            {
                //myresult?.Dispose();
            }
            return null;
        }

        private void cbVisibilityIsGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void OptionsUpdate()
        {
            Settings.Default.TempDownloadPath = tbTempDownloadPath.Text;
            Settings.Default.VisibilityIsPublic = cbVisibilityIsPublic.Checked;
            Settings.Default.IgnoreWarningsOnOpening = cbIgnoreWarnings.Checked;
            Settings.Default.OpenChummerFromSharedLinks = cbOpenChummerFromSharedLinks.Checked;
            Settings.Default.UserModeRegistered = rbListUserMode.SelectedIndex > 0;
            Settings.Default.Save();
        }

        private void cbVisibilityIsPublic_CheckedChanged(object sender, EventArgs e)
        {

            OptionsUpdate();
        }

        private void tbGroupname_TextChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }


        private async void bMultiUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog thisDialog = new OpenFileDialog
            {
                Filter = "Chummer files (*.chum5)|*.chum5|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true,
                Title = "Please Select Chummer File(s) for Batch-Upload"
            })
            {
                if (thisDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in thisDialog.FileNames)
                    {
                        using (_ = await Utils.UploadCharacterFromFile(file).ConfigureAwait(true))
                        {
                        }
                    }

                    Program.MainForm.ShowMessageBox("Upload of " + thisDialog.FileNames.Length + " files finished (successful or not - its over).");
                }
            }
        }

        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            UploadOnSave = cbUploadOnSave.Checked;

        }

        private void bBackup_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    BackupTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.MainForm.ShowMessageBox(a.Status.ToString());
                    });
                }
            }
        }

        private async Task BackupTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            using (new CursorWait(this, true))
            {
                try
                {
                    var client = StaticUtils.GetClient();
                    var getsinner = await client.AdminGetSINnersAsync().ConfigureAwait(true);
                    foreach (var sinner in getsinner)
                    {
                        try
                        {
                            if (sinner.SiNnerMetaData.Tags.Count == 0)
                            {
                                Log.Error("Sinner " + sinner.Id + " has no Tags!");
                                continue;
                            }
                            string jsonsinner = JsonConvert.SerializeObject(sinner);
                            string filePath = Path.Combine(folderName, sinner.Id.ToString() + ".chum5json");
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                            File.WriteAllText(filePath, jsonsinner);
                            Log.Info("Sinner " + sinner.Id + " saved to " + filePath);
                        }
                        catch (Exception e2)
                        {
                            Log.Error(e2);
                            Invoke(new Action(() => Program.MainForm.ShowMessageBox(e2.Message)));
                        }
                    }
                    //getsinner.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Invoke(new Action(() => Program.MainForm.ShowMessageBox(ex.Message)));
                }
            }
        }

        private void bRestore_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    RestoreTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.MainForm.ShowMessageBox(a.Status.ToString());
                    });
                }
            }
        }

        private async Task RestoreTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            try
            {
                DirectoryInfo d = new DirectoryInfo(folderName);//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.chum5json"); //Getting Text files
                using (new CursorWait(this, true))
                {
                    var client = StaticUtils.GetClient();
                    foreach (FileInfo file in Files)
                    {
                        ResultSinnerPostSIN posttask = null;
                        try
                        {
                            string sinjson = File.ReadAllText(file.FullName);
                            SINner sin = JsonConvert.DeserializeObject<SINner>(sinjson);
                            UploadInfoObject uploadInfoObject = new UploadInfoObject
                            {
                                Client = PluginHandler.MyUploadClient,
                                UploadDateTime = DateTime.Now,
                                SiNners = new List<SINner>
                                {
                                    sin
                                }
                            };
                            posttask = await client.PostSINAsync(uploadInfoObject).ConfigureAwait(true);
                            if (posttask.CallSuccess)
                            {
                                Log.Info("SINner " + sin.Id + " posted!");
                            }
                            else
                            {
                                string msg = posttask.ErrorText + ": " + Environment.NewLine;
                                var content = posttask.MyException?.ToString();
                                msg += content;
                                Log.Warn("SINner " + sin.Id + " not posted: " + msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Could not read file " + file.FullName + ": ");
                        }
                        finally
                        {
                            //posttask?.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.MainForm.ShowMessageBox(ex.Message);
            }
        }

        private void BTempPathBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbTempDownloadPath.Text = fbd.SelectedPath;
                    OptionsUpdate();
                    Settings.Default.Save();
                }
            }
        }

        private void BEditDefaultVisibility_Click(object sender, EventArgs e)
        {
            using (frmSINnerVisibility visfrm = new frmSINnerVisibility
            {
                MyVisibility = SINnerVisibility
            })
            {
                var result = visfrm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    SINnerVisibility = visfrm.MyVisibility;
                    Settings.Default.SINnerVisibility = JsonConvert.SerializeObject(SINnerVisibility);
                    Settings.Default.Save();
                }
            }
        }

        private void RbListUserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            tlpOptions.Enabled = rbListUserMode.SelectedIndex > 0;
            OptionsUpdate();
        }
    }
}
