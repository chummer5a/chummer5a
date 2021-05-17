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

using System.Diagnostics;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmLoading : Form
    {
        private string _strCharacterFile = string.Empty;

        public string CharacterFile
        {
            get => _strCharacterFile;
            set
            {
                if (_strCharacterFile == value)
                    return;
                _strCharacterFile = value;
                if (this.IsNullOrDisposed())
                    return;
                string strDisplayText = string.Format(GlobalOptions.CultureInfo,
                    LanguageManager.GetString("String_Loading_Pattern"), value);
                this.QueueThreadSafe(() => Text = strDisplayText);
            }
        }

        public frmLoading()
        {
            InitializeComponent();
            TopMost = !Utils.IsUnitTest && !Debugger.IsAttached;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void frmLoading_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Make sure we always complete the progress bar before closing out
            await pgbLoadingProgress.DoThreadSafeAsync(() => pgbLoadingProgress.Value = pgbLoadingProgress.Maximum);
            Application.DoEvents();
        }

        /// <summary>
        /// Resets the ProgressBar
        /// </summary>
        /// <param name="intMaxProgressBarValue">New Maximum Value the ProgressBar should have.</param>
        public void Reset(int intMaxProgressBarValue = 100)
        {
            if (this.IsNullOrDisposed())
                return;
            string strNewText = LanguageManager.GetString("String_Initializing");
            lblLoadingInfo.QueueThreadSafe(() => lblLoadingInfo.Text = strNewText);
            pgbLoadingProgress.DoThreadSafe(() =>
            {
                pgbLoadingProgress.Value = 0;
                pgbLoadingProgress.Maximum = intMaxProgressBarValue + 1;
            });
        }

        /// <summary>
        /// Performs a single step on the underlying ProgressBar
        /// </summary>
        /// <param name="strStepName">The text that the descriptive label above the ProgressBar should use, i.e. "Loading {strStepName}..."</param>
        public void PerformStep(string strStepName = "")
        {
            if (this.IsNullOrDisposed())
                return;
            string strNewText = string.IsNullOrEmpty(strStepName)
                    ? LanguageManager.GetString("String_Loading")
                    : string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Loading_Pattern"), strStepName);
            if (pgbLoadingProgress.Maximum > 2)
                strNewText += LanguageManager.GetString("String_Space") + '(' + (pgbLoadingProgress.Value + 1).ToString(GlobalOptions.CultureInfo)
                              + '/' + (pgbLoadingProgress.Maximum - 1).ToString(GlobalOptions.CultureInfo) + ')';
            lblLoadingInfo.QueueThreadSafe(() => lblLoadingInfo.Text = strNewText);
            pgbLoadingProgress.QueueThreadSafe(() => pgbLoadingProgress.PerformStep());
        }
    }
}
