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
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmSelectCalendarStart : Form
    {
        private int _intSelectedYear = 2072;
        private int _intSelectedWeek = 1;
        CalendarWeek objDefaultWeek = new CalendarWeek();

        #region Control Events
        public frmSelectCalendarStart()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        public frmSelectCalendarStart(CalendarWeek objWeek)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();

            nudYear.Value = objWeek.Year;
            nudMonth.Value = objWeek.Month;
            nudWeek.Value = objWeek.MonthWeek;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void nudMonth_ValueChanged(object sender, EventArgs e)
        {
            // All months have 4 weeks with the exception of months 3, 6, 9, and 12 which have 5 each.
            switch (Convert.ToInt32(nudMonth.Value))
            {
                case 3:
                case 6:
                case 9:
                case 12:
                    nudWeek.Maximum = 5;
                    break;
                default:
                    nudWeek.Maximum = 4;
                    break;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected values and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _intSelectedYear = Convert.ToInt32(nudYear.Value);
            int intMonth = Convert.ToInt32(nudMonth.Value);
            int intWeek = Convert.ToInt32(nudWeek.Value);

            // Calculate the week number based on the selected month and week combination.
            _intSelectedWeek = (intMonth - 1) * 4 + intWeek;
            // Correct the number of weeks since every third month has 5 weeks instead of 4.
            if (intMonth > 3)
                _intSelectedWeek++;
            if (intMonth > 6)
                _intSelectedWeek++;
            if (intMonth > 9)
                _intSelectedWeek++;

            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            nudYear.Left = lblYear.Left + lblYear.Width + 6;
            lblMonth.Left = nudYear.Left + nudYear.Width + 16;
            nudMonth.Left = lblMonth.Left + lblMonth.Width + 6;
            lblWeek.Left = nudMonth.Left + nudMonth.Width + 16;
            nudWeek.Left = lblWeek.Left + lblWeek.Width + 6;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Selected year.
        /// </summary>
        public int SelectedYear
        {
            get
            {
                return _intSelectedYear;
            }
        }

        /// <summary>
        /// Selected week.
        /// </summary>
        public int SelectedWeek
        {
            get
            {
                return _intSelectedWeek;
            }
        }
        #endregion
    }
}