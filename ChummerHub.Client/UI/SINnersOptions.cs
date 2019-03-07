using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdentityModel.Client;
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
        public IList<String> Roles = null;

        private static SINners.Models.SINnerVisibility _SINnerVisibility = null;
        public static SINners.Models.SINnerVisibility SINnerVisibility
        {
            get
            {
                if (_SINnerVisibility != null)
                    return _SINnerVisibility;
                //Properties.Settings.Default.Reload();
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
                    Properties.Settings.Default.SINnerVisibility = null;
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

        private void InitializeMe()
        {
            cbSINnerUrl.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
            Properties.Settings.Default.Reload();
            var sinnerurl = Properties.Settings.Default.SINnerUrl;
            this.cbSINnerUrl.DataSource = Properties.Settings.Default.SINnerUrls;
            this.cbSINnerUrl.SelectedItem = sinnerurl;
            this.cbVisibilityIsPublic.BindingContext = new BindingContext();
            var t = StartSTATask(
                async () =>
                {
                    var roles = await GetRolesStatus();
                    UpdateDisplay();
                    if(!roles.Any())
                        ShowWebBrowser();
                });
            //var t = Task.Run(
            //    async () =>
            //    {
            //        await GetRolesStatus();
            //        UpdateDisplay();
            //    });
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
            if (StaticUtils.Client != null)
                StaticUtils.Client = null;
            this.bLogin.Text = "Logout";
            this.labelAccountStatus.Text = "logged out";
            this.labelAccountStatus.ForeColor = Color.DarkRed;
            this.LoginStatus = false;
            InitializeMe();

        }

        

        public async void UpdateDisplay()
        {
            PluginHandler.MainForm.DoThreadSafe(new Action(() =>
            {
                try
                {
                    if(LoginStatus == true)
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
                                    SINnerVisibility vis = new SINnerVisibility();
                                    vis.Id = Guid.NewGuid();
                                    vis.IsGroupVisible = true;
                                    vis.IsPublic = true;
                                    vis.UserRights = new List<SINerUserRight>();
                                    SINerUserRight ur = new SINerUserRight();
                                    ur.Id = Guid.NewGuid();
                                    ur.EMail = mail;
                                    ur.CanEdit = true;
                                    vis.UserRights.Add(ur);
                                    SINnersOptions.SINnerVisibility = null;
                                    SINnersOptions.SINnerVisibility = vis;
                                    Properties.Settings.Default.SINnerVisibility = Newtonsoft.Json.JsonConvert.SerializeObject(vis);
                                    Properties.Settings.Default.Save();
                                    FillVisibilityListBox();
                                }
                                    
                            }
                                
                        });
                        
                        this.bLogin.Text = "Logout";
                        string status = Roles.Aggregate((a, b) => a + ", " + b);
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
                var result = StaticUtils.Client.GetUserByAuthorizationWithHttpMessagesAsync();
                await result;
                var user = result.Result.Body;
                if (user != null)
                {
                    Properties.Settings.Default.UserEmail = user.Email;
                    Properties.Settings.Default.Save();
                    return user.Email;
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
                              var signout = StaticUtils.Client.LogoutWithHttpMessagesAsync().Result;
                              if (signout.Response.StatusCode != HttpStatusCode.OK)
                              {
                                  var roles = GetRolesStatus().Result;
                              }
                              else
                              {
                                  Roles = new List<String>();
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
                            var roles = await GetRolesStatus();
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
                               var roles = await GetRolesStatus();
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


       

        private async Task<IList<String>> GetRolesStatus()
        {
            try
            {
                this.UseWaitCursor = true;
                var myresult = StaticUtils.Client.GetRolesWithHttpMessagesAsync().Result;
                //var result = StaticUtils.Client.GetRolesWithHttpMessagesAsync();
                //await result;
                var roles = myresult.Body as IList<string>; //result.Result.Body as IList<string>;
                if (roles != null && roles.Any())
                {
                    this.LoginStatus = true;
                    Roles = roles;
                }
                PluginHandler.MainForm.DoThreadSafe(new Action(() =>
                {
                    if(Roles.Contains("Administrator"))
                    {

                        bBackup.Visible = true;
                    }
                    else
                    {
                        bBackup.Visible = false;
                    }

                }));
                
                return roles;
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
            finally
            {
                this.UseWaitCursor = false;
            }
            return null;
            
        }

        private void cbVisibilityIsGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void OptionsUpdate()
        {

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
                string folderName = folderBrowserDialog1.SelectedPath;
                try
                {
                    var t = StaticUtils.Client.AdminGetSINnersWithHttpMessagesAsync();
                    t.ContinueWith((getsinnertask) =>
                    {
                        foreach(var sinner in getsinnertask.Result.Body)
                        {
                            if (!sinner.SiNnerMetaData.Tags.Any())
                            {
                                System.Diagnostics.Trace.TraceError("Sinner " + sinner.Id + " has no Tags!");
                                continue;
                            }
                            string jsonsinner = Newtonsoft.Json.JsonConvert.SerializeObject(sinner);
                            string filePath = Path.Combine(folderName, sinner.Id.ToString()+ ".chum5json");
                            if(File.Exists(filePath))
                                File.Delete(filePath);
                            File.WriteAllText(filePath, jsonsinner);
                            //frmCharacterRoster.CharacterCache cache = Newtonsoft.Json.JsonConvert.DeserializeObject<frmCharacterRoster.CharacterCache>(sinner.JsonSummary);
                            //Character c = Utils.DownloadFile(sinner, cache);
                            System.Diagnostics.Trace.TraceInformation("Sinner " + sinner.Id + " saved to " + filePath);
                        }
                    });
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    Invoke(new Action(() => MessageBox.Show(ex.Message)));

                }
            }
           
        }

        private void bRestore_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog1 = new FolderBrowserDialog();

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                string folderName = folderBrowserDialog1.SelectedPath;
                try
                {
                    DirectoryInfo d = new DirectoryInfo(folderName);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.chum5json"); //Getting Text files
                    foreach(FileInfo file in Files)
                    {
                        try
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
                            var t = StaticUtils.Client.PostSINWithHttpMessagesAsync(uploadInfoObject);
                            t.ContinueWith((posttask) =>
                            {
                                if(posttask.Result.Response.IsSuccessStatusCode)
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
                            });
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Trace.TraceError("Could not read file " + file.FullName + ": " + ex.ToString());
                            continue;
                        }
                        
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    Invoke(new Action(() => MessageBox.Show(ex.Message)));

                }
            }
        }
    }
}
