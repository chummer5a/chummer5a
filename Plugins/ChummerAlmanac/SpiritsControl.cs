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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Chummer;

namespace ChummerAlmanac
{
    /// <summary>
    /// Shows the spirits and sprites of a character with their attributes, powers and skills at their Force
    /// and lets the user choose their optional powers.
    /// </summary>
    public sealed class SpiritsControl : UserControl
    {
        private readonly Character _objCharacter;
        private readonly CharacterShared _frmCharacter;
        private readonly AlmanacData _objData;

        private readonly ListView _lstSpirits;
        private readonly Label _lblTitle;
        private readonly ListView _lstAttributes;
        private readonly ListBox _lstPowers;
        private readonly GroupBox _gpbOptionalPowers;
        private readonly CheckedListBox _clbOptionalPowers;
        private readonly ListView _lstSkills;

        // True while the control fills its lists itself, so that change events are not mistaken for user input.
        private bool _blnLoading;
        private Spirit _objSelectedSpirit;
        private SpiritStats _objSelectedStats;

        public SpiritsControl(CharacterShared frmCharacter)
        {
            _frmCharacter = frmCharacter ?? throw new ArgumentNullException(nameof(frmCharacter));
            _objCharacter = frmCharacter.CharacterObject;
            _objData = AlmanacPlugin.GetData(_objCharacter);

            SuspendLayout();

            // Left side: list of spirits
            Button cmdRefresh = new Button
            {
                AutoSize = true,
                Text = AlmanacStrings.Get("Button_Refresh"),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            cmdRefresh.Click += (sender, e) => RefreshSpiritList();

            _lstSpirits = CreateListView();
            _lstSpirits.Columns.Add(AlmanacStrings.Get("Column_Spirit"), 190);
            _lstSpirits.Columns.Add(AlmanacStrings.Get("Column_Force"), 50, HorizontalAlignment.Right);
            _lstSpirits.Columns.Add(AlmanacStrings.Get("Column_Services"), 60, HorizontalAlignment.Right);
            _lstSpirits.Columns.Add(AlmanacStrings.Get("Column_Bound"), 70);
            _lstSpirits.SelectedIndexChanged += (sender, e) =>
            {
                if (!_blnLoading)
                    ShowSelectedSpirit();
            };

            TableLayoutPanel tlpLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            tlpLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpLeft.Controls.Add(cmdRefresh, 0, 0);
            tlpLeft.Controls.Add(_lstSpirits, 0, 1);

            // Right side: details of the selected spirit
            _lblTitle = new Label
            {
                AutoSize = true,
                Padding = new Padding(3, 6, 3, 6),
                Font = new Font(Font, FontStyle.Bold)
            };

            _lstAttributes = CreateListView();
            _lstAttributes.Columns.Add(AlmanacStrings.Get("Column_Attribute"), 160);
            _lstAttributes.Columns.Add(AlmanacStrings.Get("Column_Value"), 100);

            _lstPowers = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                SelectionMode = SelectionMode.None
            };

            _clbOptionalPowers = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                CheckOnClick = true
            };
            _clbOptionalPowers.ItemCheck += OptionalPowers_ItemCheck;

            _lstSkills = CreateListView();
            _lstSkills.Columns.Add(AlmanacStrings.Get("Column_Skill"), 170);
            _lstSkills.Columns.Add(AlmanacStrings.Get("Column_LinkedAttribute"), 110);
            _lstSkills.Columns.Add(AlmanacStrings.Get("Column_DicePool"), 80, HorizontalAlignment.Right);

            _gpbOptionalPowers = CreateGroupBox(AlmanacStrings.Get("Group_OptionalPowers"), _clbOptionalPowers);

            TableLayoutPanel tlpRight = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlpRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            tlpRight.Controls.Add(_lblTitle, 0, 0);
            tlpRight.SetColumnSpan(_lblTitle, 2);
            tlpRight.Controls.Add(CreateGroupBox(AlmanacStrings.Get("Group_Attributes"), _lstAttributes), 0, 1);
            tlpRight.Controls.Add(CreateGroupBox(AlmanacStrings.Get("Group_Powers"), _lstPowers), 1, 1);
            tlpRight.Controls.Add(CreateGroupBox(AlmanacStrings.Get("Group_Skills"), _lstSkills), 0, 2);
            tlpRight.Controls.Add(_gpbOptionalPowers, 1, 2);

