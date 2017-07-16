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
﻿using System;
using System.Globalization;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmExpense : Form
    {
        private ExpenseType _objMode = ExpenseType.Karma;

        #region Control Events
        public frmExpense()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

            // Determine the DateTime format and use that to display the date field (removing seconds since they're not important).
            DateTimeFormatInfo objDateTimeInfo = GlobalOptions.CultureInfo.DateTimeFormat;
            string strDatePattern = objDateTimeInfo.FullDateTimePattern.Replace(":ss", string.Empty);
            if (!GlobalOptions.Instance.DatesIncludeTime)
                strDatePattern = objDateTimeInfo.LongDatePattern;
            datDate.CustomFormat = strDatePattern;
            datDate.Value = DateTime.Now;

            txtDescription.Text = LanguageManager.Instance.GetString("String_ExpenseDefault");
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (KarmaNuyenExchange && _objMode == ExpenseType.Nuyen && nudAmount.Value % 2000 != 0)
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_KarmaNuyenExchange"), LanguageManager.Instance.GetString("MessageTitle_KarmaNuyenExchange"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        public int Amount
        {
            get
            {
                decimal decReturn = nudAmount.Value;
                if (_objMode == ExpenseType.Nuyen)
                    decReturn *= (nudPercent.Value / 100);
                return Convert.ToInt32(Math.Round(decReturn, 0));
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
            get
            {
                return txtDescription.Text;
            }
            set
            {
                txtDescription.Text = value;
            }
        }

        /// <summary>
        /// Whether or not this is a Karma refund.
        /// </summary>
        public bool Refund
        {
            get
            {
                return chkRefund.Checked;
            }
            set
            {
                chkRefund.Checked = value;
            }
        }

        /// <summary>
        /// Date and Time that was selected.
        /// </summary>
        public DateTime SelectedDate
        {
            get
            {
                return datDate.Value;
            }
            set
            {
                datDate.Value = value;
            }
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
                    lblKarma.Text = LanguageManager.Instance.GetString("Label_Expense_NuyenAmount");
                    Text = LanguageManager.Instance.GetString("Title_Expense_Nuyen");
                    chkRefund.Text = LanguageManager.Instance.GetString("Checkbox_Expense_RefundNuyen");
                    nudPercent.Visible = true;
                    lblPercent.Visible = true;
                }
                else
                {
                    lblKarma.Text = LanguageManager.Instance.GetString("Label_Expense_KarmaAmount");
                    Text = LanguageManager.Instance.GetString("Title_Expense_Karma");
                    nudPercent.Visible = false;
                    lblPercent.Visible = false;
                }
                _objMode = value;
            }
        }

        public bool KarmaNuyenExchange { get; set; }
        public string KarmaNuyenExchangeString { get; internal set; }

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
                nudAmount.Increment = 2000;
                nudAmount.Value = 2000;
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