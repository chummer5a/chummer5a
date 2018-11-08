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
using System.IO;
using SINners.Models;

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
        public SINnersClient Client
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
                        Uri baseUri = new Uri(TabSINnersAdvanced.cbSINnerUrl.SelectedItem.ToString());
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
            set
            {
                _client = value;
            }

        }

        public SINnersUserControl SetCharacterFrom(CharacterShared mySINner)
        {
            _mySINner = mySINner;
            TabSINnersBasic = new SINnersBasic(this);
            TabSINnersBasic.Visible = true;
            TabSINnersAdvanced = new SINnersAdvanced(this);
            TabSINnersAdvanced.Visible = true;
            MyCharacterExtended.PopulateTags();
            MyCharacterExtended.ZipFilePath = MyCharacterExtended.PrepareModel();
            InitializeComponent();
            this.tabPageBasic.Controls.Add(TabSINnersBasic);
            this.tabPageAdvanced.Controls.Add(TabSINnersAdvanced);
           
            this.AutoSize = true;
          
            return this;
        }

        public async void PostSINnerAsync()
        {
            try
            {
                UploadInfoObject uploadInfoObject = new UploadInfoObject();
                uploadInfoObject.Client = PluginHandler.MyUploadClient;
                uploadInfoObject.UploadDateTime = DateTime.Now;
                uploadInfoObject.SiNners = new List<SINner>() { MyCharacterExtended.MySINnerFile };
                var response = await Client.ApiV1SINnerPostPostWithHttpMessagesAsync(uploadInfoObject);
                if (response.Response.StatusCode == HttpStatusCode.BadRequest
                    || response.Response.StatusCode == HttpStatusCode.Conflict)
                {
                    var errorMessage = response.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    string msg = "Answer from WebService BadRequest: " + Environment.NewLine + Environment.NewLine + errorMessage;
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        public async void UploadChummerFileAsync()
        {
            try
            {
                if (String.IsNullOrEmpty(MyCharacterExtended.ZipFilePath))
                    MyCharacterExtended.ZipFilePath = MyCharacterExtended.PrepareModel();
                using (FileStream fs = new FileStream(MyCharacterExtended.ZipFilePath, FileMode.Open, FileAccess.Read))
                {
                    Client.ApiV1SINnerPutByIdPutAsync(MyCharacterExtended.MySINnerFile.SiNnerId.Value, fs);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        
        public async void DownloadFileAsync()
        {
            try
            {
                var response =  await Client.ApiV1ChummerHelperGetBySinneridGetWithHttpMessagesAsync(MyCharacterExtended.MySINnerFile.SiNnerId.Value);
                var content = await response.Response.Content.ReadAsStringAsync();
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
                    await Client.ApiV1SINnerDeleteByIdDeleteAsync(MyCharacterExtended.MySINnerFile.SiNnerId.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

  
    }
}
