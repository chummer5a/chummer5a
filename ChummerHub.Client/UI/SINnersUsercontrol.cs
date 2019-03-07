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

        

        public CharacterExtended SetCharacterFrom(CharacterShared mySINner)
        {
            InitializeComponent();
            _mySINner = mySINner;
            MyCE = new CharacterExtended(mySINner.CharacterObject, null, PluginHandler.MySINnerLoading);
            MyCE.MySINnerFile.SiNnerMetaData.Tags = MyCE.PopulateTags();
            MyCE.ZipFilePath = MyCE.PrepareModel();
            TabSINnersBasic = new SINnersBasic(this);
            TabSINnersBasic.Visible = true;
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
                    await StaticUtils.Client.DeleteAsync(MyCE.MySINnerFile.Id.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }

  
    }
}
