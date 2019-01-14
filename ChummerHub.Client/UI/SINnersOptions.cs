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
                if (String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                    return _SINnerVisibility = new SINners.Models.SINnerVisibility()
                    {
                        Id = Guid.NewGuid(),
                        IsPublic = true, IsGroupVisible = true,
                        UserRights = new List<SINerUserRight>()
                        {
                            new SINerUserRight()
                            {
                                 EMail = Properties.Settings.Default.UserEmail,
                                  CanEdit = true,
                                  Id = Guid.NewGuid()
                            }
                            
                        },
                        Groupname = ""
                        };
                try
                {
                    return _SINnerVisibility = Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.ToString());
                }
                return _SINnerVisibility = new SINners.Models.SINnerVisibility()
                {
                    Id = Guid.NewGuid(),
                    IsPublic = true,
                    IsGroupVisible = true,
                    UserRights = new List<SINerUserRight>()
                        {
                             new SINerUserRight()
                            {
                                 EMail = Properties.Settings.Default.UserEmail,
                                  CanEdit = true,
                                  Id = Guid.NewGuid()
                            }
                        },
                    Groupname = ""
                    
                };
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
                    if (!roles.Any())
                        ShowWebBrowser();
                });
            //var t = Task.Run(
            //    async () =>
            //    {
            //        await GetRolesStatus();
            //        UpdateDisplay();
            //    });
            this.cbVisibilityIsPublic.Checked = SINnersOptions.SINnerVisibility.IsPublic.Value;
            this.cbVisibilityIsGroupVisible.Checked = SINnersOptions.SINnerVisibility.IsGroupVisible.Value;
            ((ListBox)clbVisibilityToUsers).DataSource = SINnersOptions.SINnerVisibility.UserRightsObservable;
            ((ListBox)clbVisibilityToUsers).DisplayMember = "EMail";
            ((ListBox)clbVisibilityToUsers).ValueMember = "CanEdit";
            for (int i = 0; i < clbVisibilityToUsers.Items.Count; i++)
            {
                SINerUserRight obj = (SINerUserRight)clbVisibilityToUsers.Items[i];
                clbVisibilityToUsers.SetItemChecked(i, obj.CanEdit.Value);
            }
            cbUploadOnSave.Checked = SINnersOptions.UploadOnSave;
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
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
            if (InvokeRequired)
            {
                Invoke((Action)UpdateDisplay);
                return;
            }
            try
            {
                if (LoginStatus == true)
                {
                    if (String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                    {
                        Properties.Settings.Default.UserEmail = GetUserEmail().Result;
                    }
                    this.bLogin.Text = "Logout";
                    string status = Roles.Aggregate((a, b) => a + ", " + b);
                    Invoke(new Action(() => labelAccountStatus.Text = status));
                    Invoke(new Action(() => this.labelAccountStatus.ForeColor = Color.DarkGreen));
                    Invoke(new Action(() => HideWebBrowser()));
                    
                    
                }
                else if (LoginStatus == false)
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
                              //if(!roles.Any())
                              //    ShowWebBrowser();
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
            SINnerVisibility.Save(null);
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
                    CharacterExtended ce = new CharacterExtended(c);
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
    }
}
