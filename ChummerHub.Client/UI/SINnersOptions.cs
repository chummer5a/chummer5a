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
//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class SINnersOptions : UserControl
    {
        private bool? LoginStatus = null;
        public IList<String> Roles = null;

        public static SINners.Models.SINnerVisibility SINnerVisibility
        {
            get
            {
                Properties.Settings.Default.Reload();
                if (String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                    return new SINners.Models.SINnerVisibility()
                    {   IsPublic = true, IsGroupVisible = true,
                        IsVisibleToUserGuids = new List<Guid?>(),
                        CanEditClientGuids = new List<Guid?>(),
                        CanEditUserGuids = new List<Guid?>(),
                        Groupname = "",
                        SiNnerVisibilityId = Guid.Empty };
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.ToString());
                }
                return new SINners.Models.SINnerVisibility()
                {
                    IsPublic = true,
                    IsGroupVisible = true,
                    IsVisibleToUserGuids = new List<Guid?>(),
                    CanEditClientGuids = new List<Guid?>(),
                    CanEditUserGuids = new List<Guid?>(),
                    Groupname = "",
                    SiNnerVisibilityId = Guid.Empty
                };
            }
            set
            {
                if (value != null)
                {
                    Properties.Settings.Default.SINnerVisibility = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                }
                else
                    Properties.Settings.Default.SINnerVisibility = null;

                Properties.Settings.Default.Save();
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
            Properties.Settings.Default.Reload();
            this.cbSINnerUrl.DataSource = Properties.Settings.Default.SINnerUrls;
            this.cbSINnerUrl.SelectedItem = Properties.Settings.Default.SINnerUrl;
            this.cbVisibilityIsPublic.BindingContext = new BindingContext();
            UpdateDisplay();
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
            
        }

        private async void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Properties.Settings.Default.Save();
            if (StaticUtils.Client != null)
                StaticUtils.Client = null;
            
        }

        

        public async void UpdateDisplay()
        {
            try
            {
                this.UseWaitCursor = true;
                this.tlpOptions.Enabled = false;
                Roles = await GetLoginStatus();
                if (LoginStatus == true)
                {
                    this.bLogin.Enabled = false;
                    //this.bLogout.Enabled = true;
                    this.labelAccountStatus.Text = Roles.Aggregate((a, b) => a + ", " + b);// "logged in";
                    this.labelAccountStatus.ForeColor = Color.DarkGreen;
                    HideWebBrowser();
                    this.cbVisibilityIsPublic.Checked       = SINnersOptions.SINnerVisibility.IsPublic.Value;
                    this.cbVisibilityIsGroupVisible.Checked = SINnersOptions.SINnerVisibility.IsGroupVisible.Value;
                    this.lbVisibilityToUsers.DataSource = SINnersOptions.SINnerVisibility.IsVisibleToUsers;
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
                this.UseWaitCursor = false;
            }
        }

        private void bLogin_ClickAsync(object sender, EventArgs e)
        {
            ShowWebBrowser(LoginUrl);
        }

        private void HideWebBrowser()
        {
            if (webBrowser1.Document == null)
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

        private async Task<IList<String>> GetLoginStatus()
        {
            try
            {
                var result = StaticUtils.Client.GetRolesWithHttpMessagesAsync();
                await result;
                var roles = result.Result.Body as IList<string>;
                if (roles != null && roles.Any())
                {
                    this.LoginStatus = true;
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
            return null;
            
        }

        private void cbVisibilityIsGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            VisibilityUpdate();
        }

        private void VisibilityUpdate()
        {

            SINnersOptions.SINnerVisibility.IsPublic = this.cbVisibilityIsPublic.Checked      ;
            SINnersOptions.SINnerVisibility.IsGroupVisible  = this.cbVisibilityIsGroupVisible.Checked;
            //SINnersOptions.SINnerVisibility.Groupname = this.tbGroupname.Text;
            //SINnersOptions.SINnerVisibility.IsVisibleToUserList = this.lbVisibilityToUsers.DataSource;
            Properties.Settings.Default.Save();
        }

        private void cbVisibilityIsPublic_CheckedChanged(object sender, EventArgs e)
        {
            VisibilityUpdate();
        }

        private void tbGroupname_TextChanged(object sender, EventArgs e)
        {
            VisibilityUpdate();
        }

        private void bVisibilityAddEmail_Click(object sender, EventArgs e)
        {
            string email = this.tbVisibilityAddEmail.Text;
            //StaticUtils.Client.Get
            //SINnersOptions.SINnerVisibility.IsVisibleToUserList.Add()
        }

        //private async void bLogout_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var result = StaticUtils.Client.LogoutWithHttpMessagesAsync();
        //        await result;
        //        if (result.Result.Body.Value == true)
        //        {
        //            this.LoginStatus = false;
        //        }
        //        else
        //        {
        //            this.LoginStatus = null;
        //        }
        //        StaticUtils.AuthorizationCookieContainer = null;
        //        //delete old cookie settings
        //        Properties.Settings.Default.CookieData = null;
        //        Properties.Settings.Default.Save();
        //        StaticUtils.Client = null;

        //    }
        //    catch (Microsoft.Rest.SerializationException ex)
        //    {
        //        this.LoginStatus = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Trace.TraceWarning(ex.ToString());
        //    }
        //    UpdateDisplay();
        //}

        //private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        //{
        //    UpdateDisplay();
        //    if (LoginStatus == true)
        //    {
        //        return;
        //    }
        //}


    }
}
