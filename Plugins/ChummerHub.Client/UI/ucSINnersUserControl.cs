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
using System.Threading;
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
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private CharacterShared _mySINner;
        private ucSINnersBasic TabSINnersBasic;

        public CharacterShared MySINner => _mySINner;

        private ucSINnersAdvanced TabSINnersAdvanced;

        public CharacterExtended MyCE { get; set; }

        public Character CharacterObject => MySINner.CharacterObject;

        public async Task<CharacterExtended> SetCharacterFrom(CharacterShared mySINner, CancellationToken token = default)
        {
            InitializeComponent();
            _mySINner = mySINner ?? throw new ArgumentNullException(nameof(mySINner));
            MyCE = new CharacterExtended(mySINner.CharacterObject, PluginHandler.MySINnerLoading);
            MyCE.ZipFilePath = await MyCE.PrepareModelAsync(token);
            //MyCE.ZipFilePath = await MyCE.PrepareModelAsync();

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
                    if (await mySINner.CharacterObject.DoOnSaveCompletedAsync.RemoveAsync(PluginHandler.MyOnSaveUpload, token))
                        await mySINner.CharacterObject.DoOnSaveCompletedAsync.AddAsync(PluginHandler.MyOnSaveUpload, token);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }
            //MyCE.MySINnerFile.SiNnerMetaData.Tags = MyCE.PopulateTags();
            return MyCE;
        }

        public async Task RemoveSINnerAsync(CancellationToken token = default)
        {
            try
            {
                SinnersClient client = StaticUtils.GetClient();
                if (MyCE.MySINnerFile.Id != null)
                    await client.DeleteAsync(MyCE.MySINnerFile.Id.Value, token);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}
