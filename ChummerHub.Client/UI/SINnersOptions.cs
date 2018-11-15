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
//using Nemiro.OAuth;
//using Nemiro.OAuth.LoginForms;

namespace ChummerHub.Client.UI
{
    public partial class SINnersOptions : UserControl
    {
        public SINnersOptions()
        {
            InitializeComponent();
            //OAuthManager.RegisterClient
            //(
            //"google",
            //"408317291648-kgrn9i17g9fbsoqc6r93tp0f25t1ccbb.apps.googleusercontent.com",
            //"hpMvvvCVV_D7ULV-zGKjUWO9",
            //"https://www.googleapis.com/auth/drive"
            //// for details please visit https://developers.google.com/drive/web/scopes
            //);
        }

        private void bLogin_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                DiscoveryResponse disco;
                string path = "";
#if DEBUG
                path = "localhost:5000";
#else
                 path = "sinners.azurewebsites.net";
#endif
                disco = DiscoveryClient.GetAsync("http://" + path).Result;
                if (disco.IsError)
                {
                    MessageBox.Show(disco.Error);
                    return;
                }

                // request token
                //var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
                //var tokenResponse = tokenClient.RequestClientCredentialsAsync("api1").Result;

                // request token
                var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
                var tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1").Result;


                if (tokenResponse.IsError)
                {
                    MessageBox.Show(tokenResponse.Error);
                    return;
                }

                MessageBox.Show(tokenResponse.Json.ToString());

                // call api
                var client = new HttpClient();
                client.SetBearerToken(tokenResponse.AccessToken);
                string tokenpath = "http://" + path + "/api/v1/Identity";
                var response = client.GetAsync(tokenpath).Result;
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(JArray.Parse(content).ToString());
                }

                //var glogin = new Nemiro.OAuth.LoginForms.GoogleLogin
                //(
                //  "408317291648-kgrn9i17g9fbsoqc6r93tp0f25t1ccbb.apps.googleusercontent.com",
                //  "hpMvvvCVV_D7ULV-zGKjUWO9",
                //  "https://oauthproxy.nemiro.net/", false, false

                //);
                //var ilogin = new InstagramLogin
                //(
                //  "9fcad1f7740b4b66ba9a0357eb9b7dda",
                //  "3f04cbf48f194739a10d4911c93dcece",
                //  "http://oauthproxy.nemiro.net/"
                //);
                //ilogin.Owner = this.ParentForm;
                //var result = ilogin.ShowDialog();
                //// authorization is success
                //if (ilogin.IsSuccessfully)
                //{
                //    // use the access token for requests to API
                //    MessageBox.Show(ilogin.AccessToken.Value);
                //}
                ////string url = OAuthWeb.GetAuthorizationUrl("google");
                ////var result = OAuthWeb.VerifyAuthorization(url);
                ////if (result.IsSuccessfully)
                ////{
                ////    var user = result.UserInfo;
                ////}


            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }

        }
    }
}
