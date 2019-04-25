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

        public CharacterExtended MyCE { get; set; }

        public Character CharacterObject => MySINner.CharacterObject;

        

        public async Task<CharacterExtended> SetCharacterFrom(CharacterShared mySINner)
        {
            InitializeComponent();
            _mySINner = mySINner;
            //if (PluginHandler.MyCharExtendedDic.ContainsKey(mySINner.CharacterObject.FileName))
            //{
            //    CharacterExtended outce;
            //    if (!PluginHandler.MyCharExtendedDic.TryGetValue(mySINner.CharacterObject.FileName, out outce))
            //    {
            //        throw new ArgumentException("Could not get character from MyCharExtendedDic", nameof(mySINner));
            //    }

            //    MyCE = outce;
            //}
            //else
            //{
                MyCE = new CharacterExtended(mySINner.CharacterObject, null, PluginHandler.MySINnerLoading);
                MyCE.ZipFilePath = await MyCE.PrepareModel();
            //}
            MyCE.MySINnerFile.SiNnerMetaData.Tags = MyCE.PopulateTags();

            TabSINnersBasic = new SINnersBasic(this)
            {
                Visible = true
            };
            TabSINnersAdvanced = new SINnersAdvanced(this);
#if DEBUG
            TabSINnersAdvanced.Visible = true;
#else
            TabSINnersAdvanced.Visible = false;
#endif

           
            
            this.tabPageBasic.Controls.Add(TabSINnersBasic);
            this.tabPageAdvanced.Controls.Add(TabSINnersAdvanced);
           
            this.AutoSize = true;
          
            return MyCE;
        }

        

        

        public async Task RemoveSINnerAsync()
        {
            try
            {
                var client = await StaticUtils.GetClient();
                await client.DeleteAsync(MyCE.MySINnerFile.Id.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

  
    }
}
