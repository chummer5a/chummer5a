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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class CreateExpense : Form
    {
        private ExpenseType _eMode = ExpenseType.Karma;
        private readonly CharacterSettings _objCharacterSettings;
        private bool _blnForceCareerVisible;
        private bool _blnRefund;
        private bool _blnIsGain = true;
        private string _strReason;
        private decimal _decAmount;
        private DateTime _datSelectedDate;

        #region Control Events

        public CreateExpense(CharacterSettings objCharacterSettings)
        {
            _objCharacterSettings = objCharacterSettings;

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();

            // Determine the DateTime format and use that to display the date field (removing seconds since they're not important).

            if (GlobalSettings.CustomDateTimeFormats)
            {
                datDate.CustomFormat = GlobalSettings.DatesIncludeTime
                    ? GlobalSettings.CustomDateFormat + GlobalSettings.CustomTimeFormat
                    : GlobalSettings.CustomDateFormat;
            }
            else
            {
                DateTimeFormatInfo objDateTimeInfo = GlobalSettings.CultureInfo.DateTimeFormat;
                datDate.CustomFormat = GlobalSettings.DatesIncludeTime
                    ? objDateTimeInfo.FullDateTimePattern.FastEscapeOnceFromEnd(":ss")
                    : objDateTimeInfo.LongDatePattern;
            }

            datDate.Value = GlobalSettings.GetDefaultExpenseDate();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (KarmaNuyenExchange && _eMode == ExpenseType.Nuyen)
            {
                decimal decNuyenPerBPWtfP = await _objCharacterSettings.GetNuyenPerBPWftPAsync().ConfigureAwait(false);
                decimal decDividend = await nudAmount.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false) / decNuyenPerBPWtfP;
                if (decimal.Floor(decDividend) != decimal.Ceiling(decDividend))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        this,
                        string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("Message_KarmaNuyenExchange").ConfigureAwait(false),
                            decNuyenPerBPWtfP.ToString(
                                await _objCharacterSettings.GetNuyenFormatAsync().ConfigureAwait(false), GlobalSettings.CultureInfo)
                            + await LanguageManager.GetStringAsync("String_NuyenSymbol").ConfigureAwait(false)),
                        await LanguageManager.GetStringAsync("MessageTitle_KarmaNuyenExchange").ConfigureAwait(false), MessageBoxButtons.OK,
                        MessageBoxIcon.Information).ConfigureAwait(false);
                }
            }
            else
            {
                _decAmount = await nudAmount.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);
                if (_eMode == ExpenseType.Nuyen)
                    _decAmount *= nudPercent.Value / 100.0m;
                _datSelectedDate = await datDate.DoThreadSafeFuncAsync(x => x.Value).ConfigureAwait(false);
                _strReason = await txtDescription.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
                _blnRefund = await chkRefund.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                _blnForceCareerVisible = await chkForceCareerVisible.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }).ConfigureAwait(false);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Amount gained or spent.
        /// </summary>
        public decimal Amount
        {
            get => _decAmount;
            set
            {
                if (value < 0)
                    nudAmount.Minimum = value;
                if (value == 0)
                    nudAmount.Minimum = 0;
                nudAmount.Value = value;
            }
        }

        /// <summary>
        /// Reason for the Karma change.
        /// </summary>
        public string Reason
        {
            get => _strReason;
            set => txtDescription.Text = value;
        }

        /// <summary>
        /// Whether this is a Karma refund.
        /// </summary>
        public bool Refund
        {
            get => _blnRefund;
            set => chkRefund.Checked = value;
        }

        /// <summary>
        /// Whether this is a Karma refund.
        /// </summary>
        public bool ForceCareerVisible
        {
            get => _blnForceCareerVisible;
            set => chkForceCareerVisible.Checked = value;
        }

        /// <summary>
        /// Date and Time that was selected.
        /// </summary>
        public DateTime SelectedDate
        {
            get => _datSelectedDate;
            set => datDate.Value = value;
        }

        /// <summary>
        /// Whether this dialog records a gain (true) or spend (false). Affects the window title.
        /// </summary>
        public bool IsGain
        {
            get => _blnIsGain;
            set => _blnIsGain = value;
        }

        /// <summary>
        /// Reputation track being adjusted when Mode is Reputation.
        /// </summary>
        public ReputationTrack ReputationTrack { get; set; }

        /// <summary>
        /// The Expense's mode (Karma, Nuyen, or Reputation).
        /// </summary>
        public ExpenseType Mode
        {
            set
            {
                if (InterlockedExtensions.Exchange(ref _eMode, value) == value)
                    return;
                if (value == ExpenseType.Nuyen)
                {
                    lblKarma.Text = LanguageManager.GetString("Label_Expense_NuyenAmount");
                    Text = LanguageManager.GetString(GetKarmaNuyenTitleKey(ExpenseType.Nuyen, _blnIsGain));
                    chkRefund.Text = LanguageManager.GetString("Checkbox_Expense_RefundNuyen");
                    nudPercent.Visible = true;
                    lblPercent.Visible = true;
                    chkRefund.Visible = true;
                    chkKarmaNuyenExchange.Visible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
                }
                else if (value == ExpenseType.Reputation)
                {
                    ApplyReputationModeLabels();
                    nudPercent.Visible = false;
                    lblPercent.Visible = false;
                    chkRefund.Visible = false;
                    chkKarmaNuyenExchange.Visible = false;
                    chkForceCareerVisible.Visible = false;
                }
                else
                {
                    lblKarma.Text = LanguageManager.GetString("Label_Expense_KarmaAmount");
                    Text = LanguageManager.GetString(GetKarmaNuyenTitleKey(ExpenseType.Karma, _blnIsGain));
                    nudPercent.Visible = false;
                    lblPercent.Visible = false;
                    chkRefund.Visible = true;
                    chkKarmaNuyenExchange.Visible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
                }
            }
        }

        public async Task SetModeAsync(ExpenseType value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSameMode = InterlockedExtensions.Exchange(ref _eMode, value) == value;
            string strAmountText;
            string strText;
            string strRefundText;
            bool blnPercentVisible;
            bool blnRefundVisible;
            bool blnExchangeVisible;
            if (value == ExpenseType.Nuyen)
            {
                strAmountText = await LanguageManager.GetStringAsync("Label_Expense_NuyenAmount", token: token).ConfigureAwait(false);
                strText = await LanguageManager.GetStringAsync(GetKarmaNuyenTitleKey(ExpenseType.Nuyen, _blnIsGain), token: token).ConfigureAwait(false);
                strRefundText = await LanguageManager.GetStringAsync("Checkbox_Expense_RefundNuyen", token: token).ConfigureAwait(false);
                blnPercentVisible = true;
                blnRefundVisible = true;
                blnExchangeVisible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
            }
            else if (value == ExpenseType.Reputation)
            {
                strAmountText = await GetReputationAmountLabelAsync(token).ConfigureAwait(false);
                strText = await GetReputationTitleAsync(token).ConfigureAwait(false);
                strRefundText = string.Empty;
                blnPercentVisible = false;
                blnRefundVisible = false;
                blnExchangeVisible = false;
            }
            else
            {
                strAmountText = await LanguageManager.GetStringAsync("Label_Expense_KarmaAmount", token: token).ConfigureAwait(false);
                strText = await LanguageManager.GetStringAsync(GetKarmaNuyenTitleKey(ExpenseType.Karma, _blnIsGain), token: token).ConfigureAwait(false);
                strRefundText = string.Empty;
                blnPercentVisible = false;
                blnRefundVisible = true;
                blnExchangeVisible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
            }
            await this.DoThreadSafeAsync(() =>
            {
                Text = strText;
                if (blnSameMode)
                    return;
                lblKarma.Text = strAmountText;
                chkRefund.Text = strRefundText;
                nudPercent.Visible = blnPercentVisible;
                lblPercent.Visible = blnPercentVisible;
                chkRefund.Visible = blnRefundVisible;
                chkKarmaNuyenExchange.Visible = blnExchangeVisible;
                chkForceCareerVisible.Visible = blnExchangeVisible;
            }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Configure the form for Karma or Nuyen gain/spend.
        /// </summary>
        /// <param name="eType">Karma or Nuyen.</param>
        /// <param name="blnGain">True for gained, false for spent.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task SetKarmaNuyenModeAsync(ExpenseType eType, bool blnGain, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _blnIsGain = blnGain;
            await SetModeAsync(eType, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Configure the form for a reputation track gain or loss.
        /// </summary>
        /// <param name="eTrack">Reputation track to adjust.</param>
        /// <param name="blnGain">True for gained, false for spent.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task SetReputationModeAsync(ReputationTrack eTrack, bool blnGain = true,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ReputationTrack = eTrack;
            _blnIsGain = blnGain;
            await SetModeAsync(ExpenseType.Reputation, token).ConfigureAwait(false);
        }

        private void ApplyReputationModeLabels()
        {
            lblKarma.Text = GetReputationAmountLabel();
            Text = GetReputationTitle();
        }

        private string GetReputationAmountLabel()
        {
            return LanguageManager.GetString(GetReputationAmountLabelKey(ReputationTrack));
        }

        private async Task<string> GetReputationAmountLabelAsync(CancellationToken token)
        {
            return await LanguageManager.GetStringAsync(GetReputationAmountLabelKey(ReputationTrack), token: token)
                                        .ConfigureAwait(false);
        }

        private string GetReputationTitle()
        {
            return LanguageManager.GetString(GetReputationTitleKey(ReputationTrack, _blnIsGain));
        }

        private async Task<string> GetReputationTitleAsync(CancellationToken token)
        {
            return await LanguageManager.GetStringAsync(GetReputationTitleKey(ReputationTrack, _blnIsGain), token: token)
                                        .ConfigureAwait(false);
        }

        private static string GetKarmaNuyenTitleKey(ExpenseType eType, bool blnGain)
        {
            if (eType == ExpenseType.Nuyen)
                return blnGain ? "Title_Expense_NuyenGained" : "Title_Expense_NuyenSpent";
            return blnGain ? "Title_Expense_KarmaGained" : "Title_Expense_KarmaSpent";
        }

        private static string GetReputationAmountLabelKey(ReputationTrack eTrack)
        {
            switch (eTrack)
            {
                case ReputationTrack.Notoriety:
                    return "Label_Expense_NotorietyAmount";
                case ReputationTrack.PublicAwareness:
                    return "Label_Expense_PublicAwarenessAmount";
                case ReputationTrack.AstralReputation:
                    return "Label_Expense_AstralReputationAmount";
                case ReputationTrack.WildReputation:
                    return "Label_Expense_WildReputationAmount";
                case ReputationTrack.SpiritIndex:
                    return "Label_Expense_SpiritIndexAmount";
                case ReputationTrack.WildIndex:
                    return "Label_Expense_WildIndexAmount";
                default:
                    return "Label_Expense_StreetCredAmount";
            }
        }

        private static string GetReputationTitleKey(ReputationTrack eTrack, bool blnGain)
        {
            string strSuffix = blnGain ? "Gained" : "Spent";
            switch (eTrack)
            {
                case ReputationTrack.Notoriety:
                    return "Title_Expense_Notoriety" + strSuffix;
                case ReputationTrack.PublicAwareness:
                    return "Title_Expense_PublicAwareness" + strSuffix;
                case ReputationTrack.AstralReputation:
                    return "Title_Expense_AstralReputation" + strSuffix;
                case ReputationTrack.WildReputation:
                    return "Title_Expense_WildReputation" + strSuffix;
                case ReputationTrack.SpiritIndex:
                    return "Title_Expense_SpiritIndex" + strSuffix;
                case ReputationTrack.WildIndex:
                    return "Title_Expense_WildIndex" + strSuffix;
                default:
                    return "Title_Expense_StreetCred" + strSuffix;
            }
        }

        public bool KarmaNuyenExchange { get; set; }
        public string KarmaNuyenExchangeString { get; set; }

        public bool IsInEditMode { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Lock fields on the Form so that only the Date and Reason fields are editable.
        /// </summary>
        public void LockFields(bool blnEditAmount = false)
        {
            nudAmount.Enabled = blnEditAmount;
            nudPercent.Enabled = false;
            chkRefund.Enabled = false;

            if (blnEditAmount && nudAmount.Minimum < 0)
                nudAmount.Minimum = nudAmount.Maximum * -1;
        }

        #endregion Methods

        private void chkKarmaNuyenExchange_CheckedChanged(object sender, EventArgs e)
        {
            if (chkKarmaNuyenExchange.Checked && !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString))
            {
                txtDescription.Text = KarmaNuyenExchangeString;
            }

            if (chkKarmaNuyenExchange.Checked && _eMode == ExpenseType.Nuyen)
            {
                nudAmount.Increment = _objCharacterSettings.NuyenPerBPWftP;
                nudAmount.Value = _objCharacterSettings.NuyenPerBPWftP;
            }
            else
            {
                nudAmount.Increment = 1;
            }

            chkForceCareerVisible.Enabled = chkKarmaNuyenExchange.Checked;
            if (!chkForceCareerVisible.Enabled)
            {
                chkForceCareerVisible.Checked = false;
            }
            KarmaNuyenExchange = chkKarmaNuyenExchange.Checked;
        }

        private async void CreateExpanse_Load(object sender, EventArgs e)
        {
            if (!IsInEditMode)
            {
                string strText = await LanguageManager.GetStringAsync("String_ExpenseDefault").ConfigureAwait(false);
                await txtDescription.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
            }
            bool blnShowExchange = _eMode != ExpenseType.Reputation
                                   && !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
            await chkKarmaNuyenExchange.DoThreadSafeAsync(x =>
            {
                x.Visible = blnShowExchange;
                x.Text = KarmaNuyenExchangeString;
            }).ConfigureAwait(false);
            await chkForceCareerVisible.DoThreadSafeAsync(x =>
            {
                x.Visible = blnShowExchange;
                x.Enabled = chkKarmaNuyenExchange.Checked;
            }).ConfigureAwait(false);
        }
    }
}
