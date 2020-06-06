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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Equipment;
using System.Collections.Specialized;
using System.Xml;

namespace Chummer.UI.Shared
{
    public partial class VehiclesTabUserControl : UserControl
    {
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnLoading = true;
        private Character _objCharacter;
        public event PropertyChangedEventHandler MakeDirty;
        public event PropertyChangedEventHandler MakeDirtyWithCharacterUpdate;
        public Action<Character, int> DiceRollerOpenedInt { get; set; }

        public VehiclesTabUserControl()
        {
            InitializeComponent();

            this.TranslateWinForm();
        }

        private void VehiclesTabUserControl_Load(object sender, EventArgs e)
        {
            if (_objCharacter != null)
                return;
            if (ParentForm != null)
                ParentForm.Cursor = Cursors.WaitCursor;
            RealLoad();
            if (ParentForm != null)
                ParentForm.Cursor = Cursors.Default;
        }

        public void RealLoad()
        {
            if (ParentForm is CharacterShared frmParent)
                _objCharacter = frmParent.CharacterObject;
            else
            {
                Utils.BreakIfDebug();
                _objCharacter = new Character();
            }

            chkVehicleWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled"));
            cmdVehicleGearReduceQty.SetToolTip(LanguageManager.GetString("Tip_DecreaseGearQty"));
            cmdVehicleMoveToInventory.SetToolTip(LanguageManager.GetString("Tip_TransferToInventory"));
            cmdRollVehicleWeapon.SetToolTip(LanguageManager.GetString("Tip_DiceRoller"));
            chkVehicleActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            
            _objCharacter.Vehicles.CollectionChanged += VehicleCollectionChanged;
            _objCharacter.VehicleLocations.CollectionChanged += VehicleLocationCollectionChanged;

            if (_objCharacter.Created)
            {
                nudVehicleGearQty.Visible = false;
                nudVehicleRating.Visible = false;
            }
            else
            {
                lblVehicleRating.Visible = false;
                cmdFireVehicleWeapon.Visible = false;
                cmdReloadVehicleWeapon.Visible = false;
                cmdVehicleMoveToInventory.Visible = false;
                cmdRollVehicleWeapon.Visible = false;
                cmdVehicleGearReduceQty.Visible = false;
                lblFiringModeLabel.Visible = false;
                cboVehicleWeaponFiringMode.Visible = false;
                lblVehicleWeaponAmmoRemainingLabel.Visible = false;
                lblVehicleWeaponAmmoRemaining.Visible = false;
                lblVehicleWeaponAmmoTypeLabel.Visible = false;
                cboVehicleWeaponAmmo.Visible = false;

                cmdDeleteVehicle.ContextMenuStrip = null;
                tabVehicleMatrixCM.Visible = false;
            }

            _blnLoading = false;
        }

        #region Common Control Events
        private bool AddVehicle(Location objLocation = null)
        {
            frmSelectVehicle frmPickVehicle = new frmSelectVehicle(_objCharacter);
            frmPickVehicle.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickVehicle.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");

            XmlNode objXmlVehicle = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + frmPickVehicle.SelectedVehicle + "\"]");
            Vehicle objVehicle = new Vehicle(_objCharacter);
            objVehicle.Create(objXmlVehicle);
            // Update the Used Vehicle information if applicable.
            if (frmPickVehicle.UsedVehicle)
            {
                objVehicle.Avail = frmPickVehicle.UsedAvail;
                objVehicle.Cost = frmPickVehicle.UsedCost.ToString(GlobalOptions.InvariantCultureInfo);
            }
            objVehicle.BlackMarketDiscount = frmPickVehicle.BlackMarketDiscount;

            decimal decCost = objVehicle.TotalCost;
            // Apply a markup if applicable.
            if (frmPickVehicle.Markup != 0)
            {
                decCost *= 1 + (frmPickVehicle.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objVehicle.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                decCost *= _objCharacter.Options.RestrictedCostMultiplier;
            if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickVehicle.FreeCost)
            {
                if (decCost > _objCharacter.Nuyen)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickVehicle.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicle") + LanguageManager.GetString("String_Space") + objVehicle.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicle, objVehicle.InternalId);
                objExpense.Undo = objUndo;
            }

            objVehicle.BlackMarketDiscount = frmPickVehicle.BlackMarketDiscount;

            objVehicle.Location = objLocation;

            _objCharacter.Vehicles.Add(objVehicle);

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);

            return frmPickVehicle.AddAgain;
        }

