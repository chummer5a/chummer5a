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
using System.Reflection;
 using System.Text;
 using System.Windows.Forms;

namespace Chummer
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        #region Assembly Attribute Accessors
        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description.Replace("\n\r", Environment.NewLine).Replace("\n", Environment.NewLine);
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product.Replace("\n\r", Environment.NewLine).Replace("\n", Environment.NewLine).WordWrap();
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright.Replace("\n\r", Environment.NewLine).Replace("\n", Environment.NewLine).WordWrap();
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company.Replace("\n\r", Environment.NewLine).Replace("\n", Environment.NewLine).WordWrap();
            }
        }
        #endregion

        #region Controls Methods
        private void frmAbout_Load(object sender, EventArgs e)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strReturn = LanguageManager.GetString("Label_About", false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = "About";
            Text = strReturn + strSpace + AssemblyTitle;
            lblProductName.Text = AssemblyProduct;
            strReturn = LanguageManager.GetString("String_Version", false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = "Version";
            lblVersion.Text = strReturn + strSpace + AssemblyVersion;
            strReturn = LanguageManager.GetString("About_Copyright_Text", false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = AssemblyCopyright;
            lblCopyright.Text = strReturn;
            strReturn = LanguageManager.GetString("About_Company_Text", false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = AssemblyCompany;
            lblCompanyName.Text = strReturn;
            strReturn = LanguageManager.GetString("About_Description_Text", false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = AssemblyDescription;
            txtDescription.Text = strReturn;
            txtContributors.Text += new StringBuilder(Environment.NewLine)
                .AppendLine().AppendJoin(Environment.NewLine, Properties.Contributors.Usernames)
                .AppendLine().Append("/u/Iridios").ToString();
            txtDisclaimer.Text = LanguageManager.GetString("About_Label_Disclaimer_Text");
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.OK;

            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                (sender as TextBox)?.SelectAll();
            }
        }
        #endregion
    }
}
