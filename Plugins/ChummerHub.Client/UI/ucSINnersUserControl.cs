using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.Backend;
using Chummer.Plugins;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class ucSINnersUserControl : UserControl
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
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
            MyCE = new CharacterExtended(mySINner.CharacterObject, PluginHandler.MySINnerLoading);
            MyCE.ZipFilePath = await MyCE.PrepareModelAsync();

            TabSINnersBasic = new ucSINnersBasic(this)
            {
                Visible = true
            };
            TabSINnersAdvanced = new ucSINnersAdvanced(this)
            {
                Visible = true
            };


            tabPageBasic.Controls.Add(TabSINnersBasic);
            tabPageAdvanced.Controls.Add(TabSINnersAdvanced);
            AutoSize = true;

            if (ucSINnersOptions.UploadOnSave)
            {
                try
                {
                    if (mySINner.CharacterObject.DoOnSaveCompletedAsync.Remove(PluginHandler.MyOnSaveUpload))
                        mySINner.CharacterObject.DoOnSaveCompletedAsync.Add(PluginHandler.MyOnSaveUpload);
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
                SinnersClient client = StaticUtils.GetClient();
                if (MyCE.MySINnerFile.Id != null)
                    await client.DeleteAsync(MyCE.MySINnerFile.Id.Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}
