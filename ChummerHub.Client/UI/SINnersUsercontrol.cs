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
using System.Windows.Threading;

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

        public async Task PostSINnerAsync()
        {
            try
            {
                if (!StaticUtils.IsUnitTest)
                    Cursor = Cursors.WaitCursor;
                UploadInfoObject uploadInfoObject = new UploadInfoObject();
                uploadInfoObject.Client = PluginHandler.MyUploadClient;
                uploadInfoObject.UploadDateTime = DateTime.Now;
                MyCharacterExtended.MySINnerFile.UploadDateTime = DateTime.Now;
                uploadInfoObject.SiNners = new List<SINner>() { MyCharacterExtended.MySINnerFile };
                System.Diagnostics.Trace.TraceInformation("Posting " + MyCharacterExtended.MySINnerFile.Id + "...");
                await StaticUtils.Client.PostAsync(uploadInfoObject);
                System.Diagnostics.Trace.TraceInformation("Post of " + MyCharacterExtended.MySINnerFile.Id + " finished.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
            finally
            {
                if (!StaticUtils.IsUnitTest)
                    Cursor = Cursors.Default;
            }
        }

        public async Task UploadChummerFileAsync()
        {
            try
            {
                if (String.IsNullOrEmpty(MyCharacterExtended.ZipFilePath))
                    MyCharacterExtended.ZipFilePath = MyCharacterExtended.PrepareModel();
                
                using (FileStream fs = new FileStream(MyCharacterExtended.ZipFilePath, FileMode.Open, FileAccess.Read))
                {   try
                    {
                        if (!StaticUtils.IsUnitTest)
                            Cursor = Cursors.WaitCursor;
                        var task = StaticUtils.Client.PutAsync(MyCharacterExtended.MySINnerFile.Id.Value, fs);
                        await task.ContinueWith((sender) =>
                         {
                             string msg = "Upload completed with status: " + sender.Status.ToString();
                             msg += Environment.NewLine + sender.Exception?.Message;
                             if (!StaticUtils.IsUnitTest)
                             {
                                 this.Invoke(new MethodInvoker(() =>
                                 {
                                     Cursor = Cursors.Default;
                                 }));
                             
                                 Chummer.Plugins.PluginHandler.MainForm.Invoke(new MethodInvoker(() =>
                                 {
                                     Chummer.Plugins.PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false, true);
                                 }));
                                 MessageBox.Show(msg);
                             }
                             else
                             {
                                 System.Diagnostics.Trace.TraceInformation(msg);
                             }
                         });
                    }
                    catch(Exception e)
                    {
                        System.Diagnostics.Trace.TraceError(e.ToString());
                        MessageBox.Show(e.Message);
                    }
                    finally
                    {
                        if (!StaticUtils.IsUnitTest)
                            Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

        
        public async Task DownloadFileAsync()
        {
            try
            {
                var response =  await StaticUtils.Client.GetDownloadFileWithHttpMessagesAsync(MyCharacterExtended.MySINnerFile.Id.Value);
                var content = await response.Response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }


        public async Task RemoveSINnerAsync()
        {
            try
            {
                    await StaticUtils.Client.DeleteAsync(MyCharacterExtended.MySINnerFile.Id.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

  
    }
}
