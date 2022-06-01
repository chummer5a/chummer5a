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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Backend;
using ChummerHub.Client.Properties;
using Microsoft.Rest;
//using Newtonsoft.Json;
using ChummerHub.Client.Sinners;
using GroupControls;
using NLog;
using Utils = ChummerHub.Client.Backend.Utils;
using ChummerHub.Client.OidcClient;
using IdentityModel.OidcClient;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json;
using System.Web;

//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersOptions : UserControl
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private bool? LoginStatus;

        private static SINnerVisibility _SINnerVisibility;
        public static SINnerVisibility SINnerVisibility
        {
            get
            {
                if (_SINnerVisibility != null)
                    return _SINnerVisibility;
                Settings.Default.Reload();
                SINnerUserRight ur = new SINnerUserRight
                {
                    EMail = !string.IsNullOrEmpty(Settings.Default.UserEmail)
                        ? Settings.Default.UserEmail
                        : "delete.this.and.add@your.mail",
                    CanEdit = true,
                    Id = Guid.NewGuid()
                };
                return _SINnerVisibility = Utils.DefaultSINnerVisibility ?? new SINnerVisibility
                {
                    Id = Guid.NewGuid(),
                    IsPublic = Settings.Default.VisibilityIsPublic,
                    UserRights = new List<SINnerUserRight>
                    {
                        ur
                    }
                };
            }
            set
            {
                _SINnerVisibility = value;
                if (value == null)
                    Utils.DefaultSINnerVisibility = null;
            }
        }

        public ucSINnersOptions()
        {
            InitializeComponent();
            InitializeMe(true).GetAwaiter().GetResult();
        }

        public static bool UploadOnSave
        {
            get => Settings.Default.UploadOnSave;
            set
            {
                Settings.Default.UploadOnSave = value;
                Settings.Default.Save();
            }
        }

        private bool IsLoading;

        private async Task InitializeMe(bool blnSync)
        {
            if (IsLoading)
                return;
            IsLoading = true;
            string tip = "Milestone builds always user sinners." + Environment.NewLine + "Nightly builds always user sinners-beta.";
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x =>
                {
                    x.SetToolTip(tip);
                    x.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
                });
            }
            else
            {
                await cbSINnerUrl.DoThreadSafeAsync(x =>
                {
                    x.SetToolTip(tip);
                    x.SelectedValueChanged -= CbSINnerUrl_SelectedValueChanged;
                });
            }

            Settings.Default.Reload();
            if (string.IsNullOrEmpty(Settings.Default.TempDownloadPath))
            {
                Settings.Default.TempDownloadPath = Path.GetTempPath();
                Settings.Default.Save();
            }
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                tbTempDownloadPath.DoThreadSafe(x =>
                {
                    x.Text = Settings.Default.TempDownloadPath;
                    x.SetToolTip(
                        "Where should chummer download the temporary files from the WebService?");
                });
            }
            else
            {
                await tbTempDownloadPath.DoThreadSafeAsync(x =>
                {
                    x.Text = Settings.Default.TempDownloadPath;
                    x.SetToolTip(
                        "Where should chummer download the temporary files from the WebService?");
                });
            }

            SinnersClient client = blnSync
                ? this.DoThreadSafeFunc(() => StaticUtils.GetClient())
                : await this.DoThreadSafeFuncAsync(() => StaticUtils.GetClient());
            if (client == null)
            {
                return;
            }
            string sinnerurl = client.BaseUrl;
            Settings.Default.SINnerUrls.Clear();
            Settings.Default.Save();
#if DEBUG
            Settings.Default.SINnerUrls.Add("https://localhost:64939/");
