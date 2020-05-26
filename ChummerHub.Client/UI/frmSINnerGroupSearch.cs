using System;
using System.Drawing;
using System.Windows.Forms;
using Chummer;
using Chummer.Plugins;
using ChummerHub.Client.Model;
using NLog;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerGroupSearch : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
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

        private void FrmSINnerGroupSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                PluginHandler.MainForm.CharacterRoster.DoThreadSafe(() =>
                {
                    PluginHandler.MainForm.CharacterRoster.LoadCharacters(false, false, false);
                });
            }
            catch (Exception exception)
            {
                Log.Warn(exception);
            }
        }
    }
}
