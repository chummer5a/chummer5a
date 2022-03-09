using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using Microsoft.Rest;
using Newtonsoft.Json;
using ChummerHub.Client.Sinners;
using GroupControls;
using NLog;
using Utils = ChummerHub.Client.Backend.Utils;

//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersOptions : UserControl
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private bool? LoginStatus;

        private static SINnerVisibility _SINnerVisibility;
        public static SINnerVisibility SINnerVisibility
        {
            get
            {
                if (_SINnerVisibility != null)
                    return _SINnerVisibility;
                Settings.Default.Reload();
                SINnerUserRight ur = new SINnerUserRight
                {
                    EMail = !string.IsNullOrEmpty(Settings.Default.UserEmail)
                        ? Settings.Default.UserEmail
                        : "delete.this.and.add@your.mail",
                    CanEdit = true,
                    Id = Guid.NewGuid()
                };
                return _SINnerVisibility = Utils.DefaultSINnerVisibility ?? new SINnerVisibility
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
                    Utils.DefaultSINnerVisibility = null;
            }
        }

        public ucSINnersOptions()
        {
            InitializeComponent();
            InitializeMe(true).GetAwaiter().GetResult();
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

        private bool IsLoading;

        private async Task InitializeMe(bool blnSync)
        {
            if (IsLoading)
                return;
            IsLoading = true;
            string tip = "Milestone builds always user sinners." + Environment.NewLine + "Nightly builds always user sinners-beta.";
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x =>
                {
                    x.SetToolTip(tip);
                    ((ComboBox)x).SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
                });
            }
            else
            {
                await cbSINnerUrl.DoThreadSafeAsync(x =>
                {
                    x.SetToolTip(tip);
                    ((ComboBox)x).SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
                });
            }

            Settings.Default.Reload();
            if (string.IsNullOrEmpty(Settings.Default.TempDownloadPath))
            {
                Settings.Default.TempDownloadPath = Path.GetTempPath();
                Settings.Default.Save();
            }
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                tbTempDownloadPath.DoThreadSafe(x =>
                {
                    x.Text = Settings.Default.TempDownloadPath;
                    x.SetToolTip(
                        "Where should chummer download the temporary files from the WebService?");
                });
            }
            else
            {
                await tbTempDownloadPath.DoThreadSafeAsync(x =>
                {
                    x.Text = Settings.Default.TempDownloadPath;
                    x.SetToolTip(
                        "Where should chummer download the temporary files from the WebService?");
                });
            }

            SinnersClient client = blnSync
                ? this.DoThreadSafeFunc(() => StaticUtils.GetClient())
                : await this.DoThreadSafeFuncAsync(() => StaticUtils.GetClient());
            if (client == null)
            {
                return;
            }
            string sinnerurl = client.BaseUrl;
            Settings.Default.SINnerUrls.Clear();
            Settings.Default.Save();
            Settings.Default.SINnerUrls.Add("https://chummer-stable.azurewebsites.net/");
            Settings.Default.SINnerUrls.Add("https://chummer-beta.azurewebsites.net/");
            Settings.Default.Save();
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x =>
                {
                    ((ComboBox)x).DataSource = Settings.Default.SINnerUrls;
                    ((ComboBox)x).SelectedItem = sinnerurl ?? Settings.Default.SINnerUrls[0];
                    x.Enabled = true;
                });
                // ReSharper disable once MethodHasAsyncOverload
                cbIgnoreWarnings.DoThreadSafe(
                    x => ((CheckBox)x).Checked = Settings.Default.IgnoreWarningsOnOpening);
                // ReSharper disable once MethodHasAsyncOverload
                cbOpenChummerFromSharedLinks.DoThreadSafe(
                    x => ((CheckBox)x).Checked = Settings.Default.OpenChummerFromSharedLinks);
                // ReSharper disable once MethodHasAsyncOverload
                rbListUserMode.DoThreadSafe(
                    x => ((RadioButtonList)x).SelectedIndex = Settings.Default.UserModeRegistered ? 1 : 0);
                // ReSharper disable once MethodHasAsyncOverload
                cbVisibilityIsPublic.DoThreadSafe(x =>
                {
                    ((CheckBox)x).Checked = Settings.Default.VisibilityIsPublic;
                    x.BindingContext = new BindingContext();
                });
            }
            else
            {
                await Task.WhenAll(
                    cbSINnerUrl.DoThreadSafeAsync(x =>
                    {
                        ((ComboBox)x).DataSource = Settings.Default.SINnerUrls;
                        ((ComboBox)x).SelectedItem = sinnerurl ?? Settings.Default.SINnerUrls[0];
                        x.Enabled = true;
                    }),
                    cbIgnoreWarnings.DoThreadSafeAsync(x =>
                        ((CheckBox)x).Checked = Settings.Default.IgnoreWarningsOnOpening),
                    cbOpenChummerFromSharedLinks.DoThreadSafeAsync(x =>
                        ((CheckBox)x).Checked = Settings.Default.OpenChummerFromSharedLinks),
                    rbListUserMode.DoThreadSafeAsync(x =>
                        ((RadioButtonList)x).SelectedIndex = Settings.Default.UserModeRegistered ? 1 : 0),
                    cbVisibilityIsPublic.DoThreadSafeAsync(x =>
                    {
                        ((CheckBox)x).Checked = Settings.Default.VisibilityIsPublic;
                        x.BindingContext = new BindingContext();
                    }));
            }

            if (StaticUtils.UserRoles?.Count == 0)
            {
                _ = Chummer.Utils.StartStaTask(
                    async () =>
                    {
                        IList<string> roles = await GetRolesStatus(this);
                        await UpdateDisplay();
                        if (roles.Count == 0)
                            ShowWebBrowser();
                    });
            }
            else
            {
                LoginStatus = true;
                if (blnSync)
                    Chummer.Utils.RunWithoutThreadLock(UpdateDisplay);
                else
                    await UpdateDisplay();
            }

            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbUploadOnSave.DoThreadSafe(x =>
                {
                    ((CheckBox)x).Checked = UploadOnSave;
                    ((CheckBox)x).CheckedChanged += cbUploadOnSave_CheckedChanged;
                });
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x => ((ComboBox)x).SelectedValueChanged += CbSINnerUrl_SelectedValueChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbVisibilityIsPublic.DoThreadSafe(x => ((CheckBox)x).CheckedChanged += cbVisibilityIsPublic_CheckedChanged);
                // ReSharper disable once MethodHasAsyncOverload
                rbListUserMode.DoThreadSafe(x => ((RadioButtonList)x).SelectedIndexChanged += RbListUserMode_SelectedIndexChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbIgnoreWarnings.DoThreadSafe(x => ((CheckBox)x).CheckedChanged += CbIgnoreWarningsOnCheckedChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbOpenChummerFromSharedLinks.DoThreadSafe(x => ((CheckBox)x).CheckedChanged += CbOpenChummerFromSharedLinksOnCheckedChanged);
            }
            else
            {
                await Task.WhenAll(
                    cbUploadOnSave.DoThreadSafeAsync(x =>
                    {
                        ((CheckBox)x).Checked = UploadOnSave;
                        ((CheckBox)x).CheckedChanged += cbUploadOnSave_CheckedChanged;
                    }),
                    cbSINnerUrl.DoThreadSafeAsync(x =>
                        ((ComboBox)x).SelectedValueChanged += CbSINnerUrl_SelectedValueChanged),
                    cbVisibilityIsPublic.DoThreadSafeAsync(x =>
                                                               ((CheckBox)x).CheckedChanged += cbVisibilityIsPublic_CheckedChanged),
                    rbListUserMode.DoThreadSafeAsync(x =>
                                                         ((RadioButtonList)x).SelectedIndexChanged += RbListUserMode_SelectedIndexChanged),
                    cbIgnoreWarnings.DoThreadSafeAsync(x =>
                                                           ((CheckBox)x).CheckedChanged += CbIgnoreWarningsOnCheckedChanged),
                    cbOpenChummerFromSharedLinks.DoThreadSafeAsync(x =>
                                                                       ((CheckBox)x).CheckedChanged += CbOpenChummerFromSharedLinksOnCheckedChanged));
            }
            //AddShieldToButton(bRegisterUriScheme);
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
            SinnersClient client = StaticUtils.GetClient();
            if (client != null)
                StaticUtils.GetClient(true);
            bLogin.Text = "Logout";
            //this.cbRoles.DataSource = null;
            LoginStatus = false;
            await InitializeMe(false);
        }

        public async Task UpdateDisplay()
        {
            tlpOptions.Enabled = Settings.Default.UserModeRegistered;
            string mail = await GetUserEmail();
            await this.DoThreadSafeAsync(async () =>
            {
                try
                {
                    Settings.Default.Reload();
                    tbTempDownloadPath.Text = Settings.Default.TempDownloadPath;

                    if (!string.IsNullOrEmpty(mail))
                    {
                        lUsername.Text = mail;
                        //also, since we are logged in in now, refresh the frmCharacterRoster!
                        if (PluginHandler.MainForm != null)
                            await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodes(PluginHandler.MyPluginHandlerInstance);
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
            using (CursorWait.New(this, true))
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return null;
                    ResultAccountGetUserByAuthorization result = await client.GetUserByAuthorizationAsync();
                    if (result ==  null)
                    {
                        LoginStatus = false;
                        return null;
                    }
                    string strEmail = result.MyApplicationUser.Email;
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
                _ = Chummer.Utils.StartStaTask(
                    async () =>
                    {
                        try
                        {
                            SinnersClient client = StaticUtils.GetClient();
                            if (await client.LogoutAsync())
                            {
                                StaticUtils.UserRoles.Clear();
                            }
                            else
                            {
                                await GetRolesStatus(this);
                            }

                            await UpdateDisplay();
                        }
                        catch (Exception ex)
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
                    Invoke((Action) (() =>
                        {
                            frmWebBrowser.ShowDialog(Program.MainForm);
                            _ = Chummer.Utils.StartStaTask(
                                async () =>
                                {
                                    await GetRolesStatus(this);
                                    await UpdateDisplay();
                                });
                        })
                    );
                }
                else
                {
                    frmWebBrowser.ShowDialog(Program.MainForm);
                    _ = Chummer.Utils.StartStaTask(
                        async () =>
                        {
                            await GetRolesStatus(this);
                            await UpdateDisplay();
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
            try
            {
                using (CursorWait.New(sender, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return StaticUtils.UserRoles;
                    ResultAccountGetRoles myresult = await client.GetRolesAsync();
                    await Utils.ShowErrorResponseFormAsync(myresult);
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        if (myresult?.CallSuccess == true)
                        {
                            StaticUtils.UserRoles = myresult.Roles.ToList();
                            if (StaticUtils.UserRoles != null && StaticUtils.UserRoles.Count > 0)
                            {
                                LoginStatus = true;
                            }

                            StaticUtils.PossibleRoles = myresult.PossibleRoles.ToList();
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
                if (thisDialog.ShowDialog() != DialogResult.OK)
                    return;
                foreach (string file in thisDialog.FileNames)
                {
                    CharacterExtended objTemp = null;
                    try
                    {
                        objTemp = await Utils.UploadCharacterFromFile(file);
                    }
                    finally
                    {
                        objTemp?.Dispose();
                    }
                }

                Program.ShowMessageBox("Upload of " + thisDialog.FileNames.Length + " files finished (successful or not - its over).");
            }
        }

        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            UploadOnSave = cbUploadOnSave.Checked;

        }

        private async void bBackup_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    await BackupTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.ShowMessageBox(a.Status.ToString());
                    });
                }
            }
        }

        private async Task BackupTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            using (CursorWait.New(this, true))
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    ICollection<SINner> getsinner = await client.AdminGetSINnersAsync();
                    foreach (SINner sinner in getsinner)
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
                            Invoke(new Action(() => Program.ShowMessageBox(e2.Message)));
                        }
                    }
                    //getsinner.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Invoke(new Action(() => Program.ShowMessageBox(ex.Message)));
                }
            }
        }

        private async void bRestore_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    await RestoreTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.ShowMessageBox(a.Status.ToString());
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
                using (CursorWait.New(this, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    foreach (FileInfo file in Files)
                    {
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
                            ResultSinnerPostSIN posttask = await client.PostSINAsync(uploadInfoObject);
                            if (posttask.CallSuccess)
                            {
                                Log.Info("SINner " + (sin?.Id.ToString() ?? "null") + " posted!");
                            }
                            else
                            {
                                string msg = posttask.ErrorText + ": " + Environment.NewLine;
                                string content = posttask.MyException?.ToString();

                                msg += content;
                                Log.Warn("SINner " + (sin?.Id.ToString() ?? "null") + " not posted: " + msg);
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
                Program.ShowMessageBox(ex.Message);
            }
        }

        private void BTempPathBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    return;
                tbTempDownloadPath.Text = fbd.SelectedPath;
                OptionsUpdate();
                Settings.Default.Save();
            }
        }

        private void BEditDefaultVisibility_Click(object sender, EventArgs e)
        {
            using (frmSINnerVisibility visfrm = new frmSINnerVisibility
            {
                MyVisibility = SINnerVisibility
            })
            {
                if (visfrm.ShowDialog(this) != DialogResult.OK)
                    return;
                SINnerVisibility = visfrm.MyVisibility;
                Utils.DefaultSINnerVisibility = visfrm.MyVisibility;
            }
        }

        private void RbListUserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            tlpOptions.Enabled = rbListUserMode.SelectedIndex > 0;
            OptionsUpdate();
        }
    }
}
