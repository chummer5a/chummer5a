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
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Chummer.Backend.Uniques;
using LiveCharts.Defaults;
using NLog;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class CharacterCareer : CharacterShared
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        // Set the default culture to en-US so we work with decimals correctly.
        private bool _blnSkipUpdate;

        private bool _blnReapplyImprovements;
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnDraggingGear;

        public TreeView FociTree => treFoci;

        private readonly ListViewColumnSorter _lvwKarmaColumnSorter;
        private readonly ListViewColumnSorter _lvwNuyenColumnSorter;

        public TabControl TabCharacterTabs => tabCharacterTabs;

        #region Form Events

        [Obsolete("This constructor is for use by form designers only.", true)]
        public CharacterCareer()
        {
            InitializeComponent();
        }

        public CharacterCareer(Character objCharacter) : base(objCharacter)
        {
            InitializeComponent();

            // Add EventHandlers for the MAG and RES enabled events and tab enabled events.
            CharacterObject.PropertyChanged += OnCharacterPropertyChanged;
            CharacterObjectSettings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            CharacterObject.AttributeSection.PropertyChanged += MakeDirtyWithCharacterUpdate;
            
            tabSkillsUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            lmtControl.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            lmtControl.MakeDirty += MakeDirty;

            Program.MainForm.OpenCharacterForms.Add(this);
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

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
                cmsWeaponMount
            };
            // Update the text in the Menus so they can be merged with frmMain properly.
            foreach (ToolStripMenuItem tssItem in mnuCreateMenu.Items.OfType<ToolStripMenuItem>())
            {
                tssItem.UpdateLightDarkMode();
                tssItem.TranslateToolStripItemsRecursively();
            }
            foreach (ContextMenuStrip objCMS in lstCMSToTranslate)
            {
                if (objCMS == null)
                    continue;
                foreach (ToolStripMenuItem tssItem in objCMS.Items.OfType<ToolStripMenuItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
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
                if (objTree.SelectedNode?.IsExpanded == false)
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.ExpandAll();
                    }
                }
                else if (objTree.SelectedNode?.Nodes != null)
                {
                    foreach (TreeNode objNode in objTree.SelectedNode.Nodes)
                    {
                        objNode.Collapse();
                    }
                }
            }
        }

        private void CharacterCareer_Load(object sender, EventArgs e)
        {
            using (CustomActivity op_load_frm_career = Timekeeper.StartSyncron("load_frm_career", null, CustomActivity.OperationType.RequestOperation, CharacterObject?.FileName))
            {
                if (CharacterObject == null)
                {
                    // Stupid hack to get the MDI icon to show up properly.
                    Icon = Icon.Clone() as Icon;
                    return;
                }
                try
                {
                    using (_ = Timekeeper.StartSyncron("load_frm_career_databinding", op_load_frm_career))
                    {
                        mnuSpecialAddBiowareSuite.Visible = CharacterObjectSettings.AllowBiowareSuites;

                        txtGroupName.DoDataBinding("Text", CharacterObject, nameof(Character.GroupName));
                        txtGroupNotes.DoDataBinding("Text", CharacterObject, nameof(Character.GroupNotes));

                        txtCharacterName.DoDataBinding("Text", CharacterObject, nameof(Character.Name));
                        txtGender.DoDataBinding("Text", CharacterObject, nameof(Character.Gender));
                        txtAge.DoDataBinding("Text", CharacterObject, nameof(Character.Age));
                        txtEyes.DoDataBinding("Text", CharacterObject, nameof(Character.Eyes));
                        txtHeight.DoDataBinding("Text", CharacterObject, nameof(Character.Height));
                        txtWeight.DoDataBinding("Text", CharacterObject, nameof(Character.Weight));
                        txtSkin.DoDataBinding("Text", CharacterObject, nameof(Character.Skin));
                        txtHair.DoDataBinding("Text", CharacterObject, nameof(Character.Hair));
                        rtfDescription.DoDataBinding("Rtf", CharacterObject, nameof(Character.Description));
                        rtfBackground.DoDataBinding("Rtf", CharacterObject, nameof(Character.Background));
                        rtfConcept.DoDataBinding("Rtf", CharacterObject, nameof(Character.Concept));
                        rtfNotes.DoDataBinding("Rtf", CharacterObject, nameof(Character.Notes));
                        rtfGameNotes.DoDataBinding("Rtf", CharacterObject, nameof(Character.GameNotes));
                        txtAlias.DoDataBinding("Text", CharacterObject, nameof(Character.Alias));
                        txtPlayerName.DoDataBinding("Text", CharacterObject, nameof(Character.PlayerName));

                        chkJoinGroup.Checked = CharacterObject.GroupMember;
                        chkInitiationGroup.DoOneWayDataBinding("Enabled", CharacterObject, nameof(Character.GroupMember));

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

                        lblNumMugshots.Text = LanguageManager.GetString("String_Of") +
                                              CharacterObject.Mugshots.Count.ToString(GlobalSettings.CultureInfo);

                        nudStreetCred.DoDataBinding("Value", CharacterObject, nameof(Character.StreetCred));
                        nudNotoriety.DoDataBinding("Value", CharacterObject, nameof(Character.Notoriety));
                        nudPublicAware.DoDataBinding("Value", CharacterObject, nameof(Character.PublicAwareness));
                        nudAstralReputation.DoDataBinding("Value", CharacterObject, nameof(Character.AstralReputation));
                        nudWildReputation.DoDataBinding("Value", CharacterObject, nameof(Character.WildReputation));
                        cmdAddMetamagic.DoOneWayDataBinding("Enabled", CharacterObject,
                            nameof(Character.AddInitiationsAllowed));
                        lblPossessed.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.Possessed));
                        lblMetatype.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.FormattedMetatype));

                        chkPsycheActiveMagician.DoDataBinding("Checked", CharacterObject, nameof(CharacterObject.PsycheActive));
                        chkPsycheActiveTechnomancer.DoDataBinding("Checked", CharacterObject, nameof(CharacterObject.PsycheActive));
                    }

                    if (!CharacterObjectSettings.BookEnabled("RF"))
                    {
                        cmdAddLifestyle.SplitMenuStrip = null;
                    }
                    if (!CharacterObjectSettings.BookEnabled("FA"))
                    {
                        lblWildReputation.Visible = false;
                        nudWildReputation.Visible = false;
                        lblWildReputationTotal.Visible = false;
                        if (!CharacterObjectSettings.BookEnabled("SG"))
                        {
                            lblAstralReputation.Visible = false;
                            nudAstralReputation.Visible = false;
                            lblAstralReputationTotal.Visible = false;
                        }
                    }
                    if (!CharacterObjectSettings.EnableEnemyTracking)
                    {
                        tabPeople.TabPages.Remove(tabEnemies);
                    }

                    splitKarmaNuyen.SplitterDistance = Math.Max(splitKarmaNuyen.SplitterDistance, (splitKarmaNuyen.Width - splitKarmaNuyen.SplitterWidth + 1) / 2);
                    splitMagician.SplitterDistance = Math.Max(splitMagician.SplitterDistance, ((splitMagician.Height - splitMagician.SplitterWidth) * 2 + 2) / 3);
                    splitTechnomancer.SplitterDistance = Math.Max(splitTechnomancer.SplitterDistance, ((splitTechnomancer.Height - splitTechnomancer.SplitterWidth) * 2 + 2) / 3);

                    using (_ = Timekeeper.StartSyncron("load_frm_career_refresh", op_load_frm_career))
                    {
                        RefreshQualities(treQualities, cmsQuality);
                        RefreshSpirits(panSpirits, panSprites);
                        RefreshSpells(treSpells, treMetamagic, cmsSpell, cmsInitiationNotes);
                        RefreshSustainedSpells(flpSustainedSpells, flpSustainedComplexForms, flpSustainedCritterPowers, chkPsycheActiveMagician, chkPsycheActiveTechnomancer);
                        RefreshComplexForms(treComplexForms, treMetamagic, cmsComplexForm, cmsInitiationNotes);
                        RefreshPowerCollectionListChanged(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                        RefreshAIPrograms(treAIPrograms, cmsAdvancedProgram);
                        RefreshCritterPowers(treCritterPowers, cmsCritterPowers);
                        mnuSpecialPossess.Visible =
                            CharacterObject.CritterPowers.Any(x => x.Name == "Inhabitation" || x.Name == "Possession");
                        RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique);
                        RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle);
                        RefreshCustomImprovements(treImprovements, lmtControl.LimitTreeView, cmsImprovementLocation,
                            cmsImprovement, lmtControl.LimitContextMenuStrip);
                        RefreshCalendar(lstCalendar);
                        RefreshContacts(panContacts, panEnemies, panPets);

                        RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear);
                        RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
                        RefreshFociFromGear(treFoci, null);
                        RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear);
                        RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory,
                            cmsWeaponAccessoryGear);
                        RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon,
                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsWeaponMount,
                            cmsVehicleCyberware, cmsVehicleCyberwareGear);
                        RefreshDrugs(treCustomDrugs);

                        ExpenseEntriesCollectionChanged(this, null);
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_career_sortAndCallbacks", op_load_frm_career))
                    {
                        treWeapons.SortCustomOrder();
                        treArmor.SortCustomOrder();
                        treGear.SortCustomOrder();
                        treLifestyles.SortCustomOrder();
                        treCustomDrugs.SortCustomOrder();
                        treCyberware.SortCustomOrder();
                        treVehicles.SortCustomOrder();
                        treCritterPowers.SortCustomOrder();
                        treImprovements.SortCustomOrder();

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
                        CharacterObject.Lifestyles.CollectionChanged += LifestylesCollectionChanged;
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
                        CharacterObject.Drugs.CollectionChanged += DrugCollectionChanged;
                        CharacterObject.SustainedCollection.CollectionChanged += SustainedSpellCollectionChanged;
                        CharacterObject.ExpenseEntries.CollectionChanged += ExpenseEntriesCollectionChanged;

                        SetupCommonCollectionDatabindings(true);
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_career_magictradition", op_load_frm_career))
                    {
                        // Populate the Magician Traditions list.
                        XPathNavigator xmlTraditionsBaseChummerNode =
                            CharacterObject.LoadDataXPath("traditions.xml").SelectSingleNodeAndCacheExpression("/chummer");
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstTraditions))
                        {
                            if (xmlTraditionsBaseChummerNode != null)
                            {
                                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                             "traditions/tradition[" + CharacterObjectSettings.BookXPath() + ']'))
                                {
                                    string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")?.Value;
                                    if (!string.IsNullOrEmpty(strName))
                                        lstTraditions.Add(new ListItem(
                                                              xmlTradition.SelectSingleNodeAndCacheExpression("id")
                                                                          ?.Value ?? strName,
                                                              xmlTradition
                                                                  .SelectSingleNodeAndCacheExpression("translate")
                                                                  ?.Value ?? strName));
                                }
                            }

                            if (lstTraditions.Count > 1)
                            {
                                lstTraditions.Sort(CompareListItems.CompareNames);
                                lstTraditions.Insert(0,
                                                     new ListItem("None", LanguageManager.GetString("String_None")));
                                cboTradition.BeginUpdate();
                                cboTradition.PopulateWithListItems(lstTraditions);
                                cboTradition.EndUpdate();
                            }
                            else
                            {
                                cboTradition.Visible = false;
                                lblTraditionLabel.Visible = false;
                            }
                        }

                        // Populate the Magician Custom Drain Options list.
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstDrainAttributes))
                        {
                            lstDrainAttributes.Add(ListItem.Blank);
                            if (xmlTraditionsBaseChummerNode != null)
                            {
                                foreach (XPathNavigator xmlDrain in xmlTraditionsBaseChummerNode
                                             .SelectAndCacheExpression(
                                                 "drainattributes/drainattribute"))
                                {
                                    string strName = xmlDrain.SelectSingleNodeAndCacheExpression("name")?.Value;
                                    if (!string.IsNullOrEmpty(strName))
                                        lstDrainAttributes.Add(new ListItem(strName,
                                                                            xmlDrain.SelectSingleNodeAndCacheExpression(
                                                                                "translate")?.Value ?? strName));
                                }
                            }

                            lstDrainAttributes.Sort(CompareListItems.CompareNames);
                            cboDrain.BeginUpdate();
                            cboDrain.PopulateWithListItems(lstDrainAttributes);
                            cboDrain.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                   nameof(Tradition.DrainExpression));
                            cboDrain.EndUpdate();
                        }

                        lblDrainAttributes.DoOneWayDataBinding("Text", CharacterObject.MagicTradition,
                                                               nameof(Tradition.DisplayDrainExpression));
                        dpcDrainAttributes.DoOneWayDataBinding("DicePool", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValue));
                        dpcDrainAttributes.DoOneWayDataBinding("ToolTipText", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValueToolTip));
                        CharacterObject.MagicTradition.SetSourceDetail(lblTraditionSource);

                        lblFadingAttributes.DoOneWayDataBinding("Text", CharacterObject.MagicTradition,
                            nameof(Tradition.DisplayDrainExpression));
                        dpcFadingAttributes.DoOneWayDataBinding("DicePool", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValue));
                        dpcFadingAttributes.DoOneWayDataBinding("ToolTipText", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValueToolTip));

                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> limit))
                        {
                            foreach (Improvement improvement in ImprovementManager.GetCachedImprovementListForValueOf(
                                         CharacterObject, Improvement.ImprovementType.LimitSpiritCategory))
                            {
                                limit.Add(improvement.ImprovedName);
                            }

                            // Populate the Magician Custom Spirits lists - Combat.
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstSpirit))
                            {
                                lstSpirit.Add(ListItem.Blank);
                                if (xmlTraditionsBaseChummerNode != null)
                                {
                                    foreach (XPathNavigator xmlSpirit in xmlTraditionsBaseChummerNode
                                                 .SelectAndCacheExpression("spirits/spirit"))
                                    {
                                        string strSpiritName
                                            = xmlSpirit.SelectSingleNodeAndCacheExpression("name")?.Value;
                                        if (!string.IsNullOrEmpty(strSpiritName)
                                            && (limit.Count == 0 || limit.Contains(strSpiritName)))
                                        {
                                            lstSpirit.Add(new ListItem(strSpiritName,
                                                                       xmlSpirit.SelectSingleNodeAndCacheExpression(
                                                                           "translate")?.Value ?? strSpiritName));
                                        }
                                    }
                                }

                                lstSpirit.Sort(CompareListItems.CompareNames);

                                cboSpiritCombat.BeginUpdate();
                                cboSpiritCombat.PopulateWithListItems(lstSpirit);
                                cboSpiritCombat.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                              nameof(Tradition.SpiritCombat));
                                lblSpiritCombat.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritCombat.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritCombat.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
                                cboSpiritCombat.EndUpdate();

                                cboSpiritDetection.BeginUpdate();
                                cboSpiritDetection.PopulateWithListItems(lstSpirit);
                                cboSpiritDetection.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                                 nameof(Tradition.SpiritDetection));
                                lblSpiritDetection.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritDetection.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritDetection.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
                                cboSpiritDetection.EndUpdate();

                                cboSpiritHealth.BeginUpdate();
                                cboSpiritHealth.PopulateWithListItems(lstSpirit);
                                cboSpiritHealth.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                              nameof(Tradition.SpiritHealth));
                                lblSpiritHealth.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritHealth.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritHealth.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
                                cboSpiritHealth.EndUpdate();

                                cboSpiritIllusion.BeginUpdate();
                                cboSpiritIllusion.PopulateWithListItems(lstSpirit);
                                cboSpiritIllusion.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                                nameof(Tradition.SpiritIllusion));
                                lblSpiritIllusion.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritIllusion.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritIllusion.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
                                cboSpiritIllusion.EndUpdate();

                                cboSpiritManipulation.BeginUpdate();
                                cboSpiritManipulation.PopulateWithListItems(lstSpirit);
                                cboSpiritManipulation.DoDataBinding("SelectedValue", CharacterObject.MagicTradition,
                                                                    nameof(Tradition.SpiritManipulation));
                                lblSpiritManipulation.Visible
                                    = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritManipulation.Visible
                                    = CharacterObject.MagicTradition.Type != TraditionType.None;
                                cboSpiritManipulation.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
                                cboSpiritManipulation.EndUpdate();
                            }
                        }

                        // Populate the Technomancer Streams list.
                        xmlTraditionsBaseChummerNode =
                            CharacterObject.LoadDataXPath("streams.xml").SelectSingleNodeAndCacheExpression("/chummer");
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstStreams))
                        {
                            if (xmlTraditionsBaseChummerNode != null)
                            {
                                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                             "traditions/tradition[" + CharacterObjectSettings.BookXPath() + ']'))
                                {
                                    string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")?.Value;
                                    if (!string.IsNullOrEmpty(strName))
                                        lstStreams.Add(new ListItem(
                                                           xmlTradition.SelectSingleNodeAndCacheExpression("id")?.Value
                                                           ?? strName,
                                                           xmlTradition.SelectSingleNodeAndCacheExpression("translate")
                                                                       ?.Value ?? strName));
                                }
                            }

                            if (lstStreams.Count > 1)
                            {
                                lstStreams.Sort(CompareListItems.CompareNames);
                                lstStreams.Insert(0,
                                                  new ListItem("None", LanguageManager.GetString("String_None")));
                                cboStream.BeginUpdate();
                                cboStream.PopulateWithListItems(lstStreams);
                                cboStream.EndUpdate();
                            }
                            else
                            {
                                cboStream.Visible = false;
                                lblStreamLabel.Visible = false;
                            }
                        }
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_career_shapeshifter", op_load_frm_career))
                    {
                        cboAttributeCategory.Visible = CharacterObject.MetatypeCategory == "Shapeshifter";
                        if (CharacterObject.MetatypeCategory == "Shapeshifter")
                        {
                            XPathNavigator objDoc = CharacterObject.LoadDataXPath("metatypes.xml");
                            XPathNavigator node =
                                objDoc.SelectSingleNode(
                                    "/chummer/metatypes/metatype[name = " + CharacterObject.Metatype.CleanXPath() + ']');
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstAttributeCategories))
                            {
                                lstAttributeCategories.Add(new ListItem("Standard",
                                                                        node?.SelectSingleNodeAndCacheExpression(
                                                                            "name/@translate")?.Value
                                                                        ?? CharacterObject.Metatype));

                                node = node?.SelectSingleNode(
                                    "metavariants/metavariant[name = " + CharacterObject.Metavariant.CleanXPath()
                                                                       + "]/name/@translate");

                                //The Shapeshifter attribute category is treated as the METAHUMAN form of a shapeshifter.
                                lstAttributeCategories.Add(new ListItem("Shapeshifter",
                                                                        node?.Value ?? CharacterObject.Metavariant));

                                lstAttributeCategories.Sort(CompareListItems.CompareNames);
                                cboAttributeCategory.BeginUpdate();
                                cboAttributeCategory.PopulateWithListItems(lstAttributeCategories);
                                cboAttributeCategory.EndUpdate();
                                cboAttributeCategory.SelectedValue = "Standard";
                            }
                        }

                        lblMysticAdeptMAGAdept.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.MysticAdeptPowerPoints));
                        cmdIncreasePowerPoints.DoOneWayDataBinding("Enabled", CharacterObject,
                            nameof(Character.CanAffordCareerPP));
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_career_vehicle", op_load_frm_career))
                    {
                        // Populate vehicle weapon fire mode list.
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstFireModes))
                        {
                            foreach (Weapon.FiringMode mode in Enum.GetValues(typeof(Weapon.FiringMode)))
                            {
                                if (mode == Weapon.FiringMode.NumFiringModes)
                                    continue;
                                lstFireModes.Add(new ListItem(mode,
                                                              LanguageManager.GetString("Enum_" + mode)));
                            }

                            cboVehicleWeaponFiringMode.BeginUpdate();
                            cboVehicleWeaponFiringMode.PopulateWithListItems(lstFireModes);
                            cboVehicleWeaponFiringMode.EndUpdate();
                        }
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_career_selectStuff", op_load_frm_career))
                    {
                        // Select the Magician's Tradition.
                        if (CharacterObject.MagicTradition.Type == TraditionType.MAG)
                            cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceIDString;
                        else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                            cboTradition.SelectedIndex = 0;

                        txtTraditionName.DoDataBinding("Text", CharacterObject.MagicTradition, nameof(Tradition.Name));

                        // Select the Technomancer's Stream.
                        if (CharacterObject.MagicTradition.Type == TraditionType.RES)
                            cboStream.SelectedValue = CharacterObject.MagicTradition.SourceIDString;
                        else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                            cboStream.SelectedIndex = 0;
                    }

                    IsLoading = false;

                    using (CustomActivity op_load_frm_career_databindingCallbacks2 =
                        Timekeeper.StartSyncron("load_frm_career_databindingCallbacks2", op_load_frm_career))
                    {
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
                        ToolStripManager.Merge(tsMain, "toolStrip");

                        using (_ = Timekeeper.StartSyncron("load_frm_career_tabSkillsUc.RealLoad()", op_load_frm_career_databindingCallbacks2))
                        {
                            tabSkillsUc.RealLoad();
                        }

                        using (_ = Timekeeper.StartSyncron("load_frm_career_tabPowerUc.RealLoad()", op_load_frm_career_databindingCallbacks2))
                        {
                            tabPowerUc.RealLoad();
                        }

                        using (_ = Timekeeper.StartSyncron("load_frm_career_Run through all appropriate property changers", op_load_frm_career_databindingCallbacks2))
                        {
                            // Run through all appropriate property changers
                            foreach (PropertyInfo objProperty in typeof(Character).GetProperties())
                            {
                                OnCharacterPropertyChanged(CharacterObject,
                                                           new PropertyChangedEventArgs(objProperty.Name));
                            }
                        }

                        lblCMPenalty.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.WoundModifier));
                        lblCMPhysical.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.PhysicalCMToolTip));
                        lblCMPhysical.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.PhysicalCM));
                        lblCMPhysicalLabel.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.PhysicalCMLabelText));
                        lblCMStun.Visible = true; // Needed to make sure data bindings go through
                        lblCMStun.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.StunCMToolTip));
                        lblCMStun.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.StunCM));
                        lblCMStun.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.StunCMVisible));
                        lblCMStunLabel.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.StunCMLabelText));

                        lblESSMax.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayEssence));
                        lblCyberwareESS.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplayCyberwareEssence));
                        lblBiowareESS.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayBiowareEssence));
                        lblEssenceHoleESS.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayEssenceHole));

                        lblArmor.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.TotalArmorRating));
                        lblArmor.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.TotalArmorRatingToolTip));
                        lblCMArmor.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.TotalArmorRating));
                        lblCMArmor.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.TotalArmorRatingToolTip));

                        lblDodge.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayDodge));
                        lblDodge.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.DodgeToolTip));

                        lblCMDodge.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayDodge));
                        lblCMDodge.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.DodgeToolTip));

                        lblSpellDefenseIndirectDodge.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseIndirectDodge));
                        lblSpellDefenseIndirectDodge.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseIndirectDodgeToolTip));
                        lblSpellDefenseIndirectSoak.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseIndirectSoak));
                        lblSpellDefenseIndirectSoak.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseIndirectSoakToolTip));
                        lblSpellDefenseDirectSoakMana.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDirectSoakMana));
                        lblSpellDefenseDirectSoakMana.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDirectSoakManaToolTip));
                        lblSpellDefenseDirectSoakPhysical.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDirectSoakPhysical));
                        lblSpellDefenseDirectSoakPhysical.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDirectSoakPhysicalToolTip));
                        lblSpellDefenseDetection.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDetection));
                        lblSpellDefenseDetection.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDetectionToolTip));
                        lblSpellDefenseDecAttBOD.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseBOD));
                        lblSpellDefenseDecAttBOD.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseBODToolTip));
                        lblSpellDefenseDecAttAGI.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseAGI));
                        lblSpellDefenseDecAttAGI.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseAGIToolTip));
                        lblSpellDefenseDecAttREA.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseREA));
                        lblSpellDefenseDecAttREA.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseREAToolTip));
                        lblSpellDefenseDecAttSTR.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseSTR));
                        lblSpellDefenseDecAttSTR.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseSTRToolTip));
                        lblSpellDefenseDecAttCHA.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseCHA));
                        lblSpellDefenseDecAttCHA.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseCHAToolTip));
                        lblSpellDefenseDecAttINT.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseINT));
                        lblSpellDefenseDecAttINT.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseINTToolTip));
                        lblSpellDefenseDecAttLOG.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseLOG));
                        lblSpellDefenseDecAttLOG.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseLOGToolTip));
                        lblSpellDefenseDecAttWIL.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseDecreaseWIL));
                        lblSpellDefenseDecAttWIL.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseDecreaseWILToolTip));
                        lblSpellDefenseIllusionMana.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseIllusionMana));
                        lblSpellDefenseIllusionMana.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseIllusionManaToolTip));
                        lblSpellDefenseIllusionPhysical.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseIllusionPhysical));
                        lblSpellDefenseIllusionPhysical.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseIllusionPhysicalToolTip));
                        lblSpellDefenseManipMental.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseManipulationMental));
                        lblSpellDefenseManipMental.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseManipulationMentalToolTip));
                        lblSpellDefenseManipPhysical.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplaySpellDefenseManipulationPhysical));
                        lblSpellDefenseManipPhysical.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.SpellDefenseManipulationPhysicalToolTip));
                        nudCounterspellingDice.DoDataBinding("Value", CharacterObject,
                            nameof(Character.CurrentCounterspellingDice));

                        nudLiftCarryHits.DoDataBinding("Value", CharacterObject,
                                                       nameof(Character.CurrentLiftCarryHits));

                        lblMovement.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayMovement));
                        lblSwim.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplaySwim));
                        lblFly.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayFly));

                        lblRemainingNuyen.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayNuyen));
                        lblCareerKarma.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayCareerKarma));
                        lblCareerNuyen.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayCareerNuyen));

                        lblStreetCredTotal.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.TotalStreetCred));
                        lblStreetCredTotal.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.StreetCredTooltip));
                        lblNotorietyTotal.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.TotalNotoriety));
                        lblNotorietyTotal.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.NotorietyTooltip));
                        lblPublicAwareTotal.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.TotalPublicAwareness));
                        lblPublicAwareTotal.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.PublicAwarenessTooltip));
                        lblAstralReputationTotal.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.TotalAstralReputation));
                        lblAstralReputationTotal.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.AstralReputationTooltip));
                        lblWildReputationTotal.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.TotalWildReputation));
                        lblWildReputationTotal.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.WildReputationTooltip));

                        lblMentorSpirit.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.FirstMentorSpiritDisplayName));
                        lblMentorSpiritInformation.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.FirstMentorSpiritDisplayInformation));
                        lblParagon.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.FirstMentorSpiritDisplayName));
                        lblParagonInformation.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.FirstMentorSpiritDisplayInformation));

                        lblSurprise.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.SurpriseToolTip));
                        lblSurprise.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Surprise));
                        lblComposure.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.ComposureToolTip));
                        lblComposure.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Composure));
                        lblJudgeIntentions.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.JudgeIntentionsToolTip));
                        lblJudgeIntentions.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.JudgeIntentions));
                        lblLiftCarry.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.LiftAndCarryToolTip));
                        lblLiftCarry.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.LiftAndCarry));
                        lblMemory.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.MemoryToolTip));
                        lblMemory.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Memory));

                        lblLiftCarryLimits.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.LiftAndCarryLimits));

                        lblINI.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.InitiativeToolTip));
                        lblINI.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Initiative));
                        lblAstralINI.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.AstralInitiativeToolTip));
                        lblAstralINI.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.AstralInitiative));
                        lblMatrixINI.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.MatrixInitiativeToolTip));
                        lblMatrixINI.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.MatrixInitiative));
                        lblMatrixINICold.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.MatrixInitiativeColdToolTip));
                        lblMatrixINICold.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.MatrixInitiativeCold));
                        lblMatrixINIHot.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.MatrixInitiativeHotToolTip));
                        lblMatrixINIHot.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.MatrixInitiativeHot));
                        lblRiggingINI.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.InitiativeToolTip));
                        lblRiggingINI.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Initiative));
                        
                        cmdBurnStreetCred.DoOneWayDataBinding("Enabled", CharacterObject,
                            nameof(Character.CanBurnStreetCred));

                        lblEDGInfo.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.EdgeRemainingString));
                        lblCMDamageResistancePool.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.DamageResistancePoolToolTip));
                        lblCMDamageResistancePool.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DamageResistancePool));
                        lblCMPhysicalRecoveryPool.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.PhysicalCMNaturalRecovery));
                        lblCMStunRecoveryPool.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.StunCMNaturalRecovery));
                    }

                    using (CustomActivity op_load_frm_career_finishingStuff =
                        Timekeeper.StartSyncron("load_frm_career_finishingStuff", op_load_frm_career))
                    {
                        RefreshAttributes(pnlAttributes, null, lblAttributes, -1, lblAttributesAug.PreferredWidth, lblAttributesMetatype.PreferredWidth);

                        CharacterObject.AttributeSection.Attributes.CollectionChanged += AttributeCollectionChanged;

                        // Condition Monitor.
                        ProcessCharacterConditionMonitorBoxDisplays(panPhysicalCM, CharacterObject.PhysicalCM,
                            CharacterObject.CMThreshold, CharacterObject.PhysicalCMThresholdOffset,
                            CharacterObject.CMOverflow,
                            chkPhysicalCM_CheckedChanged, true, CharacterObject.PhysicalCMFilled);
                        ProcessCharacterConditionMonitorBoxDisplays(panStunCM, CharacterObject.StunCM,
                            CharacterObject.CMThreshold, CharacterObject.StunCMThresholdOffset, 0,
                            chkStunCM_CheckedChanged,
                            true, CharacterObject.StunCMFilled);

                        IsCharacterUpdateRequested = true;
                        // Directly calling here so that we can properly unset the dirty flag after the update
                        UpdateCharacterInfo();

                        // Now we can start checking for character updates
                        Application.Idle += UpdateCharacterInfo;
                        Application.Idle += LiveUpdateFromCharacterFile;

                        // Clear the Dirty flag which gets set when creating a new Character.
                        IsDirty = false;
                        RefreshPasteStatus();
                        picMugshot_SizeChanged(sender, e);
                        // Stupid hack to get the MDI icon to show up properly.
                        Icon = Icon.Clone() as Icon;

                        Program.PluginLoader.CallPlugins(this, op_load_frm_career_finishingStuff);
                    }

                    if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0 && !Utils.IsUnitTest && Program.MainForm.ShowMessageBox(this,
                        LanguageManager.GetString("Message_ImprovementLoadError"),
                        LanguageManager.GetString("MessageTitle_ImprovementLoadError"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                        CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
                    }

                    IsDirty = CharacterObject.LoadAsDirty;

                    op_load_frm_career.SetSuccess(true);
                }
                catch (Exception ex)
                {
                    if (op_load_frm_career != null)
                    {
                        op_load_frm_career.SetSuccess(false);
                        op_load_frm_career.MyTelemetryClient.TrackException(ex);
                    }

                    Log.Error(ex);
                    throw;
                }
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

        private void LifestylesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle, e);
        }

        private void ImprovementCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshCustomImprovements(treImprovements, lmtControl.LimitTreeView, cmsImprovementLocation, cmsImprovement, lmtControl.LimitContextMenuStrip, notifyCollectionChangedEventArgs);
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

        private void SustainedSpellCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshSustainedSpells(flpSustainedSpells, flpSustainedComplexForms, flpSustainedCritterPowers, chkPsycheActiveMagician, chkPsycheActiveTechnomancer, notifyCollectionChangedEventArgs);
        }

        private void AttributeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshAttributes(pnlAttributes, notifyCollectionChangedEventArgs, lblAttributes, -1, lblAttributesAug.PreferredWidth, lblAttributesMetatype.PreferredWidth);
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

        private void DrugCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshDrugs(treCustomDrugs, notifyCollectionChangedEventArgs);
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

        private async void CharacterCareer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If there are unsaved changes to the character, as the user if they would like to save their changes.
            if (IsDirty)
            {
                string strCharacterName = CharacterObject.CharacterName;
                DialogResult objResult = Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_UnsavedChanges"), strCharacterName),
                    await LanguageManager.GetStringAsync("MessageTitle_UnsavedChanges"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (objResult)
                {
                    case DialogResult.Yes:
                        {
                            // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                            bool blnResult = await SaveCharacter();
                            if (!blnResult)
                                e.Cancel = true;
                            break;
                        }
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            // Reset the ToolStrip so the Save button is removed for the currently closing window.
            if (e.Cancel)
                return;
            IsLoading = true;
            using (new CursorWait(this))
            {
                Application.Idle -= UpdateCharacterInfo;
                Application.Idle -= LiveUpdateFromCharacterFile;
                if (Program.MainForm.ActiveMdiChild == this)
                    ToolStripManager.RevertMerge("toolStrip");
                Program.MainForm.OpenCharacterForms.Remove(this);

                // Unsubscribe from events.
                CharacterObject.AttributeSection.Attributes.CollectionChanged -= AttributeCollectionChanged;
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
                CharacterObject.Lifestyles.CollectionChanged -= LifestylesCollectionChanged;
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
                CharacterObject.Drugs.CollectionChanged -= DrugCollectionChanged;
                CharacterObject.SustainedCollection.CollectionChanged -= SustainedSpellCollectionChanged;
                CharacterObject.ExpenseEntries.CollectionChanged -= ExpenseEntriesCollectionChanged;
                CharacterObject.AttributeSection.PropertyChanged -= MakeDirtyWithCharacterUpdate;
                CharacterObject.PropertyChanged -= OnCharacterPropertyChanged;
                CharacterObjectSettings.PropertyChanged -= OnCharacterSettingsPropertyChanged;

                SetupCommonCollectionDatabindings(false);

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
                    objContactControl.MouseDown -= DragContactControl;
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

                foreach (SustainedObjectControl objSustainedSpellControl in flpSustainedSpells.Controls.OfType<SustainedObjectControl>())
                {
                    objSustainedSpellControl.SustainedObjectDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objSustainedSpellControl.UnsustainObject -= DeleteSustainedObject;
                }

                // Trash the global variables and dispose of the Form.
                if (Program.MainForm.OpenCharacters.All(x => x == CharacterObject || !x.LinkedCharacters.Contains(CharacterObject)))
                    Program.MainForm.OpenCharacters.Remove(CharacterObject);
            }
        }

        private void CharacterCareer_Activated(object sender, EventArgs e)
        {
            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(tsMain, "toolStrip");
        }

        #endregion Form Events

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
                    tslNuyen.Text = CharacterObject.DisplayNuyen;
                    break;

                case nameof(Character.DisplayKarma):
                    tslKarma.Text = CharacterObject.DisplayKarma;
                    break;

                case nameof(Character.DisplayEssence):
                    tslEssence.Text = CharacterObject.DisplayEssence;
                    break;

                case nameof(Character.DisplayTotalCarriedWeight):
                    tslCarriedWeight.Text = CharacterObject.DisplayTotalCarriedWeight;
                    break;

                case nameof(Character.Encumbrance):
                    tslCarriedWeight.ForeColor = CharacterObject.Encumbrance > 0
                        ? ColorManager.ErrorColor
                        : ColorManager.ControlText;
                    break;

                case nameof(Character.NuyenBP):
                case nameof(Character.MetatypeBP):
                case nameof(Character.ContactPoints):
                case nameof(Character.FreeSpells):
                case nameof(Character.CFPLimit):
                case nameof(Character.AIAdvancedProgramLimit):
                case nameof(Character.SpellKarmaCost):
                case nameof(Character.ComplexFormKarmaCost):
                case nameof(Character.AIProgramKarmaCost):
                case nameof(Character.AIAdvancedProgramKarmaCost):
                case nameof(Character.MysticAdeptPowerPoints):
                case nameof(Character.MagicTradition):
                case nameof(Character.HomeNode):
                case nameof(Character.ActiveCommlink):
                case nameof(Character.Nuyen):
                case nameof(Character.Karma):
                    IsCharacterUpdateRequested = true;
                    break;

                case nameof(Character.Source):
                case nameof(Character.Page):
                    CharacterObject.SetSourceDetail(lblMetatypeSource);
                    break;

                case nameof(Character.CMOverflow):
                case nameof(Character.CMThreshold):
                case nameof(Character.CMThresholdOffsets):
                    ProcessCharacterConditionMonitorBoxDisplays(panPhysicalCM, CharacterObject.PhysicalCM,
                        CharacterObject.CMThreshold, CharacterObject.PhysicalCMThresholdOffset,
                        CharacterObject.CMOverflow, chkPhysicalCM_CheckedChanged, true,
                        CharacterObject.PhysicalCMFilled);
                    ProcessCharacterConditionMonitorBoxDisplays(panStunCM, CharacterObject.StunCM,
                        CharacterObject.CMThreshold, CharacterObject.StunCMThresholdOffset, 0, chkStunCM_CheckedChanged,
                        true, CharacterObject.StunCMFilled);
                    break;

                case nameof(Character.StunCM):
                case nameof(Character.StunCMFilled):
                case nameof(Character.StunCMThresholdOffset):
                    ProcessCharacterConditionMonitorBoxDisplays(panStunCM, CharacterObject.StunCM,
                        CharacterObject.CMThreshold, CharacterObject.StunCMThresholdOffset, 0, chkStunCM_CheckedChanged,
                        true, CharacterObject.StunCMFilled);
                    break;

                case nameof(Character.PhysicalCM):
                case nameof(Character.PhysicalCMFilled):
                case nameof(Character.PhysicalCMThresholdOffset):
                    ProcessCharacterConditionMonitorBoxDisplays(panPhysicalCM, CharacterObject.PhysicalCM,
                        CharacterObject.CMThreshold, CharacterObject.PhysicalCMThresholdOffset,
                        CharacterObject.CMOverflow, chkPhysicalCM_CheckedChanged, true,
                        CharacterObject.PhysicalCMFilled);
                    break;

                case nameof(Character.MAGEnabled):
                    {
                        if (CharacterObject.MAGEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);
                            tabInitiation.Text = LanguageManager.GetString("Tab_Initiation");
                            tsMetamagicAddMetamagic.Text =
                                LanguageManager.GetString("Button_AddMetamagic");
                            cmdAddMetamagic.Text =
                                LanguageManager.GetString("Button_AddInitiateGrade");
                            cmdDeleteMetamagic.Text =
                                LanguageManager.GetString("Button_RemoveInitiateGrade");
                            gpbInitiationType.Text =
                                LanguageManager.GetString("String_InitiationType");
                            gpbInitiationGroup.Text =
                                LanguageManager.GetString("String_InitiationGroup");
                            chkInitiationSchooling.Enabled = true;
                            tsMetamagicAddArt.Visible = true;
                            tsMetamagicAddEnchantment.Visible = true;
                            tsMetamagicAddEnhancement.Visible = true;
                            tsMetamagicAddRitual.Visible = true;
                            string strInitTip = string.Format(GlobalSettings.CultureInfo,
                                                              LanguageManager.GetString("Tip_ImproveInitiateGrade"),
                                                              (CharacterObject.InitiateGrade + 1).ToString(
                                                                  GlobalSettings.CultureInfo),
                                                              (CharacterObjectSettings.KarmaInitiationFlat
                                                               + (CharacterObject.InitiateGrade + 1)
                                                               * CharacterObjectSettings.KarmaInitiation)
                                                              .ToString(GlobalSettings.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedGroup");

                            chkInitiationOrdeal.Text = LanguageManager.GetString("Checkbox_InitiationOrdeal")
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationOrdealPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_InitiationGroup")
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationGroupPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationSchooling.Text = LanguageManager.GetString("Checkbox_InitiationSchooling")
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationSchoolingPercent.ToString("P", GlobalSettings.CultureInfo));
                            if (!CharacterObject.AttributeSection.Attributes.Contains(CharacterObject.MAG))
                            {
                                CharacterObject.AttributeSection.Attributes.Add(CharacterObject.MAG);
                            }
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                            {
                                CharacterAttrib objMAGAdept =
                                    CharacterObject.AttributeSection.GetAttributeByName("MAGAdept");
                                if (!CharacterObject.AttributeSection.Attributes.Contains(objMAGAdept))
                                {
                                    CharacterObject.AttributeSection.Attributes.Add(objMAGAdept);
                                }
                            }
                        }
                        else
                        {
                            if (!CharacterObject.RESEnabled)
                                tabCharacterTabs.TabPages.Remove(tabInitiation);

                            if (CharacterObject.AttributeSection.Attributes != null)
                            {
                                CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.MAG);
                                CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.MAGAdept);
                            }
                        }

                        gpbGearBondedFoci.Visible = CharacterObject.MAGEnabled;
                        lblAstralINI.Visible = CharacterObject.MAGEnabled;
                    }
                    break;

                case nameof(Character.RESEnabled):
                    {
                        if (CharacterObject.RESEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);
                            tabInitiation.Text = LanguageManager.GetString("Tab_Submersion");
                            tsMetamagicAddMetamagic.Text =
                                LanguageManager.GetString("Button_AddEcho");
                            cmdAddMetamagic.Text =
                                LanguageManager.GetString("Button_AddSubmersionGrade");
                            cmdDeleteMetamagic.Text =
                                LanguageManager.GetString("Button_RemoveSubmersionGrade");
                            gpbInitiationType.Text =
                                LanguageManager.GetString("String_SubmersionType");
                            gpbInitiationGroup.Text =
                                LanguageManager.GetString("String_SubmersionNetwork");
                            chkInitiationSchooling.Enabled = CharacterObjectSettings.AllowTechnomancerSchooling;
                            tsMetamagicAddArt.Visible = false;
                            tsMetamagicAddEnchantment.Visible = false;
                            tsMetamagicAddEnhancement.Visible = false;
                            tsMetamagicAddRitual.Visible = false;
                            string strInitTip = string.Format(GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Tip_ImproveSubmersionGrade"),
                                (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo),
                                (CharacterObjectSettings.KarmaInitiationFlat +
                                 (CharacterObject.SubmersionGrade + 1) * CharacterObjectSettings.KarmaInitiation)
                                .ToString(GlobalSettings.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedNetwork");
                            chkInitiationOrdeal.Text = LanguageManager.GetString("Checkbox_SubmersionTask")
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationOrdealPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_NetworkSubmersion")
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationGroupPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationSchooling.Text = LanguageManager.GetString("Checkbox_InitiationSchooling")
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationSchoolingPercent.ToString("P", GlobalSettings.CultureInfo));
                            if (!CharacterObject.AttributeSection.Attributes.Contains(CharacterObject.RES))
                            {
                                CharacterObject.AttributeSection.Attributes.Add(CharacterObject.RES);
                            }
                        }
                        else
                        {
                            if (!CharacterObject.MAGEnabled) tabCharacterTabs.TabPages.Remove(tabInitiation);
                            if (CharacterObject.AttributeSection.Attributes.Contains(CharacterObject.RES))
                            {
                                CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.RES);
                            }
                        }
                    }
                    break;

                case nameof(Character.DEPEnabled):
                    {
                        if (CharacterObject.DEPEnabled)
                        {
                            if (!CharacterObject.AttributeSection.Attributes.Contains(CharacterObject.DEP))
                            {
                                CharacterObject.AttributeSection.Attributes.Add(CharacterObject.DEP);
                            }
                        }
                        else if (CharacterObject.AttributeSection.Attributes.Contains(CharacterObject.DEP))
                        {
                            CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.DEP);
                        }
                    }
                    break;

                case nameof(Character.Ambidextrous):
                    {
                        cboPrimaryArm.BeginUpdate();
                        using (new FetchSafelyFromPool<List<ListItem>>(
                                   Utils.ListItemListPool, out List<ListItem> lstPrimaryArm))
                        {
                            if (CharacterObject.Ambidextrous)
                            {
                                lstPrimaryArm.Add(new ListItem("Ambidextrous",
                                                               LanguageManager.GetString("String_Ambidextrous")));
                                cboPrimaryArm.Enabled = false;
                            }
                            else
                            {
                                //Create the dropdown for the character's primary arm.
                                lstPrimaryArm.Add(new ListItem("Left",
                                                               LanguageManager.GetString("String_Improvement_SideLeft")));
                                lstPrimaryArm.Add(new ListItem("Right",
                                                               LanguageManager.GetString("String_Improvement_SideRight")));
                                lstPrimaryArm.Sort(CompareListItems.CompareNames);
                                cboPrimaryArm.Enabled = true;
                            }

                            string strPrimaryArm = CharacterObject.PrimaryArm;
                            cboPrimaryArm.PopulateWithListItems(lstPrimaryArm);
                            cboPrimaryArm.SelectedValue = strPrimaryArm;
                            if (cboPrimaryArm.SelectedIndex == -1)
                                cboPrimaryArm.SelectedIndex = 0;
                            cboPrimaryArm.EndUpdate();
                        }
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
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                            {
                                CharacterAttrib objMAGAdept =
                                    CharacterObject.AttributeSection.GetAttributeByName("MAGAdept");
                                if (!CharacterObject.AttributeSection.Attributes.Contains(objMAGAdept))
                                {
                                    CharacterObject.AttributeSection.Attributes.Add(objMAGAdept);
                                }
                            }
                        }
                        else
                        {
                            tabCharacterTabs.TabPages.Remove(tabMagician);
                            cmdAddSpell.Enabled = false;
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute)
                            {
                                CharacterAttrib objMAGAdept =
                                    CharacterObject.AttributeSection.GetAttributeByName("MAGAdept");
                                if (CharacterObject.AttributeSection.Attributes.Contains(objMAGAdept))
                                {
                                    CharacterObject.AttributeSection.Attributes.Remove(objMAGAdept);
                                }
                            }
                        }

                        cmdAddSpirit.Visible = CharacterObject.MagicianEnabled;
                        panSpirits.Visible = CharacterObject.MagicianEnabled;
                    }
                    break;

                case nameof(Character.AdeptEnabled):
                    {
                        // Change to the status of Adept being enabled.
                        if (CharacterObject.AdeptEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabMagician))
                                tabCharacterTabs.TabPages.Insert(3, tabMagician);
                            cmdAddSpell.Enabled = true;
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
                            {
                                CharacterAttrib objMAGAdept =
                                    CharacterObject.AttributeSection.GetAttributeByName("MAGAdept");
                                if (!CharacterObject.AttributeSection.Attributes.Contains(objMAGAdept))
                                {
                                    CharacterObject.AttributeSection.Attributes.Add(objMAGAdept);
                                }
                            }

                            if (!tabCharacterTabs.TabPages.Contains(tabAdept))
                                tabCharacterTabs.TabPages.Insert(3, tabAdept);
                        }
                        else
                        {
                            if (!CharacterObject.MagicianEnabled)
                            {
                                tabCharacterTabs.TabPages.Remove(tabMagician);
                                cmdAddSpell.Enabled = false;
                                if (CharacterObjectSettings.MysAdeptSecondMAGAttribute)
                                {
                                    CharacterAttrib objMAGAdept =
                                        CharacterObject.AttributeSection.GetAttributeByName("MAGAdept");
                                    if (CharacterObject.AttributeSection.Attributes.Contains(objMAGAdept))
                                    {
                                        CharacterObject.AttributeSection.Attributes.Remove(objMAGAdept);
                                    }
                                }
                            }
                            else
                                cmdAddSpell.Enabled = true;

                            tabCharacterTabs.TabPages.Remove(tabAdept);
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

                case nameof(Character.AddBiowareEnabled):
                    {
                        if (!CharacterObject.AddBiowareEnabled)
                        {
                            string strBiowareDisabledSource = string.Empty;
                            Improvement objDisablingImprovement = ImprovementManager
                                                                  .GetCachedImprovementListForValueOf(
                                                                      CharacterObject,
                                                                      Improvement.ImprovementType.DisableBioware)
                                                                  .FirstOrDefault();
                            if (objDisablingImprovement != null)
                            {
                                strBiowareDisabledSource =
                                    LanguageManager.GetString("String_Space") + '(' +
                                    CharacterObject.GetObjectName(objDisablingImprovement) + ')' +
                                    LanguageManager.GetString("String_Space");
                            }
                            
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children,
                                         x => x.SourceType == Improvement.ImprovementSource.Bioware
                                              && x.SourceID != Cyberware.EssenceHoleGUID
                                              && x.SourceID != Cyberware.EssenceAntiHoleGUID
                                              && x.IsModularCurrentlyEquipped).ToList())
                            {
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (!objCyberware.CanRemoveThroughImprovements)
                                        continue;
                                    objCyberware.Parent?.Children.Remove(objCyberware);
                                    CharacterObject.Cyberware.Add(objCyberware);
                                    objCyberware.ChangeModularEquip(false);
                                }
                                else if (objCyberware.CanRemoveThroughImprovements)
                                {
                                    objCyberware.DeleteCyberware();
                                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                    string strEntry = LanguageManager.GetString("String_ExpenseSoldBioware");
                                    objExpense.Create(0,
                                                      strEntry + strBiowareDisabledSource +
                                                      objCyberware.DisplayNameShort(GlobalSettings.Language),
                                                      ExpenseType.Nuyen,
                                                      DateTime.Now);
                                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                                }
                            }
                        }

                        break;
                    }
                case nameof(Character.AddCyberwareEnabled):
                    {
                        if (!CharacterObject.AddCyberwareEnabled)
                        {
                            string strCyberwareDisabledSource = string.Empty;
                            Improvement objDisablingImprovement = ImprovementManager
                                                                  .GetCachedImprovementListForValueOf(
                                                                      CharacterObject,
                                                                      Improvement.ImprovementType.DisableCyberware)
                                                                  .FirstOrDefault();
                            if (objDisablingImprovement != null)
                            {
                                strCyberwareDisabledSource =
                                    LanguageManager.GetString("String_Space") + '(' +
                                    CharacterObject.GetObjectName(objDisablingImprovement) + ')' +
                                    LanguageManager.GetString("String_Space");
                            }
                            
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children,
                                         x => x.SourceType == Improvement.ImprovementSource.Cyberware
                                              && x.SourceID != Cyberware.EssenceHoleGUID
                                              && x.SourceID != Cyberware.EssenceAntiHoleGUID
                                              && x.IsModularCurrentlyEquipped).ToList())
                            {
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (!objCyberware.CanRemoveThroughImprovements)
                                        continue;
                                    objCyberware.Parent?.Children.Remove(objCyberware);
                                    CharacterObject.Cyberware.Add(objCyberware);
                                    objCyberware.ChangeModularEquip(false);
                                }
                                else if (objCyberware.CanRemoveThroughImprovements)
                                {
                                    objCyberware.DeleteCyberware();
                                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                    string strEntry = LanguageManager.GetString("String_ExpenseSoldCyberware");
                                    objExpense.Create(0,
                                                      strEntry + strCyberwareDisabledSource +
                                                      objCyberware.DisplayNameShort(GlobalSettings.Language),
                                                      ExpenseType.Nuyen,
                                                      DateTime.Now);
                                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                                }
                            }
                        }

                        break;
                    }
                case nameof(Character.ExCon):
                    {
                        if (CharacterObject.ExCon)
                        {
                            string strExConString = string.Empty;
                            Improvement objExConImprovement = ImprovementManager
                                                              .GetCachedImprovementListForValueOf(
                                                                  CharacterObject,
                                                                  Improvement.ImprovementType.ExCon)
                                                              .FirstOrDefault();
                            if (objExConImprovement != null)
                            {
                                strExConString = LanguageManager.GetString("String_Space") + '(' +
                                                 CharacterObject.GetObjectName(objExConImprovement,
                                                     GlobalSettings.Language) + ')' +
                                                 LanguageManager.GetString("String_Space");
                            }
                            
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children,
                                         x => x.Grade.Name != "None"
                                              && x.SourceID != Cyberware.EssenceHoleGUID
                                              && x.SourceID != Cyberware.EssenceAntiHoleGUID
                                              && x.IsModularCurrentlyEquipped).ToList())
                            {
                                char chrAvail = objCyberware.TotalAvailTuple(false).Suffix;
                                if (chrAvail != 'R' && chrAvail != 'F')
                                    continue;
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (!objCyberware.CanRemoveThroughImprovements)
                                        continue;
                                    objCyberware.Parent?.Children.Remove(objCyberware);
                                    CharacterObject.Cyberware.Add(objCyberware);
                                    objCyberware.ChangeModularEquip(false);
                                }
                                else if (objCyberware.CanRemoveThroughImprovements)
                                {
                                    objCyberware.DeleteCyberware();
                                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                                    string strEntry = LanguageManager.GetString(
                                        objCyberware.SourceType == Improvement.ImprovementSource.Cyberware
                                            ? "String_ExpenseSoldCyberware"
                                            : "String_ExpenseSoldBioware");
                                    objExpense.Create(0,
                                                      strEntry + strExConString
                                                               + objCyberware.DisplayNameShort(GlobalSettings.Language),
                                                      ExpenseType.Nuyen, DateTime.Now);
                                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                                }
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
                case nameof(Character.FirstMentorSpiritDisplayName):
                    {
                        MentorSpirit objMentor = CharacterObject.MentorSpirits.FirstOrDefault();
                        if (objMentor != null)
                        {
                            objMentor.SetSourceDetail(lblMentorSpiritSource);
                            objMentor.SetSourceDetail(lblParagonSource);
                        }

                        break;
                    }
                case nameof(Character.HasMentorSpirit):
                    {
                        gpbMagicianMentorSpirit.Visible = CharacterObject.HasMentorSpirit;
                        gpbTechnomancerParagon.Visible = CharacterObject.HasMentorSpirit;
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
                case nameof(Character.MetatypeCategory):
                    {
                        IsCharacterUpdateRequested = true;
                        mnuSpecialCyberzombie.Visible = CharacterObject.MetatypeCategory != "Cyberzombie";
                        break;
                    }
                case nameof(Character.IsSprite):
                    {
                        IsCharacterUpdateRequested = true;
                        mnuSpecialConvertToFreeSprite.Visible = CharacterObject.IsSprite;
                        break;
                    }
                case nameof(Character.Settings):
                    {
                        foreach (PropertyInfo objProperty in typeof(CharacterSettings).GetProperties())
                            OnCharacterPropertyChanged(CharacterObjectSettings, new PropertyChangedEventArgs(objProperty.Name));
                        break;
                    }
            }
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsCharacterUpdateRequested = true;
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.ArmorDegradation):
                    cmdArmorDecrease.Visible = CharacterObjectSettings.ArmorDegradation;
                    cmdArmorIncrease.Visible = CharacterObjectSettings.ArmorDegradation;
                    break;
                case nameof(CharacterSettings.Books):
                    {
                        using (new CursorWait(this))
                        {
                            SuspendLayout();
                            cmdAddLifestyle.SplitMenuStrip =
                                CharacterObjectSettings.BookEnabled("RF") ? cmsAdvancedLifestyle : null;

                            if (!CharacterObjectSettings.BookEnabled("FA"))
                            {
                                lblWildReputation.Visible = false;
                                nudWildReputation.Visible = false;
                                lblWildReputationTotal.Visible = false;
                                if (!CharacterObjectSettings.BookEnabled("SG"))
                                {
                                    lblAstralReputation.Visible = false;
                                    nudAstralReputation.Visible = false;
                                    lblAstralReputationTotal.Visible = false;
                                }
                                else
                                {
                                    lblAstralReputation.Visible = true;
                                    nudAstralReputation.Visible = true;
                                    lblAstralReputationTotal.Visible = true;
                                }
                            }
                            else
                            {
                                lblWildReputation.Visible = true;
                                nudWildReputation.Visible = true;
                                lblWildReputationTotal.Visible = true;
                                lblAstralReputation.Visible = true;
                                nudAstralReputation.Visible = true;
                                lblAstralReputationTotal.Visible = true;
                            }

                            // Refresh all trees because enabled sources can change the nodes that are visible
                            RefreshQualities(treQualities, cmsQuality);
                            RefreshSpirits(panSpirits, panSprites);
                            RefreshSpells(treSpells, treMetamagic, cmsSpell, cmsInitiationNotes);
                            RefreshComplexForms(treComplexForms, treMetamagic, cmsComplexForm, cmsInitiationNotes);
                            RefreshPowerCollectionListChanged(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                            RefreshInitiationGrades(treMetamagic, cmsMetamagic, cmsInitiationNotes);
                            RefreshAIPrograms(treAIPrograms, cmsAdvancedProgram);
                            RefreshCritterPowers(treCritterPowers, cmsCritterPowers);
                            RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique);
                            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle);
                            RefreshContacts(panContacts, panEnemies, panPets);
                            RefreshSustainedSpells(flpSustainedSpells, flpSustainedComplexForms, flpSustainedCritterPowers, chkPsycheActiveMagician, chkPsycheActiveTechnomancer);

                            RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear);
                            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
                            RefreshFociFromGear(treFoci, null);
                            RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear);
                            RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory,
                                cmsWeaponAccessoryGear);
                            RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon,
                                cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear, cmsWeaponMount,
                                cmsVehicleCyberware, cmsVehicleCyberwareGear);
                            RefreshDrugs(treCustomDrugs);
                            treWeapons.SortCustomOrder();
                            treArmor.SortCustomOrder();
                            treGear.SortCustomOrder();
                            treLifestyles.SortCustomOrder();
                            treCustomDrugs.SortCustomOrder();
                            treCyberware.SortCustomOrder();
                            treVehicles.SortCustomOrder();
                            treCritterPowers.SortCustomOrder();

                            XPathNavigator xmlTraditionsBaseChummerNode =
                                CharacterObject.LoadDataXPath("traditions.xml").SelectSingleNodeAndCacheExpression("/chummer");
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstTraditions))
                            {
                                if (xmlTraditionsBaseChummerNode != null)
                                {
                                    foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                                 "traditions/tradition[" + CharacterObjectSettings.BookXPath() + ']'))
                                    {
                                        string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")?.Value;
                                        if (!string.IsNullOrEmpty(strName))
                                            lstTraditions.Add(new ListItem(
                                                                  xmlTradition.SelectSingleNodeAndCacheExpression("id")
                                                                              ?.Value ?? strName,
                                                                  xmlTradition
                                                                      .SelectSingleNodeAndCacheExpression("translate")
                                                                      ?.Value ?? strName));
                                    }
                                }

                                if (lstTraditions.Count > 1)
                                {
                                    lstTraditions.Sort(CompareListItems.CompareNames);
                                    lstTraditions.Insert(
                                        0, new ListItem("None", LanguageManager.GetString("String_None")));
                                    if (!lstTraditions.SequenceEqual(cboTradition.Items.Cast<ListItem>()))
                                    {
                                        cboTradition.BeginUpdate();
                                        cboTradition.PopulateWithListItems(lstTraditions);
                                        cboTradition.EndUpdate();
                                    }
                                }
                                else
                                {
                                    cboTradition.Visible = false;
                                    lblTraditionLabel.Visible = false;
                                }
                            }

                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstDrainAttributes))
                            {
                                lstDrainAttributes.Add(ListItem.Blank);
                                if (xmlTraditionsBaseChummerNode != null)
                                {
                                    foreach (XPathNavigator xmlDrain in xmlTraditionsBaseChummerNode
                                                 .SelectAndCacheExpression(
                                                     "drainattributes/drainattribute"))
                                    {
                                        string strName = xmlDrain.SelectSingleNodeAndCacheExpression("name")?.Value;
                                        if (!string.IsNullOrEmpty(strName))
                                            lstDrainAttributes.Add(new ListItem(strName,
                                                                       xmlDrain.SelectSingleNodeAndCacheExpression(
                                                                           "translate")?.Value ?? strName));
                                    }
                                }

                                lstDrainAttributes.Sort(CompareListItems.CompareNames);
                                if (!lstDrainAttributes.SequenceEqual(cboDrain.Items.Cast<ListItem>()))
                                {
                                    cboDrain.BeginUpdate();
                                    cboDrain.PopulateWithListItems(lstDrainAttributes);
                                    cboDrain.EndUpdate();
                                }
                            }

                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> limit))
                            {
                                foreach (Improvement improvement in ImprovementManager.GetCachedImprovementListForValueOf(
                                             CharacterObject, Improvement.ImprovementType.LimitSpiritCategory))
                                {
                                    limit.Add(improvement.ImprovedName);
                                }

                                using (new FetchSafelyFromPool<List<ListItem>>(
                                           Utils.ListItemListPool, out List<ListItem> lstSpirit))
                                {
                                    lstSpirit.Add(ListItem.Blank);
                                    if (xmlTraditionsBaseChummerNode != null)
                                    {
                                        foreach (XPathNavigator xmlSpirit in xmlTraditionsBaseChummerNode
                                                     .SelectAndCacheExpression("spirits/spirit"))
                                        {
                                            string strSpiritName
                                                = xmlSpirit.SelectSingleNodeAndCacheExpression("name")?.Value;
                                            if (!string.IsNullOrEmpty(strSpiritName)
                                                && (limit.Count == 0 || limit.Contains(strSpiritName)))
                                            {
                                                lstSpirit.Add(new ListItem(strSpiritName,
                                                                           xmlSpirit.SelectSingleNodeAndCacheExpression(
                                                                               "translate")?.Value ?? strSpiritName));
                                            }
                                        }
                                    }

                                    lstSpirit.Sort(CompareListItems.CompareNames);
                                    if (!lstSpirit.SequenceEqual(cboSpiritCombat.Items.Cast<ListItem>()))
                                    {
                                        cboSpiritCombat.BeginUpdate();
                                        cboSpiritCombat.PopulateWithListItems(lstSpirit);
                                        cboSpiritCombat.EndUpdate();

                                        cboSpiritDetection.BeginUpdate();
                                        cboSpiritDetection.PopulateWithListItems(lstSpirit);
                                        cboSpiritDetection.EndUpdate();

                                        cboSpiritHealth.BeginUpdate();
                                        cboSpiritHealth.PopulateWithListItems(lstSpirit);
                                        cboSpiritHealth.EndUpdate();

                                        cboSpiritIllusion.BeginUpdate();
                                        cboSpiritIllusion.PopulateWithListItems(lstSpirit);
                                        cboSpiritIllusion.EndUpdate();

                                        cboSpiritManipulation.BeginUpdate();
                                        cboSpiritManipulation.PopulateWithListItems(lstSpirit);
                                        cboSpiritManipulation.EndUpdate();
                                    }
                                }
                            }

                            // Populate the Technomancer Streams list.
                            xmlTraditionsBaseChummerNode =
                                CharacterObject.LoadDataXPath("streams.xml").SelectSingleNodeAndCacheExpression("/chummer");
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstStreams))
                            {
                                if (xmlTraditionsBaseChummerNode != null)
                                {
                                    foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                                 "traditions/tradition[" + CharacterObjectSettings.BookXPath() + ']'))
                                    {
                                        string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")?.Value;
                                        if (!string.IsNullOrEmpty(strName))
                                            lstStreams.Add(new ListItem(
                                                               xmlTradition.SelectSingleNodeAndCacheExpression("id")
                                                                           ?.Value ?? strName,
                                                               xmlTradition
                                                                   .SelectSingleNodeAndCacheExpression("translate")
                                                                   ?.Value ?? strName));
                                    }
                                }

                                if (lstStreams.Count > 1)
                                {
                                    lstStreams.Sort(CompareListItems.CompareNames);
                                    lstStreams.Insert(
                                        0, new ListItem("None", LanguageManager.GetString("String_None")));
                                    if (!lstStreams.SequenceEqual(cboStream.Items.Cast<ListItem>()))
                                    {
                                        cboStream.BeginUpdate();
                                        cboStream.PopulateWithListItems(lstStreams);
                                        cboStream.EndUpdate();
                                    }
                                }
                                else
                                {
                                    cboStream.Visible = false;
                                    lblStreamLabel.Visible = false;
                                }
                            }

                            RefreshSelectedVehicle();
                            ResumeLayout();
                        }

                        break;
                    }
                case nameof(CharacterSettings.AllowFreeGrids):
                    {
                        if (!CharacterObjectSettings.BookEnabled("HT"))
                        {
                            using (new CursorWait(this))
                            {
                                SuspendLayout();
                                RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle);
                                treLifestyles.SortCustomOrder();
                                ResumeLayout();
                            }
                        }

                        break;
                    }
                case nameof(CharacterSettings.EnableEnemyTracking):
                {
                    using (new CursorWait(this))
                    {
                        SuspendLayout();
                        if (!CharacterObjectSettings.EnableEnemyTracking)
                        {
                            tabPeople.TabPages.Remove(tabEnemies);
                        }
                        else
                        {
                            if (!tabPeople.TabPages.Contains(tabEnemies))
                                tabPeople.TabPages.Insert(tabPeople.TabPages.IndexOf(tabContacts) + 1, tabEnemies);
                            RefreshContacts(panContacts, panEnemies, panPets);
                        }
                        ResumeLayout();
                    }

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
            //So, skills tell if anything maybe interesting have happened, but this don't have any way to see if it is relevant. Instead of redrawing EVERY FYCKING THING we do it only every 5 ms
            if (SkillPropertyChanged_StopWatch.ElapsedMilliseconds < 4) return;
            SkillPropertyChanged_StopWatch.Restart();

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
        */

        #endregion Character Events

        #region Menu Events

        private async void mnuFileSave_Click(object sender, EventArgs e)
        {
            await SaveCharacter();
        }

        private async void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            await SaveCharacterAs();
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void mnuFilePrint_Click(object sender, EventArgs e)
        {
            await DoPrint();
        }

        private async void mnuFileExport_Click(object sender, EventArgs e)
        {
            using (ExportCharacter frmExportCharacter = new ExportCharacter(CharacterObject))
                await frmExportCharacter.ShowDialogSafeAsync(this);
        }

        private async void mnuSpecialCyberzombie_Click(object sender, EventArgs e)
        {
            await CharacterObject.ConvertCyberzombie();
        }

        private async void mnuSpecialReduceAttribute_Click(object sender, EventArgs e)
        {
            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings);

            lstAbbrevs.Remove("ESS");
            if (!CharacterObject.MAGEnabled)
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!CharacterObject.IsMysticAdept || !CharacterObjectSettings.MysAdeptSecondMAGAttribute)
                lstAbbrevs.Remove("MAGAdept");

            if (!CharacterObject.RESEnabled)
                lstAbbrevs.Remove("RES");
            if (!CharacterObject.DEPEnabled)
                lstAbbrevs.Remove("DEP");

            // Display the Select CharacterAttribute window and record which Skill was selected.
            using (SelectAttribute frmPickAttribute = new SelectAttribute(lstAbbrevs.ToArray())
            {
                Description = await LanguageManager.GetStringAsync("String_CyberzombieReduceAttribute"),
                ShowMetatypeMaximum = true
            })
            {
                await frmPickAttribute.ShowDialogSafeAsync(this);

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
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void mnuSpecialCloningMachine_Click(object sender, EventArgs e)
        {
            int intClones;
            using (SelectNumber frmPickNumber = new SelectNumber(0)
            {
                Description = await LanguageManager.GetStringAsync("String_CloningMachineNumber"),
                Minimum = 1
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                intClones = frmPickNumber.SelectedValue.ToInt32();
            }

            if (intClones <= 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CloningMachineNumberRequired"), await LanguageManager.GetStringAsync("MessageTitle_CloningMachineNumberRequired"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (new CursorWait(this))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                Character[] lstClones = new Character[intClones];
                // Await structure prevents UI thread lock-ups if the LoadCharacter() function shows any messages
                await Task.Run(() => Parallel.For(0, intClones,
                    i => lstClones[i] = Program.MainForm.LoadCharacter(CharacterObject.FileName,
                        CharacterObject.Alias + strSpace + i.ToString(GlobalSettings.CultureInfo), true)));
                Program.MainForm.OpenCharacterList(lstClones, false);
            }
        }

        private async void mnuSpecialReapplyImprovements_Click(object sender, EventArgs e)
        {
            // This only re-applies the Improvements for everything the character has. If a match is not found in the data files, the current Improvement information is left as-is.
            // Verify that the user wants to go through with it.
            if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ConfirmReapplyImprovements"), await LanguageManager.GetStringAsync("MessageTitle_ConfirmReapplyImprovements"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            DoReapplyImprovements();
        }

        private void DoReapplyImprovements(ICollection<string> lstInternalIdFilter = null)
        {
            using (new CursorWait(this))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdOutdatedItems))
                {
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
                        if (objQuality.OriginSource == QualitySource.Improvement || objQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen)
                            continue;
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objQuality.InternalId) == false)
                            continue;

                        XmlNode objNode = objQuality.GetNode();
                        if (objNode != null)
                        {
                            string strSelected = objQuality.Extra;
                            objQuality.Bonus = objNode["bonus"];
                            if (objQuality.Bonus != null)
                            {
                                ImprovementManager.ForcedValue = strSelected;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.Bonus, 1, objQuality.DisplayNameShort(GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objQuality.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                                    if (objTreeNode != null)
                                        objTreeNode.Text = objQuality.CurrentDisplayName;
                                }
                            }

                            objQuality.FirstLevelBonus = objNode["firstlevelbonus"];
                            if (objQuality.FirstLevelBonus?.HasChildNodes == true)
                            {
                                bool blnDoFirstLevel = true;
                                for (int k = 0; k < CharacterObject.Qualities.Count; ++k)
                                {
                                    Quality objCheckQuality = CharacterObject.Qualities[k];
                                    if (j != k
                                        && objCheckQuality.SourceIDString == objQuality.SourceIDString
                                        && objCheckQuality.Extra == objQuality.Extra
                                        && objCheckQuality.SourceName == objQuality.SourceName
                                        && (k < j
                                            || objCheckQuality.OriginSource == QualitySource.Improvement
                                            || (lstInternalIdFilter?.Contains(objCheckQuality.InternalId) == false)))
                                    {
                                        blnDoFirstLevel = false;
                                        break;
                                    }
                                }

                                if (blnDoFirstLevel)
                                {
                                    ImprovementManager.ForcedValue = strSelected;
                                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.FirstLevelBonus, 1, objQuality.DisplayNameShort(GlobalSettings.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    {
                                        objQuality.Extra = ImprovementManager.SelectedValue;
                                        TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                                        if (objTreeNode != null)
                                            objTreeNode.Text = objQuality.CurrentDisplayName;
                                    }
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objQuality.CurrentDisplayName);
                        }
                    }

                    // Refresh Martial Art Techniques.
                    foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                    {
                        XmlNode objMartialArtNode = objMartialArt.GetNode();
                        if (objMartialArtNode != null)
                        {
                            // We're only re-apply improvements a list of items, not all of them
                            if (lstInternalIdFilter?.Contains(objMartialArt.InternalId) != false
                                && objMartialArtNode["bonus"] != null)
                            {
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId, objMartialArtNode["bonus"], 1, objMartialArt.CurrentDisplayName);
                            }

                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
                            {
                                XmlNode objNode = objTechnique.GetNode();
                                if (objNode != null)
                                {
                                    if (objNode["bonus"] != null)
                                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.MartialArtTechnique, objTechnique.InternalId, objNode["bonus"], 1, objTechnique.CurrentDisplayName);
                                }
                                else
                                {
                                    sbdOutdatedItems.AppendLine(objMartialArt.CurrentDisplayName);
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objMartialArt.CurrentDisplayName);
                        }
                    }

                    // Refresh Spells.
                    foreach (Spell objSpell in CharacterObject.Spells.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
                    {
                        XmlNode objNode = objSpell.GetNode();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objSpell.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Spell, objSpell.InternalId, objNode["bonus"], 1, objSpell.DisplayNameShort(GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objSpell.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objSpellNode = treSpells.FindNode(objSpell.InternalId);
                                    if (objSpellNode != null)
                                        objSpellNode.Text = objSpell.CurrentDisplayName;
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objSpell.CurrentDisplayName);
                        }
                    }

                    // Refresh Adept Powers.
                    foreach (Power objPower in CharacterObject.Powers.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
                    {
                        XmlNode objNode = objPower.GetNode();
                        if (objNode != null)
                        {
                            objPower.Bonus = objNode["bonus"];
                            if (objPower.Bonus != null)
                            {
                                ImprovementManager.ForcedValue = objPower.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, objPower.TotalRating,
                                    objPower.DisplayNameShort(GlobalSettings.Language));
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objPower.CurrentDisplayName);
                        }
                    }

                    // Refresh Complex Forms.
                    foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
                    {
                        XmlNode objNode = objComplexForm.GetNode();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objComplexForm.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId, objNode["bonus"], 1, objComplexForm.DisplayNameShort(GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objComplexForm.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objCFNode = treComplexForms.FindNode(objComplexForm.InternalId);
                                    if (objCFNode != null)
                                        objCFNode.Text = objComplexForm.CurrentDisplayName;
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objComplexForm.CurrentDisplayName);
                        }
                    }

                    // Refresh AI Programs and Advanced Programs
                    foreach (AIProgram objProgram in CharacterObject.AIPrograms.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
                    {
                        XmlNode objNode = objProgram.GetNode();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objProgram.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.AIProgram, objProgram.InternalId, objNode["bonus"], 1, objProgram.DisplayNameShort(GlobalSettings.Language));
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
                            sbdOutdatedItems.AppendLine(objProgram.DisplayName);
                        }
                    }

                    // Refresh Critter Powers.
                    foreach (CritterPower objPower in CharacterObject.CritterPowers.Where(x => lstInternalIdFilter?.Contains(x.InternalId) != true))
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

                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId, objPower.Bonus, intRating, objPower.DisplayNameShort(GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objPower.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objPowerNode = treCritterPowers.FindNode(objPower.InternalId);
                                    if (objPowerNode != null)
                                        objPowerNode.Text = objPower.CurrentDisplayName;
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objPower.CurrentDisplayName);
                        }
                    }

                    // Refresh Metamagics and Echoes.
                    // We cannot use foreach because metamagics/echoes can add more metamagics/echoes
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (int j = 0; j < CharacterObject.Metamagics.Count; j++)
                    {
                        Metamagic objMetamagic = CharacterObject.Metamagics[j];
                        if (objMetamagic.Grade < 0)
                            continue;
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objMetamagic.InternalId) == false)
                            continue;
                        XmlNode objNode = objMetamagic.GetNode();
                        if (objNode != null)
                        {
                            objMetamagic.Bonus = objNode["bonus"];
                            if (objMetamagic.Bonus != null)
                            {
                                ImprovementManager.CreateImprovements(CharacterObject, objMetamagic.SourceType, objMetamagic.InternalId, objMetamagic.Bonus, 1, objMetamagic.DisplayNameShort(GlobalSettings.Language));
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objMetamagic.CurrentDisplayName);
                        }
                    }

                    // Refresh Cyberware and Bioware.
                    Dictionary<Cyberware, int> dicPairableCyberwares = new Dictionary<Cyberware, int>(CharacterObject.Cyberware.Count);
                    foreach (Cyberware objCyberware in CharacterObject.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objCyberware.InternalId) != false)
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
                                    ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.Bonus, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalSettings.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                        objCyberware.Extra = ImprovementManager.SelectedValue;
                                }

                                if (!objCyberware.IsModularCurrentlyEquipped)
                                    objCyberware.ChangeModularEquip(false);
                                else
                                {
                                    objCyberware.RefreshWirelessBonuses();
                                    if (objCyberware.PairBonus != null)
                                    {
                                        Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra);
                                        if (objMatchingCyberware != null)
                                            ++dicPairableCyberwares[objMatchingCyberware];
                                        else
                                            dicPairableCyberwares.Add(objCyberware, 1);
                                    }
                                }

                                TreeNode objWareNode = objCyberware.SourceID == Cyberware.EssenceHoleGUID || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
                                    ? treCyberware.FindNode(objCyberware.SourceIDString)
                                    : treCyberware.FindNode(objCyberware.InternalId);
                                if (objWareNode != null)
                                    objWareNode.Text = objCyberware.CurrentDisplayName;
                            }
                            else
                            {
                                sbdOutdatedItems.AppendLine(objCyberware.CurrentDisplayName);
                            }
                        }

                        foreach (Gear objGear in objCyberware.GearChildren)
                        {
                            objGear.ReaddImprovements(treCyberware, sbdOutdatedItems, lstInternalIdFilter);
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
                                    ++intNotMatchLocationCount;
                                else
                                    ++intMatchLocationCount;
                            }

                            // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                            intCyberwaresCount = Math.Min(intNotMatchLocationCount, intMatchLocationCount) * 2;
                        }

                        if (intCyberwaresCount > 0)
                        {
                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                if ((intCyberwaresCount & 1) == 0)
                                {
                                    if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                        ImprovementManager.ForcedValue = objCyberware.Forced;
                                    ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                                        objLoopCyberware.DisplayNameShort(GlobalSettings.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                        objCyberware.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objNode = objLoopCyberware.SourceID == Cyberware.EssenceHoleGUID || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
                                        ? treCyberware.FindNode(objCyberware.SourceIDString)
                                        : treCyberware.FindNode(objLoopCyberware.InternalId);
                                    if (objNode != null)
                                        objNode.Text = objLoopCyberware.CurrentDisplayName;
                                }

                                --intCyberwaresCount;
                                if (intCyberwaresCount <= 0)
                                    break;
                            }
                        }
                    }

                    // Refresh Armors.
                    foreach (Armor objArmor in CharacterObject.Armor)
                    {
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objArmor.InternalId) != false)
                        {
                            XmlNode objNode = objArmor.GetNode();
                            if (objNode != null)
                            {
                                objArmor.Bonus = objNode["bonus"];
                                if (objArmor.Bonus != null && objArmor.Equipped)
                                {
                                    ImprovementManager.ForcedValue = objArmor.Extra;
                                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Armor, objArmor.InternalId, objArmor.Bonus, objArmor.Rating, objArmor.DisplayNameShort(GlobalSettings.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    {
                                        objArmor.Extra = ImprovementManager.SelectedValue;

                                        TreeNode objArmorNode = treArmor.FindNode(objArmor.InternalId);
                                        if (objArmorNode != null)
                                            objArmorNode.Text = objArmor.CurrentDisplayName;
                                    }
                                }
                            }
                            else
                            {
                                sbdOutdatedItems.AppendLine(objArmor.CurrentDisplayName);
                            }
                        }

                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            // We're only re-apply improvements a list of items, not all of them
                            if (lstInternalIdFilter?.Contains(objMod.InternalId) != false)
                            {
                                XmlNode objChild = objMod.GetNode();

                                if (objChild != null)
                                {
                                    objMod.Bonus = objChild["bonus"];
                                    if (objMod.Bonus != null && objMod.Equipped)
                                    {
                                        ImprovementManager.ForcedValue = objMod.Extra;
                                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.Bonus, objMod.Rating, objMod.DisplayNameShort(GlobalSettings.Language));
                                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                        {
                                            objMod.Extra = ImprovementManager.SelectedValue;

                                            TreeNode objPluginNode = treArmor.FindNode(objMod.InternalId);
                                            if (objPluginNode != null)
                                                objPluginNode.Text = objMod.CurrentDisplayName;
                                        }
                                    }
                                }
                                else
                                {
                                    sbdOutdatedItems.AppendLine(objMod.CurrentDisplayName);
                                }
                            }

                            foreach (Gear objGear in objMod.GearChildren)
                            {
                                objGear.ReaddImprovements(treArmor, sbdOutdatedItems, lstInternalIdFilter);
                            }
                        }

                        foreach (Gear objGear in objArmor.GearChildren)
                        {
                            objGear.ReaddImprovements(treArmor, sbdOutdatedItems, lstInternalIdFilter);
                        }
                        objArmor.RefreshWirelessBonuses();
                    }

                    // Refresh Gear.
                    foreach (Gear objGear in CharacterObject.Gear)
                    {
                        objGear.ReaddImprovements(treGear, sbdOutdatedItems, lstInternalIdFilter);
                        objGear.RefreshWirelessBonuses();
                    }

                    // Refresh Weapons Gear
                    foreach (Weapon objWeapon in CharacterObject.Weapons)
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            foreach (Gear objGear in objAccessory.GearChildren)
                            {
                                objGear.ReaddImprovements(treWeapons, sbdOutdatedItems, lstInternalIdFilter);
                            }
                        }
                        objWeapon.RefreshWirelessBonuses();
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

                    if (sbdOutdatedItems.Length > 0 && !Utils.IsUnitTest)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_ReapplyImprovementsFoundOutdatedItems_Top") +
                                                              sbdOutdatedItems +
                                                              LanguageManager.GetString("Message_ReapplyImprovementsFoundOutdatedItems_Bottom"), LanguageManager.GetString("MessageTitle_ConfirmReapplyImprovements"), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }

            IsDirty = true;
        }

        private void mnuSpecialPossess_Click(object sender, EventArgs e)
        {
            // Make sure the Spirit has been saved first.
            if (IsDirty && Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_PossessionSave"), LanguageManager.GetString("MessageTitle_Possession"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            if (Utils.IsUnitTest)
            {
                Utils.BreakIfDebug();
                return;
            }

            string strFileName;
            // Prompt the user to select a save file to possess.
            using (OpenFileDialog openFileDialog = new OpenFileDialog
                   {
                       Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|'
                           + LanguageManager.GetString("DialogFilter_All")
                   })
            {
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                strFileName = openFileDialog.FileName;
            }

            string strOpenFile = string.Empty;
            using (new CursorWait(this))
            {
                using (Character objMerge = new Character { FileName = CharacterObject.FileName })
                {
                    using (Character objVessel = new Character { FileName = strFileName })
                    {
                        using (LoadingBar frmLoadingForm = ChummerMainForm.CreateAndShowProgressBar(Path.GetFileName(objVessel.FileName), Character.NumLoadingSections * 2 + 7))
                        {
                            bool blnSuccess = objVessel.Load(frmLoadingForm);
                            if (!blnSuccess)
                            {
                                Program.MainForm.ShowMessageBox(this,
                                    LanguageManager.GetString("Message_Load_Error_Warning"),
                                    LanguageManager.GetString("String_Error"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }
                            // Make sure the Vessel is in Career Mode.
                            if (!objVessel.Created)
                            {
                                Program.MainForm.ShowMessageBox(this,
                                    LanguageManager.GetString("Message_VesselInCareerMode"),
                                    LanguageManager.GetString("MessageTitle_Possession"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }

                            // Load the Spirit's save file into a new Merge character.
                            frmLoadingForm.CharacterFile = objMerge.FileName;
                            blnSuccess = objMerge.Load(frmLoadingForm);
                            if (!blnSuccess)
                            {
                                Program.MainForm.ShowMessageBox(this,
                                    LanguageManager.GetString("Message_Load_Error_Warning"),
                                    LanguageManager.GetString("String_Error"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }
                            objMerge.Possessed = true;
                            objMerge.Alias = objVessel.CharacterName + LanguageManager.GetString("String_Space") + '(' + LanguageManager.GetString("String_Possessed") + ')';

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
                                XmlDocument objPowerDoc = CharacterObject.LoadData("critterpowers.xml");
                                XmlNode objPower = objPowerDoc.SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                                CritterPower objCritterPower = new CritterPower(objMerge);
                                objCritterPower.Create(objPower, 0, "Normal Weapons");
                                objMerge.CritterPowers.Add(objCritterPower);
                            }

                            //TOD: Implement Possession attribute bonuses.
                            /*
                            // Add the Vessel's Physical Attributes to the Spirit's Force.
                            objMerge.BOD.MetatypeMaximum = objVessel.BOD.Value + objMerge.MAG.TotalValue;
                            objMerge.BOD.Value = objVessel.BOD.Value + objMerge.MAG.TotalValue;
                            objMerge.AGI.MetatypeMaximum = objVessel.AGI.Value + objMerge.MAG.TotalValue;
                            objMerge.AGI.Value = objVessel.AGI.Value + objMerge.MAG.TotalValue;
                            objMerge.REA.MetatypeMaximum = objVessel.REA.Value + objMerge.MAG.TotalValue;
                            objMerge.REA.Value = objVessel.REA.Value + objMerge.MAG.TotalValue;
                            objMerge.STR.MetatypeMaximum = objVessel.STR.Value + objMerge.MAG.TotalValue;
                            objMerge.STR.Value = objVessel.STR.Value + objMerge.MAG.TotalValue;
                            */

                            frmLoadingForm.PerformStep(LanguageManager.GetString("String_SelectPACKSKit_Lifestyles"));
                            // Copy any Lifestyles the Vessel has.
                            foreach (Lifestyle objLifestyle in objVessel.Lifestyles)
                                objMerge.Lifestyles.Add(objLifestyle);

                            frmLoadingForm.PerformStep(LanguageManager.GetString("Tab_Armor"));
                            // Copy any Armor the Vessel has.
                            foreach (Armor objArmor in objVessel.Armor)
                            {
                                objMerge.Armor.Add(objArmor);
                                CopyArmorImprovements(objVessel, objMerge, objArmor);
                            }

                            frmLoadingForm.PerformStep(LanguageManager.GetString("Tab_Gear"));
                            // Copy any Gear the Vessel has.
                            foreach (Gear objGear in objVessel.Gear)
                            {
                                objMerge.Gear.Add(objGear);
                                CopyGearImprovements(objVessel, objMerge, objGear);
                            }

                            frmLoadingForm.PerformStep(LanguageManager.GetString("Tab_Cyberware"));
                            // Copy any Cyberware/Bioware the Vessel has.
                            foreach (Cyberware objCyberware in objVessel.Cyberware)
                            {
                                objMerge.Cyberware.Add(objCyberware);
                                CopyCyberwareImprovements(objVessel, objMerge, objCyberware);
                            }

                            frmLoadingForm.PerformStep(LanguageManager.GetString("Tab_Weapons"));
                            // Copy any Weapons the Vessel has.
                            foreach (Weapon objWeapon in objVessel.Weapons)
                                objMerge.Weapons.Add(objWeapon);

                            frmLoadingForm.PerformStep(LanguageManager.GetString("Tab_Vehicles"));
                            // Copy and Vehicles the Vessel has.
                            foreach (Vehicle objVehicle in objVessel.Vehicles)
                                objMerge.Vehicles.Add(objVehicle);

                            frmLoadingForm.PerformStep(LanguageManager.GetString("String_Settings"));
                            // Copy the character info.
                            objMerge.Gender = objVessel.Gender;
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
                        }
                    }

                    string strShowFileName = Path.GetFileName(objMerge.FileName);
                    if (string.IsNullOrEmpty(strShowFileName))
                        strShowFileName = objMerge.CharacterName;
                    strShowFileName = strShowFileName.TrimEndOnce(".chum5") + LanguageManager.GetString("String_Space") + '(' + LanguageManager.GetString("String_Possessed") + ')';

                    // Now that everything is done, save the merged character and open them.
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All"),
                        FileName = strShowFileName
                    })
                    {
                        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        using (LoadingBar frmProgressBar = ChummerMainForm.CreateAndShowProgressBar())
                        {
                            frmProgressBar.PerformStep(objMerge.CharacterName, LoadingBar.ProgressBarTextPatterns.Saving);
                            objMerge.FileName = saveFileDialog.FileName;
                            if (objMerge.Save())
                            {
                                // Get the name of the file and destroy the references to the Vessel and the merged character.
                                strOpenFile = objMerge.FileName;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(strOpenFile))
            {
                using (new CursorWait(this))
                {
                    Character objOpenCharacter = Program.MainForm.LoadCharacter(strOpenFile);
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
            }
        }

        private async void mnuSpecialPossessInanimate_Click(object sender, EventArgs e)
        {
            // Make sure the Spirit has been saved first.
            if (IsDirty && Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_PossessionSave"), await LanguageManager.GetStringAsync("MessageTitle_Possession"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            if (Utils.IsUnitTest)
            {
                Utils.BreakIfDebug();
                return;
            }

            string strSelectedVessel;
            // Prompt the user to select an inanimate Vessel.
            XPathNavigator xmlVesselsNavigator = await CharacterObject.LoadDataXPathAsync("vessels.xml");
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMetatype))
            {
                foreach (XPathNavigator xmlMetatype in xmlVesselsNavigator.SelectAndCacheExpression(
                             "/chummer/metatypes/metatype"))
                {
                    string strName = xmlMetatype.SelectSingleNodeAndCacheExpression("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        ListItem objItem
                            = new ListItem(
                                strName, xmlMetatype.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName);
                        lstMetatype.Add(objItem);
                    }
                }

                using (SelectItem frmSelectVessel = new SelectItem())
                {
                    frmSelectVessel.SetGeneralItemsMode(lstMetatype);
                    await frmSelectVessel.ShowDialogSafeAsync(this);

                    if (frmSelectVessel.DialogResult == DialogResult.Cancel)
                        return;

                    strSelectedVessel = frmSelectVessel.SelectedItem;
                }
            }

            // Get the Node for the selected Vessel.
            XmlDocument xmlVessels = await CharacterObject.LoadDataAsync("vessels.xml");
            XmlNode objSelected = xmlVessels.SelectSingleNode("/chummer/metatypes/metatype[name = " + strSelectedVessel.CleanXPath() + ']');
            if (objSelected == null)
                return;

            string strOpenFile = string.Empty;
            using (new CursorWait(this))
            {
                // Load the Spirit's save file into a new Merge character.
                using (Character objMerge = new Character { FileName = CharacterObject.FileName })
                {
                    using (LoadingBar frmLoadingForm = new LoadingBar { CharacterFile = objMerge.FileName })
                    {
                        frmLoadingForm.Reset(36);
                        frmLoadingForm.Show();
                        await objMerge.LoadAsync();
                        frmLoadingForm.PerformStep(await LanguageManager.GetStringAsync("String_UI"));
                        objMerge.Possessed = true;
                        objMerge.Alias = strSelectedVessel + await LanguageManager.GetStringAsync("String_Space") + '(' + await LanguageManager.GetStringAsync("String_Possessed") + ')';

                        int intHalfMAGRoundedUp = CharacterObject.MAG.TotalValue.DivAwayFromZero(2);
                        ImprovementManager.CreateImprovement(objMerge, "BOD", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.Attribute, string.Empty, intHalfMAGRoundedUp, 1, 0, 0,
                            intHalfMAGRoundedUp, intHalfMAGRoundedUp);
                        ImprovementManager.CreateImprovement(objMerge, "AGI", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.Attribute, string.Empty, intHalfMAGRoundedUp, 1, 0, 0,
                            intHalfMAGRoundedUp, intHalfMAGRoundedUp);
                        ImprovementManager.CreateImprovement(objMerge, "STR", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.Attribute, string.Empty, intHalfMAGRoundedUp, 1, 0, 0,
                            intHalfMAGRoundedUp, intHalfMAGRoundedUp);
                        ImprovementManager.CreateImprovement(objMerge, "REA", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.Attribute, string.Empty, intHalfMAGRoundedUp, 1, 0, 0,
                            intHalfMAGRoundedUp, intHalfMAGRoundedUp);
                        ImprovementManager.CreateImprovement(objMerge, "INT", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.ReplaceAttribute, string.Empty, 0, 1, CharacterObject.INT.MetatypeMinimum,
                            CharacterObject.INT.MetatypeMaximum, 0, CharacterObject.INT.MetatypeAugmentedMaximum);
                        ImprovementManager.CreateImprovement(objMerge, "WIL", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.ReplaceAttribute, string.Empty, 0, 1, CharacterObject.WIL.MetatypeMinimum,
                            CharacterObject.WIL.MetatypeMaximum, 0, CharacterObject.WIL.MetatypeAugmentedMaximum);
                        ImprovementManager.CreateImprovement(objMerge, "LOG", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.ReplaceAttribute, string.Empty, 0, 1, CharacterObject.LOG.MetatypeMinimum,
                            CharacterObject.LOG.MetatypeMaximum, 0, CharacterObject.LOG.MetatypeAugmentedMaximum);
                        ImprovementManager.CreateImprovement(objMerge, "CHA", Improvement.ImprovementSource.Metatype, "Possession", Improvement.ImprovementType.ReplaceAttribute, string.Empty, 0, 1, CharacterObject.CHA.MetatypeMinimum,
                            CharacterObject.CHA.MetatypeMaximum, 0, CharacterObject.CHA.MetatypeAugmentedMaximum);
                        ImprovementManager.Commit(objMerge);
                        XmlDocument xmlPowerDoc = await CharacterObject.LoadDataAsync("critterpowers.xml");

                        // Update the Movement if the Vessel has one.
                        string strMovement = objSelected["movement"]?.InnerText;
                        if (!string.IsNullOrEmpty(strMovement))
                            objMerge.Movement = strMovement;

                        // Add any additional Critter Powers the Vessel grants.
                        XmlNode xmlPowersNode = objSelected["powers"];
                        if (xmlPowersNode != null)
                        {
                            using (XmlNodeList xmlPowerList = xmlPowersNode.SelectNodes("power"))
                            {
                                if (xmlPowerList?.Count > 0)
                                {
                                    foreach (XmlNode objXmlPower in xmlPowerList)
                                    {
                                        XmlNode objXmlCritterPower = xmlPowerDoc.SelectSingleNode("/chummer/powers/power[name = " + objXmlPower.InnerText.CleanXPath() + ']');
                                        CritterPower objPower = new CritterPower(objMerge);
                                        string strSelect = objXmlPower.Attributes?["select"]?.InnerText ?? string.Empty;
                                        int intRating = Convert.ToInt32(objXmlPower.Attributes?["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                                        objPower.Create(objXmlCritterPower, intRating, strSelect);

                                        objMerge.CritterPowers.Add(objPower);
                                    }
                                }
                            }
                        }

                        // Give the Critter the Immunity to Normal Weapons Power if they don't already have it.
                        if (!objMerge.CritterPowers.Any(objCritterPower => objCritterPower.Name == "Immunity" && objCritterPower.Extra == "Normal Weapons"))
                        {
                            XmlNode objPower = xmlPowerDoc.SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                            CritterPower objCritterPower = new CritterPower(objMerge);
                            objCritterPower.Create(objPower, 0, "Normal Weapons");
                            objMerge.CritterPowers.Add(objCritterPower);
                        }

                        // Add any Improvements the Vessel grants.
                        if (objSelected["bonus"] != null)
                        {
                            ImprovementManager.CreateImprovements(objMerge, Improvement.ImprovementSource.Metatype, strSelectedVessel, objSelected["bonus"], 1, strSelectedVessel);
                        }
                    }

                    // Now that everything is done, save the merged character and open them.
                    string strShowFileName = objMerge.FileName.SplitNoAlloc(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                    if (string.IsNullOrEmpty(strShowFileName))
                        strShowFileName = objMerge.CharacterName;
                    strShowFileName = strShowFileName.TrimEndOnce(".chum5");

                    strShowFileName += await LanguageManager.GetStringAsync("String_Space") + '(' + await LanguageManager.GetStringAsync("String_Possessed") + ')';
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' + await LanguageManager.GetStringAsync("DialogFilter_All"),
                        FileName = strShowFileName
                    })
                    {
                        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        using (LoadingBar frmProgressBar = ChummerMainForm.CreateAndShowProgressBar())
                        {
                            frmProgressBar.PerformStep(objMerge.CharacterName, LoadingBar.ProgressBarTextPatterns.Saving);
                            objMerge.FileName = saveFileDialog.FileName;
                            if (objMerge.Save())
                            {
                                // Get the name of the file and destroy the references to the Vessel and the merged character.
                                strOpenFile = objMerge.FileName;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(strOpenFile))
            {
                using (new CursorWait(this))
                {
                    Character objOpenCharacter = await Program.MainForm.LoadCharacterAsync(strOpenFile);
                    Program.MainForm.OpenCharacter(objOpenCharacter);
                }
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

        private async void mnuSpecialConvertToFreeSprite_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("critterpowers.xml");
            XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"Denial\"]");
            CritterPower objPower = new CritterPower(CharacterObject);
            objPower.Create(objXmlPower);
            objPower.CountTowardsLimit = false;
            if (objPower.InternalId.IsEmptyGuid())
                return;

            CharacterObject.CritterPowers.Add(objPower);

            CharacterObject.MetatypeCategory = "Free Sprite";
        }

        private async void mnuSpecialAddCyberwareSuite_Click(object sender, EventArgs e)
        {
            await AddCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private async void mnuSpecialAddBiowareSuite_Click(object sender, EventArgs e)
        {
            await AddCyberwareSuite(Improvement.ImprovementSource.Bioware);
        }

        #endregion Menu Events

        #region Martial Tab Control Events

        private void treMartialArts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IsRefreshing = true;
            if (treMartialArts.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblMartialArtSourceLabel.Visible = true;
                lblMartialArtSource.Visible = true;
                objSelected.SetSourceDetail(lblMartialArtSource);
            }
            else
            {
                lblMartialArtSourceLabel.Visible = false;
                lblMartialArtSource.Visible = false;
            }
            switch (treMartialArts.SelectedNode?.Tag)
            {
                case MartialArt objMartialArt:
                    cmdDeleteMartialArt.Enabled = !objMartialArt.IsQuality;
                    break;

                case ICanRemove _:
                    cmdDeleteMartialArt.Enabled = true;
                    break;

                default:
                    cmdDeleteMartialArt.Enabled = false;
                    lblMartialArtSource.Text = string.Empty;
                    lblMartialArtSource.SetToolTip(string.Empty);
                    break;
            }
            IsRefreshing = false;
        }

        #endregion Martial Tab Control Events

        #region Button Events

        private void panContacts_DragDrop(object sender, DragEventArgs e)
        {
            Point mousePosition = panContacts.PointToClient(new Point(e.X, e.Y));
            Control destination = panContacts.GetChildAtPoint(mousePosition);

            if (destination != null)
            {
                TransportWrapper wrapper = (TransportWrapper)e.Data.GetData(typeof(TransportWrapper));
                Control source = wrapper.Control;
                int indexDestination = panContacts.Controls.IndexOf(destination);
                if (panContacts.Controls.IndexOf(source) < indexDestination)
                    indexDestination--;

                panContacts.Controls.SetChildIndex(source, indexDestination);
            }

            foreach (ContactControl objControl in panContacts.Controls)
            {
                objControl.BackColor = ColorManager.Control;
            }
        }

        private void panContacts_DragOver(object sender, DragEventArgs e)
        {
            Point mousePosition = panContacts.PointToClient(new Point(e.X, e.Y));
            Control destination = panContacts.GetChildAtPoint(mousePosition);

            if (destination == null)
                return;

            destination.BackColor = ColorManager.ControlDarker;
            foreach (ContactControl objControl in panContacts.Controls)
            {
                if (objControl != destination as ContactControl)
                {
                    objControl.BackColor = ColorManager.Control;
                }
            }
            // Highlight the Node that we're currently dragging over, provided it is of the same level or higher.
        }

        private void panContacts_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private async void cmdAddSpell_Click(object sender, EventArgs e)
        {
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("spells.xml");
            bool blnAddAgain;

            do
            {
                (bool blnCanTouchOnlySpellBeFree, bool blnCanGenericSpellBeFree) = CharacterObject.AllowFreeSpells;
                int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Spells");
                // Make sure the character has enough Karma before letting them select a Spell.
                if (CharacterObject.Karma < intSpellKarmaCost && !(blnCanTouchOnlySpellBeFree || blnCanGenericSpellBeFree))
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"),
                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                }

                using (SelectSpell frmPickSpell = new SelectSpell(CharacterObject)
                {
                    FreeOnly = CharacterObject.Karma < intSpellKarmaCost &&
                               (blnCanTouchOnlySpellBeFree || blnCanGenericSpellBeFree)
                })
                {
                    await frmPickSpell.ShowDialogSafeAsync(this);
                    // Make sure the dialogue window was not canceled.
                    if (frmPickSpell.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickSpell.AddAgain;

                    XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = " + frmPickSpell.SelectedSpell.CleanXPath() + ']');

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
                        objSpell.Dispose();
                        continue;
                    }

                    objSpell.FreeBonus = frmPickSpell.FreeBonus;
                    if (!objSpell.FreeBonus)
                    {
                        if (CharacterObject.Karma < intSpellKarmaCost)
                        {
                            objSpell.Dispose();
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        }
                        if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                            , objSpell.CurrentDisplayName
                            , intSpellKarmaCost.ToString(GlobalSettings.CultureInfo))))
                        {
                            objSpell.Dispose();
                            continue;
                        }
                    }
                    // Barehanded Adept
                    else if (CharacterObject.AdeptEnabled && !CharacterObject.MagicianEnabled && (objSpell.Range == "T" || objSpell.Range == "T (A)"))
                    {
                        objSpell.BarehandedAdept = true;
                    }

                    CharacterObject.Spells.Add(objSpell);
                    if (!objSpell.FreeBonus)
                    {
                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(-intSpellKarmaCost, await LanguageManager.GetStringAsync("String_ExpenseLearnSpell") + await LanguageManager.GetStringAsync("String_Space") + objSpell.Name, ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= intSpellKarmaCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddSpell, objSpell.InternalId);
                        objExpense.Undo = objUndo;
                    }
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteSpell_Click(object sender, EventArgs e)
        {
            // Locate the Spell that is selected in the tree.
            if (!(treSpells.SelectedNode?.Tag is Spell objSpell))
                return;
            // Spells that come from Initiation Grades can't be deleted normally.
            if (objSpell.Grade != 0)
                return;
            objSpell.Remove();
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

        private async void cmdAddCyberware_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickCyberware(null, Improvement.ImprovementSource.Cyberware);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCyberware_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode == null || treCyberware.SelectedNode.Level <= 0)
                return;
            // Locate the piece of Cyberware that is selected in the tree.
            if (!(treCyberware.SelectedNode?.Tag is ICanRemove objToRemove))
                return;
            objToRemove.Remove();
        }

        private async void cmdAddComplexForm_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("complexforms.xml");
            bool blnAddAgain;

            do
            {
                // The number of Complex Forms cannot exceed twice the character's RES.
                if (CharacterObject.ComplexForms.Count >= CharacterObject.RES.Value * 2 + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.ComplexFormLimit) && !CharacterObjectSettings.IgnoreComplexFormLimit)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ComplexFormLimit"), await LanguageManager.GetStringAsync("MessageTitle_ComplexFormLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                int intComplexFormKarmaCost = CharacterObject.ComplexFormKarmaCost;

                // Make sure the character has enough Karma before letting them select a Complex Form.
                if (CharacterObject.Karma < intComplexFormKarmaCost)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }

                XmlNode objXmlComplexForm;
                // Let the user select a Program.
                using (SelectComplexForm frmPickComplexForm = new SelectComplexForm(CharacterObject))
                {
                    await frmPickComplexForm.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickComplexForm.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickComplexForm.AddAgain;

                    objXmlComplexForm = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[id = " + frmPickComplexForm.SelectedComplexForm.CleanXPath() + ']');
                }

                if (objXmlComplexForm == null)
                    continue;
                ComplexForm objComplexForm = new ComplexForm(CharacterObject);
                objComplexForm.Create(objXmlComplexForm);
                if (objComplexForm.InternalId.IsEmptyGuid())
                    continue;

                CharacterObject.ComplexForms.Add(objComplexForm);

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend"), objComplexForm.DisplayNameShort(GlobalSettings.Language), intComplexFormKarmaCost.ToString(GlobalSettings.CultureInfo))))
                {
                    // Remove the Improvements created by the Complex Form.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId);
                    continue;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intComplexFormKarmaCost * -1, await LanguageManager.GetStringAsync("String_ExpenseLearnComplexForm") + await LanguageManager.GetStringAsync("String_Space") + objComplexForm.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intComplexFormKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddComplexForm, objComplexForm.InternalId);
                objExpense.Undo = objUndo;
            }
            while (blnAddAgain);
        }

        private async void cmdAddArmor_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickArmor();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteArmor_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treArmor.SelectedNode?.Tag);
        }

        private void cmdDeleteCustomDrug_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treCustomDrugs.SelectedNode?.Tag);
        }

        private async void cmdAddBioware_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickCyberware(null, Improvement.ImprovementSource.Bioware);
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> PickWeapon(object destObject)
        {
            using (SelectWeapon frmPickWeapon = new SelectWeapon(CharacterObject))
            {
                await frmPickWeapon.ShowDialogSafeAsync(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Weapons XML file and locate the selected piece.
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

                XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                List<Weapon> lstWeapons = new List<Weapon>(1);
                Weapon objWeapon = new Weapon(CharacterObject);
                objWeapon.Create(objXmlWeapon, lstWeapons);
                objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;

                decimal decCost = objWeapon.TotalCost;
                // Apply a markup if applicable.
                if (frmPickWeapon.Markup != 0)
                {
                    decCost *= 1 + frmPickWeapon.Markup / 100.0m;
                }

                // Multiply the cost if applicable.
                char chrAvail = objWeapon.TotalAvailTuple().Suffix;
                switch (chrAvail)
                {
                    case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                        decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                        break;

                    case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                        decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                        break;
                }

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickWeapon.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return frmPickWeapon.AddAgain;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeapon") + await LanguageManager.GetStringAsync("String_Space") + objWeapon.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                        DateTime.Now);
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

                return frmPickWeapon.AddAgain;
            }
        }

        private async void cmdAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickWeapon(string.Empty);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteWeapon_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            RemoveSelectedObject(treWeapons.SelectedNode?.Tag);
        }

        private void RemoveSelectedObject(object selectedObject)
        {
            if (!(selectedObject is ICanRemove iRemovable))
                return;
            iRemovable.Remove();
        }

        private async void cmdAddLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            do
            {
                using (SelectLifestyle frmPickLifestyle = new SelectLifestyle(CharacterObject))
                {
                    await frmPickLifestyle.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    {
                        frmPickLifestyle.SelectedLifestyle.Dispose();
                        return;
                    }

                    blnAddAgain = frmPickLifestyle.AddAgain;
                    Lifestyle objLifestyle = frmPickLifestyle.SelectedLifestyle;
                    objLifestyle.Increments = 0;
                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteLifestyle_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treLifestyles.SelectedNode?.Tag);
        }

        private async void cmdAddGear_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickGear(null, treGear.SelectedNode?.Tag as Location);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteGear_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treGear.SelectedNode?.Tag);
        }

        private async ValueTask<bool> AddVehicle(Location objLocation = null)
        {
            using (SelectVehicle frmPickVehicle = new SelectVehicle(CharacterObject))
            {
                await frmPickVehicle.ShowDialogSafeAsync(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickVehicle.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Vehicles XML file and locate the selected piece.
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("vehicles.xml");

                XmlNode objXmlVehicle = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = " + frmPickVehicle.SelectedVehicle.CleanXPath() + ']');
                Vehicle objVehicle = new Vehicle(CharacterObject);
                objVehicle.Create(objXmlVehicle);
                // Update the Used Vehicle information if applicable.
                if (frmPickVehicle.UsedVehicle)
                {
                    objVehicle.Avail = frmPickVehicle.UsedAvail;
                    objVehicle.Cost = frmPickVehicle.UsedCost.ToString(GlobalSettings.InvariantCultureInfo);
                }

                objVehicle.DiscountCost = frmPickVehicle.BlackMarketDiscount;

                decimal decCost = objVehicle.TotalCost;
                // Apply a markup if applicable.
                if (frmPickVehicle.Markup != 0)
                {
                    decCost *= 1 + frmPickVehicle.Markup / 100.0m;
                }

                // Multiply the cost if applicable.
                char chrAvail = objVehicle.TotalAvailTuple().Suffix;
                switch (chrAvail)
                {
                    case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                        decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                        break;

                    case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                        decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                        break;
                }

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickVehicle.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return frmPickVehicle.AddAgain;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicle") + await LanguageManager.GetStringAsync("String_Space") + objVehicle.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                        DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicle, objVehicle.InternalId);
                    objExpense.Undo = objUndo;
                }

                objVehicle.DiscountCost = frmPickVehicle.BlackMarketDiscount;

                //objVehicle.Location = objLocation;
                objLocation?.Children.Add(objVehicle);
                CharacterObject.Vehicles.Add(objVehicle);

                return frmPickVehicle.AddAgain;
            }
        }

        private async void cmdAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddVehicle(treVehicles.SelectedNode?.Tag as Location);
            }
            while (blnAddAgain);
        }

        private async void cmdDeleteVehicle_Click(object sender, EventArgs e)
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

            switch (treVehicles.SelectedNode?.Tag)
            {
                // If this is the Obsolete Mod, the user must select a percentage. This will create an Expense that costs X% of the Vehicle's base cost to remove the special Obsolete Mod.
                case VehicleMod objMod when objMod.Name == "Obsolete" || objMod.Name == "Obsolescent" && CharacterObjectSettings.AllowObsolescentUpgrade:
                {
                    using (SelectNumber frmModPercent = new SelectNumber
                           {
                               Minimum = 0,
                               Maximum = 1000000,
                               Description = await LanguageManager.GetStringAsync("String_Retrofit")
                           })
                    {
                        await frmModPercent.ShowDialogSafeAsync(this);

                        if (frmModPercent.DialogResult == DialogResult.Cancel)
                            return;

                        decimal decPercentage = frmModPercent.SelectedValue;

                        decimal decVehicleCost = objMod.Parent.OwnCost;

                        // Make sure the character has enough Nuyen for the expense.
                        decimal decCost = decVehicleCost * decPercentage / 100;

                        // Create a Vehicle Mod for the Retrofit.
                        VehicleMod objRetrofit = new VehicleMod(CharacterObject);

                        XmlDocument objVehiclesDoc = await CharacterObject.LoadDataAsync("vehicles.xml");
                        XmlNode objXmlNode = objVehiclesDoc.SelectSingleNode("/chummer/mods/mod[name = \"Retrofit\"]");
                        objRetrofit.Create(objXmlNode, 0, objMod.Parent);
                        objRetrofit.Cost = decCost.ToString(GlobalSettings.InvariantCultureInfo);
                        objRetrofit.IncludedInVehicle = true;
                        objMod.Parent.Mods.Add(objRetrofit);

                        // Create an Expense Log Entry for removing the Obsolete Mod.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_ExpenseVehicleRetrofit"), objMod.Parent.CurrentDisplayName), ExpenseType.Nuyen, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                        // Adjust the character's Nuyen total.
                        CharacterObject.Nuyen += decCost * -1;
                    }

                    break;
                }
                case ICanRemove selectedObject:
                    selectedObject.Remove();
                    break;
            }
        }

        private void cmdAddMartialArt_Click(object sender, EventArgs e)
        {
            MartialArt.Purchase(CharacterObject);
        }

        private void cmdDeleteMartialArt_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treMartialArts.SelectedNode?.Tag);
        }

        private async void cmdAddMugshot_Click(object sender, EventArgs e)
        {
            if (!await AddMugshot())
                return;
            lblNumMugshots.Text = await LanguageManager.GetStringAsync("String_Of") + CharacterObject.Mugshots.Count.ToString(GlobalSettings.CultureInfo);
            ++nudMugshotIndex.Maximum;
            nudMugshotIndex.Value = CharacterObject.Mugshots.Count;

            IsDirty = true;
        }

        private void cmdDeleteMugshot_Click(object sender, EventArgs e)
        {
            if (CharacterObject.Mugshots.Count == 0)
                return;
            RemoveMugshot(nudMugshotIndex.ValueAsInt - 1);

            lblNumMugshots.Text = LanguageManager.GetString("String_Of") + CharacterObject.Mugshots.Count.ToString(GlobalSettings.CultureInfo);
            --nudMugshotIndex.Maximum;
            if (nudMugshotIndex.Value > nudMugshotIndex.Maximum)
                nudMugshotIndex.Value = nudMugshotIndex.Maximum;
            else
            {
                if (nudMugshotIndex.ValueAsInt - 1 == CharacterObject.MainMugshotIndex)
                    chkIsMainMugshot.Checked = true;
                else if (chkIsMainMugshot.Checked)
                    chkIsMainMugshot.Checked = false;

                UpdateMugshot(picMugshot, nudMugshotIndex.ValueAsInt - 1);
            }

            IsDirty = true;
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

            if (nudMugshotIndex.ValueAsInt - 1 == CharacterObject.MainMugshotIndex)
                chkIsMainMugshot.Checked = true;
            else if (chkIsMainMugshot.Checked)
                chkIsMainMugshot.Checked = false;

            UpdateMugshot(picMugshot, nudMugshotIndex.ValueAsInt - 1);
        }

        private void chkIsMainMugshot_CheckedChanged(object sender, EventArgs e)
        {
            bool blnStatusChanged = false;
            switch (chkIsMainMugshot.Checked)
            {
                case true when CharacterObject.MainMugshotIndex != nudMugshotIndex.ValueAsInt - 1:
                    CharacterObject.MainMugshotIndex = nudMugshotIndex.ValueAsInt - 1;
                    blnStatusChanged = true;
                    break;

                case false when nudMugshotIndex.ValueAsInt - 1 == CharacterObject.MainMugshotIndex:
                    CharacterObject.MainMugshotIndex = -1;
                    blnStatusChanged = true;
                    break;
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
                    CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.InitiateGrade + 1 > CharacterObject.MAGAdept.TotalValue)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_CannotIncreaseInitiateGrade"), LanguageManager.GetString("MessageTitle_CannotIncreaseInitiateGrade"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Make sure the character has enough Karma.
                decimal decMultiplier = 1.0m;
                if (chkInitiationGroup.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationGroupPercent;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationOrdealPercent;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationSchoolingPercent;

                int intKarmaExpense = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                if (intKarmaExpense > CharacterObject.Karma)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (chkInitiationSchooling.Checked)
                {
                    if (10000 > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughNuyen"), LanguageManager.GetString("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaandNuyenExpense")
                        , LanguageManager.GetString("String_InitiateGrade")
                        , (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo)
                        , intKarmaExpense.ToString(GlobalSettings.CultureInfo)
                        , 10000.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥')))
                        return;
                }
                else if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense")
                    , LanguageManager.GetString("String_InitiateGrade")
                    , (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intKarmaExpense.ToString(GlobalSettings.CultureInfo))))
                    return;

                string strSpace = LanguageManager.GetString("String_Space");

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseInitiateGrade")
                                                        + strSpace + CharacterObject.InitiateGrade.ToString(GlobalSettings.CultureInfo)
                                                        + strSpace + "->" + strSpace
                                                        + (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.InitiateGrade + 1, false, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ImproveInitiateGrade, objGrade.InternalId);
                objExpense.Undo = objUndo;

                if (chkInitiationSchooling.Checked)
                {
                    ExpenseLogEntry objNuyenExpense = new ExpenseLogEntry(CharacterObject);
                    objNuyenExpense.Create(-10000, LanguageManager.GetString("String_ExpenseInitiateGrade")
                                                   + strSpace + CharacterObject.InitiateGrade.ToString(GlobalSettings.CultureInfo)
                                                   + strSpace + "->" + strSpace
                                                   + (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objNuyenExpense);
                    CharacterObject.Nuyen -= 10000;

                    ExpenseUndo objNuyenUndo = new ExpenseUndo();
                    objNuyenUndo.CreateNuyen(NuyenExpenseType.ImproveInitiateGrade, objGrade.InternalId, 10000);
                    objNuyenExpense.Undo = objNuyenUndo;
                }

                int intAmount = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                string strInitTip = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveInitiateGrade")
                    , (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intAmount.ToString(GlobalSettings.CultureInfo));
                cmdAddMetamagic.SetToolTip(strInitTip);
            }
            else if (CharacterObject.RESEnabled)
            {
                // Make sure that the Initiate Grade is not attempting to go above the character's RES CharacterAttribute.
                if (CharacterObject.SubmersionGrade + 1 > CharacterObject.RES.TotalValue)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_CannotIncreaseSubmersionGrade"), LanguageManager.GetString("MessageTitle_CannotIncreaseSubmersionGrade"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Make sure the character has enough Karma.
                decimal decMultiplier = 1.0m;
                if (chkInitiationGroup.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationGroupPercent;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationOrdealPercent;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationSchoolingPercent;

                int intKarmaExpense = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                if (intKarmaExpense > CharacterObject.Karma)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpense")
                    , LanguageManager.GetString("String_SubmersionGrade")
                    , (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intKarmaExpense.ToString(GlobalSettings.CultureInfo))))
                    return;

                string strSpace = LanguageManager.GetString("String_Space");

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseSubmersionGrade")
                                                        + strSpace + CharacterObject.SubmersionGrade.ToString(GlobalSettings.CultureInfo)
                                                        + strSpace + "->" + strSpace
                                                        + (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.SubmersionGrade + 1, true, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ImproveInitiateGrade, objGrade.InternalId);
                objExpense.Undo = objUndo;

                int intAmount = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                string strInitTip = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveSubmersionGrade")
                    , (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intAmount.ToString(GlobalSettings.CultureInfo));
                cmdAddMetamagic.SetToolTip(strInitTip);
            }
        }

        private void cmdDeleteMetamagic_Click(object sender, EventArgs e)
        {
            RemoveSelectedObject(treMetamagic.SelectedNode?.Tag);
        }

        private async void cmdKarmaGained_Click(object sender, EventArgs e)
        {
            using (CreateExpense frmNewExpense = new CreateExpense(CharacterObjectSettings)
            {
                KarmaNuyenExchangeString = await LanguageManager.GetStringAsync("String_WorkingForThePeople")
            })
            {
                await frmNewExpense.ShowDialogSafeAsync(this);

                if (frmNewExpense.DialogResult == DialogResult.Cancel)
                    return;

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(frmNewExpense.Amount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
                objExpense.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma += frmNewExpense.Amount.ToInt32();

                if (frmNewExpense.KarmaNuyenExchange)
                {
                    // Create the Expense Log Entry.
                    objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(frmNewExpense.Amount * -CharacterObjectSettings.NuyenPerBPWftP, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                    objExpense.ForceCareerVisible = frmNewExpense.ForceCareerVisible;
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Nuyen total.
                    CharacterObject.Nuyen += frmNewExpense.Amount * -CharacterObjectSettings.NuyenPerBPWftP;
                }
            }
        }

        private async void cmdKarmaSpent_Click(object sender, EventArgs e)
        {
            using (CreateExpense frmNewExpense = new CreateExpense(CharacterObjectSettings)
            {
                KarmaNuyenExchangeString = await LanguageManager.GetStringAsync("String_WorkingForTheMan")
            })
            {
                await frmNewExpense.ShowDialogSafeAsync(this);

                if (frmNewExpense.DialogResult == DialogResult.Cancel)
                    return;

                // Make sure the Karma expense would not put the character's remaining Karma amount below 0.
                if (CharacterObject.Karma - frmNewExpense.Amount < 0)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(frmNewExpense.Amount * -1, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                objExpense.ForceCareerVisible = frmNewExpense.ForceCareerVisible;
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
                objExpense.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= frmNewExpense.Amount.ToInt32();

                if (frmNewExpense.KarmaNuyenExchange)
                {
                    // Create the Expense Log Entry.
                    objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(frmNewExpense.Amount * CharacterObjectSettings.NuyenPerBPWftM, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                    objExpense.ForceCareerVisible = frmNewExpense.ForceCareerVisible;
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Nuyen total.
                    CharacterObject.Nuyen += frmNewExpense.Amount * CharacterObjectSettings.NuyenPerBPWftM;
                }
            }
        }

        private async void cmdNuyenGained_Click(object sender, EventArgs e)
        {
            using (CreateExpense frmNewExpense = new CreateExpense(CharacterObjectSettings)
            {
                Mode = ExpenseType.Nuyen,
                KarmaNuyenExchangeString = await LanguageManager.GetStringAsync("String_WorkingForTheMan")
            })
            {
                await frmNewExpense.ShowDialogSafeAsync(this);

                if (frmNewExpense.DialogResult == DialogResult.Cancel)
                    return;

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(frmNewExpense.Amount, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                objExpense.Refund = frmNewExpense.Refund;
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.ManualAdd, string.Empty);
                objExpense.Undo = objUndo;

                // Adjust the character's Nuyen total.
                CharacterObject.Nuyen += frmNewExpense.Amount;

                if (frmNewExpense.KarmaNuyenExchange)
                {
                    // Create the Expense Log Entry.
                    objExpense = new ExpenseLogEntry(CharacterObject);
                    int intAmount = (frmNewExpense.Amount / CharacterObjectSettings.NuyenPerBPWftM).ToInt32();
                    objExpense.Create(-intAmount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                    objExpense.ForceCareerVisible = frmNewExpense.ForceCareerVisible;
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Karma total.
                    CharacterObject.Karma -= intAmount;
                }
            }
        }

        private async void cmdNuyenSpent_Click(object sender, EventArgs e)
        {
            using (CreateExpense frmNewExpense = new CreateExpense(CharacterObjectSettings)
            {
                Mode = ExpenseType.Nuyen,
                KarmaNuyenExchangeString = await LanguageManager.GetStringAsync("String_WorkingForThePeople")
            })
            {
                await frmNewExpense.ShowDialogSafeAsync(this);

                if (frmNewExpense.DialogResult == DialogResult.Cancel)
                    return;

                // Make sure the Nuyen expense would not put the character's remaining Nuyen amount below 0.
                if (CharacterObject.Nuyen - frmNewExpense.Amount < 0)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(frmNewExpense.Amount * -1, frmNewExpense.Reason, ExpenseType.Nuyen, frmNewExpense.SelectedDate);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.ManualSubtract, string.Empty);
                objExpense.Undo = objUndo;

                // Adjust the character's Nuyen total.
                CharacterObject.Nuyen += frmNewExpense.Amount * -1;

                if (frmNewExpense.KarmaNuyenExchange)
                {
                    // Create the Expense Log Entry.
                    objExpense = new ExpenseLogEntry(CharacterObject);
                    int intAmount = (frmNewExpense.Amount / CharacterObjectSettings.NuyenPerBPWftP).ToInt32();
                    objExpense.Create(intAmount, frmNewExpense.Reason, ExpenseType.Karma, frmNewExpense.SelectedDate, frmNewExpense.Refund);
                    objExpense.ForceCareerVisible = frmNewExpense.ForceCareerVisible;
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.ManualSubtract, string.Empty);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Karma total.
                    CharacterObject.Karma += intAmount;
                }
            }
        }

        private void cmdDecreaseLifestyleMonths_Click(object sender, EventArgs e)
        {
            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(0, LanguageManager.GetString("String_ExpenseDecreaseLifestyle") + LanguageManager.GetString("String_Space") + objLifestyle.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);

            --objLifestyle.Increments;
            lblLifestyleMonths.Text = objLifestyle.Increments.ToString(GlobalSettings.CultureInfo);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdIncreaseLifestyleMonths_Click(object sender, EventArgs e)
        {
            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;
            objLifestyle.IncrementMonths();
            lblLifestyleMonths.Text = objLifestyle.Increments.ToString(GlobalSettings.CultureInfo);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void cmdAddCritterPower_Click(object sender, EventArgs e)
        {
            // Make sure the Critter is allowed to have Optional Powers.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("critterpowers.xml");

            bool blnAddAgain;
            do
            {
                using (SelectCritterPower frmPickCritterPower = new SelectCritterPower(CharacterObject))
                {
                    await frmPickCritterPower.ShowDialogSafeAsync(this);

                    if (frmPickCritterPower.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickCritterPower.AddAgain;

                    XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[id = " + frmPickCritterPower.SelectedPower.CleanXPath() + ']');
                    CritterPower objPower = new CritterPower(CharacterObject);
                    objPower.Create(objXmlPower, frmPickCritterPower.SelectedRating);
                    objPower.PowerPoints = frmPickCritterPower.PowerPoints;
                    if (objPower.InternalId.IsEmptyGuid())
                        continue;

                    if (objPower.Karma > CharacterObject.Karma)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }

                    if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                        , objPower.CurrentDisplayName
                        , objPower.Karma.ToString(GlobalSettings.CultureInfo))))
                        continue;

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(objPower.Karma * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCritterPower") + await LanguageManager.GetStringAsync("String_Space") + objPower.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma,
                        DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddCritterPower, objPower.InternalId);
                    objExpense.Undo = objUndo;

                    CharacterObject.Karma -= objPower.Karma;
                    CharacterObject.CritterPowers.Add(objPower);
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCritterPower_Click(object sender, EventArgs e)
        {
            // If the selected object is not a complex form or it comes from an initiate grade, we don't want to remove it.
            if (!(treCritterPowers.SelectedNode?.Tag is CritterPower objCritterPower) || objCritterPower.Grade != 0)
                return;
            objCritterPower.Remove();
        }

        private void cmdDeleteComplexForm_Click(object sender, EventArgs e)
        {
            // If the selected object is not a complex form or it comes from an initiate grade, we don't want to remove it.
            if (!(treComplexForms.SelectedNode?.Tag is ComplexForm objComplexForm) || objComplexForm.Grade != 0)
                return;
            objComplexForm.Remove();
        }

        private async void cmdGearReduceQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear)) return;

            int intDecimalPlaces = 0;
            if (objGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
            {
                intDecimalPlaces = CharacterObjectSettings.MaxNuyenDecimals;
            }
            else if (objGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
            }

            decimal decSelectedValue;
            using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
            {
                Minimum = 0,
                Maximum = objGear.Quantity,
                Description = await LanguageManager.GetStringAsync("String_ReduceGear")
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                decSelectedValue = frmPickNumber.SelectedValue;
            }

            if (!CommonFunctions.ConfirmDelete(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ReduceQty"), decSelectedValue.ToString(GlobalSettings.CultureInfo))))
                return;

            objGear.Quantity -= decSelectedValue;

            if (objGear.Quantity > 0)
            {
                objSelectedNode.Text = objGear.CurrentDisplayName;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            else
            {
                // Remove any Weapons that came with it.
                objGear.DeleteGear();
            }
        }

        private async void cmdGearSplitQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objSelectedGear))
                return;

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectSettings.MaxNuyenDecimals);
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
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotSplitGear"), await LanguageManager.GetStringAsync("MessageTitle_CannotSplitGear"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
            {
                Minimum = decMinimumAmount,
                Maximum = objSelectedGear.Quantity - decMinimumAmount,
                Description = await LanguageManager.GetStringAsync("String_SplitGear")
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

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
                objSelectedNode.Text = objSelectedGear.CurrentDisplayName;

                CharacterObject.Gear.Add(objGear);
            }
        }

        private async void cmdGearMergeQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear))
                return;
            List<Gear> lstGear = new List<Gear>(CharacterObject.Gear.Count);

            foreach (Gear objCharacterGear in CharacterObject.Gear)
            {
                if (objCharacterGear.InternalId != objGear.InternalId
                    && objCharacterGear.IsIdenticalToOtherGear(objGear, true))
                    lstGear.Add(objCharacterGear);
            }

            // If there were no matches, don't try to merge anything.
            if (lstGear.Count == 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotMergeGear"), await LanguageManager.GetStringAsync("MessageTitle_CannotMergeGear"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Gear objSelectedGear;
            // Show the Select Item window.
            using (SelectItem frmPickItem = new SelectItem())
            {
                frmPickItem.SetGearMode(lstGear);
                await frmPickItem.ShowDialogSafeAsync(this);

                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    return;

                objSelectedGear = CharacterObject.Gear.DeepFindById(frmPickItem.SelectedItem);
            }

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectSettings.MaxNuyenDecimals);
                // Need a for loop instead of a power system to maintain exact precision
                for (int i = 0; i < intDecimalPlaces; ++i)
                    decMinimumAmount /= 10.0m;
            }
            else if (objSelectedGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
                decMinimumAmount = 0.01m;
            }

            using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
            {
                Minimum = decMinimumAmount,
                Maximum = objGear.Quantity,
                Description = await LanguageManager.GetStringAsync("String_MergeGear")
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                // Increase the quantity for the selected item.
                objSelectedGear.Quantity += frmPickNumber.SelectedValue;
                // Located the item in the Tree and update its display information.
                TreeNode objNode = treGear.FindNode(objSelectedGear.InternalId);
                if (objNode != null)
                    objNode.Text = objSelectedGear.CurrentDisplayName;

                // Reduce the quantity for the selected item.
                objGear.Quantity -= frmPickNumber.SelectedValue;
            }

            // If the quantity has reached 0, delete the item and any Weapons it created.
            if (objGear.Quantity <= 0)
            {
                // Remove the Gear if its quantity has been reduced to 0.
                objGear.DeleteGear();
            }
            else
            {
                objSelectedNode.Text = objGear.CurrentDisplayName;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private async void cmdGearMoveToVehicle_Click(object sender, EventArgs e)
        {
            Vehicle objVehicle;
            using (SelectItem frmPickItem = new SelectItem())
            {
                frmPickItem.SetVehiclesMode(CharacterObject.Vehicles);
                await frmPickItem.ShowDialogSafeAsync(this);

                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    return;

                // Locate the selected Vehicle.
                objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == frmPickItem.SelectedItem);
            }

            if (objVehicle == null)
                return;

            TreeNode objSelectedNode = treGear.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objSelectedGear))
                return;

            decimal decMinimumAmount = 1.0m;
            int intDecimalPlaces = 0;
            if (objSelectedGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectSettings.MaxNuyenDecimals);
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
                using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
                {
                    Minimum = decMinimumAmount,
                    Maximum = objSelectedGear.Quantity,
                    Description = await LanguageManager.GetStringAsync("String_MoveGear")
                })
                {
                    await frmPickNumber.ShowDialogSafeAsync(this);

                    if (frmPickNumber.DialogResult == DialogResult.Cancel)
                        return;

                    decMove = frmPickNumber.SelectedValue;
                }
            }

            // See if the Vehicle already has a matching piece of Gear.
            Gear objFoundGear = objVehicle.GearChildren.FirstOrDefault(x => x.IsIdenticalToOtherGear(objSelectedGear));

            if (objFoundGear == null)
            {
                // Create a new piece of Gear.
                Gear objGear = new Gear(CharacterObject);

                objGear.Copy(objSelectedGear);

                objGear.Quantity = decMove;
                objGear.Location = null;

                objGear.Parent = objVehicle;
                objVehicle.GearChildren.Add(objGear);
            }
            else
            {
                // Everything matches up, so just increase the quantity.
                objFoundGear.Quantity += decMove;
                TreeNode objFoundNode = treVehicles.FindNode(objFoundGear.InternalId);
                if (objFoundNode != null)
                    objFoundNode.Text = objFoundGear.CurrentDisplayName;
            }

            // Update the selected item.
            objSelectedGear.Quantity -= decMove;
            if (objSelectedGear.Quantity <= 0)
            {
                objSelectedGear.DeleteGear();
            }
            else
            {
                objSelectedNode.Text = objSelectedGear.CurrentDisplayName;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private async void cmdVehicleMoveToInventory_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            if (objSelectedNode == null)
                return;
            switch (objSelectedNode.Tag)
            {
                // Locate the selected Weapon.
                case Weapon objWeapon:
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
                        break;
                    }
                case Gear objSelectedGear:
                    {
                        // Locate the selected Gear.
                        CharacterObject.Vehicles.FindVehicleGear(objSelectedGear.InternalId);

                        decimal decMinimumAmount = 1.0m;
                        int intDecimalPlaces = 0;
                        if (objSelectedGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
                        {
                            intDecimalPlaces = Math.Max(0, CharacterObjectSettings.MaxNuyenDecimals);
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
                            using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
                            {
                                Minimum = decMinimumAmount,
                                Maximum = objSelectedGear.Quantity,
                                Description = await LanguageManager.GetStringAsync("String_MoveGear")
                            })
                            {
                                await frmPickNumber.ShowDialogSafeAsync(this);

                                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                                    return;

                                decMove = frmPickNumber.SelectedValue;
                            }
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

                            objGear.AddGearImprovements();
                        }
                        else
                        {
                            // Everything matches up, so just increase the quantity.
                            objFoundGear.Quantity += decMove;
                            TreeNode objFoundNode = treGear.FindNode(objFoundGear.InternalId);
                            if (objFoundNode != null)
                                objFoundNode.Text = objFoundGear.CurrentDisplayName;
                        }

                        // Update the selected item.
                        objSelectedGear.Quantity -= decMove;
                        if (objSelectedGear.Quantity <= 0)
                        {
                            objSelectedGear.DeleteGear();
                        }
                        else
                        {
                            objSelectedNode.Text = objSelectedGear.CurrentDisplayName;
                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }

                        break;
                    }
            }
        }

        private async void cmdGearIncreaseQty_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
                return;
            bool blnAddAgain;
            do
            {
                // Select the root Gear node then open the Select Gear window.
                blnAddAgain = await PickGear(objGear.Parent as IHasChildren<Gear>, objGear.Location, objGear, objGear.DisplayNameShort(GlobalSettings.Language));
            }
            while (blnAddAgain);
        }

        private async void cmdVehicleGearReduceQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            if (!(objSelectedNode?.Tag is Gear objGear))
                return;
            int intDecimalPlaces = 0;
            if (objGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
            {
                intDecimalPlaces = Math.Max(0, CharacterObjectSettings.MaxNuyenDecimals);
            }
            else if (objGear.Category == "Currency")
            {
                intDecimalPlaces = 2;
            }

            decimal decSelectedValue;
            using (SelectNumber frmPickNumber = new SelectNumber(intDecimalPlaces)
            {
                Minimum = 0,
                Maximum = objGear.Quantity,
                Description = await LanguageManager.GetStringAsync("String_ReduceGear")
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                decSelectedValue = frmPickNumber.SelectedValue;
            }

            if (!CommonFunctions.ConfirmDelete(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ReduceQty"), decSelectedValue.ToString(GlobalSettings.CultureInfo))))
                return;

            objGear.Quantity -= decSelectedValue;

            if (objGear.Quantity > 0)
            {
                objSelectedNode.Text = objGear.CurrentDisplayName;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            else
            {
                // Remove the Gear if its quantity has been reduced to 0.
                objGear.DeleteGear();
            }
        }

        private async void cmdAddQuality_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("qualities.xml");
            bool blnAddAgain;
            do
            {
                bool blnFreeCost;
                XmlNode objXmlQuality;
                int intRatingToAdd;
                using (SelectQuality frmPickQuality = new SelectQuality(CharacterObject))
                {
                    await frmPickQuality.ShowDialogSafeAsync(this);

                    // Don't do anything else if the form was canceled.
                    if (frmPickQuality.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickQuality.AddAgain;
                    blnFreeCost = frmPickQuality.FreeCost;

                    objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + frmPickQuality.SelectedQuality.CleanXPath() + ']');
                    intRatingToAdd = frmPickQuality.SelectedRating;
                    int intDummy = 0;
                    if (objXmlQuality != null && objXmlQuality["nolevels"] == null && objXmlQuality.TryGetInt32FieldQuickly("limit", ref intDummy))
                    {
                        intRatingToAdd -= CharacterObject.Qualities.Count(x =>
                            x.SourceIDString.Equals(frmPickQuality.SelectedQuality,
                                StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(x.SourceName));
                    }
                }

                if (objXmlQuality == null)
                    continue;

                QualityType eQualityType = QualityType.Positive;
                string strTemp = string.Empty;
                if (objXmlQuality.TryGetStringFieldQuickly("category", ref strTemp))
                    eQualityType = Quality.ConvertToQualityType(strTemp);

                for (int i = 1; i <= intRatingToAdd; ++i)
                {
                    // Positive Metagenetic Qualities are free if you're a Changeling.
                    if (CharacterObject.MetagenicLimit > 0 && objXmlQuality["metagenic"]?.InnerText == bool.TrueString)
                        blnFreeCost = true;
                    // The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
                    else if (objXmlQuality["name"]?.InnerText == "Mentor Spirit" &&
                             CharacterObject.Qualities.Any(x =>
                                 x.Name == "The Beast's Way" || x.Name == "The Spiritual Way"))
                        blnFreeCost = true;

                    int intQualityBP = 0;
                    if (!blnFreeCost)
                    {
                        objXmlQuality.TryGetInt32FieldQuickly("karma", ref intQualityBP);
                        XmlNode xmlDiscountNode = objXmlQuality["costdiscount"];
                        if (xmlDiscountNode?.CreateNavigator().RequirementsMet(CharacterObject) == true)
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

                    int intKarmaCost = intQualityBP * CharacterObjectSettings.KarmaQuality;
                    if (!CharacterObjectSettings.DontDoubleQualityPurchases &&
                        objXmlQuality["doublecareer"]?.InnerText != bool.FalseString)
                        intKarmaCost *= 2;

                    // Make sure the character has enough Karma to pay for the Quality.
                    if (eQualityType == QualityType.Positive)
                    {
                        if (!blnFreeCost)
                        {
                            if (intKarmaCost > CharacterObject.Karma &&
                                objXmlQuality["stagedpurchase"]?.InnerText != bool.TrueString)
                            {
                                Program.MainForm.ShowMessageBox(this,
                                    await LanguageManager.GetStringAsync("Message_NotEnoughKarma"),
                                    await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                break;
                            }

                            string strDisplayName = objXmlQuality["translate"]?.InnerText ??
                                                    objXmlQuality["name"]?.InnerText ??
                                                    await LanguageManager.GetStringAsync("String_Unknown");
                            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend"), strDisplayName,
                                intKarmaCost.ToString(GlobalSettings.CultureInfo))))
                                break;
                        }
                    }
                    else if (Program.MainForm.ShowMessageBox(this,
                        await LanguageManager.GetStringAsync("Message_AddNegativeQuality"),
                        await LanguageManager.GetStringAsync("MessageTitle_AddNegativeQuality"), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.No)
                        break;

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objQuality = new Quality(CharacterObject);

                    objQuality.Create(objXmlQuality, QualitySource.Selected, lstWeapons);
                    if (objQuality.InternalId.IsEmptyGuid())
                    {
                        // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality,
                            objQuality.InternalId);
                        break;
                    }

                    // Make sure the character has enough Karma to pay for the Quality.
                    if (objQuality.Type == QualityType.Positive)
                    {
                        if (objQuality.ContributeToBP)
                        {
                            // Create the Karma expense.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(intKarmaCost * -1,
                                await LanguageManager.GetStringAsync("String_ExpenseAddPositiveQuality") +
                                await LanguageManager.GetStringAsync("String_Space") +
                                objQuality.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
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
                        objExpense.Create(0,
                            await LanguageManager.GetStringAsync("String_ExpenseAddNegativeQuality") +
                            await LanguageManager.GetStringAsync("String_Space") +
                            objQuality.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
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
                }
            }
            while (blnAddAgain);
        }

        private async void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (!(treQualities.SelectedNode?.Tag is Quality objSelectedQuality))
                return;
            string strInternalIDToRemove = objSelectedQuality.InternalId;
            // Can't do a foreach because we're removing items, this is the next best thing
            Quality objQualityToRemove =
                CharacterObject.Qualities.LastOrDefault(x => x.InternalId == strInternalIDToRemove);
            await RemoveQuality(objQualityToRemove);
        }

        private async void cmdSwapQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            Quality objQuality = treQualities.SelectedNode?.Tag as Quality;
            if (objQuality?.InternalId.IsEmptyGuid() != false)
                return;

            switch (objQuality.OriginSource)
            {
                // Qualities that come from a Metatype cannot be removed.
                case QualitySource.Metatype:
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_MetavariantQualitySwap"),
                        await LanguageManager.GetStringAsync("MessageTitle_MetavariantQualitySwap"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                // Neither can qualities from Improvements
                case QualitySource.Improvement:
                    Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ImprovementQuality"), objQuality.GetSourceName(GlobalSettings.Language)),
                        await LanguageManager.GetStringAsync("MessageTitle_MetavariantQuality"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            XmlNode objXmlQuality;
            int intRatingToAdd;
            using (SelectQuality frmPickQuality = new SelectQuality(CharacterObject)
            {
                ForceCategory = objQuality.Type.ToString(),
                IgnoreQuality = objQuality.Name
            })
            {
                await frmPickQuality.ShowDialogSafeAsync(this);

                // Don't do anything else if the form was canceled.
                if (frmPickQuality.DialogResult == DialogResult.Cancel)
                    return;

                objXmlQuality = (await CharacterObject.LoadDataAsync("qualities.xml")).SelectSingleNode("/chummer/qualities/quality[id = " + frmPickQuality.SelectedQuality.CleanXPath() + ']');
                intRatingToAdd = frmPickQuality.SelectedRating;
                int intDummy = 0;
                if (objXmlQuality != null && objXmlQuality["nolevels"] == null && objXmlQuality.TryGetInt32FieldQuickly("limit", ref intDummy))
                {
                    intRatingToAdd -= CharacterObject.Qualities.Count(x =>
                        x.SourceIDString.Equals(frmPickQuality.SelectedQuality,
                            StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(x.SourceName));
                }
            }

            if (intRatingToAdd > 0)
            {
                Quality objNewQuality = new Quality(CharacterObject);

                objNewQuality.Swap(objQuality, objXmlQuality, intRatingToAdd);
            }
        }

        private async ValueTask<bool> RemoveQuality(Quality objSelectedQuality, bool blnConfirmDelete = true, bool blnCompleteDelete = true)
        {
            XPathNavigator objXmlDeleteQuality = await objSelectedQuality.GetNodeXPathAsync();
            bool blnMetatypeQuality = false;

            switch (objSelectedQuality.OriginSource)
            {
                // Qualities that come from a Metatype cannot be removed.
                case QualitySource.Metatype:
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_MetavariantQuality"), await LanguageManager.GetStringAsync("MessageTitle_MetavariantQuality"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;

                case QualitySource.Improvement:
                    Program.MainForm.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ImprovementQuality"), objSelectedQuality.GetSourceName(GlobalSettings.Language)), await LanguageManager.GetStringAsync("MessageTitle_MetavariantQuality"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;

                case QualitySource.MetatypeRemovable:
                    {
                        // Look up the cost of the Quality.
                        int intBP = 0;
                        if (objSelectedQuality.Type == QualityType.Negative || objXmlDeleteQuality.SelectSingleNodeAndCacheExpression("refundkarmaonremove") != null)
                        {
                            intBP = Convert.ToInt32(objXmlDeleteQuality.SelectSingleNodeAndCacheExpression("karma")?.Value, GlobalSettings.InvariantCultureInfo) * CharacterObjectSettings.KarmaQuality;
                            if (blnCompleteDelete)
                                intBP *= objSelectedQuality.Levels;
                            if (!CharacterObjectSettings.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                            {
                                intBP *= 2;
                            }
                            if (objSelectedQuality.Type == QualityType.Positive)
                                intBP *= -1;
                        }
                        string strBP = intBP.ToString(GlobalSettings.CultureInfo) + await LanguageManager.GetStringAsync("String_Space") + await LanguageManager.GetStringAsync("String_Karma");

                        if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(
                            string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync(blnCompleteDelete ? "Message_DeleteMetatypeQuality" : "Message_LowerMetatypeQualityLevel"), strBP)))
                            return false;

                        blnMetatypeQuality = true;
                        break;
                    }
            }

            if (objSelectedQuality.Type == QualityType.Positive)
            {
                if (objXmlDeleteQuality.SelectSingleNodeAndCacheExpression("refundkarmaonremove") != null)
                {
                    int intKarmaCost = objSelectedQuality.BP * CharacterObjectSettings.KarmaQuality;

                    if (!CharacterObjectSettings.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                    {
                        intKarmaCost *= 2;
                    }

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(intKarmaCost, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_ExpenseSwapPositiveQuality")
                        , objSelectedQuality.DisplayNameShort(GlobalSettings.Language)
                        , await LanguageManager.GetStringAsync("String_Karma")), ExpenseType.Karma, DateTime.Now, true);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Karma += intKarmaCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.RemoveQuality, objSelectedQuality.SourceIDString);
                    objUndo.Extra = objSelectedQuality.Extra;
                    objExpense.Undo = objUndo;
                }
                else if (!blnMetatypeQuality && blnConfirmDelete && !CommonFunctions.ConfirmDelete(blnCompleteDelete
                    ? await LanguageManager.GetStringAsync("Message_DeletePositiveQualityCareer")
                    : await LanguageManager.GetStringAsync("Message_LowerPositiveQualityLevelCareer")))
                    return false;
            }
            else
            {
                // Make sure the character has enough Karma to buy off the Quality.
                int intKarmaCost = -(objSelectedQuality.BP * CharacterObjectSettings.KarmaQuality);
                if (!CharacterObjectSettings.DontDoubleQualityRefunds)
                {
                    intKarmaCost *= 2;
                }
                int intTotalKarmaCost = intKarmaCost;
                if (blnCompleteDelete)
                    intTotalKarmaCost *= objSelectedQuality.Levels;
                if (intTotalKarmaCost > CharacterObject.Karma)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (!blnMetatypeQuality && blnConfirmDelete && !CommonFunctions.ConfirmKarmaExpense(
                    string.Format(GlobalSettings.CultureInfo, blnCompleteDelete
                        ? await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseRemove") : await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseLowerLevel"),
                        objSelectedQuality.DisplayNameShort(GlobalSettings.Language), intTotalKarmaCost)))
                    return false;

                // Create the Karma expense.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(-intTotalKarmaCost, await LanguageManager.GetStringAsync("String_ExpenseRemoveNegativeQuality") + await LanguageManager.GetStringAsync("String_Space") + objSelectedQuality.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intTotalKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.RemoveQuality, objSelectedQuality.SourceIDString);
                objUndo.Extra = objSelectedQuality.Extra;
                objExpense.Undo = objUndo;
            }

            // Remove any Critter Powers that are gained through the Quality (Infected).
            if (objXmlDeleteQuality.SelectSingleNodeAndCacheExpression("powers/power") != null)
            {
                foreach (XPathNavigator objXmlPower in (await CharacterObject.LoadDataXPathAsync("critterpowers.xml")).SelectAndCacheExpression("optionalpowers/optionalpower"))
                {
                    string strExtra = objXmlPower.SelectSingleNodeAndCacheExpression("@select")?.Value;

                    foreach (CritterPower objPower in CharacterObject.CritterPowers)
                    {
                        if (objPower.Name != objXmlPower.Value || objPower.Extra != strExtra)
                            continue;
                        // Remove any Improvements created by the Critter Power.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId);

                        // Remove the Critter Power from the character.
                        CharacterObject.CritterPowers.Remove(objPower);
                        break;
                    }
                }
            }

            // Fix for legacy characters with old addqualities improvements.
            RemoveAddedQualities(objXmlDeleteQuality.Select("addqualities/addquality"));

            // Perform removal
            objSelectedQuality.DeleteQuality(blnCompleteDelete);
            return true;
        }

        private void UpdateQualityLevelValue(Quality objSelectedQuality = null)
        {
            if (objSelectedQuality == null
                || objSelectedQuality.OriginSource == QualitySource.Improvement
                || objSelectedQuality.OriginSource == QualitySource.Metatype
                || objSelectedQuality.Levels == 0)
            {
                nudQualityLevel.Value = 1;
                nudQualityLevel.Enabled = false;
                return;
            }
            XPathNavigator objQualityNode = objSelectedQuality.GetNodeXPath();
            string strLimitString = objQualityNode?.SelectSingleNodeAndCacheExpression("limit")?.Value;
            if (!string.IsNullOrWhiteSpace(strLimitString) && objQualityNode.SelectSingleNodeAndCacheExpression("nolevels") == null && int.TryParse(strLimitString, out int intMaxRating))
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

        private async void nudQualityLevel_ValueChanged(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treQualities.SelectedNode?.Tag is Quality objSelectedQuality)
            {
                int intCurrentLevels = objSelectedQuality.Levels;

                // Adding a new level
                for (; nudQualityLevel.Value > intCurrentLevels; ++intCurrentLevels)
                {
                    XPathNavigator objXmlSelectedQuality = await objSelectedQuality.GetNodeXPathAsync();
                    if (!objXmlSelectedQuality.RequirementsMet(CharacterObject, await LanguageManager.GetStringAsync("String_Quality")))
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
                        XPathNavigator xpnDiscountNode = objXmlSelectedQuality.SelectSingleNodeAndCacheExpression("costdiscount");
                        if (xpnDiscountNode?.RequirementsMet(CharacterObject) == true)
                        {
                            int intTemp = 0;
                            xpnDiscountNode.TryGetInt32FieldQuickly("value", ref intTemp);
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

                    int intKarmaCost = intQualityBP * CharacterObjectSettings.KarmaQuality;
                    if (!CharacterObjectSettings.DontDoubleQualityPurchases && objSelectedQuality.DoubleCost)
                        intKarmaCost *= 2;

                    // Make sure the character has enough Karma to pay for the Quality.
                    if (eQualityType == QualityType.Positive)
                    {
                        if (!blnFreeCost)
                        {
                            if (intKarmaCost > CharacterObject.Karma && !objSelectedQuality.StagedPurchase)
                            {
                                Program.MainForm.ShowMessageBox(
                                    this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"),
                                    await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                UpdateQualityLevelValue(objSelectedQuality);
                                break;
                            }

                            string strDisplayName
                                = objXmlSelectedQuality.SelectSingleNodeAndCacheExpression("translate")?.Value
                                  ?? objXmlSelectedQuality.SelectSingleNodeAndCacheExpression("name")?.Value
                                  ?? await LanguageManager.GetStringAsync("String_Unknown");
                            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                                , strDisplayName
                                , intKarmaCost.ToString(GlobalSettings.CultureInfo))))
                            {
                                UpdateQualityLevelValue(objSelectedQuality);
                                break;
                            }
                        }
                    }
                    else if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_AddNegativeQuality"), await LanguageManager.GetStringAsync("MessageTitle_AddNegativeQuality"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objQuality = new Quality(CharacterObject);

                    objQuality.Create(await objSelectedQuality.GetNodeAsync(), QualitySource.Selected, lstWeapons, objSelectedQuality.Extra);
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
                        objExpense.Create(intKarmaCost * -1, await LanguageManager.GetStringAsync("String_ExpenseAddPositiveQuality") + await LanguageManager.GetStringAsync("String_Space") + objQuality.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
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
                        objExpense.Create(0, await LanguageManager.GetStringAsync("String_ExpenseAddNegativeQuality") + await LanguageManager.GetStringAsync("String_Space") + objQuality.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
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
                }
                // Removing a level
                for (; nudQualityLevel.Value < intCurrentLevels; --intCurrentLevels)
                {
                    Quality objInvisibleQuality = CharacterObject.Qualities.FirstOrDefault(x => x.SourceIDString == objSelectedQuality.SourceIDString && x.Extra == objSelectedQuality.Extra && x.SourceName == objSelectedQuality.SourceName && x.InternalId != objSelectedQuality.InternalId);
                    if (objInvisibleQuality == null || !await RemoveQuality(objInvisibleQuality, false, false))
                    {
                        if (!await RemoveQuality(objSelectedQuality, false, false))
                        {
                            UpdateQualityLevelValue(objSelectedQuality);
                        }
                        break;
                    }
                }
            }
        }

        private async void cmdAddLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Gear Tree.
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                    return;

                Location objLocation = new Location(CharacterObject, CharacterObject.GearLocations, frmPickText.SelectedValue);
                CharacterObject.GearLocations.Add(objLocation);
            }
        }

        private async void cmdAddWeaponLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Gear Tree.
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                    return;

                Location objLocation = new Location(CharacterObject, CharacterObject.WeaponLocations, frmPickText.SelectedValue);
                CharacterObject.WeaponLocations.Add(objLocation);
            }
        }

        private async void cmdAddWeek_Click(object sender, EventArgs e)
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
                    ++objWeek.Year;
                }
            }
            else
            {
                using (SelectCalendarStart frmPickStart = new SelectCalendarStart())
                {
                    await frmPickStart.ShowDialogSafeAsync(this);

                    if (frmPickStart.DialogResult == DialogResult.Cancel)
                        return;

                    objWeek.Year = frmPickStart.SelectedYear;
                    objWeek.Week = frmPickStart.SelectedWeek;
                }
            }

            CharacterObject.Calendar.AddWithSort(objWeek, (x, y) => y.CompareTo(x));
        }

        private async void cmdDeleteWeek_Click(object sender, EventArgs e)
        {
            if (lstCalendar == null || lstCalendar.SelectedItems.Count == 0)
            {
                return;
            }

            string strWeekId = lstCalendar.SelectedItems[0].SubItems[2].Text;

            CalendarWeek objCharacterWeek = CharacterObject.Calendar.FirstOrDefault(x => x.InternalId == strWeekId);

            if (objCharacterWeek == null)
                return;
            if (!CommonFunctions.ConfirmDelete(await LanguageManager.GetStringAsync("Message_DeleteCalendarWeek")))
                return;

            CharacterObject.Calendar.Remove(objCharacterWeek);
        }

        private async void cmdEditWeek_Click(object sender, EventArgs e)
        {
            if (lstCalendar == null || lstCalendar.SelectedItems.Count == 0)
            {
                return;
            }

            string strWeekId = lstCalendar.SelectedItems[0].SubItems[2].Text;

            CalendarWeek objWeek = CharacterObject.Calendar.FirstOrDefault(x => x.InternalId == strWeekId);

            if (objWeek == null)
                return;
            using (EditNotes frmItemNotes = new EditNotes(objWeek.Notes))
            {
                await frmItemNotes.ShowDialogSafeAsync(this);
                if (frmItemNotes.DialogResult != DialogResult.OK)
                    return;
                objWeek.Notes = frmItemNotes.Notes;
                IsDirty = true;
            }
        }

        private async void cmdChangeStartWeek_Click(object sender, EventArgs e)
        {
            // Find the first date.
            CalendarWeek objStart = CharacterObject.Calendar?.LastOrDefault();
            if (objStart == null)
            {
                return;
            }

            int intYear;
            int intWeek;
            using (SelectCalendarStart frmPickStart = new SelectCalendarStart(objStart))
            {
                await frmPickStart.ShowDialogSafeAsync(this);

                if (frmPickStart.DialogResult == DialogResult.Cancel)
                    return;

                intYear = frmPickStart.SelectedYear;
                intWeek = frmPickStart.SelectedWeek;
            }

            // Determine the difference between the starting value and selected values for year and week.
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
                    --objWeek.Year;
                    objWeek.Week += 52;
                }
                else if (objWeek.Week > 52)
                {
                    ++objWeek.Year;
                    objWeek.Week -= 52;
                }
            }

            IsDirty = true;
        }

        private async void cmdAddImprovement_Click(object sender, EventArgs e)
        {
            string location = treImprovements.SelectedNode?.Tag is string strSelectedId && strSelectedId != "Node_SelectedImprovements"
                ? strSelectedId
                : string.Empty;
            using (CreateImprovement frmPickImprovement = new CreateImprovement(CharacterObject, location))
            {
                await frmPickImprovement.ShowDialogSafeAsync(this);

                if (frmPickImprovement.DialogResult == DialogResult.Cancel)
                    return;
            }

            RefreshCustomImprovements(treImprovements, lmtControl.LimitTreeView, cmsImprovementLocation, cmsImprovement, lmtControl.LimitContextMenuStrip);
        }

        private async void cmdCreateStackedFocus_Click(object sender, EventArgs e)
        {
            int intFree = 0;
            List<Gear> lstGear = new List<Gear>(CharacterObject.Gear.Count);

            // Run through all of the Foci the character has and count the un-Bonded ones.
            foreach (Gear objGear in CharacterObject.Gear)
            {
                if ((objGear.Category == "Foci" || objGear.Category == "Metamagic Foci") && !objGear.Bonded)
                {
                    ++intFree;
                    lstGear.Add(objGear);
                }
            }

            // If the character does not have at least 2 un-Bonded Foci, display an error and leave.
            if (intFree < 2)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotStackFoci"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Gear> lstStack = new List<Gear>(lstGear.Count);

            using (SelectItem frmPickItem = new SelectItem
            {
                Description = await LanguageManager.GetStringAsync("String_SelectItemFocus"),
                AllowAutoSelect = false
            })
            {
                // Let the character select the Foci they'd like to stack, stopping when they either click Cancel or there are no more items left in the list.
                do
                {
                    frmPickItem.SetGearMode(lstGear);
                    await frmPickItem.ShowDialogSafeAsync(this);

                    if (frmPickItem.DialogResult != DialogResult.OK)
                        continue;
                    // Move the item from the Gear list to the Stack list.
                    foreach (Gear objGear in lstGear)
                    {
                        if (objGear.InternalId != frmPickItem.SelectedItem)
                            continue;
                        objGear.Bonded = true;
                        lstStack.Add(objGear);
                        lstGear.Remove(objGear);
                        break;
                    }
                } while (lstGear.Count > 0 && frmPickItem.DialogResult != DialogResult.Cancel);
            }

            // Make sure at least 2 Foci were selected.
            if (lstStack.Count < 2)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_StackedFocusMinimum"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the combined Force of the Foci do not exceed 6.
            if (!CharacterObjectSettings.AllowHigherStackedFoci)
            {
                int intCombined = lstStack.Sum(objGear => objGear.Rating);
                if (intCombined > 6)
                {
                    foreach (Gear objGear in lstStack)
                        objGear.Bonded = false;
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_StackedFocusForce"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Name = "Stacked Focus: " + objStack.CurrentDisplayName,
                Source = "SR5",
                Page = "1",
                Cost = decCost.ToString(GlobalSettings.CultureInfo),
                Avail = "0"
            };

            CharacterObject.Gear.Add(objStackItem);

            objStack.GearId = objStackItem.InternalId;
        }

        private async void cmdBurnStreetCred_Click(object sender, EventArgs e)
        {
            if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_BurnStreetCred"), await LanguageManager.GetStringAsync("MessageTitle_BurnStreetCred"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            CharacterObject.BurntStreetCred += 2;
        }

        private async void cmdEditImprovement_Click(object sender, EventArgs e)
        {
            // Edit the selected Improvement.
            if (!(treImprovements.SelectedNode?.Tag is Improvement objImprovement))
                return;
            using (CreateImprovement frmPickImprovement = new CreateImprovement(CharacterObject, objImprovement.CustomGroup)
            {
                EditImprovementObject = objImprovement
            })
            {
                await frmPickImprovement.ShowDialogSafeAsync(this);

                if (frmPickImprovement.DialogResult == DialogResult.Cancel)
                    return;

                TreeNode newNode = null;
                if (!string.IsNullOrEmpty(frmPickImprovement.NewImprovement?.InternalId))
                    newNode = treImprovements.FindNode(frmPickImprovement.NewImprovement.InternalId);

                if (newNode != null)
                {
                    newNode.Text = frmPickImprovement.NewImprovement.CustomName;
                    newNode.ForeColor = frmPickImprovement.NewImprovement.PreferredColor;
                    newNode.ToolTipText = frmPickImprovement.NewImprovement.Notes;
                }
                else
                {
                    Utils.BreakIfDebug();
                }

                //TODO: This is currently necessary because the Custom Improvement refresh fires before the improvement is assigned a custom group.
                // Simplest way to fix this would be to make the customgroup a variable in the CreateImprovements method, but that's spooky.
                if (!string.IsNullOrWhiteSpace(frmPickImprovement.NewImprovement?.CustomGroup))
                {
                    RefreshCustomImprovements(treImprovements, lmtControl.LimitTreeView, cmsImprovementLocation, cmsImprovement, lmtControl.LimitContextMenuStrip);
                }
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void cmdDeleteImprovement_Click(object sender, EventArgs e)
        {
            object objSelectedImprovement = treImprovements.SelectedNode?.Tag;
            if (objSelectedImprovement is Improvement objImprovement)
            {
                if (CommonFunctions.ConfirmDelete(await LanguageManager.GetStringAsync("Message_DeleteImprovement")))
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Custom,
                                                          objImprovement.SourceName);
            }
            else if (objSelectedImprovement is string strSelectedId)
            {
                if (strSelectedId == "Node_SelectedImprovements")
                    return;
                if (!CommonFunctions.ConfirmDelete(
                        await LanguageManager.GetStringAsync("Message_DeleteImprovementGroup")))
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
        }

        private async void cmdAddArmorBundle_Click(object sender, EventArgs e)
        {
            // Add a new location to the Armor Tree.
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                    return;

                Location objLocation = new Location(CharacterObject, CharacterObject.ArmorLocations, frmPickText.SelectedValue);
                CharacterObject.ArmorLocations.Add(objLocation);
            }
        }

        private void cmdArmorEquipAll_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is Location selectedLocation)
            {
                // Equip all of the Armor in the Armor Bundle.
                foreach (Armor objArmor in selectedLocation.Children.OfType<Armor>())
                {
                    if (objArmor.Location == selectedLocation)
                    {
                        objArmor.Equipped = true;
                    }
                }
            }
            else if (treArmor.SelectedNode?.Tag?.ToString() == "Node_SelectedArmor")
            {
                foreach (Armor objArmor in CharacterObject.Armor.Where(objArmor =>
                    !objArmor.Equipped && objArmor.Location == null))
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
                foreach (Armor objArmor in selectedLocation.Children.OfType<Armor>())
                {
                    if (objArmor.Location == selectedLocation)
                    {
                        objArmor.Equipped = false;
                    }
                }
            }
            else if (treArmor.SelectedNode?.Tag?.ToString() == "Node_SelectedArmor")
            {
                foreach (Armor objArmor in CharacterObject.Armor.Where(objArmor =>
                    objArmor.Equipped && objArmor.Location == null))
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
            if (!(treImprovements.SelectedNode?.Tag is string strSelectedId))
                return;
            List<Improvement> lstImprovementsEnabled;
            if (strSelectedId == "Node_SelectedImprovements")
            {
                lstImprovementsEnabled = CharacterObject.Improvements
                                                        .Where(objImprovement =>
                                                                   objImprovement.Custom && !objImprovement.Enabled
                                                                   && string.IsNullOrEmpty(objImprovement.CustomGroup))
                                                        .ToList();
            }
            else
            {
                lstImprovementsEnabled = CharacterObject.Improvements
                                                        .Where(objImprovement =>
                                                                   objImprovement.Custom && !objImprovement.Enabled
                                                                   && objImprovement.CustomGroup == strSelectedId)
                                                        .ToList();
            }
            if (lstImprovementsEnabled.Count == 0)
                return;
            ImprovementManager.EnableImprovements(CharacterObject, lstImprovementsEnabled);
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdImprovementsDisableAll_Click(object sender, EventArgs e)
        {
            if (!(treImprovements.SelectedNode?.Tag is string strSelectedId))
                return;
            // Disable all of the Improvements in the Improvement Group.
            List<Improvement> lstImprovementsDisabled;
            if (strSelectedId == "Node_SelectedImprovements")
            {
                lstImprovementsDisabled = CharacterObject.Improvements
                                                         .Where(objImprovement =>
                                                                    objImprovement.Custom && objImprovement.Enabled
                                                                    && string.IsNullOrEmpty(objImprovement.CustomGroup))
                                                         .ToList();
            }
            else
            {
                lstImprovementsDisabled = CharacterObject.Improvements
                                                         .Where(objImprovement =>
                                                                    objImprovement.Custom && objImprovement.Enabled
                                                                    && objImprovement.CustomGroup == strSelectedId)
                                                         .ToList();
            }
            if (lstImprovementsDisabled.Count == 0)
                return;
            ImprovementManager.DisableImprovements(CharacterObject, lstImprovementsDisabled);
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void cmdAddVehicleLocation_Click(object sender, EventArgs e)
        {
            ICollection<Location> destCollection;
            // Make sure a Vehicle is selected.
            if (treVehicles.SelectedNode?.Tag is Vehicle objVehicle)
            {
                destCollection = objVehicle.Locations;
            }
            else if (treVehicles.SelectedNode?.Tag == null || treVehicles.SelectedNode?.Tag.ToString() == "Node_SelectedVehicles")
            {
                destCollection = CharacterObject.VehicleLocations;
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicleLocation"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                    return;
                Location objLocation = new Location(CharacterObject, destCollection, frmPickText.SelectedValue);
                destCollection.Add(objLocation);
            }
        }

        private async void cmdQuickenSpell_Click(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode == null || treSpells.SelectedNode.Level != 1)
                return;

            int intKarmaCost;
            using (SelectNumber frmPickNumber = new SelectNumber(0)
            {
                Description = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_QuickeningKarma"), treSpells.SelectedNode.Text),
                Minimum = 1
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                intKarmaCost = frmPickNumber.SelectedValue.ToInt32();
            }

            // Make sure the character has enough Karma to improve the CharacterAttribute.
            if (intKarmaCost > CharacterObject.Karma)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseQuickeningMetamagic")
                , intKarmaCost.ToString(GlobalSettings.CultureInfo)
                , treSpells.SelectedNode.Text)))
                return;

            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(intKarmaCost * -1, await LanguageManager.GetStringAsync("String_ExpenseQuickenMetamagic") + await LanguageManager.GetStringAsync("String_Space") + treSpells.SelectedNode.Text, ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.QuickeningMetamagic, string.Empty);
            objExpense.Undo = objUndo;
        }

        private void cmdAddSustainedSpell_Click(object sender, EventArgs e)
        {
            AddSustainedSpell();
        }

        private void cmdAddSustainedForm_Click(object sender, EventArgs e)
        {
            AddSustainedComplexForm();
        }

        private void cmdAddSustainedCritterPower_Click(object sender, EventArgs e)
        {
            AddSustainedCritterPower();
        }

        #endregion Button Events

        #region ContextMenu Events

        private void InitiationContextMenu_Opening(object sender, CancelEventArgs e)
        {
            // Enable and disable menu items
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;
            int intGrade = objGrade.Grade;
            bool blnHasArt = CharacterObject.Arts.Any(art => art.Grade == intGrade);
            bool blnHasBonus = CharacterObject.Metamagics.Any(bonus => bonus.Grade == intGrade) || CharacterObject.Spells.Any(spell => spell.Grade == intGrade);
            tsMetamagicAddArt.Enabled = !blnHasArt;
            tsMetamagicAddMetamagic.Enabled = !blnHasBonus;
        }

        private async void tsCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objCyberware && !string.IsNullOrWhiteSpace(objCyberware.AllowedSubsystems)))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickCyberware(objCyberware, objCyberware.SourceType);
            }
            while (blnAddAgain);
        }

        private async void tsVehicleCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware && !string.IsNullOrWhiteSpace(objCyberware.AllowedSubsystems)))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickCyberware(objCyberware, objCyberware.SourceType);
            }
            while (blnAddAgain);
        }

        private async void tsWeaponAddAccessory_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponAccessory"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Accessories cannot be added to Cyberweapons.
            if (objWeapon.Cyberware)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberweaponNoAccessory"), await LanguageManager.GetStringAsync("MessageTitle_CyberweaponNoAccessory"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

            XmlNode objXmlWeapon = await objWeapon.GetNodeAsync();
            if (objXmlWeapon == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotFindWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                // Make sure the Weapon allows Accessories to be added to it.
                if (!objWeapon.AllowAccessory)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotModifyWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }

                using (SelectWeaponAccessory frmPickWeaponAccessory = new SelectWeaponAccessory(CharacterObject)
                {
                    ParentWeapon = objWeapon
                })
                {
                    await frmPickWeaponAccessory.ShowDialogSafeAsync(this);

                    if (frmPickWeaponAccessory.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickWeaponAccessory.AddAgain;

                    // Locate the selected piece.
                    objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[id = " + frmPickWeaponAccessory.SelectedAccessory.CleanXPath() + ']');

                    WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
                    objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, frmPickWeaponAccessory.SelectedRating);
                    objAccessory.Parent = objWeapon;

                    if (objAccessory.Cost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        string strCost = objAccessory.Cost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                        if (decMin != 0 || decMax != decimal.MaxValue)
                        {
                            if (decMax > 1000000)
                                decMax = 1000000;
                            using (SelectNumber frmPickNumber = new SelectNumber(CharacterObjectSettings.MaxNuyenDecimals)
                            {
                                Minimum = decMin,
                                Maximum = decMax,
                                Description = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_SelectVariableCost"), objAccessory.CurrentDisplayNameShort),
                                AllowCancel = false
                            })
                            {
                                await frmPickNumber.ShowDialogSafeAsync(this);
                                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                                {
                                    objAccessory.DeleteWeaponAccessory();
                                    continue;
                                }
                                objAccessory.Cost = frmPickNumber.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }

                    objAccessory.DiscountCost = frmPickWeaponAccessory.BlackMarketDiscount;

                    // Check the item's Cost and make sure the character can afford it.
                    decimal decOriginalCost = objWeapon.TotalCost;
                    objWeapon.WeaponAccessories.Add(objAccessory);

                    decimal decCost = objWeapon.TotalCost - decOriginalCost;
                    // Apply a markup if applicable.
                    if (frmPickWeaponAccessory.Markup != 0)
                    {
                        decCost *= 1 + frmPickWeaponAccessory.Markup / 100.0m;
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = objAccessory.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (!frmPickWeaponAccessory.FreeCost)
                    {
                        if (decCost > CharacterObject.Nuyen)
                        {
                            objWeapon.WeaponAccessories.Remove(objAccessory);
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeaponAccessory") + await LanguageManager.GetStringAsync("String_Space") + objAccessory.CurrentDisplayNameShort,
                            ExpenseType.Nuyen, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Nuyen -= decCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateNuyen(NuyenExpenseType.AddWeaponAccessory, objAccessory.InternalId);
                        objExpense.Undo = objUndo;
                    }
                }
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> PickArmor(Location objLocation = null)
        {
            using (SelectArmor frmPickArmor = new SelectArmor(CharacterObject))
            {
                await frmPickArmor.ShowDialogSafeAsync(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickArmor.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Armor XML file and locate the selected piece.
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("armor.xml");

                XmlNode objXmlArmor = objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = " + frmPickArmor.SelectedArmor.CleanXPath() + ']');

                Armor objArmor = new Armor(CharacterObject);
                List<Weapon> lstWeapons = new List<Weapon>(1);
                objArmor.Create(objXmlArmor, frmPickArmor.Rating, lstWeapons);
                objArmor.DiscountCost = frmPickArmor.BlackMarketDiscount;

                if (objArmor.InternalId.IsEmptyGuid())
                    return frmPickArmor.AddAgain;

                decimal decCost = objArmor.TotalCost;
                // Apply a markup if applicable.
                if (frmPickArmor.Markup != 0)
                {
                    decCost *= 1 + frmPickArmor.Markup / 100.0m;
                }

                // Multiply the cost if applicable.
                char chrAvail = objArmor.TotalAvailTuple().Suffix;
                switch (chrAvail)
                {
                    case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                        decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                        break;

                    case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                        decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                        break;
                }

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickArmor.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Remove the Improvements created by the Armor.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Armor, objArmor.InternalId);

                        return frmPickArmor.AddAgain;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseArmor") + await LanguageManager.GetStringAsync("String_Space") + objArmor.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddArmor, objArmor.InternalId);
                    objExpense.Undo = objUndo;
                }

                // objArmor.Location = objLocation;
                objLocation?.Children.Add(objArmor);
                CharacterObject.Armor.Add(objArmor);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                return frmPickArmor.AddAgain;
            }
        }

        private async void tsArmorLocationAddArmor_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Location objSelectedLocation)) return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickArmor(objSelectedLocation);
            }
            while (blnAddAgain);
        }

        private async void tsAddArmorMod_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Armor XML file and locate the selected Armor.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("armor.xml");

            XmlNode objXmlArmor = await objArmor.GetNodeAsync();

            string strAllowedCategories = objArmor.Category + ',' + objArmor.Name;
            bool blnExcludeGeneralCategory = false;
            XmlNode xmlAddModCategory = objXmlArmor["forcemodcategory"];
            if (xmlAddModCategory != null)
            {
                strAllowedCategories = xmlAddModCategory.InnerText;
                blnExcludeGeneralCategory = true;
            }
            else
            {
                xmlAddModCategory = objXmlArmor["addmodcategory"];
                if (xmlAddModCategory != null)
                {
                    strAllowedCategories += ',' + xmlAddModCategory.InnerText;
                }
            }

            bool blnAddAgain;
            do
            {
                using (SelectArmorMod frmPickArmorMod = new SelectArmorMod(CharacterObject, objArmor)
                       {
                           ArmorCost = objArmor.OwnCost,
                           ArmorCapacity = Convert.ToDecimal(objArmor.CalculatedCapacity(GlobalSettings.InvariantCultureInfo), GlobalSettings.InvariantCultureInfo),
                           AllowedCategories = strAllowedCategories,
                           ExcludeGeneralCategory = blnExcludeGeneralCategory,
                           CapacityDisplayStyle = objArmor.CapacityDisplayStyle
                       })
                {
                    await frmPickArmorMod.ShowDialogSafeAsync(this);

                    if (frmPickArmorMod.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickArmorMod.AddAgain;

                    // Locate the selected piece.
                    objXmlArmor = objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = " + frmPickArmorMod.SelectedArmorMod.CleanXPath() + ']');

                    if (objXmlArmor == null)
                        continue;

                    ArmorMod objMod = new ArmorMod(CharacterObject);
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    int intRating = Convert.ToInt32(objXmlArmor["maxrating"]?.InnerText, GlobalSettings.InvariantCultureInfo) > 1 ? frmPickArmorMod.SelectedRating : 0;

                    objMod.Create(objXmlArmor, intRating, lstWeapons);
                    if (objMod.InternalId.IsEmptyGuid())
                        continue;

                    // Check the item's Cost and make sure the character can afford it.
                    decimal decOriginalCost = objArmor.TotalCost;
                    objArmor.ArmorMods.Add(objMod);

                    // Do not allow the user to add a new piece of Armor if its Capacity has been reached.
                    if (CharacterObjectSettings.EnforceCapacity && objArmor.CapacityRemaining < 0)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        objArmor.ArmorMods.Remove(objMod);
                        continue;
                    }

                    decimal decCost = objArmor.TotalCost - decOriginalCost;
                    // Apply a markup if applicable.
                    if (frmPickArmorMod.Markup != 0)
                    {
                        decCost *= 1 + frmPickArmorMod.Markup / 100.0m;
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = objMod.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (!frmPickArmorMod.FreeCost)
                    {
                        if (decCost > CharacterObject.Nuyen)
                        {
                            objArmor.ArmorMods.Remove(objMod);
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Remove the Improvements created by the Armor Mod.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                            continue;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseArmorMod") + await LanguageManager.GetStringAsync("String_Space") + objMod.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                            DateTime.Now);
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
                }
            }
            while (blnAddAgain);
        }

        private async void tsGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treGear.SelectedNode?.Tag is IHasChildren<Gear> iParent))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickGear(iParent);
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddMod_Click(object sender, EventArgs e)
        {
            while (treVehicles.SelectedNode?.Level > 1)
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;

            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Vehicle Mod window.
            if (!(objSelectedNode?.Tag is Vehicle objVehicle))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicle"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("vehicles.xml");

            bool blnAddAgain;

            do
            {
                using (SelectVehicleMod frmPickVehicleMod = new SelectVehicleMod(CharacterObject, objVehicle, objVehicle.Mods))
                {
                    await frmPickVehicleMod.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickVehicleMod.AddAgain;

                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = " + frmPickVehicleMod.SelectedMod.CleanXPath() + ']');

                    VehicleMod objMod = new VehicleMod(CharacterObject)
                    {
                        DiscountCost = frmPickVehicleMod.BlackMarketDiscount
                    };
                    objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, objVehicle, frmPickVehicleMod.Markup);
                    // Make sure that the Armor Rating does not exceed the maximum allowed by the Vehicle.
                    if (objMod.Name.StartsWith("Armor", StringComparison.Ordinal))
                    {
                        if (objMod.Rating > objVehicle.MaxArmor)
                        {
                            objMod.Rating = objVehicle.MaxArmor;
                        }
                    }
                    else
                    {
                        switch (objMod.Category)
                        {
                            case "Handling":
                                {
                                    if (objMod.Rating > objVehicle.MaxHandling)
                                    {
                                        objMod.Rating = objVehicle.MaxHandling;
                                    }

                                    break;
                                }
                            case "Speed":
                                {
                                    if (objMod.Rating > objVehicle.MaxSpeed)
                                    {
                                        objMod.Rating = objVehicle.MaxSpeed;
                                    }

                                    break;
                                }
                            case "Acceleration":
                                {
                                    if (objMod.Rating > objVehicle.MaxAcceleration)
                                    {
                                        objMod.Rating = objVehicle.MaxAcceleration;
                                    }

                                    break;
                                }
                            case "Sensor":
                                {
                                    if (objMod.Rating > objVehicle.MaxSensor)
                                    {
                                        objMod.Rating = objVehicle.MaxSensor;
                                    }

                                    break;
                                }
                            default:
                                {
                                    if (objMod.Name.StartsWith("Pilot Program", StringComparison.Ordinal) && objMod.Rating > objVehicle.MaxPilot)
                                    {
                                        objMod.Rating = objVehicle.MaxPilot;
                                    }

                                    break;
                                }
                        }
                    }

                    // Check the item's Cost and make sure the character can afford it.
                    decimal decOriginalCost = objVehicle.TotalCost;
                    if (frmPickVehicleMod.FreeCost)
                        objMod.Cost = "0";

                    objVehicle.Mods.Add(objMod);

                    // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                    if (CharacterObjectSettings.EnforceCapacity)
                    {
                        bool blnOverCapacity;
                        if (CharacterObjectSettings.BookEnabled("R5"))
                        {
                            if (objVehicle.IsDrone && CharacterObjectSettings.DroneMods)
                                blnOverCapacity = objVehicle.DroneModSlotsUsed > objVehicle.DroneModSlots;
                            else
                                blnOverCapacity = objVehicle.OverR5Capacity("Weapons");
                        }
                        else
                            blnOverCapacity = objVehicle.Slots < objVehicle.SlotsUsed;

                        if (blnOverCapacity)
                        {
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            objVehicle.Mods.Remove(objMod);
                            continue;
                        }
                    }

                    decimal decCost = objVehicle.TotalCost - decOriginalCost;

                    // Multiply the cost if applicable.
                    char chrAvail = objMod.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (decCost > CharacterObject.Nuyen)
                    {
                        objVehicle.Mods.Remove(objMod);
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleMod") + await LanguageManager.GetStringAsync("String_Space") + objMod.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                        DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleMod, objMod.InternalId);
                    objExpense.Undo = objUndo;
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddWeaponWeapon_Click(object sender, EventArgs e)
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
                    if (objMod?.Name.StartsWith("Mechanical Arm", StringComparison.Ordinal) == false && !objMod.Name.Contains("Drone Arm"))
                    {
                        objMod = null;
                    }
                }
            }

            if (objWeaponMount == null && objMod == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotAddWeapon"),
                    await LanguageManager.GetStringAsync("MessageTitle_CannotAddWeapon"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (objWeaponMount?.IsWeaponsFull == true)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponMountFull"),
                    await LanguageManager.GetStringAsync("MessageTitle_CannotAddWeapon"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddWeaponToWeaponMount(objWeaponMount, objMod, objVehicle);
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> AddWeaponToWeaponMount(WeaponMount objWeaponMount, VehicleMod objMod, Vehicle objVehicle)
        {
            using (SelectWeapon frmPickWeapon = new SelectWeapon(CharacterObject)
            {
                LimitToCategories = objMod == null ? objWeaponMount.AllowedWeaponCategories : objMod.WeaponMountCategories
            })
            {
                await frmPickWeapon.ShowDialogSafeAsync(this);

                if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Weapons XML file and locate the selected piece.
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

                XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                List<Weapon> lstWeapons = new List<Weapon>(1);
                Weapon objWeapon = new Weapon(CharacterObject)
                {
                    ParentVehicle = objVehicle,
                    ParentVehicleMod = objMod,
                    ParentMount = objMod == null ? objWeaponMount : null
                };
                objWeapon.Create(objXmlWeapon, lstWeapons);

                decimal decCost = objWeapon.TotalCost;
                // Apply a markup if applicable.
                if (frmPickWeapon.Markup != 0)
                {
                    decCost *= 1 + frmPickWeapon.Markup / 100.0m;
                }

                // Multiply the cost if applicable.
                char chrAvail = objWeapon.TotalAvailTuple().Suffix;
                switch (chrAvail)
                {
                    case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                        decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                        break;

                    case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                        decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                        break;
                }

                if (!frmPickWeapon.FreeCost)
                {
                    // Check the item's Cost and make sure the character can afford it.
                    if (decCost > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return frmPickWeapon.AddAgain;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleWeapon") + await LanguageManager.GetStringAsync("String_Space") + objWeapon.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                        DateTime.Now);
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

                return frmPickWeapon.AddAgain && (objMod != null || !objWeaponMount.IsWeaponsFull);
            }
        }

        private async void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
                return;
            WeaponMount objNewWeaponMount;
            using (CreateWeaponMount frmPickVehicleMod = new CreateWeaponMount(objVehicle, CharacterObject)
            {
                AllowDiscounts = true
            })
            {
                await frmPickVehicleMod.ShowDialogSafeAsync(this);

                if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                    return;

                objNewWeaponMount = frmPickVehicleMod.WeaponMount;
            }

            // Calculate cost based on total vehicle cost change to make sure we capture everything
            decimal decCost = -objVehicle.TotalCost;
            objVehicle.WeaponMounts.Add(objNewWeaponMount);
            decCost += objVehicle.TotalCost;

            // Multiply the cost if applicable.
            char chrAvail = objNewWeaponMount.TotalAvailTuple().Suffix;
            switch (chrAvail)
            {
                case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                    decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                    break;

                case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                    decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                    break;
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(decCost * -1,
                              await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleWeaponAccessory") + await LanguageManager.GetStringAsync("String_Space") + objNewWeaponMount.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                              DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen -= decCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMount, objNewWeaponMount.InternalId);
            objExpense.Undo = objUndo;
        }

        private async void tsVehicleAddWeaponAccessory_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleWeaponAccessories"), await LanguageManager.GetStringAsync("MessageTitle_VehicleWeaponAccessories"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");
            XmlNode objXmlWeapon = await objWeapon.GetNodeAsync();
            if (objXmlWeapon == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotFindWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                // Make sure the Weapon allows Accessories to be added to it.
                if (!objWeapon.AllowAccessory)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotModifyWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SelectWeaponAccessory frmPickWeaponAccessory = new SelectWeaponAccessory(CharacterObject)
                {
                    ParentWeapon = objWeapon
                })
                {
                    await frmPickWeaponAccessory.ShowDialogSafeAsync(this);

                    if (frmPickWeaponAccessory.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickWeaponAccessory.AddAgain;

                    // Locate the selected piece.
                    objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[id = " + frmPickWeaponAccessory.SelectedAccessory.CleanXPath() + ']');

                    WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
                    objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, frmPickWeaponAccessory.SelectedRating);
                    objAccessory.Parent = objWeapon;

                    // Check the item's Cost and make sure the character can afford it.
                    decimal intOriginalCost = objWeapon.TotalCost;
                    objWeapon.WeaponAccessories.Add(objAccessory);

                    decimal decCost = objWeapon.TotalCost - intOriginalCost;
                    // Apply a markup if applicable.
                    if (frmPickWeaponAccessory.Markup != 0)
                    {
                        decCost *= 1 + frmPickWeaponAccessory.Markup / 100.0m;
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = objAccessory.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (!frmPickWeaponAccessory.FreeCost)
                    {
                        if (decCost > CharacterObject.Nuyen)
                        {
                            objWeapon.WeaponAccessories.Remove(objAccessory);
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleWeaponAccessory") + await LanguageManager.GetStringAsync("String_Space") + objAccessory.CurrentDisplayNameShort,
                            ExpenseType.Nuyen, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Nuyen -= decCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponAccessory, objAccessory.InternalId);
                        objExpense.Undo = objUndo;
                    }
                }
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> AddUnderbarrelWeapon(Weapon objSelectedWeapon, string strExpenseString)
        {
            using (SelectWeapon frmPickWeapon = new SelectWeapon(CharacterObject)
            {
                LimitToCategories = "Underbarrel Weapons",
                ParentWeapon = objSelectedWeapon
            })
            {
                frmPickWeapon.Mounts.UnionWith(objSelectedWeapon.AccessoryMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));
                await frmPickWeapon.ShowDialogSafeAsync(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Weapons XML file and locate the selected piece.
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

                XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                List<Weapon> lstWeapons = new List<Weapon>(1);
                Weapon objWeapon = new Weapon(CharacterObject)
                {
                    ParentVehicle = objSelectedWeapon.ParentVehicle
                };
                objWeapon.Create(objXmlWeapon, lstWeapons);
                objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;
                objWeapon.Parent = objSelectedWeapon;
                if (!objSelectedWeapon.AllowAccessory)
                    objWeapon.AllowAccessory = false;

                decimal decCost = objWeapon.TotalCost;
                // Apply a markup if applicable.
                if (frmPickWeapon.Markup != 0)
                {
                    decCost *= 1 + frmPickWeapon.Markup / 100.0m;
                }

                // Multiply the cost if applicable.
                char chrAvail = objWeapon.TotalAvailTuple().Suffix;
                switch (chrAvail)
                {
                    case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                        decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                        break;

                    case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                        decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                        break;
                }

                // Check the item's Cost and make sure the character can afford it.
                if (!frmPickWeapon.FreeCost)
                {
                    if (decCost > CharacterObject.Nuyen)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return frmPickWeapon.AddAgain;
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(decCost * -1, strExpenseString + await LanguageManager.GetStringAsync("String_Space") + objWeapon.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Nuyen -= decCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeapon, objWeapon.InternalId);
                    objExpense.Undo = objUndo;
                }

                objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);

                foreach (Weapon objLoopWeapon in lstWeapons)
                {
                    if (!objSelectedWeapon.AllowAccessory)
                        objLoopWeapon.AllowAccessory = false;
                    objSelectedWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                }

                return frmPickWeapon.AddAgain;
            }
        }

        private async void tsVehicleAddUnderbarrelWeapon_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleWeaponUnderbarrel"), await LanguageManager.GetStringAsync("MessageTitle_VehicleWeaponUnderbarrel"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddUnderbarrelWeapon(objWeapon, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleWeapon"));
            }
            while (blnAddAgain);
        }

        private async void tsMartialArtsAddTechnique_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode != null)
            {
                // Select the Martial Arts node if we're currently on a child.
                if (treMartialArts.SelectedNode.Level > 1)
                    treMartialArts.SelectedNode = treMartialArts.SelectedNode.Parent;

                if (!(treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt)) return;

                bool blnAddAgain = false;
                do
                {
                    using (SelectMartialArtTechnique frmPickMartialArtTechnique = new SelectMartialArtTechnique(CharacterObject, objMartialArt))
                    {
                        await frmPickMartialArtTechnique.ShowDialogSafeAsync(this);

                        if (frmPickMartialArtTechnique.DialogResult == DialogResult.Cancel)
                            return;

                        // Open the Martial Arts XML file and locate the selected piece.
                        XmlNode xmlTechnique = (await CharacterObject.LoadDataAsync("martialarts.xml")).SelectSingleNode("/chummer/techniques/technique[id = " + frmPickMartialArtTechnique.SelectedTechnique.CleanXPath() + ']');

                        if (xmlTechnique == null)
                            continue;
                        // Create the Improvements for the Technique if there are any.
                        MartialArtTechnique objTechnique = new MartialArtTechnique(CharacterObject);
                        objTechnique.Create(xmlTechnique);
                        if (objTechnique.InternalId.IsEmptyGuid())
                            return;

                        blnAddAgain = frmPickMartialArtTechnique.AddAgain;

                        int karmaCost = objMartialArt.Techniques.Count > 0 ? CharacterObjectSettings.KarmaTechnique : 0;
                        objMartialArt.Techniques.Add(objTechnique);

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(karmaCost * -1,
                            await LanguageManager.GetStringAsync("String_ExpenseLearnTechnique") + await LanguageManager.GetStringAsync("String_Space") + objTechnique.CurrentDisplayName,
                            ExpenseType.Karma, DateTime.Now);
                        CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                        CharacterObject.Karma -= karmaCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateKarma(KarmaExpenseType.AddMartialArtTechnique, objTechnique.InternalId);
                        objExpense.Undo = objUndo;
                    }
                } while (blnAddAgain);
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectMartialArtTechnique"), await LanguageManager.GetStringAsync("MessageTitle_SelectMartialArtTechnique"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void tsVehicleAddGear_Click(object sender, EventArgs e)
        {
            Vehicle objSelectedVehicle;
            Location objLocation = null;
            switch (treVehicles.SelectedNode?.Tag)
            {
                case Vehicle vehicle:
                    objSelectedVehicle = vehicle;
                    break;

                case Location location:
                    objLocation = location;
                    objSelectedVehicle = treVehicles.SelectedNode.Parent.Tag as Vehicle;
                    break;

                default:
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGearVehicle"), await LanguageManager.GetStringAsync("MessageTitle_SelectGearVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            await PurchaseVehicleGear(objSelectedVehicle, objLocation);
        }

        private async void tsVehicleSensorAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_ModifyVehicleGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strCategories = string.Empty;
            XPathNodeIterator xmlAddonCategoryList = (await objSensor.GetNodeXPathAsync())?.Select("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                {
                    foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                        sbdCategories.Append(objXmlCategory.Value).Append(',');
                    // Remove the trailing comma.
                    --sbdCategories.Length;
                    strCategories = sbdCategories.ToString();
                }
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSensor, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseVehicleGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddVehicleGear, objGear.InternalId, frmPickGear.SelectedQty);
                            objExpense.Undo = objUndo;
                        }

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
                    }
                }
            } while (blnAddAgain);
        }

        private void cmsAmmoSingleShot_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining < objWeapon.SingleShot)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            objWeapon.AmmoRemaining -= objWeapon.SingleShot;
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsAmmoShortBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.ShortBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.ShortBurst;
            }
            else
            {
                if (objWeapon.AmmoRemaining == objWeapon.SingleShot)
                {
                    if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
                else
                {
                    if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        objWeapon.AmmoRemaining = 0;
                }
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsAmmoLongBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.LongBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.LongBurst;
            }
            else if (objWeapon.AmmoRemaining == objWeapon.SingleShot)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (objWeapon.AmmoRemaining > objWeapon.ShortBurst)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoLongBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (objWeapon.AmmoRemaining == objWeapon.ShortBurst)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                objWeapon.AmmoRemaining = 0;
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsAmmoFullBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.FullBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.FullBurst;
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoFullBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsAmmoSuppressiveFire_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.Suppressive)
            {
                objWeapon.AmmoRemaining -= objWeapon.Suppressive;
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSuppressiveFire"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsVehicleAmmoSingleShot_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining < objWeapon.SingleShot)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            objWeapon.AmmoRemaining -= objWeapon.SingleShot;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsVehicleAmmoShortBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.ShortBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.ShortBurst;
            }
            else if (objWeapon.AmmoRemaining == objWeapon.SingleShot)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                objWeapon.AmmoRemaining = 0;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsVehicleAmmoLongBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.LongBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.LongBurst;
            }
            else if (objWeapon.AmmoRemaining == objWeapon.SingleShot)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSingleShot"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (objWeapon.AmmoRemaining > objWeapon.ShortBurst)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoLongBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (objWeapon.AmmoRemaining == objWeapon.ShortBurst)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    objWeapon.AmmoRemaining = 0;
            }
            else if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoShortBurstShort"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                objWeapon.AmmoRemaining = 0;
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsVehicleAmmoFullBurst_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.FullBurst)
            {
                objWeapon.AmmoRemaining -= objWeapon.FullBurst;
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoFullBurst"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private void cmsVehicleAmmoSuppressiveFire_Click(object sender, EventArgs e)
        {
            // Locate the selected Vehicle Weapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            if (objWeapon.AmmoRemaining == 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_OutOfAmmo"), LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (objWeapon.AmmoRemaining >= objWeapon.Suppressive)
            {
                objWeapon.AmmoRemaining -= objWeapon.Suppressive;
            }
            else
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughAmmoSuppressiveFire"), LanguageManager.GetString("MessageTitle_NotEnoughAmmo"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsDirty = true;
        }

        private async void tsCyberwareSell_Click(object sender, EventArgs e)
        {
            switch (treCyberware.SelectedNode?.Tag)
            {
                case Cyberware objCyberware when objCyberware.Capacity == "[*]" && treCyberware.SelectedNode.Level == 2:
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotRemoveCyberware"), await LanguageManager.GetStringAsync("MessageTitle_CannotRemoveCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                    
                case ICanSell vendorTrash:
                    {
                        using (SellItem frmSell = new SellItem())
                        {
                            await frmSell.ShowDialogSafeAsync(this);
                            if (frmSell.DialogResult == DialogResult.Cancel)
                                return;
                            vendorTrash.Sell(frmSell.SellPercent, GlobalSettings.ConfirmDelete);
                        }

                        break;
                    }
                default:
                    Utils.BreakIfDebug();
                    break;
            }
        }

        private async void tsArmorSell_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                using (SellItem frmSell = new SellItem())
                {
                    await frmSell.ShowDialogSafeAsync(this);
                    if (frmSell.DialogResult == DialogResult.Cancel)
                        return;
                    vendorTrash.Sell(frmSell.SellPercent, GlobalSettings.ConfirmDelete);
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        private async void tsWeaponSell_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treWeapons.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                using (SellItem frmSell = new SellItem())
                {
                    await frmSell.ShowDialogSafeAsync(this);
                    if (frmSell.DialogResult == DialogResult.Cancel)
                        return;
                    vendorTrash.Sell(frmSell.SellPercent, GlobalSettings.ConfirmDelete);
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        private async void sellItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treGear.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                using (SellItem frmSell = new SellItem())
                {
                    await frmSell.ShowDialogSafeAsync(this);
                    if (frmSell.DialogResult == DialogResult.Cancel)
                        return;
                    vendorTrash.Sell(frmSell.SellPercent, GlobalSettings.ConfirmDelete);
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        private async void tsVehicleSell_Click(object sender, EventArgs e)
        {
            // Delete the selected Weapon.
            if (treVehicles.SelectedNode?.Tag is ICanSell vendorTrash)
            {
                using (SellItem frmSell = new SellItem())
                {
                    await frmSell.ShowDialogSafeAsync(this);
                    if (frmSell.DialogResult == DialogResult.Cancel)
                        return;
                    vendorTrash.Sell(frmSell.SellPercent, GlobalSettings.ConfirmDelete);
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        private async void tsAdvancedLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                Lifestyle objLifeStyle = new Lifestyle(CharacterObject);
                using (SelectLifestyleAdvanced frmPickLifestyle = new SelectLifestyleAdvanced(CharacterObject, objLifeStyle))
                {
                    await frmPickLifestyle.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    {
                        //And if it was, remove Improvements that was already added based on the lifestyle
                        foreach (LifestyleQuality lifestyleQuality in objLifeStyle.LifestyleQualities)
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, lifestyleQuality.InternalId);

                        foreach (LifestyleQuality publicGrid in objLifeStyle.FreeGrids)
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, publicGrid.InternalId);

                        return;
                    }

                    blnAddAgain = frmPickLifestyle.AddAgain;

                    Lifestyle objNewLifestyle = frmPickLifestyle.SelectedLifestyle;
                    objNewLifestyle.StyleType = LifestyleType.Advanced;

                    CharacterObject.Lifestyles.Add(objNewLifestyle);
                }
            }
            while (blnAddAgain);
        }

        private async void tsWeaponName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponName"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_WeaponName"),
                DefaultString = objWeapon.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objWeapon.CustomName = frmPickText.SelectedValue;
            }

            treWeapons.SelectedNode.Text = objWeapon.CurrentDisplayName;

            IsDirty = true;
        }

        private async void tsGearName_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGearName"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_GearName"),
                DefaultString = objGear.GearName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objGear.GearName = frmPickText.SelectedValue;
            }

            treGear.SelectedNode.Text = objGear.CurrentDisplayName;

            IsDirty = true;
        }

        private async void tsWeaponAddUnderbarrel_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objSelectedWeapon))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponAccessory"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (objSelectedWeapon.Cyberware)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareUnderbarrel"), await LanguageManager.GetStringAsync("MessageTitle_WeaponUnderbarrel"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddUnderbarrelWeapon(objSelectedWeapon, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeapon"));
            }
            while (blnAddAgain);
        }

        private void tsGearButtonAddAccessory_Click(object sender, EventArgs e)
        {
            tsGearAddAsPlugin_Click(sender, e);
        }

        private async void tsUndoKarmaExpense_Click(object sender, EventArgs e)
        {
            ListViewItem objItem = lstKarma.SelectedItems.Count > 0 ? lstKarma.SelectedItems[0] : null;

            if (objItem == null)
            {
                return;
            }

            // Find the selected Karma Expense.
            ExpenseLogEntry objExpense = (objItem.SubItems[3] as ListViewItemWithValue.ListViewSubItemWithValue)?.Value as ExpenseLogEntry;

            if (objExpense?.Undo == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoNoHistory"), await LanguageManager.GetStringAsync("MessageTitle_NoUndoHistory"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strUndoId = objExpense.Undo.ObjectId;

            if (objExpense.Undo.KarmaType == KarmaExpenseType.ImproveInitiateGrade)
            {
                // Get the grade of the item we're undoing and make sure it's the highest grade
                int intMaxGrade = 0;
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    intMaxGrade = Math.Max(intMaxGrade, objGrade.Grade);
                }
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    if (objGrade.InternalId != strUndoId)
                        continue;
                    if (objGrade.Grade < intMaxGrade)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoNotHighestGrade"), await LanguageManager.GetStringAsync("MessageTitle_NotHighestGrade"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    break;
                }
                if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoExpense"), await LanguageManager.GetStringAsync("MessageTitle_UndoExpense"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            else
            {
                if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoExpense"), await LanguageManager.GetStringAsync("MessageTitle_UndoExpense"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            switch (objExpense.Undo.KarmaType)
            {
                case KarmaExpenseType.ImproveAttribute:
                    {
                        CharacterObject.GetAttribute(strUndoId).Degrade(1);
                        break;
                    }
                case KarmaExpenseType.AddPowerPoint:
                    {
                        --CharacterObject.MysticAdeptPowerPoints;
                        break;
                    }
                case KarmaExpenseType.AddQuality:
                    {
                        // Locate the Quality that was added.
                        for (int i = CharacterObject.Qualities.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.Qualities.Count)
                                continue;
                            Quality objQuality = CharacterObject.Qualities[i];
                            if (objQuality.InternalId == strUndoId)
                                objQuality.DeleteQuality();
                        }
                    }
                    break;

                case KarmaExpenseType.AddSpell:
                    {
                        // Locate the Spell that was added.
                        for (int i = CharacterObject.Spells.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.Spells.Count)
                                continue;
                            Spell objSpell = CharacterObject.Spells[i];
                            if (objSpell.InternalId == strUndoId)
                                objSpell.Remove(false); // Remove the Spell from the character.
                        }
                        break;
                    }
                case KarmaExpenseType.SkillSpec:  //I am reasonably sure those 2 are the same. Was written looking at old AddSpecialization code
                case KarmaExpenseType.AddSpecialization:
                    {
                        SkillSpecialization objSpec = CharacterObject.SkillsSection.KnowledgeSkills
                                                                     .SelectMany(x => x.Specializations)
                                                                     .FirstOrDefault(
                                                                         x => x.InternalId == strUndoId)
                                                      ?? CharacterObject.SkillsSection.Skills
                                                                        .SelectMany(x => x.Specializations)
                                                                        .FirstOrDefault(
                                                                            x => x.InternalId == strUndoId);

                        objSpec?.Parent.Specializations.Remove(objSpec);

                        break;
                    }
                case KarmaExpenseType.ImproveSkillGroup:
                    {
                        // Locate the Skill Group that was affected.
                        SkillGroup group = CharacterObject.SkillsSection.SkillGroups.FirstOrDefault(g => g.InternalId == strUndoId);

                        if (group != null)
                        {
                            if (group.KarmaUnbroken)
                                --group.Karma;
                            else
                            {
                                Program.MainForm.ShowMessageBox(this,
                                    await LanguageManager.GetStringAsync("Message_UndoBrokenSkillGroup"),
                                    await LanguageManager.GetStringAsync("MessageTitle_UndoBrokenSkillGroup"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        break;
                    }
                case KarmaExpenseType.AddSkill:
                    {
                        // Locate the Skill that was affected.
                        Skill objSkill = CharacterObject.SkillsSection.Skills.FirstOrDefault(s => s.InternalId == strUndoId);
                        if (objSkill != null)
                        {
                            if (objSkill.AllowDelete)
                                CharacterObject.SkillsSection.Skills.Remove(objSkill);
                            else
                            {
                                objSkill.BasePoints = 0;
                                objSkill.KarmaPoints = 0;
                            }
                        }
                        else
                        {
                            KnowledgeSkill objKnowledgeSkill = CharacterObject.SkillsSection.KnowledgeSkills.FirstOrDefault(s => s.InternalId == strUndoId);
                            if (objKnowledgeSkill != null)
                            {
                                if (objKnowledgeSkill.AllowDelete)
                                    CharacterObject.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                                else
                                {
                                    objKnowledgeSkill.BasePoints = 0;
                                    objKnowledgeSkill.KarmaPoints = 0;
                                }
                            }
                        }

                        break;
                    }
                case KarmaExpenseType.ImproveSkill:
                    {
                        // Locate the Skill that was affected.
                        Skill objSkill = CharacterObject.SkillsSection.Skills.FirstOrDefault(s => s.InternalId == strUndoId) ??
                                         CharacterObject.SkillsSection.KnowledgeSkills.FirstOrDefault(s => s.InternalId == strUndoId);

                        if (objSkill != null)
                            --objSkill.Karma;

                        break;
                    }
                case KarmaExpenseType.AddMetamagic:
                    {
                        // Locate the Metamagic that was affected.
                        for (int i = CharacterObject.Metamagics.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.Metamagics.Count)
                                continue;
                            Metamagic objMetamagic = CharacterObject.Metamagics[i];
                            if (objMetamagic.InternalId == strUndoId)
                                objMetamagic.Remove(false); // Remove the Metamagic from the character.
                        }
                        break;
                    }
                case KarmaExpenseType.ImproveInitiateGrade:
                    {
                        // Locate the Initiate Grade that was affected.
                        for (int i = CharacterObject.InitiationGrades.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.InitiationGrades.Count)
                                continue;
                            InitiationGrade objGrade = CharacterObject.InitiationGrades[i];
                            if (objGrade.InternalId == strUndoId)
                                objGrade.Remove(false); // Remove the Grade from the character.
                        }
                        break;
                    }
                case KarmaExpenseType.AddMartialArt:
                    {
                        // Locate the Martial Art that was affected.
                        for (int i = CharacterObject.MartialArts.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.MartialArts.Count)
                                continue;
                            MartialArt objMartialArt = CharacterObject.MartialArts[i];
                            if (objMartialArt.InternalId == strUndoId)
                                objMartialArt.Remove(false); // Remove the Martial Art from the character.
                        }
                        break;
                    }
                case KarmaExpenseType.AddMartialArtTechnique:
                    {
                        // Locate the Martial Art Technique that was affected.
                        for (int i = CharacterObject.MartialArts.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.MartialArts.Count)
                                continue;
                            MartialArt objMartialArt = CharacterObject.MartialArts[i];
                            for (int j = objMartialArt.Techniques.Count - 1; j >= 0; --j)
                            {
                                if (j >= objMartialArt.Techniques.Count)
                                    continue;
                                MartialArtTechnique objTechnique = objMartialArt.Techniques[i];
                                if (objTechnique.InternalId == strUndoId)
                                    objTechnique.Remove(false); // Remove the Technique from the character.
                            }
                        }
                        break;
                    }

                case KarmaExpenseType.AddComplexForm:
                    {
                        // Locate the Complex Form that was affected.
                        for (int i = CharacterObject.ComplexForms.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.ComplexForms.Count)
                                continue;
                            ComplexForm objComplexForm = CharacterObject.ComplexForms[i];
                            if (objComplexForm.InternalId == strUndoId)
                                objComplexForm.Remove(false); // Remove the Complex Form from the character.
                        }
                        break;
                    }
                case KarmaExpenseType.BindFocus:
                    {
                        // Locate the Focus that was bound.
                        for (int i = CharacterObject.Foci.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.Foci.Count)
                                continue;
                            Focus objFocus = CharacterObject.Foci[i];
                            if (objFocus.InternalId != strUndoId)
                                continue;
                            TreeNode objNode = treFoci.FindNode(objFocus.InternalId);
                            if (objNode != null)
                            {
                                IsRefreshing = true;
                                objNode.Checked = false;
                                IsRefreshing = false;
                            }
                            CharacterObject.Foci.Remove(objFocus);
                        }
                        // Locate the Stacked Focus that was bound.
                        for (int i = CharacterObject.StackedFoci.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.StackedFoci.Count)
                                continue;
                            StackedFocus objStack = CharacterObject.StackedFoci[i];
                            if (objStack.InternalId != strUndoId)
                                continue;
                            TreeNode objNode = treFoci.FindNode(objStack.InternalId);
                            if (objNode == null)
                                continue;
                            IsRefreshing = true;
                            objNode.Checked = false;
                            objStack.Bonded = false;
                            IsRefreshing = false;
                        }
                        break;
                    }
                case KarmaExpenseType.JoinGroup:
                    {
                        // Remove the character from their Group.
                        IsRefreshing = true;
                        CharacterObject.GroupMember = false;
                        IsRefreshing = false;
                        break;
                    }
                case KarmaExpenseType.LeaveGroup:
                    {
                        // Put the character back in their Group.
                        IsRefreshing = true;
                        CharacterObject.GroupMember = true;
                        IsRefreshing = false;
                        break;
                    }
                case KarmaExpenseType.RemoveQuality:
                    {
                        // Add the Quality back to the character.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Quality objAddQuality = new Quality(CharacterObject);
                        XmlDocument objXmlQualityDocument = await CharacterObject.LoadDataAsync("qualities.xml");
                        XmlNode objXmlQualityNode = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strUndoId.CleanXPath() + ']')
                                                    ?? objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = " + strUndoId.CleanXPath() + ']');
                        objAddQuality.Create(objXmlQualityNode, QualitySource.Selected, lstWeapons, objExpense.Undo.Extra);

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
                        // Locate the Critter Power that was affected.
                        for (int i = CharacterObject.CritterPowers.Count - 1; i >= 0; --i)
                        {
                            if (i >= CharacterObject.CritterPowers.Count)
                                continue;
                            CritterPower objPower = CharacterObject.CritterPowers[i];
                            if (objPower.InternalId == strUndoId)
                                objPower.Remove(false); // Remove the Critter Power from the character.
                        }
                    }
                    break;
            }
            // Refund the Karma amount and remove the Expense Entry.
            CharacterObject.Karma -= objExpense.Amount.ToInt32();
            CharacterObject.ExpenseEntries.Remove(objExpense);

            IsLoading = false;

            // Select the Magician's Tradition.
            if (CharacterObject.MagicTradition.Type == TraditionType.MAG)
                cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID;
            else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                cboTradition.SelectedIndex = 0;

            // Select the Technomancer's Stream.
            if (CharacterObject.MagicTradition.Type == TraditionType.RES)
                cboStream.SelectedValue = CharacterObject.MagicTradition.SourceID;
            else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                cboStream.SelectedIndex = 0;
        }

        private async void tsUndoNuyenExpense_Click(object sender, EventArgs e)
        {
            ListViewItem objItem = lstNuyen.SelectedItems.Count > 0 ? lstNuyen.SelectedItems[0] : null;

            if (objItem == null)
            {
                return;
            }

            // Find the selected Nuyen Expense.
            ExpenseLogEntry objExpense = (objItem.SubItems[3] as ListViewItemWithValue.ListViewSubItemWithValue)?.Value as ExpenseLogEntry;

            if (objExpense?.Undo == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoNoHistory"), await LanguageManager.GetStringAsync("MessageTitle_NoUndoHistory"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strUndoId = objExpense.Undo.ObjectId;

            if (objExpense.Undo.KarmaType == KarmaExpenseType.ImproveInitiateGrade)
            {
                // Get the grade of the item we're undoing and make sure it's the highest grade
                int intMaxGrade = 0;
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    intMaxGrade = Math.Max(intMaxGrade, objGrade.Grade);
                }
                foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
                {
                    if (objGrade.InternalId != strUndoId)
                        continue;
                    if (objGrade.Grade < intMaxGrade)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoNotHighestGrade"), await LanguageManager.GetStringAsync("MessageTitle_NotHighestGrade"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    break;
                }
                if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoExpense"), await LanguageManager.GetStringAsync("MessageTitle_UndoExpense"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            else
            {
                if (Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_UndoExpense"), await LanguageManager.GetStringAsync("MessageTitle_UndoExpense"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            if (!string.IsNullOrEmpty(strUndoId))
            {
                switch (objExpense.Undo.NuyenType)
                {
                    case NuyenExpenseType.AddCyberware:
                        {
                            // Locate the Cyberware that was added.
                            Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(strUndoId) ??
                                                     CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strUndoId);
                            objCyberware?.DeleteCyberware(blnIncreaseEssenceHole: false);
                        }
                        break;

                    case NuyenExpenseType.AddGear:
                        {
                            // Locate the Gear that was added.
                            //If the gear was already deleted manually we will not be able to locate it here
                            Gear objGear = CharacterObject.Gear.DeepFindById(strUndoId);
                            TreeNode objNode;
                            if (objGear != null)
                            {
                                objNode = treGear.FindNode(objGear.InternalId);
                            }
                            else
                            {
                                objGear = CharacterObject.Vehicles.FindVehicleGear(strUndoId);
                                if (objGear != null)
                                    objNode = treVehicles.FindNode(objGear.InternalId);
                                else
                                    break;
                            }

                            objGear.Quantity -= objExpense.Undo.Qty;

                            if (objGear.Quantity <= 0)
                            {
                                objGear.DeleteGear();
                            }
                            else if (objNode != null)
                            {
                                objNode.Text = objGear.CurrentDisplayName;
                            }
                        }
                        break;

                    case NuyenExpenseType.AddVehicle:
                        {
                            // Locate the Vehicle that was added.
                            Vehicle objVehicle = CharacterObject.Vehicles.FindById(strUndoId);
                            objVehicle?.DeleteVehicle();
                        }
                        break;

                    case NuyenExpenseType.AddVehicleMod:
                        {
                            // Locate the Vehicle Mod that was added.
                            VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strUndoId);
                            objVehicleMod?.DeleteVehicleMod();
                        }
                        break;

                    case NuyenExpenseType.AddVehicleGear:
                        {
                            // Locate the Gear that was added.
                            TreeNode objNode = null;
                            Gear objGear = CharacterObject.Vehicles.FindVehicleGear(strUndoId);
                            if (objGear == null)
                            {
                                objGear = CharacterObject.Gear.DeepFindById(strUndoId);
                                if (objGear == null)
                                {
                                    objGear = CharacterObject.Cyberware.FindCyberwareGear(strUndoId);
                                    if (objGear == null)
                                    {
                                        objGear = CharacterObject.Weapons.FindWeaponGear(strUndoId);
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
                                objGear.Quantity -= objExpense.Undo.Qty;

                                // Remove the Gear if its Qty has been reduced to 0.
                                if (objGear.Quantity <= 0)
                                {
                                    objGear.DeleteGear();
                                }
                                else if (objNode != null)
                                {
                                    objNode.Text = objGear.CurrentDisplayName;
                                }
                            }
                        }
                        break;

                    case NuyenExpenseType.AddVehicleWeapon:
                        {
                            // Locate the Weapon that was added.
                            Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strUndoId) ??
                                               CharacterObject.Weapons.DeepFindById(strUndoId);
                            objWeapon?.DeleteWeapon();
                        }
                        break;

                    case NuyenExpenseType.AddVehicleWeaponAccessory:
                        {
                            // Locate the Weapon Accessory that was added.
                            WeaponAccessory objWeaponAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strUndoId) ??
                                                                 CharacterObject.Weapons.FindWeaponAccessory(strUndoId);
                            objWeaponAccessory?.DeleteWeaponAccessory();
                        }
                        break;

                    case NuyenExpenseType.AddVehicleWeaponMount:
                        {
                            WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strUndoId);
                            objWeaponMount?.DeleteWeaponMount();
                        }
                        break;

                    case NuyenExpenseType.AddVehicleWeaponMountMod:
                        {
                            VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleWeaponMountMod(strUndoId);
                            objVehicleMod?.DeleteVehicleMod();
                        }
                        break;

                    case NuyenExpenseType.AddArmor:
                        {
                            // Locate the Armor that was added.
                            Armor objArmor = CharacterObject.Armor.FindById(strUndoId);
                            objArmor?.DeleteArmor();
                        }
                        break;

                    case NuyenExpenseType.AddArmorMod:
                        {
                            // Locate the Armor Mod that was added.
                            ArmorMod objArmorMod = CharacterObject.Armor.FindArmorMod(strUndoId);
                            objArmorMod?.DeleteArmorMod();
                        }
                        break;

                    case NuyenExpenseType.AddWeapon:
                        {
                            // Locate the Weapon that was added.
                            Weapon objWeapon = CharacterObject.Weapons.DeepFindById(strUndoId) ??
                                               CharacterObject.Vehicles.FindVehicleWeapon(strUndoId);
                            objWeapon?.DeleteWeapon();
                        }
                        break;

                    case NuyenExpenseType.AddWeaponAccessory:
                        {
                            // Locate the Weapon Accessory that was added.
                            WeaponAccessory objWeaponAccessory = CharacterObject.Weapons.FindWeaponAccessory(strUndoId) ??
                                                                 CharacterObject.Vehicles.FindVehicleWeaponAccessory(strUndoId);
                            objWeaponAccessory?.DeleteWeaponAccessory();
                        }
                        break;

                    case NuyenExpenseType.IncreaseLifestyle:
                        {
                            // Locate the Lifestyle that was increased.
                            Lifestyle objLifestyle = CharacterObject.Lifestyles.FirstOrDefault(x => x.InternalId == strUndoId);
                            if (objLifestyle != null)
                                --objLifestyle.Increments;
                        }
                        break;

                    case NuyenExpenseType.AddArmorGear:
                        {
                            // Locate the Armor Gear that was added.
                            Gear objGear = CharacterObject.Armor.FindArmorGear(strUndoId);
                            if (objGear != null)
                            {
                                // Deduct the Qty from the Gear.
                                objGear.Quantity -= objExpense.Undo.Qty;

                                // Remove the Gear if its Qty has been reduced to 0.
                                if (objGear.Quantity <= 0)
                                {
                                    objGear.DeleteGear();
                                }
                                else
                                {
                                    TreeNode objNode = treArmor.FindNode(strUndoId);
                                    if (objNode != null)
                                        objNode.Text = objGear.CurrentDisplayName;
                                }
                            }
                        }
                        break;

                    case NuyenExpenseType.AddVehicleModCyberware:
                        {
                            // Locate the Cyberware that was added.
                            Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strUndoId) ??
                                                     CharacterObject.Cyberware.DeepFindById(strUndoId);
                            objCyberware?.DeleteCyberware(blnIncreaseEssenceHole: false);
                        }
                        break;

                    case NuyenExpenseType.AddCyberwareGear:
                        {
                            // Locate the Gear that was added.
                            Gear objGear = CharacterObject.Cyberware.FindCyberwareGear(strUndoId) ??
                                           CharacterObject.Vehicles.FindVehicleGear(strUndoId) ??
                                           CharacterObject.Gear.DeepFindById(strUndoId);
                            objGear?.DeleteGear();
                        }
                        break;

                    case NuyenExpenseType.AddWeaponGear:
                        {
                            // Locate the Gear that was added.
                            Gear objGear = CharacterObject.Weapons.FindWeaponGear(strUndoId) ??
                                           CharacterObject.Vehicles.FindVehicleGear(strUndoId) ??
                                           CharacterObject.Gear.DeepFindById(strUndoId);
                            objGear?.DeleteGear();
                        }
                        break;

                    case NuyenExpenseType.ManualAdd:
                    case NuyenExpenseType.ManualSubtract:
                        break;
                }
            }

            // Refund the Nuyen amount and remove the Expense Entry.
            CharacterObject.Nuyen -= objExpense.Amount;
            CharacterObject.ExpenseEntries.Remove(objExpense);
        }

        private async void tsAddArmorGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Select the root Gear node then open the Select Gear window.
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickArmorGear(objArmor.InternalId, true);
            }
            while (blnAddAgain);
        }

        private async void tsArmorGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            object objSelectedNodeTag = treArmor.SelectedNode?.Tag;
            // Make sure a parent items is selected, then open the Select Gear window.
            string strSelectedId;
            switch (objSelectedNodeTag)
            {
                case Gear objGear:
                    strSelectedId = objGear.InternalId;
                    break;

                case ArmorMod objMod:
                    {
                        strSelectedId = objMod.InternalId;
                        if (string.IsNullOrEmpty(objMod.GearCapacity))
                        {
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        break;
                    }
                default:
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            if (string.IsNullOrEmpty(strSelectedId))
                return;

            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickArmorGear(strSelectedId);
            }
            while (blnAddAgain);
        }

        private async void tsArmorNotes_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treArmor.SelectedNode);
        }

        private async void tsWeaponNotes_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treWeapons.SelectedNode);
        }

        private async void tsCyberwareNotes_Click(object sender, EventArgs e)
        {
            if (!(treCyberware.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treCyberware.SelectedNode);
        }

        private async void tsQualityNotes_Click(object sender, EventArgs e)
        {
            if (!(treQualities.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treQualities.SelectedNode);
        }

        private async void tsMartialArtsNotes_Click(object sender, EventArgs e)
        {
            if (!(treMartialArts.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treMartialArts.SelectedNode);
        }

        private async void tsSpellNotes_Click(object sender, EventArgs e)
        {
            if (!(treSpells.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treSpells.SelectedNode);
        }

        private async void tsComplexFormNotes_Click(object sender, EventArgs e)
        {
            if (!(treComplexForms.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treComplexForms.SelectedNode);
        }

        private async void tsCritterPowersNotes_Click(object sender, EventArgs e)
        {
            if (!(treCritterPowers.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treCritterPowers.SelectedNode);
        }

        private async void tsMetamagicNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treMetamagic.SelectedNode);
        }

        private async void tsGearNotes_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treGear.SelectedNode);
        }

        private async void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treVehicles.SelectedNode);
        }

        private async void tsLifestyleNotes_Click(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treLifestyles.SelectedNode);
        }

        private async void tsVehicleName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level == 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            while (treVehicles.SelectedNode.Level > 1)
            {
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;
            }

            if (!(treVehicles.SelectedNode?.Tag is IHasCustomName objRename))
                return;

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_VehicleName"),
                DefaultString = objRename.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objRename.CustomName = frmPickText.SelectedValue;
            }

            treVehicles.SelectedNode.Text = objRename.CurrentDisplayName;

            IsDirty = true;
        }

        private async void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is IHasInternalId strSelectedId))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleCyberwarePlugin"), await LanguageManager.GetStringAsync("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId.InternalId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId.InternalId, out objMod);

            if (objCyberwareParent == null && objMod?.AllowCyberware != true)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleCyberwarePlugin"), await LanguageManager.GetStringAsync("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("cyberware.xml");
            bool blnAddAgain;

            do
            {
                using (SelectCyberware frmPickCyberware = new SelectCyberware(CharacterObject, Improvement.ImprovementSource.Cyberware, objCyberwareParent ?? (object)objMod))
                {
                    if (objCyberwareParent == null)
                    {
                        //frmPickCyberware.SetGrade = "Standard";
                        frmPickCyberware.MaximumCapacity = objMod.CapacityRemaining;
                        frmPickCyberware.Subsystems = objMod.Subsystems;
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setDisallowedMounts))
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setHasMounts))
                        {
                            foreach (Cyberware objLoopCyberware in objMod.Cyberware.DeepWhere(
                                         x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                            {
                                foreach (string strLoop in objLoopCyberware.BlocksMounts.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                        setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                                }

                                string strLoopHasModularMount = objLoopCyberware.HasModularMount;
                                if (!string.IsNullOrEmpty(strLoopHasModularMount)
                                    && !setHasMounts.Contains(strLoopHasModularMount))
                                    setHasMounts.Add(strLoopHasModularMount);
                            }

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdDisallowedMounts))
                            {
                                foreach (string strLoop in setDisallowedMounts)
                                {
                                    if (!strLoop.EndsWith("Right", StringComparison.Ordinal)
                                        && (!strLoop.EndsWith("Left", StringComparison.Ordinal)
                                            || setDisallowedMounts.Contains(
                                                strLoop.Substring(0, strLoop.Length - 4) + "Right")))
                                        sbdDisallowedMounts.Append(strLoop.TrimEndOnce("Left")).Append(',');
                                }

                                // Remove trailing ","
                                if (sbdDisallowedMounts.Length > 0)
                                    --sbdDisallowedMounts.Length;
                                frmPickCyberware.DisallowedMounts = sbdDisallowedMounts.ToString();
                            }

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdHasMounts))
                            {
                                foreach (string strLoop in setHasMounts)
                                    sbdHasMounts.Append(strLoop).Append(',');
                                // Remove trailing ","
                                if (sbdHasMounts.Length > 0)
                                    --sbdHasMounts.Length;
                                frmPickCyberware.HasModularMounts = sbdHasMounts.ToString();
                            }
                        }
                    }
                    else
                    {
                        frmPickCyberware.ForcedGrade = objCyberwareParent.Grade;
                        // If the Cyberware has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that consume Capacity).
                        if (!objCyberwareParent.Capacity.Contains('['))
                        {
                            frmPickCyberware.MaximumCapacity = objCyberwareParent.CapacityRemaining;

                            // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                            if (CharacterObjectSettings.EnforceCapacity && objCyberwareParent.CapacityRemaining < 0)
                            {
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            }
                        }

                        frmPickCyberware.CyberwareParent = objCyberwareParent;
                        frmPickCyberware.Subsystems = objCyberwareParent.AllowedSubsystems;

                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setDisallowedMounts))
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setHasMounts))
                        {
                            foreach (string strLoop in objCyberwareParent.BlocksMounts.SplitNoAlloc(
                                         ',', StringSplitOptions.RemoveEmptyEntries))
                                setDisallowedMounts.Add(strLoop + objCyberwareParent.Location);
                            string strLoopHasModularMount = objCyberwareParent.HasModularMount;
                            if (!string.IsNullOrEmpty(strLoopHasModularMount))
                                setHasMounts.Add(strLoopHasModularMount);
                            foreach (Cyberware objLoopCyberware in objCyberwareParent.Children.DeepWhere(
                                         x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                            {
                                foreach (string strLoop in objLoopCyberware.BlocksMounts.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                        setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                                }

                                strLoopHasModularMount = objLoopCyberware.HasModularMount;
                                if (!string.IsNullOrEmpty(strLoopHasModularMount)
                                    && !setHasMounts.Contains(strLoopHasModularMount))
                                    setHasMounts.Add(strLoopHasModularMount);
                            }

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdDisallowedMounts))
                            {
                                foreach (string strLoop in setDisallowedMounts)
                                {
                                    if (!strLoop.EndsWith("Right", StringComparison.Ordinal)
                                        && (!strLoop.EndsWith("Left", StringComparison.Ordinal)
                                            || setDisallowedMounts.Contains(
                                                strLoop.Substring(0, strLoop.Length - 4) + "Right")))
                                        sbdDisallowedMounts.Append(strLoop.TrimEndOnce("Left")).Append(',');
                                }

                                // Remove trailing ","
                                if (sbdDisallowedMounts.Length > 0)
                                    --sbdDisallowedMounts.Length;
                                frmPickCyberware.DisallowedMounts = sbdDisallowedMounts.ToString();
                            }

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdHasMounts))
                            {
                                foreach (string strLoop in setHasMounts)
                                    sbdHasMounts.Append(strLoop).Append(',');
                                // Remove trailing ","
                                if (sbdHasMounts.Length > 0)
                                    --sbdHasMounts.Length;
                                frmPickCyberware.HasModularMounts = sbdHasMounts.ToString();
                            }
                        }
                    }

                    frmPickCyberware.LockGrade();
                    frmPickCyberware.ParentVehicle = objVehicle ?? objMod.Parent;
                    await frmPickCyberware.ShowDialogSafeAsync(this);

                    if (frmPickCyberware.DialogResult == DialogResult.Cancel)
                        break;
                    blnAddAgain = frmPickCyberware.AddAgain;

                    XmlNode objXmlCyberware = objXmlDocument.SelectSingleNode("/chummer/cyberwares/cyberware[id = " + frmPickCyberware.SelectedCyberware.CleanXPath() + ']');
                    Cyberware objCyberware = new Cyberware(CharacterObject);
                    if (!objCyberware.Purchase(objXmlCyberware, Improvement.ImprovementSource.Cyberware,
                                               frmPickCyberware.SelectedGrade, frmPickCyberware.SelectedRating,
                                               objVehicle, objMod.Cyberware, CharacterObject.Vehicles, objMod.Weapons,
                                               frmPickCyberware.Markup, frmPickCyberware.FreeCost,
                                               frmPickCyberware.BlackMarketDiscount, true,
                                               "String_ExpensePurchaseVehicleCyberware", objCyberwareParent))
                        objCyberware.DeleteCyberware();
                }
            }
            while (blnAddAgain);
        }

        private async void tsArmorName_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected.
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level == 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmorName"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            while (treArmor.SelectedNode.Level > 1)
            {
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;
            }

            if (!(treArmor.SelectedNode?.Tag is IHasCustomName objRename))
                return;

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_ArmorName"),
                DefaultString = objRename.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objRename.CustomName = frmPickText.SelectedValue;
            }

            treArmor.SelectedNode.Text = objRename.CurrentDisplayName;

            IsDirty = true;
        }

        private async void tsLifestyleName_Click(object sender, EventArgs e)
        {
            // Get the information for the currently selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is IHasCustomName objCustomName))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectLifestyleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectLifestyle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_LifestyleName"),
                DefaultString = objCustomName.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                if (objCustomName.CustomName == frmPickText.SelectedValue)
                    return;
                objCustomName.CustomName = frmPickText.SelectedValue;

                treLifestyles.SelectedNode.Text = objCustomName.CurrentDisplayName;

                IsDirty = true;
            }
        }

        private async void tsGearRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Location objLocation))
                return;
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation"),
                DefaultString = objLocation.Name
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objLocation.Name = frmPickText.SelectedValue;
            }

            treGear.SelectedNode.Text = objLocation.DisplayName();

            IsDirty = true;
        }

        private async void tsWeaponRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Location objLocation))
                return;
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation"),
                DefaultString = objLocation.Name
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objLocation.Name = frmPickText.SelectedValue;
            }

            treWeapons.SelectedNode.Text = objLocation.DisplayName();

            IsDirty = true;
        }

        private async void tsCreateSpell_Click(object sender, EventArgs e)
        {
            int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Spells");
            // Make sure the character has enough Karma before letting them select a Spell.
            if (CharacterObject.Karma < intSpellKarmaCost)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The character is still allowed to add Spells, so show the Create Spell window.
            using (CreateSpell frmSpell = new CreateSpell(CharacterObject))
            {
                await frmSpell.ShowDialogSafeAsync(this);

                if (frmSpell.DialogResult == DialogResult.Cancel)
                {
                    frmSpell.SelectedSpell.Dispose();
                    return;
                }

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
                    objSpell.Dispose();
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                    , objSpell.CurrentDisplayName
                    , intSpellKarmaCost.ToString(GlobalSettings.CultureInfo))))
                {
                    objSpell.Dispose();
                    return;
                }

                CharacterObject.Spells.Add(objSpell);

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(intSpellKarmaCost * -1, await LanguageManager.GetStringAsync("String_ExpenseLearnSpell") + await LanguageManager.GetStringAsync("String_Space") + objSpell.Name, ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intSpellKarmaCost;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddSpell, objSpell.InternalId);
                objExpense.Undo = objUndo;
            }
        }

        private async void tsImprovementNotes_Click(object sender, EventArgs e)
        {
            if (!(treImprovements.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treImprovements.SelectedNode);
        }

        private async void tsArmorRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Location objLocation))
                return;
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation"),
                DefaultString = objLocation.Name
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objLocation.Name = frmPickText.SelectedValue;
            }

            treArmor.SelectedNode.Text = objLocation.DisplayName();

            IsDirty = true;
        }

        private async void tsImprovementRenameLocation_Click(object sender, EventArgs e)
        {
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                string strNewLocation = frmPickText.SelectedValue;

                int i = -1;
                foreach (string strLocation in CharacterObject.ImprovementGroups)
                {
                    ++i;
                    if (strLocation != treImprovements.SelectedNode.Text)
                        continue;
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

        private async void tsCyberwareAddGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objCyberware))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    string strCategories = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                    {
                        foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                            sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                        if (sbdCategories.Length > 0)
                        {
                            --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }

                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objCyberware, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objCyberware.Capacity) &&
                            objCyberware.Capacity != "0" && (!objCyberware.Capacity.Contains('[') ||
                                                             objCyberware.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objCyberware.IsModularCurrentlyEquipped);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCyberwareGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, 1);
                            objExpense.Undo = objUndo;
                        }

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }

                        objCyberware.GearChildren.Add(objGear);
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleCyberwareAddGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    string strCategories = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                    {
                        foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                            sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                        if (sbdCategories.Length > 0)
                        {
                            --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }

                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objCyberware, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objCyberware.Capacity) &&
                            objCyberware.Capacity != "0" && (!objCyberware.Capacity.Contains('[') ||
                                                             objCyberware.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCyberwareGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, 1);
                            objExpense.Undo = objUndo;
                        }

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }

                        objCyberware.GearChildren.Add(objGear);
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsCyberwareGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCyberware.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level < 2)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            if (!(treCyberware.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CharacterObject.Cyberware.FindCyberwareGear(objSensor.InternalId, out Cyberware objCyberware);

            string strCategories = string.Empty;
            XPathNodeIterator xmlAddonCategoryList = (await objSensor.GetNodeXPathAsync())?.Select("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                {
                    foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                        sbdCategories.Append(objXmlCategory.Value).Append(',');
                    // Remove the trailing comma.
                    --sbdCategories.Length;
                    strCategories = sbdCategories.ToString();
                }
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSensor, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, (objSensor.Parent as Gear)?.Equipped ?? objCyberware?.IsModularCurrentlyEquipped == true);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCyberwareGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, frmPickGear.SelectedQty);
                            objExpense.Undo = objUndo;
                        }

                        objSensor.Children.Add(objGear);

                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleCyberwareGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strCategories = string.Empty;
            XPathNodeIterator xmlAddonCategoryList = (await objSensor.GetNodeXPathAsync())?.Select("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                {
                    foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                        sbdCategories.Append(objXmlCategory.Value).Append(',');
                    // Remove the trailing comma.
                    --sbdCategories.Length;
                    strCategories = sbdCategories.ToString();
                }
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSensor, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCyberwareGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddCyberwareGear, objGear.InternalId, frmPickGear.SelectedQty);
                            objExpense.Undo = objUndo;
                        }

                        objSensor.Children.Add(objGear);

                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory))
                return;
            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    string strCategories = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategories))
                    {
                        foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                            sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                        if (sbdCategories.Length > 0)
                        {
                            --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }

                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objAccessory, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objAccessory.Equipped);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeaponGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, 1);
                            objExpense.Undo = objUndo;
                        }

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }

                        objAccessory.GearChildren.Add(objGear);
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CharacterObject.Weapons.FindWeaponGear(objSensor.InternalId, out WeaponAccessory objAccessory);

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            string strCategories = string.Empty;
            XPathNodeIterator xmlAddonCategoryList = (await objSensor.GetNodeXPathAsync())?.Select("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                {
                    foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                        sbdCategories.Append(objXmlCategory.Value).Append(',');
                    // Remove the trailing comma.
                    --sbdCategories.Length;
                    strCategories = sbdCategories.ToString();
                }
            }

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSensor, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, (objSensor.Parent as Gear)?.Equipped ?? objAccessory?.Equipped == true);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeaponGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, frmPickGear.SelectedQty);
                            objExpense.Undo = objUndo;
                        }

                        objSensor.Children.Add(objGear);

                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Location objLocation))
                return;
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation"),
                DefaultString = objLocation.Name
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objLocation.Name = frmPickText.SelectedValue;
            }

            treVehicles.SelectedNode.Text = objLocation.DisplayName();

            IsDirty = true;
        }

        private async void tsCreateNaturalWeapon_Click(object sender, EventArgs e)
        {
            using (CreateNaturalWeapon frmCreateNaturalWeapon = new CreateNaturalWeapon(CharacterObject))
            {
                await frmCreateNaturalWeapon.ShowDialogSafeAsync(this);

                if (frmCreateNaturalWeapon.DialogResult == DialogResult.Cancel)
                    return;

                Weapon objWeapon = frmCreateNaturalWeapon.SelectedWeapon;
                CharacterObject.Weapons.Add(objWeapon);
            }
        }

        private async void tsVehicleWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Locate the Vehicle Sensor Gear.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            string strCategories = string.Empty;
            XPathNodeIterator xmlAddonCategoryList = (await objSensor.GetNodeXPathAsync())?.Select("addoncategory");
            if (xmlAddonCategoryList?.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
                {
                    foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                        sbdCategories.Append(objXmlCategory.Value).Append(',');
                    // Remove the trailing comma.
                    --sbdCategories.Length;
                    strCategories = sbdCategories.ToString();
                }
            }

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objSensor, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity) && (!objSensor.Capacity.Contains('[') || objSensor.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeaponGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, frmPickGear.SelectedQty);
                            objExpense.Undo = objUndo;
                        }

                        objSensor.Children.Add(objGear);
                        CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out _, out _);
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            objWeapon.ParentVehicle = objVehicle;
                            objVehicle.Weapons.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)) return;
            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory.AllowGear == null)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    string strCategories = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategories))
                    {
                        foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                            sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                        if (sbdCategories.Length > 0)
                        {
                            --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }

                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objAccessory, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        decimal decCost = objGear.TotalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objGear.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                                decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                                decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Check the item's Cost and make sure the character can afford it.
                        if (!frmPickGear.FreeCost)
                        {
                            if (decCost > CharacterObject.Nuyen)
                            {
                                objGear.DeleteGear();
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            // Create the Expense Log Entry.
                            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                            objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseWeaponGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                                DateTime.Now);
                            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                            CharacterObject.Nuyen -= decCost;

                            ExpenseUndo objUndo = new ExpenseUndo();
                            objUndo.CreateNuyen(NuyenExpenseType.AddWeaponGear, objGear.InternalId, 1);
                            objExpense.Undo = objUndo;
                        }

                        objAccessory.GearChildren.Add(objGear);

                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            objWeapon.Parent = objAccessory.Parent;
                            objAccessory.Parent.Children.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        #endregion ContextMenu Events

        #region Additional Common Tab Control Events

        private void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedQuality();
        }

        private void RefreshSelectedQuality()
        {
            // Locate the selected Quality.
            Quality objQuality = treQualities.SelectedNode?.Tag as Quality;

            UpdateQualityLevelValue(objQuality);
            if (objQuality == null)
            {
                lblQualitySourceLabel.Visible = false;
                lblQualityBPLabel.Visible = false;
                lblQualitySource.Visible = false;
                lblQualityBP.Visible = false;
            }
            else
            {
                lblQualitySourceLabel.Visible = true;
                lblQualityBPLabel.Visible = true;
                lblQualitySource.Visible = true;
                lblQualityBP.Visible = true;
                objQuality.SetSourceDetail(lblQualitySource);
                lblQualityBP.Text = (objQuality.BP * objQuality.Levels * CharacterObjectSettings.KarmaQuality).ToString(GlobalSettings.CultureInfo) +
                                    LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
            }
        }

        #endregion Additional Common Tab Control Events

        #region Additional Cyberware Tab Control Events

        private void treCyberware_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedCyberware();
            RefreshPasteStatus();
        }

        #endregion Additional Cyberware Tab Control Events

        #region Additional Street Gear Tab Control Events

        private void treWeapons_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedWeapon();
            RefreshPasteStatus();
        }

        private void treWeapons_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strSelectedWeapon = treWeapons.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedWeapon) || treWeapons.SelectedNode.Level != 1)
                return;

            // Do not allow the root element to be moved.
            if (strSelectedWeapon != "Node_SelectedWeapons")
            {
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

            TreeNode objSelected = treWeapons.SelectedNode;
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

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

            // Put the weapon in the right location (or lack thereof)
            if (treWeapons.SelectedNode.Level == 1)
                CharacterObject.MoveWeaponNode(intNewIndex, nodDestination, objSelected);
            else
                CharacterObject.MoveWeaponRoot(intNewIndex, nodDestination, objSelected);

            // Put the weapon in the right order in the tree
            MoveTreeNode(treWeapons.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treWeapons.CacheSortOrder();

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
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
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

            TreeNode objSelected = treArmor.SelectedNode;
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

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

            // Put the armor in the right location (or lack thereof)
            if (treArmor.SelectedNode.Level == 1)
                CharacterObject.MoveArmorNode(intNewIndex, nodDestination, objSelected);
            else
                CharacterObject.MoveArmorRoot(intNewIndex, nodDestination, objSelected);

            // Put the armor in the right order in the tree
            MoveTreeNode(treArmor.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treArmor.CacheSortOrder();

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
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
            treArmor.ClearNodeBackground(objNode);
        }

        private void treLifestyles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedLifestyle();
            RefreshPasteStatus();
        }

        private async void treLifestyles_DoubleClick(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            string strGuid = objLifestyle.InternalId;
            int intMonths = objLifestyle.Increments;
            int intPosition = CharacterObject.Lifestyles.IndexOf(CharacterObject.Lifestyles.FirstOrDefault(p => p.InternalId == objLifestyle.InternalId));
            string strOldLifestyleName = objLifestyle.CurrentDisplayName;
            decimal decOldLifestyleTotalCost = objLifestyle.TotalCost;

            if (objLifestyle.StyleType != LifestyleType.Standard)
            {
                Lifestyle newLifestyle = objLifestyle;
                // Edit Advanced Lifestyle.
                using (SelectLifestyleAdvanced frmPickLifestyle = new SelectLifestyleAdvanced(CharacterObject, newLifestyle))
                {
                    await frmPickLifestyle.ShowDialogSafeAsync(this);

                    if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    {
                        if (!ReferenceEquals(objLifestyle, frmPickLifestyle.SelectedLifestyle))
                            frmPickLifestyle.SelectedLifestyle.Dispose();
                        return;
                    }

                    // Update the selected Lifestyle and refresh the list.
                    objLifestyle = frmPickLifestyle.SelectedLifestyle;
                }
            }
            else
            {
                // Edit Basic Lifestyle.
                using (SelectLifestyle frmPickLifestyle = new SelectLifestyle(CharacterObject))
                {
                    frmPickLifestyle.SetLifestyle(objLifestyle);
                    await frmPickLifestyle.ShowDialogSafeAsync(this);

                    if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                    {
                        frmPickLifestyle.SelectedLifestyle.Dispose();
                        return;
                    }

                    // Update the selected Lifestyle and refresh the list.
                    objLifestyle = frmPickLifestyle.SelectedLifestyle;
                }
            }
            objLifestyle.Increments = intMonths;

            decimal decAmount = Math.Max(objLifestyle.TotalCost - decOldLifestyleTotalCost, 0);
            if (decAmount > CharacterObject.Nuyen)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            objLifestyle.SetInternalId(strGuid);
            CharacterObject.Lifestyles[intPosition] = objLifestyle;

            string strSpace = await LanguageManager.GetStringAsync("String_Space");

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(-decAmount, await LanguageManager.GetStringAsync("String_ExpenseModifiedLifestyle") + await LanguageManager.GetStringAsync("String_Space") + strOldLifestyleName + strSpace + "->" + strSpace + objLifestyle.CurrentDisplayName, ExpenseType.Nuyen, DateTime.Now);
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
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
            treLifestyles.ClearNodeBackground(objNode);
        }

        private void treGear_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedGear();
            RefreshPasteStatus();
        }

        private void chkArmorEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treArmor.SelectedNode == null)
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
                        // Add the Gear's Improvements to the character.
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

                default:
                    return;
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
            if (!(treWeapons?.SelectedNode?.Tag is Weapon objWeapon))
                return;
            objWeapon.Reload(CharacterObject.Gear, treGear);
            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkWeaponEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            // Determine if this is a Weapon.
            switch (treWeapons.SelectedNode?.Tag)
            {
                case Weapon objWeapon:
                    objWeapon.Equipped = chkWeaponEquipped.Checked;
                    break;

                case Gear objGear:
                    // Find the selected Gear.
                    objGear.Equipped = chkWeaponEquipped.Checked;
                    objGear.ChangeEquippedStatus(chkWeaponEquipped.Checked);
                    break;

                case WeaponAccessory objAccessory:
                    objAccessory.Equipped = chkWeaponEquipped.Checked;
                    break;

                default:
                    return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkIncludedInWeapon_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            // Locate the selected Weapon Accessory or Modification.
            if (!(treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory))
                return;
            objAccessory.IncludedInWeapon = chkIncludedInWeapon.Checked;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void treGear_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strSelected = treGear.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelected) || strSelected == "Node_SelectedGear")
                return;
            if (e.Button == MouseButtons.Left)
            {
                if (treGear.SelectedNode.Level > 1 || treGear.SelectedNode.Level < 0)
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

            TreeNode objSelected = treGear.SelectedNode;
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

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

            switch (_eDragButton)
            {
                // If the item was moved using the left mouse button, change the order of things.
                case MouseButtons.Left when treGear.SelectedNode.Level == 1:
                    CharacterObject.MoveGearNode(intNewIndex, nodDestination, objSelected);
                    break;

                case MouseButtons.Left:
                    CharacterObject.MoveGearRoot(intNewIndex, nodDestination, objSelected);
                    break;

                case MouseButtons.Right:
                    CharacterObject.MoveGearParent(objSelected, treGear.SelectedNode);
                    break;
            }

            // Put the gear in the right order in the tree
            MoveTreeNode(treGear.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treGear.CacheSortOrder();

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
                    objNode.BackColor = ColorManager.ControlDarker;
            }
            else
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
            treGear.ClearNodeBackground(objNode);
        }

        private void chkGearEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treGear.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            if (!(treGear.SelectedNode?.Tag is Gear objSelectedGear))
                return;
            objSelectedGear.Equipped = chkGearEquipped.Checked;
            objSelectedGear.ChangeEquippedStatus(chkGearEquipped.Checked);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cboWeaponAmmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level == 0)
                return;

            if (!(treWeapons?.SelectedNode?.Tag is Weapon objWeapon))
                return;

            objWeapon.ActiveAmmoSlot = Convert.ToInt32(cboWeaponAmmo.SelectedValue.ToString(), GlobalSettings.InvariantCultureInfo);
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkGearHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);
            }
        }

        private void chkArmorHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treArmor.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkArmorHomeNode.Checked);
            }
        }

        private void chkWeaponHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkWeaponHomeNode.Checked);
            }
        }

        private void chkCyberwareHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);
            }
        }

        private async void cmdWeaponBuyAmmo_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
                return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickGear(null, null, null, string.Empty, objWeapon);
            }
            while (blnAddAgain);
        }

        private async void cmdWeaponMoveToVehicle_Click(object sender, EventArgs e)
        {
            // Locate the selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon)) return;

            List<Vehicle> lstVehicles = new List<Vehicle>(CharacterObject.Vehicles.Count);
            foreach (Vehicle objCharacterVehicle in CharacterObject.Vehicles)
            {
                if (objCharacterVehicle.WeaponMounts.Count > 0
                    || objCharacterVehicle.Mods.Any(objVehicleMod => objVehicleMod.Name.Contains("Drone Arm")
                                                                     || objVehicleMod.Name.StartsWith("Mechanical Arm", StringComparison.Ordinal)))
                {
                    lstVehicles.Add(objCharacterVehicle);
                }
            }

            // Cannot continue if there are no Vehicles with a Weapon Mount or Mechanical Arm.
            if (lstVehicles.Count == 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotMoveWeapons"), await LanguageManager.GetStringAsync("MessageTitle_CannotMoveWeapons"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            WeaponMount objWeaponMount;
            VehicleMod objMod = null;

            using (SelectItem frmPickItem = new SelectItem())
            {
                frmPickItem.SetVehiclesMode(lstVehicles);
                await frmPickItem.ShowDialogSafeAsync(this);

                if (frmPickItem.DialogResult == DialogResult.Cancel)
                    return;

                // Locate the selected Vehicle.
                Vehicle objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == frmPickItem.SelectedItem);
                if (objVehicle == null)
                    return;

                // Now display a list of the acceptable mounting points for the Weapon.
                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
                {
                    foreach (WeaponMount objVehicleWeaponMount in objVehicle.WeaponMounts)
                    {
                        //TODO: RAW, some mounts can have multiple weapons attached. Needs support in the Weapon Mount class itself, ideally a 'CanMountThisWeapon' bool or something.
                        if ((objVehicleWeaponMount.AllowedWeaponCategories.Contains(objWeapon.SizeCategory) ||
                             objVehicleWeaponMount.AllowedWeapons.Contains(objWeapon.Name)) &&
                            !objVehicleWeaponMount.IsWeaponsFull)
                            lstItems.Add(new ListItem(objVehicleWeaponMount.InternalId,
                                                      objVehicleWeaponMount.CurrentDisplayName));
                        else
                            foreach (VehicleMod objVehicleMod in objVehicleWeaponMount.Mods)
                            {
                                if ((objVehicleMod.Name.Contains("Drone Arm") ||
                                     objVehicleMod.Name.StartsWith("Mechanical Arm", StringComparison.Ordinal)) &&
                                    objVehicleMod.Weapons.Count == 0)
                                    lstItems.Add(new ListItem(objVehicleMod.InternalId,
                                                              objVehicleMod.CurrentDisplayName));
                            }
                    }

                    foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                    {
                        if ((objVehicleMod.Name.Contains("Drone Arm") ||
                             objVehicleMod.Name.StartsWith("Mechanical Arm", StringComparison.Ordinal))
                            && objVehicleMod.Weapons.Count == 0)
                            lstItems.Add(new ListItem(objVehicleMod.InternalId, objVehicleMod.CurrentDisplayName));
                    }

                    if (lstItems.Count == 0)
                    {
                        Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NoValidWeaponMount"),
                                                        await LanguageManager.GetStringAsync("MessageTitle_NoValidWeaponMount"),
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    frmPickItem.SetGeneralItemsMode(lstItems);
                    await frmPickItem.ShowDialogSafeAsync(this);

                    if (frmPickItem.DialogResult == DialogResult.Cancel)
                        return;

                    string strId = frmPickItem.SelectedItem;
                    // Locate the selected Vehicle Mod.
                    objWeaponMount = objVehicle.WeaponMounts.FirstOrDefault(x => x.InternalId == strId);
                    // Locate the selected Vehicle Mod.
                    if (objWeaponMount == null)
                    {
                        objMod = objVehicle.FindVehicleMod(x => x.InternalId == strId, out objWeaponMount);
                        if (objMod == null)
                            return;
                    }
                }
            }

            objWeapon.Location = null;
            // Remove the Weapon from the character and add it to the Vehicle Mod.
            CharacterObject.Weapons.Remove(objWeapon);

            // Remove any Improvements from the Character.
            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
            {
                foreach (Gear objGear in objAccessory.GearChildren)
                    objGear.ChangeEquippedStatus(false);
            }
            if (objWeapon.UnderbarrelWeapons.Count > 0)
            {
                foreach (Weapon objUnderbarrelWeapon in objWeapon.UnderbarrelWeapons)
                {
                    foreach (WeaponAccessory objAccessory in objUnderbarrelWeapon.WeaponAccessories)
                    {
                        foreach (Gear objGear in objAccessory.GearChildren)
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
                objWeapon.ParentVehicleMod = objMod;
                objMod.Weapons.Add(objWeapon);
            }
        }

        private void cmdArmorIncrease_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
                return;

            --objArmor.ArmorDamage;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdArmorDecrease_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
                return;

            ++objArmor.ArmorDamage;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkCommlinks_CheckedChanged(object sender, EventArgs e)
        {
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
        }

        private void chkGearActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkGearActiveCommlink.Checked);
        }

        private void chkArmorActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkArmorActiveCommlink.Checked);
        }

        private void chkWeaponActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkWeaponActiveCommlink.Checked);
        }

        private void chkCyberwareActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);
        }

        private void chkVehicleActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);
        }

        private void cboGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearAttack, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearSleaze.Enabled)
                return;

            IsRefreshing = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearSleaze, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearDataProcessing.Enabled)
                return;

            IsRefreshing = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearDataProcessing, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearFirewall.Enabled)
                return;

            IsRefreshing = true;

            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearFirewall, cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleAttack, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleSleaze.Enabled)
                return;

            IsRefreshing = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleSleaze, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleFirewall.Enabled)
                return;

            IsRefreshing = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleFirewall, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleDataProcessing.Enabled)
                return;

            IsRefreshing = true;

            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleDataProcessing, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboArmorAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboArmorAttack, cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboArmorSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboArmorSleaze, cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboArmorDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboArmorDataProcessing, cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboArmorFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboArmorFirewall, cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboCyberwareAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareAttack, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboCyberwareSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareSleaze.Enabled)
                return;

            IsRefreshing = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareSleaze, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboCyberwareDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareDataProcessing.Enabled)
                return;

            IsRefreshing = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareDataProcessing, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboCyberwareFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboCyberwareFirewall.Enabled)
                return;

            IsRefreshing = true;

            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareFirewall, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboWeaponGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboWeaponGearAttack.Enabled)
                return;

            IsRefreshing = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboWeaponGearAttack, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboWeaponGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboWeaponGearSleaze.Enabled)
                return;

            IsRefreshing = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboWeaponGearSleaze, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboWeaponGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboWeaponGearDataProcessing.Enabled)
                return;

            IsRefreshing = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboWeaponGearDataProcessing, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboWeaponGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboWeaponGearFirewall.Enabled)
                return;

            IsRefreshing = true;

            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboWeaponGearFirewall, cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        #endregion Additional Street Gear Tab Control Events

        #region Additional Vehicle Tab Control Events

        private void treVehicles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedVehicle();
            RefreshPasteStatus();
        }

        private void treVehicles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            switch (treVehicles.SelectedNode?.Tag)
            {
                // Determine if this is a piece of Gear or a Vehicle. If not, don't let the user drag it.
                case Gear _:
                    _eDragButton = e.Button;
                    _blnDraggingGear = true;
                    _intDragLevel = treVehicles.SelectedNode.Level;
                    DoDragDrop(e.Item, DragDropEffects.Move);
                    break;

                case Vehicle _:
                    _eDragButton = e.Button;
                    _blnDraggingGear = false;
                    _intDragLevel = treVehicles.SelectedNode.Level;
                    DoDragDrop(e.Item, DragDropEffects.Move);
                    break;
            }
        }

        private void treVehicles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treVehicles_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

            TreeNode objSelected = treVehicles.SelectedNode;
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

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
                CharacterObject.MoveVehicleNode(intNewIndex, nodDestination, objSelected);
            }
            else
            {
                CharacterObject.MoveVehicleGearParent(nodDestination, objSelected);
            }

            // Put the armor in the right order in the tree
            MoveTreeNode(treVehicles.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treVehicles.CacheSortOrder();

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
                    objNode.BackColor = ColorManager.ControlDarker;
            }
            else
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
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
            if (!(treVehicles?.SelectedNode?.Tag is Weapon objWeapon))
                return;
            objWeapon.Reload(objWeapon.ParentVehicle.GearChildren, treVehicles);
            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treVehicles.SelectedNode?.Tag is ICanEquip objEquippable))
                return;
            objEquippable.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;

            IsDirty = true;
        }

        private void cboVehicleWeaponAmmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            objWeapon.ActiveAmmoSlot = Convert.ToInt32(cboVehicleWeaponAmmo.SelectedValue.ToString(), GlobalSettings.InvariantCultureInfo);
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
                return;
            objTarget.SetHomeNode(CharacterObject, chkVehicleHomeNode.Checked);
        }

        #endregion Additional Vehicle Tab Control Events

        #region Additional Spells and Spirits Tab Control Events

        private void treSpells_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedSpell();
        }

        private void treFoci_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!e.Node.Checked)
            {
                if (!(e.Node.Tag is IHasInternalId objId)) return;
                Focus objFocus = CharacterObject.Foci.Find(x => x.GearObject.InternalId == objId.InternalId);

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
                    StackedFocus objStack = CharacterObject.StackedFoci.Find(x => x.InternalId == objId.InternalId);

                    if (objStack != null)
                    {
                        objStack.Bonded = false;
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                    }
                }
            }
        }

        private void treFoci_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (IsRefreshing)
                return;

            // If the item is being unchecked, confirm that the user wants to un-bind the Focus.
            if (e.Node.Checked)
            {
                if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_UnbindFocus"), LanguageManager.GetString("MessageTitle_UnbindFocus"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
                        objSelectedFocus = objGear;
                        intFociTotal = objGear.Rating;
                        break;
                    }
                case StackedFocus objStackedFocus:
                    {
                        intFociTotal = objStackedFocus.TotalForce;
                        break;
                    }
            }

            // Run through the list of items. Count the number of Foci the character would have bonded including this one, plus the total Force of all checked Foci.
            foreach (TreeNode objNode in treFoci.Nodes)
            {
                if (objNode.Checked)
                {
                    string strNodeId = objNode.Tag.ToString();
                    ++intFociCount;
                    intFociTotal += CharacterObject.Gear.FirstOrDefault(x => x.InternalId == strNodeId && x.Bonded)?.Rating ?? 0;
                    intFociTotal += CharacterObject.StackedFoci.Find(x => x.InternalId == strNodeId && x.Bonded)?.TotalForce ?? 0;
                }
            }

            if (!CharacterObject.IgnoreRules)
            {
                if (intFociTotal > CharacterObject.MAG.TotalValue * 5 ||
                    CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && intFociTotal > CharacterObject.MAGAdept.TotalValue * 5)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_FocusMaximumForce"), LanguageManager.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }

                if (intFociCount > CharacterObject.MAG.TotalValue ||
                    CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && intFociCount > CharacterObject.MAGAdept.TotalValue)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_FocusMaximumNumber"), LanguageManager.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (objSelectedFocus.Bonus != null || objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null)
                {
                    if (!string.IsNullOrEmpty(objSelectedFocus.Extra))
                        ImprovementManager.ForcedValue = objSelectedFocus.Extra;
                    if (objSelectedFocus.Bonus != null)
                    {
                        if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.Bonus, objSelectedFocus.Rating, objSelectedFocus.DisplayNameShort(GlobalSettings.Language)))
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
                    if (objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null && !ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.WirelessBonus, objSelectedFocus.Rating, objSelectedFocus.DisplayNameShort(GlobalSettings.Language)))
                    {
                        // Clear created improvements
                        objSelectedFocus.ChangeEquippedStatus(false);
                        if (blnOldEquipped)
                            objSelectedFocus.ChangeEquippedStatus(true);
                        e.Cancel = true;
                        return;
                    }
                }

                int intKarmaExpense = objFocus.BindingKarmaCost();
                if (intKarmaExpense > CharacterObject.Karma)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Clear created improvements
                    objSelectedFocus.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objSelectedFocus.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseFocus")
                    , intKarmaExpense.ToString(GlobalSettings.CultureInfo)
                    , objSelectedFocus.DisplayNameShort(GlobalSettings.Language))))
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
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseBound") + LanguageManager.GetString("String_Space") + objSelectedFocus.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
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

                e.Node.Text = objSelectedFocus.CurrentDisplayName;
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
                    if (objGear.Bonus != null || objGear.WirelessOn && objGear.WirelessBonus != null)
                    {
                        if (!string.IsNullOrEmpty(objGear.Extra))
                            ImprovementManager.ForcedValue = objGear.Extra;
                        if (objGear.Bonus != null)
                        {
                            if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStackedFocus.InternalId, objGear.Bonus, objGear.Rating, objGear.DisplayNameShort(GlobalSettings.Language)))
                            {
                                e.Cancel = true;
                                break;
                            }
                            objGear.Extra = ImprovementManager.SelectedValue;
                        }
                        if (objGear.WirelessOn && objGear.WirelessBonus != null && !ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStackedFocus.InternalId, objGear.WirelessBonus, objGear.Rating, objGear.DisplayNameShort(GlobalSettings.Language)))
                        {
                            e.Cancel = true;
                            break;
                        }
                    }
                }

                if (e.Cancel)
                {
                    // Clear created improvements
                    foreach (Gear objGear in objStackedFocus.Gear)
                        objGear.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        foreach (Gear objGear in objStackedFocus.Gear)
                            objGear.ChangeEquippedStatus(true);
                    return;
                }

                int intKarmaExpense = objStackedFocus.BindingCost;
                if (intKarmaExpense > CharacterObject.Karma)
                {
                    Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Clear created improvements
                    objStackGear.ChangeEquippedStatus(false);
                    if (blnOldEquipped)
                        objStackGear.ChangeEquippedStatus(true);
                    e.Cancel = true;
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseFocus")
                    , intKarmaExpense.ToString(GlobalSettings.CultureInfo)
                    , LanguageManager.GetString("String_StackedFocus") + LanguageManager.GetString("String_Space") + objStackedFocus.CurrentDisplayName)))
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
                objExpense.Create(intKarmaExpense * -1, LanguageManager.GetString("String_ExpenseBound") + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_StackedFocus") + LanguageManager.GetString("String_Space") + objStackedFocus.CurrentDisplayName, ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Karma -= intKarmaExpense;

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.BindFocus, objStackedFocus.InternalId);
                objExpense.Undo = objUndo;

                objStackedFocus.Bonded = true;
                treFoci.SelectedNode.Text = objStackGear.CurrentDisplayName;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cboTradition_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TODO: Why can't IsInitialized be used here? Throws an error when trying to use chummer.helpers.

            if (IsLoading || IsRefreshing || CharacterObject.MagicTradition.Type == TraditionType.RES)
                return;
            string strSelectedId = cboTradition.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            XmlNode xmlTradition = CharacterObject.LoadData("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[id = " + strSelectedId.CleanXPath() + ']');

            if (xmlTradition == null)
            {
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

                CharacterObject.MagicTradition.ResetTradition();

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            else if (strSelectedId == Tradition.CustomMagicalTraditionGuid)
            {
                if (CharacterObject.MagicTradition.Create(xmlTradition))
                {
                    lblTraditionName.Visible = true;
                    txtTraditionName.Visible = true;
                    lblSpiritCombat.Visible = true;
                    lblSpiritDetection.Visible = true;
                    lblSpiritHealth.Visible = true;
                    lblSpiritIllusion.Visible = true;
                    lblSpiritManipulation.Visible = true;
                    lblTraditionSource.Visible = false;
                    lblTraditionSourceLabel.Visible = false;
                    cboSpiritCombat.Enabled = true;
                    cboSpiritDetection.Enabled = true;
                    cboSpiritHealth.Enabled = true;
                    cboSpiritIllusion.Enabled = true;
                    cboSpiritManipulation.Enabled = true;
                    cboSpiritCombat.Visible = true;
                    cboSpiritDetection.Visible = true;
                    cboSpiritHealth.Visible = true;
                    cboSpiritIllusion.Visible = true;
                    cboSpiritManipulation.Visible = true;

                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
                else
                {
                    CharacterObject.MagicTradition.ResetTradition();
                    cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID;
                }
            }
            else
            {
                if (CharacterObject.MagicTradition.Create(xmlTradition))
                {
                    lblTraditionName.Visible = false;
                    txtTraditionName.Visible = false;
                    lblSpiritCombat.Visible = true;
                    lblSpiritDetection.Visible = true;
                    lblSpiritHealth.Visible = true;
                    lblSpiritIllusion.Visible = true;
                    lblSpiritManipulation.Visible = true;
                    cboSpiritCombat.Enabled = false;
                    cboSpiritDetection.Enabled = false;
                    cboSpiritHealth.Enabled = false;
                    cboSpiritIllusion.Enabled = false;
                    cboSpiritManipulation.Enabled = false;
                    cboSpiritCombat.Visible = true;
                    cboSpiritDetection.Visible = true;
                    cboSpiritHealth.Visible = true;
                    cboSpiritIllusion.Visible = true;
                    cboSpiritManipulation.Visible = true;

                    lblTraditionSource.Visible = true;
                    lblTraditionSourceLabel.Visible = true;
                    CharacterObject.MagicTradition.SetSourceDetail(lblTraditionSource);

                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
                else
                {
                    CharacterObject.MagicTradition.ResetTradition();
                    cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID;
                }
            }

            cboDrain.Visible = (!CharacterObject.AdeptEnabled || CharacterObject.MagicianEnabled) &&
                               CharacterObject.MagicTradition.CanChooseDrainAttribute;
        }

        #endregion Additional Spells and Spirits Tab Control Events

        #region Additional Sprites and Complex Forms Tab Control Events

        private void treComplexForms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedComplexForm();
        }

        private void cboStream_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || CharacterObject.MagicTradition.Type != TraditionType.MAG)
                return;
            string strSelectedId = cboStream.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId) || strSelectedId == CharacterObject.MagicTradition.SourceIDString)
                return;

            XmlNode xmlNewStreamNode = CharacterObject.LoadData("streams.xml").SelectSingleNode("/chummer/traditions/tradition[id = " + strSelectedId.CleanXPath() + ']');
            if (xmlNewStreamNode != null && CharacterObject.MagicTradition.Create(xmlNewStreamNode, true))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            else
            {
                CharacterObject.MagicTradition.ResetTradition();
                cboStream.SelectedValue = CharacterObject.MagicTradition.SourceID;
            }
        }

        #endregion Additional Sprites and Complex Forms Tab Control Events

        #region Additional Initiation Tab Control Events

        private void chkInitiationGroup_EnabledChanged(object sender, EventArgs e)
        {
            if (!chkInitiationGroup.Enabled)
            {
                chkInitiationGroup.Checked = false;
            }
        }

        private void chkInitiationSchooling_EnabledChanged(object sender, EventArgs e)
        {
            if (!chkInitiationSchooling.Enabled)
            {
                chkInitiationSchooling.Checked = false;
            }
        }

        private void treComplexForms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteComplexForm_Click(sender, e);
            }
        }

        private void treMetamagic_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treMetamagic.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblMetamagicSourceLabel.Visible = true;
                lblMetamagicSource.Visible = true;
                objSelected.SetSourceDetail(lblMetamagicSource);
            }
            else
            {
                lblMetamagicSourceLabel.Visible = false;
                lblMetamagicSource.Visible = false;
            }
            switch (treMetamagic.SelectedNode?.Tag)
            {
                case Metamagic objMetamagic:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objMetamagic.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objMetamagic.Grade >= 0;
                        break;
                    }
                case Art objArt:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objArt.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objArt.Grade >= 0;
                        break;
                    }
                case Spell objSpell:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveMetamagic");
                        cmdDeleteMetamagic.Enabled = objSpell.Grade >= 0;
                        break;
                    }
                case ComplexForm objComplexForm:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objComplexForm.Grade >= 0;
                        break;
                    }
                case Enhancement objEnhancement:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objEnhancement.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objEnhancement.Grade >= 0;
                        break;
                    }
                default:
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(CharacterObject.MAGEnabled ? "Button_RemoveInitiateGrade" : "Button_RemoveSubmersionGrade");
                    cmdDeleteMetamagic.Enabled = true;
                    lblMetamagicSource.Text = string.Empty;
                    lblMetamagicSource.SetToolTip(string.Empty);
                    break;
            }
        }

        private void chkJoinGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || IsLoading || chkJoinGroup.Checked == CharacterObject.GroupMember)
                return;

            // Joining a Network does not cost Karma for Technomancers, so this only applies to Magicians/Adepts.
            if (CharacterObject.MAGEnabled)
            {
                if (chkJoinGroup.Checked)
                {
                    int intKarmaExpense = CharacterObjectSettings.KarmaJoinGroup;

                    if (intKarmaExpense > CharacterObject.Karma)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        IsRefreshing = true;
                        chkJoinGroup.Checked = false;
                        IsRefreshing = false;
                        return;
                    }

                    string strMessage;
                    string strExpense;
                    if (CharacterObject.MAGEnabled)
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseJoinGroup");
                        strExpense = LanguageManager.GetString("String_ExpenseJoinGroup");
                    }
                    else
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseJoinNetwork");
                        strExpense = LanguageManager.GetString("String_ExpenseJoinNetwork");
                    }

                    if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, strMessage, intKarmaExpense.ToString(GlobalSettings.CultureInfo))))
                    {
                        IsRefreshing = true;
                        chkJoinGroup.Checked = false;
                        IsRefreshing = false;
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
                    int intKarmaExpense = CharacterObjectSettings.KarmaLeaveGroup;

                    if (intKarmaExpense > CharacterObject.Karma)
                    {
                        Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        IsRefreshing = true;
                        chkJoinGroup.Checked = true;
                        IsRefreshing = false;
                        return;
                    }

                    string strMessage;
                    string strExpense;
                    if (CharacterObject.MAGEnabled)
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseLeaveGroup");
                        strExpense = LanguageManager.GetString("String_ExpenseLeaveGroup");
                    }
                    else
                    {
                        strMessage = LanguageManager.GetString("Message_ConfirmKarmaExpenseLeaveNetwork");
                        strExpense = LanguageManager.GetString("String_ExpenseLeaveNetwork");
                    }

                    if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, strMessage, intKarmaExpense.ToString(GlobalSettings.CultureInfo))))
                    {
                        IsRefreshing = true;
                        chkJoinGroup.Checked = true;
                        IsRefreshing = false;
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

            //TODO: If using a databinding for GroupMember, changing Karma here causes chkJoinGroup to revert to false. Unclear why, lazy fix to resolve it for now.
            CharacterObject.GroupMember = chkJoinGroup.Checked;

            if (!chkJoinGroup.Enabled)
            {
                chkInitiationGroup.Checked = false;
            }
        }

        private void txtNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                ((TextBox)sender)?.SelectAll();
            }
        }

        #endregion Additional Initiation Tab Control Events

        #region Additional Critter Powers Tab Control Events

        private void treCritterPowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Look for the selected Critter Power.
            if (treCritterPowers.SelectedNode?.Tag is CritterPower objPower)
            {
                cmdDeleteCritterPower.Enabled = objPower.Grade == 0;
                lblCritterPowerName.Text = objPower.CurrentDisplayName;
                lblCritterPowerCategory.Text = objPower.DisplayCategory(GlobalSettings.Language);
                lblCritterPowerType.Text = objPower.DisplayType(GlobalSettings.Language);
                lblCritterPowerAction.Text = objPower.DisplayAction(GlobalSettings.Language);
                lblCritterPowerRange.Text = objPower.DisplayRange(GlobalSettings.Language);
                lblCritterPowerDuration.Text = objPower.DisplayDuration(GlobalSettings.Language);
                chkCritterPowerCount.Checked = objPower.CountTowardsLimit;
                objPower.SetSourceDetail(lblCritterPowerSource);
                if (objPower.PowerPoints > 0)
                {
                    lblCritterPowerPointCost.Text = objPower.PowerPoints.ToString(GlobalSettings.CultureInfo);
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
                lblCritterPowerSource.SetToolTip(null);
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

        #endregion Additional Critter Powers Tab Control Events

        #region Additional Karma and Nuyen Tab Control Events

        private async void lstKarma_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem objItem = lstKarma.SelectedItems.Count > 0 ? lstKarma.SelectedItems[0] : null;
            if (objItem == null)
            {
                return;
            }

            // Find the selected Karma Expense.
            ExpenseLogEntry objExpense = (objItem.SubItems[3] as ListViewItemWithValue.ListViewSubItemWithValue)?.Value as ExpenseLogEntry
                                         ?? new ExpenseLogEntry(CharacterObject);

            // If this is a manual entry, let the player modify the amount.
            int intOldAmount = objExpense.Amount.ToInt32();
            bool blnAllowEdit = objExpense.Undo != null && (objExpense.Undo.KarmaType == KarmaExpenseType.ManualAdd ||
                                                            objExpense.Undo.KarmaType ==
                                                            KarmaExpenseType.ManualSubtract);

            bool blnDoRepopulateList;
            using (CreateExpense frmEditExpense = new CreateExpense(CharacterObjectSettings)
                   {
                       Reason = objExpense.Reason,
                       Amount = objExpense.Amount,
                       Refund = objExpense.Refund,
                       SelectedDate = objExpense.Date,
                       ForceCareerVisible = objExpense.ForceCareerVisible
                   })
            {
                frmEditExpense.LockFields(blnAllowEdit);

                await frmEditExpense.ShowDialogSafeAsync(this);

                if (frmEditExpense.DialogResult == DialogResult.Cancel)
                    return;

                // If this is a manual entry, update the character's Karma total.
                int intNewAmount = frmEditExpense.Amount.ToInt32();
                if (blnAllowEdit && intOldAmount != intNewAmount)
                {
                    objExpense.Amount = intNewAmount;
                    CharacterObject.Karma += intNewAmount - intOldAmount;
                    blnDoRepopulateList = true;
                }
                else
                    blnDoRepopulateList = intNewAmount != 0 || chkShowFreeKarma.Checked;

                // Rename the Expense.
                objExpense.Reason = frmEditExpense.Reason;
                objExpense.Date = frmEditExpense.SelectedDate;
            }

            if (blnDoRepopulateList)
                RepopulateKarmaExpenseList();

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void lstNuyen_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem objItem = lstNuyen.SelectedItems.Count > 0 ? lstNuyen.SelectedItems[0] : null;
            if (objItem == null)
            {
                return;
            }

            // Find the selected Nuyen Expense.
            ExpenseLogEntry objExpense = (objItem.SubItems[3] as ListViewItemWithValue.ListViewSubItemWithValue)?.Value as ExpenseLogEntry
                                         ?? new ExpenseLogEntry(CharacterObject);

            // If this is a manual entry, let the player modify the amount.
            decimal decOldAmount = objExpense.Amount;
            bool blnAllowEdit = objExpense.Undo != null && (objExpense.Undo.NuyenType == NuyenExpenseType.ManualAdd ||
                                                            objExpense.Undo.NuyenType ==
                                                            NuyenExpenseType.ManualSubtract);

            bool blnDoRepopulateList;
            using (CreateExpense frmEditExpense = new CreateExpense(CharacterObjectSettings)
                   {
                       Mode = ExpenseType.Nuyen,
                       Reason = objExpense.Reason,
                       Amount = objExpense.Amount,
                       Refund = objExpense.Refund,
                       SelectedDate = objExpense.Date,
                       ForceCareerVisible = objExpense.ForceCareerVisible
                   })
            {
                frmEditExpense.LockFields(blnAllowEdit);

                await frmEditExpense.ShowDialogSafeAsync(this);

                if (frmEditExpense.DialogResult == DialogResult.Cancel)
                    return;

                // If this is a manual entry, update the character's Karma total.
                decimal decNewAmount = frmEditExpense.Amount;
                if (blnAllowEdit && decOldAmount != decNewAmount)
                {
                    objExpense.Amount = decNewAmount;
                    CharacterObject.Nuyen += decNewAmount - decOldAmount;
                    blnDoRepopulateList = true;
                }
                else
                    blnDoRepopulateList = decNewAmount != 0 || chkShowFreeNuyen.Checked;

                // Rename the Expense.
                objExpense.Reason = frmEditExpense.Reason;
                objExpense.Date = frmEditExpense.SelectedDate;
            }

            if (blnDoRepopulateList)
                RepopulateNuyenExpenseList();

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

        #endregion Additional Karma and Nuyen Tab Control Events

        #region Additional Calendar Tab Control Events

        private void lstCalendar_DoubleClick(object sender, EventArgs e)
        {
            cmdEditWeek_Click(sender, e);
        }

        #endregion Additional Calendar Tab Control Events

        #region Additional Improvements Tab Control Events

        private void treImprovements_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IsRefreshing = true;
            if (treImprovements.SelectedNode?.Tag is Improvement objImprovement)
            {
                // Get the human-readable name of the Improvement from the Improvements file.

                XmlNode objNode = CharacterObject.LoadData("improvements.xml").SelectSingleNode("/chummer/improvements/improvement[id = " + objImprovement.CustomId.CleanXPath() + ']');
                if (objNode != null)
                {
                    lblImprovementType.Text = objNode["translate"]?.InnerText ?? objNode["name"]?.InnerText;
                }

                string strSpace = LanguageManager.GetString("String_Space");
                // Build a string that contains the value(s) of the Improvement.
                string strValue = string.Empty;
                if (objImprovement.Value != 0)
                    strValue += LanguageManager.GetString("Label_CreateImprovementValue") + strSpace + objImprovement.Value.ToString(GlobalSettings.CultureInfo) + ',' + strSpace;
                if (objImprovement.Minimum != 0)
                    strValue += LanguageManager.GetString("Label_CreateImprovementMinimum") + strSpace + objImprovement.Minimum.ToString(GlobalSettings.CultureInfo) + ',' + strSpace;
                if (objImprovement.Maximum != 0)
                    strValue += LanguageManager.GetString("Label_CreateImprovementMaximum") + strSpace + objImprovement.Maximum.ToString(GlobalSettings.CultureInfo) + ',' + strSpace;
                if (objImprovement.Augmented != 0)
                    strValue += LanguageManager.GetString("Label_CreateImprovementAugmented") + strSpace + objImprovement.Augmented.ToString(GlobalSettings.CultureInfo) + ',' + strSpace;

                // Remove the trailing comma.
                if (!string.IsNullOrEmpty(strValue))
                    strValue = strValue.Substring(0, strValue.Length - 1 - strSpace.Length);

                cmdImprovementsEnableAll.Visible = false;
                cmdImprovementsDisableAll.Visible = false;
                lblImprovementValue.Text = strValue;
                chkImprovementActive.Checked = objImprovement.Enabled;
                chkImprovementActive.Visible = true;
            }
            else if (treImprovements.SelectedNode?.Level == 0)
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
                cmdImprovementsEnableAll.Visible = false;
                cmdImprovementsDisableAll.Visible = false;
                lblImprovementType.Text = string.Empty;
                lblImprovementValue.Text = string.Empty;
                chkImprovementActive.Checked = false;
                chkImprovementActive.Visible = false;
            }
            IsRefreshing = false;
        }

        private void treImprovements_DoubleClick(object sender, EventArgs e)
        {
            if (treImprovements.SelectedNode?.Tag is Improvement)
            {
                cmdEditImprovement_Click(sender, e);
            }
            else
            {
                cmdAddImprovement_Click(sender, e);
            }
        }

        private void chkImprovementActive_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treImprovements.SelectedNode?.Tag is Improvement objImprovement)
            {
                if (chkImprovementActive.Checked)
                    ImprovementManager.EnableImprovements(CharacterObject, objImprovement);
                else
                    ImprovementManager.DisableImprovements(CharacterObject, objImprovement);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
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

            TreeNode objSelected = treImprovements.SelectedNode;
            for (TreeNode nodLoop = nodDestination; nodLoop != null; nodLoop = nodLoop.Parent)
            {
                if (nodLoop == objSelected)
                    return;
            }

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
                CharacterObject.MoveImprovementNode(nodDestination, treImprovements.SelectedNode);
            else
                CharacterObject.MoveImprovementRoot(intNewIndex, nodDestination, treImprovements.SelectedNode);

            // Put the armor in the right order in the tree
            MoveTreeNode(treImprovements.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treImprovements.CacheSortOrder();

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
                objNode.BackColor = ColorManager.ControlDarker;

            // Clear the background color for all other Nodes.
            treImprovements.ClearNodeBackground(objNode);
        }

        private async void cmdAddImprovementGroup_Click(object sender, EventArgs e)
        {
            // Add a new location to the Improvements Tree.
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                    return;

                string strLocation = frmPickText.SelectedValue;

                CharacterObject.ImprovementGroups.Add(strLocation);
            }

            IsDirty = true;
        }

        #endregion Additional Improvements Tab Control Events

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

        #endregion Tree KeyDown Events

        #region Additional Drug Tab Control Events

        private void treCustomDrugs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedDrug();
        }

        #endregion Additional Drug Tab Control Events

        #region Splitter Resize Events

        private void splitKarmaNuyen_Panel1_Resize(object sender, EventArgs e)
        {
            if (lstKarma.Columns.Count >= 2 && lstKarma.Width > 409)
            {
                lstKarma.Columns[2].Width = lstKarma.Width - 195;
            }
        }

        private void splitKarmaNuyen_Panel2_Resize(object sender, EventArgs e)
        {
            if (lstNuyen.Columns.Count >= 2 && lstNuyen.Width > 409)
            {
                lstNuyen.Columns[2].Width = lstNuyen.Width - 195;
            }
        }

        #endregion Splitter Resize Events

        #region Other Control Events

        private void cmdEdgeSpent_Click(object sender, EventArgs e)
        {
            if (CharacterObject.EdgeUsed >= CharacterObject.EDG.TotalValue)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_CannotSpendEdge"),
                    LanguageManager.GetString("MessageTitle_CannotSpendEdge"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ++CharacterObject.EdgeUsed;

            IsDirty = true;
        }

        private void cmdEdgeGained_Click(object sender, EventArgs e)
        {
            if (CharacterObject.EdgeUsed <= 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_CannotRegainEdge"),
                    LanguageManager.GetString("MessageTitle_CannotRegainEdge"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            --CharacterObject.EdgeUsed;

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

        private enum CmdOperation { None, Up, Down }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            CmdOperation op = CmdOperation.None;

            // Determine which custom operation we're attempting, if any
            switch (keyData)
            {
                case Keys.Up | Keys.Alt:
                    op = CmdOperation.Up;
                    break;

                case Keys.Down | Keys.Alt:
                    op = CmdOperation.Down;
                    break;
            }

            bool up = op == CmdOperation.Up;
            if (up || op == CmdOperation.Down)
            {
                bool requireParentSortable = false;
                TreeView treActiveView = null;

                if (tabCharacterTabs.SelectedTab == tabStreetGear)
                {
                    // Lifestyle Tab.
                    if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                    {
                        treActiveView = treLifestyles;
                    }
                    // Armor Tab.
                    else if (tabStreetGearTabs.SelectedTab == tabArmor)
                    {
                        treActiveView = treArmor;
                    }
                    // Weapons Tab.
                    else if (tabStreetGearTabs.SelectedTab == tabWeapons)
                    {
                        treActiveView = treWeapons;
                    }
                    // Gear Tab.
                    else if (tabStreetGearTabs.SelectedTab == tabGear)
                    {
                        treActiveView = treGear;
                    }
                    // Drugs Tab.
                    else if (tabStreetGearTabs.SelectedTab == tabDrugs)
                    {
                        treActiveView = treCustomDrugs;
                    }
                }
                // Cyberware Tab.
                else if (tabCharacterTabs.SelectedTab == tabCyberware)
                {
                    // Top-level cyberware is sorted alphabetically, but we can re-arrange any plugins/gear inside them
                    requireParentSortable = true;
                    treActiveView = treCyberware;
                }
                // Vehicles Tab.
                else if (tabCharacterTabs.SelectedTab == tabVehicles)
                {
                    treActiveView = treVehicles;
                }
                // Critter Powers Tab.
                else if (tabCharacterTabs.SelectedTab == tabCritter)
                {
                    treActiveView = treCritterPowers;
                }
                // Improvements Tab.
                else if (tabCharacterTabs.SelectedTab == tabImprovements)
                {
                    treActiveView = treImprovements;
                }

                if (treActiveView != null)
                {
                    TreeNode objSelectedNode = treActiveView.SelectedNode;
                    TreeNode objParentNode = objSelectedNode?.Parent;

                    if (!requireParentSortable || objParentNode?.Tag is ICanSort)
                    {
                        TreeNodeCollection lstNodes = objParentNode?.Nodes ?? treActiveView.Nodes;
                        int intNewIndex = lstNodes.IndexOf(objSelectedNode);
                        intNewIndex = up ? Math.Max(0, intNewIndex - 1) : Math.Min(lstNodes.Count - 1, intNewIndex + 1);

                        MoveTreeNode(objSelectedNode, intNewIndex);
                    }
                }

                // Returning true tells the program to consume the input
                return true;
            }

            // If none of our key combinations are used then use the default logic
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion Other Control Events

        #region Condition Monitors

        private void chkPhysicalCM_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => CharacterObject.PhysicalCMFilled = i);
        }

        private void chkStunCM_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
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
        /// <param name="button_Click">Event handler for when a CM box is clicked</param>
        /// <param name="check">Whether or not to check the checkbox when finished processing. Expected to only be called on load.</param>
        /// <param name="value">Tag value of the checkbox to enable when using the check parameter. Expected to be the StunCMFilled or PhysicalCMFilled properties.</param>
        private void ProcessCharacterConditionMonitorBoxDisplays(Control pnlConditionMonitorPanel, int intConditionMax, int intThreshold, int intThresholdOffset, int intOverflow, EventHandler button_Click, bool check = false, int value = 0)
        {
            pnlConditionMonitorPanel.SuspendLayout();
            if (intConditionMax > 0)
            {
                pnlConditionMonitorPanel.Visible = true;
                List<DpiFriendlyCheckBoxDisguisedAsButton> lstCheckBoxes = pnlConditionMonitorPanel.Controls.OfType<DpiFriendlyCheckBoxDisguisedAsButton>().ToList();
                if (lstCheckBoxes.Count < intConditionMax + intOverflow)
                {
                    int intMax = 0;
                    DpiFriendlyCheckBoxDisguisedAsButton objMaxCheckBox = null;
                    foreach (DpiFriendlyCheckBoxDisguisedAsButton objLoopCheckBox in lstCheckBoxes)
                    {
                        int intLoop = Convert.ToInt32(objLoopCheckBox.Tag, GlobalSettings.InvariantCultureInfo);
                        if (objMaxCheckBox == null || intMax < intLoop)
                        {
                            intMax = intLoop;
                            objMaxCheckBox = objLoopCheckBox;
                        }
                    }

                    if (objMaxCheckBox != null)
                    {
                        for (int i = intMax + 1; i <= intConditionMax + intOverflow; i++)
                        {
                            DpiFriendlyCheckBoxDisguisedAsButton cb = new DpiFriendlyCheckBoxDisguisedAsButton(components)
                            {
                                Tag = i,
                                Appearance = objMaxCheckBox.Appearance,
                                AutoSize = objMaxCheckBox.AutoSize,
                                MinimumSize = objMaxCheckBox.MinimumSize,
                                Size = objMaxCheckBox.Size,
                                Padding = objMaxCheckBox.Padding,
                                Margin = objMaxCheckBox.Margin,
                                TextAlign = objMaxCheckBox.TextAlign,
                                Font = objMaxCheckBox.Font,
                                FlatStyle = objMaxCheckBox.FlatStyle,
                                UseVisualStyleBackColor = objMaxCheckBox.UseVisualStyleBackColor
                            };
                            cb.Click += button_Click;
                            pnlConditionMonitorPanel.Controls.Add(cb);
                            lstCheckBoxes.Add(cb);
                        }
                    }
                }

                int intMaxDimension = 0;
                int intMaxMargin = 0;
                foreach (DpiFriendlyCheckBoxDisguisedAsButton chkCmBox in lstCheckBoxes)
                {
                    int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag, GlobalSettings.InvariantCultureInfo);
                    chkCmBox.BackColor = SystemColors.Control; // Condition Monitor checkboxes shouldn't get colored based on Dark Mode
                    if (check && intCurrentBoxTag <= value)
                    {
                        chkCmBox.Checked = true;
                    }
                    if (intCurrentBoxTag <= intConditionMax)
                    {
                        chkCmBox.Visible = true;
                        chkCmBox.ImageDpi96 = null;
                        chkCmBox.ImageDpi192 = null;
                        if (intCurrentBoxTag > intThresholdOffset && (intCurrentBoxTag - intThresholdOffset) % intThreshold == 0)
                        {
                            int intModifiers = (intThresholdOffset - intCurrentBoxTag) / intThreshold;
                            chkCmBox.Text = intModifiers.ToString(GlobalSettings.CultureInfo);
                        }
                        else
                            chkCmBox.Text = " "; // Non-breaking space to help with DPI stuff
                    }
                    else if (intOverflow != 0 && intCurrentBoxTag <= intConditionMax + intOverflow)
                    {
                        chkCmBox.Visible = true;
                        chkCmBox.BackColor = SystemColors.ControlDark; // Condition Monitor checkboxes shouldn't get colored based on Dark Mode
                        if (intCurrentBoxTag == intConditionMax + intOverflow)
                        {
                            chkCmBox.ImageDpi96 = Properties.Resources.rip;
                            chkCmBox.ImageDpi192 = Properties.Resources.rip1;
                        }
                        else
                        {
                            chkCmBox.ImageDpi96 = null;
                            chkCmBox.ImageDpi192 = null;
                        }
                    }
                    else
                    {
                        chkCmBox.Visible = false;
                        chkCmBox.ImageDpi96 = null;
                        chkCmBox.ImageDpi192 = null;
                        chkCmBox.Text = " "; // Non-breaking space to help with DPI stuff
                    }

                    intMaxDimension = Math.Max(intMaxDimension, Math.Max(chkCmBox.Width, chkCmBox.Height));
                    intMaxMargin = Math.Max(intMaxMargin, Math.Max(Math.Max(chkCmBox.Margin.Left, chkCmBox.Margin.Right), Math.Max(chkCmBox.Margin.Top, chkCmBox.Margin.Bottom)));
                }

                Size objSquareSize = new Size(intMaxDimension, intMaxDimension);
                Padding objSquarePadding = new Padding(intMaxMargin);
                foreach (DpiFriendlyCheckBoxDisguisedAsButton chkCmBox in lstCheckBoxes)
                {
                    chkCmBox.MinimumSize = objSquareSize;
                    chkCmBox.Margin = objSquarePadding;
                }
                pnlConditionMonitorPanel.MaximumSize = new Size((2 * intThreshold + 1) * (intMaxDimension + intMaxMargin) / 2, pnlConditionMonitorPanel.MaximumSize.Height); // Width slightly longer to give enough wiggle room to take care of any funny business
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
            bool blnOldSkipRefresh = IsRefreshing;
            IsRefreshing = true;

            pnlConditionMonitorPanel.SuspendLayout();
            if (intConditionMax > 0)
            {
                pnlConditionMonitorPanel.Visible = true;
                foreach (DpiFriendlyCheckBoxDisguisedAsButton chkCmBox in pnlConditionMonitorPanel.Controls.OfType<DpiFriendlyCheckBoxDisguisedAsButton>())
                {
                    int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag, GlobalSettings.InvariantCultureInfo);

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

            IsRefreshing = blnOldSkipRefresh;
        }

        /// <summary>
        /// Changes which boxes are filled and unfilled in a condition monitor when a box in that condition monitor is clicked.
        /// </summary>
        /// <param name="chkSender">Checkbox we're currently changing.</param>
        /// <param name="blnDoUIUpdate">Whether to update all the other boxes in the UI or not. If something like ProcessEquipmentConditionMonitorBoxDisplays would be called later, this can be false.</param>
        /// <param name="funcPropertyToUpdate">Function to run once the condition monitor has been processed, probably a property setter. Uses the amount of filled boxes as its argument.</param>
        private void ProcessConditionMonitorCheckedChanged(DpiFriendlyCheckBoxDisguisedAsButton chkSender, Action<int> funcPropertyToUpdate = null, bool blnDoUIUpdate = true)
        {
            if (IsRefreshing)
                return;

            if (blnDoUIUpdate)
            {
                Control pnlConditionMonitorPanel = chkSender.Parent;

                if (pnlConditionMonitorPanel == null)
                    return;

                int intBoxTag = Convert.ToInt32(chkSender.Tag, GlobalSettings.InvariantCultureInfo);

                int intFillCount = chkSender.Checked ? 1 : 0;

                // If this is being checked, make sure everything before it is checked off.
                IsRefreshing = true;

                pnlConditionMonitorPanel.SuspendLayout();
                foreach (DpiFriendlyCheckBoxDisguisedAsButton chkCmBox in pnlConditionMonitorPanel.Controls.OfType<DpiFriendlyCheckBoxDisguisedAsButton>())
                {
                    if (chkCmBox != chkSender)
                    {
                        int intCurrentBoxTag = Convert.ToInt32(chkCmBox.Tag, GlobalSettings.InvariantCultureInfo);
                        if (intCurrentBoxTag < intBoxTag)
                        {
                            chkCmBox.Checked = true;
                            ++intFillCount;
                        }
                        else if (intCurrentBoxTag > intBoxTag)
                        {
                            chkCmBox.Checked = false;
                        }
                    }
                }

                pnlConditionMonitorPanel.ResumeLayout();

                funcPropertyToUpdate?.Invoke(intFillCount);

                IsRefreshing = false;
            }
            else
            {
                int intFillCount = Convert.ToInt32(chkSender.Tag, GlobalSettings.InvariantCultureInfo);
                if (!chkSender.Checked)
                    --intFillCount;
                funcPropertyToUpdate?.Invoke(intFillCount);
            }

            IsDirty = true;
        }

        private void chkCyberwareCM_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objItem && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        private void chkGearCM_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            // Locate the selected Gear.
            TreeNode objGearNode = treGear.SelectedNode;
            while (objGearNode?.Level > 1)
                objGearNode = objGearNode.Parent;

            if (objGearNode?.Tag is Gear objGear && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objGear.MatrixCMFilled = i, false);
        }

        private void chkArmorMatrixCM_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treArmor.SelectedNode?.Tag is IHasMatrixAttributes objItem && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        private void chkWeaponCM_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objItem && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        private void chkVehicleCM_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (panVehicleCM.SelectedIndex == 0)
            {
                // Locate the selected Vehicle.
                TreeNode objVehicleNode = treVehicles.SelectedNode;
                while (objVehicleNode?.Level > 1)
                    objVehicleNode = objVehicleNode.Parent;

                if (treVehicles.SelectedNode?.Tag is Vehicle objVehicle && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                    ProcessConditionMonitorCheckedChanged(objBox, i => objVehicle.PhysicalCMFilled = i);
            }
            else if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objItem && sender is DpiFriendlyCheckBoxDisguisedAsButton objBox)
                ProcessConditionMonitorCheckedChanged(objBox, i => objItem.MatrixCMFilled = i, false);
        }

        #endregion Condition Monitors

        #region Custom Methods

        /// <summary>
        /// Refresh the currently-selected Drug.
        /// </summary>
        private void RefreshSelectedDrug()
        {
            IsRefreshing = true;
            flpDrugs.SuspendLayout();

            if (treCustomDrugs.SelectedNode?.Level != 0 && treCustomDrugs.SelectedNode?.Tag is Drug objDrug)
            {
                flpDrugs.Visible = true;
                btnDeleteCustomDrug.Enabled = true;

                lblDrugName.Text = objDrug.Name;
                lblDrugAvail.Text = objDrug.DisplayTotalAvail;
                lblDrugGrade.Text = objDrug.Grade.CurrentDisplayName;
                lblDrugCost.Text = objDrug.Cost.ToString(CharacterObject.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                lblDrugQty.Text = objDrug.Quantity.ToString(GlobalSettings.CultureInfo);
                lblDrugCategory.Text = objDrug.Category;
                lblDrugAddictionRating.Text = objDrug.AddictionRating.ToString(GlobalSettings.CultureInfo);
                lblDrugAddictionThreshold.Text = objDrug.AddictionThreshold.ToString(GlobalSettings.CultureInfo);
                lblDrugEffect.Text = objDrug.EffectDescription;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdComponents))
                {
                    foreach (DrugComponent objComponent in objDrug.Components)
                    {
                        sbdComponents.AppendLine(objComponent.CurrentDisplayName);
                    }

                    lblDrugComponents.Text = sbdComponents.ToString();
                }
                btnIncreaseDrugQty.Enabled = objDrug.Cost <= CharacterObject.Nuyen;
                btnDecreaseDrugQty.Enabled = objDrug.Quantity != 0;
            }
            else
            {
                flpDrugs.Visible = false;
                btnDeleteCustomDrug.Enabled = treCustomDrugs.SelectedNode?.Tag is ICanRemove;
            }

            IsRefreshing = false;
            flpDrugs.ResumeLayout();
        }

        private void LiveUpdateFromCharacterFile(object sender, EventArgs e)
        {
            if (IsDirty || !GlobalSettings.LiveUpdateCleanCharacterFiles || IsLoading || _blnSkipUpdate || IsCharacterUpdateRequested)
                return;

            string strCharacterFile = CharacterObject.FileName;
            if (string.IsNullOrEmpty(strCharacterFile) || !File.Exists(strCharacterFile))
                return;

            if (File.GetLastWriteTimeUtc(strCharacterFile) <= CharacterObject.FileLastWriteTime)
                return;

            _blnSkipUpdate = true;

            // Character is not dirty and their save file was updated outside of Chummer5 while it is open, so reload them
            using (new CursorWait(this))
            {
                using (LoadingBar frmLoadingForm = ChummerMainForm.CreateAndShowProgressBar(Path.GetFileName(CharacterObject.FileName), Character.NumLoadingSections))
                {
                    CharacterObject.Load(frmLoadingForm);
                    frmLoadingForm.PerformStep(LanguageManager.GetString("String_UI"));

                    IsCharacterUpdateRequested = true;
                    _blnSkipUpdate = false;
                    // Immediately call character update because we know it's necessary
                    UpdateCharacterInfo();

                    IsDirty = false;
                }
            }

            if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0 && !Utils.IsUnitTest && Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_ImprovementLoadError"),
                LanguageManager.GetString("MessageTitle_ImprovementLoadError"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
            }
        }

        /// <summary>
        /// Update the Character information.
        /// </summary>
        private void UpdateCharacterInfo(object sender = null, EventArgs e = null)
        {
            if (IsLoading || _blnSkipUpdate || !IsCharacterUpdateRequested)
                return;

            _blnSkipUpdate = true;

            using (new CursorWait(this))
            {
                // TODO: DataBind these wherever possible

                if (CharacterObject.Metatype == "Free Spirit" && !CharacterObject.IsCritter ||
                    CharacterObject.MetatypeCategory.EndsWith("Spirits", StringComparison.Ordinal))
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
                else
                {
                    lblCritterPowerPointsLabel.Visible = false;
                    lblCritterPowerPoints.Visible = false;
                }

                UpdateInitiationCost(this, EventArgs.Empty);

                RefreshSelectedQuality();
                RefreshSelectedCyberware();
                RefreshSelectedArmor();
                RefreshSelectedGear();
                RefreshSelectedDrug();
                RefreshSelectedLifestyle();
                RefreshSelectedVehicle();
                RefreshSelectedWeapon();
                RefreshSelectedSpell();
                RefreshSelectedComplexForm();

                if (AutosaveStopWatch.Elapsed.Minutes >= 5 && IsDirty)
                {
                    AutoSaveCharacter();
                }
            }

            _blnSkipUpdate = false;
            IsCharacterUpdateRequested = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed piece of Cyberware.
        /// </summary>
        public void RefreshSelectedCyberware()
        {
            IsRefreshing = true;
            flpCyberware.SuspendLayout();

            if (treCyberware.SelectedNode == null || treCyberware.SelectedNode.Level == 0)
            {
                gpbCyberwareCommon.Visible = false;
                gpbCyberwareMatrix.Visible = false;
                tabCyberwareCM.Visible = false;

                // Buttons
                cmdDeleteCyberware.Enabled = treCyberware.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpCyberware.ResumeLayout();
                return;
            }

            if (treCyberware.SelectedNode?.Tag is IHasWirelessBonus objHasWirelessBonus)
            {
                chkCyberwareWireless.Visible = true;
                chkCyberwareWireless.Checked = objHasWirelessBonus.WirelessOn;
            }
            else
                chkCyberwareWireless.Visible = false;

            if (treCyberware.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblCyberwareSourceLabel.Visible = true;
                lblCyberwareSource.Visible = true;
                objSelected.SetSourceDetail(lblCyberwareSource);
            }
            else
            {
                lblCyberwareSourceLabel.Visible = false;
                lblCyberwareSource.Visible = false;
            }

            if (treCyberware.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblCyberwareRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            string strESSFormat = CharacterObjectSettings.EssenceFormat;
            switch (treCyberware.SelectedNode?.Tag)
            {
                case Cyberware objCyberware:
                    {
                        gpbCyberwareCommon.Visible = true;
                        gpbCyberwareMatrix.Visible = tabCyberwareCM.Visible = objCyberware.SourceType == Improvement.ImprovementSource.Cyberware;

                        // Buttons
                        cmdDeleteCyberware.Enabled = string.IsNullOrEmpty(objCyberware.ParentID);

                        // gpbCyberwareCommon
                        lblCyberwareName.Text = objCyberware.DisplayNameShort(GlobalSettings.Language);
                        lblCyberwareCategory.Text = objCyberware.DisplayCategory(GlobalSettings.Language);
                        lblCyberwareGradeLabel.Visible = true;
                        lblCyberwareGrade.Visible = true;
                        lblCyberwareGrade.Text = objCyberware.Grade.CurrentDisplayName;
                        lblCyberwareEssenceLabel.Visible = true;
                        lblCyberwareEssence.Visible = true;
                        if (objCyberware.Parent == null)
                            lblCyberwareEssence.Text = objCyberware.CalculatedESS.ToString(strESSFormat, GlobalSettings.CultureInfo);
                        else if (objCyberware.AddToParentESS)
                            lblCyberwareEssence.Text = '+' + objCyberware.CalculatedESS.ToString(strESSFormat, GlobalSettings.CultureInfo);
                        else
                            lblCyberwareEssence.Text = 0.0m.ToString(strESSFormat, GlobalSettings.CultureInfo);
                        lblCyberwareAvail.Text = objCyberware.DisplayTotalAvail;
                        cmdCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                        lblCyberwareRating.Text = objCyberware.Rating.ToString(GlobalSettings.CultureInfo);
                        lblCyberwareCapacity.Text = objCyberware.DisplayCapacity;
                        lblCyberwareCost.Text = objCyberware.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        if (objCyberware.Category.Equals("Cyberlimb", StringComparison.Ordinal))
                        {
                            lblCyberlimbAGILabel.Visible = true;
                            lblCyberlimbAGI.Visible = true;
                            lblCyberlimbAGI.Text = objCyberware.GetAttributeTotalValue("AGI").ToString(GlobalSettings.CultureInfo);
                            lblCyberlimbSTRLabel.Visible = true;
                            lblCyberlimbSTR.Visible = true;
                            lblCyberlimbSTR.Text = objCyberware.GetAttributeTotalValue("STR").ToString(GlobalSettings.CultureInfo);
                        }
                        else
                        {
                            lblCyberlimbAGILabel.Visible = false;
                            lblCyberlimbAGI.Visible = false;
                            lblCyberlimbSTRLabel.Visible = false;
                            lblCyberlimbSTR.Visible = false;
                        }

                        // gpbCyberwareMatrix
                        if (gpbCyberwareMatrix.Visible)
                        {
                            int intDeviceRating = objCyberware.GetTotalMatrixAttribute("Device Rating");
                            lblCyberDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                            objCyberware.RefreshMatrixAttributeComboBoxes(cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall);

                            chkCyberwareActiveCommlink.Visible = objCyberware.IsCommlink;
                            chkCyberwareActiveCommlink.Checked = objCyberware.IsActiveCommlink(CharacterObject);
                            if (CharacterObject.IsAI)
                            {
                                chkCyberwareHomeNode.Visible = true;
                                chkCyberwareHomeNode.Checked = objCyberware.IsHomeNode(CharacterObject);
                                chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible &&
                                                               objCyberware.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                            }
                            else
                                chkCyberwareHomeNode.Visible = false;

                            lblCyberwareOverclockerLabel.Visible = false;
                            cboCyberwareOverclocker.Visible = false;
                        }

                        break;
                    }
                case Gear objGear:
                    {
                        gpbCyberwareCommon.Visible = true;
                        gpbCyberwareMatrix.Visible = true;
                        tabCyberwareCM.Visible = true;

                        // Buttons
                        cmdDeleteCyberware.Enabled = !objGear.IncludedInParent;

                        // gpbCyberwareCommon
                        lblCyberwareName.Text = objGear.CurrentDisplayNameShort;
                        lblCyberwareCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                        lblCyberwareGradeLabel.Visible = false;
                        lblCyberwareGrade.Visible = false;
                        lblCyberwareEssenceLabel.Visible = false;
                        lblCyberwareEssence.Visible = false;
                        lblCyberwareAvail.Text = objGear.DisplayTotalAvail;

                        lblCyberwareRating.Text = objGear.Rating.ToString(GlobalSettings.CultureInfo);
                        lblCyberwareCapacity.Text = objGear.DisplayCapacity;
                        lblCyberwareCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblCyberlimbAGILabel.Visible = false;
                        lblCyberlimbAGI.Visible = false;
                        lblCyberlimbSTRLabel.Visible = false;
                        lblCyberlimbSTR.Visible = false;

                        // gpbCyberwareMatrix
                        int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                        lblCyberDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objGear.RefreshMatrixAttributeComboBoxes(cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall);

                        chkCyberwareActiveCommlink.Visible = objGear.IsCommlink;
                        chkCyberwareActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkCyberwareHomeNode.Visible = true;
                            chkCyberwareHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                            chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkCyberwareHomeNode.Visible = false;

                        if (CharacterObject.Overclocker && objGear.Category == "Cyberdecks")
                        {
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstOverclocker))
                            {
                                lstOverclocker.Add(new ListItem("None", LanguageManager.GetString("String_None")));
                                lstOverclocker.Add(new ListItem("Attack", LanguageManager.GetString("String_Attack")));
                                lstOverclocker.Add(new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze")));
                                lstOverclocker.Add(new ListItem("Data Processing",
                                                                LanguageManager.GetString("String_DataProcessing")));
                                lstOverclocker.Add(
                                    new ListItem("Firewall", LanguageManager.GetString("String_Firewall")));

                                cboCyberwareOverclocker.BeginUpdate();
                                cboCyberwareOverclocker.PopulateWithListItems(lstOverclocker);
                                cboCyberwareOverclocker.SelectedValue = objGear.Overclocked;
                                if (cboCyberwareOverclocker.SelectedIndex == -1)
                                    cboCyberwareOverclocker.SelectedIndex = 0;
                                cboCyberwareOverclocker.EndUpdate();
                            }

                            cboCyberwareOverclocker.Visible = true;
                            lblCyberwareOverclockerLabel.Visible = true;
                        }
                        else
                        {
                            cboCyberwareOverclocker.Visible = false;
                            lblCyberwareOverclockerLabel.Visible = false;
                        }

                        break;
                    }
            }

            if (tabCyberwareCM.Visible)
            {
                if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objMatrixCM)
                {
                    ProcessEquipmentConditionMonitorBoxDisplays(panCyberwareMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
                }
                else
                {
                    tabCyberwareCM.Visible = false;
                }
            }

            IsRefreshing = false;
            flpCyberware.ResumeLayout();
        }

        /// <summary>
        /// Refresh the information for the currently displayed Weapon.
        /// </summary>
        public void RefreshSelectedWeapon()
        {
            IsRefreshing = true;
            flpWeapons.SuspendLayout();

            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level <= 0)
            {
                gpbWeaponsCommon.Visible = false;
                gpbWeaponsWeapon.Visible = false;
                gpbWeaponsMatrix.Visible = false;

                // Buttons
                cmdDeleteWeapon.Enabled = treWeapons.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpWeapons.ResumeLayout();

                return;
            }
            string strSpace = LanguageManager.GetString("String_Space");
            if (treWeapons.SelectedNode?.Tag is IHasWirelessBonus objHasWirelessBonus)
            {
                chkWeaponWireless.Visible = true;
                chkWeaponWireless.Checked = objHasWirelessBonus.WirelessOn;
            }
            else
                chkWeaponWireless.Visible = false;

            if (treWeapons.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblWeaponSourceLabel.Visible = true;
                lblWeaponSource.Visible = true;
                objSelected.SetSourceDetail(lblWeaponSource);
            }
            else
            {
                lblWeaponSourceLabel.Visible = false;
                lblWeaponSource.Visible = false;
            }

            if (treWeapons.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblWeaponRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            switch (treWeapons.SelectedNode?.Tag)
            {
                case Weapon objWeapon:
                    {
                        gpbWeaponsCommon.Visible = true;
                        gpbWeaponsWeapon.Visible = true;
                        gpbWeaponsMatrix.Visible = true;

                        // Buttons
                        cmdDeleteWeapon.Enabled = !objWeapon.IncludedInWeapon &&
                                                  !objWeapon.Cyberware &&
                                                  objWeapon.Category != "Gear" &&
                                                  !objWeapon.Category.StartsWith("Quality", StringComparison.Ordinal) &&
                                                  string.IsNullOrEmpty(objWeapon.ParentID);

                        // gpbWeaponsCommon
                        lblWeaponName.Text = objWeapon.CurrentDisplayName;
                        lblWeaponCategory.Text = objWeapon.DisplayCategory(GlobalSettings.Language);
                        lblWeaponRatingLabel.Visible = false;
                        lblWeaponRating.Visible = false;
                        lblWeaponCapacityLabel.Visible = false;
                        lblWeaponCapacity.Visible = false;
                        lblWeaponAvail.Text = objWeapon.DisplayTotalAvail;
                        lblWeaponCost.Text = objWeapon.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblWeaponSlotsLabel.Visible = true;
                        lblWeaponSlots.Visible = true;
                        if (!string.IsNullOrWhiteSpace(objWeapon.AccessoryMounts))
                        {
                            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                            {
                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdSlotsText))
                                {
                                    foreach (string strMount in objWeapon.AccessoryMounts.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        sbdSlotsText.Append(LanguageManager.GetString("String_Mount" + strMount))
                                                    .Append('/');
                                    --sbdSlotsText.Length;
                                    lblWeaponSlots.Text = sbdSlotsText.ToString();
                                }
                            }
                            else
                                lblWeaponSlots.Text = objWeapon.AccessoryMounts;
                        }
                        else
                            lblWeaponSlots.Text = LanguageManager.GetString("String_None");
                        lblWeaponConcealLabel.Visible = true;
                        lblWeaponConceal.Visible = true;
                        lblWeaponConceal.Text = objWeapon.DisplayConcealability;
                        cmdWeaponMoveToVehicle.Visible = cmdDeleteWeapon.Enabled && CharacterObject.Vehicles.Count > 0;
                        chkWeaponEquipped.Text
                            = LanguageManager.GetString(objWeapon.Parent == null
                                                            ? "Checkbox_Equipped"
                                                            : "Checkbox_Installed");
                        chkWeaponEquipped.Enabled = !objWeapon.IncludedInWeapon;
                        chkWeaponEquipped.Checked = objWeapon.Equipped;
                        chkIncludedInWeapon.Visible = objWeapon.Parent != null;
                        chkIncludedInWeapon.Enabled = false;
                        chkIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                        // gpbWeaponsWeapon
                        gpbWeaponsWeapon.Text = LanguageManager.GetString("String_Weapon");
                        lblWeaponDamageLabel.Visible = true;
                        lblWeaponDamage.Visible = true;
                        lblWeaponDamage.Text = objWeapon.DisplayDamage;
                        lblWeaponAPLabel.Visible = true;
                        lblWeaponAP.Visible = true;
                        lblWeaponAP.Text = objWeapon.DisplayTotalAP;
                        lblWeaponAccuracyLabel.Visible = true;
                        lblWeaponAccuracy.Visible = true;
                        lblWeaponAccuracy.Text = objWeapon.DisplayAccuracy;
                        lblWeaponDicePoolLabel.Visible = true;
                        dpcWeaponDicePool.Visible = true;
                        dpcWeaponDicePool.DicePool = objWeapon.DicePool;
                        dpcWeaponDicePool.CanBeRolled = true;
                        dpcWeaponDicePool.SetLabelToolTip(objWeapon.DicePoolTooltip);
                        if (objWeapon.RangeType == "Ranged")
                        {
                            lblWeaponReachLabel.Visible = false;
                            lblWeaponReach.Visible = false;
                            lblWeaponRCLabel.Visible = true;
                            lblWeaponRC.Visible = true;
                            lblWeaponRC.Text = objWeapon.DisplayTotalRC;
                            lblWeaponRC.SetToolTip(objWeapon.RCToolTip);
                            lblWeaponAmmoLabel.Visible = true;
                            lblWeaponAmmo.Visible = true;
                            lblWeaponAmmo.Text = objWeapon.DisplayAmmo;
                            lblWeaponModeLabel.Visible = true;
                            lblWeaponMode.Visible = true;
                            lblWeaponMode.Text = objWeapon.DisplayMode;

                            tlpWeaponsRanges.Visible = true;
                            lblWeaponRangeMain.Text = objWeapon.CurrentDisplayRange;
                            lblWeaponRangeAlternate.Text = objWeapon.CurrentDisplayAlternateRange;
                            Dictionary<string, string> dictionaryRanges = objWeapon.GetRangeStrings(GlobalSettings.CultureInfo);
                            lblWeaponRangeShortLabel.Text = objWeapon.RangeModifier("Short");
                            lblWeaponRangeMediumLabel.Text = objWeapon.RangeModifier("Medium");
                            lblWeaponRangeLongLabel.Text = objWeapon.RangeModifier("Long");
                            lblWeaponRangeExtremeLabel.Text = objWeapon.RangeModifier("Extreme");
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
                            lblWeaponReachLabel.Visible = true;
                            lblWeaponReach.Visible = true;
                            lblWeaponReach.Text = objWeapon.TotalReach.ToString(GlobalSettings.CultureInfo);
                            lblWeaponRCLabel.Visible = false;
                            lblWeaponRC.Visible = false;
                            if (objWeapon.Ammo != "0")
                            {
                                lblWeaponAmmoLabel.Visible = true;
                                lblWeaponAmmo.Visible = true;
                                lblWeaponAmmo.Text = objWeapon.DisplayAmmo;
                            }
                            else
                            {
                                lblWeaponAmmoLabel.Visible = false;
                                lblWeaponAmmo.Visible = false;
                            }
                            lblWeaponModeLabel.Visible = false;
                            lblWeaponMode.Visible = false;

                            tlpWeaponsRanges.Visible = false;
                        }
                        // Enable the fire button if the Weapon is Ranged.
                        if (objWeapon.RangeType == "Ranged" || objWeapon.RangeType == "Melee" && objWeapon.Ammo != "0")
                        {
                            tlpWeaponsCareer.Visible = true;
                            lblWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);
                            cmdFireWeapon.Enabled = objWeapon.AmmoRemaining != 0;

                            cmsAmmoSingleShot.Enabled = objWeapon.AllowSingleShot;
                            cmsAmmoShortBurst.Enabled = objWeapon.AllowShortBurst;
                            cmsAmmoLongBurst.Enabled = objWeapon.AllowLongBurst;
                            cmsAmmoFullBurst.Enabled = objWeapon.AllowFullBurst;
                            cmsAmmoSuppressiveFire.Enabled = objWeapon.AllowSuppressive;

                            // Melee Weapons with Ammo are considered to be Single Shot.
                            if (objWeapon.RangeType == "Melee" && objWeapon.Ammo != "0")
                                cmsAmmoSingleShot.Enabled = true;

                            if (cmsAmmoSingleShot.Enabled)
                                cmsAmmoSingleShot.Text = string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("String_SingleShot")
                                    , objWeapon.SingleShot.ToString(GlobalSettings.CultureInfo),
                                    objWeapon.SingleShot == 1
                                        ? LanguageManager.GetString("String_Bullet")
                                        : LanguageManager.GetString("String_Bullets"));
                            else
                                cmsAmmoSingleShot.Text = LanguageManager.GetString("String_SingleShotNA");

                            if (cmsAmmoShortBurst.Enabled)
                                cmsAmmoShortBurst.Text = string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("String_ShortBurst")
                                    , objWeapon.ShortBurst.ToString(GlobalSettings.CultureInfo),
                                    objWeapon.ShortBurst == 1
                                        ? LanguageManager.GetString("String_Bullet")
                                        : LanguageManager.GetString("String_Bullets"));
                            else
                                cmsAmmoShortBurst.Text = LanguageManager.GetString("String_ShortBurstNA");

                            if (cmsAmmoLongBurst.Enabled)
                                cmsAmmoLongBurst.Text = string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("String_LongBurst")
                                    , objWeapon.LongBurst.ToString(GlobalSettings.CultureInfo),
                                    objWeapon.LongBurst == 1
                                        ? LanguageManager.GetString("String_Bullet")
                                        : LanguageManager.GetString("String_Bullets"));
                            else
                                cmsAmmoLongBurst.Text = LanguageManager.GetString("String_LongBurstNA");

                            if (cmsAmmoFullBurst.Enabled)
                                cmsAmmoFullBurst.Text = string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("String_FullBurst")
                                    , objWeapon.FullBurst.ToString(GlobalSettings.CultureInfo),
                                    objWeapon.FullBurst == 1
                                        ? LanguageManager.GetString("String_Bullet")
                                        : LanguageManager.GetString("String_Bullets"));
                            else
                                cmsAmmoFullBurst.Text = LanguageManager.GetString("String_FullBurstNA");

                            if (cmsAmmoSuppressiveFire.Enabled)
                                cmsAmmoSuppressiveFire.Text = string.Format(GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("String_SuppressiveFire")
                                    , objWeapon.Suppressive,
                                    LanguageManager.GetString(objWeapon.Suppressive == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));
                            else
                                cmsAmmoSuppressiveFire.Text = LanguageManager.GetString("String_SuppressiveFireNA");

                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstAmmo))
                            {
                                int intCurrentSlot = objWeapon.ActiveAmmoSlot;
                                for (int i = 1; i <= objWeapon.AmmoSlots; i++)
                                {
                                    objWeapon.ActiveAmmoSlot = i;
                                    string strAmmoName;
                                    if (objWeapon.RequireAmmo)
                                    {
                                        Gear objGear = CharacterObject.Gear.DeepFindById(objWeapon.AmmoLoaded);
                                        strAmmoName = objGear?.DisplayNameShort(GlobalSettings.Language) ??
                                                      LanguageManager.GetString(objWeapon.AmmoRemaining > 0
                                                          ? "String_ExternalSource"
                                                          : "String_Empty");
                                        if (objWeapon.AmmoSlots > 1)
                                            strAmmoName += strSpace + '(' + string.Format(GlobalSettings.CultureInfo
                                                , LanguageManager.GetString("String_SlotNumber")
                                                , i.ToString(GlobalSettings.CultureInfo)) + ')';

                                        string strPlugins = string.Empty;
                                        if (objGear?.Children.Count > 0)
                                        {
                                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                       out StringBuilder sbdPlugins))
                                            {
                                                foreach (Gear objChild in objGear.Children)
                                                {
                                                    sbdPlugins.Append(objChild.CurrentDisplayNameShort).Append(',')
                                                              .Append(strSpace);
                                                }

                                                strPlugins = sbdPlugins.ToString();
                                            }
                                        }

                                        // Remove the trailing comma.
                                        if (!string.IsNullOrEmpty(strPlugins))
                                            strPlugins = strPlugins.Substring(
                                                0, strPlugins.Length - 1 - strSpace.Length);

                                        if (!string.IsNullOrEmpty(strPlugins))
                                            strAmmoName += strSpace + '[' + strPlugins + ']';
                                    }
                                    else
                                        strAmmoName = LanguageManager.GetString(objWeapon.AmmoRemaining > 0
                                            ? "String_MountInternal"
                                            : "String_Empty");

                                    lstAmmo.Add(new ListItem(i.ToString(GlobalSettings.InvariantCultureInfo),
                                                             strAmmoName));
                                }

                                objWeapon.ActiveAmmoSlot = intCurrentSlot;
                                cboWeaponAmmo.BeginUpdate();
                                cboWeaponAmmo.PopulateWithListItems(lstAmmo);
                                cboWeaponAmmo.SelectedValue
                                    = objWeapon.ActiveAmmoSlot.ToString(GlobalSettings.InvariantCultureInfo);
                                if (cboWeaponAmmo.SelectedIndex == -1)
                                    cboWeaponAmmo.SelectedIndex = 0;
                                cboWeaponAmmo.Enabled = lstAmmo.Count > 1;
                                cboWeaponAmmo.EndUpdate();
                            }
                        }
                        else
                        {
                            tlpWeaponsCareer.Visible = false;
                        }

                        // gpbWeaponsMatrix
                        int intDeviceRating = objWeapon.GetTotalMatrixAttribute("Device Rating");
                        lblWeaponDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objWeapon.RefreshMatrixAttributeComboBoxes(cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall);

                        chkWeaponActiveCommlink.Visible = objWeapon.IsCommlink;
                        chkWeaponActiveCommlink.Checked = objWeapon.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkWeaponHomeNode.Visible = true;
                            chkWeaponHomeNode.Checked = objWeapon.IsHomeNode(CharacterObject);
                            chkWeaponHomeNode.Enabled = chkWeaponActiveCommlink.Visible && objWeapon.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkWeaponHomeNode.Visible = false;

                        cboWeaponOverclocker.Visible = false;
                        lblWeaponOverclockerLabel.Visible = false;
                        break;
                    }
                case WeaponAccessory objSelectedAccessory:
                    {
                        gpbWeaponsCommon.Visible = true;
                        gpbWeaponsWeapon.Visible = true;
                        gpbWeaponsMatrix.Visible = false;

                        // Buttons
                        cmdDeleteWeapon.Enabled = !objSelectedAccessory.IncludedInWeapon && string.IsNullOrEmpty(objSelectedAccessory.ParentID);

                        // gpbWeaponsCommon
                        lblWeaponName.Text = objSelectedAccessory.CurrentDisplayNameShort;
                        lblWeaponCategory.Text = LanguageManager.GetString("String_WeaponAccessory");
                        if (objSelectedAccessory.MaxRating > 0)
                        {
                            lblWeaponRatingLabel.Visible = true;
                            lblWeaponRating.Visible = true;
                            lblWeaponRating.Text = objSelectedAccessory.Rating.ToString(GlobalSettings.CultureInfo);
                        }
                        else
                        {
                            lblWeaponRatingLabel.Visible = false;
                            lblWeaponRating.Visible = false;
                        }
                        lblWeaponCapacityLabel.Visible = false;
                        lblWeaponCapacity.Visible = false;
                        lblWeaponAvail.Text = objSelectedAccessory.DisplayTotalAvail;
                        lblWeaponCost.Text = objSelectedAccessory.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblWeaponSlotsLabel.Visible = true;
                        lblWeaponSlots.Visible = true;
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdSlotsText))
                        {
                            sbdSlotsText.Append(objSelectedAccessory.Mount);
                            if (sbdSlotsText.Length > 0
                                && !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                   StringComparison.OrdinalIgnoreCase))
                            {
                                sbdSlotsText.Clear();
                                foreach (string strMount in objSelectedAccessory.Mount.SplitNoAlloc(
                                             '/', StringSplitOptions.RemoveEmptyEntries))
                                    sbdSlotsText.Append(LanguageManager.GetString("String_Mount" + strMount))
                                                .Append('/');
                                --sbdSlotsText.Length;
                            }

                            if (!string.IsNullOrEmpty(objSelectedAccessory.ExtraMount)
                                && objSelectedAccessory.ExtraMount != "None")
                            {
                                bool boolHaveAddedItem = false;
                                foreach (string strCurrentExtraMount in objSelectedAccessory.ExtraMount.SplitNoAlloc(
                                             '/', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!boolHaveAddedItem)
                                    {
                                        sbdSlotsText.Append(strSpace).Append('+').Append(strSpace);
                                        boolHaveAddedItem = true;
                                    }

                                    sbdSlotsText
                                        .Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount))
                                        .Append('/');
                                }

                                // Remove the trailing /
                                if (boolHaveAddedItem)
                                    --sbdSlotsText.Length;
                            }

                            lblWeaponSlots.Text = sbdSlotsText.ToString();
                        }

                        lblWeaponConcealLabel.Visible = objSelectedAccessory.TotalConcealability != 0;
                        lblWeaponConceal.Visible = objSelectedAccessory.TotalConcealability != 0;
                        lblWeaponConceal.Text = objSelectedAccessory.TotalConcealability.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        chkWeaponEquipped.Text
                            = LanguageManager.GetString(objSelectedAccessory.Parent == null
                                                            ? "Checkbox_Equipped"
                                                            : "Checkbox_Installed");
                        chkWeaponEquipped.Enabled = !objSelectedAccessory.IncludedInWeapon;
                        chkWeaponEquipped.Checked = objSelectedAccessory.Equipped;
                        chkIncludedInWeapon.Visible = objSelectedAccessory.Parent != null;
                        chkIncludedInWeapon.Enabled = CharacterObjectSettings.AllowEditPartOfBaseWeapon;
                        chkIncludedInWeapon.Checked = objSelectedAccessory.IncludedInWeapon;

                        // gpbWeaponsWeapon
                        gpbWeaponsWeapon.Text = LanguageManager.GetString("String_WeaponAccessory");
                        if (string.IsNullOrEmpty(objSelectedAccessory.Damage))
                        {
                            lblWeaponDamageLabel.Visible = false;
                            lblWeaponDamage.Visible = false;
                        }
                        else
                        {
                            lblWeaponDamageLabel.Visible = !string.IsNullOrEmpty(objSelectedAccessory.Damage);
                            lblWeaponDamage.Visible = !string.IsNullOrEmpty(objSelectedAccessory.Damage);
                            lblWeaponDamage.Text = Convert.ToInt32(objSelectedAccessory.Damage, GlobalSettings.InvariantCultureInfo).ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (string.IsNullOrEmpty(objSelectedAccessory.AP))
                        {
                            lblWeaponAPLabel.Visible = false;
                            lblWeaponAP.Visible = false;
                        }
                        else
                        {
                            lblWeaponAPLabel.Visible = true;
                            lblWeaponAP.Visible = true;
                            lblWeaponAP.Text = Convert.ToInt32(objSelectedAccessory.AP, GlobalSettings.InvariantCultureInfo).ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (objSelectedAccessory.Accuracy == 0)
                        {
                            lblWeaponAccuracyLabel.Visible = false;
                            lblWeaponAccuracy.Visible = false;
                        }
                        else
                        {
                            lblWeaponAccuracyLabel.Visible = true;
                            lblWeaponAccuracy.Visible = true;
                            lblWeaponAccuracy.Text = objSelectedAccessory.Accuracy.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (objSelectedAccessory.DicePool == 0)
                        {
                            lblWeaponDicePoolLabel.Visible = false;
                            dpcWeaponDicePool.Visible = false;
                        }
                        else
                        {
                            lblWeaponDicePoolLabel.Visible = true;
                            dpcWeaponDicePool.Visible = true;
                            dpcWeaponDicePool.DicePool = objSelectedAccessory.DicePool;
                            dpcWeaponDicePool.CanBeRolled = false;
                            dpcWeaponDicePool.SetLabelToolTip(string.Empty);
                        }
                        lblWeaponReachLabel.Visible = false;
                        lblWeaponReach.Visible = false;
                        if (string.IsNullOrEmpty(objSelectedAccessory.RC))
                        {
                            lblWeaponRCLabel.Visible = false;
                            lblWeaponRC.Visible = false;
                        }
                        else
                        {
                            lblWeaponRCLabel.Visible = true;
                            lblWeaponRC.Visible = true;
                            lblWeaponRC.Text = Convert.ToInt32(objSelectedAccessory.RC, GlobalSettings.InvariantCultureInfo).ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (objSelectedAccessory.TotalAmmoBonus != 0
                            || (!string.IsNullOrEmpty(objSelectedAccessory.ModifyAmmoCapacity)
                                && objSelectedAccessory.ModifyAmmoCapacity != "0"))
                        {
                            lblWeaponAmmoLabel.Visible = true;
                            lblWeaponAmmo.Visible = true;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdAmmoBonus))
                            {
                                int intAmmoBonus = objSelectedAccessory.TotalAmmoBonus;
                                if (intAmmoBonus != 0)
                                    sbdAmmoBonus.Append(
                                        (intAmmoBonus / 100.0m).ToString("+#,0%;-#,0%;0%", GlobalSettings.CultureInfo));
                                if (!string.IsNullOrEmpty(objSelectedAccessory.ModifyAmmoCapacity)
                                    && objSelectedAccessory.ModifyAmmoCapacity != "0")
                                    sbdAmmoBonus.Append(objSelectedAccessory.ModifyAmmoCapacity);
                                lblWeaponAmmo.Text = sbdAmmoBonus.ToString();
                            }
                        }
                        else
                        {
                            lblWeaponAmmoLabel.Visible = false;
                            lblWeaponAmmo.Visible = false;
                        }
                        lblWeaponModeLabel.Visible = false;
                        lblWeaponMode.Visible = false;

                        tlpWeaponsRanges.Visible = false;
                        tlpWeaponsCareer.Visible = false;
                        break;
                    }
                case Gear objGear:
                    {
                        gpbWeaponsCommon.Visible = true;
                        gpbWeaponsWeapon.Visible = false;
                        gpbWeaponsMatrix.Visible = true;

                        // Buttons
                        cmdDeleteWeapon.Enabled = !objGear.IncludedInParent;

                        // gpbWeaponsCommon
                        lblWeaponName.Text = objGear.CurrentDisplayNameShort;
                        lblWeaponCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                        int intGearMaxRatingValue = objGear.MaxRatingValue;
                        if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                        {
                            lblWeaponRatingLabel.Visible = true;
                            lblWeaponRating.Visible = true;
                            lblWeaponRating.Text = objGear.Rating.ToString(GlobalSettings.CultureInfo);
                        }
                        else
                        {
                            lblWeaponRatingLabel.Visible = false;
                            lblWeaponRating.Visible = false;
                        }
                        lblWeaponCapacityLabel.Visible = true;
                        lblWeaponCapacity.Visible = true;
                        lblWeaponCapacity.Text = objGear.DisplayCapacity;
                        lblWeaponAvail.Text = objGear.DisplayTotalAvail;
                        lblWeaponCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblWeaponSlotsLabel.Visible = false;
                        lblWeaponSlots.Visible = false;
                        lblWeaponConcealLabel.Visible = false;
                        lblWeaponConceal.Visible = false;
                        chkWeaponEquipped.Text = LanguageManager.GetString("Checkbox_Equipped");
                        chkWeaponEquipped.Enabled = !objGear.IncludedInParent;
                        chkWeaponEquipped.Checked = objGear.Equipped;
                        chkIncludedInWeapon.Visible = false;

                        // gpbWeaponsMatrix
                        int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                        lblWeaponDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objGear.RefreshMatrixAttributeComboBoxes(cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall);

                        chkWeaponActiveCommlink.Visible = objGear.IsCommlink;
                        chkWeaponActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkWeaponHomeNode.Visible = true;
                            chkWeaponHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                            chkWeaponHomeNode.Enabled = chkWeaponActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkWeaponHomeNode.Visible = false;

                        if (CharacterObject.Overclocker && objGear.Category == "Cyberdecks")
                        {
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstOverclocker))
                            {
                                lstOverclocker.Add(new ListItem("None", LanguageManager.GetString("String_None")));
                                lstOverclocker.Add(new ListItem("Attack", LanguageManager.GetString("String_Attack")));
                                lstOverclocker.Add(new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze")));
                                lstOverclocker.Add(new ListItem("Data Processing",
                                                                LanguageManager.GetString("String_DataProcessing")));
                                lstOverclocker.Add(
                                    new ListItem("Firewall", LanguageManager.GetString("String_Firewall")));

                                cboWeaponOverclocker.BeginUpdate();
                                cboWeaponOverclocker.PopulateWithListItems(lstOverclocker);
                                cboWeaponOverclocker.SelectedValue = objGear.Overclocked;
                                if (cboWeaponOverclocker.SelectedIndex == -1)
                                    cboWeaponOverclocker.SelectedIndex = 0;
                                cboWeaponOverclocker.EndUpdate();
                            }

                            cboWeaponOverclocker.Visible = true;
                            lblWeaponOverclockerLabel.Visible = true;
                        }
                        else
                        {
                            cboWeaponOverclocker.Visible = false;
                            lblWeaponOverclockerLabel.Visible = false;
                        }
                        break;
                    }
                default:
                    gpbWeaponsCommon.Visible = false;
                    gpbWeaponsWeapon.Visible = false;
                    gpbWeaponsMatrix.Visible = false;

                    // Buttons
                    cmdDeleteWeapon.Enabled = false;
                    break;
            }

            if (treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objMatrixCM)
            {
                tabWeaponCM.Visible = true;
                ProcessEquipmentConditionMonitorBoxDisplays(panWeaponMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
            }
            else
                tabWeaponCM.Visible = false;

            gpbWeaponsMatrix.Visible = treWeapons.SelectedNode.Tag is IHasMatrixAttributes ||
                                       treWeapons.SelectedNode.Tag is IHasWirelessBonus;

            IsRefreshing = false;
            flpWeapons.ResumeLayout();
        }

        /// <summary>
        /// Refresh the information for the currently displayed Armor.
        /// </summary>
        public void RefreshSelectedArmor()
        {
            IsRefreshing = true;
            flpArmor.SuspendLayout();

            if (treArmor.SelectedNode == null)
            {
                gpbArmorCommon.Visible = false;
                gpbArmorMatrix.Visible = false;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = treArmor.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpArmor.ResumeLayout();
                return;
            }

            if (treArmor.SelectedNode?.Tag is IHasWirelessBonus objHasWirelessBonus)
            {
                chkArmorWireless.Visible = true;
                chkArmorWireless.Checked = objHasWirelessBonus.WirelessOn;
            }
            else
                chkArmorWireless.Visible = false;

            if (treArmor.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblArmorSourceLabel.Visible = true;
                lblArmorSource.Visible = true;
                objSelected.SetSourceDetail(lblArmorSource);
            }
            else
            {
                lblArmorSourceLabel.Visible = false;
                lblArmorSource.Visible = false;
            }

            if (treArmor.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblArmorRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            if (treArmor.SelectedNode?.Tag is Armor objArmor)
            {
                gpbArmorCommon.Visible = true;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = true;

                // gpbArmorCommon
                lblArmorValueLabel.Visible = true;
                flpArmorValue.Visible = true;
                lblArmorValue.Text = objArmor.DisplayArmorValue;
                cmdArmorIncrease.Visible = true;
                cmdArmorIncrease.Enabled = objArmor.ArmorDamage < objArmor.TotalArmor &&
                                           objArmor.ArmorDamage < (string.IsNullOrEmpty(objArmor.ArmorOverrideValue) ? int.MaxValue : objArmor.TotalOverrideArmor);
                cmdArmorDecrease.Visible = true;
                cmdArmorDecrease.Enabled = objArmor.ArmorDamage > 0;
                lblArmorAvail.Text = objArmor.DisplayTotalAvail;
                lblArmorCapacity.Text = objArmor.DisplayCapacity;
                lblArmorRatingLabel.Visible = false;
                lblArmorRating.Visible = false;
                lblArmorCost.Text = objArmor.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                chkArmorEquipped.Visible = true;
                chkArmorEquipped.Checked = objArmor.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Visible = false;
            }
            else
            {
                string strSpace = LanguageManager.GetString("String_Space");
                if (treArmor.SelectedNode?.Tag is ArmorMod objArmorMod)
                {
                    gpbArmorCommon.Visible = true;
                    gpbArmorLocation.Visible = false;

                    // Buttons
                    cmdDeleteArmor.Enabled = !objArmorMod.IncludedInArmor;

                    // gpbArmorCommon
                    if (objArmorMod.Armor != 0)
                    {
                        lblArmorValueLabel.Visible = true;
                        flpArmorValue.Visible = true;
                        lblArmorValue.Text = objArmorMod.Armor.ToString("+0;-0;0", GlobalSettings.CultureInfo);
                    }
                    else
                    {
                        lblArmorValueLabel.Visible = false;
                        flpArmorValue.Visible = false;
                    }

                    cmdArmorIncrease.Visible = false;
                    cmdArmorDecrease.Visible = false;
                    lblArmorAvail.Text = objArmorMod.DisplayTotalAvail;
                    lblArmorCapacity.Text = objArmorMod.Parent.CapacityDisplayStyle == CapacityStyle.Zero
                        ? "[0]"
                        : objArmorMod.CalculatedCapacity;
                    if (!string.IsNullOrEmpty(objArmorMod.GearCapacity))
                        lblArmorCapacity.Text = objArmorMod.GearCapacity + '/' + lblArmorCapacity.Text + strSpace + '('
                                                +
                                                objArmorMod.GearCapacityRemaining.ToString(
                                                    "#,0.##", GlobalSettings.CultureInfo) +
                                                strSpace + LanguageManager.GetString("String_Remaining") + ')';
                    if (objArmorMod.MaximumRating > 1)
                    {
                        lblArmorRatingLabel.Visible = true;
                        lblArmorRating.Visible = true;
                        lblArmorRating.Text = objArmorMod.Rating.ToString(GlobalSettings.CultureInfo);
                    }
                    else
                    {
                        lblArmorRatingLabel.Visible = false;
                        lblArmorRating.Visible = false;
                    }

                    lblArmorCost.Text
                        = objArmorMod.TotalCost.ToString(CharacterObjectSettings.NuyenFormat,
                                                         GlobalSettings.CultureInfo) + '¥';
                    chkArmorEquipped.Visible = true;
                    chkArmorEquipped.Checked = objArmorMod.Equipped;
                    chkArmorEquipped.Enabled = true;
                    chkIncludedInArmor.Visible = true;
                    chkIncludedInArmor.Checked = objArmorMod.IncludedInArmor;
                }
                else
                {
                    switch (treArmor.SelectedNode?.Tag)
                    {
                        case Gear objSelectedGear:
                        {
                            gpbArmorCommon.Visible = true;
                            gpbArmorLocation.Visible = false;

                            // Buttons
                            cmdDeleteArmor.Enabled = !objSelectedGear.IncludedInParent;

                            // gpbArmorCommon
                            lblArmorValueLabel.Visible = false;
                            flpArmorValue.Visible = false;
                            lblArmorAvail.Text = objSelectedGear.DisplayTotalAvail;
                            CharacterObject.Armor.FindArmorGear(objSelectedGear.InternalId, out objArmor,
                                                                out objArmorMod);
                            if (objArmorMod != null)
                                lblArmorCapacity.Text = objSelectedGear.CalculatedCapacity;
                            else if (objArmor.CapacityDisplayStyle == CapacityStyle.Zero)
                                lblArmorCapacity.Text = '[' + 0.ToString(GlobalSettings.CultureInfo) + ']';
                            else
                                lblArmorCapacity.Text = objSelectedGear.CalculatedArmorCapacity;
                            int intMaxRatingValue = objSelectedGear.MaxRatingValue;
                            if (intMaxRatingValue > 1 && intMaxRatingValue != int.MaxValue)
                            {
                                lblArmorRatingLabel.Visible = true;
                                lblArmorRating.Visible = true;
                                lblArmorRating.Text = objSelectedGear.Rating.ToString(GlobalSettings.CultureInfo);
                            }
                            else
                            {
                                lblArmorRatingLabel.Visible = false;
                                lblArmorRating.Visible = false;
                            }

                            lblArmorCost.Text
                                = objSelectedGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat,
                                                                     GlobalSettings.CultureInfo) + '¥';
                            chkArmorEquipped.Visible = true;
                            chkArmorEquipped.Checked = objSelectedGear.Equipped;
                            chkArmorEquipped.Enabled = true;
                            chkIncludedInArmor.Visible = true;
                            chkIncludedInArmor.Checked = objSelectedGear.IncludedInParent;

                            break;
                        }
                        case Location objLocation:
                        {
                            gpbArmorCommon.Visible = false;
                            gpbArmorLocation.Visible = true;

                            // Buttons
                            cmdDeleteArmor.Enabled = true;

                            // gpbArmorLocation
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdArmorEquipped))
                            {
                                foreach (Armor objLoopArmor in CharacterObject.Armor.Where(
                                             objLoopArmor =>
                                                 objLoopArmor.Equipped && objLoopArmor.Location == objLocation))
                                {
                                    sbdArmorEquipped
                                        .Append(objLoopArmor.CurrentDisplayName).Append(strSpace).Append('(')
                                        .Append(objLoopArmor.DisplayArmorValue).AppendLine(')');
                                }

                                if (sbdArmorEquipped.Length > 0)
                                {
                                    --sbdArmorEquipped.Length;
                                    lblArmorEquipped.Text = sbdArmorEquipped.ToString();
                                }
                                else
                                    lblArmorEquipped.Text = LanguageManager.GetString("String_None");
                            }

                            break;
                        }
                        default:
                        {
                            if (treArmor.SelectedNode?.Tag.ToString() == "Node_SelectedArmor")
                            {
                                gpbArmorCommon.Visible = false;
                                gpbArmorMatrix.Visible = false;
                                gpbArmorLocation.Visible = true;

                                // Buttons
                                cmdDeleteArmor.Enabled = false;

                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdArmorEquipped))
                                {
                                    foreach (Armor objLoopArmor in CharacterObject.Armor.Where(
                                                 objLoopArmor =>
                                                     objLoopArmor.Equipped && objLoopArmor.Location == null))
                                    {
                                        sbdArmorEquipped.Append(objLoopArmor.CurrentDisplayName).Append(strSpace)
                                                        .Append('(').Append(objLoopArmor.DisplayArmorValue)
                                                        .AppendLine(')');
                                    }

                                    if (sbdArmorEquipped.Length > 0)
                                    {
                                        --sbdArmorEquipped.Length;
                                        lblArmorEquipped.Text = sbdArmorEquipped.ToString();
                                    }
                                    else
                                        lblArmorEquipped.Text = LanguageManager.GetString("String_None");
                                }
                            }
                            else
                            {
                                gpbArmorCommon.Visible = false;
                                gpbArmorMatrix.Visible = false;
                                gpbArmorLocation.Visible = false;

                                // Buttons
                                cmdDeleteArmor.Enabled = false;
                            }

                            break;
                        }
                    }
                }
            }

            if (treArmor.SelectedNode.Tag is IHasMatrixAttributes objHasMatrixAttributes)
            {
                int intDeviceRating = objHasMatrixAttributes.GetTotalMatrixAttribute("Device Rating");
                lblArmorDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                objHasMatrixAttributes.RefreshMatrixAttributeComboBoxes(cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall);
                if (CharacterObject.IsAI)
                {
                    chkArmorHomeNode.Visible = true;
                    chkArmorHomeNode.Checked = objHasMatrixAttributes.IsHomeNode(CharacterObject);
                    chkArmorHomeNode.Enabled = chkArmorActiveCommlink.Enabled &&
                                               objHasMatrixAttributes.GetTotalMatrixAttribute("Program Limit") >=
                                              (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                }
                else
                    chkArmorHomeNode.Visible = false;
                chkArmorActiveCommlink.Checked = objHasMatrixAttributes.IsActiveCommlink(CharacterObject);
                chkArmorActiveCommlink.Visible = objHasMatrixAttributes.IsCommlink;
                cboArmorOverclocker.BeginUpdate();
                if (CharacterObject.Overclocker && objHasMatrixAttributes is Gear objGear && objGear.Category == "Cyberdecks")
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(
                               Utils.ListItemListPool, out List<ListItem> lstOverclocker))
                    {
                        lstOverclocker.Add(new ListItem("None", LanguageManager.GetString("String_None")));
                        lstOverclocker.Add(new ListItem("Attack", LanguageManager.GetString("String_Attack")));
                        lstOverclocker.Add(new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze")));
                        lstOverclocker.Add(new ListItem("Data Processing",
                                                        LanguageManager.GetString("String_DataProcessing")));
                        lstOverclocker.Add(new ListItem("Firewall", LanguageManager.GetString("String_Firewall")));

                        cboArmorOverclocker.BeginUpdate();
                        cboArmorOverclocker.PopulateWithListItems(lstOverclocker);
                        cboArmorOverclocker.SelectedValue = objHasMatrixAttributes.Overclocked;
                        if (cboArmorOverclocker.SelectedIndex == -1)
                            cboArmorOverclocker.SelectedIndex = 0;
                        cboArmorOverclocker.EndUpdate();
                    }

                    cboArmorOverclocker.Visible = true;
                    lblArmorOverclockerLabel.Visible = true;
                }
                else
                {
                    cboArmorOverclocker.Visible = false;
                    lblArmorOverclockerLabel.Visible = false;
                }

                tabArmorCM.Visible = true;
                ProcessEquipmentConditionMonitorBoxDisplays(panArmorMatrixCM, objHasMatrixAttributes.MatrixCM, objHasMatrixAttributes.MatrixCMFilled);
                lblArmorDeviceRatingLabel.Visible = true;
                lblArmorDeviceRating.Visible = true;
                lblArmorAttackLabel.Visible = true;
                lblArmorSleazeLabel.Visible = true;
                lblArmorDataProcessingLabel.Visible = true;
                lblArmorFirewallLabel.Visible = true;
                cboArmorAttack.Visible = true;
                cboArmorSleaze.Visible = true;
                cboArmorDataProcessing.Visible = true;
                cboArmorFirewall.Visible = true;
                chkArmorWireless.Visible = treArmor.SelectedNode.Tag is IHasWirelessBonus;
                gpbArmorMatrix.Visible = true;
            }
            else
            {
                tabArmorCM.Visible = false;
                if (treArmor.SelectedNode.Tag is IHasWirelessBonus)
                {
                    cboArmorOverclocker.Visible = false;
                    lblArmorOverclockerLabel.Visible = false;
                    chkArmorHomeNode.Visible = false;
                    lblArmorDeviceRatingLabel.Visible = false;
                    lblArmorDeviceRating.Visible = false;
                    lblArmorAttackLabel.Visible = false;
                    lblArmorSleazeLabel.Visible = false;
                    lblArmorDataProcessingLabel.Visible = false;
                    lblArmorFirewallLabel.Visible = false;
                    cboArmorAttack.Visible = false;
                    cboArmorSleaze.Visible = false;
                    cboArmorDataProcessing.Visible = false;
                    cboArmorFirewall.Visible = false;
                    chkArmorWireless.Visible = true;
                    gpbArmorMatrix.Visible = true;
                }
                else
                    gpbArmorMatrix.Visible = false;
            }

            IsRefreshing = false;
            flpArmor.ResumeLayout();
        }

        /// <summary>
        /// Refresh the information for the currently displayed Gear.
        /// </summary>
        public void RefreshSelectedGear()
        {
            IsRefreshing = true;
            flpGear.SuspendLayout();

            if (treGear.SelectedNode == null || treGear.SelectedNode.Level == 0)
            {
                gpbGearCommon.Visible = false;
                gpbGearMatrix.Visible = false;
                tabGearCM.Visible = false;

                // Buttons
                cmdDeleteGear.Enabled = treGear.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpGear.ResumeLayout();
                return;
            }

            if (treGear.SelectedNode?.Tag is IHasWirelessBonus objHasWirelessBonus)
            {
                chkGearWireless.Visible = true;
                chkGearWireless.Checked = objHasWirelessBonus.WirelessOn;
            }
            else
                chkArmorWireless.Visible = false;

            if (treGear.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblGearSourceLabel.Visible = true;
                lblGearSource.Visible = true;
                objSelected.SetSourceDetail(lblGearSource);
            }
            else
            {
                lblGearSourceLabel.Visible = false;
                lblGearSource.Visible = false;
            }

            if (treGear.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblGearRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            if (treGear.SelectedNode?.Tag is Gear objGear)
            {
                gpbGearCommon.Visible = true;
                gpbGearMatrix.Visible = true;
                tabGearCM.Visible = true;

                // Buttons
                cmdDeleteGear.Enabled = !objGear.IncludedInParent;

                // gpbGearCommon
                lblGearName.Text = objGear.CurrentDisplayNameShort;
                lblGearCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                int intGearMaxRatingValue = objGear.MaxRatingValue;
                if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                {
                    lblGearRatingLabel.Visible = true;
                    lblGearRating.Visible = true;
                    lblGearRating.Text = objGear.Rating.ToString(GlobalSettings.CultureInfo);
                }
                else
                {
                    lblGearRatingLabel.Visible = false;
                    lblGearRating.Visible = false;
                }
                lblGearQty.Text = objGear.Quantity.ToString(GlobalSettings.CultureInfo);
                cmdGearIncreaseQty.Visible = true;
                cmdGearIncreaseQty.Enabled = !objGear.IncludedInParent;
                cmdGearReduceQty.Visible = true;
                cmdGearReduceQty.Enabled = !objGear.IncludedInParent;
                cmdGearReduceQty.Visible = true;
                cmdGearSplitQty.Enabled = !objGear.IncludedInParent;
                cmdGearReduceQty.Visible = true;
                cmdGearMergeQty.Enabled = !objGear.IncludedInParent;
                cmdGearMoveToVehicle.Visible = true;
                cmdGearMoveToVehicle.Enabled = !objGear.IncludedInParent && CharacterObject.Vehicles.Count > 0;
                lblGearAvail.Text = objGear.DisplayTotalAvail;
                try
                {
                    lblGearCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                }
                catch (FormatException)
                {
                    lblGearCost.Text = objGear.Cost + '¥';
                }
                lblGearCapacity.Text = objGear.DisplayCapacity;
                chkGearEquipped.Visible = true;
                chkGearEquipped.Checked = objGear.Equipped;
                // If this is a Program, determine if its parent Gear (if any) is a Commlink. If so, show the Equipped checkbox.
                if (objGear.IsProgram && objGear.Parent is IHasMatrixAttributes objCommlink && objCommlink.IsCommlink)
                {
                    chkGearEquipped.Text = LanguageManager.GetString("Checkbox_SoftwareRunning");
                }
                else
                {
                    chkGearEquipped.Text = LanguageManager.GetString("Checkbox_Equipped");
                }

                // gpbGearMatrix
                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                lblGearDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                objGear.RefreshMatrixAttributeComboBoxes(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);
                if (CharacterObject.IsAI)
                {
                    chkGearHomeNode.Visible = true;
                    chkGearHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                    chkGearHomeNode.Enabled = chkGearActiveCommlink.Enabled &&
                                              objGear.GetTotalMatrixAttribute("Program Limit") >=
                                              (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                }
                else
                    chkGearHomeNode.Visible = false;
                chkGearActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                chkGearActiveCommlink.Visible = objGear.IsCommlink;
                cboGearOverclocker.BeginUpdate();
                if (CharacterObject.Overclocker && objGear.Category == "Cyberdecks")
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(
                               Utils.ListItemListPool, out List<ListItem> lstOverclocker))
                    {
                        lstOverclocker.Add(new ListItem("None", LanguageManager.GetString("String_None")));
                        lstOverclocker.Add(new ListItem("Attack", LanguageManager.GetString("String_Attack")));
                        lstOverclocker.Add(new ListItem("Sleaze", LanguageManager.GetString("String_Sleaze")));
                        lstOverclocker.Add(new ListItem("Data Processing", LanguageManager.GetString("String_DataProcessing")));
                        lstOverclocker.Add(new ListItem("Firewall", LanguageManager.GetString("String_Firewall")));

                        cboGearOverclocker.BeginUpdate();
                        cboGearOverclocker.PopulateWithListItems(lstOverclocker);
                        cboGearOverclocker.SelectedValue = objGear.Overclocked;
                        if (cboGearOverclocker.SelectedIndex == -1)
                            cboGearOverclocker.SelectedIndex = 0;
                        cboGearOverclocker.EndUpdate();
                    }

                    cboGearOverclocker.Visible = true;
                    lblGearOverclockerLabel.Visible = true;
                }
                else
                {
                    cboGearOverclocker.Visible = false;
                    lblGearOverclockerLabel.Visible = false;
                }

                treGear.SelectedNode.Text = objGear.CurrentDisplayName;

                ProcessEquipmentConditionMonitorBoxDisplays(panGearMatrixCM, objGear.MatrixCM, objGear.MatrixCMFilled);
            }
            else
            {
                gpbGearCommon.Visible = false;
                gpbGearMatrix.Visible = false;
                tabGearCM.Visible = false;

                // Buttons
                cmdDeleteGear.Enabled = treGear.SelectedNode?.Tag is ICanRemove;
            }

            gpbGearMatrix.Visible = treGear.SelectedNode.Tag is IHasMatrixAttributes ||
                                    treGear.SelectedNode.Tag is IHasWirelessBonus;

            IsRefreshing = false;
            flpGear.ResumeLayout();
        }

        protected override string FormMode => LanguageManager.GetString("Title_CareerMode");

        /// <summary>
        /// Open the Select Cyberware window and handle adding to the Tree and Character.
        /// </summary>
        public async ValueTask<bool> PickCyberware(Cyberware objSelectedCyberware, Improvement.ImprovementSource objSource)
        {
            using (SelectCyberware frmPickCyberware = new SelectCyberware(CharacterObject, objSource, objSelectedCyberware))
            {
                List<Improvement> lstUsedImprovements;
                decimal decMultiplier = 1.0m;
                switch (objSource)
                {
                    // Apply the character's Cyberware Essence cost multiplier if applicable.
                    case Improvement.ImprovementSource.Cyberware:
                    {
                        lstUsedImprovements
                            = ImprovementManager.GetCachedImprovementListForValueOf(
                                CharacterObject, Improvement.ImprovementType.CyberwareEssCost);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1 - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplier);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1 - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            break;
                        }
                    // Apply the character's Bioware Essence cost multiplier if applicable.
                    case Improvement.ImprovementSource.Bioware:
                        {
                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareEssCost);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplier);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1 - objImprovement.Value / 100;
                                }

                                frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BasicBiowareEssCost);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.BasicBiowareESSMultiplier = decMultiplier;
                            }

                            // Genetech Cost multiplier.
                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.GenetechCostMultiplier);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.GenetechCostMultiplier = decMultiplier;
                            }

                            // Apply the character's Genetech Essence cost multiplier if applicable.
                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.GenetechEssMultiplier);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.GenetechEssMultiplier = decMultiplier;
                            }

                            break;
                        }
                }

                Dictionary<string, int> dicDisallowedMounts = new Dictionary<string, int>(6);
                Dictionary<string, int> dicHasMounts = new Dictionary<string, int>(6);
                if (objSelectedCyberware != null)
                {
                    frmPickCyberware.ForcedGrade = objSelectedCyberware.Grade;
                    frmPickCyberware.LockGrade();
                    frmPickCyberware.Subsystems = objSelectedCyberware.AllowedSubsystems;
                    // If the Cyberware has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that consume Capacity).
                    if (!objSelectedCyberware.Capacity.Contains('['))
                    {
                        frmPickCyberware.MaximumCapacity = objSelectedCyberware.CapacityRemaining;

                        // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                        if (CharacterObjectSettings.EnforceCapacity && objSelectedCyberware.CapacityRemaining < 0)
                        {
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                    }

                    foreach (string strLoop in objSelectedCyberware.BlocksMounts.SplitNoAlloc(',',
                        StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!dicDisallowedMounts.ContainsKey(strLoop))
                            dicDisallowedMounts.Add(strLoop, int.MaxValue);
                    }
                    string strLoopHasModularMount = objSelectedCyberware.HasModularMount;
                    if (!string.IsNullOrEmpty(strLoopHasModularMount) && !dicHasMounts.ContainsKey(strLoopHasModularMount))
                        dicHasMounts.Add(strLoopHasModularMount, int.MaxValue);
                    foreach (Cyberware objLoopCyberware in objSelectedCyberware.Children.DeepWhere(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                    {
                        foreach (string strLoop in objLoopCyberware.BlocksMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            string strKey = strLoop;
                            if (objSelectedCyberware.Location != objLoopCyberware.Location)
                                strKey += objLoopCyberware.Location;
                            if (!dicDisallowedMounts.ContainsKey(strKey))
                                dicDisallowedMounts.Add(strKey, int.MaxValue);
                        }
                        strLoopHasModularMount = objSelectedCyberware.Location != objLoopCyberware.Location
                            ? objLoopCyberware.HasModularMount + objLoopCyberware.Location
                            : objLoopCyberware.HasModularMount;
                        if (!string.IsNullOrEmpty(strLoopHasModularMount) && !dicHasMounts.ContainsKey(strLoopHasModularMount))
                            dicHasMounts.Add(strLoopHasModularMount, int.MaxValue);
                    }
                }
                else
                {
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setLoopDisallowedMounts))
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setLoopHasModularMount))
                    {
                        foreach (Cyberware objLoopCyberware in CharacterObject.Cyberware)
                        {
                            setLoopDisallowedMounts.Clear();
                            setLoopDisallowedMounts.AddRange(
                                objLoopCyberware.BlocksMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));
                            setLoopHasModularMount.Clear();
                            if (!string.IsNullOrEmpty(objLoopCyberware.HasModularMount))
                                setLoopHasModularMount.Add(objLoopCyberware.HasModularMount);
                            foreach (Cyberware objInnerLoopCyberware in objLoopCyberware.Children.DeepWhere(
                                         x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                            {
                                foreach (string strLoop in objInnerLoopCyberware.BlocksMounts.SplitNoAlloc(
                                             ',', StringSplitOptions.RemoveEmptyEntries))
                                    setLoopDisallowedMounts.Add(strLoop);
                                if (!string.IsNullOrEmpty(objInnerLoopCyberware.HasModularMount))
                                    setLoopHasModularMount.Add(objInnerLoopCyberware.HasModularMount);
                            }

                            foreach (string strLoop in setLoopDisallowedMounts)
                            {
                                string strKey = strLoop + objLoopCyberware.Location;
                                if (!dicDisallowedMounts.ContainsKey(strKey))
                                    dicDisallowedMounts.Add(strKey, objLoopCyberware.LimbSlotCount);
                                else
                                    dicDisallowedMounts[strKey] += objLoopCyberware.LimbSlotCount;
                            }

                            foreach (string strLoop in setLoopHasModularMount)
                            {
                                string strKey = strLoop + objLoopCyberware.Location;
                                if (!dicHasMounts.ContainsKey(strKey))
                                    dicHasMounts.Add(strKey, objLoopCyberware.LimbSlotCount);
                                else
                                    dicHasMounts[strKey] += objLoopCyberware.LimbSlotCount;
                            }
                        }
                    }
                }

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdDisallowedMounts))
                {
                    foreach (KeyValuePair<string, int> kvpLoop in dicDisallowedMounts)
                    {
                        string strKey = kvpLoop.Key;
                        if (strKey.EndsWith("Right", StringComparison.Ordinal))
                            continue;
                        int intValue = kvpLoop.Value;
                        if (strKey.EndsWith("Left", StringComparison.Ordinal))
                        {
                            strKey = strKey.TrimEndOnce("Left", true);
                            intValue = dicDisallowedMounts.TryGetValue(strKey + "Right", out int intExistingValue)
                                ? 2 * Math.Min(intValue, intExistingValue)
                                : 0;
                            if (dicDisallowedMounts.TryGetValue(strKey, out intExistingValue))
                                intValue += intExistingValue;
                        }

                        if (intValue >= CharacterObject.LimbCount(Cyberware.MountToLimbType(strKey)))
                            sbdDisallowedMounts.Append(strKey).Append(',');
                    }

                    // Remove trailing ","
                    if (sbdDisallowedMounts.Length > 0)
                        --sbdDisallowedMounts.Length;
                    frmPickCyberware.DisallowedMounts = sbdDisallowedMounts.ToString();
                }

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdHasMounts))
                {
                    foreach (KeyValuePair<string, int> kvpLoop in dicHasMounts)
                    {
                        string strKey = kvpLoop.Key;
                        if (strKey.EndsWith("Right", StringComparison.Ordinal))
                            continue;
                        int intValue = kvpLoop.Value;
                        if (strKey.EndsWith("Left", StringComparison.Ordinal))
                        {
                            strKey = strKey.TrimEndOnce("Left", true);
                            intValue = dicHasMounts.TryGetValue(strKey + "Right", out int intExistingValue)
                                ? 2 * Math.Min(intValue, intExistingValue)
                                : 0;
                            if (dicHasMounts.TryGetValue(strKey, out intExistingValue))
                                intValue += intExistingValue;
                        }

                        if (intValue >= CharacterObject.LimbCount(Cyberware.MountToLimbType(strKey)))
                            sbdHasMounts.Append(strKey).Append(',');
                    }

                    // Remove trailing ","
                    if (sbdHasMounts.Length > 0)
                        --sbdHasMounts.Length;
                    frmPickCyberware.HasModularMounts = sbdHasMounts.ToString();
                }

                await frmPickCyberware.ShowDialogSafeAsync(this);

                // Make sure the dialogue window was not canceled.
                if (frmPickCyberware.DialogResult == DialogResult.Cancel)
                    return false;

                // Open the Cyberware XML file and locate the selected piece.
                XmlNode objXmlCyberware = objSource == Improvement.ImprovementSource.Bioware
                    ? (await CharacterObject.LoadDataAsync("bioware.xml")).SelectSingleNode("/chummer/biowares/bioware[id = " + frmPickCyberware.SelectedCyberware.CleanXPath() + ']')
                    : (await CharacterObject.LoadDataAsync("cyberware.xml")).SelectSingleNode("/chummer/cyberwares/cyberware[id = " + frmPickCyberware.SelectedCyberware.CleanXPath() + ']');

                Cyberware objCyberware = new Cyberware(CharacterObject) { ESSDiscount = frmPickCyberware.SelectedESSDiscount, Parent = objSelectedCyberware};
                if (!objCyberware.Purchase(objXmlCyberware, objSource, frmPickCyberware.SelectedGrade,
                                           frmPickCyberware.SelectedRating, null,
                                           objSelectedCyberware?.Children ?? CharacterObject.Cyberware,
                                           CharacterObject.Vehicles,
                                           CharacterObject.Weapons, frmPickCyberware.Markup, frmPickCyberware.FreeCost,
                                           frmPickCyberware.BlackMarketDiscount, objParent: objSelectedCyberware))
                    objCyberware.DeleteCyberware();

                return frmPickCyberware.AddAgain;
            }
        }

        /// <summary>
        /// Select a piece of Gear to be added to the character.
        /// </summary>
        /// <param name="iParent">Parent to which the gear should be added.</param>
        /// <param name="objLocation">Location to which the gear should be added.</param>
        /// <param name="objStackGear">Whether or not the selected item should stack with a matching item on the character.</param>
        /// <param name="strForceItemValue">Force the user to select an item with the passed name.</param>
        /// <param name="objAmmoForWeapon">Gear is being bought as ammo for this weapon.</param>
        private async ValueTask<bool> PickGear(IHasChildren<Gear> iParent, Location objLocation = null, Gear objStackGear = null, string strForceItemValue = "", Weapon objAmmoForWeapon = null)
        {
            bool blnNullParent = false;
            Gear objSelectedGear = null;
            if (iParent is Gear gear)
            {
                objSelectedGear = gear;
            }
            else
            {
                blnNullParent = true;
            }

            // Open the Gear XML file and locate the selected Gear.
            XPathNavigator xmlParent = blnNullParent ? null : await objSelectedGear.GetNodeXPathAsync();

            using (new CursorWait(this))
            {
                string strCategories = string.Empty;

                if (xmlParent != null)
                {
                    XPathNodeIterator xmlAddonCategoryList = xmlParent.Select("addoncategory");
                    if (xmlAddonCategoryList.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(
                                   Utils.StringBuilderPool, out StringBuilder sbdCategories))
                        {
                            foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                                sbdCategories.Append(objXmlCategory.Value).Append(',');
                            // Remove the trailing comma.
                            --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }
                }

                using (SelectGear frmPickGear = new SelectGear(CharacterObject, objSelectedGear?.ChildAvailModifier ?? 0, objSelectedGear?.ChildCostMultiplier ?? 1, objSelectedGear, strCategories)
                {
                    ShowFlechetteAmmoOnly = objAmmoForWeapon?.Damage.EndsWith("(f)", StringComparison.Ordinal) == true
                })
                {
                    if (!blnNullParent)
                    {
                        // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that consume Capacity).
                        if (!string.IsNullOrEmpty(objSelectedGear.Capacity) && !objSelectedGear.Capacity.Contains('[') || objSelectedGear.Capacity.Contains("/["))
                        {
                            frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;

                            // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                            if (CharacterObjectSettings.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                            {
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    frmPickGear.ForceItemAmmoForWeaponType = objAmmoForWeapon?.WeaponType ?? string.Empty;

                    await frmPickGear.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickGear.DialogResult == DialogResult.Cancel)
                        return false;

                    // Open the Cyberware XML file and locate the selected piece.
                    XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                    XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                    // Create the new piece of Gear.
                    List<Weapon> lstWeapons = new List<Weapon>(1);

                    string strForceValue = objStackGear?.Extra ?? strForceItemValue;
                    if (string.IsNullOrEmpty(strForceValue) && objAmmoForWeapon != null)
                    {
                        //If the amount of an ammunition was increased, force the correct weapon category.
                        strForceValue = objAmmoForWeapon.AmmoCategory;
                    }

                    Gear objGear = new Gear(CharacterObject);
                    objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, strForceValue, objSelectedGear?.Equipped != false);

                    if (objGear.InternalId.IsEmptyGuid())
                        return frmPickGear.AddAgain;

                    objGear.Quantity = frmPickGear.SelectedQty;

                    objGear.Parent = blnNullParent ? null : objSelectedGear;

                    //Reduce the Cost for Black Market Pipeline
                    objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                    // Reduce the cost for Do It Yourself components.
                    if (frmPickGear.DoItYourself)
                        objGear.Cost = '(' + objGear.Cost + ") * 0.5";

                    decimal decCost;
                    if (objGear.Cost.Contains("Gear Cost"))
                    {
                        string strCost = objGear.Cost.Replace("Gear Cost", (objSelectedGear?.CalculatedCost ?? 0).ToString(GlobalSettings.InvariantCultureInfo));
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                        decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : objGear.TotalCost;
                    }
                    else
                    {
                        decCost = objGear.TotalCost;
                    }

                    Gear objStackWith = null;
                    // See if the character already has the item on them if they chose to stack.
                    if (frmPickGear.Stack)
                    {
                        objStackWith = objStackGear
                                       ?? CharacterObject.Gear.FirstOrDefault(x => x.Location == objLocation
                                           && objGear.IsIdenticalToOtherGear(x));
                    }

                    if (objStackWith != null)
                    {
                        if (objStackWith.InternalId.IsEmptyGuid())
                            return frmPickGear.AddAgain;
                        // If a match was found, we need to use the cost of a single item in the stack which can include plugins.
                        foreach (Gear objPlugin in objStackWith.Children)
                            decCost += objPlugin.TotalCost * frmPickGear.SelectedQty;
                    }

                    // Apply a markup if applicable.
                    if (frmPickGear.Markup != 0)
                    {
                        decCost *= 1 + frmPickGear.Markup / 100.0m;
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = objGear.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    if (!blnNullParent && objStackWith == null)
                    {
                        // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                        if (CharacterObjectSettings.EnforceCapacity &&
                            objSelectedGear.CapacityRemaining - objGear.PluginCapacity < 0)
                        {
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"),
                                await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }
                        // Multiply cost by parent gear's quantity
                        decCost *= objSelectedGear.Quantity;
                    }

                    ExpenseUndo objUndo = new ExpenseUndo();
                    // Check the item's Cost and make sure the character can afford it.
                    if (!frmPickGear.FreeCost)
                    {
                        if (decCost > CharacterObject.Nuyen)
                        {
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Remove any Improvements created by the Gear.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                            return frmPickGear.AddAgain;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                            DateTime.Now);
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

                        objLocation?.Children.Add(objGear);
                        if (!blnNullParent)
                        {
                            objSelectedGear.Children.Add(objGear);
                        }
                        else
                        {
                            CharacterObject.Gear.Add(objGear);
                        }
                    }

                    return frmPickGear.AddAgain;
                }
            }
        }

        /// <summary>
        /// Select a piece of Gear and add it to a piece of Armor.
        /// </summary>
        /// <param name="blnShowArmorCapacityOnly">Whether or not only items that consume capacity should be shown.</param>
        /// <param name="strSelectedId">Id attached to the object to which the gear should be added.</param>
        private async ValueTask<bool> PickArmorGear(string strSelectedId, bool blnShowArmorCapacityOnly = false)
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
            object objParent = objSelectedGear ?? objSelectedMod ?? (object)objSelectedArmor;

            using (new CursorWait(this))
            {
                string strCategories = string.Empty;

                if (!string.IsNullOrEmpty(strSelectedId) && objParent is IHasXmlDataNode objParentWithDataNode)
                {
                    XPathNodeIterator xmlAddonCategoryList
                        = (await objParentWithDataNode.GetNodeXPathAsync())?.Select("addoncategory");
                    if (xmlAddonCategoryList?.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdCategories))
                        {
                            foreach (XPathNavigator objXmlCategory in xmlAddonCategoryList)
                                sbdCategories.Append(objXmlCategory.Value).Append(',');
                            // Remove the trailing comma.
                            if (sbdCategories.Length > 0)
                                --sbdCategories.Length;
                            strCategories = sbdCategories.ToString();
                        }
                    }
                }

                using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objParent, strCategories)
                {
                    EnableStack = false,
                    ShowArmorCapacityOnly = blnShowArmorCapacityOnly,
                    CapacityDisplayStyle = objSelectedMod != null ? CapacityStyle.Standard : objSelectedArmor.CapacityDisplayStyle
                })
                {
                    // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that consume Capacity).
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        if (objSelectedGear != null && (!objSelectedGear.Capacity.Contains('[') || objSelectedGear.Capacity.Contains("/[")))
                        {
                            frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;

                            // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                            if (CharacterObjectSettings.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                            {
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                return false;
                            }
                        }
                        else if (objSelectedMod != null)
                        {
                            frmPickGear.MaximumCapacity = objSelectedMod.GearCapacityRemaining;

                            // Do not allow the user to add a new piece of Gear if its Capacity has been reached.
                            if (CharacterObjectSettings.EnforceCapacity && objSelectedMod.GearCapacityRemaining < 0)
                            {
                                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                return false;
                            }
                        }
                    }

                    await frmPickGear.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickGear.DialogResult == DialogResult.Cancel)
                        return false;

                    // Open the Cyberware XML file and locate the selected piece.
                    XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
                    XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                    // Create the new piece of Gear.
                    List<Weapon> lstWeapons = new List<Weapon>(1);

                    Gear objGear = new Gear(CharacterObject);
                    objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objSelectedGear?.Equipped ?? objSelectedMod?.Equipped ?? objSelectedArmor.Equipped);

                    if (objGear.InternalId.IsEmptyGuid())
                        return frmPickGear.AddAgain;

                    objGear.Quantity = frmPickGear.SelectedQty;

                    if (objSelectedGear != null)
                        objGear.Parent = objSelectedGear;

                    // Reduce the cost for Do It Yourself components.
                    if (frmPickGear.DoItYourself)
                        objGear.Cost = '(' + objGear.Cost + ") * 0.5";

                    // Apply a markup if applicable.
                    decimal decCost = objGear.TotalCost;
                    if (frmPickGear.Markup != 0)
                    {
                        decCost *= 1 + frmPickGear.Markup / 100.0m;
                    }

                    // Multiply the cost if applicable.
                    char chrAvail = objGear.TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                            decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                            break;

                        case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                            decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                            break;
                    }

                    Gear objMatchingGear = null;
                    // If this is Ammunition, see if the character already has it on them.
                    if (objGear.Category == "Ammunition" || !string.IsNullOrEmpty(objGear.AmmoForWeaponType))
                    {
                        TaggedObservableCollection<Gear> lstToSearch = string.IsNullOrEmpty(objSelectedGear?.Name) ? objSelectedArmor.GearChildren : objSelectedGear.Children;
                        objMatchingGear = lstToSearch.FirstOrDefault(x => objGear.IsIdenticalToOtherGear(x));
                    }

                    decimal decGearQuantity = 0;
                    if (objMatchingGear != null)
                    {
                        decGearQuantity = objGear.Quantity;
                        // A match was found, so increase the quantity instead.
                        objMatchingGear.Quantity += decGearQuantity;

                        objGear.DeleteGear();
                        if (CharacterObjectSettings.EnforceCapacity && objMatchingGear.CapacityRemaining < 0)
                        {
                            objMatchingGear.Quantity -= decGearQuantity;
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }
                    }
                    // Add the Gear.
                    else if (!string.IsNullOrEmpty(objSelectedGear?.Name))
                    {
                        objSelectedGear.Children.Add(objGear);
                        if (CharacterObjectSettings.EnforceCapacity && objSelectedGear.CapacityRemaining < 0)
                        {
                            objGear.DeleteGear();
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }
                    }
                    else if (!string.IsNullOrEmpty(objSelectedMod?.Name))
                    {
                        objSelectedMod.GearChildren.Add(objGear);
                        if (CharacterObjectSettings.EnforceCapacity && objSelectedMod.GearCapacityRemaining < 0)
                        {
                            objGear.DeleteGear();
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }
                    }
                    else
                    {
                        objSelectedArmor.GearChildren.Add(objGear);
                        if (CharacterObjectSettings.EnforceCapacity && objSelectedArmor.CapacityRemaining < 0)
                        {
                            objGear.DeleteGear();
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }
                    }

                    // Check the item's Cost and make sure the character can afford it.
                    if (!frmPickGear.FreeCost)
                    {
                        if (decCost > CharacterObject.Nuyen)
                        {
                            // Remove the added gear
                            if (objMatchingGear != null)
                                objMatchingGear.Quantity -= decGearQuantity;
                            // Remove any Improvements created by the Gear.
                            else
                                objGear.DeleteGear();
                            Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return frmPickGear.AddAgain;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                        objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseArmorGear") + await LanguageManager.GetStringAsync("String_Space") + objGear.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen,
                            DateTime.Now);
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

                    return frmPickGear.AddAgain;
                }
            }
        }

        /// <summary>
        /// Refresh the currently-selected Lifestyle.
        /// </summary>
        private void RefreshSelectedLifestyle()
        {
            IsRefreshing = true;
            flpLifestyleDetails.SuspendLayout();
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level == 0 || !(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
            {
                flpLifestyleDetails.Visible = false;
                cmdDeleteLifestyle.Enabled = treLifestyles.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpLifestyleDetails.ResumeLayout();
                return;
            }

            flpLifestyleDetails.Visible = true;
            cmdDeleteLifestyle.Enabled = true;

            lblLifestyleCost.Text = objLifestyle.TotalMonthlyCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            lblLifestyleMonths.Text = objLifestyle.Increments.ToString(GlobalSettings.CultureInfo);
            objLifestyle.SetSourceDetail(lblLifestyleSource);

            string strIncrementString;
            // Change the Cost/Month label.
            switch (objLifestyle.IncrementType)
            {
                case LifestyleIncrement.Day:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerDay");
                    strIncrementString = LanguageManager.GetString("String_Days");
                    break;

                case LifestyleIncrement.Week:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerWeek");
                    strIncrementString = LanguageManager.GetString("String_Weeks");
                    break;

                default:
                    lblLifestyleCostLabel.Text = LanguageManager.GetString("Label_SelectLifestyle_CostPerMonth");
                    strIncrementString = LanguageManager.GetString("String_Months");
                    break;
            }

            lblLifestyleMonthsLabel.Text = strIncrementString + string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_LifestylePermanent"), objLifestyle.IncrementsRequiredForPermanent.ToString(GlobalSettings.CultureInfo));
            cmdIncreaseLifestyleMonths.ToolTipText = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tab_IncreaseLifestyleMonths"), strIncrementString);
            cmdDecreaseLifestyleMonths.ToolTipText = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tab_DecreaseLifestyleMonths"), strIncrementString);

            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdQualities))
                {
                    sbdQualities.AppendJoin(',' + Environment.NewLine, objLifestyle.LifestyleQualities.Select(r => r.CurrentFormattedDisplayName));
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                 CharacterObject, Improvement.ImprovementType.LifestyleCost))
                    {
                        if (sbdQualities.Length > 0)
                            sbdQualities.AppendLine(',');

                        sbdQualities.Append(CharacterObject.GetObjectName(objImprovement))
                                    .Append(LanguageManager.GetString("String_Space")).Append('[')
                                    .Append(objImprovement.Value.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo))
                                    .Append("%]");
                    }

                    if (objLifestyle.FreeGrids.Count > 0)
                    {
                        if (sbdQualities.Length > 0)
                            sbdQualities.AppendLine(',');

                        sbdQualities.AppendJoin(',' + Environment.NewLine,
                                                objLifestyle.FreeGrids.Select(r => r.CurrentFormattedDisplayName));
                    }

                    lblLifestyleQualities.Text = sbdQualities.ToString();
                }

                lblBaseLifestyle.Text = objLifestyle.CurrentDisplayName;
                lblLifestyleQualitiesLabel.Visible = true;
                lblLifestyleQualities.Visible = true;
            }
            else
            {
                lblBaseLifestyle.Text = LanguageManager.GetString("String_Error");
                lblLifestyleQualitiesLabel.Visible = false;
                lblLifestyleQualities.Visible = false;
            }

            //Controls Visibility and content of the City, District and Borough Labels
            if (!string.IsNullOrEmpty(objLifestyle.City))
            {
                lblLifestyleCity.Text = objLifestyle.City;
                lblLifestyleCity.Visible = true;
                lblLifestyleCityLabel.Visible = true;
            }
            else
            {
                lblLifestyleCity.Visible = false;
                lblLifestyleCityLabel.Visible = false;
            }

            if (!string.IsNullOrEmpty(objLifestyle.District))
            {
                lblLifestyleDistrict.Text = objLifestyle.District;
                lblLifestyleDistrict.Visible = true;
                lblLifestyleDistrictLabel.Visible = true;
            }
            else
            {
                lblLifestyleDistrict.Visible = false;
                lblLifestyleDistrictLabel.Visible = false;
            }

            if (!string.IsNullOrEmpty(objLifestyle.Borough))
            {
                lblLifestyleBorough.Text = objLifestyle.Borough;
                lblLifestyleBorough.Visible = true;
                lblLifestyleBoroughLabel.Visible = true;
            }
            else
            {
                lblLifestyleBorough.Visible = false;
                lblLifestyleBoroughLabel.Visible = false;
            }

            IsRefreshing = false;
            flpLifestyleDetails.ResumeLayout();
        }

        /// <summary>
        /// Refresh the currently-selected Vehicle.
        /// </summary>
        private void RefreshSelectedVehicle()
        {
            IsRefreshing = true;
            flpVehicles.SuspendLayout();

            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();

            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level <= 0 || strSelectedId == "String_WeaponMounts")
            {
                panVehicleCM.Visible = false;
                gpbVehiclesCommon.Visible = false;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = treVehicles.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpVehicles.ResumeLayout();
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space");

            if (treVehicles.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblVehicleRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            if (treVehicles.SelectedNode?.Tag is IHasSource objSelected)
            {
                lblVehicleSourceLabel.Visible = true;
                lblVehicleSource.Visible = true;
                objSelected.SetSourceDetail(lblVehicleSource);
            }
            else
            {
                lblVehicleSourceLabel.Visible = false;
                lblVehicleSource.Visible = false;
            }

            switch (treVehicles.SelectedNode?.Tag)
            {
                // Locate the selected Vehicle.
                case Vehicle objVehicle:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = true;
                        gpbVehiclesWeapon.Visible = false;
                        gpbVehiclesMatrix.Visible = true;

                        // Buttons
                        cmdDeleteVehicle.Enabled = string.IsNullOrEmpty(objVehicle.ParentID);

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objVehicle.CurrentDisplayName;
                        lblVehicleCategory.Text = objVehicle.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Visible = false;
                        lblVehicleRating.Visible = false;
                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleGearQty.Visible = false;
                        cmdVehicleGearReduceQty.Visible = false;
                        lblVehicleAvail.Text = objVehicle.DisplayTotalAvail;
                        lblVehicleCost.Text = objVehicle.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = !CharacterObjectSettings.BookEnabled("R5");
                        lblVehicleSlots.Visible = !CharacterObjectSettings.BookEnabled("R5");
                        if (!CharacterObjectSettings.BookEnabled("R5"))
                            lblVehicleSlots.Text = objVehicle.Slots.ToString(GlobalSettings.CultureInfo) + strSpace + '('
                                                   + (objVehicle.Slots - objVehicle.SlotsUsed).ToString(GlobalSettings.CultureInfo)
                                                   + strSpace + LanguageManager.GetString("String_Remaining") + ')';
                        cmdVehicleMoveToInventory.Visible = false;
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        // gpbVehiclesVehicle
                        lblVehicleHandling.Text = objVehicle.TotalHandling;
                        lblVehicleAccel.Text = objVehicle.TotalAccel;
                        lblVehicleSpeed.Text = objVehicle.TotalSpeed;
                        lblVehiclePilot.Text = objVehicle.Pilot.ToString(GlobalSettings.CultureInfo);
                        lblVehicleBody.Text = objVehicle.TotalBody.ToString(GlobalSettings.CultureInfo);
                        lblVehicleArmor.Text = objVehicle.TotalArmor.ToString(GlobalSettings.CultureInfo);
                        lblVehicleSeats.Text = objVehicle.TotalSeats.ToString(GlobalSettings.CultureInfo);
                        lblVehicleSensor.Text = objVehicle.CalculatedSensor.ToString(GlobalSettings.CultureInfo);
                        if (CharacterObjectSettings.BookEnabled("R5"))
                        {
                            if (objVehicle.IsDrone && CharacterObjectSettings.DroneMods)
                            {
                                lblVehiclePowertrainLabel.Visible = false;
                                lblVehiclePowertrain.Visible = false;
                                lblVehicleCosmeticLabel.Visible = false;
                                lblVehicleCosmetic.Visible = false;
                                lblVehicleElectromagneticLabel.Visible = false;
                                lblVehicleElectromagnetic.Visible = false;
                                lblVehicleBodymodLabel.Visible = false;
                                lblVehicleBodymod.Visible = false;
                                lblVehicleWeaponsmodLabel.Visible = false;
                                lblVehicleWeaponsmod.Visible = false;
                                lblVehicleProtectionLabel.Visible = false;
                                lblVehicleProtection.Visible = false;
                                lblVehicleDroneModSlotsLabel.Visible = true;
                                lblVehicleDroneModSlots.Visible = true;
                                lblVehicleDroneModSlots.Text = objVehicle.DroneModSlotsUsed.ToString(GlobalSettings.CultureInfo) + '/' + objVehicle.DroneModSlots.ToString(GlobalSettings.CultureInfo);
                            }
                            else
                            {
                                lblVehiclePowertrainLabel.Visible = true;
                                lblVehiclePowertrain.Visible = true;
                                lblVehiclePowertrain.Text = objVehicle.PowertrainModSlotsUsed();
                                lblVehicleCosmeticLabel.Visible = true;
                                lblVehicleCosmetic.Visible = true;
                                lblVehicleCosmetic.Text = objVehicle.CosmeticModSlotsUsed();
                                lblVehicleElectromagneticLabel.Visible = true;
                                lblVehicleElectromagnetic.Visible = true;
                                lblVehicleElectromagnetic.Text = objVehicle.ElectromagneticModSlotsUsed();
                                lblVehicleBodymodLabel.Visible = true;
                                lblVehicleBodymod.Visible = true;
                                lblVehicleBodymod.Text = objVehicle.BodyModSlotsUsed();
                                lblVehicleWeaponsmodLabel.Visible = true;
                                lblVehicleWeaponsmod.Visible = true;
                                lblVehicleWeaponsmod.Text = objVehicle.WeaponModSlotsUsed();
                                lblVehicleProtectionLabel.Visible = true;
                                lblVehicleProtection.Visible = true;
                                lblVehicleProtection.Text = objVehicle.ProtectionModSlotsUsed();
                                lblVehicleDroneModSlotsLabel.Visible = false;
                                lblVehicleDroneModSlots.Visible = false;
                            }
                        }
                        else
                        {
                            lblVehiclePowertrainLabel.Visible = false;
                            lblVehiclePowertrain.Visible = false;
                            lblVehicleCosmeticLabel.Visible = false;
                            lblVehicleCosmetic.Visible = false;
                            lblVehicleElectromagneticLabel.Visible = false;
                            lblVehicleElectromagnetic.Visible = false;
                            lblVehicleBodymodLabel.Visible = false;
                            lblVehicleBodymod.Visible = false;
                            lblVehicleWeaponsmodLabel.Visible = false;
                            lblVehicleWeaponsmod.Visible = false;
                            lblVehicleProtectionLabel.Visible = false;
                            lblVehicleProtection.Visible = false;
                            lblVehicleDroneModSlotsLabel.Visible = false;
                            lblVehicleDroneModSlots.Visible = false;
                        }

                        // gpbVehiclesMatrix
                        int intDeviceRating = objVehicle.GetTotalMatrixAttribute("Device Rating");
                        lblVehicleDevice.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objVehicle.RefreshMatrixAttributeComboBoxes(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);
                        chkVehicleActiveCommlink.Visible = objVehicle.IsCommlink;
                        chkVehicleActiveCommlink.Checked = objVehicle.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkVehicleHomeNode.Visible = true;
                            chkVehicleHomeNode.Checked = objVehicle.IsHomeNode(CharacterObject);
                            chkVehicleHomeNode.Enabled = objVehicle.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkVehicleHomeNode.Visible = false;

                        UpdateSensor(objVehicle);
                        break;
                    }
                case WeaponMount objWeaponMount:
                    gpbVehiclesCommon.Visible = true;
                    gpbVehiclesVehicle.Visible = false;
                    gpbVehiclesWeapon.Visible = false;
                    gpbVehiclesMatrix.Visible = false;

                    // Buttons
                    cmdDeleteVehicle.Enabled = !objWeaponMount.IncludedInVehicle;

                    // gpbVehiclesCommon
                    lblVehicleCategory.Text = objWeaponMount.DisplayCategory(GlobalSettings.Language);
                    lblVehicleName.Text = objWeaponMount.CurrentDisplayName;
                    lblVehicleRatingLabel.Visible = false;
                    lblVehicleRating.Visible = false;
                    lblVehicleGearQtyLabel.Visible = false;
                    lblVehicleGearQty.Visible = false;
                    cmdVehicleGearReduceQty.Visible = false;
                    lblVehicleAvail.Text = objWeaponMount.DisplayTotalAvail;
                    lblVehicleCost.Text = objWeaponMount.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo);
                    lblVehicleSlotsLabel.Visible = true;
                    lblVehicleSlots.Visible = true;
                    lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString(GlobalSettings.CultureInfo);
                    cmdVehicleMoveToInventory.Visible = false;
                    cmdVehicleCyberwareChangeMount.Visible = false;
                    chkVehicleWeaponAccessoryInstalled.Visible = true;
                    chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Equipped;
                    chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                    chkVehicleIncludedInWeapon.Visible = false;
                    break;

                case VehicleMod objMod:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = false;
                        gpbVehiclesWeapon.Visible = false;
                        gpbVehiclesMatrix.Visible = false;

                        // Buttons
                        cmdDeleteVehicle.Enabled = !objMod.IncludedInVehicle;

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objMod.CurrentDisplayName;
                        lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleModification");
                        if (!objMod.MaxRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                        {
                            if (objMod.MaxRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                            {
                                objMod.MaxRating = objMod.Parent.TotalSeats.ToString(GlobalSettings.CultureInfo);
                            }
                            else if (objMod.MaxRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                            {
                                objMod.MaxRating = objMod.Parent.TotalBody.ToString(GlobalSettings.CultureInfo);
                            }
                            if (Convert.ToInt32(objMod.MaxRating, GlobalSettings.InvariantCultureInfo) > 0)
                            {
                                lblVehicleRatingLabel.Visible = true;
                                lblVehicleRating.Text = objMod.Rating.ToString(GlobalSettings.CultureInfo);
                                lblVehicleRating.Visible = true;
                            }
                            else
                            {
                                lblVehicleRatingLabel.Visible = false;
                                lblVehicleRating.Visible = false;
                            }
                        }
                        else
                        {
                            lblVehicleRatingLabel.Visible = true;
                            lblVehicleRating.Text = objMod.Rating.ToString(GlobalSettings.CultureInfo);
                            lblVehicleRating.Visible = true;
                        }
                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleGearQty.Visible = false;
                        cmdVehicleGearReduceQty.Visible = false;
                        lblVehicleAvail.Text = objMod.DisplayTotalAvail;
                        lblVehicleCost.Text = objMod.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        lblVehicleSlots.Text = objMod.CalculatedSlots.ToString(GlobalSettings.CultureInfo);
                        cmdVehicleMoveToInventory.Visible = false;
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objMod.Equipped;
                        chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                        chkVehicleIncludedInWeapon.Visible = false;
                        break;
                    }
                case Weapon objWeapon:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = false;
                        gpbVehiclesWeapon.Visible = true;
                        gpbVehiclesMatrix.Visible = true;

                        // Buttons
                        cmdDeleteVehicle.Enabled = !objWeapon.Cyberware && objWeapon.Category != "Gear" && !objWeapon.IncludedInWeapon && string.IsNullOrEmpty(objWeapon.ParentID) && !objWeapon.Category.StartsWith("Quality", StringComparison.Ordinal);

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objWeapon.CurrentDisplayName;
                        lblVehicleCategory.Text = objWeapon.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Visible = false;
                        lblVehicleRating.Visible = false;
                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleGearQty.Visible = false;
                        cmdVehicleGearReduceQty.Visible = false;
                        lblVehicleAvail.Text = objWeapon.DisplayTotalAvail;
                        lblVehicleCost.Text = objWeapon.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        if (!string.IsNullOrWhiteSpace(objWeapon.AccessoryMounts))
                        {
                            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                            {
                                using (new FetchSafelyFromPool<StringBuilder>(
                                           Utils.StringBuilderPool, out StringBuilder sbdSlotsText))
                                {
                                    foreach (string strMount in objWeapon.AccessoryMounts.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        sbdSlotsText.Append(LanguageManager.GetString("String_Mount" + strMount))
                                                    .Append('/');
                                    --sbdSlotsText.Length;
                                    lblWeaponSlots.Text = sbdSlotsText.ToString();
                                }
                            }
                            else
                                lblWeaponSlots.Text = objWeapon.AccessoryMounts;
                        }
                        else
                            lblWeaponSlots.Text = LanguageManager.GetString("String_None");
                        cmdVehicleMoveToInventory.Visible = !objWeapon.IncludedInWeapon;
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objWeapon.Equipped;
                        chkVehicleWeaponAccessoryInstalled.Enabled = objWeapon.ParentID != objWeapon.Parent?.InternalId && objWeapon.ParentID != objWeapon.ParentVehicle.InternalId;
                        chkVehicleIncludedInWeapon.Visible = true;
                        chkVehicleIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                        // gpbVehiclesWeapon
                        lblVehicleWeaponDamageLabel.Visible = true;
                        lblVehicleWeaponDamage.Text = objWeapon.DisplayDamage;
                        lblVehicleWeaponDamage.Visible = true;
                        lblVehicleWeaponAPLabel.Visible = true;
                        lblVehicleWeaponAP.Text = objWeapon.DisplayTotalAP;
                        lblVehicleWeaponAP.Visible = true;
                        lblVehicleWeaponAccuracyLabel.Visible = true;
                        lblVehicleWeaponAccuracy.Text = objWeapon.DisplayAccuracy;
                        lblVehicleWeaponAccuracy.Visible = true;
                        lblVehicleWeaponDicePoolLabel.Visible = true;
                        dpcVehicleWeaponDicePool.Visible = true;
                        dpcVehicleWeaponDicePool.DicePool = objWeapon.DicePool;
                        dpcVehicleWeaponDicePool.CanBeRolled = true;
                        dpcVehicleWeaponDicePool.SetLabelToolTip(objWeapon.DicePoolTooltip);
                        if (objWeapon.RangeType == "Ranged")
                        {
                            lblVehicleWeaponAmmoLabel.Visible = true;
                            lblVehicleWeaponAmmo.Visible = true;
                            lblVehicleWeaponAmmo.Text = objWeapon.DisplayAmmo;
                            lblVehicleWeaponModeLabel.Visible = true;
                            lblVehicleWeaponMode.Visible = true;
                            lblVehicleWeaponMode.Text = objWeapon.DisplayMode;

                            tlpVehiclesWeaponRanges.Visible = true;
                            lblVehicleWeaponRangeMain.Text = objWeapon.CurrentDisplayRange;
                            lblVehicleWeaponRangeAlternate.Text = objWeapon.CurrentDisplayAlternateRange;
                            Dictionary<string, string> dictionaryRanges = objWeapon.GetRangeStrings(GlobalSettings.CultureInfo);
                            lblVehicleWeaponRangeShortLabel.Text = objWeapon.RangeModifier("Short");
                            lblVehicleWeaponRangeMediumLabel.Text = objWeapon.RangeModifier("Medium");
                            lblVehicleWeaponRangeLongLabel.Text = objWeapon.RangeModifier("Long");
                            lblVehicleWeaponRangeExtremeLabel.Text = objWeapon.RangeModifier("Extreme");
                            lblVehicleWeaponRangeShort.Text = dictionaryRanges["short"];
                            lblVehicleWeaponRangeMedium.Text = dictionaryRanges["medium"];
                            lblVehicleWeaponRangeLong.Text = dictionaryRanges["long"];
                            lblVehicleWeaponRangeExtreme.Text = dictionaryRanges["extreme"];
                            lblVehicleWeaponAlternateRangeShort.Text = dictionaryRanges["alternateshort"];
                            lblVehicleWeaponAlternateRangeMedium.Text = dictionaryRanges["alternatemedium"];
                            lblVehicleWeaponAlternateRangeLong.Text = dictionaryRanges["alternatelong"];
                            lblVehicleWeaponAlternateRangeExtreme.Text = dictionaryRanges["alternateextreme"];
                        }
                        else
                        {
                            if (objWeapon.Ammo != "0")
                            {
                                lblVehicleWeaponAmmoLabel.Visible = true;
                                lblVehicleWeaponAmmo.Visible = true;
                                lblVehicleWeaponAmmo.Text = objWeapon.DisplayAmmo;
                            }
                            else
                            {
                                lblVehicleWeaponAmmoLabel.Visible = false;
                                lblVehicleWeaponAmmo.Visible = false;
                            }
                            lblVehicleWeaponModeLabel.Visible = false;
                            lblVehicleWeaponMode.Visible = false;

                            tlpVehiclesWeaponRanges.Visible = false;
                        }

                        if (objWeapon.RangeType == "Ranged" || objWeapon.RangeType == "Melee" && objWeapon.Ammo != "0")
                        {
                            tlpVehiclesWeaponCareer.Visible = true;
                            lblVehicleWeaponAmmoRemaining.Text = objWeapon.AmmoRemaining.ToString(GlobalSettings.CultureInfo);
                            cmdFireVehicleWeapon.Enabled = objWeapon.AmmoRemaining != 0;

                            cboVehicleWeaponFiringMode.SelectedValue = objWeapon.FireMode;
                            cmsVehicleAmmoSingleShot.Enabled =
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeSingleShot")) ||
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic"));
                            cmsVehicleAmmoShortBurst.Enabled =
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeBurstFire")) ||
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));
                            cmsVehicleAmmoLongBurst.Enabled =
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));
                            cmsVehicleAmmoFullBurst.Enabled =
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));
                            cmsVehicleAmmoSuppressiveFire.Enabled =
                                objWeapon.AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

                            // Melee Weapons with Ammo are considered to be Single Shot.
                            if (objWeapon.RangeType == "Melee" && objWeapon.Ammo != "0")
                                cmsVehicleAmmoSingleShot.Enabled = true;

                            if (cmsVehicleAmmoSingleShot.Enabled)
                                cmsVehicleAmmoSingleShot.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_SingleShot")
                                    , objWeapon.SingleShot,
                                    LanguageManager.GetString(objWeapon.SingleShot == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));
                            if (cmsVehicleAmmoShortBurst.Enabled)
                                cmsVehicleAmmoShortBurst.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_ShortBurst")
                                    , objWeapon.ShortBurst,
                                    LanguageManager.GetString(objWeapon.ShortBurst == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));
                            if (cmsVehicleAmmoLongBurst.Enabled)
                                cmsVehicleAmmoLongBurst.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_LongBurst")
                                    , objWeapon.LongBurst,
                                    LanguageManager.GetString(objWeapon.LongBurst == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));
                            if (cmsVehicleAmmoFullBurst.Enabled)
                                cmsVehicleAmmoFullBurst.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_FullBurst")
                                    , objWeapon.FullBurst,
                                    LanguageManager.GetString(objWeapon.FullBurst == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));
                            if (cmsVehicleAmmoSuppressiveFire.Enabled)
                                cmsVehicleAmmoSuppressiveFire.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_SuppressiveFire")
                                    , objWeapon.Suppressive,
                                    LanguageManager.GetString(objWeapon.Suppressive == 1
                                        ? "String_Bullet"
                                        : "String_Bullets"));

                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstAmmo))
                            {
                                int intCurrentSlot = objWeapon.ActiveAmmoSlot;

                                for (int i = 1; i <= objWeapon.AmmoSlots; i++)
                                {
                                    objWeapon.ActiveAmmoSlot = i;
                                    Gear objVehicleGear
                                        = objWeapon.ParentVehicle.GearChildren.DeepFindById(objWeapon.AmmoLoaded);
                                    string strAmmoName = objVehicleGear?.DisplayNameShort(GlobalSettings.Language)
                                                         ?? LanguageManager.GetString(
                                                             objWeapon.AmmoRemaining == 0
                                                                 ? "String_Empty"
                                                                 : "String_ExternalSource");
                                    if (objWeapon.AmmoSlots > 1)
                                        strAmmoName += strSpace + '('
                                                                + string.Format(
                                                                    GlobalSettings.CultureInfo,
                                                                    LanguageManager.GetString("String_SlotNumber"),
                                                                    i.ToString(GlobalSettings.CultureInfo)) + ')';

                                    using (new FetchSafelyFromPool<StringBuilder>(
                                               Utils.StringBuilderPool, out StringBuilder sbdPlugins))
                                    {
                                        foreach (Gear objCurrentAmmo in objWeapon.ParentVehicle.GearChildren)
                                        {
                                            if (objCurrentAmmo.InternalId != objWeapon.AmmoLoaded)
                                                continue;
                                            foreach (Gear objChild in objCurrentAmmo.Children)
                                            {
                                                sbdPlugins.Append(objChild.DisplayNameShort(GlobalSettings.Language))
                                                          .Append(',').Append(strSpace);
                                            }
                                        }

                                        // Remove the trailing comma.
                                        if (sbdPlugins.Length > 0)
                                        {
                                            sbdPlugins.Length -= 1 + strSpace.Length;
                                            strAmmoName += strSpace + '[' + sbdPlugins + ']';
                                        }
                                    }

                                    lstAmmo.Add(new ListItem(i.ToString(GlobalSettings.InvariantCultureInfo),
                                                             strAmmoName));
                                }

                                objWeapon.ActiveAmmoSlot = intCurrentSlot;
                                cboVehicleWeaponAmmo.BeginUpdate();
                                cboVehicleWeaponAmmo.PopulateWithListItems(lstAmmo);
                                cboVehicleWeaponAmmo.SelectedValue
                                    = objWeapon.ActiveAmmoSlot.ToString(GlobalSettings.InvariantCultureInfo);
                                if (cboVehicleWeaponAmmo.SelectedIndex == -1)
                                    cboVehicleWeaponAmmo.SelectedIndex = 0;
                                cboVehicleWeaponAmmo.Enabled = lstAmmo.Count > 1;
                                cboVehicleWeaponAmmo.EndUpdate();
                            }
                        }
                        else
                        {
                            tlpVehiclesWeaponCareer.Visible = false;
                        }

                        // gpbVehiclesMatrix
                        int intDeviceRating = objWeapon.GetTotalMatrixAttribute("Device Rating");
                        lblVehicleDevice.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objWeapon.RefreshMatrixAttributeComboBoxes(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);
                        chkVehicleActiveCommlink.Visible = objWeapon.IsCommlink;
                        chkVehicleActiveCommlink.Checked = objWeapon.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkVehicleHomeNode.Visible = true;
                            chkVehicleHomeNode.Checked = objWeapon.IsHomeNode(CharacterObject);
                            chkVehicleHomeNode.Enabled = objWeapon.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkVehicleHomeNode.Visible = false;

                        break;
                    }
                case WeaponAccessory objAccessory:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = false;
                        gpbVehiclesWeapon.Visible = true;
                        gpbVehiclesMatrix.Visible = false;

                        // Buttons
                        cmdDeleteVehicle.Enabled = !objAccessory.IncludedInWeapon;

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objAccessory.CurrentDisplayNameShort;
                        lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeaponAccessory");
                        if (objAccessory.MaxRating > 0)
                        {
                            lblVehicleRatingLabel.Visible = true;
                            lblVehicleRating.Visible = true;
                            lblVehicleRating.Text = objAccessory.Rating.ToString(GlobalSettings.CultureInfo);
                        }
                        else
                        {
                            lblVehicleRatingLabel.Visible = false;
                            lblVehicleRating.Visible = false;
                        }

                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleGearQty.Visible = false;
                        cmdVehicleGearReduceQty.Visible = false;
                        lblVehicleAvail.Text = objAccessory.DisplayTotalAvail;
                        lblVehicleCost.Text = objAccessory.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMount))
                        {
                            foreach (string strCurrentMount in objAccessory.Mount.SplitNoAlloc(
                                         '/', StringSplitOptions.RemoveEmptyEntries))
                                sbdMount.Append(LanguageManager.GetString("String_Mount" + strCurrentMount))
                                        .Append('/');
                            // Remove the trailing /
                            if (sbdMount.Length > 0)
                                --sbdMount.Length;
                            if (!string.IsNullOrEmpty(objAccessory.ExtraMount) && objAccessory.ExtraMount != "None")
                            {
                                bool boolHaveAddedItem = false;
                                foreach (string strCurrentExtraMount in objAccessory.ExtraMount.SplitNoAlloc(
                                             '/', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!boolHaveAddedItem)
                                    {
                                        sbdMount.Append(strSpace).Append('+').Append(strSpace);
                                        boolHaveAddedItem = true;
                                    }

                                    sbdMount.Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount))
                                            .Append('/');
                                }

                                // Remove the trailing /
                                if (boolHaveAddedItem)
                                    --sbdMount.Length;
                            }

                            lblVehicleSlotsLabel.Visible = true;
                            lblVehicleSlots.Visible = true;
                            lblVehicleSlots.Text = sbdMount.ToString();
                        }

                        cmdVehicleMoveToInventory.Visible = false;
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Enabled = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Equipped;
                        chkVehicleIncludedInWeapon.Visible = true;
                        chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;

                        // gpbWeaponsWeapon
                        gpbWeaponsWeapon.Text = LanguageManager.GetString("String_WeaponAccessory");
                        if (string.IsNullOrEmpty(objAccessory.Damage))
                        {
                            lblVehicleWeaponDamageLabel.Visible = false;
                            lblVehicleWeaponDamage.Visible = false;
                        }
                        else
                        {
                            lblVehicleWeaponDamageLabel.Visible = !string.IsNullOrEmpty(objAccessory.Damage);
                            lblVehicleWeaponDamage.Visible = !string.IsNullOrEmpty(objAccessory.Damage);
                            lblVehicleWeaponDamage.Text = Convert.ToInt32(objAccessory.Damage, GlobalSettings.InvariantCultureInfo).ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (string.IsNullOrEmpty(objAccessory.AP))
                        {
                            lblVehicleWeaponAPLabel.Visible = false;
                            lblVehicleWeaponAP.Visible = false;
                        }
                        else
                        {
                            lblVehicleWeaponAPLabel.Visible = true;
                            lblVehicleWeaponAP.Visible = true;
                            lblVehicleWeaponAP.Text = Convert.ToInt32(objAccessory.AP, GlobalSettings.InvariantCultureInfo).ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (objAccessory.Accuracy == 0)
                        {
                            lblVehicleWeaponAccuracyLabel.Visible = false;
                            lblVehicleWeaponAccuracy.Visible = false;
                        }
                        else
                        {
                            lblVehicleWeaponAccuracyLabel.Visible = true;
                            lblVehicleWeaponAccuracy.Visible = true;
                            lblVehicleWeaponAccuracy.Text = objAccessory.Accuracy.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                        }
                        if (objAccessory.DicePool == 0)
                        {
                            lblVehicleWeaponDicePoolLabel.Visible = false;
                            dpcVehicleWeaponDicePool.Visible = false;
                        }
                        else
                        {
                            lblVehicleWeaponDicePoolLabel.Visible = true;
                            dpcVehicleWeaponDicePool.Visible = true;
                            dpcVehicleWeaponDicePool.DicePool = objAccessory.DicePool;
                            dpcVehicleWeaponDicePool.CanBeRolled = false;
                            dpcVehicleWeaponDicePool.SetLabelToolTip(string.Empty);
                        }
                        if (objAccessory.TotalAmmoBonus != 0
                            || (!string.IsNullOrEmpty(objAccessory.ModifyAmmoCapacity)
                                && objAccessory.ModifyAmmoCapacity != "0"))
                        {
                            lblVehicleWeaponAmmoLabel.Visible = true;
                            lblVehicleWeaponAmmo.Visible = true;
                            using (new FetchSafelyFromPool<StringBuilder>(
                                       Utils.StringBuilderPool, out StringBuilder sbdAmmoBonus))
                            {
                                int intAmmoBonus = objAccessory.TotalAmmoBonus;
                                if (intAmmoBonus != 0)
                                    sbdAmmoBonus.Append(
                                        (intAmmoBonus / 100.0m).ToString("+#,0%;-#,0%;0%", GlobalSettings.CultureInfo));
                                if (!string.IsNullOrEmpty(objAccessory.ModifyAmmoCapacity)
                                    && objAccessory.ModifyAmmoCapacity != "0")
                                    sbdAmmoBonus.Append(objAccessory.ModifyAmmoCapacity);
                                lblVehicleWeaponAmmo.Text = sbdAmmoBonus.ToString();
                            }
                        }
                        else
                        {
                            lblVehicleWeaponAmmoLabel.Visible = false;
                            lblVehicleWeaponAmmo.Visible = false;
                        }
                        lblVehicleWeaponModeLabel.Visible = false;
                        lblVehicleWeaponMode.Visible = false;

                        tlpVehiclesWeaponRanges.Visible = false;
                        tlpVehiclesWeaponCareer.Visible = false;
                        break;
                    }
                case Cyberware objCyberware:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = false;
                        gpbVehiclesWeapon.Visible = false;
                        gpbVehiclesMatrix.Visible = true;

                        // Buttons
                        cmdDeleteVehicle.Enabled = string.IsNullOrEmpty(objCyberware.ParentID);

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalSettings.Language);
                        lblVehicleCategory.Text = objCyberware.DisplayCategory(GlobalSettings.Language);
                        if (objCyberware.MaxRating == 0)
                        {
                            lblVehicleRating.Visible = false;
                            lblVehicleRatingLabel.Visible = false;
                        }
                        else
                        {
                            lblVehicleRating.Visible = true;
                            lblVehicleRating.Text = objCyberware.Rating.ToString(GlobalSettings.CultureInfo);
                            lblVehicleRatingLabel.Visible = true;
                        }
                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleGearQty.Visible = false;
                        cmdVehicleGearReduceQty.Visible = false;
                        lblVehicleAvail.Text = objCyberware.DisplayTotalAvail;
                        lblVehicleCost.Text = objCyberware.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        cmdVehicleMoveToInventory.Visible = false;
                        cmdVehicleCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        // gpbVehiclesMatrix
                        int intDeviceRating = objCyberware.GetTotalMatrixAttribute("Device Rating");
                        lblVehicleDevice.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objCyberware.RefreshMatrixAttributeComboBoxes(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);

                        chkVehicleActiveCommlink.Visible = objCyberware.IsCommlink;
                        chkVehicleActiveCommlink.Checked = objCyberware.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkVehicleHomeNode.Visible = true;
                            chkVehicleHomeNode.Checked = objCyberware.IsHomeNode(CharacterObject);
                            chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objCyberware.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkVehicleHomeNode.Visible = false;

                        break;
                    }
                case Gear objGear:
                    {
                        gpbVehiclesCommon.Visible = true;
                        gpbVehiclesVehicle.Visible = false;
                        gpbVehiclesWeapon.Visible = false;
                        gpbVehiclesMatrix.Visible = true;

                        // Buttons
                        cmdDeleteVehicle.Enabled = !objGear.IncludedInParent;

                        // gpbVehiclesCommon
                        lblVehicleName.Text = objGear.DisplayNameShort(GlobalSettings.Language);
                        lblVehicleCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Visible = true;
                        lblVehicleRating.Visible = true;
                        lblVehicleRating.Text = objGear.Rating.ToString(GlobalSettings.CultureInfo);
                        lblVehicleGearQtyLabel.Visible = true;
                        lblVehicleGearQty.Visible = true;
                        string strQuantity = objGear.DisplayQuantity(GlobalSettings.CultureInfo);
                        if (string.IsNullOrEmpty(strQuantity))
                            strQuantity = 1.ToString(GlobalSettings.CultureInfo);
                        lblVehicleGearQty.Text = strQuantity;
                        cmdVehicleGearReduceQty.Enabled = !objGear.IncludedInParent;
                        lblVehicleAvail.Text = objGear.DisplayTotalAvail;
                        lblVehicleCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        lblVehicleSlots.Text = objGear.CalculatedCapacity + LanguageManager.GetString("String_Space") + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Remaining") + ')';
                        cmdVehicleMoveToInventory.Enabled = !objGear.IncludedInParent;
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        // gpbVehiclesMatrix
                        int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                        lblVehicleDevice.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        objGear.RefreshMatrixAttributeComboBoxes(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);

                        chkVehicleActiveCommlink.Visible = objGear.IsCommlink;
                        chkVehicleActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkVehicleHomeNode.Visible = true;
                            chkVehicleHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                            chkVehicleHomeNode.Enabled = chkVehicleActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkVehicleHomeNode.Visible = false;

                        break;
                    }
                default:
                    gpbVehiclesCommon.Visible = false;
                    gpbVehiclesVehicle.Visible = false;
                    gpbVehiclesWeapon.Visible = false;
                    gpbVehiclesMatrix.Visible = false;

                    // Buttons
                    cmdDeleteVehicle.Enabled = false;
                    break;
            }

            panVehicleCM.Visible = treVehicles.SelectedNode?.Tag is IHasPhysicalConditionMonitor ||
                                   treVehicles.SelectedNode?.Tag is IHasMatrixAttributes;

            gpbVehiclesMatrix.Visible = treVehicles.SelectedNode.Tag is IHasMatrixAttributes ||
                                        treVehicles.SelectedNode.Tag is IHasWirelessBonus;

            if (panVehicleCM.Visible)
            {
                if (treVehicles.SelectedNode?.Tag is IHasPhysicalConditionMonitor objCM)
                {
                    ProcessEquipmentConditionMonitorBoxDisplays(panVehiclePhysicalCM, objCM.PhysicalCM, objCM.PhysicalCMFilled);
                }

                if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objMatrixCM)
                {
                    ProcessEquipmentConditionMonitorBoxDisplays(panVehicleMatrixCM, objMatrixCM.MatrixCM, objMatrixCM.MatrixCMFilled);
                }
            }

            IsRefreshing = false;
            flpVehicles.ResumeLayout();
        }

        private void ExpenseEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoRepopulateKarmaExpenseList = false;
            bool blnDoRepopulateNuyenExpenseList = false;
            if (e == null || e.Action == NotifyCollectionChangedAction.Reset)
            {
                blnDoRepopulateKarmaExpenseList = true;
                blnDoRepopulateNuyenExpenseList = true;
            }
            else
            {
                switch (e.Action)
                {
                    // TODO: Find a way to add, remove, or replace a single item at a time instead of needing to redraw the entire list
                    case NotifyCollectionChangedAction.Add:
                        foreach (ExpenseLogEntry objEntry in e.NewItems)
                        {
                            switch (objEntry.Type)
                            {
                                case ExpenseType.Karma:
                                    blnDoRepopulateKarmaExpenseList = blnDoRepopulateKarmaExpenseList || objEntry.Amount != 0 || chkShowFreeKarma.Checked;
                                    break;

                                case ExpenseType.Nuyen:
                                    blnDoRepopulateNuyenExpenseList = blnDoRepopulateNuyenExpenseList || objEntry.Amount != 0 || chkShowFreeNuyen.Checked;
                                    break;
                            }
                            if (blnDoRepopulateKarmaExpenseList && blnDoRepopulateNuyenExpenseList)
                                break;
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (ExpenseLogEntry objEntry in e.OldItems)
                        {
                            switch (objEntry.Type)
                            {
                                case ExpenseType.Karma:
                                    blnDoRepopulateKarmaExpenseList = blnDoRepopulateKarmaExpenseList || objEntry.Amount != 0 || chkShowFreeKarma.Checked;
                                    break;

                                case ExpenseType.Nuyen:
                                    blnDoRepopulateNuyenExpenseList = blnDoRepopulateNuyenExpenseList || objEntry.Amount != 0 || chkShowFreeNuyen.Checked;
                                    break;
                            }
                            if (blnDoRepopulateKarmaExpenseList && blnDoRepopulateNuyenExpenseList)
                                break;
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (ExpenseLogEntry objEntry in e.OldItems)
                        {
                            switch (objEntry.Type)
                            {
                                case ExpenseType.Karma:
                                    blnDoRepopulateKarmaExpenseList = blnDoRepopulateKarmaExpenseList || objEntry.Amount != 0 || chkShowFreeKarma.Checked;
                                    break;

                                case ExpenseType.Nuyen:
                                    blnDoRepopulateNuyenExpenseList = blnDoRepopulateNuyenExpenseList || objEntry.Amount != 0 || chkShowFreeNuyen.Checked;
                                    break;
                            }
                            if (blnDoRepopulateKarmaExpenseList && blnDoRepopulateNuyenExpenseList)
                                break;
                        }

                        if (!blnDoRepopulateKarmaExpenseList || !blnDoRepopulateNuyenExpenseList)
                        {
                            foreach (ExpenseLogEntry objEntry in e.NewItems)
                            {
                                switch (objEntry.Type)
                                {
                                    case ExpenseType.Karma:
                                        blnDoRepopulateKarmaExpenseList = blnDoRepopulateKarmaExpenseList || objEntry.Amount != 0 || chkShowFreeKarma.Checked;
                                        break;

                                    case ExpenseType.Nuyen:
                                        blnDoRepopulateNuyenExpenseList = blnDoRepopulateNuyenExpenseList || objEntry.Amount != 0 || chkShowFreeNuyen.Checked;
                                        break;
                                }
                                if (blnDoRepopulateKarmaExpenseList && blnDoRepopulateNuyenExpenseList)
                                    break;
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        return;
                }
            }
            if (blnDoRepopulateKarmaExpenseList)
                RepopulateKarmaExpenseList();
            if (blnDoRepopulateNuyenExpenseList)
                RepopulateNuyenExpenseList();
        }

        private void chkShowFreeKarma_CheckedChanged(object sender, EventArgs e)
        {
            if (CharacterObject.ExpenseEntries.Any(x => x.Type == ExpenseType.Karma && x.Amount == 0))
            {
                RepopulateKarmaExpenseList();
            }
        }

        private void RepopulateKarmaExpenseList()
        {
            using (new CursorWait(this))
            {
                lstKarma.Items.Clear();
                lstKarma.ContextMenuStrip = null;
                chtKarma.SuspendLayout();
                chtKarma.ExpenseValues.Clear();
                decimal decKarmaValue = 0;
                //Find the last karma/nuyen entry as well in case a chart only contains one point
                DateTime KarmaLast = DateTime.MinValue;
                foreach (ExpenseLogEntry objExpense in CharacterObject.ExpenseEntries)
                {
                    if (objExpense.Type != ExpenseType.Karma || (objExpense.Amount == 0 && !chkShowFreeKarma.Checked))
                        continue;
                    ListViewItemWithValue.ListViewSubItemWithValue objAmountItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense.Amount,
                            Text = objExpense.Amount.ToString("#,0.##", GlobalSettings.CultureInfo)
                        };
                    ListViewItemWithValue.ListViewSubItemWithValue objReasonItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense.Reason,
                            Text = objExpense.DisplayReason(GlobalSettings.Language)
                        };
                    ListViewItemWithValue.ListViewSubItemWithValue objInternalIdItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense,
                            Text = objExpense.InternalId
                        };

                    ListViewItemWithValue objItem = new ListViewItemWithValue(objExpense.Date,
                        objExpense.Date.ToString(GlobalSettings.CustomDateTimeFormats
                                ? GlobalSettings.CustomDateFormat
                                  + ' ' + GlobalSettings.CustomTimeFormat
                                : GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern
                                  + ' ' + GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern,
                            GlobalSettings.CultureInfo)
                    );
                    objItem.SubItems.Add(objAmountItem);
                    objItem.SubItems.Add(objReasonItem);
                    objItem.SubItems.Add(objInternalIdItem);

                    lstKarma.Items.Add(objItem);
                    if (objExpense.Undo != null)
                        lstKarma.ContextMenuStrip = cmsUndoKarmaExpense;
                    if (objExpense.Amount == 0)
                        continue;
                    if (objExpense.Date > KarmaLast)
                        KarmaLast = objExpense.Date;
                    decKarmaValue += objExpense.Amount;
                    chtKarma.ExpenseValues.Add(new DateTimePoint(objExpense.Date, decimal.ToDouble(decKarmaValue)));
                }

                if (KarmaLast == DateTime.MinValue)
                    KarmaLast = File.Exists(CharacterObject.FileName)
                        ? File.GetCreationTime(CharacterObject.FileName)
                        : new DateTime(DateTime.Now.Ticks - 1000);
                if (chtKarma.ExpenseValues.Count < 2)
                {
                    if (chtKarma.ExpenseValues.Count < 1)
                        chtKarma.ExpenseValues.Add(new DateTimePoint(KarmaLast, decimal.ToDouble(decKarmaValue)));
                    chtKarma.ExpenseValues.Add(new DateTimePoint(DateTime.Now, decimal.ToDouble(decKarmaValue)));
                }

                chtKarma.NormalizeYAxis();
                chtKarma.ResumeLayout();
            }
        }

        private void chkShowFreeNuyen_CheckedChanged(object sender, EventArgs e)
        {
            if (CharacterObject.ExpenseEntries.Any(x => x.Type == ExpenseType.Nuyen && x.Amount == 0))
            {
                RepopulateNuyenExpenseList();
            }
        }

        private void RepopulateNuyenExpenseList()
        {
            using (new CursorWait(this))
            {
                lstNuyen.Items.Clear();
                lstNuyen.ContextMenuStrip = null;
                chtNuyen.SuspendLayout();
                chtNuyen.ExpenseValues.Clear();
                decimal decNuyenValue = 0;
                //Find the last karma/nuyen entry as well in case a chart only contains one point
                DateTime NuyenLast = DateTime.MinValue;
                foreach (ExpenseLogEntry objExpense in CharacterObject.ExpenseEntries)
                {
                    if (objExpense.Type != ExpenseType.Nuyen || (objExpense.Amount == 0 && !chkShowFreeNuyen.Checked))
                        continue;
                    ListViewItemWithValue.ListViewSubItemWithValue objAmountItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense.Amount,
                            Text = objExpense.Amount.ToString("#,0.##", GlobalSettings.CultureInfo)
                        };
                    ListViewItemWithValue.ListViewSubItemWithValue objReasonItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense.Reason,
                            Text = objExpense.DisplayReason(GlobalSettings.Language)
                        };
                    ListViewItemWithValue.ListViewSubItemWithValue objInternalIdItem =
                        new ListViewItemWithValue.ListViewSubItemWithValue
                        {
                            Value = objExpense,
                            Text = objExpense.InternalId
                        };

                    ListViewItemWithValue objItem = new ListViewItemWithValue(objExpense.Date,
                        objExpense.Date.ToString(GlobalSettings.CustomDateTimeFormats
                                ? GlobalSettings.CustomDateFormat
                                  + ' ' + GlobalSettings.CustomTimeFormat
                                : GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern
                                  + ' ' + GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern,
                            GlobalSettings.CultureInfo)
                    );
                    objItem.SubItems.Add(objAmountItem);
                    objItem.SubItems.Add(objReasonItem);
                    objItem.SubItems.Add(objInternalIdItem);

                    lstNuyen.Items.Add(objItem);
                    if (objExpense.Undo != null)
                        lstNuyen.ContextMenuStrip = cmsUndoNuyenExpense;
                    if (objExpense.Amount == 0)
                        continue;
                    if (objExpense.Date > NuyenLast)
                        NuyenLast = objExpense.Date;
                    decNuyenValue += objExpense.Amount;
                    chtNuyen.ExpenseValues.Add(new DateTimePoint(objExpense.Date, decimal.ToDouble(decNuyenValue)));
                }

                if (NuyenLast == DateTime.MinValue)
                    NuyenLast = File.Exists(CharacterObject.FileName)
                        ? File.GetCreationTime(CharacterObject.FileName)
                        : new DateTime(DateTime.Now.Ticks - 1000);
                if (chtNuyen.ExpenseValues.Count < 2)
                {
                    if (chtNuyen.ExpenseValues.Count < 1)
                        chtNuyen.ExpenseValues.Add(new DateTimePoint(NuyenLast, decimal.ToDouble(decNuyenValue)));
                    chtNuyen.ExpenseValues.Add(new DateTimePoint(DateTime.Now, decimal.ToDouble(decNuyenValue)));
                }

                chtNuyen.NormalizeYAxis();
                chtNuyen.ResumeLayout();
            }
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
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationGroupPercent;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationOrdealPercent;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaMAGInitiationSchoolingPercent;
                intAmount = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                strInitTip = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveInitiateGrade")
                    , (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intAmount.ToString(GlobalSettings.CultureInfo));
            }
            else
            {
                if (chkInitiationGroup.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationGroupPercent;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationOrdealPercent;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= CharacterObjectSettings.KarmaRESInitiationSchoolingPercent;
                intAmount = ((CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectSettings.KarmaInitiation) * decMultiplier).StandardRound();

                strInitTip = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveSubmersionGrade")
                    , (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo)
                    , intAmount.ToString(GlobalSettings.CultureInfo));
            }

            cmdAddMetamagic.SetToolTip(strInitTip);
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
            cmdWeaponBuyAmmo.ToolTipText = LanguageManager.GetString("Tip_BuyAmmo");
            cmdWeaponMoveToVehicle.ToolTipText = LanguageManager.GetString("Tip_TransferToVehicle");
            // Gear Tab.
            cmdGearIncreaseQty.ToolTipText = LanguageManager.GetString("Tip_IncreaseGearQty");
            cmdGearReduceQty.ToolTipText = LanguageManager.GetString("Tip_DecreaseGearQty");
            cmdGearSplitQty.ToolTipText = LanguageManager.GetString("Tip_SplitGearQty");
            cmdGearMergeQty.ToolTipText = LanguageManager.GetString("Tip_MergeGearQty");
            cmdGearMoveToVehicle.ToolTipText = LanguageManager.GetString("Tip_TransferToVehicle");
            chkGearActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            chkCyberwareActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            // Vehicles Tab.
            chkVehicleWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled"));
            cmdVehicleGearReduceQty.ToolTipText = LanguageManager.GetString("Tip_DecreaseGearQty");
            cmdVehicleMoveToInventory.ToolTipText = LanguageManager.GetString("Tip_TransferToInventory");
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
            lblLiftCarryLimitsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarryLimits"));
            lblComposureLabel.SetToolTip(LanguageManager.GetString("Tip_OtherComposure"));
            lblSurpriseLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSurprise"));
            lblJudgeIntentionsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherJudgeIntentions"));
            lblLiftCarryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarry"));
            lblMemoryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMemory"));
            // Condition Monitor Tab.
            lblCMPenaltyLabel.SetToolTip(LanguageManager.GetString("Tip_CMPenalty"));
            lblCMArmorLabel.SetToolTip(LanguageManager.GetString("Tip_OtherArmor"));
            lblCMDamageResistancePoolLabel.SetToolTip(LanguageManager.GetString("Tip_CMDamageResistance"));
            cmdEdgeGained.ToolTipText = LanguageManager.GetString("Tip_CMRegainEdge");
            cmdEdgeSpent.ToolTipText = LanguageManager.GetString("Tip_CMSpendEdge");
            // Common Info Tab.
            lblStreetCred.SetToolTip(LanguageManager.GetString("Tip_StreetCred"));
            lblNotoriety.SetToolTip(LanguageManager.GetString("Tip_Notoriety"));
            if (CharacterObjectSettings.UseCalculatedPublicAwareness)
                lblPublicAware.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness"));
            cmdBurnStreetCred.ToolTipText = LanguageManager.GetString("Tip_BurnStreetCred");
        }

        /// <summary>
        /// Refresh the information for the currently selected Spell
        /// </summary>
        private void RefreshSelectedSpell()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;
            if (treSpells.SelectedNode?.Level > 0 && treSpells.SelectedNode.Tag is Spell objSpell)
            {
                gpbMagicianSpell.Visible = true;
                cmdDeleteSpell.Enabled = objSpell.Grade == 0;

                lblSpellDescriptors.Text = objSpell.DisplayDescriptors(GlobalSettings.Language);
                if (string.IsNullOrEmpty(lblSpellDescriptors.Text))
                    lblSpellDescriptors.Text = LanguageManager.GetString("String_None");
                lblSpellCategory.Text = objSpell.DisplayCategory(GlobalSettings.Language);
                lblSpellType.Text = objSpell.DisplayType(GlobalSettings.Language);
                lblSpellRange.Text = objSpell.DisplayRange(GlobalSettings.Language);
                lblSpellDamage.Text = objSpell.DisplayDamage(GlobalSettings.Language);
                lblSpellDuration.Text = objSpell.DisplayDuration(GlobalSettings.Language);
                lblSpellDV.Text = objSpell.DisplayDv(GlobalSettings.Language);
                lblSpellDV.SetToolTip(objSpell.DvTooltip);
                objSpell.SetSourceDetail(lblSpellSource);

                // Determine the size of the Spellcasting Dice Pool.
                dpcSpellDicePool.DicePool = objSpell.DicePool;
                dpcSpellDicePool.SetLabelToolTip(objSpell.DicePoolTooltip);
            }
            else
            {
                gpbMagicianSpell.Visible = false;
                cmdDeleteSpell.Enabled = treSpells.SelectedNode?.Tag is ICanRemove;
            }
            IsRefreshing = false;
        }

        /// <summary>
        /// Recheck all mods to see if Sensor has changed.
        /// </summary>
        /// <param name="objVehicle">Vehicle to modify.</param>
        private void UpdateSensor(Vehicle objVehicle)
        {
            foreach (Gear objGear in objVehicle.GearChildren)
            {
                if (objGear.Category == "Sensors" && objGear.Name == "Sensor Array" && objGear.IncludedInParent)
                {
                    // Update the name of the item in the TreeView.
                    TreeNode objNode = treVehicles.FindNode(objGear.InternalId);
                    objNode.Text = objGear.CurrentDisplayName;
                }
            }
        }

        /// <summary>
        /// Copy the Improvements from a piece of Armor on one character to another.
        /// </summary>
        /// <param name="objSource">Source character.</param>
        /// <param name="objDestination">Destination character.</param>
        /// <param name="objArmor">Armor to copy.</param>
        private static void CopyArmorImprovements(Character objSource, Character objDestination, Armor objArmor)
        {
            foreach (Improvement objImprovement in objSource.Improvements)
            {
                if (objImprovement.SourceName == objArmor.InternalId)
                {
                    objDestination.Improvements.Add(objImprovement);
                }
            }
            // Look through any Armor Mods and add the Improvements as well.
            foreach (ArmorMod objMod in objArmor.ArmorMods)
            {
                foreach (Improvement objImprovement in objSource.Improvements)
                {
                    if (objImprovement.SourceName == objMod.InternalId)
                    {
                        objDestination.Improvements.Add(objImprovement);
                    }
                }
                // Look through any children and add their Improvements as well.
                foreach (Gear objChild in objMod.GearChildren)
                    CopyGearImprovements(objSource, objDestination, objChild);
            }
            // Look through any children and add their Improvements as well.
            foreach (Gear objChild in objArmor.GearChildren)
                CopyGearImprovements(objSource, objDestination, objChild);
        }

        /// <summary>
        /// Copy the Improvements from a piece of Gear on one character to another.
        /// </summary>
        /// <param name="objSource">Source character.</param>
        /// <param name="objDestination">Destination character.</param>
        /// <param name="objGear">Gear to copy.</param>
        private static void CopyGearImprovements(Character objSource, Character objDestination, Gear objGear)
        {
            foreach (Improvement objImprovement in objSource.Improvements)
            {
                if (objImprovement.SourceName == objGear.InternalId)
                {
                    objDestination.Improvements.Add(objImprovement);
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
        private static void CopyCyberwareImprovements(Character objSource, Character objDestination, Cyberware objCyberware)
        {
            foreach (Improvement objImprovement in objSource.Improvements)
            {
                if (objImprovement.SourceName == objCyberware.InternalId)
                {
                    objDestination.Improvements.Add(objImprovement);
                }
            }
            // Look through any children and add their Improvements as well.
            foreach (Cyberware objChild in objCyberware.Children)
                CopyCyberwareImprovements(objSource, objDestination, objChild);
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
            if (IsRefreshing)
                return;

            IsRefreshing = true;
            // Locate the Program that is selected in the tree.
            if (treComplexForms.SelectedNode?.Tag is ComplexForm objComplexForm)
            {
                gpbTechnomancerComplexForm.Visible = true;
                cmdDeleteComplexForm.Enabled = objComplexForm.Grade == 0;

                lblDuration.Text = objComplexForm.DisplayDuration(GlobalSettings.Language);
                lblTarget.Text = objComplexForm.DisplayTarget(GlobalSettings.Language);
                lblFV.Text = objComplexForm.DisplayFv(GlobalSettings.Language);
                lblFV.SetToolTip(objComplexForm.FvTooltip);

                // Determine the size of the Threading Dice Pool.
                dpcComplexFormDicePool.DicePool = objComplexForm.DicePool;
                dpcComplexFormDicePool.SetLabelToolTip(objComplexForm.DicePoolTooltip);

                objComplexForm.SetSourceDetail(lblComplexFormSource);
            }
            else
            {
                gpbTechnomancerComplexForm.Visible = false;
                cmdDeleteComplexForm.Enabled = treComplexForms.SelectedNode?.Tag is ICanRemove;
            }

            IsRefreshing = false;
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
            List<Weapon> lstWeapons = new List<Weapon>(1);
            List<Vehicle> lstVehicles = new List<Vehicle>(1);
            Cyberware objCyberware = new Cyberware(CharacterObject);
            string strForced = xmlSuiteNode.SelectSingleNode("name/@select")?.InnerText ?? string.Empty;

            objCyberware.Create(xmlCyberwareNode, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, true, strForced);
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
            string strXPathPrefix = strType + "s/" + strType;
            using (XmlNodeList xmlChildrenList = xmlSuiteNode.SelectNodes(strXPathPrefix))
            {
                if (xmlChildrenList?.Count > 0)
                {
                    XmlDocument objXmlDocument = CharacterObject.LoadData(strType + ".xml");
                    foreach (XmlNode objXmlChild in xmlChildrenList)
                    {
                        string strChildName = objXmlChild["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strChildName))
                            continue;
                        XmlNode objXmlChildCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strXPathPrefix + "[name = " + strChildName.CleanXPath() + ']');
                        int intChildRating = Convert.ToInt32(objXmlChild["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                        objCyberware.Children.Add(CreateSuiteCyberware(objXmlChild, objXmlChildCyberware, objGrade, intChildRating, eSource));
                    }
                }
            }

            return objCyberware;
        }

        private async ValueTask AddCyberwareSuite(Improvement.ImprovementSource objSource)
        {
            using (SelectCyberwareSuite frmPickCyberwareSuite = new SelectCyberwareSuite(CharacterObject, objSource))
            {
                await frmPickCyberwareSuite.ShowDialogSafeAsync(this);

                if (frmPickCyberwareSuite.DialogResult == DialogResult.Cancel)
                    return;

                decimal decCost = frmPickCyberwareSuite.TotalCost;
                if (decCost > CharacterObject.Nuyen)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string strType = objSource == Improvement.ImprovementSource.Cyberware ? "cyberware" : "bioware";
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync(strType + ".xml");

                XmlNode xmlSuite = frmPickCyberwareSuite.SelectedSuite.IsGuid()
                    ? objXmlDocument.SelectSingleNode("/chummer/suites/suite[id = " + frmPickCyberwareSuite.SelectedSuite.CleanXPath() + ']')
                    : objXmlDocument.SelectSingleNode("/chummer/suites/suite[name = " + frmPickCyberwareSuite.SelectedSuite.CleanXPath() + ']');
                if (xmlSuite == null)
                    return;

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(decCost * -1, await LanguageManager.GetStringAsync("String_ExpensePurchaseCyberwareSuite") + await LanguageManager.GetStringAsync("String_Space") + xmlSuite["name"]?.InnerText, ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                CharacterObject.Nuyen -= decCost;

                Grade objGrade = Grade.ConvertToCyberwareGrade(xmlSuite["grade"]?.InnerText, objSource, CharacterObject);

                // Run through each of the items in the Suite and add them to the character.
                using (XmlNodeList xmlItemList = xmlSuite.SelectNodes(strType + "s/" + strType))
                {
                    if (xmlItemList?.Count > 0)
                    {
                        foreach (XmlNode xmlItem in xmlItemList)
                        {
                            string strItemName = xmlItem["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strItemName))
                                continue;
                            XmlNode objXmlCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strType + "s/" + strType + "[name = " + strItemName.CleanXPath() + ']');
                            int intRating = Convert.ToInt32(xmlItem["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                            Cyberware objCyberware = CreateSuiteCyberware(xmlItem, objXmlCyberware, objGrade, intRating, objSource);
                            CharacterObject.Cyberware.Add(objCyberware);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a sustained spell
        /// </summary>
        private void AddSustainedSpell()
        {
            //Could be merged with AddSustainedComplex form and create an more or less universal method to add ISustainables from tre viewers. Not worth the trouble and probably not better at the moment.
            if (treSpells.SelectedNode?.Level > 0 && treSpells.SelectedNode.Tag is Spell objSpell)
            {
                SustainedObject objSustained = new SustainedObject(CharacterObject);
                objSustained.Create(objSpell);
                CharacterObject.SustainedCollection.Add(objSustained);
            }
        }

        /// <summary>
        /// Adds a sustained complex form
        /// </summary>
        private void AddSustainedComplexForm()
        {
            if (treComplexForms.SelectedNode?.Level > 0 && treComplexForms.SelectedNode.Tag is ComplexForm objComplexForm)
            {
                SustainedObject objSustained = new SustainedObject(CharacterObject);
                objSustained.Create(objComplexForm);
                CharacterObject.SustainedCollection.Add(objSustained);
            }
        }

        /// <summary>
        /// Add a sustained critter power
        /// </summary>
        private void AddSustainedCritterPower()
        {
            if (treCritterPowers.SelectedNode?.Level > 0 && treCritterPowers.SelectedNode.Tag is CritterPower objCritterPower)
            {
                SustainedObject objSustained = new SustainedObject(CharacterObject);
                objSustained.Create(objCritterPower);
                CharacterObject.SustainedCollection.Add(objSustained);
            }
        }

        #endregion Custom Methods

        private void cmdIncreasePowerPoints_Click(object sender, EventArgs e)
        {
            // Make sure the character has enough Karma to improve the CharacterAttribute.
            int intKarmaCost = CharacterObject.Settings.KarmaMysticAdeptPowerPoint;
            if (intKarmaCost > CharacterObject.Karma)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (CharacterObject.MysticAdeptPowerPoints + 1 > CharacterObject.MAG.TotalValue)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughMagic"), LanguageManager.GetString("MessageTitle_NotEnoughMagic"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ConfirmKarmaExpenseSpend")
                , LanguageManager.GetString("String_PowerPoint")
                , intKarmaCost.ToString(GlobalSettings.CultureInfo))))
                return;

            // Create the Karma expense.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(intKarmaCost * -1, LanguageManager.GetString("String_PowerPoint"), ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Karma -= intKarmaCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddPowerPoint, string.Empty);
            objExpense.Undo = objUndo;

            ++CharacterObject.MysticAdeptPowerPoints;
        }

        private async void tsMetamagicAddMetamagic_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode?.Level != 0)
                return;

            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.

            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            // Evaluate each object
            bool blnPayWithKarma = CharacterObject.Metamagics.Any(objMetamagic => objMetamagic.Grade == objGrade.Grade) ||
                                   CharacterObject.Spells.Any(objSpell => objSpell.Grade == objGrade.Grade);

            // Additional Metamagics beyond the standard 1 per Grade cost additional Karma, so ask if the user wants to spend the additional Karma.
            if (blnPayWithKarma && CharacterObject.Karma < CharacterObjectSettings.KarmaMetamagic)
            {
                // Make sure the Karma expense would not put them over the limit.
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (CharacterObject.MAGEnabled && blnPayWithKarma)
            {
                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                    , await LanguageManager.GetStringAsync("String_Metamagic")
                    , CharacterObjectSettings.KarmaMetamagic.ToString(GlobalSettings.CultureInfo))))
                    return;
            }
            else if (blnPayWithKarma && !CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                , await LanguageManager.GetStringAsync("String_Echo")
                , CharacterObjectSettings.KarmaMetamagic.ToString(GlobalSettings.CultureInfo))))
                return;

            using (SelectMetamagic frmPickMetamagic = new SelectMetamagic(CharacterObject, objGrade))
            {
                await frmPickMetamagic.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickMetamagic.DialogResult == DialogResult.Cancel)
                    return;

                Metamagic objNewMetamagic = new Metamagic(CharacterObject);

                XmlNode objXmlMetamagic;
                Improvement.ImprovementSource objSource;
                if (CharacterObject.RESEnabled)
                {
                    objXmlMetamagic = (await CharacterObject.LoadDataAsync("echoes.xml")).SelectSingleNode("/chummer/echoes/echo[id = " + frmPickMetamagic.SelectedMetamagic.CleanXPath() + ']');
                    objSource = Improvement.ImprovementSource.Echo;
                }
                else
                {
                    objXmlMetamagic = (await CharacterObject.LoadDataAsync("metamagic.xml")).SelectSingleNode("/chummer/metamagics/metamagic[id = " + frmPickMetamagic.SelectedMetamagic.CleanXPath() + ']');
                    objSource = Improvement.ImprovementSource.Metamagic;
                }

                objNewMetamagic.Create(objXmlMetamagic, objSource);
                objNewMetamagic.Grade = objGrade.Grade;
                if (objNewMetamagic.InternalId.IsEmptyGuid())
                    return;

                CharacterObject.Metamagics.Add(objNewMetamagic);

                if (blnPayWithKarma)
                {
                    string strType = await LanguageManager.GetStringAsync(objNewMetamagic.SourceType == Improvement.ImprovementSource.Echo ? "String_Echo" : "String_Metamagic");
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(CharacterObjectSettings.KarmaMetamagic * -1, strType + await LanguageManager.GetStringAsync("String_Space") + objNewMetamagic.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddMetamagic, objNewMetamagic.InternalId);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Karma total.
                    CharacterObject.Karma -= CharacterObjectSettings.KarmaMetamagic;
                }
            }
        }

        private async void tsMetamagicAddArt_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode?.Level != 0)
                return;

            int intGrade = 0;
            if (treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade)
                intGrade = objGrade.Grade;

            /*
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;
            if (blnPayWithKarma && CharacterObject.Karma < CharacterObjectSettings.KarmaMetamagic)
            {
                // Make sure the Karma expense would not put them over the limit.
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughKarma"), LanguageManager.GetString("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            */

            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Art))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode objXmlArt = (await CharacterObject.LoadDataAsync("metamagic.xml")).SelectSingleNode("/chummer/arts/art[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');

                Art objArt = new Art(CharacterObject);
                objArt.Create(objXmlArt, Improvement.ImprovementSource.Metamagic);
                objArt.Grade = intGrade;
                if (objArt.InternalId.IsEmptyGuid())
                    return;

                CharacterObject.Arts.Add(objArt);

                /*
                if (blnPayWithKarma)
                {
                    string strType = LanguageManager.GetString("String_Art");
                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create(CharacterObjectSettings.KarmaMetamagic * -1, strType + LanguageManager.GetString("String_Space") + objArt.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(KarmaExpenseType.AddMetamagic, objArt.InternalId);
                    objExpense.Undo = objUndo;

                    // Adjust the character's Karma total.
                    CharacterObject.Karma -= CharacterObjectSettings.KarmaMetamagic;
                }
                */
            }
        }

        private async void tsMetamagicAddEnchantment_Click(object sender, EventArgs e)
        {
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.
            bool blnPayWithKarma = false;

            if (treMetamagic.SelectedNode?.Level != 0)
                return;

            int intGrade = 0;
            if (treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade)
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

            if (blnPayWithKarma)
            {
                if (CharacterObject.Karma < intSpellKarmaCost)
                {
                    // Make sure the Karma expense would not put them over the limit.
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"),
                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                    , await LanguageManager.GetStringAsync("String_Enchantment")
                    , intSpellKarmaCost.ToString(GlobalSettings.CultureInfo))))
                    return;
            }

            XmlNode objXmlArt;
            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Enchantment))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                objXmlArt = (await CharacterObject.LoadDataAsync("spells.xml")).SelectSingleNode("/chummer/spells/spell[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');
            }

            Spell objNewSpell = new Spell(CharacterObject);
            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, Improvement.ImprovementSource.Initiation);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
            {
                objNewSpell.Dispose();
                return;
            }

            CharacterObject.Spells.Add(objNewSpell);

            if (blnPayWithKarma)
            {
                string strType = await LanguageManager.GetStringAsync("String_Enhancement");
                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(-intSpellKarmaCost, strType + await LanguageManager.GetStringAsync("String_Space") + objNewSpell.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddSpell, objNewSpell.InternalId);
                objExpense.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= intSpellKarmaCost;
            }
        }

        private async void tsMetamagicAddRitual_Click(object sender, EventArgs e)
        {
            // Character can only have a number of Metamagics/Echoes equal to their Initiate Grade. Additional ones cost Karma.

            if (treMetamagic.SelectedNode?.Level != 0)
                return;

            int intGrade = 0;
            if (treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade)
                intGrade = objGrade.Grade;

            // Evaluate each object
            bool blnPayWithKarma = CharacterObject.Metamagics.Any(objMetamagic => objMetamagic.Grade == intGrade)
                || CharacterObject.Spells.Any(objSpell => objSpell.Grade == intGrade);

            int intSpellKarmaCost = CharacterObject.SpellKarmaCost("Rituals");
            if (blnPayWithKarma)
            {
                if (CharacterObject.Karma < intSpellKarmaCost)
                {
                    // Make sure the Karma expense would not put them over the limit.
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"),
                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                    , await LanguageManager.GetStringAsync("String_Ritual")
                    , intSpellKarmaCost.ToString(GlobalSettings.CultureInfo))))
                    return;
            }

            XmlNode objXmlArt;
            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Ritual))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                objXmlArt = (await CharacterObject.LoadDataAsync("spells.xml")).SelectSingleNode("/chummer/spells/spell[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');
            }

            Spell objNewSpell = new Spell(CharacterObject);
            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, Improvement.ImprovementSource.Initiation);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
            {
                objNewSpell.Dispose();
                return;
            }

            CharacterObject.Spells.Add(objNewSpell);

            if (blnPayWithKarma)
            {
                string strType = await LanguageManager.GetStringAsync("String_Ritual");
                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                objExpense.Create(-intSpellKarmaCost, strType + await LanguageManager.GetStringAsync("String_Space") + objNewSpell.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objExpense);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateKarma(KarmaExpenseType.AddSpell, objNewSpell.InternalId);
                objExpense.Undo = objUndo;

                // Adjust the character's Karma total.
                CharacterObject.Karma -= intSpellKarmaCost;
            }
        }

        private async void tsInitiationNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treMetamagic.SelectedNode);
        }

        private async void tsMetamagicAddEnhancement_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode?.Level != 0)
                return;

            int intGrade = 0;
            if (treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade)
                intGrade = objGrade.Grade;

            if (CharacterObject.Karma < CharacterObjectSettings.KarmaEnhancement)
            {
                // Make sure the Karma expense would not put them over the limit.
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                , await LanguageManager.GetStringAsync("String_Enhancement")
                , CharacterObjectSettings.KarmaEnhancement.ToString(GlobalSettings.CultureInfo))))
                return;

            XmlNode objXmlArt;
            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Enhancement))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                objXmlArt = (await CharacterObject.LoadDataAsync("powers.xml")).SelectSingleNode("/chummer/enhancements/enhancement[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');
            }

            if (objXmlArt == null)
                return;

            Enhancement objEnhancement = new Enhancement(CharacterObject);
            objEnhancement.Create(objXmlArt, Improvement.ImprovementSource.Initiation);
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

            string strType = await LanguageManager.GetStringAsync("String_Enhancement");
            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(CharacterObjectSettings.KarmaEnhancement * -1, strType + await LanguageManager.GetStringAsync("String_Space") + objEnhancement.DisplayNameShort(GlobalSettings.Language), ExpenseType.Karma, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateKarma(KarmaExpenseType.AddSpell, objEnhancement.InternalId);
            objExpense.Undo = objUndo;

            // Adjust the character's Karma total.
            CharacterObject.Karma -= CharacterObjectSettings.KarmaEnhancement;
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
            if (IsLoading || IsRefreshing || !CharacterObject.Overclocker)
                return;
            if (!(treGear.SelectedNode?.Tag is Gear objCommlink))
                return;
            objCommlink.Overclocked = cboGearOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeComboBoxes(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);
        }

        private void cboArmorOverclocker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || !CharacterObject.Overclocker)
                return;
            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.Overclocked = cboArmorOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeComboBoxes(cboArmorAttack, cboArmorSleaze, cboArmorDataProcessing, cboArmorFirewall);
        }

        private void cboWeaponOverclocker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || !CharacterObject.Overclocker)
                return;
            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.Overclocked = cboWeaponOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeComboBoxes(cboWeaponGearAttack, cboWeaponGearSleaze, cboWeaponGearDataProcessing, cboWeaponGearFirewall);
        }

        private void cboCyberwareOverclocker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || !CharacterObject.Overclocker)
                return;
            /* Que?
            List<Gear> lstGearToSearch = new List<Gear>(CharacterObject.Gear);
            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
            {
                lstGearToSearch.AddRange(objCyberware.Gear);
            }*/
            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.Overclocked = cboCyberwareOverclocker.SelectedValue.ToString();
            objCommlink.RefreshMatrixAttributeComboBoxes(cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall);
        }

        private async void cmdAddAIProgram_Click(object sender, EventArgs e)
        {
            int intNewAIProgramCost = CharacterObject.AIProgramKarmaCost;
            int intNewAIAdvancedProgramCost = CharacterObject.AIAdvancedProgramKarmaCost;
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("programs.xml");

            bool blnAddAgain;
            do
            {
                // Make sure the character has enough Karma before letting them select a Spell.
                if (CharacterObject.Karma < intNewAIProgramCost && CharacterObject.Karma < intNewAIAdvancedProgramCost)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughKarma"), await LanguageManager.GetStringAsync("MessageTitle_NotEnoughKarma"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                // Let the user select a Program.
                using (SelectAIProgram frmPickProgram = new SelectAIProgram(CharacterObject, CharacterObject.Karma >= intNewAIAdvancedProgramCost))
                {
                    await frmPickProgram.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickProgram.DialogResult == DialogResult.Cancel)
                        break;

                    blnAddAgain = frmPickProgram.AddAgain;

                    XmlNode objXmlProgram = objXmlDocument.SelectSingleNode("/chummer/programs/program[id = " + frmPickProgram.SelectedProgram.CleanXPath() + ']');
                    if (objXmlProgram == null)
                        continue;

                    // Check for SelectText.
                    string strExtra = string.Empty;
                    XmlNode xmlSelectText = objXmlProgram.SelectSingleNode("bonus/selecttext");
                    if (xmlSelectText != null)
                    {
                        using (SelectText frmPickText = new SelectText
                        {
                            Description = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Improvement_SelectText")
                                , objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"]?.InnerText ?? await LanguageManager.GetStringAsync("String_Unknown"))
                        })
                        {
                            await frmPickText.ShowDialogSafeAsync(this);
                            strExtra = frmPickText.SelectedValue;
                        }
                    }

                    AIProgram objProgram = new AIProgram(CharacterObject);
                    objProgram.Create(objXmlProgram, strExtra);
                    if (objProgram.InternalId.IsEmptyGuid())
                        continue;

                    bool boolIsAdvancedProgram = objProgram.IsAdvancedProgram;
                    if (!CommonFunctions.ConfirmKarmaExpense(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ConfirmKarmaExpenseSpend")
                        , objProgram.DisplayName
                        , (boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost).ToString(GlobalSettings.CultureInfo))))
                        continue;

                    CharacterObject.AIPrograms.Add(objProgram);

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
                    objExpense.Create((boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost) * -1, await LanguageManager.GetStringAsync("String_ExpenseLearnProgram") + await LanguageManager.GetStringAsync("String_Space") + objProgram.Name,
                        ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objExpense);
                    CharacterObject.Karma -= boolIsAdvancedProgram ? intNewAIAdvancedProgramCost : intNewAIProgramCost;

                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateKarma(boolIsAdvancedProgram ? KarmaExpenseType.AddAIAdvancedProgram : KarmaExpenseType.AddAIProgram, objProgram.InternalId);
                    objExpense.Undo = objUndo;
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteAIProgram_Click(object sender, EventArgs e)
        {
            // Delete the selected AI Program.
            if (!(treAIPrograms.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            selectedObject.Remove();
        }

        private void treAIPrograms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the Program that is selected in the tree.
            if (treAIPrograms.SelectedNode?.Tag is AIProgram objProgram)
            {
                lblAIProgramsRequires.Text = objProgram.DisplayRequiresProgram(GlobalSettings.Language);
                objProgram.SetSourceDetail(lblAIProgramsSource);
            }
            else
            {
                lblAIProgramsRequires.Text = string.Empty;
                lblAIProgramsSource.Text = string.Empty;
                lblAIProgramsSource.SetToolTip(string.Empty);
            }
        }

        private void treAIPrograms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteAIProgram_Click(sender, e);
            }
        }

        private async void tsAIProgramNotes_Click(object sender, EventArgs e)
        {
            if (!(treAIPrograms.SelectedNode?.Tag is IHasNotes selectedObject))
                return;
            await WriteNotes(selectedObject, treAIPrograms.SelectedNode);
        }

        private void cboPrimaryArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || CharacterObject.Ambidextrous)
                return;
            CharacterObject.PrimaryArm = cboPrimaryArm.SelectedValue.ToString();

            IsDirty = true;
        }

        private void picMugshot_SizeChanged(object sender, EventArgs e)
        {
            if (this.IsNullOrDisposed() || picMugshot.IsNullOrDisposed())
                return;
            try
            {
                picMugshot.SizeMode = picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width
                    ? PictureBoxSizeMode.CenterImage
                    : PictureBoxSizeMode.Zoom;
            }
            catch (ArgumentException) // No other way to catch when the Image is not null, but is disposed
            {
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private async void cmdCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objModularCyberware))
                return;
            string strSelectedParentID;
            using (new FetchSafelyFromPool<List<ListItem>>(
                       Utils.ListItemListPool, out List<ListItem> lstModularMounts))
            {
                lstModularMounts.AddRange(CharacterObject.ConstructModularCyberlimbList(objModularCyberware));
                //Mounted cyberware should always be allowed to be dismounted.
                //Unmounted cyberware requires that a valid mount be present.
                if (!objModularCyberware.IsModularCurrentlyEquipped
                    && lstModularMounts.All(
                        x => !string.Equals(x.Value.ToString(), "None", StringComparison.OrdinalIgnoreCase)))
                {
                    Program.MainForm.ShowMessageBox(this,
                                                    await LanguageManager.GetStringAsync("Message_NoValidModularMount"),
                                                    await LanguageManager.GetStringAsync("MessageTitle_NoValidModularMount"),
                                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SelectItem frmPickMount = new SelectItem
                       {
                           Description = await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware")
                       })
                {
                    frmPickMount.SetGeneralItemsMode(lstModularMounts);
                    await frmPickMount.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickMount.DialogResult == DialogResult.Cancel)
                    {
                        return;
                    }

                    strSelectedParentID = frmPickMount.SelectedItem;
                }
            }

            Cyberware objOldParent = objModularCyberware.Parent;
            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);
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
                    else if (objOldParent != null)
                    {
                        objOldParent.Children.Remove(objModularCyberware);

                        CharacterObject.Cyberware.Add(objModularCyberware);
                    }
                }
            }
        }

        private async void cmdVehicleCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objModularCyberware))
                return;
            string strSelectedParentID;
            using (new FetchSafelyFromPool<List<ListItem>>(
                       Utils.ListItemListPool, out List<ListItem> lstModularMounts))
            {
                lstModularMounts.AddRange(CharacterObject.ConstructModularCyberlimbList(objModularCyberware));
                //Mounted cyberware should always be allowed to be dismounted.
                //Unmounted cyberware requires that a valid mount be present.
                if (!objModularCyberware.IsModularCurrentlyEquipped
                    && lstModularMounts.All(
                        x => !string.Equals(x.Value.ToString(), "None", StringComparison.OrdinalIgnoreCase)))
                {
                    Program.MainForm.ShowMessageBox(this,
                                                    await LanguageManager.GetStringAsync("Message_NoValidModularMount"),
                                                    await LanguageManager.GetStringAsync("MessageTitle_NoValidModularMount"),
                                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                using (SelectItem frmPickMount = new SelectItem
                       {
                           Description = await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware")
                       })
                {
                    frmPickMount.SetGeneralItemsMode(lstModularMounts);
                    await frmPickMount.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickMount.DialogResult == DialogResult.Cancel)
                    {
                        return;
                    }

                    strSelectedParentID = frmPickMount.SelectedItem;
                }
            }
            CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId, out VehicleMod objOldParentVehicleMod);

            Cyberware objOldParent = objModularCyberware.Parent;
            if (objOldParent != null)
                objModularCyberware.ChangeModularEquip(false);
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
        }

        private void cboAttributeCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                CharacterObject.AttributeSection.AttributeCategory
                    = AttributeSection.ConvertAttributeCategory(cboAttributeCategory.SelectedValue.ToString());
            }
        }

        private void cmdContactsExpansionToggle_Click(object sender, EventArgs e)
        {
            if (panContacts.Controls.Count > 0)
            {
                panContacts.SuspendLayout();
                bool toggle = ((ContactControl)panContacts.Controls[0]).Expanded;

                foreach (ContactControl c in panContacts.Controls)
                {
                    c.Expanded = !toggle;
                }

                panContacts.ResumeLayout();
            }
        }

        private void cmdSwapContactOrder_Click(object sender, EventArgs e)
        {
            panContacts.FlowDirection = panContacts.FlowDirection == FlowDirection.LeftToRight
                ? FlowDirection.TopDown
                : FlowDirection.LeftToRight;
        }

        private async void tsGearLocationAddGear_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is Location objLocation))
                return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickGear(null, objLocation);
            }
            while (blnAddAgain);
        }

        private async void tsVehicleLocationAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddVehicle(treVehicles.SelectedNode?.Tag as Location);
            }
            while (blnAddAgain);
        }

        private async void tsWeaponLocationAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickWeapon(treWeapons.SelectedNode?.Tag as Location);
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
            if (IsRefreshing)
                return;

            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
                return;
            objWeapon.FireMode = cboVehicleWeaponFiringMode.SelectedIndex >= 0
                ? (Weapon.FiringMode)cboVehicleWeaponFiringMode.SelectedValue
                : Weapon.FiringMode.DogBrain;
            RefreshSelectedVehicle();

            IsDirty = true;
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender, e);
        }

        private async void btnCreateCustomDrug_Click(object sender, EventArgs e)
        {
            using (CreateCustomDrug form = new CreateCustomDrug(CharacterObject))
            {
                await form.ShowDialogSafeAsync(this);

                if (form.DialogResult == DialogResult.Cancel)
                    return;

                Drug objCustomDrug = form.CustomDrug;
                objCustomDrug.Quantity = 0;
                CharacterObject.Drugs.Add(objCustomDrug);
                objCustomDrug.GenerateImprovement();
            }
        }

        private async void btnIncreaseDrugQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCustomDrugs.SelectedNode;
            if (!(objSelectedNode?.Tag is Drug selectedDrug)) return;

            decimal decCost = selectedDrug.Cost;
            /* Apply a markup if applicable.
            if (frmPickArmor.Markup != 0)
            {
                decCost *= 1 + (frmPickArmor.Markup / 100.0m);
            }*/

            // Multiply the cost if applicable.
            char chrAvail = selectedDrug.TotalAvailTuple().Suffix;
            switch (chrAvail)
            {
                case 'R' when CharacterObjectSettings.MultiplyRestrictedCost:
                    decCost *= CharacterObjectSettings.RestrictedCostMultiplier;
                    break;

                case 'F' when CharacterObjectSettings.MultiplyForbiddenCost:
                    decCost *= CharacterObjectSettings.ForbiddenCostMultiplier;
                    break;
            }

            // Check the item's Cost and make sure the character can afford it.
            if (decCost > CharacterObject.Nuyen)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NotEnoughNuyen"),
                    await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (!CharacterObject.Improvements.Any(imp =>
                imp.ImproveSource == Improvement.ImprovementSource.Drug && imp.SourceName == selectedDrug.InternalId))
            {
                selectedDrug.GenerateImprovement();
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            objExpense.Create(decCost * -1,
                await LanguageManager.GetStringAsync("String_ExpensePurchaseDrug") +
                await LanguageManager.GetStringAsync("String_Space") +
                selectedDrug.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen -= decCost;
            selectedDrug.Quantity++;
            objSelectedNode.Text = selectedDrug.CurrentDisplayName;
            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.AddGear, selectedDrug.InternalId);
            objExpense.Undo = objUndo;
        }

        private async void btnDecreaseDrugQty_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCustomDrugs.SelectedNode;
            if (!(objSelectedNode?.Tag is Drug objDrug))
                return;

            using (SelectNumber frmPickNumber = new SelectNumber
            {
                Minimum = 0,
                Maximum = objDrug.Quantity,
                Description = await LanguageManager.GetStringAsync("String_ReduceGear")
            })
            {
                await frmPickNumber.ShowDialogSafeAsync(this);

                if (frmPickNumber.DialogResult == DialogResult.Cancel)
                    return;

                decimal decSelectedValue = frmPickNumber.SelectedValue;

                if (!CommonFunctions.ConfirmDelete(string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ReduceQty"), decSelectedValue.ToString(GlobalSettings.CultureInfo))))
                    return;

                objDrug.Quantity -= decSelectedValue;
                objSelectedNode.Text = objDrug.CurrentDisplayName;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        #region Wireless Toggles

        private void chkGearWireless_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treGear.SelectedNode.Tag is IHasWirelessBonus obj)
            {
                obj.WirelessOn = chkGearWireless.Checked;
            }
        }

        private void chkCyberwareWireless_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treCyberware.SelectedNode?.Tag is IHasWirelessBonus obj)
            {
                obj.WirelessOn = chkCyberwareWireless.Checked;
            }
        }

        private void chkWeaponWireless_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treWeapons.SelectedNode?.Tag is IHasWirelessBonus obj)
            {
                obj.WirelessOn = chkWeaponWireless.Checked;
            }
        }

        private void chkArmorWireless_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treArmor.SelectedNode?.Tag is IHasWirelessBonus obj)
            {
                obj.WirelessOn = chkArmorWireless.Checked;
            }
        }

        #endregion Wireless Toggles

        private void pnlAttributes_Layout(object sender, LayoutEventArgs e)
        {
            pnlAttributes.SuspendLayout();
            foreach (Control objAttributeControl in pnlAttributes.Controls)
            {
                if (pnlAttributes.ClientSize.Width < objAttributeControl.MinimumSize.Height)
                    objAttributeControl.MinimumSize = new Size(pnlAttributes.ClientSize.Width, objAttributeControl.MinimumSize.Height);
                if (pnlAttributes.ClientSize.Width != objAttributeControl.MaximumSize.Height)
                    objAttributeControl.MaximumSize = new Size(pnlAttributes.ClientSize.Width, objAttributeControl.MaximumSize.Height);
                if (pnlAttributes.ClientSize.Width > objAttributeControl.MinimumSize.Height)
                    objAttributeControl.MinimumSize = new Size(pnlAttributes.ClientSize.Width, objAttributeControl.MinimumSize.Height);
            }
            pnlAttributes.ResumeLayout();
        }

        private async void tsCyberwareUpgrade_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
            {
                if (objCyberware.Capacity == "[*]" && treCyberware.SelectedNode.Level == 2)
                {
                    Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotRemoveCyberware"), await LanguageManager.GetStringAsync("MessageTitle_CannotRemoveCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SellItem frmSell = new SellItem())
                {
                    await frmSell.ShowDialogSafeAsync(this);

                    if (frmSell.DialogResult == DialogResult.Cancel)
                        return;

                    using (SelectCyberware pickCyber = new SelectCyberware(CharacterObject, objCyberware.SourceType)
                    {
                        DefaultSearchText = objCyberware.DisplayNameShort(GlobalSettings.Language),
                        Upgrading = true
                    })
                    {
                        await pickCyber.ShowDialogSafeAsync(this);

                        if (pickCyber.DialogResult == DialogResult.Cancel)
                            return;

                        objCyberware.Upgrade(pickCyber.SelectedGrade, pickCyber.SelectedRating, frmSell.SellPercent, pickCyber.FreeCost);
                    }
                }

                //TODO: Bind displayname to selectednode text properly.
                if (treCyberware.SelectedNode.Tag != objCyberware)
                {
                    treCyberware.FindNodeByTag(objCyberware).Text = objCyberware.CurrentDisplayName;
                }
                else
                {
                    treCyberware.SelectedNode.Text = objCyberware.CurrentDisplayName;
                }
            }
            else
            {
                Utils.BreakIfDebug();
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        // Data binding doesn't work for some reason, so handle visibility toggles through events

        private void chkShowKarmaChart_CheckedChanged(object sender, EventArgs e)
        {
            chtKarma.Visible = chkShowKarmaChart.Checked;
        }

        private void chkShowNuyenChart_CheckedChanged(object sender, EventArgs e)
        {
            chtNuyen.Visible = chkShowNuyenChart.Checked;
        }

        private async void mnuSpecialChangeOptions_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                using (SelectBuildMethod frmPickBP = new SelectBuildMethod(CharacterObject, true))
                {
                    await frmPickBP.ShowDialogSafeAsync(this);
                }
            }
        }
    }
}
