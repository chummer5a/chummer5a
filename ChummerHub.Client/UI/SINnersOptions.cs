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

//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class SINnersOptions : UserControl
    {
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
                        IsPublic = true,
                        IsGroupVisible = true,
                        UserRights = new List<SINerUserRight>()
                        {


                        },
                    };
                    if(!String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                    {
                        var ur = new SINerUserRight()
                        {
                            EMail = Properties.Settings.Default.UserEmail,
                            CanEdit = true,
                            Id = Guid.NewGuid()
                        };
                        _SINnerVisibility.UserRights.Add(ur);
                    }
                    else
                    {
                        var ur = new SINerUserRight()
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
                    return _SINnerVisibility = Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.ToString());
                }
                _SINnerVisibility = new SINners.Models.SINnerVisibility()
                {
                    Id = Guid.NewGuid(),
                    IsPublic = true,
                    IsGroupVisible = true,
                    UserRights = new List<SINerUserRight>()
                    {


                    }
                    
                };
                if(!String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                {
                    var ur = new SINerUserRight()
                    {
                        EMail = Properties.Settings.Default.UserEmail,
                        CanEdit = true,
                        Id = Guid.NewGuid()
                    };
                    _SINnerVisibility.UserRights.Add(ur);
                }
                else
                {
                    var ur = new SINerUserRight()
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

      
       
        public SINnersOptions()
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

        private async Task InitializeMe()
        {
            
            string tip = "Milestone builds always user sinners." + Environment.NewLine + "Nightly builds always user sinners-beta.";
            cbSINnerUrl.SetToolTip(tip);
            cbSINnerUrl.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
            Properties.Settings.Default.Reload();
            if (String.IsNullOrEmpty(Properties.Settings.Default.TempDownloadPath))
            {
                Properties.Settings.Default.TempDownloadPath = Path.GetTempPath();
                Properties.Settings.Default.Save();
            }
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
            cbSINnerUrl.Enabled = false;
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
            FillVisibilityListBox();
            cbUploadOnSave.Checked = SINnersOptions.UploadOnSave;
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
        }

        private void FillVisibilityListBox()
        {
            PluginHandler.MainForm.DoThreadSafe(new Action(() =>
            {
                try
                {
                    this.cbVisibilityIsPublic.CheckedChanged -= cbVisibilityIsPublic_CheckedChanged;
                    this.cbVisibilityIsGroupVisible.CheckedChanged -= cbVisibilityIsGroupVisible_CheckedChanged;
                    SINnersOptions.SINnerVisibility = null;
                    ((ListBox)clbVisibilityToUsers).DataSource = null;
                    if(SINnersOptions.SINnerVisibility != null)
                    {
                        if(SINnersOptions.SINnerVisibility.IsPublic != null)
                            this.cbVisibilityIsPublic.Checked = SINnersOptions.SINnerVisibility.IsPublic.Value;
                        if(SINnersOptions.SINnerVisibility.IsGroupVisible != null)
                            this.cbVisibilityIsGroupVisible.Checked = SINnersOptions.SINnerVisibility.IsGroupVisible.Value;
                        ((ListBox)clbVisibilityToUsers).DataSource = SINnersOptions.SINnerVisibility.UserRightsObservable;
                    }

                    ((ListBox)clbVisibilityToUsers).DisplayMember = "EMail";
                    ((ListBox)clbVisibilityToUsers).ValueMember = "CanEdit";
                    for(int i = 0; i < clbVisibilityToUsers.Items.Count; i++)
                    {
                        SINerUserRight obj = (SINerUserRight)clbVisibilityToUsers.Items[i];
                        clbVisibilityToUsers.SetItemChecked(i, obj.CanEdit != null && obj.CanEdit.Value);
                    }
                    clbVisibilityToUsers.Refresh();
                    this.cbVisibilityIsPublic.CheckedChanged += cbVisibilityIsPublic_CheckedChanged;
                    this.cbVisibilityIsGroupVisible.CheckedChanged += cbVisibilityIsGroupVisible_CheckedChanged;
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.Message, e);
                    Console.WriteLine(e);
                    throw;
                }

            }));
            
            
        }

        ~SINnersOptions()
        {
            SINnerVisibility.Save(null);
        }

        private async void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Properties.Settings.Default.Save();
            var client = StaticUtils.GetClient();
            if (client != null)
                StaticUtils.GetClient(true);
            this.bLogin.Text = "Logout";
            this.labelAccountStatus.Text = "logged out";
            this.labelAccountStatus.ForeColor = Color.DarkRed;
            this.LoginStatus = false;
            await InitializeMe();

        }

        

        public async void UpdateDisplay()
        {
           
            PluginHandler.MainForm.DoThreadSafe(new Action(() =>
            {
                try
                {
                    this.tbTempDownloadPath.Text = Properties.Settings.Default.TempDownloadPath;
                    if (LoginStatus == true)
                    {
                        var t = GetUserEmail();
                        t.ContinueWith((emailtask) =>
                        {
                            string mail = emailtask.Result;
                            if(!String.IsNullOrEmpty(mail))
                            {
                                Properties.Settings.Default.UserEmail = mail;
                                bool createVis = false;
                                if (String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                                {
                                    createVis = true;
                                }
                                else
                                {
                                    SINnerVisibility vis = Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                                    bool found = false;
                                    foreach(var ur in vis.UserRights)
                                    {
                                        if (ur.EMail.ToLowerInvariant() == mail.ToLowerInvariant())
                                        {
                                            ur.CanEdit = true;
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        createVis = true;
                                    }
                                }
                                if (createVis)
                                {

                                    SINnerVisibility vis = SINnersOptions.SINnerVisibility;
                                    if (vis == null)
                                    {
                                        vis = new SINnerVisibility();
                                        vis.Id = Guid.NewGuid();
                                        vis.IsGroupVisible = true;
                                        vis.IsPublic = true;
                                        vis.UserRights = new List<SINerUserRight>();
                                        
                                        SINnersOptions.SINnerVisibility = null;
                                        SINnersOptions.SINnerVisibility = vis;
                                    }
                                    var foundseq = from a in vis.UserRights.ToList()
                                        where a.EMail.ToLowerInvariant() == mail.ToLowerInvariant()
                                        select a;
                                    if (!foundseq.Any())
                                    {
                                        SINerUserRight ur = new SINerUserRight();
                                        ur.Id = Guid.NewGuid();
                                        ur.EMail = mail;
                                        ur.CanEdit = true;
                                        vis.UserRights.Add(ur);
                                    }

                                    SINnersOptions.SINnerVisibility = vis;
                                    Properties.Settings.Default.SINnerVisibility = Newtonsoft.Json.JsonConvert.SerializeObject(vis);
                                    Properties.Settings.Default.Save();
                                    FillVisibilityListBox();
                                }
                                //also, since we are logged in in now, refresh the frmCharacterRoster!
                                PluginHandler.MainForm.DoThreadSafe(() =>
                                {
                                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(true, true, true, true);
                                });
                            }
                        });
                        this.bLogin.Text = "Logout";
                        string status = StaticUtils.UserRoles.Aggregate((a, b) => a + ", " + b);
                        labelAccountStatus.Text = status;
                        labelAccountStatus.ForeColor = Color.DarkGreen;
                        HideWebBrowser();
                    }
                    else if(LoginStatus == false)
                    {
                        this.bLogin.Text = "Login";
                        this.labelAccountStatus.Text = "logged out";
                        this.labelAccountStatus.ForeColor = Color.DarkRed;
                    }
                    else
                    {
                        this.bLogin.Text = "Login";
                        this.labelAccountStatus.Text = "unknown";
                        this.labelAccountStatus.ForeColor = Color.DeepPink;
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                }
                
            }));
            
        }

        public async Task<string> GetUserEmail()
        {
            try
            {
                this.UseWaitCursor = true;
                var client = StaticUtils.GetClient();
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
                System.Diagnostics.Trace.TraceWarning(ex.ToString());
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
                              System.Diagnostics.Trace.TraceWarning(ex.ToString());
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
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
        }


       

        private async Task<IList<String>> GetRolesStatus(UserControl sender)
        {
            try
            {
                using (new CursorWait(true, sender))
                {
                    var client = StaticUtils.GetClient();
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
                        }
                        bBackup.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        bRestore.Visible = StaticUtils.UserRoles.Contains("Administrator");
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
                System.Diagnostics.Trace.TraceWarning(ex.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(ex.ToString());
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
            SINnersOptions.SINnerVisibility.IsPublic = this.cbVisibilityIsPublic.Checked      ;
            SINnersOptions.SINnerVisibility.IsGroupVisible  = this.cbVisibilityIsGroupVisible.Checked;
            SINnerVisibility.Save(this.clbVisibilityToUsers);
        }

        private void cbVisibilityIsPublic_CheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void tbGroupname_TextChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void bVisibilityAddEmail_Click(object sender, EventArgs e)
        {
            string email = this.tbVisibilityAddEmail.Text;
            AddVisibilityForEmail(email);
            //Save it!
            SINnerVisibility.Save(clbVisibilityToUsers);
        }

        public static void AddVisibilityForEmail(string email)
        {
            if(!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address!");
                return;
            }
            SINerUserRight ur = new SINerUserRight()
            {
                EMail = email,
                CanEdit = true,
                Id = Guid.NewGuid()
            };
            var found = from a in SINnersOptions.SINnerVisibility.UserRightsObservable where a.EMail.ToLowerInvariant() == email.ToLowerInvariant() select a;
            if(found.Any())
                ur = found.FirstOrDefault();
            if (!SINnersOptions.SINnerVisibility.UserRightsObservable.Contains(ur))
                SINnersOptions.SINnerVisibility.UserRightsObservable.Add(ur);
       
        }

        private void clbVisibilityToUsers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var obj = clbVisibilityToUsers.Items[e.Index];
            var userright = obj as SINerUserRight;
            if (e.NewValue == CheckState.Checked)
                userright.CanEdit = true;
            else
                userright.CanEdit = false;
        }

        private void bVisibilityRemove_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = new ListBox.SelectedObjectCollection(clbVisibilityToUsers);
            selectedItems = clbVisibilityToUsers.SelectedItems;

            if (clbVisibilityToUsers.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var userright = selectedItems[i] as SINerUserRight;
                    SINnersOptions.SINnerVisibility.UserRightsObservable.Remove(userright);
                }
                //Save it!
                SINnerVisibility.Save(clbVisibilityToUsers);
            }
            else
                MessageBox.Show("No email selected!");
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void bMultiUpload_Click(object sender, EventArgs e)
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
                    Debug.WriteLine("Loading: " + file);
                    var c = new Character { FileName = file };
                    if(!c.Load())
                        continue;
                    Debug.WriteLine("Character loaded: " + c.Name);
                    CharacterExtended ce = new CharacterExtended(c, null);
                    ce.UploadInBackground();
                }
                catch (Exception ex)
                {
                    string msg = "Exception while loading " + file + ":";
                    msg += Environment.NewLine + ex.ToString();
                    Debug.Write(msg);
                    System.Diagnostics.Trace.TraceWarning(msg);
                    throw;
                }
            }
        }

    
        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            SINnersOptions.UploadOnSave = cbUploadOnSave.Checked;

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
                                    System.Diagnostics.Trace.TraceError("Sinner " + sinner.Id + " has no Tags!");
                                    continue;
                                }
                                string jsonsinner = Newtonsoft.Json.JsonConvert.SerializeObject(sinner);
                                string filePath = Path.Combine(folderName, sinner.Id.ToString() + ".chum5json");
                                if (File.Exists(filePath))
                                    File.Delete(filePath);
                                File.WriteAllText(filePath, jsonsinner);
                                System.Diagnostics.Trace.TraceInformation("Sinner " + sinner.Id + " saved to " + filePath);
                            }
                            catch (Exception e2)
                            {
                                System.Diagnostics.Trace.TraceError(e2.ToString());
                                Invoke(new Action(() => MessageBox.Show(e2.Message)));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
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
                                System.Diagnostics.Trace.TraceInformation("SINner " + sin.Id + " posted!");
                            }
                            else
                            {
                                string msg = posttask.Result.Response.ReasonPhrase + ": " + Environment.NewLine;
                                var content = posttask.Result.Response.Content.ReadAsStringAsync().Result;
                                msg += content;
                                System.Diagnostics.Trace.TraceWarning("SINner " + sin.Id + " not posted: " + msg);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("Could not read file " + file.FullName + ": " + ex.ToString());
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
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
    }
}
