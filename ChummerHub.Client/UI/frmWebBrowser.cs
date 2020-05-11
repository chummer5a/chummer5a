using ChummerHub.Client.Backend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Newtonsoft.Json;
using NLog;
using SINners.Models;

namespace ChummerHub.Client.UI
{
    public partial class frmWebBrowser : Form
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public frmWebBrowser()
        {
            InitializeComponent();
        }





        private string LoginUrl
        {
            get
            {
                if(String.IsNullOrEmpty(Properties.Settings.Default.SINnerUrl))
                {
                    Properties.Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net/";
                    string msg = "if you are (want to be) a Beta-Tester, change this to http://sinners-beta.azurewebsites.net/!";
                    Log.Warn(msg);
                    Properties.Settings.Default.Save();
                }
                string path = Properties.Settings.Default.SINnerUrl.TrimEnd('/');

                path += "/Identity/Account/Login?returnUrl=/Identity/Account/Manage";
                return path;
            }
        }



        private void frmWebBrowser_Load(object sender, EventArgs e)
        {
            
                Invoke((Action)(() =>
                {
                    this.SuspendLayout();
                    webBrowser2.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser2_Navigated);
                    webBrowser2.ScriptErrorsSuppressed = true;
                    webBrowser2.Navigate(LoginUrl);
                    this.BringToFront();
                })
                );
                        
        }

        private bool login = false;

        private async void webBrowser2_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if(e.Url.AbsoluteUri == LoginUrl)
                return;
            if((e.Url.AbsoluteUri.Contains("/Identity/Account/Logout")))
            {
                //maybe we are logged in now
                GetCookieContainer();
            }
            else if (e.Url.AbsoluteUri.Contains("/Identity/Account/Manage"))
            {
                try
                {
                    //we are logged in!
                    GetCookieContainer();
                    var client = StaticUtils.GetClient();
                    if (client == null)
                    {
                        Log.Error("Cloud not create an instance of SINnersclient!");
                        return;
                    }
                    var user = await client.GetUserByAuthorizationWithHttpMessagesAsync().ConfigureAwait(true);
                    if (user.Body?.CallSuccess == true)
                    {
                        if (user.Body != null)
                        {
                            login = true;
                            SINnerVisibility tempvis;
                            if (!String.IsNullOrEmpty(Properties.Settings.Default.SINnerVisibility))
                            {
                                tempvis = JsonConvert.DeserializeObject<SINnerVisibility>(Properties.Settings.Default.SINnerVisibility);
                            }
                            else
                            {
                                tempvis = new SINnerVisibility()
                                {
                                    IsGroupVisible = true,
                                    IsPublic = true
                                };
                            }

                            tempvis.AddVisibilityForEmail(user.Body.MyApplicationUser?.Email);
                            this.Close();
                        }
                        else
                        {
                            login = false;
                        }
                    }

                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    throw;
                }

            }
        }

        private void GetCookieContainer()
        {
            try
            {
                using (new CursorWait(true, this))
                {
                    Properties.Settings.Default.CookieData = null;
                    Properties.Settings.Default.Save();
                    var cookies =
                        StaticUtils.AuthorizationCookieContainer?.GetCookies(new Uri(Properties.Settings.Default
                            .SINnerUrl));
                    var client = StaticUtils.GetClient(true);
                }
            }
            catch(Exception ex)
            {
                Log.Warn(ex);
            }
            
        }

        private void FrmWebBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (login == false)
                GetCookieContainer();
        }
    }
}
