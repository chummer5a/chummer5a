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
    public partial class ucSINnersUserControl : UserControl
    {
        
        private CharacterShared _mySINner = null;
        private ucSINnersBasic TabSINnersBasic;

        public CharacterShared MySINner => _mySINner;

        public ucSINnersAdvanced TabSINnersAdvanced = null;

        public CharacterExtended MyCE { get; set; }

        public Character CharacterObject => MySINner.CharacterObject;

        

        public async Task<CharacterExtended> SetCharacterFrom(CharacterShared mySINner)
        {
            InitializeComponent();
            _mySINner = mySINner;
            MyCE = new CharacterExtended(mySINner.CharacterObject, null, PluginHandler.MySINnerLoading);
            MyCE.ZipFilePath = await MyCE.PrepareModel();
            MyCE.MySINnerFile.SiNnerMetaData.Tags = MyCE.PopulateTags();

            TabSINnersBasic = new ucSINnersBasic(this)
            {
                Visible = true
            };
            TabSINnersAdvanced = new ucSINnersAdvanced(this)
            {
                Visible = true
            };


            this.tabPageBasic.Controls.Add(TabSINnersBasic);
            this.tabPageAdvanced.Controls.Add(TabSINnersAdvanced);
           
            this.AutoSize = true;

            if ((ucSINnersOptions.UploadOnSave == true))
            {
                try
                {
                    mySINner.CharacterObject.OnSaveCompleted = null;
                    mySINner.CharacterObject.OnSaveCompleted += PluginHandler.MyOnSaveUpload;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                
            }
            return MyCE;
        }

        

        

        public async Task RemoveSINnerAsync()
        {
            try
            {
                var client = StaticUtils.GetClient();
                await client.DeleteAsync(MyCE.MySINnerFile.Id.Value);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                throw;
            }
        }

  
    }
}
