using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Model;
using System.Net;
using Microsoft.Rest;
using System.Net.Http;
using SINners;
using ChummerHub.Client.Backend;
using System.Composition;
using Chummer.Plugins;

namespace ChummerHub.Client.UI
{
    public partial class SINnersUserControl : UserControl
    {
        
        private CharacterShared _mySINner = null;
        private SINnersBasic TabSINnersBasic;

        public CharacterShared MySINner => _mySINner;

        public SINnersAdvanced TabSINnersAdvanced = null;

        public CharacterExtended MyCharacterExtended
        {
            get
            {
                return PluginHandler.MyCharacterExtended;
            }
            set
            {
                PluginHandler.MyCharacterExtended = value;
            }
        }

        public Character CharacterObject => MySINner.CharacterObject;

        private SINnersClient _client = null;
        public SINnersClient client
        {
            get
            {
                if (_client == null)
                {
                    try
                    {
                        ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        ServiceClientCredentials credentials = new TokenCredentials("Bearer");
                        Uri baseUri = new Uri("https://sinners.azurewebsites.net");
                        //Uri baseUri = new Uri("https://localhost:5001");
                        ServiceClientCredentials creds = new Backend.ApiKeyCredentials();
                        DelegatingHandler delegatingHandler = new MyMessageHandler();
                        _client = new SINnersClient(baseUri, creds, delegatingHandler);

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        throw;
                    }
                }
                return _client;
            }
        }

        public SINnersUserControl SetCharacterFrom(CharacterShared mySINner)
        {
            _mySINner = mySINner;
            
            MyCharacterExtended.PopulateTags();
            MyCharacterExtended.PrepareModel();
            TabSINnersBasic = new SINnersBasic(this);
            TabSINnersBasic.Visible = true;
            TabSINnersAdvanced = new SINnersAdvanced(this);
            TabSINnersAdvanced.Visible = true;
            InitializeComponent();
            this.tabPageBasic.Controls.Add(TabSINnersBasic);
            this.tabPageAdvanced.Controls.Add(TabSINnersAdvanced);
            return this;
        }

        public async void UploadSINnerAsync()
        {
            try
            {
                if (client.ApiV1SINnerHelperByIdGet(MyCharacterExtended.MySINnerFile.SiNnerId.Value) == true)
                {
                    client.ApiV1SINnerByIdPut(MyCharacterExtended.MySINnerFile.SiNnerId.Value, MyCharacterExtended.MySINnerFile);
                }
                else
                {
                    client.ApiV1SINnerPost(MyCharacterExtended.MySINnerFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        public async void RemoveSINnerAsync()
        {
            try
            {
                if (client.ApiV1SINnerHelperByIdGet(MyCharacterExtended.MySINnerFile.SiNnerId.Value) == true)
                {
                    await client.ApiV1SINnerByIdDeleteAsync(MyCharacterExtended.MySINnerFile.SiNnerId.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }


    }
}
