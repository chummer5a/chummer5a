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
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialConfirmValidity);
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialKarmaValue);
                lblNuyen.Parent.Controls.Remove(lblNuyen);
                lblNuyenTotal.Parent.Controls.Remove(lblNuyenTotal);
                lblStolenNuyenLabel.Parent.Controls.Remove(lblStolenNuyenLabel);
                lblStolenNuyen.Parent.Controls.Remove(lblStolenNuyen);
            }
            else
            {
                tabCharacterTabs.TabPages.Remove(tabKarma);
                tabCharacterTabs.TabPages.Remove(tabCalendar);
                tabCharacterTabs.TabPages.Remove(tabNotes);
                tabCharacterTabs.TabPages.Remove(tabImprovements);
                tabInfo.TabPages.Remove(tabConditionMonitor);
                mnuCreateFile.DropDownItems.Remove(mnuFileExport);
                mnuCreateSpecial.DropDownItems.Remove(mnuSpecialCloningMachine);
                tabCyberwareCM.Parent.Controls.Remove(tabCyberwareCM);
                panVehicleCM.Parent.Controls.Remove(panVehicleCM);
                lblPossessed.Parent.Controls.Remove(lblPossessed);
                cboAttributeCategory.Parent.Controls.Remove(cboAttributeCategory);
            }

            this.UpdateLightDarkMode();
            this.TranslateWinForm();

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
    }
}