#endif
            Settings.Default.SINnerUrls.Add("https://chummer-stable.azurewebsites.net/");
            Settings.Default.SINnerUrls.Add("https://chummer-beta.azurewebsites.net/");
            Settings.Default.Save();
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x =>
                {
                    x.DataSource = Settings.Default.SINnerUrls;
                    x.SelectedItem = sinnerurl ?? Settings.Default.SINnerUrls[0];
                    x.Enabled = true;
                });
                // ReSharper disable once MethodHasAsyncOverload
                cbIgnoreWarnings.DoThreadSafe(
                    x => x.Checked = Settings.Default.IgnoreWarningsOnOpening);
                // ReSharper disable once MethodHasAsyncOverload
                cbOpenChummerFromSharedLinks.DoThreadSafe(
                    x => x.Checked = Settings.Default.OpenChummerFromSharedLinks);
                // ReSharper disable once MethodHasAsyncOverload
                rbListUserMode.DoThreadSafe(
                    x => x.SelectedIndex = Settings.Default.UserModeRegistered ? 1 : 0);
                // ReSharper disable once MethodHasAsyncOverload
                cbVisibilityIsPublic.DoThreadSafe(x =>
                {
                    x.Checked = Settings.Default.VisibilityIsPublic;
                    x.BindingContext = new BindingContext();
                });
            }
            else
            {
                await Task.WhenAll(
                    cbSINnerUrl.DoThreadSafeAsync(x =>
                    {
                        x.DataSource = Settings.Default.SINnerUrls;
                        x.SelectedItem = sinnerurl ?? Settings.Default.SINnerUrls[0];
                        x.Enabled = true;
                    }),
                    cbIgnoreWarnings.DoThreadSafeAsync(x =>
                        x.Checked = Settings.Default.IgnoreWarningsOnOpening),
                    cbOpenChummerFromSharedLinks.DoThreadSafeAsync(x =>
                        x.Checked = Settings.Default.OpenChummerFromSharedLinks),
                    rbListUserMode.DoThreadSafeAsync(x =>
                        x.SelectedIndex = Settings.Default.UserModeRegistered ? 1 : 0),
                    cbVisibilityIsPublic.DoThreadSafeAsync(x =>
                    {
                        x.Checked = Settings.Default.VisibilityIsPublic;
                        x.BindingContext = new BindingContext();
                    }));
            }

            if (StaticUtils.UserRoles?.Count == 0)
            {
                if (blnSync)
                {
                    // ReSharper disable MethodHasAsyncOverload
                    IList<string> roles = GetRolesStatus(this);
                    UpdateDisplay();
                    if (roles.Count == 0)
                        await SignIn();
                    // ReSharper restore MethodHasAsyncOverload
                }
                else
                {
                    IList<string> roles = await GetRolesStatusAsync(this);
                    await UpdateDisplayAsync();
                    if (roles.Count == 0)
                        await SignIn();
                }
            }
            else
            {
                LoginStatus = true;
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    UpdateDisplay();
                else
                    await UpdateDisplayAsync();
            }

            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                cbUploadOnSave.DoThreadSafe(x =>
                {
                    x.Checked = UploadOnSave;
                    x.CheckedChanged += cbUploadOnSave_CheckedChanged;
                });
                // ReSharper disable once MethodHasAsyncOverload
                cbSINnerUrl.DoThreadSafe(x => x.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbVisibilityIsPublic.DoThreadSafe(x => x.CheckedChanged += cbVisibilityIsPublic_CheckedChanged);
                // ReSharper disable once MethodHasAsyncOverload
                rbListUserMode.DoThreadSafe(x => x.SelectedIndexChanged += RbListUserMode_SelectedIndexChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbIgnoreWarnings.DoThreadSafe(x => x.CheckedChanged += CbIgnoreWarningsOnCheckedChanged);
                // ReSharper disable once MethodHasAsyncOverload
                cbOpenChummerFromSharedLinks.DoThreadSafe(x => x.CheckedChanged += CbOpenChummerFromSharedLinksOnCheckedChanged);
            }
            else
            {
                await Task.WhenAll(
                    cbUploadOnSave.DoThreadSafeAsync(x =>
                    {
                        x.Checked = UploadOnSave;
                        x.CheckedChanged += cbUploadOnSave_CheckedChanged;
                    }),
                    cbSINnerUrl.DoThreadSafeAsync(x =>
                        x.SelectedValueChanged += CbSINnerUrl_SelectedValueChanged),
                    cbVisibilityIsPublic.DoThreadSafeAsync(x =>
                                                               x.CheckedChanged += cbVisibilityIsPublic_CheckedChanged),
                    rbListUserMode.DoThreadSafeAsync(x =>
                                                         x.SelectedIndexChanged += RbListUserMode_SelectedIndexChanged),
                    cbIgnoreWarnings.DoThreadSafeAsync(x =>
                                                           x.CheckedChanged += CbIgnoreWarningsOnCheckedChanged),
                    cbOpenChummerFromSharedLinks.DoThreadSafeAsync(x =>
                                                                       x.CheckedChanged += CbOpenChummerFromSharedLinksOnCheckedChanged));
            }
            //AddShieldToButton(bRegisterUriScheme);
        }

        private void CbIgnoreWarningsOnCheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void CbOpenChummerFromSharedLinksOnCheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }


        ~ucSINnersOptions()
        {
            Settings.Default.Save();
        }

        private async void CbSINnerUrl_SelectedValueChanged(object sender, EventArgs e)
        {
            Settings.Default.SINnerUrl = cbSINnerUrl.SelectedValue.ToString();
            Settings.Default.Save();
            SinnersClient client = StaticUtils.GetClient();
            if (client != null)
                StaticUtils.GetClient(true);
            bLogin.Text = "Logout";
            //this.cbRoles.DataSource = null;
            LoginStatus = false;
            await InitializeMe(false);
        }

        public void UpdateDisplay()
        {
            tlpOptions.DoThreadSafe(x => x.Enabled = Settings.Default.UserModeRegistered);
            string mail = GetUserEmail();
            try
            {
                Settings.Default.Reload();
                tbTempDownloadPath.DoThreadSafe(x => x.Text = Settings.Default.TempDownloadPath);

                if (!string.IsNullOrEmpty(mail))
                {
                    lUsername.DoThreadSafe(x => x.Text = mail);
                    //also, since we are logged in in now, refresh the frmCharacterRoster!
                    if (PluginHandler.MainForm != null)
                        Chummer.Utils.RunWithoutThreadLock(
                            () => PluginHandler.MainForm.CharacterRoster.RefreshPluginNodesAsync(
                                PluginHandler.MyPluginHandlerInstance));
                    bLogin.DoThreadSafe(x => x.Text = "Logout");
                    BindingSource bs = new BindingSource
                    {
                        DataSource = StaticUtils.UserRoles
                    };
                    cbRoles.DoThreadSafe(x => x.DataSource = bs);
                    frmWebBrowser?.DoThreadSafe(x => x.Hide());
                }
                else
                {
                    bLogin.DoThreadSafe(x => x.Text = "Login");
                    BindingSource bs = new BindingSource
                    {
                        DataSource = StaticUtils.UserRoles
                    };
                    cbRoles.DoThreadSafe(x => x.DataSource = bs);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }

        public async Task UpdateDisplayAsync()
        {
            await tlpOptions.DoThreadSafeAsync(x => x.Enabled = Settings.Default.UserModeRegistered);
            string mail = await GetUserEmailAsync();
            try
            {
                Settings.Default.Reload();
                await tbTempDownloadPath.DoThreadSafeAsync(x => x.Text = Settings.Default.TempDownloadPath);

                if (!string.IsNullOrEmpty(mail))
                {
                    await lUsername.DoThreadSafeAsync(x => x.Text = mail);
                    //also, since we are logged in in now, refresh the frmCharacterRoster!
                    if (PluginHandler.MainForm != null)
                        await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodesAsync(PluginHandler.MyPluginHandlerInstance);
                    await bLogin.DoThreadSafeAsync(x => x.Text = "Logout");
                    BindingSource bs = new BindingSource
                    {
                        DataSource = StaticUtils.UserRoles
                    };
                    await cbRoles.DoThreadSafeAsync(x => x.DataSource = bs);
                    if (frmWebBrowser != null)
                        await frmWebBrowser.DoThreadSafeAsync(x => x.Hide());
                }
                else
                {
                    await bLogin.DoThreadSafeAsync(x => x.Text = "Login");
                    BindingSource bs = new BindingSource
                    {
                        DataSource = StaticUtils.UserRoles
                    };
                    await cbRoles.DoThreadSafeAsync(x => x.DataSource = bs);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }

        public string GetUserEmail()
        {
            using (CursorWait.New(this, true))
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return null;
                    ResultAccountGetUserByAuthorization result = Chummer.Utils.RunWithoutThreadLock(() => client.GetUserByAuthorizationAsync());
                    if (result == null)
                    {
                        LoginStatus = false;
                        return null;
                    }
                    string strEmail = result.MyApplicationUser.Email;
                    if (!string.IsNullOrEmpty(strEmail))
                    {
                        Settings.Default.UserEmail = strEmail;
                        Settings.Default.Save();
                    }
                    return strEmail;
                }
                catch (SerializationException)
                {
                    LoginStatus = false;
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            }
            return null;
        }

        public async Task<string> GetUserEmailAsync()
        {
            using (await CursorWait.NewAsync(this, true))
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return null;
                    ResultAccountGetUserByAuthorization result = await client.GetUserByAuthorizationAsync();
                    if (result ==  null)
                    {
                        LoginStatus = false;
                        return null;
                    }
                    string strEmail = result.MyApplicationUser.Email;
                    if (!string.IsNullOrEmpty(strEmail))
                    {
                        Settings.Default.UserEmail = strEmail;
                        Settings.Default.Save();
                    }
                    return strEmail;
                }
                catch (SerializationException)
                {
                    LoginStatus = false;
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            }
            return null;
        }

        private async void bLogin_ClickAsync(object sender, EventArgs e)
        {
            if (bLogin.Text == "Logout")
            {
                StaticUtils.AuthorizationCookieContainer = null;
                LoginStatus = false;
                bLogin.Text = "Login";
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (await client.LogoutAsync())
                    {
                        StaticUtils.UserRoles.Clear();
                    }
                    else
                    {
                        await GetRolesStatusAsync(this);
                    }

                    await UpdateDisplayAsync();
                }
                catch (Exception ex)
                {
                    Log.Warn(ex);
                }
            }
            else
            {
                Settings.Default.SINnerUrl = cbSINnerUrl.SelectedItem?.ToString();
                Settings.Default.Save();
                await SignIn();
                //await ShowWebBrowserAsync();
            }
        }


        static string _authority = "https://demo.duendesoftware.com";
        static string _api = "https://demo.duendesoftware.com/api/test";

        static IdentityModel.OidcClient.OidcClient _oidcClient;
        static HttpClient _apiClient = new HttpClient { BaseAddress = new Uri(_api) };

        private static async Task SignIn()
        {
            // create a redirect URI using an available port on the loopback address.
            // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
            var browser = new SystemBrowser();
            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = "interactive.public",
                RedirectUri = redirectUri,
                Scope = "openid profile api offline_access",
                FilterClaims = false,

                Browser = browser,
                IdentityTokenValidator = new IdentityModel.OidcClient.JwtHandlerIdentityTokenValidator(),
                RefreshTokenInnerHttpHandler = new HttpClientHandler()
            };

            _oidcClient = new IdentityModel.OidcClient.OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            _apiClient = new HttpClient(result.RefreshTokenHandler)
            {
                BaseAddress = new Uri(_api)
            };

            ShowResult(result);
            await NextSteps(result);
        }

        private static void ShowResult(LoginResult result)
        {
            if (result.IsError)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            var values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.TokenResponse.Raw);

            Console.WriteLine($"token response...");
            foreach (var item in values)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "  x...exit  c...call api   ";
            if (currentRefreshToken != null) menu += "r...refresh token   ";

            while (true)
            {
                Console.WriteLine("\n\n");

                Console.Write(menu);
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.X) return;
                if (key.Key == ConsoleKey.C) await CallApi();
                if (key.Key == ConsoleKey.R)
                {
                    var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
                    if (refreshResult.IsError)
                    {
                        Console.WriteLine($"Error: {refreshResult.Error}");
                    }
                    else
                    {
                        currentRefreshToken = refreshResult.RefreshToken;
                        currentAccessToken = refreshResult.AccessToken;

                        Console.WriteLine("\n\n");
                        Console.WriteLine($"access token:   {currentAccessToken}");
                        Console.WriteLine($"refresh token:  {currentRefreshToken ?? "none"}");
                    }
                }
            }
        }

        private static async Task CallApi()
        {
            var response = await _apiClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Console.WriteLine("\n\n");
                Console.WriteLine(json.RootElement);
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }


        private frmWebBrowser frmWebBrowser;

        private void ShowWebBrowser()
        {
            try
            {
                if (frmWebBrowser == null)
                {
                    frmWebBrowser = Chummer.Utils.RunOnMainThread(() => new frmWebBrowser());
                }
                frmWebBrowser.DoThreadSafe(x => x.ShowDialogSafe(Program.MainForm));
                GetRolesStatus(this);
                UpdateDisplay();
                ResumeLayout(false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }

        private async Task ShowWebBrowserAsync()
        {
            try
            {
                if (frmWebBrowser == null)
                {
                    frmWebBrowser = await Chummer.Utils.RunOnMainThreadAsync(() => new frmWebBrowser());
                }
                await frmWebBrowser.DoThreadSafeAsync(x => x.ShowDialogSafe(Program.MainForm));
                await GetRolesStatusAsync(this);
                await UpdateDisplayAsync();
                ResumeLayout(false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
        }

        private IList<string> GetRolesStatus(Control sender)
        {
            try
            {
                using (CursorWait.New(sender, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return StaticUtils.UserRoles;
                    ResultAccountGetRoles myresult = Chummer.Utils.RunWithoutThreadLock(() => client.GetRolesAsync());
                    Utils.ShowErrorResponseForm(myresult);
                    PluginHandler.MainForm.DoThreadSafe(() =>
                    {
                        if (myresult?.CallSuccess == true)
                        {
                            StaticUtils.UserRoles = myresult.Roles.ToList();
                            if (StaticUtils.UserRoles != null && StaticUtils.UserRoles.Count > 0)
                            {
                                LoginStatus = true;
                            }

                            StaticUtils.PossibleRoles = myresult.PossibleRoles.ToList();
                        }

                        //bBackup.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        //bRestore.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        cbRoles.DataSource = bs;
                    });
                }

                return StaticUtils.UserRoles;
            }
            catch (SerializationException)
            {
                LoginStatus = false;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                Log.Info(ex);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            finally
            {
                //myresult?.Dispose();
            }
            return null;
        }

        private async Task<IList<string>> GetRolesStatusAsync(Control sender)
        {
            try
            {
                using (await CursorWait.NewAsync(sender, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    if (client == null)
                        return StaticUtils.UserRoles;
                    ResultAccountGetRoles myresult = await client.GetRolesAsync();
                    await Utils.ShowErrorResponseFormAsync(myresult);
                    await PluginHandler.MainForm.DoThreadSafeAsync(() =>
                    {
                        if (myresult?.CallSuccess == true)
                        {
                            StaticUtils.UserRoles = myresult.Roles.ToList();
                            if (StaticUtils.UserRoles != null && StaticUtils.UserRoles.Count > 0)
                            {
                                LoginStatus = true;
                            }

                            StaticUtils.PossibleRoles = myresult.PossibleRoles.ToList();
                        }

                        //bBackup.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        //bRestore.Visible = StaticUtils.UserRoles.Contains("Administrator");
                        BindingSource bs = new BindingSource
                        {
                            DataSource = StaticUtils.UserRoles
                        };
                        cbRoles.DataSource = bs;
                    });
                }

                return StaticUtils.UserRoles;
            }
            catch (SerializationException)
            {
                LoginStatus = false;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                Log.Info(ex);
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            finally
            {
                //myresult?.Dispose();
            }
            return null;
        }

        private void cbVisibilityIsGroupVisible_CheckedChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }

        private void OptionsUpdate()
        {
            Settings.Default.TempDownloadPath = tbTempDownloadPath.Text;
            Settings.Default.VisibilityIsPublic = cbVisibilityIsPublic.Checked;
            Settings.Default.IgnoreWarningsOnOpening = cbIgnoreWarnings.Checked;
            Settings.Default.OpenChummerFromSharedLinks = cbOpenChummerFromSharedLinks.Checked;
            Settings.Default.UserModeRegistered = rbListUserMode.SelectedIndex > 0;
            Settings.Default.Save();
        }

        private void cbVisibilityIsPublic_CheckedChanged(object sender, EventArgs e)
        {

            OptionsUpdate();
        }

        private void tbGroupname_TextChanged(object sender, EventArgs e)
        {
            OptionsUpdate();
        }


        private async void bMultiUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog thisDialog = new OpenFileDialog
            {
                Filter = "Chummer files (*.chum5)|*.chum5|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true,
                Title = "Please Select Chummer File(s) for Batch-Upload"
            })
            {
                if (thisDialog.ShowDialog() != DialogResult.OK)
                    return;
                foreach (string file in thisDialog.FileNames)
                {
                    CharacterExtended objTemp = null;
                    try
                    {
                        objTemp = await Utils.UploadCharacterFromFile(file);
                    }
                    finally
                    {
                        objTemp?.Dispose();
                    }
                }

                Program.ShowMessageBox("Upload of " + thisDialog.FileNames.Length + " files finished (successful or not - its over).");
            }
        }

        private void cbUploadOnSave_CheckedChanged(object sender, EventArgs e)
        {
            UploadOnSave = cbUploadOnSave.Checked;

        }

        private async void bBackup_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    await BackupTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.ShowMessageBox(a.Status.ToString());
                    });
                }
            }
        }

        private async Task BackupTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            using (await CursorWait.NewAsync(this, true))
            {
                try
                {
                    SinnersClient client = StaticUtils.GetClient();
                    ICollection<SINner> getsinner = await client.AdminGetSINnersAsync();
                    foreach (SINner sinner in getsinner)
                    {
                        try
                        {
                            if (sinner.SiNnerMetaData.Tags.Count == 0)
                            {
                                Log.Error("Sinner " + sinner.Id + " has no Tags!");
                                continue;
                            }
                            string jsonsinner = JsonConvert.SerializeObject(sinner);
                            string filePath = Path.Combine(folderName, sinner.Id.ToString() + ".chum5json");
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                            File.WriteAllText(filePath, jsonsinner);
                            Log.Info("Sinner " + sinner.Id + " saved to " + filePath);
                        }
                        catch (Exception e2)
                        {
                            Log.Error(e2);
                            Invoke(new Action(() => Program.ShowMessageBox(e2.Message)));
                        }
                    }
                    //getsinner.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Invoke(new Action(() => Program.ShowMessageBox(ex.Message)));
                }
            }
        }

        private async void bRestore_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                // Show the FolderBrowserDialog.
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    await RestoreTask(folderBrowserDialog1).ContinueWith(a =>
                    {
                        Program.ShowMessageBox(a.Status.ToString());
                    });
                }
            }
        }

        private async Task RestoreTask(FolderBrowserDialog folderBrowserDialog1)
        {
            string folderName = folderBrowserDialog1.SelectedPath;
            try
            {
                DirectoryInfo d = new DirectoryInfo(folderName);//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.chum5json"); //Getting Text files
                using (await CursorWait.NewAsync(this, true))
                {
                    SinnersClient client = StaticUtils.GetClient();
                    foreach (FileInfo file in Files)
                    {
                        try
                        {
                            string sinjson = File.ReadAllText(file.FullName);
                            SINner sin = JsonConvert.DeserializeObject<SINner>(sinjson);
                            UploadInfoObject uploadInfoObject = new UploadInfoObject
                            {
                                Client = PluginHandler.MyUploadClient,
                                UploadDateTime = DateTime.Now,
                                SiNners = new List<SINner>
                                {
                                    sin
                                }
                            };
                            ResultSinnerPostSIN posttask = await client.PostSINAsync(uploadInfoObject);
                            if (posttask.CallSuccess)
                            {
                                Log.Info("SINner " + (sin?.Id.ToString() ?? "null") + " posted!");
                            }
                            else
                            {
                                string msg = posttask.ErrorText + ": " + Environment.NewLine;
                                string content = posttask.MyException?.ToString();

                                msg += content;
                                Log.Warn("SINner " + (sin?.Id.ToString() ?? "null") + " not posted: " + msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Could not read file " + file.FullName + ": ");
                        }
                        finally
                        {
                            //posttask?.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Program.ShowMessageBox(ex.Message);
            }
        }

        private void BTempPathBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    return;
                tbTempDownloadPath.Text = fbd.SelectedPath;
                OptionsUpdate();
                Settings.Default.Save();
            }
        }

        private void BEditDefaultVisibility_Click(object sender, EventArgs e)
        {
            using (frmSINnerVisibility visfrm = new frmSINnerVisibility
            {
                MyVisibility = SINnerVisibility
            })
            {
                if (visfrm.ShowDialog(this) != DialogResult.OK)
                    return;
                SINnerVisibility = visfrm.MyVisibility;
                Utils.DefaultSINnerVisibility = visfrm.MyVisibility;
            }
        }

        private void RbListUserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            tlpOptions.Enabled = rbListUserMode.SelectedIndex > 0;
            OptionsUpdate();
        }
    }
}
