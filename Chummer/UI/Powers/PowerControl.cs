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
using System.Drawing;
using System.Windows.Forms;

// PowerRatingChanged Event Handler.
public delegate void PowerRatingChangedHandler(Object sender);
// DeletePower Event Handler;
public delegate void DeletePowerHandler(Object sender);

namespace Chummer
{
    public partial class PowerControl : UserControl
    {
		private Power _objPower;
	    private Form _parent;

	    // Events.
        public event PowerRatingChangedHandler PowerRatingChanged;
        public event DeletePowerHandler DeletePower;


		#region Control Events
		public PowerControl(Power objPower)
		{
			this.PowerObject = objPower;
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			nudRating.DataBindings.Add("Enabled", PowerObject, nameof(PowerObject.LevelsEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
			if (PowerObject.LevelsEnabled)
			{
				nudRating.DataBindings.Add("Minimum", PowerObject, nameof(PowerObject.FreeLevels), false,
					DataSourceUpdateMode.OnPropertyChanged);
				nudRating.DataBindings.Add("Maximum", PowerObject, nameof(PowerObject.TotalMaximumLevels), false,
					DataSourceUpdateMode.OnPropertyChanged);
				nudRating.DataBindings.Add("Value", PowerObject, nameof(PowerObject.Rating), false,
					DataSourceUpdateMode.OnPropertyChanged);
				
			}
			lblPowerName.DataBindings.Add("Text", PowerObject, nameof(PowerObject.DisplayName), false, DataSourceUpdateMode.OnPropertyChanged);
			lblPowerPoints.DataBindings.Add("Text", PowerObject, nameof(PowerObject.PowerPoints), false, DataSourceUpdateMode.OnPropertyChanged);
			lblActivation.DataBindings.Add("Text", PowerObject, nameof(PowerObject.DisplayAction), false, DataSourceUpdateMode.OnPropertyChanged);
			chkDiscountedAdeptWay.DataBindings.Add("Visible", PowerObject, nameof(PowerObject.AdeptWayDiscountEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
			chkDiscountedAdeptWay.DataBindings.Add("Checked", PowerObject, nameof(PowerObject.DiscountedAdeptWay), false, DataSourceUpdateMode.OnPropertyChanged);
			chkDiscountedGeas.DataBindings.Add("Checked", PowerObject, nameof(PowerObject.DiscountedGeas), false, DataSourceUpdateMode.OnPropertyChanged);

			tipTooltip.SetToolTip(lblPowerPoints, PowerObject.ToolTip());
			MoveControls();
        }

		private void PowerControl_Load(object sender, EventArgs e)
		{
			this.Width = cmdDelete.Left + cmdDelete.Width;
        }

		private void cmdDelete_Click(object sender, EventArgs e)
		{
			PowerObject.CharacterObject.Powers.Remove(PowerObject);
			RequestCharacterUpdate();
		}

		private void imgNotes_Click(object sender, EventArgs e)
		{
			frmNotes frmPowerNotes = new frmNotes();
			frmPowerNotes.Notes = _objPower.Notes;
			frmPowerNotes.ShowDialog(this);

			if (frmPowerNotes.DialogResult == DialogResult.OK)
				_objPower.Notes = frmPowerNotes.Notes;

			string strTooltip = LanguageManager.Instance.GetString("Tip_Power_EditNotes");
			if (_objPower.Notes != string.Empty)
				strTooltip += "\n\n" + _objPower.Notes;
			tipTooltip.SetToolTip(imgNotes, CommonFunctions.WordWrap(strTooltip, 100));
		}
		#endregion

		#region Properties
		/// <summary>
		/// The Power object this control is linked to.
		/// </summary>
		public Power PowerObject
		{
			get
			{
				return _objPower;
			}
			set
			{
				_objPower = value;
			}
		}

        /// <summary>
        /// Power name.
        /// </summary>
        public string PowerName
        {
            get
            {
				return _objPower.Name;
            }
            set
            {
				_objPower.Name = value;
            }
        }

        /// <summary>
        /// Extra Power information (selected CharacterAttribute or Skill name).
        /// </summary>
        public string Extra
        {
            get
            {
				return _objPower.Extra;
            }
            set
            {
				_objPower.Extra = value;
            }
        }

        /// <summary>
        /// Power level.
        /// </summary>
        public int PowerLevel
        {
            get
            {
                return Convert.ToInt32(nudRating.Value);
            }
            set
            {
				_objPower.Rating = value;
                if (_objPower.Rating > nudRating.Maximum) 
                {
                   value = Convert.ToInt32(nudRating.Maximum);
                }
                nudRating.Value = value;
	            RequestCharacterUpdate();
				MoveControls();
            }
        }

		#endregion

		#region Methods
		private void RequestCharacterUpdate(object sender = null, EventArgs e = null)
		{
			if (_objPower.CharacterObject.Created)
			{
				frmCareer parent = ParentForm as frmCareer;
				parent.UpdateCharacterInfo();
			}
			else
			{
				frmCreate parent = ParentForm as frmCreate;
				parent.UpdateCharacterInfo();
			}
		}
		private void lblPowerName_Click(object sender, EventArgs e)
		{
			string strBook = _objPower.Source + " " + _objPower.Page;
			CommonFunctions objCommon = new CommonFunctions();
			objCommon.OpenPDF(strBook);
		}

		private void MoveControls()
	    {
		    while (true)
		    {
			    if (lblPowerName.Font.Size < 8.25f)
			    {
				    for (float i = 8.25f; i >= lblPowerName.Font.Size; i -= 0.25f)
						lblActivation.Left -= 1;
			    }

			    nudRating.Left = lblActivation.Left + lblActivation.Width + 6;
			    lblPowerPoints.Left = nudRating.Left + nudRating.Width + 6;
			    chkDiscountedAdeptWay.Left = lblPowerPoints.Left + lblPowerPoints.Width + 6;
			    if (chkDiscountedGeas.Visible)
			    {
				    chkDiscountedGeas.Left = chkDiscountedAdeptWay.Left + chkDiscountedAdeptWay.Width + 6;
					imgNotes.Left = chkDiscountedGeas.Left + chkDiscountedGeas.Width + 6;
				}
			    else
				{
					imgNotes.Left = chkDiscountedAdeptWay.Left + chkDiscountedAdeptWay.Width + 6;
				}
			    cmdDelete.Left = imgNotes.Left + imgNotes.Width + 6;

			    if (cmdDelete.Left + cmdDelete.Width > this.Width)
			    {
				    lblPowerName.Font = new Font(lblPowerName.Font.FontFamily.Name, lblPowerName.Font.Size - 0.25f);
				    nudRating.Font = lblPowerName.Font;
				    lblPowerPoints.Font = lblPowerName.Font;
				    chkDiscountedAdeptWay.Font = lblPowerName.Font;
				    chkDiscountedGeas.Font = lblPowerName.Font;
				    cmdDelete.Font = lblPowerName.Font;
				    continue;
			    }
			    break;
		    }
	    }

	    #endregion
	}
}