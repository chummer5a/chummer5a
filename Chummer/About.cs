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
using System.Windows.Forms;

namespace Chummer
{
    public partial class About : Form
    {
        public About()
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
                return attributes.Length == 0 ? string.Empty : ((AssemblyDescriptionAttribute)attributes[0]).Description.NormalizeLineEndings();
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product.NormalizeLineEndings().WordWrap();
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright.NormalizeLineEndings().WordWrap();
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCompanyAttribute)attributes[0]).Company.NormalizeLineEndings().WordWrap();
            }
        }

        #endregion Assembly Attribute Accessors

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
            txtContributors.Text += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, Properties.Contributors.Usernames)
                                    + Environment.NewLine + "/u/Iridios";
            txtDisclaimer.Text = LanguageManager.GetString("About_Label_Disclaimer_Text");
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    DialogResult = DialogResult.OK;
                    break;

                case Keys.A:
                    {
                        if (e.Control)
                        {
                            e.SuppressKeyPress = true;
                            (sender as TextBox)?.SelectAll();
                        }

                        break;
                    }
            }
        }

        #endregion Controls Methods
    }
}
