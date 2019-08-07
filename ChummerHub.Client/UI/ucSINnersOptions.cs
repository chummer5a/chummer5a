using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Net;
using ChummerHub.Client.Backend;
using SINners.Models;
using System.Diagnostics;
using Chummer;
using Chummer.Plugins;
using System.Threading;
using ChummerHub.Client.Model;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Windows;
using Microsoft.Win32;
using NLog;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersOptions : UserControl
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private bool? LoginStatus = null;
        

        private static SINners.Models.SINnerVisibility _SINnerVisibility = null;
        public static SINners.Models.SINnerVisibility SINnerVisibility
        {
            get
            {
                if (_SINnerVisibility != null)
                    return _SINnerVisibility;
                Properties.Settings.Default.Reload();
                if(String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                {
                    _SINnerVisibility = new SINners.Models.SINnerVisibility()
                    {
                        Id = Guid.NewGuid(),
                        IsPublic = Properties.Settings.Default.VisibilityIsPublic,
                        UserRights = new List<SINnerUserRight>()
                        {


                        },
                    };
                    if(!String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                    {
                        var ur = new SINnerUserRight()
                        {
                            EMail = Properties.Settings.Default.UserEmail,
                            CanEdit = true,
                            Id = Guid.NewGuid()
                        };
                        _SINnerVisibility.UserRights.Add(ur);
                    }
                    else
                    {
                        var ur = new SINnerUserRight()
                        {
                            EMail = "delete.this.and.add@your.mail",
                            CanEdit = true,
                            Id = Guid.NewGuid()
                        };
                        _SINnerVisibility.UserRights.Add(ur);
                    }
                    return _SINnerVisibility;
                }
                try
                {
                    return _SINnerVisibility = Newtonsoft.Json.JsonConvert.DeserializeObject<SINners.Models.SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                }
                catch(Exception e)
                {
                    Log.Warn(e);
                    
                }
                _SINnerVisibility = new SINners.Models.SINnerVisibility()
                {
                    Id = Guid.NewGuid(),
                    IsPublic = Properties.Settings.Default.VisibilityIsPublic,
                    UserRights = new List<SINnerUserRight>()
                    {


                    }
                    
                };
                if(!String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                {
                    var ur = new SINnerUserRight()
                    {
                        EMail = Properties.Settings.Default.UserEmail,
                        CanEdit = true,
                        Id = Guid.NewGuid()
                    };
                    _SINnerVisibility.UserRights.Add(ur);
                }
                else
                {
                    var ur = new SINnerUserRight()
                    {
                        EMail = "delete.this.and.add@your.mail",
                        CanEdit = true,
                        Id = Guid.NewGuid()
                    };
                    _SINnerVisibility.UserRights.Add(ur);
                }
                return _SINnerVisibility;
            }
            set
            {
                if (value != null)
                {
                    _SINnerVisibility = value;
                }
                else
                {
                    Properties.Settings.Default.SINnerVisibility = null;
                    _SINnerVisibility = null;
                }
            }
        }

      
       
        public ucSINnersOptions()
        {
            InitializeComponent();
            InitializeMe();
            

        }

        public static bool UploadOnSave
        {
            get
            {
                return Properties.Settings.Default.UploadOnSave;
            }
            set
            {
                Properties.Settings.Default.UploadOnSave = value;
                Properties.Settings.Default.Save();
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
                catch(Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private bool IsLoading = false;

        private async Task InitializeMe()
        {
            if (IsLoading)
                return;
            IsLoading = true;
            string tip = "Milestone builds always user sinners." + Environment.NewLine + "Nightly builds always user sinners-beta.";
            cbSINnerUrl.SetToolTip(tip);
            cbSINnerUrl.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
            Properties.Settings.Default.Reload();
            if (String.IsNullOrEmpty(Properties.Settings.Default.TempDownloadPath))
            {
                Properties.Settings.Default.TempDownloadPath = Path.GetTempPath();
                Properties.Settings.Default.Save();
            }
            tbTempDownloadPath.Text = Properties.Settings.Default.TempDownloadPath;
            tbTempDownloadPath.SetToolTip("Where should chummer download the temporary files from the WebService?");
            var client = StaticUtils.GetClient(); 
            if (client == null)
            {
                return;
            }
            var sinnerurl = client.BaseUri.ToString();
            if (Properties.Settings.Default.SINnerUrls.Contains("http://sinners-beta.azurewebsites.net/"))
            {
                Properties.Settings.Default.SINnerUrls.Remove("http://sinners-beta.azurewebsites.net/");
                Properties.Settings.Default.SINnerUrls.Add("https://sinners-beta.azurewebsites.net/");
                Properties.Settings.Default.Save();
            }
            this.cbSINnerUrl.DataSource = Properties.Settings.Default.SINnerUrls;
            this.cbSINnerUrl.SelectedItem = sinnerurl;
            this.cbVisibilityIsPublic.Checked = Properties.Settings.Default.VisibilityIsPublic;
            //this.cbVisibilityIsGroupVisible.Checked = Properties.Settings.Default.VisibilityIsGroupVisible;
            cbSINnerUrl.Enabled = false;
            if (ChummerHub.Client.Properties.Settings.Default.UserModeRegistered == true)
            {
                this.rbListUserMode.SelectedIndex = 1;
            }
            else
            {
                this.rbListUserMode.SelectedIndex = 0;
            }
            this.cbVisibilityIsPublic.BindingContext = new BindingContext();
            if ((StaticUtils.UserRoles == null)
                || (!StaticUtils.UserRoles.Any()))
            {
                var t = StartSTATask(
                    async () =>
                    {
                        var roles = await GetRolesStatus(this);
                        UpdateDisplay();
                        if (!roles.Any())
                            ShowWebBrowser();
                    });
            }
            else
            {
                LoginStatus = true;
                UpdateDisplay();
            }
            cbUploadOnSave.Checked = ucSINnersOptions.UploadOnSave;
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
            AddShieldToButton(bRegisterUriScheme);
            this.cbVisibilityIsPublic.CheckedChanged += cbVisibilityIsPublic_CheckedChanged;
            this.cbUploadOnSave.CheckedChanged += cbUploadOnSave_CheckedChanged;
            this.rbListUserMode.SelectedIndexChanged += RbListUserMode_SelectedIndexChanged;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
            uint Msg, int wParam, int lParam);

        // Make the button display the UAC shield.
        public static void AddShieldToButton(Button btn)
        {
            const Int32 BCM_SETSHIELD = 0x160C;

            // Give the button the flat style and make it
            // display the UAC shield.
            btn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            SendMessage(btn.Handle, BCM_SETSHIELD, 0, 1);
        }

        ~ucSINnersOptions()
        {
            Properties.Settings.Default.Save();
        }

        private async void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Properties.Settings.Default.Save();
            var client = StaticUtils.GetClient();
            if (client != null)
                StaticUtils.GetClient(true);
            this.bLogin.Text = "Logout";
            //this.cbRoles.DataSource = null;
            this.LoginStatus = false;
            await InitializeMe();

        }

        

        public async void UpdateDisplay()
        {
            this.tlpOptions.Enabled = Properties.Settings.Default.UserModeRegistered;
            var mail = await GetUserEmail();
            this.DoThreadSafe(new Action(() =>
            {
                try
                {
                    ChummerHub.Client.Properties.Settings.Default.Reload();
                    this.tbTempDownloadPath.Text = ChummerHub.Client.Properties.Settings.Default.TempDownloadPath;

                    if (!String.IsNullOrEmpty(mail))
                    {
                        this.lUsername.Text = mail;
                        //also, since we are logged in in now, refresh the frmCharacterRoster!
                        PluginHandler.MainForm?.DoThreadSafe(() =>
                        {
                            PluginHandler.MainForm.CharacterRoster.LoadCharacters(true, true, true, true);
                        });
                        this.bLogin.Text = "Logout";
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        this.cbRoles.DataSource = bs;
                        HideWebBrowser();
                    }
                    else
                    {
                        this.bLogin.Text = "Login";
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        this.cbRoles.DataSource = bs;
                    }
                }
                catch(Exception ex)
                {
                    Log.Warn(ex);

                }
                
            }));
            
        }

        public async Task<string> GetUserEmail()
        {
            try
            {
                this.UseWaitCursor = true;
                var client = StaticUtils.GetClient();
                if (client == null)
                    return null;
                var result = client.GetUserByAuthorizationWithHttpMessagesAsync();
                await result;
                var user = result.Result.Body;
                if (user != null)
                {
                    Properties.Settings.Default.UserEmail = user.MyApplicationUser.Email;
                    Properties.Settings.Default.Save();
                    return user.MyApplicationUser.Email;
                }
                return null;
            }
            catch (Microsoft.Rest.SerializationException ex)
            {
                this.LoginStatus = false;
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            finally
            {
                this.UseWaitCursor = false;
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
                var t = StartSTATask(
                      async () =>
                      {
                          try
                          {
                              var client = StaticUtils.GetClient();
                              var signout = client.LogoutWithHttpMessagesAsync().Result;
                              if (signout.Response.StatusCode != HttpStatusCode.OK)
                              {
                                  var roles = GetRolesStatus(this).Result;
                              }
                              else
                              {
                                  StaticUtils.UserRoles = null;
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
                ShowWebBrowser();
            }
            
        }

        private void HideWebBrowser()
        {
            frmWebBrowser?.Hide();
        }

        private frmWebBrowser frmWebBrowser = null;
        
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
                        frmWebBrowser.ShowDialog();
                        var t = StartSTATask(
                        async () =>
                        {
                            var roles = await GetRolesStatus(this);
                            UpdateDisplay();
                        });
                    })
                    );
                }
                else
                {
                    frmWebBrowser.ShowDialog();
                    var t = StartSTATask(
                           async () =>
                           {
                               var roles = await GetRolesStatus(this);
                               UpdateDisplay();
                           });
                }
               
                this.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }


       

        private async Task<IList<String>> GetRolesStatus(UserControl sender)
        {
            try
            {
                using (new CursorWait(true, sender))
                {
                    var client = StaticUtils.GetClient();
                    if (client == null)
                        return StaticUtils.UserRoles;
                    var myresult = await client.GetRolesWithHttpMessagesAsync();

                    PluginHandler.MainForm.DoThreadSafe(new Action(() =>
                    {
                        Backend.Utils.HandleError(myresult, myresult.Body);
                        if (myresult.Body.CallSuccess == true)
                        {
                            StaticUtils.UserRoles = myresult.Body.Roles.ToList();
                            if (StaticUtils.UserRoles != null && StaticUtils.UserRoles.Any())
                            {
                                this.LoginStatus = true;
                            }
                            StaticUtils.PossibleRoles = myresult.Body.PossibleRoles.ToList();
                        }
                        //bBackup.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        //bRestore.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        this.cbRoles.DataSource = bs;
                    }));
                }
                return StaticUtils.UserRoles;
            }
            catch(Microsoft.Rest.SerializationException ex)
            {
                this.LoginStatus = false;
                return null;
            }
            catch(TaskCanceledException ex)
            {
                Log.Info(ex);
                
            }
            catch(Exception ex)
            {
                Log.Warn(ex);
                
            }
            return null;
            
        }

        private void cbVisibilityIsGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void OptionsUpdate()
        {
            Properties.Settings.Default.TempDownloadPath = this.tbTempDownloadPath.Text;
            Properties.Settings.Default.VisibilityIsPublic = this.cbVisibilityIsPublic.Checked;
            if (this.rbListUserMode.SelectedIndex <= 0)
                Properties.Settings.Default.UserModeRegistered = false;
            else
                Properties.Settings.Default.UserModeRegistered = true;
            Properties.Settings.Default.Save();
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
            OpenFileDialog thisDialog = new OpenFileDialog();

            thisDialog.Filter = "Chummer files (*.chum5)|*.chum5|All files (*.*)|*.*";
            thisDialog.FilterIndex = 1;
            thisDialog.RestoreDirectory = true;
            thisDialog.Multiselect = true;
            thisDialog.Title = "Please Select Chummer File(s) for Batch-Upload";

            if (thisDialog.ShowDialog() != DialogResult.OK) return;
            foreach (var file in thisDialog.FileNames)
            {
                try
                {
                    Log.Trace("Loading: " + file);
                    var c = new Character { FileName = file };
                    using (frmLoading frmLoadingForm = new frmLoading {CharacterFile = file})
                    {
                        frmLoadingForm.Reset(36);
                        frmLoadingForm.TopMost = true;
                        frmLoadingForm.Show();
                        if (!(await c.Load(frmLoadingForm, false)))
                            continue;
                        Log.Trace("Character loaded: " + c.Name);
                    }
                    frmCharacterRoster.CharacterCache myCharacterCache = new frmCharacterRoster.CharacterCache(file);
                    CharacterExtended ce = new CharacterExtended(c, null, null, myCharacterCache);
                    await ce.Upload(null);
                }
                catch (Exception ex)
                {
                    string msg = "Exception while loading " + file + ":";
                    msg += Environment.NewLine + ex.ToString();
                    Log.Warn(msg);
                    /* run your code here */
                    MessageBox.Show(msg);
                 
                }
            }

            MessageBox.Show("Upload of " + thisDialog.FileNames.Length + " files finished (successful or not - its over).");
        }

    
        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            ucSINnersOptions.UploadOnSave = cbUploadOnSave.Checked;

        }

        private void bBackup_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                BackupTask(folderBrowserDialog1).ContinueWith((a) =>
                {
                    MessageBox.Show(a.Status.ToString());
                });
            }

        }

        private async Task BackupTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            try
            {
                using (new CursorWait(true, this))
                {
                    var client = StaticUtils.GetClient();
                    var getsinnertask = client.AdminGetSINnersWithHttpMessagesAsync();
                    await getsinnertask;
                    using (new CursorWait(true, this))
                    {
                        foreach (var sinner in getsinnertask.Result.Body)
                        {
                            try
                            {
                                if (!sinner.SiNnerMetaData.Tags.Any())
                                {
                                    Log.Error("Sinner " + sinner.Id + " has no Tags!");
                                    continue;
                                }
                                string jsonsinner = Newtonsoft.Json.JsonConvert.SerializeObject(sinner);
                                string filePath = Path.Combine(folderName, sinner.Id.ToString() + ".chum5json");
                                if (File.Exists(filePath))
                                    File.Delete(filePath);
                                File.WriteAllText(filePath, jsonsinner);
                                Log.Info("Sinner " + sinner.Id + " saved to " + filePath);
                            }
                            catch (Exception e2)
                            {
                                Log.Error(e2);
                                Invoke(new Action(() => MessageBox.Show(e2.Message)));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex); 
                Invoke(new Action(() => MessageBox.Show(ex.Message)));

            }
        }

        private void bRestore_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                RestoreTask(folderBrowserDialog1).ContinueWith((a) =>
                {
                    MessageBox.Show(a.Status.ToString());
                });
            }
        }

        private async Task RestoreTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            try
            {
                DirectoryInfo d = new DirectoryInfo(folderName);//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.chum5json"); //Getting Text files
                foreach (FileInfo file in Files)
                {
                    try
                    {
                        using (new CursorWait(true, this))
                        {
                            string sinjson = File.ReadAllText(file.FullName);
                            SINner sin = Newtonsoft.Json.JsonConvert.DeserializeObject<SINner>(sinjson);
                            UploadInfoObject uploadInfoObject = new UploadInfoObject();
                            uploadInfoObject.Client = PluginHandler.MyUploadClient;
                            uploadInfoObject.UploadDateTime = DateTime.Now;
                            uploadInfoObject.SiNners = new List<SINner>
                                {
                                    sin
                                };
                            var client = StaticUtils.GetClient();
                            var posttask = client.PostSINWithHttpMessagesAsync(uploadInfoObject);
                            await posttask;

                            if (posttask.Result.Response.IsSuccessStatusCode)
                            {
                                Log.Info("SINner " + sin.Id + " posted!");
                            }
                            else
                            {
                                string msg = posttask.Result.Response.ReasonPhrase + ": " + Environment.NewLine;
                                var content = posttask.Result.Response.Content.ReadAsStringAsync().Result;
                                msg += content;
                                Log.Warn("SINner " + sin.Id + " not posted: " + msg);
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                        Log.Error(ex,"Could not read file " + file.FullName + ": " );
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Invoke(new Action(() => MessageBox.Show(ex.Message)));

            }
        }

        private void BTempPathBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    this.tbTempDownloadPath.Text = fbd.SelectedPath;
                    OptionsUpdate();
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void BEditDefaultVisibility_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default.UserEmail = mail;
            //bool createVis = false;
            //if (String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
            //{
            //    createVis = true;
            //}
            //else
            //{
            //    SINnerVisibility vis =
            //        Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings
            //            .Default.SINnerVisibility);
            //    bool found = false;
            //    foreach (var ur in vis.UserRights)
            //    {
            //        if (ur.EMail.ToLowerInvariant() == mail.ToLowerInvariant())
            //        {
            //            ur.CanEdit = true;
            //            found = true;
            //            break;
            //        }
            //    }

            //    if (!found)
            //    {
            //        createVis = true;
            //    }
            //}

            //if (createVis)
            //{

            //    SINnerVisibility vis = SINnersOptions.SINnerVisibility;
            //    if (vis == null)
            //    {
            //        vis = new SINnerVisibility();
            //        vis.Id = Guid.NewGuid();
            //        vis.IsGroupVisible = true;
            //        vis.IsPublic = true;
            //        vis.UserRights = new List<SINerUserRight>();

            //        SINnersOptions.SINnerVisibility = null;
            //        SINnersOptions.SINnerVisibility = vis;
            //    }

            //    var foundseq = from a in vis.UserRights.ToList()
            //        where a.EMail.ToLowerInvariant() == mail.ToLowerInvariant()
            //        select a;
            //    if (!foundseq.Any())
            //    {
            //        SINerUserRight ur = new SINerUserRight();
            //        ur.Id = Guid.NewGuid();
            //        ur.EMail = mail;
            //        ur.CanEdit = true;
            //        vis.UserRights.Add(ur);
            //    }

            //    SINnersOptions.SINnerVisibility = vis;
            //    Properties.Settings.Default.SINnerVisibility =
            //        Newtonsoft.Json.JsonConvert.SerializeObject(vis);
            //    Properties.Settings.Default.Save();
            //    FillVisibilityListBox();
            //}


            var visfrm = new frmSINnerVisibility();
            visfrm.MyVisibility = SINnerVisibility;
            var result = visfrm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                SINnerVisibility = visfrm.MyVisibility;
            }
        }

        private void RbListUserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.rbListUserMode.SelectedIndex <= 0)
                this.tlpOptions.Enabled = false;
            else
                this.tlpOptions.Enabled = true;
            OptionsUpdate();
        }

        private void BRegisterUriScheme_Click(object sender, EventArgs e)
        {
            if (StaticUtils.RegisterChummerProtocol(null))
                MessageBox.Show("Url is registered!");
            else
            {
                MessageBox.Show("Url is NOT registered!");
            }
        }

       
    }
}
