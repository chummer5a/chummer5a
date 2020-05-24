using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Model;
using SINners;
using ChummerHub.Client.Backend;
using Chummer.Plugins;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersUserControl : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CharacterShared _mySINner;
        private ucSINnersBasic TabSINnersBasic;

        public CharacterShared MySINner => _mySINner;

        private ucSINnersAdvanced TabSINnersAdvanced;

        public CharacterExtended MyCE { get; set; }

        public Character CharacterObject => MySINner.CharacterObject;

        public async Task<CharacterExtended> SetCharacterFrom(CharacterShared mySINner)
        {
            InitializeComponent();
            _mySINner = mySINner ?? throw new ArgumentNullException(nameof(mySINner));
            MyCE = new CharacterExtended(mySINner.CharacterObject, null, PluginHandler.MySINnerLoading);
            MyCE.ZipFilePath = await MyCE.PrepareModel().ConfigureAwait(true);

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

            if (ucSINnersOptions.UploadOnSave)
            {
                try
                {
                    mySINner.CharacterObject.OnSaveCompleted = null;
                    mySINner.CharacterObject.OnSaveCompleted += PluginHandler.MyOnSaveUpload;
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }
            //MyCE.MySINnerFile.SiNnerMetaData.Tags = MyCE.PopulateTags();
            return MyCE;
        }

        public async Task RemoveSINnerAsync()
        {
            try
            {
                var client = StaticUtils.GetClient();
                if (MyCE.MySINnerFile.Id != null)
                    await client.DeleteAsync(MyCE.MySINnerFile.Id.Value).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}
