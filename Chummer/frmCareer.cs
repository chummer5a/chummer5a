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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Chummer.UI.Attributes;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class frmCareer : CharacterShared
    {
        // Set the default culture to en-US so we work with decimals correctly.
        private bool _blnSkipRefresh;
        private bool _blnSkipUpdate;
        private bool _blnLoading = true;
        private readonly bool _blnSkipToolStripRevert = false;
        private bool _blnReapplyImprovements;
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnDraggingGear;

        private readonly ListViewColumnSorter _lvwKarmaColumnSorter;
        private readonly ListViewColumnSorter _lvwNuyenColumnSorter;

        public Action<object> DiceRollerOpened { get; set; }
        public Action<Character, int> DiceRollerOpenedInt { get; set; }

        #region Form Events
        [Obsolete("This constructor is for use by form designers only.", true)]
        public frmCareer()
        {
            InitializeComponent();
        }
        public frmCareer(Character objCharacter) : base(objCharacter)
        {
            InitializeComponent();

            // Add EventHandlers for the MAG and RES enabled events and tab enabled events.
            CharacterObject.PropertyChanged += OnCharacterPropertyChanged;

            tabPowerUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            tabSkillsUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            
            Program.MainForm.OpenCharacterForms.Add(this);
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);

            ContextMenuStrip[] lstCMSToTranslate = {
                cmsAdvancedLifestyle,
                cmsAdvancedProgram,
                cmsAmmoExpense,
                cmsArmor,
                cmsArmorGear,
                cmsArmorLocation,
                cmsArmorMod,
                cmsBioware,
                cmsComplexForm,
                cmsComplexFormPlugin,
                cmsCritterPowers,
                cmsCyberware,
                cmsCyberwareGear,
                cmsVehicleCyberware,
                cmsVehicleCyberwareGear,
                cmsDeleteArmor,
                cmsDeleteCyberware,
                cmsDeleteGear,
                cmsDeleteVehicle,
                cmsDeleteWeapon,
                cmsGear,
                cmsGearButton,
                cmsGearLocation,
                cmsGearPlugin,
                cmsImprovement,
                cmsImprovementLocation,
                cmsInitiationNotes,
                cmsLifestyle,
                cmsLifestyleNotes,
                cmsLimitModifier,
                cmsMartialArts,
                cmsMetamagic,
                cmsQuality,
                cmsSpell,
                cmsSpellButton,
                cmsTechnique,
                cmsUndoKarmaExpense,
                cmsUndoNuyenExpense,
                cmsVehicle,
                cmsVehicleGear,
                cmsVehicleLocation,
                cmsVehicleWeapon,
                cmsVehicleWeaponAccessory,
                cmsVehicleWeaponAccessoryGear,
                cmsVehicleWeaponMod,
                cmsWeapon,
                cmsWeaponAccessory,
                cmsWeaponAccessoryGear,
                cmsWeaponLocation,
                cmsWeaponMod,
                cmsWeaponMount
            };
            // Update the text in the Menus so they can be merged with frmMain properly.
            foreach (ToolStripMenuItem objItem in mnuCreateMenu.Items.OfType<ToolStripMenuItem>())
            {
                LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
            }
            foreach (ContextMenuStrip objCMS in lstCMSToTranslate)
            {
                if (objCMS != null)
                {
                    foreach (ToolStripMenuItem objItem in objCMS.Items.OfType<ToolStripMenuItem>())
                    {
                        LanguageManager.TranslateToolStripItemsRecursively(objItem, GlobalOptions.Language);
                    }
                }
            }

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

            SetTooltips();
            MoveControls();
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

        private void frmCareer_Load(object sender, EventArgs e)
        {
            Timekeeper.Finish("load_free");

            Timekeeper.Start("load_frm_career");
            
            mnuSpecialAddBiowareSuite.Visible = CharacterObjectOptions.AllowBiowareSuites;
            
            txtGroupName.DataBindings.Add("Text", CharacterObject, nameof(Character.GroupName), false, DataSourceUpdateMode.OnPropertyChanged);
            txtGroupNotes.DataBindings.Add("Text", CharacterObject, nameof(Character.GroupNotes), false, DataSourceUpdateMode.OnPropertyChanged);
            chkJoinGroup.Checked = CharacterObject.GroupMember;

            Utils.DoDatabinding(txtCharacterName, "Text", CharacterObject, nameof(Character.Name));
            Utils.DoDatabinding(txtSex, "Text", CharacterObject, nameof(Character.Sex));
            Utils.DoDatabinding(txtAge, "Text", CharacterObject, nameof(Character.Age));
            Utils.DoDatabinding(txtEyes, "Text", CharacterObject, nameof(Character.Eyes));
            Utils.DoDatabinding(txtHeight, "Text", CharacterObject, nameof(Character.Height));
            Utils.DoDatabinding(txtWeight, "Text", CharacterObject, nameof(Character.Weight));
            Utils.DoDatabinding(txtSkin, "Text", CharacterObject, nameof(Character.Skin));
            Utils.DoDatabinding(txtHair, "Text", CharacterObject, nameof(Character.Hair));
            Utils.DoDatabinding(txtDescription, "Text", CharacterObject, nameof(Character.Description));
            Utils.DoDatabinding(txtBackground, "Text", CharacterObject, nameof(Character.Background));
            Utils.DoDatabinding(txtConcept, "Text", CharacterObject, nameof(Character.Concept));
            Utils.DoDatabinding(txtNotes, "Text", CharacterObject, nameof(Character.Notes));
            Utils.DoDatabinding(txtGameNotes, "Text", CharacterObject, nameof(Character.GameNotes));
            Utils.DoDatabinding(txtAlias, "Text", CharacterObject, nameof(Character.Alias));
            Utils.DoDatabinding(txtPlayerName, "Text", CharacterObject, nameof(Character.PlayerName));


            Utils.DoDatabinding(chkInitiationGroup, "Checked", CharacterObject, nameof(Character.GroupMember));

            // If the character has a mugshot, decode it and put it in the PictureBox.
            if (CharacterObject.Mugshots.Count > 0)
            {
                nudMugshotIndex.Minimum = 1;
                nudMugshotIndex.Maximum = CharacterObject.Mugshots.Count;
                nudMugshotIndex.Value = Math.Max(CharacterObject.MainMugshotIndex, 0) + 1;
            }
            else
            {
                nudMugshotIndex.Minimum = 0;
                nudMugshotIndex.Maximum = 0;
                nudMugshotIndex.Value = 0;
            }
            lblNumMugshots.Text = LanguageManager.GetString("String_Of", GlobalOptions.Language) + CharacterObject.Mugshots.Count.ToString(GlobalOptions.CultureInfo);

            // Refresh character information fields.
            RefreshMetatypeFields();
            Utils.DoDatabinding(nudStreetCred,  "Value", CharacterObject, nameof(Character.StreetCred));
            Utils.DoDatabinding(nudNotoriety,   "Value", CharacterObject, nameof(Character.Notoriety));
            Utils.DoDatabinding(nudPublicAware, "Value", CharacterObject, nameof(Character.PublicAwareness));
            Utils.DoDatabinding(cmdAddMetamagic, "Enabled", CharacterObject, nameof(Character.AddInitiationsAllowed));

            RefreshQualities(treQualities, cmsQuality);
            RefreshSpirits(panSpirits, panSprites);
            RefreshSpells(treSpells, treMetamagic, cmsSpell, cmsInitiationNotes);
            RefreshComplexForms(treComplexForms, treMetamagic, cmsComplexForm, cmsInitiationNotes);
            RefreshPowerCollectionListChanged(treMetamagic, cmsMetamagic, cmsInitiationNotes);
            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
            RefreshAIPrograms(treAIPrograms, cmsAdvancedProgram);
            RefreshCritterPowers(treCritterPowers, cmsCritterPowers);
            mnuSpecialPossess.Visible = CharacterObject.CritterPowers.Any(x => x.Name == "Inhabitation" || x.Name == "Possession");
            RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique);
            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle);
            RefreshCustomImprovements(treImprovements, treLimit, cmsImprovementLocation, cmsImprovement, cmsLimitModifier);
            RefreshCalendar(lstCalendar);
            RefreshContacts(panContacts, panEnemies, panPets);

            RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear);
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
            RefreshFociFromGear(treFoci, null);
            RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear);
            RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
            RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsWeaponMount, cmsVehicleCyberware, cmsVehicleCyberwareGear);

            // Set up events that would change various lists
            CharacterObject.Spells.CollectionChanged += SpellCollectionChanged;
            CharacterObject.ComplexForms.CollectionChanged += ComplexFormCollectionChanged;
            CharacterObject.Arts.CollectionChanged += ArtCollectionChanged;
            CharacterObject.Enhancements.CollectionChanged += EnhancementCollectionChanged;
            CharacterObject.Metamagics.CollectionChanged += MetamagicCollectionChanged;
            CharacterObject.InitiationGrades.CollectionChanged += InitiationGradeCollectionChanged;
            CharacterObject.Powers.ListChanged += PowersListChanged;
            CharacterObject.Powers.BeforeRemove += PowersBeforeRemove;
            CharacterObject.AIPrograms.CollectionChanged += AIProgramCollectionChanged;
            CharacterObject.CritterPowers.CollectionChanged += CritterPowerCollectionChanged;
            CharacterObject.Qualities.CollectionChanged += QualityCollectionChanged;
            CharacterObject.MartialArts.CollectionChanged += MartialArtCollectionChanged;
            CharacterObject.Lifestyles.CollectionChanged += LifestyleCollectionChanged;
            CharacterObject.LimitModifiers.CollectionChanged += LimitModifierCollectionChanged;
            CharacterObject.Contacts.CollectionChanged += ContactCollectionChanged;
            CharacterObject.Armor.CollectionChanged += ArmorCollectionChanged;
            CharacterObject.ArmorLocations.CollectionChanged += ArmorLocationCollectionChanged;
            CharacterObject.Weapons.CollectionChanged += WeaponCollectionChanged;
            CharacterObject.WeaponLocations.CollectionChanged += WeaponLocationCollectionChanged;
            CharacterObject.Gear.CollectionChanged += GearCollectionChanged;
            CharacterObject.GearLocations.CollectionChanged += GearLocationCollectionChanged;
            CharacterObject.Cyberware.CollectionChanged += CyberwareCollectionChanged;
            CharacterObject.Vehicles.CollectionChanged += VehicleCollectionChanged;
            CharacterObject.VehicleLocations.CollectionChanged += VehicleLocationCollectionChanged;
            CharacterObject.Spirits.CollectionChanged += SpiritCollectionChanged;
            CharacterObject.Improvements.CollectionChanged += ImprovementCollectionChanged;
            CharacterObject.ImprovementGroups.CollectionChanged += ImprovementGroupCollectionChanged;
            CharacterObject.Calendar.ListChanged += CalendarWeekListChanged;
            CharacterObjectOptions.PropertyChanged += OptionsChanged;

            // Populate the Magician Traditions list.
            XPathNavigator xmlTraditionsBaseChummerNode = XmlManager.Load("traditions.xml").GetFastNavigator().SelectSingleNode("/chummer");
            List<ListItem> lstTraditions = new List<ListItem>();
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select("traditions/tradition[" + CharacterObjectOptions.BookXPath() + "]"))
                {
                    string strName = xmlTradition.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstTraditions.Add(new ListItem(strName, xmlTradition.SelectSingleNode("translate")?.Value ?? strName));
                }
            }
            if (lstTraditions.Count > 1)
            {
                lstTraditions.Sort(CompareListItems.CompareNames);
                lstTraditions.Insert(0, new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language)));
                cboTradition.BeginUpdate();
                cboTradition.ValueMember = "Value";
                cboTradition.DisplayMember = "Name";
                cboTradition.DataSource = lstTraditions;
                cboTradition.EndUpdate();
            }
            else
            {
                cboTradition.Visible = false;
                lblTraditionLabel.Visible = false;
            }

            // Populate the Magician Custom Drain Options list.
            List<ListItem> lstDrainAttributes = new List<ListItem>
            {
                ListItem.Blank
            };
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlDrain in xmlTraditionsBaseChummerNode.Select("drainattributes/drainattribute"))
                {
                    string strName = xmlDrain.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstDrainAttributes.Add(new ListItem(strName, xmlDrain.SelectSingleNode("translate")?.Value ?? strName));
                }
            }
            lstDrainAttributes.Sort(CompareListItems.CompareNames);
            cboDrain.BeginUpdate();
            cboDrain.ValueMember = nameof(ListItem.Value);
            cboDrain.DisplayMember = nameof(ListItem.Name);
            cboDrain.DataSource = lstDrainAttributes;
            Utils.DoDatabinding(cboDrain, "SelectedValue", CharacterObject, nameof(Character.TraditionDrain));
            cboDrain.EndUpdate();
            
            Utils.DoDatabinding(lblDrainAttributes, "Text", CharacterObject, nameof(Character.DisplayTraditionDrain));
            Utils.DoDatabinding(lblDrainAttributesValue, "Text", CharacterObject, nameof(Character.TraditionDrainValue));
            Utils.DoDatabinding(lblDrainAttributesValue, "ToolTipText", CharacterObject, nameof(Character.TraditionDrainValueToolTip));

            Utils.DoDatabinding(lblFadingAttributes, "Text", CharacterObject, nameof(Character.DisplayTechnomancerFading));
            Utils.DoDatabinding(lblFadingAttributesValue, "Text", CharacterObject, nameof(Character.TechnomancerFadingValue));
            Utils.DoDatabinding(lblFadingAttributesValue, "ToolTipText", CharacterObject, nameof(Character.TechnomancerFadingValueToolTip));

            HashSet<string> limit = new HashSet<string>();
            foreach (Improvement improvement in CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LimitSpiritCategory && x.Enabled))
            {
                limit.Add(improvement.ImprovedName);
            }

            // Populate the Magician Custom Spirits lists - Combat.
            List<ListItem> lstSpirit = new List<ListItem>
            {
                ListItem.Blank
            };
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlSpirit in xmlTraditionsBaseChummerNode.Select("spirits/spirit"))
                {
                    string strSpiritName = xmlSpirit.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strSpiritName))
                    {
                        if (limit.Count == 0 || limit.Contains(strSpiritName))
                        {
                            lstSpirit.Add(new ListItem(strSpiritName, xmlSpirit.SelectSingleNode("translate")?.Value ?? strSpiritName));
                        }
                    }
                }
            }
            lstSpirit.Sort(CompareListItems.CompareNames);

            List<ListItem> lstCombat = new List<ListItem>(lstSpirit);
            cboSpiritCombat.BeginUpdate();
            cboSpiritCombat.ValueMember = "Value";
            cboSpiritCombat.DisplayMember = "Name";
            cboSpiritCombat.DataSource = lstCombat;
            Utils.DoDatabinding(cboSpiritCombat, "SelectedValue", CharacterObject, nameof(Character.SpiritCombat));
            cboSpiritCombat.EndUpdate();

            List<ListItem> lstDetection = new List<ListItem>(lstSpirit);
            cboSpiritDetection.BeginUpdate();
            cboSpiritDetection.ValueMember = "Value";
            cboSpiritDetection.DisplayMember = "Name";
            cboSpiritDetection.DataSource = lstDetection;
            Utils.DoDatabinding(cboSpiritDetection, "SelectedValue", CharacterObject, nameof(Character.SpiritDetection));
            cboSpiritDetection.EndUpdate();

            List<ListItem> lstHealth = new List<ListItem>(lstSpirit);
            cboSpiritHealth.BeginUpdate();
            cboSpiritHealth.ValueMember = "Value";
            cboSpiritHealth.DisplayMember = "Name";
            cboSpiritHealth.DataSource = lstHealth;
            Utils.DoDatabinding(cboSpiritHealth, "SelectedValue", CharacterObject, nameof(Character.SpiritHealth));
            cboSpiritHealth.EndUpdate();

            List<ListItem> lstIllusion = new List<ListItem>(lstSpirit);
            cboSpiritIllusion.BeginUpdate();
            cboSpiritIllusion.ValueMember = "Value";
            cboSpiritIllusion.DisplayMember = "Name";
            cboSpiritIllusion.DataSource = lstIllusion;
            Utils.DoDatabinding(cboSpiritIllusion, "SelectedValue", CharacterObject, nameof(Character.SpiritIllusion));
            cboSpiritIllusion.EndUpdate();

            List<ListItem> lstManip = new List<ListItem>(lstSpirit);
            cboSpiritManipulation.BeginUpdate();
            cboSpiritManipulation.ValueMember = "Value";
            cboSpiritManipulation.DisplayMember = "Name";
            cboSpiritManipulation.DataSource = lstManip;
            Utils.DoDatabinding(cboSpiritManipulation, "SelectedValue", CharacterObject, nameof(Character.SpiritManipulation));
            cboSpiritManipulation.EndUpdate();

            // Populate the Technomancer Streams list.
            xmlTraditionsBaseChummerNode = XmlManager.Load("streams.xml").GetFastNavigator().SelectSingleNode("/chummer");
            List<ListItem> lstStreams = new List<ListItem>();
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select("traditions/tradition[" + CharacterObjectOptions.BookXPath() + "]"))
                {
                    string strName = xmlTradition.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstStreams.Add(new ListItem(strName, xmlTradition.SelectSingleNode("translate")?.Value ?? strName));
                }
            }
            if (lstStreams.Count > 1)
            {
                lstStreams.Sort(CompareListItems.CompareNames);
                lstStreams.Insert(0, new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language)));
                cboStream.BeginUpdate();
                cboStream.ValueMember = "Value";
                cboStream.DisplayMember = "Name";
                cboStream.DataSource = lstStreams;
                cboStream.EndUpdate();
            }
            else
            {
                cboStream.Visible = false;
                lblStreamLabel.Visible = false;
            }

            cboAttributeCategory.Visible = CharacterObject.MetatypeCategory == "Shapeshifter";
            if (CharacterObject.MetatypeCategory == "Shapeshifter")
            {
                XmlDocument objDoc = XmlManager.Load("metatypes.xml");
                XmlNode node = objDoc.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{CharacterObject.Metatype}\"]");
                List<ListItem> lstAttributeCategories = new List<ListItem>
                {
                    new ListItem("Shapeshifter", node?.SelectSingleNode("name/@translate")?.InnerText ?? CharacterObject.Metatype)
                };

                node = node?.SelectSingleNode($"metavariants/metavariant[name = \"{CharacterObject.Metavariant}\"]/name/@translate");

                lstAttributeCategories.Add(new ListItem("Standard", node?.InnerText ?? CharacterObject.Metavariant));

                lstAttributeCategories.Sort(CompareListItems.CompareNames);
                cboAttributeCategory.BeginUpdate();
                cboAttributeCategory.ValueMember = "Value";
                cboAttributeCategory.DisplayMember = "Name";
                cboAttributeCategory.DataSource = lstAttributeCategories;
                cboAttributeCategory.EndUpdate();
                cboAttributeCategory.SelectedValue = "Standard";
            }

            Utils.DoDatabinding(lblMysticAdeptMAGAdept, "Text", CharacterObject, nameof(Character.MysticAdeptPowerPoints));
            Utils.DoDatabinding(cmdIncreasePowerPoints, "Enabled", CharacterObject, nameof(Character.CanAffordCareerPP));

            // Populate vehicle weapon fire mode list.
            List<ListItem> lstFireModes = new List<ListItem>();
            foreach (Weapon.FiringMode mode in Enum.GetValues(typeof(Weapon.FiringMode)))
            {
                lstFireModes.Add(new ListItem(mode.ToString(), LanguageManager.GetString($"Enum_{mode}", GlobalOptions.Language)));
            }
            lstStreams.Sort(CompareListItems.CompareNames);
            cboVehicleWeaponFiringMode.BeginUpdate();
            cboVehicleWeaponFiringMode.ValueMember = "Value";
            cboVehicleWeaponFiringMode.DisplayMember = "Name";
            cboVehicleWeaponFiringMode.DataSource = lstFireModes;
            cboVehicleWeaponFiringMode.EndUpdate();

            _blnLoading = false;

            // Select the Magician's Tradition.
            if (!string.IsNullOrEmpty(CharacterObject.MagicTradition))
                cboTradition.SelectedValue = CharacterObject.MagicTradition;
            else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                cboTradition.SelectedIndex = 0;

            Utils.DoDatabinding(txtTraditionName, "Text", CharacterObject, nameof(Character.TraditionName));

            // Select the Technomancer's Stream.
            if (!string.IsNullOrEmpty(CharacterObject.TechnomancerStream))
                cboStream.SelectedValue = CharacterObject.TechnomancerStream;
            else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                cboStream.SelectedIndex = 0;

            treGear.ItemDrag += treGear_ItemDrag;
            treGear.DragEnter += treGear_DragEnter;
            treGear.DragDrop += treGear_DragDrop;

            /*
            treLifestyles.ItemDrag += treLifestyles_ItemDrag;
            treLifestyles.DragEnter += treLifestyles_DragEnter;
            treLifestyles.DragDrop += treLifestyles_DragDrop;
            */

            treArmor.ItemDrag += treArmor_ItemDrag;
            treArmor.DragEnter += treArmor_DragEnter;
            treArmor.DragDrop += treArmor_DragDrop;

            treWeapons.ItemDrag += treWeapons_ItemDrag;
            treWeapons.DragEnter += treWeapons_DragEnter;
            treWeapons.DragDrop += treWeapons_DragDrop;

            treVehicles.ItemDrag += treVehicles_ItemDrag;
            treVehicles.DragEnter += treVehicles_DragEnter;
            treVehicles.DragDrop += treVehicles_DragDrop;

            treImprovements.ItemDrag += treImprovements_ItemDrag;
            treImprovements.DragEnter += treImprovements_DragEnter;
            treImprovements.DragDrop += treImprovements_DragDrop;

            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(toolStrip, "toolStrip");

            tabSkillsUc.RealLoad();
            tabPowerUc.RealLoad();

            // Run through all appropriate property changers
            foreach (PropertyInfo objProperty in CharacterObject.GetType().GetProperties())
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(objProperty.Name));

            Utils.DoDatabinding(lblCMPenalty,   "Text", CharacterObject, nameof(Character.WoundModifier));
            Utils.DoDatabinding(lblCMPhysical,  "Text", CharacterObject, nameof(Character.PhysicalCM));
            Utils.DoDatabinding(lblCMStun,      "Text", CharacterObject, nameof(Character.StunCM));
            Utils.DoDatabinding(lblPhysical, "Text", CharacterObject, nameof(Character.LimitPhysical));
            Utils.DoDatabinding(lblPhysical, "ToolTipText", CharacterObject, nameof(Character.LimitPhysicalToolTip));
            Utils.DoDatabinding(lblMental, "Text", CharacterObject, nameof(Character.LimitMental));
            Utils.DoDatabinding(lblMental, "ToolTipText", CharacterObject, nameof(Character.LimitMentalToolTip));
            Utils.DoDatabinding(lblSocial, "Text", CharacterObject, nameof(Character.LimitSocial));
            Utils.DoDatabinding(lblSocial, "ToolTipText", CharacterObject, nameof(Character.LimitSocialToolTip));
            Utils.DoDatabinding(lblAstral, "Text", CharacterObject, nameof(Character.LimitAstral));
            Utils.DoDatabinding(lblAstral, "ToolTipText", CharacterObject, nameof(Character.LimitAstralToolTip));

            Utils.DoDatabinding(lblESSMax, "Text", CharacterObject, nameof(Character.DisplayEssence));
            Utils.DoDatabinding(lblCyberwareESS, "Text", CharacterObject, nameof(Character.DisplayCyberwareEssence));
            Utils.DoDatabinding(lblBiowareESS, "Text", CharacterObject, nameof(Character.DisplayBiowareEssence));
            Utils.DoDatabinding(lblEssenceHoleESS, "Text", CharacterObject, nameof(Character.DisplayEssenceHole));

            Utils.DoDatabinding(lblArmor, "Text", CharacterObject, nameof(Character.TotalArmorRating));
            Utils.DoDatabinding(lblArmor, "ToolTipText", CharacterObject, nameof(Character.TotalArmorRatingToolTip));
            Utils.DoDatabinding(lblCMArmor, "Text", CharacterObject, nameof(Character.TotalArmorRating));
            Utils.DoDatabinding(lblCMArmor, "ToolTipText", CharacterObject, nameof(Character.TotalArmorRatingToolTip));

            Utils.DoDatabinding(lblSpellDefenceIndirectDodge, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseIndirectDodge));
            Utils.DoDatabinding(lblSpellDefenceIndirectDodge, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseIndirectDodgeToolTip));
            Utils.DoDatabinding(lblSpellDefenceIndirectSoak, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseIndirectSoak));
            Utils.DoDatabinding(lblSpellDefenceIndirectSoak, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseIndirectSoakToolTip));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakMana, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakMana));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakMana, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDirectSoakManaToolTip));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakPhysical, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakPhysical));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakPhysical, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDirectSoakPhysicalToolTip));
            Utils.DoDatabinding(lblSpellDefenceDetection, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDetection));
            Utils.DoDatabinding(lblSpellDefenceDetection, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDetectionToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttBOD, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseBOD));
            Utils.DoDatabinding(lblSpellDefenceDecAttBOD, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseBODToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttAGI, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseAGI));
            Utils.DoDatabinding(lblSpellDefenceDecAttAGI, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseAGIToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttREA, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseREA));
            Utils.DoDatabinding(lblSpellDefenceDecAttREA, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseREAToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttSTR, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseSTR));
            Utils.DoDatabinding(lblSpellDefenceDecAttSTR, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseSTRToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttCHA, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseCHA));
            Utils.DoDatabinding(lblSpellDefenceDecAttCHA, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseCHAToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttINT, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseINT));
            Utils.DoDatabinding(lblSpellDefenceDecAttINT, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseINTToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttLOG, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseLOG));
            Utils.DoDatabinding(lblSpellDefenceDecAttLOG, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseLOGToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttWIL, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseWIL));
            Utils.DoDatabinding(lblSpellDefenceDecAttWIL, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseDecreaseWILToolTip));
            Utils.DoDatabinding(lblSpellDefenceIllusionMana, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseIllusionMana));
            Utils.DoDatabinding(lblSpellDefenceIllusionMana, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseIllusionManaToolTip));
            Utils.DoDatabinding(lblSpellDefenceIllusionPhysical, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseIllusionPhysical));
            Utils.DoDatabinding(lblSpellDefenceIllusionPhysical, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseIllusionPhysicalToolTip));
            Utils.DoDatabinding(lblSpellDefenceManipMental, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseManipulationMental));
            Utils.DoDatabinding(lblSpellDefenceManipMental, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseManipulationMentalToolTip));
            Utils.DoDatabinding(lblSpellDefenceManipPhysical, "Text", CharacterObject, nameof(Character.DisplaySpellDefenseManipulationPhysical));
            Utils.DoDatabinding(lblSpellDefenceManipPhysical, "ToolTipText", CharacterObject, nameof(Character.SpellDefenseManipulationPhysicalToolTip));
            Utils.DoDatabinding(nudCounterspellingDice, "Value", CharacterObject, nameof(Character.CurrentCounterspellingDice));

            Utils.DoDatabinding(lblMovement, "Text", CharacterObject, nameof(Character.DisplayMovement));
            Utils.DoDatabinding(lblSwim, "Text", CharacterObject, nameof(Character.DisplaySwim));
            Utils.DoDatabinding(lblFly, "Text", CharacterObject, nameof(Character.DisplayFly));

            Utils.DoDatabinding(lblRemainingNuyen, "Text", CharacterObject, nameof(Character.DisplayNuyen));
            Utils.DoDatabinding(lblCareerKarma, "Text", CharacterObject, nameof(Character.DisplayCareerKarma));
            Utils.DoDatabinding(lblCareerNuyen, "Text", CharacterObject, nameof(Character.DisplayCareerNuyen));

            Utils.DoDatabinding(lblStreetCredTotal, "Text", CharacterObject, nameof(Character.TotalStreetCred));
            Utils.DoDatabinding(lblStreetCredTotal, "ToolTipText", CharacterObject, nameof(Character.StreetCredTooltip));
            Utils.DoDatabinding(lblNotorietyTotal, "Text", CharacterObject, nameof(Character.TotalNotoriety));
            Utils.DoDatabinding(lblNotorietyTotal, "ToolTipText", CharacterObject, nameof(Character.NotorietyTooltip));
            Utils.DoDatabinding(lblPublicAwareTotal, "Text", CharacterObject, nameof(Character.TotalPublicAwareness));
            Utils.DoDatabinding(lblPublicAwareTotal, "ToolTipText", CharacterObject, nameof(Character.PublicAwarenessTooltip));

            Utils.DoDatabinding(lblMentorSpirit, "Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayName));
            Utils.DoDatabinding(lblMentorSpiritInformation, "Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayInformation));

            Utils.DoDatabinding(lblComposure, "ToolTipText", CharacterObject, nameof(Character.ComposureToolTip));
            Utils.DoDatabinding(lblComposure, "Text", CharacterObject, nameof(Character.Composure));
            Utils.DoDatabinding(lblJudgeIntentions, "ToolTipText", CharacterObject, nameof(Character.JudgeIntentionsToolTip));
            Utils.DoDatabinding(lblJudgeIntentions, "Text", CharacterObject, nameof(Character.JudgeIntentions));
            Utils.DoDatabinding(lblLiftCarry, "ToolTipText", CharacterObject, nameof(Character.LiftAndCarryToolTip));
            Utils.DoDatabinding(lblLiftCarry, "Text", CharacterObject, nameof(Character.LiftAndCarry));
            Utils.DoDatabinding(lblMemory, "ToolTipText", CharacterObject, nameof(Character.MemoryToolTip));
            Utils.DoDatabinding(lblMemory, "Text", CharacterObject, nameof(Character.Memory));

            Utils.DoDatabinding(cmdAddCyberware, "Enabled", CharacterObject, nameof(Character.AddCyberwareEnabled));
            Utils.DoDatabinding(cmdAddBioware, "Enabled", CharacterObject, nameof(Character.AddBiowareEnabled));
            Utils.DoDatabinding(cmdBurnStreetCred, "Enabled", CharacterObject, nameof(Character.CanBurnStreetCred));

            Utils.DoDatabinding(lblEDGInfo, "Text", CharacterObject.EDG, nameof(CharacterAttrib.CareerRemainingString));
            Utils.DoDatabinding(lblCMDamageResistancePool, "ToolTipText", CharacterObject, nameof(Character.DamageResistancePoolToolTip));
            Utils.DoDatabinding(lblCMDamageResistancePool, "Text", CharacterObject, nameof(Character.DamageResistancePool));

            RefreshAttributes(pnlAttributes);

            PrimaryAttributes.CollectionChanged += AttributeCollectionChanged;
            SpecialAttributes.CollectionChanged += AttributeCollectionChanged;

            // Condition Monitor.
            int intCMPhysical = CharacterObject.PhysicalCM;
            int intCMStun = CharacterObject.StunCM;
            int intCMOverflow = CharacterObject.CMOverflow;
            int intCMThreshold = CharacterObject.CMThreshold;
            int intStunCMThresholdOffset = CharacterObject.StunCMThresholdOffset;
            int intPhysicalCMThresholdOffset = CharacterObject.PhysicalCMThresholdOffset;

            ProcessCharacterConditionMonitorBoxDisplays(panPhysicalCM, intCMPhysical, intCMThreshold, intPhysicalCMThresholdOffset, intCMOverflow, true, CharacterObject.PhysicalCMFilled);
            ProcessCharacterConditionMonitorBoxDisplays(panStunCM, intCMStun, intCMThreshold, intStunCMThresholdOffset, 0, true, CharacterObject.StunCMFilled);
            IsCharacterUpdateRequested = true;
            // Directly calling here so that we can properly unset the dirty flag after the update
            UpdateCharacterInfo();

            // Now we can start checking for character updates
            Application.Idle += UpdateCharacterInfo;
            Application.Idle += LiveUpdateFromCharacterFile;

            // Clear the Dirty flag which gets set when creating a new Character.
            IsDirty = false;
            RefreshPasteStatus();
            frmCareer_Resize(sender, e);
            picMugshot_SizeChanged(sender, e);
            // Stupid hack to get the MDI icon to show up properly.
            Icon = Icon.Clone() as Icon;
            Timekeeper.Finish("load_frm_career");
            Timekeeper.Finish("loading");

            if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0)
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_ImprovementLoadError", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_ImprovementLoadError", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                    CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
                }
            }
        }

        private void OptionsChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CharacterOptions.ArmorDegradation):
                    cmdArmorDecrease.Visible = CharacterObjectOptions.ArmorDegradation;
                    cmdArmorIncrease.Visible = CharacterObjectOptions.ArmorDegradation;
                    break;
            }
        }

        private void PowersBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            RefreshPowerCollectionBeforeRemove(treMetamagic, e);
        }

        private void PowersListChanged(object sender, ListChangedEventArgs e)
        {
            RefreshPowerCollectionListChanged(treMetamagic, cmsMetamagic, cmsInitiationNotes, e);
        }

        private void SpellCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshSpells(treSpells, treMetamagic, cmsSpell, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void ComplexFormCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshComplexForms(treComplexForms, treMetamagic, cmsComplexForm, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void ArtCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshArtCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void EnhancementCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshEnhancementCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void MetamagicCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshMetamagicCollection(treMetamagic, cmsMetamagic, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void InitiationGradeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes, notifyCollectionChangedEventArgs);
        }

        private void AIProgramCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshAIPrograms(treAIPrograms, cmsAdvancedProgram, notifyCollectionChangedEventArgs);
        }

        private void CritterPowerCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshCritterPowers(treCritterPowers, cmsCritterPowers, notifyCollectionChangedEventArgs);
            mnuSpecialPossess.Visible = CharacterObject.CritterPowers.Any(x => x.Name == "Inhabitation" || x.Name == "Possession");
        }

        private void QualityCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshQualities(treQualities, cmsQuality, notifyCollectionChangedEventArgs);
        }

        private void MartialArtCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique, notifyCollectionChangedEventArgs);
        }

        private void LifestyleCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle, notifyCollectionChangedEventArgs);
        }

        private void LimitModifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshLimitModifiers(treLimit, cmsLimitModifier, notifyCollectionChangedEventArgs);
        }

        private void ImprovementCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshCustomImprovements(treImprovements, treLimit, cmsImprovementLocation, cmsImprovement, cmsLimitModifier, notifyCollectionChangedEventArgs);
        }

        private void ImprovementGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshCustomImprovementLocations(treImprovements, cmsImprovementLocation, notifyCollectionChangedEventArgs);
        }

        private void CalendarWeekListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            RefreshCalendar(lstCalendar, listChangedEventArgs);
        }

        private void ContactCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshContacts(panContacts, panEnemies, panPets, notifyCollectionChangedEventArgs);
        }

        private void SpiritCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshSpirits(panSpirits, panSprites, notifyCollectionChangedEventArgs);
        }

        private void AttributeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshAttributes(pnlAttributes, notifyCollectionChangedEventArgs);
        }

        private void ArmorCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear, notifyCollectionChangedEventArgs);
        }

        private void ArmorLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshArmorLocations(treArmor, cmsArmorLocation, notifyCollectionChangedEventArgs);
        }

        private void WeaponCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, notifyCollectionChangedEventArgs);
        }

        private void WeaponLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshWeaponLocations(treWeapons, cmsWeaponLocation, notifyCollectionChangedEventArgs);
        }

        private void GearCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked, notifyCollectionChangedEventArgs);
            RefreshFociFromGear(treFoci, null, notifyCollectionChangedEventArgs);
        }

        private void GearLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshGearLocations(treGear, cmsGearLocation, notifyCollectionChangedEventArgs);
        }

        private void CyberwareCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear, notifyCollectionChangedEventArgs);
        }

        private void VehicleCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsWeaponMount, cmsVehicleCyberware, cmsVehicleCyberwareGear, notifyCollectionChangedEventArgs);
        }

        private void VehicleLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshVehicleLocations(treVehicles, cmsVehicleLocation, notifyCollectionChangedEventArgs);
        }

        private void frmCareer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If there are unsaved changes to the character, as the user if they would like to save their changes.
            if (IsDirty)
            {
                string strCharacterName = CharacterObject.CharacterName;
                DialogResult objResult = MessageBox.Show(LanguageManager.GetString("Message_UnsavedChanges", GlobalOptions.Language).Replace("{0}", strCharacterName), LanguageManager.GetString("MessageTitle_UnsavedChanges", GlobalOptions.Language), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (objResult == DialogResult.Yes)
                {
                    // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                    bool blnResult = SaveCharacter();
                    if (!blnResult)
                        e.Cancel = true;
                }
                else if (objResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            // Reset the ToolStrip so the Save button is removed for the currently closing window.
            if (!e.Cancel)
            {
                _blnLoading = true;
                Cursor = Cursors.WaitCursor;
                Application.Idle -= UpdateCharacterInfo;
                Application.Idle -= LiveUpdateFromCharacterFile;
                Program.MainForm.OpenCharacterForms.Remove(this);
                if (!_blnSkipToolStripRevert)
                    ToolStripManager.RevertMerge("toolStrip");

                // Unsubscribe from events.
                PrimaryAttributes.CollectionChanged -= AttributeCollectionChanged;
                SpecialAttributes.CollectionChanged -= AttributeCollectionChanged;
                CharacterObject.Spells.CollectionChanged -= SpellCollectionChanged;
                CharacterObject.ComplexForms.CollectionChanged -= ComplexFormCollectionChanged;
                CharacterObject.Arts.CollectionChanged -= ArtCollectionChanged;
                CharacterObject.Enhancements.CollectionChanged -= EnhancementCollectionChanged;
                CharacterObject.Metamagics.CollectionChanged -= MetamagicCollectionChanged;
                CharacterObject.InitiationGrades.CollectionChanged -= InitiationGradeCollectionChanged;
                CharacterObject.Powers.ListChanged -= PowersListChanged;
                CharacterObject.Powers.BeforeRemove -= PowersBeforeRemove;
                CharacterObject.AIPrograms.CollectionChanged -= AIProgramCollectionChanged;
                CharacterObject.CritterPowers.CollectionChanged -= CritterPowerCollectionChanged;
                CharacterObject.Qualities.CollectionChanged -= QualityCollectionChanged;
                CharacterObject.MartialArts.CollectionChanged -= MartialArtCollectionChanged;
                CharacterObject.Lifestyles.CollectionChanged -= LifestyleCollectionChanged;
                CharacterObject.LimitModifiers.CollectionChanged -= LimitModifierCollectionChanged;
                CharacterObject.Contacts.CollectionChanged -= ContactCollectionChanged;
                CharacterObject.Armor.CollectionChanged -= ArmorCollectionChanged;
                CharacterObject.ArmorLocations.CollectionChanged -= ArmorLocationCollectionChanged;
                CharacterObject.Weapons.CollectionChanged -= WeaponCollectionChanged;
                CharacterObject.WeaponLocations.CollectionChanged -= WeaponLocationCollectionChanged;
                CharacterObject.Gear.CollectionChanged -= GearCollectionChanged;
                CharacterObject.GearLocations.CollectionChanged -= GearLocationCollectionChanged;
                CharacterObject.Cyberware.CollectionChanged -= CyberwareCollectionChanged;
                CharacterObject.Vehicles.CollectionChanged -= VehicleCollectionChanged;
                CharacterObject.VehicleLocations.CollectionChanged -= VehicleLocationCollectionChanged;
                CharacterObject.Spirits.CollectionChanged -= SpiritCollectionChanged;
                CharacterObject.Improvements.CollectionChanged -= ImprovementCollectionChanged;
                CharacterObject.ImprovementGroups.CollectionChanged -= ImprovementGroupCollectionChanged;
                CharacterObject.Calendar.ListChanged -= CalendarWeekListChanged;
                CharacterObject.PropertyChanged -= OnCharacterPropertyChanged;

                treGear.ItemDrag -= treGear_ItemDrag;
                treGear.DragEnter -= treGear_DragEnter;
                treGear.DragDrop -= treGear_DragDrop;

                /*
                treLifestyles.ItemDrag -= treLifestyles_ItemDrag;
                treLifestyles.DragEnter -= treLifestyles_DragEnter;
                treLifestyles.DragDrop -= treLifestyles_DragDrop;
                */

                treArmor.ItemDrag -= treArmor_ItemDrag;
                treArmor.DragEnter -= treArmor_DragEnter;
                treArmor.DragDrop -= treArmor_DragDrop;

                treWeapons.ItemDrag -= treWeapons_ItemDrag;
                treWeapons.DragEnter -= treWeapons_DragEnter;
                treWeapons.DragDrop -= treWeapons_DragDrop;

                treVehicles.ItemDrag -= treVehicles_ItemDrag;
                treVehicles.DragEnter -= treVehicles_DragEnter;
                treVehicles.DragDrop -= treVehicles_DragDrop;

                treImprovements.ItemDrag -= treImprovements_ItemDrag;
                treImprovements.DragEnter -= treImprovements_DragEnter;
                treImprovements.DragDrop -= treImprovements_DragDrop;

                foreach (ContactControl objContactControl in panContacts.Controls.OfType<ContactControl>())
                {
                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objContactControl.DeleteContact -= DeleteContact;
                    objContactControl.MouseDown -= DragContactControl;
                }

                foreach (ContactControl objContactControl in panEnemies.Controls.OfType<ContactControl>())
                {
                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objContactControl.DeleteContact -= DeleteEnemy;
                }

                foreach (PetControl objContactControl in panPets.Controls.OfType<PetControl>())
                {
                    objContactControl.DeleteContact -= DeletePet;
                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                }

                foreach (SpiritControl objSpiritControl in panSpirits.Controls.OfType<SpiritControl>())
                {
                    objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objSpiritControl.DeleteSpirit -= DeleteSpirit;
                }

                foreach (SpiritControl objSpiritControl in panSprites.Controls.OfType<SpiritControl>())
                {
                    objSpiritControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objSpiritControl.DeleteSpirit -= DeleteSpirit;
                }

                // Trash the global variables and dispose of the Form.
                if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(CharacterObject) && x != CharacterObject))
                {
                    Program.MainForm.OpenCharacters.Remove(CharacterObject);
                    CharacterObject.DeleteCharacter();
                }
                Dispose(true);
            }
        }

        private void frmCareer_Activated(object sender, EventArgs e)
        {
            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(toolStrip, "toolStrip");
        }

        private void frmCareer_Shown(object sender, EventArgs e)
        {
            frmCareer_Resize(sender, e);
        }

        private void frmCareer_Resize(object sender, EventArgs e)
        {
            TabPage objPage = tabCharacterTabs.SelectedTab;
            // Reseize the form elements with the form.
            // Character Info Tab.
            int intHeight = ((objPage.Height - lblDescription.Top) / 4 - 20);
            txtDescription.Height = intHeight;
            lblBackground.Top = txtDescription.Top + txtDescription.Height + 3;
            txtBackground.Top = lblBackground.Top + lblBackground.Height + 3;
            txtBackground.Height = intHeight;
            lblConcept.Top = txtBackground.Top + txtBackground.Height + 3;
            txtConcept.Top = lblConcept.Top + lblConcept.Height + 3;
            txtConcept.Height = intHeight;
            lblNotes.Top = txtConcept.Top + txtConcept.Height + 3;
            txtNotes.Top = lblNotes.Top + lblNotes.Height + 3;
            txtNotes.Height = intHeight;
        }
        #endregion

        #region Character Events
        private void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_blnReapplyImprovements)
                return;
            
            IsDirty = true;

            switch (e.PropertyName)
            {
                case nameof(Character.CharacterName):
                    UpdateWindowTitle(false);
                    break;
                case nameof(Character.DisplayNuyen):
                    tssNuyen.Text = CharacterObject.DisplayNuyen;
                    break;
                case nameof(Character.DisplayKarma):
                    tssKarma.Text = CharacterObject.DisplayKarma;
                    break;
                case nameof(Character.DisplayEssence):
                    tssEssence.Text = CharacterObject.DisplayEssence;
                    break;
                case nameof(Character.NuyenBP):
                case nameof(Character.MetatypeBP):
                case nameof(Character.BuildKarma):
                case nameof(Character.ContactPoints):
                case nameof(Character.SpellLimit):
                case nameof(Character.CFPLimit):
                case nameof(Character.AIAdvancedProgramLimit):
                case nameof(Character.SpellKarmaCost):
                case nameof(Character.ComplexFormKarmaCost):
                case nameof(Character.AIProgramKarmaCost):
                case nameof(Character.AIAdvancedProgramKarmaCost):
                case nameof(Character.MysticAdeptPowerPoints):
                case nameof(Character.TraditionDrain):
                case nameof(Character.SpiritCombat):
                case nameof(Character.SpiritDetection):
                case nameof(Character.SpiritHealth):
                case nameof(Character.SpiritIllusion):
                case nameof(Character.SpiritManipulation):
                    IsCharacterUpdateRequested = true;
                    break;
                case nameof(Character.MAGEnabled):
                    {
                        if (CharacterObject.MAGEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);

                            tabInitiation.Text = LanguageManager.GetString("Tab_Initiation", GlobalOptions.Language);
                            tsMetamagicAddMetamagic.Text = LanguageManager.GetString("Button_AddMetamagic", GlobalOptions.Language);
                            cmdAddMetamagic.Text = LanguageManager.GetString("Button_AddInitiateGrade", GlobalOptions.Language);
                            chkInitiationOrdeal.Text = LanguageManager.GetString("Checkbox_InitiationOrdeal", GlobalOptions.Language);
                            tsMetamagicAddArt.Visible = true;
                            tsMetamagicAddEnchantment.Visible = true;
                            tsMetamagicAddEnhancement.Visible = true;
                            tsMetamagicAddRitual.Visible = true;
                            string strInitTip = LanguageManager.GetString("Tip_ImproveInitiateGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.InitiateGrade + 1).ToString()).Replace("{1}", (CharacterObjectOptions.KarmaInititationFlat + ((CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation)).ToString());
                            GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
                            chkJoinGroup.Visible = true;
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedGroup", GlobalOptions.Language);

                            treMetamagic.Top = chkInitiationSchooling.Top + chkInitiationSchooling.Height + 6;
                            cmdAddMetamagic.Left = treMetamagic.Left + treMetamagic.Width - cmdAddMetamagic.Width;
                            cmdDeleteMetamagic.Left = cmdAddMetamagic.Left;
                            lblMetamagicSourceLabel.Left = treMetamagic.Left + treMetamagic.Left + treMetamagic.Width + 6;
                            lblMetamagicSource.Left = lblMetamagicSourceLabel.Left + lblMetamagicSourceLabel.Width + 6;

                            if (!SpecialAttributes.Contains(CharacterObject.MAG))
                            {
                                SpecialAttributes.Add(CharacterObject.MAG);
                            }
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && !SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Add(CharacterObject.MAGAdept);
                            }
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabInitiation);

                            if (SpecialAttributes.Contains(CharacterObject.MAG))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAG);
                            }
                            if (SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
                            }
                        }
                        chkInitiationGroup.Visible = CharacterObject.MAGEnabled;
                        chkInitiationOrdeal.Visible = CharacterObject.MAGEnabled;
                        chkInitiationSchooling.Visible = CharacterObject.MAGEnabled;
                        lblFoci.Visible = CharacterObject.MAGEnabled;
                        treFoci.Visible = CharacterObject.MAGEnabled;
                        cmdCreateStackedFocus.Visible = CharacterObject.MAGEnabled;
                        lblAstralINI.Visible = CharacterObject.MAGEnabled;
                        lblSpirits.Visible = CharacterObject.MAGEnabled;
                        panSpirits.Visible = CharacterObject.MAGEnabled;
                        cmdAddSpirit.Visible = CharacterObject.MAGEnabled;
                    }
                    break;
                case nameof(Character.RESEnabled):
                    {
                        if (CharacterObject.RESEnabled)
                        {
                            tabInitiation.Text = LanguageManager.GetString("Tab_Submersion", GlobalOptions.Language);
                            tsMetamagicAddMetamagic.Text = LanguageManager.GetString("Button_AddEcho", GlobalOptions.Language);
                            cmdAddMetamagic.Text = LanguageManager.GetString("Button_AddSubmersionGrade", GlobalOptions.Language);
                            chkInitiationOrdeal.Text = LanguageManager.GetString("Checkbox_SubmersionTask", GlobalOptions.Language);
                            tsMetamagicAddArt.Visible = false;
                            tsMetamagicAddEnchantment.Visible = false;
                            tsMetamagicAddEnhancement.Visible = false;
                            tsMetamagicAddRitual.Visible = false;
                            string strInitTip = LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.SubmersionGrade + 1).ToString()).Replace("{1}", (CharacterObjectOptions.KarmaInititationFlat + ((CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation)).ToString());
                            GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
                            chkJoinGroup.Visible = false;
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedNetwork", GlobalOptions.Language);

                            treMetamagic.Top = chkInitiationSchooling.Top + chkInitiationSchooling.Height + 6;
                            cmdAddMetamagic.Left = treMetamagic.Left + treMetamagic.Width - cmdAddMetamagic.Width;
                            cmdDeleteMetamagic.Left = cmdAddMetamagic.Left;
                            lblMetamagicSourceLabel.Left = treMetamagic.Left + treMetamagic.Left + treMetamagic.Width + 6;
                            lblMetamagicSource.Left = lblMetamagicSourceLabel.Left + lblMetamagicSourceLabel.Width + 6;

                            if (!SpecialAttributes.Contains(CharacterObject.RES))
                            {
                                SpecialAttributes.Add(CharacterObject.RES);
                            }
                        }
                        else
                        {
                            if (SpecialAttributes.Contains(CharacterObject.RES))
                            {
                                SpecialAttributes.Remove(CharacterObject.RES);
                            }
                        }
                    }
                    break;
                case nameof(Character.DEPEnabled):
                    {
                        if (CharacterObject.DEPEnabled)
                        {
                            if (!SpecialAttributes.Contains(CharacterObject.DEP))
                            {
                                SpecialAttributes.Add(CharacterObject.DEP);
                            }
                        }
                        else
                        {
                            if (SpecialAttributes.Contains(CharacterObject.DEP))
                            {
                                SpecialAttributes.Remove(CharacterObject.DEP);
                            }
                        }
                    }
                    break;
                case nameof(Character.Ambidextrous):
                    {
                        cboPrimaryArm.BeginUpdate();

                        List<ListItem> lstPrimaryArm;
                        if (CharacterObject.Ambidextrous)
                        {
                            lstPrimaryArm = new List<ListItem>
                            {
                                new ListItem("Ambidextrous", LanguageManager.GetString("String_Ambidextrous", GlobalOptions.Language))
                            };
                            cboPrimaryArm.Enabled = false;
                        }
                        else
                        {
                            //Create the dropdown for the character's primary arm.
                            lstPrimaryArm = new List<ListItem>
                            {
                                new ListItem("Left", LanguageManager.GetString("String_Improvement_SideLeft", GlobalOptions.Language)),
                                new ListItem("Right", LanguageManager.GetString("String_Improvement_SideRight", GlobalOptions.Language))
                            };
                            lstPrimaryArm.Sort(CompareListItems.CompareNames);
                            cboPrimaryArm.Enabled = true;
                        }

                        string strPrimaryArm = CharacterObject.PrimaryArm;

                        cboPrimaryArm.ValueMember = "Value";
                        cboPrimaryArm.DisplayMember = "Name";
                        cboPrimaryArm.DataSource = lstPrimaryArm;
                        cboPrimaryArm.SelectedValue = strPrimaryArm;
                        if (cboPrimaryArm.SelectedIndex == -1)
                            cboPrimaryArm.SelectedIndex = 0;

                        cboPrimaryArm.EndUpdate();
                    }
                    break;
                case nameof(Character.MagicianEnabled):
                    {
                        // Change to the status of Magician being enabled.
                        if (CharacterObject.MagicianEnabled || CharacterObject.AdeptEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabMagician))
                                tabCharacterTabs.TabPages.Insert(3, tabMagician);

                            cmdAddSpell.Enabled = true;
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && !SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Add(CharacterObject.MAGAdept);
                            }
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabMagician);
                            cmdAddSpell.Enabled = false;
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
                            }
                        }
                        lblSpirits.Visible = CharacterObject.MagicianEnabled;
                        cmdAddSpirit.Visible = CharacterObject.MagicianEnabled;
                        panSpirits.Visible = CharacterObject.MagicianEnabled;
                    }
                    break;
                case nameof(Character.AdeptEnabled):
                    {
                        // Change to the status of Adept being enabled.
                        if (CharacterObject.AdeptEnabled)
                        {
                            cmdAddSpell.Enabled = true;
                            if (!tabCharacterTabs.TabPages.Contains(tabAdept))
                                tabCharacterTabs.TabPages.Insert(3, tabAdept);
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && !SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Add(CharacterObject.MAGAdept);
                            }
                        }
                        else
                        {
                            cmdAddSpell.Enabled = CharacterObject.MagicianEnabled;
                            tabCharacterTabs.TabPages.Remove(tabAdept);

                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
                            }
                        }
                    }
                    break;
                case nameof(Character.TechnomancerEnabled):
                    {
                        // Change to the status of Technomancer being enabled.
                        if (CharacterObject.TechnomancerEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabTechnomancer))
                                tabCharacterTabs.TabPages.Insert(3, tabTechnomancer);
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabTechnomancer);
                        }
                    }
                    break;
                case nameof(Character.AdvancedProgramsEnabled):
                    {
                        // Change to the status of Advanced Programs being enabled.
                        if (CharacterObject.AdvancedProgramsEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabAdvancedPrograms))
                                tabCharacterTabs.TabPages.Insert(3, tabAdvancedPrograms);
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabAdvancedPrograms);
                        }
                    }
                    break;
                case nameof(Character.CritterEnabled):
                    {
                        // Change the status of Critter being enabled.
                        if (CharacterObject.CritterEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabCritter))
                                tabCharacterTabs.TabPages.Insert(3, tabCritter);
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabCritter);
                        }
                    }
                    break;
                case nameof(Character.ExCon):
                    {
                        if (CharacterObject.ExCon)
                        {
                            bool blnDoRefresh = false;
                            bool funcExConIneligibleWare(Cyberware x)
                            {
                                if (x.Grade.Name == "None")
                                    return false;
                                Cyberware objParent = x;
                                bool blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                                while (objParent.Parent != null && blnNoParentIsModular)
                                {
                                    objParent = objParent.Parent;
                                    blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                                }

                                return blnNoParentIsModular;
                            }
                            string strExConString = CharacterObject.Qualities.FirstOrDefault(x => x.Name == "Ex-Con")?.DisplayNameShort(GlobalOptions.Language);
                            if (string.IsNullOrEmpty(strExConString))
                            {
                                XmlNode xmlErasedQuality = XmlManager.Load("qualities.xml").SelectSingleNode("chummer/qualities/quality[name = \"Ex-Con\"]");
                                if (xmlErasedQuality != null)
                                {
                                    strExConString = xmlErasedQuality["translate"]?.InnerText ?? xmlErasedQuality["name"]?.InnerText ?? string.Empty;
                                }
                            }
                            if (!string.IsNullOrEmpty(strExConString))
                                strExConString = LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + strExConString + ')' + LanguageManager.GetString("String_Space", GlobalOptions.Language);
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children, funcExConIneligibleWare))
                            {
                                char chrAvail = objCyberware.TotalAvailTuple(false).Suffix;
                                if (chrAvail == 'R' || chrAvail == 'F')
                                {
                                    objCyberware.DeleteCyberware();

                                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                    string strEntry = LanguageManager.GetString(objCyberware.SourceType == Improvement.ImprovementSource.Cyberware ? "String_ExpenseSoldCyberware" : "String_ExpenseSoldBioware", GlobalOptions.Language);
                                    objExpense.Create(0, strEntry + strExConString + objCyberware.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                                    Cyberware objParent = objCyberware.Parent;
                                    if (objParent != null)
                                        objParent.Children.Remove(objCyberware);
                                    else
                                        CharacterObject.Cyberware.Remove(objCyberware);

                                    IncreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100));

                                    blnDoRefresh = true;
                                }
                            }

                            if (blnDoRefresh)
                            {
                                IsCharacterUpdateRequested = true;

                                IsDirty = true;
                            }
                        }
                    }
                    break;
                case nameof(Character.InitiationEnabled):
                    {
                        // Change the status of the Initiation tab being show.
                        if (CharacterObject.InitiationEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabInitiation);
                        }
                    }
                    break;
                case nameof(Character.QuickeningEnabled):
                    {
                        cmdQuickenSpell.Visible = CharacterObject.QuickeningEnabled;
                        break;
                    }
                case nameof(Character.HasMentorSpirit):
                    {
                        lblMentorSpirit.Visible = CharacterObject.HasMentorSpirit;
                        lblMentorSpiritLabel.Visible = CharacterObject.HasMentorSpirit;
                        lblMentorSpiritInformation.Visible = CharacterObject.HasMentorSpirit;
                        break;
                    }
                case nameof(Character.UseMysticAdeptPPs):
                    {
                        lblMysticAdeptAssignment.Visible = CharacterObject.UseMysticAdeptPPs;
                        lblMysticAdeptMAGAdept.Visible = CharacterObject.UseMysticAdeptPPs;
                        break;
                    }
                case nameof(Character.MysAdeptAllowPPCareer):
                    {
                        cmdIncreasePowerPoints.Visible = CharacterObject.MysAdeptAllowPPCareer;
                        break;
                    }
            }
        }
        /*
        //TODO: UpdatePowerRelatedInfo method? Powers hook into so much stuff that it may need to wait for outbound improvement events?
        private readonly Stopwatch PowerPropertyChanged_StopWatch = Stopwatch.StartNew();
        private void PowerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;

            if (PowerPropertyChanged_StopWatch.ElapsedMilliseconds < 4) return;
            PowerPropertyChanged_StopWatch.Restart();
            tabPowerUc.CalculatePowerPoints();
            IsCharacterUpdateRequested = true;
        }

        private readonly Stopwatch SkillPropertyChanged_StopWatch = Stopwatch.StartNew();
        private void SkillPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //HACK PERFORMANCE
            //So, skills tell if anything maybe intresting have happened, but this don't have any way to see if it is relevant. Instead of redrawing EVYER FYCKING THING we do it only every 5 ms
            if (SkillPropertyChanged_StopWatch.ElapsedMilliseconds < 4) return;
            SkillPropertyChanged_StopWatch.Restart();
            
            IsCharacterUpdateRequested = true;
            
            IsDirty = true;
        }
        */
        #endregion

        #region Menu Events
        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            SaveCharacter();
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveCharacterAs();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            mnuFileSave_Click(sender, e);
        }

        private void tsbPrint_Click(object sender, EventArgs e)
        {
            DoPrint();
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuFilePrint_Click(object sender, EventArgs e)
        {
            DoPrint();
        }

        private void mnuFileExport_Click(object sender, EventArgs e)
        {
            // Write the Character information to a MemoryStream so we don't need to create any files.
            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

            // Being the document.
            objWriter.WriteStartDocument();

            // </characters>
            objWriter.WriteStartElement("characters");

#if DEBUG
            CharacterObject.PrintToStream(objStream, objWriter, GlobalOptions.CultureInfo, GlobalOptions.Language);
#else
            CharacterObject.PrintToStream(objWriter, GlobalOptions.CultureInfo, GlobalOptions.Language);
#endif

            // </characters>
            objWriter.WriteEndElement();

            // Finish the document and flush the Writer and Stream.
            objWriter.WriteEndDocument();
            objWriter.Flush();

            // Read the stream.
            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
            objStream.Position = 0;
            XmlDocument objCharacterXML = new XmlDocument();

            // Put the stream into an XmlDocument and send it off to the Viewer.
            string strXML = objReader.ReadToEnd();
            objCharacterXML.LoadXml(strXML);

            objWriter.Close();

            frmExport frmExportCharacter = new frmExport(objCharacterXML);
            frmExportCharacter.ShowDialog(this);
        }

        private void mnuSpecialCyberzombie_Click(object sender, EventArgs e)
        {
            bool blnEssence = true;
            bool blnCyberware = false;
            string strMessage = LanguageManager.GetString("Message_CyberzombieRequirements", GlobalOptions.Language);

            // Make sure the character has an Essence lower than 0.
            if (CharacterObject.Essence() >= 0)
            {
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_CyberzombieRequirementsEssence", GlobalOptions.Language);
                blnEssence = false;
            }

            // Make sure the character has an Invoked Memory Stimulator.
            foreach (Cyberware objCyberware in CharacterObject.Cyberware)
            {
                if (objCyberware.Name == "Invoked Memory Stimulator")
                {
                    blnCyberware = true;
                    break;
                }
            }

            if (!blnCyberware)
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_CyberzombieRequirementsStimulator", GlobalOptions.Language);

            if (!blnEssence || !blnCyberware)
            {
                MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_CyberzombieRequirements", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show(LanguageManager.GetString("Message_CyberzombieConfirm", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberzombieConfirm", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            // Convert the character.
            // Characters lose access to Resonance.
            CharacterObject.RESEnabled = false;

            // Gain MAG that is permanently set to 1.
            CharacterObject.MAGEnabled = true;
            CharacterObject.MAG.MetatypeMinimum = 1;
            CharacterObject.MAG.MetatypeMaximum = 1;

            // Add the Cyberzombie Lifestyle if it is not already taken.
            if (CharacterObject.Lifestyles.All(x => x.BaseLifestyle != "Cyberzombie Lifestyle Addition"))
            {
                XmlDocument objXmlLifestyleDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objXmlLifestyle = objXmlLifestyleDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Cyberzombie Lifestyle Addition\"]");

                if (objXmlLifestyle != null)
                {
                    Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                    objLifestyle.Create(objXmlLifestyle);
                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }

            // Change the MetatypeCategory to Cyberzombie.
            CharacterObject.MetatypeCategory = "Cyberzombie";

            // Gain access to Critter Powers.
            CharacterObject.CritterEnabled = true;

            // Gain the Dual Natured Critter Power if it does not yet exist.
            if (CharacterObject.CritterPowers.All(x => x.Name != "Dual Natured"))
            {
                XmlNode objXmlPowerNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers/power[name = \"Dual Natured\"]");

                if (objXmlPowerNode != null)
                {
                    CritterPower objCritterPower = new CritterPower(CharacterObject);
                    objCritterPower.Create(objXmlPowerNode);
                    CharacterObject.CritterPowers.Add(objCritterPower);
                }
            }

            // Gain the Immunity (Normal Weapons) Critter Power if it does not yet exist.
            if (!CharacterObject.CritterPowers.Any(x => x.Name == "Immunity" && x.Extra == "Normal Weapons"))
            {
                XmlNode objXmlPowerNode = XmlManager.Load("critterpowers.xml").SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                if (objXmlPowerNode != null)
                {
                    CritterPower objCritterPower = new CritterPower(CharacterObject);
                    objCritterPower.Create(objXmlPowerNode, 0, "Normal Weapons");
                    CharacterObject.CritterPowers.Add(objCritterPower);
                }
            }

            RefreshMetatypeFields();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void mnuSpecialReduceAttribute_Click(object sender, EventArgs e)
        {
            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings);

            lstAbbrevs.Remove("ESS");
            if (!CharacterObject.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!CharacterObject.IsMysticAdept || !CharacterObjectOptions.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!CharacterObject.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!CharacterObject.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            // Display the Select CharacterAttribute window and record which Skill was selected.
            frmSelectAttribute frmPickAttribute = new frmSelectAttribute(lstAbbrevs.ToArray())
            {
                Description = LanguageManager.GetString("String_CyberzombieReduceAttribute", GlobalOptions.Language),
                ShowMetatypeMaximum = true
            };
            frmPickAttribute.ShowDialog(this);

            if (frmPickAttribute.DialogResult == DialogResult.Cancel)
                return;

            // Create an Improvement to reduce the CharacterAttribute's Metatype Maximum.
            if (!frmPickAttribute.DoNotAffectMetatypeMaximum)
            {
                ImprovementManager.CreateImprovement(CharacterObject, frmPickAttribute.SelectedAttribute, Improvement.ImprovementSource.AttributeLoss, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, -1);
                ImprovementManager.Commit(CharacterObject);
            }
            // Permanently reduce the CharacterAttribute's value.
            CharacterObject.GetAttribute(frmPickAttribute.SelectedAttribute).Degrade(1);

            IsDirty = true;

            IsCharacterUpdateRequested = true;
        }

        private void mnuSpecialCloningMachine_Click(object sender, EventArgs e)
        {
            frmSelectNumber frmPickNumber = new frmSelectNumber(0)
            {
                Description = LanguageManager.GetString("String_CloningMachineNumber", GlobalOptions.Language),
                Minimum = 1
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            int intClones = decimal.ToInt32(frmPickNumber.SelectedValue);
            if (intClones <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CloningMachineNumberRequired", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CloningMachineNumberRequired", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Cursor = Cursors.WaitCursor;
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            Character[] lstClones = new Character[intClones];
            object lstClonesLock = new object();
            Parallel.For(0, intClones, (i =>
            {
                Character objLoopCharacter = Program.MainForm.LoadCharacter(CharacterObject.FileName, CharacterObject.Alias + strSpaceCharacter + i.ToString(), true);
                lock (lstClonesLock)
                {
                    lstClones[i] = objLoopCharacter;
                }
            }));
            Cursor = Cursors.Default;
            Program.MainForm.OpenCharacterList(lstClones, false);
        }

        private void mnuSpecialReapplyImprovements_Click(object sender, EventArgs e)
        {
            // This only re-applies the Improvements for everything the character has. If a match is not found in the data files, the current Improvement information is left as-is.
            // Verify that the user wants to go through with it.
            if (MessageBox.Show(LanguageManager.GetString("Message_ConfirmReapplyImprovements", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ConfirmReapplyImprovements", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            DoReapplyImprovements();
        }

        private void DoReapplyImprovements(ICollection<string> lstInternalIdFilter = null)
        {
            Cursor = Cursors.WaitCursor;

            StringBuilder strOutdatedItems = new StringBuilder();

            // Record the status of any flags that normally trigger character events.
            bool blnMAGEnabled = CharacterObject.MAGEnabled;
            bool blnRESEnabled = CharacterObject.RESEnabled;
            bool blnDEPEnabled = CharacterObject.DEPEnabled;
            decimal decEssenceAtSpecialStart = CharacterObject.EssenceAtSpecialStart;

            _blnReapplyImprovements = true;

            // Wipe all improvements that we will reapply, this is mainly to eliminate orphaned improvements caused by certain bugs and also for a performance increase
            if (lstInternalIdFilter == null)
                ImprovementManager.RemoveImprovements(CharacterObject, CharacterObject.Improvements.Where(x =>
                                                                        x.ImproveSource == Improvement.ImprovementSource.AIProgram ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Armor ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.ArmorMod ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Bioware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.ComplexForm ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.CritterPower ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Cyberware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Echo ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Gear ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.MartialArt ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Metamagic ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Power ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Quality ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Spell ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.StackedFocus).ToList(), _blnReapplyImprovements);
            else
                ImprovementManager.RemoveImprovements(CharacterObject, CharacterObject.Improvements.Where(x => lstInternalIdFilter.Contains(x.SourceName) &&
                                                                        (x.ImproveSource == Improvement.ImprovementSource.AIProgram ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Armor ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.ArmorMod ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Bioware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.ComplexForm ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.CritterPower ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Cyberware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Echo ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Gear ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.MartialArt ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Metamagic ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Power ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Quality ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.Spell ||
                                                                        x.ImproveSource == Improvement.ImprovementSource.StackedFocus)).ToList(), _blnReapplyImprovements);

            // Refresh Qualities.
            // We cannot use foreach because qualities can add more qualities
            for (int j = 0; j < CharacterObject.Qualities.Count; j++)
            {
                Quality objQuality = CharacterObject.Qualities[j];
                if (objQuality.OriginSource == QualitySource.Improvement)
                    continue;
                // We're only re-apply improvements a list of items, not all of them
                if (lstInternalIdFilter != null && !lstInternalIdFilter.Contains(objQuality.InternalId))
                    continue;
                string strSelected = objQuality.Extra;

                XmlNode objNode = objQuality.GetNode();
                if (objNode != null)
                {
                    objQuality.Bonus = objNode["bonus"];
                    if (objQuality.Bonus != null)
                    {
                        ImprovementManager.ForcedValue = strSelected;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.Bonus, false, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objQuality.Extra = ImprovementManager.SelectedValue;
                            TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                            if (objTreeNode != null)
                                objTreeNode.Text = objQuality.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        }
                    }
                    objQuality.FirstLevelBonus = objNode["firstlevelbonus"];
                    if (objQuality.FirstLevelBonus?.HasChildNodes == true)
                    {
                        bool blnDoFirstLevel = true;
                        for (int k = 0; k < CharacterObject.Qualities.Count; ++k)
                        {
                            Quality objCheckQuality = CharacterObject.Qualities[k];
                            if (j != k && objCheckQuality.QualityId == objQuality.QualityId && objCheckQuality.Extra == objQuality.Extra && objCheckQuality.SourceName == objQuality.SourceName)
                            {
                                if (k < j || objCheckQuality.OriginSource == QualitySource.Improvement || (lstInternalIdFilter != null && !lstInternalIdFilter.Contains(objCheckQuality.InternalId)))
                                {
                                    blnDoFirstLevel = false;
                                    break;
                                }
                            }
                        }
                        if (blnDoFirstLevel)
                        {
                            ImprovementManager.ForcedValue = strSelected;
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.FirstLevelBonus, false, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                objQuality.Extra = ImprovementManager.SelectedValue;
                                TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                                if (objTreeNode != null)
                                    objTreeNode.Text = objQuality.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objQuality.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language));
                }
            }

            // Refresh Martial Art Advantages.
            foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
            {
                XmlNode objMartialArtNode = objMartialArt.GetNode();
                if (objMartialArtNode != null)
                {
                    // We're only re-apply improvements a list of items, not all of them
                    if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(objMartialArt.InternalId))
                    {
                        if (objMartialArtNode["bonus"] != null)
                        {
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId, objMartialArtNode["bonus"], false, 1, objMartialArt.DisplayNameShort(GlobalOptions.Language));
                        }
                    }
                    foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
                    {
                        XmlNode objNode = objTechnique.GetNode();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.MartialArtTechnique, objTechnique.InternalId, objNode["bonus"], false, 1, objTechnique.DisplayName(GlobalOptions.Language));
                        }
                        else
                        {
                            strOutdatedItems.AppendLine(objMartialArt.DisplayName(GlobalOptions.Language));
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objMartialArt.DisplayName(GlobalOptions.Language));
                }
            }

            // Refresh Spells.
            foreach (Spell objSpell in CharacterObject.Spells.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
            {
                XmlNode objNode = objSpell.GetNode();
                if (objNode != null)
                {
                    if (objNode["bonus"] != null)
                    {
                        ImprovementManager.ForcedValue = objSpell.Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Spell, objSpell.InternalId, objNode["bonus"], false, 1, objSpell.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objSpell.Extra = ImprovementManager.SelectedValue;
                            TreeNode objSpellNode = treSpells.FindNode(objSpell.InternalId);
                            if (objSpellNode != null)
                                objSpellNode.Text = objSpell.DisplayName(GlobalOptions.Language);
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objSpell.DisplayName(GlobalOptions.Language));
                }
            }

            // Refresh Adept Powers.
            foreach (Power objPower in CharacterObject.Powers.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
            {
                XmlNode objNode = objPower.GetNode();
                if (objNode != null)
                {
                    objPower.Bonus = objNode["bonus"];
                    if (objPower.Bonus != null)
                    {
                        ImprovementManager.ForcedValue = objPower.Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false, Convert.ToInt32(objPower.TotalRating), objPower.DisplayNameShort(GlobalOptions.Language));
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objPower.DisplayName);
                }
            }

            // Refresh Complex Forms.
            foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
            {
                XmlNode objNode = objComplexForm.GetNode();
                if (objNode != null)
                {
                    if (objNode["bonus"] != null)
                    {
                        ImprovementManager.ForcedValue = objComplexForm.Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId, objNode["bonus"], false, 1, objComplexForm.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objComplexForm.Extra = ImprovementManager.SelectedValue;
                            TreeNode objCFNode = treComplexForms.FindNode(objComplexForm.InternalId);
                            if (objCFNode != null)
                                objCFNode.Text = objComplexForm.DisplayName;
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objComplexForm.DisplayName);
                }
            }

            // Refresh AI Programs and Advanced Programs
            foreach (AIProgram objProgram in CharacterObject.AIPrograms.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
            {
                XmlNode objNode = objProgram.GetNode();
                if (objNode != null)
                {
                    if (objNode["bonus"] != null)
                    {
                        ImprovementManager.ForcedValue = objProgram.Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.AIProgram, objProgram.InternalId, objNode["bonus"], false, 1, objProgram.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objProgram.Extra = ImprovementManager.SelectedValue;
                            TreeNode objProgramNode = treAIPrograms.FindNode(objProgram.InternalId);
                            if (objProgramNode != null)
                                objProgramNode.Text = objProgram.DisplayName;
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objProgram.DisplayName);
                }
            }

            // Refresh Critter Powers.
            foreach (CritterPower objPower in CharacterObject.CritterPowers.Where(x => lstInternalIdFilter == null || !lstInternalIdFilter.Contains(x.InternalId)))
            {
                XmlNode objNode = objPower.GetNode();
                if (objNode != null)
                {
                    objPower.Bonus = objNode["bonus"];
                    if (objPower.Bonus != null)
                    {
                        string strSelected = objPower.Extra;
                        if (!int.TryParse(strSelected, out int intRating))
                        {
                            intRating = 1;
                            ImprovementManager.ForcedValue = strSelected;
                        }
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId, objPower.Bonus, false, intRating, objPower.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objPower.Extra = ImprovementManager.SelectedValue;
                            TreeNode objPowerNode = treCritterPowers.FindNode(objPower.InternalId);
                            if (objPowerNode != null)
                                objPowerNode.Text = objPower.DisplayName(GlobalOptions.Language);
                        }
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objPower.DisplayName(GlobalOptions.Language));
                }
            }

            // Refresh Metamagics and Echoes.
            // We cannot use foreach because metamagics/echoes can add more metamagics/echoes
            for (int j = 0; j < CharacterObject.Metamagics.Count; j++)
            {
                Metamagic objMetamagic = CharacterObject.Metamagics[j];
                if (objMetamagic.Grade < 0)
                    continue;
                // We're only re-apply improvements a list of items, not all of them
                if (lstInternalIdFilter != null && !lstInternalIdFilter.Contains(objMetamagic.InternalId))
                    continue;
                XmlNode objNode = objMetamagic.GetNode();
                if (objNode != null)
                {
                    objMetamagic.Bonus = objNode["bonus"];
                    if (objMetamagic.Bonus != null)
                    {
                        ImprovementManager.CreateImprovements(CharacterObject, objMetamagic.SourceType, objMetamagic.InternalId, objMetamagic.Bonus, false, 1, objMetamagic.DisplayNameShort(GlobalOptions.Language));
                    }
                }
                else
                {
                    strOutdatedItems.AppendLine(objMetamagic.DisplayName(GlobalOptions.Language));
                }
            }

            // Refresh Cyberware and Bioware.
            Dictionary<Cyberware, int> dicPairableCyberwares = new Dictionary<Cyberware, int>();
            foreach (Cyberware objCyberware in CharacterObject.Cyberware.GetAllDescendants(x => x.Children))
            {
                // We're only re-apply improvements a list of items, not all of them
                if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(objCyberware.InternalId))
                {
                    XmlNode objNode = objCyberware.GetNode();
                    if (objNode != null)
                    {
                        objCyberware.Bonus = objNode["bonus"];
                        objCyberware.WirelessBonus = objNode["wirelessbonus"];
                        objCyberware.PairBonus = objNode["pairbonus"];
                        if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                            ImprovementManager.ForcedValue = objCyberware.Forced;
                        if (objCyberware.Bonus != null)
                        {
                            ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.Bonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                        }
                        if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                        {
                            ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.WirelessBonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                        }
                        if (!objCyberware.IsModularCurrentlyEquipped)
                            objCyberware.ChangeModularEquip(false);
                        else if (objCyberware.PairBonus != null)
                        {
                            Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra);
                            if (objMatchingCyberware != null)
                                dicPairableCyberwares[objMatchingCyberware] = dicPairableCyberwares[objMatchingCyberware] + 1;
                            else
                                dicPairableCyberwares.Add(objCyberware, 1);
                        }
                        TreeNode objWareNode = objCyberware.SourceID == Cyberware.EssenceHoleGUID ? treCyberware.FindNode(Cyberware.EssenceHoleGUID.ToString("D")) : treCyberware.FindNode(objCyberware.InternalId);
                        if (objWareNode != null)
                            objWareNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
                    }
                    else
                    {
                        strOutdatedItems.AppendLine(objCyberware.DisplayName(GlobalOptions.Language));
                    }
                }
                foreach (Gear objGear in objCyberware.Gear)
                {
                    objGear.ReaddImprovements(treCyberware, strOutdatedItems, lstInternalIdFilter);
                }
            }
            // Separate Pass for PairBonuses
            foreach (KeyValuePair<Cyberware, int> objItem in dicPairableCyberwares)
            {
                Cyberware objCyberware = objItem.Key;
                int intCyberwaresCount = objItem.Value;
                List<Cyberware> lstPairableCyberwares = CharacterObject.Cyberware.DeepWhere(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped).ToList();
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(objCyberware.Location) && objCyberware.IncludePair.All(x => x == objCyberware.Name))
                {
                    int intMatchLocationCount = 0;
                    int intNotMatchLocationCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != objCyberware.Location)
                            intNotMatchLocationCount += 1;
                        else
                            intMatchLocationCount += 1;
                    }
                    // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                    intCyberwaresCount = Math.Min(intNotMatchLocationCount, intMatchLocationCount) * 2;
                }
                if (intCyberwaresCount > 0)
                {
                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        if (intCyberwaresCount % 2 == 0)
                        {
                            if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                ImprovementManager.ForcedValue = objCyberware.Forced;
                            ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                            TreeNode objNode = objLoopCyberware.SourceID == Cyberware.EssenceHoleGUID ? treCyberware.FindNode(Cyberware.EssenceHoleGUID.ToString("D")) : treCyberware.FindNode(objLoopCyberware.InternalId);
                            if (objNode != null)
                                objNode.Text = objLoopCyberware.DisplayName(GlobalOptions.Language);
                        }
                        intCyberwaresCount -= 1;
                        if (intCyberwaresCount <= 0)
                            break;
                    }
                }
            }

            // Refresh Armors.
            foreach (Armor objArmor in CharacterObject.Armor)
            {
                // We're only re-apply improvements a list of items, not all of them
                if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(objArmor.InternalId))
                {
                    XmlNode objNode = objArmor.GetNode();
                    if (objNode != null)
                    {
                        objArmor.Bonus = objNode["bonus"];
                        if (objArmor.Bonus != null)
                        {
                            if (objArmor.Equipped)
                            {
                                ImprovementManager.ForcedValue = objArmor.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Armor, objArmor.InternalId, objArmor.Bonus, false, objArmor.Rating, objArmor.DisplayNameShort(GlobalOptions.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objArmor.Extra = ImprovementManager.SelectedValue;

                                    TreeNode objArmorNode = treArmor.FindNode(objArmor.InternalId);
                                    if (objArmorNode != null)
                                        objArmorNode.Text = objArmor.DisplayName(GlobalOptions.Language);
                                }
                            }
                        }
                    }
                    else
                    {
                        strOutdatedItems.AppendLine(objArmor.DisplayName(GlobalOptions.Language));
                    }
                }

                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    // We're only re-apply improvements a list of items, not all of them
                    if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(objMod.InternalId))
                    {
                        XmlNode objChild = objMod.GetNode();

                        if (objChild != null)
                        {
                            objMod.Bonus = objChild["bonus"];
                            if (objMod.Bonus != null)
                            {
                                if (objMod.Equipped)
                                {
                                    ImprovementManager.ForcedValue = objMod.Extra;
                                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.Bonus, false, objMod.Rating, objMod.DisplayNameShort(GlobalOptions.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    {
                                        objMod.Extra = ImprovementManager.SelectedValue;

                                        TreeNode objPluginNode = treArmor.FindNode(objMod.InternalId);
                                        if (objPluginNode != null)
                                            objPluginNode.Text = objMod.DisplayName(GlobalOptions.Language);
                                    }
                                }
                            }
                        }
                        else
                        {
                            strOutdatedItems.AppendLine(objMod.DisplayName(GlobalOptions.Language));
                        }
                    }
                    foreach (Gear objGear in objMod.Gear)
                    {
                        objGear.ReaddImprovements(treArmor, strOutdatedItems, lstInternalIdFilter);
                    }
                }

                foreach (Gear objGear in objArmor.Gear)
                {
                    objGear.ReaddImprovements(treArmor, strOutdatedItems, lstInternalIdFilter);
                }
            }

            // Refresh Gear.
            foreach (Gear objGear in CharacterObject.Gear)
            {
                objGear.ReaddImprovements(treGear, strOutdatedItems, lstInternalIdFilter);
            }

            // Refresh Weapons Gear
            for (int i = 0; i < CharacterObject.Weapons.Count; i++)
            {
                Weapon objWeapon = CharacterObject.Weapons[i];
                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                {
                    foreach (Gear objGear in objAccessory.Gear)
                    {
                        objGear.ReaddImprovements(treWeapons, strOutdatedItems, lstInternalIdFilter);
                    }
                }
            }

            _blnReapplyImprovements = false;

            // If the status of any Character Event flags has changed, manually trigger those events.
            if (blnMAGEnabled != CharacterObject.MAGEnabled)
            {
                CharacterObject.EssenceAtSpecialStart = decEssenceAtSpecialStart;
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.MAGEnabled)));
            }

            if (blnRESEnabled != CharacterObject.RESEnabled)
            {
                CharacterObject.EssenceAtSpecialStart = decEssenceAtSpecialStart;
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.RESEnabled)));
            }

            if (blnDEPEnabled != CharacterObject.DEPEnabled)
            {
                CharacterObject.EssenceAtSpecialStart = decEssenceAtSpecialStart;
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.DEPEnabled)));
            }

            IsCharacterUpdateRequested = true;
            // Immediately call character update because it re-applies essence loss improvements
            UpdateCharacterInfo();

            Cursor = Cursors.Default;

            if (strOutdatedItems.Length > 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ReapplyImprovementsFoundOutdatedItems_Top", GlobalOptions.Language) +
                                strOutdatedItems.ToString() +
                                LanguageManager.GetString("Message_ReapplyImprovementsFoundOutdatedItems_Bottom", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ConfirmReapplyImprovements", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            IsDirty = true;
        }

        private void mnuSpecialPossess_Click(object sender, EventArgs e)
        {
            // Make sure the Spirit has been saved first.
            if (IsDirty)
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_PossessionSave", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Possession", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            // Prompt the user to select a save file to possess.
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                Character objVessel = new Character
                {
                    FileName = openFileDialog.FileName
                };
                objVessel.Load();
                // Make sure the Vessel is in Career Mode.
                if (!objVessel.Created)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(LanguageManager.GetString("Message_VesselInCareerMode", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Possession", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    objVessel.DeleteCharacter();
                    return;
                }

                // Load the Spirit's save file into a new Merge character.
                Character objMerge = new Character
                {
                    FileName = CharacterObject.FileName
                };
                objMerge.Load();
                objMerge.Possessed = true;
                objMerge.Alias = objVessel.CharacterName + " (" + LanguageManager.GetString("String_Possessed", GlobalOptions.Language) + ')';

                // Give the Critter the Immunity to Normal Weapons Power if they don't already have it.
                bool blnHasImmunity = false;
                foreach (CritterPower objCritterPower in objMerge.CritterPowers)
                {
                    if (objCritterPower.Name == "Immunity" && objCritterPower.Extra == "Normal Weapons")
                    {
                        blnHasImmunity = true;
                        break;
                    }
                }
                if (!blnHasImmunity)
                {
                    XmlDocument objPowerDoc = XmlManager.Load("critterpowers.xml");
                    XmlNode objPower = objPowerDoc.SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                    CritterPower objCritterPower = new CritterPower(objMerge);
                    objCritterPower.Create(objPower, 0, "Normal Weapons");
                    objMerge.CritterPowers.Add(objCritterPower);
                }

                //TOD: Implement Possession attribute bonuses.
                /* Add the Vessel's Physical Attributes to the Spirit's Force.
                objMerge.BOD.MetatypeMaximum = objVessel.BOD.Value + objMerge.MAG.TotalValue;
                objMerge.BOD.Value = objVessel.BOD.Value + objMerge.MAG.TotalValue;
                objMerge.AGI.MetatypeMaximum = objVessel.AGI.Value + objMerge.MAG.TotalValue;
                objMerge.AGI.Value = objVessel.AGI.Value + objMerge.MAG.TotalValue;
                objMerge.REA.MetatypeMaximum = objVessel.REA.Value + objMerge.MAG.TotalValue;
                objMerge.REA.Value = objVessel.REA.Value + objMerge.MAG.TotalValue;
                objMerge.STR.MetatypeMaximum = objVessel.STR.Value + objMerge.MAG.TotalValue;
                objMerge.STR.Value = objVessel.STR.Value + objMerge.MAG.TotalValue;*/

                // Copy any Lifestyles the Vessel has.
                foreach (Lifestyle objLifestyle in objVessel.Lifestyles)
                    objMerge.Lifestyles.Add(objLifestyle);

                // Copy any Armor the Vessel has.
                foreach (Armor objArmor in objVessel.Armor)
                {
                    objMerge.Armor.Add(objArmor);
                    CopyArmorImprovements(objVessel, objMerge, objArmor);
                }

                // Copy any Gear the Vessel has.
                foreach (Gear objGear in objVessel.Gear)
                {
                    objMerge.Gear.Add(objGear);
                    CopyGearImprovements(objVessel, objMerge, objGear);
                }

                // Copy any Cyberware/Bioware the Vessel has.
                foreach (Cyberware objCyberware in objVessel.Cyberware)
                {
                    objMerge.Cyberware.Add(objCyberware);
                    CopyCyberwareImprovements(objVessel, objMerge, objCyberware);
                }

                // Copy any Weapons the Vessel has.
                foreach (Weapon objWeapon in objVessel.Weapons)
                    objMerge.Weapons.Add(objWeapon);

                // Copy and Vehicles the Vessel has.
                foreach (Vehicle objVehicle in objVessel.Vehicles)
                    objMerge.Vehicles.Add(objVehicle);

                // Copy the character info.
                objMerge.Sex = objVessel.Sex;
                objMerge.Age = objVessel.Age;
                objMerge.Eyes = objVessel.Eyes;
                objMerge.Hair = objVessel.Hair;
                objMerge.Height = objVessel.Height;
                objMerge.Weight = objVessel.Weight;
                objMerge.Skin = objVessel.Skin;
                objMerge.Name = objVessel.Name;
                objMerge.StreetCred = objVessel.StreetCred;
                objMerge.BurntStreetCred = objVessel.BurntStreetCred;
                objMerge.Notoriety = objVessel.Notoriety;
                objMerge.PublicAwareness = objVessel.PublicAwareness;
                foreach (Image objMugshot in objVessel.Mugshots)
                    objMerge.Mugshots.Add(objMugshot);

                string strShowFileName = Path.GetFileName(CharacterObject.FileName);

                if (string.IsNullOrEmpty(strShowFileName))
                    strShowFileName = CharacterObject.CharacterName;
                strShowFileName = strShowFileName.TrimEndOnce(".chum5");

                strShowFileName += " (" + LanguageManager.GetString("String_Possessed", GlobalOptions.Language) + ')';

                // Now that everything is done, save the merged character and open them.
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language),
                    FileName = strShowFileName
                };

                Cursor = Cursors.Default;

                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    objMerge.FileName = saveFileDialog.FileName;
                    if (objMerge.Save())
                    {
                        // Get the name of the file and destroy the references to the Vessel and the merged character.
                        string strOpenFile = objMerge.FileName;
                        objMerge.DeleteCharacter();
                        objVessel.DeleteCharacter();

                        Character objOpenCharacter = Program.MainForm.LoadCharacter(strOpenFile);
                        Cursor = Cursors.Default;
                        Program.MainForm.OpenCharacter(objOpenCharacter);
                    }
                    else
                    {
                        // The save process was canceled, so drop everything.
                        objMerge.DeleteCharacter();
                        objVessel.DeleteCharacter();
                        Cursor = Cursors.Default;
                    }
                }
                else
                {
                    // The save process was canceled, so drop everything.
                    objMerge.DeleteCharacter();
                    objVessel.DeleteCharacter();
                }
            }
        }

        private void mnuSpecialPossessInanimate_Click(object sender, EventArgs e)
        {
            // Make sure the Spirit has been saved first.
            if (IsDirty)
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_PossessionSave", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_Possession", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            // Prompt the user to select an inanimate Vessel.
            XmlDocument objVesselDoc = XmlManager.Load("vessels.xml");
            List<ListItem> lstMetatype = new List<ListItem>();
            using (XmlNodeList xmlMetatypeList = objVesselDoc.SelectNodes("/chummer/metatypes/metatype"))
                if (xmlMetatypeList?.Count > 0)
                    foreach (XmlNode xmlMetatype in xmlMetatypeList)
                    {
                        string strName = xmlMetatype["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            ListItem objItem = new ListItem(strName, xmlMetatype["translate"]?.InnerText ?? strName);
                            lstMetatype.Add(objItem);
                        }
                    }

            frmSelectItem frmSelectVessel = new frmSelectItem
            {
                GeneralItems = lstMetatype
            };
            frmSelectVessel.ShowDialog(this);

            if (frmSelectVessel.DialogResult == DialogResult.Cancel)
                return;

            // Get the Node for the selected Vessel.
            XmlNode objSelected = objVesselDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + frmSelectVessel.SelectedItem + "\"]");

            if (objSelected == null)
                return;

            Cursor = Cursors.WaitCursor;

            // Load the Spirit's save file into a new Merge character.
            Character objMerge = new Character
            {
                FileName = CharacterObject.FileName
            };
            objMerge.Load();
            objMerge.Possessed = true;
            objMerge.Alias = frmSelectVessel.SelectedItem + " (" + LanguageManager.GetString("String_Possessed", GlobalOptions.Language) + ')';

            //TODO: Update spirit attribute values.
            /*
            // Get the CharacterAttribute Modifiers for the Vessel.
            int intBOD = Convert.ToInt32(objSelected["bodmin"].InnerText);
            int intAGI = Convert.ToInt32(objSelected["agimin"].InnerText);
            int intREA = Convert.ToInt32(objSelected["reamin"].InnerText);
            int intSTR = Convert.ToInt32(objSelected["strmin"].InnerText);

            // Add the CharacterAttribute modifiers, making sure that none of them go below 1.
            int intSetBOD = objMerge.MAG.TotalValue + intBOD;
            int intSetAGI = objMerge.MAG.TotalValue + intAGI;
            int intSetREA = objMerge.MAG.TotalValue + intREA;
            int intSetSTR = objMerge.MAG.TotalValue + intSTR;

            objMerge.BOD.MetatypeMinimum += intBOD;
            if (objMerge.BOD.MetatypeMinimum < 1)
                objMerge.BOD.MetatypeMinimum = 1;
            objMerge.BOD.MetatypeMaximum += intBOD;
            if (objMerge.BOD.MetatypeMaximum < 1)
                objMerge.BOD.MetatypeMaximum = 1;
            objMerge.BOD.Value = intSetBOD;
            if (objMerge.BOD.Value < 1)
                objMerge.BOD.Value = 1;

            objMerge.AGI.MetatypeMinimum += intAGI;
            if (objMerge.AGI.MetatypeMinimum < 1)
                objMerge.AGI.MetatypeMinimum = 1;
            objMerge.AGI.MetatypeMaximum += intAGI;
            if (objMerge.AGI.MetatypeMaximum < 1)
                objMerge.AGI.MetatypeMaximum = 1;
            objMerge.AGI.Value = intSetAGI;
            if (objMerge.AGI.Value < 1)
                objMerge.AGI.Value = 1;

            objMerge.REA.MetatypeMinimum += intREA;
            if (objMerge.REA.MetatypeMinimum < 1)
                objMerge.REA.MetatypeMinimum = 1;
            objMerge.REA.MetatypeMaximum += intREA;
            if (objMerge.REA.MetatypeMaximum < 1)
                objMerge.REA.MetatypeMaximum = 1;
            objMerge.REA.Value = intSetREA;
            if (objMerge.REA.Value < 1)
                objMerge.REA.Value = 1;

            objMerge.STR.MetatypeMinimum += intSTR;
            if (objMerge.STR.MetatypeMinimum < 1)
                objMerge.STR.MetatypeMinimum = 1;
            objMerge.STR.MetatypeMaximum += intSTR;
            if (objMerge.STR.MetatypeMaximum < 1)
                objMerge.STR.MetatypeMaximum = 1;
            objMerge.STR.Value = intSetSTR;
            if (objMerge.STR.Value < 1)
                objMerge.STR.Value = 1;
            */

            XmlDocument xmlPowerDoc = XmlManager.Load("critterpowers.xml");

            // Update the Movement if the Vessel has one.
            string strMovement = objSelected["movement"]?.InnerText;
            if (!string.IsNullOrEmpty(strMovement))
                objMerge.Movement = strMovement;

            // Add any additional Critter Powers the Vessel grants.
            XmlNode xmlPowersNode = objSelected["powers"];
            if (xmlPowersNode != null)
            {
                using (XmlNodeList xmlPowerList = xmlPowersNode.SelectNodes("power"))
                    if (xmlPowerList?.Count > 0)
                        foreach (XmlNode objXmlPower in xmlPowerList)
                        {
                            XmlNode objXmlCritterPower = xmlPowerDoc.SelectSingleNode("/chummer/powers/power[name = \"" + objXmlPower.InnerText + "\"]");
                            CritterPower objPower = new CritterPower(objMerge);
                            string strSelect = objXmlPower.Attributes?["select"]?.InnerText ?? string.Empty;
                            int intRating = Convert.ToInt32(objXmlPower.Attributes?["rating"]?.InnerText);

                            objPower.Create(objXmlCritterPower, intRating, strSelect);

                            objMerge.CritterPowers.Add(objPower);
                        }
            }

            // Give the Critter the Immunity to Normal Weapons Power if they don't already have it.
            bool blnHasImmunity = false;
            foreach (CritterPower objCritterPower in objMerge.CritterPowers)
            {
                if (objCritterPower.Name == "Immunity" && objCritterPower.Extra == "Normal Weapons")
                {
                    blnHasImmunity = true;
                    break;
                }
            }
            if (!blnHasImmunity)
            {
                XmlNode objPower = xmlPowerDoc.SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                CritterPower objCritterPower = new CritterPower(objMerge);
                objCritterPower.Create(objPower, 0, "Normal Weapons");
                objMerge.CritterPowers.Add(objCritterPower);
            }

            // Add any Improvements the Vessel grants.
            if (objSelected["bonus"] != null)
            {
                ImprovementManager.CreateImprovements(objMerge, Improvement.ImprovementSource.Metatype, frmSelectVessel.SelectedItem, objSelected["bonus"], false, 1, frmSelectVessel.SelectedItem);
            }

            // Now that everything is done, save the merged character and open them.
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };

            string[] strFile = CharacterObject.FileName.Split(Path.DirectorySeparatorChar);
            string strShowFileName = strFile[strFile.Length - 1];

            if (string.IsNullOrEmpty(strShowFileName))
                strShowFileName = CharacterObject.CharacterName;
            strShowFileName = strShowFileName.TrimEndOnce(".chum5");

            strShowFileName += " (" + LanguageManager.GetString("String_Possessed", GlobalOptions.Language) + ')';

            saveFileDialog.FileName = strShowFileName;

            Cursor = Cursors.Default;

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                objMerge.FileName = saveFileDialog.FileName;
                if (objMerge.Save())
                {
                    // Get the name of the file and destroy the references to the Vessel and the merged character.
                    string strOpenFile = objMerge.FileName;
                    objMerge.DeleteCharacter();

                    Character objOpenCharacter = Program.MainForm.LoadCharacter(strOpenFile);
                    Cursor = Cursors.Default;
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
                else
                {
                    // The save process was canceled, so drop everything.
                    objMerge.DeleteCharacter();
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                // The save process was canceled, so drop everything.
                objMerge.DeleteCharacter();
            }
        }

        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            object selectedObject = null;
            if (tabCharacterTabs.SelectedTab == tabStreetGear)
            {
                // Lifestyle Tab.
                if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                {
                    selectedObject = treLifestyles.SelectedNode?.Tag;
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    selectedObject = treArmor.SelectedNode?.Tag;
                }
                // Weapons Tab.
                else if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    selectedObject = treWeapons.SelectedNode?.Tag;
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    selectedObject = treGear.SelectedNode?.Tag;
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                selectedObject = treCyberware.SelectedNode?.Tag;
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                selectedObject = treVehicles.SelectedNode?.Tag;
            }

            CopyObject(selectedObject);
        }

        private void tsbCopy_Click(object sender, EventArgs e)
        {
            mnuEditCopy_Click(sender, e);
        }

        private void mnuSpecialConvertToFreeSprite_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("critterpowers.xml");
            XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Denial\"]");
            CritterPower objPower = new CritterPower(CharacterObject);
            objPower.Create(objXmlPower);
            objPower.CountTowardsLimit = false;
            if (objPower.InternalId.IsEmptyGuid())
                return;

            CharacterObject.CritterPowers.Add(objPower);

            CharacterObject.MetatypeCategory = "Free Sprite";

            RefreshMetatypeFields();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void mnuSpecialAddCyberwareSuite_Click(object sender, EventArgs e)
        {
            AddCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private void mnuSpecialAddBiowareSuite_Click(object sender, EventArgs e)
        {
            AddCyberwareSuite(Improvement.ImprovementSource.Bioware);
        }
        #endregion

        #region Martial Tab Control Events
        private void treMartialArts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _blnSkipRefresh = true;
            if (treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt)
            {
                cmdDeleteMartialArt.Enabled = !objMartialArt.IsQuality;
                string strPage = objMartialArt.Page(GlobalOptions.Language);
                lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objMartialArt.Source, GlobalOptions.Language) + ' ' + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objMartialArt.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            }
            else if (treMartialArts.SelectedNode?.Tag is MartialArtTechnique objTechnique)
            {
                // Display the Martial Art Advantage information.
                cmdDeleteMartialArt.Enabled = true;
                string strPage = objTechnique.Page(GlobalOptions.Language);
                lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objTechnique.Source, GlobalOptions.Language) + ' ' + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objTechnique.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            }
#if LEGACY
                else if (treMartialArts.SelectedNode?.Tag is MartialArtManeuver objManeuver)
                {
                            cmdDeleteMartialArt.Enabled = true;
                            string strPage = objManeuver.Page(GlobalOptions.Language);
                            lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objManeuver.Source, GlobalOptions.Language) + ' ' + strPage;
                            GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objManeuver.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                }
#endif
            else
            {
                cmdDeleteMartialArt.Enabled = false;
                lblMartialArtSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, string.Empty);
            }
            _blnSkipRefresh = false;
        }
#endregion

#region Button Events
        private void treLimit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteLimitModifier_Click(sender, e);
            }
        }
        private void panContacts_DragDrop(object sender, DragEventArgs e)
        {
            TransportWrapper wrapper = (TransportWrapper)e.Data.GetData(typeof(TransportWrapper));
            Control source = wrapper.Control;

            Point mousePosition = panContacts.PointToClient(new Point(e.X, e.Y));
            Control destination = panContacts.GetChildAtPoint(mousePosition);

            if (destination != null)
            {
                int indexDestination = panContacts.Controls.IndexOf(destination);
                if (panContacts.Controls.IndexOf(source) < indexDestination)
                    indexDestination--;

                panContacts.Controls.SetChildIndex(source, indexDestination);
            }

            foreach (ContactControl objControl in panContacts.Controls)
            {
                objControl.BackColor = SystemColors.Control;
            }
        }

        private void panContacts_DragOver(object sender, DragEventArgs e)
        {
            Point mousePosition = panContacts.PointToClient(new Point(e.X, e.Y));
            Control destination = panContacts.GetChildAtPoint(mousePosition);

            if (destination == null)
                return;

            destination.BackColor = SystemColors.ControlDark;
            foreach (ContactControl objControl in panContacts.Controls)
            {
                if (objControl != (destination as ContactControl))
                {
                    objControl.BackColor = SystemColors.Control;
                }
            }
            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
        }

        void panContacts_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        
        private void cmdAddSpell_Click(object sender, EventArgs e)
        {
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("spells.xml");
            bool blnAddAgain;

            do
            {
                int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Spells");
                // Make sure the character has enough Karma before letting them select a Spell.
                if (CharacterObject.Karma < intSpellKarmaCost)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }

                frmSelectSpell frmPickSpell = new frmSelectSpell(CharacterObject);
                frmPickSpell.ShowDialog(this);
                // Make sure the dialogue window was not canceled.
                if (frmPickSpell.DialogResult == DialogResult.Cancel)
                {
                    frmPickSpell.Dispose();
                    break;
                }
                blnAddAgain = frmPickSpell.AddAgain;

                XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickSpell.SelectedSpell + "\"]");

                Spell objSpell = new Spell(CharacterObject);
                objSpell.Create(objXmlSpell, string.Empty, frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
                if (objSpell.Alchemical)
                {
                    intSpellKarmaCost = CharacterObject.SpellKarmaCost("Preparations");
                }
                else if (objSpell.Category == "Rituals")
                {
                    intSpellKarmaCost = CharacterObject.SpellKarmaCost("Rituals");
                }
                if (objSpell.InternalId.IsEmptyGuid())
                {
                    frmPickSpell.Dispose();
                    continue;
                }
                if (CharacterObject.Karma < intSpellKarmaCost)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                objSpell.FreeBonus = frmPickSpell.FreeBonus;
                if (!objSpell.FreeBonus)
                {
                    if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language)
                        .Replace("{0}", objSpell.DisplayName(GlobalOptions.Language)).Replace("{1}", intSpellKarmaCost.ToString())))
                    {
                        frmPickSpell.Dispose();
                        continue;
                    }
                }
                // Barehanded Adept
                else if (CharacterObject.AdeptEnabled && !CharacterObject.MagicianEnabled && objSpell.Range == "T")
                {
                    objSpell.UsesUnarmed = true;
                }

                CharacterObject.Spells.Add(objSpell);
                if (!objSpell.FreeBonus)
                {
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                    objEntry.Create(-intSpellKarmaCost, LanguageManager.GetString("String_ExpenseLearnSpell", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objSpell.Name, ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objEntry);
                    CharacterObject.Karma -= intSpellKarmaCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddSpell, objSpell.InternalId);
                    objEntry.Undo = objUndo;
                }
                IsCharacterUpdateRequested = true;

                IsDirty = true;
                frmPickSpell.Dispose();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteSpell_Click(object sender, EventArgs e)
        {
            // Locate the Spell that is selected in the tree.
            if (!(treSpells.SelectedNode?.Tag is Spell objSpell)) return;
            // Spells that come from Initiation Grades can't be deleted normally. 
            if (objSpell.Grade != 0) return;
            if (!objSpell.Remove(CharacterObject)) return;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdAddSpirit_Click(object sender, EventArgs e)
        {
            AddSpirit();
        }

        private void cmdAddSprite_Click(object sender, EventArgs e)
        {
            AddSprite();
        }

        private void cmdAddContact_Click(object sender, EventArgs e)
        {
            AddContact();
        }

        private void cmdAddEnemy_Click(object sender, EventArgs e)
        {
            AddEnemy();
        }

        private void cmdAddPet_Click(object sender, EventArgs e)
        {
            AddPet();
        }

        private void tsAddFromFile_Click(object sender, EventArgs e)
        {
            AddContactsFromFile();
        }

        private void cmdAddCyberware_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickCyberware(null, Improvement.ImprovementSource.Cyberware);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCyberware_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode == null || treCyberware.SelectedNode.Level <= 0)
                return;
            // Locate the piece of Cyberware that is selected in the tree.
            if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (string.IsNullOrEmpty(objCyberware.ParentID))
                {
                    if (objCyberware.Capacity == "[*]" && treCyberware.SelectedNode.Level == 2)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                    {
                        if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteBioware", GlobalOptions.Language)))
                            return;
                    }
                    else
                    {
                        if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteCyberware", GlobalOptions.Language)))
                            return;
                    }

                    objCyberware.DeleteCyberware();

                    // If the Parent is populated, remove the item from its Parent.
                    Cyberware objParent = objCyberware.Parent;
                    if (objParent != null)
                    {
                        objParent.Children.Remove(objCyberware);
                    }
                    else
                    {
                        CharacterObject.Cyberware.Remove(objCyberware);
                        //Add essence hole.
                        IncreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100m));
                    }
                }
            }
            else if (treCyberware.SelectedNode?.Tag is Gear objGear)
            {
                // Find and remove the selected piece of Gear.
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear", GlobalOptions.Language)))
                    return;

                objGear.DeleteGear();

                if (objGear.Parent is IHasChildren<Gear> objParent)
                {
                    objParent.Children.Remove(objGear);
                }
                else
                {
                    CharacterObject.Cyberware.FindCyberwareGear(objGear.InternalId, out objCyberware);
                    objCyberware.Gear.Remove(objGear);
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void IncreaseEssenceHole(int intCentiessence)
        {
            Cyberware objHole = CharacterObject.Cyberware.FirstOrDefault(x => x.SourceID == Cyberware.EssenceHoleGUID);

            if (objHole == null)
            {
                XmlNode xmlEssHole = XmlManager.Load("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[id = \"b57eadaa-7c3b-4b80-8d79-cbbd922c1196\"]");
                objHole = new Cyberware(CharacterObject);
                List<Weapon> lstWeapons = new List<Weapon>();
                List<Vehicle> lstVehicles = new List<Vehicle>();
                objHole.Create(xmlEssHole, CharacterObject, CharacterObject.GetGradeList(Improvement.ImprovementSource.Cyberware, true).FirstOrDefault(x => x.Name == "None"), Improvement.ImprovementSource.Cyberware, intCentiessence, lstWeapons, lstVehicles);

                CharacterObject.Cyberware.Add(objHole);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    CharacterObject.Vehicles.Add(objVehicle);
                }
            }
            else
            {
                objHole.Rating += intCentiessence;
            }
        }

        private void DecreaseEssenceHole(int intCentiessence)
        {
            Cyberware objHole = CharacterObject.Cyberware.FirstOrDefault(x => x.SourceID == Cyberware.EssenceHoleGUID);

            if (objHole != null)
            {
                if (objHole.Rating > intCentiessence)
                {
                    objHole.Rating -= intCentiessence;
                }
                else
                {
                    objHole.DeleteCyberware();
                    CharacterObject.Cyberware.Remove(objHole);
                }
            }
        }

        private void cmdAddComplexForm_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("complexforms.xml");
            bool blnAddAgain;

            do
            {
                // The number of Complex Forms cannot exceed the character's LOG.
                if (CharacterObject.ComplexForms.Count >= ((CharacterObject.RES.Value * 2) + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.ComplexFormLimit)))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_ComplexFormLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ComplexFormLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                int intComplexFormKarmaCost = CharacterObject.ComplexFormKarmaCost;

                // Make sure the character has enough Karma before letting them select a Complex Form.
                if (CharacterObject.Karma < intComplexFormKarmaCost)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                // Let the user select a Program.
                frmSelectComplexForm frmPickComplexForm = new frmSelectComplexForm(CharacterObject);
                frmPickComplexForm.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickComplexForm.DialogResult == DialogResult.Cancel)
                {
                    frmPickComplexForm.Dispose();
                    break;
                }
                blnAddAgain = frmPickComplexForm.AddAgain;

                XmlNode objXmlComplexForm = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[id = \"" + frmPickComplexForm.SelectedComplexForm + "\"]");
                frmPickComplexForm.Dispose();
                if (objXmlComplexForm == null)
                    continue;

                // Check for SelectText.
                string strExtra = string.Empty;
                XmlNode xmlSelectText = objXmlComplexForm.SelectSingleNode("bonus/selecttext");
                if (xmlSelectText != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objXmlComplexForm["translate"]?.InnerText ?? objXmlComplexForm["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language))
                    };
                    frmPickText.ShowDialog(this);
                    strExtra = frmPickText.SelectedValue;
                }

                ComplexForm objComplexForm = new ComplexForm(CharacterObject);
                objComplexForm.Create(objXmlComplexForm, strExtra);
                if (objComplexForm.InternalId.IsEmptyGuid())
                    continue;

                CharacterObject.ComplexForms.Add(objComplexForm);

                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", objComplexForm.DisplayNameShort(GlobalOptions.Language)).Replace("{1}", intComplexFormKarmaCost.ToString())))
                {
                    // Remove the Improvements created by the Complex Form.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId);
                    continue;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intComplexFormKarmaCost * -1, LanguageManager.GetString("String_ExpenseLearnComplexForm", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objComplexForm.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intComplexFormKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddComplexForm, objComplexForm.InternalId);
                objExpense.Undo = objUndo;
                
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdAddArmor_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmor(null);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteArmor_Click(object sender, EventArgs e)
        {
            object objSelectedNode = treArmor.SelectedNode?.Tag;
            if (objSelectedNode == null)
                return;

            if (objSelectedNode is ICanRemove selectedObject)
            {
                selectedObject.Remove(CharacterObject);
            }
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddBioware_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickCyberware(null, Improvement.ImprovementSource.Bioware);
            }
            while (blnAddAgain);
        }

        private bool PickWeapon(object destObject)
        {
            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject);
            frmPickWeapon.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Weapons XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + frmPickWeapon.SelectedWeapon + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Weapon objWeapon = new Weapon(CharacterObject);
            objWeapon.Create(objXmlWeapon, lstWeapons);
            objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;

            decimal decCost = objWeapon.TotalCost;
            // Apply a markup if applicable.
            if (frmPickWeapon.Markup != 0)
            {
                decCost *= 1 + (frmPickWeapon.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objWeapon.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickWeapon.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickWeapon.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeapon", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objWeapon.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddWeapon, objWeapon.InternalId);
                objExpense.Undo = objUndo;
            }

            if (destObject is Location objLocation)
            {
                objWeapon.Location = objLocation;
                foreach (Weapon objExtraWeapon in lstWeapons)
                {
                    objExtraWeapon.Location = objLocation;
                }
            }

            foreach (Weapon objExtraWeapon in lstWeapons)
            {
                CharacterObject.Weapons.Add(objExtraWeapon);
            }

            CharacterObject.Weapons.Add(objWeapon);

            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickWeapon.AddAgain;
        }

        private void cmdAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickWeapon(string.Empty);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteWeapon_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treWeapons.SelectedNode == null) return;

            // Locate the Weapon that is selected in the tree.
            if (treWeapons.SelectedNode?.Tag is ICanRemove objRemovable)
            {
                objRemovable.Remove(CharacterObject);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            do
            {
                frmSelectLifestyle frmPickLifestyle = new frmSelectLifestyle(CharacterObject);
                frmPickLifestyle.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                {
                    frmPickLifestyle.Dispose();
                    break;
                }
                blnAddAgain = frmPickLifestyle.AddAgain;
                Lifestyle objLifestyle = frmPickLifestyle.SelectedLifestyle;
                frmPickLifestyle.Dispose();

                objLifestyle.Increments = 0;
                CharacterObject.Lifestyles.Add(objLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteLifestyle_Click(object sender, EventArgs e)
        {
            // Delete the selected Lifestyle.
            if (treLifestyles.SelectedNode?.Tag is ICanRemove selectedObject)
            {
                if (!selectedObject.Remove(CharacterObject)) return;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void cmdAddGear_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = treGear.SelectedNode?.Tag is Location objLocation
                    ? PickGear(null, objLocation)
                    : PickGear(null);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteGear_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode?.Tag is ICanRemove objSelectedGear)
            {
                objSelectedGear.Remove(CharacterObject);
            }
            else
            {
                return;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private bool AddVehicle(Location objLocation = null)
        {
            frmSelectVehicle frmPickVehicle = new frmSelectVehicle(CharacterObject);
            frmPickVehicle.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickVehicle.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");

            XmlNode objXmlVehicle = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + frmPickVehicle.SelectedVehicle + "\"]");
            Vehicle objVehicle = new Vehicle(CharacterObject);
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
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickVehicle.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickVehicle.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicle", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objVehicle.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicle, objVehicle.InternalId);
                objExpense.Undo = objUndo;
            }

            objVehicle.BlackMarketDiscount = frmPickVehicle.BlackMarketDiscount;

            objVehicle.Location = objLocation;

            CharacterObject.Vehicles.Add(objVehicle);

            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickVehicle.AddAgain;
        }

        private void cmdAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddVehicle(null);
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
                selectedObject.Remove(CharacterObject);
            }
            else if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["improvesensor"] != null || (objMod.WirelessOn && objMod.WirelessBonus?["improvesensor"] != null))
                {
                    objMod.Parent.ChangeVehicleSensor(treVehicles, false, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                }

                // If this is the Obsolete Mod, the user must select a percentage. This will create an Expense that costs X% of the Vehicle's base cost to remove the special Obsolete Mod.
                if (objMod.Name == "Obsolete" || (objMod.Name == "Obsolescent" && CharacterObjectOptions.AllowObsolescentUpgrade))
                {
                    frmSelectNumber frmModPercent = new frmSelectNumber()
                    {
                        Minimum = 0,
                        Maximum = 1000000,
                        Description = LanguageManager.GetString("String_Retrofit", GlobalOptions.Language)
                    };
                    frmModPercent.ShowDialog(this);

                    if (frmModPercent.DialogResult == DialogResult.Cancel)
                        return;

                    decimal decPercentage = frmModPercent.SelectedValue;
                    decimal decVehicleCost = objMod.Parent.OwnCost;

                    // Make sure the character has enough Nuyen for the expense.
                    decimal decCost = decVehicleCost * decPercentage / 100;

                    // Create a Vehicle Mod for the Retrofit.
                    VehicleMod objRetrofit = new VehicleMod(CharacterObject);

                    XmlDocument objVehiclesDoc = XmlManager.Load("vehicles.xml");
                    XmlNode objXmlNode = objVehiclesDoc.SelectSingleNode("/chummer/mods/mod[name = \"Retrofit\"]");
                    objRetrofit.Create(objXmlNode, 0, objMod.Parent);
                    objRetrofit.Cost = decCost.ToString(GlobalOptions.InvariantCultureInfo);
                    objRetrofit.IncludedInVehicle = true;
                    objMod.Parent.Mods.Add(objRetrofit);

                    // Create an Expense Log Entry for removing the Obsolete Mod.
                    ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                    objEntry.Create(decCost * -1, LanguageManager.GetString("String_ExpenseVehicleRetrofit", GlobalOptions.Language).Replace("{0}", objMod.Parent.DisplayName(GlobalOptions.Language)), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                    // Adjust the character's Nuyen total.
                    CharacterObject.Nuyen += decCost * -1;
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
                else
                {
                    objCyberware =
                        CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == objCyberware.InternalId,
                            out objMod);
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
                    objGear = CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out objCyberware);
                    if (objCyberware != null)
                        objCyberware.Gear.Remove(objGear);
                    else if (objWeaponAccessory != null)
                        objWeaponAccessory.Gear.Remove(objGear);
                    else
                        objVehicle.Gear.Remove(objGear);
                }

                objGear.DeleteGear();
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddMartialArt_Click(object sender, EventArgs e)
        {
            if (MartialArt.Purchase(CharacterObject))
            {
                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void cmdDeleteMartialArt_Click(object sender, EventArgs e)
        {
            if (!(treMartialArts.SelectedNode?.Tag is ICanRemove objSelectedNode)) return;
            if (!objSelectedNode.Remove(CharacterObject)) return;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

#if LEGACY
        private void cmdAddManeuver_Click(object sender, EventArgs e)
        {
            // Characters may only have 2 Maneuvers per Martial Art Rating.
            int intTotalRating = 0;
            foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                intTotalRating += objMartialArt.Rating * 2;

            if (CharacterObject.MartialArtManeuvers.Count >= intTotalRating)
            {
                MessageBox.Show(LanguageManager.GetString("Message_MartialArtManeuverLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_MartialArtManeuverLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the character has enough Karma.
            int intKarmaCost = CharacterObjectOptions.KarmaManeuver;


            if (intKarmaCost > CharacterObject.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectMartialArtManeuver frmPickMartialArtManeuver = new frmSelectMartialArtManeuver(CharacterObject);
            frmPickMartialArtManeuver.ShowDialog(this);

            if (frmPickMartialArtManeuver.DialogResult == DialogResult.Cancel)
                return;

            // Open the Martial Arts XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("martialarts.xml");

            XmlNode objXmlManeuver = objXmlDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + frmPickMartialArtManeuver.SelectedManeuver + "\"]");

            MartialArtManeuver objManeuver = new MartialArtManeuver(CharacterObject);
            objManeuver.Create(objXmlManeuver);
            CharacterObject.MartialArtManeuvers.Add(objManeuver);

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseLearnManeuver", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objManeuver.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddMartialArtManeuver, objManeuver.InternalId);
            objExpense.Undo = objUndo;

            TreeNode objSelectedNode = treMartialArts.FindNode(objManeuver.InternalId);
            if (objSelectedNode != null)
                treMartialArts.SelectedNode = objSelectedNode;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endif

        private void cmdAddMugshot_Click(object sender, EventArgs e)
        {
            if (AddMugshot())
            {
                lblNumMugshots.Text = LanguageManager.GetString("String_Of", GlobalOptions.Language) + CharacterObject.Mugshots.Count.ToString(GlobalOptions.CultureInfo);
                nudMugshotIndex.Maximum += 1;
                nudMugshotIndex.Value = CharacterObject.Mugshots.Count;

                IsDirty = true;
            }
        }

        private void cmdDeleteMugshot_Click(object sender, EventArgs e)
        {
            if (CharacterObject.Mugshots.Count > 0)
            {
                RemoveMugshot(decimal.ToInt32(nudMugshotIndex.Value) - 1);

                lblNumMugshots.Text = LanguageManager.GetString("String_Of", GlobalOptions.Language) + CharacterObject.Mugshots.Count.ToString(GlobalOptions.CultureInfo);
                nudMugshotIndex.Maximum -= 1;
                if (nudMugshotIndex.Value > nudMugshotIndex.Maximum)
                    nudMugshotIndex.Value = nudMugshotIndex.Maximum;
                else
                {
                    if (decimal.ToInt32(nudMugshotIndex.Value) - 1 == CharacterObject.MainMugshotIndex)
                        chkIsMainMugshot.Checked = true;
                    else if (chkIsMainMugshot.Checked)
                        chkIsMainMugshot.Checked = false;

                    UpdateMugshot(picMugshot, decimal.ToInt32(nudMugshotIndex.Value) - 1);
                }

                IsDirty = true;
            }
        }


        private void nudMugshotIndex_ValueChanged(object sender, EventArgs e)
        {
            if (CharacterObject.Mugshots.Count == 0)
            {
                nudMugshotIndex.Minimum = 0;
                nudMugshotIndex.Maximum = 0;
                nudMugshotIndex.Value = 0;
            }
            else
            {
                nudMugshotIndex.Minimum = 1;
                if (nudMugshotIndex.Value < nudMugshotIndex.Minimum)
                    nudMugshotIndex.Value = nudMugshotIndex.Maximum;
                else if (nudMugshotIndex.Value > nudMugshotIndex.Maximum)
                    nudMugshotIndex.Value = nudMugshotIndex.Minimum;
            }

            if (decimal.ToInt32(nudMugshotIndex.Value) - 1 == CharacterObject.MainMugshotIndex)
                chkIsMainMugshot.Checked = true;
            else if (chkIsMainMugshot.Checked)
                chkIsMainMugshot.Checked = false;

            UpdateMugshot(picMugshot, decimal.ToInt32(nudMugshotIndex.Value) - 1);
        }

        private void chkIsMainMugshot_CheckedChanged(object sender, EventArgs e)
        {
            bool blnStatusChanged = false;
            if (chkIsMainMugshot.Checked && CharacterObject.MainMugshotIndex != decimal.ToInt32(nudMugshotIndex.Value) - 1)
            {
                CharacterObject.MainMugshotIndex = decimal.ToInt32(nudMugshotIndex.Value) - 1;
                blnStatusChanged = true;
            }
            else if (chkIsMainMugshot.Checked == false && decimal.ToInt32(nudMugshotIndex.Value) - 1 == CharacterObject.MainMugshotIndex)
            {
                CharacterObject.MainMugshotIndex = -1;
                blnStatusChanged = true;
            }

            if (blnStatusChanged)
            {
                IsDirty = true;
            }
        }

        private void cmdAddMetamagic_Click(object sender, EventArgs e)
        {
            if (CharacterObject.MAGEnabled)
            {
                // Make sure that the Initiate Grade is not attempting to go above the character's MAG CharacterAttribute.
                if (CharacterObject.InitiateGrade + 1 > CharacterObject.MAG.TotalValue ||
                    (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.InitiateGrade + 1 > CharacterObject.MAGAdept.TotalValue))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotIncreaseInitiateGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotIncreaseInitiateGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Make sure the character has enough Karma.
                decimal decMultiplier = 1.0m;
                if (chkInitiationGroup.Checked)
                    decMultiplier -= 0.1m;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= 0.1m;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= 0.1m;

                int intKarmaExpense = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                if (intKarmaExpense > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (chkInitiationSchooling.Checked && (10000 > CharacterObject.Nuyen))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (chkInitiationSchooling.Checked)
                {
                    if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaandNuyenExpense", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_InitiateGrade", GlobalOptions.Language)).Replace("{1}", (CharacterObject.InitiateGrade + 1).ToString()).Replace("{2}", intKarmaExpense.ToString()).Replace("{3}", (10000).ToString())))
                        return;
                }
                else
                {
                    if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_InitiateGrade", GlobalOptions.Language)).Replace("{1}", (CharacterObject.InitiateGrade + 1).ToString()).Replace("{2}", intKarmaExpense.ToString())))
                        return;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseInitiateGrade", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + CharacterObject.InitiateGrade + " -> " + (CharacterObject.InitiateGrade + 1), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.InitiateGrade + 1, CharacterObject.RESEnabled, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ImproveInitiateGrade, objGrade.InternalId);
                objExpense.Undo = objUndo;

                if (chkInitiationSchooling.Checked)
                {
                    ExpenseLogEntry objNuyenExpense = new ExpenseLogEntry(CharacterObject);
                    objNuyenExpense.Create(-10000, LanguageManager.GetString("String_ExpenseInitiateGrade", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + CharacterObject.InitiateGrade + " -> " + (CharacterObject.InitiateGrade + 1), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objNuyenExpense);
                    CharacterObject.Nuyen -= 10000;

                    ExpenseUndo objNuyenUndo = new ExpenseUndo();
                    objNuyenUndo.CreateNuyen(NuyenExpenseType.ImproveInitiateGrade, objGrade.InternalId, 10000);
                    objNuyenExpense.Undo = objNuyenUndo;
                }

                int intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                string strInitTip = LanguageManager.GetString("Tip_ImproveInitiateGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.InitiateGrade + 1).ToString()).Replace("{1}", intAmount.ToString());
                GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
            }
            else if (CharacterObject.RESEnabled)
            {

                // Make sure that the Initiate Grade is not attempting to go above the character's RES CharacterAttribute.
                if (CharacterObject.SubmersionGrade + 1 > CharacterObject.RES.TotalValue)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotIncreaseSubmersionGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotIncreaseSubmersionGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Make sure the character has enough Karma.
                decimal decMultiplier = 1.0m;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= 0.2m;

                int intKarmaExpense = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                if (intKarmaExpense > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpense", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_SubmersionGrade", GlobalOptions.Language)).Replace("{1}", (CharacterObject.SubmersionGrade + 1).ToString()).Replace("{2}", intKarmaExpense.ToString())))
                    return;

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseSubmersionGrade", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + CharacterObject.SubmersionGrade + " -> " + (CharacterObject.SubmersionGrade + 1), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.SubmersionGrade + 1, CharacterObject.RESEnabled, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ImproveInitiateGrade, objGrade.InternalId);
                objExpense.Undo = objUndo;

                int intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                string strInitTip = LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.SubmersionGrade + 1).ToString()).Replace("{1}", intAmount.ToString());
                GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdDeleteMetamagic_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject)) return;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdKarmaGained_Click(object sender, EventArgs e)
        {
            frmExpense frmNewExpense = new frmExpense(CharacterObjectOptions)
            {
                KarmaNuyenExchangeString = LanguageManager.GetString("String_WorkingForThePeople", GlobalOptions.Language)
            };
            frmNewExpense.ShowDialog(this);

            if (frmNewExpense.DialogResult == DialogResult.Cancel)
                return;

            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(frmNewExpense.Amount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
            objEntry.Undo = objUndo;

            // Adjust the character's Karma total.
            CharacterObject.Karma += decimal.ToInt32(frmNewExpense.Amount);

            if (frmNewExpense.KarmaNuyenExchange)
            {
                // Create the Expense Log Entry.
                objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(frmNewExpense.Amount * -CharacterObjectOptions.NuyenPerBP, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
                objEntry.Undo = objUndo;

                // Adjust the character's Nuyen total.
                CharacterObject.Nuyen += frmNewExpense.Amount * -CharacterObjectOptions.NuyenPerBP;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdKarmaSpent_Click(object sender, EventArgs e)
        {
            frmExpense frmNewExpense = new frmExpense(CharacterObjectOptions)
            {
                KarmaNuyenExchangeString = LanguageManager.GetString("String_WorkingForTheMan", GlobalOptions.Language)
            };

            frmNewExpense.ShowDialog(this);

            if (frmNewExpense.DialogResult == DialogResult.Cancel)
                return;

            // Make sure the Karma expense would not put the character's remaining Karma amount below 0.
            if (CharacterObject.Karma - frmNewExpense.Amount < 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(frmNewExpense.Amount * -1, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
            objEntry.Undo = objUndo;

            // Adjust the character's Karma total.
            CharacterObject.Karma += decimal.ToInt32(frmNewExpense.Amount) * -1;

            if (frmNewExpense.KarmaNuyenExchange)
            {
                // Create the Expense Log Entry.
                objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(frmNewExpense.Amount * CharacterObjectOptions.NuyenPerBP, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
                objEntry.Undo = objUndo;

                // Adjust the character's Nuyen total.
                CharacterObject.Nuyen += frmNewExpense.Amount * CharacterObjectOptions.NuyenPerBP;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdKarmaEdit_Click(object sender, EventArgs e)
        {
            lstKarma_DoubleClick(sender, e);
        }

        private void cmdNuyenGained_Click(object sender, EventArgs e)
        {
            frmExpense frmNewExpense = new frmExpense(CharacterObjectOptions)
            {
                Mode = ExpenseType.Nuyen,
                KarmaNuyenExchangeString = LanguageManager.GetString("String_WorkingForTheMan", GlobalOptions.Language)
            };
            frmNewExpense.ShowDialog(this);

            if (frmNewExpense.DialogResult == DialogResult.Cancel)
                return;

            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(frmNewExpense.Amount, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
            objEntry.Refund = frmNewExpense.Refund;
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.ManualAdd, string.Empty);
            objEntry.Undo = objUndo;

            // Adjust the character's Nuyen total.
            CharacterObject.Nuyen += frmNewExpense.Amount;

            if (frmNewExpense.KarmaNuyenExchange)
            {
                // Create the Expense Log Entry.
                objEntry = new ExpenseLogEntry(CharacterObject);
                int intAmount = -decimal.ToInt32(frmNewExpense.Amount / CharacterObjectOptions.NuyenPerBP);
                objEntry.Create(intAmount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma += intAmount;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdNuyenSpent_Click(object sender, EventArgs e)
        {
            frmExpense frmNewExpense = new frmExpense(CharacterObjectOptions)
            {
                Mode = ExpenseType.Nuyen,
                KarmaNuyenExchangeString = LanguageManager.GetString("String_WorkingForThePeople", GlobalOptions.Language)
            };
            frmNewExpense.ShowDialog(this);

            if (frmNewExpense.DialogResult == DialogResult.Cancel)
                return;

            // Make sure the Nuyen expense would not put the character's remaining Nuyen amount below 0.
            if (CharacterObject.Nuyen - frmNewExpense.Amount < 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(frmNewExpense.Amount * -1, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
            objEntry.Undo = objUndo;

            // Adjust the character's Nuyen total.
            CharacterObject.Nuyen += frmNewExpense.Amount * -1;

            if (frmNewExpense.KarmaNuyenExchange)
            {
                // Create the Expense Log Entry.
                objEntry = new ExpenseLogEntry(CharacterObject);
                int intAmount = decimal.ToInt32(frmNewExpense.Amount / CharacterObjectOptions.NuyenPerBP);
                objEntry.Create(intAmount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma += intAmount;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdNuyenEdit_Click(object sender, EventArgs e)
        {
            lstNuyen_DoubleClick(sender, e);
        }

        private void cmdDecreaseLifestyleMonths_Click(object sender, EventArgs e)
        {
            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(0, LanguageManager.GetString("String_ExpenseDecreaseLifestyle", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objLifestyle.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);

            objLifestyle.Increments -= 1;
            lblLifestyleMonths.Text = objLifestyle.Increments.ToString();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdIncreaseLifestyleMonths_Click(object sender, EventArgs e)
        {
            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;
            objLifestyle.IncrementMonths(CharacterObject);
            lblLifestyleMonths.Text = objLifestyle.Increments.ToString();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddCritterPower_Click(object sender, EventArgs e)
        {
            // Make sure the Critter is allowed to have Optional Powers.
            XmlDocument objXmlDocument = XmlManager.Load("critterpowers.xml");

            bool blnAddAgain;
            do
            {
                frmSelectCritterPower frmPickCritterPower = new frmSelectCritterPower(CharacterObject);
                frmPickCritterPower.ShowDialog(this);

                if (frmPickCritterPower.DialogResult == DialogResult.Cancel)
                {
                    frmPickCritterPower.Dispose();
                    break;
                }
                blnAddAgain = frmPickCritterPower.AddAgain;

                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = \"" + frmPickCritterPower.SelectedPower + "\"]");
                CritterPower objPower = new CritterPower(CharacterObject);
                objPower.Create(objXmlPower, frmPickCritterPower.SelectedRating);
                objPower.PowerPoints = frmPickCritterPower.PowerPoints;
                if (objPower.InternalId.IsEmptyGuid())
                {
                    frmPickCritterPower.Dispose();
                    continue;
                }

                CharacterObject.CritterPowers.Add(objPower);
                
                if (objPower.Karma > 0)
                {
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(objPower.Karma * -1, LanguageManager.GetString("String_ExpensePurchaseCritterPower", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objPower.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddCritterPower, objPower.InternalId);
                    objExpense.Undo = objUndo;
                }
                
                IsCharacterUpdateRequested = true;

                IsDirty = true;
                frmPickCritterPower.Dispose();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCritterPower_Click(object sender, EventArgs e)
        {
            // If the selected object is not a complex form or it comes from an initiate grade, we don't want to remove it.
            if (!(treCritterPowers.SelectedNode?.Tag is CritterPower objCritterPower) || objCritterPower.Grade != 0) return;
            if (!objCritterPower.Remove(CharacterObject)) return;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdDeleteComplexForm_Click(object sender, EventArgs e)
        {
            // If the selected object is not a complex form or it comes from an initiate grade, we don't want to remove it.
            if (!(treComplexForms.SelectedNode?.Tag is ComplexForm objComplexForm) || objComplexForm.Grade != 0) return;
            if (!objComplexForm.Remove(CharacterObject)) return;
                
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

#if LEGACY
        private void cmdImproveComplexForm_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treComplexForms.SelectedNode;
            if (objSelectedNode == null)
                return;

            if (objSelectedNode is ComplexForm objComplexForm)
            {
                // Make sure the character has enough Karma.
                int intKarmaCost = CharacterObjectOptions.KarmaImproveComplexForm;

                if (intKarmaCost > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", intKarmaCost.ToString()).Replace("{1}", objComplexForm.DisplayNameShort(GlobalOptions.Language))))
                    return;

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseComplexForm", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objComplexForm.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ImproveComplexForm, objComplexForm.InternalId);
                objExpense.Undo = objUndo;

                objSelectedNode.Text = objComplexForm.DisplayName;

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
#endif

        private void cmdGearReduceQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear)) return;
            
            int intDecimalPlaces = 0;
            if (objGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = CharacterObjectOptions.NuyenDecimals;
            }
            else if (objGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
            }

            frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
            {
                Minimum = 0,
                Maximum = objGear.Quantity,
                Description = LanguageManager.GetString("String_ReduceGear", GlobalOptions.Language)
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            decimal decSelectedValue = frmPickNumber.SelectedValue;

            if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_ReduceQty", GlobalOptions.Language).Replace("{0}", decSelectedValue.ToString(GlobalOptions.CultureInfo))))
                return;
                
            objGear.Quantity -= decSelectedValue;

            if (objGear.Quantity > 0)
            {
                objSelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else
            {
                // Remove any Weapons that came with it.
                objGear.DeleteGear();

                // Remove the Gear if its quantity has been reduced to 0.
                if (objGear.Parent is Gear objParent)
                {
                    objParent.Children.Remove(objGear);
                }
                else
                {
                    CharacterObject.Gear.Remove(objGear);
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdGearSplitQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objSelectedGear))
                return;

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectOptions.NuyenDecimals);
                // Need a for loop instead of a power system to maintain exact precision
                for (int i = 0; i < intDecimalPlaces; ++i)
                    decMinimumAmount /= 10.0m;
            }
            else if (objSelectedGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
                decMinimumAmount = 0.01m;
            }
            // Cannot split a stack of 1 item.
            if (objSelectedGear.Quantity <= decMinimumAmount)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotSplitGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotSplitGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
            {
                Minimum = decMinimumAmount,
                Maximum = objSelectedGear.Quantity - decMinimumAmount,
                Description = LanguageManager.GetString("String_SplitGear", GlobalOptions.Language)
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            // Create a new piece of Gear.
            Gear objGear = new Gear(CharacterObject);

            objGear.Copy(objSelectedGear);

            objGear.Quantity = frmPickNumber.SelectedValue;
            objGear.Equipped = objSelectedGear.Equipped;
            objGear.Location = objSelectedGear.Location;
            objGear.Notes = objSelectedGear.Notes;

            // Update the selected item.
            objSelectedGear.Quantity -= frmPickNumber.SelectedValue;
            objSelectedNode.Text = objSelectedGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

            CharacterObject.Gear.Add(objGear);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdGearMergeQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear))
                return;
            List<Gear> lstGear = new List<Gear>();

            foreach (Gear objCharacterGear in CharacterObject.Gear)
            {
                bool blnMatch = false;
                // Matches must happen on Name, Category, Rating, and Extra, plus all plugins.
                if (objCharacterGear.Name == objGear.Name && objCharacterGear.Category == objGear.Category && objCharacterGear.Rating == objGear.Rating && objCharacterGear.Extra == objGear.Extra && objCharacterGear.InternalId != objGear.InternalId)
                {
                    blnMatch = true;
                    if (objCharacterGear.Children.Count == objGear.Children.Count)
                    {
                        for (int i = 0; i <= objCharacterGear.Children.Count - 1; i++)
                        {
                            if (objCharacterGear.Children[i].Name != objGear.Children[i].Name || objCharacterGear.Children[i].Extra != objGear.Children[i].Extra || objCharacterGear.Children[i].Rating != objGear.Children[i].Rating)
                            {
                                blnMatch = false;
                                break;
                            }
                        }
                    }
                    else
                        blnMatch = false;
                }

                if (blnMatch)
                    lstGear.Add(objCharacterGear);
            }

            // If there were no matches, don't try to merge anything.
            if (lstGear.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotMergeGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotMergeGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show the Select Item window.
            frmSelectItem frmPickItem = new frmSelectItem
            {
                Gear = lstGear
            };
            frmPickItem.ShowDialog(this);

            if (frmPickItem.DialogResult == DialogResult.Cancel)
                return;

            Gear objSelectedGear = CharacterObject.Gear.DeepFindById(frmPickItem.SelectedItem);

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectOptions.NuyenDecimals);
                // Need a for loop instead of a power system to maintain exact precision
                for (int i = 0; i < intDecimalPlaces; ++i)
                    decMinimumAmount /= 10.0m;
            }
            else if (objSelectedGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
                decMinimumAmount = 0.01m;
            }
            frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
            {
                Minimum = decMinimumAmount,
                Maximum = objGear.Quantity,
                Description = LanguageManager.GetString("String_MergeGear", GlobalOptions.Language)
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            // Increase the quantity for the selected item.
            objSelectedGear.Quantity += frmPickNumber.SelectedValue;
            // Located the item in the Tree and update its display information.
            TreeNode objNode = treGear.FindNode(objSelectedGear.InternalId);
            if (objNode != null)
                objNode.Text = objSelectedGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

            // Reduce the quantity for the selected item.
            objGear.Quantity -= frmPickNumber.SelectedValue;
            // If the quantity has reached 0, delete the item and any Weapons it created.
            if (objGear.Quantity <= 0)
            {
                // Remove the Gear if its quantity has been reduced to 0.
                if (objGear.Parent is Gear objParent)
                {
                    objParent.Children.Remove(objGear);
                }
                else
                {
                    CharacterObject.Gear.Remove(objGear);
                }
                objGear.DeleteGear();
            }
            else
                objSelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

            IsDirty = true;
        }

        private void cmdGearMoveToVehicle_Click(object sender, EventArgs e)
        {
            frmSelectItem frmPickItem = new frmSelectItem
            {
                Vehicles = CharacterObject.Vehicles
            };
            frmPickItem.ShowDialog(this);

            if (frmPickItem.DialogResult == DialogResult.Cancel)
                return;

            // Locate the selected Vehicle.
            Vehicle objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == frmPickItem.SelectedItem);

            if (objVehicle == null)
                return;

            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objSelectedGear))
                return;

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectOptions.NuyenDecimals);
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
                    Description = LanguageManager.GetString("String_MoveGear", GlobalOptions.Language)
                };
                frmPickNumber.ShowDialog(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                decMove = frmPickNumber.SelectedValue;
            }

            // See if the Vehicle already has a matching piece of Gear.
            Gear objFoundGear = objVehicle.Gear.FirstOrDefault(x => x.IsIdenticalToOtherGear(objSelectedGear));

            if (objFoundGear == null)
            {
                // Create a new piece of Gear.
                Gear objGear = new Gear(CharacterObject);

                objGear.Copy(objSelectedGear);

                objGear.Quantity = decMove;
                objGear.Location = null;

                objVehicle.Gear.Add(objGear);
            }
            else
            {
                // Everything matches up, so just increase the quantity.
                objFoundGear.Quantity += decMove;
                TreeNode objFoundNode = treVehicles.FindNode(objFoundGear.InternalId);
                if (objFoundNode != null)
                    objFoundNode.Text = objFoundGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }

            // Update the selected item.
            objSelectedGear.Quantity -= decMove;
            if (objSelectedGear.Quantity <= 0)
            {
                if (objSelectedGear.Parent is Gear objParent)
                    objParent.Children.Remove(objSelectedGear);
                else
                    CharacterObject.Gear.Remove(objSelectedGear);
                objSelectedGear.DeleteGear();
            }
            else
                objSelectedNode.Text = objSelectedGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdVehicleMoveToInventory_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Locate the selected Weapon.
            if (objSelectedNode?.Tag is Weapon objWeapon)
            {
                CharacterObject.Vehicles.FindVehicleWeapon(objWeapon.InternalId, out Vehicle objVehicle, out WeaponMount objMount, out VehicleMod objMod);
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

                CharacterObject.Weapons.Add(objWeapon);

                objWeapon.ParentVehicle = null;
            }
            else if (objSelectedNode?.Tag is Gear objSelectedGear)
            {
                // Locate the selected Gear.
                CharacterObject.Vehicles.FindVehicleGear(objSelectedGear.InternalId, out Vehicle objVehicle,
                    out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);

                decimal decMinimumAmount = 1.0m;
                int intDecimalPlaces = 0;
                if (objSelectedGear.Name.StartsWith("Nuyen"))
                {
                    intDecimalPlaces = Math.Max(0, CharacterObjectOptions.NuyenDecimals);
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
                        Description = LanguageManager.GetString("String_MoveGear", GlobalOptions.Language)
                    };
                    frmPickNumber.ShowDialog(this);

                    if (frmPickNumber.DialogResult == DialogResult.Cancel)
                        return;

                    decMove = frmPickNumber.SelectedValue;
                }

                // See if the character already has a matching piece of Gear.
                Gear objFoundGear = CharacterObject.Gear.FirstOrDefault(x => objSelectedGear.IsIdenticalToOtherGear(x));

                if (objFoundGear == null)
                {
                    // Create a new piece of Gear.
                    Gear objGear = new Gear(CharacterObject);

                    objGear.Copy(objSelectedGear);

                    objGear.Quantity = decMove;

                    CharacterObject.Gear.Add(objGear);

                    AddGearImprovements(objGear);
                }
                else
                {
                    // Everything matches up, so just increase the quantity.
                    objFoundGear.Quantity += decMove;
                    TreeNode objFoundNode = treGear.FindNode(objFoundGear.InternalId);
                    if (objFoundNode != null)
                        objFoundNode.Text = objFoundGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                    objSelectedNode.Text = objSelectedGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else return;
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
        
        private void cmdGearIncreaseQty_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Gear objGear)) return;
            bool blnAddAgain;
            do
            {
                // Select the root Gear node then open the Select Gear window.
                string strGuid = string.Empty;
                if (objGear.Location != null)
                {
                    strGuid = objGear.Location.InternalId;
                }
                if (objGear.Parent is Gear parent)
                {
                    strGuid = parent.InternalId;
                }
                blnAddAgain = PickGear(objGear, null, objGear.Category == "Ammunition", objGear, objGear.DisplayNameShort(GlobalOptions.Language));
            } while (blnAddAgain);
        }

        private void cmdVehicleGearReduceQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear)) return;

            int intDecimalPlaces = 0;
            if (objGear.Name.StartsWith("Nuyen"))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectOptions.NuyenDecimals);
            }
            else if (objGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
            }

            frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces)
            {
                Minimum = 0,
                Maximum = objGear.Quantity,
                Description = LanguageManager.GetString("String_ReduceGear", GlobalOptions.Language)
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            decimal decSelectedValue = frmPickNumber.SelectedValue;

            if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_ReduceQty", GlobalOptions.Language).Replace("{0}", decSelectedValue.ToString(GlobalOptions.CultureInfo))))
                return;

            objGear.Quantity -= decSelectedValue;

            if (objGear.Quantity > 0)
            {
                objSelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else
            {
                CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);
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

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            bool blnAddAgain;

            do
            {
                frmSelectQuality frmPickQuality = new frmSelectQuality(CharacterObject);
                frmPickQuality.ShowDialog(this);

                // Don't do anything else if the form was canceled.
                if (frmPickQuality.DialogResult == DialogResult.Cancel)
                {
                    frmPickQuality.Dispose();
                    break;
                }
                blnAddAgain = frmPickQuality.AddAgain;
                bool blnFreeCost = frmPickQuality.FreeCost;

                XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmPickQuality.SelectedQuality + "\"]");
                frmPickQuality.Dispose();
                if (objXmlQuality == null)
                    continue;

                QualityType eQualityType = QualityType.Positive;
                string strTemp = string.Empty;
                if (objXmlQuality.TryGetStringFieldQuickly("category", ref strTemp))
                    eQualityType = Quality.ConvertToQualityType(strTemp);

                // Positive Metagenetic Qualities are free if you're a Changeling.
                if (CharacterObject.MetageneticLimit > 0 && objXmlQuality["metagenic"]?.InnerText == bool.TrueString)
                    blnFreeCost = true;
                // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                else if (objXmlQuality["name"]?.InnerText == "Mentor Spirit" && CharacterObject.Qualities.Any(x => x.Name == "The Beast's Way" || x.Name == "The Spiritual Way"))
                    blnFreeCost = true;

                int intQualityBP = 0;
                if (!blnFreeCost)
                {
                    objXmlQuality.TryGetInt32FieldQuickly("karma", ref intQualityBP);
                    XmlNode xmlDiscountNode = objXmlQuality["costdiscount"];
                    if (xmlDiscountNode != null && xmlDiscountNode.RequirementsMet(CharacterObject))
                    {
                        int intTemp = 0;
                        xmlDiscountNode.TryGetInt32FieldQuickly("value", ref intTemp);
                        switch (eQualityType)
                        {
                            case QualityType.Positive:
                                intQualityBP += intTemp;
                                break;
                            case QualityType.Negative:
                                intQualityBP -= intTemp;
                                break;
                        }
                    }
                }

                int intKarmaCost = intQualityBP * CharacterObjectOptions.KarmaQuality;
                if (!CharacterObjectOptions.DontDoubleQualityPurchases && objXmlQuality["doublecareer"]?.InnerText != bool.FalseString)
                    intKarmaCost *= 2;

                // Make sure the character has enough Karma to pay for the Quality.
                if (eQualityType == QualityType.Positive)
                {
                    if (!blnFreeCost)
                    {
                        if (intKarmaCost > CharacterObject.Karma)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        string strDisplayName = objXmlQuality["translate"]?.InnerText ?? objXmlQuality["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                        if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", strDisplayName)
                            .Replace("{1}", intKarmaCost.ToString())))
                            continue;
                    }
                }
                else if (MessageBox.Show(LanguageManager.GetString("Message_AddNegativeQuality", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_AddNegativeQuality", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    continue;

                List<Weapon> lstWeapons = new List<Weapon>();
                Quality objQuality = new Quality(CharacterObject);

                objQuality.Create(objXmlQuality, QualitySource.Selected, lstWeapons);
                if (objQuality.InternalId.IsEmptyGuid())
                {
                    // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                    continue;
                }

                // Make sure the character has enough Karma to pay for the Quality.
                if (objQuality.Type == QualityType.Positive)
                {
                    if (objQuality.ContributeToBP)
                    {
                        // Create the Karma expense.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseAddPositiveQuality", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objQuality.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= intKarmaCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddQuality, objQuality.InternalId);
                        objExpense.Undo = objUndo;
                    }
                }
                else
                {
                    // Create a Karma Expense for the Negative Quality.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(0, LanguageManager.GetString("String_ExpenseAddNegativeQuality", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objQuality.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddQuality, objQuality.InternalId);
                    objExpense.Undo = objUndo;
                }

                CharacterObject.Qualities.Add(objQuality);

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treQualities.SelectedNode?.Tag is Quality objSelectedQuality)
            {
                string strInternalIDToRemove = objSelectedQuality.InternalId;
                // Can't do a foreach because we're removing items, this is the next best thing
                bool blnFirstRemoval = true;
                for (int i = CharacterObject.Qualities.Count - 1; i >= 0; --i)
                {
                    Quality objLoopQuality = CharacterObject.Qualities.ElementAt(i);
                    if (objLoopQuality.InternalId == strInternalIDToRemove)
                    {
                        if (!RemoveQuality(objLoopQuality, blnFirstRemoval))
                            break;
                        blnFirstRemoval = false;
                        if (i > CharacterObject.Qualities.Count)
                        {
                            i = CharacterObject.Qualities.Count;
                        }
                    }
                }

                // Only refresh if at least one quality was removed
                if (!blnFirstRemoval)
                {
                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void cmdSwapQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            Quality objQuality = treQualities.SelectedNode?.Tag as Quality;
            if (objQuality?.InternalId.IsEmptyGuid() != false)
                return;

            // Qualities that come from a Metatype cannot be removed.
            if (objQuality.OriginSource == QualitySource.Metatype)
            {
                MessageBox.Show(LanguageManager.GetString("Message_MetavariantQualitySwap", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_MetavariantQualitySwap", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Neither can qualities from Improvements
            if (objQuality.OriginSource == QualitySource.Improvement)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ImprovementQuality", GlobalOptions.Language).Replace("{0}", objQuality.GetSourceName(GlobalOptions.Language)), LanguageManager.GetString("MessageTitle_MetavariantQuality", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectQuality frmPickQuality = new frmSelectQuality(CharacterObject)
            {
                ForceCategory = objQuality.Type.ToString(),
                IgnoreQuality = objQuality.Name
            };
            frmPickQuality.ShowDialog(this);

            // Don't do anything else if the form was canceled.
            if (frmPickQuality.DialogResult == DialogResult.Cancel)
                return;

            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmPickQuality.SelectedQuality + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Quality objNewQuality = new Quality(CharacterObject);

            if (objNewQuality.Swap(objQuality, CharacterObject, objXmlQuality))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private bool RemoveQuality(Quality objSelectedQuality, bool blnConfirmDelete = true, bool blnCompleteDelete = true)
        {
            XmlNode objXmlDeleteQuality = objSelectedQuality.GetNode();
            bool blnMetatypeQuality = false;

            // Qualities that come from a Metatype cannot be removed.
            if (objSelectedQuality.OriginSource == QualitySource.Metatype)
            {
                MessageBox.Show(LanguageManager.GetString("Message_MetavariantQuality", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_MetavariantQuality", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (objSelectedQuality.OriginSource == QualitySource.Improvement)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ImprovementQuality", GlobalOptions.Language).Replace("{0}", objSelectedQuality.GetSourceName(GlobalOptions.Language)), LanguageManager.GetString("MessageTitle_MetavariantQuality", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (objSelectedQuality.OriginSource == QualitySource.MetatypeRemovable)
            {
                // Look up the cost of the Quality.
                int intBP = 0;
                if (objSelectedQuality.Type == QualityType.Negative || objXmlDeleteQuality["refundkarmaonremove"] != null)
                {
                    intBP = Convert.ToInt32(objXmlDeleteQuality["karma"]?.InnerText) * CharacterObjectOptions.KarmaQuality;
                    if (blnCompleteDelete)
                        intBP *= objSelectedQuality.Levels;
                    if (!CharacterObjectOptions.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                    {
                        intBP *= 2;
                    }
                    if (objSelectedQuality.Type == QualityType.Positive)
                        intBP *= -1;
                }
                string strBP = intBP.ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);

                if (blnConfirmDelete && !CharacterObject.ConfirmDelete(blnCompleteDelete ?
                        LanguageManager.GetString("Message_DeleteMetatypeQuality", GlobalOptions.Language).Replace("{0}", strBP) :
                        LanguageManager.GetString("Message_LowerMetatypeQualityLevel", GlobalOptions.Language).Replace("{0}", strBP)))
                    return false;

                blnMetatypeQuality = true;
            }

            if (objSelectedQuality.Type == QualityType.Positive)
            {
                if (objXmlDeleteQuality["refundkarmaonremove"] != null)
                {
                    int intKarmaCost = objSelectedQuality.BP * CharacterObjectOptions.KarmaQuality;

                    if (!CharacterObjectOptions.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                    {
                        intKarmaCost *= 2;
                    }

                    ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                    objEntry.Create(intKarmaCost, LanguageManager.GetString("String_ExpenseSwapPositiveQuality", GlobalOptions.Language).Replace("{0}", objSelectedQuality.DisplayNameShort(GlobalOptions.Language)).Replace("{1}", LanguageManager.GetString("String_Karma", GlobalOptions.Language)), ExpenseType.Karma, DateTime.Now, true);
                    CharacterObject.ExpenseEntries.AddWithSort(objEntry);
                    CharacterObject.Karma += intKarmaCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.RemoveQuality, objSelectedQuality.QualityId);
                    objUndo.Extra = objSelectedQuality.Extra;
                    objEntry.Undo = objUndo;
                }
                else if (!blnMetatypeQuality)
                {
                    if (blnConfirmDelete && !CharacterObject.ConfirmDelete(blnCompleteDelete ?
                                                                        LanguageManager.GetString("Message_DeletePositiveQualityCareer", GlobalOptions.Language) :
                                                                        LanguageManager.GetString("Message_LowerPositiveQualityLevelCareer", GlobalOptions.Language)))
                        return false;
                }
            }
            else
            {
                // Make sure the character has enough Karma to buy off the Quality.
                int intKarmaCost = -(objSelectedQuality.BP * CharacterObjectOptions.KarmaQuality);
                if (!CharacterObjectOptions.DontDoubleQualityRefunds)
                {
                    intKarmaCost *= 2;
                }
                int intTotalKarmaCost = intKarmaCost;
                if (blnCompleteDelete)
                    intTotalKarmaCost *= objSelectedQuality.Levels;
                if (intTotalKarmaCost > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (!blnMetatypeQuality)
                {
                    if (blnConfirmDelete && !CharacterObject.ConfirmKarmaExpense((blnCompleteDelete ? LanguageManager.GetString("Message_ConfirmKarmaExpenseRemove", GlobalOptions.Language) :
                        LanguageManager.GetString("Message_ConfirmKarmaExpenseLowerLevel", GlobalOptions.Language)).Replace("{0}", objSelectedQuality.DisplayNameShort(GlobalOptions.Language)).Replace("{1}", intTotalKarmaCost.ToString())))
                        return false;
                }

                // Create the Karma expense.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(-intKarmaCost, LanguageManager.GetString("String_ExpenseRemoveNegativeQuality", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objSelectedQuality.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.RemoveQuality, objSelectedQuality.QualityId);
                objUndo.Extra = objSelectedQuality.Extra;
                objExpense.Undo = objUndo;
            }

            // Remove the Improvements that were created by the Quality.
            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objSelectedQuality.InternalId);
            // Remove any Critter Powers that are gained through the Quality (Infected).
            if (objXmlDeleteQuality.SelectNodes("powers/power")?.Count > 0)
            {
                using (XmlNodeList xmlPowerList = XmlManager.Load("critterpowers.xml").SelectNodes("optionalpowers/optionalpower"))
                    if (xmlPowerList?.Count > 0)
                        foreach (XmlNode objXmlPower in xmlPowerList)
                        {
                            string strExtra = objXmlPower.Attributes?["select"]?.InnerText;

                            foreach (CritterPower objPower in CharacterObject.CritterPowers)
                            {
                                if (objPower.Name == objXmlPower.InnerText && objPower.Extra == strExtra)
                                {
                                    // Remove any Improvements created by the Critter Power.
                                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId);

                                    // Remove the Critter Power from the character.
                                    CharacterObject.CritterPowers.Remove(objPower);
                                    break;
                                }
                            }
                        }
            }

            // Remove any Weapons created by the Quality if applicable.
            if (!objSelectedQuality.WeaponID.IsEmptyGuid())
            {
                List<Weapon> lstWeapons = CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objSelectedQuality.InternalId).ToList();
                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.DeleteWeapon();
                    // We can remove here because lstWeapons is separate from the Weapons that were yielded through DeepWhere
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else
                        CharacterObject.Weapons.Remove(objWeapon);
                }
            }

            // Fix for legacy characters with old addqualities improvements.
            RemoveAddedQualities(objXmlDeleteQuality.SelectNodes("addqualities/addquality"));

            CharacterObject.Qualities.Remove(objSelectedQuality);
            return true;
        }

        private void UpdateQualityLevelValue(Quality objSelectedQuality = null)
        {
            if (objSelectedQuality == null || objSelectedQuality.OriginSource == QualitySource.Improvement || objSelectedQuality.OriginSource == QualitySource.Metatype)
            {
                nudQualityLevel.Value = 1;
                nudQualityLevel.Enabled = false;
                return;
            }
            XmlNode objQualityNode = objSelectedQuality.GetNode();
            string strLimitString = objQualityNode?["limit"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(strLimitString) && objQualityNode["nolevels"] == null && int.TryParse(strLimitString, out int intMaxRating))
            {
                nudQualityLevel.Maximum = intMaxRating;
                nudQualityLevel.Value = objSelectedQuality.Levels;
                nudQualityLevel.Enabled = true;
            }
            else
            {
                nudQualityLevel.Value = 1;
                nudQualityLevel.Enabled = false;
            }
        }

        private void nudQualityLevel_ValueChanged(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treQualities.SelectedNode?.Tag is Quality objSelectedQuality)
            {
                int intCurrentLevels = objSelectedQuality.Levels;

                // Adding a new level
                for (; nudQualityLevel.Value > intCurrentLevels; ++intCurrentLevels)
                {
                    XmlNode objXmlSelectedQuality = objSelectedQuality.GetNode();
                    if (!objXmlSelectedQuality.RequirementsMet(CharacterObject, LanguageManager.GetString("String_Quality", GlobalOptions.Language)))
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }

                    if (objXmlSelectedQuality == null)
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }

                    bool blnFreeCost = objSelectedQuality.BP == 0 || !objSelectedQuality.ContributeToBP;

                    QualityType eQualityType = objSelectedQuality.Type;

                    int intQualityBP = 0;
                    if (!blnFreeCost)
                    {
                        objXmlSelectedQuality.TryGetInt32FieldQuickly("karma", ref intQualityBP);
                        XmlNode xmlDiscountNode = objXmlSelectedQuality["costdiscount"];
                        if (xmlDiscountNode != null && xmlDiscountNode.RequirementsMet(CharacterObject))
                        {
                            int intTemp = 0;
                            xmlDiscountNode.TryGetInt32FieldQuickly("value", ref intTemp);
                            switch (eQualityType)
                            {
                                case QualityType.Positive:
                                    intQualityBP += intTemp;
                                    break;
                                case QualityType.Negative:
                                    intQualityBP -= intTemp;
                                    break;
                            }
                        }
                    }

                    int intKarmaCost = intQualityBP * CharacterObjectOptions.KarmaQuality;
                    if (!CharacterObjectOptions.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                        intKarmaCost *= 2;

                    // Make sure the character has enough Karma to pay for the Quality.
                    if (eQualityType == QualityType.Positive)
                    {
                        if (!blnFreeCost)
                        {
                            if (intKarmaCost > CharacterObject.Karma)
                            {
                                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                {
                                    UpdateQualityLevelValue(objSelectedQuality);
                                    break;
                                }
                            }

                            string strDisplayName = objXmlSelectedQuality["translate"]?.InnerText ?? objXmlSelectedQuality["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", strDisplayName)
                                .Replace("{1}", intKarmaCost.ToString())))
                            {
                                UpdateQualityLevelValue(objSelectedQuality);
                                break;
                            }
                        }
                    }
                    else if (MessageBox.Show(LanguageManager.GetString("Message_AddNegativeQuality", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_AddNegativeQuality", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }

                    List<Weapon> lstWeapons = new List<Weapon>();
                    Quality objQuality = new Quality(CharacterObject);

                    objQuality.Create(objXmlSelectedQuality, QualitySource.Selected, lstWeapons, objSelectedQuality.Extra);
                    if (objQuality.InternalId.IsEmptyGuid())
                    {
                        // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }

                    objQuality.BP = objSelectedQuality.BP;
                    objQuality.ContributeToLimit = objSelectedQuality.ContributeToLimit;

                    // Make sure the character has enough Karma to pay for the Quality.
                    if (objQuality.Type == QualityType.Positive)
                    {
                        // Create the Karma expense.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseAddPositiveQuality", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objQuality.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= intKarmaCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddQuality, objQuality.InternalId);
                        objExpense.Undo = objUndo;
                    }
                    else
                    {
                        // Create a Karma Expense for the Negative Quality.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(0, LanguageManager.GetString("String_ExpenseAddNegativeQuality", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objQuality.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddQuality, objQuality.InternalId);
                        objExpense.Undo = objUndo;
                    }
                    
                    // Add the Quality to the appropriate parent node.
                    CharacterObject.Qualities.Add(objQuality);

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
                // Removing a level
                for (; nudQualityLevel.Value < intCurrentLevels; --intCurrentLevels)
                {
                    Quality objInvisibleQuality = CharacterObject.Qualities.FirstOrDefault(x => x.QualityId == objSelectedQuality.QualityId && x.Extra == objSelectedQuality.Extra && x.SourceName == objSelectedQuality.SourceName && x.InternalId != objSelectedQuality.InternalId);
                    if (objInvisibleQuality != null && RemoveQuality(objInvisibleQuality, false, false))
                    {
                        IsCharacterUpdateRequested = true;

                        IsDirty = true;
                    }
                    else if (RemoveQuality(objSelectedQuality, false, false))
                    {
                        IsCharacterUpdateRequested = true;

                        IsDirty = true;
                        break;
                    }
                    else
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }
                }
            }
        }

        private void cmdAddLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Gear Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;

            string strLocation = frmPickText.SelectedValue;
            Location objLocation = new Location(CharacterObject, CharacterObject.GearLocations);
            objLocation.Name = strLocation;

            IsDirty = true;
        }

        private void cmdAddWeaponLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Gear Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;

            string strLocation = frmPickText.SelectedValue;
            Location objLocation = new Location(CharacterObject, CharacterObject.WeaponLocations);
            objLocation.Name = strLocation;

            IsDirty = true;
        }

        private void cmdAddWeek_Click(object sender, EventArgs e)
        {
            CalendarWeek objWeek = new CalendarWeek();
            CalendarWeek objLastWeek = CharacterObject.Calendar?.FirstOrDefault();
            if (objLastWeek != null)
            {
                objWeek.Year = objLastWeek.Year;
                objWeek.Week = objLastWeek.Week + 1;
                if (objWeek.Week > 52)
                {
                    objWeek.Week = 1;
                    objWeek.Year += 1;
                }
            }
            else
            {
                frmSelectCalendarStart frmPickStart = new frmSelectCalendarStart();
                frmPickStart.ShowDialog(this);

                if (frmPickStart.DialogResult == DialogResult.Cancel)
                    return;

                objWeek.Year = frmPickStart.SelectedYear;
                objWeek.Week = frmPickStart.SelectedWeek;
            }

            CharacterObject.Calendar.AddWithSort(objWeek, true);
            
            IsDirty = true;
        }


        private void cmdDeleteWeek_Click(object sender, EventArgs e)
        {
            if (lstCalendar == null || lstCalendar.SelectedItems.Count == 0)
            {
                return;
            }

            string strWeekId = lstCalendar.SelectedItems[0].SubItems[2].Text;

            CalendarWeek objCharacterWeek = CharacterObject.Calendar.FirstOrDefault(x => x.InternalId == strWeekId);

            if (objCharacterWeek != null)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteCalendarWeek", GlobalOptions.Language)))
                    return;

                CharacterObject.Calendar.Remove(objCharacterWeek);
                IsDirty = true;
            }
        }

        private void cmdEditWeek_Click(object sender, EventArgs e)
        {
            if (lstCalendar == null || lstCalendar.SelectedItems.Count == 0)
            {
                return;
            }

            string strWeekId = lstCalendar.SelectedItems[0].SubItems[2].Text;

            CalendarWeek objWeek = CharacterObject.Calendar.FirstOrDefault(x => x.InternalId == strWeekId);

            if (objWeek != null)
            {
                string strOldValue = objWeek.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objWeek.Notes = frmItemNotes.Notes;
                    if (objWeek.Notes != strOldValue)
                    {
                        IsDirty = true;
                    }
                }
            }
        }

        private void cmdChangeStartWeek_Click(object sender, EventArgs e)
        {
            // Find the first date.
            CalendarWeek objStart = CharacterObject.Calendar?.LastOrDefault();
            if (objStart == null)
            {
                return;
            }

            frmSelectCalendarStart frmPickStart = new frmSelectCalendarStart(objStart);
            frmPickStart.ShowDialog(this);

            if (frmPickStart.DialogResult == DialogResult.Cancel)
                return;

            // Determine the difference between the starting value and selected values for year and week.
            int intYear = frmPickStart.SelectedYear;
            int intWeek = frmPickStart.SelectedWeek;
            int intYearDiff = intYear - objStart.Year;
            int intWeekDiff = intWeek - objStart.Week;

            // Update each of the CalendarWeek entries for the character.
            foreach (CalendarWeek objWeek in CharacterObject.Calendar)
            {
                objWeek.Week += intWeekDiff;
                objWeek.Year += intYearDiff;

                // If the date range goes outside of 52 weeks, increase or decrease the year as necessary.
                if (objWeek.Week < 1)
                {
                    objWeek.Year -= 1;
                    objWeek.Week += 52;
                }
                if (objWeek.Week > 52)
                {
                    objWeek.Year += 1;
                    objWeek.Week -= 52;
                }
            }

            IsDirty = true;
        }

        private void cmdAddImprovement_Click(object sender, EventArgs e)
        {
            frmCreateImprovement frmPickImprovement = new frmCreateImprovement(CharacterObject);
            frmPickImprovement.ShowDialog(this);

            if (frmPickImprovement.DialogResult == DialogResult.Cancel)
                return;
            TreeNode newNode = treImprovements.FindNode(frmPickImprovement.NewImprovement.InternalId);

            if (newNode != null)
            {
                newNode.Text = frmPickImprovement.NewImprovement.CustomName;
                newNode.ForeColor = frmPickImprovement.NewImprovement.PreferredColor;
                newNode.ToolTipText = frmPickImprovement.NewImprovement.Notes;
            }
            else {Utils.BreakIfDebug();}
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdCreateStackedFocus_Click(object sender, EventArgs e)
        {
            int intFree = 0;
            List<Gear> lstGear = new List<Gear>();
            List<Gear> lstStack = new List<Gear>();

            // Run through all of the Foci the character has and count the un-Bonded ones.
            foreach (Gear objGear in CharacterObject.Gear)
            {
                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci")
                {
                    if (!objGear.Bonded)
                    {
                        intFree++;
                        lstGear.Add(objGear);
                    }
                }
            }

            // If the character does not have at least 2 un-Bonded Foci, display an error and leave.
            if (intFree < 2)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotStackFoci", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotStackFoci", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectItem frmPickItem = new frmSelectItem();

            // Let the character select the Foci they'd like to stack, stopping when they either click Cancel or there are no more items left in the list.
            do
            {
                frmPickItem.Gear = lstGear;
                frmPickItem.AllowAutoSelect = false;
                frmPickItem.Description = LanguageManager.GetString("String_SelectItemFocus", GlobalOptions.Language);
                frmPickItem.ShowDialog(this);

                if (frmPickItem.DialogResult == DialogResult.OK)
                {
                    // Move the item from the Gear list to the Stack list.
                    foreach (Gear objGear in lstGear)
                    {
                        if (objGear.InternalId == frmPickItem.SelectedItem)
                        {
                            objGear.Bonded = true;
                            lstStack.Add(objGear);
                            lstGear.Remove(objGear);
                            break;
                        }
                    }
                }
            } while (lstGear.Count > 0 && frmPickItem.DialogResult != DialogResult.Cancel);

            // Make sure at least 2 Foci were selected.
            if (lstStack.Count < 2)
            {
                MessageBox.Show(LanguageManager.GetString("Message_StackedFocusMinimum", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotStackFoci", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the combined Force of the Foci do not exceed 6.
            if (!CharacterObjectOptions.AllowHigherStackedFoci)
            {
                int intCombined = 0;
                foreach (Gear objGear in lstStack)
                    intCombined += objGear.Rating;
                if (intCombined > 6)
                {
                    foreach (Gear objGear in lstStack)
                        objGear.Bonded = false;
                    MessageBox.Show(LanguageManager.GetString("Message_StackedFocusForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotStackFoci", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // Create the Stacked Focus.
            StackedFocus objStack = new StackedFocus(CharacterObject);
            foreach (Gear objGear in lstStack)
                objStack.Gear.Add(objGear);
            CharacterObject.StackedFoci.Add(objStack);

            // Remove the Gear from the character and replace it with a Stacked Focus item.
            decimal decCost = 0.0m;
            foreach (Gear objGear in lstStack)
            {
                decCost += objGear.TotalCost;
                CharacterObject.Gear.Remove(objGear);
            }

            Gear objStackItem = new Gear(CharacterObject)
            {
                Category = "Stacked Focus",
                Name = "Stacked Focus: " + objStack.Name(GlobalOptions.CultureInfo, GlobalOptions.Language),
                MinRating = 0,
                MaxRating = 0,
                Source = "SR5",
                Page = "1",
                Cost = decCost.ToString(GlobalOptions.CultureInfo),
                Avail = "0"
            };

            CharacterObject.Gear.Add(objStackItem);

            objStack.GearId = objStackItem.InternalId;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdBurnStreetCred_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(LanguageManager.GetString("Message_BurnStreetCred", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_BurnStreetCred", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            CharacterObject.BurntStreetCred += 2;
        }

        private void cmdEditImprovement_Click(object sender, EventArgs e)
        {
            treImprovements_DoubleClick(sender, e);
        }

        private void cmdDeleteImprovement_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectedImprovement = treImprovements.SelectedNode;
            if (treImprovements.SelectedNode?.Tag is Improvement objImprovement)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteImprovement", GlobalOptions.Language)))
                    return;

                // Remove the Improvement from the character.
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Custom, objImprovement.SourceName);

                IsCharacterUpdateRequested = true;
            }
            else if (treImprovements.SelectedNode.Level > 0 && treImprovements.SelectedNode?.Tag is string strSelectedId)
            {
                if (strSelectedId == "Node_SelectedImprovements")
                    return;

                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteImprovementGroup", GlobalOptions.Language)))
                    return;

                foreach (Improvement imp in CharacterObject.Improvements)
                {
                    if (imp.CustomGroup == strSelectedId)
                    {
                        imp.CustomGroup = string.Empty;
                    }
                }

                // Remove the Group from the character, then remove the selected node.
                CharacterObject.ImprovementGroups.Remove(strSelectedId);
            }
            IsDirty = true;
        }

        private void cmdAddArmorBundle_Click(object sender, EventArgs e)
        {
            // Add a new location to the Armor Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;

            string strLocation = frmPickText.SelectedValue;
            Location objLocation = new Location(CharacterObject, CharacterObject.ArmorLocations);
            objLocation.Name = strLocation;

            IsDirty = true;
        }

        private void cmdArmorEquipAll_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is Location selectedLocation)
            {
                // Equip all of the Armor in the Armor Bundle.
                foreach (Armor objArmor in selectedLocation.Children)
                {
                    if (objArmor.Location == selectedLocation)
                    {
                        objArmor.Equipped = true;
                    }
                }
            }
            else if (treArmor.SelectedNode?.Tag.ToString() == "Node_SelectedArmor")
            {
                foreach (Armor objArmor in CharacterObject.Armor.Where(objArmor =>
                    objArmor.Equipped == false && objArmor.Location == null))
                {
                    objArmor.Equipped = true;
                }
            }
            else
            {
                return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdArmorUnEquipAll_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is Location selectedLocation)
            {
                // Equip all of the Armor in the Armor Bundle.
                foreach (Armor objArmor in selectedLocation.Children)
                {
                    if (objArmor.Location == selectedLocation)
                    {
                        objArmor.Equipped = false;
                    }
                }
            }
            else if (treArmor.SelectedNode?.Tag.ToString() == "Node_SelectedArmor")
            {
                foreach (Armor objArmor in CharacterObject.Armor.Where(objArmor =>
                    objArmor.Equipped == true && objArmor.Location == null))
                {
                    objArmor.Equipped = false;
                }
            }
            else
            {
                return;

            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdImprovementsEnableAll_Click(object sender, EventArgs e)
        {
            // Enable all of the Improvements in the Improvement Group.
            if (!(treImprovements.SelectedNode?.Tag is string strSelectedId)) return;
            List<Improvement> lstImprovementsEnabled = new List<Improvement>();
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if (!objImprovement.Enabled && (objImprovement.CustomGroup == strSelectedId || (strSelectedId == "Node_SelectedImprovements" && string.IsNullOrEmpty(objImprovement.CustomGroup))))
                {
                    lstImprovementsEnabled.Add(objImprovement);
                }
            }
            if (lstImprovementsEnabled.Count > 0)
            {
                ImprovementManager.EnableImprovements(CharacterObject, lstImprovementsEnabled);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdImprovementsDisableAll_Click(object sender, EventArgs e)
        {
            if (!(treImprovements.SelectedNode?.Tag is string strSelectedId)) return;
            // Disable all of the Improvements in the Improvement Group.
            List<Improvement> lstImprovementsDisabled = new List<Improvement>();
            foreach (Improvement objImprovement in CharacterObject.Improvements.Where(objImprovement =>
                objImprovement.Custom && objImprovement.Enabled))
            {
                if ((objImprovement.CustomGroup == strSelectedId ||
                     (strSelectedId == "Node_SelectedImprovements" &&
                      string.IsNullOrEmpty(objImprovement.CustomGroup))))
                {
                    lstImprovementsDisabled.Add(objImprovement);
                }
            }

            if (lstImprovementsDisabled.Count > 0)
            {
                ImprovementManager.DisableImprovements(CharacterObject, lstImprovementsDisabled);
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdRollSpell_Click(object sender, EventArgs e)
        {
            int.TryParse(lblSpellDicePool.Text, out int intDice);
            DiceRollerOpenedInt(CharacterObject, intDice);
        }

        private void cmdRollDrain_Click(object sender, EventArgs e)
        {
            int.TryParse(lblDrainAttributesValue.Text, out int intDice);
            DiceRollerOpenedInt(CharacterObject, intDice);
        }

        private void cmdRollFading_Click(object sender, EventArgs e)
        {
            int.TryParse(lblFadingAttributesValue.Text, out int intDice);
            DiceRollerOpenedInt(CharacterObject, intDice);
        }

        private void cmdRollWeapon_Click(object sender, EventArgs e)
        {
            int.TryParse(lblWeaponDicePool.Text, out int intDice);
            DiceRollerOpenedInt(CharacterObject, intDice);
        }

        private void cmdRollVehicleWeapon_Click(object sender, EventArgs e)
        {
            int.TryParse(lblVehicleWeaponDicePool.Text, out int intDice);
            DiceRollerOpenedInt(CharacterObject, intDice);
        }

        private void cmdAddVehicleLocation_Click(object sender, EventArgs e)
        {
            // Make sure a Vehicle is selected.
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleLocation", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;
            Location objLocation = new Location(CharacterObject, objVehicle.Locations, frmPickText.SelectedValue);

            IsDirty = true;
        }
        
        private void cmdQuickenSpell_Click(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode == null || treSpells.SelectedNode.Level != 1)
                return;

            frmSelectNumber frmPickNumber = new frmSelectNumber(0)
            {
                Description = LanguageManager.GetString("String_QuickeningKarma", GlobalOptions.Language).Replace("{0}", treSpells.SelectedNode.Text),
                Minimum = 1
            };
            frmPickNumber.ShowDialog(this);

            if (frmPickNumber.DialogResult == DialogResult.Cancel)
                return;

            // Make sure the character has enough Karma to improve the CharacterAttribute.
            int intKarmaCost = decimal.ToInt32(frmPickNumber.SelectedValue);
            if (intKarmaCost > CharacterObject.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseQuickeningMetamagic", GlobalOptions.Language).Replace("{0}", intKarmaCost.ToString()).Replace("{1}", treSpells.SelectedNode.Text)))
                return;

            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_ExpenseQuickenMetamagic", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + treSpells.SelectedNode.Text, ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.QuickeningMetamagic, string.Empty);
            objExpense.Undo = objUndo;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endregion

#region ContextMenu Events
        private void InitiationContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Enable and disable menu items
            if (treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade)
            {
                int intGrade = objGrade.Grade;
                bool blnHasArt = CharacterObject.Arts.Any(art => art.Grade == intGrade);
                bool blnHasBonus = CharacterObject.Metamagics.Any(bonus => bonus.Grade == intGrade) || CharacterObject.Spells.Any(spell => spell.Grade == intGrade);
                tsMetamagicAddArt.Enabled = !blnHasArt;
                tsMetamagicAddMetamagic.Enabled = !blnHasBonus;
            }

        }
        
        private void tsCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objCyberware))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickCyberware(objCyberware, objCyberware.SourceType);
            }
            while (blnAddAgain);
        }

        private void tsVehicleCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickCyberware(objCyberware, objCyberware.SourceType);
            }
            while (blnAddAgain);
        }

        private void tsWeaponAddAccessory_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponAccessory", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Accessories cannot be added to Cyberweapons.
            if (objWeapon.Cyberware)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberweaponNoAccessory", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberweaponNoAccessory", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            XmlNode objXmlWeapon = objWeapon.GetNode();
            if (objXmlWeapon == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotFindWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotModifyWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                // Make sure the Weapon allows Accessories to be added to it.
                if (!objWeapon.AllowAccessory)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotModifyWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotModifyWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }

                frmSelectWeaponAccessory frmPickWeaponAccessory = new frmSelectWeaponAccessory(CharacterObject)
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

                WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
                objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, Convert.ToInt32(frmPickWeaponAccessory.SelectedRating));
                objAccessory.Parent = objWeapon;

                if (objAccessory.Cost.StartsWith("Variable("))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    string strCost = objAccessory.Cost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMin != 0 || decMax != decimal.MaxValue)
                    {
                        frmSelectNumber frmPickNumber = new frmSelectNumber(CharacterObjectOptions.NuyenDecimals);
                        if (decMax > 1000000)
                            decMax = 1000000;
                        frmPickNumber.Minimum = decMin;
                        frmPickNumber.Maximum = decMax;
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language).Replace("{0}", objAccessory.DisplayNameShort(GlobalOptions.Language));
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        objAccessory.Cost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }

                // Check the item's Cost and make sure the character can afford it.
                decimal decOriginalCost = objWeapon.TotalCost;
                objWeapon.WeaponAccessories.Add(objAccessory);

                decimal decCost = objWeapon.TotalCost - decOriginalCost;
                // Apply a markup if applicable.
                if (frmPickWeaponAccessory.Markup != 0)
                {
                    decCost *= 1 + (frmPickWeaponAccessory.Markup / 100.0m);
                }

                // Multiply the cost if applicable.
                char chrAvail = objAccessory.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                if (!frmPickWeaponAccessory.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objWeapon.WeaponAccessories.Remove(objAccessory);
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);

                        frmPickWeaponAccessory.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponAccessory", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objAccessory.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponAccessory, objAccessory.InternalId);
                    objExpense.Undo = objUndo;
                }
                
                IsCharacterUpdateRequested = true;

                IsDirty = true;
                
                frmPickWeaponAccessory.Dispose();
            }
            while (blnAddAgain);
        }

        private bool PickArmor(Location objLocation = null)
        {
            frmSelectArmor frmPickArmor = new frmSelectArmor(CharacterObject);
            frmPickArmor.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickArmor.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Armor XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("armor.xml");

            XmlNode objXmlArmor = objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" + frmPickArmor.SelectedArmor + "\"]");

            Armor objArmor = new Armor(CharacterObject);
            List<Weapon> lstWeapons = new List<Weapon>();
            objArmor.Create(objXmlArmor, frmPickArmor.Rating, lstWeapons);
            objArmor.DiscountCost = frmPickArmor.BlackMarketDiscount;
            objArmor.Location = objLocation;

            if (objArmor.InternalId.IsEmptyGuid())
                return frmPickArmor.AddAgain;

            decimal decCost = objArmor.TotalCost;
            // Apply a markup if applicable.
            if (frmPickArmor.Markup != 0)
            {
                decCost *= 1 + (frmPickArmor.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objArmor.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickArmor.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Remove the Improvements created by the Armor.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Armor, objArmor.InternalId);

                    return frmPickArmor.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseArmor", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objArmor.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddArmor, objArmor.InternalId);
                objExpense.Undo = objUndo;
            }

            CharacterObject.Armor.Add(objArmor);
            
            foreach (Weapon objWeapon in lstWeapons)
            {
                CharacterObject.Weapons.Add(objWeapon);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickArmor.AddAgain;
        }

        private void tsArmorLocationAddArmor_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Location objSelectedLocation)) return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmor(objSelectedLocation);
            }
            while (blnAddAgain);
        }

        private void tsAddArmorMod_Click(object sender, EventArgs e)
        {
            while (treArmor.SelectedNode != null && treArmor.SelectedNode.Level > 1)
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;

            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Armor XML file and locate the selected Armor.
            XmlDocument objXmlDocument = XmlManager.Load("armor.xml");

            bool blnAddAgain;
            do
            {
                XmlNode objXmlArmor = objArmor.GetNode();

                frmSelectArmorMod frmPickArmorMod = new frmSelectArmorMod(CharacterObject)
                {
                    ArmorCost = objArmor.OwnCost,
                    ArmorCapacity = Convert.ToDecimal(objArmor.CalculatedCapacity, GlobalOptions.CultureInfo),
                    AllowedCategories = objArmor.Category + "," + objArmor.Name,
                    CapacityDisplayStyle = objArmor.CapacityDisplayStyle
                };
                XmlNode xmlAddModCategory = objXmlArmor["addmodcategory"];
                if (xmlAddModCategory != null)
                    frmPickArmorMod.AllowedCategories += "," + xmlAddModCategory.InnerText;

                frmPickArmorMod.ShowDialog(this);

                if (frmPickArmorMod.DialogResult == DialogResult.Cancel)
                {
                    frmPickArmorMod.Dispose();
                    break;
                }
                blnAddAgain = frmPickArmorMod.AddAgain;

                // Locate the selected piece.
                objXmlArmor = objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = \"" + frmPickArmorMod.SelectedArmorMod + "\"]");

                ArmorMod objMod = new ArmorMod(CharacterObject);
                List<Weapon> lstWeapons = new List<Weapon>();
                int intRating = 0;
                if (Convert.ToInt32(objXmlArmor["maxrating"].InnerText) > 1)
                    intRating = frmPickArmorMod.SelectedRating;

                objMod.Create(objXmlArmor, intRating, lstWeapons);
                if (objMod.InternalId.IsEmptyGuid())
                {
                    frmPickArmorMod.Dispose();
                    continue;
                }

                // Check the item's Cost and make sure the character can afford it.
                decimal decOriginalCost = objArmor.TotalCost;
                objArmor.ArmorMods.Add(objMod);

                // Do not allow the user to add a new piece of Armor if its Capacity has been reached.
                if (CharacterObjectOptions.EnforceCapacity && objArmor.CapacityRemaining < 0)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    objArmor.ArmorMods.Remove(objMod);
                    frmPickArmorMod.Dispose();
                    continue;
                }

                decimal decCost = objArmor.TotalCost - decOriginalCost;
                // Apply a markup if applicable.
                if (frmPickArmorMod.Markup != 0)
                {
                    decCost *= 1 + (frmPickArmorMod.Markup / 100.0m);
                }

                // Multiply the cost if applicable.
                char chrAvail = objMod.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                if (!frmPickArmorMod.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objArmor.ArmorMods.Remove(objMod);
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Remove the Improvements created by the Armor Mod.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                        frmPickArmorMod.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseArmorMod", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddArmorMod, objMod.InternalId);
                    objExpense.Undo = objUndo;
                }
                
                // Add any Weapons created by the Mod.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }
                
                IsCharacterUpdateRequested = true;

                IsDirty = true;
                
                frmPickArmorMod.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treGear.SelectedNode?.Tag is IHasChildren<Gear> iParent))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(iParent);
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
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");

            bool blnAddAgain;

            do
            {
                frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(CharacterObject, objVehicle.Mods)
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

                VehicleMod objMod = new VehicleMod(CharacterObject)
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
                if (CharacterObjectOptions.EnforceCapacity)
                {
                    bool blnOverCapacity = false;
                    if (CharacterObjectOptions.BookEnabled("R5"))
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
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objVehicle.Mods.Remove(objMod);
                        frmPickVehicleMod.Dispose();
                        continue;
                    }
                }

                decimal decCost = objVehicle.TotalCost - decOriginalCost;

                // Multiply the cost if applicable.
                char chrAvail = objMod.TotalAvailTuple().Suffix;
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                if (decCost > CharacterObject.Nuyen)
                {
                    objVehicle.Mods.Remove(objMod);
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmPickVehicleMod.Dispose();
                    continue;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleMod", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objMod.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddVehicleMod, objMod.InternalId);
                objExpense.Undo = objUndo;

                // Check for Improved Sensor bonus.
                if (objMod.Bonus?["improvesensor"] != null)
                {
                    objVehicle.ChangeVehicleSensor(treVehicles, true, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;

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
                objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out objVehicle);
                if (objWeaponMount == null)
                {
                    objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out objVehicle, out objWeaponMount);
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
                MessageBox.Show(LanguageManager.GetString("Message_CannotAddWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotAddWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject)
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
            Weapon objWeapon = new Weapon(CharacterObject)
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
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            if (!frmPickWeapon.FreeCost)
            {
                // Check the item's Cost and make sure the character can afford it.
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return frmPickWeapon.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeapon", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objWeapon.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

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

            IsCharacterUpdateRequested = true;

            IsDirty = true;
            return frmPickWeapon.AddAgain;
        }

        private void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle)) return;
            frmCreateWeaponMount frmPickVehicleMod = new frmCreateWeaponMount(objVehicle, CharacterObject)
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
                    if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                        decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                    if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                        decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                    if (decCost > CharacterObject.Nuyen)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeaponAccessory", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + frmPickVehicleMod.WeaponMount.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMount, frmPickVehicleMod.WeaponMount.InternalId);
                    objExpense.Undo = objUndo;
                }

                objVehicle.WeaponMounts.Add(frmPickVehicleMod.WeaponMount);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void tsVehicleAddWeaponAccessory_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleWeaponAccessories", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_VehicleWeaponAccessories", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            XmlNode objXmlWeapon = objWeapon.GetNode();
            if (objXmlWeapon == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotFindWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotModifyWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                // Make sure the Weapon allows Accessories to be added to it.
                if (!objWeapon.AllowAccessory)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotModifyWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotModifyWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                frmSelectWeaponAccessory frmPickWeaponAccessory = new frmSelectWeaponAccessory(CharacterObject)
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

                WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                if (!frmPickWeaponAccessory.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objWeapon.WeaponAccessories.Remove(objAccessory);
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);

                        frmPickWeaponAccessory.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleWeaponAccessory", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objAccessory.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponAccessory, objAccessory.InternalId);
                    objExpense.Undo = objUndo;
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;

                frmPickWeaponAccessory.Dispose();
            }
            while (blnAddAgain);
        }

        private bool AddUnderbarrelWeapon(Weapon objSelectedWeapon, string strExpenseString)
        {
            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject)
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
            Weapon objWeapon = new Weapon(CharacterObject)
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
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickWeapon.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickWeapon.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, strExpenseString + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objWeapon.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

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

            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickWeapon.AddAgain;
        }

        private void tsVehicleAddUnderbarrelWeapon_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleWeaponUnderbarrel", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_VehicleWeaponUnderbarrel", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = AddUnderbarrelWeapon(objWeapon, LanguageManager.GetString("String_ExpensePurchaseVehicleWeapon", GlobalOptions.Language));
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddWeaponAccessoryAlt_Click(object sender, EventArgs e)
        {
            tsVehicleAddWeaponAccessory_Click(sender, e);
        }

        private void tsVehicleAddUnderbarrelWeaponAlt_Click(object sender, EventArgs e)
        {
            tsVehicleAddUnderbarrelWeapon_Click(sender, e);
        }

        private void tsMartialArtsAddAdvantage_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode != null)
            {
                // Select the Martial Arts node if we're currently on a child.
                if (treMartialArts.SelectedNode.Level > 1)
                    treMartialArts.SelectedNode = treMartialArts.SelectedNode.Parent;

                if (!(treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt)) return;
                frmSelectMartialArtTechnique frmPickMartialArtTechnique = new frmSelectMartialArtTechnique(CharacterObject, objMartialArt);
                frmPickMartialArtTechnique.ShowDialog(this);

                if (frmPickMartialArtTechnique.DialogResult == DialogResult.Cancel)
                    return;

                // Open the Martial Arts XML file and locate the selected piece.
                XmlNode xmlTechnique = XmlManager.Load("martialarts.xml").SelectSingleNode("/chummer/techniques/technique[id = \"" + frmPickMartialArtTechnique.SelectedTechnique + "\"]");

                if (xmlTechnique != null)
                {
                    // Create the Improvements for the Advantage if there are any.
                    MartialArtTechnique objAdvantage = new MartialArtTechnique(CharacterObject);
                    objAdvantage.Create(xmlTechnique);
                    if (objAdvantage.InternalId.IsEmptyGuid())
                        return;

                    int karmaCost = objMartialArt.Techniques.Count > 0 ? CharacterObjectOptions.KarmaManeuver : 0;
                            objMartialArt.Techniques.Add(objAdvantage);

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                        objEntry.Create(karmaCost * -1, LanguageManager.GetString("String_ExpenseLearnTechnique", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objAdvantage.DisplayName(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objEntry);
                        CharacterObject.Karma -= karmaCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddMartialArtManeuver, objAdvantage.InternalId);
                    objEntry.Undo = objUndo;

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectMartialArtTechnique", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectMartialArtTechnique", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void tsVehicleAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Locate the selected Vehicle.
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objSelectedVehicle))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGearVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSelectedVehicle.GetNode());
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

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

                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        objWeapon.ParentVehicle = objSelectedVehicle;
                        objSelectedVehicle.Weapons.Add(objWeapon);
                    }
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleSensorAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ModifyVehicleGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlSensorGear, strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                if (lstWeapons.Count > 0)
                {
                    CharacterObject.Vehicles.FindVehicleGear(objSensor.InternalId, out Vehicle objVehicle,
                        out WeaponAccessory _, out Cyberware _);
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        objVehicle.Weapons.Add(objWeapon);
                    }
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            tsVehicleSensorAddAsPlugin_Click(sender, e);
        }
        private void cmsAmmoSingleShot_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;
            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            objWeapon.AmmoRemaining -= 1;
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsAmmoShortBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsAmmoLongBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining > 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoLongBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining == 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurst", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsAmmoFullBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.FullBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.FullBurst;
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoFullBurst", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsAmmoSuppressiveFire_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.Suppressive)
            {
                objWeapon.AmmoRemaining -= objWeapon.Suppressive;
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSuppressiveFire", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsVehicleAmmoSingleShot_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            objWeapon.AmmoRemaining -= 1;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsVehicleAmmoShortBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsVehicleAmmoLongBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSingleShot", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining > 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoLongBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else if (objWeapon.AmmoRemaining == 3)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurst", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsVehicleAmmoFullBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.FullBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.FullBurst;
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoFullBurst", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void cmsVehicleAmmoSuppressiveFire_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;

            if (objWeapon.AmmoRemaining == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmo", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.Suppressive)
            {
                objWeapon.AmmoRemaining -= objWeapon.Suppressive;
            }
            else
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughAmmoSuppressiveFire", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsDirty = true;
        }

        private void tsCyberwareSell_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (objCyberware.Capacity == "[*]" && treCyberware.SelectedNode.Level == 2)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;
                objCyberware.Sell(CharacterObject, frmSell.SellPercent);

                IncreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100));
            }
            else if (treCyberware.SelectedNode?.Tag is Gear objGear)
            {
                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;

                        // Create the Expense Log Entry for the sale.
                        decimal decAmount = objGear.TotalCost * frmSell.SellPercent;
                        decAmount += objGear.DeleteGear() * frmSell.SellPercent;
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        string strEntry = LanguageManager.GetString("String_ExpenseSoldCyberwareGear", GlobalOptions.Language);
                        objExpense.Create(decAmount, strEntry + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Nuyen += decAmount;

                if (objGear.Parent is Gear objParent)
                    objParent.Children.Remove(objGear);
                else
                {
                    CharacterObject.Cyberware.FindCyberwareGear(objGear.InternalId, out objCyberware);
                    objCyberware.Gear.Remove(objGear);
                }
            }
            
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void tsArmorSell_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is ICanSell selectedObject)
            {
                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;

                selectedObject.Sell(CharacterObject, frmSell.SellPercent);
            }
            else if (treArmor.SelectedNode?.Tag is Gear objGear)
            {
                CharacterObject.Armor.FindArmorGear(objGear.InternalId, out Armor objArmor, out ArmorMod objMod);
                // Record the cost of the Armor with the ArmorMod.
                decimal decOriginal = objMod?.TotalCost ?? objArmor.TotalCost;

                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;

                if (objGear.Parent is Gear objParent)
                    objParent.Children.Remove(objGear);
                else if (objMod != null)
                    objMod.Gear.Remove(objGear);
                else
                    objArmor.Gear.Remove(objGear);

                // Create the Expense Log Entry for the sale.
                decimal decNewCost = objMod?.TotalCost ?? objArmor.TotalCost;
                decimal decAmount = (decOriginal - decNewCost) * frmSell.SellPercent;
                decAmount += objGear.DeleteGear() * frmSell.SellPercent;
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmorGear", GlobalOptions.Language) + ' ' + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen += decAmount;

            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void tsWeaponSell_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treWeapons.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;
                
                vendorTrash.Sell(CharacterObject, frmSell.SellPercent);
            }
            else
            {
                Utils.BreakIfDebug();
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void sellItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treGear.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                frmSellItem frmSell = new frmSellItem();
                frmSell.ShowDialog(this);

                if (frmSell.DialogResult == DialogResult.Cancel)
                    return;

                vendorTrash.Sell(CharacterObject, frmSell.SellPercent);
            }
            else
            {
                Utils.BreakIfDebug();
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
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

                vendorTrash.Sell(CharacterObject, frmSell.SellPercent);
            }
            else
            {
                Utils.BreakIfDebug();
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void tsAdvancedLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleAdvanced frmPickLifestyle = new frmSelectLifestyleAdvanced(CharacterObject);
                frmPickLifestyle.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                {
                    frmPickLifestyle.Dispose();
                    break;
                }
                blnAddAgain = frmPickLifestyle.AddAgain;

                Lifestyle objNewLifestyle = frmPickLifestyle.SelectedLifestyle;
                frmPickLifestyle.Dispose();
                objNewLifestyle.StyleType = LifestyleType.Advanced;

                CharacterObject.Lifestyles.Add(objNewLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsBoltHole_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleAdvanced frmPickLifestyle = new frmSelectLifestyleAdvanced(CharacterObject)
                {
                    StyleType = LifestyleType.BoltHole
                };
                frmPickLifestyle.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                {
                    frmPickLifestyle.Dispose();
                    break;
                }
                blnAddAgain = frmPickLifestyle.AddAgain;

                Lifestyle objNewLifestyle = frmPickLifestyle.SelectedLifestyle;
                frmPickLifestyle.Dispose();
                objNewLifestyle.Increments = 0;
                CharacterObject.Lifestyles.Add(objNewLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsSafehouse_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleAdvanced frmPickLifestyle = new frmSelectLifestyleAdvanced(CharacterObject)
                {
                    StyleType = LifestyleType.Safehouse
                };
                frmPickLifestyle.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                {
                    frmPickLifestyle.Dispose();
                    break;
                }
                blnAddAgain = frmPickLifestyle.AddAgain;

                Lifestyle objLifestyle = frmPickLifestyle.SelectedLifestyle;
                frmPickLifestyle.Dispose();
                objLifestyle.IncrementType = LifestyleIncrement.Week;
                objLifestyle.Increments = 0;
                CharacterObject.Lifestyles.Add(objLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsWeaponName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_WeaponName", GlobalOptions.Language),
                DefaultString = objWeapon.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objWeapon.CustomName = frmPickText.SelectedValue;
            treWeapons.SelectedNode.Text = objWeapon.DisplayName(GlobalOptions.Language);

            IsDirty = true;
        }

        private void tsGearName_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_GearName", GlobalOptions.Language),
                DefaultString = objGear.GearName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objGear.GearName = frmPickText.SelectedValue;
            treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

            IsDirty = true;
        }

        private void tsWeaponAddUnderbarrel_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objSelectedWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponAccessory", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (objSelectedWeapon.Cyberware)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberwareUnderbarrel", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_WeaponUnderbarrel", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = AddUnderbarrelWeapon(objSelectedWeapon, LanguageManager.GetString("String_ExpensePurchaseWeapon", GlobalOptions.Language));
            }
            while (blnAddAgain);
        }

        private void tsGearButtonAddAccessory_Click(object sender, EventArgs e)
        {
            tsGearAddAsPlugin_Click(sender, e);
        }

#if LEGACY
        private void tsGearAddNexus_Click(object sender, EventArgs e)
        {
            treGear.SelectedNode = treGear.Nodes[0];

            frmSelectNexus frmPickNexus = new frmSelectNexus(CharacterObject);
            frmPickNexus.ShowDialog(this);

            if (frmPickNexus.DialogResult == DialogResult.Cancel)
                return;

            Gear objGear = frmPickNexus.SelectedNexus;

            decimal decCost = objGear.TotalCost;

            // Multiply the cost if applicable.
            char chrAvail = objGear.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickNexus.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
            }

            CharacterObject.Gear.Add(objGear);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsVehicleAddNexus_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGearVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (treVehicles.SelectedNode.Level > 1)
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;

            // Attempt to locate the selected Vehicle.
            Vehicle objSelectedVehicle = CharacterObject.Vehicles.FindById(treVehicles.SelectedNode?.Tag.ToString());

            frmSelectNexus frmPickNexus = new frmSelectNexus(CharacterObject);
            frmPickNexus.ShowDialog(this);

            if (frmPickNexus.DialogResult == DialogResult.Cancel)
                return;

            Gear objGear = frmPickNexus.SelectedNexus;

            decimal decCost = objGear.TotalCost;

            // Multiply the cost if applicable.
            char chrAvail = objGear.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickNexus.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseVehicleGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
            }

            treVehicles.SelectedNode.Nodes.Add(objGear.CreateTreeNode(cmsVehicleGear));
            treVehicles.SelectedNode.Expand();

            objSelectedVehicle.Gear.Add(objGear);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endif

        private void tsUndoKarmaExpense_Click(object sender, EventArgs e)
        {
            ListViewItem objItem = lstKarma.SelectedItems.Count > 0 ? lstKarma.SelectedItems[0] : null;

            if (objItem == null)
            {
                return;
            }

            // Find the selected Karma Expense.
            string strNeedle = objItem.SubItems[3].Text;
            ExpenseLogEntry objEntry = CharacterObject.ExpenseEntries.FirstOrDefault(x => x.InternalId == strNeedle);

            if (objEntry?.Undo == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_UndoNoHistory", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoUndoHistory", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (objEntry.Undo.KarmaType == KarmaExpenseType.ImproveInitiateGrade)
            {
                // Get the grade of the item we're undoing and make sure it's the highest grade
                int intMaxGrade = 0;
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    intMaxGrade = Math.Max(intMaxGrade, objGrade.Grade);
                }
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    if (objGrade.InternalId == objEntry.Undo.ObjectId)
                    {
                        if (objGrade.Grade < intMaxGrade)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_UndoNotHighestGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotHighestGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        break;
                    }
                }
                if (MessageBox.Show(LanguageManager.GetString("Message_UndoExpense", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_UndoExpense", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            else
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_UndoExpense", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_UndoExpense", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            switch (objEntry.Undo.KarmaType)
            {
                case KarmaExpenseType.ImproveAttribute:
                    {
                        CharacterObject.GetAttribute(objEntry.Undo.ObjectId).Degrade(1);
                        break;
                    }
                case KarmaExpenseType.AddPowerPoint:
                    {
                        CharacterObject.MysticAdeptPowerPoints -= 1;
                        break;
                    }
                case KarmaExpenseType.AddQuality:
                    {
                        // Locate the Quality that was added.
                        foreach (Quality objQuality in CharacterObject.Qualities.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements that it created.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);

                            // Remove the Quality from thc character.
                            CharacterObject.Qualities.Remove(objQuality);

                            // Remove any Weapons created by the Quality if applicable.
                            if (!objQuality.WeaponID.IsEmptyGuid())
                            {
                                List<Weapon> lstWeapons = CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objQuality.InternalId).ToList();
                                foreach (Weapon objWeapon in lstWeapons)
                                {
                                    objWeapon.DeleteWeapon();
                                    // We can remove here because lstWeapons is separate from the Weapons that were yielded through DeepWhere
                                    if (objWeapon.Parent != null)
                                        objWeapon.Parent.Children.Remove(objWeapon);
                                    else
                                        CharacterObject.Weapons.Remove(objWeapon);
                                }
                            }
                        }
                    }
                    break;
                case KarmaExpenseType.AddSpell:
                    {
                        // Locate the Spell that was added.
                        foreach (Spell objSpell in CharacterObject.Spells.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements that it created.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Spell, objSpell.InternalId);

                            // Remove the Spell from the character.
                            CharacterObject.Spells.Remove(objSpell);
                        }
                        break;
                    }
                case KarmaExpenseType.SkillSpec:  //I am resonable sure those 2 are the same. Was written looking at old AddSpecialization code
                case KarmaExpenseType.AddSpecialization:
                    {
                        Skill ContainingSkill = CharacterObject.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.Specializations.Any(s => s.InternalId == objEntry.Undo.ObjectId)) ??
                                                CharacterObject.SkillsSection.Skills.FirstOrDefault(x => x.Specializations.Any(s => s.InternalId == objEntry.Undo.ObjectId));

                        ContainingSkill?.Specializations.Remove(ContainingSkill.Specializations.FirstOrDefault(x => x.InternalId == objEntry.Undo.ObjectId));

                        break;
                    }
                case KarmaExpenseType.ImproveSkillGroup:
                    {
                        // Locate the Skill Group that was affected.
                        SkillGroup group = CharacterObject.SkillsSection.SkillGroups.FirstOrDefault(g => g.InternalId == objEntry.Undo.ObjectId);

                        if (group != null)
                            group.Karma -= 1;

                        break;
                    }
                case KarmaExpenseType.AddSkill:
                case KarmaExpenseType.ImproveSkill:
                    {
                        // Locate the Skill that was affected.
                        Skill skill = CharacterObject.SkillsSection.Skills.FirstOrDefault(s => s.InternalId == objEntry.Undo.ObjectId) ??
                                      CharacterObject.SkillsSection.KnowledgeSkills.FirstOrDefault(s => s.InternalId == objEntry.Undo.ObjectId);

                        if (skill != null)
                            skill.Karma -= 1;

                        break;
                    }
                case KarmaExpenseType.AddMetamagic:
                    {
                        // Locate the Metamagic that was affected.
                        foreach (Metamagic objMetamagic in CharacterObject.Metamagics.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements created by the Metamagic.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Metamagic, objMetamagic.InternalId);

                            // Remove the Metamagic from the character.
                            CharacterObject.Metamagics.Remove(objMetamagic);
                        }
                        break;
                    }
                case KarmaExpenseType.ImproveInitiateGrade:
                    {
                        // Locate the Initiate Grade that was affected.
                        foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove the Grade from the character.
                            CharacterObject.InitiationGrades.Remove(objGrade);
                        }
                        break;
                    }
                case KarmaExpenseType.AddMartialArt:
                    {
                        // Locate the Martial Art that was affected.
                        foreach (MartialArt objMartialArt in CharacterObject.MartialArts.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements created by the Martial Art.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);

                            // Remove the Martial Art from the character.
                            CharacterObject.MartialArts.Remove(objMartialArt);
                        }
                        break;
                    }
#if LEGACY
                    case KarmaExpenseType.AddMartialArtManeuver:
                    {
                        // Locate the Martial Art Maneuver that was affected.
                        foreach (MartialArtManeuver objManeuver in CharacterObject.MartialArtManeuvers.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements created by the Maneuver.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.MartialArtTechnique, objManeuver.InternalId);

                            // Remove the Maneuver from the character.
                            CharacterObject.MartialArtManeuvers.Remove(objManeuver);
                        }
                    }
                    break;
#endif
                case KarmaExpenseType.AddComplexForm:
                    {
                        // Locate the Complex Form that was affected.
                        foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            // Remove any Improvements created by the Complex Form.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId);

                            // Remove the Complex Form from the character.
                            CharacterObject.ComplexForms.Remove(objComplexForm);
                        }
                        break;
                    }
                case KarmaExpenseType.BindFocus:
                    {
                        // Locate the Focus that was bound.
                        foreach (Focus objFocus in CharacterObject.Foci.Where(x => x.GearObject.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            TreeNode objNode = treFoci.FindNode(objEntry.Undo.ObjectId);
                            if (objNode != null)
                            {
                                _blnSkipRefresh = true;
                                objNode.Checked = false;
                                _blnSkipRefresh = false;
                            }
                            CharacterObject.Foci.Remove(objFocus);
                        }

                        // Locate the Stacked Focus that was bound.
                        foreach (StackedFocus objStack in CharacterObject.StackedFoci.Where(x => x.InternalId == objEntry.Undo.ObjectId).ToList())
                        {
                            TreeNode objNode = treFoci.FindNode(objEntry.Undo.ObjectId);
                            if (objNode != null)
                            {
                                _blnSkipRefresh = true;
                                objNode.Checked = false;
                                objStack.Bonded = false;
                                _blnSkipRefresh = false;
                            }
                        }
                        break;
                    }
                case KarmaExpenseType.JoinGroup:
                    {
                        // Remove the character from their Group.
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = false;
                        CharacterObject.GroupMember = false;
                        _blnSkipRefresh = false;
                        break;
                    }
                case KarmaExpenseType.LeaveGroup:
                    {
                        // Put the character back in their Group.
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = true;
                        CharacterObject.GroupMember = true;
                        _blnSkipRefresh = false;
                        break;
                    }
                case KarmaExpenseType.RemoveQuality:
                    {
                        // Add the Quality back to the character.
                        List<Weapon> lstWeapons = new List<Weapon>();

                        Quality objAddQuality = new Quality(CharacterObject);
                        XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");
                        XmlNode objXmlQualityNode = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + objEntry.Undo.ObjectId + "\"]") ??
                            objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objEntry.Undo.ObjectId + "\"]");
                        objAddQuality.Create(objXmlQualityNode, QualitySource.Selected, lstWeapons, objEntry.Undo.Extra);

                        CharacterObject.Qualities.Add(objAddQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                        break;
                    }
                case KarmaExpenseType.ManualAdd:
                case KarmaExpenseType.ManualSubtract:
                case KarmaExpenseType.QuickeningMetamagic:
                    break;
                case KarmaExpenseType.AddCritterPower:
                    {
                        foreach (CritterPower objPower in CharacterObject.CritterPowers.Where(objPower => objPower.InternalId == objEntry.Undo.ObjectId))
                        {
                            // Remove any Improvements created by the Critter Power.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId);

                            CharacterObject.CritterPowers.Remove(objPower);
                        }
                    }
                    break;

            }
            // Refund the Karma amount and remove the Expense Entry.
            CharacterObject.Karma -= decimal.ToInt32(objEntry.Amount);
            CharacterObject.ExpenseEntries.Remove(objEntry);

            _blnLoading = false;

            // Select the Magician's Tradition.
            if (!string.IsNullOrEmpty(CharacterObject.MagicTradition))
                cboTradition.SelectedValue = CharacterObject.MagicTradition;
            else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                cboTradition.SelectedIndex = 0;

            // Select the Technomancer's Stream.
            if (!string.IsNullOrEmpty(CharacterObject.TechnomancerStream))
                cboStream.SelectedValue = CharacterObject.TechnomancerStream;
            else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                cboStream.SelectedIndex = 0;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsUndoNuyenExpense_Click(object sender, EventArgs e)
        {
            ListViewItem objItem = lstNuyen.SelectedItems.Count > 0 ? lstNuyen.SelectedItems[0] : null;

            if (objItem == null)
            {
                return;
            }

            // Find the selected Nuyen Expense.
            string strNeedle = objItem.SubItems[3].Text;
            ExpenseLogEntry objEntry = CharacterObject.ExpenseEntries.FirstOrDefault(x => x.InternalId == strNeedle);

            if (objEntry?.Undo == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_UndoNoHistory", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoUndoHistory", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strUndoId = objEntry.Undo.ObjectId;

            if (objEntry.Undo.KarmaType == KarmaExpenseType.ImproveInitiateGrade)
            {
                // Get the grade of the item we're undoing and make sure it's the highest grade
                int intMaxGrade = 0;
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    intMaxGrade = Math.Max(intMaxGrade, objGrade.Grade);
                }
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    if (objGrade.InternalId == strUndoId)
                    {
                        if (objGrade.Grade < intMaxGrade)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_UndoNotHighestGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotHighestGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        break;
                    }
                }
                if (MessageBox.Show(LanguageManager.GetString("Message_UndoExpense", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_UndoExpense", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            else
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_UndoExpense", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_UndoExpense", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            if (!string.IsNullOrEmpty(strUndoId))
            {
                switch (objEntry.Undo.NuyenType)
                {
                    case NuyenExpenseType.AddCyberware:
                    {
                        // Locate the Cyberware that was added.
                        VehicleMod objVehicleMod = null;
                        Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(strUndoId) ??
                                                 CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strUndoId, out objVehicleMod);
                        if (objCyberware != null)
                        {
                            objCyberware.DeleteCyberware();

                            // Remove the Cyberware.
                            Cyberware objParent = objCyberware.Parent;
                            if (objParent != null)
                                objParent.Children.Remove(objCyberware);
                            else if (objVehicleMod != null)
                                objVehicleMod.Cyberware.Remove(objCyberware);
                            else
                                CharacterObject.Cyberware.Remove(objCyberware);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddGear:
                    {
                        // Locate the Gear that was added.
                        //If the gear was already deleted manually we will not be able to locate it here
                        Gear objGear = CharacterObject.Gear.DeepFindById(strUndoId);
                        TreeNode objNode;
                        Vehicle objVehicle = null;
                        WeaponAccessory objWeaponAccessory = null;
                        Cyberware objCyberware = null;
                        if (objGear != null)
                        {
                            objNode = treGear.FindNode(objGear.InternalId);
                        }
                        else
                        {
                            objGear = CharacterObject.Vehicles.FindVehicleGear(strUndoId, out objVehicle, out objWeaponAccessory, out objCyberware);
                            if (objGear != null)
                                objNode = treVehicles.FindNode(objGear.InternalId);
                            else
                                break;
                        }

                        objGear.Quantity -= objEntry.Undo.Qty;

                        if (objGear.Quantity <= 0)
                        {
                            if (objGear.Parent is Gear objParent)
                                objParent.Children.Remove(objGear);
                            else if (objWeaponAccessory != null)
                                objWeaponAccessory.Gear.Remove(objGear);
                            else if (objCyberware != null)
                                objCyberware.Gear.Remove(objGear);
                            else if (objVehicle != null)
                                objVehicle.Gear.Remove(objGear);
                            else
                                CharacterObject.Gear.Remove(objGear);

                            objGear.DeleteGear();
                        }
                        else if (objNode != null)
                        {
                            objNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicle:
                    {
                        // Locate the Vehicle that was added.
                        Vehicle objVehicle = CharacterObject.Vehicles.FindById(strUndoId);
                        if (objVehicle != null)
                        {
                            objVehicle.DeleteVehicle();

                            // Remove the Vehicle.
                            CharacterObject.Vehicles.Remove(objVehicle);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleMod:
                    {
                        // Locate the Vehicle Mod that was added.
                        VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strUndoId, out Vehicle objVehicle, out WeaponMount objWeaponMount);
                        if (objVehicleMod != null)
                        {
                            // Check for Improved Sensor bonus.
                            if (objVehicleMod.Bonus?["improvesensor"] != null || (objVehicleMod.WirelessOn && objVehicleMod.WirelessBonus?["improvesensor"] != null))
                            {
                                objVehicle.ChangeVehicleSensor(treVehicles, false, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                            }

                            objVehicleMod.DeleteVehicleMod();

                            // Remove the Vehicle Mod.
                            if (objWeaponMount != null)
                                objWeaponMount.Mods.Remove(objVehicleMod);
                            else
                                objVehicle.Mods.Remove(objVehicleMod);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleGear:
                    {
                        // Locate the Gear that was added.
                        TreeNode objNode = null;
                        Gear objGear = CharacterObject.Vehicles.FindVehicleGear(strUndoId, out Vehicle objVehicle, out WeaponAccessory objWeaponAccessory, out Cyberware objCyberware);
                        if (objGear == null)
                        {
                            objGear = CharacterObject.Gear.DeepFindById(strUndoId);
                            if (objGear == null)
                            {
                                objGear = CharacterObject.Cyberware.FindCyberwareGear(strUndoId, out objCyberware);
                                if (objGear == null)
                                {
                                    objGear = CharacterObject.Weapons.FindWeaponGear(strUndoId, out objWeaponAccessory);
                                    if (objGear != null)
                                        objNode = treWeapons.FindNode(strUndoId);
                                }
                                else
                                    objNode = treCyberware.FindNode(strUndoId);
                            }
                            else
                                objNode = treGear.FindNode(strUndoId);
                        }
                        else
                            objNode = treVehicles.FindNode(strUndoId);

                        if (objGear != null)
                        {
                            // Deduct the Qty from the Gear.
                            objGear.Quantity -= objEntry.Undo.Qty;

                            // Remove the Gear if its Qty has been reduced to 0.
                            if (objGear.Quantity <= 0)
                            {
                                if (objGear.Parent is Gear objParent)
                                    objParent.Children.Remove(objGear);
                                else if (objWeaponAccessory != null)
                                    objWeaponAccessory.Gear.Remove(objGear);
                                else if (objCyberware != null)
                                    objCyberware.Gear.Remove(objGear);
                                else if (objVehicle != null)
                                    objVehicle.Gear.Remove(objGear);
                                else
                                    CharacterObject.Gear.Remove(objGear);

                                objGear.DeleteGear();
                            }
                            else if (objNode != null)
                            {
                                objNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleWeapon:
                    {
                        // Locate the Weapon that was added.
                        Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strUndoId, out Vehicle objVehicle, out WeaponMount objWeaponMount, out VehicleMod objVehicleMod) ??
                                           CharacterObject.Weapons.DeepFindById(strUndoId);
                        if (objWeapon != null)
                        {
                            objWeapon.DeleteWeapon();

                            // Remove the Weapon.
                            if (objWeapon.Parent != null)
                                objWeapon.Parent.Children.Remove(objWeapon);
                            else if (objWeaponMount != null)
                                objWeaponMount.Weapons.Remove(objWeapon);
                            else if (objVehicleMod != null)
                                objVehicleMod.Weapons.Remove(objWeapon);
                            else if (objVehicle != null)
                                objVehicle.Weapons.Remove(objWeapon);
                            else
                                CharacterObject.Weapons.Remove(objWeapon);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleWeaponAccessory:
                    {
                        // Locate the Weapon Accessory that was added.
                        WeaponAccessory objWeaponAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strUndoId) ??
                                                             CharacterObject.Weapons.FindWeaponAccessory(strUndoId);
                        if (objWeaponAccessory != null)
                        {
                            objWeaponAccessory.DeleteWeaponAccessory();

                            // Remove the Weapon Accessory.
                            objWeaponAccessory.Parent.WeaponAccessories.Remove(objWeaponAccessory);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleWeaponMount:
                    {
                        WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strUndoId, out Vehicle objVehicle);
                        if (objWeaponMount != null)
                        {
                            objWeaponMount.DeleteWeaponMount();

                            objVehicle.WeaponMounts.Remove(objWeaponMount);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleWeaponMountMod:
                    {
                        VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleWeaponMountMod(strUndoId, out WeaponMount objWeaponMount);
                        if (objVehicleMod != null)
                        {
                            objVehicleMod.DeleteVehicleMod();
                            objWeaponMount.Mods.Remove(objVehicleMod);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddArmor:
                    {
                        // Locate the Armor that was added.
                        Armor objArmor = CharacterObject.Armor.FindById(strUndoId);

                        if (objArmor != null)
                        {
                            objArmor.DeleteArmor();

                            // Remove the Armor from the character.
                            CharacterObject.Armor.Remove(objArmor);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddArmorMod:
                    {
                        // Locate the Armor Mod that was added.
                        ArmorMod objArmorMod = CharacterObject.Armor.FindArmorMod(strUndoId);
                        if (objArmorMod != null)
                        {
                            objArmorMod.DeleteArmorMod();

                            // Remove the Armor Mod from the Armor.
                            objArmorMod.Parent?.ArmorMods.Remove(objArmorMod);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddWeapon:
                    {
                        // Locate the Weapon that was added.
                        Vehicle objVehicle = null;
                        WeaponMount objWeaponMount = null;
                        VehicleMod objVehicleMod = null;
                        Weapon objWeapon = CharacterObject.Weapons.DeepFindById(strUndoId) ??
                                           CharacterObject.Vehicles.FindVehicleWeapon(strUndoId, out objVehicle, out objWeaponMount, out objVehicleMod);
                        if (objWeapon != null)
                        {
                            objWeapon.DeleteWeapon();

                            // Remove the Weapn from the character.
                            if (objWeapon.Parent != null)
                                objWeapon.Parent.Children.Remove(objWeapon);
                            else if (objWeaponMount != null)
                                objWeaponMount.Weapons.Remove(objWeapon);
                            else if (objVehicleMod != null)
                                objVehicleMod.Weapons.Remove(objWeapon);
                            else if (objVehicle != null)
                                objVehicle.Weapons.Remove(objWeapon);
                            else
                                CharacterObject.Weapons.Remove(objWeapon);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddWeaponAccessory:
                    {
                        // Locate the Weapon Accessory that was added.
                        WeaponAccessory objWeaponAccessory = CharacterObject.Weapons.FindWeaponAccessory(strUndoId) ??
                                                             CharacterObject.Vehicles.FindVehicleWeaponAccessory(strUndoId);
                        if (objWeaponAccessory != null)
                        {
                            objWeaponAccessory.DeleteWeaponAccessory();
                            // Remove the Weapon Accessory.
                            objWeaponAccessory.Parent.WeaponAccessories.Remove(objWeaponAccessory);
                        }
                    }
                        break;
                    case NuyenExpenseType.IncreaseLifestyle:
                    {
                        // Locate the Lifestyle that was increased.
                        Lifestyle objLifestyle = CharacterObject.Lifestyles.FirstOrDefault(x => x.InternalId == strUndoId);
                        if (objLifestyle != null)
                        {
                            objLifestyle.Increments -= 1;
                        }
                    }
                        break;
                    case NuyenExpenseType.AddArmorGear:
                    {
                        // Locate the Armor Gear that was added.
                        Gear objGear = CharacterObject.Armor.FindArmorGear(strUndoId, out Armor objArmor, out ArmorMod objArmorMod);
                        if (objGear != null)
                        {
                            // Deduct the Qty from the Gear.
                            objGear.Quantity -= objEntry.Undo.Qty;

                            // Remove the Gear if its Qty has been reduced to 0.
                            if (objGear.Quantity <= 0)
                            {
                                objGear.DeleteGear();

                                if (objGear.Parent is Gear objParent)
                                    objParent.Children.Remove(objGear);
                                else if (objArmorMod != null)
                                    objArmorMod.Gear.Remove(objGear);
                                else
                                    objArmor?.Gear.Remove(objGear);
                            }
                            else
                            {
                                TreeNode objNode = treArmor.FindNode(strUndoId);
                                if (objNode != null)
                                    objNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                        }
                    }
                        break;
                    case NuyenExpenseType.AddVehicleModCyberware:
                    {
                        // Locate the Cyberware that was added.
                        Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strUndoId, out VehicleMod objVehicleMod) ??
                                                 CharacterObject.Cyberware.DeepFindById(strUndoId);
                        if (objCyberware != null)
                        {
                            objCyberware.DeleteCyberware();
                            // Remove the Cyberware.
                            Cyberware objParent = objCyberware.Parent;
                            if (objParent != null)
                                objParent.Children.Remove(objCyberware);
                            else if (objVehicleMod != null)
                                objVehicleMod.Cyberware.Remove(objCyberware);
                            else
                                CharacterObject.Cyberware.Remove(objCyberware);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddCyberwareGear:
                    {
                        // Locate the Gear that was added.
                        Vehicle objVehicle = null;
                        WeaponAccessory objWeaponAccessory = null;
                        Gear objGear = CharacterObject.Cyberware.FindCyberwareGear(strUndoId, out Cyberware objCyberware) ??
                                       CharacterObject.Vehicles.FindVehicleGear(strUndoId, out objVehicle, out objWeaponAccessory, out objCyberware) ??
                                       CharacterObject.Gear.DeepFindById(strUndoId);
                        if (objGear != null)
                        {
                            objGear.DeleteGear();

                            if (objGear.Parent is Gear objParent)
                                objParent.Children.Remove(objGear);
                            else if (objWeaponAccessory != null)
                                objWeaponAccessory.Gear.Remove(objGear);
                            else if (objCyberware != null)
                                objCyberware.Gear.Remove(objGear);
                            else if (objVehicle != null)
                                objVehicle.Gear.Remove(objGear);
                            else
                                CharacterObject.Gear.Remove(objGear);
                        }
                    }
                        break;
                    case NuyenExpenseType.AddWeaponGear:
                    {
                        // Locate the Gear that was added.
                        Vehicle objVehicle = null;
                        Cyberware objCyberware = null;
                        Gear objGear = CharacterObject.Weapons.FindWeaponGear(strUndoId, out WeaponAccessory objWeaponAccessory) ??
                                       CharacterObject.Vehicles.FindVehicleGear(strUndoId, out objVehicle, out objWeaponAccessory, out objCyberware) ??
                                       CharacterObject.Gear.DeepFindById(strUndoId);
                        if (objGear != null)
                        {
                            objGear.DeleteGear();

                            if (objGear.Parent is Gear objParent)
                                objParent.Children.Remove(objGear);
                            else if (objWeaponAccessory != null)
                                objWeaponAccessory.Gear.Remove(objGear);
                            else if (objCyberware != null)
                                objCyberware.Gear.Remove(objGear);
                            else if (objVehicle != null)
                                objVehicle.Gear.Remove(objGear);
                            else
                                CharacterObject.Gear.Remove(objGear);
                        }
                    }
                        break;
                    case NuyenExpenseType.ManualAdd:
                    case NuyenExpenseType.ManualSubtract:
                        break;
                }
            }

            // Refund the Nuyen amount and remove the Expense Entry.
            CharacterObject.Nuyen -= objEntry.Amount;
            CharacterObject.ExpenseEntries.Remove(objEntry);
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsEditNuyenExpense_Click(object sender, EventArgs e)
        {
            cmdNuyenEdit_Click(sender, e);
        }

        private void tsEditKarmaExpense_Click(object sender, EventArgs e)
        {
            cmdKarmaEdit_Click(sender, e);
        }

        private void tsAddArmorGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Select the root Gear node then open the Select Gear window.
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmorGear(objArmor.InternalId, true);
            }
            while (blnAddAgain);
        }

        private void tsArmorGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSelectedId = string.Empty;
            bool capacityOnly = false;
            if (treArmor.SelectedNode?.Tag is Gear objGear)
            {
                strSelectedId = objGear.InternalId;
            }
            else if (treArmor.SelectedNode?.Tag is ArmorMod objMod)
            {
                strSelectedId = objMod.InternalId;
                if (string.IsNullOrEmpty(objMod?.GearCapacity))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

            }

            if (strSelectedId == string.Empty) return;

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmorGear(strSelectedId, capacityOnly);
            }
            while (blnAddAgain);
        }

        private void tsArmorNotes_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treArmor.SelectedNode);

            IsDirty = true;
        }

        private void tsWeaponNotes_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treWeapons.SelectedNode);

            IsDirty = true;
        }

        private void tsCyberwareNotes_Click(object sender, EventArgs e)
        {
            if (!(treCyberware.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treCyberware.SelectedNode);

            IsDirty = true;
        }

        private void tsQualityNotes_Click(object sender, EventArgs e)
        {
            if (!(treQualities.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treQualities.SelectedNode);

            IsDirty = true;
        }

        private void tsMartialArtsNotes_Click(object sender, EventArgs e)
        {
            if (!(treMartialArts.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treMartialArts.SelectedNode);

            IsDirty = true;
        }

        private void tsSpellNotes_Click(object sender, EventArgs e)
        {
            if (!(treSpells.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treSpells.SelectedNode);

            IsDirty = true;
        }

        private void tsComplexFormNotes_Click(object sender, EventArgs e)
        {
            if (!(treComplexForms.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treComplexForms.SelectedNode);

            IsDirty = true;
        }

        private void tsCritterPowersNotes_Click(object sender, EventArgs e)
        {
            if (!(treCritterPowers.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treCritterPowers.SelectedNode);

            IsDirty = true;
        }

        private void tsMetamagicNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treMetamagic.SelectedNode);

            IsDirty = true;
        }

        private void tsGearNotes_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treGear.SelectedNode);

            IsDirty = true;
        }

        private void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treVehicles.SelectedNode);

            IsDirty = true;
        }

        private void tsLifestyleNotes_Click(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treLifestyles.SelectedNode);

            IsDirty = true;
        }

        private void tsVehicleName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            while (treVehicles.SelectedNode.Level > 1)
            {
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;
            }

            if (!(treVehicles.SelectedNode?.Tag is IHasCustomName objRename)) return;

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_VehicleName", GlobalOptions.Language),
                DefaultString = objRename.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objRename.CustomName = frmPickText.SelectedValue;
            treVehicles.SelectedNode.Text = objRename.DisplayName(GlobalOptions.Language);

            IsDirty = true;
        }

        private void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasInternalId strSelectedId))
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleCyberwarePlugin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId.InternalId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId.InternalId, out objMod);

            if (objCyberwareParent == null && (objMod == null || !objMod.AllowCyberware))
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleCyberwarePlugin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("cyberware.xml");
            bool blnAddAgain;

            do
            {
                frmSelectCyberware frmPickCyberware = new frmSelectCyberware(CharacterObject, Improvement.ImprovementSource.Cyberware, objCyberwareParent?.GetNode() ?? objMod.GetNode());
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
                        if (CharacterObjectOptions.EnforceCapacity && objCyberwareParent.CapacityRemaining < 0)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Cyberware objCyberware = new Cyberware(CharacterObject);
                if (objCyberware.Purchase(objCyberware, objXmlCyberware, Improvement.ImprovementSource.Cyberware, frmPickCyberware.SelectedGrade,frmPickCyberware.SelectedRating,CharacterObject,objVehicle,objMod.Cyberware,CharacterObject.Vehicles,objMod.Weapons,frmPickCyberware.Markup,frmPickCyberware.FreeCost, "String_ExpensePurchaseVehicleCyberware"))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }

                frmPickCyberware.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsArmorName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected.
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmorName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            while (treArmor.SelectedNode.Level > 1)
            {
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;
            }

            if (!(treArmor.SelectedNode?.Tag is IHasCustomName objRename)) return;

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_ArmorName", GlobalOptions.Language),
                DefaultString = objRename.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objRename.CustomName = frmPickText.SelectedValue;
            treArmor.SelectedNode.Text = objRename.DisplayName(GlobalOptions.Language);

            IsDirty = true;
        }

        private void tsEditAdvancedLifestyle_Click(object sender, EventArgs e)
        {
            treLifestyles_DoubleClick(sender, e);
        }

        private void tsAdvancedLifestyleNotes_Click(object sender, EventArgs e)
        {
            tsLifestyleNotes_Click(sender, e);
        }

        private void tsEditLifestyle_Click(object sender, EventArgs e)
        {
            treLifestyles_DoubleClick(sender, e);
        }

        private void tsLifestyleName_Click(object sender, EventArgs e)
        {
            // Get the information for the currently selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is IHasCustomName objCustomName))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectLifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectLifestyle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_LifestyleName", GlobalOptions.Language),
                DefaultString = objCustomName.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            if (objCustomName.CustomName != frmPickText.SelectedValue)
            {
                objCustomName.CustomName = frmPickText.SelectedValue;

                treLifestyles.SelectedNode.Text = objCustomName.DisplayName(GlobalOptions.Language);

                IsDirty = true;
            }
        }

        private void tsGearRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Location objLocation)) return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language),
                DefaultString = objLocation.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;
            objLocation.Name = frmPickText.SelectedValue;

            IsDirty = true;
        }

        private void tsWeaponRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Location objLocation)) return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language),
                DefaultString = objLocation.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;
            objLocation.Name = frmPickText.SelectedValue;

            IsDirty = true;
        }

        private void tsCreateSpell_Click(object sender, EventArgs e)
        {
            int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Spells");
            // Make sure the character has enough Karma before letting them select a Spell.
            if (CharacterObject.Karma < intSpellKarmaCost)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The character is still allowed to add Spells, so show the Create Spell window.
            frmCreateSpell frmSpell = new frmCreateSpell(CharacterObject);
            frmSpell.ShowDialog(this);

            if (frmSpell.DialogResult == DialogResult.Cancel)
                return;

            Spell objSpell = frmSpell.SelectedSpell;
            if (objSpell.Alchemical)
            {
                intSpellKarmaCost = CharacterObject.SpellKarmaCost("Preparations");
            }
            else if (objSpell.Category == "Rituals")
            {
                intSpellKarmaCost = CharacterObject.SpellKarmaCost("Rituals");
            }
            if (CharacterObject.Karma < intSpellKarmaCost)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", objSpell.DisplayName(GlobalOptions.Language)).Replace("{1}", intSpellKarmaCost.ToString())))
                return;

            CharacterObject.Spells.Add(objSpell);

            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(intSpellKarmaCost * -1, LanguageManager.GetString("String_ExpenseLearnSpell", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objSpell.Name, ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);
            CharacterObject.Karma -= intSpellKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddSpell, objSpell.InternalId);
            objEntry.Undo = objUndo;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsImprovementNotes_Click(object sender, EventArgs e)
        {
            if (!(treImprovements.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treImprovements.SelectedNode);

            IsDirty = true;
        }

        private void tsArmorRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Location objLocation)) return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language),
                DefaultString = objLocation.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;
            objLocation.Name = frmPickText.SelectedValue;

            IsDirty = true;
        }

        private void tsImprovementRenameLocation_Click(object sender, EventArgs e)
        {
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            string strNewLocation = frmPickText.SelectedValue;

            int i = -1;
            foreach (string strLocation in CharacterObject.ImprovementGroups)
            {
                i++;
                if (strLocation == treImprovements.SelectedNode.Text)
                {
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.CustomGroup == strLocation)
                            objImprovement.CustomGroup = strNewLocation;
                    }

                    CharacterObject.ImprovementGroups[i] = strNewLocation;
                    break;
                }
            }

            IsDirty = true;
        }

        private void tsCyberwareAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCyberware.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objCyberware))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberwareGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objCyberware.GetNode(), strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwearGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                objCyberware.Gear.Add(objGear);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleCyberwareAddGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberwareGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objCyberware.GetNode(), strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwearGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                objCyberware.Gear.Add(objGear);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsCyberwareGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCyberware.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level < 2)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            if (!(treCyberware.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlSensorGear, strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwearGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleCyberwareGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlSensorGear, strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwearGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory))
                return;
            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_WeaponGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objAccessory.GetNode(), strCategories);
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

                // Open the Gear XML file and locate the selected piece.
                XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, 1);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                objAccessory.Gear.Add(objGear);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlSensorGear, strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Location objLocation)) return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language),
                DefaultString = objLocation.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;
            objLocation.Name = frmPickText.SelectedValue;

            IsDirty = true;
        }

        private void tsCreateNaturalWeapon_Click(object sender, EventArgs e)
        {
            frmNaturalWeapon frmCreateNaturalWeapon = new frmNaturalWeapon(CharacterObject);
            frmCreateNaturalWeapon.ShowDialog(this);

            if (frmCreateNaturalWeapon.DialogResult == DialogResult.Cancel)
                return;

            Weapon objWeapon = frmCreateNaturalWeapon.SelectedWeapon;
            CharacterObject.Weapons.Add(objWeapon);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsVehicleWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Locate the Vehicle Sensor Gear.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlSensorGear, strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, frmPickGear.SelectedQty);
                    objExpense.Undo = objUndo;
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);
                CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out _, out _);
                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = objVehicle;
                    objVehicle.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)) return;
            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_WeaponGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objAccessory.GetNode(), strCategories);
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

                Gear objGear = new Gear(CharacterObject);
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
                if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                    decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                    decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickGear.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        objGear.DeleteGear();
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        frmPickGear.Dispose();
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseWeaponGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

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

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }
#endregion

#region Additional Common Tab Control Events
        private void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the selected Quality.
            Quality objQuality = treQualities.SelectedNode?.Tag as Quality;

            UpdateQualityLevelValue(objQuality);
            if (objQuality == null)
            {
                lblQualitySource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblQualitySource, null);
                lblQualityBP.Text = string.Empty;
            }
            else
            {
                string strPage = objQuality.DisplayPage(GlobalOptions.Language);
                lblQualitySource.Text = CommonFunctions.LanguageBookShort(objQuality.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblQualitySource, CommonFunctions.LanguageBookLong(objQuality.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                lblQualityBP.Text = (objQuality.BP * objQuality.Levels * CharacterObjectOptions.KarmaQuality).ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            }
        }
#endregion

#region Additional Cyberware Tab Control Events
        private void treCyberware_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedCyberware();
        }
#endregion

#region Additional Street Gear Tab Control Events
        private void treWeapons_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedWeapon();
            RefreshPasteStatus();
        }

        private void treWeapons_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is IHasInternalId))
            {
                if (treWeapons.SelectedNode.Level != 1 && treWeapons.SelectedNode.Level != 0)
                    return;
                
                _intDragLevel = treWeapons.SelectedNode.Level;
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void treWeapons_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treWeapons_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else if (treWeapons.Nodes.Count > 0)
            {
                intNewIndex = treWeapons.Nodes[treWeapons.Nodes.Count - 1].Nodes.Count;
                nodDestination = treWeapons.Nodes[treWeapons.Nodes.Count - 1];
            }

            if (treWeapons.SelectedNode.Level == 1)
                CharacterObject.MoveWeaponNode(intNewIndex, nodDestination, treWeapons.SelectedNode);
            else
                CharacterObject.MoveWeaponRoot(intNewIndex, nodDestination, treWeapons.SelectedNode);

            // Clear the background color for all Nodes.
            treWeapons.ClearNodeBackground(null);

            IsDirty = true;
        }

        private void treWeapons_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = ((TreeView)sender).GetNodeAt(pt);

            if (objNode == null)
                return;

            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
            if (objNode.Level <= _intDragLevel)
                objNode.BackColor = SystemColors.ControlDark;

            // Clear the background colour for all other Nodes.
            treWeapons.ClearNodeBackground(objNode);
        }

        private void treArmor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedArmor();
            RefreshPasteStatus();
        }

        private void treArmor_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level != 1)
                    return;

            _intDragLevel = treArmor.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treArmor_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treArmor_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else if (treArmor.Nodes.Count > 0)
            {
                intNewIndex = treArmor.Nodes[treArmor.Nodes.Count - 1].Nodes.Count;
                nodDestination = treArmor.Nodes[treArmor.Nodes.Count - 1];
            }

            if (treArmor.SelectedNode.Level == 1)
                CharacterObject.MoveArmorNode(intNewIndex, nodDestination, treArmor.SelectedNode);
            else
                CharacterObject.MoveArmorRoot(intNewIndex, nodDestination, treArmor.SelectedNode);

            // Clear the background color for all Nodes.
            treArmor.ClearNodeBackground(null);

            IsDirty = true;
        }

        private void treArmor_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = ((TreeView)sender).GetNodeAt(pt);

            if (objNode == null)
                return;

            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
            if (objNode.Level <= _intDragLevel)
                objNode.BackColor = SystemColors.ControlDark;

            // Clear the background colour for all other Nodes.
            treArmor.ClearNodeBackground(objNode);
        }

        private void treLifestyles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedLifestyle();
            RefreshPasteStatus();
        }

        private void treLifestyles_DoubleClick(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            string strGuid = strGuid = objLifestyle.InternalId;
            int intMonths = objLifestyle.Increments;
            int intPosition = CharacterObject.Lifestyles.IndexOf(CharacterObject.Lifestyles.Where(p => p.InternalId == objLifestyle.InternalId).First());
            string strOldLifestyleName = objLifestyle.DisplayName(GlobalOptions.Language);
            decimal decOldLifestyleTotalCost = objLifestyle.TotalCost;

            if (objLifestyle.StyleType != LifestyleType.Standard)
            {
                // Edit Advanced Lifestyle.
                frmSelectLifestyleAdvanced frmPickLifestyle = new frmSelectLifestyleAdvanced(CharacterObject);
                frmPickLifestyle.SetLifestyle(objLifestyle);
                frmPickLifestyle.ShowDialog(this);

                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    return;

                // Update the selected Lifestyle and refresh the list.
                objLifestyle = frmPickLifestyle.SelectedLifestyle;
            }
            else
            {
                // Edit Basic Lifestyle.
                frmSelectLifestyle frmPickLifestyle = new frmSelectLifestyle(CharacterObject);
                frmPickLifestyle.SetLifestyle(objLifestyle);
                frmPickLifestyle.ShowDialog(this);

                if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    return;

                // Update the selected Lifestyle and refresh the list.
                objLifestyle = frmPickLifestyle.SelectedLifestyle;
            }
            objLifestyle.Increments = intMonths;

            decimal decAmount = Math.Max(objLifestyle.TotalCost - decOldLifestyleTotalCost, 0);
            if (decAmount > CharacterObject.Nuyen)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            objLifestyle.SetInternalId(strGuid);
            CharacterObject.Lifestyles[intPosition] = objLifestyle;
            treLifestyles.SelectedNode.Text = objLifestyle.DisplayName(GlobalOptions.Language);
            treLifestyles.SelectedNode.Tag = objLifestyle;
            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(-decAmount, LanguageManager.GetString("String_ExpenseModifiedLifestyle", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strOldLifestyleName + " -> " + objLifestyle.DisplayName(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        /*
        private void treLifestyles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level != 1)
            {
                    return;
            }
            _intDragLevel = treLifestyles.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treLifestyles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treLifestyles_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else if (treLifestyles.Nodes.Count > 0)
            {
                intNewIndex = treLifestyles.Nodes[treLifestyles.Nodes.Count - 1].Nodes.Count;
                nodDestination = treLifestyles.Nodes[treLifestyles.Nodes.Count - 1];
            }

            CommonFunctions.MoveLifestyleNode(CharacterObject, intNewIndex, nodDestination, treLifestyles);

            // Clear the background color for all Nodes.
            treLifestyles.ClearNodeBackground(null);

            IsDirty = true;
        }
        */

        private void treLifestyles_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = ((TreeView)sender).GetNodeAt(pt);

            if (objNode == null)
                return;

            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
            if (objNode.Level <= _intDragLevel)
                objNode.BackColor = SystemColors.ControlDark;

            // Clear the background colour for all other Nodes.
            treLifestyles.ClearNodeBackground(objNode);
        }

        private void treGear_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedGear();
            RefreshPasteStatus();
        }

        private void chkArmorEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treArmor.SelectedNode == null)
                return;

            // Locate the selected Armor or Armor Mod.
            switch (treArmor.SelectedNode?.Tag)
            {
                case Armor objArmor:
                    objArmor.Equipped = chkArmorEquipped.Checked;
                    break;
                case ArmorMod objMod:
                    objMod.Equipped = chkArmorEquipped.Checked;
                    break;
                case Gear objGear:
                    objGear.Equipped = chkArmorEquipped.Checked;
                    if (chkArmorEquipped.Checked)
                    {
                        CharacterObject.Armor.FindArmorGear(objGear.InternalId, out Armor objParentArmor, out ArmorMod objParentMod);
                        // Add the Gear's Improevments to the character.
                        if (objParentArmor.Equipped && objParentMod?.Equipped != false)
                        {
                            objGear.ChangeEquippedStatus(true);
                        }
                    }
                    else
                    {
                        objGear.ChangeEquippedStatus(false);
                    }

                    break;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdFireWeapon_Click(object sender, EventArgs e)
        {
            // "Click" the first menu item available.
            if (cmsAmmoSingleShot.Enabled)
                cmsAmmoSingleShot_Click(sender, e);
            else
            {
                if (cmsAmmoShortBurst.Enabled)
                    cmsAmmoShortBurst_Click(sender, e);
                else
                    cmsAmmoLongBurst_Click(sender, e);
            }
        }

        private void cmdReloadWeapon_Click(object sender, EventArgs e)
        {
            if (!(treWeapons?.SelectedNode?.Tag is Weapon objWeapon)) return;
            List<Gear> lstAmmo = new List<Gear>();
            List<string> lstCount = new List<string>();
            bool blnExternalSource = false;
            Gear objExternalSource = new Gear(CharacterObject)
            {
                Name = "External Source"
            };

            if (!objWeapon.RequireAmmo)
            {
                // If the Weapon does not require Ammo, just use External Source.
                lstAmmo.Add(objExternalSource);
            }
            else
            {
                string ammoString = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage);
                // Determine which loading methods are available to the Weapon.
                if (ammoString.IndexOfAny('x', '+') != -1 || ammoString.Contains(" or ") || ammoString.Contains("Special"))
                {
                    string strWeaponAmmo = ammoString.ToLower();
                    if (strWeaponAmmo.Contains("external source"))
                        blnExternalSource = true;
                    // Get rid of external source, special, or belt, and + energy.
                    strWeaponAmmo = strWeaponAmmo.Replace("external source", "100")
                        .Replace("special", "100")
                        .FastEscapeOnceFromEnd(" + energy")
                        .Replace(" or belt", " or 250(belt)");

                    string[] strAmmos = strWeaponAmmo.Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string strAmmo in strAmmos)
                    {
                        string strThisAmmo = strAmmo.TrimStartOnce("2x", "3x", "4x").TrimEndOnce("x2", "x3", "x4");

                        int intPos = strThisAmmo.IndexOf('(');
                        if (intPos != -1)
                            strThisAmmo = strThisAmmo.Substring(0, intPos);

                        lstCount.Add(strThisAmmo);
                    }
                }
                else
                {
                    // Nothing weird in the ammo string, so just use the number given.
                    string strAmmo = ammoString;
                    int intPos = strAmmo.IndexOf('(');
                    if (intPos != -1)
                        strAmmo = strAmmo.Substring(0, intPos);
                    lstCount.Add(strAmmo);
                }

                // Find all of the Ammo for the current Weapon that the character is carrying.
                HashSet<string> setAmmoPrefixStringSet = new HashSet<string>(objWeapon.AmmoPrefixStrings);
                // This is a standard Weapon, so consume traditional Ammunition.
                lstAmmo.AddRange(CharacterObject.Gear.DeepWhere(x => x.Children, x => x.Quantity > 0 && (x.Category == "Ammunition" && x.Extra == objWeapon.AmmoCategory ||
                                                                                                         string.IsNullOrEmpty(x.Extra) && setAmmoPrefixStringSet.Any(y => x.Name.StartsWith(y)) ||
                                                                                                         objWeapon.UseSkill == "Throwing Weapons" && objWeapon.Name == x.Name)));

                // If the Weapon is allowed to use an External Source, put in an External Source item.
                if (blnExternalSource)
                {
                    lstAmmo.Add(objExternalSource);
                }

                // Make sure the character has some form of Ammunition for this Weapon.
                if (lstAmmo.Count == 0)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmoType", GlobalOptions.Language).Replace("{0}", objWeapon.DisplayAmmoCategory(GlobalOptions.Language)), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            // Show the Ammunition Selection window.
            frmReload frmReloadWeapon = new frmReload
            {
                Ammo = lstAmmo,
                Count = lstCount
            };
            frmReloadWeapon.ShowDialog(this);

            if (frmReloadWeapon.DialogResult == DialogResult.Cancel)
                return;

            // Return any unspent rounds to the Ammo.
            if (objWeapon.AmmoRemaining > 0)
            {
                foreach (Gear objAmmo in CharacterObject.Gear)
                {
                    if (objAmmo.InternalId == objWeapon.AmmoLoaded)
                    {
                        objAmmo.Quantity += objWeapon.AmmoRemaining;

                        // Refresh the Gear tree.
                        TreeNode objNode = treGear.FindNode(objAmmo.InternalId);
                        if (objNode != null)
                        {
                            objNode.Text = objAmmo.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        }

                        break;
                    }
                    foreach (Gear objChild in objAmmo.Children.GetAllDescendants(x => x.Children))
                    {
                        if (objChild.InternalId == objWeapon.AmmoLoaded)
                        {
                            // If this is a plugin for a Spare Clip, move any extra rounds to the character instead of messing with the Clip amount.
                            if (objChild.Parent is Gear parent && (parent.Name.StartsWith("Spare Clip") || parent.Name.StartsWith("Speed Loader")))
                            {
                                Gear objNewGear = new Gear(CharacterObject);
                                objNewGear.Copy(objChild);
                                objNewGear.Quantity = objWeapon.AmmoRemaining;
                                CharacterObject.Gear.Add(objNewGear);

                                goto EndLoop;
                            }

                            objChild.Quantity += objWeapon.AmmoRemaining;

                            // Refresh the Gear tree.
                            TreeNode objNode = treGear.FindNode(objChild.InternalId);
                            if (objNode != null)
                            {
                                objNode.Text = objAmmo.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                            break;
                        }
                    }
                }
                EndLoop:;
            }

            Gear objSelectedAmmo;
            decimal decQty = frmReloadWeapon.SelectedCount;
            // If an External Source is not being used, consume ammo.
            if (frmReloadWeapon.SelectedAmmo != objExternalSource.InternalId)
            {
                objSelectedAmmo = CharacterObject.Gear.DeepFindById(frmReloadWeapon.SelectedAmmo);

                if (objSelectedAmmo.Quantity == decQty && objSelectedAmmo.Parent != null)
                {
                    // If the Ammo is coming from a Spare Clip, reduce the container quantity instead of the plugin quantity.
                    if (objSelectedAmmo.Parent is Gear objParent && (objParent.Name.StartsWith("Spare Clip") || objParent.Name.StartsWith("Speed Loader")))
                    {
                        if (objParent.Quantity > 0)
                            objParent.Quantity -= 1;
                        TreeNode objNode = treGear.FindNode(objParent.InternalId);
                        objNode.Text = objParent.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    }
                }
                else
                {
                    // Deduct the ammo qty from the ammo. If there isn't enough remaining, use whatever is left.
                    if (objSelectedAmmo.Quantity > decQty)
                        objSelectedAmmo.Quantity -= decQty;
                    else
                    {
                        decQty = objSelectedAmmo.Quantity;
                        objSelectedAmmo.Quantity = 0;
                    }
                }

                // Refresh the Gear tree.
                TreeNode objSelectedNode = treGear.FindNode(objSelectedAmmo.InternalId);
                if (objSelectedNode != null)
                    objSelectedNode.Text = objSelectedAmmo.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else
            {
                objSelectedAmmo = objExternalSource;
            }

            objWeapon.AmmoRemaining = decimal.ToInt32(decQty);
            objWeapon.AmmoLoaded = objSelectedAmmo.InternalId;
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            // Determine if this is a Weapon.
            switch (treWeapons.SelectedNode?.Tag)
            {
                case Weapon objWeapon:
                    objWeapon.Equipped = chkWeaponAccessoryInstalled.Checked;
                    break;
                case Gear objGear:
                    // Find the selected Gear.
                    objGear.Equipped = chkWeaponAccessoryInstalled.Checked;
                    objGear.ChangeEquippedStatus(chkWeaponAccessoryInstalled.Checked);
                    break;
                case WeaponAccessory objAccessory:
                    objAccessory.Equipped = chkWeaponAccessoryInstalled.Checked;
                    break;
            }
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkIncludedInWeapon_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            // Locate the selected Weapon Accessory or Modification.
            if (treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                objAccessory.IncludedInWeapon = chkIncludedInWeapon.Checked;

                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void treGear_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is IHasInternalId)) return;
            if (e.Button == MouseButtons.Left)
            {
                if (treGear.SelectedNode.Level != 1 && treGear.SelectedNode.Level != 0)
                    return;
                _eDragButton = MouseButtons.Left;
            }
            else
            {
                if (treGear.SelectedNode.Level == 0)
                    return;
                _eDragButton = MouseButtons.Right;
            }

            _intDragLevel = treGear.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treGear_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treGear_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else if (treGear.Nodes.Count > 0)
            {
                intNewIndex = treGear.Nodes[treGear.Nodes.Count - 1].Nodes.Count;
                nodDestination = treGear.Nodes[treGear.Nodes.Count - 1];
            }

            // If the item was moved using the left mouse button, change the order of things.
            if (_eDragButton == MouseButtons.Left)
            {
                if (treGear.SelectedNode.Level == 1)
                    CharacterObject.MoveGearNode(intNewIndex, nodDestination, treGear.SelectedNode);
                else
                    CharacterObject.MoveGearRoot(intNewIndex, nodDestination, treGear.SelectedNode);
            }
            if (_eDragButton == MouseButtons.Right)
                CharacterObject.MoveGearParent(nodDestination, treGear.SelectedNode);

            // Clear the background color for all Nodes.
            treGear.ClearNodeBackground(null);

            _eDragButton = MouseButtons.None;

            IsDirty = true;
        }

        private void treGear_DragOver(object sender, DragEventArgs e)
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
            treGear.ClearNodeBackground(objNode);
        }

        private void chkGearEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treGear.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            if (treGear.SelectedNode?.Tag is Gear objSelectedGear)
            {
                objSelectedGear.Equipped = chkGearEquipped.Checked;
                objSelectedGear.ChangeEquippedStatus(chkGearEquipped.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void cboWeaponAmmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level == 0)
                    return;

            if (!(treWeapons?.SelectedNode?.Tag is Weapon objWeapon) || _blnSkipRefresh)
                return;

            objWeapon.ActiveAmmoSlot = Convert.ToInt32(cboWeaponAmmo.SelectedValue.ToString());
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkGearHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void chkCyberwareHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void cmdWeaponBuyAmmo_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            string[] lstAmmoPrefixStrings = objWeapon.AmmoPrefixStrings;
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(null, null, true, null, string.Empty, lstAmmoPrefixStrings);
            }
            while (blnAddAgain);
        }

        private void cmdWeaponMoveToVehicle_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            List<Vehicle> lstVehicles = new List<Vehicle>();
            foreach (Vehicle objCharacterVehicle in CharacterObject.Vehicles)
            {
                if (objCharacterVehicle.WeaponMounts.Count > 0)
                {
                    lstVehicles.Add(objCharacterVehicle);
                }
                else
                {
                    foreach (VehicleMod objVehicleMod in objCharacterVehicle.Mods)
                    {
                        // Only add a Vehicle to the list if it has a Weapon Mount or Mechanical Arm.
                        if (objVehicleMod.Name.Contains("Drone Arm") ||
                            objVehicleMod.Name.StartsWith("Mechanical Arm"))
                        {
                            lstVehicles.Add(objCharacterVehicle);
                            goto NextVehicle;
                        }
                    }

                    NextVehicle:;
                }
            }

            // Cannot continue if there are no Vehicles with a Weapon Mount or Mechanical Arm.
            if (lstVehicles.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotMoveWeapons", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotMoveWeapons", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectItem frmPickItem = new frmSelectItem
            {
                Vehicles = lstVehicles
            };
            frmPickItem.ShowDialog(this);

            if (frmPickItem.DialogResult == DialogResult.Cancel)
                return;

            // Locate the selected Vehicle.
            Vehicle objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == frmPickItem.SelectedItem);
            if (objVehicle == null)
                return;

            // Now display a list of the acceptable mounting points for the Weapon.
            List<ListItem> lstItems = new List<ListItem>();
            foreach (WeaponMount objVehicleWeaponMount in objVehicle.WeaponMounts)
            {
                //TODO: RAW, some mounts can have multiple weapons attached. Needs support in the Weapon Mount class itself, ideally a 'CanMountThisWeapon' bool or something.  
                if ((objVehicleWeaponMount.AllowedWeaponCategories.Contains(objWeapon.SizeCategory) ||
                    objVehicleWeaponMount.AllowedWeapons.Contains(objWeapon.Name)) &&
                    objVehicleWeaponMount.Weapons.Count == 0)
                    lstItems.Add(new ListItem(objVehicleWeaponMount.InternalId,
                        objVehicleWeaponMount.DisplayName(GlobalOptions.Language)));
                else
                    foreach (VehicleMod objVehicleMod in objVehicleWeaponMount.Mods)
                    {
                        if ((objVehicleMod.Name.Contains("Drone Arm") ||
                            objVehicleMod.Name.StartsWith("Mechanical Arm")) &&
                            objVehicleMod.Weapons.Count == 0)
                            lstItems.Add(new ListItem(objVehicleMod.InternalId,
                                objVehicleMod.DisplayName(GlobalOptions.Language)));
                    }
            }
            foreach (VehicleMod objVehicleMod in objVehicle.Mods)
            {
                if ((objVehicleMod.Name.Contains("Drone Arm") ||
                    objVehicleMod.Name.StartsWith("Mechanical Arm")) && objVehicleMod.Weapons.Count == 0)
                    lstItems.Add(new ListItem(objVehicleMod.InternalId, objVehicleMod.DisplayName(GlobalOptions.Language)));
            }

            if (lstItems.Count == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NoValidWeaponMount", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoValidWeaponMount", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmPickItem.GeneralItems = lstItems;
            frmPickItem.ShowDialog(this);

            if (frmPickItem.DialogResult == DialogResult.Cancel)
                return;

            WeaponMount objWeaponMount = objVehicle.WeaponMounts.FirstOrDefault(x => x.InternalId == frmPickItem.SelectedItem);
            // Locate the selected Vehicle Mod.
            VehicleMod objMod = null;
            if (objWeaponMount == null)
            {
                objMod = objVehicle.FindVehicleMod(x => x.InternalId == frmPickItem.SelectedItem, out objWeaponMount);
                if (objMod == null)
                    return;
            }

            objWeapon.Location = null;
            // Remove the Weapon from the character and add it to the Vehicle Mod.
            CharacterObject.Weapons.Remove(objWeapon);

            // Remove any Improvements from the Character.
            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
            {
                foreach (Gear objGear in objAccessory.Gear)
                    objGear.ChangeEquippedStatus(false);
            }
            if (objWeapon.UnderbarrelWeapons.Count > 0)
            {
                foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                {
                    foreach (WeaponAccessory objAccessory in objUnderbarrelWeapon.WeaponAccessories)
                    {
                        foreach (Gear objGear in objAccessory.Gear)
                            objGear.ChangeEquippedStatus(false);
                    }
                }
            }

            if (objWeaponMount != null)
            {
                objWeapon.ParentMount = objWeaponMount;
                objWeaponMount.Weapons.Add(objWeapon);
            }
            else
            {
                objMod.Weapons.Add(objWeapon);
            }
            
            IsDirty = true;
        }

        private void cmdArmorIncrease_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
                return;

            objArmor.ArmorDamage -= 1;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdArmorDecrease_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
                return;

            objArmor.ArmorDamage += 1;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkIncludedInArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            // Locate the selected Armor Modification.
            if (!(treArmor.SelectedNode?.Tag is ArmorMod objMod))
                return;
            if (objMod != null)
                objMod.IncludedInArmor = chkIncludedInArmor.Checked;

            IsDirty = true;
            IsCharacterUpdateRequested = true;
        }

        private void chkCommlinks_CheckedChanged(object sender, EventArgs e)
        {
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
        }

        private void chkGearActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink)) return;

            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkGearActiveCommlink.Checked);
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }


        private void chkCyberwareActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;

            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkVehicleActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;

            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cboGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboGearAttack.Enabled)
                return;

            _blnLoading = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearAttack, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboGearSleaze.Enabled)
                return;

            _blnLoading = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearSleaze, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboGearDataProcessing.Enabled)
                return;

            _blnLoading = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearDataProcessing, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboGearFirewall.Enabled)
                return;

            _blnLoading = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearFirewall, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }

        private void cboVehicleGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearAttack.Enabled)
                return;

            _blnLoading = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboVehicleGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearSleaze.Enabled)
                return;

            _blnLoading = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearSleaze, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboVehicleGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearFirewall.Enabled)
                return;

            _blnLoading = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearFirewall, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboVehicleGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearDataProcessing.Enabled)
                return;

            _blnLoading = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearDataProcessing, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }

        private void cboCyberwareGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboCyberwareGearAttack.Enabled)
                return;

            _blnLoading = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareGearAttack, cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboCyberwareGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboCyberwareGearSleaze.Enabled)
                return;

            _blnLoading = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareGearSleaze, cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboCyberwareGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboCyberwareGearDataProcessing.Enabled)
                return;

            _blnLoading = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareGearDataProcessing, cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboCyberwareGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboCyberwareGearFirewall.Enabled)
                return;

            _blnLoading = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareGearFirewall, cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }

        private void cboWeaponGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboWeaponGearAttack.Enabled)
                return;

            _blnLoading = true;
            
            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboWeaponGearAttack, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboWeaponGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboWeaponGearSleaze.Enabled)
                return;

            _blnLoading = true;
            
            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboWeaponGearSleaze, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboWeaponGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboWeaponGearDataProcessing.Enabled)
                return;

            _blnLoading = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboWeaponGearDataProcessing, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
        private void cboWeaponGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboWeaponGearFirewall.Enabled)
                return;

            _blnLoading = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboWeaponGearFirewall, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            _blnLoading = false;
        }
#endregion

#region Additional Vehicle Tab Control Events
        private void treVehicles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedVehicle();
            RefreshPasteStatus();
        }

        private void treVehicles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Gear objGear)) return;
            _eDragButton = e.Button;
            _blnDraggingGear = true;
            _intDragLevel = treVehicles.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treVehicles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treVehicles_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else if (treVehicles.Nodes.Count > 0)
            {
                intNewIndex = treVehicles.Nodes[treVehicles.Nodes.Count - 1].Nodes.Count;
                nodDestination = treVehicles.Nodes[treVehicles.Nodes.Count - 1];
            }

            if (!_blnDraggingGear)
            {
                CharacterObject.MoveVehicleNode(intNewIndex, nodDestination, treVehicles.SelectedNode);
            }
            else
            {
                if (_eDragButton == MouseButtons.Left)
                    return;
                CharacterObject.MoveVehicleGearParent(nodDestination, treVehicles.SelectedNode);
            }

            // Clear the background color for all Nodes.
            treVehicles.ClearNodeBackground(null);

            _blnDraggingGear = false;

            IsDirty = true;
        }

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
            List<Gear> lstAmmo = new List<Gear>();
            List<string> lstCount = new List<string>();
            bool blnExternalSource = false;

            Gear objExternalSource = new Gear(CharacterObject)
            {
                Name = "External Source"
            };

            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon)) return;
            // Determine which loading methods are available to the Weapon.
            string ammoString = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage);
            if (ammoString.IndexOfAny('x', '+') != -1 || ammoString.Contains(" or ") || ammoString.Contains("Special"))
            {
                string strWeaponAmmo = ammoString.ToLower();
                if (strWeaponAmmo.Contains("external source"))
                    blnExternalSource = true;
                // Get rid of external source, special, or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.Replace("external source", "100")
                    .Replace("special", "100")
                    .FastEscapeOnceFromEnd(" + energy")
                    .FastEscapeOnceFromEnd(" or belt");

                string[] strSplit = { " or " };
                string[] strAmmos = strWeaponAmmo.Split(strSplit, StringSplitOptions.RemoveEmptyEntries);

                foreach (string strAmmo in strAmmos)
                {
                    string strThisAmmo = strAmmo.TrimStartOnce("2x", "3x", "4x").TrimEndOnce("x2", "x3", "x4");

                    int intPos = strThisAmmo.IndexOf('(');
                    if (intPos != -1)
                        strThisAmmo = strThisAmmo.Substring(0, intPos);

                    lstCount.Add(strThisAmmo);
                }
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                int intPos = ammoString.IndexOf('(');
                if (intPos != -1)
                    ammoString = ammoString.Substring(0, intPos);
                lstCount.Add(ammoString);
            }

            // Find all of the Ammo for the current Weapon that the character is carrying.
            HashSet<string> setAmmoPrefixStringSet = new HashSet<string>(objWeapon.AmmoPrefixStrings);
            foreach (Gear objAmmo in objWeapon.ParentVehicle.Gear)
            {
                if (objAmmo.Quantity > 0)
                {
                    if (objAmmo.Category == "Ammunition" && objAmmo.Extra == objWeapon.AmmoCategory ||
                        string.IsNullOrEmpty(objAmmo.Extra) && setAmmoPrefixStringSet.Any(y => objAmmo.Name.StartsWith(y)) ||
                        objWeapon.UseSkill == "Throwing Weapons" && objWeapon.Name == objAmmo.Name)
                        lstAmmo.Add(objAmmo);
                }
            }

            // If the Weapon is allowed to use an External Source, put in an External Source item.
            if (blnExternalSource)
                lstAmmo.Add(objExternalSource);

            // Make sure the character has some form of Ammunition for this Weapon.
            if (lstAmmo.Count == 0 && objWeapon.RequireAmmo)
            {
                MessageBox.Show(LanguageManager.GetString("Message_OutOfAmmoType", GlobalOptions.Language).Replace("{0}", objWeapon.DisplayAmmoCategory(GlobalOptions.Language)), LanguageManager.GetString("MessageTitle_OutOfAmmo", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!objWeapon.RequireAmmo)
            {
                // If the Weapon does not require Ammo, clear the Ammo list and just use External Source.
                lstAmmo.Clear();
                lstAmmo.Add(objExternalSource);
            }

            // Show the Ammunition Selection window.
            frmReload frmReloadWeapon = new frmReload
            {
                Ammo = lstAmmo,
                Count = lstCount
            };
            frmReloadWeapon.ShowDialog(this);

            if (frmReloadWeapon.DialogResult == DialogResult.Cancel)
                return;

            // Return any unspent rounds to the Ammo.
            if (objWeapon.AmmoRemaining > 0)
            {
                foreach (Gear objAmmo in objWeapon.ParentVehicle.Gear)
                {
                    if (objAmmo.InternalId == objWeapon.AmmoLoaded)
                    {
                        objAmmo.Quantity += objWeapon.AmmoRemaining;

                        TreeNode objSelectedNode = treVehicles.FindNode(objAmmo.InternalId);
                        if (objSelectedNode != null)
                            objSelectedNode.Text = objAmmo.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        break;
                    }
                }
            }

            Gear objSelectedAmmo = frmReloadWeapon.SelectedAmmo != objExternalSource.InternalId ? objWeapon.ParentVehicle.Gear.FirstOrDefault(x => x.InternalId == frmReloadWeapon.SelectedAmmo) : null;
            decimal decQty = frmReloadWeapon.SelectedCount;
            // If an External Source is not being used, consume ammo.
            if (objSelectedAmmo != null)
            {
                // Deduct the ammo qty from the ammo. If there isn't enough remaining, use whatever is left.
                if (objSelectedAmmo.Quantity > decQty)
                    objSelectedAmmo.Quantity -= decQty;
                else
                {
                    decQty = objSelectedAmmo.Quantity;
                    objSelectedAmmo.Quantity = 0;
                }

                // Refresh the Vehicle tree.
                TreeNode objSelectedNode = treVehicles.FindNode(objSelectedAmmo.InternalId);
                if (objSelectedNode != null)
                    objSelectedNode.Text = objSelectedAmmo.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else
            {
                objSelectedAmmo = objExternalSource;
            }

            objWeapon.AmmoRemaining = decimal.ToInt32(decQty);
            objWeapon.AmmoLoaded = objSelectedAmmo.InternalId;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if(_blnSkipRefresh)
                return;
            if (!(treVehicles.SelectedNode?.Tag is ICanEquip objEquippable)) return;
            objEquippable.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;

            IsDirty = true;
        }

        private void cboVehicleWeaponAmmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon) || _blnSkipRefresh)
                return;
            objWeapon.ActiveAmmoSlot = Convert.ToInt32(cboVehicleWeaponAmmo.SelectedValue.ToString());
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget) || _blnSkipRefresh) return;
            objTarget.SetHomeNode(CharacterObject, chkVehicleHomeNode.Checked);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endregion

#region Additional Spells and Spirits Tab Control Events
        private void treSpells_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _blnSkipRefresh = true;
            if (e.Node.Tag is Spell objSpell)
            {
                cmdDeleteSpell.Enabled = objSpell.Grade == 0;
                lblSpellDescriptors.Text = objSpell.DisplayDescriptors(GlobalOptions.Language);
                lblSpellCategory.Text = objSpell.DisplayCategory(GlobalOptions.Language);
                lblSpellType.Text = objSpell.DisplayType(GlobalOptions.Language);
                lblSpellRange.Text = objSpell.DisplayRange(GlobalOptions.Language);
                lblSpellDamage.Text = objSpell.DisplayDamage(GlobalOptions.Language);
                lblSpellDuration.Text = objSpell.DisplayDuration(GlobalOptions.Language);
                lblSpellDV.Text = objSpell.DisplayDV(GlobalOptions.Language);
                string strPage = objSpell.DisplayPage(GlobalOptions.Language);
                lblSpellSource.Text = CommonFunctions.LanguageBookShort(objSpell.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellSource, CommonFunctions.LanguageBookLong(objSpell.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                // Determine the size of the Spellcasting Dice Pool.
                lblSpellDicePool.Text = objSpell.DicePool.ToString();
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellDicePool, objSpell.DicePoolTooltip);

                // Build the DV tooltip.
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellDV, objSpell.DVTooltip);

                // Update the Drain CharacterAttribute Value.
                if (CharacterObject.MAGEnabled && !string.IsNullOrEmpty(lblDrainAttributes.Text))
                {
                    string strDrain = lblDrainAttributes.Text;

                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                    {
                        CharacterAttrib objAttrib = CharacterObject.GetAttribute(strAttribute);
                        strDrain = strDrain.CheapReplace(objAttrib.DisplayAbbrev, () => objAttrib.TotalValue.ToString());
                    }
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strDrain, out bool blnIsSuccess);
                    int intDrain = blnIsSuccess ? Convert.ToInt32(objProcess) : 0;
                    intDrain += ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.DrainResistance);

                    string strTip = lblDrainAttributes.Text;

                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                    {
                        CharacterAttrib objAttrib = CharacterObject.GetAttribute(strAttribute);
                        strTip = strTip.CheapReplace(objAttrib.DisplayAbbrev, () => objAttrib.DisplayAbbrev + " (" + objAttrib.TotalValue + ')');
                    }

                    if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.DrainResistance) != 0)
                        strTip += " + " + LanguageManager.GetString("Tip_Skill_DicePoolModifiers", GlobalOptions.Language) + " (" + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.DrainResistance) + ')';
                    //if (objSpell.Limited)
                    //{
                    //    intDrain += 2;
                    //    strTip += " + " + LanguageManager.GetString("String_SpellLimited") + " (2)";
                    //}
                    lblDrainAttributesValue.Text = intDrain.ToString();
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblDrainAttributesValue, strTip);
                }
            }
            else
            {
                cmdDeleteSpell.Enabled = false;
                lblSpellDescriptors.Text = string.Empty;
                lblSpellCategory.Text = string.Empty;
                lblSpellType.Text = string.Empty;
                lblSpellRange.Text = string.Empty;
                lblSpellDamage.Text = string.Empty;
                lblSpellDuration.Text = string.Empty;
                lblSpellDV.Text = string.Empty;
                lblSpellSource.Text = string.Empty;
                lblSpellDicePool.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellSource, null);
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellDV, null);
            }
            _blnSkipRefresh = false;
        }

        private void treFoci_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!e.Node.Checked)
            {
                if (!(e.Node.Tag is IHasInternalId objId)) return;
                Focus objFocus = CharacterObject.Foci.FirstOrDefault(x => x.GearObject.InternalId == objId.InternalId);

                // Mark the Gear as not Bonded and remove any Improvements.
                Gear objGear = objFocus?.GearObject;

                if (objGear != null)
                {
                    objGear.Bonded = false;
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                    CharacterObject.Foci.Remove(objFocus);
                }
                else
                {
                    // This is a Stacked Focus.
                    StackedFocus objStack = CharacterObject.StackedFoci.FirstOrDefault(x => x.InternalId == objId.InternalId);

                    if (objStack != null)
                    {
                        objStack.Bonded = false;
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void treFoci_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            // If the item is being unchecked, confirm that the user wants to un-bind the Focus.
            if (e.Node.Checked)
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_UnbindFocus", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_UnbindFocus", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    e.Cancel = true;
                return;
            }

            // Set the Focus count to 1 and get its current Rating (Force). This number isn't used in the following loops because it isn't yet checked or unchecked.
            int intFociCount = 1;
            int intFociTotal = 0;

            Gear objSelectedFocus = null;

            switch (e.Node.Tag)
            {
                case Gear objGear:
                {
                    objSelectedFocus = (Gear) e.Node.Tag;
                    intFociTotal = objSelectedFocus.Rating;
                    objGear = null;
                    break;
                }
                case StackedFocus objStackedFocus:
                {
                    intFociTotal = objStackedFocus.TotalForce;
                    objStackedFocus = null;
                    break;
                }
            }

            // Run through the list of items. Count the number of Foci the character would have bonded including this one, plus the total Force of all checked Foci.
            foreach (TreeNode objNode in treFoci.Nodes)
            {
                if (objNode.Checked)
                {
                    string strNodeId = objNode.Tag.ToString();
                    intFociCount += 1;
                    intFociTotal += CharacterObject.Gear.FirstOrDefault(x => x.InternalId == strNodeId && x.Bonded)?.Rating ?? 0;
                    intFociTotal += CharacterObject.StackedFoci.FirstOrDefault(x => x.InternalId == strNodeId && x.Bonded)?.TotalForce ?? 0;
                }
            }

            if (!CharacterObject.IgnoreRules)
            {
                if (intFociTotal > CharacterObject.MAG.TotalValue * 5 ||
                    (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && intFociTotal > CharacterObject.MAGAdept.TotalValue * 5))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FocusMaximumForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }

                if (intFociCount > CharacterObject.MAG.TotalValue ||
                    (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && intFociCount > CharacterObject.MAGAdept.TotalValue))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FocusMaximumNumber", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }
            }

            // If we've made it this far, everything is okay, so create a Karma Expense for the newly-bound Focus.

            if (objSelectedFocus != null)
            {
                bool blnOldEquipped = objSelectedFocus.Equipped;
                Focus objFocus = new Focus(CharacterObject)
                {
                    GearObject = objSelectedFocus
                };
                if (objSelectedFocus.Bonus != null || (objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null))
                {
                    if (!string.IsNullOrEmpty(objSelectedFocus.Extra))
                        ImprovementManager.ForcedValue = objSelectedFocus.Extra;
                    if (objSelectedFocus.Bonus != null)
                    {
                        if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.Bonus, false, objSelectedFocus.Rating, objSelectedFocus.DisplayNameShort(GlobalOptions.Language)))
                        {
                            // Clear created improvements
                            objSelectedFocus.ChangeEquippedStatus(false);
                            if (blnOldEquipped)
                                objSelectedFocus.ChangeEquippedStatus(true);
                            e.Cancel = true;
                            return;
                        }
                        objSelectedFocus.Extra = ImprovementManager.SelectedValue;
                    }
                    if (objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null)
                    {
                        if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.WirelessBonus, false, objSelectedFocus.Rating, objSelectedFocus.DisplayNameShort(GlobalOptions.Language)))
                        {
                            // Clear created improvements
                            objSelectedFocus.ChangeEquippedStatus(false);
                            if (blnOldEquipped)
                                objSelectedFocus.ChangeEquippedStatus(true);
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                // Determine how much Karma the Focus will cost to bind.
                string strFocusName = objSelectedFocus.Name;
                string strFocusExtra = objSelectedFocus.Extra;
                int intPosition = strFocusName.IndexOf('(');
                if (intPosition > -1)
                    strFocusName = strFocusName.Substring(0, intPosition - 1);
                intPosition = strFocusName.IndexOf(',');
                if (intPosition > -1)
                    strFocusName = strFocusName.Substring(0, intPosition);
                int intKarmaMultiplier = 1;
                int intExtraKarmaCost = 0;
                switch (strFocusName)
                {
                    case "Qi Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaQiFocus;
                        break;
                    }
                    case "Sustaining Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSustainingFocus;
                        break;
                    }
                    case "Counterspelling Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaCounterspellingFocus;
                        break;
                    }
                    case "Banishing Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaBanishingFocus;
                        break;
                    }
                    case "Binding Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaBindingFocus;
                        break;
                    }
                    case "Weapon Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaWeaponFocus;
                        break;
                    }
                    case "Spellcasting Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSpellcastingFocus;
                        break;
                    }
                    case "Ritual Spellcasting Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaRitualSpellcastingFocus;
                        break;
                    }
                    case "Spell Shaping Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSpellShapingFocus;
                        break;
                    }
                    case "Summoning Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSummoningFocus;
                        break;
                    }
                    case "Alchemical Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaAlchemicalFocus;
                        break;
                    }
                    case "Centering Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaCenteringFocus;
                        break;
                    }
                    case "Masking Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaMaskingFocus;
                        break;
                    }
                    case "Disenchanting Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaDisenchantingFocus;
                        break;
                    }
                    case "Power Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaPowerFocus;
                        break;
                    }
                    case "Flexible Signature Focus":
                    {
                        intKarmaMultiplier = CharacterObjectOptions.KarmaFlexibleSignatureFocus;
                        break;
                    }
                }
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements.Where(x => x.ImprovedName == strFocusName && (string.IsNullOrEmpty(x.Target) || strFocusExtra.Contains(x.Target)) && x.Enabled))
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaCost)
                        intExtraKarmaCost += objLoopImprovement.Value;
                    else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaMultiplier)
                        intKarmaMultiplier += objLoopImprovement.Value;
                }
                int intKarmaExpense = objSelectedFocus.Rating * intKarmaMultiplier + intExtraKarmaCost;
                if (intKarmaExpense > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Clear created improvements
                    objSelectedFocus.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objSelectedFocus.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseFocus", GlobalOptions.Language).Replace("{0}", intKarmaExpense.ToString()).Replace("{1}", objSelectedFocus.DisplayNameShort(GlobalOptions.Language))))
                {
                    // Clear created improvements
                    objSelectedFocus.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objSelectedFocus.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseBound", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objSelectedFocus.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.BindFocus, objSelectedFocus.InternalId);
                objExpense.Undo = objUndo;

                CharacterObject.Foci.Add(objFocus);
                objSelectedFocus.Bonded = true;
                if (!blnOldEquipped)
                {
                    objSelectedFocus.ChangeEquippedStatus(false);
                }

                treFoci.SelectedNode.Text = objSelectedFocus.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else
            {
                // The Focus was not found in Gear, so this is a Stacked Focus.
                if (!(e.Node.Tag is StackedFocus objStackedFocus))
                {
                    e.Cancel = true;
                    return;
                }

                Gear objStackGear = CharacterObject.Gear.DeepFindById(objStackedFocus.GearId);
                if (objStackGear == null)
                {
                    e.Cancel = true;
                    return;
                }
                bool blnOldEquipped = objStackGear.Equipped;
                foreach (Gear objGear in objStackedFocus.Gear)
                {
                    if (objGear.Bonus != null || (objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null))
                    {
                        if (!string.IsNullOrEmpty(objGear.Extra))
                            ImprovementManager.ForcedValue = objGear.Extra;
                        if (objGear.Bonus != null)
                        {
                            if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStackedFocus.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language)))
                            {
                                // Clear created improvements
                                objSelectedFocus.ChangeEquippedStatus(false);
                                if (blnOldEquipped)
                                    objSelectedFocus.ChangeEquippedStatus(true);
                                e.Cancel = true;
                                return;
                            }
                            objGear.Extra = ImprovementManager.SelectedValue;
                        }
                        if (objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null)
                        {
                            if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStackedFocus.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language)))
                            {
                                // Clear created improvements
                                objSelectedFocus.ChangeEquippedStatus(false);
                                if (blnOldEquipped)
                                    objSelectedFocus.ChangeEquippedStatus(true);
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }

                int intKarmaExpense = objStackedFocus.BindingCost;
                if (intKarmaExpense > CharacterObject.Karma)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Clear created improvements
                    objStackGear.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objStackGear.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseFocus", GlobalOptions.Language).Replace("{0}", intKarmaExpense.ToString()).Replace("{1}", LanguageManager.GetString("String_StackedFocus", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objStackedFocus.Name(GlobalOptions.CultureInfo, GlobalOptions.Language))))
                {
                    // Clear created improvements
                    objStackGear.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objStackGear.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseBound", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_StackedFocus", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objStackedFocus.Name(GlobalOptions.CultureInfo, GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.BindFocus, objStackedFocus.InternalId);
                objExpense.Undo = objUndo;

                objStackedFocus.Bonded = true;
                treFoci.SelectedNode.Text = objStackGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cboTradition_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TODO: Why can't IsInitialised be used here? Throws an error when trying to use chummer.helpers.

            if (_blnLoading || string.IsNullOrEmpty(cboTradition.SelectedValue?.ToString()))
                return;
            
            XmlNode objXmlTradition = XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[name = \"" + cboTradition.SelectedValue + "\"]");

            if (objXmlTradition == null)
            {
                cboDrain.Visible = false;
                lblTraditionName.Visible = false;
                txtTraditionName.Visible = false;
                lblSpiritCombat.Visible = false;
                lblSpiritDetection.Visible = false;
                lblSpiritHealth.Visible = false;
                lblSpiritIllusion.Visible = false;
                lblSpiritManipulation.Visible = false;
                lblTraditionSource.Visible = false;
                lblTraditionSourceLabel.Visible = false;
                cboSpiritCombat.Visible = false;
                cboSpiritDetection.Visible = false;
                cboSpiritHealth.Visible = false;
                cboSpiritIllusion.Visible = false;
                cboSpiritManipulation.Visible = false;

                CharacterObject.MagicTradition = cboTradition.SelectedValue.ToString();
                CharacterObject.TraditionDrain = string.Empty;
            }
            else if (objXmlTradition["name"]?.InnerText == "Custom")
            {
                cboDrain.Visible = CharacterObject.AdeptEnabled && !CharacterObject.MagicianEnabled;
                lblTraditionName.Visible = true;
                txtTraditionName.Visible = true;
                lblSpiritCombat.Visible = true;
                lblSpiritDetection.Visible = true;
                lblSpiritHealth.Visible = true;
                lblSpiritIllusion.Visible = true;
                lblSpiritManipulation.Visible = true;
                lblTraditionSource.Visible = false;
                lblTraditionSourceLabel.Visible = false;
                cboSpiritCombat.Visible = true;
                cboSpiritDetection.Visible = true;
                cboSpiritHealth.Visible = true;
                cboSpiritIllusion.Visible = true;
                cboSpiritManipulation.Visible = true;

                CharacterObject.MagicTradition = string.IsNullOrEmpty(txtTraditionName.Text) ? cboTradition.SelectedValue.ToString() : txtTraditionName.Text;
            }
            else
            {
                cboDrain.Visible = !CharacterObject.AdeptEnabled || CharacterObject.MagicianEnabled;
                lblTraditionName.Visible = false;
                txtTraditionName.Visible = false;
                lblSpiritCombat.Visible = false;
                lblSpiritDetection.Visible = false;
                lblSpiritHealth.Visible = false;
                lblSpiritIllusion.Visible = false;
                lblSpiritManipulation.Visible = false;
                lblTraditionSource.Visible = true;
                lblTraditionSourceLabel.Visible = true;
                cboSpiritCombat.Visible = false;
                cboSpiritDetection.Visible = false;
                cboSpiritHealth.Visible = false;
                cboSpiritIllusion.Visible = false;
                cboSpiritManipulation.Visible = false;

                string strSource = objXmlTradition["source"]?.InnerText;
                string strPage = objXmlTradition["altpage"]?.InnerText ?? objXmlTradition["page"]?.InnerText;
                lblTraditionSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblMetatypeSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                CharacterObject.MagicTradition = cboTradition.SelectedValue.ToString();
                CharacterObject.TraditionDrain = objXmlTradition["drain"]?.InnerText;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endregion

#region Additional Sprites and Complex Forms Tab Control Events
        private void treComplexForms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedComplexForm();
        }

        private void cboStream_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedId = cboStream.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            string strDrain = XmlManager.Load("streams.xml").SelectSingleNode("/chummer/traditions/tradition[name = \"" + strSelectedId + "\"]/drain")?.InnerText;
            CharacterObject.TechnomancerFading = !string.IsNullOrEmpty(strDrain) ? strDrain : string.Empty;
            CharacterObject.TechnomancerStream = strSelectedId;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void treComplexForms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteComplexForm_Click(sender, e);
            }
        }
        #endregion

        #region Additional Initiation Tab Control Events
        private void treMetamagic_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch (treMetamagic.SelectedNode?.Tag)
            {
                case Metamagic objMetamagic:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objMetamagic.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objMetamagic.Grade >= 0;
                        string strPage = objMetamagic.Page(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objMetamagic.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objMetamagic.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                        break;
                    }
                case Art objArt:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objArt.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objArt.Grade >= 0;
                        string strPage = objArt.Page(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objArt.Source, GlobalOptions.Language) + ' ' + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objArt.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                        break;
                    }
                case Spell objSpell:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveMetamagic", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objSpell.Grade >= 0;
                        string strPage = objSpell.DisplayPage(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objSpell.Source, GlobalOptions.Language) + ' ' + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objSpell.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                        break;
                    }
                case ComplexForm objComplexForm:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveEcho", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objComplexForm.Grade >= 0;
                        string strPage = objComplexForm.Page(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objComplexForm.Source, GlobalOptions.Language) + ' ' + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objComplexForm.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                        break;
                    }
                case Enhancement objEnhancement:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objEnhancement.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objEnhancement.Grade >= 0;
                        string strPage = objEnhancement.Page(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objEnhancement.Source, GlobalOptions.Language) + ' ' + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objEnhancement.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                        break;
                    }
                default:
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(CharacterObject.MAGEnabled ? "Button_RemoveInitiateGrade" : "Button_RemoveSubmersionGrade", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = true;
                    lblMetamagicSource.Text = string.Empty;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, string.Empty);
                    break;
            }
        }

        private void chkJoinGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || _blnLoading)
                return;

            // Joining a Network does not cost Karma for Technomancers, so this only applies to Magicians/Adepts.
            if (CharacterObject.MAGEnabled)
            {
                if (chkJoinGroup.Checked)
                {
                    int intKarmaExpense = CharacterObjectOptions.KarmaJoinGroup;

                    if (intKarmaExpense > CharacterObject.Karma)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = false;
                        _blnSkipRefresh = false;
                        return;
                    }

                    string strMessage;
                    string strExpense;
                    if (CharacterObject.MAGEnabled)
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseJoinGroup", GlobalOptions.Language);
                        strExpense = LanguageManager.GetString("String_ExpenseJoinGroup", GlobalOptions.Language);
                    }
                    else
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseJoinNetwork", GlobalOptions.Language);
                        strExpense = LanguageManager.GetString("String_ExpenseJoinNetwork", GlobalOptions.Language);
                    }

                    if (!CharacterObject.ConfirmKarmaExpense(strMessage.Replace("{0}", intKarmaExpense.ToString())))
                    {
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = false;
                        _blnSkipRefresh = false;
                        return;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(intKarmaExpense * -1, strExpense, ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Karma -= intKarmaExpense;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.JoinGroup, string.Empty);
                    objExpense.Undo = objUndo;
                }
                else
                {
                    int intKarmaExpense = CharacterObjectOptions.KarmaLeaveGroup;

                    if (intKarmaExpense > CharacterObject.Karma)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = true;
                        _blnSkipRefresh = false;
                        return;
                    }

                    string strMessage;
                    string strExpense;
                    if (CharacterObject.MAGEnabled)
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseLeaveGroup", GlobalOptions.Language);
                        strExpense = LanguageManager.GetString("String_ExpenseLeaveGroup", GlobalOptions.Language);
                    }
                    else
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseLeaveNetwork", GlobalOptions.Language);
                        strExpense = LanguageManager.GetString("String_ExpenseLeaveNetwork", GlobalOptions.Language);
                    }

                    if (!CharacterObject.ConfirmKarmaExpense(strMessage.Replace("{0}", intKarmaExpense.ToString())))
                    {
                        _blnSkipRefresh = true;
                        chkJoinGroup.Checked = true;
                        _blnSkipRefresh = false;
                        return;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(intKarmaExpense * -1, strExpense, ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Karma -= intKarmaExpense;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.LeaveGroup, string.Empty);
                    objExpense.Undo = objUndo;
                }
            }
            CharacterObject.GroupMember = chkJoinGroup.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void txtNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                ((TextBox) sender)?.SelectAll();
            }
        }

        private void txtGroupNotes_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }
#endregion

#region Additional Critter Powers Tab Control Events
        private void treCritterPowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Look for the selected Critter Power.
            if (treCritterPowers.SelectedNode?.Tag is CritterPower objPower)
            {
                cmdDeleteCritterPower.Enabled = objPower.Grade == 0;
                lblCritterPowerName.Text = objPower.DisplayName(GlobalOptions.Language);
                lblCritterPowerCategory.Text = objPower.DisplayCategory(GlobalOptions.Language);
                lblCritterPowerType.Text = objPower.DisplayType(GlobalOptions.Language);
                lblCritterPowerAction.Text = objPower.DisplayAction(GlobalOptions.Language);
                lblCritterPowerRange.Text = objPower.DisplayRange(GlobalOptions.Language);
                lblCritterPowerDuration.Text = objPower.DisplayDuration(GlobalOptions.Language);
                chkCritterPowerCount.Checked = objPower.CountTowardsLimit;
                string strPage = objPower.Page(GlobalOptions.Language);
                lblCritterPowerSource.Text = CommonFunctions.LanguageBookShort(objPower.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCritterPowerSource, CommonFunctions.LanguageBookLong(objPower.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                if (objPower.PowerPoints > 0)
                {
                    lblCritterPowerPointCost.Text = objPower.PowerPoints.ToString(GlobalOptions.CultureInfo);
                    lblCritterPowerPointCost.Visible = true;
                    lblCritterPowerPointCostLabel.Visible = true;
                }
                else
                {
                    lblCritterPowerPointCost.Visible = false;
                    lblCritterPowerPointCostLabel.Visible = false;
                }
            }
            else
            {
                cmdDeleteCritterPower.Enabled = false;
                lblCritterPowerName.Text = string.Empty;
                lblCritterPowerCategory.Text = string.Empty;
                lblCritterPowerType.Text = string.Empty;
                lblCritterPowerAction.Text = string.Empty;
                lblCritterPowerRange.Text = string.Empty;
                lblCritterPowerDuration.Text = string.Empty;
                chkCritterPowerCount.Checked = false;
                lblCritterPowerSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCritterPowerSource, null);
                lblCritterPowerPointCost.Visible = false;
                lblCritterPowerPointCostLabel.Visible = false;
            }
        }

        private void chkCritterPowerCount_CheckedChanged(object sender, EventArgs e)
        {
            // Locate the selected Critter Power.
            if (treCritterPowers.SelectedNode?.Tag is CritterPower objPower)
            {
                objPower.CountTowardsLimit = chkCritterPowerCount.Checked;

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
#endregion

#region Additional Karma and Nuyen Tab Control Events
        private void lstKarma_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem objItem = lstKarma.SelectedItems.Count > 0 ? lstKarma.SelectedItems[0] : null;
            if (objItem == null)
            {
                return;
            }

            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);

            // Find the selected Karma Expense.
            foreach (ExpenseLogEntry objCharacterEntry in CharacterObject.ExpenseEntries)
            {
                if (objCharacterEntry.InternalId == objItem.SubItems[3].Text)
                {
                    objEntry = objCharacterEntry;
                    break;
                }
            }

            // If this is a manual entry, let the player modify the amount.
            int intOldAmount = decimal.ToInt32(objEntry.Amount);
            bool blnAllowEdit = false;
            if (objEntry.Undo != null)
            {
                if (objEntry.Undo.KarmaType == KarmaExpenseType.ManualAdd || objEntry.Undo.KarmaType == KarmaExpenseType.ManualSubtract)
                    blnAllowEdit = true;
            }

            frmExpense frmEditExpense = new frmExpense(CharacterObjectOptions)
            {
                Reason = objEntry.Reason,
                Amount = objEntry.Amount,
                Refund = objEntry.Refund,
                SelectedDate = objEntry.Date
            };
            frmEditExpense.LockFields(blnAllowEdit);

            frmEditExpense.ShowDialog(this);

            if (frmEditExpense.DialogResult == DialogResult.Cancel)
                return;

            // If this is a manual entry, update the character's Karma total.
            int intNewAmount = decimal.ToInt32(frmEditExpense.Amount);
            if (blnAllowEdit && intOldAmount != intNewAmount)
            {
                objEntry.Amount = intNewAmount;
                CharacterObject.Karma += (intNewAmount - intOldAmount);
            }

            // Rename the Expense.
            objEntry.Reason = frmEditExpense.Reason;
            objEntry.Date = frmEditExpense.SelectedDate;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void lstNuyen_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem objItem = lstNuyen.SelectedItems.Count > 0 ? lstNuyen.SelectedItems[0] : null;
            if (objItem == null)
            {
                return;
            }

            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);

            // Find the selected Nuyen Expense.
            foreach (ExpenseLogEntry objCharacterEntry in CharacterObject.ExpenseEntries)
            {
                if (objCharacterEntry.InternalId == objItem.SubItems[3].Text)
                {
                    objEntry = objCharacterEntry;
                    break;
                }
            }

            // If this is a manual entry, let the player modify the amount.
            decimal decOldAmount = objEntry.Amount;
            bool blnAllowEdit = false;
            if (objEntry.Undo != null)
            {
                if (objEntry.Undo.NuyenType == NuyenExpenseType.ManualAdd || objEntry.Undo.NuyenType == NuyenExpenseType.ManualSubtract)
                    blnAllowEdit = true;
            }

            frmExpense frmEditExpense = new frmExpense(CharacterObjectOptions)
            {
                Mode = ExpenseType.Nuyen,
                Reason = objEntry.Reason,
                Amount = objEntry.Amount,
                Refund = objEntry.Refund,
                SelectedDate = objEntry.Date
            };
            frmEditExpense.LockFields(blnAllowEdit);

            frmEditExpense.ShowDialog(this);

            if (frmEditExpense.DialogResult == DialogResult.Cancel)
                return;

            // If this is a manual entry, update the character's Karma total.
            decimal decNewAmount = frmEditExpense.Amount;
            if (blnAllowEdit && decOldAmount != decNewAmount)
            {
                objEntry.Amount = decNewAmount;
                CharacterObject.Nuyen += (decNewAmount - decOldAmount);
            }

            // Rename the Expense.
            objEntry.Reason = frmEditExpense.Reason;
            objEntry.Date = frmEditExpense.SelectedDate;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void lstKarma_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _lvwKarmaColumnSorter.SortColumn)
            {
                _lvwKarmaColumnSorter.Order = _lvwKarmaColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _lvwKarmaColumnSorter.SortColumn = e.Column;
                _lvwKarmaColumnSorter.Order = SortOrder.Ascending;
            }
            lstKarma.Sort();
        }

        private void lstNuyen_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _lvwNuyenColumnSorter.SortColumn)
            {
                _lvwNuyenColumnSorter.Order = _lvwNuyenColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _lvwNuyenColumnSorter.SortColumn = e.Column;
                _lvwNuyenColumnSorter.Order = SortOrder.Ascending;
            }
            lstNuyen.Sort();
        }
#endregion

#region Additional Calendar Tab Control Events
        private void lstCalendar_DoubleClick(object sender, EventArgs e)
        {
            cmdEditWeek_Click(sender, e);
        }
#endregion

#region Additional Improvements Tab Control Events
        private void treImprovements_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _blnSkipRefresh = true;
            if (treImprovements.SelectedNode != null)
            {
                if (treImprovements.SelectedNode.Level == 0)
                {
                    cmdImprovementsEnableAll.Visible = true;
                    cmdImprovementsDisableAll.Visible = true;
                    lblImprovementType.Text = string.Empty;
                    lblImprovementValue.Text = string.Empty;
                    chkImprovementActive.Checked = false;
                    chkImprovementActive.Visible = false;
                }
                else
                {
                    string strSelectedId = treImprovements.SelectedNode?.Tag.ToString();
                    Improvement objImprovement = CharacterObject.Improvements.FirstOrDefault(x => x.SourceName == strSelectedId);

                    if (objImprovement != null)
                    {
                        // Get the human-readable name of the Improvement from the Improvements file.

                        XmlNode objNode = XmlManager.Load("improvements.xml").SelectSingleNode("/chummer/improvements/improvement[id = \"" + objImprovement.CustomId + "\"]");
                        if (objNode != null)
                        {
                            lblImprovementType.Text = objNode["translate"]?.InnerText ?? objNode["name"]?.InnerText;
                        }

                        // Build a string that contains the value(s) of the Improvement.
                        string strValue = string.Empty;
                        if (objImprovement.Value != 0)
                            strValue += LanguageManager.GetString("Label_CreateImprovementValue", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objImprovement.Value + ", ";
                        if (objImprovement.Minimum != 0)
                            strValue += LanguageManager.GetString("Label_CreateImprovementMinimum", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objImprovement.Minimum + ", ";
                        if (objImprovement.Maximum != 0)
                            strValue += LanguageManager.GetString("Label_CreateImprovementMaximum", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objImprovement.Maximum + ", ";
                        if (objImprovement.Augmented != 0)
                            strValue += LanguageManager.GetString("Label_CreateImprovementAugmented", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objImprovement.Augmented + ", ";

                        // Remove the trailing comma.
                        if (!string.IsNullOrEmpty(strValue))
                            strValue = strValue.Substring(0, strValue.Length - 2);

                        cmdImprovementsEnableAll.Visible = false;
                        cmdImprovementsDisableAll.Visible = false;
                        lblImprovementValue.Text = strValue;
                        chkImprovementActive.Checked = objImprovement.Enabled;
                        chkImprovementActive.Visible = true;
                    }
                    else
                    {
                        cmdImprovementsEnableAll.Visible = false;
                        cmdImprovementsDisableAll.Visible = false;
                        lblImprovementType.Text = string.Empty;
                        lblImprovementValue.Text = string.Empty;
                        chkImprovementActive.Checked = false;
                        chkImprovementActive.Visible = false;
                    }
                }
            }
            else
            {
                cmdImprovementsEnableAll.Visible = false;
                cmdImprovementsDisableAll.Visible = false;
                lblImprovementType.Text = string.Empty;
                lblImprovementValue.Text = string.Empty;
                chkImprovementActive.Checked = false;
                chkImprovementActive.Visible = false;
            }
            _blnSkipRefresh = false;
        }

        private void treImprovements_DoubleClick(object sender, EventArgs e)
        {
            if (treImprovements.SelectedNode?.Tag is Improvement objImprovement)
            {
                // Edit the selected Improvement.
                frmCreateImprovement frmPickImprovement = new frmCreateImprovement(CharacterObject)
                {
                    EditImprovementObject = objImprovement
                };
                frmPickImprovement.ShowDialog(this);

                if (frmPickImprovement.DialogResult == DialogResult.Cancel) return;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void chkImprovementActive_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (treImprovements.SelectedNode?.Level > 0)
            {
                string strSelectedId = treImprovements.SelectedNode?.Tag.ToString();
                Improvement objImprovement = CharacterObject.Improvements.FirstOrDefault(x => x.SourceName == strSelectedId);

                if (objImprovement != null)
                {
                    if (chkImprovementActive.Checked)
                        ImprovementManager.EnableImprovements(CharacterObject, new List<Improvement> { objImprovement });
                    else
                        ImprovementManager.DisableImprovements(CharacterObject, new List<Improvement> { objImprovement });

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void treImprovements_ItemDrag(object sender, ItemDragEventArgs e)
        {
                // Do not allow the root element to be moved.
            if (treImprovements.SelectedNode == null || treImprovements.SelectedNode?.Tag.ToString() == "Node_SelectedImprovements")
                    return;
            _intDragLevel = treImprovements.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treImprovements_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treImprovements_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            int intNewIndex = 0;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
            {
                int intNodeCount = treImprovements.Nodes.Count;
                if (intNodeCount > 0)
                {
                    intNewIndex = treImprovements.Nodes[intNodeCount - 1].Nodes.Count;
                    nodDestination = treImprovements.Nodes[intNodeCount - 1];
                }
            }

            if (treImprovements.SelectedNode.Level == 1)
                CharacterObject.MoveImprovementNode(intNewIndex, nodDestination, treImprovements.SelectedNode);
            else
                CharacterObject.MoveImprovementRoot(intNewIndex, nodDestination, treImprovements.SelectedNode);

            // Clear the background color for all Nodes.
            treImprovements.ClearNodeBackground(null);

            IsDirty = true;
        }

        private void treImprovements_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode objNode = ((TreeView)sender).GetNodeAt(pt);

            if (objNode == null)
                return;

            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
            if (objNode.Level <= _intDragLevel)
                objNode.BackColor = SystemColors.ControlDark;

            // Clear the background colour for all other Nodes.
            treImprovements.ClearNodeBackground(objNode);
        }

        private void cmdAddImprovementGroup_Click(object sender, EventArgs e)
        {
            // Add a new location to the Improvements Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;

            string strLocation = frmPickText.SelectedValue;
            CharacterObject.ImprovementGroups.Add(strLocation);

            IsDirty = true;
        }
#endregion
        
#region Notes Tab Events
        private void txtGameNotes_TextChanged(object sender, EventArgs e)
        {
            CharacterObject.GameNotes = txtGameNotes.Text;
            IsDirty = true;
        }
#endregion

#region Tree KeyDown Events
        private void treQualities_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteQuality_Click(sender, e);
            }
        }

        private void treSpells_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteSpell_Click(sender, e);
            }
        }

        private void treCyberware_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteCyberware_Click(sender, e);
            }
        }

        private void treLifestyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteLifestyle_Click(sender, e);
            }
        }

        private void treArmor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteArmor_Click(sender, e);
            }
        }

        private void treWeapons_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteWeapon_Click(sender, e);
            }
        }

        private void treGear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteGear_Click(sender, e);
            }
        }

        private void treVehicles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteVehicle_Click(sender, e);
            }
        }

        private void treMartialArts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteMartialArt_Click(sender, e);
            }
        }

        private void treCritterPowers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteCritterPower_Click(sender, e);
            }
        }

        private void treMetamagic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteMetamagic_Click(sender, e);
            }
        }

        private void treImprovements_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteImprovement_Click(sender, e);
            }
        }
#endregion

#region Splitter Resize Events
        private void splitKarmaNuyen_Panel1_Resize(object sender, EventArgs e)
        {
            lstKarma.Width = splitKarmaNuyen.Panel1.Width;
            chtKarma.Width = splitKarmaNuyen.Panel1.Width;
            chtKarma.Height = 210;
            chtKarma.Top = splitKarmaNuyen.Panel1.Height - 6 - chtKarma.Height;
            lstKarma.Height = chtKarma.Top - 6 - lstKarma.Top;
            if (lstKarma.Columns.Count > 2)
            {
                if (lstKarma.Width > 409)
                {
                    lstKarma.Columns[2].Width = lstKarma.Width - 220;
                }
            }
        }

        private void splitKarmaNuyen_Panel2_Resize(object sender, EventArgs e)
        {
            lstNuyen.Width = splitKarmaNuyen.Panel2.Width;
            chtNuyen.Width = splitKarmaNuyen.Panel2.Width;
            chtNuyen.Height = 210;
            chtNuyen.Top = splitKarmaNuyen.Panel2.Height - 6 - chtNuyen.Height;
            lstNuyen.Height = chtNuyen.Top - 6 - lstNuyen.Top;
            if (lstNuyen.Columns.Count > 2)
            {
                if (lstNuyen.Width > 409)
                {
                    lstNuyen.Columns[2].Width = lstNuyen.Width - 220;
                }
            }
        }
#endregion

#region Other Control Events
        private void cmdEdgeSpent_Click(object sender, EventArgs e)
        {
            int intEdgeUsed = 0;
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == "EDG" && objImprovement.ImproveSource == Improvement.ImprovementSource.EdgeUse && objImprovement.Enabled)
                    intEdgeUsed += objImprovement.Augmented * objImprovement.Rating;
            }

            if (intEdgeUsed - 1 < CharacterObject.EDG.Value * -1)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotSpendEdge", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotSpendEdge", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.EdgeUse);
            intEdgeUsed -= 1;

            ImprovementManager.CreateImprovement(CharacterObject, "EDG", Improvement.ImprovementSource.EdgeUse, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, intEdgeUsed);
            ImprovementManager.Commit(CharacterObject);
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdEdgeGained_Click(object sender, EventArgs e)
        {
            int intEdgeUsed = 0;
            foreach (Improvement objImprovement in CharacterObject.Improvements)
            {
                if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == "EDG" && objImprovement.ImproveSource == Improvement.ImprovementSource.EdgeUse && objImprovement.Enabled)
                    intEdgeUsed += objImprovement.Augmented * objImprovement.Rating;
            }

            if (intEdgeUsed + 1 > 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotRegainEdge", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRegainEdge", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.EdgeUse);
            intEdgeUsed += 1;

            if (intEdgeUsed < 0)
            {
                ImprovementManager.CreateImprovement(CharacterObject, "EDG", Improvement.ImprovementSource.EdgeUse, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, intEdgeUsed);
                ImprovementManager.Commit(CharacterObject);
            }
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tabCharacterTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus();
        }

        private void tabStreetGearTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus();
        }
#endregion

#region Condition Monitors
        private void chkPhysicalCM_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => CharacterObject.PhysicalCMFilled = i);
        }

        private void chkStunCM_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => CharacterObject.StunCMFilled = i);
        }

        /// <summary>
        /// Manages the rendering of condition monitor checkboxes for characters that can have modifiers like overflow and threshold offsets.
        /// </summary>
        /// <param name="pnlConditionMonitorPanel">Container panel for the condition monitor checkboxes.</param>
        /// <param name="intConditionMax">Highest value of the condition monitor type.</param>
        /// <param name="intThreshold">Show an increase in modifiers every <paramref name="intThreshold"/> boxes.</param>
        /// <param name="intThresholdOffset">Initial threshold for penalties from <paramref name="intThreshold"/> should be offset by this much.</param>
        /// <param name="intOverflow">Number of overflow boxes to show (set to 0 if none, like for the stun condition monitor).</param>
        /// <param name="check">Whether or not to check the checkbox when finished processing. Expected to only be called on load.</param>
        /// <param name="value">Tag value of the checkbox to enable when using the check parameter. Expected to be the StunCMFilled or PhysicalCMFilled properties.</param>
        private void ProcessCharacterConditionMonitorBoxDisplays(Control pnlConditionMonitorPanel, int intConditionMax, int intThreshold, int intThresholdOffset, int intOverflow, bool check = false, int value = 0)
        {
            pnlConditionMonitorPanel.SuspendLayout();
            CheckBox currentBox = null;
            if (intConditionMax > 0)
            {
                pnlConditionMonitorPanel.Visible = true;
                foreach (CheckBox chkCmBox in pnlConditionMonitorPanel.Controls.OfType<CheckBox>())
                {
                    int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag);
                    if (intCurrentBoxTag == value && check)
                        currentBox = chkCmBox;
                    if (intCurrentBoxTag <= intConditionMax)
                    {
                        chkCmBox.Visible = true;
                        if (intCurrentBoxTag > intThresholdOffset && (intCurrentBoxTag - intThresholdOffset) % intThreshold == 0)
                        {
                            int intModifiers = (intThresholdOffset - intCurrentBoxTag) / intThreshold;
                            chkCmBox.Text = intModifiers.ToString();
                        }
                        else
                            chkCmBox.Text = string.Empty;
                    }
                    else if (intOverflow != 0 && intCurrentBoxTag <= intConditionMax + intOverflow)
                    {
                        chkCmBox.Visible = true;
                        chkCmBox.BackColor = SystemColors.ControlDark;
                        chkCmBox.Text = intCurrentBoxTag == intConditionMax + intOverflow ? "D" : string.Empty;
                    }
                    else
                    {
                        chkCmBox.Visible = false;
                        chkCmBox.Text = string.Empty;
                    }
                }
                if (currentBox != null && check)
                {
                    currentBox.Checked = true;
                }
            }
            else
            {
                pnlConditionMonitorPanel.Visible = false;
            }
            pnlConditionMonitorPanel.ResumeLayout();
        }

        /// <summary>
        /// Manages the rendering of condition monitor checkboxes for characters that can have modifiers like overflow and threshold offsets.
        /// </summary>
        /// <param name="pnlConditionMonitorPanel">Container panel for the condition monitor checkboxes.</param>
        /// <param name="intConditionMax">Highest value of the condition monitor type.</param>
        /// <param name="intCurrentConditionFilled">Current amount of boxes that should be filled in the condition monitor.</param>
        private void ProcessEquipmentConditionMonitorBoxDisplays(Control pnlConditionMonitorPanel, int intConditionMax, int intCurrentConditionFilled)
        {
            bool blnOldSkipRefresh = _blnSkipRefresh;
            _blnSkipRefresh = true;

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

            _blnSkipRefresh = blnOldSkipRefresh;
        }

        /// <summary>
        /// Changes which boxes are filled and unfilled in a condition monitor when a box in that condition monitor is clicked.
        /// </summary>
        /// <param name="chkSender">Checkbox we're currently changing.</param>
        /// <param name="blnDoUIUpdate">Whether to update all the other boxes in the UI or not. If something like ProcessEquipmentConditionMonitorBoxDisplays would be called later, this can be false.</param>
        /// <param name="funcPropertyToUpdate">Function to run once the condition monitor has been processed, probably a property setter. Uses the amount of filled boxes as its argument.</param>
        private void ProcessConditionMonitorCheckedChanged(CheckBox chkSender, Action<int> funcPropertyToUpdate = null, bool blnDoUIUpdate = true)
        {
            if (_blnSkipRefresh)
                return;

            if (blnDoUIUpdate)
            {
                Control pnlConditionMonitorPanel = chkSender.Parent;

                if (pnlConditionMonitorPanel == null)
                    return;

                int intBoxTag = Convert.ToInt32(chkSender.Tag);

                int intFillCount = chkSender.Checked ? 1 : 0;

                // If this is being checked, make sure everything before it is checked off.
                _blnSkipRefresh = true;

                pnlConditionMonitorPanel.SuspendLayout();
                foreach (CheckBox chkCmBox in pnlConditionMonitorPanel.Controls.OfType<CheckBox>())
                {
                    if (chkCmBox != chkSender)
                    {
                        int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag);
                        if (intCurrentBoxTag < intBoxTag)
                        {
                            chkCmBox.Checked = true;
                            intFillCount += 1;
                        }
                        else if (intCurrentBoxTag > intBoxTag)
                        {
                            chkCmBox.Checked = false;
                        }
                    }
                }

                pnlConditionMonitorPanel.ResumeLayout();

                funcPropertyToUpdate?.Invoke(intFillCount);

                _blnSkipRefresh = false;
            }
            else
            {
                int intFillCount = Convert.ToInt32(chkSender.Tag);
                if (!chkSender.Checked)
                    intFillCount -= 1;
                funcPropertyToUpdate?.Invoke(intFillCount);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkCyberwareCM_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedId = treCyberware.SelectedNode?.Tag?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            // Locate the selected Cyberware.
            IHasMatrixAttributes objItem = CharacterObject.Cyberware.DeepFindById(strSelectedId) ??
                                           (IHasMatrixAttributes) CharacterObject.Cyberware.FindCyberwareGear(strSelectedId);

            if (objItem != null && sender is CheckBox objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        private void chkGearCM_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            // Locate the selected Gear.
            TreeNode objGearNode = treGear.SelectedNode;
            while (objGearNode?.Level > 1)
                objGearNode = objGearNode.Parent;

            if (!(objGearNode?.Tag is Gear objGear))
                return;

            if (objGear != null && sender is CheckBox objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objGear.MatrixCMFilled = i, false);
        }

        private void chkWeaponCM_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedId = treCyberware.SelectedNode?.Tag?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            // Locate the selected Weapon.
            IHasMatrixAttributes objItem = CharacterObject.Weapons.FindWeaponGear(strSelectedId) ??
                                           (IHasMatrixAttributes) (CharacterObject.Weapons.DeepFindById(strSelectedId) ??
                                                                   CharacterObject.Weapons.FindWeaponAccessory(strSelectedId)?.Parent);

            if (objItem != null && sender is CheckBox objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        private void chkVehicleCM_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            // Locate the selected Vehicle.
            TreeNode objVehicleNode = treVehicles.SelectedNode;
            while (objVehicleNode?.Level > 1)
                objVehicleNode = objVehicleNode.Parent;

            if (!(objVehicleNode?.Tag is Vehicle objVehicle))
                return;

            if (sender is CheckBox objBox)
            {
                if (panVehicleCM.SelectedIndex == 0)
                {
                    ProcessConditionMonitorCheckedChanged(objBox, i => objVehicle.PhysicalCMFilled = i, false);
                }
                else
                {
                    ProcessConditionMonitorCheckedChanged(objBox, i => objVehicle.MatrixCMFilled = i, false);
                }
            }
        }
#endregion

#region Custom Methods
        /// <summary>
        /// Refresh the fields related to the character's metatype.
        /// </summary>
        public void RefreshMetatypeFields()
        {
            XmlNode objMetatypeNode = XmlManager.Load("metatypes.xml").SelectSingleNode("/chummer/metatypes/metatype[name = \"" + CharacterObject.Metatype + "\"]") ??
               XmlManager.Load("critters.xml").SelectSingleNode("/chummer/metatypes/metatype[name = \"" + CharacterObject.Metatype + "\"]");

            string strMetatype = objMetatypeNode?["translate"]?.InnerText ?? CharacterObject.Metatype;
            string strSource = objMetatypeNode?["source"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strPage = objMetatypeNode?["altpage"]?.InnerText ?? objMetatypeNode?["page"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

            if (!string.IsNullOrEmpty(CharacterObject.Metavariant) && CharacterObject.Metavariant != "None")
            {
                objMetatypeNode = objMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + CharacterObject.Metavariant + "\"]");

                strMetatype += strSpaceCharacter + '(' + (objMetatypeNode?["translate"]?.InnerText ?? CharacterObject.Metavariant) + ')';

                if (objMetatypeNode != null)
                {
                    strSource = objMetatypeNode["source"]?.InnerText ?? strSource;
                    strPage = objMetatypeNode["altpage"]?.InnerText ?? objMetatypeNode["page"]?.InnerText ?? strPage;
                }
            }
            lblMetatype.Text = strMetatype;
            lblMetatypeSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMetatypeSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);

            mnuSpecialConvertToFreeSprite.Visible = CharacterObject.IsSprite;

            mnuSpecialCyberzombie.Visible = CharacterObject.MetatypeCategory != "Cyberzombie";

            if (CharacterObject.Possessed)
                lblPossessed.Text = LanguageManager.GetString("String_Possessed", GlobalOptions.Language);
            else
                lblPossessed.Visible = false;
        }
        
        private void LiveUpdateFromCharacterFile(object sender, EventArgs e)
        {
            if (IsDirty || !GlobalOptions.LiveUpdateCleanCharacterFiles || _blnLoading || _blnSkipUpdate || IsCharacterUpdateRequested)
                return;

            string strCharacterFile = CharacterObject.FileName;
            if (string.IsNullOrEmpty(strCharacterFile) || !File.Exists(strCharacterFile))
                return;

            if (File.GetLastWriteTimeUtc(strCharacterFile) <= CharacterObject.FileLastWriteTime)
                return;

            _blnSkipUpdate = true;

            // Character is not dirty and their savefile was updated outside of Chummer5 while it is open, so reload them
            Cursor = Cursors.WaitCursor;

            CharacterObject.Load();

            // Update character information fields.
            RefreshMetatypeFields();

            IsCharacterUpdateRequested = true;
            _blnSkipUpdate = false;
            // Immediately call character update because we know it's necessary
            UpdateCharacterInfo();

            IsDirty = false;

            Cursor = Cursors.Default;

            if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0)
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_ImprovementLoadError", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_ImprovementLoadError", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                    CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
                }
            }
        }

        /// <summary>
        /// Update the Character information.
        /// </summary>
        private void UpdateCharacterInfo(object sender = null, EventArgs e = null)
        {
            if (_blnLoading || _blnSkipUpdate || !IsCharacterUpdateRequested)
                return;

            _blnSkipUpdate = true;

            // TODO: DataBind these wherever possible
            
            string strModifiers = LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language);

            // Condition Monitor.
            int intCMPhysical = CharacterObject.PhysicalCM;
            int intCMStun = CharacterObject.StunCM;
            int intCMOverflow = CharacterObject.CMOverflow;
            int intCMThreshold = CharacterObject.CMThreshold;
            int intStunCMThresholdOffset = CharacterObject.StunCMThresholdOffset;
            int intPhysicalCMThresholdOffset = CharacterObject.PhysicalCMThresholdOffset;

            // Update the Condition Monitor labels.
            bool blnIsAI = CharacterObject.IsAI;
            if (blnIsAI)
            {
                if (CharacterObject.HomeNode == null)
                {
                    lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherCoreCM", GlobalOptions.Language);
                    lblCMStunLabel.Text = string.Empty;
                    lblCMStun.Visible = false;

                    string strCM = $"8 + ({CharacterObject.DEP.DisplayAbbrev}/2)({(CharacterObject.DEP.TotalValue + 1) / 2})";

                    int intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                    if (intBonus != 0)
                        strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';

                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStun, string.Empty);
                }
                else
                {
                    lblCMStunLabel.Text = LanguageManager.GetString("Label_OtherMatrixCM", GlobalOptions.Language);
                    lblCMStun.Visible = true;

                    string strCM = $"8 + ({LanguageManager.GetString("String_DeviceRating", GlobalOptions.Language)}/2)({(CharacterObject.HomeNode.GetTotalMatrixAttribute("Device Rating") + 1) / 2})";

                    int intBonus = CharacterObject.HomeNode.TotalBonusMatrixBoxes;
                    if (intBonus != 0)
                        strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';

                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);

                    if (CharacterObject.HomeNode is Vehicle objVehicleHomeNode)
                    {
                        lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherPhysicalCM", GlobalOptions.Language);
                        strCM = $"{objVehicleHomeNode.BasePhysicalBoxes} + ({CharacterObject.BOD.DisplayAbbrev}/2)({(objVehicleHomeNode.TotalBody + 1) / 2})";

                        intBonus = objVehicleHomeNode.Mods.Sum(objMod => objMod.ConditionMonitor);
                        if (intBonus != 0)
                            strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';

                        GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    }
                    else
                    {
                        lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherCoreCM", GlobalOptions.Language);
                        strCM = $"8 + ({CharacterObject.DEP.DisplayAbbrev}/2)({(CharacterObject.DEP.TotalValue + 1) / 2})";

                        intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                        if (intBonus != 0)
                            strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';

                        GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    }
                }
            }
            else
            {
                lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherPhysicalCM", GlobalOptions.Language);
                lblCMStunLabel.Text = LanguageManager.GetString("Label_OtherStunCM", GlobalOptions.Language);
                lblCMStun.Visible = true;
                string strCM = $"8 + ({CharacterObject.BOD.DisplayAbbrev}/2)({(CharacterObject.BOD.TotalValue + 1) / 2})";

                int intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                if (intBonus != 0)
                    strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);

                strCM = $"8 + ({CharacterObject.WIL.DisplayAbbrev}/2)({(CharacterObject.WIL.TotalValue + 1) / 2})";
                intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.StunCM);
                if (intBonus != 0)
                    strCM += " + " + strModifiers + " (" + intBonus.ToString() + ')';
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStun, strCM);
            }

            ProcessCharacterConditionMonitorBoxDisplays(panPhysicalCM, intCMPhysical, intCMThreshold, intPhysicalCMThresholdOffset, intCMOverflow);
            ProcessCharacterConditionMonitorBoxDisplays(panStunCM, intCMStun, intCMThreshold, intStunCMThresholdOffset, 0);
            
            int intINTAttributeModifiers = CharacterObject.INT.AttributeModifiers;
            int intREAAttributeModifiers = CharacterObject.REA.AttributeModifiers;

            // Initiative.
            lblINI.Text = CharacterObject.Initiative;
            string strInitText = LanguageManager.GetString("String_Initiative", GlobalOptions.Language);
            string strMatrixInitText = LanguageManager.GetString("String_MatrixInitiativeLong", GlobalOptions.Language);
            string strInit =
                $"{CharacterObject.REA.DisplayAbbrev} ({CharacterObject.REA.Value}) + {CharacterObject.INT.DisplayAbbrev} ({CharacterObject.INT.Value})";
            if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Initiative) > 0 ||
                intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += " + " + strModifiers + " (" +
                           (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Initiative) +
                            intINTAttributeModifiers + intREAAttributeModifiers) + ')';
            GlobalOptions.ToolTipProcessor.SetToolTip(lblINI,
                strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString()));

            // Astral Initiative.
            lblAstralINI.Text = CharacterObject.AstralInitiative;
            if (CharacterObject.MAGEnabled)
            {
                strInit = $"{CharacterObject.INT.DisplayAbbrev} ({CharacterObject.INT.Value}) x 2";
                if (intINTAttributeModifiers > 0)
                    strInit += $"{strModifiers} ({intINTAttributeModifiers})";
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINI,
                    strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.AstralInitiativeDice.ToString()));
            }
            else
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINI, string.Empty);

            // Matrix Initiative (AR).
            lblMatrixINI.Text = CharacterObject.MatrixInitiative;
            strInit =
                $"{CharacterObject.REA.DisplayAbbrev} ({CharacterObject.REA.Value}) + {CharacterObject.INT.DisplayAbbrev} ({CharacterObject.INT.Value})";
            if (intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += $"{strModifiers} ({intREAAttributeModifiers + intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINI,
                strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString()));

            // Matrix Initiative (Cold).
            lblMatrixINICold.Text = CharacterObject.MatrixInitiativeCold;
            strInit = strMatrixInitText.Replace("{0}", CharacterObject.INT.Value.ToString())
                .Replace("{1}", CharacterObject.MatrixInitiativeColdDice.ToString());
            if (intINTAttributeModifiers > 0)
                strInit += $"{strModifiers} ({intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINICold, strInit);

            // Matrix Initiative (Hot).
            lblMatrixINIHot.Text = CharacterObject.MatrixInitiativeHot;
            strInit = strMatrixInitText.Replace("{0}", CharacterObject.INT.Value.ToString())
                .Replace("{1}", CharacterObject.MatrixInitiativeHotDice.ToString());
            if (intINTAttributeModifiers > 0)
                strInit += $"{strModifiers} ({intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINIHot, strInit);

            // Rigger Initiative.
            lblRiggingINI.Text = CharacterObject.Initiative;
            strInit =
                $"{CharacterObject.REA.DisplayAbbrev} ({CharacterObject.REA.Value}) + {CharacterObject.INT.DisplayAbbrev} ({CharacterObject.INT.Value})";
            if (intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += $"{strModifiers} ({intREAAttributeModifiers + intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblRiggingINI,
                strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString()));

            if ((CharacterObject.Metatype == "Free Spirit" && !CharacterObject.IsCritter) ||
                CharacterObject.MetatypeCategory.EndsWith("Spirits"))
            {
                lblCritterPowerPointsLabel.Visible = true;
                lblCritterPowerPoints.Visible = true;
                lblCritterPowerPoints.Text = CharacterObject.CalculateFreeSpiritPowerPoints();
            }
            else if (CharacterObject.IsFreeSprite)
            {
                lblCritterPowerPointsLabel.Visible = true;
                lblCritterPowerPoints.Visible = true;
                lblCritterPowerPoints.Text = CharacterObject.CalculateFreeSpritePowerPoints();
            }                               
            
            PopulateExpenseList(null, EventArgs.Empty);

            // If the Viewer window is open for this character, call its RefreshView method which updates it asynchronously
            PrintWindow?.RefreshCharacters();
            if (Program.MainForm.PrintMultipleCharactersForm?.CharacterList?.Contains(CharacterObject) == true)
                Program.MainForm.PrintMultipleCharactersForm.PrintViewForm?.RefreshCharacters();
            
            UpdateInitiationCost(this, EventArgs.Empty);
            UpdateQualityLevelValue(treQualities.SelectedNode?.Tag as Quality);
            
            RefreshSelectedCyberware();
            RefreshSelectedArmor();
            RefreshSelectedGear();
            RefreshSelectedLifestyle();
            RefreshSelectedVehicle();
            RefreshSelectedWeapon();

            if (AutosaveStopWatch.Elapsed.Minutes >= 5 && IsDirty)
            {
                AutoSaveCharacter();
            }
            _blnSkipUpdate = false;
            IsCharacterUpdateRequested = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed piece of Cyberware.
        /// </summary>
        public void RefreshSelectedCyberware()
        {
            _blnSkipRefresh = true;
            cboCyberwareGearAttack.Visible = false;
            cboCyberwareGearSleaze.Visible = false;
            cboCyberwareGearDataProcessing.Visible = false;
            cboCyberwareGearFirewall.Visible = false;
            cboCyberwareGearOverclocker.Visible = false;
            lblCyberDeviceRating.Visible = false;
            lblCyberDeviceRatingLabel.Visible = false;
            lblCyberAttackLabel.Visible = false;
            lblCyberSleazeLabel.Visible = false;
            lblCyberDataProcessingLabel.Visible = false;
            lblCyberFirewallLabel.Visible = false;
            cmdDeleteCyberware.Enabled = treCyberware.SelectedNode != null && treCyberware.SelectedNode.Level != 0;
            cmdCyberwareChangeMount.Visible = false;
            tabCyberwareCM.Visible = false;

            chkCyberwareActiveCommlink.Visible = false;
            chkCyberwareHomeNode.Visible = false;

            if (treCyberware.SelectedNode == null || treCyberware.SelectedNode.Level == 0)
            {
                lblCyberwareName.Text = string.Empty;
                lblCyberwareCategory.Text = string.Empty;
                lblCyberwareRating.Text = string.Empty;
                lblCyberwareAvail.Text = string.Empty;
                lblCyberwareCost.Text = string.Empty;
                lblCyberwareCapacity.Text = string.Empty;
                lblCyberwareEssence.Text = string.Empty;
                lblCyberwareSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCyberwareSource, null);
                lblCyberlimbAGI.Visible = false;
                lblCyberlimbAGILabel.Visible = false;
                lblCyberlimbSTR.Visible = false;
                lblCyberlimbSTRLabel.Visible = false;
                _blnSkipRefresh = false;
                return;
            }

            string strESSFormat = CharacterObjectOptions.EssenceFormat;

            // Locate the selected piece of Cyberware.
            if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (!string.IsNullOrEmpty(objCyberware.ParentID))
                    cmdDeleteCyberware.Enabled = false;
                cmdCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                lblCyberwareName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblCyberwareCategory.Text = objCyberware.DisplayCategory(GlobalOptions.Language);
                string strPage = objCyberware.Page(GlobalOptions.Language);
                lblCyberwareSource.Text = CommonFunctions.LanguageBookShort(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCyberwareSource, CommonFunctions.LanguageBookLong(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                lblCyberwareRating.Text = objCyberware.Rating.ToString();

                lblCyberwareGrade.Text = objCyberware.Grade.DisplayName(GlobalOptions.Language);

                if (objCyberware.Category.Equals("Cyberlimb"))
                {
                    lblCyberlimbAGI.Visible = true;
                    lblCyberlimbAGILabel.Visible = true;
                    lblCyberlimbSTR.Visible = true;
                    lblCyberlimbSTRLabel.Visible = true;

                    lblCyberlimbAGI.Text = objCyberware.TotalAgility.ToString();
                    lblCyberlimbSTR.Text = objCyberware.TotalStrength.ToString();
                }
                else
                {
                    lblCyberlimbAGI.Visible = false;
                    lblCyberlimbAGILabel.Visible = false;
                    lblCyberlimbSTR.Visible = false;
                    lblCyberlimbSTRLabel.Visible = false;
                }

                if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                {
                    // Locate the selected Cyberware.
                    tabCyberwareCM.Visible = (treCyberware.SelectedNode?.Tag is IHasMatrixConditionMonitor);

                    if (treCyberware.SelectedNode?.Tag is IHasMatrixConditionMonitor objMatrixCM)
                    {
                        ProcessEquipmentConditionMonitorBoxDisplays(tabCyberwareCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
                    }

                    lblCyberDeviceRating.Text = objCyberware.GetTotalMatrixAttribute("Device Rating").ToString();
                    lblCyberDeviceRating.Visible = true;
                    lblCyberDeviceRatingLabel.Visible = true;
                    lblCyberAttackLabel.Visible = true;
                    lblCyberSleazeLabel.Visible = true;
                    lblCyberDataProcessingLabel.Visible = true;
                    lblCyberFirewallLabel.Visible = true;
                    objCyberware.RefreshMatrixAttributeCBOs(cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall);

                    chkCyberwareActiveCommlink.Visible = objCyberware.IsCommlink;
                    chkCyberwareActiveCommlink.Checked = objCyberware.IsActiveCommlink(CharacterObject);
                    if (CharacterObject.Metatype == "A.I.")
                    {
                        chkCyberwareHomeNode.Visible = true;
                        chkCyberwareHomeNode.Checked = objCyberware.IsHomeNode(CharacterObject);
                        chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible && objCyberware.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objCyberware.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                    }
                }
                else
                {
                    tabCyberwareCM.Visible = false;
                }

                lblCyberwareAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblCyberwareCost.Text = objCyberware.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblCyberwareCapacity.Text = objCyberware.CalculatedCapacity + " (" +
                                            objCyberware.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                            LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                if (objCyberware.Parent == null)
                    lblCyberwareEssence.Text = objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else if (objCyberware.AddToParentESS)
                    lblCyberwareEssence.Text = '+' + objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else
                    lblCyberwareEssence.Text = (0.0m).ToString(strESSFormat, GlobalOptions.CultureInfo);
            }
            else if (treCyberware.SelectedNode?.Tag is Gear objGear)
            {
                // Locate the selected piece of Gear.
                if (objGear.IncludedInParent)
                    cmdDeleteCyberware.Enabled = false;

                if (CharacterObject.Overclocker && objGear.Category == "Cyberdecks")
                {
                    cboCyberwareGearOverclocker.Visible = true;
                    lblCyberwareGearOverclocker.Visible = true;
                    List<ListItem> lstOverclocker = new List<ListItem>
                        {
                            new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language)),
                            new ListItem("Attack", LanguageManager.GetString("String_Attack", GlobalOptions.Language)),
                            new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze", GlobalOptions.Language)),
                            new ListItem("Data Processing", LanguageManager.GetString("String_DataProcessing", GlobalOptions.Language)),
                            new ListItem("Firewall", LanguageManager.GetString("String_Firewall", GlobalOptions.Language))
                        };

                    cboCyberwareGearOverclocker.BindingContext = new BindingContext();
                    cboCyberwareGearOverclocker.DisplayMember = "Name";
                    cboCyberwareGearOverclocker.ValueMember = "Value";
                    cboCyberwareGearOverclocker.DataSource = lstOverclocker;
                    cboCyberwareGearOverclocker.SelectedValue = objGear.Overclocked;
                    if (cboCyberwareGearOverclocker.SelectedIndex == -1)
                        cboCyberwareGearOverclocker.SelectedIndex = 0;
                    cboCyberwareGearOverclocker.EndUpdate();
                }
                else
                {
                    cboCyberwareGearOverclocker.Visible = false;
                    lblCyberwareGearOverclocker.Visible = false;
                }

                objGear.RefreshMatrixAttributeCBOs(cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall);

                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                chkCyberwareActiveCommlink.Visible = objGear.IsCommlink;
                chkCyberwareActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                if (CharacterObject.Metatype == "A.I.")
                {
                    chkCyberwareHomeNode.Visible = true;
                    chkCyberwareHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                    chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                }

                lblCyberDeviceRating.Text = intDeviceRating.ToString();
                lblCyberDeviceRating.Visible = true;
                lblCyberDeviceRatingLabel.Visible = true;
                lblCyberAttackLabel.Visible = true;
                lblCyberSleazeLabel.Visible = true;
                lblCyberDataProcessingLabel.Visible = true;
                lblCyberFirewallLabel.Visible = true;

                lblCyberwareName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblCyberwareCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblCyberwareAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblCyberwareCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblCyberwareCapacity.Text = objGear.CalculatedCapacity + " (" + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                            LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblCyberwareEssence.Text = (0.0m).ToString(strESSFormat, GlobalOptions.CultureInfo);
                lblCyberwareGrade.Text = string.Empty;
                lblCyberwareRating.Text = objGear.Rating.ToString();
                string strPage = objGear.DisplayPage(GlobalOptions.Language);
                lblCyberwareSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCyberwareSource,
                    CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            }
            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed Weapon.
        /// </summary>
        public void RefreshSelectedWeapon()
        {
            _blnSkipRefresh = true;
            string strSelectedId = treWeapons.SelectedNode?.Tag is IHasInternalId objHasInternalId ? objHasInternalId.InternalId : treWeapons.SelectedNode?.Tag.ToString();
            cmdDeleteWeapon.Enabled = !string.IsNullOrEmpty(strSelectedId) && strSelectedId != "Node_SelectedWeapons";

            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level == 0)
            {
                lblWeaponName.Text = string.Empty;
                lblWeaponCategory.Text = string.Empty;
                lblWeaponAvail.Text = string.Empty;
                lblWeaponCost.Text = string.Empty;
                lblWeaponConceal.Text = string.Empty;
                lblWeaponAccuracy.Text = string.Empty;
                lblWeaponDamage.Text = string.Empty;
                lblWeaponRC.Text = string.Empty;
                lblWeaponAP.Text = string.Empty;
                lblWeaponReach.Text = string.Empty;
                lblWeaponMode.Text = string.Empty;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponRating.Text = string.Empty;
                lblWeaponSource.Text = string.Empty;
                cboWeaponAmmo.Enabled = false;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, null);
                chkWeaponAccessoryInstalled.Enabled = false;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = false;
                tabWeaponMatrixCM.Visible = false;

                lblWeaponCapacity.Visible = false;
                lblWeaponCapacityLabel.Visible = false;

                lblWeaponDeviceRatingLabel.Visible = false;
                lblWeaponDeviceRating.Visible = false;
                lblWeaponFirewallLabel.Visible = false;
                lblWeaponDataProcessingLabel.Visible = false;
                lblWeaponSleazeLabel.Visible = false;
                lblWeaponAttackLabel.Visible = false;
                cboWeaponGearAttack.Visible = false;
                cboWeaponGearDataProcessing.Visible = false;
                cboWeaponGearFirewall.Visible = false;
                cboWeaponGearSleaze.Visible = false;

                // Hide Weapon Ranges.
                lblWeaponRangeMain.Text = string.Empty;
                lblWeaponRangeAlternate.Text = string.Empty;
                lblWeaponRangeShort.Text = string.Empty;
                lblWeaponRangeMedium.Text = string.Empty;
                lblWeaponRangeLong.Text = string.Empty;
                lblWeaponRangeExtreme.Text = string.Empty;
                lblWeaponAlternateRangeShort.Text = string.Empty;
                lblWeaponAlternateRangeMedium.Text = string.Empty;
                lblWeaponAlternateRangeLong.Text = string.Empty;
                lblWeaponAlternateRangeExtreme.Text = string.Empty;

                // Disable the fire button.
                cmdFireWeapon.Enabled = false;
                cmdReloadWeapon.Enabled = false;
                cmdWeaponBuyAmmo.Enabled = false;
                cboWeaponAmmo.Enabled = false;
                _blnSkipRefresh = false;
                return;
            }

            tabWeaponMatrixCM.Visible = (treWeapons.SelectedNode?.Tag is IHasMatrixConditionMonitor);

            if (treWeapons.SelectedNode?.Tag is IHasMatrixConditionMonitor objMatrixCM)
            {
                ProcessEquipmentConditionMonitorBoxDisplays(tabWeaponMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
            }

            if (treWeapons.SelectedNode?.Tag is Weapon objWeapon)
            {
                if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || objWeapon.IncludedInWeapon || !string.IsNullOrEmpty(objWeapon.ParentID))
                    cmdDeleteWeapon.Enabled = false;
                lblWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                string strPage = objWeapon.DisplayPage(GlobalOptions.Language);
                lblWeaponSource.Text = CommonFunctions.LanguageBookShort(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, CommonFunctions.LanguageBookLong(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                chkWeaponAccessoryInstalled.Enabled = objWeapon.Parent != null;
                chkWeaponAccessoryInstalled.Checked = objWeapon.Equipped;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                objWeapon.RefreshMatrixAttributeCBOs(cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall);
                lblWeaponDeviceRatingLabel.Visible = true;
                lblWeaponDeviceRating.Visible = true;
                lblWeaponDeviceRating.Text = objWeapon.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponAttackLabel.Visible = true;
                lblWeaponSleazeLabel.Visible = true;
                lblWeaponDataProcessingLabel.Visible = true;
                lblWeaponFirewallLabel.Visible = true;

                lblWeaponCapacity.Visible = false;
                lblWeaponCapacityLabel.Visible = false;

                cmdWeaponMoveToVehicle.Enabled = cmdDeleteWeapon.Enabled && CharacterObject.Vehicles.Count > 0;

                // Enable the fire button if the Weapon is Ranged.
                if (objWeapon.WeaponType == "Ranged" || (objWeapon.WeaponType == "Melee" && objWeapon.Ammo != "0"))
                {
                    cmdFireWeapon.Enabled = true;
                    cmdReloadWeapon.Enabled = true;
                    cmdWeaponBuyAmmo.Enabled = true;
                    lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString();
                    //lblWeaponAmmoType.Text = "External Source";

                    cmsAmmoSingleShot.Enabled = objWeapon.AllowMode(LanguageManager.GetString("String_ModeSingleShot", GlobalOptions.Language)) || objWeapon.AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic", GlobalOptions.Language));
                    cmsAmmoShortBurst.Enabled = objWeapon.AllowMode(LanguageManager.GetString("String_ModeBurstFire", GlobalOptions.Language)) || objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic", GlobalOptions.Language));
                    cmsAmmoLongBurst.Enabled = objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic", GlobalOptions.Language));
                    cmsAmmoFullBurst.Enabled = objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic", GlobalOptions.Language));
                    cmsAmmoSuppressiveFire.Enabled = objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic", GlobalOptions.Language));

                    // Melee Weapons with Ammo are considered to be Single Shot.
                    if (objWeapon.WeaponType == "Melee" && objWeapon.Ammo != "0")
                        cmsAmmoSingleShot.Enabled = true;

                    if (cmsAmmoFullBurst.Enabled)
                        cmsAmmoFullBurst.Text = LanguageManager.GetString("String_FullBurst", GlobalOptions.Language).Replace("{0}", objWeapon.FullBurst.ToString());
                    if (cmsAmmoSuppressiveFire.Enabled)
                        cmsAmmoSuppressiveFire.Text = LanguageManager.GetString("String_SuppressiveFire", GlobalOptions.Language).Replace("{0}", objWeapon.Suppressive.ToString());

                    List<ListItem> lstAmmo = new List<ListItem>();
                    int intCurrentSlot = objWeapon.ActiveAmmoSlot;
                    for (int i = 1; i <= objWeapon.AmmoSlots; i++)
                    {
                        objWeapon.ActiveAmmoSlot = i;
                        Gear objGear = CharacterObject.Gear.DeepFindById(objWeapon.AmmoLoaded);

                        string strPlugins = string.Empty;
                        if (objGear != null)
                        {
                            foreach (Gear objChild in objGear.Children)
                            {
                                strPlugins += objChild.DisplayNameShort(GlobalOptions.Language) + ", ";
                            }
                        }
                        // Remove the trailing comma.
                        if (!string.IsNullOrEmpty(strPlugins))
                            strPlugins = strPlugins.Substring(0, strPlugins.Length - 2);

                        string strAmmoName;
                        if (objGear == null)
                        {
                            if (objWeapon.AmmoRemaining == 0)
                                strAmmoName = LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language).Replace("{0}", i.ToString()) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Empty", GlobalOptions.Language);
                            else
                                strAmmoName = LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language).Replace("{0}", i.ToString()) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_ExternalSource", GlobalOptions.Language);
                        }
                        else
                            strAmmoName = LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language).Replace("{0}", i.ToString()) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language);
                        if (!string.IsNullOrEmpty(strPlugins))
                            strAmmoName += " [" + strPlugins + "]";
                        lstAmmo.Add(new ListItem(i.ToString(), strAmmoName));
                    }
                    objWeapon.ActiveAmmoSlot = intCurrentSlot;
                    cboWeaponAmmo.BeginUpdate();
                    cboWeaponAmmo.Enabled = true;
                    cboWeaponAmmo.ValueMember = "Value";
                    cboWeaponAmmo.DisplayMember = "Name";
                    cboWeaponAmmo.DataSource = lstAmmo;
                    cboWeaponAmmo.SelectedValue = objWeapon.ActiveAmmoSlot.ToString();
                    if (cboWeaponAmmo.SelectedIndex == -1)
                        cboWeaponAmmo.SelectedIndex = 0;
                    cboWeaponAmmo.EndUpdate();
                }
                else
                {
                    cmdFireWeapon.Enabled = false;
                    cmdReloadWeapon.Enabled = false;
                    cmdWeaponBuyAmmo.Enabled = false;
                    lblWeaponAmmoRemaining.Text = string.Empty;
                    cboWeaponAmmo.Enabled = false;
                }
                lblWeaponRangeMain.Text = objWeapon.DisplayRange(GlobalOptions.Language);
                lblWeaponRangeAlternate.Text = objWeapon.DisplayAlternateRange(GlobalOptions.Language);
                IDictionary<string, string> dictionaryRanges = objWeapon.GetRangeStrings(GlobalOptions.CultureInfo);
                lblWeaponRangeShort.Text = dictionaryRanges["short"];
                lblWeaponRangeMedium.Text = dictionaryRanges["medium"];
                lblWeaponRangeLong.Text = dictionaryRanges["long"];
                lblWeaponRangeExtreme.Text = dictionaryRanges["extreme"];
                lblWeaponAlternateRangeShort.Text = dictionaryRanges["alternateshort"];
                lblWeaponAlternateRangeMedium.Text = dictionaryRanges["alternatemedium"];
                lblWeaponAlternateRangeLong.Text = dictionaryRanges["alternatelong"];
                lblWeaponAlternateRangeExtreme.Text = dictionaryRanges["alternateextreme"];

                lblWeaponAvail.Text = objWeapon.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponCost.Text = objWeapon.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblWeaponConceal.Text = objWeapon.CalculatedConcealability(GlobalOptions.CultureInfo);
                lblWeaponDamage.Text = objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAccuracy.Text = objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponRC.Text = objWeapon.TotalRC(GlobalOptions.CultureInfo, GlobalOptions.Language, true);
                lblWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblWeaponReach.Text = objWeapon.TotalReach.ToString();
                lblWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);
                lblWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponRating.Text = string.Empty;
                if (!string.IsNullOrWhiteSpace(objWeapon.AccessoryMounts))
                {
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        StringBuilder strSlotsText = new StringBuilder();
                        foreach (string strMount in objWeapon.AccessoryMounts.Split('/'))
                        {
                            strSlotsText.Append(LanguageManager.GetString("String_Mount" + strMount, GlobalOptions.Language));
                            strSlotsText.Append('/');
                        }
                        strSlotsText.Length -= 1;
                        lblWeaponSlots.Text = strSlotsText.ToString();
                    }
                    else
                        lblWeaponSlots.Text = objWeapon.AccessoryMounts;
                }
                else
                    lblWeaponSlots.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                lblWeaponDicePool.Text = objWeapon.GetDicePool(GlobalOptions.CultureInfo, GlobalOptions.Language);
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponDicePool, objWeapon.DicePoolTooltip);
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponRC, objWeapon.RCToolTip);
            }
            else if (treWeapons.SelectedNode?.Tag is WeaponAccessory objSelectedAccessory)
            {
                if (objSelectedAccessory.IncludedInWeapon)
                    cmdDeleteWeapon.Enabled = false;

                cmdWeaponMoveToVehicle.Enabled = false;
                lblWeaponDicePool.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponDicePool, string.Empty);
                cmdFireWeapon.Enabled = false;
                cmdReloadWeapon.Enabled = false;
                cmdWeaponBuyAmmo.Enabled = false;
                cboWeaponAmmo.Enabled = false;

                objWeapon = objSelectedAccessory.Parent;
                lblWeaponName.Text = objSelectedAccessory.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = LanguageManager.GetString("String_WeaponAccessory", GlobalOptions.Language);
                lblWeaponAvail.Text =
                    objSelectedAccessory.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAccuracy.Text =
                    objSelectedAccessory.Accuracy.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                lblWeaponCost.Text =
                    objSelectedAccessory.TotalCost.ToString(CharacterObjectOptions.NuyenFormat,
                        GlobalOptions.CultureInfo) + '¥';
                lblWeaponConceal.Text =
                    objSelectedAccessory.TotalConcealability.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                lblWeaponDamage.Text = string.Empty;
                lblWeaponRC.Text = objSelectedAccessory.RC;
                lblWeaponAP.Text = string.Empty;
                lblWeaponReach.Text = string.Empty;
                lblWeaponMode.Text = string.Empty;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponRating.Text = objSelectedAccessory.Rating.ToString();

                lblWeaponCapacity.Visible = false;
                lblWeaponCapacityLabel.Visible = false;

                StringBuilder strSlotsText = new StringBuilder(objSelectedAccessory.Mount);
                if (strSlotsText.Length > 0 && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    strSlotsText.Clear();
                    foreach (string strMount in objSelectedAccessory.Mount.Split('/'))
                    {
                        strSlotsText.Append(
                            LanguageManager.GetString("String_Mount" + strMount, GlobalOptions.Language));
                        strSlotsText.Append('/');
                    }

                    strSlotsText.Length -= 1;
                }

                if (!string.IsNullOrEmpty(objSelectedAccessory.ExtraMount) &&
                    (objSelectedAccessory.ExtraMount != "None"))
                {
                    bool boolHaveAddedItem = false;
                    string[] strExtraMounts = objSelectedAccessory.ExtraMount.Split('/');
                    foreach (string strCurrentExtraMount in strExtraMounts)
                    {
                        if (!string.IsNullOrEmpty(strCurrentExtraMount))
                        {
                            if (!boolHaveAddedItem)
                            {
                                strSlotsText.Append(" + ");
                                boolHaveAddedItem = true;
                            }

                            strSlotsText.Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount,
                                GlobalOptions.Language));
                            strSlotsText.Append('/');
                        }
                    }

                    // Remove the trailing /
                    if (boolHaveAddedItem)
                        strSlotsText.Length -= 1;
                }

                lblWeaponSlots.Text = strSlotsText.ToString();
                string strPage = objSelectedAccessory.Page(GlobalOptions.Language);
                lblWeaponSource.Text =
                    CommonFunctions.LanguageBookShort(objSelectedAccessory.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    CommonFunctions.LanguageBookLong(objSelectedAccessory.Source, GlobalOptions.Language);
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource,
                    CommonFunctions.LanguageBookLong(objSelectedAccessory.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkWeaponAccessoryInstalled.Enabled = true;
                chkWeaponAccessoryInstalled.Checked = objSelectedAccessory.Equipped;
                chkIncludedInWeapon.Enabled = CharacterObjectOptions.AllowEditPartOfBaseWeapon;
                chkIncludedInWeapon.Checked = objSelectedAccessory.IncludedInWeapon;

                objWeapon.RefreshMatrixAttributeCBOs(cboWeaponGearAttack, cboWeaponGearSleaze,
                    cboWeaponGearDataProcessing, cboWeaponGearFirewall);
                lblWeaponDeviceRatingLabel.Visible = true;
                lblWeaponDeviceRating.Visible = true;
                lblWeaponDeviceRating.Text = objWeapon.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponAttackLabel.Visible = true;
                lblWeaponSleazeLabel.Visible = true;
                lblWeaponDataProcessingLabel.Visible = true;
                lblWeaponFirewallLabel.Visible = true;
            }
            else if (treWeapons.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.IncludedInParent)
                    cmdDeleteWeapon.Enabled = false;
                lblWeaponName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblWeaponAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                //lblWeaponAccuracy.Text = objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAccuracy.Text = string.Empty;
                lblWeaponConceal.Text = string.Empty;
                lblWeaponDamage.Text = string.Empty;
                lblWeaponRC.Text = string.Empty;
                lblWeaponAP.Text = string.Empty;
                lblWeaponReach.Text = string.Empty;
                lblWeaponMode.Text = string.Empty;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponRating.Text = string.Empty;
                lblWeaponSlots.Text = string.Empty;
                string strPage = objGear.DisplayPage(GlobalOptions.Language);
                lblWeaponSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkWeaponAccessoryInstalled.Enabled = true;
                chkWeaponAccessoryInstalled.Checked = objGear.Equipped;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = false;

                lblWeaponCapacity.Visible = true;
                lblWeaponCapacityLabel.Visible = true;
                lblWeaponCapacity.Text = objGear.CalculatedCapacity + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';

                objGear.RefreshMatrixAttributeCBOs(cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall);
                lblWeaponDeviceRatingLabel.Visible = true;
                lblWeaponDeviceRating.Visible = true;
                lblWeaponDeviceRating.Text = objGear.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponDeviceRatingLabel.Visible = true;
                lblWeaponAttackLabel.Visible = true;
                lblWeaponSleazeLabel.Visible = true;
                lblWeaponDataProcessingLabel.Visible = true;
                lblWeaponFirewallLabel.Visible = true;

                // Hide Weapon Ranges.
                lblWeaponRangeMain.Text = string.Empty;
                lblWeaponRangeAlternate.Text = string.Empty;
                lblWeaponRangeShort.Text = string.Empty;
                lblWeaponRangeMedium.Text = string.Empty;
                lblWeaponRangeLong.Text = string.Empty;
                lblWeaponRangeExtreme.Text = string.Empty;
                lblWeaponAlternateRangeShort.Text = string.Empty;
                lblWeaponAlternateRangeMedium.Text = string.Empty;
                lblWeaponAlternateRangeLong.Text = string.Empty;
                lblWeaponAlternateRangeExtreme.Text = string.Empty;
            }

            /* TODO: Find some clever way to have the parent weapon propogate down through all children.
            // Show the Weapon Ranges.
            if (objWeapon != null)
            {
                lblWeaponRangeMain.Text = objWeapon.DisplayRange(GlobalOptions.Language);
                lblWeaponRangeAlternate.Text = objWeapon.DisplayAlternateRange(GlobalOptions.Language);
                IDictionary<string, string> dictionaryRanges = objWeapon.GetRangeStrings(GlobalOptions.CultureInfo);
                lblWeaponRangeShort.Text = dictionaryRanges["short"];
                lblWeaponRangeMedium.Text = dictionaryRanges["medium"];
                lblWeaponRangeLong.Text = dictionaryRanges["long"];
                lblWeaponRangeExtreme.Text = dictionaryRanges["extreme"];
                lblWeaponAlternateRangeShort.Text = dictionaryRanges["alternateshort"];
                lblWeaponAlternateRangeMedium.Text = dictionaryRanges["alternatemedium"];
                lblWeaponAlternateRangeLong.Text = dictionaryRanges["alternatelong"];
                lblWeaponAlternateRangeExtreme.Text = dictionaryRanges["alternateextreme"];
            }
            else
            {
                // Hide Weapon Ranges.
                lblWeaponRangeMain.Text = string.Empty;
                lblWeaponRangeAlternate.Text = string.Empty;
                lblWeaponRangeShort.Text = string.Empty;
                lblWeaponRangeMedium.Text = string.Empty;
                lblWeaponRangeLong.Text = string.Empty;
                lblWeaponRangeExtreme.Text = string.Empty;
                lblWeaponAlternateRangeShort.Text = string.Empty;
                lblWeaponAlternateRangeMedium.Text = string.Empty;
                lblWeaponAlternateRangeLong.Text = string.Empty;
                lblWeaponAlternateRangeExtreme.Text = string.Empty;
            }*/

            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed Armor.
        /// </summary>
        public void RefreshSelectedArmor()
        {
            _blnSkipRefresh = true;
            cmdDeleteArmor.Enabled = treArmor.SelectedNode?.Tag is ICanRemove;
            cmdArmorDecrease.Enabled = false;
            cmdArmorIncrease.Enabled = false;
            lblArmorEquipped.Visible = false;
            cmdArmorEquipAll.Visible = false;
            cmdArmorUnEquipAll.Visible = false;
            lblArmorEquippedLabel.Visible = false;
            lblArmorEquipped.Visible = false;

            if (treArmor.SelectedNode?.Tag is Armor objArmor)
            {
                lblArmorDeviceRatingLabel.Visible = false;
                lblArmorDeviceRating.Visible = false;
                lblArmorAttackLabel.Visible = false;
                lblArmorAttack.Visible = false;
                lblArmorSleazeLabel.Visible = false;
                lblArmorSleaze.Visible = false;
                lblArmorDataProcessingLabel.Visible = false;
                lblArmorDataProcessing.Visible = false;
                lblArmorFirewallLabel.Visible = false;
                lblArmorFirewall.Visible = false;

                lblArmorValue.Text = objArmor.DisplayArmorValue;
                lblArmorAvail.Text = objArmor.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblArmorCapacity.Text = objArmor.CalculatedCapacity + " (" + objArmor.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblArmorRating.Text = string.Empty;
                lblArmorCost.Text = objArmor.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                string strPage = objArmor.Page(GlobalOptions.Language);
                lblArmorSource.Text = CommonFunctions.LanguageBookShort(objArmor.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, CommonFunctions.LanguageBookLong(objArmor.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkArmorEquipped.Checked = objArmor.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Enabled = false;
                chkIncludedInArmor.Checked = false;
                cmdArmorDecrease.Enabled = true;
                cmdArmorIncrease.Enabled = true;
            }
            else if (treArmor.SelectedNode?.Tag is ArmorMod objArmorMod)
            {
                lblArmorDeviceRatingLabel.Visible = false;
                lblArmorDeviceRating.Visible = false;
                lblArmorAttackLabel.Visible = false;
                lblArmorAttack.Visible = false;
                lblArmorSleazeLabel.Visible = false;
                lblArmorSleaze.Visible = false;
                lblArmorDataProcessingLabel.Visible = false;
                lblArmorDataProcessing.Visible = false;
                lblArmorFirewallLabel.Visible = false;
                lblArmorFirewall.Visible = false;

                objArmor = objArmorMod.Parent;
                if (objArmorMod.IncludedInArmor)
                    cmdDeleteArmor.Enabled = false;
                lblArmorValue.Text = objArmorMod.Armor.ToString("+0;-0;0");
                lblArmorAvail.Text = objArmorMod.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblArmorCapacity.Text = objArmor.CapacityDisplayStyle == CapacityStyle.Zero
                    ? "[0]"
                    : objArmorMod.CalculatedCapacity;
                if (!string.IsNullOrEmpty(objArmorMod.GearCapacity))
                    lblArmorCapacity.Text = objArmorMod.GearCapacity + '/' + lblArmorCapacity.Text + " (" +
                                            objArmorMod.GearCapacityRemaining.ToString("#,0.##",
                                                GlobalOptions.CultureInfo) +
                                            LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                            LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblArmorCost.Text =
                    objArmorMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                string strPage = objArmorMod.DisplayPage(GlobalOptions.Language);
                lblArmorSource.Text = CommonFunctions.LanguageBookShort(objArmorMod.Source, GlobalOptions.Language) +
                                      LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource,
                    CommonFunctions.LanguageBookLong(objArmorMod.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkArmorEquipped.Checked = objArmorMod.Equipped;
                chkArmorEquipped.Enabled = true;
                lblArmorRating.Text = objArmorMod.Rating.ToString();
                chkIncludedInArmor.Enabled = true;
                chkIncludedInArmor.Checked = objArmorMod.IncludedInArmor;
            }
            else if (treArmor.SelectedNode?.Tag is Gear objSelectedGear)
            {
                if (objSelectedGear.IncludedInParent)
                    cmdDeleteArmor.Enabled = false;
                lblArmorValue.Text = string.Empty;
                lblArmorAvail.Text = objSelectedGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);

                CharacterObject.Armor.FindArmorGear(objSelectedGear.InternalId, out objArmor, out objArmorMod);

                if (objArmorMod != null)
                    lblArmorCapacity.Text = objSelectedGear.CalculatedCapacity;
                else if (objArmor.CapacityDisplayStyle == CapacityStyle.Zero)
                    lblArmorCapacity.Text = "[0]";
                else
                    lblArmorCapacity.Text = objSelectedGear.CalculatedArmorCapacity;

                lblArmorCost.Text = objSelectedGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                string strPage = objSelectedGear.DisplayPage(GlobalOptions.Language);
                lblArmorSource.Text = CommonFunctions.LanguageBookShort(objSelectedGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, CommonFunctions.LanguageBookLong(objSelectedGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkArmorEquipped.Checked = objSelectedGear.Equipped;
                chkArmorEquipped.Enabled = true;
                lblArmorRating.Text = objSelectedGear.Rating.ToString();

                lblArmorDeviceRating.Text = objSelectedGear.GetTotalMatrixAttribute("Device Rating").ToString();
                lblArmorAttack.Text = objSelectedGear.GetTotalMatrixAttribute("Attack").ToString();
                lblArmorSleaze.Text = objSelectedGear.GetTotalMatrixAttribute("Sleaze").ToString();
                lblArmorDataProcessing.Text = objSelectedGear.GetTotalMatrixAttribute("Data Processing").ToString();
                lblArmorFirewall.Text = objSelectedGear.GetTotalMatrixAttribute("Firewall").ToString();
                lblArmorDeviceRatingLabel.Visible = true;
                lblArmorDeviceRating.Visible = true;
                lblArmorAttackLabel.Visible = true;
                lblArmorAttack.Visible = true;
                lblArmorSleazeLabel.Visible = true;
                lblArmorSleaze.Visible = true;
                lblArmorDataProcessingLabel.Visible = true;
                lblArmorDataProcessing.Visible = true;
                lblArmorFirewallLabel.Visible = true;
                lblArmorFirewall.Visible = true;
            }
            else if (treArmor.SelectedNode?.Tag is Location objLocation)
            {
                cmdArmorEquipAll.Visible = true;
                cmdArmorUnEquipAll.Visible = true;
                lblArmorEquippedLabel.Visible = true;
                StringBuilder strArmorEquipped = new StringBuilder();
                foreach (Armor objLoopArmor in CharacterObject.Armor.Where(objLoopArmor => objLoopArmor.Equipped && objLoopArmor.Location == objLocation))
                {
                    strArmorEquipped.Append(objLoopArmor.DisplayName(GlobalOptions.Language));
                    strArmorEquipped.Append(" (");
                    strArmorEquipped.Append(objLoopArmor.DisplayArmorValue);
                    strArmorEquipped.AppendLine(")");
                }
                if (strArmorEquipped.Length > 0)
                {
                    strArmorEquipped.Length -= 1;
                    lblArmorEquipped.Text = strArmorEquipped.ToString();
                }
                else
                    lblArmorEquipped.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                lblArmorEquipped.Visible = true;
            }
            else if (treArmor.SelectedNode?.Tag.ToString() == "Node_SelectedArmor")
            {
                cmdArmorEquipAll.Visible = true;
                cmdArmorUnEquipAll.Visible = true;
                lblArmorEquippedLabel.Visible = true;
                StringBuilder strArmorEquipped = new StringBuilder();
                foreach (Armor objLoopArmor in CharacterObject.Armor.Where(objLoopArmor => objLoopArmor.Equipped && objLoopArmor.Location == null))
                {
                    strArmorEquipped.Append(objLoopArmor.DisplayName(GlobalOptions.Language));
                    strArmorEquipped.Append(" (");
                    strArmorEquipped.Append(objLoopArmor.DisplayArmorValue);
                    strArmorEquipped.AppendLine(")");
                }
                if (strArmorEquipped.Length > 0)
                {
                    strArmorEquipped.Length -= 1;
                    lblArmorEquipped.Text = strArmorEquipped.ToString();
                }
                else
                    lblArmorEquipped.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                lblArmorEquipped.Visible = true;
            }
            else
            {
                lblArmorDeviceRatingLabel.Visible = false;
                lblArmorDeviceRating.Visible = false;
                lblArmorAttackLabel.Visible = false;
                lblArmorAttack.Visible = false;
                lblArmorSleazeLabel.Visible = false;
                lblArmorSleaze.Visible = false;
                lblArmorDataProcessingLabel.Visible = false;
                lblArmorDataProcessing.Visible = false;
                lblArmorFirewallLabel.Visible = false;
                lblArmorFirewall.Visible = false;
                cmdArmorEquipAll.Visible = false;
                cmdArmorUnEquipAll.Visible = false;
                lblArmorEquippedLabel.Visible = false;
                lblArmorEquipped.Visible = false;
                chkIncludedInArmor.Enabled = false;
                chkIncludedInArmor.Checked = false;
                lblArmorValue.Text = string.Empty;
                lblArmorAvail.Text = string.Empty;
                lblArmorCost.Text = string.Empty;
                lblArmorSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, null);
                lblArmorRating.Text = string.Empty;
                chkArmorEquipped.Enabled = false;
                _blnSkipRefresh = false;
                return;
            }
            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed Gear.
        /// </summary>
        public void RefreshSelectedGear()
        {
            _blnSkipRefresh = true;
            cmdDeleteGear.Enabled = treGear.SelectedNode?.Tag is ICanRemove;
            if (treGear.SelectedNode == null || treGear.SelectedNode.Level == 0)
            {
                lblGearRating.Text = string.Empty;
                lblGearQty.Text = string.Empty;
                cmdGearIncreaseQty.Enabled = false;
                cmdGearReduceQty.Enabled = false;
                chkGearEquipped.Text = LanguageManager.GetString("Checkbox_Equipped", GlobalOptions.Language);
                chkGearEquipped.Visible = false;
                chkGearActiveCommlink.Enabled = false;
                chkGearActiveCommlink.Checked = false;
                cmdGearSplitQty.Enabled = false;
                cmdGearMergeQty.Enabled = false;
                cmdGearMoveToVehicle.Enabled = false;
                tabGearMatrixCM.Visible = false;
                return;
            }
            chkGearHomeNode.Visible = false;

            if (treGear.SelectedNode.Level > 0)
            {
                if (treGear.SelectedNode?.Tag is Gear objGear)
                {
                    if (objGear.IncludedInParent)
                        cmdDeleteGear.Enabled = false;
                    lblGearName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                    lblGearCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                    lblGearAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    try
                    {
                        lblGearCost.Text =
                            objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) +
                            '¥';
                    }
                    catch (FormatException)
                    {
                        lblGearCost.Text = objGear.Cost + "¥";
                    }

                    lblGearCapacity.Text = objGear.CalculatedCapacity + " (" +
                                           objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                           LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                           LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                    string strPage = objGear.DisplayPage(GlobalOptions.Language);
                    lblGearSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) +
                                         LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblGearSource,
                        CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) +
                        LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                        LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                        LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                    tabGearMatrixCM.Visible = (treGear.SelectedNode?.Tag is IHasMatrixConditionMonitor);

                    if (treGear.SelectedNode?.Tag is IHasMatrixConditionMonitor objMatrixCM)
                    {
                        ProcessEquipmentConditionMonitorBoxDisplays(tabGearMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
                    }

                    cboGearOverclocker.BeginUpdate();

                    chkGearActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                    chkGearActiveCommlink.Enabled = objGear.IsCommlink;

                    if (CharacterObject.Overclocker && objGear.Category == "Cyberdecks")
                    {
                        cboGearOverclocker.Visible = true;
                        lblGearOverclocker.Visible = true;
                        List<ListItem> lstOverclocker = new List<ListItem>
                        {
                            new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language)),
                            new ListItem("Attack", LanguageManager.GetString("String_Attack", GlobalOptions.Language)),
                            new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze", GlobalOptions.Language)),
                            new ListItem("Data Processing",
                                LanguageManager.GetString("String_DataProcessing", GlobalOptions.Language)),
                            new ListItem("Firewall",
                                LanguageManager.GetString("String_Firewall", GlobalOptions.Language))
                        };

                        cboGearOverclocker.BindingContext = new BindingContext();
                        cboGearOverclocker.DisplayMember = "Name";
                        cboGearOverclocker.ValueMember = "Value";
                        cboGearOverclocker.DataSource = lstOverclocker;
                        cboGearOverclocker.SelectedValue = objGear.Overclocked;
                        if (cboGearOverclocker.SelectedIndex == -1)
                            cboGearOverclocker.SelectedIndex = 0;
                        cboGearOverclocker.EndUpdate();
                    }
                    else
                    {
                        cboGearOverclocker.Visible = false;
                        lblGearOverclocker.Visible = false;
                    }

                    objGear.RefreshMatrixAttributeCBOs(cboGearAttack, cboGearSleaze, cboGearDataProcessing,
                        cboGearFirewall);
                    int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                    lblGearDeviceRating.Text = intDeviceRating.ToString();

                    lblGearDeviceRating.Visible = true;
                    lblGearDeviceRatingLabel.Visible = true;
                    lblGearAttackLabel.Visible = true;
                    lblGearSleazeLabel.Visible = true;
                    lblGearDataProcessingLabel.Visible = true;
                    lblGearFirewallLabel.Visible = true;

                    if (CharacterObject.Metatype == "A.I.")
                    {
                        chkGearHomeNode.Visible = true;
                        chkGearHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                        chkGearHomeNode.Enabled = chkGearActiveCommlink.Enabled &&
                                                  objGear.GetTotalMatrixAttribute("Program Limit") >=
                                                  (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                    }

                    lblGearRating.Text = objGear.MaxRating > 0 ? objGear.Rating.ToString() : string.Empty;

                    lblGearQty.Text = objGear.Quantity.ToString(GlobalOptions.CultureInfo);

                    if (treGear.SelectedNode.Level == 1)
                    {
                        lblGearQty.Text = objGear.Quantity.ToString(GlobalOptions.CultureInfo);
                        chkGearEquipped.Visible = true;
                        chkGearEquipped.Checked = objGear.Equipped;
                    }
                    else
                    {
                        lblGearQty.Text = "1";
                        chkGearEquipped.Visible = true;
                        chkGearEquipped.Checked = objGear.Equipped;

                        // If this is a Program, determine if its parent Gear (if any) is a Commlink. If so, show the Equipped checkbox.
                        if (objGear.IsProgram && CharacterObjectOptions.CalculateCommlinkResponse)
                        {
                            if (objGear.Parent is IHasMatrixAttributes commlink && commlink.IsCommlink == true)
                            {
                                chkGearEquipped.Text = LanguageManager.GetString("Checkbox_SoftwareRunning",
                                    GlobalOptions.Language);
                            }
                        }
                    }

                    // Show the Weapon Bonus information if it's available.
                    if (objGear.WeaponBonus != null)
                    {
                        lblGearDamageLabel.Visible = true;
                        lblGearDamage.Visible = true;
                        lblGearAPLabel.Visible = true;
                        lblGearAP.Visible = true;
                        lblGearDamage.Text = objGear.WeaponBonusDamage(GlobalOptions.Language);
                        lblGearAP.Text = objGear.WeaponBonusAP;
                    }
                    else
                    {
                        lblGearDamageLabel.Visible = false;
                        lblGearDamage.Visible = false;
                        lblGearAPLabel.Visible = false;
                        lblGearAP.Visible = false;
                    }

                    cmdGearIncreaseQty.Enabled = !objGear.IncludedInParent;
                    cmdGearReduceQty.Enabled = !objGear.IncludedInParent;

                    treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

                    if (treGear.SelectedNode.Level == 1)
                    {
                        cmdGearSplitQty.Enabled = !objGear.IncludedInParent;
                        cmdGearMergeQty.Enabled = !objGear.IncludedInParent;
                        if (CharacterObject.Vehicles.Count > 0)
                            cmdGearMoveToVehicle.Enabled = !objGear.IncludedInParent;
                        else
                            cmdGearMoveToVehicle.Enabled = false;
                    }
                }
                else
                {
                    cmdGearSplitQty.Enabled = false;
                    cmdGearMergeQty.Enabled = false;
                    cmdGearMoveToVehicle.Enabled = false;
                }
            }
            _blnSkipRefresh = false;
        }

        protected override string FormMode => LanguageManager.GetString("Title_CareerMode", GlobalOptions.Language);

        /// <summary>
        /// Open the Select Cyberware window and handle adding to the Tree and Character.
        /// </summary>
        private bool PickCyberware(Cyberware objSelectedCyberware, Improvement.ImprovementSource objSource)
        {
            frmSelectCyberware frmPickCyberware = new frmSelectCyberware(CharacterObject, objSource, objSelectedCyberware?.GetNode());
            decimal decMultiplier = 1.0m;
            // Apply the character's Cyberware Essence cost multiplier if applicable.
            if (objSource == Improvement.ImprovementSource.Cyberware)
            {
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.CyberwareEssCost) != 0)
                {
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.CyberwareEssCost && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                    frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplier) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.CyberwareTotalEssMultiplier && objImprovement.Enabled)
                            decMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m);
                    }
                    frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.CyberwareEssCostNonRetroactive) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.CyberwareEssCostNonRetroactive && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                    frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive && objImprovement.Enabled)
                            decMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m);
                    }
                    frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                }
            }
            // Apply the character's Bioware Essence cost multiplier if applicable.
            else if (objSource == Improvement.ImprovementSource.Bioware)
            {
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.BiowareEssCost) != 0)
                {
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.BiowareEssCost && objImprovement.Enabled)
                            decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                    frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplier) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.BiowareTotalEssMultiplier && objImprovement.Enabled)
                            decMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m);
                    }
                    frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.BiowareEssCostNonRetroactive) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.BiowareEssCostNonRetroactive && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100));
                    }
                    frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                }
                if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive) != 0)
                {
                    decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive && objImprovement.Enabled)
                            decMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m);
                    }
                    frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                }
            }

            // Apply the character's Basic Bioware Essence cost multiplier if applicable.
            if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.BasicBiowareEssCost) != 0 && objSource == Improvement.ImprovementSource.Bioware)
            {
                decMultiplier = 1.0m;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.BasicBiowareEssCost && objImprovement.Enabled)
                        decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                }
                frmPickCyberware.BasicBiowareESSMultiplier = decMultiplier;
            }

            // Genetech Cost multiplier.
            if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.GenetechCostMultiplier) != 0 && objSource == Improvement.ImprovementSource.Bioware)
            {
                decMultiplier = 1.0m;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier && objImprovement.Enabled)
                        decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                }
                frmPickCyberware.GenetechCostMultiplier = decMultiplier;
            }

            if (objSelectedCyberware != null)
            {
                frmPickCyberware.SetGrade = objSelectedCyberware.Grade;
                frmPickCyberware.LockGrade();
                // If the Cyberware has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                if (!objSelectedCyberware.Capacity.Contains('['))
                {
                    frmPickCyberware.Subsystems = objSelectedCyberware.AllowedSubsystems;
                    frmPickCyberware.MaximumCapacity = objSelectedCyberware.CapacityRemaining;

                    // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedCyberware.CapacityRemaining < 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }

                frmPickCyberware.CyberwareParent = objSelectedCyberware;
                frmPickCyberware.Subsystems = objSelectedCyberware.AllowedSubsystems;

                HashSet<string> setDisallowedMounts = new HashSet<string>();
                HashSet<string> setHasMounts = new HashSet<string>();
                string[] strLoopDisallowedMounts = objSelectedCyberware.BlocksMounts.Split(',');
                foreach (string strLoop in strLoopDisallowedMounts)
                    setDisallowedMounts.Add(strLoop + objSelectedCyberware.Location);
                string strLoopHasModularMount = objSelectedCyberware.HasModularMount;
                if (!string.IsNullOrEmpty(strLoopHasModularMount))
                    setHasMounts.Add(strLoopHasModularMount);
                foreach (Cyberware objLoopCyberware in objSelectedCyberware.Children.DeepWhere(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
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

            frmPickCyberware.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickCyberware.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Cyberware XML file and locate the selected piece.
            XmlNode objXmlCyberware = objSource == Improvement.ImprovementSource.Bioware ? XmlManager.Load("bioware.xml").SelectSingleNode("/chummer/biowares/bioware[id = \"" + frmPickCyberware.SelectedCyberware + "\"]") : XmlManager.Load("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + frmPickCyberware.SelectedCyberware + "\"]");

            Cyberware objCyberware = new Cyberware(CharacterObject);
            if (objCyberware.Purchase(objCyberware, objXmlCyberware, objSource, frmPickCyberware.SelectedGrade, frmPickCyberware.SelectedRating, CharacterObject, null, objSelectedCyberware?.Children ?? CharacterObject.Cyberware, CharacterObject.Vehicles, CharacterObject.Weapons, frmPickCyberware.Markup, frmPickCyberware.FreeCost))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
                DecreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100));
            }

            frmPickCyberware.Dispose();
            
            return frmPickCyberware.AddAgain;
        }

        /// <summary>
        /// Select a piece of Gear to be added to the character.
        /// </summary>
        /// <param name="strSelectedId">InternalId or location of the parent to which the gear should be added.</param>
        /// <param name="blnAmmoOnly">Whether or not only Ammunition should be shown in the window.</param>
        /// <param name="objStackGear">Whether or not the selected item should stack with a matching item on the character.</param>
        /// <param name="strForceItemValue">Force the user to select an item with the passed name.</param>
        /// <param name="lstForceItemPrefixes">Force the user to select an item that begins with one of the strings in this list.</param>
        private bool PickGear(IHasChildren<Gear> iParent, Location objLocation = null, bool blnAmmoOnly = false, Gear objStackGear = null, string strForceItemValue = "", IEnumerable<string> lstForceItemPrefixes = null)
        {
            bool blnNullParent = false;

            if (!((iParent is Gear ? iParent : null) is Gear objSelectedGear))
            {
                objSelectedGear = new Gear(CharacterObject);
                blnNullParent = true;
            }

            // Open the Gear XML file and locate the selected Gear.
            XmlNode objXmlGear = blnNullParent ? null : objSelectedGear.GetNode();

            Cursor = Cursors.WaitCursor;

            string strCategories = string.Empty;
            if (blnAmmoOnly)
                strCategories = "Ammunition";
            else if (!blnNullParent)
            {
                XmlNodeList xmlAddonCategoryList = objXmlGear?.SelectNodes("addoncategory");
                if (xmlAddonCategoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                        strCategories += objXmlCategory.InnerText + ",";
                    // Remove the trailing comma.
                    strCategories = strCategories.Substring(0, strCategories.Length - 1);
                }
            }

            frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, objSelectedGear.ChildAvailModifier, objSelectedGear.ChildCostMultiplier, objXmlGear, strCategories);
            if (!blnNullParent)
            {
                // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                if (!string.IsNullOrEmpty(objSelectedGear.Capacity) && !objSelectedGear.Capacity.Contains('[') || objSelectedGear.Capacity.Contains("/["))
                {
                    frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;

                    // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    if (!string.IsNullOrEmpty(strCategories))
                        frmPickGear.ShowNegativeCapacityOnly = true;
                }

                // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                if (CharacterObject.ActiveCommlink == null && objSelectedGear.IsCommlink)
                {
                    objSelectedGear.SetActiveCommlink(CharacterObject, true);
                }
            }
            
            frmPickGear.DefaultSearchText = strForceItemValue;
            if (lstForceItemPrefixes != null)
            {
                foreach (string strPrefix in lstForceItemPrefixes)
                    frmPickGear.ForceItemPrefixStrings.Add(strPrefix);
            }
            if (blnAmmoOnly)
            {
                frmPickGear.SelectedGear = objSelectedGear.SourceID;
            }

            frmPickGear.ShowDialog(this);
            Cursor = Cursors.Default;

            // Make sure the dialogue window was not canceled.
            if (frmPickGear.DialogResult == DialogResult.Cancel)
                return false;
            
            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>();

            string strForceValue = string.Empty;
            if (blnAmmoOnly)
            {
                strForceValue = objSelectedGear.Extra;
            }
            if (!string.IsNullOrEmpty(strForceItemValue))
                strForceValue = strForceItemValue;
            Gear objGear = new Gear(CharacterObject);
            objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, strForceValue);

            if (objGear.InternalId.IsEmptyGuid())
                return frmPickGear.AddAgain;

            objGear.Quantity = frmPickGear.SelectedQty;

            objGear.Parent = blnNullParent ? null : objSelectedGear;

            //Reduce the Cost for Black Market Pipelin
            objGear.DiscountCost = frmPickGear.BlackMarketDiscount;
            
            // Reduce the cost for Do It Yourself components.
            if (frmPickGear.DoItYourself)
                objGear.Cost = "(" + objGear.Cost + ") * 0.5";

            decimal decCost;
            if (objGear.Cost.Contains("Gear Cost"))
            {
                string strCost = objGear.Cost.Replace("Gear Cost", objSelectedGear.CalculatedCost.ToString(GlobalOptions.InvariantCultureInfo));
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : objGear.TotalCost;
            }
            else
            {
                decCost = objGear.TotalCost;
            }

            Gear objStackWith = null;
            // See if the character already has the item on them if they chose to stack.
            if (frmPickGear.Stack)
            {
                objStackWith = objStackGear ?? CharacterObject.Gear.FirstOrDefault(x => objGear.IsIdenticalToOtherGear(x));
            }
            
            if (objStackWith != null)
            {
                if (objStackWith.InternalId.IsEmptyGuid())
                    return frmPickGear.AddAgain;
                // If a match was found, we need to use the cost of a single item in the stack which can include plugins.
                foreach (Gear objPlugin in objStackWith.Children)
                    decCost += (objPlugin.TotalCost - objPlugin.OwnCost) * frmPickGear.SelectedQty;
            }
            if (!blnNullParent && !blnAmmoOnly)
                decCost *= objSelectedGear.Quantity;

            // Apply a markup if applicable.
            if (frmPickGear.Markup != 0)
            {
                decCost *= 1 + (frmPickGear.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objGear.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
            // This is wrapped in a try statement since the character may not have a piece of Gear selected and has clicked the Buy Additional Ammo button for a Weapon.
            if (!blnNullParent)
            {
                if (objStackWith == null)
                {
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedGear.CapacityRemaining - objGear.PluginCapacity < 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language),
                            LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return frmPickGear.AddAgain;
                    }
                }
            }

            ExpenseUndo objUndo = new ExpenseUndo();
            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickGear.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Remove any Improvements created by the Gear.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                    return frmPickGear.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                objUndo.CreateNuyen(NuyenExpenseType.AddGear, objGear.InternalId, objGear.Quantity);
                objExpense.Undo = objUndo;
            }
            
            if (objStackWith != null)
            {
                // A match was found, so increase the quantity instead.
                objStackWith.Quantity += objGear.Quantity;

                if (!string.IsNullOrEmpty(objUndo.ObjectId))
                    objUndo.ObjectId = objStackWith.InternalId;
            }
            // Add the Gear.
            else
            {
                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                if (!blnNullParent)
                {
                    objSelectedGear.Children.Add(objGear);
                }
                else
                {
                    CharacterObject.Gear.Add(objGear);
                }

                objLocation?.Children.Add(objGear);
            }
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickGear.AddAgain;
        }

        /// <summary>
        /// Select a piece of Gear and add it to a piece of Armor.
        /// </summary>
        /// <param name="blnShowArmorCapacityOnly">Whether or not only items that consume capacity should be shown.</param>
        /// <param name="strSelectedId">Id attached to the object to which the gear should be added.</param>
        private bool PickArmorGear(string strSelectedId, bool blnShowArmorCapacityOnly = false)
        {
            Gear objSelectedGear = null;
            Armor objSelectedArmor = CharacterObject.Armor.FindById(strSelectedId);
            ArmorMod objSelectedMod = null;

            if (objSelectedArmor == null)
            {
                objSelectedGear = CharacterObject.Armor.FindArmorGear(strSelectedId, out objSelectedArmor, out objSelectedMod);
                if (objSelectedGear == null)
                    objSelectedMod = CharacterObject.Armor.FindArmorMod(strSelectedId);
            }

            // Open the Gear XML file and locate the selected Gear.
            XmlNode objXmlGear = objSelectedGear?.GetNode();
            XmlNode objXmlParent = objXmlGear ?? (objSelectedMod != null ? objSelectedMod.GetNode() : objSelectedArmor.GetNode());

            Cursor = Cursors.WaitCursor;

            string strCategories = string.Empty;

            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNodeList xmlAddonCategoryList = objXmlGear?.SelectNodes("addoncategory");
                if (xmlAddonCategoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                        strCategories += objXmlCategory.InnerText + ",";
                    // Remove the trailing comma.
                    strCategories = strCategories.Substring(0, strCategories.Length - 1);
                }
            }

            frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlParent, strCategories)
            {
                EnableStack = false,
                ShowArmorCapacityOnly = blnShowArmorCapacityOnly,
                CapacityDisplayStyle = objSelectedMod != null ? CapacityStyle.Standard : objSelectedArmor.CapacityDisplayStyle
            };

            // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                if (objSelectedGear != null && (!objSelectedGear.Capacity.Contains('[') || objSelectedGear.Capacity.Contains("/[")))
                {
                    frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;

                    // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return false;
                    }
                }
                else if (objSelectedMod != null)
                {
                    frmPickGear.MaximumCapacity = objSelectedMod.GearCapacityRemaining;

                    // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedMod.GearCapacityRemaining < 0)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return false;
                    }
                }
            }

            frmPickGear.ShowDialog(this);
            Cursor = Cursors.Default;

            // Make sure the dialogue window was not canceled.
            if (frmPickGear.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>();

            Gear objGear = new Gear(CharacterObject);
            objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);

            if (objGear.InternalId.IsEmptyGuid())
                return frmPickGear.AddAgain;

            objGear.Quantity = frmPickGear.SelectedQty;

            if (objSelectedGear != null)
                objGear.Parent = objSelectedGear;

            // Reduce the cost for Do It Yourself components.
            if (frmPickGear.DoItYourself)
                objGear.Cost = "(" + objGear.Cost + ") * 0.5";

            // Apply a markup if applicable.
            decimal decCost = objGear.TotalCost;
            if (frmPickGear.Markup != 0)
            {
                decCost *= 1 + (frmPickGear.Markup / 100.0m);
            }

            // Multiply the cost if applicable.
            char chrAvail = objGear.TotalAvailTuple().Suffix;
            if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
            if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;

            Gear objMatchingGear = null;
            // If this is Ammunition, see if the character already has it on them.
            if (objGear.Category == "Ammunition")
            {
                IList<Gear> lstToSearch = string.IsNullOrEmpty(objSelectedGear?.Name) ? objSelectedArmor.Gear : objSelectedGear.Children;
                objMatchingGear = lstToSearch.FirstOrDefault(x => objGear.IsIdenticalToOtherGear(x));
            }

            if (objMatchingGear != null)
            {
                decimal decGearQuantity = objGear.Quantity;
                // A match was found, so increase the quantity instead.
                objMatchingGear.Quantity += decGearQuantity;

                objGear.DeleteGear();
                if (CharacterObjectOptions.EnforceCapacity && objMatchingGear.CapacityRemaining < 0)
                {
                    objMatchingGear.Quantity -= decGearQuantity;
                    MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return frmPickGear.AddAgain;
                }
            }
            // Add the Gear.
            else
            {
                if (!string.IsNullOrEmpty(objSelectedGear?.Name))
                {
                    objSelectedGear.Children.Add(objGear);
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                    {
                        objSelectedGear.Children.Remove(objGear);
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objGear.DeleteGear();
                        return frmPickGear.AddAgain;
                    }
                }
                else if (!string.IsNullOrEmpty(objSelectedMod?.Name))
                {
                    objSelectedMod.Gear.Add(objGear);
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedMod.GearCapacityRemaining < 0)
                    {
                        objSelectedMod.Gear.Remove(objGear);
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objGear.DeleteGear();
                        return frmPickGear.AddAgain;
                    }
                }
                else
                {
                    objSelectedArmor.Gear.Add(objGear);
                    if (CharacterObjectOptions.EnforceCapacity && objSelectedArmor.CapacityRemaining < 0)
                    {
                        objSelectedArmor.Gear.Remove(objGear);
                        MessageBox.Show(LanguageManager.GetString("Message_CapacityReached", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CapacityReached", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objGear.DeleteGear();
                        return frmPickGear.AddAgain;
                    }
                }
            }

            // Check the item's Cost and make sure the character can afford it.
            if (!frmPickGear.FreeCost)
            {
                if (decCost > CharacterObject.Nuyen)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Remove any Improvements created by the Gear.
                    objGear.DeleteGear();
                    return frmPickGear.AddAgain;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseArmorGear", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objGear.DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddArmorGear, objMatchingGear != null ? objMatchingGear.InternalId : objGear.InternalId, objGear.Quantity);
                objExpense.Undo = objUndo;
            }
            
            // Create any Weapons that came with this Gear.
            foreach (Weapon objWeapon in lstWeapons)
            {
                CharacterObject.Weapons.Add(objWeapon);
            }
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickGear.AddAgain;
        }

        /// <summary>
        /// Refresh the currently-selected Lifestyle.
        /// </summary>
        private void RefreshSelectedLifestyle()
        {
            _blnSkipRefresh = true;
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level == 0)
            {
                tblLifestyleDetails.Visible = false;

                _blnSkipRefresh = false;
                return;
            }

            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
            {
                _blnSkipRefresh = false;
                return;
            }

            tblLifestyleDetails.Visible = true;
            cmdIncreaseLifestyleMonths.Visible = true;
            cmdDecreaseLifestyleMonths.Visible = true;

            lblLifestyleCost.Text = objLifestyle.TotalMonthlyCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            lblLifestyleMonths.Text = Convert.ToDecimal(objLifestyle.Increments, GlobalOptions.InvariantCultureInfo).ToString(GlobalOptions.CultureInfo);
            string strPage = objLifestyle.DisplayPage(GlobalOptions.Language);
            lblLifestyleSource.Text = CommonFunctions.LanguageBookShort(objLifestyle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
            GlobalOptions.ToolTipProcessor.SetToolTip(lblLifestyleSource, CommonFunctions.LanguageBookLong(objLifestyle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            //lblLifestyleTotalCost.Text = "= " + objLifestyle.TotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

            string strIncrementString;
            // Change the Cost/Month label.
            switch (objLifestyle.IncrementType)
            {
                case LifestyleIncrement.Day:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerDay", GlobalOptions.Language);
                    strIncrementString = LanguageManager.GetString("String_Days", GlobalOptions.Language);
                    break;
                case LifestyleIncrement.Week:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerWeek", GlobalOptions.Language);
                    strIncrementString = LanguageManager.GetString("String_Weeks", GlobalOptions.Language);
                    break;
                default:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerMonth", GlobalOptions.Language);
                    strIncrementString = LanguageManager.GetString("String_Months", GlobalOptions.Language);
                    break;
            }
            lblLifestyleCost.Left = lblLifestyleCostLabel.Left + lblLifestyleCostLabel.Width + 6;

            lblLifestyleMonthsLabel.Text = strIncrementString + LanguageManager.GetString("Label_LifestylePermanent", GlobalOptions.Language).Replace("{0}", objLifestyle.IncrementsRequiredForPermanent.ToString(GlobalOptions.CultureInfo));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdIncreaseLifestyleMonths, LanguageManager.GetString("Tab_IncreaseLifestyleMonths", GlobalOptions.Language).Replace("{0}", strIncrementString));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdDecreaseLifestyleMonths, LanguageManager.GetString("Tab_DecreaseLifestyleMonths", GlobalOptions.Language).Replace("{0}", strIncrementString));

            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
            {
                string strQualities = string.Join(", ", objLifestyle.LifestyleQualities.Select(r => r.FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language)));

                lblLifestyleQualities.Text = string.Empty;

                foreach (Improvement objImprovement in CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LifestyleCost && x.Enabled))
                {
                    if (strQualities.Length > 0)
                        strQualities += ", ";

                    strQualities += objImprovement.Value > 0
                        ? objImprovement.ImproveSource + " [+" + objImprovement.Value + "%]"
                        : objImprovement.ImproveSource + " [" + objImprovement.Value + "%]";
                }

                if (objLifestyle.FreeGrids.Count > 0)
                {
                    if (strQualities.Length > 0)
                        strQualities += ", ";

                    strQualities += string.Join(", ",
                        objLifestyle.FreeGrids.Select(r => r.DisplayName(GlobalOptions.Language)));
                }

                lblBaseLifestyle.Text = objLifestyle.DisplayNameShort(GlobalOptions.Language);
                lblLifestyleQualities.Text += strQualities;
            }
            else
            {
                lblBaseLifestyle.Text = LanguageManager.GetString("String_Error", GlobalOptions.Language);
                lblLifestyleQualities.Text = string.Empty;
            }
            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Switches the visibility of Weapon attributes on the Vehicles and Drones form.
        /// </summary>
        /// <param name="blnDisplay">Whether to hide or show the objects.</param>
        private void DisplayVehicleWeaponStats(bool blnDisplay)
        {
            lblVehicleWeaponName.Visible = blnDisplay;
            lblVehicleWeaponCategory.Visible = blnDisplay;
            lblVehicleWeaponAP.Visible = blnDisplay;
            lblVehicleWeaponDamage.Visible = blnDisplay;
            lblVehicleWeaponAccuracy.Visible = blnDisplay;
            lblVehicleWeaponMode.Visible = blnDisplay;
            lblVehicleWeaponAmmo.Visible = blnDisplay;
            lblVehicleWeaponDicePool.Visible = blnDisplay;

            lblVehicleWeaponRangeShort.Visible = blnDisplay;
            lblVehicleWeaponRangeMedium.Visible = blnDisplay;
            lblVehicleWeaponRangeLong.Visible = blnDisplay;
            lblVehicleWeaponRangeExtreme.Visible = blnDisplay;

            lblVehicleWeaponNameLabel.Visible = blnDisplay;
            lblVehicleWeaponCategoryLabel.Visible = blnDisplay;
            lblVehicleWeaponAPLabel.Visible = blnDisplay;
            lblVehicleWeaponDamageLabel.Visible = blnDisplay;
            lblVehicleWeaponAccuracyLabel.Visible = blnDisplay;
            lblVehicleWeaponModeLabel.Visible = blnDisplay;
            lblVehicleWeaponAmmoLabel.Visible = blnDisplay;
            lblVehicleWeaponRangeLabel.Visible = blnDisplay;
            lblVehicleWeaponDicePoolLabel.Visible = blnDisplay;
            lblVehicleWeaponAmmoTypeLabel.Visible = blnDisplay;

            lblVehicleWeaponRangeMain.Visible = blnDisplay;
            lblVehicleWeaponRangeAlternate.Visible = blnDisplay;
            lblVehicleWeaponRangeShortLabel.Visible = blnDisplay;
            lblVehicleWeaponRangeMediumLabel.Visible = blnDisplay;
            lblVehicleWeaponRangeLongLabel.Visible = blnDisplay;
            lblVehicleWeaponRangeExtremeLabel.Visible = blnDisplay;
            lblVehicleWeaponAlternateRangeShort.Visible = blnDisplay;
            lblVehicleWeaponAlternateRangeMedium.Visible = blnDisplay;
            lblVehicleWeaponAlternateRangeLong.Visible = blnDisplay;
            lblVehicleWeaponAlternateRangeExtreme.Visible = blnDisplay;

            lblFiringModeLabel.Visible = blnDisplay;
            cboVehicleWeaponFiringMode.Visible = blnDisplay;
            cboVehicleWeaponAmmo.Visible = blnDisplay;
            cmdFireVehicleWeapon.Visible = blnDisplay;
            cmdReloadVehicleWeapon.Visible = blnDisplay;
            cmdFireVehicleWeapon.Enabled = blnDisplay;
            cmdReloadVehicleWeapon.Enabled = blnDisplay;
            lblVehicleWeaponAmmoRemainingLabel.Visible = blnDisplay;
            lblVehicleWeaponAmmoRemaining.Visible = blnDisplay;
        }

        /// <summary>
        /// Switches the visibility of Commlink attributes on the Vehicles and Drones form.
        /// </summary>
        /// <param name="blnDisplay">Whether to hide or show the objects.</param>
        private void DisplayVehicleCommlinkStats(bool blnDisplay)
        {
            cboVehicleGearAttack.Visible = blnDisplay;
            cboVehicleGearSleaze.Visible = blnDisplay;
            cboVehicleGearDataProcessing.Visible = blnDisplay;
            cboVehicleGearFirewall.Visible = blnDisplay;
            lblVehicleAttackLabel.Visible = blnDisplay;
            lblVehicleSleazeLabel.Visible = blnDisplay;
            lblVehicleDataProcessingLabel.Visible = blnDisplay;
            lblVehicleFirewallLabel.Visible = blnDisplay;
            lblVehicleDevice.Visible = blnDisplay;
            lblVehicleDeviceLabel.Visible = blnDisplay;
        }

        /// <summary>
        /// Switches the visibility of Commlink attributes on the Vehicles and Drones form.
        /// </summary>
        /// <param name="blnDisplay">Whether to hide or show the objects.</param>
        private void DisplayVehicleStats(bool blnDisplay)
        {
            lblVehicleHandling.Visible = blnDisplay;
            lblVehicleAccel.Visible = blnDisplay;
            lblVehicleSpeed.Visible = blnDisplay;
            lblVehicleDevice.Visible = blnDisplay;
            lblVehiclePilot.Visible = blnDisplay;
            lblVehicleBody.Visible = blnDisplay;
            lblVehicleArmor.Visible = blnDisplay;
            lblVehicleSensor.Visible = blnDisplay;
            lblVehicleHandlingLabel.Visible = blnDisplay;
            lblVehicleAccelLabel.Visible = blnDisplay;
            lblVehicleSpeedLabel.Visible = blnDisplay;
            lblVehicleDeviceLabel.Visible = blnDisplay;
            lblVehiclePilotLabel.Visible = blnDisplay;
            lblVehicleBodyLabel.Visible = blnDisplay;
            lblVehicleArmorLabel.Visible = blnDisplay;
            lblVehicleSensorLabel.Visible = blnDisplay;
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
            lblVehicleDroneModSlotsLabel.Visible = blnDisplay;
            lblVehicleDroneModSlots.Visible = blnDisplay;
            lblVehicleSeatsLabel.Visible = blnDisplay;
            lblVehicleSeats.Visible = blnDisplay;
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

        /// <summary>
        /// Refresh the currently-selected Vehicle.
        /// </summary>
        private void RefreshSelectedVehicle()
        {
            _blnSkipRefresh = true;
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
                panVehicleCM.Visible = false;
                DisplayVehicleWeaponStats(false);
                DisplayVehicleCommlinkStats(false);
                DisplayVehicleStats(false);

                lblVehicleCategory.Text = string.Empty;
                lblVehicleName.Text = string.Empty;
                lblVehicleAvail.Text = string.Empty;
                lblVehicleCost.Text = string.Empty;
                lblVehicleSource.Text = string.Empty;

                chkVehicleWeaponAccessoryInstalled.Enabled = false;
                _blnSkipRefresh = false;
                return;
            }

            // Locate the selected Vehicle.
            if (treVehicles.SelectedNode?.Tag is Vehicle objVehicle)
            {
                if (!string.IsNullOrEmpty(objVehicle.ParentID))
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleRatingLabel.Visible = false;
                lblVehicleRating.Visible = false;

                lblVehicleName.Text = objVehicle.DisplayNameShort(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleCategory.Text = objVehicle.DisplayCategory(GlobalOptions.Language);
                lblVehicleCategoryLabel.Visible = true;
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objVehicle.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text = objVehicle.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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

                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehiclePowertrainLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleCosmeticLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleElectromagneticLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleBodymodLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleWeaponsmodLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleProtectionLabel, LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
                }

                lblVehicleSensor.Text = objVehicle.CalculatedSensor.ToString();
                UpdateSensor(objVehicle);
                
                string strPage = objVehicle.Page(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objVehicle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objVehicle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkVehicleWeaponAccessoryInstalled.Enabled = false;
                chkVehicleIncludedInWeapon.Checked = false;

                objVehicle.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                chkVehicleActiveCommlink.Visible = objVehicle.IsCommlink;
                chkVehicleActiveCommlink.Checked = objVehicle.IsActiveCommlink(CharacterObject);
                if (CharacterObject.Metatype.Contains("A.I.") || CharacterObject.MetatypeCategory == "Protosapients")
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objVehicle.IsHomeNode(CharacterObject);
                    chkVehicleHomeNode.Enabled = objVehicle.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objVehicle.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }

                DisplayVehicleWeaponStats(false);
                DisplayVehicleCommlinkStats(true);
                DisplayVehicleStats(true);
                if (CharacterObjectOptions.BookEnabled("R5"))
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
                    lblVehicleSlots.Text = objVehicle.Slots + " (" + (objVehicle.Slots - objVehicle.SlotsUsed) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                }
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount)
            {
                cmdDeleteVehicle.Enabled = !objWeaponMount.IncludedInVehicle;

                lblVehicleRatingLabel.Visible = false;
                lblVehicleRating.Visible = false;

                DisplayVehicleWeaponStats(false);
                DisplayVehicleCommlinkStats(false);
                DisplayVehicleStats(false);

                lblVehicleCategoryLabel.Visible = true;
                lblVehicleCategory.Text = objWeaponMount.DisplayCategory(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleName.Text = objWeaponMount.DisplayNameShort(GlobalOptions.Language);
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objWeaponMount.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text =
                    objWeaponMount.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo);

                chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Checked = false;

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString();

                string strPage = objWeaponMount.Page(GlobalOptions.Language);
                lblVehicleSource.Text =
                    CommonFunctions.LanguageBookShort(objWeaponMount.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource,
                    CommonFunctions.LanguageBookLong(objWeaponMount.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
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

                    if (Convert.ToInt32(objMod.MaxRating) > 0)
                    {
                        lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                        lblVehicleRating.Text = objMod.Rating.ToString();
                    }
                    else
                    {
                        lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                        lblVehicleRating.Text = string.Empty;
                    }
                }
                else
                {
                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                    lblVehicleRating.Text = objMod.Rating.ToString();
                }

                DisplayVehicleStats(false);
                DisplayVehicleWeaponStats(false);
                DisplayVehicleCommlinkStats(false);

                lblVehicleName.Text = objMod.DisplayNameShort(GlobalOptions.Language);
                lblVehicleNameLabel.Visible = true;
                lblVehicleCategoryLabel.Visible = true;
                lblVehicleCategory.Text =
                    LanguageManager.GetString("String_VehicleModification", GlobalOptions.Language);
                lblVehicleAvailLabel.Visible = true;
                lblVehicleAvail.Text = objMod.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCostLabel.Visible = true;
                lblVehicleCost.Text =
                    objMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                chkVehicleWeaponAccessoryInstalled.Checked = objMod.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Checked = false;

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objMod.CalculatedSlots.ToString();

                string strPage = objMod.Page(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objMod.Source, GlobalOptions.Language) +
                                        LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource,
                    CommonFunctions.LanguageBookLong(objMod.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            }
            else if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") ||
                    objWeapon.IncludedInWeapon || !string.IsNullOrEmpty(objWeapon.ParentID))
                    cmdDeleteVehicle.Enabled = false;
                DisplayVehicleWeaponStats(true);
                lblVehicleWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblVehicleWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                lblVehicleWeaponDamage.Text =
                    objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleWeaponAccuracy.Text =
                    objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblVehicleWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeapon", GlobalOptions.Language);
                lblVehicleAvail.Text = objWeapon.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text =
                    objWeapon.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                DisplayVehicleStats(false);
                string strPage = objWeapon.DisplayPage(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objWeapon.Source, GlobalOptions.Language) +
                                        LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource,
                    CommonFunctions.LanguageBookLong(objWeapon.Source, GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Page", GlobalOptions.Language) +
                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                cboVehicleWeaponFiringMode.SelectedValue = objWeapon.FireMode;
                lblVehicleWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblVehicleWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                lblVehicleWeaponDamage.Text =
                    objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleWeaponAccuracy.Text =
                    objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblVehicleWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);
                if (objWeapon.WeaponType == "Ranged" || (objWeapon.WeaponType == "Melee" && objWeapon.Ammo != "0"))
                {
                    lblVehicleWeaponAmmo.Text =
                        objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                            .GetString("String_FullBurst", GlobalOptions.Language)
                            .Replace("{0}", objWeapon.FullBurst.ToString());
                    if (cmsVehicleAmmoSuppressiveFire.Enabled)
                        cmsVehicleAmmoSuppressiveFire.Text = LanguageManager
                            .GetString("String_SuppressiveFire", GlobalOptions.Language)
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
                                    LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language)
                                        .Replace("{0}", i.ToString()) +
                                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                    LanguageManager.GetString("String_Empty", GlobalOptions.Language);
                            else
                                strAmmoName =
                                    LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language)
                                        .Replace("{0}", i.ToString()) +
                                    LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                    LanguageManager.GetString("String_ExternalSource", GlobalOptions.Language);
                        }
                        else
                            strAmmoName =
                                LanguageManager.GetString("String_SlotNumber", GlobalOptions.Language)
                                    .Replace("{0}", i.ToString()) +
                                LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                objVehicleGear.DisplayNameShort(GlobalOptions.Language);

                        if (!string.IsNullOrEmpty(strPlugins))
                            strAmmoName += " [" + strPlugins + "]";
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
                objVehicle = objAccessory.Parent.ParentVehicle;
                if (objAccessory.IncludedInWeapon)
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleName.Text = objAccessory.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeaponAccessory", GlobalOptions.Language);
                lblVehicleAvail.Text = objAccessory.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objAccessory.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                DisplayVehicleWeaponStats(false);
                DisplayVehicleStats(false);

                string[] strMounts = objAccessory.Mount.Split('/');
                StringBuilder strMount = new StringBuilder();
                foreach (string strCurrentMount in strMounts)
                {
                    if (!string.IsNullOrEmpty(strCurrentMount))
                        strMount.Append(LanguageManager.GetString("String_Mount" + strCurrentMount, GlobalOptions.Language) + '/');
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
                            strMount.Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount, GlobalOptions.Language) + '/');
                        }
                    }
                    // Remove the trailing /
                    if (boolHaveAddedItem)
                        strMount.Length -= 1;
                }

                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = strMount.ToString();
                string strPage = objAccessory.Page(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objAccessory.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objAccessory.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkVehicleWeaponAccessoryInstalled.Enabled = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Equipped;
                chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                objVehicle = objCyberware.ParentVehicle;
                if (!string.IsNullOrEmpty(objCyberware.ParentID))
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                lblVehicleRating.Visible = true;
                lblVehicleRating.Text = objCyberware.Rating.ToString(GlobalOptions.CultureInfo);
                cmdVehicleCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);

                lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleModification", GlobalOptions.Language);
                lblVehicleAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objCyberware.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                string strPage = objCyberware.Page(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                lblVehicleDevice.Text = objCyberware.GetTotalMatrixAttribute("Device Rating").ToString();
                objCyberware.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                DisplayVehicleWeaponStats(false);
                DisplayVehicleStats(false);
                DisplayVehicleCommlinkStats(true);

                chkVehicleActiveCommlink.Visible = objCyberware.IsCommlink;
                chkVehicleActiveCommlink.Checked = objCyberware.IsActiveCommlink(CharacterObject);
                if (CharacterObject.Metatype == "A.I.")
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objCyberware.IsHomeNode(CharacterObject);
                    chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objCyberware.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objCyberware.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.IncludedInParent)
                    cmdDeleteVehicle.Enabled = false;

                lblVehicleGearQty.Text = objGear.Quantity.ToString(GlobalOptions.CultureInfo);
                cmdVehicleGearReduceQty.Enabled = !objGear.IncludedInParent;
                cmdVehicleMoveToInventory.Enabled = !objGear.IncludedInParent;

                lblVehicleGearQtyLabel.Visible = true;
                lblVehicleGearQty.Visible = true;
                lblVehicleGearQty.Text = objGear.Quantity.ToString(objGear.Name.StartsWith("Nuyen") ? CharacterObjectOptions.NuyenFormat : (objGear.Category == "Currency" ? "#,0.00" : "#,0"), GlobalOptions.CultureInfo);

                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                lblVehicleRating.Visible = true;
                lblVehicleRating.Text = objGear.Rating.ToString(GlobalOptions.CultureInfo);

                lblVehicleName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblVehicleAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objGear.CalculatedCapacity + " (" + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                string strPage = objGear.DisplayPage(GlobalOptions.Language);
                lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                objGear.RefreshMatrixAttributeCBOs(cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall);

                DisplayVehicleStats(false);
                DisplayVehicleWeaponStats(false);
                DisplayVehicleCommlinkStats(true);

                chkVehicleActiveCommlink.Visible = objGear.IsCommlink;
                chkVehicleActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                if (CharacterObject.Metatype == "A.I.")
                {
                    chkVehicleHomeNode.Visible = true;
                    chkVehicleHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                    chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objGear.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }

                chkVehicleWeaponAccessoryInstalled.Checked = true;
                chkVehicleWeaponAccessoryInstalled.Enabled = false;
                chkVehicleIncludedInWeapon.Checked = false;
            }

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

            _blnSkipRefresh = false;
        }
        
        /// <summary>
        /// Populate the Expense Log Lists.
        /// TODO: Change this so that it works off of ObservableCollection Events instead of needing repopulation
        /// </summary>
        public void PopulateExpenseList(object sender, EventArgs e)
        {
            lstKarma.Items.Clear();
            lstNuyen.Items.Clear();
            lstKarma.ContextMenuStrip = null;
            lstNuyen.ContextMenuStrip = null;
            //Find the first karma/nuyen entry
            DateTime KarmaFirst = DateTime.MinValue;
            DateTime KarmaLast = DateTime.Now;
            DateTime NuyenFirst = DateTime.MinValue;
            DateTime NuyenLast = DateTime.Now;
            foreach (ExpenseLogEntry objExpense in CharacterObject.ExpenseEntries)
            {
                if (objExpense.Type == ExpenseType.Karma)
                {
                    if (KarmaFirst == DateTime.MinValue || objExpense.Date.CompareTo(KarmaFirst) < 0)
                        KarmaFirst = objExpense.Date;
                    if (objExpense.Date.CompareTo(KarmaLast) > 0)
                        KarmaLast = objExpense.Date;

                    if (objExpense.Amount != 0 || chkShowFreeKarma.Checked)
                    {
                        ListViewItem.ListViewSubItem objAmountItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.Amount.ToString("#,0.##", GlobalOptions.CultureInfo)
                        };
                        ListViewItem.ListViewSubItem objReasonItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.DisplayReason(GlobalOptions.Language)
                        };
                        ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.InternalId
                        };

                        ListViewItem objItem = new ListViewItem
                        {
                            Text = objExpense.Date.ToShortDateString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objExpense.Date.ToShortTimeString()
                        };
                        objItem.SubItems.Add(objAmountItem);
                        objItem.SubItems.Add(objReasonItem);
                        objItem.SubItems.Add(objInternalIdItem);

                        lstKarma.Items.Insert(0, objItem);
                        if (objExpense.Undo != null)
                            lstKarma.ContextMenuStrip = cmsUndoKarmaExpense;
                    }
                }
                else
                {
                    if (NuyenFirst == DateTime.MinValue || objExpense.Date.CompareTo(NuyenFirst) < 0)
                        NuyenFirst = objExpense.Date;
                    if (objExpense.Date.CompareTo(NuyenLast) > 0)
                        NuyenLast = objExpense.Date;

                    if (objExpense.Amount != 0 || chkShowFreeNuyen.Checked)
                    {
                        ListViewItem.ListViewSubItem objAmountItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.Amount.ToString(CharacterObjectOptions.NuyenFormat + '¥', GlobalOptions.CultureInfo)
                        };
                        ListViewItem.ListViewSubItem objReasonItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.DisplayReason(GlobalOptions.Language)
                        };
                        ListViewItem.ListViewSubItem objInternalIdItem = new ListViewItem.ListViewSubItem
                        {
                            Text = objExpense.InternalId
                        };

                        ListViewItem objItem = new ListViewItem
                        {
                            Text = objExpense.Date.ToShortDateString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objExpense.Date.ToShortTimeString()
                        };
                        objItem.SubItems.Add(objAmountItem);
                        objItem.SubItems.Add(objReasonItem);
                        objItem.SubItems.Add(objInternalIdItem);

                        lstNuyen.Items.Insert(0, objItem);
                        if (objExpense.Undo != null)
                            lstNuyen.ContextMenuStrip = cmsUndoNuyenExpense;
                    }
                }
            }

            // Charting test for Expenses.
            Series objKarmaSeries;
            if (chtKarma.Series.Count > 0)
            {
                objKarmaSeries = chtKarma.Series[0];
                objKarmaSeries.Points.Clear();
            }
            else
            {
                objKarmaSeries = new Series
                {
                    Name = "KarmaSeries",
                    Color = Color.Blue,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Area
                };
                chtKarma.Series.Add(objKarmaSeries);
            }
            Series objNuyenSeries;
            if (chtNuyen.Series.Count > 0)
            {
                objNuyenSeries = chtNuyen.Series[0];
                objNuyenSeries.Points.Clear();
            }
            else
            {
                objNuyenSeries = new Series
                {
                    Name = "NuyenSeries",
                    Color = Color.Green,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Area
                };
                chtNuyen.Series.Add(objNuyenSeries);
            }

            // Configure the Karma chart.
            ChartArea objKarmaChartArea = chtKarma.ChartAreas[0];
            objKarmaChartArea.AxisX.LabelStyle.Enabled = false;
            objKarmaChartArea.AxisY.Title = LanguageManager.GetString("Label_KarmaRemaining", GlobalOptions.Language).TrimEndOnce(':');
            objKarmaChartArea.AxisX.Minimum = 0;
            objKarmaChartArea.AxisX.Maximum = (KarmaLast - KarmaFirst).TotalDays;

            // Configure the Nuyen chart.
            ChartArea objNuyenChartArea = chtNuyen.ChartAreas[0];
            objNuyenChartArea.AxisX.LabelStyle.Enabled = false;
            objNuyenChartArea.AxisY.Title = LanguageManager.GetString("Label_OtherNuyenRemain", GlobalOptions.Language).TrimEndOnce(':');
            objNuyenChartArea.AxisX.Minimum = 0;
            objNuyenChartArea.AxisX.Maximum = (NuyenLast - NuyenFirst).TotalDays;

            double dblKarmaValue = 0;
            double dblNuyenValue = 0;

            foreach (ExpenseLogEntry objExpense in CharacterObject.ExpenseEntries)
            {
                if (objExpense.Type == ExpenseType.Karma)
                {
                    dblKarmaValue += decimal.ToDouble(objExpense.Amount);
                    objKarmaSeries.Points.AddXY((objExpense.Date - KarmaFirst).TotalDays, dblKarmaValue);
                }
                else
                {
                    dblNuyenValue += decimal.ToDouble(objExpense.Amount);
                    objNuyenSeries.Points.AddXY((objExpense.Date - NuyenFirst).TotalDays, dblNuyenValue);
                }
            }

            objKarmaSeries.Points.AddXY((KarmaLast - KarmaFirst).TotalDays, CharacterObject.Karma);
            objNuyenSeries.Points.AddXY((NuyenLast - NuyenFirst).TotalDays, CharacterObject.Nuyen);

            chtKarma.Invalidate();
            chtNuyen.Invalidate();
        }

        /// <summary>
        /// Update the karma cost tooltip for Initiation/Submersion.
        /// </summary>
        private void UpdateInitiationCost(object sender, EventArgs e)
        {
            decimal decMultiplier = 1.0m;
            int intAmount;
            string strInitTip;

            if (CharacterObject.MAGEnabled)
            {
                if (chkInitiationGroup.Checked)
                    decMultiplier -= 0.1m;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= 0.1m;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= 0.1m;
                intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                strInitTip = LanguageManager.GetString("Tip_ImproveInitiateGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.InitiateGrade + 1).ToString()).Replace("{1}", intAmount.ToString());
            }
            else
            {
                if (chkInitiationGroup.Checked)
                    decMultiplier -= 0.2m;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= 0.2m;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= 0.1m;
                intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.InvariantCultureInfo) * decMultiplier));

                strInitTip = LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.SubmersionGrade + 1).ToString()).Replace("{1}", intAmount.ToString());
            }

            GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
        }
        
        /// <summary>
        /// Set the ToolTips from the Language file.
        /// </summary>
        private void SetTooltips()
        {
            // Spells Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdRollSpell, LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdRollDrain, LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            // Complex Forms Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdRollFading, LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            // Armor Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkArmorEquipped, LanguageManager.GetString("Tip_ArmorEquipped", GlobalOptions.Language));
            // GlobalOptions.ToolTipProcessor.SetToolTip(cmdArmorIncrease, LanguageManager.GetString("Tip_ArmorDegradationAPlus"));
            // GlobalOptions.ToolTipProcessor.SetToolTip(cmdArmorDecrease, LanguageManager.GetString("Tip_ArmorDegradationAMinus"));
            // Weapon Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkWeaponAccessoryInstalled, LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdWeaponBuyAmmo, LanguageManager.GetString("Tip_BuyAmmo", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdWeaponMoveToVehicle, LanguageManager.GetString("Tip_TransferToVehicle", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdRollWeapon, LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            // Gear Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdGearIncreaseQty, LanguageManager.GetString("Tip_IncreaseGearQty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdGearReduceQty, LanguageManager.GetString("Tip_DecreaseGearQty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdGearSplitQty, LanguageManager.GetString("Tip_SplitGearQty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdGearMergeQty, LanguageManager.GetString("Tip_MergeGearQty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdGearMoveToVehicle, LanguageManager.GetString("Tip_TransferToVehicle", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(chkGearActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(chkCyberwareActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            // Vehicles Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkVehicleWeaponAccessoryInstalled, LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdVehicleGearReduceQty, LanguageManager.GetString("Tip_DecreaseGearQty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdVehicleMoveToInventory, LanguageManager.GetString("Tip_TransferToInventory", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdRollVehicleWeapon, LanguageManager.GetString("Tip_DiceRoller", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(chkVehicleActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            // Other Info Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysicalLabel, LanguageManager.GetString("Tip_OtherCMPhysical", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStunLabel, LanguageManager.GetString("Tip_OtherCMStun", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblINILabel, LanguageManager.GetString("Tip_OtherInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINILabel, LanguageManager.GetString("Tip_OtherMatrixInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINILabel, LanguageManager.GetString("Tip_OtherAstralInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorLabel, LanguageManager.GetString("Tip_OtherArmor", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblESS, LanguageManager.GetString("Tip_OtherEssence", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblRemainingNuyenLabel, LanguageManager.GetString("Tip_OtherNuyen", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCareerKarmaLabel, LanguageManager.GetString("Tip_OtherCareerKarma", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMovementLabel, LanguageManager.GetString("Tip_OtherMovement", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSwimLabel, LanguageManager.GetString("Tip_OtherSwim", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblFlyLabel, LanguageManager.GetString("Tip_OtherFly", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblComposureLabel, LanguageManager.GetString("Tip_OtherComposure", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblJudgeIntentionsLabel, LanguageManager.GetString("Tip_OtherJudgeIntentions", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblLiftCarryLabel, LanguageManager.GetString("Tip_OtherLiftAndCarry", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMemoryLabel, LanguageManager.GetString("Tip_OtherMemory", GlobalOptions.Language));
            // Condition Monitor Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPenaltyLabel, LanguageManager.GetString("Tip_CMCMPenalty", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMArmorLabel, LanguageManager.GetString("Tip_OtherArmor", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMDamageResistancePoolLabel, LanguageManager.GetString("Tip_CMDamageResistance", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdEdgeGained, LanguageManager.GetString("Tip_CMRegainEdge", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdEdgeSpent, LanguageManager.GetString("Tip_CMSpendEdge", GlobalOptions.Language));
            // Common Info Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblStreetCred, LanguageManager.GetString("Tip_StreetCred", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblNotoriety, LanguageManager.GetString("Tip_Notoriety", GlobalOptions.Language));
            if (CharacterObjectOptions.UseCalculatedPublicAwareness)
            {
                GlobalOptions.ToolTipProcessor.SetToolTip(lblPublicAware, LanguageManager.GetString("Tip_PublicAwareness", GlobalOptions.Language));
            }
            GlobalOptions.ToolTipProcessor.SetToolTip(cmdBurnStreetCred, LanguageManager.GetString("Tip_BurnStreetCred", GlobalOptions.Language));

            // Reposition controls based on their new sizes.
            // Common Tab.
            txtAlias.Left = lblAlias.Left + lblAlias.Width + 6;
            txtAlias.Width = lblMetatypeLabel.Left - 6 - txtAlias.Left;
            cmdSwapQuality.Left = cmdAddQuality.Left + cmdAddQuality.Width + 6;
            cmdDeleteQuality.Left = cmdSwapQuality.Left + cmdSwapQuality.Width + 6;
            // Martial Arts Tab.
            cmdDeleteMartialArt.Left = cmdAddMartialArt.Left + cmdAddMartialArt.Width + 6;
            // Magician Tab.
            cmdDeleteSpell.Left = cmdAddSpell.Left + cmdAddSpell.Width + 6;
            // Technomancer Tab.
            cmdDeleteComplexForm.Left = cmdAddComplexForm.Left + cmdAddComplexForm.Width + 6;
            // Critter Powers Tab.
            cmdDeleteCritterPower.Left = cmdAddCritterPower.Left + cmdAddCritterPower.Width + 6;
            // Initiation Tab.
            // Cyberware Tab.
            cmdAddBioware.Left = cmdAddCyberware.Left + cmdAddCyberware.Width + 6;
            cmdDeleteCyberware.Left = cmdAddBioware.Left + cmdAddBioware.Width + 6;
            // Lifestyle Tab.
            cmdDeleteLifestyle.Left = cmdAddLifestyle.Left + cmdAddLifestyle.Width + 6;
            // Armor Tab.
            cmdDeleteArmor.Left = cmdAddArmor.Left + cmdAddArmor.Width + 6;
            cmdAddArmorBundle.Left = cmdDeleteArmor.Left + cmdDeleteArmor.Width + 6;
            cmdArmorEquipAll.Left = chkArmorEquipped.Left + chkArmorEquipped.Width + 6;
            cmdArmorUnEquipAll.Left = cmdArmorEquipAll.Left + cmdArmorEquipAll.Width + 6;
            // Weapons Tab.
            cmdDeleteWeapon.Left = cmdAddWeapon.Left + cmdAddWeapon.Width + 6;
            cmdAddWeaponLocation.Left = cmdDeleteWeapon.Left + cmdDeleteWeapon.Width + 6;
            // Gear Tab.
            cmdDeleteGear.Left = cmdAddGear.Left + cmdAddGear.Width + 6;
            cmdAddLocation.Left = cmdDeleteGear.Left + cmdDeleteGear.Width + 6;
            // Vehicle Tab.
            cmdDeleteVehicle.Left = cmdAddVehicle.Left + cmdAddVehicle.Width + 6;
            cmdAddVehicleLocation.Left = cmdDeleteVehicle.Left + cmdDeleteVehicle.Width + 6;
            // Improvements Tab.
            cmdImprovementsEnableAll.Left = chkImprovementActive.Left + chkImprovementActive.Width + 6;
            cmdImprovementsDisableAll.Left = cmdImprovementsEnableAll.Left + cmdImprovementsEnableAll.Width + 6;
        }

        private void MoveControls()
        {
            // Common tab.
            lblAlias.Left = Math.Max(288, cmdDeleteQuality.Left + cmdDeleteQuality.Width + 6);
            txtAlias.Left = lblAlias.Left + lblAlias.Width + 6;
            txtAlias.Width = lblMetatypeLabel.Left - txtAlias.Left - 6;

            // Skills tab.

            // Martial Arts tab.
            lblMartialArtSource.Left = lblMartialArtSourceLabel.Left + lblMartialArtSourceLabel.Width + 6;

            // Spells and Spirits tab.
            int intWidth = Math.Max(lblSpellDescriptorsLabel.Width, lblSpellCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellRangeLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellDurationLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellSourceLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellDicePoolLabel.Width);

            lblSpellDescriptors.Left = lblSpellDescriptorsLabel.Left + intWidth + 6;
            lblSpellCategory.Left = lblSpellCategoryLabel.Left + intWidth + 6;
            lblSpellRange.Left = lblSpellRangeLabel.Left + intWidth + 6;
            lblSpellDuration.Left = lblSpellDurationLabel.Left + intWidth + 6;
            lblSpellSource.Left = lblSpellSourceLabel.Left + intWidth + 6;
            lblSpellDicePool.Left = lblSpellDicePoolLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblSpellTypeLabel.Width, lblSpellDamageLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellDVLabel.Width);
            lblSpellTypeLabel.Left = lblSpellCategoryLabel.Left + 179;
            lblSpellType.Left = lblSpellTypeLabel.Left + intWidth + 6;
            lblSpellDamageLabel.Left = lblSpellRangeLabel.Left + 179;
            lblSpellDamage.Left = lblSpellDamageLabel.Left + intWidth + 6;
            lblSpellDVLabel.Left = lblSpellDurationLabel.Left + 179;
            lblSpellDV.Left = lblSpellDVLabel.Left + intWidth + 6;
            cmdQuickenSpell.Left = lblSpellDVLabel.Left;

            intWidth = Math.Max(lblTraditionLabel.Width, lblDrainAttributesLabel.Width);
            intWidth = Math.Max(intWidth, lblMentorSpiritLabel.Width);
            cboTradition.Left = lblTraditionLabel.Left + intWidth + 6;
            lblDrainAttributes.Left = lblDrainAttributesLabel.Left + intWidth + 6;
            lblTraditionSource.Left = lblTraditionSourceLabel.Left + intWidth + 6;
            lblDrainAttributesValue.Left = lblDrainAttributes.Left + 91;
            lblMentorSpirit.Left = lblMentorSpiritLabel.Left + intWidth + 6;

            cmdRollSpell.Left = lblSpellDicePool.Left + lblSpellDicePool.Width + 6;
            cmdRollDrain.Left = lblDrainAttributesValue.Left + lblDrainAttributesValue.Width + 6;
            cmdRollSpell.Visible = CharacterObjectOptions.AllowSkillDiceRolling;
            cmdRollDrain.Visible = CharacterObjectOptions.AllowSkillDiceRolling;

            // Sprites and Complex Forms tab.
            int intLeft = lblDurationLabel.Width;
            intLeft = Math.Max(intLeft, lblTargetLabel.Width);
            intLeft = Math.Max(intLeft, lblFV.Width);
            intLeft = Math.Max(intLeft, lblComplexFormSource.Width);

            lblTarget.Left = lblTargetLabel.Left + intLeft + 6;
            lblDuration.Left = lblDurationLabel.Left + intLeft + 6;
            lblFV.Left = lblFVLabel.Left + intLeft + 6;
            lblComplexFormSource.Left = lblComplexFormSourceLabel.Left + intLeft + 6;

            intWidth = lblFadingAttributesLabel.Width;
            lblFadingAttributes.Left = lblFadingAttributesLabel.Left + intWidth + 6;
            lblFadingAttributesValue.Left = lblFadingAttributes.Left + 91;

            cmdRollFading.Left = lblFadingAttributesValue.Left + lblFadingAttributesValue.Width + 6;
            cmdRollFading.Visible = CharacterObjectOptions.AllowSkillDiceRolling;

            // Critter Powers tab.
            lblCritterPowerPointsLabel.Left = cmdDeleteCritterPower.Left + cmdDeleteCritterPower.Width + 16;
            lblCritterPowerPoints.Left = lblCritterPowerPointsLabel.Left + lblCritterPowerPointsLabel.Width + 6;

            intWidth = Math.Max(lblCritterPowerNameLabel.Width, lblCritterPowerCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerTypeLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerActionLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerRangeLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerDurationLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerSourceLabel.Width);
            intWidth = Math.Max(intWidth, lblCritterPowerPointCostLabel.Width);

            lblCritterPowerName.Left = lblCritterPowerNameLabel.Left + intWidth + 6;
            lblCritterPowerCategory.Left = lblCritterPowerCategoryLabel.Left + intWidth + 6;
            lblCritterPowerType.Left = lblCritterPowerTypeLabel.Left + intWidth + 6;
            lblCritterPowerAction.Left = lblCritterPowerActionLabel.Left + intWidth + 6;
            lblCritterPowerRange.Left = lblCritterPowerRangeLabel.Left + intWidth + 6;
            lblCritterPowerDuration.Left = lblCritterPowerDurationLabel.Left + intWidth + 6;
            lblCritterPowerSource.Left = lblCritterPowerSourceLabel.Left + intWidth + 6;
            lblCritterPowerPointCost.Left = lblCritterPowerPointCostLabel.Left + intWidth + 6;

            // Initiation and Submersion tab.

            // Cyberware and Bioware tab.
            intWidth = Math.Max(lblCyberwareNameLabel.Width, lblCyberwareCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareGradeLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareEssenceLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareSourceLabel.Width);

            lblCyberwareName.Left = lblCyberwareNameLabel.Left + intWidth + 6;
            lblCyberwareCategory.Left = lblCyberwareCategoryLabel.Left + intWidth + 6;
            lblCyberwareGrade.Left = lblCyberwareGradeLabel.Left + intWidth + 6;
            lblCyberwareEssence.Left = lblCyberwareEssenceLabel.Left + intWidth + 6;
            lblCyberwareAvail.Left = lblCyberwareAvailLabel.Left + intWidth + 6;
            lblCyberwareSource.Left = lblCyberwareSourceLabel.Left + intWidth + 6;

            intWidth = lblEssenceHoleESSLabel.Width;
            lblCyberwareESS.Left = lblEssenceHoleESSLabel.Left + intWidth + 6;
            lblBiowareESS.Left = lblEssenceHoleESSLabel.Left + intWidth + 6;
            lblEssenceHoleESS.Left = lblEssenceHoleESSLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblCyberwareRatingLabel.Width, lblCyberwareCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareCostLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberlimbSTRLabel.Width);

            lblCyberAttackLabel.Left = lblCyberDeviceRating.Left + lblCyberDeviceRating.Width + 20;
            cboCyberwareGearAttack.Left = lblCyberAttackLabel.Left + lblCyberAttackLabel.Width + 6;
            lblCyberSleazeLabel.Left = cboCyberwareGearAttack.Left + cboCyberwareGearAttack.Width + 20;
            cboCyberwareGearSleaze.Left = lblCyberSleazeLabel.Left + lblCyberSleazeLabel.Width + 6;
            lblCyberDataProcessingLabel.Left = cboCyberwareGearSleaze.Left + cboCyberwareGearSleaze.Width + 20;
            cboCyberwareGearDataProcessing.Left = lblCyberDataProcessingLabel.Left + lblCyberDataProcessingLabel.Width + 6;
            lblCyberFirewallLabel.Left = cboCyberwareGearDataProcessing.Left + cboCyberwareGearDataProcessing.Width + 20;
            cboCyberwareGearFirewall.Left = lblCyberFirewallLabel.Left + lblCyberFirewallLabel.Width + 6;

            lblCyberwareRatingLabel.Left = lblCyberwareName.Left + 208;
            lblCyberwareRating.Left = lblCyberwareRatingLabel.Left + intWidth + 6;
            lblCyberwareCapacityLabel.Left = lblCyberwareName.Left + 208;
            lblCyberwareCapacity.Left = lblCyberwareCapacityLabel.Left + intWidth + 6;
            lblCyberwareCostLabel.Left = lblCyberwareName.Left + 208;
            lblCyberwareCost.Left = lblCyberwareCostLabel.Left + intWidth + 6;
            lblCyberlimbAGILabel.Left = lblCyberwareName.Left + 208;
            lblCyberlimbAGI.Left = lblCyberlimbAGILabel.Left + intWidth + 6;
            lblCyberlimbSTRLabel.Left = lblCyberwareName.Left + 208;
            lblCyberlimbSTR.Left = lblCyberlimbSTRLabel.Left + intWidth + 6;

            // Street Gear tab.

            // Armor tab.
            intWidth = lblArmorValueLabel.Width;
            intWidth = Math.Max(intWidth, lblArmorRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblArmorCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblArmorSourceLabel.Width);

            lblArmorValue.Left = lblArmorValueLabel.Left + intWidth + 6;
            lblArmorRating.Left = lblArmorRatingLabel.Left + intWidth + 6;
            lblArmorCapacity.Left = lblArmorCapacityLabel.Left + intWidth + 6;
            lblArmorSource.Left = lblArmorSourceLabel.Left + intWidth + 6;

            lblArmorAvailLabel.Left = lblArmorRating.Left + Math.Max(lblArmorRating.Width, 50) + 6;
            lblArmorAvail.Left = lblArmorAvailLabel.Left + lblArmorAvailLabel.Width + 6;

            lblArmorCostLabel.Left = lblArmorAvail.Left + Math.Max(lblArmorAvail.Width, 50) + 6;
            lblArmorCost.Left = lblArmorCostLabel.Left + lblArmorCostLabel.Width + 6;

            cmdArmorIncrease.Left = lblArmorValue.Left + 45;
            cmdArmorDecrease.Left = cmdArmorIncrease.Left + cmdArmorIncrease.Width + 6;

            lblArmorAttackLabel.Left = lblArmorDeviceRating.Left + lblArmorDeviceRating.Width + 20;
            lblArmorAttack.Left = lblArmorAttackLabel.Left + lblArmorAttackLabel.Width + 6;
            lblArmorSleazeLabel.Left = lblArmorAttack.Left + lblArmorAttack.Width + 20;
            lblArmorSleaze.Left = lblArmorSleazeLabel.Left + lblArmorSleazeLabel.Width + 6;
            lblArmorDataProcessingLabel.Left = lblArmorSleaze.Left + lblArmorSleaze.Width + 20;
            lblArmorDataProcessing.Left = lblArmorDataProcessingLabel.Left + lblArmorDataProcessingLabel.Width + 6;
            lblArmorFirewallLabel.Left = lblArmorDataProcessing.Left + lblArmorDataProcessing.Width + 20;
            lblArmorFirewall.Left = lblArmorFirewallLabel.Left + lblArmorFirewallLabel.Width + 6;

            // Weapons tab.
            lblWeaponName.Left = lblWeaponNameLabel.Left + lblWeaponNameLabel.Width + 6;
            lblWeaponCategory.Left = lblWeaponCategoryLabel.Left + lblWeaponCategoryLabel.Width + 6;

            intWidth = Math.Max(lblWeaponNameLabel.Width, lblWeaponCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponDamageLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponReachLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponSlotsLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponSourceLabel.Width);

            lblWeaponName.Left = lblWeaponNameLabel.Left + intWidth + 6;
            lblWeaponCategory.Left = lblWeaponCategoryLabel.Left + intWidth + 6;
            lblWeaponDamage.Left = lblWeaponDamageLabel.Left + intWidth + 6;
            lblWeaponReach.Left = lblWeaponReachLabel.Left + intWidth + 6;
            lblWeaponAvail.Left = lblWeaponAvailLabel.Left + intWidth + 6;
            lblWeaponSlots.Left = lblWeaponSlotsLabel.Left + intWidth + 6;
            lblWeaponSource.Left = lblWeaponSourceLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblWeaponRCLabel.Width, lblWeaponModeLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponCostLabel.Width);

            lblWeaponRCLabel.Left = lblWeaponDamageLabel.Left + 176;
            lblWeaponRC.Left = lblWeaponRCLabel.Left + intWidth + 6;
            lblWeaponModeLabel.Left = lblWeaponDamageLabel.Left + 176;
            lblWeaponMode.Left = lblWeaponModeLabel.Left + intWidth + 6;
            lblWeaponCostLabel.Left = lblWeaponDamageLabel.Left + 176;
            lblWeaponCost.Left = lblWeaponCostLabel.Left + intWidth + 6;
            chkIncludedInWeapon.Left = lblWeaponDamageLabel.Left + 176;
            lblWeaponAccuracy.Left = lblWeaponAccuracyLabel.Left + lblWeaponAccuracyLabel.Width + 6;

            intWidth = Math.Max(lblWeaponAPLabel.Width, lblWeaponAmmoLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponConcealLabel.Width);

            lblWeaponAPLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponAP.Left = lblWeaponAPLabel.Left + intWidth + 6;
            lblWeaponAmmoLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponAmmo.Left = lblWeaponAmmoLabel.Left + intWidth + 6;
            lblWeaponConcealLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponConceal.Left = lblWeaponConcealLabel.Left + intWidth + 6;
            chkWeaponAccessoryInstalled.Left = lblWeaponRC.Left + 95;
            cmdWeaponMoveToVehicle.Left = chkWeaponAccessoryInstalled.Left + chkWeaponAccessoryInstalled.Width + 6;

            intWidth = Math.Max(lblWeaponAmmoRemainingLabel.Width, lblWeaponAmmoTypeLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponDicePoolLabel.Width);

            lblWeaponAmmoRemaining.Left = lblWeaponAmmoRemainingLabel.Left + intWidth + 6;
            cboWeaponAmmo.Left = lblWeaponAmmoTypeLabel.Left + intWidth + 6;
            lblWeaponDicePool.Left = lblWeaponDicePoolLabel.Left + intWidth + 6;

            cmdFireWeapon.Left = lblWeaponAmmoRemaining.Left + 123;
            cmdReloadWeapon.Left = cmdFireWeapon.Left + cmdFireWeapon.Width + 6;
            cmdWeaponBuyAmmo.Left = cmdReloadWeapon.Left + cmdReloadWeapon.Width + 6;

            cmdRollWeapon.Left = lblWeaponDicePool.Left + lblWeaponDicePool.Width + 6;
            cmdRollWeapon.Visible = CharacterObjectOptions.AllowSkillDiceRolling;

            lblWeaponAttackLabel.Left = lblWeaponDeviceRating.Left + lblWeaponDeviceRating.Width + 20;
            cboWeaponGearAttack.Left = lblWeaponAttackLabel.Left + lblWeaponAttackLabel.Width + 6;
            lblWeaponSleazeLabel.Left = cboWeaponGearAttack.Left + cboWeaponGearAttack.Width + 20;
            cboWeaponGearSleaze.Left = lblWeaponSleazeLabel.Left + lblWeaponSleazeLabel.Width + 6;
            lblWeaponDataProcessingLabel.Left = cboWeaponGearSleaze.Left + cboWeaponGearSleaze.Width + 20;
            cboWeaponGearDataProcessing.Left = lblWeaponDataProcessingLabel.Left + lblWeaponDataProcessingLabel.Width + 6;
            lblWeaponFirewallLabel.Left = cboWeaponGearDataProcessing.Left + cboWeaponGearDataProcessing.Width + 20;
            cboWeaponGearFirewall.Left = lblWeaponFirewallLabel.Left + lblWeaponFirewallLabel.Width + 6;

            // Gear tab.
            intWidth = Math.Max(lblGearNameLabel.Width, lblGearCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblGearRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblGearCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblGearQtyLabel.Width);

            chkCommlinks.Left = cmdAddLocation.Left + cmdAddLocation.Width + 16;

            lblGearName.Left = lblGearNameLabel.Left + intWidth + 6;
            lblGearCategory.Left = lblGearCategoryLabel.Left + intWidth + 6;
            lblGearRating.Left = lblGearRatingLabel.Left + intWidth + 6;
            lblGearCapacity.Left = lblGearCapacityLabel.Left + intWidth + 6;
            lblGearQty.Left = lblGearQtyLabel.Left + intWidth + 6;

            cmdGearIncreaseQty.Left = lblGearQty.Left + 57;
            cmdGearReduceQty.Left = cmdGearIncreaseQty.Left + cmdGearIncreaseQty.Width + 6;
            cmdGearSplitQty.Left = cmdGearReduceQty.Left + 79;
            cmdGearMergeQty.Left = cmdGearSplitQty.Left + cmdGearSplitQty.Width + 6;
            cmdGearMoveToVehicle.Left = cmdGearMergeQty.Left + 56;

            intWidth = lblGearDamageLabel.Width;
            lblGearDamage.Left = lblGearDamageLabel.Left + intWidth + 6;

            intWidth = lblGearAPLabel.Width;
            lblGearAP.Left = lblGearAPLabel.Left + intWidth + 6;

            lblGearSource.Left = lblGearSourceLabel.Left + lblGearSourceLabel.Width + 6;
            chkGearHomeNode.Left = chkGearEquipped.Left + chkGearEquipped.Width + 16;

            // Vehicles and Drones tab.
            intWidth = Math.Max(lblVehicleNameLabel.Width, lblVehicleCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleHandlingLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleAttackLabel.Width);
            intWidth = Math.Max(intWidth, lblVehiclePilotLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleFirewallLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleGearQtyLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSourceLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleWeaponNameLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleWeaponCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleWeaponDamageLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleWeaponAccuracyLabel.Width);

            lblVehicleName.Left = lblVehicleNameLabel.Left + intWidth + 6;
            lblVehicleCategory.Left = lblVehicleCategoryLabel.Left + intWidth + 6;
            lblVehicleHandling.Left = lblVehicleHandlingLabel.Left + intWidth + 6;
            lblVehiclePilot.Left = lblVehiclePilotLabel.Left + intWidth + 6;
            cboVehicleGearAttack.Left = lblVehicleAttackLabel.Left + intWidth + 6;
            cboVehicleGearFirewall.Left = lblVehicleFirewallLabel.Left + intWidth + 6;
            lblVehicleAvail.Left = lblVehicleAvailLabel.Left + intWidth + 6;
            lblVehicleRating.Left = lblVehicleRatingLabel.Left + intWidth + 6;
            lblVehicleGearQty.Left = lblVehicleGearQtyLabel.Left + intWidth + 6;
            lblVehicleSource.Left = lblVehicleSourceLabel.Left + intWidth + 6;
            lblVehicleWeaponName.Left = lblVehicleWeaponNameLabel.Left + intWidth + 6;
            lblVehicleWeaponCategory.Left = lblVehicleWeaponCategoryLabel.Left + intWidth + 6;
            lblVehicleWeaponDamage.Left = lblVehicleWeaponDamageLabel.Left + intWidth + 6;
            lblVehicleWeaponAccuracy.Left = lblVehicleWeaponDamageLabel.Left + intWidth + 6;
            intWidth = Math.Max(lblVehicleAccelLabel.Width, lblVehicleBodyLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleCostLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleProtectionLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleElectromagneticLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSleazeLabel.Width);

            lblVehicleAccelLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleAccel.Left = lblVehicleAccelLabel.Left + intWidth + 6;
            lblVehicleBodyLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleBody.Left = lblVehicleBodyLabel.Left + intWidth + 6;
            lblVehicleCostLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleCost.Left = lblVehicleCostLabel.Left + intWidth + 6;
            lblVehicleProtectionLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleProtection.Left = lblVehicleProtectionLabel.Left + intWidth + 6;
            lblVehicleElectromagneticLabel.Left = lblVehicleHandling.Left + 60;
            lblVehicleElectromagnetic.Left = lblVehicleElectromagneticLabel.Left + intWidth + 6;
            lblVehicleSleazeLabel.Left = lblVehicleHandling.Left + 60;
            cboVehicleGearSleaze.Left = lblVehicleSleazeLabel.Left + intWidth + 6;

            cmdVehicleGearReduceQty.Left = lblVehicleGearQtyLabel.Left + 144;
            cmdVehicleMoveToInventory.Left = cmdVehicleGearReduceQty.Left + 29;
            chkVehicleIncludedInWeapon.Left = lblVehicleAccel.Left;

            intWidth = Math.Max(lblVehicleSpeedLabel.Width, lblVehicleArmorLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleDataProcessingLabel.Width);
            intWidth = Math.Max(intWidth, lblVehiclePowertrainLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleCosmeticLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleDeviceLabel.Width);

            lblVehicleSpeedLabel.Left = lblVehicleAccel.Left + 60;
            lblVehicleSpeed.Left = lblVehicleSpeedLabel.Left + intWidth + 6;
            lblVehicleArmorLabel.Left = lblVehicleAccel.Left + 60;
            lblVehicleArmor.Left = lblVehicleArmorLabel.Left + intWidth + 6;
            lblVehiclePowertrainLabel.Left = lblVehicleAccel.Left + 60;
            lblVehiclePowertrain.Left = lblVehiclePowertrainLabel.Left + intWidth + 6;
            lblVehicleCosmeticLabel.Left = lblVehicleAccel.Left + 60;
            lblVehicleCosmetic.Left = lblVehicleCosmeticLabel.Left + intWidth + 6;
            lblVehicleDataProcessingLabel.Left = lblVehicleAccel.Left + 60;
            cboVehicleGearDataProcessing.Left = lblVehicleDataProcessingLabel.Left + intWidth + 6;
            lblVehicleDeviceLabel.Left = lblVehicleAccel.Left + 60;
            lblVehicleDevice.Left = lblVehicleDeviceLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblVehicleFirewallLabel.Width, lblVehicleSensorLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSeatsLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleDroneModSlotsLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSlotsLabel.Width);

            lblVehicleSensorLabel.Left = lblVehicleSpeed.Left + 60;
            lblVehicleSensor.Left = lblVehicleSensorLabel.Left + intWidth + 6;
            lblVehicleSeatsLabel.Left = lblVehicleSpeed.Left + 60;
            lblVehicleSeats.Left = lblVehicleSeatsLabel.Left + intWidth + 6;
            lblVehicleFirewallLabel.Left = lblVehicleSpeed.Left + 60;
            lblVehicleDroneModSlotsLabel.Left = lblVehicleSpeed.Left + 60;
            lblVehicleDroneModSlots.Left = lblVehicleDroneModSlotsLabel.Left + intWidth + 6;

            cboVehicleGearFirewall.Left = lblVehicleFirewallLabel.Left + intWidth + 6;

            lblVehicleSlotsLabel.Left = lblVehicleSpeed.Left + 60;
            lblVehicleSlots.Left = lblVehicleSlotsLabel.Left + intWidth + 6;

            chkVehicleHomeNode.Left = lblVehicleSlotsLabel.Left;
            chkVehicleWeaponAccessoryInstalled.Left = lblVehicleSlotsLabel.Left;

            // Character Info.
            intWidth = Math.Max(lblSex.Width, lblHeight.Width);
            txtSex.Left = lblSex.Left + intWidth + 6;
            txtSex.Width = lblAge.Left - txtSex.Left - 16;
            txtHeight.Left = lblHeight.Left + intWidth + 6;
            txtHeight.Width = lblWeight.Left - txtHeight.Left - 16;

            intWidth = Math.Max(lblAge.Width, lblWeight.Width);
            txtAge.Left = lblAge.Left + intWidth + 6;
            txtAge.Width = lblEyes.Left - txtAge.Left - 16;
            txtWeight.Left = lblWeight.Left + intWidth + 6;
            txtWeight.Width = lblSkin.Left - txtWeight.Left - 16;

            intWidth = Math.Max(lblEyes.Width, lblSkin.Width);
            txtEyes.Left = lblEyes.Left + intWidth + 6;
            txtEyes.Width = lblHair.Left - txtEyes.Left - 16;
            txtSkin.Left = lblSkin.Left + intWidth + 6;
            txtSkin.Width = lblCharacterName.Left - txtSkin.Left - 16;

            intWidth = Math.Max(lblHair.Width, lblCharacterName.Width);
            txtHair.Left = lblHair.Left + intWidth + 6;
            txtHair.Width = lblPlayerName.Left - txtHair.Left - 16;
            txtCharacterName.Left = lblCharacterName.Left + intWidth + 6;
            txtCharacterName.Width = lblPlayerName.Left - txtCharacterName.Left - 16;

            txtPlayerName.Left = lblPlayerName.Left + lblPlayerName.Width + 6;
            txtPlayerName.Width = tabCharacterInfo.Width - txtPlayerName.Left - 16;

            intWidth = Math.Max(lblStreetCred.Width, lblNotoriety.Width);
            intWidth = Math.Max(intWidth, lblPublicAware.Width);

            nudStreetCred.Left = lblStreetCred.Left + intWidth + 6;
            nudNotoriety.Left = lblNotoriety.Left + intWidth + 6;
            nudPublicAware.Left = lblPublicAware.Left + intWidth + 6;
            lblStreetCredTotal.Left = nudStreetCred.Left + nudStreetCred.Width + 6;
            lblNotorietyTotal.Left = nudNotoriety.Left + nudNotoriety.Width + 6;
            lblPublicAwareTotal.Left = nudPublicAware.Left + nudPublicAware.Width + 6;

            // Expense Tab.
            cmdKarmaSpent.Left = cmdKarmaGained.Left + cmdKarmaGained.Width + 6;
            cmdKarmaEdit.Left = cmdKarmaSpent.Left + cmdKarmaSpent.Width + 6;
            chkShowFreeKarma.Left = cmdKarmaEdit.Left + cmdKarmaEdit.Width + 6;
            cmdNuyenSpent.Left = cmdNuyenGained.Left + cmdNuyenGained.Width + 6;
            cmdNuyenEdit.Left = cmdNuyenSpent.Left + cmdNuyenSpent.Width + 6;
            chkShowFreeNuyen.Left = cmdNuyenEdit.Left + cmdNuyenEdit.Width + 6;
            // Calendar Tab.
            cmdEditWeek.Left = cmdAddWeek.Left + cmdAddWeek.Width + 6;

            // Improvements tab.
            cmdEditImprovement.Left = cmdAddImprovement.Left + cmdAddImprovement.Width + 6;
            cmdDeleteImprovement.Left = cmdEditImprovement.Left + cmdEditImprovement.Width + 6;
            cmdAddImprovementGroup.Left = cmdDeleteImprovement.Left + cmdDeleteImprovement.Width + 6;
            lblImprovementType.Left = lblImprovementTypeLabel.Left + lblImprovementTypeLabel.Width + 6;

            // Other Info tab.
            intWidth = Math.Max(lblCMPhysicalLabel.Width, lblCMStunLabel.Width);
            intWidth = Math.Max(intWidth, lblINILabel.Width);
            intWidth = Math.Max(intWidth, lblMatrixINILabel.Width);
            intWidth = Math.Max(intWidth, lblAstralINILabel.Width);
            intWidth = Math.Max(intWidth, lblRiggingINILabel.Width);
            intWidth = Math.Max(intWidth, lblMatrixINIColdLabel.Width);
            intWidth = Math.Max(intWidth, lblMatrixINIHotLabel.Width);
            intWidth = Math.Max(intWidth, lblArmorLabel.Width);
            intWidth = Math.Max(intWidth, lblESS.Width);
            intWidth = Math.Max(intWidth, lblRemainingNuyenLabel.Width);
            intWidth = Math.Max(intWidth, lblCareerKarmaLabel.Width);
            intWidth = Math.Max(intWidth, lblCareerNuyenLabel.Width);
            intWidth = Math.Max(intWidth, lblComposureLabel.Width);
            intWidth = Math.Max(intWidth, lblJudgeIntentionsLabel.Width);
            intWidth = Math.Max(intWidth, lblLiftCarryLabel.Width);
            intWidth = Math.Max(intWidth, lblMemoryLabel.Width);
            intWidth = Math.Max(intWidth, lblMovementLabel.Width);
            intWidth = Math.Max(intWidth, lblSwimLabel.Width);
            intWidth = Math.Max(intWidth, lblFlyLabel.Width);

            lblCMPhysical.Left = lblPhysicalCMLabel.Left + intWidth + 6;
            lblCMStun.Left = lblCMPhysical.Left;
            lblINI.Left = lblCMPhysical.Left;
            lblMatrixINI.Left = lblCMPhysical.Left;
            lblAstralINI.Left = lblCMPhysical.Left;
            lblRiggingINI.Left = lblCMPhysical.Left;
            lblMatrixINICold.Left = lblCMPhysical.Left;
            lblMatrixINIHot.Left = lblCMPhysical.Left;
            lblArmor.Left = lblCMPhysical.Left;
            lblESSMax.Left = lblCMPhysical.Left;
            lblRemainingNuyen.Left = lblCMPhysical.Left;
            lblCareerKarma.Left = lblCMPhysical.Left;
            lblCareerNuyen.Left = lblCMPhysical.Left;
            lblComposure.Left = lblCMPhysical.Left;
            lblJudgeIntentions.Left = lblCMPhysical.Left;
            lblLiftCarry.Left = lblCMPhysical.Left;
            lblMemory.Left = lblCMPhysical.Left;
            lblMovement.Left = lblCMPhysical.Left;
            lblSwim.Left = lblCMPhysical.Left;
            lblFly.Left = lblCMPhysical.Left;

            // Condition Monitor tab.
            intWidth = Math.Max(lblCMPenaltyLabel.Width, lblCMArmorLabel.Width);
            intWidth = Math.Max(intWidth, lblCMDamageResistancePoolLabel.Width);

            lblCMPenalty.Left = lblCMPenaltyLabel.Left + intWidth + 6;
            lblCMArmor.Left = lblCMPenalty.Left;
            lblCMDamageResistancePool.Left = lblCMPenalty.Left;

            // Relationships tab
            cmdContactsExpansionToggle.Left = cmdAddContact.Right + 6;
            cmdSwapContactOrder.Left = cmdContactsExpansionToggle.Right + 6;
        }



        /// <summary>
        /// Recheck all mods to see if Sensor has changed.
        /// </summary>
        /// <param name="objVehicle">Vehicle to modify.</param>
        private void UpdateSensor(Vehicle objVehicle)
        {
            foreach (Gear objGear in objVehicle.Gear)
            {
                if (objGear.Category == "Sensors" && objGear.Name == "Sensor Array" && objGear.IncludedInParent)
                {
                    // Update the name of the item in the TreeView.
                    TreeNode objNode = treVehicles.FindNode(objGear.InternalId);
                    objNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                }
            }
        }
        
        private void AdjustBurnStreetCredButtonLocation(object sender, EventArgs e)
        {
            cmdBurnStreetCred.Left = lblStreetCredTotal.Left + lblStreetCredTotal.Width + 6;
        }

        /// <summary>
        /// Copy the Improvements from a piece of Armor on one character to another.
        /// </summary>
        /// <param name="objSource">Source character.</param>
        /// <param name="objDestination">Destination character.</param>
        /// <param name="objArmor">Armor to copy.</param>
        private void CopyArmorImprovements(Character objSource, Character objDestination, Armor objArmor)
        {
            foreach (Improvement objImproevment in objSource.Improvements)
            {
                if (objImproevment.SourceName == objArmor.InternalId)
                {
                    objDestination.Improvements.Add(objImproevment);
                }
            }
            // Look through any Armor Mods and add the Improvements as well.
            foreach (ArmorMod objMod in objArmor.ArmorMods)
            {
                foreach (Improvement objImproevment in objSource.Improvements)
                {
                    if (objImproevment.SourceName == objMod.InternalId)
                    {
                        objDestination.Improvements.Add(objImproevment);
                    }
                }
                // Look through any children and add their Improvements as well.
                foreach (Gear objChild in objMod.Gear)
                    CopyGearImprovements(objSource, objDestination, objChild);
            }
            // Look through any children and add their Improvements as well.
            foreach (Gear objChild in objArmor.Gear)
                CopyGearImprovements(objSource, objDestination, objChild);
        }

        /// <summary>
        /// Copy the Improvements from a piece of Gear on one character to another.
        /// </summary>
        /// <param name="objSource">Source character.</param>
        /// <param name="objDestination">Destination character.</param>
        /// <param name="objGear">Gear to copy.</param>
        private void CopyGearImprovements(Character objSource, Character objDestination, Gear objGear)
        {
            foreach (Improvement objImproevment in objSource.Improvements)
            {
                if (objImproevment.SourceName == objGear.InternalId)
                {
                    objDestination.Improvements.Add(objImproevment);
                }
            }
            // Look through any children and add their Improvements as well.
            foreach (Gear objChild in objGear.Children)
                CopyGearImprovements(objSource, objDestination, objChild);
        }

        /// <summary>
        /// Copy the Improvements from a piece of Cyberware on one character to another.
        /// </summary>
        /// <param name="objSource">Source character.</param>
        /// <param name="objDestination">Destination character.</param>
        /// <param name="objCyberware">Cyberware to copy.</param>
        private void CopyCyberwareImprovements(Character objSource, Character objDestination, Cyberware objCyberware)
        {
            foreach (Improvement objImproevment in objSource.Improvements)
            {
                if (objImproevment.SourceName == objCyberware.InternalId)
                {
                    objDestination.Improvements.Add(objImproevment);
                }
            }
            // Look through any children and add their Improvements as well.
            foreach (Cyberware objChild in objCyberware.Children)
                CopyCyberwareImprovements(objSource, objDestination, objChild);
        }

        /// <summary>
        /// Recursive method to add a Gear's Improvements to a character when moving them from a Vehicle.
        /// </summary>
        /// <param name="objGear">Gear to create Improvements for.
        /// </param>
        private void AddGearImprovements(Gear objGear)
        {
            string strForce = string.Empty;
            if (objGear.Bonus != null || (objGear.WirelessOn && objGear.WirelessBonus != null))
            {
                if (!string.IsNullOrEmpty(objGear.Extra))
                    strForce = objGear.Extra;
                ImprovementManager.ForcedValue = strForce;
                if (objGear.Bonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, true, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                if (objGear.WirelessOn && objGear.WirelessBonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, true, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
            }
            foreach (Gear objChild in objGear.Children)
                AddGearImprovements(objChild);
        }

        /// <summary>
        /// Enable/Disable the Paste Menu and ToolStrip items as appropriate.
        /// </summary>
        private void RefreshPasteStatus()
        {
            bool blnCopyEnabled = false;

            if (tabCharacterTabs.SelectedTab == tabStreetGear)
            {
                // Lifestyle Tab.
                if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                {
                    blnCopyEnabled = treLifestyles.SelectedNode?.Tag is Lifestyle;
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    blnCopyEnabled = treArmor.SelectedNode?.Tag is Armor ||
                                     treArmor.SelectedNode?.Tag is Gear;
                }

                // Weapons Tab.
                if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    blnCopyEnabled = treWeapons.SelectedNode?.Tag is Weapon ||
                                     treWeapons.SelectedNode?.Tag is Gear;
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    blnCopyEnabled = treWeapons.SelectedNode?.Tag is Gear;
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                blnCopyEnabled = treCyberware.SelectedNode?.Tag is Cyberware ||
                                 treCyberware.SelectedNode?.Tag is Gear;
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles && treVehicles.SelectedNode != null)
            {
                blnCopyEnabled = treVehicles.SelectedNode?.Tag is Vehicle ||
                                 treVehicles.SelectedNode?.Tag is Gear ||
                                 treVehicles.SelectedNode?.Tag is Weapon;
            }

            mnuEditCopy.Enabled = blnCopyEnabled;
            tsbCopy.Enabled = blnCopyEnabled;
        }

        /// <summary>
        /// Refresh the information for the currently selected Complex Form.
        /// </summary>
        private void RefreshSelectedComplexForm()
        {
            if (_blnSkipRefresh)
                return;

            // Locate the Program that is selected in the tree.
            if (treComplexForms.SelectedNode?.Tag is ComplexForm objComplexForm)
            {
                cmdDeleteComplexForm.Enabled = objComplexForm.Grade == 0;
                lblDuration.Text = objComplexForm.DisplayDuration(GlobalOptions.Language);
                lblTarget.Text = objComplexForm.DisplayTarget(GlobalOptions.Language);
                lblFV.Text = objComplexForm.DisplayFV(GlobalOptions.Language);
                GlobalOptions.ToolTipProcessor.SetToolTip(lblFV, objComplexForm.FVTooltip);

                string strPage = objComplexForm.Page(GlobalOptions.Language);
                lblComplexFormSource.Text = CommonFunctions.LanguageBookShort(objComplexForm.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblComplexFormSource, CommonFunctions.LanguageBookLong(objComplexForm.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            }
            else
            {
                cmdDeleteComplexForm.Enabled = false;
                lblDuration.Text = string.Empty;
                lblTarget.Text = string.Empty;
                lblFV.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblFV, string.Empty);
                lblComplexFormSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblComplexFormSource, string.Empty);
            }
        }

        /// <summary>
        /// Create Cyberware from a Cyberware Suite.
        /// </summary>
        /// <param name="xmlSuiteNode">XmlNode for the cyberware suite to add.</param>
        /// <param name="xmlCyberwareNode">XmlNode for the Cyberware to add.</param>
        /// <param name="objGrade">CyberwareGrade to add the item as.</param>
        /// <param name="intRating">Rating of the Cyberware.</param>
        /// <param name="eSource">Source representing whether the suite is cyberware or bioware.</param>
        private Cyberware CreateSuiteCyberware(XmlNode xmlSuiteNode, XmlNode xmlCyberwareNode, Grade objGrade, int intRating, Improvement.ImprovementSource eSource)
        {
            // Create the Cyberware object.
            List<Weapon> lstWeapons = new List<Weapon>();
            List<Vehicle> lstVehicles = new List<Vehicle>();
            Cyberware objCyberware = new Cyberware(CharacterObject);
            string strForced = xmlSuiteNode.SelectSingleNode("name/@select")?.InnerText ?? string.Empty;

            objCyberware.Create(xmlCyberwareNode, CharacterObject, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, true, strForced);
            objCyberware.Suite = true;

            foreach (Weapon objWeapon in lstWeapons)
            {
                CharacterObject.Weapons.Add(objWeapon);
            }

            foreach (Vehicle objVehicle in lstVehicles)
            {
                CharacterObject.Vehicles.Add(objVehicle);
            }

            string strType = eSource == Improvement.ImprovementSource.Cyberware ? "cyberware" : "bioware";
            using (XmlNodeList xmlChildrenList = xmlSuiteNode.SelectNodes(strType + "s/" + strType))
                if (xmlChildrenList?.Count > 0)
                {
                    XmlDocument objXmlDocument = XmlManager.Load(strType + ".xml");
                    foreach (XmlNode objXmlChild in xmlChildrenList)
                    {
                        XmlNode objXmlChildCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strType + "s/" + strType + "[name = \"" + objXmlChild["name"]?.InnerText + "\"]");
                        int intChildRating = Convert.ToInt32(objXmlChild["rating"]?.InnerText);

                        objCyberware.Children.Add(CreateSuiteCyberware(objXmlChild, objXmlChildCyberware, objGrade, intChildRating, eSource));
                    }
                }

            return objCyberware;
        }

        private void AddCyberwareSuite(Improvement.ImprovementSource objSource)
        {
            frmSelectCyberwareSuite frmPickCyberwareSuite = new frmSelectCyberwareSuite(CharacterObject, objSource);
            frmPickCyberwareSuite.ShowDialog(this);

            if (frmPickCyberwareSuite.DialogResult == DialogResult.Cancel)
                return;

            decimal decCost = frmPickCyberwareSuite.TotalCost;
            if (decCost > CharacterObject.Nuyen)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughNuyen", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughNuyen", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strType = objSource == Improvement.ImprovementSource.Cyberware ? "cyberware" : "bioware";
            XmlDocument objXmlDocument = XmlManager.Load(strType + ".xml");

            XmlNode xmlSuite = null;
            if (Guid.TryParse(frmPickCyberwareSuite.SelectedSuite, out Guid _result))
            {
                xmlSuite = objXmlDocument.SelectSingleNode("/chummer/suites/suite[id = \"" + frmPickCyberwareSuite.SelectedSuite + "\"]");
            }
            else
            {
                xmlSuite = objXmlDocument.SelectSingleNode("/chummer/suites/suite[name = \"" + frmPickCyberwareSuite.SelectedSuite + "\"]");
            }
            if (xmlSuite == null)
                return;

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(decCost * -1, LanguageManager.GetString("String_ExpensePurchaseCyberwareSuite", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + xmlSuite["name"].InnerText, ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen -= decCost;

            Grade objGrade = Cyberware.ConvertToCyberwareGrade(xmlSuite["grade"]?.InnerText, objSource, CharacterObject);

            // Run through each of the items in the Suite and add them to the character.
            using (XmlNodeList xmlItemList = xmlSuite.SelectNodes(strType + "s/" + strType))
                if (xmlItemList?.Count > 0)
                    foreach (XmlNode xmlItem in xmlItemList)
                    {
                        XmlNode objXmlCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strType + "s/" + strType + "[name = \"" + xmlItem["name"]?.InnerText + "\"]");
                        int intRating = Convert.ToInt32(xmlItem["rating"]?.InnerText);

                        Cyberware objCyberware = CreateSuiteCyberware(xmlItem, objXmlCyberware, objGrade, intRating, objSource);
                        CharacterObject.Cyberware.Add(objCyberware);
                    }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
#endregion

        private void cmdAddLimitModifier_Click(object sender, EventArgs e)
        {
            frmSelectLimitModifier frmPickLimitModifier = new frmSelectLimitModifier(null, "Physical", "Mental", "Social");
            frmPickLimitModifier.ShowDialog(this);

            if (frmPickLimitModifier.DialogResult == DialogResult.Cancel)
                return;

            // Create the new limit modifier.
            LimitModifier objLimitModifier = new LimitModifier(CharacterObject);
            objLimitModifier.Create(frmPickLimitModifier.SelectedName, frmPickLimitModifier.SelectedBonus, frmPickLimitModifier.SelectedLimitType, frmPickLimitModifier.SelectedCondition);
            if (objLimitModifier.InternalId.IsEmptyGuid())
                return;

            CharacterObject.LimitModifiers.Add(objLimitModifier);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdDeleteLimitModifier_Click(object sender, EventArgs e)
        {
            if (!(treLimit.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject)) return;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void tssLimitModifierNotes_Click(object sender, EventArgs e)
        {
            if (treLimit.SelectedNode != null)
            {
                if (treMetamagic.SelectedNode?.Tag is IHasNotes objNotes)
                {
                    WriteNotes(objNotes, treMetamagic.SelectedNode);
                }
                else
                {
                    // the limit modifier has a source
                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                            objImprovement.SourceName == treLimit.SelectedNode?.Tag.ToString())
                        {
                            string strOldValue = objImprovement.Notes;
                            frmNotes frmItemNotes = new frmNotes
                            {
                                Notes = strOldValue
                            };
                            frmItemNotes.ShowDialog(this);

                            if (frmItemNotes.DialogResult == DialogResult.OK)
                            {
                                objImprovement.Notes = frmItemNotes.Notes;
                                if (objImprovement.Notes != strOldValue)
                                {
                                    IsDirty = true;

                                    treLimit.SelectedNode.ForeColor = objImprovement.PreferredColor;
                                    treLimit.SelectedNode.ToolTipText = objImprovement.Notes.WordWrap(100);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void cmdIncreasePowerPoints_Click(object sender, EventArgs e)
        {
            // Make sure the character has enough Karma to improve the CharacterAttribute.
            int intKarmaCost = CharacterObject.Options.KarmaMysticAdeptPowerPoint;
            if (intKarmaCost > CharacterObject.Karma)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (CharacterObject.MysticAdeptPowerPoints + 1 > CharacterObject.MAG.TotalValue)
            {
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughMagic", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughMagic", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_PowerPoint", GlobalOptions.Language)).Replace("{1}", (intKarmaCost).ToString())))
                return;

            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_PowerPoint", GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddPowerPoint, string.Empty);
            objExpense.Undo = objUndo;

            CharacterObject.MysticAdeptPowerPoints += 1;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddMetamagic_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;

            int intGrade = 0;
            if ((treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                intGrade = objGrade.Grade;

            // Evaluate each object
            foreach (Metamagic objMetamagic in CharacterObject.Metamagics)
            {
                if (objMetamagic.Grade == intGrade)
                {
                    blnPayWithKarma = true;
                    break;
                }
            }
            if (!blnPayWithKarma)
            {
                foreach (Spell objSpell in CharacterObject.Spells)
                {
                    if (objSpell.Grade == intGrade)
                    {
                        blnPayWithKarma = true;
                        break;
                    }
                }
            }

            // Additional Metamagics beyond the standard 1 per Grade cost additional Karma, so ask if the user wants to spend the additional Karma.
            if (blnPayWithKarma && CharacterObject.Karma < CharacterObjectOptions.KarmaMetamagic)
            {
                // Make sure the Karma expense would not put them over the limit.
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (CharacterObject.MAGEnabled && blnPayWithKarma)
            {
                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)).Replace("{1}", CharacterObjectOptions.KarmaMetamagic.ToString())))
                    return;
            }
            else if (blnPayWithKarma)
            {
                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_Echo", GlobalOptions.Language)).Replace("{1}", CharacterObjectOptions.KarmaMetamagic.ToString())))
                    return;
            }

            frmSelectMetamagic frmPickMetamagic = new frmSelectMetamagic(CharacterObject, CharacterObject.RESEnabled ? frmSelectMetamagic.Mode.Echo : frmSelectMetamagic.Mode.Metamagic);
            frmPickMetamagic.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickMetamagic.DialogResult == DialogResult.Cancel)
            {
                frmPickMetamagic.Dispose();
                return;
            }

            Metamagic objNewMetamagic = new Metamagic(CharacterObject);

            XmlNode objXmlMetamagic;
            Improvement.ImprovementSource objSource;
            if (CharacterObject.RESEnabled)
            {
                objXmlMetamagic = XmlManager.Load("echoes.xml").SelectSingleNode("/chummer/echoes/echo[id = \"" + frmPickMetamagic.SelectedMetamagic + "\"]");
                objSource = Improvement.ImprovementSource.Echo;
            }
            else
            {
                objXmlMetamagic = XmlManager.Load("metamagic.xml").SelectSingleNode("/chummer/metamagics/metamagic[id = \"" + frmPickMetamagic.SelectedMetamagic + "\"]");
                objSource = Improvement.ImprovementSource.Metamagic;
            }
            frmPickMetamagic.Dispose();

            objNewMetamagic.Create(objXmlMetamagic, objSource);
            objNewMetamagic.Grade = intGrade;
            if (objNewMetamagic.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Metamagics.Add(objNewMetamagic);

            if (blnPayWithKarma)
            {
                string strType = LanguageManager.GetString(objNewMetamagic.SourceType == Improvement.ImprovementSource.Echo ? "String_Echo" : "String_Metamagic", GlobalOptions.Language);
                // Create the Expense Log Entry.
                ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(CharacterObjectOptions.KarmaMetamagic * -1, strType + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objNewMetamagic.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddMetamagic, objNewMetamagic.InternalId);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= CharacterObjectOptions.KarmaMetamagic;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddArt_Click(object sender, EventArgs e)
        {
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;

            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            if ((treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                intGrade = objGrade.Grade;

            if (blnPayWithKarma && CharacterObject.Karma < CharacterObjectOptions.KarmaMetamagic)
            {
                // Make sure the Karma expense would not put them over the limit.
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Art);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("metamagic.xml").SelectSingleNode("/chummer/arts/art[id = \"" + frmPickArt.SelectedItem + "\"]");
            
            Art objArt = new Art(CharacterObject);
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Metamagic;

            objArt.Create(objXmlArt, objSource);
            objArt.Grade = intGrade;
            if (objArt.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Arts.Add(objArt);

            if (blnPayWithKarma)
            {
                string strType = LanguageManager.GetString("String_Art", GlobalOptions.Language);
                // Create the Expense Log Entry.
                ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(CharacterObjectOptions.KarmaMetamagic * -1, strType + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objArt.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddMetamagic, objArt.InternalId);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= CharacterObjectOptions.KarmaMetamagic;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddEnchantment_Click(object sender, EventArgs e)
        {
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;

            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            if ((treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                intGrade = objGrade.Grade;

            // Evaluate each object
            foreach (Metamagic objMetamagic in CharacterObject.Metamagics)
            {
                if (objMetamagic.Grade == intGrade)
                    blnPayWithKarma = true;
            }

            foreach (Spell objSpell in CharacterObject.Spells)
            {
                if (objSpell.Grade == intGrade)
                    blnPayWithKarma = true;
            }

            int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Enchantments");

            if (blnPayWithKarma && CharacterObject.Karma < intSpellKarmaCost)
            {
                // Make sure the Karma expense would not put them over the limit.
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (blnPayWithKarma)
                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_Enchantment", GlobalOptions.Language)).Replace("{1}", intSpellKarmaCost.ToString())))
                    return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Enchantment);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");

            Spell objNewSpell = new Spell(CharacterObject);
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            if (blnPayWithKarma)
            {
                string strType = LanguageManager.GetString("String_Enhancement", GlobalOptions.Language);
                // Create the Expense Log Entry.
                ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(-intSpellKarmaCost, strType + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objNewSpell.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddSpell, objNewSpell.InternalId);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= intSpellKarmaCost;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddRitual_Click(object sender, EventArgs e)
        {
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;

            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            if ((treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                intGrade = objGrade.Grade;

            // Evaluate each object
            foreach (Metamagic objMetamagic in CharacterObject.Metamagics)
            {
                if (objMetamagic.Grade == intGrade)
                {
                    blnPayWithKarma = true;
                    break;
                }
            }

            if (!blnPayWithKarma)
            {
                foreach (Spell objSpell in CharacterObject.Spells)
                {
                    if (objSpell.Grade == intGrade)
                    {
                        blnPayWithKarma = true;
                        break;
                    }
                }
            }

            int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Rituals");
            if (blnPayWithKarma && CharacterObject.Karma < intSpellKarmaCost)
            {
                // Make sure the Karma expense would not put them over the limit.
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (blnPayWithKarma)
                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_Ritual", GlobalOptions.Language)).Replace("{1}", intSpellKarmaCost.ToString())))
                    return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Ritual);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");

            Spell objNewSpell = new Spell(CharacterObject);
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            if (blnPayWithKarma)
            {
                string strType = LanguageManager.GetString("String_Ritual", GlobalOptions.Language);
                // Create the Expense Log Entry.
                ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create(-intSpellKarmaCost, strType + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objNewSpell.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddSpell, objNewSpell.InternalId);
                objEntry.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= intSpellKarmaCost;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsInitiationNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treMetamagic.SelectedNode);

            IsDirty = true;
        }

        private void tsMetamagicAddEnhancement_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            if ((treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                intGrade = objGrade.Grade;

            if (CharacterObject.Karma < CharacterObjectOptions.KarmaEnhancement)
            {
                // Make sure the Karma expense would not put them over the limit.
                MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", LanguageManager.GetString("String_Enhancement", GlobalOptions.Language)).Replace("{1}", CharacterObjectOptions.KarmaEnhancement.ToString())))
                return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Enhancement);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;
            
            XmlNode objXmlArt = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/enhancements/enhancement[id = \"" + frmPickArt.SelectedItem + "\"]");
            if (objXmlArt == null)
                return;

            Enhancement objEnhancement = new Enhancement(CharacterObject);
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;
            
            objEnhancement.Create(objXmlArt, objSource);
            objEnhancement.Grade = intGrade;
            if (objEnhancement.InternalId.IsEmptyGuid())
                return;

            // Find the associated Power
            string strPower = objXmlArt["power"]?.InnerText;
            bool blnPowerFound = false;
            foreach (Power objPower in CharacterObject.Powers)
            {
                if (objPower.Name == strPower)
                {
                    objPower.Enhancements.Add(objEnhancement);
                    blnPowerFound = true;
                    break;
                }
            }

            if (!blnPowerFound)
            {
                // Add it to the character instead
                CharacterObject.Enhancements.Add(objEnhancement);
            }

            string strType = LanguageManager.GetString("String_Enhancement", GlobalOptions.Language);
            // Create the Expense Log Entry.
            ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
            objEntry.Create(CharacterObjectOptions.KarmaEnhancement * -1, strType + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objEnhancement.DisplayNameShort(GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objEntry);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddSpell, objEnhancement.InternalId);
            objEntry.Undo = objUndo;

            // Adjust the character's Karma total.
            CharacterObject.Karma -= CharacterObjectOptions.KarmaEnhancement;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void panContacts_Click(object sender, EventArgs e)
        {
            panContacts.Focus();
        }

        private void panEnemies_Click(object sender, EventArgs e)
        {
            panEnemies.Focus();
        }

        private void cboGearOverclocker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !CharacterObject.Overclocker)
                return;
            if (!(treGear.SelectedNode?.Tag is Gear objCommlink))
                return;
            objCommlink.Overclocked = cboGearOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeCBOs(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);
        }

        private void cboCyberwareGearOverclocker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || !CharacterObject.Overclocker)
                return;
            /* Que? 
            List<Gear> lstGearToSearch = new List<Gear>(CharacterObject.Gear);
            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
            {
                lstGearToSearch.AddRange(objCyberware.Gear);
            }*/
            if (!(treCyberware.SelectedNode?.Tag is Gear objCommlink))
                return;
            objCommlink.Overclocked = cboCyberwareGearOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeCBOs(cboCyberwareGearAttack, cboCyberwareGearSleaze, cboCyberwareGearDataProcessing, cboCyberwareGearFirewall);
        }

        private void tssLimitModifierEdit_Click(object sender, EventArgs e)
        {
            UpdateLimitModifier(treLimit);
        }

        private void cmdAddAIProgram_Click(object sender, EventArgs e)
        {
            int intNewAIProgramCost = CharacterObject.AIProgramKarmaCost;
            int intNewAIAdvancedProgramCost = CharacterObject.AIAdvancedProgramKarmaCost;
            XmlDocument objXmlDocument = XmlManager.Load("programs.xml");

            bool blnAddAgain;
            do
            {
                // Make sure the character has enough Karma before letting them select a Spell.
                if (CharacterObject.Karma < intNewAIProgramCost && CharacterObject.Karma < intNewAIAdvancedProgramCost)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_NotEnoughKarma", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NotEnoughKarma", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                // Let the user select a Program.
                frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(CharacterObject, CharacterObject.Karma >= intNewAIAdvancedProgramCost);
                frmPickProgram.ShowDialog(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickProgram.DialogResult == DialogResult.Cancel)
                {
                    frmPickProgram.Dispose();
                    break;
                }
                blnAddAgain = frmPickProgram.AddAgain;

                XmlNode objXmlProgram = objXmlDocument.SelectSingleNode("/chummer/programs/program[id = \"" + frmPickProgram.SelectedProgram + "\"]");
                frmPickProgram.Dispose();
                if (objXmlProgram == null)
                    continue;

                // Check for SelectText.
                string strExtra = string.Empty;
                XmlNode xmlSelectText = objXmlProgram.SelectSingleNode("bonus/selecttext");
                if (xmlSelectText != null)
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language))
                    };
                    frmPickText.ShowDialog(this);
                    strExtra = frmPickText.SelectedValue;
                }

                AIProgram objProgram = new AIProgram(CharacterObject);
                objProgram.Create(objXmlProgram, strExtra);
                if (objProgram.InternalId.IsEmptyGuid())
                    continue;

                bool boolIsAdvancedProgram = objProgram.IsAdvancedProgram;
                if (!CharacterObject.ConfirmKarmaExpense(LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend", GlobalOptions.Language).Replace("{0}", objProgram.DisplayName).Replace("{1}", (boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost).ToString())))
                    continue;

                CharacterObject.AIPrograms.Add(objProgram);

                // Create the Expense Log Entry.
                ExpenseLogEntry objEntry = new ExpenseLogEntry(CharacterObject);
                objEntry.Create((boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost) * -1, LanguageManager.GetString("String_ExpenseLearnProgram", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + objProgram.Name, ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objEntry);
                CharacterObject.Karma -= (boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma((boolIsAdvancedProgram ? KarmaExpenseType.AddAIAdvancedProgram : KarmaExpenseType.AddAIProgram), objProgram.InternalId);
                objEntry.Undo = objUndo;
                
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteAIProgram_Click(object sender, EventArgs e)
        {
            // Delete the selected AI Program.
            if (!(treAIPrograms.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject)) return;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void treAIPrograms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the Program that is selected in the tree.
            if (treAIPrograms.SelectedNode?.Tag is AIProgram objProgram)
            {
                lblAIProgramsRequires.Text = objProgram.DisplayRequiresProgram(GlobalOptions.Language);

                string strPage = objProgram.Page(GlobalOptions.Language);
                lblAIProgramsSource.Text = CommonFunctions.LanguageBookShort(objProgram.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAIProgramsSource, CommonFunctions.LanguageBookLong(objProgram.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            }
            else
            {
                lblAIProgramsRequires.Text = string.Empty;
                lblAIProgramsSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAIProgramsSource, string.Empty);
            }
        }

        private void treAIPrograms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteAIProgram_Click(sender, e);
            }
        }

        private void tsAIProgramNotes_Click(object sender, EventArgs e)
        {
            if (!(treAIPrograms.SelectedNode?.Tag is IHasNotes selectedObject)) return;
            WriteNotes(selectedObject, treAIPrograms.SelectedNode);

            IsDirty = true;
        }

        private void cboPrimaryArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || CharacterObject.Ambidextrous)
                return;
            CharacterObject.PrimaryArm = cboPrimaryArm.SelectedValue.ToString();

            IsDirty = true;
        }
        
        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void cmdCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objModularCyberware))
                return;
            frmSelectItem frmPickMount = new frmSelectItem
            {
                GeneralItems = CharacterObject.ConstructModularCyberlimbList(objModularCyberware),
                Description = LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language)
            };
            frmPickMount.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMount.DialogResult == DialogResult.Cancel)
            {
                return;
            }

            Cyberware objOldParent = objModularCyberware.Parent;
            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);
            string strSelectedParentID = frmPickMount.SelectedItem;
            if (strSelectedParentID == "None")
            {
                if (objOldParent != null)
                {
                    objOldParent.Children.Remove(objModularCyberware);

                    CharacterObject.Cyberware.Add(objModularCyberware);
                }
            }
            else
            {
                Cyberware objNewParent = CharacterObject.Cyberware.DeepFindById(strSelectedParentID);
                if (objNewParent != null)
                {
                    if (objOldParent != null)
                        objOldParent.Children.Remove(objModularCyberware);
                    else
                        CharacterObject.Cyberware.Remove(objModularCyberware);

                    objNewParent.Children.Add(objModularCyberware);

                    objModularCyberware.ChangeModularEquip(true);
                }
                else
                {
                    VehicleMod objNewVehicleModParent = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedParentID);
                    if (objNewVehicleModParent == null)
                        objNewParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedParentID, out objNewVehicleModParent);
                    if (objNewVehicleModParent != null || objNewParent != null)
                    {
                        if (objOldParent != null)
                            objOldParent.Children.Remove(objModularCyberware);
                        else
                            CharacterObject.Cyberware.Remove(objModularCyberware);

                        if (objNewParent != null)
                            objNewParent.Children.Add(objModularCyberware);
                        else
                            objNewVehicleModParent.Cyberware.Add(objModularCyberware);
                    }
                    else
                    {
                        if (objOldParent != null)
                        {
                            objOldParent.Children.Remove(objModularCyberware);

                            CharacterObject.Cyberware.Add(objModularCyberware);
                        }
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdVehicleCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objModularCyberware))
                return;
            CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId, out VehicleMod objOldParentVehicleMod);
            if (objModularCyberware == null)
                return;
            frmSelectItem frmPickMount = new frmSelectItem
            {
                GeneralItems = CharacterObject.ConstructModularCyberlimbList(objModularCyberware),
                Description = LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language)
            };
            frmPickMount.ShowDialog();

            // Make sure the dialogue window was not canceled.
            if (frmPickMount.DialogResult == DialogResult.Cancel)
            {
                return;
            }

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

                CharacterObject.Cyberware.Add(objModularCyberware);
            }
            else
            {
                Cyberware objNewParent = CharacterObject.Cyberware.DeepFindById(strSelectedParentID);
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
                    VehicleMod objNewVehicleModParent = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedParentID);
                    if (objNewVehicleModParent == null)
                        objNewParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedParentID, out objNewVehicleModParent);
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

                        CharacterObject.Cyberware.Add(objModularCyberware);
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
        private void cboAttributeCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            CharacterObject.AttributeSection.AttributeCategory = AttributeSection.ConvertAttributeCategory(cboAttributeCategory.SelectedValue.ToString());
            CharacterObject.AttributeSection.ForceAttributePropertyChangedNotificationAll(nameof(CharacterAttrib.TotalAugmentedMaximum));
            CharacterObject.AttributeSection.ResetBindings();
            MakeDirtyWithCharacterUpdate(this, EventArgs.Empty);
        }

        private void cmdContactsExpansionToggle_Click(object sender, EventArgs e)
        {
            if (panContacts.Controls.Count > 0)
            {
                bool toggle = ((ContactControl)panContacts.Controls[0]).Expanded;

                foreach (ContactControl c in panContacts.Controls)
                {
                    c.Expanded = !toggle;
                }
            }
        }

        private void cmdSwapContactOrder_Click(object sender, EventArgs e)
        {
            panContacts.FlowDirection = panContacts.FlowDirection == FlowDirection.LeftToRight
                ? FlowDirection.TopDown
                : FlowDirection.LeftToRight;
        }

        private void tsGearLocationAddGear_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Location objLocation)) return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(null, objLocation);
            }
            while (blnAddAgain);
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

        private void tsWeaponLocationAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickWeapon(treWeapons.SelectedNode?.Tag is Location objLocation ? objLocation : null);
            }
            while (blnAddAgain);
        }

        private void tsVehicleLocationAddWeapon_Click(object sender, EventArgs e)
        {
            //TODO: Where should weapons attached to locations of vehicles go?
            //PickWeapon(treVehicles.SelectedNode);
        }

        private void cboVehicleWeaponFiringMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                objWeapon.FireMode = Weapon.ConvertToFiringMode(cboVehicleWeaponFiringMode.SelectedValue.ToString());

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
    }
}
