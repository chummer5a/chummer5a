using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using SystemColors = System.Drawing.SystemColors;

namespace Chummer
{
    public static class ColorManager
    {
        private static bool _blnIsLightMode = true;
        private static readonly RegistryKey _objPersonalizeKey;

        static ColorManager()
        {
            if (Utils.IsDesignerMode)
                return;

            try
            {
                _objPersonalizeKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            }
            catch (System.ObjectDisposedException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }

            if (_objPersonalizeKey != null)
            {
                object objLightModeResult = _objPersonalizeKey.GetValue("AppsUseLightTheme");
                if (int.TryParse(objLightModeResult.ToString(), out int intTemp))
                    _blnIsLightMode = intTemp != 0;
            }
        }

        public static bool IsLightMode => _blnIsLightMode;

        public static Color WindowText => IsLightMode ? SystemColors.WindowText : SystemColors.Window;
        public static Color Window => IsLightMode ? SystemColors.Window : SystemColors.WindowText;
        public static Color InfoText => IsLightMode ? SystemColors.InfoText : SystemColors.Info;
        public static Color Info => IsLightMode ? SystemColors.Info : SystemColors.InfoText;
        public static Color GrayText => SystemColors.GrayText;
        public static Color Highlight => SystemColors.Highlight;
        public static Color HighlightText => SystemColors.HighlightText;
        public static Color ControlText => IsLightMode ? SystemColors.ControlText : SystemColors.ControlLightLight;
        public static Color ControlDarkDark => IsLightMode ? SystemColors.ControlDarkDark : SystemColors.ControlLight;
        public static Color ControlDark => IsLightMode ? SystemColors.ControlDark : SystemColors.Control;
        public static Color Control => IsLightMode ? SystemColors.Control : SystemColors.ControlDark;
        public static Color ControlLight => IsLightMode ? SystemColors.ControlLight : SystemColors.ControlDarkDark;
        public static Color ControlLightLight => IsLightMode ? SystemColors.ControlLightLight : SystemColors.ControlText;
        public static Color ButtonFace => IsLightMode ? SystemColors.ButtonFace : SystemColors.ButtonShadow;
        public static Color ButtonShadow => IsLightMode ? SystemColors.ButtonShadow : SystemColors.ButtonFace;

        public static Color SplitterColor => IsLightMode ? SplitterColorLight : SplitterColorDark;
        private static Color SplitterColorLight => Color.LightBlue;
        private static Color SplitterColorDark => Color.DarkBlue;
        public static Color HasNotesColor => IsLightMode ? HasNotesColorLight : HasNotesColorDark;
        private static Color HasNotesColorLight => Color.SaddleBrown;
        private static Color HasNotesColorDark => Color.Chocolate;
        public static Color GrayHasNotesColor => Color.Tan;
        public static Color ErrorColor => Color.Red;

        public static void UpdateLightDarkMode(this Control objControl, bool blnFirstUpdate = true)
        {
            // Light mode is the default theme, so don't do any updates if this is the startup call and the system already is in light mode
            if (blnFirstUpdate && IsLightMode)
                return;

            objControl.SuspendLayout();
            InvertControlColorsRecusively(objControl);
            objControl.ResumeLayout();
        }

        #region Color Inversion Methods

        private static Color InvertColor(Color objInputColor, out bool blnWasInverted)
        {
            blnWasInverted = true;
            // Trivial Inversions
            if (objInputColor == ControlText)
                return ControlLightLight;
            if (objInputColor == ControlDarkDark)
                return ControlLight;
            if (objInputColor == ControlDark)
                return Control;
            if (objInputColor == Control)
                return ControlDark;
            if (objInputColor == ControlLight)
                return ControlDarkDark;
            if (objInputColor == ControlLightLight)
                return ControlText;
            if (objInputColor == Window)
                return WindowText;
            if (objInputColor == WindowText)
                return Window;
            if (objInputColor == Info)
                return InfoText;
            if (objInputColor == InfoText)
                return Info;
            if (objInputColor == ButtonFace)
                return ButtonShadow;
            if (objInputColor == ButtonShadow)
                return ButtonFace;
            // Non-trivial Inversions
            if (objInputColor == SplitterColorLight)
                return SplitterColorDark;
            if (objInputColor == SplitterColorDark)
                return SplitterColorLight;
            if (objInputColor == HasNotesColorLight)
                return HasNotesColorDark;
            if (objInputColor == HasNotesColorDark)
                return HasNotesColorLight;
            blnWasInverted = false;
            return objInputColor;
        }

