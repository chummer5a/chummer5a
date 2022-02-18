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
using System.Windows.Forms;

namespace Chummer
{
    public sealed partial class CreateExpense : Form
    {
        private ExpenseType _objMode = ExpenseType.Karma;
        private readonly CharacterSettings _objCharacterSettings;

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
            if (KarmaNuyenExchange && _objMode == ExpenseType.Nuyen && nudAmount.Value % _objCharacterSettings.NuyenPerBPWftP != 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_KarmaNuyenExchange"),
                    await LanguageManager.GetStringAsync("MessageTitle_KarmaNuyenExchange"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Amount gained or spent.
        /// </summary>
        public decimal Amount
        {
            get
            {
                decimal decReturn = nudAmount.Value;
                if (_objMode == ExpenseType.Nuyen)
                    decReturn *= (nudPercent.Value / 100.0m);
                return decReturn;
            }
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
            get => txtDescription.Text;
            set => txtDescription.Text = value;
        }

        /// <summary>
        /// Whether or not this is a Karma refund.
        /// </summary>
        public bool Refund
        {
            get => chkRefund.Checked;
            set => chkRefund.Checked = value;
        }

        /// <summary>
        /// Whether or not this is a Karma refund.
        /// </summary>
        public bool ForceCareerVisible
        {
            get => chkForceCareerVisible.Checked;
            set => chkForceCareerVisible.Checked = value;
        }

        /// <summary>
        /// Date and Time that was selected.
        /// </summary>
        public DateTime SelectedDate
        {
            get => datDate.Value;
            set => datDate.Value = value;
        }

        /// <summary>
        /// The Expense's mode (either Karma or Nuyen).
        /// </summary>
        public ExpenseType Mode
        {
            set
            {
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
                _objMode = value;
            }
        }

        public bool KarmaNuyenExchange { get; set; }
        public string KarmaNuyenExchangeString { get; set; }

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

            if (chkKarmaNuyenExchange.Checked && _objMode == ExpenseType.Nuyen)
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
            txtDescription.Text = await LanguageManager.GetStringAsync("String_ExpenseDefault");
            chkKarmaNuyenExchange.Visible = !string.IsNullOrWhiteSpace(KarmaNuyenExchangeString);
            chkKarmaNuyenExchange.Text = KarmaNuyenExchangeString;
            chkForceCareerVisible.Enabled = chkKarmaNuyenExchange.Checked;
        }
    }
}
