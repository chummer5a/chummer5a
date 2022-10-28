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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public partial class LoadingBar : Form
    {
        private string _strCharacterFile = string.Empty;

        public enum ProgressBarTextPatterns
        {
            Saving = 0,
            Loading,
            Scanning,
            Initializing
        }

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
                string strDisplayText = string.Format(GlobalSettings.CultureInfo,
                    LanguageManager.GetString("String_Loading_Pattern"), value);
                this.DoThreadSafe(x => x.Text = strDisplayText);
            }
        }

        public LoadingBar()
        {
            InitializeComponent();
            TopMost = !Utils.IsUnitTest && !Debugger.IsAttached;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
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
            lblLoadingInfo.DoThreadSafe(x => x.Text = strNewText);
            pgbLoadingProgress.DoThreadSafe(x =>
            {
                x.Value = 0;
                x.Maximum = intMaxProgressBarValue + 1;
            });
            Utils.DoEventsSafe();
        }

        /// <summary>
        /// Resets the ProgressBar
        /// </summary>
        /// <param name="intMaxProgressBarValue">New Maximum Value the ProgressBar should have.</param>
        /// <param name="token">Cancellation token to use.</param>
        public async ValueTask ResetAsync(int intMaxProgressBarValue = 100, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            string strTemp = await LanguageManager.GetStringAsync("String_Initializing", token: token).ConfigureAwait(false);
            await lblLoadingInfo.DoThreadSafeAsync(x => x.Text = strTemp, token).ConfigureAwait(false);
            await pgbLoadingProgress.DoThreadSafeAsync(x =>
            {
                x.Value = 0;
                x.Maximum = intMaxProgressBarValue + 1;
            }, token).ConfigureAwait(false);
            Utils.DoEventsSafe();
        }

        /// <summary>
        /// Performs a single step on the underlying ProgressBar
        /// </summary>
        /// <param name="strStepName">The text that the descriptive label above the ProgressBar should use, i.e. "Loading {strStepName}..."</param>
        /// <param name="eUseTextPattern">The text pattern to use in combination with <paramref name="strStepName"/>, e.g. "Loading", "Saving", et al.</param>
        public void PerformStep(string strStepName = "", ProgressBarTextPatterns eUseTextPattern = ProgressBarTextPatterns.Loading)
        {
            if (this.IsNullOrDisposed())
                return;
            string strNewText;
            switch (eUseTextPattern)
            {
                case ProgressBarTextPatterns.Saving:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = LanguageManager.GetString("String_Saving");
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Saving_Pattern"),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Loading:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = LanguageManager.GetString("String_Loading");
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Loading_Pattern"),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Scanning:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = LanguageManager.GetString("String_Scanning");
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Scanning_Pattern"),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Initializing:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = LanguageManager.GetString("String_Initializing");
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Initializing_Pattern"),
                                                   strStepName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eUseTextPattern), eUseTextPattern, null);
            }

            string strSpace = LanguageManager.GetString("String_Space");
            pgbLoadingProgress.DoThreadSafe(x =>
            {
                int intLoadingMaximum = x.Maximum;
                if (intLoadingMaximum > 2)
                    strNewText += strSpace + '('
                                           + (x.Value + 1).ToString(
                                               GlobalSettings.CultureInfo)
                                           + '/' + (intLoadingMaximum - 1).ToString(GlobalSettings.CultureInfo) + ')';
                x.PerformStep();
            });
            lblLoadingInfo.DoThreadSafe(x => x.Text = strNewText);
            Utils.DoEventsSafe();
        }

        /// <summary>
        /// Performs a single step on the underlying ProgressBar
        /// </summary>
        /// <param name="strStepName">The text that the descriptive label above the ProgressBar should use, i.e. "Loading {strStepName}..."</param>
        /// <param name="eUseTextPattern">The text pattern to use in combination with <paramref name="strStepName"/>, e.g. "Loading", "Saving", et al.</param>
        /// <param name="token">Cancellation token to use.</param>
        public async ValueTask PerformStepAsync(string strStepName = "", ProgressBarTextPatterns eUseTextPattern = ProgressBarTextPatterns.Loading, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (this.IsNullOrDisposed())
                return;
            string strNewText;
            switch (eUseTextPattern)
            {
                case ProgressBarTextPatterns.Saving:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = await LanguageManager.GetStringAsync("String_Saving", token: token).ConfigureAwait(false);
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   await LanguageManager.GetStringAsync("String_Saving_Pattern", token: token).ConfigureAwait(false),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Loading:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = await LanguageManager.GetStringAsync("String_Loading", token: token).ConfigureAwait(false);
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   await LanguageManager.GetStringAsync("String_Loading_Pattern", token: token).ConfigureAwait(false),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Scanning:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = await LanguageManager.GetStringAsync("String_Scanning", token: token).ConfigureAwait(false);
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   await LanguageManager.GetStringAsync("String_Scanning_Pattern", token: token).ConfigureAwait(false),
                                                   strStepName);
                    break;
                case ProgressBarTextPatterns.Initializing:
                    if (string.IsNullOrEmpty(strStepName))
                        strNewText = await LanguageManager.GetStringAsync("String_Initializing", token: token).ConfigureAwait(false);
                    else
                        strNewText = string.Format(GlobalSettings.CultureInfo,
                                                   await LanguageManager.GetStringAsync("String_Initializing_Pattern", token: token).ConfigureAwait(false),
                                                   strStepName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eUseTextPattern), eUseTextPattern, null);
            }

            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            await pgbLoadingProgress.DoThreadSafeAsync(x =>
            {
                int intLoadingMaximum = x.Maximum;
                if (intLoadingMaximum > 2)
                    strNewText += strSpace + '('
                                           + (x.Value + 1).ToString(
                                               GlobalSettings.CultureInfo)
                                           + '/' + (intLoadingMaximum - 1).ToString(GlobalSettings.CultureInfo) + ')';
                x.PerformStep();
            }, token).ConfigureAwait(false);
            await lblLoadingInfo.DoThreadSafeAsync(x => x.Text = strNewText, token).ConfigureAwait(false);
            Utils.DoEventsSafe();
        }
    }
}
