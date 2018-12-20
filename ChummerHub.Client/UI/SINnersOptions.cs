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
                    { IsPublic = true, IsGroupVisible = true,
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

        private string LoginUrl
        {
            get
            {
                string path = Properties.Settings.Default.SINnerUrl.TrimEnd('/');

                path += "/Identity/Account/Login?returnUrl=/Identity/Account/Manage/ExternalLogins";
                return path;
            }
        }
       
        private WebBrowser webBrowser1;
        public SINnersOptions()
        {
            InitializeComponent();
            InitializeMe();
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;

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

        private void InitializeMe()
        {
            Properties.Settings.Default.Reload();
            this.cbSINnerUrl.DataSource = Properties.Settings.Default.SINnerUrls;
            this.cbSINnerUrl.SelectedItem = Properties.Settings.Default.SINnerUrl;
            this.cbVisibilityIsPublic.BindingContext = new BindingContext();
            var t = Task.Run(
                async () =>
                {
                    await GetRolesStatus();
                    UpdateDisplay();
                });
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
                this.tlpOptions.Enabled = false;
                
                if (LoginStatus == true)
                {
                    if (String.IsNullOrEmpty(Properties.Settings.Default.UserEmail))
                    {
                        Properties.Settings.Default.UserEmail = await GetUserEmail();
                    }
                    this.bLogin.Enabled = false;
                    //this.bLogout.Enabled = true;
                    this.labelAccountStatus.Text = Roles.Aggregate((a, b) => a + ", " + b);// "logged in";
                    this.labelAccountStatus.ForeColor = Color.DarkGreen;
                    HideWebBrowser();
                }
                else if (LoginStatus == false)
                {
                    this.bLogin.Enabled = true;
                    //this.bLogout.Enabled = false;
                    this.labelAccountStatus.Text = "logged out";
                    this.labelAccountStatus.ForeColor = Color.DarkRed;
                    ShowWebBrowser(LoginUrl);
                }
                else
                {
                    this.bLogin.Enabled = true;
                    //this.bLogout.Enabled = true;
                    this.labelAccountStatus.Text = "unknown";
                    this.labelAccountStatus.ForeColor = Color.DeepPink;
                    ShowWebBrowser(LoginUrl);
                }
                


            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            finally
            {
                this.tlpOptions.Enabled = true;
            }
        }

        private async Task<string> GetUserEmail()
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
            ShowWebBrowser(LoginUrl);
        }

        private void HideWebBrowser()
        {
            if (webBrowser1?.Document == null)
                return;
            if (tlpOptions.Controls.Contains(webBrowser1))
            {
                tlpOptions.Controls.Remove(webBrowser1);
                webBrowser1 = null;
            }
        }
    

        private void ShowWebBrowser(string path)
        {
            try
            {
                if (tlpOptions.Controls.Contains(webBrowser1))
                    return;
                webBrowser1 = new System.Windows.Forms.WebBrowser();
                this.SuspendLayout();
                webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
                webBrowser1.Location = new System.Drawing.Point(0, 0);
                webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
                webBrowser1.Name = "webBrowser1";
                webBrowser1.Size = new System.Drawing.Size(746, 603);
                webBrowser1.TabIndex = 0;
                webBrowser1.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser1_Navigated);
                webBrowser1.ScriptErrorsSuppressed = true;
                tlpOptions.Controls.Add(webBrowser1, 0, 2);
                tlpOptions.SetColumnSpan(webBrowser1, 5);
                webBrowser1.Navigate(path);
                this.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
        }


        private async void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.AbsoluteUri == LoginUrl)
                return;
            if ((e.Url.AbsoluteUri.Contains("/Identity/Account/Manage/ExternalLogins"))
                || (e.Url.AbsoluteUri.Contains("/Identity/Account/Logout")))
            {
                
                
                //maybe we are logged in now
                try
                {
                    this.UseWaitCursor = true;
                    StaticUtils.AuthorizationCookieContainer = null;
                    //delete old cookie settings
                    Properties.Settings.Default.CookieData = null;
                    Properties.Settings.Default.Save();
                    //recreate cookiecontainer
                    var cookies = StaticUtils.AuthorizationCookieContainer.GetCookies(new Uri(Properties.Settings.Default.SINnerUrl));
                    StaticUtils.Client = null;
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceInformation(ex.ToString());
                }
                finally
                {
                    this.UseWaitCursor = false;
                }
            }
        }

        private async Task<IList<String>> GetRolesStatus()
        {
            try
            {
                this.UseWaitCursor = true;
                var result = StaticUtils.Client.GetRolesWithHttpMessagesAsync();
                await result;
                var roles = result.Result.Body as IList<string>;
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

        bool IsValidEmail(string email)
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
            if (!IsValidEmail(email))
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
            SINnersOptions.SINnerVisibility.UserRightsObservable.Add(ur);
            //Save it!
            SINnerVisibility.Save(this.clbVisibilityToUsers);
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

            if (thisDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in thisDialog.FileNames)
                {
                    try
                    {
                        Debug.WriteLine("Loading: " + file);
                        Character c = PluginHandler.MainForm.LoadCharacter(file);
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
                                var posttask = Utils.PostSINnerAsync(ce);
                                posttask.Wait();
                                var uptask = Utils.UploadChummerFileAsync(ce);
                                uptask.Wait();
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
                                var posttask = Utils.PostSINnerAsync(ce);
                                posttask.Wait();
                                var uptask = Utils.UploadChummerFileAsync(ce);
                                uptask.Wait();
                                create.Hide();
                                create.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = "Exception while loading " + file + ":";
                        msg += Environment.NewLine + ex.ToString();
                        Debug.Write(msg);
                        throw;
                    }
                }
            }
        }

        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            SINnersOptions.UploadOnSave = cbUploadOnSave.Checked;

        }
    }
}