        private static void InvertControlColorsRecusively(Control objControl)
        {
            // Foreground Color
            Color objTemp = InvertColor(objControl.ForeColor, out bool blnWasInverted);
            if (blnWasInverted)
                objControl.ForeColor = objTemp;

            // Background Color
            objTemp = InvertColor(objControl.BackColor, out blnWasInverted);
            if (blnWasInverted)
                objControl.BackColor = objTemp;

            if (objControl.HasChildren)
            {
                if (objControl is Form frmControl)
                {
                    if (frmControl.MainMenuStrip != null)
                        foreach (ToolStripMenuItem tssItem in frmControl.MainMenuStrip.Items)
                            InvertControlColorsRecusively(tssItem);
                }

                foreach (Control objChild in objControl.Controls)
                {
                    if (objChild is ToolStrip tssStrip)
                    {
                        foreach (ToolStripItem tssItem in tssStrip.Items)
                            InvertControlColorsRecusively(tssItem);
                    }
                    else if (objChild is TabControl objTabControl)
                    {
                        foreach (TabPage tabPage in objTabControl.TabPages)
                            InvertControlColorsRecusively(tabPage);
                    }
                    else if (objChild is SplitContainer objSplitControl)
                    {
                        InvertControlColorsRecusively(objSplitControl.Panel1);
                        InvertControlColorsRecusively(objSplitControl.Panel2);
                    }
                    else if (objChild is TreeView treTree)
                    {
                        foreach (TreeNode objNode in treTree.Nodes)
                            InvertControlColorsRecusively(objNode);
                    }
                    else if (objChild is DataGridView objDataGridView)
                    {
                        // Foreground Color
                        objTemp = InvertColor(objDataGridView.DefaultCellStyle.ForeColor, out blnWasInverted);
                        if (blnWasInverted)
                            objDataGridView.DefaultCellStyle.ForeColor = objTemp;

                        // Background Color
                        objTemp = InvertColor(objDataGridView.DefaultCellStyle.BackColor, out blnWasInverted);
                        if (blnWasInverted)
                            objDataGridView.DefaultCellStyle.BackColor = objTemp;

                        foreach (DataGridViewTextBoxColumn objColumn in objDataGridView.Columns)
                        {
                            // Foreground Color
                            objTemp = InvertColor(objColumn.DefaultCellStyle.ForeColor, out blnWasInverted);
                            if (blnWasInverted)
                                objColumn.DefaultCellStyle.ForeColor = objTemp;

                            // Background Color
                            objTemp = InvertColor(objColumn.DefaultCellStyle.BackColor, out blnWasInverted);
                            if (blnWasInverted)
                                objColumn.DefaultCellStyle.BackColor = objTemp;
                        }
                    }
                    else
                    {
                        InvertControlColorsRecusively(objChild);
                    }
                }
            }
        }

        private static void InvertControlColorsRecusively(ToolStripItem tssItem)
        {
            // Foreground Color
            Color objTemp = InvertColor(tssItem.ForeColor, out bool blnWasInverted);
            if (blnWasInverted)
                tssItem.ForeColor = objTemp;

            // Background Color
            objTemp = InvertColor(tssItem.BackColor, out blnWasInverted);
            if (blnWasInverted)
                tssItem.BackColor = objTemp;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    InvertControlColorsRecusively(tssDropDownChild);
        }

        private static void InvertControlColorsRecusively(TreeNode nodNode)
        {
            // Foreground Color
            Color objTemp = InvertColor(nodNode.ForeColor, out bool blnWasInverted);
            if (blnWasInverted)
                nodNode.ForeColor = objTemp;

            // Background Color
            objTemp = InvertColor(nodNode.BackColor, out blnWasInverted);
            if (blnWasInverted)
                nodNode.BackColor = objTemp;

            foreach (TreeNode nodNodeChild in nodNode.Nodes)
                InvertControlColorsRecusively(nodNodeChild);
        }
        #endregion
    }
}
