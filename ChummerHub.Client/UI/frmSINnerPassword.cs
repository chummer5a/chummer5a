using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SINners.Models;

namespace ChummerHub.Client.UI
{
    public partial class frmSINnerPassword : Form
    {
        public enum SHSTOCKICONID : uint
        {
            //...
            SIID_INFO = 79,
            //...
        }

        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            public string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        public frmSINnerPassword()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO
            {
                cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO))
            };

            Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SHSTOCKICONID.SIID_INFO,
                SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON,
                ref sii));
            pbIcon.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            this.AcceptButton = this.bOk;
        }

        public string ShowDialog(string text, string caption)
        {
            this.Text = caption;
            this.lPasswordText.Text = text;
            return this.ShowDialog() == DialogResult.OK ? SINnerGroup.GetHashString(tbPassword.Text) : "";
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