        private void cmdAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
				blnAddAgain = AddVehicle(treVehicles.SelectedNode?.Tag is Location objLocation ? objLocation : null);
			}
            while (blnAddAgain);
        }

        private void cmdDeleteVehicle_Click(object sender, EventArgs e)
        {
            if (!cmdDeleteVehicle.Enabled)
                return;
            // Delete the selected Vehicle.
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Delete the selected Vehicle.
            if (objSelectedNode == null)
            {
                return;
            }

            if (treVehicles.SelectedNode?.Tag is ICanRemove selectedObject)
            {
                selectedObject.Remove(_objCharacter, _objCharacter.Options.ConfirmDelete);
            }
            else if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["improvesensor"] != null || (objMod.WirelessOn && objMod.WirelessBonus?["improvesensor"] != null))
                {
                    objMod.Parent.ChangeVehicleSensor(treVehicles, false, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                }

                // If this is the Obsolete Mod, the user must select a percentage. This will create an Expense that costs X% of the Vehicle's base cost to remove the special Obsolete Mod.
                if (objMod.Name == "Obsolete" || (objMod.Name == "Obsolescent" && _objCharacter.Options.AllowObsolescentUpgrade))
                {
                    frmSelectNumber frmModPercent = new frmSelectNumber()
                    {
                        Minimum = 0,
                        Maximum = 1000000,
                        Description = LanguageManager.GetString("String_Retrofit")
                    };
                    frmModPercent.ShowDialog(this);

                    if (frmModPercent.DialogResult == DialogResult.Cancel)
                        return;

                    decimal decPercentage = frmModPercent.SelectedValue;
                    decimal decVehicleCost = objMod.Parent.OwnCost;

                    // Make sure the character has enough Nuyen for the expense.
                    decimal decCost = decVehicleCost * decPercentage / 100;

                    // Create a Vehicle Mod for the Retrofit.
                    VehicleMod objRetrofit = new VehicleMod(_objCharacter);

                    XmlDocument objVehiclesDoc = XmlManager.Load("vehicles.xml");
                    XmlNode objXmlNode = objVehiclesDoc.SelectSingleNode("/chummer/mods/mod[name = \"Retrofit\"]");
                    objRetrofit.Create(objXmlNode, 0, objMod.Parent);
                    objRetrofit.Cost = decCost.ToString(GlobalOptions.InvariantCultureInfo);
                    objRetrofit.IncludedInVehicle = true;
                    objMod.Parent.Mods.Add(objRetrofit);

                    // Create an Expense Log Entry for removing the Obsolete Mod.
                    ExpenseLogEntry objEntry = new ExpenseLogEntry(_objCharacter);
                    objEntry.Create(decCost * -1, LanguageManager.GetString("String_ExpenseVehicleRetrofit").Replace("{0}", objMod.Parent.CurrentDisplayName), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objEntry);

                    // Adjust the character's Nuyen total.
                    _objCharacter.Nuyen += decCost * -1;
                }

                objMod.DeleteVehicleMod();
                if (objMod.WeaponMountParent != null)
                    objMod.WeaponMountParent.Mods.Remove(objMod);
                else
                    objMod.Parent.Mods.Remove(objMod);
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                objAccessory.DeleteWeaponAccessory();
                objAccessory.Parent.WeaponAccessories.Remove(objAccessory);
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (objCyberware.Parent != null)
                    objCyberware.Parent.Children.Remove(objCyberware);
                else if (_objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == objCyberware.InternalId, out objMod) != null)
                {
                    objMod.Cyberware.Remove(objCyberware);
                }

                objCyberware.DeleteCyberware();
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.Parent is Gear objParent)
                    objParent.Children.Remove(objGear);
                else
                {
                    objGear = _objCharacter.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out objCyberware);
                    if (objCyberware != null)
                        objCyberware.Gear.Remove(objGear);
                    else if (objWeaponAccessory != null)
                        objWeaponAccessory.Gear.Remove(objGear);
                    else
                        objVehicle.Gear.Remove(objGear);
                }

                objGear.DeleteGear();
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void cmdAddVehicleLocation_Click(object sender, EventArgs e)
        {
            // Make sure a Vehicle is selected.
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectVehicleLocation"), LanguageManager.GetString("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation")
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;
            Location objLocation = new Location(_objCharacter, objVehicle.Locations, frmPickText.SelectedValue);
            objVehicle.Locations.Add(objLocation);

            MakeDirty?.Invoke(null, null);
        }

        private void cmdVehicleGearReduceQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear)) return;

            int intDecimalPlaces = 0;
            if (objGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = Math.Max(0, _objCharacter.Options.NuyenDecimals);
            }
            else if (objGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
            }

            frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
            {
                Minimum = 0,
                Maximum = objGear.Quantity,
                Description = LanguageManager.GetString("String_ReduceGear")
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            decimal decSelectedValue = frmPickNumber.SelectedValue;

            if (!_objCharacter.ConfirmDelete(LanguageManager.GetString("Message_ReduceQty").Replace("{0}", decSelectedValue.ToString(GlobalOptions.CultureInfo))))
                return;

            objGear.Quantity -= decSelectedValue;

            if (objGear.Quantity > 0)
            {
                objSelectedNode.Text = objGear.CurrentDisplayName;
            }
            else
            {
                _objCharacter.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);
                // Remove the Gear if its quantity has been reduced to 0.
                if (objGear.Parent is Gear objParent)
                    objParent.Children.Remove(objGear);
                else if (objWeaponAccessory != null)
                    objWeaponAccessory.Gear.Remove(objGear);
                else if (objCyberware != null)
                    objCyberware.Gear.Remove(objGear);
                else
                    objVehicle.Gear.Remove(objGear);
                objGear.DeleteGear();
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void cmdVehicleMoveToInventory_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Locate the selected Weapon.
            if (objSelectedNode?.Tag is Weapon objWeapon)
            {
                _objCharacter.Vehicles.FindVehicleWeapon(objWeapon.InternalId, out Vehicle objVehicle, out WeaponMount objMount, out VehicleMod objMod);
                // Move the Weapons from the Vehicle Mod (or Vehicle) to the character.
                Weapon objParent = objWeapon.Parent;
                if (objParent != null)
                    objParent.Children.Remove(objWeapon);
                else if (objMount != null)
                    objMount.Weapons.Remove(objWeapon);
                else if (objMod != null)
                    objMod.Weapons.Remove(objWeapon);
                else
                    objVehicle.Weapons.Remove(objWeapon);

                _objCharacter.Weapons.Add(objWeapon);

                objWeapon.ParentVehicle = null;
            }
            else if (objSelectedNode?.Tag is Gear objSelectedGear)
            {
                // Locate the selected Gear.
                _objCharacter.Vehicles.FindVehicleGear(objSelectedGear.InternalId, out Vehicle objVehicle,
                    out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);

                decimal decMinimumAmount = 1.0m;
                int intDecimalPlaces = 0;
                if (objSelectedGear.Name.StartsWith("Nuyen"))
                {
                    intDecimalPlaces = Math.Max(0, _objCharacter.Options.NuyenDecimals);
                    // Need a for loop instead of a power system to maintain exact precision
                    for (int i = 0; i < intDecimalPlaces; ++i)
                        decMinimumAmount /= 10.0m;
                }
                else if (objSelectedGear.Category == "Currency")
                {
                    intDecimalPlaces = 2;
                    decMinimumAmount = 0.01m;
                }

                decimal decMove;
                if (objSelectedGear.Quantity == decMinimumAmount)
                    decMove = decMinimumAmount;
                else
                {
                    frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
                    {
                        Minimum = decMinimumAmount,
                        Maximum = objSelectedGear.Quantity,
                        Description = LanguageManager.GetString("String_MoveGear")
                    };
                    frmPickNumber.ShowDialog(this);

                    if (frmPickNumber.DialogResult == DialogResult.Cancel)
                        return;

                    decMove = frmPickNumber.SelectedValue;
                }

                // See if the character already has a matching piece of Gear.
                Gear objFoundGear = _objCharacter.Gear.FirstOrDefault(x => objSelectedGear.IsIdenticalToOtherGear(x));

                if (objFoundGear == null)
                {
                    // Create a new piece of Gear.
                    Gear objGear = new Gear(_objCharacter);

                    objGear.Copy(objSelectedGear);

                    objGear.Quantity = decMove;

                    _objCharacter.Gear.Add(objGear);

                    objGear.AddGearImprovements();
                }
                else
                {
                    // Everything matches up, so just increase the quantity.
                    objFoundGear.Quantity += decMove;
                }

                // Update the selected item.
                objSelectedGear.Quantity -= decMove;
                if (objSelectedGear.Quantity <= 0)
                {
                    if (objSelectedGear.Parent is Gear objParent)
                        objParent.Children.Remove(objSelectedGear);
                    else if (objWeaponAccessory != null)
                        objWeaponAccessory.Gear.Remove(objSelectedGear);
                    else if (objCyberware != null)
                        objCyberware.Gear.Remove(objSelectedGear);
                    else
                        objVehicle?.Gear.Remove(objSelectedGear);

                    objSelectedGear.DeleteGear();
                }
                else
                    objSelectedNode.Text = objSelectedGear.CurrentDisplayName;
            }
            else
                return;

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void nudVehicleRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                objMod.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objMod.CurrentDisplayName;
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                {
                    if (Parent is frmCareer objParentAsCareer)
                    {
                        if (!objGear.RefreshSingleFocusRating(objParentAsCareer.FociTree, decimal.ToInt32(nudVehicleRating.Value)))
                        {
                            bool blnOldLoading = _blnLoading;
                            _blnLoading = true;
                            nudVehicleRating.Value = objGear.Rating;
                            _blnLoading = blnOldLoading;
                            return;
                        }
                    }
                    else if (Parent is frmCreate objParentAsCreate)
                    {
                        if (!objGear.RefreshSingleFocusRating(objParentAsCreate.FociTree, decimal.ToInt32(nudVehicleRating.Value)))
                        {
                            bool blnOldLoading = _blnLoading;
                            _blnLoading = true;
                            nudVehicleRating.Value = objGear.Rating;
                            _blnLoading = blnOldLoading;
                            return;
                        }
                    }
                }
                else
                    objGear.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objGear.CurrentDisplayName;
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                objCyberware.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objCyberware.CurrentDisplayName;
            }
            else
            {
                return;
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void nudVehicleGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                objGear.Quantity = nudVehicleGearQty.Value;
                treVehicles.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo);

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
        }

        private void cmdVehicleCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objModularCyberware))
                return;
            frmSelectItem frmPickMount = new frmSelectItem
            {
                GeneralItems = CharacterObject.ConstructModularCyberlimbList(objModularCyberware, out bool blnMountChangeAllowed),
                Description = LanguageManager.GetString("MessageTitle_SelectCyberware")
            };
            if (!blnMountChangeAllowed)
            {
                Program.MainForm.ShowMessageBox(
                    LanguageManager.GetString("Message_NoValidModularMount"),
                    LanguageManager.GetString("MessageTitle_NoValidModularMount"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            frmPickMount.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMount.DialogResult == DialogResult.Cancel)
            {
                return;
            }

            _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId,
                out VehicleMod objOldParentVehicleMod);
            Cyberware objOldParent = objModularCyberware.Parent;
            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);
            string strSelectedParentID = frmPickMount.SelectedItem;
            if (strSelectedParentID == "None")
            {
                if (objOldParent != null)
                    objOldParent.Children.Remove(objModularCyberware);
                else
                    objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                _objCharacter.Cyberware.Add(objModularCyberware);
            }
            else
            {
                Cyberware objNewParent = _objCharacter.Cyberware.DeepFindById(strSelectedParentID);
                if (objNewParent != null)
                {
                    if (objOldParent != null)
                        objOldParent.Children.Remove(objModularCyberware);
                    else
                        objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                    objNewParent.Children.Add(objModularCyberware);

                    objModularCyberware.ChangeModularEquip(true);
                }
                else
                {
                    VehicleMod objNewVehicleModParent = _objCharacter.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedParentID);
                    if (objNewVehicleModParent == null)
                        objNewParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedParentID, out objNewVehicleModParent);

                    if (objNewVehicleModParent != null || objNewParent != null)
                    {
                        if (objOldParent != null)
                            objOldParent.Children.Remove(objModularCyberware);
                        else
                            objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                        if (objNewParent != null)
                            objNewParent.Children.Add(objModularCyberware);
                        else
                            objNewVehicleModParent.Cyberware.Add(objModularCyberware);
                    }
                    else
                    {
                        if (objOldParent != null)
                            objOldParent.Children.Remove(objModularCyberware);
                        else
                            objOldParentVehicleMod.Cyberware.Remove(objModularCyberware);

                        _objCharacter.Cyberware.Add(objModularCyberware);
                    }
                }
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }
        #endregion

        #region TreeView Control Events
        private void treVehicles_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = ((TreeView)sender).GetNodeAt(pt);

            if (objNode == null)
                return;

            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
            if (_eDragButton == MouseButtons.Left)
            {
                if (objNode.Level <= _intDragLevel)
                    objNode.BackColor = SystemColors.ControlDark;
            }
            else
                objNode.BackColor = SystemColors.ControlDark;

            // Clear the background colour for all other Nodes.
            treVehicles.ClearNodeBackground(objNode);
        }

        private void treVehicles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteVehicle_Click(sender, e);
            }
        }

        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            // Generic event for all TreeViews to allow right-clicking to select a TreeNode so the proper ContextMenu is shown.
            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //{
            TreeView objTree = (TreeView)sender;
            objTree.SelectedNode = objTree.HitTest(e.X, e.Y).Node;
            //}
            if (ModifierKeys == Keys.Control)
            {
                if (!objTree.SelectedNode.IsExpanded)
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.ExpandAll();
                    }
                }
                else
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.Collapse();
                    }
                }
            }
        }

        private void treVehicles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level != 1)
                return;

            _intDragLevel = treVehicles.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treVehicles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedVehicle();
            // TODO: RefreshPasteStatus() conversion
        }
        #endregion

        #region Refresher Methods
        /// <summary>
        /// Refresh the currently-selected Vehicle.
        /// </summary>
        private void RefreshSelectedVehicle()
        {
            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            cmdDeleteVehicle.Enabled = !string.IsNullOrEmpty(strSelectedId) && strSelectedId != "Node_SelectedVehicles" && strSelectedId != "String_WeaponMounts";

            cmdVehicleCyberwareChangeMount.Visible = false;
            lblVehicleGearQty.Text = string.Empty;
            cmdVehicleGearReduceQty.Enabled = false;
            cmdVehicleMoveToInventory.Enabled = false;

            chkVehicleHomeNode.Visible = false;
            chkVehicleActiveCommlink.Visible = false;
            lblVehicleSlotsLabel.Visible = false;
            lblVehicleSlots.Visible = false;

            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level <= 0 || strSelectedId == "String_WeaponMounts")
            {
                tlpCommonInfos.Visible = false;
                tlpMatrixInfos.Visible = false;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;
                panVehicleCM.Visible = false;
                _blnLoading = blnOldLoading;
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space");

            if (treVehicles.SelectedNode?.Tag is IHasSource selected)
            {
                selected.SetSourceDetail(lblVehicleSource);
            }
            // Locate the selected Vehicle.
            if (treVehicles.SelectedNode?.Tag is Vehicle objVehicle)
            {
                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = true;
                tlpVehicleInfos.Visible = true;
                tlpVehicleWeapon.Visible = false;

                if (!string.IsNullOrEmpty(objVehicle.ParentID))
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleRatingLabel.Visible = false;
                lblVehicleRating.Visible = false;

                lblVehicleName.Text = objVehicle.DisplayNameShort(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleCategory.Text = objVehicle.DisplayCategory(GlobalOptions.Language);
                lblVehicleCategoryLabel.Visible = true;
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objVehicle.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text = objVehicle.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleHandling.Text = objVehicle.TotalHandling;
                lblVehicleAccel.Text = objVehicle.TotalAccel;
                lblVehicleSpeed.Text = objVehicle.TotalSpeed;
                lblVehicleDevice.Text = objVehicle.GetTotalMatrixAttribute("Device Rating").ToString();
                lblVehiclePilot.Text = objVehicle.Pilot.ToString();
                lblVehicleBody.Text = objVehicle.TotalBody.ToString();
                lblVehicleArmor.Text = objVehicle.TotalArmor.ToString();
                lblVehicleSeats.Text = objVehicle.TotalSeats.ToString();

                // Update the vehicle mod slots
                if (objVehicle.IsDrone && GlobalOptions.Dronemods)
                {
                    lblVehicleDroneModSlots.Text = objVehicle.DroneModSlotsUsed.ToString() + '/' + objVehicle.DroneModSlots;
                }
                else
                {
                    lblVehiclePowertrain.Text = objVehicle.PowertrainModSlotsUsed();
                    lblVehicleCosmetic.Text = objVehicle.CosmeticModSlotsUsed();
                    lblVehicleElectromagnetic.Text = objVehicle.ElectromagneticModSlotsUsed();
                    lblVehicleBodymod.Text = objVehicle.BodyModSlotsUsed();
                    lblVehicleWeaponsmod.Text = objVehicle.WeaponModSlotsUsed();
                    lblVehicleProtection.Text = objVehicle.ProtectionModSlotsUsed();

                    lblVehiclePowertrainLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                    lblVehicleCosmeticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                    lblVehicleElectromagneticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                    lblVehicleBodymodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                    lblVehicleWeaponsmodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                    lblVehicleProtectionLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
                }

                lblVehicleSensor.Text = objVehicle.CalculatedSensor.ToString();

                chkVehicleWeaponAccessoryInstalled.Enabled = false;
                chkVehicleIncludedInWeapon.Checked = false;

                objVehicle.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                chkVehicleActiveCommlink.Visible = objVehicle.IsCommlink;
                chkVehicleActiveCommlink.Checked = objVehicle.IsActiveCommlink(_objCharacter);
                if (_objCharacter.Metatype.Contains("A.I.") || _objCharacter.MetatypeCategory == "Protosapients")
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objVehicle.IsHomeNode(_objCharacter);
                    chkVehicleHomeNode.Enabled = objVehicle.GetTotalMatrixAttribute("Program Limit") >= (_objCharacter.DEP.TotalValue > objVehicle.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }
                
                if (_objCharacter.Options.BookEnabled("R5"))
                {
                    DisplayVehicleDroneMods(objVehicle.IsDrone && GlobalOptions.Dronemods);
                    DisplayVehicleMods(!(objVehicle.IsDrone && GlobalOptions.Dronemods));
                }
                else
                {
                    DisplayVehicleMods(false);
                    DisplayVehicleDroneMods(false);
                    lblVehicleSlotsLabel.Visible = true;
                    lblVehicleSlots.Visible = true;
                    lblVehicleSlots.Text = objVehicle.Slots + strSpace + '(' + (objVehicle.Slots - objVehicle.SlotsUsed).ToString(GlobalOptions.CultureInfo) + strSpace + LanguageManager.GetString("String_Remaining") + ')';
                }
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount)
            {
                cmdDeleteVehicle.Enabled = !objWeaponMount.IncludedInVehicle;

                lblVehicleRatingLabel.Visible = false;
                lblVehicleRating.Visible = false;

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = false;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;

                lblVehicleCategoryLabel.Visible = true;
                lblVehicleCategory.Text = objWeaponMount.DisplayCategory(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleName.Text = objWeaponMount.DisplayNameShort(GlobalOptions.Language);
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objWeaponMount.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text =
                    objWeaponMount.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo);

                chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Checked = false;

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString();

                string strPage = objWeaponMount.Page(GlobalOptions.Language);
                lblVehicleSource.Text =
                    CommonFunctions.LanguageBookShort(objWeaponMount.Source) +
                    strSpace + strPage;
                lblVehicleSource.SetToolTip(CommonFunctions.LanguageBookLong(objWeaponMount.Source) +
                    strSpace + LanguageManager.GetString("String_Page") +
                    strSpace + strPage);
            }
            else if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                if (objMod.IncludedInVehicle)
                    cmdDeleteVehicle.Enabled = false;
                if (objMod.MaxRating != "qty")
                {
                    if (objMod.MaxRating == "Seats")
                    {
                        objMod.MaxRating = objMod.Parent.Seats.ToString();
                    }

                    if (objMod.MaxRating == "body")
                    {
                        objMod.MaxRating = objMod.Parent.Body.ToString();
                    }

                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                    if (_objCharacter.Created)
                    {
                        lblVehicleRating.Text = Convert.ToInt32(objMod.MaxRating) > 0 ? objMod.Rating.ToString() : string.Empty;
                    }
                    else
                    {
                        lblVehicleRating.Text = string.Empty;
                        if (Convert.ToInt32(objMod.MaxRating) > 0)
                        {
                            lblVehicleRatingLabel.Visible = true;
                            // If the Mod is Armor, use the lower of the Mod's maximum Rating and MaxArmor value for the Vehicle instead.
                            nudVehicleRating.Maximum = objMod.Name.StartsWith("Armor,") ? Math.Min(Convert.ToInt32(objMod.MaxRating), objMod.Parent.MaxArmor) : Convert.ToInt32(objMod.MaxRating);
                            nudVehicleRating.Minimum = 1;
                            nudVehicleRating.Visible = true;
                            nudVehicleRating.Value = objMod.Rating;
                            nudVehicleRating.Increment = 1;
                            nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                        }
                        else
                        {
                            lblVehicleRatingLabel.Visible = false;
                            nudVehicleRating.Minimum = 0;
                            nudVehicleRating.Increment = 1;
                            nudVehicleRating.Maximum = 0;
                            nudVehicleRating.Enabled = false;
                            nudVehicleRating.Visible = false;
                        }
                    }
                }
                else
                {
                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Qty");
                    lblVehicleRatingLabel.Visible = true;
                    if (_objCharacter.Created)
                    {
                        nudVehicleRating.Visible = false;
                        lblVehicleRating.Text = objMod.Rating.ToString();
                    }
                    else
                    {
                        nudVehicleRating.Visible = true;
                        lblVehicleRating.Text = string.Empty;
                        nudVehicleRating.Minimum = 1;
                        nudVehicleRating.Maximum = 20;
                        nudVehicleRating.Value = objMod.Rating;
                        nudVehicleRating.Increment = 1;
                        nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                    }
                }

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = false;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;

                lblVehicleName.Text = objMod.DisplayNameShort(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleCategoryLabel.Visible = true;
                lblVehicleCategory.Text =
                    LanguageManager.GetString("String_VehicleModification");
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objMod.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text =
                    objMod.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                chkVehicleWeaponAccessoryInstalled.Checked = objMod.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Checked = false;

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objMod.CalculatedSlots.ToString();
            }
            else if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") ||
                    objWeapon.IncludedInWeapon || !string.IsNullOrEmpty(objWeapon.ParentID))
                    cmdDeleteVehicle.Enabled = false;

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = true;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = true;

                lblVehicleName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                lblVehicleWeaponDamage.Text =
                    objWeapon.CalculatedDamage(GlobalOptions.CultureInfo);
                lblVehicleWeaponAccuracy.Text =
                    objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo);
                lblVehicleWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblVehicleWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo);
                lblVehicleWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);

                lblVehicleWeaponRangeMain.Text = objWeapon.DisplayRange(GlobalOptions.Language);
                lblVehicleWeaponRangeAlternate.Text = objWeapon.DisplayAlternateRange(GlobalOptions.Language);
                IDictionary<string, string> dictionaryRanges = objWeapon.GetRangeStrings(GlobalOptions.CultureInfo);
                lblVehicleWeaponRangeShort.Text = dictionaryRanges["short"];
                lblVehicleWeaponRangeMedium.Text = dictionaryRanges["medium"];
                lblVehicleWeaponRangeLong.Text = dictionaryRanges["long"];
                lblVehicleWeaponRangeExtreme.Text = dictionaryRanges["extreme"];
                lblVehicleWeaponAlternateRangeShort.Text = dictionaryRanges["alternateshort"];
                lblVehicleWeaponAlternateRangeMedium.Text = dictionaryRanges["alternatemedium"];
                lblVehicleWeaponAlternateRangeLong.Text = dictionaryRanges["alternatelong"];
                lblVehicleWeaponAlternateRangeExtreme.Text = dictionaryRanges["alternateextreme"];

                lblVehicleName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeapon");
                lblVehicleAvail.Text = objWeapon.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCost.Text = objWeapon.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                cboVehicleWeaponFiringMode.SelectedValue = objWeapon.FireMode;
                lblVehicleWeaponDamage.Text =
                    objWeapon.CalculatedDamage(GlobalOptions.CultureInfo);
                lblVehicleWeaponAccuracy.Text =
                    objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo);
                lblVehicleWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblVehicleWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);
                if (objWeapon.WeaponType == "Ranged" || (objWeapon.WeaponType == "Melee" && objWeapon.Ammo != "0"))
                {
                    lblVehicleWeaponAmmo.Text =
                        objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo);
                    lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

                    cmsVehicleAmmoSingleShot.Enabled =
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeSingleShot",
                            GlobalOptions.Language)) ||
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic",
                            GlobalOptions.Language));
                    cmsVehicleAmmoShortBurst.Enabled =
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeBurstFire",
                            GlobalOptions.Language)) ||
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic",
                            GlobalOptions.Language));
                    cmsVehicleAmmoLongBurst.Enabled =
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic",
                            GlobalOptions.Language));
                    cmsVehicleAmmoFullBurst.Enabled =
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic",
                            GlobalOptions.Language));
                    cmsVehicleAmmoSuppressiveFire.Enabled =
                        objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic",
                            GlobalOptions.Language));

                    // Melee Weapons with Ammo are considered to be Single Shot.
                    if (objWeapon.WeaponType == "Melee" && objWeapon.Ammo != "0")
                        cmsVehicleAmmoSingleShot.Enabled = true;

                    if (cmsVehicleAmmoFullBurst.Enabled)
                        cmsVehicleAmmoFullBurst.Text = LanguageManager
                            .GetString("String_FullBurst")
                            .Replace("{0}", objWeapon.FullBurst.ToString());
                    if (cmsVehicleAmmoSuppressiveFire.Enabled)
                        cmsVehicleAmmoSuppressiveFire.Text = LanguageManager
                            .GetString("String_SuppressiveFire")
                            .Replace("{0}", objWeapon.Suppressive.ToString());

                    List<ListItem> lstAmmo = new List<ListItem>();
                    int intCurrentSlot = objWeapon.ActiveAmmoSlot;

                    for (int i = 1; i <= objWeapon.AmmoSlots; i++)
                    {
                        objWeapon.ActiveAmmoSlot = i;
                        Gear objVehicleGear = objWeapon.ParentVehicle.Gear.DeepFindById(objWeapon.AmmoLoaded);

                        string strPlugins = string.Empty;
                        foreach (Gear objCurrentAmmo in objWeapon.ParentVehicle.Gear)
                        {
                            if (objCurrentAmmo.InternalId == objWeapon.AmmoLoaded)
                            {
                                foreach (Gear objChild in objCurrentAmmo.Children)
                                {
                                    strPlugins += objChild.DisplayNameShort(GlobalOptions.Language) + ", ";
                                }
                            }
                        }

                        // Remove the trailing comma.
                        if (!string.IsNullOrEmpty(strPlugins))
                            strPlugins = strPlugins.Substring(0, strPlugins.Length - 2);

                        string strAmmoName;
                        if (objVehicleGear == null)
                        {
                            if (objWeapon.AmmoRemaining == 0)
                                strAmmoName =
                                    LanguageManager.GetString("String_SlotNumber")
                                        .Replace("{0}", i.ToString()) +
                                    strSpace +
                                    LanguageManager.GetString("String_Empty");
                            else
                                strAmmoName =
                                    LanguageManager.GetString("String_SlotNumber")
                                        .Replace("{0}", i.ToString()) +
                                    strSpace +
                                    LanguageManager.GetString("String_ExternalSource");
                        }
                        else
                            strAmmoName =
                                LanguageManager.GetString("String_SlotNumber")
                                    .Replace("{0}", i.ToString()) +
                                strSpace +
                                objVehicleGear.DisplayNameShort(GlobalOptions.Language);

                        if (!string.IsNullOrEmpty(strPlugins))
                            strAmmoName += '[' + strPlugins + ']';
                        lstAmmo.Add(new ListItem(i.ToString(), strAmmoName));
                    }

                    cmdVehicleMoveToInventory.Enabled = true;
                    objWeapon.ActiveAmmoSlot = intCurrentSlot;
                    cboVehicleWeaponAmmo.BeginUpdate();
                    cboVehicleWeaponAmmo.ValueMember = "Value";
                    cboVehicleWeaponAmmo.DisplayMember = "Name";
                    cboVehicleWeaponAmmo.DataSource = lstAmmo;
                    cboVehicleWeaponAmmo.SelectedValue = objWeapon.ActiveAmmoSlot.ToString();
                    if (cboVehicleWeaponAmmo.SelectedIndex == -1)
                        cboVehicleWeaponAmmo.SelectedIndex = 0;
                    cboVehicleWeaponAmmo.EndUpdate();
                }
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                if (objAccessory.IncludedInWeapon)
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleName.Text = objAccessory.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeaponAccessory");
                lblVehicleAvail.Text = objAccessory.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCost.Text = objAccessory.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = false;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;

                string[] strMounts = objAccessory.Mount.Split('/');
                StringBuilder strMount = new StringBuilder();
                foreach (string strCurrentMount in strMounts)
                {
                    if (!string.IsNullOrEmpty(strCurrentMount))
                        strMount.Append(LanguageManager.GetString("String_Mount" + strCurrentMount) + '/');
                }
                // Remove the trailing /
                if (strMount.Length > 0)
                    strMount.Length -= 1;
                if (!string.IsNullOrEmpty(objAccessory.ExtraMount) && (objAccessory.ExtraMount != "None"))
                {
                    bool boolHaveAddedItem = false;
                    string[] strExtraMounts = objAccessory.ExtraMount.Split('/');
                    foreach (string strCurrentExtraMount in strExtraMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentExtraMount))
                        {
                            if (!boolHaveAddedItem)
                            {
                                strMount.Append(" + ");
                                boolHaveAddedItem = true;
                            }
                            strMount.Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount) + '/');
                        }
                    }
                    // Remove the trailing /
                    if (boolHaveAddedItem)
                        strMount.Length -= 1;
                }

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = strMount.ToString();
                chkVehicleWeaponAccessoryInstalled.Enabled = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Equipped;
                chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (!string.IsNullOrEmpty(objCyberware.ParentID))
                    cmdDeleteVehicle.Enabled = false;

                lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                if (_objCharacter.Created)
                {
                    nudVehicleRating.Visible = false;
                    lblVehicleRating.Visible = true;
                    lblVehicleRating.Text = objCyberware.Rating.ToString(GlobalOptions.CultureInfo);
                }
                else
                {
                    lblVehicleRating.Visible = false;
                    nudVehicleRating.Visible = true;
                    nudVehicleRating.Minimum = objCyberware.MinRating;
                    nudVehicleRating.Maximum = objCyberware.MaxRating;
                    nudVehicleRating.Value = objCyberware.Rating;
                }

                cmdVehicleCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);

                lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleModification");
                lblVehicleAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCost.Text = objCyberware.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                lblVehicleDevice.Text = objCyberware.GetTotalMatrixAttribute("Device Rating").ToString();
                objCyberware.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = true;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;

                chkVehicleActiveCommlink.Visible = objCyberware.IsCommlink;
                chkVehicleActiveCommlink.Checked = objCyberware.IsActiveCommlink(_objCharacter);
                if (_objCharacter.IsAI)
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objCyberware.IsHomeNode(_objCharacter);
                    chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objCyberware.GetTotalMatrixAttribute("Program Limit") >= (_objCharacter.DEP.TotalValue > objCyberware.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.IncludedInParent)
                    cmdDeleteVehicle.Enabled = false;

                tlpCommonInfos.Visible = true;
                tlpMatrixInfos.Visible = true;
                tlpVehicleInfos.Visible = false;
                tlpVehicleWeapon.Visible = false;

                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                lblVehicleGearQtyLabel.Visible = true;
                if (_objCharacter.Created)
                {
                    nudVehicleGearQty.Visible = false;
                    nudVehicleRating.Visible = false;

                    lblVehicleRating.Visible = true;
                    lblVehicleRating.Text = objGear.Rating.ToString(GlobalOptions.CultureInfo);
                    
                    lblVehicleGearQty.Visible = true;
                    lblVehicleGearQty.Text = objGear.Quantity.ToString(objGear.Name.StartsWith("Nuyen") ? _objCharacter.Options.NuyenFormat : (objGear.Category == "Currency" ? "#,0.00" : "#,0"), GlobalOptions.CultureInfo);
                }
                else
                {
                    nudVehicleGearQty.Visible = true;

                    lblVehicleRating.Visible = false;
                    lblVehicleGearQty.Visible = false;

                    nudVehicleGearQty.Enabled = !objGear.IncludedInParent;
                    if (objGear.Name.StartsWith("Nuyen"))
                    {
                        int intDecimalPlaces = _objCharacter.Options.NuyenDecimals;
                        if (intDecimalPlaces <= 0)
                        {
                            nudVehicleGearQty.DecimalPlaces = 0;
                            nudVehicleGearQty.Minimum = 1.0m;
                        }
                        else
                        {
                            nudVehicleGearQty.DecimalPlaces = intDecimalPlaces;
                            decimal decMinimum = 1.0m;
                            // Need a for loop instead of a power system to maintain exact precision
                            for (int i = 0; i < intDecimalPlaces; ++i)
                                decMinimum /= 10.0m;
                            nudVehicleGearQty.Minimum = decMinimum;
                        }
                    }
                    else if (objGear.Category == "Currency")
                    {
                        nudVehicleGearQty.DecimalPlaces = 2;
                        nudVehicleGearQty.Minimum = 0.01m;
                    }
                    else
                    {
                        nudVehicleGearQty.DecimalPlaces = 0;
                        nudVehicleGearQty.Minimum = 1.0m;
                    }
                    nudVehicleGearQty.Value = objGear.Quantity;
                    nudVehicleGearQty.Increment = objGear.CostFor;

                    int intGearMaxRatingValue = objGear.MaxRatingValue;
                    if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                    {
                        lblVehicleRatingLabel.Visible = true;
                        nudVehicleRating.Visible = true;
                        nudVehicleRating.Maximum = intGearMaxRatingValue;
                        nudVehicleRating.Value = objGear.Rating;
                    }
                    else
                    {
                        lblVehicleRatingLabel.Visible = false;
                        nudVehicleRating.Visible = false;
                        nudVehicleRating.Minimum = 0;
                        nudVehicleRating.Maximum = 0;
                    }
                }
                
                cmdVehicleGearReduceQty.Enabled = !objGear.IncludedInParent;
                cmdVehicleMoveToInventory.Enabled = !objGear.IncludedInParent;
                
                lblVehicleName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblVehicleAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo);
                lblVehicleCost.Text = objGear.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objGear.CalculatedCapacity
                    + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo)
                    + strSpace + LanguageManager.GetString("String_Remaining") + ')';

                objGear.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                chkVehicleActiveCommlink.Visible = objGear.IsCommlink;
                chkVehicleActiveCommlink.Checked = objGear.IsActiveCommlink(_objCharacter);
                if (_objCharacter.IsAI)
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objGear.IsHomeNode(_objCharacter);
                    chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (_objCharacter.DEP.TotalValue > objGear.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }

                chkVehicleWeaponAccessoryInstalled.Checked = true;
                chkVehicleWeaponAccessoryInstalled.Enabled = false;
                chkVehicleIncludedInWeapon.Checked = false;
            }

            if (_objCharacter.Created)
            {
                panVehicleCM.Visible = (treVehicles.SelectedNode?.Tag is IHasPhysicalConditionMonitor ||
                                        treVehicles.SelectedNode?.Tag is IHasMatrixConditionMonitor);

                if (treVehicles.SelectedNode?.Tag is IHasPhysicalConditionMonitor objCM)
                {
                    ProcessEquipmentConditionMonitorBoxDisplays(tabVehiclePhysicalCM, objCM.PhysicalCM, objCM.PhysicalCMFilled);
                }

                if (treVehicles.SelectedNode?.Tag is IHasMatrixConditionMonitor objMatrixCM)
                {
                    ProcessEquipmentConditionMonitorBoxDisplays(tabVehicleMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
                }
            }

            _blnLoading = blnOldLoading;
        }

        /// <summary>
        /// Manages the rendering of condition monitor checkboxes for characters that can have modifiers like overflow and threshold offsets.
        /// </summary>
        /// <param name="pnlConditionMonitorPanel">Container panel for the condition monitor checkboxes.</param>
        /// <param name="intConditionMax">Highest value of the condition monitor type.</param>
        /// <param name="intCurrentConditionFilled">Current amount of boxes that should be filled in the condition monitor.</param>
        private void ProcessEquipmentConditionMonitorBoxDisplays(Control pnlConditionMonitorPanel, int intConditionMax, int intCurrentConditionFilled)
        {
            bool blnOldLoading = _blnLoading;
            _blnLoading = true;

            pnlConditionMonitorPanel.SuspendLayout();
            if (intConditionMax > 0)
            {
                pnlConditionMonitorPanel.Visible = true;
                foreach (CheckBox chkCmBox in pnlConditionMonitorPanel.Controls.OfType<CheckBox>())
                {
                    int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag);

                    chkCmBox.Text = string.Empty;
                    if (intCurrentBoxTag <= intConditionMax)
                    {
                        chkCmBox.Visible = true;
                        chkCmBox.Checked = intCurrentBoxTag <= intCurrentConditionFilled;
                    }
                    else
                    {
                        chkCmBox.Visible = false;
                        chkCmBox.Checked = false;
                    }
                }
            }
            else
            {
                pnlConditionMonitorPanel.Visible = false;
            }
            pnlConditionMonitorPanel.ResumeLayout();

            _blnLoading = blnOldLoading;
        }

        /// <summary>
        /// Switches the visibility of Vehicle (non-drone) Mods on the Vehicles and Drones form.
        /// </summary>
        /// <param name="blnDisplay">Whether to hide or show the objects.</param>
        private void DisplayVehicleMods(bool blnDisplay)
        {
            lblVehiclePowertrainLabel.Visible = blnDisplay;
            lblVehiclePowertrain.Visible = blnDisplay;
            lblVehicleCosmeticLabel.Visible = blnDisplay;
            lblVehicleCosmetic.Visible = blnDisplay;
            lblVehicleElectromagneticLabel.Visible = blnDisplay;
            lblVehicleElectromagnetic.Visible = blnDisplay;
            lblVehicleBodymodLabel.Visible = blnDisplay;
            lblVehicleBodymod.Visible = blnDisplay;
            lblVehicleWeaponsmodLabel.Visible = blnDisplay;
            lblVehicleWeaponsmod.Visible = blnDisplay;
            lblVehicleProtectionLabel.Visible = blnDisplay;
            lblVehicleProtection.Visible = blnDisplay;
        }

        /// <summary>
        /// Switches the visibility of Drone Mods on the Vehicles and Drones form.
        /// </summary>
        /// <param name="blnDisplay">Whether to hide or show the objects.</param>
        private void DisplayVehicleDroneMods(bool blnDisplay)
        {
            lblVehicleDroneModSlotsLabel.Visible = blnDisplay;
            lblVehicleDroneModSlots.Visible = blnDisplay;
        }
        #endregion

        #region Weapon Control Events
        private void cmdRollVehicleWeapon_Click(object sender, EventArgs e)
        {
            int.TryParse(lblVehicleWeaponDicePool.Text, out int intDice);
            DiceRollerOpenedInt(_objCharacter, intDice);
        }

        private void cmdFireVehicleWeapon_Click(object sender, EventArgs e)
        {
            // "Click" the first menu item available.
            if (cmsVehicleAmmoSingleShot.Enabled)
                cmsVehicleAmmoSingleShot_Click(sender, e);
            else
            {
                if (cmsVehicleAmmoShortBurst.Enabled)
                    cmsVehicleAmmoShortBurst_Click(sender, e);
                else
                    cmsVehicleAmmoLongBurst_Click(sender, e);
            }
        }

        private void cmdReloadVehicleWeapon_Click(object sender, EventArgs e)
        {
            if (_blnLoading || !(treVehicles?.SelectedNode?.Tag is Weapon objWeapon))
                return;
            objWeapon.Reload(objWeapon.ParentVehicle.Gear, treVehicles);
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !(treVehicles.SelectedNode?.Tag is ICanEquip objEquippable))
                return;

            objEquippable.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;

            MakeDirty?.Invoke(null, null);
        }

        private void cboVehicleWeaponAmmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;

            objWeapon.ActiveAmmoSlot = Convert.ToInt32(cboVehicleWeaponAmmo.SelectedValue.ToString());
            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }
        private void cboVehicleWeaponFiringMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                objWeapon.FireMode = Weapon.ConvertToFiringMode(cboVehicleWeaponFiringMode.SelectedValue.ToString());

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
        }
        private void cmsVehicleAmmoSingleShot_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            objWeapon.AmmoRemaining -= 1;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirty?.Invoke(null, null);
        }

        private void cmsVehicleAmmoShortBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= 3)
            {
                objWeapon.AmmoRemaining -= 3;
            }
            else
            {
                if (objWeapon.AmmoRemaining == 1)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirty?.Invoke(null, null);
        }

        private void cmsVehicleAmmoLongBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= 6)
            {
                objWeapon.AmmoRemaining -= 6;
            }
            else
            {
                if (objWeapon.AmmoRemaining == 1)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining > 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoLongBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining == 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirty?.Invoke(null, null);
        }

        private void cmsVehicleAmmoFullBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.FullBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.FullBurst;
            }
            else
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughAmmoFullBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirty?.Invoke(null, null);
        }

        private void cmsVehicleAmmoSuppressiveFire_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.Suppressive)
            {
                objWeapon.AmmoRemaining -= objWeapon.Suppressive;
            }
            else
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughAmmoSuppressiveFire"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            MakeDirty?.Invoke(null, null);
        }
        #endregion

        #region Matrix Control Events
        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                objTarget.SetHomeNode(_objCharacter, chkVehicleHomeNode.Checked);

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
        }
        private void chkVehicleActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink)
            {
                objSelectedCommlink.SetActiveCommlink(_objCharacter, chkVehicleActiveCommlink.Checked);

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
        }
        private void cboVehicleGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !cboVehicleGearAttack.Enabled)
                return;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(_objCharacter, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                }
            }

            _blnLoading = blnOldLoading;
        }
        private void cboVehicleGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !cboVehicleGearSleaze.Enabled)
                return;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(_objCharacter, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                }
            }

            _blnLoading = blnOldLoading;
        }
        private void cboVehicleGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !cboVehicleGearDataProcessing.Enabled)
                return;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(_objCharacter, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                }
            }

            _blnLoading = blnOldLoading;
        }
        private void cboVehicleGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !cboVehicleGearFirewall.Enabled)
                return;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(_objCharacter, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                }
            }

            _blnLoading = blnOldLoading;
        }
        #endregion

        #region Misc. Control Events
        private void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasNotes selectedObject))
                return;

            if (selectedObject.WriteNotes(treVehicles.SelectedNode))
                MakeDirty?.Invoke(null, null);
        }
        private void tsVehicleCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectCyberware"), LanguageManager.GetString("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (ParentForm is frmCareer objParentAsCareer)
            {
                bool blnAddAgain;
                do
                {
                    blnAddAgain = objParentAsCareer.PickCyberware(objCyberware, objCyberware.SourceType);
                } while (blnAddAgain);
            }
        }
        private void tsVehicleCyberwareAddGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectCyberware"), LanguageManager.GetString("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CyberwareGear"), LanguageManager.GetString("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objCyberware, strCategories);
                if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objCyberware.Capacity) && (!objCyberware.Capacity.Contains('[') || objCyberware.Capacity.Contains("/[")))
                    frmPickGear.ShowNegativeCapacityOnly = true;
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                // Open the Gear XML file and locate the selected piece.
                XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        objGear.DeleteGear();
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwareGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    _objCharacter.Weapons.Add(objWeapon);
                }

                objCyberware.Gear.Add(objGear);

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }
        private void tsVehicleName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level == 0)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectVehicleName"), LanguageManager.GetString("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            while (treVehicles.SelectedNode.Level > 1)
            {
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;
            }

            if (!(treVehicles.SelectedNode?.Tag is IHasCustomName objRename)) return;

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_VehicleName"),
                DefaultString = objRename.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objRename.CustomName = frmPickText.SelectedValue;
            treVehicles.SelectedNode.Text = objRename.CurrentDisplayName;

            MakeDirty?.Invoke(null, null);
        }

        private void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasInternalId strSelectedId))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_VehicleCyberwarePlugin"), LanguageManager.GetString("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            VehicleMod objMod = _objCharacter.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId.InternalId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId.InternalId, out objMod);

            if (objCyberwareParent == null && (objMod == null || !objMod.AllowCyberware))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_VehicleCyberwarePlugin"), LanguageManager.GetString("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("cyberware.xml");
            bool blnAddAgain;

            do
            {
                frmSelectCyberware frmPickCyberware = new frmSelectCyberware(_objCharacter, Improvement.ImprovementSource.Cyberware, objCyberwareParent ?? (object)objMod);
                if (objCyberwareParent == null)
                {
                    //frmPickCyberware.SetGrade = "Standard";
                    frmPickCyberware.MaximumCapacity = objMod.CapacityRemaining;
                    frmPickCyberware.Subsystems = objMod.Subsystems;
                    HashSet<string> setDisallowedMounts = new HashSet<string>();
                    HashSet<string> setHasMounts = new HashSet<string>();
                    foreach (Cyberware objLoopCyberware in objMod.Cyberware.DeepWhere(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                    {
                        string[] strLoopDisallowedMounts = objLoopCyberware.BlocksMounts.Split(',');
                        foreach (string strLoop in strLoopDisallowedMounts)
                            if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                        string strLoopHasModularMount = objLoopCyberware.HasModularMount;
                        if (!string.IsNullOrEmpty(strLoopHasModularMount))
                            if (!setHasMounts.Contains(strLoopHasModularMount))
                                setHasMounts.Add(strLoopHasModularMount);
                    }
                    string strDisallowedMounts = string.Empty;
                    foreach (string strLoop in setDisallowedMounts)
                        if (!strLoop.EndsWith("Right") && (!strLoop.EndsWith("Left") || setDisallowedMounts.Contains(strLoop.Substring(0, strLoop.Length - 4) + "Right")))
                            strDisallowedMounts += strLoop + ",";
                    // Remove trailing ","
                    if (!string.IsNullOrEmpty(strDisallowedMounts))
                        strDisallowedMounts = strDisallowedMounts.Substring(0, strDisallowedMounts.Length - 1);
                    frmPickCyberware.DisallowedMounts = strDisallowedMounts;
                    string strHasMounts = string.Empty;
                    foreach (string strLoop in setHasMounts)
                        strHasMounts += strLoop + ",";
                    // Remove trailing ","
                    if (!string.IsNullOrEmpty(strHasMounts))
                        strHasMounts = strHasMounts.Substring(0, strHasMounts.Length - 1);
                    frmPickCyberware.HasModularMounts = strHasMounts;
                }
                else
                {
                    frmPickCyberware.SetGrade = objCyberwareParent.Grade;
                    // If the Cyberware has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                    if (!objCyberwareParent.Capacity.Contains('['))
                    {
                        frmPickCyberware.MaximumCapacity = objCyberwareParent.CapacityRemaining;

                        // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                        if (_objCharacter.Options.EnforceCapacity && objCyberwareParent.CapacityRemaining < 0)
                        {
                            Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CapacityReached"), LanguageManager.GetString("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            frmPickCyberware.Dispose();
                            break;
                        }
                    }

                    frmPickCyberware.CyberwareParent = objCyberwareParent;
                    frmPickCyberware.Subsystems = objCyberwareParent.AllowedSubsystems;

                    HashSet<string> setDisallowedMounts = new HashSet<string>();
                    HashSet<string> setHasMounts = new HashSet<string>();
                    string[] strLoopDisallowedMounts = objCyberwareParent.BlocksMounts.Split(',');
                    foreach (string strLoop in strLoopDisallowedMounts)
                        setDisallowedMounts.Add(strLoop + objCyberwareParent.Location);
                    string strLoopHasModularMount = objCyberwareParent.HasModularMount;
                    if (!string.IsNullOrEmpty(strLoopHasModularMount))
                        setHasMounts.Add(strLoopHasModularMount);
                    foreach (Cyberware objLoopCyberware in objCyberwareParent.Children.DeepWhere(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                    {
                        strLoopDisallowedMounts = objLoopCyberware.BlocksMounts.Split(',');
                        foreach (string strLoop in strLoopDisallowedMounts)
                            if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                        strLoopHasModularMount = objLoopCyberware.HasModularMount;
                        if (!string.IsNullOrEmpty(strLoopHasModularMount))
                            if (!setHasMounts.Contains(strLoopHasModularMount))
                                setHasMounts.Add(strLoopHasModularMount);
                    }
                    string strDisallowedMounts = string.Empty;
                    foreach (string strLoop in setDisallowedMounts)
                        if (!strLoop.EndsWith("Right") && (!strLoop.EndsWith("Left") || setDisallowedMounts.Contains(strLoop.Substring(0, strLoop.Length - 4) + "Right")))
                            strDisallowedMounts += strLoop + ",";
                    // Remove trailing ","
                    if (!string.IsNullOrEmpty(strDisallowedMounts))
                        strDisallowedMounts = strDisallowedMounts.Substring(0, strDisallowedMounts.Length - 1);
                    frmPickCyberware.DisallowedMounts = strDisallowedMounts;
                    string strHasMounts = string.Empty;
                    foreach (string strLoop in setHasMounts)
                        strHasMounts += strLoop + ",";
                    // Remove trailing ","
                    if (!string.IsNullOrEmpty(strHasMounts))
                        strHasMounts = strHasMounts.Substring(0, strHasMounts.Length - 1);
                    frmPickCyberware.HasModularMounts = strHasMounts;
                }
                frmPickCyberware.LockGrade();
                frmPickCyberware.ParentVehicle = objVehicle ?? objMod.Parent;
                frmPickCyberware.ShowDialog(this);

                if (frmPickCyberware.DialogResult == DialogResult.Cancel)
                {
                    frmPickCyberware.Dispose();
                    break;
                }
                blnAddAgain = frmPickCyberware.AddAgain;

                XmlNode objXmlCyberware = objXmlDocument.SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + frmPickCyberware.SelectedCyberware + "\"]");
                Cyberware objCyberware = new Cyberware(_objCharacter);
                if (objCyberware.Purchase(objXmlCyberware, Improvement.ImprovementSource.Cyberware, frmPickCyberware.SelectedGrade, frmPickCyberware.SelectedRating, _objCharacter, objVehicle, objMod.Cyberware, _objCharacter.Vehicles, objMod.Weapons, frmPickCyberware.Markup, frmPickCyberware.FreeCost, "String_ExpensePurchaseVehicleCyberware"))
                {
                    MakeDirtyWithCharacterUpdate?.Invoke(null, null);
                }

                frmPickCyberware.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsVehicleCyberwareGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_ModifyVehicleGear"), LanguageManager.GetString("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlNode objXmlSensorGear = objSensor.GetNode();
            string strCategories = string.Empty;
            XmlNodeList xmlAddonCategoryList = objXmlSensorGear?.SelectNodes("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                    strCategories += objXmlCategory.InnerText + ",";
                // Remove the trailing comma.
                strCategories = strCategories.Substring(0, strCategories.Length - 1);
            }
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objSensor, strCategories);
                if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                    frmPickGear.ShowNegativeCapacityOnly = true;
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                // Open the Gear XML file and locate the selected piece.
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        objGear.DeleteGear();
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwareGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    _objCharacter.Weapons.Add(objWeapon);
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Location objLocation)) return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation"),
                DefaultString = objLocation.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;
            objLocation.Name = frmPickText.SelectedValue;

            MakeDirty?.Invoke(null, null);
        }

        private void tsVehicleLocationAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddVehicle(treVehicles.SelectedNode?.Tag is Location objLocation ? objLocation : null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleLocationAddWeapon_Click(object sender, EventArgs e)
        {
            //TODO: Where should weapons attached to locations of vehicles go?
            //PickWeapon(treVehicles.SelectedNode);
        }

        private void tsVehicleWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Locate the Vehicle Sensor Gear.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_ModifyVehicleGear"), LanguageManager.GetString("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            XmlNode objXmlSensorGear = objSensor.GetNode();
            string strCategories = string.Empty;
            XmlNodeList xmlAddonCategoryList = objXmlSensorGear?.SelectNodes("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                    strCategories += objXmlCategory.InnerText + ",";
                // Remove the trailing comma.
                strCategories = strCategories.Substring(0, strCategories.Length - 1);
            }
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objSensor, strCategories);
                if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                    frmPickGear.ShowNegativeCapacityOnly = true;
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        objGear.DeleteGear();
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);
                _objCharacter.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out _, out _);
                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = objVehicle;
                    objVehicle.Weapons.Add(objWeapon);
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)) return;
            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_WeaponGear"), LanguageManager.GetString("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objAccessory, strCategories);
                if (!string.IsNullOrEmpty(strCategories))
                    frmPickGear.ShowNegativeCapacityOnly = true;
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        objGear.DeleteGear();
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objAccessory.Gear.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.Parent = objAccessory.Parent;
                    objAccessory.Parent.Children.Add(objWeapon);
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddGear_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle.
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objSelectedVehicle))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectGearVehicle"), LanguageManager.GetString("MessageTitle_SelectGearVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objSelectedVehicle);
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                // Open the Gear XML file and locate the selected piece.
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Change the cost of the Sensor itself to 0.
                //if (frmPickGear.SelectedCategory == "Sensors")
                //{
                //    objGear.Cost = "0";
                //    objGear.DictionaryCostN = new Tuple<int, Dictionary<int, string>>(-1, new Dictionary<int, string>());
                //}

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                bool blnMatchFound = false;
                // If this is Ammunition, see if the character already has it on them.
                if (objGear.Category == "Ammunition")
                {
                    foreach (Gear objVehicleGear in objSelectedVehicle.Gear)
                    {
                        if (objVehicleGear.Name == objGear.Name && objVehicleGear.Category == objGear.Category && objVehicleGear.Rating == objGear.Rating && objVehicleGear.Extra == objGear.Extra)
                        {
                            // A match was found, so increase the quantity instead.
                            objVehicleGear.Quantity += objGear.Quantity;
                            blnMatchFound = true;
                            break;
                        }
                    }
                }

                if (!blnMatchFound)
                {
                    // Add the Gear to the Vehicle.
                    objSelectedVehicle.Gear.Add(objGear);
                    objGear.Parent = objSelectedVehicle;

                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        objWeapon.ParentVehicle = objSelectedVehicle;
                        objSelectedVehicle.Weapons.Add(objWeapon);
                    }
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_ModifyVehicleGear"), LanguageManager.GetString("MessageTitle_ModifyVehicleGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlNode objXmlSensorGear = objSensor.GetNode();
            string strCategories = string.Empty;
            XmlNodeList xmlAddonCategoryList = objXmlSensorGear?.SelectNodes("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                    strCategories += objXmlCategory.InnerText + ",";
                // Remove the trailing comma.
                strCategories = strCategories.Substring(0, strCategories.Length - 1);
            }
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                frmSelectGear frmPickGear = new frmSelectGear(_objCharacter, 0, 1, objSensor, strCategories);
                if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                    frmPickGear.ShowNegativeCapacityOnly = true;
                frmPickGear.ShowDialog(this);
                Cursor = Cursors.Default;

                if (frmPickGear.DialogResult == DialogResult.Cancel)
                {
                    frmPickGear.Dispose();
                    break;
                }
                blnAddAgain = frmPickGear.AddAgain;

                // Open the Gear XML file and locate the selected piece.
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(_objCharacter);
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                decimal decCost = objGear.TotalCost;

                // Multiply the cost if applicable.
                char chrAvail = objGear.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleGear") + LanguageManager.GetString("String_Space") + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                if (lstWeapons.Count > 0)
                {
                    _objCharacter.Vehicles.FindVehicleGear(objSensor.InternalId, out Vehicle objVehicle,
                        out WeaponAccessory _, out Cyberware _);
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        objVehicle.Weapons.Add(objWeapon);
                    }
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddMod_Click(object sender, EventArgs e)
        {
            while (treVehicles.SelectedNode != null && treVehicles.SelectedNode.Level > 1)
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;

            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Vehicle Mod window.
            if (!(objSelectedNode?.Tag is Vehicle objVehicle))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_SelectVehicle"), LanguageManager.GetString("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");

            bool blnAddAgain;

            do
            {
                frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter, objVehicle.Mods)
                {
                    // Pass the selected vehicle on to the form.
                    SelectedVehicle = objVehicle
                };

                frmPickVehicleMod.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                {
                    frmPickVehicleMod.Dispose();
                    break;
                }
                blnAddAgain = frmPickVehicleMod.AddAgain;

                XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = \"" + frmPickVehicleMod.SelectedMod + "\"]");

                VehicleMod objMod = new VehicleMod(_objCharacter)
                {
                    DiscountCost = frmPickVehicleMod.BlackMarketDiscount
                };
                objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, objVehicle, frmPickVehicleMod.Markup);
                // Make sure that the Armor Rating does not exceed the maximum allowed by the Vehicle.
                if (objMod.Name.StartsWith("Armor"))
                {
                    if (objMod.Rating > objVehicle.MaxArmor)
                    {
                        objMod.Rating = objVehicle.MaxArmor;
                    }
                }
                else if (objMod.Category == "Handling")
                {
                    if (objMod.Rating > objVehicle.MaxHandling)
                    {
                        objMod.Rating = objVehicle.MaxHandling;
                    }
                }
                else if (objMod.Category == "Speed")
                {
                    if (objMod.Rating > objVehicle.MaxSpeed)
                    {
                        objMod.Rating = objVehicle.MaxSpeed;
                    }
                }
                else if (objMod.Category == "Acceleration")
                {
                    if (objMod.Rating > objVehicle.MaxAcceleration)
                    {
                        objMod.Rating = objVehicle.MaxAcceleration;
                    }
                }
                else if (objMod.Category == "Sensor")
                {
                    if (objMod.Rating > objVehicle.MaxSensor)
                    {
                        objMod.Rating = objVehicle.MaxSensor;
                    }
                }
                else if (objMod.Name.StartsWith("Pilot Program"))
                {
                    if (objMod.Rating > objVehicle.MaxPilot)
                    {
                        objMod.Rating = objVehicle.MaxPilot;
                    }
                }

                // Check the item's Cost and make sure the character can afford it.
                decimal decOriginalCost = objVehicle.TotalCost;
                if (frmPickVehicleMod.FreeCost)
                    objMod.Cost = "0";

                objVehicle.Mods.Add(objMod);

                // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                if (_objCharacter.Options.EnforceCapacity)
                {
                    bool blnOverCapacity = false;
                    if (_objCharacter.Options.BookEnabled("R5"))
                    {
                        if (objVehicle.IsDrone && GlobalOptions.Dronemods)
                        {
                            if (objVehicle.DroneModSlotsUsed > objVehicle.DroneModSlots)
                                blnOverCapacity = true;
                        }
                        else
                        {
                            int intUsed = objVehicle.CalcCategoryUsed(objMod.Category);
                            int intAvail = objVehicle.CalcCategoryAvail(objMod.Category);
                            if (intUsed > intAvail)
                                blnOverCapacity = true;
                        }
                    }
                    else if (objVehicle.Slots < objVehicle.SlotsUsed)
                    {
                        blnOverCapacity = true;
                    }

                    if (blnOverCapacity)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CapacityReached"), LanguageManager.GetString("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objVehicle.Mods.Remove(objMod);
                        frmPickVehicleMod.Dispose();
                        continue;
                    }
                }

                decimal decCost = objVehicle.TotalCost - decOriginalCost;

                // Multiply the cost if applicable.
                char chrAvail = objMod.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                if (decCost > _objCharacter.Nuyen)
                {
                    objVehicle.Mods.Remove(objMod);
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmPickVehicleMod.Dispose();
                    continue;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleMod") + LanguageManager.GetString("String_Space") + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicleMod, objMod.InternalId);
                objExpense.Undo = objUndo;

                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["improvesensor"] != null)
                {
                    objVehicle.ChangeVehicleSensor(treVehicles, true, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);

                frmPickVehicleMod.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddWeaponWeapon_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasInternalId selectedObject)) return;
            string strSelectedId = selectedObject.InternalId;
            // Make sure that a Weapon Mount has been selected.
            // Attempt to locate the selected VehicleMod.
            VehicleMod objMod = null;
            WeaponMount objWeaponMount = null;
            Vehicle objVehicle = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                objWeaponMount = _objCharacter.Vehicles.FindVehicleWeaponMount(strSelectedId, out objVehicle);
                if (objWeaponMount == null)
                {
                    objMod = _objCharacter.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out objVehicle, out objWeaponMount);
                    if (objMod != null)
                    {
                        if (!objMod.Name.StartsWith("Mechanical Arm") && !objMod.Name.Contains("Drone Arm"))
                        {
                            objMod = null;
                        }
                    }
                }
            }

            if (objWeaponMount == null && objMod == null)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CannotAddWeapon"), LanguageManager.GetString("MessageTitle_CannotAddWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = AddWeaponToWeaponMount(objWeaponMount, objMod, objVehicle);
            }
            while (blnAddAgain);
        }

        private bool AddWeaponToWeaponMount(WeaponMount objWeaponMount, VehicleMod objMod, Vehicle objVehicle)
        {
            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(_objCharacter)
            {
                LimitToCategories = objMod == null ? objWeaponMount.AllowedWeaponCategories : objMod.WeaponMountCategories
            };
            frmPickWeapon.ShowDialog();

            if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Weapons XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + frmPickWeapon.SelectedWeapon + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Weapon objWeapon = new Weapon(_objCharacter)
            {
                ParentVehicle = objVehicle,
                ParentMount = objMod == null ? objWeaponMount : null
            };
            objWeapon.Create(objXmlWeapon, lstWeapons);

            decimal decCost = objWeapon.TotalCost;
            // Apply a markup if applicable.
            if (frmPickWeapon.Markup != 0)
            {
                decCost *= 1 + (frmPickWeapon.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objWeapon.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                decCost *= _objCharacter.Options.RestrictedCostMultiplier;
            if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

            if (!frmPickWeapon.FreeCost)
            {
                // Check the item's Cost and make sure the character can afford it.
                if (decCost > _objCharacter.Nuyen)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return frmPickWeapon.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeapon") + LanguageManager.GetString("String_Space") + objWeapon.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeapon, objWeapon.InternalId);
                objExpense.Undo = objUndo;
            }

            if (objMod != null)
                objMod.Weapons.Add(objWeapon);
            else
                objWeaponMount.Weapons.Add(objWeapon);

            foreach (Weapon objLoopWeapon in lstWeapons)
            {
                if (objMod != null)
                    objMod.Weapons.Add(objLoopWeapon);
                else
                    objWeaponMount.Weapons.Add(objLoopWeapon);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);

            return frmPickWeapon.AddAgain;
        }

        private void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle)) return;
            frmCreateWeaponMount frmPickVehicleMod = new frmCreateWeaponMount(objVehicle, _objCharacter)
            {
                AllowDiscounts = true
            };
            frmPickVehicleMod.ShowDialog(this);

            if (frmPickVehicleMod.DialogResult != DialogResult.Cancel)
            {
                if (!frmPickVehicleMod.FreeCost)
                {
                    // Check the item's Cost and make sure the character can afford it.
                    decimal decCost = frmPickVehicleMod.WeaponMount.TotalCost;
                    // Apply a markup if applicable.
                    if (frmPickVehicleMod.Markup != 0)
                    {
                        decCost *= 1 + (frmPickVehicleMod.Markup / 100.0m);
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = frmPickVehicleMod.WeaponMount.TotalAvailTuple().Suffix;
                    if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                        decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                    if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                        decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeaponAccessory") + LanguageManager.GetString("String_Space") + frmPickVehicleMod.WeaponMount.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMount, frmPickVehicleMod.WeaponMount.InternalId);
                    objExpense.Undo = objUndo;
                }

                objVehicle.WeaponMounts.Add(frmPickVehicleMod.WeaponMount);

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);
            }
        }

        private void tsVehicleAddWeaponAccessory_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_VehicleWeaponAccessories"), LanguageManager.GetString("MessageTitle_VehicleWeaponAccessories"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            XmlNode objXmlWeapon = objWeapon.GetNode();
            if (objXmlWeapon == null)
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CannotFindWeapon"), LanguageManager.GetString("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                // Make sure the Weapon allows Accessories to be added to it.
                if (!objWeapon.AllowAccessory)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CannotModifyWeapon"), LanguageManager.GetString("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                frmSelectWeaponAccessory frmPickWeaponAccessory = new frmSelectWeaponAccessory(_objCharacter)
                {
                    ParentWeapon = objWeapon
                };
                frmPickWeaponAccessory.ShowDialog();

                if (frmPickWeaponAccessory.DialogResult == DialogResult.Cancel)
                {
                    frmPickWeaponAccessory.Dispose();
                    break;
                }
                blnAddAgain = frmPickWeaponAccessory.AddAgain;

                // Locate the selected piece.
                objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[id = \"" + frmPickWeaponAccessory.SelectedAccessory + "\"]");

                WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, Convert.ToInt32(frmPickWeaponAccessory.SelectedRating));
                objAccessory.Parent = objWeapon;

                // Check the item's Cost and make sure the character can afford it.
                decimal intOriginalCost = objWeapon.TotalCost;
                objWeapon.WeaponAccessories.Add(objAccessory);

                decimal decCost = objWeapon.TotalCost - intOriginalCost;
                // Apply a markup if applicable.
                if (frmPickWeaponAccessory.Markup != 0)
                {
                    decCost *= 1 + (frmPickWeaponAccessory.Markup / 100.0m);
                }

                // Multiply the cost if applicable.
                char chrAvail = objAccessory.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                    decCost *= _objCharacter.Options.RestrictedCostMultiplier;
                if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                    decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

                if (!frmPickWeaponAccessory.FreeCost)
                {
                    if (decCost > _objCharacter.Nuyen)
                    {
                        objWeapon.WeaponAccessories.Remove(objAccessory);
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                        frmPickWeaponAccessory.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeaponAccessory") + LanguageManager.GetString("String_Space") + objAccessory.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                    _objCharacter.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponAccessory, objAccessory.InternalId);
                    objExpense.Undo = objUndo;
                }

                MakeDirtyWithCharacterUpdate?.Invoke(null, null);

                frmPickWeaponAccessory.Dispose();
            }
            while (blnAddAgain);
        }

        private bool AddUnderbarrelWeapon(Weapon objSelectedWeapon, string strExpenseString)
        {
            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(_objCharacter)
            {
                LimitToCategories = "Underbarrel Weapons",
                Mounts = objSelectedWeapon.AccessoryMounts,

                Underbarrel = true
            };

            frmPickWeapon.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Weapons XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + frmPickWeapon.SelectedWeapon + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Weapon objWeapon = new Weapon(_objCharacter)
            {
                ParentVehicle = objSelectedWeapon.ParentVehicle
            };
            objWeapon.Create(objXmlWeapon, lstWeapons);
            objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;
            objWeapon.Parent = objSelectedWeapon;
            if (objSelectedWeapon.AllowAccessory == false)
                objWeapon.AllowAccessory = false;

            decimal decCost = objWeapon.TotalCost;
            // Apply a markup if applicable.
            if (frmPickWeapon.Markup != 0)
            {
                decCost *= 1 + (frmPickWeapon.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objWeapon.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && _objCharacter.Options.MultiplyRestrictedCost)
                decCost *= _objCharacter.Options.RestrictedCostMultiplier;
            if (chrAvail == 'F' && _objCharacter.Options.MultiplyForbiddenCost)
                decCost *= _objCharacter.Options.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickWeapon.FreeCost)
            {
                if (decCost > _objCharacter.Nuyen)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickWeapon.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decCost * -1, strExpenseString + LanguageManager.GetString("String_Space") + objWeapon.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeapon, objWeapon.InternalId);
                objExpense.Undo = objUndo;
            }

            objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);

            foreach (Weapon objLoopWeapon in lstWeapons)
            {
                if (objSelectedWeapon.AllowAccessory == false)
                    objLoopWeapon.AllowAccessory = false;
                objSelectedWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);

            return frmPickWeapon.AddAgain;
        }

        private void tsVehicleAddUnderbarrelWeapon_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_VehicleWeaponUnderbarrel"), LanguageManager.GetString("MessageTitle_VehicleWeaponUnderbarrel"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = AddUnderbarrelWeapon(objWeapon, LanguageManager.GetString("String_ExpensePurchaseVehicleWeapon"));
            }
            while (blnAddAgain);
        }

        private void tsVehicleSell_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treVehicles.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;

                vendorTrash.Sell(_objCharacter, frmSell.SellPercent);
            }
            else
            {
                Utils.BreakIfDebug();
            }

            MakeDirtyWithCharacterUpdate?.Invoke(null, null);
        }
        #endregion

        #region TreeView Build/Update Methods
        private void VehicleCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshVehicles(notifyCollectionChangedEventArgs);
        }
        private void VehicleLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshVehicleLocations(notifyCollectionChangedEventArgs);
        }

        protected void RefreshVehicleLocations(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = new TreeNode
                            {
                                Tag = objLocation,
                                Text = objLocation.DisplayName(),
                                ContextMenuStrip = cmsVehicleLocation
                            };
                            treVehicles.Nodes.Insert(intNewIndex, objNode);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objNode != null)
                            {
                                objNode.Remove();
                                if (objNode.Nodes.Count > 0)
                                {
                                    if (nodRoot == null)
                                    {
                                        nodRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectedVehicles",
                                            Text = LanguageManager.GetString("Node_SelectedVehicles")
                                        };
                                        treVehicles.Nodes.Insert(0, nodRoot);
                                    }
                                    for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                    {
                                        TreeNode nodArmor = objNode.Nodes[i];
                                        nodArmor.Remove();
                                        nodRoot.Nodes.Add(nodArmor);
                                    }
                                }
                            }
                            objLocation.Remove(_objCharacter);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int intNewItemsIndex = 0;
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objLocation != null)
                            {
                                if (notifyCollectionChangedEventArgs.NewItems[intNewItemsIndex] is Location objNewLocation)
                                {
                                    objNode.Tag = objNewLocation;
                                    objNode.Text = objNewLocation.DisplayName();
                                }
                                intNewItemsIndex += 1;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        List<Tuple<Location, TreeNode>> lstMoveNodes = new List<Tuple<Location, TreeNode>>();
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objLocation != null)
                            {
                                lstMoveNodes.Add(new Tuple<Location, TreeNode>(objLocation, objNode));
                                objNode.Remove();
                            }
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            Tuple<Location, TreeNode> objLocationTuple = lstMoveNodes.FirstOrDefault(x => x.Item1 == objLocation);
                            if (objLocationTuple != null)
                            {
                                treVehicles.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                                intNewIndex += 1;
                                lstMoveNodes.Remove(objLocationTuple);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (Location objLocation in _objCharacter.WeaponLocations)
                        {
                            TreeNode nodLocation = treVehicles.FindNodeByTag(objLocation, false);
                            if (nodLocation == null) continue;
                            if (nodLocation.Nodes.Count > 0)
                            {
                                if (nodRoot == null)
                                {
                                    nodRoot = new TreeNode
                                    {
                                        Tag = "Node_SelectedVehicles",
                                        Text = LanguageManager.GetString("Node_SelectedVehicles")
                                    };
                                    treVehicles.Nodes.Insert(0, nodRoot);
                                }
                                for (int i = nodLocation.Nodes.Count - 1; i >= 0; --i)
                                {
                                    TreeNode nodWeapon = nodLocation.Nodes[i];
                                    nodWeapon.Remove();
                                    nodRoot.Nodes.Add(nodWeapon);
                                }
                            }
                            objLocation.Remove(_objCharacter);
                        }
                    }
                    break;
            }

            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
        }

        protected void RefreshLocationsInVehicle(Vehicle objVehicle, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = treVehicles.FindNodeByTag(objVehicle);
            if (nodRoot == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = new TreeNode
                            {
                                Tag = objLocation,
                                Text = objLocation.DisplayName(),
                                ContextMenuStrip = cmsVehicleLocation
                            };
                            treVehicles.Nodes.Insert(intNewIndex, objNode);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objNode != null)
                            {
                                objNode.Remove();
                                for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                {
                                    TreeNode nodVehicle = objNode.Nodes[i];
                                    nodVehicle.Remove();
                                    nodRoot.Nodes.Add(nodVehicle);
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int intNewItemsIndex = 0;
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objLocation != null)
                            {
                                if (notifyCollectionChangedEventArgs.NewItems[intNewItemsIndex] is Location objNewLocation)
                                {
                                    objNode.Tag = objNewLocation;
                                    objNode.Text = objNewLocation.DisplayName();
                                }
                                intNewItemsIndex += 1;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        List<Tuple<Location, TreeNode>> lstMoveNodes = new List<Tuple<Location, TreeNode>>();
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objNode != null)
                            {
                                lstMoveNodes.Add(new Tuple<Location, TreeNode>(objLocation, objNode));
                                objNode.Remove();
                            }
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (Location objLocation in notifyCollectionChangedEventArgs.NewItems)
                        {
                            Tuple<Location, TreeNode> objLocationTuple = lstMoveNodes.FirstOrDefault(x => x.Item1 == objLocation);
                            if (objLocationTuple != null)
                            {
                                treVehicles.Nodes.Insert(intNewIndex, objLocationTuple.Item2);
                                intNewIndex += 1;
                                lstMoveNodes.Remove(objLocationTuple);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (Location objLocation in objVehicle.Locations)
                        {
                            TreeNode objNode = treVehicles.FindNodeByTag(objLocation, false);
                            if (objNode != null)
                            {
                                objNode.Remove();
                                for (int i = objNode.Nodes.Count - 1; i >= 0; --i)
                                {
                                    TreeNode nodVehicle = objNode.Nodes[i];
                                    nodVehicle.Remove();
                                    nodRoot.Nodes.Add(nodVehicle);
                                }
                            }
                        }
                    }
                    break;
            }

            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
        }

        protected void RefreshVehicleMods(IHasInternalId objParent, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodParent = treVehicles.FindNodeByTag(objParent);
            if (nodParent == null)
                return;

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objVehicleMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                            objVehicleMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            objVehicleMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                            objVehicleMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicleMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicleMod.Cyberware.Count, y));
                            foreach (Weapon objWeapon in objVehicleMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent.FindNodeByTag(objVehicleMod)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        if (funcOffset != null)
                            intNewIndex += funcOffset.Invoke();
                        foreach (VehicleMod objVehicleMod in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objVehicleMod, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodParent.Nodes.Clear();
                    }
                    break;
            }

            void AddToTree(VehicleMod objVehicleMod, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicleMod.CreateTreeNode(cmsVehicle, cmsCyberware, cmsVehicleCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                if (objNode == null)
                    return;

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        protected void RefreshVehicleWeaponMounts(IHasInternalId objParent, Func<int> funcOffset, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs == null)
                return;

            TreeNode nodVehicleParent = treVehicles.FindNodeByTag(objParent);
            if (nodVehicleParent == null)
                return;
            TreeNode nodParent = nodVehicleParent.FindNode("String_WeaponMounts", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => RefreshVehicleMods(objWeaponMount, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            if (nodParent != null)
                            {
                                nodParent.FindNodeByTag(objWeaponMount)?.Remove();
                                if (nodParent.Nodes.Count == 0)
                                    nodParent.Remove();
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            objWeaponMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                            objWeaponMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                            }
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            objWeaponMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                (x, y) => RefreshVehicleMods(objWeaponMount, null, y));
                            objWeaponMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objWeaponMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objWeaponMount.Mods.Count, y));
                            foreach (Weapon objWeapon in objWeaponMount.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            foreach (VehicleMod objMod in objWeaponMount.Mods)
                            {
                                objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                                foreach (Cyberware objCyberware in objMod.Cyberware)
                                    objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                foreach (Weapon objWeapon in objMod.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.OldItems)
                        {
                            nodParent?.FindNodeByTag(objWeaponMount)?.Remove();
                        }
                        int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                        foreach (WeaponMount objWeaponMount in notifyCollectionChangedEventArgs.NewItems)
                        {
                            AddToTree(objWeaponMount, intNewIndex);
                            intNewIndex += 1;
                        }
                        treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        nodParent?.Remove();
                    }
                    break;
            }

            void AddToTree(WeaponMount objWeaponMount, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objWeaponMount.CreateTreeNode(cmsWeaponMount, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsCyberware, cmsVehicleCyberwareGear, cmsVehicle);
                if (objNode == null)
                    return;

                if (nodParent == null)
                {
                    nodParent = new TreeNode
                    {
                        Tag = "String_WeaponMounts",
                        Text = LanguageManager.GetString("String_WeaponMounts")
                    };
                    nodVehicleParent.Nodes.Insert(funcOffset?.Invoke() ?? 0, nodParent);
                    nodParent.Expand();
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }

        protected void RefreshVehicles(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null)
        {
            string strSelectedId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            TreeNode nodRoot = null;

            if (notifyCollectionChangedEventArgs == null)
            {
                treVehicles.Nodes.Clear();

                // Start by populating Locations.
                foreach (Location objLocation in _objCharacter.VehicleLocations)
                {
                    treVehicles.Nodes.Add(objLocation.CreateTreeNode(cmsVehicleLocation));
                }

                // Add Vehicles.
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    AddToTree(objVehicle, -1, false);
                    objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(objVehicle, null, y));
                    objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(objVehicle, () => objVehicle.Mods.Count, y));
                    objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                        foreach (Cyberware objCyberware in objMod.Cyberware)
                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                        foreach (Weapon objWeapon in objMod.Weapons)
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                    }
                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                            (x, y) => RefreshVehicleMods(objMount, null, y));
                        objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                        foreach (Weapon objWeapon in objMount.Weapons)
                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        foreach (VehicleMod objMod in objMount.Mods)
                        {
                            foreach (Cyberware objCyberware in objMod.Cyberware)
                                objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                            foreach (Weapon objWeapon in objMod.Weapons)
                                objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                        }
                    }
                    foreach (Weapon objWeapon in objVehicle.Weapons)
                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                    objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                    foreach (Gear objGear in objVehicle.Gear)
                        objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                    objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(objVehicle, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location == null), y));
                }

                treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
            }
            else
            {
                nodRoot = treVehicles.FindNode("Node_SelectedVehicles", false);

                switch (notifyCollectionChangedEventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(objVehicle, null, y));
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(objVehicle, () => objVehicle.Mods.Count, y));
                                objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                    objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshVehicleMods(objMount, null, y));
                                    objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(objVehicle, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location == null), y));
                                intNewIndex += 1;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                    objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                objVehicle.Gear.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                objVehicle.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.WeaponMounts.RemoveTaggedCollectionChanged(treVehicles);
                                objVehicle.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                    objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.RemoveTaggedCollectionChanged(treVehicles);
                                    objMount.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(false, treVehicles);
                                        objMod.Weapons.RemoveTaggedCollectionChanged(treVehicles);
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(false, treVehicles);
                                objVehicle.Gear.RemoveTaggedCollectionChanged(treVehicles);
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(false, treVehicles);
                                objVehicle.Locations.RemoveTaggedCollectionChanged(treVehicles);
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                objVehicle.Mods.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleMods(objVehicle, null, y));
                                objVehicle.WeaponMounts.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshVehicleWeaponMounts(objVehicle, () => objVehicle.Mods.Count, y));
                                objVehicle.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objVehicle.Mods.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                                    foreach (Cyberware objCyberware in objMod.Cyberware)
                                        objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                    objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                    foreach (Weapon objWeapon in objMod.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                }
                                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                                {
                                    objMount.Mods.AddTaggedCollectionChanged(treVehicles,
                                        (x, y) => RefreshVehicleMods(objMount, null, y));
                                    objMount.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMount.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMount.Mods.Count, y));
                                    foreach (Weapon objWeapon in objMount.Weapons)
                                        objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    foreach (VehicleMod objMod in objMount.Mods)
                                    {
                                        objMod.Cyberware.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenCyberware(treVehicles, cmsCyberware, cmsVehicleCyberwareGear, null, y));
                                        foreach (Cyberware objCyberware in objMod.Cyberware)
                                            objCyberware.SetupChildrenCyberwareCollectionChanged(true, treVehicles, cmsCyberware, cmsVehicleCyberwareGear);
                                        objMod.Weapons.AddTaggedCollectionChanged(treVehicles, (x, y) => objMod.RefreshChildrenWeapons(treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, () => objMod.Cyberware.Count, y));
                                        foreach (Weapon objWeapon in objMod.Weapons)
                                            objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                    }
                                }
                                foreach (Weapon objWeapon in objVehicle.Weapons)
                                    objWeapon.SetupChildrenWeaponsCollectionChanged(true, treVehicles, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                                objVehicle.Gear.AddTaggedCollectionChanged(treVehicles, (x, y) => objVehicle.RefreshChildrenGears(treVehicles, cmsVehicleGear, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0), y));
                                foreach (Gear objGear in objVehicle.Gear)
                                    objGear.SetupChildrenGearsCollectionChanged(true, treVehicles, cmsVehicleGear);
                                objVehicle.Locations.AddTaggedCollectionChanged(treVehicles, (x, y) => RefreshLocationsInVehicle(objVehicle, () => objVehicle.Mods.Count + objVehicle.Weapons.Count + (objVehicle.WeaponMounts.Count > 0 ? 1 : 0) + objVehicle.Gear.Count(z => z.Location != null), y));
                                intNewIndex += 1;
                            }
                            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.OldItems)
                            {
                                treVehicles.FindNodeByTag(objVehicle)?.Remove();
                            }
                            int intNewIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
                            foreach (Vehicle objVehicle in notifyCollectionChangedEventArgs.NewItems)
                            {
                                AddToTree(objVehicle, intNewIndex);
                                intNewIndex += 1;
                            }
                            treVehicles.SelectedNode = treVehicles.FindNode(strSelectedId);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            RefreshVehicles();
                        }
                        break;
                }
            }

            void AddToTree(Vehicle objVehicle, int intIndex = -1, bool blnSingleAdd = true)
            {
                TreeNode objNode = objVehicle.CreateTreeNode(cmsVehicle, cmsVehicleLocation, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsWeaponMount, cmsCyberware, cmsVehicleCyberwareGear);
                if (objNode == null)
                    return;

                TreeNode nodParent = null;
                if (objVehicle.Location != null)
                {
                    nodParent = treVehicles.FindNodeByTag(objVehicle.Location, false);
                }
                if (nodParent == null)
                {
                    if (nodRoot == null)
                    {
                        nodRoot = new TreeNode
                        {
                            Tag = "Node_SelectedVehicles",
                            Text = LanguageManager.GetString("Node_SelectedVehicles")
                        };
                        treVehicles.Nodes.Insert(0, nodRoot);
                    }
                    nodParent = nodRoot;
                }

                if (intIndex >= 0)
                    nodParent.Nodes.Insert(intIndex, objNode);
                else
                    nodParent.Nodes.Add(objNode);
                nodParent.Expand();
                if (blnSingleAdd)
                    treVehicles.SelectedNode = objNode;
            }
        }
        #endregion
    }
}
