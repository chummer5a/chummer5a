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
 using System.ComponentModel;
 using System.Drawing;
 using System.Linq;
 using System.Windows.Forms;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class PowerControl : UserControl
    {
        private Power _objPower;

        #region Control Events
        public PowerControl(Power objPower)
        {
            PowerObject = objPower;
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            nudRating.DataBindings.Add("Enabled", PowerObject, nameof(PowerObject.LevelsEnabled), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            nudRating.DataBindings.Add("Minimum", PowerObject, nameof(PowerObject.FreeLevels), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudRating.DataBindings.Add("Maximum", PowerObject, nameof(PowerObject.TotalMaximumLevels), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudRating.DataBindings.Add("Value", PowerObject, nameof(PowerObject.TotalRating), false,
                DataSourceUpdateMode.OnPropertyChanged);
            nudRating.DataBindings.Add("InterceptMouseWheel", PowerObject.CharacterObject.Options, 
                nameof(CharacterOptions.InterceptMode), false, DataSourceUpdateMode.OnPropertyChanged);
            lblPowerName.DataBindings.Add("Text", PowerObject, nameof(PowerObject.DisplayName), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            lblPowerPoints.DataBindings.Add("Text", PowerObject, nameof(PowerObject.DisplayPoints), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            lblActivation.DataBindings.Add("Text", PowerObject, nameof(PowerObject.DisplayAction), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkDiscountedAdeptWay.DataBindings.Add("Visible", PowerObject, nameof(PowerObject.AdeptWayDiscountEnabled), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkDiscountedAdeptWay.DataBindings.Add("Checked", PowerObject, nameof(PowerObject.DiscountedAdeptWay), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            chkDiscountedGeas.DataBindings.Add("Checked", PowerObject, nameof(PowerObject.DiscountedGeas), false, 
                DataSourceUpdateMode.OnPropertyChanged);
            
            PowerObject.PropertyChanged += Power_PropertyChanged;
            PowerObject.CharacterObject.PropertyChanged += Power_PropertyChanged;
            if (PowerObject.Name == "Improved Ability (skill)")
            {
                PowerObject.CharacterObject.SkillsSection.PropertyChanged += Power_PropertyChanged;
            }

            tipTooltip.SetToolTip(lblPowerPoints, PowerObject.ToolTip());
            MoveControls();
        }

        public void UnbindPowerControl()
        {
            PowerObject.PropertyChanged -= Power_PropertyChanged;
            PowerObject.CharacterObject.PropertyChanged -= Power_PropertyChanged;
            if (PowerObject.Name == "Improved Ability (skill)")
                PowerObject.CharacterObject.SkillsSection.PropertyChanged -= Power_PropertyChanged;
            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        private void Power_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            string strPropertyName = propertyChangedEventArgs?.PropertyName;
            if (strPropertyName == nameof(PowerObject.FreeLevels) || strPropertyName == nameof(PowerObject.TotalRating))
            {
                PowerObject.DisplayPoints = PowerObject.PowerPoints.ToString(GlobalOptions.CultureInfo);
                tipTooltip.SetToolTip(lblPowerPoints, PowerObject.ToolTip());
                cmdDelete.Enabled = PowerObject.FreeLevels == 0;
            }
            if (strPropertyName == nameof(PowerObject.Name))
            {
                PowerObject.CharacterObject.SkillsSection.PropertyChanged -= Power_PropertyChanged;
                if (PowerObject.Name == "Improved Ability (skill)")
                    PowerObject.CharacterObject.SkillsSection.PropertyChanged += Power_PropertyChanged;
            }
            // Super hacky solution, but we need all values updated properly if maxima change for any reason
            if (strPropertyName == nameof(PowerObject.TotalMaximumLevels))
            {
                nudRating.Maximum = PowerObject.TotalMaximumLevels;
            }
            else if (PowerObject.Name == "Improved Ability (skill)" || strPropertyName == nameof(PowerObject.CharacterObject.MAG.TotalValue) || strPropertyName == nameof(PowerObject.CharacterObject.MAGAdept.TotalValue))
            {
                PowerObject.ForceEvent(nameof(PowerObject.TotalMaximumLevels));
            }
        }

        private void PowerControl_Load(object sender, EventArgs e)
        {
            Width = cmdDelete.Left + cmdDelete.Width;
            cmdDelete.Enabled = PowerObject.FreeLevels == 0;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            //Cache the parentform prior to deletion, otherwise the relationship is broken.
            Form frmParent = ParentForm;
            if (PowerObject.FreeLevels > 0)
            {
                string strImprovementSourceName = PowerObject.CharacterObject.Improvements.FirstOrDefault(x => x.ImproveType == Improvement.ImprovementType.AdeptPowerFreePoints && x.ImprovedName == PowerObject.Name && x.UniqueName == PowerObject.Extra)?.SourceName;
                Gear objGear = PowerObject.CharacterObject.Gear.FirstOrDefault(x => x.Bonded && x.InternalId == strImprovementSourceName);
                if (objGear != null)
                {
                    objGear.Equipped = false;
                    objGear.Extra = string.Empty;
                }
            }
            PowerObject.Deleting = true;
            ImprovementManager.RemoveImprovements(PowerObject.CharacterObject, Improvement.ImprovementSource.Power, PowerObject.InternalId);
            PowerObject.CharacterObject.Powers.Remove(PowerObject);

            if (frmParent is CharacterShared objParent)
                objParent.IsCharacterUpdateRequested = true;
        }

        private void imgNotes_Click(object sender, EventArgs e)
        {
            frmNotes frmPowerNotes = new frmNotes
            {
                Notes = _objPower.Notes
            };
            frmPowerNotes.ShowDialog(this);

            if (frmPowerNotes.DialogResult == DialogResult.OK)
                _objPower.Notes = frmPowerNotes.Notes;

            string strTooltip = LanguageManager.GetString("Tip_Power_EditNotes", GlobalOptions.Language);
            if (!string.IsNullOrEmpty(_objPower.Notes))
                strTooltip += Environment.NewLine + Environment.NewLine + _objPower.Notes;
            tipTooltip.SetToolTip(imgNotes, strTooltip.WordWrap(100));
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Power object this control is linked to.
        /// </summary>
        public Power PowerObject
        {
            get => _objPower;
            set => _objPower = value;
        }

        /// <summary>
        /// Power name.
        /// </summary>
        public string PowerName
        {
            get => _objPower.Name;
            set => _objPower.Name = value;
        }

        /// <summary>
        /// Extra Power information (selected CharacterAttribute or Skill name).
        /// </summary>
        public string Extra
        {
            get => _objPower.Extra;
            set => _objPower.Extra = value;
        }

        #endregion

        #region Methods
        private void lblPowerName_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(_objPower.Source + ' ' + _objPower.Page(GlobalOptions.Language));
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

                if (cmdDelete.Left + cmdDelete.Width > Width)
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
