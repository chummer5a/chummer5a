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
//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class SINnersOptions : UserControl
    {
        private bool? LoginStatus = null;

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
            UpdateDisplay();
            cbSINnerUrl.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged;
        }

        private void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Properties.Settings.Default.Save();
            if (StaticUtils.Client != null)
                StaticUtils.Client = null;
        }

        

        public void UpdateDisplay()
        {
            //StaticUtils.Client
            if (LoginStatus == true)
            {
                this.bLogin.Enabled = false;
                this.bLogout.Enabled = true;
                this.labelAccountStatus.Text = "logged in";
                this.labelAccountStatus.ForeColor = Color.DarkSeaGreen;
            }
            else if (LoginStatus == false)
            {
                this.bLogin.Enabled = true;
                this.bLogout.Enabled = false;
                this.labelAccountStatus.Text = "logged out";
                this.labelAccountStatus.ForeColor = Color.DarkRed;
            }
            else
            {
                this.bLogin.Enabled = true;
                this.bLogout.Enabled = true;
                this.labelAccountStatus.Text = "unknown";
                this.labelAccountStatus.ForeColor = Color.DeepPink;
            }
        }

        private void bLogin_ClickAsync(object sender, EventArgs e)
        {
            try
            {


                webBrowser1 = new System.Windows.Forms.WebBrowser();
                this.SuspendLayout();
                // 
                // webBrowser1
                // 
                webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
                webBrowser1.Location = new System.Drawing.Point(0, 0);
                webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
                webBrowser1.Name = "webBrowser1";
                webBrowser1.Size = new System.Drawing.Size(746, 603);
                webBrowser1.TabIndex = 0;
                webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
                webBrowser1.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser1_Navigated);
                webBrowser1.ScriptErrorsSuppressed = true;

                tlpOptions.Controls.Add(webBrowser1, 0, 2);
                tlpOptions.SetColumnSpan(webBrowser1, 5);


                
                webBrowser1.Navigate(LoginUrl);
                this.ResumeLayout(false);
                UpdateDisplay();


            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }

        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.AbsoluteUri == LoginUrl)
                return;
            if (e.Url.AbsoluteUri.Contains("/Identity/Account/Manage/ExternalLogins"))
            {
                
                //delete old cookie settings
                Properties.Settings.Default.CookieData = null;
                Properties.Settings.Default.Save();
                //maybe we are logged in now
                try
                {
                    this.UseWaitCursor = true;
                    
                    //recreate cookiecontainer
                    var cookies = StaticUtils.AuthorizationCookieContainer.GetCookies(new Uri(Properties.Settings.Default.SINnerUrl));
                    StaticUtils.Client = null;
                    
                    var result = StaticUtils.Client.GetClaimsWithHttpMessagesAsync().Result;
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
            else
            {
                //we are not logged in
                this.LoginStatus = false;
            }
            
            
            UpdateDisplay();
        }

        private void bLogout_Click(object sender, EventArgs e)
        {
            string path = Properties.Settings.Default.SINnerUrl.TrimEnd('/');

            path += "/Identity/Account/Logout";
            webBrowser1.Navigate(path);
            UpdateDisplay();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            UpdateDisplay();
            if (LoginStatus == true)
            {
                return;
            }
        }


    }
}
