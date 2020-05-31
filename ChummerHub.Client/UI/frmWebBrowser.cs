using System;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using Newtonsoft.Json;
using NLog;
using SINners.Models;

namespace ChummerHub.Client.UI
{
    public partial class frmWebBrowser : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public frmWebBrowser()
        {
            InitializeComponent();
        }

        private Uri LoginUrl
        {
            get
            {
                if(string.IsNullOrEmpty(Settings.Default.SINnerUrl))
                {
                    Settings.Default.SINnerUrl = "https://sinners.azurewebsites.net/";
                    string msg = "if you are (want to be) a Beta-Tester, change this to http://sinners-beta.azurewebsites.net/!";
                    Log.Warn(msg);
                    Settings.Default.Save();
                }
                string path = Settings.Default.SINnerUrl.TrimEnd('/');

                path += "/Identity/Account/Login?returnUrl=/Identity/Account/Manage";
                return new Uri(path);
            }
        }

        private void frmWebBrowser_Load(object sender, EventArgs e)
        {
            Invoke((Action)(() =>
                {
                    SuspendLayout();
                    webBrowser2.Navigated += webBrowser2_Navigated;
                    webBrowser2.ScriptErrorsSuppressed = true;
                    webBrowser2.Navigate(LoginUrl);
                    BringToFront();
                })
                );
        }

        private bool login;

        private async void webBrowser2_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if(e.Url.AbsoluteUri == LoginUrl.AbsoluteUri)
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

                    using (var user = await client.GetUserByAuthorizationWithHttpMessagesAsync().ConfigureAwait(true))
                    {
                        if (user.Body?.CallSuccess == true)
                        {
                            if (user.Body != null)
                            {
                                login = true;
                                SINnerVisibility tempvis;
                                if (!string.IsNullOrEmpty(Settings.Default.SINnerVisibility))
                                {
                                    tempvis = JsonConvert.DeserializeObject<SINnerVisibility>(Settings.Default.SINnerVisibility);
                                }
                                else
                                {
                                    tempvis = new SINnerVisibility
                                    {
                                        IsGroupVisible = true,
                                        IsPublic = true
                                    };
                                }

                                tempvis.AddVisibilityForEmail(user.Body.MyApplicationUser?.Email);
                                Close();
                            }
                            else
                            {
                                login = false;
                            }
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
                    Settings.Default.CookieData = null;
                    Settings.Default.Save();
                    var cookies =
                        StaticUtils.AuthorizationCookieContainer?.GetCookies(new Uri(Settings.Default
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
