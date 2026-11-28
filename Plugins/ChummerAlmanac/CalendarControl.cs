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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// In-game calendar of a character: current date and time, a place, sunrise and sunset, the phase of the moon
    /// and a note for each day.
    /// </summary>
    public sealed class CalendarControl : UserControl
    {
        private readonly Character _objCharacter;
        private readonly CharacterShared _frmCharacter;
        private readonly AlmanacData _objData;

        private readonly DateTimePicker _dtpNow;
        private readonly ComboBox _cboLocation;
        private readonly NumericUpDown _nudLatitude;
        private readonly NumericUpDown _nudLongitude;
        private readonly ComboBox _cboTimeZone;
        private readonly MonthCalendar _calMonth;
        private readonly Label _lblDay;
        private readonly Label _lblSunrise;
        private readonly Label _lblSunset;
        private readonly Label _lblDayLength;
        private readonly Label _lblMoon;
        private readonly TextBox _txtNotes;

        // True while the control sets its own fields, so that change events are not mistaken for user input.
        private bool _blnLoading;

        // False until the month view has been moved to the current in-game day for the first time.
        private bool _blnDaySelected;

        // Entry of the place list that stands for "coordinates entered by hand".
        private readonly string _strCustomLocation = AlmanacStrings.Get("Location_Custom");

        public CalendarControl(CharacterShared frmCharacter)
        {
            _frmCharacter = frmCharacter ?? throw new ArgumentNullException(nameof(frmCharacter));
            _objCharacter = frmCharacter.CharacterObject;
            _objData = AlmanacPlugin.GetData(_objCharacter);

            SuspendLayout();

            // Row 1: current in-game date and time
            _dtpNow = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = AlmanacFormat.DateTimePickerFormat,
                Width = 170
            };
            _dtpNow.ValueChanged += Now_ValueChanged;
            Button cmdAddHour = CreateButton(AlmanacStrings.Get("Button_AddHour"));
            cmdAddHour.Click += (sender, e) => ShiftNow(TimeSpan.FromHours(1));
            Button cmdAddDay = CreateButton(AlmanacStrings.Get("Button_AddDay"));
            cmdAddDay.Click += (sender, e) => ShiftNow(TimeSpan.FromDays(1));
            Button cmdShowToday = CreateButton(AlmanacStrings.Get("Button_ShowCurrentDay"));
            cmdShowToday.Click += (sender, e) => SelectDay(_dtpNow.Value.Date);

            FlowLayoutPanel flpNow = CreateRow();
            flpNow.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_CurrentDateTime")));
            flpNow.Controls.Add(_dtpNow);
            flpNow.Controls.Add(cmdAddHour);
            flpNow.Controls.Add(cmdAddDay);
            flpNow.Controls.Add(cmdShowToday);

            // Row 2: place
            _cboLocation = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150
            };
            foreach (AlmanacLocation objPreset in AlmanacLocation.Presets)
                _cboLocation.Items.Add(objPreset);
            _cboLocation.Items.Add(_strCustomLocation);
            _cboLocation.SelectedIndexChanged += Location_SelectedIndexChanged;

            _nudLatitude = CreateCoordinateBox(-90, 90);
            _nudLatitude.ValueChanged += Coordinates_Changed;
            _nudLongitude = CreateCoordinateBox(-180, 180);
            _nudLongitude.ValueChanged += Coordinates_Changed;

            _cboTimeZone = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300
            };
            foreach (TimeZoneInfo objTimeZone in TimeZoneInfo.GetSystemTimeZones())
                _cboTimeZone.Items.Add(objTimeZone);
            _cboTimeZone.SelectedIndexChanged += Coordinates_Changed;

            FlowLayoutPanel flpLocation = CreateRow();
            flpLocation.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Location")));
            flpLocation.Controls.Add(_cboLocation);
            flpLocation.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Latitude")));
            flpLocation.Controls.Add(_nudLatitude);
            flpLocation.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Longitude")));
            flpLocation.Controls.Add(_nudLongitude);
            flpLocation.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_TimeZone")));
            flpLocation.Controls.Add(_cboTimeZone);

            // Left: month view
            _calMonth = new MonthCalendar
            {
                MaxSelectionCount = 1,
                ShowToday = false,
                ShowTodayCircle = false,
                Margin = new Padding(6)
            };
            _calMonth.DateChanged += (sender, e) =>
            {
                if (!_blnLoading)
                    RefreshDay();
            };

            // Right: details of the selected day
            _lblDay = new Label
            {
                AutoSize = true,
                Padding = new Padding(3, 6, 3, 6),
                Font = new Font(Font, FontStyle.Bold)
            };
            _lblSunrise = CreateValueLabel();
            _lblSunset = CreateValueLabel();
            _lblDayLength = CreateValueLabel();
            _lblMoon = CreateValueLabel();
            _txtNotes = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                AcceptsReturn = true,
                ScrollBars = ScrollBars.Vertical
            };
            _txtNotes.TextChanged += Notes_TextChanged;

            TableLayoutPanel tlpDay = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7
            };
            tlpDay.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpDay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; ++i)
                tlpDay.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpDay.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpDay.Controls.Add(_lblDay, 0, 0);
            tlpDay.SetColumnSpan(_lblDay, 2);
            tlpDay.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Sunrise")), 0, 1);
            tlpDay.Controls.Add(_lblSunrise, 1, 1);
            tlpDay.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Sunset")), 0, 2);
            tlpDay.Controls.Add(_lblSunset, 1, 2);
            tlpDay.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_DayLength")), 0, 3);
            tlpDay.Controls.Add(_lblDayLength, 1, 3);
            tlpDay.Controls.Add(CreateRowLabel(AlmanacStrings.Get("Label_Moon")), 0, 4);
            tlpDay.Controls.Add(_lblMoon, 1, 4);
            Label lblNotes = CreateRowLabel(AlmanacStrings.Get("Label_Notes"));
            tlpDay.Controls.Add(lblNotes, 0, 5);
            tlpDay.SetColumnSpan(lblNotes, 2);
            tlpDay.Controls.Add(_txtNotes, 0, 6);
            tlpDay.SetColumnSpan(_txtNotes, 2);

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpMain.Controls.Add(flpNow, 0, 0);
            tlpMain.SetColumnSpan(flpNow, 2);
            tlpMain.Controls.Add(flpLocation, 0, 1);
            tlpMain.SetColumnSpan(flpLocation, 2);
            tlpMain.Controls.Add(_calMonth, 0, 2);
            tlpMain.Controls.Add(tlpDay, 1, 2);

            Controls.Add(tlpMain);
            ResumeLayout(false);
            PerformLayout();
        }

        #region Building blocks

        private static FlowLayoutPanel CreateRow()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(3)
            };
        }

        private static Label CreateRowLabel(string strText)
        {
            return new Label
            {
                AutoSize = true,
                Text = strText,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };
        }

        private static Label CreateValueLabel()
        {
            return new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            };
        }

        private static Button CreateButton(string strText)
        {
            return new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Text = strText
            };
        }

        private static NumericUpDown CreateCoordinateBox(int intMinimum, int intMaximum)
        {
            return new NumericUpDown
            {
                Minimum = intMinimum,
                Maximum = intMaximum,
                DecimalPlaces = 4,
                Increment = 0.1m,
                Width = 90
            };
        }

        #endregion Building blocks

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                LoadFromData();
        }

        /// <summary>
        /// Show what is stored for the character.
        /// </summary>
        private void LoadFromData()
        {
            DateTime dtNow = AlmanacFormat.ClampToPicker(GameClock.GetCurrentDateTime(_objCharacter, _objData));
            AlmanacLocation objLocation = GameClock.GetLocation(_objData);
            _blnLoading = true;
            try
            {
                _dtpNow.Value = dtNow;
                ShowLocation(objLocation);
            }
            finally
            {
                _blnLoading = false;
            }
            if (_blnDaySelected)
            {
                // Keep the day the user was looking at; only what may have changed elsewhere is refreshed.
                UpdateBoldedDates();
                RefreshDay();
            }
            else
            {
                _blnDaySelected = true;
                SelectDay(dtNow.Date);
            }
        }

        // Only call while _blnLoading is set.
        private void ShowLocation(AlmanacLocation objLocation)
        {
            int intPreset = -1;
            for (int i = 0; i < AlmanacLocation.Presets.Count; ++i)
            {
                AlmanacLocation objPreset = AlmanacLocation.Presets[i];
                if (objPreset.Name == objLocation.Name
                    && Math.Abs(objPreset.Latitude - objLocation.Latitude) < 0.00005
                    && Math.Abs(objPreset.Longitude - objLocation.Longitude) < 0.00005
                    && objPreset.TimeZoneId == objLocation.TimeZoneId)
                {
                    intPreset = i;
                    break;
                }
            }
            _cboLocation.SelectedIndex = intPreset >= 0 ? intPreset : _cboLocation.Items.Count - 1;
            _nudLatitude.Value = (decimal)objLocation.Latitude;
            _nudLongitude.Value = (decimal)objLocation.Longitude;
            SelectTimeZone(objLocation.GetTimeZone());
        }

        // Only call while _blnLoading is set.
        private void SelectTimeZone(TimeZoneInfo objTimeZone)
        {
            for (int i = 0; i < _cboTimeZone.Items.Count; ++i)
            {
                if (_cboTimeZone.Items[i] is TimeZoneInfo objItem && objItem.Id == objTimeZone.Id)
                {
                    _cboTimeZone.SelectedIndex = i;
                    return;
                }
            }
            _cboTimeZone.SelectedIndex = _cboTimeZone.Items.Add(objTimeZone);
        }

        /// <summary>
        /// The place as it is currently entered in the fields.
        /// </summary>
        private AlmanacLocation GetLocationFromFields()
        {
            string strName = _cboLocation.SelectedItem is AlmanacLocation objPreset
                ? objPreset.Name
                : AlmanacLocation.CustomName;
            string strTimeZoneId = _cboTimeZone.SelectedItem is TimeZoneInfo objTimeZone
                ? objTimeZone.Id
                : TimeZoneInfo.Utc.Id;
            return new AlmanacLocation(strName, (double)_nudLatitude.Value, (double)_nudLongitude.Value, strTimeZoneId);
        }

        private void MarkChanged()
        {
            // Everything shown here is stored in the character's save file.
            _frmCharacter.IsDirty = true;
        }

        #region Events

        private void Now_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _objData.CurrentDateTime = _dtpNow.Value;
            MarkChanged();
        }

        private void ShiftNow(TimeSpan objSpan)
        {
            // Setting the value raises ValueChanged, which stores the new time.
            _dtpNow.Value = AlmanacFormat.ClampToPicker(_dtpNow.Value.Add(objSpan));
            SelectDay(_dtpNow.Value.Date);
        }

        private void Location_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_cboLocation.SelectedItem is AlmanacLocation objPreset)
            {
                _blnLoading = true;
                try
                {
                    _nudLatitude.Value = (decimal)objPreset.Latitude;
                    _nudLongitude.Value = (decimal)objPreset.Longitude;
                    SelectTimeZone(objPreset.GetTimeZone());
                }
                finally
                {
                    _blnLoading = false;
                }
            }
            StoreLocation();
        }

        private void Coordinates_Changed(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            // Coordinates or time zone edited by hand: this is no longer one of the listed places.
            _blnLoading = true;
            try
            {
                _cboLocation.SelectedIndex = _cboLocation.Items.Count - 1;
            }
            finally
            {
                _blnLoading = false;
            }
            StoreLocation();
        }

        private void StoreLocation()
        {
            _objData.Location = GetLocationFromFields();
            MarkChanged();
            RefreshDay();
        }

        private void Notes_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            DateTime dtDay = _calMonth.SelectionStart.Date;
            bool blnHadNote = !string.IsNullOrWhiteSpace(_objData.GetNote(dtDay));
            _objData.SetNote(dtDay, _txtNotes.Text);
            MarkChanged();
            if (blnHadNote != !string.IsNullOrWhiteSpace(_txtNotes.Text))
                UpdateBoldedDates();
        }

        #endregion Events

        private void SelectDay(DateTime dtDay)
        {
            _blnLoading = true;
            try
            {
                if (dtDay < _calMonth.MinDate)
                    dtDay = _calMonth.MinDate;
                else if (dtDay > _calMonth.MaxDate)
                    dtDay = _calMonth.MaxDate;
                _calMonth.SetDate(dtDay);
                UpdateBoldedDates();
            }
            finally
            {
                _blnLoading = false;
            }
            RefreshDay();
        }

        /// <summary>
        /// Days with a note are shown in bold in the month view.
        /// </summary>
        private void UpdateBoldedDates()
        {
            _calMonth.BoldedDates = _objData.GetNoteDays();
        }

        /// <summary>
        /// Show sun, moon and note of the day selected in the month view.
        /// </summary>
        private void RefreshDay()
        {
            DateTime dtDay = _calMonth.SelectionStart.Date;
            AlmanacLocation objLocation = GetLocationFromFields();
            TimeZoneInfo objTimeZone = objLocation.GetTimeZone();
            CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;

            _lblDay.Text = dtDay.ToString("D", objCulture);

            SunTimes objTimes = Astronomy.GetSunTimes(dtDay, objLocation.Latitude, objLocation.Longitude);
            if (objTimes.DayType == SunDayType.Normal && objTimes.SunriseUtc.HasValue && objTimes.SunsetUtc.HasValue)
            {
                DateTime dtSunrise = TimeZoneInfo.ConvertTimeFromUtc(objTimes.SunriseUtc.Value, objTimeZone);
                DateTime dtSunset = TimeZoneInfo.ConvertTimeFromUtc(objTimes.SunsetUtc.Value, objTimeZone);
                _lblSunrise.Text = dtSunrise.ToString("t", objCulture);
                _lblSunset.Text = dtSunset.ToString("t", objCulture);
                TimeSpan objDayLength = objTimes.SunsetUtc.Value - objTimes.SunriseUtc.Value;
                _lblDayLength.Text = string.Format(objCulture, AlmanacStrings.Get("String_HoursMinutes"),
                    (int)Math.Floor(objDayLength.TotalHours), objDayLength.Minutes);
            }
            else
            {
                string strPolar = AlmanacStrings.Get(objTimes.DayType == SunDayType.PolarNight
                    ? "String_PolarNight"
                    : "String_PolarDay");
                _lblSunrise.Text = strPolar;
                _lblSunset.Text = strPolar;
                _lblDayLength.Text = string.Empty;
            }

            // Moon at noon local time of that day.
            DateTime dtNoonUtc;
            try
            {
                dtNoonUtc = TimeZoneInfo.ConvertTimeToUtc(
                    new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, 12, 0, 0, DateTimeKind.Unspecified), objTimeZone);
            }
            catch (ArgumentException)
            {
                dtNoonUtc = new DateTime(dtDay.Year, dtDay.Month, dtDay.Day, 12, 0, 0, DateTimeKind.Utc);
            }
            _lblMoon.Text = string.Format(objCulture, AlmanacStrings.Get("String_MoonPhase"),
                AlmanacStrings.Get("Moon_" + Astronomy.GetMoonPhaseIndex(dtNoonUtc).ToString(CultureInfo.InvariantCulture)),
                (int)Math.Round(Astronomy.GetMoonIllumination(dtNoonUtc) * 100.0));

            _blnLoading = true;
            try
            {
                _txtNotes.Text = AlmanacFormat.ToTextBoxLines(_objData.GetNote(dtDay));
            }
            finally
            {
                _blnLoading = false;
            }
        }
    }
}
