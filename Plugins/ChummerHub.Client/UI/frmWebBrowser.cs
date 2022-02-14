using System;
using System.Net;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using ChummerHub.Client.Sinners;
using NLog;


namespace ChummerHub.Client.UI
{
    public partial class frmWebBrowser : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public frmWebBrowser()
        {
            InitializeComponent();
        }

        private static Uri LoginUrl
        {
            get
            {
                if(string.IsNullOrEmpty(Settings.Default.SINnerUrl))
                {
                    Settings.Default.SINnerUrl = "https://chummer-stable.azurewebsites.net/";
                    Log.Warn("if you are (want to be) a Beta-Tester, change this to http://chummer-beta.azurewebsites.net/!");
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
                    SinnersClient client = GetCookieContainer();
                    if (client == null)
                    {
                        Log.Error("Cloud not create an instance of SINnersclient!");
                        login = false;
                        return;
                    }
                    ResultAccountGetUserByAuthorization body = await client.GetUserByAuthorizationAsync().ConfigureAwait(false);
                    if (body?.CallSuccess == true)
                    {
                        login = true;
                        Program.MainForm.Invoke(new Action(() =>
                        {
                            SINnerVisibility tempvis = Backend.Utils.DefaultSINnerVisibility
                                                       ?? new SINnerVisibility
                                                       {
                                                           IsGroupVisible = true,
                                                           IsPublic = true
                                                       };
                            tempvis.AddVisibilityForEmail(body.MyApplicationUser?.Email);
                            Close();
                        }));
                    }
                    else
                    {
                        login = false;
                    }
                }
                catch(ApiException ae)
                {
                    Log.Info(ae);
                    throw;
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    throw;
                }
            }
        }

        private SinnersClient GetCookieContainer()
        {
            try
            {
                using (new CursorWait(this, true))
                {
                    Settings.Default.CookieData = null;
                    Settings.Default.Save();
                    CookieCollection cookies =
                        StaticUtils.AuthorizationCookieContainer?.GetCookies(new Uri(Settings.Default
                            .SINnerUrl));
                    return StaticUtils.GetClient(true);
                }
            }
            catch(Exception ex)
            {
                Log.Warn(ex);
            }
            return null;
        }

        private void FrmWebBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!login)
                GetCookieContainer();
        }
    }
}
