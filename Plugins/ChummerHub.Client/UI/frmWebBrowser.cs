/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
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
            SuspendLayout();
            try
            {
                webBrowser2.Navigated += webBrowser2_Navigated;
                webBrowser2.ScriptErrorsSuppressed = true;
                webBrowser2.Navigate(LoginUrl);
                BringToFront();
            }
            finally
            {
                ResumeLayout();
            }
        }

        private bool login;

        private async void webBrowser2_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url == null)
                return;
            if(e.Url.AbsoluteUri == LoginUrl.AbsoluteUri)
                return;
            if(e.Url.AbsoluteUri.Contains("/Identity/Account/Logout"))
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
                        await Program.MainForm.DoThreadSafeAsync(x =>
                        {
                            SINnerVisibility tempvis = Backend.Utils.DefaultSINnerVisibility
                                                       ?? new SINnerVisibility
                                                       {
                                                           IsGroupVisible = true,
                                                           IsPublic = true
                                                       };
                            tempvis.AddVisibilityForEmail(body.MyApplicationUser?.Email);
                            x.Close();
                        }).ConfigureAwait(false);
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
                using (CursorWait.New(this, true))
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
