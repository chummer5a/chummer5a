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
    public sealed partial class frmExpense : Form
    {
        private ExpenseType _objMode = ExpenseType.Karma;
        private readonly CharacterOptions _objCharacterOptions;

        #region Control Events
        public frmExpense(CharacterOptions objCharacterOptions)
        {
            _objCharacterOptions = objCharacterOptions;

            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            // Determine the DateTime format and use that to display the date field (removing seconds since they're not important).
            DateTimeFormatInfo objDateTimeInfo = GlobalOptions.CultureInfo.DateTimeFormat;
            datDate.CustomFormat = GlobalOptions.DatesIncludeTime ? objDateTimeInfo.FullDateTimePattern.FastEscapeOnceFromEnd(":ss") : objDateTimeInfo.LongDatePattern;
            datDate.Value = DateTime.Now;

            txtDescription.Text = LanguageManager.GetString("String_ExpenseDefault", GlobalOptions.Language);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (KarmaNuyenExchange && _objMode == ExpenseType.Nuyen && nudAmount.Value % _objCharacterOptions.NuyenPerBP != 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_KarmaNuyenExchange", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_KarmaNuyenExchange", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        #endregion

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
                    lblKarma.Text = LanguageManager.GetString("Label_Expense_NuyenAmount", GlobalOptions.Language);
                    Text = LanguageManager.GetString("Title_Expense_Nuyen", GlobalOptions.Language);
                    chkRefund.Text = LanguageManager.GetString("Checkbox_Expense_RefundNuyen", GlobalOptions.Language);
                    nudPercent.Visible = true;
                    lblPercent.Visible = true;
                }
                else
                {
                    lblKarma.Text = LanguageManager.GetString("Label_Expense_KarmaAmount", GlobalOptions.Language);
                    Text = LanguageManager.GetString("Title_Expense_Karma", GlobalOptions.Language);
                    nudPercent.Visible = false;
                    lblPercent.Visible = false;
                }
                _objMode = value;
            }
        }

        public bool KarmaNuyenExchange { get; set; }
        public string KarmaNuyenExchangeString { get; set; }

        #endregion

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
        #endregion

        private void chkKarmaNuyenExchange_CheckedChanged(object sender, EventArgs e)
        {
            if (chkKarmaNuyenExchange.Checked)
            {
                txtDescription.Text = KarmaNuyenExchangeString;
            }
            if (chkKarmaNuyenExchange.Checked && _objMode == ExpenseType.Nuyen)
            {
                nudAmount.Increment = _objCharacterOptions.NuyenPerBP;
                nudAmount.Value = _objCharacterOptions.NuyenPerBP;
            }
            else
            {
                nudAmount.Increment = 1;
            }
            KarmaNuyenExchange = chkKarmaNuyenExchange.Checked;
        }

        private void frmExpanse_Load(object sender, EventArgs e)
        {
            chkKarmaNuyenExchange.Text = KarmaNuyenExchangeString;
        }
    }
}
