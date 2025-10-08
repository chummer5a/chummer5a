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

            datDate.Value = DateTime.Now;
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
        /// The Expense's mode (either Karma or Nuyen).
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
                    Text = LanguageManager.GetString("Title_Expense_Nuyen");
                    chkRefund.Text = LanguageManager.GetString("Checkbox_Expense_RefundNuyen");
                    nudPercent.Visible = true;
                    lblPercent.Visible = true;
                }
                else
                {
                    lblKarma.Text = LanguageManager.GetString("Label_Expense_KarmaAmount");
                    Text = LanguageManager.GetString("Title_Expense_Karma");
                    nudPercent.Visible = false;
                    lblPercent.Visible = false;
                }
            }
        }

        public async Task SetModeAsync(ExpenseType value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (InterlockedExtensions.Exchange(ref _eMode, value) == value)
                return;
            string strAmountText;
            string strText;
            string strRefundText;
            bool blnPercentVisible;
            if (value == ExpenseType.Nuyen)
            {
                strAmountText = await LanguageManager.GetStringAsync("Label_Expense_NuyenAmount", token: token).ConfigureAwait(false);
                strText = await LanguageManager.GetStringAsync("Title_Expense_Nuyen", token: token).ConfigureAwait(false);
                strRefundText = await LanguageManager.GetStringAsync("Checkbox_Expense_RefundNuyen", token: token).ConfigureAwait(false);
                blnPercentVisible = true;
            }
            else
            {
                strAmountText = await LanguageManager.GetStringAsync("Label_Expense_KarmaAmount", token: token).ConfigureAwait(false);
                strText = await LanguageManager.GetStringAsync("Title_Expense_Karma", token: token).ConfigureAwait(false);
                strRefundText = string.Empty;
                blnPercentVisible = false;
            }
            await this.DoThreadSafeAsync(() =>
            {
                lblKarma.Text = strAmountText;
                Text = strText;
                chkRefund.Text = strRefundText;
                nudPercent.Visible = blnPercentVisible;
                lblPercent.Visible = blnPercentVisible;
            }, token).ConfigureAwait(false);
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
            await chkKarmaNuyenExchange.DoThreadSafeAsync(x =>
            {
                x.Visible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
                x.Text = KarmaNuyenExchangeString;
            }).ConfigureAwait(false);
            await chkForceCareerVisible.DoThreadSafeAsync(x => x.Enabled = chkKarmaNuyenExchange.Checked).ConfigureAwait(false);
        }
    }
}
