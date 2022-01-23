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
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class CharacterReadOnly : CharacterShared
    {
        private bool _blnIsReopenQueued;
        private readonly ListViewColumnSorter _lvwKarmaColumnSorter;
        private readonly ListViewColumnSorter _lvwNuyenColumnSorter;

        [Obsolete("This constructor is for use by form designers only.", true)]
        public CharacterReadOnly()
        {
            InitializeComponent();
        }

        public CharacterReadOnly(Character objCharacter) : base(objCharacter)
        {
            InitializeComponent();

            if (objCharacter.Created)
            {
                tabInfo.TabPages.Remove(tabBPSummary);
                mnuCreateFile.DropDownItems.Remove(mnuFileOpenWritableCreate);
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialConfirmValidity);
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialKarmaValue);
                lblNuyen.Parent.Controls.Remove(lblNuyen);
                lblNuyenTotal.Parent.Controls.Remove(lblNuyenTotal);
                lblStolenNuyenLabel.Parent.Controls.Remove(lblStolenNuyenLabel);
                lblStolenNuyen.Parent.Controls.Remove(lblStolenNuyen);

                tabBPSummary.Dispose();
                mnuFileOpenWritableCreate.Dispose();
                mnuSpecialConfirmValidity.Dispose();
                mnuSpecialKarmaValue.Dispose();
                lblNuyen.Dispose();
                lblNuyenTotal.Dispose();
                lblStolenNuyenLabel.Dispose();
                lblStolenNuyen.Dispose();

                _lvwKarmaColumnSorter = new ListViewColumnSorter
                {
                    SortColumn = 0,
                    Order = SortOrder.Descending
                };
                lstKarma.ListViewItemSorter = _lvwKarmaColumnSorter;
                _lvwNuyenColumnSorter = new ListViewColumnSorter
                {
                    SortColumn = 0,
                    Order = SortOrder.Descending
                };
                lstNuyen.ListViewItemSorter = _lvwNuyenColumnSorter;
            }
            else
            {
                tabCharacterTabs.TabPages.Remove(tabKarma);
                tabCharacterTabs.TabPages.Remove(tabCalendar);
                tabCharacterTabs.TabPages.Remove(tabNotes);
                tabCharacterTabs.TabPages.Remove(tabImprovements);
                tabInfo.TabPages.Remove(tabConditionMonitor);
                mnuCreateFile.DropDownItems.Remove(mnuFileOpenWritableCareer);
                mnuCreateFile.DropDownItems.Remove(mnuFileExport);
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialCloningMachine);
                tabCyberwareCM.Parent.Controls.Remove(tabCyberwareCM);
                panVehicleCM.Parent.Controls.Remove(panVehicleCM);
                lblPossessed.Parent.Controls.Remove(lblPossessed);
                cboAttributeCategory.Parent.Controls.Remove(cboAttributeCategory);

                tabKarma.Dispose();
                tabCalendar.Dispose();
                tabNotes.Dispose();
                tabImprovements.Dispose();
                tabConditionMonitor.Dispose();
                mnuFileOpenWritableCareer.Dispose();
                mnuFileExport.Dispose();
                mnuSpecialCloningMachine.Dispose();
                tabCyberwareCM.Dispose();
                panVehicleCM.Dispose();
                lblPossessed.Dispose();
                cboAttributeCategory.Dispose();
            }

            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            SetTooltips();

            // Update the text in the Menus so they can be merged with frmMain properly.
            foreach (ToolStripMenuItem tssItem in mnuCreateMenu.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }

            tabStreetGearTabs.MouseWheel += ShiftTabsOnMouseScroll;
            tabPeople.MouseWheel += ShiftTabsOnMouseScroll;
            tabInfo.MouseWheel += ShiftTabsOnMouseScroll;
            tabCharacterTabs.MouseWheel += ShiftTabsOnMouseScroll;

            Program.MainForm.OpenCharacterForms.Add(this);
        }

        #region Menu Events

        private void tsbPrint_Click(object sender, EventArgs e)
        {
            DoPrint();
        }

        private void mnuFilePrint_Click(object sender, EventArgs e)
        {
            DoPrint();
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuFileExport_Click(object sender, EventArgs e)
        {
            using (ExportCharacter frmExportCharacter = new ExportCharacter(CharacterObject))
                frmExportCharacter.ShowDialogSafe(this);
        }

        #endregion

        protected override bool IsReadOnly => true;

        private void ReopenCharacterAsWritable(object sender, EventArgs e)
        {
            _blnIsReopenQueued = true;
            FormClosed += ReopenCharacter;
            Close();
        }

        private void CharacterReadOnly_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (new CursorWait(this))
            {
                if (Program.MainForm.ActiveMdiChild == this)
                    ToolStripManager.RevertMerge("toolStrip");
                Program.MainForm.OpenCharacterForms.Remove(this);

                foreach (ContactControlReadOnly objContactControl in panContacts.Controls.OfType<ContactControlReadOnly>())
                {
                    objContactControl.MouseDown -= DragContactControl;
                }

                foreach (ContactControlReadOnly objContactControl in panEnemies.Controls.OfType<ContactControlReadOnly>())
                {
                    objContactControl.MouseDown -= DragContactControl;
                }

                // Trash the global variables and dispose of the Form.
                if (!_blnIsReopenQueued && Program.MainForm.OpenCharacters.All(x => x == CharacterObject || !x.LinkedCharacters.Contains(CharacterObject)))
                    Program.MainForm.OpenCharacters.Remove(CharacterObject);
            }
        }

        private void CharacterReadOnly_Activated(object sender, EventArgs e)
        {
            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(tsMain, "toolStrip");
        }

        private void ReopenCharacter(object sender, FormClosedEventArgs e)
        {
            Program.MainForm.OpenCharacter(CharacterObject);
            FormClosed -= ReopenCharacter;
        }

        /// <summary>
        /// Set the ToolTips from the Language file.
        /// </summary>
        private void SetTooltips()
        {
            // Armor Tab.
            chkArmorEquipped.SetToolTip(LanguageManager.GetString("Tip_ArmorEquipped"));
            // ToolTipFactory.SetToolTip(cmdArmorIncrease, LanguageManager.GetString("Tip_ArmorDegradationAPlus"));
            // ToolTipFactory.SetToolTip(cmdArmorDecrease, LanguageManager.GetString("Tip_ArmorDegradationAMinus"));
            // Weapon Tab.
            chkWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled"));
            // Gear Tab.
            chkGearActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            chkCyberwareActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            // Vehicles Tab.
            chkVehicleWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled"));
            chkVehicleActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            // Other Info Tab.
            lblCMPhysicalLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMPhysical"));
            lblCMStunLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMStun"));
            lblINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherInitiative"));
            lblMatrixINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherMatrixInitiative"));
            lblAstralINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherAstralInitiative"));
            lblArmorLabel.SetToolTip(LanguageManager.GetString("Tip_OtherArmor"));
            lblESS.SetToolTip(LanguageManager.GetString("Tip_OtherEssence"));
            lblRemainingNuyenLabel.SetToolTip(LanguageManager.GetString("Tip_OtherNuyen"));
            lblCareerKarmaLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCareerKarma"));
            lblMovementLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMovement"));
            lblSwimLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSwim"));
            lblFlyLabel.SetToolTip(LanguageManager.GetString("Tip_OtherFly"));
            lblComposureLabel.SetToolTip(LanguageManager.GetString("Tip_OtherComposure"));
            lblSurpriseLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSurprise"));
            lblJudgeIntentionsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherJudgeIntentions"));
            lblLiftCarryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarry"));
            lblMemoryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMemory"));
            // Condition Monitor Tab.
            lblCMPenaltyLabel.SetToolTip(LanguageManager.GetString("Tip_CMPenalty"));
            lblCMArmorLabel.SetToolTip(LanguageManager.GetString("Tip_OtherArmor"));
            lblCMDamageResistancePoolLabel.SetToolTip(LanguageManager.GetString("Tip_CMDamageResistance"));
            // Common Info Tab.
            lblStreetCred.SetToolTip(LanguageManager.GetString("Tip_StreetCred"));
            lblNotoriety.SetToolTip(LanguageManager.GetString("Tip_Notoriety"));
            if (CharacterObjectSettings.UseCalculatedPublicAwareness)
                lblPublicAware.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness"));
        }
    }
}