            TableLayoutPanel tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpMain.Controls.Add(tlpLeft, 0, 0);
            tlpMain.Controls.Add(tlpRight, 1, 0);

            Controls.Add(tlpMain);
            ResumeLayout(false);
            PerformLayout();
        }

        private static ListView CreateListView()
        {
            return new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
        }

        private static GroupBox CreateGroupBox(string strText, Control objContent)
        {
            GroupBox gpbReturn = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = strText
            };
            gpbReturn.Controls.Add(objContent);
            return gpbReturn;
        }

        /// <inheritdoc />
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            // Spirits can be added or changed on Chummer's own tabs, so read them again whenever this tab is shown.
            if (Visible)
                RefreshSpiritList();
        }

        private void RefreshSpiritList()
        {
            string strSelectedId = _objSelectedSpirit?.InternalId;
            _blnLoading = true;
            try
            {
                CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
                HashSet<string> setSpiritIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _lstSpirits.BeginUpdate();
                try
                {
                    _lstSpirits.Items.Clear();
                    foreach (Spirit objSpirit in _objCharacter.Spirits)
                    {
                        if (objSpirit == null)
                            continue;
                        setSpiritIds.Add(objSpirit.InternalId);
                        ListViewItem objItem = new ListViewItem(objSpirit.CurrentDisplayName)
                        {
                            Tag = objSpirit
                        };
                        objItem.SubItems.Add(objSpirit.Force.ToString(objCulture));
                        objItem.SubItems.Add(objSpirit.ServicesOwed.ToString(objCulture));
                        objItem.SubItems.Add(AlmanacStrings.Get(objSpirit.Bound ? "String_Yes" : "String_No"));
                        _lstSpirits.Items.Add(objItem);
                        if (objSpirit.InternalId == strSelectedId)
                            objItem.Selected = true;
                    }
                    if (_lstSpirits.SelectedItems.Count == 0 && _lstSpirits.Items.Count > 0)
                        _lstSpirits.Items[0].Selected = true;
                }
                finally
                {
                    _lstSpirits.EndUpdate();
                }
                // Choices made for spirits that have been removed from the character are not needed any more.
                _objData.RemoveAllExcept(setSpiritIds);
            }
            catch (Exception e)
            {
                ShowMessage(string.Format(CultureInfo.CurrentCulture, AlmanacStrings.Get("Message_Error"), e.Message));
                return;
            }
            finally
            {
                _blnLoading = false;
            }
            ShowSelectedSpirit();
        }

        private void ShowSelectedSpirit()
        {
            Spirit objSpirit = _lstSpirits.SelectedItems.Count > 0
                ? _lstSpirits.SelectedItems[0].Tag as Spirit
                : null;
            if (objSpirit == null)
            {
                ShowMessage(_lstSpirits.Items.Count == 0 ? AlmanacStrings.Get("Message_NoSpirits") : string.Empty);
                return;
            }

            SpiritStats objStats;
            try
            {
                objStats = SpiritStats.Build(objSpirit, _objCharacter);
            }
            catch (Exception e)
            {
                ShowMessage(string.Format(CultureInfo.CurrentCulture, AlmanacStrings.Get("Message_Error"), e.Message));
                return;
            }
            if (objStats == null)
            {
                ShowMessage(AlmanacStrings.Get("Message_NoData"));
                return;
            }

            _blnLoading = true;
            try
            {
                _objSelectedSpirit = objSpirit;
                _objSelectedStats = objStats;

                CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
                string strTitle = objStats.TypeName + ", " + AlmanacStrings.Get("Column_Force") + " "
                                  + objSpirit.Force.ToString(objCulture);
                string strCritterName = objSpirit.CritterName;
                if (!string.IsNullOrEmpty(strCritterName))
                    strTitle = strCritterName + " (" + strTitle + ")";
                if (!string.IsNullOrEmpty(objStats.Source))
                    strTitle += "  [" + objStats.Source + "]";
                _lblTitle.Text = strTitle;

                _lstAttributes.BeginUpdate();
                _lstAttributes.Items.Clear();
                foreach (SpiritStatLine objLine in objStats.Attributes)
                    _lstAttributes.Items.Add(new ListViewItem(new[] { objLine.Label, objLine.Value }));
                _lstAttributes.EndUpdate();

                _lstPowers.BeginUpdate();
                _lstPowers.Items.Clear();
                foreach (string strPower in objStats.Powers)
                    _lstPowers.Items.Add(strPower);
                _lstPowers.EndUpdate();

                _lstSkills.BeginUpdate();
                _lstSkills.Items.Clear();
                foreach (SpiritSkillLine objSkill in objStats.Skills)
                    _lstSkills.Items.Add(new ListViewItem(new[] { objSkill.Name, objSkill.Attribute, objSkill.Pool }));
                _lstSkills.EndUpdate();

                List<string> lstChosen = _objData.GetOptionalPowers(objSpirit.InternalId);
                int intChosen = 0;
                _clbOptionalPowers.BeginUpdate();
                _clbOptionalPowers.Items.Clear();
                foreach (SpiritPowerEntry objPower in objStats.OptionalPowers)
                {
                    bool blnChosen = lstChosen.Contains(objPower.Key);
                    if (blnChosen)
                        ++intChosen;
                    _clbOptionalPowers.Items.Add(objPower, blnChosen);
                }
                _clbOptionalPowers.EndUpdate();
                _clbOptionalPowers.Enabled = objStats.OptionalPowers.Count > 0;
                UpdateOptionalPowersCaption(intChosen);
            }
            finally
            {
                _blnLoading = false;
            }
        }

        /// <summary>
        /// Clear the detail area and show a text instead, e.g. when the character has no spirits.
        /// </summary>
        private void ShowMessage(string strMessage)
        {
            _blnLoading = true;
            try
            {
                _objSelectedSpirit = null;
                _objSelectedStats = null;
                _lblTitle.Text = strMessage;
                _lstAttributes.Items.Clear();
                _lstPowers.Items.Clear();
                _lstSkills.Items.Clear();
                _clbOptionalPowers.Items.Clear();
                _clbOptionalPowers.Enabled = false;
                _gpbOptionalPowers.Text = AlmanacStrings.Get("Group_OptionalPowers");
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private void UpdateOptionalPowersCaption(int intChosen)
        {
            if (_objSelectedStats == null || _objSelectedStats.OptionalPowerLimit < 0)
            {
                _gpbOptionalPowers.Text = AlmanacStrings.Get("Group_OptionalPowers");
                return;
            }
            CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
            _gpbOptionalPowers.Text = string.Format(objCulture, AlmanacStrings.Get("Group_OptionalPowersCount"),
                intChosen, _objSelectedStats.OptionalPowerLimit);
        }

        private void OptionalPowers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_blnLoading || _objSelectedSpirit == null || _objSelectedStats == null)
                return;

            // Do not allow more optional powers than the spirit's Force grants. Removing a choice is always possible.
            int intLimit = _objSelectedStats.OptionalPowerLimit;
            if (intLimit >= 0 && e.NewValue == CheckState.Checked && e.CurrentValue != CheckState.Checked
                && _clbOptionalPowers.CheckedItems.Count >= intLimit)
            {
                e.NewValue = e.CurrentValue;
                return;
            }

            // The list itself only changes after this event, so work out the new state by hand.
            List<string> lstChosen = new List<string>();
            for (int i = 0; i < _clbOptionalPowers.Items.Count; ++i)
            {
                bool blnChecked = i == e.Index
                    ? e.NewValue == CheckState.Checked
                    : _clbOptionalPowers.GetItemChecked(i);
                if (blnChecked && _clbOptionalPowers.Items[i] is SpiritPowerEntry objPower)
                    lstChosen.Add(objPower.Key);
            }
            _objData.SetOptionalPowers(_objSelectedSpirit.InternalId, lstChosen);
            UpdateOptionalPowersCaption(lstChosen.Count);
            // The choice is stored in the character's save file, so the character now has unsaved changes.
            _frmCharacter.IsDirty = true;
        }
    }
}
