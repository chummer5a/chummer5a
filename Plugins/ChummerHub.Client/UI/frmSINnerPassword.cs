using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ChummerHub.Client.Sinners;
using IntPtr = System.IntPtr;
using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;
using LayoutKind = System.Runtime.InteropServices.LayoutKind;

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
            public readonly uint cbSize;
            public readonly IntPtr hIcon;
            public readonly int iSysIconIndex;
            public readonly int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            public readonly string szPath;

            public SHSTOCKICONINFO(uint uintSize, IntPtr ptrIcon, int intSysIconIndex = 0, int intIcon = 0, string strPath = "")
            {
                cbSize = uintSize;
                hIcon = ptrIcon;
                iSysIconIndex = intSysIconIndex;
                iIcon = intIcon;
                szPath = strPath;
            }

            public override bool Equals(object obj)
            {
                if (obj is SHSTOCKICONINFO right)
                {
                    return cbSize == right.cbSize
                           && hIcon == right.hIcon
                           && iSysIconIndex == right.iSysIconIndex
                           && iIcon == right.iIcon
                           && szPath == right.szPath;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (cbSize, hIcon, iSysIconIndex, iIcon, szPath).GetHashCode();
            }

            public static bool operator ==(SHSTOCKICONINFO left, SHSTOCKICONINFO right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(SHSTOCKICONINFO left, SHSTOCKICONINFO right)
            {
                return !(left == right);
            }
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        private static extern int SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        public frmSINnerPassword()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            SHSTOCKICONINFO sii = new SHSTOCKICONINFO((uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO)), IntPtr.Zero);

            Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SHSTOCKICONID.SIID_INFO,
                SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON,
                ref sii));
            pbIcon.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
            AcceptButton = bOk;
        }

        public string ShowDialog(IWin32Window owner, string text, string caption)
        {
            Text = caption;
            lPasswordText.Text = text;
            return ShowDialog(owner) == DialogResult.OK ? SINnerGroup.GetHashString(tbPassword.Text) : "";
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
