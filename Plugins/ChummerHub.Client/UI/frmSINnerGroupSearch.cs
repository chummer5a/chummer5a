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
using System.Drawing;
using System.Windows.Forms;
using Chummer.Plugins;
using ChummerHub.Client.Sinners;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerGroupSearch : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private CharacterExtended MyCE { get; }
        public ucSINnersBasic MyParentForm { get; }

        public ucSINnerGroupSearch MySINnerGroupSearch => siNnerGroupSearch1;

        public frmSINnerGroupSearch(CharacterExtended ce, ucSINnersBasic parentBasic)
        {
            MyCE = ce;
            MyParentForm = parentBasic;
            InitializeComponent();
            siNnerGroupSearch1.MyCE = ce;
            siNnerGroupSearch1.MyParentForm = this;
            VisibleChanged += (sender, args) =>
            {
                if (Visible)
                    ReallyCenterToScreen();
            };

        }
        protected void ReallyCenterToScreen()
        {
            Screen screen = Screen.FromControl(PluginHandler.MainForm);

            Rectangle workingArea = screen.WorkingArea;
            Location = new Point
            {
                X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - Width) / 2),
                Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - Height) / 2)
            };
        }

        private async void FrmSINnerGroupSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                await PluginHandler.MainForm.CharacterRoster.RefreshPluginNodesAsync(PluginHandler.MyPluginHandlerInstance);
            }
            catch (Exception exception)
            {
                Log.Warn(exception);
            }
        }
    }
}
