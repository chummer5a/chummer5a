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
using System.Globalization;
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
using Microsoft.ApplicationInsights;
using NLog;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class CharacterCreate : CharacterShared
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        // Set the default culture to en-US so we work with decimals correctly.
        private bool _blnSkipUpdate;

        private bool _blnReapplyImprovements;
        private bool _blnFreestyle;
        private bool _blnIsReopenQueued;
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnDraggingGear;
        private StoryBuilder _objStoryBuilder;

        public TreeView FociTree => treFoci;
        //private readonly Stopwatch PowerPropertyChanged_StopWatch = Stopwatch.StartNew();
        //private readonly Stopwatch SkillPropertyChanged_StopWatch = Stopwatch.StartNew();

        public TabControl TabCharacterTabs => tabCharacterTabs;

        #region Form Events

        [Obsolete("This constructor is for use by form designers only.", true)]
        public CharacterCreate()
        {
            InitializeComponent();
        }

        public CharacterCreate(Character objCharacter) : base(objCharacter)
        {
            InitializeComponent();

            GlobalSettings.ClipboardChanged += RefreshPasteStatus;
            tabStreetGearTabs.MouseWheel += ShiftTabsOnMouseScroll;
            tabPeople.MouseWheel += ShiftTabsOnMouseScroll;
            tabInfo.MouseWheel += ShiftTabsOnMouseScroll;
            tabCharacterTabs.MouseWheel += ShiftTabsOnMouseScroll;

            Program.MainForm.OpenCharacterForms.Add(this);

            // Add EventHandlers for the various events MAG, RES, Qualities, etc.
            CharacterObject.PropertyChanged += OnCharacterPropertyChanged;
            CharacterObjectSettings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            tabSkillsUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            lmtControl.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            lmtControl.MakeDirty += MakeDirty;

            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            ContextMenuStrip[] lstCMSToTranslate = {
                cmsAdvancedLifestyle,
                cmsAdvancedProgram,
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
                cmsGear,
                cmsGearAllowRename,
                cmsGearButton,
                cmsGearLocation,
                cmsGearPlugin,
                cmsInitiationNotes,
                cmsLifestyle,
                cmsLifestyleNotes,
                cmsMartialArts,
                cmsMetamagic,
                cmsQuality,
                cmsSpell,
                cmsSpellButton,
                cmsTechnique,
                cmsVehicle,
                cmsVehicleGear,
                cmsVehicleLocation,
                cmsVehicleWeapon,
                cmsVehicleWeaponAccessory,
                cmsVehicleWeaponAccessoryGear,
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
                if (objCMS != null)
                {
                    foreach (ToolStripMenuItem tssItem in objCMS.Items.OfType<ToolStripMenuItem>())
                    {
                        tssItem.UpdateLightDarkMode();
                        tssItem.TranslateToolStripItemsRecursively();
                    }
                }
            }

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
            if (ModifierKeys != Keys.Control)
                return;
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

        private async void CharacterCreate_Load(object sender, EventArgs e)
        {
            using (CustomActivity op_load_frm_create = Timekeeper.StartSyncron("load_frm_create", null, CustomActivity.OperationType.RequestOperation, CharacterObject?.FileName))
            {
                if (CharacterObject == null)
                {
                    // Stupid hack to get the MDI icon to show up properly.
                    Icon = Icon.Clone() as Icon;
                    return;
                }
                try
                {
                    if (!CharacterObject.IsCritter
                        && !CharacterObject.EffectiveBuildMethodIsLifeModule
                        && CharacterObjectSettings.BuildKarma == 0)
                    {
                        _blnFreestyle = true;
                        tslKarmaRemaining.Visible = false;
                        tslKarmaRemainingLabel.Visible = false;
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_BuildMethod", op_load_frm_create))
                    {
                        // Initialize elements if we're using Priority to build.
                        if (CharacterObject.EffectiveBuildMethodUsesPriorityTables)
                        {
                            mnuSpecialChangeMetatype.Tag = "Menu_SpecialChangePriorities";
                            mnuSpecialChangeMetatype.Text = await LanguageManager.GetStringAsync("Menu_SpecialChangePriorities");
                        }
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_databinding", op_load_frm_create))
                    {
                        lblNuyenTotal.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplayTotalStartingNuyen));
                        lblStolenNuyen.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayStolenNuyen));
                        lblAttributesBase.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.EffectiveBuildMethodUsesPriorityTables));

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
                        txtAlias.DoDataBinding("Text", CharacterObject, nameof(Character.Alias));
                        txtPlayerName.DoDataBinding("Text", CharacterObject, nameof(Character.PlayerName));

                        lblPositiveQualitiesBP.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayPositiveQualityKarma));
                        lblNegativeQualitiesBP.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayNegativeQualityKarma));
                        lblMetagenicQualities.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayMetagenicQualityKarma));
                        lblMetagenicQualities.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.IsChangeling));
                        lblMetagenicQualitiesLabel.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.IsChangeling));
                        lblEnemiesBP.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayEnemyKarma));
                        tslKarmaLabel.Text = await LanguageManager.GetStringAsync("Label_Karma");
                        tslKarmaRemainingLabel.Text = await LanguageManager.GetStringAsync("Label_KarmaRemaining");
                        tabBPSummary.Text = await LanguageManager.GetStringAsync("Tab_BPSummary_Karma");
                        lblQualityBPLabel.Text = await LanguageManager.GetStringAsync("Label_Karma");

                        lblMetatype.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.FormattedMetatype));

                        // Set the visibility of the Bioware Suites menu options.
                        mnuSpecialAddBiowareSuite.Visible = CharacterObjectSettings.AllowBiowareSuites;
                        mnuSpecialCreateBiowareSuite.Visible = CharacterObjectSettings.AllowBiowareSuites;

                        chkJoinGroup.DoDataBinding("Checked", CharacterObject, nameof(Character.GroupMember));
                        chkInitiationGroup.DoOneWayDataBinding("Enabled", CharacterObject, nameof(Character.GroupMember));

                        chkCyberwareBlackMarketDiscount.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.BlackMarketDiscount));
                        chkGearBlackMarketDiscount.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.BlackMarketDiscount));
                        chkWeaponBlackMarketDiscount.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.BlackMarketDiscount));
                        chkArmorBlackMarketDiscount.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.BlackMarketDiscount));
                        chkVehicleBlackMarketDiscount.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.BlackMarketDiscount));

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

                        lblNumMugshots.Text = await LanguageManager.GetStringAsync("String_Of") +
                                              CharacterObject.Mugshots.Count.ToString(GlobalSettings.CultureInfo);
                    }

                    if (!CharacterObjectSettings.BookEnabled("RF"))
                    {
                        cmdAddLifestyle.SplitMenuStrip = null;
                    }
                    if (!CharacterObjectSettings.BookEnabled("FA"))
                    {
                        lblWildReputation.Visible = false;
                        lblWildReputationTotal.Visible = false;
                        if (!CharacterObjectSettings.BookEnabled("SG"))
                        {
                            lblAstralReputation.Visible = false;
                            lblAstralReputationTotal.Visible = false;
                        }
                    }
                    if (!CharacterObjectSettings.EnableEnemyTracking)
                    {
                        tabPeople.TabPages.Remove(tabEnemies);
                    }

                    splitMagician.SplitterDistance = Math.Max(splitMagician.SplitterDistance, ((splitMagician.Height - splitMagician.SplitterWidth) * 2 + 2) / 3);
                    splitTechnomancer.SplitterDistance = Math.Max(splitTechnomancer.SplitterDistance, ((splitTechnomancer.Height - splitTechnomancer.SplitterWidth) * 2 + 2) / 3);

                    using (_ = Timekeeper.StartSyncron("load_frm_create_refresh", op_load_frm_create))
                    {
                        cmdAddMetamagic.DoOneWayDataBinding("Enabled", CharacterObject, nameof(Character.AddInitiationsAllowed));

                        cmdLifeModule.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.EffectiveBuildMethodIsLifeModule));
                        btnCreateBackstory.DoOneWayDataBinding("Visible", CharacterObject, nameof(Character.EnableAutomaticStoryButton));

                        if (!CharacterObjectSettings.BookEnabled("RF"))
                        {
                            cmdAddLifestyle.SplitMenuStrip = null;
                        }

                        if (!CharacterObjectSettings.BookEnabled("FA"))
                        {
                            lblWildReputation.Visible = false;
                            lblWildReputationTotal.Visible = false;
                            if (!CharacterObjectSettings.BookEnabled("SG"))
                            {
                                lblAstralReputation.Visible = false;
                                lblAstralReputationTotal.Visible = false;
                            }
                        }

                        if (!CharacterObjectSettings.EnableEnemyTracking)
                        {
                            tabPeople.TabPages.Remove(tabEnemies);
                            lblEnemiesBP.Visible = false;
                            lblBuildEnemies.Visible = false;
                        }

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
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_sortAndCallback", op_load_frm_create))
                    {
                        treWeapons.SortCustomOrder();
                        treArmor.SortCustomOrder();
                        treGear.SortCustomOrder();
                        treLifestyles.SortCustomOrder();
                        treCustomDrugs.SortCustomOrder();
                        treCyberware.SortCustomOrder();
                        treVehicles.SortCustomOrder();
                        treCritterPowers.SortCustomOrder();

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
                        CharacterObject.Contacts.CollectionChanged += ContactCollectionChanged;
                        CharacterObject.Spirits.CollectionChanged += SpiritCollectionChanged;
                        CharacterObject.Armor.CollectionChanged += ArmorCollectionChanged;
                        CharacterObject.ArmorLocations.CollectionChanged += ArmorLocationCollectionChanged;
                        CharacterObject.Weapons.CollectionChanged += WeaponCollectionChanged;
                        CharacterObject.WeaponLocations.CollectionChanged += WeaponLocationCollectionChanged;
                        CharacterObject.Gear.CollectionChanged += GearCollectionChanged;
                        CharacterObject.GearLocations.CollectionChanged += GearLocationCollectionChanged;
                        CharacterObject.Drugs.CollectionChanged += DrugCollectionChanged;
                        CharacterObject.Cyberware.CollectionChanged += CyberwareCollectionChanged;
                        CharacterObject.Vehicles.CollectionChanged += VehicleCollectionChanged;
                        CharacterObject.VehicleLocations.CollectionChanged += VehicleLocationCollectionChanged;

                        SetupCommonCollectionDatabindings(true);
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_tradition", op_load_frm_create))
                    {
                        // Populate the Magician Traditions list.
                        XPathNavigator xmlTraditionsBaseChummerNode =
                            (await CharacterObject.LoadDataXPathAsync("traditions.xml")).SelectSingleNodeAndCacheExpression("/chummer");
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstTraditions))
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
                                                     new ListItem("None", await LanguageManager.GetStringAsync("String_None")));
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
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstDrainAttributes))
                        {
                            lstDrainAttributes.Add(ListItem.Blank);
                            if (xmlTraditionsBaseChummerNode != null)
                            {
                                foreach (XPathNavigator xmlDrain in xmlTraditionsBaseChummerNode.Select(
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
                        lblDrainAttributesValue.DoOneWayDataBinding("Text", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValue));
                        lblDrainAttributesValue.DoOneWayDataBinding("ToolTipText", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValueToolTip));
                        CharacterObject.MagicTradition.SetSourceDetail(lblTraditionSource);

                        lblFadingAttributes.DoOneWayDataBinding("Text", CharacterObject.MagicTradition,
                            nameof(Tradition.DisplayDrainExpression));
                        lblFadingAttributesValue.DoOneWayDataBinding("Text", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValue));
                        lblFadingAttributesValue.DoOneWayDataBinding("ToolTipText", CharacterObject.MagicTradition,
                            nameof(Tradition.DrainValueToolTip));

                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> limit))
                        {
                            foreach (Improvement improvement in ImprovementManager.GetCachedImprovementListForValueOf(
                                         CharacterObject, Improvement.ImprovementType.LimitSpiritCategory))
                            {
                                limit.Add(improvement.ImprovedName);
                            }

                            /* Populate drugs. //TODO: fix
                            foreach (Drug objDrug in CharacterObj.Drugs)
                            {
                                treCustomDrugs.Add(objDrug);
                            }
                            */

                            // Populate the Magician Custom Spirits lists - Combat.
                            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                           out List<ListItem> lstSpirit))
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
                            (await CharacterObject.LoadDataXPathAsync("streams.xml")).SelectSingleNodeAndCacheExpression("/chummer");
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstStreams))
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
                                                  new ListItem("None", await LanguageManager.GetStringAsync("String_None")));
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

                        nudMysticAdeptMAGMagician.DoOneWayDataBinding("Maximum", CharacterObject.MAG,
                                                                      nameof(CharacterAttrib.TotalValue));
                        nudMysticAdeptMAGMagician.DoDataBinding("Value", CharacterObject,
                            nameof(Character.MysticAdeptPowerPoints));

                        // Select the Magician's Tradition.
                        if (CharacterObject.MagicTradition.Type == TraditionType.MAG)
                            cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID.ToString();
                        else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                            cboTradition.SelectedIndex = 0;

                        txtTraditionName.DoDataBinding("Text", CharacterObject.MagicTradition, nameof(Tradition.Name));

                        // Select the Technomancer's Stream.
                        if (CharacterObject.MagicTradition.Type == TraditionType.RES)
                            cboStream.SelectedValue = CharacterObject.MagicTradition.SourceID.ToString();
                        else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                            cboStream.SelectedIndex = 0;
                    }

                    using (CustomActivity op_load_frm_create_longloads = Timekeeper.StartSyncron("load_frm_create_longloads", op_load_frm_create))
                    {
                        using (_ = Timekeeper.StartSyncron("load_frm_create_tabSkillsUc.RealLoad()",
                                   op_load_frm_create_longloads))
                        {
                            tabSkillsUc.RealLoad();
                        }

                        using (_ = Timekeeper.StartSyncron("load_frm_create_tabPowerUc.RealLoad()",
                                   op_load_frm_create_longloads))
                        {
                            tabPowerUc.RealLoad();
                        }

                        using (_ = Timekeeper.StartSyncron(
                                   "load_frm_create_Run through all appropriate property changers",
                                   op_load_frm_create_longloads))
                        {
                            // Run through all appropriate property changers
                            foreach (PropertyInfo objProperty in typeof(Character).GetProperties())
                                await DoOnCharacterPropertyChanged(new PropertyChangedEventArgs(objProperty.Name));
                        }
                    }

                    IsLoading = false;

                    using (_ = Timekeeper.StartSyncron("load_frm_create_databinding2", op_load_frm_create))
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

                        // Merge the ToolStrips.
                        ToolStripManager.RevertMerge("toolStrip");
                        ToolStripManager.Merge(tsMain, "toolStrip");

                        nudNuyen.DoDataBinding("Value", CharacterObject, nameof(Character.NuyenBP));
                        nudNuyen.DoOneWayDataBinding("Maximum", CharacterObject, nameof(Character.TotalNuyenMaximumBP));

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

                        lblPrototypeTranshumanESS.DoOneWayDataBinding("Text", CharacterObject,
                            nameof(Character.DisplayPrototypeTranshumanEssenceUsed));

                        lblArmor.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.TotalArmorRating));
                        lblArmor.DoOneWayDataBinding("ToolTipText", CharacterObject,
                            nameof(Character.TotalArmorRatingToolTip));

                        lblDodge.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.DisplayDodge));
                        lblDodge.DoOneWayDataBinding("ToolTipText", CharacterObject,
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

                        lblComposure.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.ComposureToolTip));
                        lblComposure.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Composure));
                        lblSurprise.DoOneWayDataBinding("ToolTipText", CharacterObject, nameof(Character.SurpriseToolTip));
                        lblSurprise.DoOneWayDataBinding("Text", CharacterObject, nameof(Character.Surprise));
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
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_vehicle", op_load_frm_create))
                    {
                        // Populate vehicle weapon fire mode list.
                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstFireModes))
                        {
                            foreach (Weapon.FiringMode mode in Enum.GetValues(typeof(Weapon.FiringMode)))
                            {
                                if (mode == Weapon.FiringMode.NumFiringModes)
                                    continue;
                                lstFireModes.Add(new ListItem(mode,
                                                              await LanguageManager.GetStringAsync("Enum_" + mode)));
                            }

                            cboVehicleWeaponFiringMode.BeginUpdate();
                            cboVehicleWeaponFiringMode.PopulateWithListItems(lstFireModes);
                            cboVehicleWeaponFiringMode.EndUpdate();
                        }
                    }

                    using (_ = Timekeeper.StartSyncron("load_frm_create_finish", op_load_frm_create))
                    {
                        RefreshAttributes(pnlAttributes, null, lblAttributes, lblKarma.PreferredWidth, lblAttributesAug.PreferredWidth, lblAttributesMetatype.PreferredWidth);

                        CharacterObject.AttributeSection.Attributes.CollectionChanged += AttributeCollectionChanged;

                        IsCharacterUpdateRequested = true;
                        // Directly calling here so that we can properly unset the dirty flag after the update
                        await DoUpdateCharacterInfoAsync();

                        // Now we can start checking for character updates
                        Application.Idle += UpdateCharacterInfo;
                        Application.Idle += LiveUpdateFromCharacterFile;

                        // Clear the Dirty flag which gets set when creating a new Character.
                        IsDirty = false;
                        RefreshPasteStatus(sender, e);
                        picMugshot_SizeChanged(sender, e);

                        // Stupid hack to get the MDI icon to show up properly.
                        Icon = Icon.Clone() as Icon;

                        Program.PluginLoader.CallPlugins(this, op_load_frm_create);
                    }

                    if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0
                        && !Utils.IsUnitTest
                        && Program.ShowMessageBox(this,
                            await LanguageManager.GetStringAsync("Message_ImprovementLoadError"),
                            await LanguageManager.GetStringAsync("MessageTitle_ImprovementLoadError"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        await DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                        CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
                    }

                    IsDirty = CharacterObject.LoadAsDirty;

                    op_load_frm_create.SetSuccess(true);
                }
                catch (Exception ex)
                {
                    if (op_load_frm_create != null)
                    {
                        op_load_frm_create.SetSuccess(false);
                        TelemetryClient.TrackException(ex);
                    }

                    Log.Error(ex);
                    throw;
                }
            }
        }

        private async void CharacterCreate_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (new CursorWait(this))
            {
                bool blnOldIsLoading = IsLoading;
                IsLoading = true;
                try
                {
                    // If there are unsaved changes to the character, as the user if they would like to save their changes.
                    if (IsDirty && !Utils.IsUnitTest)
                    {
                        string strCharacterName = CharacterObject.CharacterName;
                        DialogResult objResult = Program.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo,
                                          await LanguageManager.GetStringAsync("Message_UnsavedChanges"),
                                          strCharacterName),
                            await LanguageManager.GetStringAsync("MessageTitle_UnsavedChanges"),
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
                    Application.Idle -= UpdateCharacterInfo;
                    Application.Idle -= LiveUpdateFromCharacterFile;
                    if (Program.MainForm.ActiveMdiChild == this)
                        ToolStripManager.RevertMerge("toolStrip");
                    Program.MainForm.OpenCharacterForms.Remove(this);

                    // Unsubscribe from events.
                    GlobalSettings.ClipboardChanged -= RefreshPasteStatus;
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
                    CharacterObject.Lifestyles.CollectionChanged -= LifestyleCollectionChanged;
                    CharacterObject.Contacts.CollectionChanged -= ContactCollectionChanged;
                    CharacterObject.Spirits.CollectionChanged -= SpiritCollectionChanged;
                    CharacterObject.Armor.CollectionChanged -= ArmorCollectionChanged;
                    CharacterObject.ArmorLocations.CollectionChanged -= ArmorLocationCollectionChanged;
                    CharacterObject.Weapons.CollectionChanged -= WeaponCollectionChanged;
                    CharacterObject.Drugs.CollectionChanged -= DrugCollectionChanged;
                    CharacterObject.WeaponLocations.CollectionChanged -= WeaponLocationCollectionChanged;
                    CharacterObject.Gear.CollectionChanged -= GearCollectionChanged;
                    CharacterObject.GearLocations.CollectionChanged -= GearLocationCollectionChanged;
                    CharacterObject.Cyberware.CollectionChanged -= CyberwareCollectionChanged;
                    CharacterObject.Vehicles.CollectionChanged -= VehicleCollectionChanged;
                    CharacterObject.VehicleLocations.CollectionChanged -= VehicleLocationCollectionChanged;
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

                    foreach (PetControl objPetControl in panPets.Controls.OfType<PetControl>())
                    {
                        objPetControl.DeleteContact -= DeletePet;
                        objPetControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
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
                    if (!_blnIsReopenQueued
                        && Program.OpenCharacters.All(
                            x => x == CharacterObject || !x.LinkedCharacters.Contains(CharacterObject)))
                        Program.OpenCharacters.Remove(CharacterObject);
                }
                finally
                {
                    if (IsLoading)
                        IsLoading = blnOldIsLoading;
                }
            }
        }

        private void CharacterCreate_Activated(object sender, EventArgs e)
        {
            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(tsMain, "toolStrip");
        }

        private async void ReopenCharacter(object sender, FormClosedEventArgs e)
        {
            await Program.OpenCharacter(CharacterObject);
            FormClosed -= ReopenCharacter;
        }

        #endregion Form Events

        #region Character Events

        private async void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await DoOnCharacterPropertyChanged(e);
        }

        private async ValueTask DoOnCharacterPropertyChanged(PropertyChangedEventArgs e)
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
                    tslNuyenRemaining.Text = CharacterObject.DisplayNuyen;
                    break;

                case nameof(Character.StolenNuyen):
                    bool show = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Nuyen, strImprovedName: "Stolen") != 0;

                    lblStolenNuyen.Visible = show;
                    lblStolenNuyenLabel.Visible = show;
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
                case nameof(Character.GroupMember):
                case nameof(Character.HomeNode):
                case nameof(Character.ActiveCommlink):
                    IsCharacterUpdateRequested = true;
                    break;

                case nameof(Character.Source):
                case nameof(Character.Page):
                    CharacterObject.SetSourceDetail(lblMetatypeSource);
                    break;

                case nameof(Character.MAGEnabled):
                    {
                        if (CharacterObject.MAGEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);

                            /*
                            int intEssenceLoss = 0;
                            if (!CharacterObjectSettings.ESSLossReducesMaximumOnly)
                                intEssenceLoss = _objCharacter.EssencePenalty;
                            */
                            // If the character options permit initiation in create mode, show the Initiation page.
                            UpdateInitiationCost();

                            tabInitiation.Text = await LanguageManager.GetStringAsync("Tab_Initiation");
                            tsMetamagicAddMetamagic.Text = await LanguageManager.GetStringAsync("Button_AddMetamagic");
                            cmdAddMetamagic.Text = await LanguageManager.GetStringAsync("Button_AddInitiateGrade");
                            cmdDeleteMetamagic.Text = await LanguageManager.GetStringAsync("Button_RemoveInitiateGrade");
                            chkInitiationOrdeal.Text = (await LanguageManager.GetStringAsync("Checkbox_InitiationOrdeal"))
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationOrdealPercent.ToString("P", GlobalSettings.CultureInfo));
                            gpbInitiationType.Text = await LanguageManager.GetStringAsync("String_InitiationType");
                            gpbInitiationGroup.Text = await LanguageManager.GetStringAsync("String_InitiationGroup");
                            chkInitiationGroup.Text = (await LanguageManager.GetStringAsync("Checkbox_InitiationGroup"))
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationGroupPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationSchooling.Text = (await LanguageManager.GetStringAsync("Checkbox_InitiationSchooling"))
                                .Replace("{0}", CharacterObjectSettings.KarmaMAGInitiationSchoolingPercent.ToString("P", GlobalSettings.CultureInfo));

                            chkInitiationSchooling.Enabled = true;
                            tsMetamagicAddArt.Visible = true;
                            tsMetamagicAddEnchantment.Visible = true;
                            tsMetamagicAddEnhancement.Visible = true;
                            tsMetamagicAddRitual.Visible = true;

                            string strInitTip = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Tip_ImproveInitiateGrade")
                                , (CharacterObject.InitiateGrade + 1).ToString(GlobalSettings.CultureInfo)
                                , (CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectSettings.KarmaInitiation).ToString(GlobalSettings.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = await LanguageManager.GetStringAsync("Checkbox_JoinedGroup");

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

                        IsCharacterUpdateRequested = true;
                    }
                    break;

                case nameof(Character.RESEnabled):
                    {
                        // Change to the status of RES being enabled.
                        if (CharacterObject.RESEnabled)
                        {
                            /*
                            int intEssenceLoss = 0;
                            if (!CharacterObjectSettings.ESSLossReducesMaximumOnly)
                                intEssenceLoss = _objCharacter.EssencePenalty;
                            // If the character options permit submersion in create mode, show the Initiation page.
                            */
                            UpdateInitiationCost();

                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);

                            tabInitiation.Text = await LanguageManager.GetStringAsync("Tab_Submersion");
                            tsMetamagicAddMetamagic.Text = await LanguageManager.GetStringAsync("Button_AddEcho");
                            cmdAddMetamagic.Text = await LanguageManager.GetStringAsync("Button_AddSubmersionGrade");
                            cmdDeleteMetamagic.Text = await LanguageManager.GetStringAsync("Button_RemoveSubmersionGrade");
                            gpbInitiationType.Text = await LanguageManager.GetStringAsync("String_SubmersionType");
                            gpbInitiationGroup.Text = await LanguageManager.GetStringAsync("String_SubmersionNetwork");
                            chkInitiationOrdeal.Text = (await LanguageManager.GetStringAsync("Checkbox_SubmersionTask"))
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationOrdealPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationGroup.Text = (await LanguageManager.GetStringAsync("Checkbox_NetworkSubmersion"))
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationGroupPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationSchooling.Text = (await LanguageManager.GetStringAsync("Checkbox_InitiationSchooling"))
                                .Replace("{0}", CharacterObjectSettings.KarmaRESInitiationSchoolingPercent.ToString("P", GlobalSettings.CultureInfo));
                            chkInitiationSchooling.Enabled = CharacterObjectSettings.AllowTechnomancerSchooling;
                            tsMetamagicAddArt.Visible = false;
                            tsMetamagicAddEnchantment.Visible = false;
                            tsMetamagicAddEnhancement.Visible = false;
                            tsMetamagicAddRitual.Visible = false;

                            string strInitTip = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Tip_ImproveSubmersionGrade")
                                , (CharacterObject.SubmersionGrade + 1).ToString(GlobalSettings.CultureInfo)
                                , (CharacterObjectSettings.KarmaInitiationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectSettings.KarmaInitiation).ToString(GlobalSettings.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = await LanguageManager.GetStringAsync("Checkbox_JoinedNetwork");

                            if (CharacterObject.AttributeSection.Attributes?.Contains(CharacterObject.RES) == false)
                            {
                                CharacterObject.AttributeSection.Attributes.Add(CharacterObject.RES);
                            }
                        }
                        else
                        {
                            if (!CharacterObject.MAGEnabled)
                                tabCharacterTabs.TabPages.Remove(tabInitiation);

                            if (CharacterObject.AttributeSection.Attributes?.Contains(CharacterObject.RES) == true)
                            {
                                CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.RES);
                            }
                        }

                        IsCharacterUpdateRequested = true;
                    }
                    break;

                case nameof(Character.DEPEnabled):
                    {
                        if (CharacterObject.DEPEnabled)
                        {
                            if (CharacterObject.AttributeSection.Attributes?.Contains(CharacterObject.DEP) == false)
                            {
                                CharacterObject.AttributeSection.Attributes.Add(CharacterObject.DEP);
                            }
                        }
                        else if (CharacterObject.AttributeSection.Attributes?.Contains(CharacterObject.DEP) == true)
                        {
                            CharacterObject.AttributeSection.Attributes.Remove(CharacterObject.DEP);
                        }
                    }
                    break;

                case nameof(Character.Ambidextrous):
                    {
                        cboPrimaryArm.BeginUpdate();

                        using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstPrimaryArm))
                        {
                            if (CharacterObject.Ambidextrous)
                            {
                                lstPrimaryArm.Add(new ListItem("Ambidextrous", await LanguageManager.GetStringAsync("String_Ambidextrous")));
                                cboPrimaryArm.Enabled = false;
                            }
                            else
                            {
                                //Create the dropdown for the character's primary arm.
                                lstPrimaryArm.Add(new ListItem("Left", await LanguageManager.GetStringAsync("String_Improvement_SideLeft")));
                                lstPrimaryArm.Add(new ListItem("Right", await LanguageManager.GetStringAsync("String_Improvement_SideRight")));
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
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.AttributeSection.Attributes != null)
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
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.AttributeSection.Attributes != null)
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
                            if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.AttributeSection.Attributes != null)
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
                                if (CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.AttributeSection.Attributes != null)
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
                            bool blnDoRefresh = false;
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(
                                                                                  x => x.Children, x =>
                                                                                      x.SourceType
                                                                                      == Improvement.ImprovementSource
                                                                                          .Bioware
                                                                                      && x.SourceID != Cyberware
                                                                                          .EssenceHoleGUID
                                                                                      && x.SourceID != Cyberware
                                                                                          .EssenceAntiHoleGUID
                                                                                      && x.IsModularCurrentlyEquipped)
                                                                              .ToList())
                            {
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (objCyberware.CanRemoveThroughImprovements)
                                    {
                                        objCyberware.Parent?.Children.Remove(objCyberware);
                                        CharacterObject.Cyberware.Add(objCyberware);
                                        objCyberware.ChangeModularEquip(false);
                                    }
                                    continue;
                                }
                                if (!objCyberware.CanRemoveThroughImprovements)
                                    continue;
                                objCyberware.DeleteCyberware();
                                blnDoRefresh = true;
                            }

                            if (blnDoRefresh)
                            {
                                IsCharacterUpdateRequested = true;
                            }
                        }
                    }
                    break;

                case nameof(Character.AddCyberwareEnabled):
                    {
                        if (!CharacterObject.AddCyberwareEnabled)
                        {
                            bool blnDoRefresh = false;
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(
                                                                                  x => x.Children, x =>
                                                                                      x.SourceType
                                                                                      == Improvement.ImprovementSource
                                                                                          .Cyberware
                                                                                      && x.SourceID != Cyberware
                                                                                          .EssenceHoleGUID
                                                                                      && x.SourceID != Cyberware
                                                                                          .EssenceAntiHoleGUID
                                                                                      && x.IsModularCurrentlyEquipped)
                                                                              .ToList())
                            {
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (objCyberware.CanRemoveThroughImprovements)
                                    {
                                        objCyberware.ChangeModularEquip(false);
                                        objCyberware.Parent?.Children.Remove(objCyberware);
                                        CharacterObject.Cyberware.Add(objCyberware);
                                    }
                                    continue;
                                }
                                if (!objCyberware.CanRemoveThroughImprovements)
                                    continue;
                                objCyberware.DeleteCyberware();
                                blnDoRefresh = true;
                            }

                            if (blnDoRefresh)
                            {
                                IsCharacterUpdateRequested = true;
                            }
                        }
                    }
                    break;

                case nameof(Character.ExCon):
                    {
                        if (CharacterObject.ExCon)
                        {
                            bool blnDoRefresh = false;
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(
                                                                                  x => x.Children, x =>
                                                                                      x.SourceID != Cyberware.EssenceHoleGUID
                                                                                      && x.SourceID != Cyberware
                                                                                          .EssenceAntiHoleGUID
                                                                                      && x.IsModularCurrentlyEquipped)
                                                                              .ToList())
                            {
                                char chrAvail = objCyberware.TotalAvailTuple(false).Suffix;
                                if (chrAvail != 'R' && chrAvail != 'F')
                                    continue;
                                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                                {
                                    if (objCyberware.CanRemoveThroughImprovements)
                                    {
                                        objCyberware.Parent?.Children.Remove(objCyberware);
                                        CharacterObject.Cyberware.Add(objCyberware);
                                        objCyberware.ChangeModularEquip(false);
                                    }
                                    continue;
                                }
                                if (!objCyberware.CanRemoveThroughImprovements)
                                    continue;
                                objCyberware.DeleteCyberware();
                                blnDoRefresh = true;
                            }

                            if (blnDoRefresh)
                            {
                                IsCharacterUpdateRequested = true;
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
                        gpbInitiationType.Visible = CharacterObject.InitiationEnabled;
                        gpbInitiationGroup.Visible = CharacterObject.InitiationEnabled;
                    }
                    break;

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
                        nudMysticAdeptMAGMagician.Visible = CharacterObject.UseMysticAdeptPPs;
                        break;
                    }
                case nameof(Character.IsPrototypeTranshuman):
                    {
                        IsCharacterUpdateRequested = true;
                        lblPrototypeTranshumanESS.Visible = CharacterObject.IsPrototypeTranshuman;
                        lblPrototypeTranshumanESSLabel.Visible = CharacterObject.IsPrototypeTranshuman;
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
                case nameof(Character.BlackMarketDiscount):
                    {
                        RefreshSelectedCyberware();
                        RefreshSelectedArmor();
                        RefreshSelectedGear();
                        RefreshSelectedVehicle();
                        RefreshSelectedWeapon();
                        break;
                    }
                case nameof(Character.Settings):
                {
                    if (!IsLoading)
                    {
                        foreach (PropertyInfo objProperty in typeof(CharacterSettings).GetProperties())
                            await DoOnCharacterSettingsPropertyChanged(
                                new PropertyChangedEventArgs(objProperty.Name));
                    }
                    break;
                }
            }
        }

        private async void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await DoOnCharacterSettingsPropertyChanged(e);
        }

        private async ValueTask DoOnCharacterSettingsPropertyChanged(PropertyChangedEventArgs e)
        {
            IsCharacterUpdateRequested = true;
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.Books):
                    {
                        if (IsLoading)
                            break;
                        using (new CursorWait(this))
                        {
                            SuspendLayout();
                            cmdAddLifestyle.SplitMenuStrip =
                                CharacterObjectSettings.BookEnabled("RF") ? cmsAdvancedLifestyle : null;

                            if (!CharacterObjectSettings.BookEnabled("FA"))
                            {
                                lblWildReputation.Visible = false;
                                lblWildReputationTotal.Visible = false;
                                if (!CharacterObjectSettings.BookEnabled("SG"))
                                {
                                    lblAstralReputation.Visible = false;
                                    lblAstralReputationTotal.Visible = false;
                                }
                                else
                                {
                                    lblAstralReputation.Visible = true;
                                    lblAstralReputationTotal.Visible = true;
                                }
                            }
                            else
                            {
                                lblWildReputation.Visible = true;
                                lblWildReputationTotal.Visible = true;
                                lblAstralReputation.Visible = true;
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

                            RefreshArmor(treArmor, cmsArmorLocation, cmsArmor, cmsArmorMod, cmsArmorGear);
                            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
                            RefreshFociFromGear(treFoci, null);
                            RefreshCyberware(treCyberware, cmsCyberware, cmsCyberwareGear);
                            RefreshWeapons(treWeapons, cmsWeaponLocation, cmsWeapon, cmsWeaponAccessory,
                                           cmsWeaponAccessoryGear);
                            RefreshVehicles(treVehicles, cmsVehicleLocation, cmsVehicle, cmsVehicleWeapon,
                                            cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, cmsVehicleGear,
                                            cmsWeaponMount,
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
                                (await CharacterObject.LoadDataXPathAsync("traditions.xml"))
                                               .SelectSingleNodeAndCacheExpression("/chummer");
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
                                        0, new ListItem("None", await LanguageManager.GetStringAsync("String_None")));
                                    if (!lstTraditions.SequenceEqual(cboTradition.Items.Cast<ListItem>()))
                                    {
                                        cboTradition.BeginUpdate();
                                        cboTradition.PopulateWithListItems(lstTraditions);
                                        if (CharacterObject.MagicTradition.Type == TraditionType.MAG)
                                            cboTradition.SelectedValue
                                                = CharacterObject.MagicTradition.SourceID.ToString();
                                        else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                                            cboTradition.SelectedIndex = 0;
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
                                foreach (Improvement improvement in ImprovementManager
                                             .GetCachedImprovementListForValueOf(
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
                                (await CharacterObject.LoadDataXPathAsync("streams.xml"))
                                               .SelectSingleNodeAndCacheExpression("/chummer");
                            using (new FetchSafelyFromPool<List<ListItem>>(
                                       Utils.ListItemListPool, out List<ListItem> lstStreams))
                            {
                                if (xmlTraditionsBaseChummerNode != null)
                                {
                                    foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                                 "traditions/tradition[" + CharacterObjectSettings.BookXPath()
                                                                         + ']'))
                                    {
                                        string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name")
                                                                     ?.Value;
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
                                        0, new ListItem("None", await LanguageManager.GetStringAsync("String_None")));
                                    if (!lstStreams.SequenceEqual(cboStream.Items.Cast<ListItem>()))
                                    {
                                        cboStream.BeginUpdate();
                                        cboStream.PopulateWithListItems(lstStreams);
                                        if (CharacterObject.MagicTradition.Type == TraditionType.RES)
                                            cboStream.SelectedValue
                                                = CharacterObject.MagicTradition.SourceID.ToString();
                                        else if (cboStream.SelectedIndex == -1 && cboStream.Items.Count > 0)
                                            cboStream.SelectedIndex = 0;
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
                            lblEnemiesBP.Visible = false;
                            lblBuildEnemies.Visible = false;
                        }
                        else
                        {
                            lblEnemiesBP.Visible = true;
                            lblBuildEnemies.Visible = true;
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
        private void PowerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PowerPropertyChanged_StopWatch.ElapsedMilliseconds < 4) return;
            PowerPropertyChanged_StopWatch.Restart();
            tabPowerUc.CalculatePowerPoints();
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void SkillPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //HACK PERFORMANCE
            //So, skills tell if anything maybe interesting have happened, but this don't have any way to see if it is relevant. Instead of redrawing EVYER FYCKING THING we do it only every 5 ms
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

        private async void mnuFileSaveAsCreated_Click(object sender, EventArgs e)
        {
            await SaveCharacterAs(true);
        }

        private async void mnuFilePrint_Click(object sender, EventArgs e)
        {
            await DoPrint();
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void mnuSpecialAddPACKSKit_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddPACKSKit();
            }
            while (blnAddAgain);
        }

        private async void mnuSpecialCreatePACKSKit_Click(object sender, EventArgs e)
        {
            await CreatePACKSKit();
        }

        private async void mnuSpecialChangeMetatype_Click(object sender, EventArgs e)
        {
            await ChangeMetatype();
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

        private async void mnuSpecialCyberzombie_Click(object sender, EventArgs e)
        {
            await CharacterObject.ConvertCyberzombie();
        }

        private async void mnuSpecialAddCyberwareSuite_Click(object sender, EventArgs e)
        {
            await AddCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private async void mnuSpecialAddBiowareSuite_Click(object sender, EventArgs e)
        {
            await AddCyberwareSuite(Improvement.ImprovementSource.Bioware);
        }

        private async void mnuSpecialCreateCyberwareSuite_Click(object sender, EventArgs e)
        {
            await CreateCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private async void mnuSpecialCreateBiowareSuite_Click(object sender, EventArgs e)
        {
            await CreateCyberwareSuite(Improvement.ImprovementSource.Bioware);
        }

        private async void mnuSpecialReapplyImprovements_Click(object sender, EventArgs e)
        {
            // This only re-applies the Improvements for everything the character has. If a match is not found in the data files, the current Improvement information is left as-is.
            // Verify that the user wants to go through with it.
            if (Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ConfirmReapplyImprovements"), await LanguageManager.GetStringAsync("MessageTitle_ConfirmReapplyImprovements"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            await DoReapplyImprovements();
        }

        private async ValueTask DoReapplyImprovements(ICollection<string> lstInternalIdFilter = null)
        {
            using (new CursorWait(this))
            using (await EnterWriteLock.EnterAsync(CharacterObject.LockObject))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdOutdatedItems))
                {
                    // Record the status of any flags that normally trigger character events.
                    bool blnMAGEnabled = CharacterObject.MAGEnabled;
                    bool blnRESEnabled = CharacterObject.RESEnabled;
                    bool blnDEPEnabled = CharacterObject.DEPEnabled;

                    _blnReapplyImprovements = true;

                    // Wipe all improvements that we will reapply, this is mainly to eliminate orphaned improvements caused by certain bugs and also for a performance increase
                    if (lstInternalIdFilter == null)
                        ImprovementManager.RemoveImprovements(CharacterObject, CharacterObject.Improvements.Where(x =>
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .AIProgram ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Armor ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .ArmorMod ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Bioware ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .ComplexForm ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .CritterPower ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Cyberware ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Echo ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Gear ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .MartialArt ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .MartialArtTechnique ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Metamagic ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Power ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Quality ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Spell ||
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .StackedFocus).ToList(),
                                                              _blnReapplyImprovements);
                    else
                        ImprovementManager.RemoveImprovements(CharacterObject, CharacterObject.Improvements.Where(
                                                                  x => lstInternalIdFilter.Contains(x.SourceName) &&
                                                                       (x.ImproveSource == Improvement.ImprovementSource
                                                                            .AIProgram ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Armor ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .ArmorMod ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Bioware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .ComplexForm ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .CritterPower ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Cyberware ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Echo ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Gear ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .MartialArt ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .MartialArtTechnique ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Metamagic ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Power ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Quality ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .Spell ||
                                                                        x.ImproveSource == Improvement.ImprovementSource
                                                                            .StackedFocus)).ToList(),
                                                              _blnReapplyImprovements);

                    // Refresh Qualities.
                    // We cannot use foreach because qualities can add more qualities
                    for (int j = 0; j < CharacterObject.Qualities.Count; j++)
                    {
                        Quality objQuality = CharacterObject.Qualities[j];
                        if (objQuality.OriginSource == QualitySource.Improvement
                            || objQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen)
                            continue;
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objQuality.InternalId) == false)
                            continue;

                        XmlNode objNode = await objQuality.GetNodeAsync();
                        if (objNode != null)
                        {
                            string strSelected = objQuality.Extra;

                            objQuality.Bonus = objNode["bonus"];
                            if (objQuality.Bonus != null)
                            {
                                ImprovementManager.ForcedValue = strSelected;
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.Quality,
                                                                      objQuality.InternalId, objQuality.Bonus, 1,
                                                                      objQuality.DisplayNameShort(
                                                                          GlobalSettings.Language));
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
                                            || lstInternalIdFilter?.Contains(objCheckQuality.InternalId) == false))
                                    {
                                        blnDoFirstLevel = false;
                                        break;
                                    }
                                }

                                if (blnDoFirstLevel)
                                {
                                    ImprovementManager.ForcedValue = strSelected;
                                    ImprovementManager.CreateImprovements(
                                        CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                        objQuality.FirstLevelBonus, 1,
                                        objQuality.CurrentDisplayNameShort);
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    {
                                        objQuality.Extra = ImprovementManager.SelectedValue;
                                        TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                                        if (objTreeNode != null)
                                            objTreeNode.Text = objQuality.CurrentDisplayName;
                                    }
                                }
                            }

                            objQuality.NaturalWeaponsNode = objNode["naturalweapons"];
                            if (objQuality.NaturalWeaponsNode != null)
                            {
                                ImprovementManager.ForcedValue = strSelected;
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.NaturalWeaponsNode, 1, objQuality.CurrentDisplayNameShort);
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objQuality.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objTreeNode = treQualities.FindNodeByTag(objQuality);
                                    if (objTreeNode != null)
                                        objTreeNode.Text = objQuality.CurrentDisplayName;
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
                        XmlNode objMartialArtNode = await objMartialArt.GetNodeAsync();
                        if (objMartialArtNode != null)
                        {
                            // We're only re-apply improvements a list of items, not all of them
                            if (lstInternalIdFilter?.Contains(objMartialArt.InternalId) != false
                                && objMartialArtNode["bonus"] != null)
                            {
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.MartialArt,
                                                                      objMartialArt.InternalId,
                                                                      objMartialArtNode["bonus"], 1,
                                                                      objMartialArt.DisplayNameShort(
                                                                          GlobalSettings.Language));
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objMartialArt.CurrentDisplayName);
                        }

                        foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques.Where(
                                     x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                        {
                            XmlNode objNode = await objTechnique.GetNodeAsync();
                            if (objNode != null)
                            {
                                if (objNode["bonus"] != null)
                                    ImprovementManager.CreateImprovements(
                                        CharacterObject, Improvement.ImprovementSource.MartialArtTechnique,
                                        objTechnique.InternalId, objNode["bonus"], 1,
                                        objTechnique.CurrentDisplayName);
                            }
                            else
                            {
                                sbdOutdatedItems.AppendLine(objMartialArt.CurrentDisplayName);
                            }
                        }
                    }

                    // Refresh Spells.
                    foreach (Spell objSpell in CharacterObject.Spells.Where(
                                 x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                    {
                        XmlNode objNode = await objSpell.GetNodeAsync();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objSpell.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.Spell,
                                                                      objSpell.InternalId, objNode["bonus"], 1,
                                                                      objSpell.DisplayNameShort(
                                                                          GlobalSettings.Language));
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
                    foreach (Power objPower in CharacterObject.Powers.Where(
                                 x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                    {
                        XmlNode objNode = await objPower.GetNodeAsync();
                        if (objNode != null)
                        {
                            objPower.Bonus = objNode["bonus"];
                            if (objPower.Bonus != null)
                            {
                                ImprovementManager.ForcedValue = objPower.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.Power,
                                                                      objPower.InternalId, objPower.Bonus,
                                                                      objPower.TotalRating,
                                                                      objPower.DisplayNameShort(
                                                                          GlobalSettings.Language));
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objPower.CurrentDisplayName);
                        }
                    }

                    // Refresh Complex Forms.
                    foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms.Where(
                                 x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                    {
                        XmlNode objNode = await objComplexForm.GetNodeAsync();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objComplexForm.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.ComplexForm,
                                                                      objComplexForm.InternalId, objNode["bonus"], 1,
                                                                      objComplexForm.DisplayNameShort(
                                                                          GlobalSettings.Language));
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
                    foreach (AIProgram objProgram in CharacterObject.AIPrograms.Where(
                                 x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                    {
                        XmlNode objNode = await objProgram.GetNodeAsync();
                        if (objNode != null)
                        {
                            if (objNode["bonus"] != null)
                            {
                                ImprovementManager.ForcedValue = objProgram.Extra;
                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.AIProgram,
                                                                      objProgram.InternalId, objNode["bonus"], 1,
                                                                      objProgram.DisplayNameShort(
                                                                          GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                {
                                    objProgram.Extra = ImprovementManager.SelectedValue;
                                    TreeNode objProgramNode = treAIPrograms.FindNode(objProgram.InternalId);
                                    if (objProgramNode != null)
                                        objProgramNode.Text = objProgram.CurrentDisplayNameShort;
                                }
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objProgram.CurrentDisplayNameShort);
                        }
                    }

                    // Refresh Critter Powers.
                    foreach (CritterPower objPower in CharacterObject.CritterPowers.Where(
                                 x => lstInternalIdFilter?.Contains(x.InternalId) != false))
                    {
                        XmlNode objNode = await objPower.GetNodeAsync();
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

                                ImprovementManager.CreateImprovements(CharacterObject,
                                                                      Improvement.ImprovementSource.CritterPower,
                                                                      objPower.InternalId, objPower.Bonus, intRating,
                                                                      objPower.DisplayNameShort(
                                                                          GlobalSettings.Language));
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
                        XmlNode objNode = await objMetamagic.GetNodeAsync();
                        if (objNode != null)
                        {
                            objMetamagic.Bonus = objNode["bonus"];
                            if (objMetamagic.Bonus != null)
                            {
                                ImprovementManager.CreateImprovements(CharacterObject, objMetamagic.SourceType,
                                                                      objMetamagic.InternalId, objMetamagic.Bonus, 1,
                                                                      objMetamagic.DisplayNameShort(
                                                                          GlobalSettings.Language));
                            }
                        }
                        else
                        {
                            sbdOutdatedItems.AppendLine(objMetamagic.CurrentDisplayName);
                        }
                    }

                    // Refresh Cyberware and Bioware.
                    Dictionary<Cyberware, int> dicPairableCyberwares
                        = new Dictionary<Cyberware, int>(CharacterObject.Cyberware.Count);
                    foreach (Cyberware objCyberware in CharacterObject.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objCyberware.InternalId) != false)
                        {
                            XmlNode objNode = await objCyberware.GetNodeAsync();
                            if (objNode != null)
                            {
                                objCyberware.Bonus = objNode["bonus"];
                                objCyberware.WirelessBonus = objNode["wirelessbonus"];
                                objCyberware.PairBonus = objNode["pairbonus"];
                                if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right"
                                                                               && objCyberware.Forced != "Left")
                                    ImprovementManager.ForcedValue = objCyberware.Forced;
                                if (objCyberware.Bonus != null)
                                {
                                    ImprovementManager.CreateImprovements(
                                        CharacterObject, objCyberware.SourceType, objCyberware.InternalId,
                                        objCyberware.Bonus, objCyberware.Rating,
                                        objCyberware.CurrentDisplayNameShort);
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
                                        Cyberware objMatchingCyberware
                                            = dicPairableCyberwares.Keys.FirstOrDefault(
                                                x => objCyberware.IncludePair.Contains(x.Name)
                                                     && x.Extra == objCyberware.Extra);
                                        if (objMatchingCyberware != null)
                                            ++dicPairableCyberwares[objMatchingCyberware];
                                        else
                                            dicPairableCyberwares.Add(objCyberware, 1);
                                    }
                                }

                                TreeNode objWareNode = objCyberware.SourceID == Cyberware.EssenceHoleGUID
                                                       || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
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
                        List<Cyberware> lstPairableCyberwares = CharacterObject.Cyberware
                                                                               .DeepWhere(x => x.Children,
                                                                                   x => objCyberware.IncludePair
                                                                                           .Contains(x.Name)
                                                                                       && x.Extra == objCyberware.Extra
                                                                                       && x.IsModularCurrentlyEquipped)
                                                                               .ToList();
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(objCyberware.Location)
                            && objCyberware.IncludePair.All(x => x == objCyberware.Name))
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

                        if (intCyberwaresCount <= 0)
                            continue;
                        foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                        {
                            if ((intCyberwaresCount & 1) == 0)
                            {
                                if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right"
                                                                               && objCyberware.Forced != "Left")
                                    ImprovementManager.ForcedValue = objCyberware.Forced;
                                ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType,
                                                                      objLoopCyberware.InternalId + "Pair",
                                                                      objLoopCyberware.PairBonus,
                                                                      objLoopCyberware.Rating,
                                                                      objLoopCyberware.DisplayNameShort(
                                                                          GlobalSettings.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue)
                                    && string.IsNullOrEmpty(objCyberware.Extra))
                                    objCyberware.Extra = ImprovementManager.SelectedValue;
                                TreeNode objNode = objLoopCyberware.SourceID == Cyberware.EssenceHoleGUID
                                                   || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
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

                    // Refresh Armors.
                    foreach (Armor objArmor in CharacterObject.Armor)
                    {
                        // We're only re-apply improvements a list of items, not all of them
                        if (lstInternalIdFilter?.Contains(objArmor.InternalId) != false)
                        {
                            XmlNode objNode = await objArmor.GetNodeAsync();
                            if (objNode != null)
                            {
                                objArmor.Bonus = objNode["bonus"];
                                if (objArmor.Bonus != null && objArmor.Equipped)
                                {
                                    ImprovementManager.ForcedValue = objArmor.Extra;
                                    ImprovementManager.CreateImprovements(
                                        CharacterObject, Improvement.ImprovementSource.Armor, objArmor.InternalId,
                                        objArmor.Bonus, objArmor.Rating,
                                        objArmor.CurrentDisplayNameShort);
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
                                XmlNode objChild = await objMod.GetNodeAsync();

                                if (objChild != null)
                                {
                                    objMod.Bonus = objChild["bonus"];
                                    if (objMod.Bonus != null && objMod.Equipped)
                                    {
                                        ImprovementManager.ForcedValue = objMod.Extra;
                                        ImprovementManager.CreateImprovements(
                                            CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId,
                                            objMod.Bonus, objMod.Rating,
                                            objMod.CurrentDisplayNameShort);
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
                        await DoOnCharacterPropertyChanged(new PropertyChangedEventArgs(nameof(Character.MAGEnabled)));
                    if (blnRESEnabled != CharacterObject.RESEnabled)
                        await DoOnCharacterPropertyChanged(new PropertyChangedEventArgs(nameof(Character.RESEnabled)));
                    if (blnDEPEnabled != CharacterObject.DEPEnabled)
                        await DoOnCharacterPropertyChanged(new PropertyChangedEventArgs(nameof(Character.DEPEnabled)));

                    IsCharacterUpdateRequested = true;
                    // Immediately call character update because it re-applies essence loss improvements
                    await DoUpdateCharacterInfoAsync();

                    if (sbdOutdatedItems.Length > 0 && !Utils.IsUnitTest)
                    {
                        Program.ShowMessageBox(
                            this, await LanguageManager.GetStringAsync("Message_ReapplyImprovementsFoundOutdatedItems_Top") +
                                  sbdOutdatedItems +
                                  await LanguageManager.GetStringAsync("Message_ReapplyImprovementsFoundOutdatedItems_Bottom"),
                            await LanguageManager.GetStringAsync("MessageTitle_ConfirmReapplyImprovements"), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }

            IsDirty = true;
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

        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            object objSelectedObject = null;
            if (tabCharacterTabs?.SelectedTab == tabStreetGear)
            {
                objSelectedObject = treGear.SelectedNode?.Tag;
            }
            else if (tabCharacterTabs?.SelectedTab == tabArmor)
            {
                objSelectedObject = treArmor.SelectedNode?.Tag;
            }
            else if (tabCharacterTabs?.SelectedTab == tabVehicles)
            {
                objSelectedObject = treVehicles.SelectedNode?.Tag;
            }
            else if (tabCharacterTabs?.SelectedTab == tabWeapons)
            {
                objSelectedObject = treWeapons.SelectedNode?.Tag;
            }
            else if (tabCharacterTabs?.SelectedTab == tabCyberware)
            {
                objSelectedObject = treCyberware.SelectedNode?.Tag;
            }
            else if (tabCharacterTabs?.SelectedTab == tabLifestyle)
            {
                // Intentionally blank, lifestyles are always clones.
            }
            else
            {
                Utils.BreakIfDebug();
                return;
            }

            switch (GlobalSettings.ClipboardContentType)
            {
                case ClipboardContentType.Armor:
                    {
                        // Paste Armor.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/armor");
                        if (objXmlNode != null)
                        {
                            Armor objArmor = new Armor(CharacterObject);
                            objArmor.Load(objXmlNode, true);
                            CharacterObject.Armor.Add(objArmor);

                            AddChildVehicles(objArmor.InternalId);
                            AddChildWeapons(objArmor.InternalId);
                        }

                        break;
                    }
                case ClipboardContentType.ArmorMod:
                    {
                        if (!(objSelectedObject is Armor selectedArmor && selectedArmor.AllowPasteXml)) break;
                        // Paste Armor.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/armormod");
                        if (objXmlNode != null)
                        {
                            ArmorMod objArmorMod = new ArmorMod(CharacterObject);
                            objArmorMod.Load(objXmlNode, true);
                            selectedArmor.ArmorMods.Add(objArmorMod);

                            AddChildVehicles(objArmorMod.InternalId);
                            AddChildWeapons(objArmorMod.InternalId);
                        }

                        break;
                    }
                case ClipboardContentType.Cyberware:
                    {
                        // Paste Cyberware.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/cyberware");
                        if (objXmlNode != null)
                        {
                            Cyberware objCyberware = new Cyberware(CharacterObject);
                            objCyberware.Load(objXmlNode, true);
                            if (objSelectedObject is Cyberware objCyberwareParent)
                            {
                                if (!objCyberwareParent.AllowPasteObject(objCyberware))
                                {
                                    objCyberware.DeleteCyberware();
                                    return;
                                }

                                objCyberware.Grade = objCyberwareParent.Grade;
                                objCyberwareParent.Children.Add(objCyberware);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(objCyberware.LimbSlot) &&
                                    !objCyberware.GetValidLimbSlot(objCyberware.GetNodeXPath(GlobalSettings.Language)))
                                {
                                    objCyberware.DeleteCyberware();
                                    return;
                                }

                                CharacterObject.Cyberware.Add(objCyberware);
                            }

                            AddChildVehicles(objCyberware.InternalId);
                            AddChildWeapons(objCyberware.InternalId);
                        }

                        break;
                    }
                case ClipboardContentType.Gear:
                    {
                        // Paste Gear.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/gear");
                        if (objXmlNode == null)
                            break;
                        Gear objGear = new Gear(CharacterObject);
                        objGear.Load(objXmlNode, true);
                        if (objSelectedObject is ICanPaste selected && selected.AllowPasteXml &&
                            objSelectedObject is IHasGear gear)
                        {
                            gear.GearChildren.Add(objGear);
                            if (gear is ICanEquip selectedEquip && !selectedEquip.Equipped)
                                objGear.ChangeEquippedStatus(false);
                        }
                        else
                        {
                            CharacterObject.Gear.Add(objGear);
                        }

                        AddChildVehicles(objGear.InternalId);
                        AddChildWeapons(objGear.InternalId);
                        break;
                    }
                case ClipboardContentType.Lifestyle:
                    {
                        // Lifestyle Tab.
                        if (tabStreetGearTabs.SelectedTab != tabLifestyle)
                            break;

                        // Paste Lifestyle.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/lifestyle");
                        if (objXmlNode == null)
                            break;

                        Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                        objLifestyle.Load(objXmlNode, true);
                        // Reset the number of months back to 1 since 0 isn't valid in Create Mode.
                        objLifestyle.Increments = 1;
                        CharacterObject.Lifestyles.Add(objLifestyle);
                        break;
                    }
                case ClipboardContentType.Vehicle:
                    {
                        // Paste Vehicle.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/vehicle");
                        Vehicle objVehicle = new Vehicle(CharacterObject);
                        objVehicle.Load(objXmlNode, true);
                        CharacterObject.Vehicles.Add(objVehicle);
                        break;
                    }
                case ClipboardContentType.Weapon:
                    {
                        // Paste Weapon.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/weapon");
                        if (objXmlNode != null)
                        {
                            Weapon objWeapon;
                            switch (objSelectedObject)
                            {
                                case Weapon objWeaponParent when !objWeaponParent.AllowPasteXml:
                                    return;

                                case Weapon objWeaponParent:
                                    objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objXmlNode, true);
                                    objWeaponParent.Children.Add(objWeapon);
                                    break;

                                case WeaponMount objWeaponMount when !objWeaponMount.AllowPasteXml:
                                    return;

                                case WeaponMount objWeaponMount:
                                    objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objXmlNode, true);
                                    objWeaponMount.Weapons.Add(objWeapon);
                                    break;

                                case VehicleMod objMod when !objMod.AllowPasteXml:
                                    return;

                                case VehicleMod objMod:
                                    objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objXmlNode, true);
                                    objMod.Weapons.Add(objWeapon);
                                    break;

                                default:
                                    objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objXmlNode, true);
                                    CharacterObject.Weapons.Add(objWeapon);
                                    break;
                            }

                            AddChildVehicles(objWeapon.InternalId);
                            AddChildWeapons(objWeapon.InternalId);
                        }

                        break;
                    }
                case ClipboardContentType.WeaponAccessory:
                    {
                        if (!(objSelectedObject is Weapon selectedWeapon && selectedWeapon.AllowPasteXml))
                            break;
                        // Paste Armor.
                        XmlNode objXmlNode = GlobalSettings.Clipboard.SelectSingleNode("/character/accessory");
                        if (objXmlNode != null)
                        {
                            WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                            objMod.Load(objXmlNode, true);
                            selectedWeapon.WeaponAccessories.Add(objMod);

                            AddChildVehicles(objMod.InternalId);
                            AddChildWeapons(objMod.InternalId);
                        }

                        break;
                    }
                default:
                    Utils.BreakIfDebug();
                    break;
            }

            void AddChildWeapons(string parentId)
            {
                XmlNodeList objXmlNodeList = GlobalSettings.Clipboard.SelectNodes("/character/weapons/weapon");
                if (!(objXmlNodeList?.Count > 0))
                    return;
                foreach (XmlNode objLoopNode in objXmlNodeList)
                {
                    Weapon objWeapon = new Weapon(CharacterObject);
                    objWeapon.Load(objLoopNode, true);
                    CharacterObject.Weapons.Add(objWeapon);
                    objWeapon.ParentID = parentId;
                }
            }
            void AddChildVehicles(string parentId)
            {
                // Add any Vehicles that come with the Cyberware.
                XmlNodeList objXmlNodeList = GlobalSettings.Clipboard.SelectNodes("/character/vehicles/vehicle");
                if (!(objXmlNodeList?.Count > 0))
                    return;
                foreach (XmlNode objLoopNode in objXmlNodeList)
                {
                    Vehicle objVehicle = new Vehicle(CharacterObject);
                    objVehicle.Load(objLoopNode, true);
                    CharacterObject.Vehicles.Add(objVehicle);
                    objVehicle.ParentID = parentId;
                }
            }
        }

        private async void mnuSpecialConvertToFreeSprite_Click(object sender, EventArgs e)
        {
            XmlNode objXmlPower = (await CharacterObject.LoadDataAsync("critterpowers.xml")).SelectSingleNode("/chummer/powers/power[name = \"Denial\"]");
            CritterPower objPower = new CritterPower(CharacterObject);
            objPower.Create(objXmlPower);
            objPower.CountTowardsLimit = false;
            if (objPower.InternalId.IsEmptyGuid())
                return;

            CharacterObject.CritterPowers.Add(objPower);

            CharacterObject.MetatypeCategory = "Free Sprite";
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

        private async void cmdAddSpell_Click(object sender, EventArgs e)
        {
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("spells.xml");

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    using (SelectSpell frmPickSpell = new SelectSpell(CharacterObject))
                    {
                        await frmPickSpell.ShowDialogSafeAsync(this);
                        // Make sure the dialogue window was not canceled.
                        if (frmPickSpell.DialogResult == DialogResult.Cancel)
                            break;

                        blnAddAgain = frmPickSpell.AddAgain;

                        XmlNode objXmlSpell = objXmlDocument.SelectSingleNode("/chummer/spells/spell[id = " + frmPickSpell.SelectedSpell.CleanXPath() + ']');

                        Spell objSpell = new Spell(CharacterObject);
                        objSpell.Create(objXmlSpell, string.Empty, frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
                        if (objSpell.InternalId.IsEmptyGuid())
                        {
                            objSpell.Dispose();
                            continue;
                        }

                        objSpell.FreeBonus = frmPickSpell.FreeBonus;
                        // Barehanded Adept
                        if (objSpell.FreeBonus && CharacterObject.AdeptEnabled && !CharacterObject.MagicianEnabled && (objSpell.Range == "T" || objSpell.Range == "T (A)"))
                        {
                            objSpell.BarehandedAdept = true;
                        }

                        CharacterObject.Spells.Add(objSpell);
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
            if (!(treCyberware.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            selectedObject.Remove();
        }

        private async void cmdAddComplexForm_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("complexforms.xml");
            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    // The number of Complex Forms cannot exceed twice the character's RES.
                    if (CharacterObject.ComplexForms.Count >= CharacterObject.RES.Value * 2 + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.ComplexFormLimit) && !CharacterObject.IgnoreRules)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ComplexFormLimit"), await LanguageManager.GetStringAsync("MessageTitle_ComplexFormLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                }
            }
            while (blnAddAgain);
        }

        private async void cmdAddAIProgram_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("programs.xml");
            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    XmlNode objXmlProgram;
                    // Let the user select a Program.
                    using (SelectAIProgram frmPickProgram = new SelectAIProgram(CharacterObject))
                    {
                        await frmPickProgram.ShowDialogSafeAsync(this);

                        // Make sure the dialogue window was not canceled.
                        if (frmPickProgram.DialogResult == DialogResult.Cancel)
                        {
                            break;
                        }

                        blnAddAgain = frmPickProgram.AddAgain;

                        objXmlProgram = objXmlDocument.SelectSingleNode("/chummer/programs/program[id = " + frmPickProgram.SelectedProgram.CleanXPath() + ']');
                    }

                    if (objXmlProgram == null)
                        continue;

                    // Check for SelectText.
                    string strExtra = string.Empty;
                    XmlNode xmlSelectText = objXmlProgram.SelectSingleNode("bonus/selecttext");
                    if (xmlSelectText != null)
                    {
                        using (SelectText frmPickText = new SelectText
                        {
                            Description = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Improvement_SelectText"), objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"]?.InnerText)
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

                    CharacterObject.AIPrograms.Add(objProgram);
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteArmor_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            selectedObject.Remove();
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

        private async void cmdAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            Location location = null;
            if (treWeapons.SelectedNode?.Tag is Location objLocation)
            {
                location = objLocation;
            }
            do
            {
                blnAddAgain = await AddWeapon(location);
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> AddWeapon(Location objLocation = null)
        {
            using (new CursorWait(this))
            {
                using (SelectWeapon frmPickWeapon = new SelectWeapon(CharacterObject))
                {
                    await frmPickWeapon.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                        return false;

                    // Open the Weapons XML file and locate the selected piece.
                    XmlNode objXmlWeapon = (await CharacterObject.LoadDataAsync("weapons.xml")).SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');
                    if (objXmlWeapon == null)
                        return frmPickWeapon.AddAgain;

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Weapon objWeapon = new Weapon(CharacterObject);
                    objWeapon.Create(objXmlWeapon, lstWeapons);
                    objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;
                    if (frmPickWeapon.FreeCost)
                    {
                        objWeapon.Cost = "0";
                    }

                    //objWeapon.Location = objLocation;
                    objLocation?.Children.Add(objWeapon);
                    CharacterObject.Weapons.Add(objWeapon);

                    foreach (Weapon objExtraWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objExtraWeapon);
                    }

                    return frmPickWeapon.AddAgain;
                }
            }
        }

        private void cmdDeleteWeapon_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is ICanRemove objSelectedNode))
                return;
            objSelectedNode.Remove();
        }

        private async void cmdAddLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    Lifestyle objLifestyle;
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
                        objLifestyle = frmPickLifestyle.SelectedLifestyle;
                    }

                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteLifestyle_Click(object sender, EventArgs e)
        {
            // Delete the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is ICanRemove objSelectedObject))
                return;
            objSelectedObject.Remove();
        }

        private async void cmdAddGear_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            string strSelectedId = string.Empty;
            if (treGear.SelectedNode?.Tag is Location objNode)
            {
                strSelectedId = objNode.InternalId;
            }
            do
            {
                blnAddAgain = await PickGear(strSelectedId);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteGear_Click(object sender, EventArgs e)
        {
            if (!(treGear.SelectedNode?.Tag is ICanRemove objSelectedGear))
                return;
            objSelectedGear.Remove();
        }

        private async ValueTask<bool> AddVehicle(Location objLocation = null)
        {
            using (new CursorWait(this))
            {
                using (SelectVehicle frmPickVehicle = new SelectVehicle(CharacterObject))
                {
                    await frmPickVehicle.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickVehicle.DialogResult == DialogResult.Cancel)
                        return false;

                    // Open the Vehicles XML file and locate the selected piece.
                    XmlNode objXmlVehicle = (await CharacterObject.LoadDataAsync("vehicles.xml")).SelectSingleNode("/chummer/vehicles/vehicle[id = " + frmPickVehicle.SelectedVehicle.CleanXPath() + ']');
                    if (objXmlVehicle == null)
                        return frmPickVehicle.AddAgain;
                    Vehicle objVehicle = new Vehicle(CharacterObject);
                    objVehicle.Create(objXmlVehicle);
                    // Update the Used Vehicle information if applicable.
                    if (frmPickVehicle.UsedVehicle)
                    {
                        objVehicle.Avail = frmPickVehicle.UsedAvail;
                        objVehicle.Cost = frmPickVehicle.UsedCost.ToString(GlobalSettings.InvariantCultureInfo);
                    }

                    objVehicle.DiscountCost = frmPickVehicle.BlackMarketDiscount;
                    if (frmPickVehicle.FreeCost)
                    {
                        objVehicle.Cost = "0";
                    }

                    //objVehicle.Location = objLocation;
                    objLocation?.Children.Add(objVehicle);

                    CharacterObject.Vehicles.Add(objVehicle);

                    return frmPickVehicle.AddAgain;
                }
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
            await DeleteVehicle();
        }

        private async ValueTask DeleteVehicle()
        {
            if (!cmdDeleteVehicle.Enabled)
                return;
            // Delete the selected Vehicle.
            object objSelectedNodeTag = treVehicles.SelectedNode?.Tag;
            switch (objSelectedNodeTag)
            {
                // Delete the selected Vehicle.
                case null:
                    return;

                case VehicleMod objMod:
                    {
                        // If this is the Obsolete Mod, the user must select a percentage. This will create an Expense that costs X% of the Vehicle's base cost to remove the special Obsolete Mod.
                        if (objMod.Name == "Obsolete" ||
                            objMod.Name == "Obsolescent" && CharacterObjectSettings.AllowObsolescentUpgrade)
                        {
                            decimal decPercentage;
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

                                decPercentage = frmModPercent.SelectedValue;
                            }

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
                        }

                        objMod.DeleteVehicleMod();
                        break;
                    }
                case ICanRemove selectedObject:
                    {
                        selectedObject.Remove();
                        break;
                    }
            }
        }

        private void cmdAddMartialArt_Click(object sender, EventArgs e)
        {
            MartialArt.Purchase(CharacterObject);
        }

        private void cmdDeleteMartialArt_Click(object sender, EventArgs e)
        {
            if (!(treMartialArts.SelectedNode?.Tag is ICanRemove objSelectedNode))
                return;
            objSelectedNode.Remove();
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
            using (new CursorWait(this))
            {
                if (CharacterObject.MAGEnabled)
                {
                    // Make sure that the Initiate Grade is not attempting to go above the character's MAG CharacterAttribute.
                    if (CharacterObject.InitiateGrade + 1 > CharacterObject.MAG.TotalValue ||
                        CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.InitiateGrade + 1 > CharacterObject.MAGAdept.TotalValue)
                    {
                        Program.ShowMessageBox(this, LanguageManager.GetString("Message_CannotIncreaseInitiateGrade"), LanguageManager.GetString("MessageTitle_CannotIncreaseInitiateGrade"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Create the Initiate Grade object.
                    InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                    objGrade.Create(CharacterObject.InitiateGrade + 1, false, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                    CharacterObject.InitiationGrades.AddWithSort(objGrade);
                }
                else if (CharacterObject.RESEnabled)
                {
                    tsMetamagicAddArt.Visible = false;
                    tsMetamagicAddEnchantment.Visible = false;
                    tsMetamagicAddEnhancement.Visible = false;
                    tsMetamagicAddRitual.Visible = false;
                    tsMetamagicAddMetamagic.Text = LanguageManager.GetString("Button_AddEcho");

                    // Make sure that the Initiate Grade is not attempting to go above the character's RES CharacterAttribute.
                    if (CharacterObject.SubmersionGrade + 1 > CharacterObject.RES.TotalValue)
                    {
                        Program.ShowMessageBox(this, LanguageManager.GetString("Message_CannotIncreaseSubmersionGrade"), LanguageManager.GetString("MessageTitle_CannotIncreaseSubmersionGrade"), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Create the Initiate Grade object.
                    InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                    objGrade.Create(CharacterObject.SubmersionGrade + 1, true, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                    CharacterObject.InitiationGrades.AddWithSort(objGrade);
                }
            }
        }

        private void cmdDeleteMetamagic_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            selectedObject.Remove();
        }

        private async void cmdAddCritterPower_Click(object sender, EventArgs e)
        {
            // Make sure the Critter is allowed to have Optional Powers.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("critterpowers.xml");

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
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

                        CharacterObject.CritterPowers.Add(objPower);
                    }
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCritterPower_Click(object sender, EventArgs e)
        {
            // If the selected object is not a critter or it comes from an initiate grade, we don't want to remove it.
            if (!(treCritterPowers.SelectedNode?.Tag is CritterPower objCritterPower) || objCritterPower.Grade != 0)
                return;
            objCritterPower.Remove();
        }

        private void cmdDeleteComplexForm_Click(object sender, EventArgs e)
        {
            if (!(treComplexForms.SelectedNode?.Tag is ICanRemove objSelectedObject))
                return;
            objSelectedObject.Remove();
        }

        private void cmdDeleteAIProgram_Click(object sender, EventArgs e)
        {
            // Delete the selected AI Program.
            if (!(treAIPrograms.SelectedNode?.Tag is ICanRemove objSelectedObject))
                return;
            objSelectedObject.Remove();
        }

        private async void cmdLifeModule_Click(object sender, EventArgs e)
        {
            XmlNode xmlStagesParentNode = (await CharacterObject.LoadDataAsync("lifemodules.xml")).SelectSingleNode("chummer/stages");

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    //from 1 to second highest life module order possible (ye hardcoding is bad, but extra stage is a niche case)
                    int intStage;
                    for (intStage = 1; intStage < 5; ++intStage)
                    {
                        XmlNode xmlStageNode = xmlStagesParentNode?.SelectSingleNode("stage[@order = " + intStage.ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() + ']');
                        if (xmlStageNode == null)
                        {
                            --intStage;
                            break;
                        }

                        if (!CharacterObject.Qualities.Any(x => x.Type == QualityType.LifeModule && x.Stage == xmlStageNode.InnerText))
                        {
                            break;
                        }
                    }

                    //i--; //Counter last increment
                    XmlNode objXmlLifeModule;
                    using (SelectLifeModule frmSelectLifeModule = new SelectLifeModule(CharacterObject, intStage))
                    {
                        await frmSelectLifeModule.ShowDialogSafeAsync(this);

                        if (frmSelectLifeModule.DialogResult == DialogResult.Cancel)
                            break;

                        blnAddAgain = frmSelectLifeModule.AddAgain;
                        objXmlLifeModule = frmSelectLifeModule.SelectedNode;
                    }

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objLifeModule = new Quality(CharacterObject);

                    objLifeModule.Create(objXmlLifeModule, QualitySource.LifeModule, lstWeapons);
                    if (objLifeModule.InternalId.IsEmptyGuid())
                        continue;

                    //Is there any reason not to add it?
                    if (true)
                    {
                        CharacterObject.Qualities.Add(objLifeModule);

                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }

                    //Stupid hardcoding but no sane way
                    //To do group skills (not that anything else is sane)
                }
            }
            while (blnAddAgain);
        }

        private async void cmdAddQuality_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("qualities.xml");
            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    using (SelectQuality frmPickQuality = new SelectQuality(CharacterObject))
                    {
                        await frmPickQuality.ShowDialogSafeAsync(this);

                        // Don't do anything else if the form was canceled.
                        if (frmPickQuality.DialogResult == DialogResult.Cancel)
                            break;

                        blnAddAgain = frmPickQuality.AddAgain;
                        int intRatingToAdd = frmPickQuality.SelectedRating;

                        XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + frmPickQuality.SelectedQuality.CleanXPath() + ']');
                        int intDummy = 0;
                        if (objXmlQuality != null && objXmlQuality["nolevels"] == null && objXmlQuality.TryGetInt32FieldQuickly("limit", ref intDummy))
                        {
                            intRatingToAdd -= CharacterObject.Qualities.Count(x =>
                                x.SourceIDString.Equals(frmPickQuality.SelectedQuality,
                                    StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(x.SourceName));
                        }
                        for (int i = 1; i <= intRatingToAdd; ++i)
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality objQuality = new Quality(CharacterObject);

                            objQuality.Create(objXmlQuality, QualitySource.Selected, lstWeapons);
                            if (objQuality.InternalId.IsEmptyGuid())
                            {
                                // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                                ImprovementManager.RemoveImprovements(CharacterObject,
                                    Improvement.ImprovementSource.Quality, objQuality.InternalId);
                                break;
                            }

                            if (frmPickQuality.FreeCost)
                                objQuality.BP = 0;

                            // Make sure that adding the Quality would not cause the character to exceed their BP limits.
                            bool blnAddItem = true;
                            if (objQuality.ContributeToLimit && !CharacterObject.IgnoreRules)
                            {
                                // If the item being checked would cause the limit of 25 BP spent on Positive Qualities to be exceed, do not let it be checked and display a message.
                                int intMaxQualityAmount = CharacterObjectSettings.QualityKarmaLimit;
                                string strAmount =
                                    CharacterObjectSettings.QualityKarmaLimit.ToString(GlobalSettings.CultureInfo) +
                                    await LanguageManager.GetStringAsync("String_Space") +
                                    await LanguageManager.GetStringAsync("String_Karma");

                                // Add the cost of the Quality that is being added.
                                int intBP = objQuality.BP;

                                if (objQuality.Type == QualityType.Negative)
                                {
                                    // Check if adding this Quality would put the character over their limit.
                                    if (!CharacterObjectSettings.ExceedNegativeQualities)
                                    {
                                        intBP += CharacterObject.NegativeQualityLimitKarma;
                                        if (intBP < intMaxQualityAmount * -1)
                                        {
                                            Program.ShowMessageBox(this,
                                                string.Format(GlobalSettings.CultureInfo,
                                                    await LanguageManager.GetStringAsync("Message_NegativeQualityLimit"),
                                                    strAmount),
                                                await LanguageManager.GetStringAsync("MessageTitle_NegativeQualityLimit"),
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            blnAddItem = false;
                                        }
                                        else if (CharacterObject.MetatypeBP < 0
                                                 && intBP + CharacterObject.MetatypeBP < intMaxQualityAmount * -1)
                                        {
                                            Program.ShowMessageBox(this,
                                                string.Format(GlobalSettings.CultureInfo,
                                                    await LanguageManager.GetStringAsync(
                                                        "Message_NegativeQualityAndMetatypeLimit"), strAmount),
                                                await LanguageManager.GetStringAsync("MessageTitle_NegativeQualityLimit"),
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            blnAddItem = false;
                                        }
                                    }
                                }
                                // Check if adding this Quality would put the character over their limit.
                                else if (!CharacterObjectSettings.ExceedPositiveQualities)
                                {
                                    intBP += CharacterObject.PositiveQualityKarma;
                                    if (intBP > intMaxQualityAmount)
                                    {
                                        Program.ShowMessageBox(this,
                                            string.Format(GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync("Message_PositiveQualityLimit"), strAmount),
                                            await LanguageManager.GetStringAsync("MessageTitle_PositiveQualityLimit"),
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        blnAddItem = false;
                                    }
                                }
                            }

                            if (blnAddItem)
                            {
                                CharacterObject.Qualities.Add(objQuality);

                                // Add any created Weapons to the character.
                                foreach (Weapon objWeapon in lstWeapons)
                                {
                                    CharacterObject.Weapons.Add(objWeapon);
                                }
                            }
                            else
                            {
                                // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                                ImprovementManager.RemoveImprovements(CharacterObject,
                                    Improvement.ImprovementSource.Quality, objQuality.InternalId);
                                break;
                            }
                        }
                    }
                }
            }
            while (blnAddAgain);
        }

        /*
        private Quality AddQuality(XmlNode objXmlAddQuality, XmlNode objXmlSelectedQuality)
        {
            string strForceValue = string.Empty;
            if (objXmlAddQuality.Attributes["select"] != null)
                strForceValue = objXmlAddQuality.Attributes["select"].InnerText;
            bool blnAddQuality = true;

            // Make sure the character does not yet have this Quality.
            foreach (Quality objCharacterQuality in _objCharacter.Qualities)
            {
                if (objCharacterQuality.Name == objXmlAddQuality.InnerText && objCharacterQuality.Extra == strForceValue)
                {
                    blnAddQuality = false;
                    break;
                }
            }

            if (blnAddQuality)
            {
                List<Weapon> objAddWeapons = new List<Weapon>(1);
                Quality objAddQuality = new Quality(_objCharacter);
                objAddQuality.Create(objXmlSelectedQuality, _objCharacter, QualitySource.Selected, lstWeapons, strForceValue);

                // Add any created Weapons to the character.
                foreach (Weapon objWeapon in objAddWeapons)
                {
                    _objCharacter.Weapons.Add(objWeapon);
                }

                return objAddQuality;
            }

            return null;
        }
        */

        private bool RemoveQuality(Quality objSelectedQuality, bool blnConfirmDelete = true, bool blnCompleteDelete = true)
        {
            XmlNode objXmlDeleteQuality = objSelectedQuality.GetNode();
            switch (objSelectedQuality.OriginSource)
            {
                // Qualities that come from a Metatype cannot be removed.
                case QualitySource.Metatype:
                case QualitySource.Heritage:
                    Program.ShowMessageBox(this, LanguageManager.GetString("Message_MetavariantQuality"), LanguageManager.GetString("MessageTitle_MetavariantQuality"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;

                case QualitySource.Improvement:
                    Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ImprovementQuality"), objSelectedQuality.GetSourceName(GlobalSettings.Language)),
                        LanguageManager.GetString("MessageTitle_MetavariantQuality"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
            }

            using (new CursorWait(this))
            {
                if (objSelectedQuality.OriginSource == QualitySource.MetatypeRemovable)
                {
                    int intBP = 0;
                    if (objSelectedQuality.Type == QualityType.Negative && objXmlDeleteQuality.TryGetInt32FieldQuickly("karma", ref intBP))
                    {
                        intBP = -intBP;
                    }

                    intBP *= CharacterObjectSettings.KarmaQuality;
                    int intShowBP = intBP;
                    if (blnCompleteDelete)
                        intShowBP *= objSelectedQuality.Levels;
                    string strBP = intShowBP.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");

                    if (blnConfirmDelete &&
                        !CommonFunctions.ConfirmDelete(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString(blnCompleteDelete ? "Message_DeleteMetatypeQuality" : "Message_LowerMetatypeQualityLevel"), strBP)))
                        return false;

                    // Remove any Improvements that the Quality might have.
                    XmlNode xmlDeleteQualityNoBonus = objXmlDeleteQuality.Clone();
                    if (xmlDeleteQualityNoBonus["bonus"] != null)
                        xmlDeleteQualityNoBonus["bonus"].InnerText = string.Empty;
                    if (xmlDeleteQualityNoBonus["firstlevelbonus"] != null)
                        xmlDeleteQualityNoBonus["firstlevelbonus"].InnerText = string.Empty;

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objReplaceQuality = new Quality(CharacterObject);

                    objReplaceQuality.Create(xmlDeleteQualityNoBonus, QualitySource.MetatypeRemovedAtChargen, lstWeapons);
                    objReplaceQuality.BP *= -1;
                    // If a Negative Quality is being bought off, the replacement one is Positive.
                    if (objSelectedQuality.Type == QualityType.Positive)
                    {
                        objReplaceQuality.Type = QualityType.Negative;
                        if (!string.IsNullOrEmpty(objReplaceQuality.Extra))
                            objReplaceQuality.Extra += ',' + LanguageManager.GetString("String_Space");
                        objReplaceQuality.Extra += LanguageManager.GetString("String_ExpenseRemovePositiveQuality");
                    }
                    else
                    {
                        objReplaceQuality.Type = QualityType.Positive;
                        if (!string.IsNullOrEmpty(objReplaceQuality.Extra))
                            objReplaceQuality.Extra += ',' + LanguageManager.GetString("String_Space");
                        objReplaceQuality.Extra += LanguageManager.GetString("String_ExpenseRemoveNegativeQuality");
                    }

                    // The replacement Quality does not count towards the BP limit of the new type, nor should it be printed.
                    objReplaceQuality.AllowPrint = false;
                    objReplaceQuality.ContributeToLimit = false;
                    CharacterObject.Qualities.Add(objReplaceQuality);
                    // The replacement Quality no longer adds its weapons to the character
                }
                else
                {
                    if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(blnCompleteDelete ? LanguageManager.GetString("Message_DeleteQuality") : LanguageManager.GetString("Message_LowerQualityLevel")))
                        return false;

                    if (objSelectedQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen)
                    {
                        XPathNavigator xmlCharacterNode = CharacterObject.GetNodeXPath();
                        if (xmlCharacterNode != null)
                        {
                            XmlDocument xmlQualitiesDoc = CharacterObject.LoadData("qualities.xml");
                            // Create the Qualities that come with the Metatype.
                            foreach (XPathNavigator objXmlQualityItem in xmlCharacterNode.Select("qualities/*/quality[. = " + objSelectedQuality.Name.CleanXPath() + ']'))
                            {
                                XmlNode objXmlQuality = xmlQualitiesDoc.SelectSingleNode("/chummer/qualities/quality[name = " + objXmlQualityItem.Value.CleanXPath() + ']');
                                Quality objQuality = new Quality(CharacterObject);
                                string strForceValue = objXmlQualityItem.GetAttribute("select", string.Empty);
                                QualitySource objSource = objXmlQualityItem.GetAttribute("removable", string.Empty) == bool.TrueString
                                    ? QualitySource.MetatypeRemovable
                                    : QualitySource.Metatype;
                                objQuality.Create(objXmlQuality, objSource, CharacterObject.Weapons, strForceValue);
                                CharacterObject.Qualities.Add(objQuality);
                            }
                        }
                    }
                }

                if (objSelectedQuality.Type == QualityType.LifeModule)
                {
                    objXmlDeleteQuality = Quality.GetNodeOverrideable(objSelectedQuality.SourceIDString, CharacterObject.LoadData("lifemodules.xml"));
                }

                // Fix for legacy characters with old addqualities improvements.
                RemoveAddedQualities(objXmlDeleteQuality?.CreateNavigator().Select("addqualities/addquality"));

                // Perform removal
                objSelectedQuality.DeleteQuality(blnCompleteDelete);
            }

            return true;
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (!(treQualities.SelectedNode?.Tag is Quality objSelectedQuality))
                return;
            string strInternalIDToRemove = objSelectedQuality.InternalId;
            // Can't do a foreach because we're removing items, this is the next best thing
            bool blnFirstRemoval = true;
            for (int i = CharacterObject.Qualities.Count - 1; i >= 0; --i)
            {
                Quality objLoopQuality = CharacterObject.Qualities.ElementAt(i);
                if (objLoopQuality.InternalId != strInternalIDToRemove)
                    continue;
                if (!RemoveQuality(objLoopQuality, blnFirstRemoval))
                    break;
                blnFirstRemoval = false;
                if (i > CharacterObject.Qualities.Count)
                {
                    i = CharacterObject.Qualities.Count;
                }
            }
        }

        private async void cmdAddLocation_Click(object sender, EventArgs e)
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
                Location objLocation = new Location(CharacterObject, CharacterObject.GearLocations, frmPickText.SelectedValue);
                CharacterObject.GearLocations.Add(objLocation);
            }
        }

        private async void cmdAddWeaponLocation_Click(object sender, EventArgs e)
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
                Location objLocation = new Location(CharacterObject, CharacterObject.WeaponLocations, frmPickText.SelectedValue);
                CharacterObject.WeaponLocations.Add(objLocation);
            }
        }

        private async void cmdCreateStackedFocus_Click(object sender, EventArgs e)
        {
            int intFree = 0;
            List<Gear> lstGear = new List<Gear>(2);
            List<Gear> lstStack = new List<Gear>(2);

            // Run through all of the Foci the character has and count the un-Bonded ones.
            foreach (Gear objGear in CharacterObject.Gear)
            {
                if ((objGear.Category == "Foci" || objGear.Category == "Metamagic Foci") && !objGear.Bonded)
                {
                    intFree++;
                    lstGear.Add(objGear);
                }
            }

            // If the character does not have at least 2 un-Bonded Foci, display an error and leave.
            if (intFree < 2)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotStackFoci"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
                        if (objGear.InternalId == frmPickItem.SelectedItem)
                        {
                            objGear.Bonded = true;
                            lstStack.Add(objGear);
                            lstGear.Remove(objGear);
                            break;
                        }
                    }
                } while (lstGear.Count > 0 && frmPickItem.DialogResult != DialogResult.Cancel);
            }

            // Make sure at least 2 Foci were selected.
            if (lstStack.Count < 2)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_StackedFocusMinimum"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_StackedFocusForce"), await LanguageManager.GetStringAsync("MessageTitle_CannotStackFoci"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // Create the Stacked Focus.
            StackedFocus objStack = new StackedFocus(CharacterObject);
            foreach (Gear objGear in lstStack)
                objStack.Gear.Add(objGear);
            CharacterObject.StackedFoci.Add(objStack);

            // Remove the Gear from the character and replace it with a Stacked Focus item.
            decimal decCost = 0;
            foreach (Gear objGear in lstStack)
            {
                decCost += objGear.TotalCost;
                CharacterObject.Gear.Remove(objGear);
            }

            Gear objStackItem = new Gear(CharacterObject)
            {
                Category = "Stacked Focus",
                Name = "Stacked Focus: " + objStack.CurrentDisplayName,
                MinRating = string.Empty,
                MaxRating = string.Empty,
                Source = "SR5",
                Page = "1",
                Cost = decCost.ToString(GlobalSettings.CultureInfo),
                Avail = "0"
            };

            CharacterObject.Gear.Add(objStackItem);

            objStack.GearId = objStackItem.InternalId;
        }

        private async void cmdAddArmor_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddArmor(treArmor.SelectedNode?.Tag as Location);
            }
            while (blnAddAgain);
        }

        private async ValueTask<bool> AddArmor(Location objLocation = null)
        {
            using (new CursorWait(this))
            {
                using (SelectArmor frmPickArmor = new SelectArmor(CharacterObject))
                {
                    await frmPickArmor.ShowDialogSafeAsync(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickArmor.DialogResult == DialogResult.Cancel)
                        return false;

                    // Open the Armor XML file and locate the selected piece.
                    XmlNode objXmlArmor = (await CharacterObject.LoadDataAsync("armor.xml")).SelectSingleNode("/chummer/armors/armor[id = " + frmPickArmor.SelectedArmor.CleanXPath() + ']');

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Armor objArmor = new Armor(CharacterObject);

                    objArmor.Create(objXmlArmor, frmPickArmor.Rating, lstWeapons);
                    objArmor.DiscountCost = frmPickArmor.BlackMarketDiscount;
                    if (objArmor.InternalId.IsEmptyGuid())
                        return frmPickArmor.AddAgain;
                    if (frmPickArmor.FreeCost)
                    {
                        objArmor.Cost = "0";
                    }

                    //objArmor.Location = objLocation;
                    objLocation?.Children.Add(objArmor);
                    CharacterObject.Armor.Add(objArmor);

                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }

                    return frmPickArmor.AddAgain;
                }
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
            if (!(treArmor.SelectedNode?.Tag is Location objLocation))
                return;
            foreach (Armor objArmor in objLocation.Children.OfType<Armor>())
            {
                objArmor.Equipped = true;
            }
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdArmorUnEquipAll_Click(object sender, EventArgs e)
        {
            if (!(treArmor.SelectedNode?.Tag is Location objLocation))
                return;
            foreach (Armor objArmor in objLocation.Children.OfType<Armor>())
            {
                objArmor.Equipped = false;
            }
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
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicleLocation"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        #endregion Button Events

        #region ContextMenu Events

        private async void tsCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treCyberware.SelectedNode?.Tag is Cyberware objCyberware && !string.IsNullOrWhiteSpace(objCyberware.AllowedSubsystems)))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponAccessory"),
                    await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Accessories cannot be added to Cyberweapons.
            if (objWeapon.Cyberware)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberweaponNoAccessory"), await LanguageManager.GetStringAsync("MessageTitle_CyberweaponNoAccessory"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlNode objXmlWeapon = await objWeapon.GetNodeAsync();
            if (objXmlWeapon == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotFindWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument xmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    // Make sure the Weapon allows Accessories to be added to it.
                    if (!objWeapon.AllowAccessory)
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotModifyWeapon"), await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        objXmlWeapon = xmlDocument.SelectSingleNode("/chummer/accessories/accessory[id = " + frmPickWeaponAccessory.SelectedAccessory.CleanXPath() + ']');

                        WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
                        objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, frmPickWeaponAccessory.SelectedRating);
                        objAccessory.Parent = objWeapon;
                        objAccessory.DiscountCost = frmPickWeaponAccessory.BlackMarketDiscount;

                        if (frmPickWeaponAccessory.FreeCost)
                        {
                            objAccessory.Cost = "0";
                        }
                        else if (objAccessory.Cost.StartsWith("Variable(", StringComparison.Ordinal))
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

                        objWeapon.WeaponAccessories.Add(objAccessory);
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsAddArmorMod_Click(object sender, EventArgs e)
        {
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                using (new CursorWait(this))
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

                        ArmorMod objMod = new ArmorMod(CharacterObject);
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        int intRating = Convert.ToInt32(objXmlArmor?["maxrating"]?.InnerText, GlobalSettings.InvariantCultureInfo) > 1 ? frmPickArmorMod.SelectedRating : 0;

                        objMod.Create(objXmlArmor, intRating, lstWeapons);
                        if (objMod.InternalId.IsEmptyGuid())
                            continue;

                        if (frmPickArmorMod.FreeCost)
                        {
                            objMod.Cost = "0";
                        }

                        objArmor.ArmorMods.Add(objMod);

                        // Add any Weapons created by the Mod.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickGear(objGear.InternalId);
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
                return;
            using (new CursorWait(this))
            {
                using (CreateWeaponMount frmPickVehicleMod = new CreateWeaponMount(objVehicle, CharacterObject))
                {
                    await frmPickVehicleMod.ShowDialogSafeAsync(this);

                    if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                        return;
                    objVehicle.WeaponMounts.Add(frmPickVehicleMod.WeaponMount);
                }
            }
        }

        private async void tsVehicleAddMod_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            while (objSelectedNode?.Level > 1)
                objSelectedNode = objSelectedNode.Parent;

            // Make sure a parent items is selected, then open the Select Vehicle Mod window.
            if (!(objSelectedNode?.Tag is Vehicle objVehicle))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicle"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Vehicles XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("vehicles.xml");

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
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
                        if (frmPickVehicleMod.FreeCost)
                            objMod.Cost = "0";
                        else
                        {
                            // Multiply the cost if applicable.
                            decimal decOldCost = objMod.TotalCost;
                            decimal decCost = decOldCost;
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
                            decCost -= decOldCost;
                            objMod.Markup = decCost;
                        }

                        objVehicle.Mods.Add(objMod);
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddWeaponWeapon_Click(object sender, EventArgs e)
        {
            // Make sure that a Weapon Mount has been selected.
            // Attempt to locate the selected VehicleMod.
            WeaponMount objWeaponMount = null;
            VehicleMod objMod = null;
            Vehicle objVehicle = null;
            switch (treVehicles.SelectedNode?.Tag)
            {
                case WeaponMount selectedMount:
                    objWeaponMount = selectedMount;
                    objVehicle = selectedMount.Parent;
                    break;

                case VehicleMod selectedMod when (selectedMod.Name.StartsWith("Mechanical Arm", StringComparison.Ordinal) || selectedMod.Name.Contains("Drone Arm")):
                    objMod = selectedMod;
                    objVehicle = selectedMod.Parent;
                    break;
            }

            if (objWeaponMount == null && objMod == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotAddWeapon"),
                    await LanguageManager.GetStringAsync("MessageTitle_CannotAddWeapon"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (objWeaponMount?.IsWeaponsFull == true)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponMountFull"),
                    await LanguageManager.GetStringAsync("MessageTitle_CannotAddWeapon"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    using (SelectWeapon frmPickWeapon = new SelectWeapon(CharacterObject)
                    {
                        LimitToCategories = objMod == null ? objWeaponMount.AllowedWeaponCategories : objMod.WeaponMountCategories
                    })
                    {
                        await frmPickWeapon.ShowDialogSafeAsync(this);

                        if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                            return;

                        // Open the Weapons XML file and locate the selected piece.
                        XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        Weapon objWeapon = new Weapon(CharacterObject)
                        {
                            ParentVehicle = objVehicle,
                            ParentVehicleMod = objMod,
                            ParentMount = objMod == null ? objWeaponMount : null
                        };
                        objWeapon.Create(objXmlWeapon, lstWeapons);
                        objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;

                        if (frmPickWeapon.FreeCost)
                        {
                            objWeapon.Cost = "0";
                        }

                        if (objMod != null)
                            objMod.Weapons.Add(objWeapon);
                        else
                            objWeaponMount.Weapons.Add(objWeapon);

                        foreach (Weapon objLoopWeapon in lstWeapons)
                        {
                            if (objMod == null)
                                objWeaponMount.Weapons.Add(objLoopWeapon);
                            else
                                objMod.Weapons.Add(objLoopWeapon);
                        }

                        blnAddAgain = frmPickWeapon.AddAgain && (objMod != null || !objWeaponMount.IsWeaponsFull);
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddWeaponAccessory_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.ShowMessageBox(
                    this, await LanguageManager.GetStringAsync("Message_VehicleWeaponAccessories"),
                    await LanguageManager.GetStringAsync("MessageTitle_VehicleWeaponAccessories"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Open the Weapons XML file and locate the selected Weapon.
            XmlNode objXmlWeapon = await objWeapon.GetNodeAsync();
            if (objXmlWeapon == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CannotFindWeapon"),
                                                await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"),
                                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("weapons.xml");

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    // Make sure the Weapon allows Accessories to be added to it.
                    if (!objWeapon.AllowAccessory)
                    {
                        Program.ShowMessageBox(
                            this, await LanguageManager.GetStringAsync("Message_CannotModifyWeapon"),
                            await LanguageManager.GetStringAsync("MessageTitle_CannotModifyWeapon"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        objAccessory.DiscountCost = frmPickWeaponAccessory.BlackMarketDiscount;

                        if (frmPickWeaponAccessory.FreeCost)
                        {
                            objAccessory.Cost = "0";
                        }

                        objWeapon.WeaponAccessories.Add(objAccessory);
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleAddUnderbarrelWeapon_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            if (!(treVehicles.SelectedNode?.Tag is Weapon objSelectedWeapon))
            {
                Program.ShowMessageBox(
                    this, await LanguageManager.GetStringAsync("Message_VehicleWeaponUnderbarrel"),
                    await LanguageManager.GetStringAsync("MessageTitle_VehicleWeaponUnderbarrel"), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using (new CursorWait(this))
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
                        return;

                    // Open the Weapons XML file and locate the selected piece.
                    XmlNode objXmlWeapon = (await CharacterObject.LoadDataAsync("weapons.xml")).SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Weapon objWeapon = new Weapon(CharacterObject)
                    {
                        ParentVehicle = objSelectedWeapon.ParentVehicle
                    };
                    objWeapon.Create(objXmlWeapon, lstWeapons);
                    objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;

                    if (frmPickWeapon.FreeCost)
                    {
                        objWeapon.Cost = "0";
                    }

                    objWeapon.Parent = objSelectedWeapon;
                    objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);
                    if (!objSelectedWeapon.AllowAccessory)
                        objWeapon.AllowAccessory = false;

                    foreach (Weapon objLoopWeapon in lstWeapons)
                    {
                        objSelectedWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                        if (!objSelectedWeapon.AllowAccessory)
                            objLoopWeapon.AllowAccessory = false;
                    }
                }
            }
        }

        private async void tsMartialArtsAddTechnique_Click(object sender, EventArgs e)
        {
            // Select the Martial Arts node if we're currently on a child.
            while (treMartialArts.SelectedNode?.Level > 1)
                treMartialArts.SelectedNode = treMartialArts.SelectedNode.Parent;

            if (treMartialArts.SelectedNode == null || treMartialArts.SelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectMartialArtTechnique"), await LanguageManager.GetStringAsync("MessageTitle_SelectMartialArtTechnique"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!(treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectMartialArtTechnique"), await LanguageManager.GetStringAsync("MessageTitle_SelectMartialArtTechnique"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument xmlDocument = await CharacterObject.LoadDataAsync("martialarts.xml");

            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    XmlNode xmlTechnique;
                    using (SelectMartialArtTechnique frmPickMartialArtTechnique = new SelectMartialArtTechnique(CharacterObject, objMartialArt))
                    {
                        await frmPickMartialArtTechnique.ShowDialogSafeAsync(this);

                        if (frmPickMartialArtTechnique.DialogResult == DialogResult.Cancel)
                            return;

                        blnAddAgain = frmPickMartialArtTechnique.AddAgain;

                        // Open the Martial Arts XML file and locate the selected piece.
                        xmlTechnique = xmlDocument.SelectSingleNode("/chummer/techniques/technique[id = " + frmPickMartialArtTechnique.SelectedTechnique.CleanXPath() + ']');
                    }

                    // Create the Improvements for the Technique if there are any.
                    MartialArtTechnique objTechnique = new MartialArtTechnique(CharacterObject);
                    objTechnique.Create(xmlTechnique);
                    if (objTechnique.InternalId.IsEmptyGuid())
                        return;

                    objMartialArt.Techniques.Add(objTechnique);
                }
            } while (blnAddAgain);
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
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGearVehicle"), await LanguageManager.GetStringAsync("MessageTitle_SelectGearVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            await PurchaseVehicleGear(objSelectedVehicle, objLocation);
        }

        private async void tsVehicleSensorAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level < 2)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_ModifyVehicleGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Gear was found.
            if (!(objSelectedNode.Tag is Gear objSensor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_ModifyVehicleGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
            List<Weapon> lstWeapons = new List<Weapon>(1);
            do
            {
                using (new CursorWait(this))
                {
                    Gear objGear;
                    lstWeapons.Clear();
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
                        objGear = new Gear(CharacterObject);
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
                    }

                    IsRefreshing = true;
                    nudVehicleGearQty.Increment = objGear.CostFor;
                    //nudVehicleGearQty.Minimum = objGear.CostFor;
                    IsRefreshing = false;

                    objSensor.Children.Add(objGear);

                    if (lstWeapons.Count > 0)
                    {
                        CharacterObject.Vehicles.FindVehicleGear(objSensor.InternalId, out Vehicle objVehicle, out WeaponAccessory _, out Cyberware _);
                        if (objVehicle != null)
                        {
                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                objVehicle.Weapons.Add(objWeapon);
                            }
                        }
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsVehicleGearNotes_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode == null)
                return;
            switch (treVehicles.SelectedNode?.Tag)
            {
                case Gear objGear:
                    {
                        using (EditNotes frmItemNotes = new EditNotes(objGear.Notes, objGear.NotesColor))
                        {
                            await frmItemNotes.ShowDialogSafeAsync(this);
                            if (frmItemNotes.DialogResult != DialogResult.OK)
                                return;
                            objGear.Notes = frmItemNotes.Notes;
                            objGear.NotesColor = frmItemNotes.NotesColor;
                            IsDirty = true;

                            treVehicles.SelectedNode.ForeColor = objGear.PreferredColor;
                            treVehicles.SelectedNode.ToolTipText = objGear.Notes.WordWrap();
                        }

                        break;
                    }
            }
        }

        private async void tsAdvancedLifestyle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                using (new CursorWait(this))
                {
                    Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                    using (SelectLifestyleAdvanced frmPickLifestyle = new SelectLifestyleAdvanced(CharacterObject, objLifestyle))
                    {
                        await frmPickLifestyle.ShowDialogSafeAsync(this);

                        // Make sure the dialogue window was not canceled.
                        if (frmPickLifestyle.DialogResult == DialogResult.Cancel)
                        {
                            if (!ReferenceEquals(objLifestyle, frmPickLifestyle.SelectedLifestyle) && frmPickLifestyle.SelectedLifestyle != null)
                                frmPickLifestyle.SelectedLifestyle.Dispose();
                            return;
                        }

                        blnAddAgain = frmPickLifestyle.AddAgain;

                        objLifestyle = frmPickLifestyle.SelectedLifestyle;
                    }

                    objLifestyle.StyleType = LifestyleType.Advanced;

                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }
            while (blnAddAgain);
        }

        private async void tsWeaponName_Click(object sender, EventArgs e)
        {
            while (treWeapons.SelectedNode?.Level > 1)
                treWeapons.SelectedNode = treWeapons.SelectedNode.Parent;

            // Make sure a parent item is selected, then open the Select Accessory window.
            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponName"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Weapon.
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponName"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (treGear.SelectedNode == null || treGear.SelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGearName"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Gear.
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectGearName"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            // Locate the Weapon that is selected in the tree.
            if (!(objSelectedNode?.Tag is Weapon objSelectedWeapon))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectWeaponUnderbarrel"), await LanguageManager.GetStringAsync("MessageTitle_SelectWeapon"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (objSelectedWeapon.Cyberware)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareUnderbarrel"), await LanguageManager.GetStringAsync("MessageTitle_WeaponUnderbarrel"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (new CursorWait(this))
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
                        return;

                    // Open the Weapons XML file and locate the selected piece.
                    XmlNode objXmlWeapon = (await CharacterObject.LoadDataAsync("weapons.xml")).SelectSingleNode("/chummer/weapons/weapon[id = " + frmPickWeapon.SelectedWeapon.CleanXPath() + ']');

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Weapon objWeapon = new Weapon(CharacterObject);
                    objWeapon.Create(objXmlWeapon, lstWeapons);
                    objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;
                    objWeapon.Parent = objSelectedWeapon;
                    objWeapon.AllowAccessory = objSelectedWeapon.AllowAccessory;
                    if (!objSelectedWeapon.AllowAccessory)
                        objWeapon.AllowAccessory = false;

                    if (frmPickWeapon.FreeCost)
                    {
                        objWeapon.Cost = "0";
                    }

                    objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);
                }
            }
        }

        private async void tsGearRename_Click(object sender, EventArgs e)
        {
            using (SelectText frmPickText = new SelectText())
            {
                //frmPickText.Description = LanguageManager.GetString("String_AddLocation");
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;
                if (treGear.SelectedNode?.Tag is Gear objGear)
                {
                    objGear.Extra = frmPickText.SelectedValue;
                    treGear.SelectedNode.Text = objGear.CurrentDisplayName;
                    IsDirty = true;
                }
            }
        }

        private async void tsArmorLocationAddArmor_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddArmor(treArmor.SelectedNode?.Tag as Location);
            }
            while (blnAddAgain);
        }

        private async void tsAddArmorGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                // Select the root Gear node then open the Select Gear window.
                blnAddAgain = await PickArmorGear(objArmor.InternalId, true);
            }
            while (blnAddAgain);
        }

        private async void tsArmorGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedGuid = string.Empty;
            switch (objSelectedNode.Tag)
            {
                // Make sure the selected item is another piece of Gear.
                case ArmorMod objMod when string.IsNullOrEmpty(objMod.GearCapacity):
                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmor"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;

                case ArmorMod objMod:
                    selectedGuid = objMod.InternalId;
                    break;

                case Gear objGear:
                    selectedGuid = objGear.InternalId;
                    break;
            }
            bool blnAddAgain;
            do
            {
                blnAddAgain = await PickArmorGear(selectedGuid);
            }
            while (blnAddAgain);
        }

        private async void tsArmorNotes_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treArmor.SelectedNode);
            }
        }

        private async void tsWeaponNotes_Click(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treWeapons.SelectedNode);
            }
        }

        private async void tsCyberwareNotes_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treCyberware.SelectedNode);
            }
        }

        private async void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treVehicles.SelectedNode);
            }
        }

        private async void tsQualityNotes_Click(object sender, EventArgs e)
        {
            if (treQualities.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treQualities.SelectedNode);
            }
        }

        private async void tsMartialArtsNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treMartialArts.SelectedNode);
            }
        }

        private async void tsSpellNotes_Click(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treSpells.SelectedNode);
            }
        }

        private async void tsComplexFormNotes_Click(object sender, EventArgs e)
        {
            if (treComplexForms.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treComplexForms.SelectedNode);
            }
        }

        private async void tsAIProgramNotes_Click(object sender, EventArgs e)
        {
            if (treAIPrograms.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treAIPrograms.SelectedNode);
            }
        }

        private async void tsCritterPowersNotes_Click(object sender, EventArgs e)
        {
            if (treCritterPowers.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treCritterPowers.SelectedNode);
            }
        }

        private async void tsMetamagicNotes_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treMetamagic.SelectedNode);
            }
        }

        private async void tsGearNotes_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treGear.SelectedNode);
            }
        }

        private async void tsLifestyleNotes_Click(object sender, EventArgs e)
        {
            if (treLifestyles.SelectedNode?.Tag is IHasNotes objNotes)
            {
                await WriteNotes(objNotes, treLifestyles.SelectedNode);
            }
        }

        private async void tsWeaponMountLocation_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount))
                return;
            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_VehicleName"),
                DefaultString = objWeaponMount.Location
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objWeaponMount.Location = frmPickText.SelectedValue;
            }

            treVehicles.SelectedNode.Text = objWeaponMount.CurrentDisplayName;
        }

        private async void tsVehicleName_Click(object sender, EventArgs e)
        {
            while (treVehicles.SelectedNode?.Level > 1)
            {
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;
            }

            // Make sure a parent item is selected.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Vehicle.
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectVehicleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectVehicle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_VehicleName"),
                DefaultString = objVehicle.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objVehicle.CustomName = frmPickText.SelectedValue;
            }

            treVehicles.SelectedNode.Text = objVehicle.CurrentDisplayName;
        }

        private async void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode?.Tag is string)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleCyberwarePlugin"), await LanguageManager.GetStringAsync("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            string strNeedleId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId;
            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strNeedleId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strNeedleId, out objMod);

            if (objCyberwareParent == null && objMod?.AllowCyberware != true)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_VehicleCyberwarePlugin"), await LanguageManager.GetStringAsync("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Cyberware XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("cyberware.xml");

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
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
                            // If the Cyberware has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                            if (!objCyberwareParent.Capacity.Contains('[') || objCyberwareParent.Capacity.Contains("/["))
                            {
                                frmPickCyberware.Subsystems = objCyberwareParent.AllowedSubsystems;
                                frmPickCyberware.MaximumCapacity = objCyberwareParent.CapacityRemaining;

                                // Do not allow the user to add a new piece of Cyberware if its Capacity has been reached.
                                if (CharacterObjectSettings.EnforceCapacity && objCyberwareParent.CapacityRemaining < 0)
                                {
                                    Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CapacityReached"), await LanguageManager.GetStringAsync("MessageTitle_CapacityReached"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                                   objVehicle, objMod.Cyberware, CharacterObject.Vehicles,
                                                   objMod.Weapons,
                                                   frmPickCyberware.Markup, frmPickCyberware.FreeCost,
                                                   frmPickCyberware.BlackMarketDiscount, true,
                                                   "String_ExpensePurchaseVehicleCyberware", objCyberwareParent))
                            objCyberware.DeleteCyberware();
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void tsArmorName_Click(object sender, EventArgs e)
        {
            while (treArmor.SelectedNode?.Level > 1)
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;

            // Make sure a parent item is selected, then open the Select Accessory window.
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmorName"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Armor.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectArmorName"), await LanguageManager.GetStringAsync("MessageTitle_SelectArmor"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_ArmorName"),
                DefaultString = objArmor.CustomName,
                AllowEmptyString = true
            })
            {
                await frmPickText.ShowDialogSafeAsync(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objArmor.CustomName = frmPickText.SelectedValue;
            }

            treArmor.SelectedNode.Text = objArmor.CurrentDisplayName;

            IsDirty = true;
        }

        private async void tsLifestyleName_Click(object sender, EventArgs e)
        {
            // Get the information for the currently selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is IHasCustomName objCustomName))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectLifestyleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectLifestyle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // Run through the list of Active Skills and pick out the two applicable ones.
            int intSkillValue = Math.Max(CharacterObject.SkillsSection.GetActiveSkill("Spellcasting")?.Rating ?? 0, CharacterObject.SkillsSection.GetActiveSkill("Ritual Spellcasting")?.Rating ?? 0);

            // The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
            if (CharacterObject.Spells.Count >= 2 * intSkillValue + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SpellLimit) && !CharacterObject.IgnoreRules)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SpellLimit"), await LanguageManager.GetStringAsync("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The character is still allowed to add Spells, so show the Create Spell window.
            using (CreateSpell frmSpell = new CreateSpell(CharacterObject))
            {
                await frmSpell.ShowDialogSafeAsync(this);

                if (frmSpell.DialogResult == DialogResult.Cancel)
                    return;

                Spell objSpell = frmSpell.SelectedSpell;
                CharacterObject.Spells.Add(objSpell);
            }
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

        private async void tsCyberwareAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCyberware.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (!(objSelectedNode.Tag is Cyberware objCyberware) || objCyberware.AllowGear == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        using (XmlNodeList xmlGearCategoryList = objCyberware.AllowGear?.SelectNodes("gearcategory"))
                        {
                            if (xmlGearCategoryList != null)
                            {
                                foreach (XmlNode objXmlCategory in xmlGearCategoryList)
                                    sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                                if (sbdCategories.Length > 0)
                                    --sbdCategories.Length;
                                strCategories = sbdCategories.ToString();
                            }
                        }
                    }

                    string strGearNames = string.Empty;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdGearNames))
                    {
                        using (XmlNodeList xmlGearNameList = objCyberware.AllowGear?.SelectNodes("gearname"))
                        {
                            if (xmlGearNameList?.Count > 0)
                            {
                                foreach (XmlNode objXmlName in xmlGearNameList)
                                    sbdGearNames.Append(objXmlName.InnerText).Append(',');
                                --sbdGearNames.Length;
                                strGearNames = sbdGearNames.ToString();
                            }
                        }
                    }

                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objCyberware, strCategories, strGearNames))
                    {
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objCyberware.Capacity) &&
                            objCyberware.Capacity != "0" && (!objCyberware.Capacity.Contains('[') ||
                                                             objCyberware.Capacity.Contains("/[")))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objNewGear = new Gear(CharacterObject);
                        objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objCyberware.IsModularCurrentlyEquipped);
                        objNewGear.Quantity = frmPickGear.SelectedQty;

                        objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        if (objNewGear.InternalId.IsEmptyGuid())
                            continue;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objNewGear.Cost = '(' + objNewGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objNewGear.Cost = "0";
                        }

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }

                        objCyberware.GearChildren.Add(objNewGear);
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleCyberwareAddGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treVehicles.SelectedNode?.Tag is Cyberware objCyberware))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectCyberware"), await LanguageManager.GetStringAsync("MessageTitle_SelectCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware.AllowGear == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    string strCategories;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategories))
                    {
                        foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                            sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                        if (sbdCategories.Length > 0)
                            --sbdCategories.Length;
                        strCategories = sbdCategories.ToString();
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

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objNewGear = new Gear(CharacterObject);
                        objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);
                        objNewGear.Quantity = frmPickGear.SelectedQty;

                        objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        if (objNewGear.InternalId.IsEmptyGuid())
                            continue;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objNewGear.Cost = '(' + objNewGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objNewGear.Cost = "0";
                        }

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }

                        objCyberware.GearChildren.Add(objNewGear);
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
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            if (!(objSelectedNode.Tag is Gear objSensor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CharacterObject.Cyberware.FindCyberwareGear(objSensor.InternalId, out Cyberware objCyberware);
            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
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
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, (objSensor.Parent as Gear)?.Equipped ?? objCyberware?.IsModularCurrentlyEquipped == true);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
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
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            if (!(objSelectedNode.Tag is Gear objSensor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
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

                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (!(objSelectedNode?.Tag is WeaponAccessory objAccessory) || objAccessory.AllowGear == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            bool blnAddAgain;
            string strCategories;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
            {
                foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                    sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                if (sbdCategories.Length > 0)
                    --sbdCategories.Length;
                strCategories = sbdCategories.ToString();
            }

            do
            {
                using (new CursorWait(this))
                {
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
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objAccessory.Equipped);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        objAccessory.GearChildren.Add(objGear);

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Gear objSensor))
            // Make sure the Gear was found.
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            CharacterObject.Weapons.FindWeaponGear(objSensor.InternalId, out WeaponAccessory objAccessory);

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

            // Open the Gear XML file and locate the selected piece.
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

                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objGear = new Gear(CharacterObject);
                        objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, (objSensor.Parent as Gear)?.Equipped ?? objAccessory?.Equipped == true);

                        if (objGear.InternalId.IsEmptyGuid())
                            continue;

                        objGear.Quantity = frmPickGear.SelectedQty;

                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        objSensor.Children.Add(objGear);

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objWeapon);
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleRenameLocation_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Location objLocation))
                return;

            using (SelectText frmPickText = new SelectText
            {
                Description = await LanguageManager.GetStringAsync("String_AddLocation")
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
            // Make sure the Gear was found.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ModifyVehicleGear"), await LanguageManager.GetStringAsync("MessageTitle_SelectGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
                        if (!string.IsNullOrEmpty(strCategories) && !string.IsNullOrEmpty(objSensor.Capacity)
                                                                 && (!objSensor.Capacity.Contains('[')
                                                                     || objSensor.Capacity.Contains("/[")))
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

                        objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objGear.Cost = "0";
                        }

                        objSensor.Children.Add(objGear);

                        // Create any Weapons that came with this Gear.
                        if (lstWeapons.Count > 0)
                        {
                            CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory _, out Cyberware _);
                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                objVehicle.Weapons.Add(objWeapon);
                            }
                        }
                    }
                }
            } while (blnAddAgain);
        }

        private async void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (!(objSelectedNode?.Tag is WeaponAccessory objAccessory) || objAccessory.AllowGear == null)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_WeaponGear"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGear"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("gear.xml");
            string strCategories;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCategories))
            {
                foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                    sbdCategories.Append(objXmlCategory.InnerText).Append(',');
                if (sbdCategories.Length > 0)
                    --sbdCategories.Length;
                strCategories = sbdCategories.ToString();
            }

            bool blnAddAgain;

            do
            {
                using (new CursorWait(this))
                {
                    using (SelectGear frmPickGear = new SelectGear(CharacterObject, 0, 1, objAccessory, strCategories))
                    {
                        if (!string.IsNullOrEmpty(strCategories))
                            frmPickGear.ShowNegativeCapacityOnly = true;
                        await frmPickGear.ShowDialogSafeAsync(this);

                        if (frmPickGear.DialogResult == DialogResult.Cancel)
                            break;
                        blnAddAgain = frmPickGear.AddAgain;

                        // Open the Gear XML file and locate the selected piece.
                        XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = " + frmPickGear.SelectedGear.CleanXPath() + ']');

                        // Create the new piece of Gear.
                        List<Weapon> lstWeapons = new List<Weapon>(1);

                        Gear objNewGear = new Gear(CharacterObject);
                        objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                        if (objNewGear.InternalId.IsEmptyGuid())
                            continue;

                        objNewGear.Quantity = frmPickGear.SelectedQty;

                        objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                        // Reduce the cost for Do It Yourself components.
                        if (frmPickGear.DoItYourself)
                            objNewGear.Cost = '(' + objNewGear.Cost + ") * 0.5";
                        // If the item was marked as free, change its cost.
                        if (frmPickGear.FreeCost)
                        {
                            objNewGear.Cost = "0";
                        }

                        objAccessory.GearChildren.Add(objNewGear);

                        // Create any Weapons that came with this Gear.
                        foreach (Weapon objLoopWeapon in lstWeapons)
                        {
                            objAccessory.Parent.Children.Add(objLoopWeapon);
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

        private void UpdateQualityLevelValue(Quality objSelectedQuality = null)
        {
            if (objSelectedQuality == null
                || objSelectedQuality.OriginSource == QualitySource.Improvement
                || objSelectedQuality.OriginSource == QualitySource.Metatype
                || objSelectedQuality.OriginSource == QualitySource.Heritage
                || objSelectedQuality.Levels == 0)
            {
                nudQualityLevel.Value = 1;
                nudQualityLevel.Enabled = false;
                return;
            }
            XPathNavigator objQualityNode = objSelectedQuality.GetNodeXPath();
            string strLimitString = objQualityNode != null
                ? objQualityNode.SelectSingleNodeAndCacheExpression("chargenlimit")?.Value
                  ?? objQualityNode.SelectSingleNodeAndCacheExpression("limit")?.Value
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(strLimitString) && objQualityNode?.SelectSingleNodeAndCacheExpression("nolevels") == null && int.TryParse(strLimitString, out int intMaxRating))
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

        private bool _blnSkipQualityLevelChanged;

        private void nudQualityLevel_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipQualityLevelChanged)
                return;
            // Locate the selected Quality.
            if (!(treQualities.SelectedNode?.Tag is Quality objSelectedQuality))
                return;
            int intCurrentLevels = objSelectedQuality.Levels;

            // Adding new levels
            for (; nudQualityLevel.Value > intCurrentLevels; ++intCurrentLevels)
            {
                if (!objSelectedQuality.GetNodeXPath().RequirementsMet(CharacterObject, LanguageManager.GetString("String_Quality")))
                {
                    UpdateQualityLevelValue(objSelectedQuality);
                    break;
                }
                List<Weapon> lstWeapons = new List<Weapon>(1);
                Quality objQuality = new Quality(CharacterObject);

                objQuality.Create(objSelectedQuality.GetNode(), QualitySource.Selected, lstWeapons, objSelectedQuality.Extra);
                if (objQuality.InternalId.IsEmptyGuid())
                {
                    // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                    UpdateQualityLevelValue(objSelectedQuality);
                    break;
                }

                objQuality.BP = objSelectedQuality.BP;
                objQuality.ContributeToLimit = objSelectedQuality.ContributeToLimit;

                // Make sure that adding the Quality would not cause the character to exceed their BP limits.
                bool blnAddItem = true;
                if (objQuality.ContributeToLimit && !CharacterObject.IgnoreRules)
                {
                    // If the item being checked would cause the limit of 25 BP spent on Positive Qualities to be exceed, do not let it be checked and display a message.
                    string strAmount = CharacterObjectSettings.QualityKarmaLimit.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
                    int intMaxQualityAmount = CharacterObjectSettings.QualityKarmaLimit;

                    // Add the cost of the Quality that is being added.
                    int intBP = objQuality.BP;

                    if (objQuality.Type == QualityType.Negative)
                    {
                        // Check if adding this Quality would put the character over their limit.
                        if (!CharacterObjectSettings.ExceedNegativeQualities)
                        {
                            intBP += CharacterObject.NegativeQualityLimitKarma;
                            if (intBP < intMaxQualityAmount * -1)
                            {
                                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_NegativeQualityLimit"), strAmount),
                                    LanguageManager.GetString("MessageTitle_NegativeQualityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
                            else if (CharacterObject.MetatypeBP < 0 && intBP + CharacterObject.MetatypeBP < intMaxQualityAmount * -1)
                            {
                                Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_NegativeQualityAndMetatypeLimit"), strAmount),
                                    LanguageManager.GetString("MessageTitle_NegativeQualityLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
                        }
                    }
                    // Check if adding this Quality would put the character over their limit.
                    else if (!CharacterObjectSettings.ExceedPositiveQualities)
                    {
                        intBP += CharacterObject.PositiveQualityKarma;
                        if (intBP > intMaxQualityAmount)
                        {
                            Program.ShowMessageBox(this,
                                string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_PositiveQualityLimit"), strAmount),
                                LanguageManager.GetString("MessageTitle_PositiveQualityLimit"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            blnAddItem = false;
                        }
                    }
                }

                if (blnAddItem)
                {
                    //to avoid an System.InvalidOperationException: Cannot change ObservableCollection during a CollectionChanged event.
                    _blnSkipQualityLevelChanged = true;
                    CharacterObject.Qualities.Add(objQuality);
                    _blnSkipQualityLevelChanged = false;

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }
                }
                else
                {
                    // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                    UpdateQualityLevelValue(objSelectedQuality);
                    break;
                }
            }
            // Removing levels
            for (; nudQualityLevel.Value < intCurrentLevels; --intCurrentLevels)
            {
                Quality objInvisibleQuality = CharacterObject.Qualities.FirstOrDefault(x => x.SourceIDString == objSelectedQuality.SourceIDString && x.Extra == objSelectedQuality.Extra && x.SourceName == objSelectedQuality.SourceName && x.InternalId != objSelectedQuality.InternalId);
                if (objInvisibleQuality == null || !RemoveQuality(objInvisibleQuality, false, false))
                {
                    if (!RemoveQuality(objSelectedQuality, false, false))
                        UpdateQualityLevelValue(objSelectedQuality);
                    break;
                }
            }
        }

        #endregion Additional Common Tab Control Events

        #region Additional Cyberware Tab Control Events

        private void treCyberware_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedCyberware();
            RefreshPasteStatus(sender, e);
        }

        private void cboCyberwareGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || IsLoading)
                return;
            string strSelectedGrade = cboCyberwareGrade.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedGrade) ||
                !(treCyberware.SelectedNode?.Tag is Cyberware objCyberware))
                return;
            // Locate the selected piece of Cyberware.
            Grade objNewGrade = CharacterObject.GetGradeList(objCyberware.SourceType).FirstOrDefault(x => x.Name == strSelectedGrade);
            if (objNewGrade == null)
                return;
            // Updated the selected Cyberware Grade.
            objCyberware.Grade = objNewGrade;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkPrototypeTranshuman_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !(treCyberware.SelectedNode?.Tag is Cyberware objCyberware))
                return;
            // Update the selected Cyberware Rating.
            objCyberware.PrototypeTranshuman = chkPrototypeTranshuman.Checked;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void nudCyberwareRating_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            switch (treCyberware.SelectedNode?.Tag)
            {
                // Locate the selected piece of Cyberware.
                case Cyberware objCyberware:
                    {
                        // Update the selected Cyberware Rating.
                        objCyberware.Rating = nudCyberwareRating.ValueAsInt;

                        // See if a Bonus node exists.
                        if (objCyberware.Bonus?.InnerXml.Contains("Rating") == true || objCyberware.PairBonus?.InnerXml.Contains("Rating") == true || objCyberware.WirelessOn && objCyberware.WirelessBonus?.InnerXml.Contains("Rating") == true)
                        {
                            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                            ImprovementManager.RemoveImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId);
                            if (objCyberware.Bonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.Bonus, objCyberware.Rating, objCyberware.CurrentDisplayNameShort);
                            if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.WirelessBonus, objCyberware.Rating, objCyberware.CurrentDisplayNameShort);

                            if (objCyberware.PairBonus != null)
                            {
                                List<Cyberware> lstPairableCyberwares = CharacterObject.Cyberware.DeepWhere(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped).ToList();
                                int intCyberwaresCount = lstPairableCyberwares.Count;
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
                                foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                                {
                                    ImprovementManager.RemoveImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair");
                                    // Go down the list and create pair bonuses for every second item
                                    if (intCyberwaresCount > 0 && (intCyberwaresCount & 1) == 0)
                                    {
                                        ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating, objLoopCyberware.CurrentDisplayNameShort);
                                    }
                                    --intCyberwaresCount;
                                }
                            }

                            if (!objCyberware.IsModularCurrentlyEquipped)
                                objCyberware.ChangeModularEquip(false);
                        }

                        treCyberware.SelectedNode.Text = objCyberware.CurrentDisplayName;
                        break;
                    }
                case Gear objGear:
                    {
                        // Find the selected piece of Gear.
                        if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                        {
                            if (!objGear.RefreshSingleFocusRating(treFoci, nudCyberwareRating.ValueAsInt))
                            {
                                IsRefreshing = true;
                                nudCyberwareRating.Value = objGear.Rating;
                                IsRefreshing = false;
                                return;
                            }
                        }
                        else
                            objGear.Rating = nudCyberwareRating.ValueAsInt;

                        // See if a Bonus node exists.
                        if (objGear.Bonus != null || objGear.WirelessOn && objGear.WirelessBonus != null)
                        {
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                            if (!string.IsNullOrEmpty(objGear.Extra))
                            {
                                ImprovementManager.ForcedValue = objGear.Extra.TrimEndOnce(", Hacked");
                            }
                            if (objGear.Bonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, objGear.Rating, objGear.CurrentDisplayNameShort);
                            if (objGear.WirelessOn && objGear.WirelessBonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, objGear.Rating, objGear.CurrentDisplayNameShort);

                            if (!objGear.Equipped)
                                objGear.ChangeEquippedStatus(false);
                        }

                        treCyberware.SelectedNode.Text = objGear.CurrentDisplayName;
                        break;
                    }
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        #endregion Additional Cyberware Tab Control Events

        #region Additional Street Gear Tab Control Events

        private void treWeapons_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedWeapon();
            RefreshPasteStatus(sender, e);
        }

        private void treWeapons_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strSelectedWeapon = treWeapons.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedWeapon) || treWeapons.SelectedNode.Level != 1)
                return;

            // Do not allow the root element to be moved.
            if (strSelectedWeapon == "Node_SelectedWeapons")
                return;
            _intDragLevel = treWeapons.SelectedNode.Level;
            DoDragDrop(e.Item, DragDropEffects.Move);
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

            int intNewIndex;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
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

            // Store our new order so it's loaded properly the next time we open the character
            treWeapons.CacheSortOrder();

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

            // Clear the background colour for all other Nodes.
            treWeapons.ClearNodeBackground(objNode);
        }

        private void treArmor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedArmor();
            RefreshPasteStatus(sender, e);
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

            int intNewIndex;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
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

            // Store our new order so it's loaded properly the next time we open the character
            treArmor.CacheSortOrder();

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

            // Clear the background colour for all other Nodes.
            treArmor.ClearNodeBackground(objNode);
        }

        private void treLifestyles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedLifestyle();
            RefreshPasteStatus(sender, e);
        }

        private async void treLifestyles_DoubleClick(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            string strGuid = objLifestyle.InternalId;
            int intMonths = objLifestyle.Increments;
            int intPosition = CharacterObject.Lifestyles.IndexOf(CharacterObject.Lifestyles.FirstOrDefault(p => p.InternalId == objLifestyle.InternalId));

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

            objLifestyle.SetInternalId(strGuid);
            CharacterObject.Lifestyles[intPosition] = objLifestyle;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        /*
        private void treLifestyles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level != 1)
                return;

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
            else
            {
                intNewIndex = treLifestyles.Nodes[treLifestyles.Nodes.Count - 1].Nodes.Count;
                nodDestination = treLifestyles.Nodes[treLifestyles.Nodes.Count - 1];
            }

            // Put the lifestyle in the right location (or lack thereof)
            CommonFunctions.MoveLifestyleNode(CharacterObject, intNewIndex, nodDestination, treLifestyles);

            // Put the lifestyle in the right order in the tree
            MoveTreeNode(treLifestyles.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treLifestyles.CacheSortOrder();

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

            // Clear the background colour for all other Nodes.
            treLifestyles.ClearNodeBackground(objNode);
        }

        private void nudLifestyleMonths_ValueChanged(object sender, EventArgs e)
        {
            if (!(treLifestyles.SelectedNode?.Level > 0))
                return;
            IsRefreshing = true;

            // Locate the selected Lifestyle.
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            objLifestyle.Increments = nudLifestyleMonths.ValueAsInt;

            IsRefreshing = false;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void treGear_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedGear();
            RefreshPasteStatus(sender, e);
        }

        private void nudGearRating_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treGear?.SelectedNode == null)
                return;

            if (treGear.SelectedNode.Level <= 0)
                return;
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
                return;

            if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
            {
                if (!objGear.RefreshSingleFocusRating(treFoci, nudGearRating.ValueAsInt))
                {
                    IsRefreshing = true;
                    nudGearRating.Value = objGear.Rating;
                    IsRefreshing = false;
                    return;
                }
            }
            else
                objGear.Rating = nudGearRating.ValueAsInt;
            if (objGear.Bonus != null || objGear.WirelessOn && objGear.WirelessBonus != null)
            {
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                if (!string.IsNullOrEmpty(objGear.Extra))
                {
                    ImprovementManager.ForcedValue = objGear.Extra.TrimEndOnce(", Hacked");
                }
                if (objGear.Bonded || (objGear.Category != "Foci" && objGear.Category != "Metamagic Foci" && objGear.Category != "Stacked Focus"))
                {
                    if (objGear.Bonus != null)
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, objGear.Rating, objGear.CurrentDisplayNameShort);
                    if (objGear.WirelessOn && objGear.WirelessBonus != null)
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, objGear.Rating, objGear.CurrentDisplayNameShort);
                }

                if (!objGear.Equipped)
                    objGear.ChangeEquippedStatus(false);
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treGear.SelectedNode == null)
                return;
            // Attempt to locate the selected piece of Gear.
            if (treGear.SelectedNode.Level <= 0 || !(treGear.SelectedNode?.Tag is Gear objGear))
                return;
            objGear.Quantity = nudGearQty.Value;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void nudDrugQty_ValueChanged(object sender, EventArgs e)
        {
            // Don't attempt to do anything while the data is still being populated.
            if (IsLoading || IsRefreshing)
                return;

            if (!(treCustomDrugs.SelectedNode?.Tag is Drug objDrug))
                return;
            objDrug.Quantity = Convert.ToInt32(nudDrugQty.Value);
            RefreshSelectedDrug();

            IsCharacterUpdateRequested = true;
            IsDirty = true;
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

        private void chkWeaponEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
                return;
            switch (treWeapons.SelectedNode?.Tag)
            {
                // Locate the selected Weapon Accessory or Modification.
                case WeaponAccessory objAccessory:
                    objAccessory.Equipped = chkWeaponEquipped.Checked;
                    break;

                case Weapon objWeapon:
                    objWeapon.Equipped = chkWeaponEquipped.Checked;
                    break;

                case Gear objGear:
                    objGear.Equipped = chkWeaponEquipped.Checked;
                    objGear.ChangeEquippedStatus(chkWeaponEquipped.Checked);
                    break;

                default:
                    return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkIncludedInWeapon_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
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

            int intNewIndex;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
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
                    CharacterObject.MoveGearParent(nodDestination, objSelected);
                    break;
            }

            // Put the gear in the right order in the tree
            MoveTreeNode(treGear.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treGear.CacheSortOrder();

            // Clear the background color for all Nodes.
            treGear.ClearNodeBackground(null);

            // Store our new order so it's loaded properly the next time we open the character
            treGear.CacheSortOrder();

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
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
                return;
            objGear.Equipped = chkGearEquipped.Checked;
            objGear.ChangeEquippedStatus(chkGearEquipped.Checked);

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkGearHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treGear.SelectedNode == null)
                return;
            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);
        }

        private void chkArmorHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treArmor.SelectedNode == null)
                return;
            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.SetHomeNode(CharacterObject, chkArmorHomeNode.Checked);
        }

        private void chkWeaponHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
                return;
            if (!(treWeapons.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.SetHomeNode(CharacterObject, chkWeaponHomeNode.Checked);
        }

        private void chkCyberwareHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objCommlink))
                return;
            objCommlink.SetHomeNode(CharacterObject, chkCyberwareHomeNode.Checked);
        }

        private void chkCommlinks_CheckedChanged(object sender, EventArgs e)
        {
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
        }

        private void chkGearActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treGear.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            if (!(treGear.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkGearActiveCommlink.Checked);
        }

        private void chkArmorActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treArmor.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            if (!(treArmor.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink))
                return;
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkArmorActiveCommlink.Checked);
        }

        private void chkWeaponActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
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
            objSelectedCommlink.SetActiveCommlink(CharacterObject, chkVehicleActiveCommlink.Checked);
        }

        private void cboGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearAttack.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
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
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
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
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
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
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleAttack.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleAttack, cboVehicleAttack,
                    cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleSleaze.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleAttack, cboVehicleAttack,
                    cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleFirewall.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleAttack, cboVehicleAttack,
                    cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        private void cboVehicleDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleDataProcessing.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget
                && objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboVehicleAttack, cboVehicleAttack,
                    cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
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

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareAttack, cboCyberwareAttack,
                cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareSleaze, cboCyberwareAttack,
                cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareDataProcessing, cboCyberwareAttack,
                cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeComboBoxChange(CharacterObject, cboCyberwareFirewall, cboCyberwareAttack,
                cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }

        #endregion Additional Street Gear Tab Control Events

        #region Additional Drug Tab Control Events

        private void treCustomDrugs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedDrug();
            RefreshPasteStatus(sender, e);
        }

        #endregion Additional Drug Tab Control Events

        #region Additional Vehicle Tab Control Events

        private void treVehicles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedVehicle();
            RefreshPasteStatus(sender, e);
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

            int intNewIndex;
            if (nodDestination != null)
            {
                intNewIndex = nodDestination.Index;
            }
            else
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

            // Put the vehicle in the right order in the tree
            MoveTreeNode(treVehicles.FindNodeByTag(objSelected?.Tag), intNewIndex);
            // Update the entire tree to prevent any holes in the sort order
            treVehicles.CacheSortOrder();

            // Clear the background color for all Nodes.
            treVehicles.ClearNodeBackground(null);

            // Store our new order so it's loaded properly the next time we open the character
            treWeapons.CacheSortOrder();

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

        private void nudVehicleRating_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            switch (treVehicles.SelectedNode?.Tag)
            {
                case VehicleMod objMod:
                    objMod.Rating = nudVehicleRating.ValueAsInt;
                    treVehicles.SelectedNode.Text = objMod.CurrentDisplayName;
                    break;

                case Gear objGear:
                    {
                        if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                        {
                            if (!objGear.RefreshSingleFocusRating(treFoci, nudVehicleRating.ValueAsInt))
                            {
                                IsRefreshing = true;
                                nudVehicleRating.Value = objGear.Rating;
                                IsRefreshing = false;
                                return;
                            }
                        }
                        else
                            objGear.Rating = nudVehicleRating.ValueAsInt;
                        treVehicles.SelectedNode.Text = objGear.CurrentDisplayName;
                        break;
                    }
                case WeaponAccessory objAccessory:
                    objAccessory.Rating = nudVehicleRating.ValueAsInt;
                    treVehicles.SelectedNode.Text = objAccessory.CurrentDisplayName;
                    break;

                case Cyberware objCyberware:
                    objCyberware.Rating = nudVehicleRating.ValueAsInt;
                    treVehicles.SelectedNode.Text = objCyberware.CurrentDisplayName;
                    break;

                default:
                    return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            switch (treVehicles.SelectedNode?.Tag)
            {
                case WeaponAccessory objAccessory:
                    objAccessory.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
                    break;

                case Weapon objWeapon:
                    objWeapon.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
                    break;

                case VehicleMod objMod:
                    objMod.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
                    break;

                case WeaponMount objWeaponMount:
                    objWeaponMount.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
                    break;

                default:
                    return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void nudVehicleGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (!(treVehicles.SelectedNode?.Tag is Gear objGear))
                return;
            objGear.Quantity = nudVehicleGearQty.Value;
            treVehicles.SelectedNode.Text = objGear.CurrentDisplayName;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget))
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

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void treFoci_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            // Don't bother to do anything since a node is being unchecked.
            if (e.Node.Checked)
                return;

            string strSelectedId = (e.Node?.Tag as IHasInternalId)?.InternalId ?? string.Empty;

            // Locate the Focus that is being touched.
            Gear objSelectedFocus = CharacterObject.Gear.DeepFindById(strSelectedId);

            // Set the Focus count to 1 and get its current Rating (Force). This number isn't used in the following loops because it isn't yet checked or unchecked.
            int intFociCount = 1;
            int intFociTotal;

            if (objSelectedFocus != null)
                intFociTotal = objSelectedFocus.Rating;
            else
            {
                // This is a Stacked Focus.
                intFociTotal = CharacterObject.StackedFoci.Find(x => x.InternalId == strSelectedId)?.TotalForce ?? 0;
            }

            // Run through the list of items. Count the number of Foci the character would have bonded including this one, plus the total Force of all checked Foci.
            foreach (TreeNode objNode in treFoci.Nodes)
            {
                if (!objNode.Checked)
                    continue;
                string strNodeId = objNode.Tag.ToString();
                ++intFociCount;
                intFociTotal += CharacterObject.Gear.FirstOrDefault(x => x.InternalId == strNodeId && x.Bonded)?.Rating ?? 0;
                intFociTotal += CharacterObject.StackedFoci.Find(x => x.InternalId == strNodeId && x.Bonded)?.TotalForce ?? 0;
            }

            if (!CharacterObject.IgnoreRules)
            {
                if (intFociTotal > CharacterObject.MAG.TotalValue * 5 ||
                    CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.InitiateGrade + 1 > CharacterObject.MAGAdept.TotalValue)
                {
                    Program.ShowMessageBox(this, LanguageManager.GetString("Message_FocusMaximumForce"), LanguageManager.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }

                if (intFociCount > CharacterObject.MAG.TotalValue ||
                    CharacterObjectSettings.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && intFociCount > CharacterObject.MAGAdept.TotalValue)
                {
                    Program.ShowMessageBox(this, LanguageManager.GetString("Message_FocusMaximumNumber"), LanguageManager.GetString("MessageTitle_FocusMaximum"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }
            }

            if (objSelectedFocus != null)
            {
                Focus objFocus = new Focus(CharacterObject)
                {
                    GearObject = objSelectedFocus
                };

                if (objSelectedFocus.Equipped && (objSelectedFocus.Bonus != null || objSelectedFocus.WirelessOn && objSelectedFocus.WirelessBonus != null))
                {
                    if (!string.IsNullOrEmpty(objSelectedFocus.Extra))
                        ImprovementManager.ForcedValue = objSelectedFocus.Extra;
                    if (objSelectedFocus.Bonus != null)
                    {
                        if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.Bonus, objSelectedFocus.Rating, objSelectedFocus.CurrentDisplayNameShort))
                        {
                            // Clear created improvements
                            objSelectedFocus.ChangeEquippedStatus(false);
                            objSelectedFocus.ChangeEquippedStatus(true);
                            e.Cancel = true;
                            return;
                        }
                        objSelectedFocus.Extra = ImprovementManager.SelectedValue;
                    }
                    if (objSelectedFocus.WirelessOn
                        && objSelectedFocus.WirelessBonus != null
                        && !ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objSelectedFocus.InternalId, objSelectedFocus.WirelessBonus, objSelectedFocus.Rating, objSelectedFocus.CurrentDisplayNameShort))
                    {
                        // Clear created improvements
                        objSelectedFocus.ChangeEquippedStatus(false);
                        objSelectedFocus.ChangeEquippedStatus(true);
                        e.Cancel = true;
                        return;
                    }
                }

                e.Node.Text = objSelectedFocus.CurrentDisplayName;
                CharacterObject.Foci.Add(objFocus);
                objSelectedFocus.Bonded = true;
            }
            else
            {
                // This is a Stacked Focus.
                StackedFocus objStack = CharacterObject.StackedFoci.Find(x => x.InternalId == strSelectedId);
                if (objStack != null)
                {
                    Gear objStackGear = CharacterObject.Gear.DeepFindById(objStack.GearId);
                    if (objStackGear.Equipped)
                    {
                        foreach (Gear objGear in objStack.Gear)
                        {
                            if (objGear.Bonus == null && (!objGear.WirelessOn || objGear.WirelessBonus == null))
                                continue;
                            if (!string.IsNullOrEmpty(objGear.Extra))
                                ImprovementManager.ForcedValue = objGear.Extra;
                            if (objGear.Bonus != null)
                            {
                                if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objGear.Bonus, objGear.Rating, objGear.CurrentDisplayNameShort))
                                {
                                    // Clear created improvements
                                    objStackGear.ChangeEquippedStatus(false);
                                    objStackGear.ChangeEquippedStatus(true);
                                    e.Cancel = true;
                                    return;
                                }
                                objGear.Extra = ImprovementManager.SelectedValue;
                            }
                            if (objGear.WirelessOn
                                && objGear.WirelessBonus != null
                                && !ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objGear.WirelessBonus, objGear.Rating, objGear.CurrentDisplayNameShort))
                            {
                                // Clear created improvements
                                objStackGear.ChangeEquippedStatus(false);
                                objStackGear.ChangeEquippedStatus(true);
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                    objStack.Bonded = true;
                    treFoci.SelectedNode.Text = objStackGear.CurrentDisplayName;
                }
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void nudArmorRating_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            switch (treArmor.SelectedNode?.Tag)
            {
                // Locate the selected ArmorMod.
                case ArmorMod objMod:
                    {
                        objMod.Rating = nudArmorRating.ValueAsInt;
                        treArmor.SelectedNode.Text = objMod.CurrentDisplayName;

                        // See if a Bonus node exists.
                        if (objMod.Bonus?.InnerXml.Contains("Rating") == true || objMod.WirelessOn && objMod.WirelessBonus?.InnerXml.Contains("Rating") == true)
                        {
                            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                            if (objMod.Bonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.Bonus, objMod.Rating, objMod.CurrentDisplayNameShort);
                            if (objMod.WirelessOn && objMod.WirelessBonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.WirelessBonus, objMod.Rating, objMod.CurrentDisplayNameShort);
                        }

                        break;
                    }
                case Gear objGear:
                    {
                        if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                        {
                            if (!objGear.RefreshSingleFocusRating(treFoci, nudArmorRating.ValueAsInt))
                            {
                                IsRefreshing = true;
                                nudArmorRating.Value = objGear.Rating;
                                IsRefreshing = false;
                                return;
                            }
                        }
                        else
                            objGear.Rating = nudArmorRating.ValueAsInt;
                        treArmor.SelectedNode.Text = objGear.CurrentDisplayName;

                        // See if a Bonus node exists.
                        if (objGear.Bonus?.InnerXml.Contains("Rating") == true || objGear.WirelessOn && objGear.WirelessBonus?.InnerXml.Contains("Rating") == true)
                        {
                            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                            if (objGear.Bonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, objGear.Rating, objGear.CurrentDisplayNameShort);
                            if (objGear.WirelessOn && objGear.WirelessBonus != null)
                                ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, objGear.Rating, objGear.CurrentDisplayNameShort);

                            if (!objGear.Equipped)
                                objGear.ChangeEquippedStatus(false);
                        }

                        break;
                    }
                case Armor objArmor:
                    objArmor.Rating = nudArmorRating.ValueAsInt;
                    treArmor.SelectedNode.Text = objArmor.CurrentDisplayName;
                    break;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cboTradition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || CharacterObject.MagicTradition.Type == TraditionType.RES)
                return;
            string strSelectedId = cboTradition.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            XmlNode xmlTradition = CharacterObject.LoadData("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[id = " + strSelectedId.CleanXPath() + ']');

            if (xmlTradition == null)
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
                    cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID.ToString();
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
            if (IsLoading || IsRefreshing || CharacterObject.MagicTradition.Type == TraditionType.MAG)
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

        private void treComplexForms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cmdDeleteComplexForm_Click(sender, e);
            }
        }

        #endregion Additional Sprites and Complex Forms Tab Control Events

        #region Additional AI Advanced Programs Tab Control Events

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

        #endregion Additional AI Advanced Programs Tab Control Events

        #region Additional Initiation Tab Control Events

        private void chkInitiationGroup_EnabledChanged(object sender, EventArgs e)
        {
            if (!chkInitiationGroup.Enabled)
            {
                chkInitiationGroup.Checked = false;
            }
        }

        private void chkInitiationSchooling_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkInitiationSchooling_EnabledChanged(object sender, EventArgs e)
        {
            if (!chkInitiationSchooling.Enabled)
            {
                chkInitiationSchooling.Checked = false;
            }
        }

        private void treMetamagic_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch (treMetamagic.SelectedNode?.Tag)
            {
                case Metamagic objMetamagic:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objMetamagic.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objMetamagic.Grade >= 0;
                        objMetamagic.SetSourceDetail(lblMetamagicSource);
                        break;
                    }
                case Art objArt:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objArt.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objArt.Grade >= 0;
                        objArt.SetSourceDetail(lblMetamagicSource);
                        break;
                    }
                case Spell objSpell:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveMetamagic");
                        cmdDeleteMetamagic.Enabled = objSpell.Grade >= 0;
                        objSpell.SetSourceDetail(lblMetamagicSource);
                        break;
                    }
                case ComplexForm objComplexForm:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objComplexForm.Grade >= 0;
                        objComplexForm.SetSourceDetail(lblMetamagicSource);
                        break;
                    }
                case Enhancement objEnhancement:
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objEnhancement.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho");
                        cmdDeleteMetamagic.Enabled = objEnhancement.Grade >= 0;
                        objEnhancement.SetSourceDetail(lblMetamagicSource);
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

        private void txtGroupNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.A)
                return;
            e.SuppressKeyPress = true;
            ((TextBox)sender)?.SelectAll();
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
            if (!(treCritterPowers.SelectedNode?.Tag is CritterPower objPower))
                return;
            objPower.CountTowardsLimit = chkCritterPowerCount.Checked;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        #endregion Additional Critter Powers Tab Control Events

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

        private async void treVehicles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                await DeleteVehicle();
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

        #endregion Tree KeyDown Events

        #region Other Control Events

        private void tabCharacterTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus(sender, e);
        }

        private void tabStreetGearTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus(sender, e);
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

            if (op != CmdOperation.Up && op != CmdOperation.Down)
                return base.ProcessCmdKey(ref msg, keyData);
            bool up = op == CmdOperation.Up;
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

            if (treActiveView == null)
                return true;
            TreeNode objSelectedNode = treActiveView.SelectedNode;
            TreeNode objParentNode = objSelectedNode?.Parent;
            TreeNodeCollection lstNodes = objParentNode?.Nodes ?? treActiveView.Nodes;

            if (requireParentSortable && !(objParentNode?.Tag is ICanSort))
                return true;
            int intNewIndex = lstNodes.IndexOf(objSelectedNode);
            intNewIndex = up ? Math.Max(0, intNewIndex - 1) : Math.Min(lstNodes.Count - 1, intNewIndex + 1);

            MoveTreeNode(objSelectedNode, intNewIndex);

            // Returning true tells the program to consume the input
            return true;

            // If none of our key combinations are used then use the default logic
        }

        #endregion Other Control Events

        #region Custom Methods

        /// <summary>
        /// Calculate the BP used by Primary Attributes.
        /// </summary>
        private static int CalculateAttributeBP(IEnumerable<CharacterAttrib> attribs, IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            // Primary and Special Attributes are calculated separately since you can only spend a maximum of 1/2 your BP allotment on Primary Attributes.
            // Special Attributes are not subject to the 1/2 of max BP rule.
            int intBP = attribs.Sum(att => att.TotalKarmaCost);
            if (extraAttribs != null)
            {
                intBP += extraAttribs.Sum(att => att.TotalKarmaCost);
            }
            return intBP;
        }

        private int CalculateAttributePriorityPoints(IEnumerable<CharacterAttrib> attribs, IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            int intAtt = 0;
            if (CharacterObject.EffectiveBuildMethodUsesPriorityTables)
            {
                // Get the total of "free points" spent
                intAtt += attribs.Sum(att => att.SpentPriorityPoints);
                if (extraAttribs != null)
                {
                    // Get the total of "free points" spent
                    intAtt += extraAttribs.Sum(att => att.SpentPriorityPoints);
                }
            }
            return intAtt;
        }

        private string BuildAttributes(ICollection<CharacterAttrib> attribs, ICollection<CharacterAttrib> extraAttribs = null, bool special = false)
        {
            int bp = CalculateAttributeBP(attribs, extraAttribs);
            string s = bp.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
            if (CharacterObject.EffectiveBuildMethodUsesPriorityTables)
            {
                int att = CalculateAttributePriorityPoints(attribs, extraAttribs);
                int total = special ? CharacterObject.TotalSpecial : CharacterObject.TotalAttributes;
                if (bp > 0)
                {
                    s = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_OverPriorityPoints"),
                        total - att, total, bp);
                }
                else
                {
                    s = (total - att).ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Of") + total.ToString(GlobalSettings.CultureInfo);
                }
            }
            return s;
        }

        /// <summary>
        /// Calculate the number of Build Points the character has remaining.
        /// </summary>
        private int CalculateBP(bool blnDoUIUpdate = true)
        {
            int intKarmaPointsRemain = CharacterObjectSettings.BuildKarma;
            //int intPointsUsed = 0; // used as a running total for each section
            const int intFreestyleBPMin = 0;
            int intFreestyleBP = 0;
            string strSpace = LanguageManager.GetString("String_Space");
            string strPoints = blnDoUIUpdate ? LanguageManager.GetString("String_Karma") : string.Empty;

            // ------------------------------------------------------------------------------
            // Metatype/Metavariant only cost points when working with BP (or when the Metatype Costs Karma option is enabled when working with Karma).
            if (!CharacterObject.EffectiveBuildMethodUsesPriorityTables)
            {
                // Subtract the BP used for Metatype.
                intKarmaPointsRemain -= CharacterObject.MetatypeBP * CharacterObjectSettings.MetatypeCostsKarmaMultiplier;
            }
            else
            {
                intKarmaPointsRemain -= CharacterObject.MetatypeBP;
            }

            // ------------------------------------------------------------------------------
            // Calculate the points used by Contacts.
            int intPointsInContacts = 0;

            int intContactPoints = CharacterObject.ContactPoints;
            int intContactPointsLeft = intContactPoints;
            int intHighPlacesFriends = 0;
            foreach (Contact objContact in CharacterObject.Contacts)
            {
                // Don't care about free contacts
                if (objContact.EntityType != ContactType.Contact || objContact.Free)
                    continue;

                if (objContact.Connection >= 8 && CharacterObject.FriendsInHighPlaces)
                {
                    intHighPlacesFriends += objContact.Connection + objContact.Loyalty;
                }
                else if (!objContact.IsGroup)
                {
                    int over = intContactPointsLeft - objContact.ContactPoints;

                    //Prefers to eat 0, we went over
                    if (over < 0)
                    {
                        //over is negative so to add we substract
                        //instead of +abs(over)
                        intPointsInContacts -= over;
                        intContactPointsLeft = 0; //we went over so we know none are left
                    }
                    else
                    {
                        //otherwise just set;
                        intContactPointsLeft = over;
                    }
                }
            }

            CharacterObject.ContactPointsUsed = intContactPointsLeft;

            if (intPointsInContacts > 0 || CharacterObject.CHA.Value * 4 < intHighPlacesFriends)
            {
                intPointsInContacts += Math.Max(0, intHighPlacesFriends - CharacterObject.CHA.Value * 4);
            }

            intKarmaPointsRemain -= intPointsInContacts;
            // ------------------------------------------------------------------------------
            // Calculate the BP used by Qualities.
            int intLifeModuleQualities = 0;

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdPositiveQualityTooltip))
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdNegativeQualityTooltip))
            {
                foreach (Quality objLoopQuality in CharacterObject.Qualities.Where(q => q.ContributeToBP))
                {
                    if (objLoopQuality.Type == QualityType.LifeModule)
                    {
                        intLifeModuleQualities += objLoopQuality.BP * CharacterObjectSettings.KarmaQuality;
                        if (blnDoUIUpdate)
                        {
                            sbdPositiveQualityTooltip.AppendFormat(
                                GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                                strSpace, objLoopQuality.BP * CharacterObjectSettings.KarmaQuality).AppendLine();
                        }
                    }
                    else if (blnDoUIUpdate)
                    {
                        switch (objLoopQuality.Type)
                        {
                            case QualityType.Positive:
                                sbdPositiveQualityTooltip.AppendFormat(
                                    GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                                    strSpace, objLoopQuality.BP * CharacterObjectSettings.KarmaQuality).AppendLine();
                                break;

                            case QualityType.Negative:
                                sbdNegativeQualityTooltip.AppendFormat(
                                    GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                                    strSpace, objLoopQuality.BP * CharacterObjectSettings.KarmaQuality).AppendLine();
                                break;
                        }
                    }
                }

                if (CharacterObject.Contacts.Any(x => x.EntityType == ContactType.Contact && x.IsGroup && !x.Free))
                {
                    sbdPositiveQualityTooltip.AppendLine(LanguageManager.GetString("Label_GroupContacts"));
                    foreach (Contact objGroupContact in CharacterObject.Contacts.Where(x =>
                                 x.EntityType == ContactType.Contact && x.IsGroup && !x.Free))
                    {
                        string strNameToUse = objGroupContact.GroupName;
                        if (string.IsNullOrEmpty(strNameToUse))
                        {
                            strNameToUse = objGroupContact.Name;
                            if (string.IsNullOrEmpty(strNameToUse))
                                strNameToUse = LanguageManager.GetString("String_Unknown");
                        }
                        else if (!string.IsNullOrWhiteSpace(objGroupContact.Name))
                            strNameToUse += '/' + objGroupContact.Name;

                        sbdPositiveQualityTooltip.AppendFormat(GlobalSettings.CultureInfo, "{0}{1}({2})",
                                                               strNameToUse,
                                                               strSpace,
                                                               objGroupContact.ContactPoints
                                                               * CharacterObjectSettings.KarmaContact).AppendLine();
                    }
                }

                lblPositiveQualitiesBP.SetToolTip(sbdPositiveQualityTooltip.ToString());
                lblNegativeQualitiesBP.SetToolTip(sbdNegativeQualityTooltip.ToString());
            }

            int intQualityPointsUsed = intLifeModuleQualities - CharacterObject.NegativeQualityKarma + CharacterObject.PositiveQualityKarmaTotal;

            intKarmaPointsRemain -= intQualityPointsUsed;
            intFreestyleBP += intQualityPointsUsed;
            // Changelings must either have a balanced negative and positive number of metagenic qualities, or have 1 more point of positive than negative.
            // If the latter, karma is used to balance them out.
            if (CharacterObject.MetagenicPositiveQualityKarma + CharacterObject.MetagenicNegativeQualityKarma == 1)
                intKarmaPointsRemain--;

            // ------------------------------------------------------------------------------
            // Update Primary Attributes and Special Attributes values.
            int intAttributePointsUsed = CalculateAttributeBP(CharacterObject.AttributeSection.AttributeList);
            intAttributePointsUsed += CalculateAttributeBP(CharacterObject.AttributeSection.SpecialAttributeList);
            intKarmaPointsRemain -= intAttributePointsUsed;

            // ------------------------------------------------------------------------------
            // Include the BP used by Martial Arts.
            int intMartialArtsPoints = 0;
            string strColon = LanguageManager.GetString("String_Colon");
            string strOf = LanguageManager.GetString("String_Of");
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdMartialArtsBPToolTip))
            {
                foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                {
                    if (objMartialArt.IsQuality)
                        continue;
                    int intLoopCost = objMartialArt.Cost;
                    intMartialArtsPoints += intLoopCost;

                    if (blnDoUIUpdate)
                    {
                        if (sbdMartialArtsBPToolTip.Length > 0)
                            sbdMartialArtsBPToolTip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                        sbdMartialArtsBPToolTip.Append(objMartialArt.CurrentDisplayName).Append(strSpace).Append('(')
                                               .Append(intLoopCost.ToString(GlobalSettings.CultureInfo)).Append(')');

                        bool blnIsFirst = true;
                        foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                        {
                            if (blnIsFirst)
                            {
                                blnIsFirst = false;
                                continue;
                            }

                            intLoopCost = CharacterObjectSettings.KarmaTechnique;
                            intMartialArtsPoints += intLoopCost;

                            sbdMartialArtsBPToolTip.AppendLine().Append(strSpace).Append('+').Append(strSpace)
                                                   .Append(objTechnique.CurrentDisplayName).Append(strSpace).Append('(')
                                                   .Append(intLoopCost.ToString(GlobalSettings.CultureInfo))
                                                   .Append(')');
                        }
                    }
                    else
                        // Add in the Techniques
                        intMartialArtsPoints += Math.Max(objMartialArt.Techniques.Count - 1, 0)
                                                * CharacterObjectSettings.KarmaTechnique;
                }

                lblBuildMartialArts.SetToolTip(sbdMartialArtsBPToolTip.ToString());
            }

            intKarmaPointsRemain -= intMartialArtsPoints;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Skill Groups.
            int intSkillGroupsPoints = CharacterObject.SkillsSection.SkillGroups.TotalCostKarma();
            intKarmaPointsRemain -= intSkillGroupsPoints;
            // ------------------------------------------------------------------------------
            // Calculate the BP used by Active Skills.
            int skillPointsKarma = CharacterObject.SkillsSection.Skills.TotalCostKarma();
            intKarmaPointsRemain -= skillPointsKarma;

            // ------------------------------------------------------------------------------
            // Calculate the points used by Knowledge Skills.
            int knowledgeKarmaUsed = CharacterObject.SkillsSection.KnowledgeSkills.TotalCostKarma();

            //TODO: Remaining is named USED?
            intKarmaPointsRemain -= knowledgeKarmaUsed;

            intFreestyleBP += knowledgeKarmaUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Resources/Nuyen.
            int intNuyenBP = CharacterObject.NuyenBP.StandardRound();

            intKarmaPointsRemain -= intNuyenBP;

            intFreestyleBP += intNuyenBP;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spells.
            int intSpellPointsUsed = 0;
            int intRitualPointsUsed = 0;
            int intPrepPointsUsed = 0;
            if (CharacterObject.MagicianEnabled
                || CharacterObject.AdeptEnabled
                || ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.FreeSpells).Count > 0
                || ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.FreeSpellsATT).Count > 0
                || ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.FreeSpellsSkill).Count > 0)
            {
                // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
                int spells = CharacterObject.Spells.Count(spell => spell.Grade == 0 && !spell.Alchemical && spell.Category != "Rituals" && !spell.FreeBonus);
                int intTouchOnlySpells = CharacterObject.Spells.Count(spell => spell.Grade == 0 && !spell.Alchemical && spell.Category != "Rituals" && (spell.Range == "T (A)" || spell.Range == "T") && !spell.FreeBonus);
                int rituals = CharacterObject.Spells.Count(spell => spell.Grade == 0 && !spell.Alchemical && spell.Category == "Rituals" && !spell.FreeBonus);
                int preps = CharacterObject.Spells.Count(spell => spell.Grade == 0 && spell.Alchemical && !spell.FreeBonus);

                // Each spell costs KarmaSpell.
                int spellCost = CharacterObject.SpellKarmaCost("Spells");
                int ritualCost = CharacterObject.SpellKarmaCost("Rituals");
                int prepCost = CharacterObject.SpellKarmaCost("Preparations");
                int limit = CharacterObject.FreeSpells;

                // Factor in any qualities that can be bought with spell points.
                // It is only karma-efficient to use spell points for Mastery qualities if real spell karma cost is not greater than unmodified spell karma cost
                if (spellCost <= CharacterObjectSettings.KarmaSpell && CharacterObject.FreeSpells > 0)
                {
                    int intQualityKarmaToSpellPoints = CharacterObjectSettings.KarmaSpell;
                    if (intQualityKarmaToSpellPoints != 0)
                    {
                        // Assume that every [spell cost] karma spent on a Mastery quality is paid for with a priority-given spell point instead, as that is the most karma-efficient.
                        int intMasteryQualityKarmaUsed = CharacterObject.Qualities.Where(objQuality => objQuality.CanBuyWithSpellPoints)
                                                                        .Sum(objQuality => objQuality.BP);
                        if (intMasteryQualityKarmaUsed != 0)
                        {
                            intQualityKarmaToSpellPoints
                                = Math.Min(
                                    limit,
                                    intMasteryQualityKarmaUsed * CharacterObjectSettings.KarmaQuality
                                    / CharacterObjectSettings.KarmaSpell);
                            spells += intQualityKarmaToSpellPoints;
                            // Add the karma paid for by spell points back into the available karma pool.
                            intKarmaPointsRemain += intQualityKarmaToSpellPoints * CharacterObjectSettings.KarmaSpell;
                        }
                    }
                }

                int intLimitMod = (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SpellLimit)
                                   + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeSpells)).StandardRound();
                int intLimitModTouchOnly = 0;
                foreach (Improvement imp in ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.FreeSpellsATT))
                {
                    int intAttValue = CharacterObject.GetAttribute(imp.ImprovedName).TotalValue;
                    if (imp.UniqueName.Contains("half"))
                        intAttValue = (intAttValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        intLimitModTouchOnly += intAttValue;
                    else
                        intLimitMod += intAttValue;
                }

                foreach (Improvement imp in ImprovementManager.GetCachedImprovementListForValueOf(CharacterObject, Improvement.ImprovementType.FreeSpellsSkill))
                {
                    Skill skill = CharacterObject.SkillsSection.GetActiveSkill(imp.ImprovedName);
                    if (skill == null) continue;
                    int intSkillValue = skill.TotalBaseRating;

                    if (imp.UniqueName.Contains("half"))
                        intSkillValue = (intSkillValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        intLimitModTouchOnly += intSkillValue;
                    else
                        intLimitMod += intSkillValue;
                    //TODO: I don't like this being hardcoded, even though I know full well CGL are never going to reuse this.
                    spells -= skill.Specializations.Count(
                        spec => CharacterObject.Spells.Any(spell => spell.Category == spec.Name && !spell.FreeBonus));
                }

                if (nudMysticAdeptMAGMagician.Value > 0)
                {
                    int intPPBought = nudMysticAdeptMAGMagician.ValueAsInt;
                    if (CharacterObjectSettings.PrioritySpellsAsAdeptPowers)
                    {
                        spells += Math.Min(limit, intPPBought);
                        intPPBought = Math.Max(0, intPPBought - limit);
                    }
                    intAttributePointsUsed = intPPBought * CharacterObject.Settings.KarmaMysticAdeptPowerPoint;
                    intKarmaPointsRemain -= intAttributePointsUsed;
                }
                spells -= intTouchOnlySpells - Math.Max(0, intTouchOnlySpells - intLimitModTouchOnly);

                int spellPoints = limit + intLimitMod;
                int ritualPoints = limit + intLimitMod;
                int prepPoints = limit + intLimitMod;
                for (int i = limit + intLimitMod; i > 0; i--)
                {
                    if (spells > 0)
                    {
                        spells--;
                        spellPoints--;
                    }
                    else if (rituals > 0)
                    {
                        rituals--;
                        ritualPoints--;
                    }
                    else if (preps > 0)
                    {
                        preps--;
                        prepPoints--;
                    }
                    else
                    {
                        break;
                    }
                }
                intKarmaPointsRemain -= Math.Max(0, spells) * spellCost;
                intKarmaPointsRemain -= Math.Max(0, rituals) * ritualCost;
                intKarmaPointsRemain -= Math.Max(0, preps) * prepCost;

                intSpellPointsUsed += Math.Max(Math.Max(0, spells) * spellCost, 0);
                intRitualPointsUsed += Math.Max(Math.Max(0, rituals) * ritualCost, 0);
                intPrepPointsUsed += Math.Max(Math.Max(0, preps) * prepCost, 0);
                if (blnDoUIUpdate
                    && (lblBuildPrepsBP != null
                        || lblSpellsBP != null
                        || lblBuildRitualsBP != null))
                {
                    string strFormat = "{0}" + strSpace + '×' + strSpace + "{1}" + strSpace + LanguageManager.GetString("String_Karma")
                                       + strSpace + '=' + strSpace + "{2}" + strSpace + LanguageManager.GetString("String_Karma");
                    lblSpellsBP?.SetToolTip(string.Format(GlobalSettings.CultureInfo, strFormat, spells, spellCost, intSpellPointsUsed));
                    lblBuildRitualsBP?.SetToolTip(string.Format(GlobalSettings.CultureInfo, strFormat, rituals, spellCost, intRitualPointsUsed));
                    lblBuildPrepsBP?.SetToolTip(string.Format(GlobalSettings.CultureInfo, strFormat, preps, spellCost, intPrepPointsUsed));
                    if (limit + intLimitMod > 0)
                    {
                        if (lblBuildPrepsBP != null)
                        {
                            string strText = string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}", prepPoints + spellPoints + ritualPoints - 2 * (limit + intLimitMod), strOf, spellPoints + ritualPoints - (limit + intLimitMod));
                            if (intPrepPointsUsed > 0)
                                strText += string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}{1}{3}", strColon, strSpace, intPrepPointsUsed, strPoints);
                            lblBuildPrepsBP.Text = strText;
                        }
                        if (lblSpellsBP != null)
                        {
                            string strText = string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}", prepPoints + spellPoints + ritualPoints - 2 * (limit + intLimitMod), strOf, prepPoints + ritualPoints - (limit + intLimitMod));
                            if (intSpellPointsUsed > 0)
                                strText += string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}{1}{3}", strColon, strSpace, intSpellPointsUsed, strPoints);
                            lblSpellsBP.Text = strText;
                        }
                        if (lblBuildRitualsBP != null)
                        {
                            string strText = string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}", prepPoints + spellPoints + ritualPoints - 2 * (limit + intLimitMod), strOf, prepPoints + spellPoints - (limit + intLimitMod));
                            if (intRitualPointsUsed > 0)
                                strText += string.Format(GlobalSettings.CultureInfo, "{0}{1}{2}{1}{3}", strColon, strSpace, intRitualPointsUsed, strPoints);
                            lblBuildRitualsBP.Text = strText;
                        }
                    }
                    else if (intLimitMod == 0)
                    {
                        if (lblBuildPrepsBP != null)
                            lblBuildPrepsBP.Text =
                                intPrepPointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;
                        if (lblSpellsBP != null)
                            lblSpellsBP.Text =
                                intSpellPointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;
                        if (lblBuildRitualsBP != null)
                            lblBuildRitualsBP.Text =
                                intRitualPointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;
                    }
                    else
                    {
                        //TODO: Make the costs render better, currently looks wrong as hell
                        strFormat = "{0}" + strOf + "{1}" + strColon + strSpace + "{2}" + strSpace + strPoints;
                        if (lblBuildPrepsBP != null)
                            lblBuildPrepsBP.Text =
                                string.Format(GlobalSettings.CultureInfo, strFormat, prepPoints + spellPoints + ritualPoints - 2 * intLimitMod, spellPoints + ritualPoints - intLimitMod, intPrepPointsUsed);
                        if (lblSpellsBP != null)
                            lblSpellsBP.Text =
                                string.Format(GlobalSettings.CultureInfo, strFormat, prepPoints + spellPoints + ritualPoints - 2 * intLimitMod, prepPoints + ritualPoints - intLimitMod, intSpellPointsUsed);
                        if (lblBuildRitualsBP != null)
                            lblBuildRitualsBP.Text =
                                string.Format(GlobalSettings.CultureInfo, strFormat, prepPoints + spellPoints + ritualPoints - 2 * intLimitMod, prepPoints + spellPoints - intLimitMod, intRitualPointsUsed);
                    }
                }
            }

            intFreestyleBP += intSpellPointsUsed + intRitualPointsUsed + intPrepPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Foci.
            int intFociPointsUsed = 0;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdFociPointsTooltip))
            {
                foreach (Focus objFocus in CharacterObject.Foci)
                {
                    intFociPointsUsed += objFocus.BindingKarmaCost();

                    if (!blnDoUIUpdate)
                        continue;
                    if (sbdFociPointsTooltip.Length > 0)
                        sbdFociPointsTooltip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                    sbdFociPointsTooltip.Append(objFocus.GearObject.CurrentDisplayName).Append(strSpace).Append('(')
                                        .Append(objFocus.BindingKarmaCost().ToString(GlobalSettings.CultureInfo))
                                        .Append(')');
                }

                intKarmaPointsRemain -= intFociPointsUsed;

                // Calculate the BP used by Stacked Foci.
                foreach (StackedFocus objFocus in CharacterObject.StackedFoci)
                {
                    if (!objFocus.Bonded)
                        continue;
                    int intBindingCost = objFocus.BindingCost;
                    intKarmaPointsRemain -= intBindingCost;
                    intFociPointsUsed += intBindingCost;

                    if (!blnDoUIUpdate)
                        continue;
                    if (sbdFociPointsTooltip.Length > 0)
                        sbdFociPointsTooltip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                    sbdFociPointsTooltip.Append(objFocus.CurrentDisplayName).Append(strSpace).Append('(')
                                        .Append(intBindingCost.ToString(GlobalSettings.CultureInfo)).Append(')');
                }

                intFreestyleBP += intFociPointsUsed;

                lblBuildFoci.SetToolTip(sbdFociPointsTooltip.ToString());
            }

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spirits and Sprites.
            int intSpiritPointsUsed = 0;
            int intSpritePointsUsed = 0;
            foreach (Spirit objSpirit in CharacterObject.Spirits)
            {
                int intLoopKarma = objSpirit.ServicesOwed * CharacterObjectSettings.KarmaSpirit;
                // Each Sprite costs KarmaSpirit x Services Owed.
                intKarmaPointsRemain -= intLoopKarma;
                if (objSpirit.EntityType == SpiritType.Spirit)
                {
                    intSpiritPointsUsed += intLoopKarma;
                    // Each Fettered Spirit costs 3 x Force.
                    if (objSpirit.Fettered)
                    {
                        intKarmaPointsRemain -= objSpirit.Force * CharacterObjectSettings.KarmaSpiritFettering;
                        intSpiritPointsUsed += objSpirit.Force * CharacterObjectSettings.KarmaSpiritFettering;
                    }
                }
                else
                {
                    intSpritePointsUsed += intLoopKarma;
                }
            }
            intFreestyleBP += intSpiritPointsUsed + intSpritePointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Complex Forms.
            int intFormsPointsUsed = 0;
            foreach (ComplexForm objComplexForm in CharacterObject.ComplexForms)
            {
                if (objComplexForm.Grade == 0)
                    ++intFormsPointsUsed;
            }
            if (intFormsPointsUsed > CharacterObject.CFPLimit)
                intKarmaPointsRemain -= (intFormsPointsUsed - CharacterObject.CFPLimit) * CharacterObject.ComplexFormKarmaCost;
            intFreestyleBP += intFormsPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Programs and Advanced Programs.
            int intAINormalProgramPointsUsed = 0;
            int intAIAdvancedProgramPointsUsed = 0;
            foreach (AIProgram objProgram in CharacterObject.AIPrograms)
            {
                if (objProgram.CanDelete)
                {
                    if (objProgram.IsAdvancedProgram)
                        ++intAIAdvancedProgramPointsUsed;
                    else
                        ++intAINormalProgramPointsUsed;
                }
            }
            int intKarmaCost = 0;
            int intNumAdvancedProgramPointsAsNormalPrograms = 0;
            if (intAINormalProgramPointsUsed > CharacterObject.AINormalProgramLimit)
            {
                if (intAIAdvancedProgramPointsUsed < CharacterObject.AIAdvancedProgramLimit)
                {
                    intNumAdvancedProgramPointsAsNormalPrograms = Math.Min(intAINormalProgramPointsUsed - CharacterObject.AINormalProgramLimit, CharacterObject.AIAdvancedProgramLimit - intAIAdvancedProgramPointsUsed);
                    intAINormalProgramPointsUsed -= intNumAdvancedProgramPointsAsNormalPrograms;
                }
                if (intAINormalProgramPointsUsed > CharacterObject.AINormalProgramLimit)
                    intKarmaCost += (intAINormalProgramPointsUsed - CharacterObject.AINormalProgramLimit) * CharacterObject.AIProgramKarmaCost;
            }
            if (intAIAdvancedProgramPointsUsed > CharacterObject.AIAdvancedProgramLimit)
            {
                intKarmaCost += (intAIAdvancedProgramPointsUsed - CharacterObject.AIAdvancedProgramLimit) * CharacterObject.AIAdvancedProgramKarmaCost;
            }
            intKarmaPointsRemain -= intKarmaCost;
            intFreestyleBP += intAIAdvancedProgramPointsUsed + intAINormalProgramPointsUsed + intNumAdvancedProgramPointsAsNormalPrograms;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Initiation.
            int intInitiationPoints = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                intInitiationPoints += objGrade.KarmaCost;
                // Add the Karma cost of extra Metamagic/Echoes to the Initiation cost.
                int metamagicKarma = Math.Max(CharacterObject.Metamagics.Count(x => x.Grade == objGrade.Grade) - 1, 0);
                intInitiationPoints += CharacterObjectSettings.KarmaMetamagic * metamagicKarma;
            }

            // Add the Karma cost of extra Metamagic/Echoes to the Initiation cost.
            intInitiationPoints += CharacterObject.Enhancements.Count * 2;
            /*
            foreach (Enhancement objEnhancement in CharacterObject.Enhancements)
            {
                intInitiationPoints += 2;
            }
            */
            foreach (Power objPower in CharacterObject.Powers)
            {
                intInitiationPoints += objPower.Enhancements.Count * 2;
                /*
                foreach (Enhancement objEnhancement in objPower.Enhancements)
                    intInitiationPoints += 2;
                    */
            }

            // Joining a Network does not cost Karma for Technomancers, so this only applies to Magicians/Adepts.
            // Check to see if the character is a member of a Group.
            if (CharacterObject.GroupMember && CharacterObject.MAGEnabled)
                intInitiationPoints += CharacterObjectSettings.KarmaJoinGroup;

            intKarmaPointsRemain -= intInitiationPoints;
            intFreestyleBP += intInitiationPoints;

            // Add the Karma cost of any Critter Powers.
            foreach (CritterPower objPower in CharacterObject.CritterPowers)
            {
                intKarmaPointsRemain -= objPower.Karma;
            }

            CharacterObject.Karma = intKarmaPointsRemain;

            if (!blnDoUIUpdate)
                return intKarmaPointsRemain;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdContactPoints))
            {
                sbdContactPoints.Append(CharacterObject.ContactPointsUsed.ToString(GlobalSettings.CultureInfo));
                if (CharacterObject.FriendsInHighPlaces)
                {
                    sbdContactPoints.Append('/')
                                    .Append(Math.Max(0, CharacterObject.CHA.Value * 4 - intHighPlacesFriends)
                                                .ToString(GlobalSettings.CultureInfo));
                }

                sbdContactPoints.Append(strOf).Append(intContactPoints.ToString(GlobalSettings.CultureInfo));
                if (CharacterObject.FriendsInHighPlaces)
                {
                    sbdContactPoints.Append('/')
                                    .Append((CharacterObject.CHA.Value * 4).ToString(GlobalSettings.CultureInfo));
                }

                if (intPointsInContacts > 0 || CharacterObject.CHA.Value * 4 < intHighPlacesFriends)
                {
                    sbdContactPoints.Append(strSpace).Append('(')
                                    .Append(intPointsInContacts.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                    .Append(strPoints).Append(')');
                }

                string strContactPoints = sbdContactPoints.ToString();

                lblContactsBP.Text = strContactPoints;
                lblContactPoints.Text = strContactPoints;
            }

            lblAttributesBP.Text = BuildAttributes(CharacterObject.AttributeSection.AttributeList);
            lblPBuildSpecial.Text = BuildAttributes(CharacterObject.AttributeSection.SpecialAttributeList, null, true);

            lblMartialArtsBP.Text = intMartialArtsPoints.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            lblNuyenBP.Text = intNuyenBP.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            lblFociBP.Text = intFociPointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            lblSpiritsBP.Text = intSpiritPointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            lblSpritesBP.Text = intSpritePointsUsed.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdComplexFormsBP))
            {
                if (CharacterObject.CFPLimit > 0)
                {
                    sbdComplexFormsBP.Append(intFormsPointsUsed.ToString(GlobalSettings.CultureInfo)).Append(strOf)
                                     .Append(CharacterObject.CFPLimit.ToString(GlobalSettings.CultureInfo));
                    if (intFormsPointsUsed > CharacterObject.CFPLimit)
                    {
                        sbdComplexFormsBP.Append(strColon).Append(strSpace)
                                         .Append(((intFormsPointsUsed - CharacterObject.CFPLimit)
                                                  * CharacterObject.ComplexFormKarmaCost)
                                                 .ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                                         .Append(strPoints);
                    }
                }
                else
                {
                    sbdComplexFormsBP
                        .Append(((intFormsPointsUsed - CharacterObject.CFPLimit) * CharacterObject.ComplexFormKarmaCost)
                                .ToString(GlobalSettings.CultureInfo)).Append(strSpace).Append(strPoints);
                }

                lblComplexFormsBP.Text = sbdComplexFormsBP.ToString();
            }

            lblAINormalProgramsBP.Text = ((intAINormalProgramPointsUsed - CharacterObject.AINormalProgramLimit) * CharacterObject.AIProgramKarmaCost).ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;
            lblAIAdvancedProgramsBP.Text = ((intAIAdvancedProgramPointsUsed - CharacterObject.AIAdvancedProgramLimit) * CharacterObject.AIAdvancedProgramKarmaCost).ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;

            lblInitiationBP.Text = intInitiationPoints.ToString(GlobalSettings.CultureInfo) + strSpace + strPoints;
            // ------------------------------------------------------------------------------
            // Update the number of BP remaining in the StatusBar.
            tslKarmaRemaining.Text = intKarmaPointsRemain.ToString(GlobalSettings.CultureInfo);
            if (_blnFreestyle)
            {
                tslKarma.Text = Math.Max(intFreestyleBP, intFreestyleBPMin).ToString(GlobalSettings.CultureInfo);
                tslKarma.ForeColor = intFreestyleBP < intFreestyleBPMin ? ColorManager.ErrorColor : ColorManager.ControlText;
            }
            else
            {
                tslKarma.Text = CharacterObjectSettings.BuildKarma.ToString(GlobalSettings.CultureInfo);
                tslKarma.ForeColor = ColorManager.ControlText;
            }

            return intKarmaPointsRemain;
        }

        private void UpdateSkillRelatedInfo()
        {
            string strKarma = LanguageManager.GetString("String_Karma");
            string strOf = LanguageManager.GetString("String_Of");
            string strColon = LanguageManager.GetString("String_Colon");
            string strSpace = LanguageManager.GetString("String_Space");
            string strZeroKarma = 0.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
            //Update Skill Labels
            //Active skills
            string strTemp = strZeroKarma;
            int intActiveSkillPointsMaximum = CharacterObject.SkillsSection.SkillPointsMaximum;
            if (intActiveSkillPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.SkillPoints.ToString(GlobalSettings.CultureInfo) + strOf + intActiveSkillPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }
            int intActiveSkillsTotalCostKarma = CharacterObject.SkillsSection.Skills.TotalCostKarma();
            if (intActiveSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intActiveSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intActiveSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }
            lblActiveSkillsBP.Text = strTemp;
            //Knowledge skills
            strTemp = strZeroKarma;
            int intKnowledgeSkillPointsMaximum = CharacterObject.SkillsSection.KnowledgeSkillPoints;
            if (intKnowledgeSkillPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalSettings.CultureInfo) + strOf + intKnowledgeSkillPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }
            int intKnowledgeSkillsTotalCostKarma = CharacterObject.SkillsSection.KnowledgeSkills.TotalCostKarma();
            if (intKnowledgeSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intKnowledgeSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intKnowledgeSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }
            lblKnowledgeSkillsBP.Text = strTemp;
            //Groups
            strTemp = strZeroKarma;
            int intSkillGroupPointsMaximum = CharacterObject.SkillsSection.SkillGroupPointsMaximum;
            if (intSkillGroupPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.SkillGroupPoints.ToString(GlobalSettings.CultureInfo) + strOf + intSkillGroupPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }
            int intSkillGroupsTotalCostKarma = CharacterObject.SkillsSection.SkillGroups.TotalCostKarma();
            if (intSkillGroupsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intSkillGroupsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intSkillGroupsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }
            lblSkillGroupsBP.Text = strTemp;
        }

        private async void LiveUpdateFromCharacterFile(object sender, EventArgs e)
        {
            if (IsDirty || !GlobalSettings.LiveUpdateCleanCharacterFiles || IsLoading || _blnSkipUpdate || IsCharacterUpdateRequested)
                return;

            string strCharacterFile = CharacterObject.FileName;
            if (string.IsNullOrEmpty(strCharacterFile) || !File.Exists(strCharacterFile))
                return;

            if (File.GetLastWriteTimeUtc(strCharacterFile) <= CharacterObject.FileLastWriteTime)
                return;

            _blnSkipUpdate = true;

            // Character is not dirty and their savefile was updated outside of Chummer5 while it is open, so reload them
            using (new CursorWait(this))
            {
                using (LoadingBar frmLoadingForm = await Program.CreateAndShowProgressBarAsync(Path.GetFileName(CharacterObject.FileName), Character.NumLoadingSections))
                {
                    await CharacterObject.LoadAsync(frmLoadingForm);
                    frmLoadingForm.PerformStep(await LanguageManager.GetStringAsync("String_UI"));

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

                    IsCharacterUpdateRequested = true;
                    _blnSkipUpdate = false;
                    // Immediately call character update because we know it's necessary
                    await DoUpdateCharacterInfoAsync();

                    IsDirty = false;
                }
            }

            if (CharacterObject.InternalIdsNeedingReapplyImprovements.Count > 0 && !Utils.IsUnitTest
                && Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ImprovementLoadError"),
                    await LanguageManager.GetStringAsync("MessageTitle_ImprovementLoadError"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                await DoReapplyImprovements(CharacterObject.InternalIdsNeedingReapplyImprovements);
                CharacterObject.InternalIdsNeedingReapplyImprovements.Clear();
            }
        }
        
        private async void UpdateCharacterInfo(object sender, EventArgs e)
        {
            await DoUpdateCharacterInfoAsync();
        }

        /// <summary>
        /// Update the Character information.
        /// </summary>
        private void DoUpdateCharacterInfo()
        {
            DoUpdateCharacterInfoCoreAsync(true).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update the Character information.
        /// </summary>
        private Task DoUpdateCharacterInfoAsync()
        {
            return DoUpdateCharacterInfoCoreAsync(false);
        }

        /// <summary>
        /// Update the Character information.
        /// </summary>
        private async Task DoUpdateCharacterInfoCoreAsync(bool blnSync)
        {
            if (IsLoading || _blnSkipUpdate || !IsCharacterUpdateRequested)
                return;

            _blnSkipUpdate = true;

            using (new CursorWait(this))
            {
                // TODO: DataBind these wherever possible

                UpdateSkillRelatedInfo();

                // Calculate the number of Build Points remaining.
                CalculateBP();
                CalculateNuyen();

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

                UpdateInitiationCost();

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
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        AutoSaveCharacter();
                    else
                        await AutoSaveCharacterAsync();
                }
            }

            _blnSkipUpdate = false;
            IsCharacterUpdateRequested = false;
        }

        /// <summary>
        /// Calculate the amount of Nuyen the character has remaining.
        /// </summary>
        private decimal CalculateNuyen()
        {
            object objDeductionsLock = new object();
            decimal decDeductions = 0;
            decimal decStolenDeductions = 0;
            decimal decStolenNuyenAllowance
                = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Nuyen,
                                             strImprovedName: "Stolen");
            //If the character has the Stolen Gear quality or something similar, we need to handle the nuyen a little differently.
            if (decStolenNuyenAllowance != 0)
            {
                object objStolenDeductionsLock = new object();
                Utils.RunWithoutThreadLock(
                    () =>
                    {
                        // Cyberware/Bioware cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                        {
                            if (objCyberware.Stolen)
                                decTempStolen += objCyberware.StolenTotalCost;
                            else
                                decTemp += objCyberware.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            // ReSharper disable once AccessToModifiedClosure
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Armor cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Armor objArmor in CharacterObject.Armor)
                        {
                            if (objArmor.Stolen)
                                decTempStolen += objArmor.StolenTotalCost;
                            else
                                decTemp += objArmor.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Weapon cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Weapon objWeapon in CharacterObject.Weapons)
                        {
                            if (objWeapon.Stolen)
                                decTempStolen += objWeapon.StolenTotalCost;
                            else
                                decTemp += objWeapon.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Gear cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Gear objGear in CharacterObject.Gear)
                        {
                            if (objGear.Stolen)
                                decTempStolen += objGear.StolenTotalCost;
                            else
                                decTemp += objGear.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Vehicle cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                        {
                            if (objVehicle.Stolen)
                                decTempStolen += objVehicle.StolenTotalCost;
                            else
                                decTemp += objVehicle.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Drug cost.
                        decimal decTempStolen = 0;
                        decimal decTemp = 0;
                        foreach (Drug objDrug in CharacterObject.Drugs)
                        {
                            if (objDrug.Stolen)
                                decTempStolen += objDrug.StolenTotalCost;
                            else
                                decTemp += objDrug.TotalCost;
                        }
                        lock (objStolenDeductionsLock)
                            decStolenDeductions += decTempStolen;
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Lifestyle cost.
                        decimal decTemp = CharacterObject.Lifestyles.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    });
            }
            else
            {
                Utils.RunWithoutThreadLock(
                    () =>
                    {
                        // Cyberware/Bioware cost.
                        decimal decTemp = CharacterObject.Cyberware.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            // ReSharper disable once AccessToModifiedClosure
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Armor cost.
                        decimal decTemp = CharacterObject.Armor.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Weapon cost.
                        decimal decTemp = CharacterObject.Weapons.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Gear cost.
                        decimal decTemp = CharacterObject.Gear.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Vehicle cost.
                        decimal decTemp = CharacterObject.Vehicles.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Drug cost.
                        decimal decTemp = CharacterObject.Drugs.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    },
                    () =>
                    {
                        // Lifestyle cost.
                        decimal decTemp = CharacterObject.Lifestyles.Sum(x => x.TotalCost);
                        lock (objDeductionsLock)
                            decDeductions += decTemp;
                    });
            }

            // Initiation Grade cost.
            decDeductions += 10000 * CharacterObject.InitiationGrades.Count(x => x.Schooling);

            CharacterObject.StolenNuyen = decStolenNuyenAllowance - decStolenDeductions;
            return CharacterObject.Nuyen = CharacterObject.TotalStartingNuyen - decDeductions;
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

                // Buttons
                cmdDeleteCyberware.Enabled = treCyberware.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpCyberware.ResumeLayout();
                return;
            }

            if (treCyberware.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblCyberwareRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            string strESSFormat = CharacterObjectSettings.EssenceFormat;
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

            if (treCyberware.SelectedNode?.Tag is IHasStolenProperty loot && ImprovementManager
                                                                             .GetCachedImprovementListForValueOf(
                                                                                 CharacterObject,
                                                                                 Improvement.ImprovementType.Nuyen,
                                                                                 "Stolen").Count > 0)
            {
                chkCyberwareStolen.Visible = true;
                chkCyberwareStolen.Checked = loot.Stolen;
            }
            else
            {
                chkCyberwareStolen.Visible = false;
            }

            switch (treCyberware.SelectedNode?.Tag)
            {
                // Locate the selected piece of Cyberware.
                case Cyberware objCyberware:
                    {
                        gpbCyberwareCommon.Visible = true;
                        gpbCyberwareMatrix.Visible = objCyberware.SourceType == Improvement.ImprovementSource.Cyberware;

                        // Buttons
                        cmdDeleteCyberware.Enabled = string.IsNullOrEmpty(objCyberware.ParentID);

                        // gpbCyberwareCommon
                        lblCyberwareName.Text = objCyberware.CurrentDisplayName;
                        lblCyberwareCategory.Text = objCyberware.DisplayCategory(GlobalSettings.Language);
                        // Cyberware Grade is not available for Genetech items.
                        // Cyberware Grade is only available on root-level items (sub-components cannot have a different Grade than the piece they belong to).
                        cboCyberwareGrade.Enabled = objCyberware.Parent == null && !objCyberware.Suite && string.IsNullOrWhiteSpace(objCyberware.ForceGrade);
                        bool blnIgnoreSecondHand = objCyberware.GetNodeXPath()?.SelectSingleNode("nosecondhand") != null;
                        PopulateCyberwareGradeList(objCyberware.SourceType == Improvement.ImprovementSource.Bioware, blnIgnoreSecondHand, cboCyberwareGrade.Enabled ? string.Empty : objCyberware.Grade.Name);
                        lblCyberwareGradeLabel.Visible = true;
                        cboCyberwareGrade.Visible = true;
                        cboCyberwareGrade.SelectedValue = objCyberware.Grade.Name;
                        if (cboCyberwareGrade.SelectedIndex == -1 && cboCyberwareGrade.Items.Count > 0)
                            cboCyberwareGrade.SelectedIndex = 0;
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
                        // Enable and set the Rating values as needed.
                        if (objCyberware.MaxRating == 0)
                        {
                            nudCyberwareRating.Maximum = 0;
                            nudCyberwareRating.Minimum = 0;
                            nudCyberwareRating.Value = 0;
                            nudCyberwareRating.Visible = false;
                            lblCyberwareRatingLabel.Visible = false;
                        }
                        else
                        {
                            nudCyberwareRating.Maximum = objCyberware.MaxRating;
                            nudCyberwareRating.Minimum = objCyberware.MinRating;
                            nudCyberwareRating.Value = objCyberware.Rating;
                            nudCyberwareRating.Visible = true;
                            nudCyberwareRating.Enabled = nudCyberwareRating.Maximum != nudCyberwareRating.Minimum && string.IsNullOrEmpty(objCyberware.ParentID);
                            lblCyberwareRatingLabel.Visible = true;
                        }

                        lblCyberwareCapacity.Text = objCyberware.DisplayCapacity;
                        lblCyberwareCost.Text = objCyberware.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        if (objCyberware.Category.Equals("Cyberlimb", StringComparison.Ordinal) || objCyberware.AllowedSubsystems.Contains("Cyberlimb"))
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

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = CharacterObject.GenerateBlackMarketMappings(CharacterObject
                                    .LoadDataXPath(objCyberware.SourceType == Improvement.ImprovementSource.Cyberware
                                        ? "cyberware.xml"
                                        : "bioware.xml").SelectSingleNodeAndCacheExpression("/chummer"))
                                .Contains(objCyberware.Category);
                            chkCyberwareBlackMarketDiscount.Checked = !string.IsNullOrEmpty(objCyberware.ParentID)
                                ? objCyberware.Parent?.DiscountCost == true
                                : objCyberware.DiscountCost;
                        }
                        else
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = false;
                            chkCyberwareBlackMarketDiscount.Checked = false;
                        }

                        chkPrototypeTranshuman.Visible = CharacterObject.IsPrototypeTranshuman;
                        chkPrototypeTranshuman.Enabled = objCyberware.Parent == null && objCyberware.SourceType == Improvement.ImprovementSource.Bioware;
                        chkPrototypeTranshuman.Checked = objCyberware.PrototypeTranshuman && CharacterObject.IsPrototypeTranshuman;

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
                        }

                        treCyberware.SelectedNode.Text = objCyberware.CurrentDisplayName;
                        break;
                    }
                case Gear objGear:
                    {
                        gpbCyberwareCommon.Visible = true;
                        gpbCyberwareMatrix.Visible = true;

                        // Buttons
                        cmdDeleteCyberware.Enabled = !objGear.IncludedInParent;

                        // gpbCyberwareCommon
                        lblCyberwareName.Text = objGear.CurrentDisplayNameShort;
                        lblCyberwareCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                        lblCyberwareGradeLabel.Visible = false;
                        cboCyberwareGrade.Visible = false;
                        lblCyberwareEssenceLabel.Visible = false;
                        lblCyberwareEssence.Visible = false;
                        lblCyberwareAvail.Text = objGear.DisplayTotalAvail;
                        cmdCyberwareChangeMount.Visible = false;
                        int intGearMaxRatingValue = objGear.MaxRatingValue;
                        if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                        {
                            int intGearMinRatingValue = objGear.MinRatingValue;
                            if (objGear.MinRatingValue > 0)
                                nudCyberwareRating.Minimum = intGearMinRatingValue;
                            else if (intGearMinRatingValue == 0 && objGear.Name.Contains("Credstick,"))
                                nudCyberwareRating.Minimum = 0;
                            else
                                nudCyberwareRating.Minimum = 1;
                            nudCyberwareRating.Maximum = intGearMaxRatingValue;
                            nudCyberwareRating.Value = objGear.Rating;
                            nudCyberwareRating.Enabled = nudCyberwareRating.Minimum != nudCyberwareRating.Maximum && string.IsNullOrEmpty(objGear.ParentID);
                            nudCyberwareRating.Visible = true;
                            lblCyberwareRatingLabel.Visible = true;
                        }
                        else
                        {
                            nudCyberwareRating.Minimum = 0;
                            nudCyberwareRating.Maximum = 0;
                            nudCyberwareRating.Visible = false;
                            lblCyberwareRatingLabel.Visible = false;
                        }

                        lblCyberwareCapacity.Text = objGear.DisplayCapacity;
                        lblCyberwareCost.Text =
                            objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblCyberlimbAGILabel.Visible = false;
                        lblCyberlimbAGI.Visible = false;
                        lblCyberlimbSTRLabel.Visible = false;
                        lblCyberlimbSTR.Visible = false;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = !objGear.IncludedInParent && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("gear.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objGear.Category);
                            chkCyberwareBlackMarketDiscount.Checked = objGear.IncludedInParent
                                ? (objGear.Parent as ICanBlackMarketDiscount)?.DiscountCost == true
                                : objGear.DiscountCost;
                        }
                        else
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = false;
                            chkCyberwareBlackMarketDiscount.Checked = false;
                        }

                        chkPrototypeTranshuman.Visible = false;

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

                        treCyberware.SelectedNode.Text = objGear.CurrentDisplayName;
                        break;
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

            if (treWeapons.SelectedNode?.Tag is IHasStolenProperty loot && ImprovementManager
                                                                           .GetCachedImprovementListForValueOf(
                                                                               CharacterObject,
                                                                               Improvement.ImprovementType.Nuyen,
                                                                               "Stolen").Count > 0)
            {
                chkWeaponStolen.Visible = true;
                chkWeaponStolen.Checked = loot.Stolen;
            }
            else
            {
                chkWeaponStolen.Visible = false;
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
                                    if (sbdSlotsText.Length > 0)
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
                        chkWeaponEquipped.Text
                            = LanguageManager.GetString(objWeapon.Parent == null
                                                            ? "Checkbox_Equipped"
                                                            : "Checkbox_Installed");
                        chkWeaponEquipped.Enabled = !objWeapon.IncludedInWeapon;
                        chkWeaponEquipped.Checked = objWeapon.Equipped;
                        chkIncludedInWeapon.Visible = objWeapon.Parent != null;
                        chkIncludedInWeapon.Enabled = false;
                        chkIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objWeapon.IncludedInWeapon && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("weapons.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objWeapon.Category);
                            chkWeaponBlackMarketDiscount.Checked = objWeapon.IncludedInWeapon
                                ? objWeapon.Parent?.DiscountCost == true
                                : objWeapon.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

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
                        lblWeaponDicePool.Visible = true;
                        lblWeaponDicePool.Text = objWeapon.DicePool.ToString(GlobalSettings.CultureInfo);
                        lblWeaponDicePool.SetToolTip(objWeapon.DicePoolTooltip);
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

                        // gpbWeaponsMatrix
                        int intDeviceRating = objWeapon.GetTotalMatrixAttribute("Device Rating");
                        lblWeaponDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        lblWeaponAttack.Text = objWeapon.GetTotalMatrixAttribute("Attack").ToString(GlobalSettings.CultureInfo);
                        lblWeaponSleaze.Text = objWeapon.GetTotalMatrixAttribute("Sleaze").ToString(GlobalSettings.CultureInfo);
                        lblWeaponDataProcessing.Text = objWeapon.GetTotalMatrixAttribute("Data Processing").ToString(GlobalSettings.CultureInfo);
                        lblWeaponFirewall.Text = objWeapon.GetTotalMatrixAttribute("Firewall").ToString(GlobalSettings.CultureInfo);
                        chkWeaponActiveCommlink.Visible = objWeapon.IsCommlink;
                        chkWeaponActiveCommlink.Checked = objWeapon.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkWeaponHomeNode.Visible = true;
                            chkWeaponHomeNode.Checked = objWeapon.IsHomeNode(CharacterObject);
                            chkWeaponHomeNode.Enabled = chkWeaponActiveCommlink.Enabled &&
                                                        objWeapon.GetTotalMatrixAttribute("Program Limit") >=
                                                        (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkWeaponHomeNode.Visible = false;
                        break;
                    }
                case WeaponAccessory objSelectedAccessory:
                    {
                        gpbWeaponsCommon.Visible = true;
                        gpbWeaponsWeapon.Visible = true;
                        gpbWeaponsMatrix.Visible = false;

                        // Buttons
                        cmdDeleteWeapon.Enabled = !objSelectedAccessory.IncludedInWeapon &&
                                                  string.IsNullOrEmpty(objSelectedAccessory.ParentID);

                        // gpbWeaponsCommon
                        lblWeaponName.Text = objSelectedAccessory.CurrentDisplayName;
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

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objSelectedAccessory.IncludedInWeapon && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("weapons.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objSelectedAccessory.Parent.Category);
                            chkWeaponBlackMarketDiscount.Checked = objSelectedAccessory.IncludedInWeapon
                                ? objSelectedAccessory.Parent?.DiscountCost == true
                                : objSelectedAccessory.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

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
                            lblWeaponDicePool.Visible = false;
                        }
                        else
                        {
                            lblWeaponDicePoolLabel.Visible = true;
                            lblWeaponDicePool.Visible = true;
                            lblWeaponDicePool.Text = objSelectedAccessory.DicePool.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
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

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objGear.IncludedInParent && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("gear.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objGear.Category);
                            chkWeaponBlackMarketDiscount.Checked = objGear.IncludedInParent
                                ? (objGear.Parent as ICanBlackMarketDiscount)?.DiscountCost == true
                                : objGear.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

                        // gpbWeaponsMatrix
                        int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                        lblWeaponDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                        lblWeaponAttack.Text = objGear.GetTotalMatrixAttribute("Attack").ToString(GlobalSettings.CultureInfo);
                        lblWeaponSleaze.Text = objGear.GetTotalMatrixAttribute("Sleaze").ToString(GlobalSettings.CultureInfo);
                        lblWeaponDataProcessing.Text = objGear.GetTotalMatrixAttribute("Data Processing").ToString(GlobalSettings.CultureInfo);
                        lblWeaponFirewall.Text = objGear.GetTotalMatrixAttribute("Firewall").ToString(GlobalSettings.CultureInfo);
                        chkWeaponActiveCommlink.Visible = objGear.IsCommlink;
                        chkWeaponActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                        if (CharacterObject.IsAI)
                        {
                            chkWeaponHomeNode.Visible = true;
                            chkWeaponHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                            chkWeaponHomeNode.Enabled = chkWeaponActiveCommlink.Enabled &&
                                                        objGear.GetTotalMatrixAttribute("Program Limit") >=
                                                        (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                        }
                        else
                            chkWeaponHomeNode.Visible = false;
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

            if (treArmor.SelectedNode?.Tag is IHasStolenProperty loot && ImprovementManager
                                                                         .GetCachedImprovementListForValueOf(
                                                                             CharacterObject,
                                                                             Improvement.ImprovementType.Nuyen,
                                                                             "Stolen").Count > 0)
            {
                chkArmorStolen.Visible = true;
                chkArmorStolen.Checked = loot.Stolen;
            }
            else
            {
                chkArmorStolen.Visible = false;
            }

            if (treArmor.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblArmorRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
            }

            if (treArmor.SelectedNode?.Tag is Armor objArmor)
            {
                gpbArmorCommon.Visible = true;
                gpbArmorMatrix.Visible = true;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = true;

                // gpbArmorCommon
                lblArmorValueLabel.Visible = true;
                lblArmorValue.Visible = true;
                lblArmorValue.Text = objArmor.DisplayArmorValue;
                lblArmorAvail.Text = objArmor.DisplayTotalAvail;
                lblArmorCapacity.Text = objArmor.DisplayCapacity;
                lblArmorRatingLabel.Visible = false;
                nudArmorRating.Visible = false;
                lblArmorCost.Text = objArmor.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                chkArmorEquipped.Visible = true;
                chkArmorEquipped.Checked = objArmor.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Visible = false;

                if (CharacterObject.BlackMarketDiscount)
                {
                    chkArmorBlackMarketDiscount.Enabled = CharacterObject
                        .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("armor.xml")
                            .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objArmor.Category);
                    chkArmorBlackMarketDiscount.Checked = objArmor.DiscountCost;
                }
                else
                {
                    chkArmorBlackMarketDiscount.Enabled = false;
                    chkArmorBlackMarketDiscount.Checked = false;
                }

                // gpbArmorMatrix
                int intDeviceRating = objArmor.GetTotalMatrixAttribute("Device Rating");
                lblArmorDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                lblArmorAttack.Text = objArmor.GetTotalMatrixAttribute("Attack").ToString(GlobalSettings.CultureInfo);
                lblArmorSleaze.Text = objArmor.GetTotalMatrixAttribute("Sleaze").ToString(GlobalSettings.CultureInfo);
                lblArmorDataProcessing.Text = objArmor.GetTotalMatrixAttribute("Data Processing").ToString(GlobalSettings.CultureInfo);
                lblArmorFirewall.Text = objArmor.GetTotalMatrixAttribute("Firewall").ToString(GlobalSettings.CultureInfo);
                chkArmorActiveCommlink.Visible = objArmor.IsCommlink;
                chkArmorActiveCommlink.Checked = objArmor.IsActiveCommlink(CharacterObject);
                if (CharacterObject.IsAI)
                {
                    chkArmorHomeNode.Visible = true;
                    chkArmorHomeNode.Checked = objArmor.IsHomeNode(CharacterObject);
                    chkArmorHomeNode.Enabled = chkArmorActiveCommlink.Enabled &&
                                               objArmor.GetTotalMatrixAttribute("Program Limit") >=
                                               (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                }
                else
                    chkArmorHomeNode.Visible = false;
            }
            else
            {
                string strSpace = LanguageManager.GetString("String_Space");
                if (treArmor.SelectedNode?.Tag is ArmorMod objArmorMod)
                {
                    gpbArmorCommon.Visible = true;
                    gpbArmorMatrix.Visible = false;
                    gpbArmorLocation.Visible = false;

                    // Buttons
                    cmdDeleteArmor.Enabled = !objArmorMod.IncludedInArmor;

                    // gpbArmorCommon
                    if (objArmorMod.Armor != 0)
                    {
                        lblArmorValueLabel.Visible = true;
                        lblArmorValue.Visible = true;
                        lblArmorValue.Text = objArmorMod.Armor.ToString("+0;-0;0", GlobalSettings.CultureInfo);
                    }
                    else
                    {
                        lblArmorValueLabel.Visible = false;
                        lblArmorValue.Visible = false;
                    }

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
                        nudArmorRating.Visible = true;
                        nudArmorRating.Maximum = objArmorMod.MaximumRating;
                        nudArmorRating.Value = objArmorMod.Rating;
                        nudArmorRating.Enabled = !objArmorMod.IncludedInArmor;
                    }
                    else
                    {
                        lblArmorRatingLabel.Visible = false;
                        nudArmorRating.Visible = false;
                    }

                    lblArmorCost.Text
                        = objArmorMod.TotalCost.ToString(CharacterObjectSettings.NuyenFormat,
                                                         GlobalSettings.CultureInfo) + '¥';
                    chkArmorEquipped.Visible = true;
                    chkArmorEquipped.Checked = objArmorMod.Equipped;
                    chkArmorEquipped.Enabled = true;
                    chkIncludedInArmor.Visible = true;
                    chkIncludedInArmor.Checked = objArmorMod.IncludedInArmor;

                    if (CharacterObject.BlackMarketDiscount)
                    {
                        chkArmorBlackMarketDiscount.Enabled = !objArmorMod.IncludedInArmor && CharacterObject
                            .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("armor.xml")
                                                                        .SelectSingleNodeAndCacheExpression(
                                                                            "/chummer/modcategories"))
                            .Contains(objArmorMod.Category);
                        chkArmorBlackMarketDiscount.Checked = objArmorMod.IncludedInArmor
                            ? objArmorMod.Parent?.DiscountCost == true
                            : objArmorMod.DiscountCost;
                    }
                    else
                    {
                        chkArmorBlackMarketDiscount.Enabled = false;
                        chkArmorBlackMarketDiscount.Checked = false;
                    }
                }
                else
                {
                    switch (treArmor.SelectedNode?.Tag)
                    {
                        case Gear objSelectedGear:
                        {
                            gpbArmorCommon.Visible = true;
                            gpbArmorMatrix.Visible = true;
                            gpbArmorLocation.Visible = false;

                            // Buttons
                            cmdDeleteArmor.Enabled = !objSelectedGear.IncludedInParent;

                            // gpbArmorCommon
                            lblArmorValueLabel.Visible = false;
                            lblArmorValue.Visible = false;
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
                                nudArmorRating.Visible = true;
                                nudArmorRating.Maximum = intMaxRatingValue;
                                int intMinRatingValue = objSelectedGear.MinRatingValue;
                                nudArmorRating.Minimum = intMinRatingValue;
                                nudArmorRating.Value = objSelectedGear.Rating;
                                nudArmorRating.Enabled = intMinRatingValue != intMaxRatingValue
                                                         && string.IsNullOrEmpty(objSelectedGear.ParentID);
                            }
                            else
                            {
                                lblArmorRatingLabel.Visible = false;
                                nudArmorRating.Visible = false;
                            }

                            lblArmorCost.Text
                                = objSelectedGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat,
                                                                     GlobalSettings.CultureInfo) + '¥';
                            chkArmorEquipped.Visible = true;
                            chkArmorEquipped.Checked = objSelectedGear.Equipped;
                            chkArmorEquipped.Enabled = true;
                            chkIncludedInArmor.Visible = true;
                            chkIncludedInArmor.Checked = objSelectedGear.IncludedInParent;

                            if (CharacterObject.BlackMarketDiscount)
                            {
                                chkArmorBlackMarketDiscount.Enabled = !objSelectedGear.IncludedInParent
                                                                      && CharacterObject
                                                                         .GenerateBlackMarketMappings(
                                                                             CharacterObject.LoadDataXPath("gear.xml")
                                                                                 .SelectSingleNodeAndCacheExpression(
                                                                                     "/chummer"))
                                                                         .Contains(objSelectedGear.Category);
                                chkArmorBlackMarketDiscount.Checked = objSelectedGear.IncludedInParent
                                    ? (objSelectedGear.Parent as ICanBlackMarketDiscount)?.DiscountCost == true
                                    : objSelectedGear.DiscountCost;
                            }
                            else
                            {
                                chkArmorBlackMarketDiscount.Enabled = false;
                                chkArmorBlackMarketDiscount.Checked = false;
                            }

                            // gpbArmorMatrix
                            int intDeviceRating = objSelectedGear.GetTotalMatrixAttribute("Device Rating");
                            lblArmorDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                            lblArmorAttack.Text = objSelectedGear.GetTotalMatrixAttribute("Attack")
                                                                 .ToString(GlobalSettings.CultureInfo);
                            lblArmorSleaze.Text = objSelectedGear.GetTotalMatrixAttribute("Sleaze")
                                                                 .ToString(GlobalSettings.CultureInfo);
                            lblArmorDataProcessing.Text = objSelectedGear.GetTotalMatrixAttribute("Data Processing")
                                                                         .ToString(GlobalSettings.CultureInfo);
                            lblArmorFirewall.Text = objSelectedGear.GetTotalMatrixAttribute("Firewall")
                                                                   .ToString(GlobalSettings.CultureInfo);
                            chkArmorActiveCommlink.Visible = objSelectedGear.IsCommlink;
                            chkArmorActiveCommlink.Checked = objSelectedGear.IsActiveCommlink(CharacterObject);
                            if (CharacterObject.IsAI)
                            {
                                chkArmorHomeNode.Visible = true;
                                chkArmorHomeNode.Checked = objSelectedGear.IsHomeNode(CharacterObject);
                                chkArmorHomeNode.Enabled = chkArmorActiveCommlink.Enabled &&
                                                           objSelectedGear.GetTotalMatrixAttribute("Program Limit") >=
                                                           (CharacterObject.DEP.TotalValue > intDeviceRating ? 2 : 1);
                            }
                            else
                                chkArmorHomeNode.Visible = false;

                            break;
                        }
                        case Location objLocation:
                        {
                            gpbArmorCommon.Visible = false;
                            gpbArmorMatrix.Visible = false;
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
                                                        .Append('(')
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

                // Buttons
                cmdDeleteGear.Enabled = treGear.SelectedNode?.Tag is ICanRemove;

                IsRefreshing = false;
                flpGear.ResumeLayout();
                return;
            }

            if (treGear.SelectedNode?.Tag is IHasStolenProperty loot && ImprovementManager
                                                                        .GetCachedImprovementListForValueOf(
                                                                            CharacterObject,
                                                                            Improvement.ImprovementType.Nuyen,
                                                                            "Stolen").Count > 0)
            {
                chkGearStolen.Visible = true;
                chkGearStolen.Checked = loot.Stolen;
            }
            else
            {
                chkGearStolen.Visible = false;
            }

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

                // Buttons
                cmdDeleteGear.Enabled = !objGear.IncludedInParent;

                // gpbGearCommon
                lblGearName.Text = objGear.CurrentDisplayNameShort;
                lblGearCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                int intGearMaxRatingValue = objGear.MaxRatingValue;
                if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                {
                    int intGearMinRatingValue = objGear.MinRatingValue;
                    if (intGearMinRatingValue > 0)
                        nudGearRating.Minimum = intGearMinRatingValue;
                    else if (intGearMinRatingValue == 0 && objGear.Name.Contains("Credstick,"))
                        nudGearRating.Minimum = 0;
                    else
                        nudGearRating.Minimum = 1;
                    nudGearRating.Maximum = objGear.MaxRatingValue;
                    nudGearRating.Value = objGear.Rating;
                    nudGearRating.Enabled = nudGearRating.Minimum != nudGearRating.Maximum && string.IsNullOrEmpty(objGear.ParentID);
                }
                else
                {
                    nudGearRating.Minimum = 0;
                    nudGearRating.Maximum = 0;
                    nudGearRating.Enabled = false;
                }

                nudGearQty.Increment = objGear.CostFor;
                if (objGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
                {
                    int intDecimalPlaces = CharacterObjectSettings.MaxNuyenDecimals;
                    if (intDecimalPlaces <= 0)
                    {
                        nudGearQty.DecimalPlaces = 0;
                        nudGearQty.Minimum = 1.0m;
                    }
                    else
                    {
                        nudGearQty.DecimalPlaces = intDecimalPlaces;
                        decimal decMinimum = 1.0m;
                        // Need a for loop instead of a power system to maintain exact precision
                        for (int i = 0; i < intDecimalPlaces; ++i)
                            decMinimum /= 10.0m;
                        nudGearQty.Minimum = decMinimum;
                    }
                }
                else if (objGear.Category == "Currency")
                {
                    nudGearQty.DecimalPlaces = 2;
                    nudGearQty.Minimum = 0.01m;
                }
                else
                {
                    nudGearQty.DecimalPlaces = 0;
                    nudGearQty.Minimum = 1.0m;
                }
                nudGearQty.Value = objGear.Quantity;
                nudGearQty.Enabled = !objGear.IncludedInParent;
                try
                {
                    lblGearCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat + '¥', GlobalSettings.CultureInfo);
                }
                catch (FormatException)
                {
                    lblGearCost.Text = objGear.Cost + '¥';
                }
                lblGearAvail.Text = objGear.DisplayTotalAvail;
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

                if (CharacterObject.BlackMarketDiscount)
                {
                    chkGearBlackMarketDiscount.Enabled = !objGear.IncludedInParent && CharacterObject
                        .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("gear.xml")
                            .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objGear.Category);
                    chkGearBlackMarketDiscount.Checked = objGear.IncludedInParent
                        ? (objGear.Parent as ICanBlackMarketDiscount)?.DiscountCost == true
                        : objGear.DiscountCost;
                }
                else
                {
                    chkGearBlackMarketDiscount.Enabled = false;
                    chkGearBlackMarketDiscount.Checked = false;
                }

                // gpbGearMatrix
                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                lblGearDeviceRating.Text = intDeviceRating.ToString(GlobalSettings.CultureInfo);
                objGear.RefreshMatrixAttributeComboBoxes(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);
                chkGearActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                chkGearActiveCommlink.Visible = objGear.IsCommlink;
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
                treGear.SelectedNode.Text = objGear.CurrentDisplayName;
            }
            else
            {
                gpbGearCommon.Visible = false;
                gpbGearMatrix.Visible = false;

                // Buttons
                cmdDeleteGear.Enabled = treGear.SelectedNode?.Tag is ICanRemove;
            }

            IsRefreshing = false;
            flpGear.ResumeLayout();
        }

        protected override string FormMode => LanguageManager.GetString("Title_CreateNewCharacter");

        /// <summary>
        /// Save the Character.
        /// </summary>
        public override ValueTask<bool> SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false)
        {
            return base.SaveCharacter(blnNeedConfirm, blnDoCreated || chkCharacterCreated.Checked);
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        /// <param name="blnDoCreated">If True, forces the character to be saved in Career Mode (if possible to do so).</param>
        public override ValueTask<bool> SaveCharacterAs(bool blnDoCreated = false)
        {
            return base.SaveCharacterAs(blnDoCreated || chkCharacterCreated.Checked);
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public override async Task<bool> SaveCharacterAsCreated()
        {
            using (new CursorWait(this))
            {
                // If the character was built with Karma, record their staring Karma amount (if any).
                if (CharacterObject.Karma > 0)
                {
                    ExpenseLogEntry objKarma = new ExpenseLogEntry(CharacterObject);
                    objKarma.Create(CharacterObject.Karma, await LanguageManager.GetStringAsync("Label_SelectBP_StartingKarma"), ExpenseType.Karma, DateTime.Now);
                    CharacterObject.ExpenseEntries.AddWithSort(objKarma);

                    // Create an Undo entry so that the starting Karma amount can be modified if needed.
                    ExpenseUndo objKarmaUndo = new ExpenseUndo();
                    objKarmaUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
                    objKarma.Undo = objKarmaUndo;
                }

                List<CharacterAttrib> lstAttributesToAdd = null;
                if (CharacterObject.MetatypeCategory == "Shapeshifter")
                {
                    lstAttributesToAdd = new List<CharacterAttrib>(AttributeSection.AttributeStrings.Count);
                    XmlDocument xmlDoc = await CharacterObject.LoadDataAsync("metatypes.xml");
                    string strMetavariantXPath = "/chummer/metatypes/metatype[id = "
                                                 + CharacterObject.MetatypeGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                                 + "]/metavariants/metavariant[id = "
                                                 + CharacterObject.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                                 + ']';
                    foreach (CharacterAttrib objOldAttribute in CharacterObject.AttributeSection.AttributeList)
                    {
                        CharacterAttrib objNewAttribute = new CharacterAttrib(CharacterObject, objOldAttribute.Abbrev,
                            CharacterAttrib.AttributeCategory.Shapeshifter);
                        AttributeSection.CopyAttribute(objOldAttribute, objNewAttribute, strMetavariantXPath, xmlDoc);
                        lstAttributesToAdd.Add(objNewAttribute);
                    }

                    foreach (CharacterAttrib objAttributeToAdd in lstAttributesToAdd)
                    {
                        CharacterObject.AttributeSection.AttributeList.Add(objAttributeToAdd);
                    }
                }

                // Create an Expense Entry for Starting Nuyen.
                ExpenseLogEntry objNuyen = new ExpenseLogEntry(CharacterObject);
                objNuyen.Create(CharacterObject.Nuyen, await LanguageManager.GetStringAsync("Title_LifestyleNuyen"), ExpenseType.Nuyen, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objNuyen);

                // Create an Undo entry so that the Starting Nuyen amount can be modified if needed.
                ExpenseUndo objNuyenUndo = new ExpenseUndo();
                objNuyenUndo.CreateNuyen(NuyenExpenseType.ManualAdd, string.Empty);
                objNuyen.Undo = objNuyenUndo;

                CharacterObject.Created = true;

                using (LoadingBar frmProgressBar = await Program.CreateAndShowProgressBarAsync())
                {
                    frmProgressBar.PerformStep(CharacterObject.CharacterName, LoadingBar.ProgressBarTextPatterns.Saving);
                    if (!await CharacterObject.SaveAsync())
                    {
                        CharacterObject.ExpenseEntries.Clear();
                        if (lstAttributesToAdd != null)
                        {
                            foreach (CharacterAttrib objAttributeToAdd in lstAttributesToAdd)
                            {
                                CharacterObject.AttributeSection.AttributeList.Remove(objAttributeToAdd);
                            }
                        }

                        CharacterObject.Created = false;
                        return false;
                    }

                    IsDirty = false;
                }

                _blnIsReopenQueued = true;
                FormClosed += ReopenCharacter;
                Close();
            }
            return true;
        }

        /// <summary>
        /// Open the Select Cyberware window and handle adding to the Tree and Character.
        /// </summary>
        private async ValueTask<bool> PickCyberware(Cyberware objSelectedCyberware, Improvement.ImprovementSource objSource)
        {
            using (SelectCyberware frmPickCyberware = new SelectCyberware(CharacterObject, objSource, objSelectedCyberware))
            {
                List<Improvement> lstImprovements;
                decimal decMultiplier = 1.0m;
                switch (objSource)
                {
                    // Apply the character's Cyberware Essence cost multiplier if applicable.
                    case Improvement.ImprovementSource.Cyberware:
                        {
                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareEssCost);
                            if (lstImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplier);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareEssCostNonRetroactive);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier *= decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
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
                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareEssCost);
                            if (lstImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplier);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareEssCostNonRetroactive);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterESSMultiplier = decMultiplier;
                            }

                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier *= objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.CharacterTotalESSMultiplier *= decMultiplier;
                            }

                            // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.BasicBiowareEssCost);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.BasicBiowareESSMultiplier = decMultiplier;
                            }

                            // Apply the character's Genetech Essence cost multiplier if applicable.
                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.GenetechEssMultiplier);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.GenetechEssMultiplier = decMultiplier;
                            }

                            // Genetech Cost multiplier.
                            lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    CharacterObject, Improvement.ImprovementType.GenetechCostMultiplier);
                            if (lstImprovements.Count != 0)
                            {
                                decMultiplier = 1.0m;
                                foreach (Improvement objImprovement in lstImprovements)
                                {
                                    decMultiplier -= 1.0m - objImprovement.Value / 100.0m;
                                }

                                frmPickCyberware.GenetechCostMultiplier = decMultiplier;
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
                            setLoopDisallowedMounts.AddRange(objLoopCyberware.BlocksMounts.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));
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
                            intValue = dicDisallowedMounts.ContainsKey(strKey + "Right")
                                ? 2 * Math.Min(intValue, dicDisallowedMounts[strKey + "Right"])
                                : 0;
                            if (dicDisallowedMounts.TryGetValue(strKey, out int intExistingValue))
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
                            intValue = dicHasMounts.ContainsKey(strKey + "Right")
                                ? 2 * Math.Min(intValue, dicHasMounts[strKey + "Right"])
                                : 0;
                            if (dicHasMounts.TryGetValue(strKey, out int intExistingValue))
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

                // Create the Cyberware object.
                Cyberware objCyberware = new Cyberware(CharacterObject);

                List<Weapon> lstWeapons = new List<Weapon>(1);
                List<Vehicle> lstVehicles = new List<Vehicle>(1);
                objCyberware.Create(objXmlCyberware, frmPickCyberware.SelectedGrade, objSource, frmPickCyberware.SelectedRating, lstWeapons, lstVehicles, true, true, string.Empty, objSelectedCyberware);
                if (objCyberware.InternalId.IsEmptyGuid())
                {
                    objCyberware.Dispose();
                    return false;
                }

                if (objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID)
                {
                    CharacterObject.DecreaseEssenceHole(objCyberware.Rating);
                }
                else if (objCyberware.SourceID == Cyberware.EssenceHoleGUID)
                {
                    CharacterObject.IncreaseEssenceHole(objCyberware.Rating);
                }
                else
                {
                    objCyberware.DiscountCost = frmPickCyberware.BlackMarketDiscount;
                    objCyberware.PrototypeTranshuman = frmPickCyberware.PrototypeTranshuman;

                    // Apply the ESS discount if applicable.
                    if (CharacterObjectSettings.AllowCyberwareESSDiscounts)
                        objCyberware.ESSDiscount = frmPickCyberware.SelectedESSDiscount;

                    if (frmPickCyberware.FreeCost)
                        objCyberware.Cost = "0";

                    if (objSelectedCyberware != null)
                        objSelectedCyberware.Children.Add(objCyberware);
                    else
                        CharacterObject.Cyberware.Add(objCyberware);

                    CharacterObject.Weapons.AddRange(lstWeapons);
                    CharacterObject.Vehicles.AddRange(lstVehicles);
                }

                return frmPickCyberware.AddAgain;
            }
        }

        /// <summary>
        /// Select a piece of Gear to be added to the character.
        /// </summary>
        private async ValueTask<bool> PickGear(string strSelectedId)
        {
            bool blnNullParent = false;
            Gear objSelectedGear = CharacterObject.Gear.DeepFindById(strSelectedId);
            Location objLocation = null;
            if (objSelectedGear == null)
            {
                blnNullParent = true;
                objLocation =
                    CharacterObject.GearLocations.FirstOrDefault(location => location.InternalId == strSelectedId);
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

                using (SelectGear frmPickGear = new SelectGear(CharacterObject, objSelectedGear?.ChildAvailModifier ?? 0, objSelectedGear?.ChildCostMultiplier ?? 1, objSelectedGear, strCategories))
                {
                    if (!blnNullParent && (!string.IsNullOrEmpty(objSelectedGear.Capacity) && !objSelectedGear.Capacity.Contains('[') || objSelectedGear.Capacity.Contains("/[")))
                    {
                        // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                        frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;
                        if (!string.IsNullOrEmpty(strCategories))
                            frmPickGear.ShowNegativeCapacityOnly = true;
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
                    objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, objSelectedGear?.Equipped != false);
                    if (objGear.InternalId.IsEmptyGuid())
                        return frmPickGear.AddAgain;
                    objGear.Quantity = frmPickGear.SelectedQty;

                    // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                    if (CharacterObject.ActiveCommlink == null && objGear.IsCommlink)
                    {
                        objGear.SetActiveCommlink(CharacterObject, true);
                    }

                    // reduce the cost for Black Market Pipeline
                    objGear.DiscountCost = frmPickGear.BlackMarketDiscount;
                    // Reduce the cost for Do It Yourself components.
                    if (frmPickGear.DoItYourself)
                        objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                    // If the item was marked as free, change its cost.
                    if (frmPickGear.FreeCost)
                    {
                        objGear.Cost = "0";
                    }

                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }

                    ICollection<Gear> destinationGear =
                        blnNullParent ? CharacterObject.Gear : objSelectedGear.Children;
                    bool blnMatchFound = false;
                    foreach (Gear objExistingGear in destinationGear)
                    {
                        if (objExistingGear.Location == objLocation
                            && objGear.IsIdenticalToOtherGear(objExistingGear, true)
                            && Program.ShowMessageBox(this,
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_MergeIdentical"), objGear.CurrentDisplayNameShort),
                                await LanguageManager.GetStringAsync("MessageTitle_MergeIdentical"),
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // A match was found, so increase the quantity instead.
                            objExistingGear.Quantity += objGear.Quantity;
                            blnMatchFound = true;
                            break;
                        }
                    }

                    if (!blnMatchFound)
                    {
                        objLocation?.Children.Add(objGear);
                        destinationGear.Add(objGear);
                    }
                    else
                    {
                        IsCharacterUpdateRequested = true;
                        IsDirty = true;
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
            ArmorMod objSelectedMod = null;
            Armor objSelectedArmor = CharacterObject.Armor.FindById(strSelectedId);
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
                           ShowArmorCapacityOnly = blnShowArmorCapacityOnly,
                           CapacityDisplayStyle = objSelectedMod != null ? CapacityStyle.Standard : objSelectedArmor.CapacityDisplayStyle
                       })
                {
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                        if (objSelectedGear?.Capacity.Contains('[') == false)
                            frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;
                        else if (objSelectedMod != null)
                            frmPickGear.MaximumCapacity = objSelectedMod.GearCapacityRemaining;
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
                    objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty
                        , objSelectedGear?.Equipped ?? objSelectedMod?.Equipped ?? objSelectedArmor.Equipped);

                    if (objGear.InternalId.IsEmptyGuid())
                        return frmPickGear.AddAgain;

                    objGear.Quantity = frmPickGear.SelectedQty;
                    objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                    if (objSelectedGear != null)
                        objGear.Parent = objSelectedGear;

                    // Reduce the cost for Do It Yourself components.
                    if (frmPickGear.DoItYourself)
                        objGear.Cost = '(' + objGear.Cost + ") * 0.5";
                    // If the item was marked as free, change its cost.
                    if (frmPickGear.FreeCost)
                    {
                        objGear.Cost = "0";
                    }

                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }

                    bool blnMatchFound = false;
                    // If this is Ammunition, see if the character already has it on them.
                    if (objGear.Category == "Ammunition" || !string.IsNullOrEmpty(objGear.AmmoForWeaponType))
                    {
                        foreach (Gear objCharacterGear in CharacterObject.Gear)
                        {
                            if (!objGear.IsIdenticalToOtherGear(objCharacterGear))
                                continue;
                            // A match was found, so increase the quantity instead.
                            objCharacterGear.Quantity += objGear.Quantity;
                            blnMatchFound = true;
                            break;
                        }
                    }

                    // Add the Gear.
                    if (!blnMatchFound)
                    {
                        if (!string.IsNullOrEmpty(objSelectedGear?.Name))
                        {
                            objSelectedGear.Children.Add(objGear);
                        }
                        else if (!string.IsNullOrEmpty(objSelectedMod?.Name))
                        {
                            objSelectedMod.GearChildren.Add(objGear);
                        }
                        else
                        {
                            objSelectedArmor.GearChildren.Add(objGear);
                        }
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
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level <= 0 || !(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
            {
                flpLifestyleDetails.Visible = false;
                cmdDeleteLifestyle.Enabled = treLifestyles.SelectedNode?.Tag is ICanRemove;
                IsRefreshing = false;
                flpLifestyleDetails.ResumeLayout();
                return;
            }

            flpLifestyleDetails.Visible = true;
            cmdDeleteLifestyle.Enabled = true;

            string strSpace = LanguageManager.GetString("String_Space");
            lblLifestyleCost.Text = objLifestyle.TotalMonthlyCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            nudLifestyleMonths.Value = objLifestyle.Increments;
            lblLifestyleStartingNuyen.Text = objLifestyle.Dice.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_D6") + strSpace
                                             + '×' + strSpace + objLifestyle.Multiplier.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            objLifestyle.SetSourceDetail(lblLifestyleSource);
            lblLifestyleTotalCost.Text = objLifestyle.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';

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
            lblLifestyleMonthsLabel.Text = strIncrementString + string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_LifestylePermanent"), objLifestyle.IncrementsRequiredForPermanent);

            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdQualities))
                {
                    sbdQualities.AppendJoin(',' + Environment.NewLine,
                                            objLifestyle.LifestyleQualities.Select(r => r.CurrentFormattedDisplayName));
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

            if (string.IsNullOrEmpty(strSelectedId) || treVehicles.SelectedNode?.Level <= 0 || treVehicles.SelectedNode?.Tag is Location)
            {
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
            if (treVehicles.SelectedNode?.Tag is IHasStolenProperty selectedLoot && ImprovementManager
                    .GetCachedImprovementListForValueOf(
                        CharacterObject,
                        Improvement.ImprovementType.Nuyen,
                        "Stolen").Count > 0)
            {
                chkVehicleStolen.Visible = true;
                chkVehicleStolen.Checked = selectedLoot.Stolen;
            }
            else
            {
                chkVehicleStolen.Visible = false;
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

            if (treVehicles.SelectedNode?.Tag is IHasRating objHasRating)
            {
                lblVehicleRatingLabel.Text = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(objHasRating.RatingLabel));
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
                        lblVehicleName.Text = objVehicle.CurrentDisplayNameShort;
                        lblVehicleCategory.Text = objVehicle.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Visible = false;
                        nudVehicleRating.Visible = false;
                        lblVehicleGearQtyLabel.Visible = false;
                        nudVehicleGearQty.Visible = false;
                        lblVehicleAvail.Text = objVehicle.DisplayTotalAvail;
                        lblVehicleCost.Text = objVehicle.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = !CharacterObjectSettings.BookEnabled("R5");
                        lblVehicleSlots.Visible = !CharacterObjectSettings.BookEnabled("R5");
                        if (!CharacterObjectSettings.BookEnabled("R5"))
                            lblVehicleSlots.Text = objVehicle.Slots.ToString(GlobalSettings.CultureInfo)
                                                   + strSpace + '(' + (objVehicle.Slots - objVehicle.SlotsUsed).ToString(GlobalSettings.CultureInfo)
                                                   + strSpace + LanguageManager.GetString("String_Remaining") + ')';
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("vehicles.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objVehicle.Category);
                            chkWeaponBlackMarketDiscount.Checked = objVehicle.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

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
                // Locate the selected VehicleMod.
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
                    nudVehicleRating.Visible = false;
                    lblVehicleGearQtyLabel.Visible = false;
                    nudVehicleGearQty.Visible = false;
                    lblVehicleAvail.Text = objWeaponMount.DisplayTotalAvail;
                    lblVehicleCost.Text = objWeaponMount.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo);
                    lblVehicleSlotsLabel.Visible = true;
                    lblVehicleSlots.Visible = true;
                    lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString(GlobalSettings.CultureInfo);
                    cmdVehicleCyberwareChangeMount.Visible = false;
                    chkVehicleWeaponAccessoryInstalled.Visible = true;
                    chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Equipped;
                    chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                    chkVehicleIncludedInWeapon.Visible = false;
                    if (CharacterObject.BlackMarketDiscount)
                    {
                        chkWeaponBlackMarketDiscount.Enabled = !objWeaponMount.IncludedInVehicle && CharacterObject
                            .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("vehicles.xml")
                                .SelectSingleNodeAndCacheExpression("/chummer/weaponmountcategories")).Contains(objWeaponMount.Category);
                        chkWeaponBlackMarketDiscount.Checked = objWeaponMount.IncludedInVehicle
                            ? objWeaponMount.Parent?.DiscountCost == true
                            : objWeaponMount.DiscountCost;
                    }
                    else
                    {
                        chkWeaponBlackMarketDiscount.Enabled = false;
                        chkWeaponBlackMarketDiscount.Checked = false;
                    }
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
                        lblVehicleRatingLabel.Text = LanguageManager.GetString(objMod.RatingLabel);
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
                            if (int.TryParse(objMod.MaxRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intMaxRating) && intMaxRating > 0)
                            {
                                lblVehicleRatingLabel.Visible = true;
                                // If the Mod is Armor, use the lower of the Mod's maximum Rating and MaxArmor value for the Vehicle instead.
                                nudVehicleRating.Maximum = objMod.Name.StartsWith("Armor,", StringComparison.Ordinal) ? Math.Min(intMaxRating, objMod.Parent.MaxArmor) : intMaxRating;
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
                        else
                        {
                            lblVehicleRatingLabel.Visible = true;
                            nudVehicleRating.Visible = true;
                            nudVehicleRating.Minimum = 1;
                            nudVehicleRating.Maximum = Vehicle.MaxWheels;
                            nudVehicleRating.Value = objMod.Rating;
                            nudVehicleRating.Increment = 1;
                            nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                        }
                        nudVehicleGearQty.Visible = false;
                        lblVehicleGearQtyLabel.Visible = false;
                        lblVehicleAvail.Text = objMod.DisplayTotalAvail;
                        lblVehicleCost.Text = objMod.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        lblVehicleSlots.Text = objMod.CalculatedSlots.ToString(GlobalSettings.CultureInfo);
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objMod.Equipped;
                        chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                        chkVehicleIncludedInWeapon.Visible = false;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objMod.IncludedInVehicle && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("weapons.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer/modcategories")).Contains(objMod.Category);
                            chkWeaponBlackMarketDiscount.Checked = objMod.IncludedInVehicle
                                ? (objMod.WeaponMountParent?.DiscountCost ?? objMod.Parent?.DiscountCost) == true
                                : objMod.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }
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
                        nudVehicleRating.Visible = false;
                        lblVehicleGearQtyLabel.Visible = false;
                        nudVehicleGearQty.Visible = false;
                        lblVehicleAvail.Text = objWeapon.DisplayTotalAvail;
                        lblVehicleCost.Text = objWeapon.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
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
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objWeapon.Equipped;
                        chkVehicleWeaponAccessoryInstalled.Enabled = objWeapon.ParentID != objWeapon.Parent?.InternalId && objWeapon.ParentID != objWeapon.ParentVehicle.InternalId;
                        chkVehicleIncludedInWeapon.Visible = true;
                        chkVehicleIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objWeapon.IncludedInWeapon && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("weapons.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objWeapon.Category);
                            chkWeaponBlackMarketDiscount.Checked = objWeapon.IncludedInWeapon
                                ? objWeapon.Parent?.DiscountCost == true
                                : objWeapon.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

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
                        lblVehicleWeaponDicePool.Text = objWeapon.DicePool.ToString(GlobalSettings.CultureInfo);
                        lblVehicleWeaponDicePool.SetToolTip(objWeapon.DicePoolTooltip);
                        lblVehicleWeaponDicePool.Visible = true;
                        if (objWeapon.RangeType == "Ranged")
                        {
                            lblVehicleWeaponAmmoLabel.Visible = true;
                            lblVehicleWeaponAmmo.Visible = true;
                            lblVehicleWeaponAmmo.Text = objWeapon.DisplayAmmo;
                            lblVehicleWeaponModeLabel.Visible = true;
                            lblVehicleWeaponMode.Visible = true;
                            lblVehicleWeaponMode.Text = objWeapon.DisplayMode;
                            cboVehicleWeaponFiringMode.Visible = true;
                            cboVehicleWeaponFiringMode.SelectedValue = objWeapon.FireMode;

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
                                cboVehicleWeaponFiringMode.Visible = true;
                                cboVehicleWeaponFiringMode.SelectedValue = objWeapon.FireMode;
                            }
                            else
                            {
                                lblVehicleWeaponAmmoLabel.Visible = false;
                                lblVehicleWeaponAmmo.Visible = false;
                                cboVehicleWeaponFiringMode.Visible = false;
                            }
                            lblVehicleWeaponModeLabel.Visible = false;
                            lblVehicleWeaponMode.Visible = false;

                            tlpVehiclesWeaponRanges.Visible = false;
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
                        lblVehicleName.Text = objAccessory.CurrentDisplayName;
                        lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeaponAccessory");
                        if (objAccessory.MaxRating > 0)
                        {
                            lblVehicleRatingLabel.Visible = true;
                            nudVehicleRating.Visible = true;
                            nudVehicleRating.Minimum = 1;
                            nudVehicleRating.Maximum = objAccessory.MaxRating;
                            nudVehicleRating.Value = objAccessory.Rating;
                            nudVehicleRating.Increment = 1;
                            nudVehicleRating.Enabled = !objAccessory.IncludedInWeapon;
                        }
                        else
                        {
                            lblVehicleRatingLabel.Visible = false;
                            nudVehicleRating.Visible = false;
                        }

                        lblVehicleGearQtyLabel.Visible = false;
                        nudVehicleGearQty.Visible = false;
                        lblVehicleAvail.Text = objAccessory.DisplayTotalAvail;
                        lblVehicleCost.Text = objAccessory.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdMount))
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

                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = true;
                        chkVehicleWeaponAccessoryInstalled.Enabled = true;
                        chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Equipped;
                        chkVehicleIncludedInWeapon.Visible = true;
                        chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkWeaponBlackMarketDiscount.Enabled = !objAccessory.IncludedInWeapon && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("weapons.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objAccessory.Parent.Category);
                            chkWeaponBlackMarketDiscount.Checked = objAccessory.IncludedInWeapon
                                ? objAccessory.Parent?.DiscountCost == true
                                : objAccessory.DiscountCost;
                        }
                        else
                        {
                            chkWeaponBlackMarketDiscount.Enabled = false;
                            chkWeaponBlackMarketDiscount.Checked = false;
                        }

                        // gpbVehiclesWeapon
                        lblVehicleWeaponModeLabel.Visible = false;
                        lblVehicleWeaponMode.Visible = false;
                        cboVehicleWeaponFiringMode.Visible = false;
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
                            lblVehicleWeaponDicePool.Visible = false;
                        }
                        else
                        {
                            lblVehicleWeaponDicePoolLabel.Visible = true;
                            lblVehicleWeaponDicePool.Visible = true;
                            lblVehicleWeaponDicePool.Text = objAccessory.DicePool.ToString("+#,0;-#,0;0", GlobalSettings.CultureInfo);
                            lblVehicleWeaponDicePool.SetToolTip(string.Empty);
                        }
                        if (objAccessory.TotalAmmoBonus != 0
                            || (!string.IsNullOrEmpty(objAccessory.ModifyAmmoCapacity)
                                && objAccessory.ModifyAmmoCapacity != "0"))
                        {
                            lblVehicleWeaponAmmoLabel.Visible = true;
                            lblVehicleWeaponAmmo.Visible = true;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdAmmoBonus))
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
                        lblVehicleName.Text = objCyberware.CurrentDisplayName;
                        lblVehicleCategory.Text = objCyberware.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Text = LanguageManager.GetString(objCyberware.RatingLabel);
                        if (objCyberware.MaxRating == 0)
                        {
                            nudVehicleRating.Maximum = 0;
                            nudVehicleRating.Minimum = 0;
                            nudVehicleRating.Value = 0;
                            nudVehicleRating.Visible = false;
                            lblVehicleRatingLabel.Visible = false;
                        }
                        else
                        {
                            nudVehicleRating.Maximum = objCyberware.MaxRating;
                            nudVehicleRating.Minimum = objCyberware.MinRating;
                            nudVehicleRating.Value = objCyberware.Rating;
                            nudVehicleRating.Enabled = nudVehicleRating.Maximum == nudVehicleRating.Minimum && string.IsNullOrEmpty(objCyberware.ParentID);
                            nudVehicleRating.Visible = true;
                            lblVehicleRatingLabel.Visible = true;
                        }
                        lblVehicleGearQtyLabel.Visible = false;
                        nudVehicleGearQty.Visible = false;
                        lblVehicleAvail.Text = objCyberware.DisplayTotalAvail;
                        lblVehicleCost.Text = objCyberware.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        cmdVehicleCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        if (CharacterObject.BlackMarketDiscount && string.IsNullOrEmpty(objCyberware.ParentID))
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = CharacterObject.GenerateBlackMarketMappings(CharacterObject
                                    .LoadDataXPath(objCyberware.SourceType == Improvement.ImprovementSource.Cyberware
                                                       ? "cyberware.xml"
                                                       : "bioware.xml").SelectSingleNodeAndCacheExpression("/chummer"))
                                .Contains(objCyberware.Category);
                            chkCyberwareBlackMarketDiscount.Checked = objCyberware.DiscountCost;
                        }
                        else
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = false;
                            chkCyberwareBlackMarketDiscount.Checked = false;
                        }

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
                        lblVehicleName.Text = objGear.CurrentDisplayNameShort;
                        lblVehicleCategory.Text = objGear.DisplayCategory(GlobalSettings.Language);
                        lblVehicleRatingLabel.Text = LanguageManager.GetString(objGear.RatingLabel);
                        int intGearMaxRatingValue = objGear.MaxRatingValue;
                        if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                        {
                            lblVehicleRatingLabel.Visible = true;
                            nudVehicleRating.Visible = true;
                            nudVehicleRating.Enabled = string.IsNullOrEmpty(objGear.ParentID);
                            nudVehicleRating.Maximum = intGearMaxRatingValue;
                            nudVehicleRating.Value = objGear.Rating;
                        }
                        else
                        {
                            nudVehicleRating.Minimum = 0;
                            nudVehicleRating.Maximum = 0;
                            nudVehicleRating.Visible = false;
                        }
                        nudVehicleGearQty.Enabled = !objGear.IncludedInParent;
                        if (objGear.Name.StartsWith("Nuyen", StringComparison.Ordinal))
                        {
                            int intDecimalPlaces = CharacterObjectSettings.MaxNuyenDecimals;
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
                        nudVehicleGearQty.Visible = true;
                        lblVehicleGearQtyLabel.Visible = true;
                        lblVehicleAvail.Text = objGear.DisplayTotalAvail;
                        lblVehicleCost.Text = objGear.TotalCost.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        lblVehicleSlots.Text = objGear.CalculatedCapacity + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo) +
                                               strSpace + LanguageManager.GetString("String_Remaining") + ')';
                        cmdVehicleCyberwareChangeMount.Visible = false;
                        chkVehicleWeaponAccessoryInstalled.Visible = false;
                        chkVehicleIncludedInWeapon.Visible = false;

                        if (CharacterObject.BlackMarketDiscount)
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = !objGear.IncludedInParent && CharacterObject
                                .GenerateBlackMarketMappings(CharacterObject.LoadDataXPath("gear.xml")
                                    .SelectSingleNodeAndCacheExpression("/chummer")).Contains(objGear.Category);
                            chkCyberwareBlackMarketDiscount.Checked = objGear.IncludedInParent
                                ? (objGear.Parent as ICanBlackMarketDiscount)?.DiscountCost == true
                                : objGear.DiscountCost;
                        }
                        else
                        {
                            chkCyberwareBlackMarketDiscount.Enabled = false;
                            chkCyberwareBlackMarketDiscount.Checked = false;
                        }

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

            IsRefreshing = false;
            flpVehicles.ResumeLayout();
        }

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
                nudDrugQty.Value = objDrug.Quantity;
                nudDrugQty.Visible = true;
                nudDrugQty.Enabled = true;
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
            }
            else
            {
                flpDrugs.Visible = false;
                btnDeleteCustomDrug.Enabled = treArmor.SelectedNode?.Tag is ICanRemove;
            }

            IsRefreshing = false;
            flpDrugs.ResumeLayout();
        }

        /// <summary>
        /// Refresh the information for the currently selected Spell
        /// </summary>
        public void RefreshSelectedSpell()
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
                lblSpellDicePool.Text = objSpell.DicePool.ToString(GlobalSettings.CultureInfo);
                lblSpellDicePool.SetToolTip(objSpell.DicePoolTooltip);
            }
            else
            {
                gpbMagicianSpell.Visible = false;
                cmdDeleteSpell.Enabled = treSpells.SelectedNode?.Tag is ICanRemove;
            }

            IsRefreshing = false;
        }

        /// <summary>
        /// Refresh the information for the currently selected Complex Form.
        /// </summary>
        public void RefreshSelectedComplexForm()
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
                lblComplexFormDicePool.Text = objComplexForm.DicePool.ToString(GlobalSettings.CultureInfo);
                lblComplexFormDicePool.SetToolTip(objComplexForm.DicePoolTooltip);

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
        /// Add or remove the Adapsin Cyberware Grade categories.
        /// </summary>
        public void PopulateCyberwareGradeList(bool blnBioware = false, bool blnIgnoreSecondHand = false, string strForceGrade = "")
        {
            List<Grade> objGradeList = CharacterObject.GetGradeList(blnBioware ? Improvement.ImprovementSource.Bioware : Improvement.ImprovementSource.Cyberware).ToList();
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCyberwareGrades))
            {
                foreach (Grade objWareGrade in objGradeList)
                {
                    if (objWareGrade.Name == "None" && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != "None"))
                        continue;
                    if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                        continue;
                    if (blnBioware)
                    {
                        if (objWareGrade.Adapsin)
                            continue;

                        if (ImprovementManager
                            .GetCachedImprovementListForValueOf(CharacterObject,
                                                                Improvement.ImprovementType.DisableBiowareGrade).Any(
                                x => objWareGrade.Name.Contains(x.ImprovedName)))
                            continue;
                    }
                    else
                    {
                        if (CharacterObject.AdapsinEnabled)
                        {
                            if (!objWareGrade.Adapsin && objGradeList.Any(x => objWareGrade.Name.Contains(x.Name)))
                            {
                                continue;
                            }
                        }
                        else if (objWareGrade.Adapsin)
                            continue;

                        if (ImprovementManager
                            .GetCachedImprovementListForValueOf(CharacterObject,
                                                                Improvement.ImprovementType.DisableCyberwareGrade).Any(
                                x => objWareGrade.Name.Contains(x.ImprovedName)))
                            continue;
                    }

                    if (CharacterObject.BurnoutEnabled)
                    {
                        if (!objWareGrade.Burnout
                            && objGradeList.Any(x => objWareGrade.Burnout && objWareGrade.Name.Contains(x.Name)))
                        {
                            continue;
                        }
                    }
                    else if (objWareGrade.Burnout)
                        continue;

                    if (CharacterObjectSettings.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s))
                        && !CharacterObject.IgnoreRules)
                        continue;

                    lstCyberwareGrades.Add(new ListItem(objWareGrade.Name, objWareGrade.CurrentDisplayName));
                }

                cboCyberwareGrade.BeginUpdate();
                cboCyberwareGrade.PopulateWithListItems(lstCyberwareGrades);
                cboCyberwareGrade.EndUpdate();
            }
        }

        /// <summary>
        /// Check the character and determine if it has broken any of the rules.
        /// </summary>
        /// <returns></returns>
        public bool CheckCharacterValidity(bool blnUseArgBuildPoints = false, int intBuildPoints = 0)
        {
            if (CharacterObject.IgnoreRules)
                return true;

            bool blnValid = true;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdMessage))
            {
                sbdMessage.Append(LanguageManager.GetString("Message_InvalidBeginning"));
                using (new CursorWait(this))
                {
                    // Check if the character has more than 1 Martial Art, not counting qualities. TODO: Make the OTP check an optional rule. Make the Martial Arts limit an optional rule.
                    int intMartialArts = CharacterObject.MartialArts.Count(objArt => !objArt.IsQuality);
                    if (intMartialArts > 1)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t')
                                  .AppendFormat(GlobalSettings.CultureInfo,
                                                LanguageManager.GetString("Message_InvalidPointExcess"),
                                                intMartialArts - 1)
                                  .Append(LanguageManager.GetString("String_Space"))
                                  .Append(LanguageManager.GetString("String_MartialArtsCount"));
                    }

                    // Check if the character has more than 5 Techniques in a Martial Art
                    if (CharacterObject.MartialArts.Count > 0)
                    {
                        int intTechniques = 0;
                        foreach (MartialArt objLoopArt in CharacterObject.MartialArts)
                            intTechniques += objLoopArt.Techniques.Count;
                        if (intTechniques > 5)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t')
                                      .AppendFormat(GlobalSettings.CultureInfo,
                                                    LanguageManager.GetString("Message_InvalidPointExcess"),
                                                    intTechniques - 5).Append(LanguageManager.GetString("String_Space"))
                                      .Append(LanguageManager.GetString("String_TechniquesCount"));
                        }
                    }

                    // if positive points > 25
                    if (CharacterObject.PositiveQualityKarma > CharacterObjectSettings.QualityKarmaLimit
                        && !CharacterObjectSettings.ExceedPositiveQualities)
                    {
                        sbdMessage.AppendLine().Append('\t').AppendFormat(
                            GlobalSettings.CultureInfo, LanguageManager.GetString("Message_PositiveQualityLimit"),
                            CharacterObjectSettings.QualityKarmaLimit);
                        blnValid = false;
                    }

                    // if negative points > 25
                    if (CharacterObject.NegativeQualityLimitKarma > CharacterObjectSettings.QualityKarmaLimit
                        && !CharacterObjectSettings.ExceedNegativeQualities)
                    {
                        sbdMessage.AppendLine().Append('\t').AppendFormat(
                            GlobalSettings.CultureInfo, LanguageManager.GetString("Message_NegativeQualityLimit"),
                            CharacterObjectSettings.QualityKarmaLimit);
                        blnValid = false;
                    }

                    if (CharacterObject.FriendsInHighPlaces)
                    {
                        if (CharacterObject.Contacts.Any(x => x.Connection < 8
                                                              && Math.Max(0, x.Connection) + Math.Max(0, x.Loyalty) > 7
                                                              && !x.Free))
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t')
                                      .Append(LanguageManager.GetString("Message_HighContact"));
                        }
                    }
                    else if (CharacterObject.Contacts.Any(
                                 x => Math.Max(0, x.Connection) + Math.Max(0, x.Loyalty) > 7 && !x.Free))
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').Append(LanguageManager.GetString("Message_HighContact"));
                    }

                    // Check if the character has gone over the Build Point total.
                    if (!blnUseArgBuildPoints)
                        intBuildPoints = CalculateBP(false);
                    int intStagedPurchaseQualityPoints = CharacterObject.Qualities
                                                                        .Where(objQuality =>
                                                                                   objQuality.StagedPurchase
                                                                                   && objQuality.Type
                                                                                   == QualityType.Positive
                                                                                   && objQuality.ContributeToBP)
                                                                        .Sum(x => x.BP);
                    if (intBuildPoints + intStagedPurchaseQualityPoints < 0 && !_blnFreestyle)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t')
                                  .AppendFormat(GlobalSettings.CultureInfo,
                                                LanguageManager.GetString("Message_InvalidPointExcess"),
                                                -(intBuildPoints + intStagedPurchaseQualityPoints))
                                  .Append(LanguageManager.GetString("String_Space"))
                                  .Append(LanguageManager.GetString("String_Karma"));
                    }

                    // if character has more than permitted Metagenic qualities
                    if (CharacterObject.MetagenicLimit > 0)
                    {
                        if (-CharacterObject.MetagenicNegativeQualityKarma > CharacterObject.MetagenicLimit)
                        {
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_OverNegativeMetagenicQualities"),
                                -CharacterObject.MetagenicNegativeQualityKarma, CharacterObject.MetagenicLimit);
                            blnValid = false;
                        }

                        if (CharacterObject.MetagenicPositiveQualityKarma > CharacterObject.MetagenicLimit)
                        {
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_OverPositiveMetagenicQualities"),
                                CharacterObject.MetagenicPositiveQualityKarma, CharacterObject.MetagenicLimit);
                            blnValid = false;
                        }

                        if (-CharacterObject.MetagenicNegativeQualityKarma
                            != CharacterObject.MetagenicPositiveQualityKarma &&
                            -CharacterObject.MetagenicNegativeQualityKarma
                            != CharacterObject.MetagenicPositiveQualityKarma - 1)
                        {
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_MetagenicQualitiesUnbalanced"),
                                -CharacterObject.MetagenicNegativeQualityKarma,
                                CharacterObject.MetagenicPositiveQualityKarma - 1,
                                CharacterObject.MetagenicPositiveQualityKarma);
                            blnValid = false;
                        }
                    }

                    // Check if the character has more attributes at their metatype max than allowed
                    if (CharacterObject.Settings.MaxNumberMaxAttributesCreate
                        < CharacterObject.AttributeSection.AttributeList.Count)
                    {
                        int intCountAttributesAtMax = CharacterObject.AttributeSection.AttributeList.Count(
                            x => x.MetatypeCategory == CharacterAttrib.AttributeCategory.Standard
                                 && x.AtMetatypeMaximum);
                        if (intCountAttributesAtMax > CharacterObject.Settings.MaxNumberMaxAttributesCreate)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                              LanguageManager.GetString(
                                                                                  "Message_TooManyAttributesAtMax"),
                                                                              intCountAttributesAtMax,
                                                                              CharacterObject.Settings
                                                                                  .MaxNumberMaxAttributesCreate);
                        }
                    }

                    int i = CharacterObject.TotalAttributes
                            - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.AttributeList);
                    // Check if the character has gone over on Primary Attributes
                    if (i < 0)
                    {
                        //TODO: ATTACH TO ATTRIBUTE SECTION
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidAttributeExcess"), -i);
                    }

                    i = CharacterObject.TotalSpecial
                        - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.SpecialAttributeList);
                    // Check if the character has gone over on Special Attributes
                    if (i < 0)
                    {
                        //TODO: ATTACH TO ATTRIBUTE SECTION
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidSpecialExcess"), -i);
                    }

                    // Check if the character has gone over on Skill Groups
                    if (CharacterObject.SkillsSection.SkillGroupPoints < 0)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidSkillGroupExcess"),
                                                                          -CharacterObject.SkillsSection
                                                                              .SkillGroupPoints);
                    }

                    // Check if the character has gone over on Active Skills
                    if (CharacterObject.SkillsSection.SkillPoints < 0)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidActiveSkillExcess"),
                                                                          -CharacterObject.SkillsSection.SkillPoints);
                    }

                    // Check if the character has gone over on Knowledge Skills
                    if (CharacterObject.SkillsSection.KnowledgeSkillPointsRemain < 0)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidKnowledgeSkillExcess"),
                                                                          -CharacterObject.SkillsSection
                                                                              .KnowledgeSkillPointsRemain);
                    }

                    if (CharacterObject.SkillsSection.Skills.Any(s => s.Specializations.Count > 1))
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidActiveSkillExcessSpecializations"),
                                                                          -CharacterObject.SkillsSection
                                                                              .KnowledgeSkillPointsRemain);
                        foreach (Skill objSkill in CharacterObject.SkillsSection.Skills.Where(
                                     s => s.Specializations.Count > 1))
                        {
                            sbdMessage.AppendLine().Append(objSkill.CurrentDisplayName)
                                      .Append(LanguageManager.GetString("String_Space")).Append('(')
                                      .AppendJoin(',' + LanguageManager.GetString("String_Space"),
                                                  objSkill.Specializations.Select(x => x.CurrentDisplayName))
                                      .Append(')');
                        }
                    }

                    // Check if the character has gone over the Nuyen limit.
                    decimal decNuyen = CalculateNuyen();
                    if (decNuyen < 0)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t')
                                  .AppendFormat(GlobalSettings.CultureInfo,
                                                LanguageManager.GetString("Message_InvalidNuyenExcess"),
                                                (-decNuyen).ToString(CharacterObjectSettings.NuyenFormat,
                                                                     GlobalSettings.CultureInfo)).Append('¥');
                    }

                    if (CharacterObject.StolenNuyen < 0)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidStolenNuyenExcess"),
                                                                          (-CharacterObject.StolenNuyen).ToString(
                                                                              CharacterObjectSettings.NuyenFormat,
                                                                              GlobalSettings.CultureInfo)).Append('¥');
                    }

                    // Check if the character's Essence is above 0.
                    if (CharacterObject.ESS.MetatypeMaximum > 0)
                    {
                        decimal decEss = CharacterObject.Essence();
                        decimal decExcessEss = 0.0m;
                        // Need to split things up this way because without internal rounding, Essence can be as small as the player wants as long as it is positive
                        // And getting the smallest positive number supported by the decimal type is way trickier than just checking if it's zero or negative
                        if (CharacterObjectSettings.DontRoundEssenceInternally)
                        {
                            if (decEss < 0)
                                decExcessEss = -decEss;
                            else if (decEss == 0)
                                decExcessEss
                                    = 10.0m.RaiseToPower(-CharacterObjectSettings
                                                             .EssenceDecimals); // Hacky, but necessary so that the player knows they need to increase their ESS
                        }
                        else
                        {
                            decimal decMinEss = 10.0m.RaiseToPower(-CharacterObjectSettings.EssenceDecimals);
                            if (decEss < decMinEss)
                                decExcessEss = decMinEss - decEss;
                        }

                        if (decExcessEss > 0)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo, LanguageManager.GetString("Message_InvalidEssenceExcess"),
                                decExcessEss);
                        }
                    }

                    // If the character has the Spells & Spirits Tab enabled, make sure a Tradition has been selected.
                    if ((CharacterObject.MagicianEnabled || CharacterObject.AdeptEnabled)
                        && CharacterObject.MagicTradition.Type != TraditionType.MAG)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t')
                                  .Append(LanguageManager.GetString("Message_InvalidNoTradition"));
                    }

                    // If the character has the Spells & Spirits Tab enabled, make sure a Tradition has been selected.
                    if (CharacterObject.AdeptEnabled
                        && CharacterObject.PowerPointsUsed > CharacterObject.PowerPointsTotal)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                          LanguageManager.GetString(
                                                                              "Message_InvalidPowerPoints"),
                                                                          CharacterObject.PowerPointsUsed
                                                                          - CharacterObject.PowerPointsTotal,
                                                                          CharacterObject.PowerPointsTotal);
                    }

                    // If the character has the Technomancer Tab enabled, make sure a Stream has been selected.
                    if (CharacterObject.TechnomancerEnabled && CharacterObject.MagicTradition.Type != TraditionType.RES)
                    {
                        blnValid = false;
                        sbdMessage.AppendLine().Append('\t')
                                  .Append(LanguageManager.GetString("Message_InvalidNoStream"));
                    }

                    // Check if the character has more than the permitted amount of native languages.
                    int intLanguages
                        = CharacterObject.SkillsSection.KnowledgeSkills.Count(objSkill => objSkill.IsNativeLanguage);

                    int intLanguageLimit = 1 + ImprovementManager
                                               .ValueOf(CharacterObject,
                                                        Improvement.ImprovementType.NativeLanguageLimit)
                                               .StandardRound();

                    if (intLanguages != intLanguageLimit)
                    {
                        if (intLanguages > intLanguageLimit)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                              LanguageManager.GetString(
                                                                                  "Message_OverLanguageLimit"),
                                                                              intLanguages, intLanguageLimit);
                        }
                        else if (Program.ShowMessageBox(this,
                                                                 string.Format(
                                                                     GlobalSettings.CultureInfo,
                                                                     LanguageManager.GetString(
                                                                         "Message_ExtraNativeLanguages")
                                                                     , (intLanguageLimit - intLanguages).ToString(
                                                                         GlobalSettings.CultureInfo)),
                                                                 LanguageManager.GetString(
                                                                     "MessageTitle_ExtraNativeLanguages"),
                                                                 MessageBoxButtons.YesNo,
                                                                 MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            blnValid = false;
                        }
                    }

                    // Check the character's equipment and make sure nothing goes over their set Maximum Availability.
                    // Number of items over the specified Availability the character is allowed to have (typically from the Restricted Gear Quality).
                    Dictionary<int, int> dicRestrictedGearLimits = new Dictionary<int, int>(1);
                    List<Improvement> lstUsedImprovements
                        = ImprovementManager.GetCachedImprovementListForValueOf(
                            CharacterObject, Improvement.ImprovementType.RestrictedGear);
                    bool blnHasRestrictedGearAvailable = lstUsedImprovements.Count != 0;
                    if (blnHasRestrictedGearAvailable)
                    {
                        foreach (Improvement objImprovement in lstUsedImprovements)
                        {
                            int intLoopAvailability = objImprovement.Value.StandardRound();
                            if (dicRestrictedGearLimits.TryGetValue(intLoopAvailability, out int intExistingValue))
                                dicRestrictedGearLimits[intLoopAvailability] = intExistingValue + objImprovement.Rating;
                            else
                                dicRestrictedGearLimits.Add(intLoopAvailability, objImprovement.Rating);
                        }
                    }

                    // Remove all Restricted Gear availabilities with non-positive counts
                    foreach (int intLoopAvailability in dicRestrictedGearLimits.Keys.ToList())
                    {
                        if (dicRestrictedGearLimits.TryGetValue(intLoopAvailability, out int intLoopCount)
                            && intLoopCount <= 0)
                            dicRestrictedGearLimits.Remove(intLoopAvailability);
                    }

                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdAvailItems))
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdRestrictedItems))
                    {
                        int intRestrictedCount = 0;

                        // Gear Availability.
                        foreach (Gear objGear in CharacterObject.Gear)
                        {
                            objGear.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                        ref intRestrictedCount);
                        }

                        // Cyberware Availability.
                        foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                        {
                            objCyberware.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                             ref intRestrictedCount);
                        }

                        // Armor Availability.
                        foreach (Armor objArmor in CharacterObject.Armor)
                        {
                            objArmor.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                         ref intRestrictedCount);
                        }

                        // Weapon Availability.
                        foreach (Weapon objWeapon in CharacterObject.Weapons)
                        {
                            objWeapon.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                          ref intRestrictedCount);
                        }

                        // Vehicle Availability.
                        foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                        {
                            objVehicle.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                           ref intRestrictedCount);
                        }

                        // Make sure the character is not carrying more items over the allowed Avail than they are allowed.
                        if (intRestrictedCount > 0)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(GlobalSettings.CultureInfo,
                                                                              LanguageManager.GetString(
                                                                                  "Message_InvalidAvail"),
                                                                              intRestrictedCount,
                                                                              CharacterObjectSettings
                                                                                  .MaximumAvailability);
                            sbdMessage.Append(sbdAvailItems);
                            if (blnHasRestrictedGearAvailable)
                            {
                                sbdMessage.AppendLine().AppendFormat(GlobalSettings.CultureInfo,
                                                                     LanguageManager.GetString(
                                                                         "Message_RestrictedGearUsed"),
                                                                     sbdRestrictedItems.ToString());
                            }
                        }
                    }

                    // Check for any illegal cyberware grades
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdIllegalCyberwareFromGrade))
                    {
                        foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                        {
                            objCyberware.CheckBannedGrades(sbdIllegalCyberwareFromGrade);
                        }

                        foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                        {
                            foreach (Cyberware objCyberware in objVehicle.Mods.SelectMany(objMod => objMod.Cyberware))
                            {
                                objCyberware.CheckBannedGrades(sbdIllegalCyberwareFromGrade);
                            }

                            foreach (Cyberware objCyberware in objVehicle.WeaponMounts.SelectMany(
                                         objMount => objMount.Mods.SelectMany(objMod => objMod.Cyberware)))
                            {
                                objCyberware.CheckBannedGrades(sbdIllegalCyberwareFromGrade);
                            }
                        }

                        if (sbdIllegalCyberwareFromGrade.Length > 0)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t')
                                      .Append(LanguageManager.GetString("Message_InvalidCyberwareGrades"))
                                      .Append(sbdIllegalCyberwareFromGrade);
                        }
                    }

                    // Cyberware: Prototype Transhuman
                    decimal decPrototypeTranshumanEssenceMax = CharacterObject.PrototypeTranshuman;
                    if (decPrototypeTranshumanEssenceMax > 0)
                    {
                        decimal decPrototypeTranshumanEssenceUsed = CharacterObject.PrototypeTranshumanEssenceUsed;
                        if (decPrototypeTranshumanEssenceMax < decPrototypeTranshumanEssenceUsed)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo, LanguageManager.GetString("Message_OverPrototypeLimit"),
                                decPrototypeTranshumanEssenceUsed.ToString(CharacterObjectSettings.EssenceFormat,
                                                                           GlobalSettings.CultureInfo),
                                decPrototypeTranshumanEssenceMax.ToString(CharacterObjectSettings.EssenceFormat,
                                                                          GlobalSettings.CultureInfo));
                        }
                    }

                    // Check item Capacities if the option is enabled.
                    if (CharacterObjectSettings.EnforceCapacity)
                    {
                        List<string> lstOverCapacity = new List<string>(1);
                        bool blnOverCapacity = false;
                        int intCapacityOver = 0;
                        // Armor Capacity.
                        foreach (Armor objArmor in CharacterObject.Armor.Where(
                                     objArmor => objArmor.CapacityRemaining < 0))
                        {
                            blnOverCapacity = true;
                            lstOverCapacity.Add(objArmor.Name);
                            intCapacityOver++;
                        }

                        // Gear Capacity.
                        foreach (Gear objGear in CharacterObject.Gear)
                        {
                            if (objGear.CapacityRemaining < 0)
                            {
                                blnOverCapacity = true;
                                lstOverCapacity.Add(objGear.Name);
                                intCapacityOver++;
                            }

                            // Child Gear.
                            foreach (Gear objChild in
                                     objGear.Children.Where(objChild => objChild.CapacityRemaining < 0))
                            {
                                blnOverCapacity = true;
                                lstOverCapacity.Add(objChild.Name);
                                intCapacityOver++;
                            }
                        }

                        // Cyberware Capacity.
                        foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                        {
                            if (objCyberware.CapacityRemaining < 0)
                            {
                                blnOverCapacity = true;
                                lstOverCapacity.Add(objCyberware.Name);
                                intCapacityOver++;
                            }

                            // Check plugins.
                            foreach (Cyberware objChild in objCyberware.Children.Where(
                                         objChild => objChild.CapacityRemaining < 0))
                            {
                                blnOverCapacity = true;
                                lstOverCapacity.Add(objChild.Name);
                                intCapacityOver++;
                            }
                        }

                        // Vehicle Capacity.
                        foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                        {
                            if (CharacterObjectSettings.BookEnabled("R5"))
                            {
                                if (objVehicle.IsDrone && CharacterObjectSettings.DroneMods)
                                {
                                    if (objVehicle.DroneModSlotsUsed > objVehicle.DroneModSlots)
                                    {
                                        blnOverCapacity = true;
                                        lstOverCapacity.Add(objVehicle.Name);
                                        intCapacityOver++;
                                    }
                                }
                                else
                                {
                                    if (objVehicle.OverR5Capacity())
                                    {
                                        blnOverCapacity = true;
                                        lstOverCapacity.Add(objVehicle.Name);
                                        intCapacityOver++;
                                    }
                                }
                            }
                            else if (objVehicle.Slots < objVehicle.SlotsUsed)
                            {
                                blnOverCapacity = true;
                                lstOverCapacity.Add(objVehicle.Name);
                                intCapacityOver++;
                            }

                            // Check Vehicle Gear.
                            foreach (Gear objGear in objVehicle.GearChildren)
                            {
                                if (objGear.CapacityRemaining < 0)
                                {
                                    blnOverCapacity = true;
                                    lstOverCapacity.Add(objGear.Name);
                                    intCapacityOver++;
                                }

                                // Check Child Gear.
                                foreach (Gear objChild in objGear.Children.Where(
                                             objChild => objChild.CapacityRemaining < 0))
                                {
                                    blnOverCapacity = true;
                                    lstOverCapacity.Add(objChild.Name);
                                    intCapacityOver++;
                                }
                            }
                        }

                        if (blnOverCapacity)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_CapacityReachedValidate"),
                                intCapacityOver);
                            foreach (string strItem in lstOverCapacity)
                            {
                                sbdMessage.AppendLine().Append("\t- ").Append(strItem);
                            }
                        }
                    }

                    //Check Drone mods for illegalities
                    if (CharacterObjectSettings.BookEnabled("R5"))
                    {
                        List<string> lstDronesIllegalDowngrades = new List<string>(1);
                        bool blnIllegalDowngrades = false;
                        int intIllegalDowngrades = 0;
                        foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                        {
                            if (!objVehicle.IsDrone || !CharacterObjectSettings.DroneMods)
                                continue;
                            foreach (string strModCategory in objVehicle.Mods
                                                                        .Where(objMod => !objMod.IncludedInVehicle
                                                                                   && objMod.Equipped
                                                                                   && objMod.Downgrade)
                                                                        .Select(x => x.Category))
                            {
                                //Downgrades can't reduce a attribute to less than 1 (except Speed which can go to 0)
                                if (strModCategory == "Handling"
                                    && Convert.ToInt32(objVehicle.TotalHandling, GlobalSettings.InvariantCultureInfo)
                                    < 1 ||
                                    strModCategory == "Speed"
                                    && Convert.ToInt32(objVehicle.TotalSpeed, GlobalSettings.InvariantCultureInfo) < 0
                                    ||
                                    strModCategory == "Acceleration"
                                    && Convert.ToInt32(objVehicle.TotalAccel, GlobalSettings.InvariantCultureInfo) < 1
                                    ||
                                    strModCategory == "Body" && objVehicle.TotalBody < 1 ||
                                    strModCategory == "Armor" && objVehicle.TotalArmor < 1 ||
                                    strModCategory == "Sensor" && objVehicle.CalculatedSensor < 1)
                                {
                                    blnIllegalDowngrades = true;
                                    intIllegalDowngrades++;
                                    lstDronesIllegalDowngrades.Add(objVehicle.Name);
                                    break;
                                }
                            }
                        }

                        if (blnIllegalDowngrades)
                        {
                            blnValid = false;
                            sbdMessage.AppendLine().Append('\t').AppendFormat(
                                GlobalSettings.CultureInfo, LanguageManager.GetString("Message_DroneIllegalDowngrade"),
                                intIllegalDowngrades);
                            foreach (string strItem in lstDronesIllegalDowngrades)
                            {
                                sbdMessage.AppendLine().Append("\t- ").Append(strItem);
                            }
                        }
                    }

                    i = CharacterObject.Attributes
                        - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.AttributeList);
                    // Check if the character has gone over on Primary Attributes
                    if (blnValid && i > 0 && Program.ShowMessageBox(this,
                                                                             string.Format(
                                                                                 GlobalSettings.CultureInfo,
                                                                                 LanguageManager.GetString(
                                                                                     "Message_ExtraPoints")
                                                                                 , i.ToString(
                                                                                     GlobalSettings.CultureInfo)
                                                                                 , LanguageManager.GetString("Label_SummaryPrimaryAttributes")),
                                                                             LanguageManager.GetString(
                                                                                 "MessageTitle_ExtraPoints"),
                                                                             MessageBoxButtons.YesNo,
                                                                             MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        blnValid = false;
                    }

                    i = CharacterObject.Special
                        - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.SpecialAttributeList);
                    // Check if the character has gone over on Special Attributes
                    if (blnValid && i > 0 && Program.ShowMessageBox(this,
                                                                             string.Format(
                                                                                 GlobalSettings.CultureInfo,
                                                                                 LanguageManager.GetString(
                                                                                     "Message_ExtraPoints")
                                                                                 , i.ToString(
                                                                                     GlobalSettings.CultureInfo)
                                                                                 , LanguageManager.GetString("Label_SummarySpecialAttributes")),
                                                                             LanguageManager.GetString(
                                                                                 "MessageTitle_ExtraPoints"),
                                                                             MessageBoxButtons.YesNo,
                                                                             MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        blnValid = false;
                    }

                    // Check if the character has gone over on Skill Groups
                    if (blnValid && CharacterObject.SkillsSection.SkillGroupPoints > 0
                                 && Program.ShowMessageBox(this,
                                                                    string.Format(
                                                                        GlobalSettings.CultureInfo,
                                                                        LanguageManager.GetString("Message_ExtraPoints")
                                                                        , CharacterObject.SkillsSection.SkillGroupPoints
                                                                            .ToString(GlobalSettings.CultureInfo)
                                                                        , LanguageManager.GetString("Label_SummarySkillGroups")),
                                                                    LanguageManager.GetString(
                                                                        "MessageTitle_ExtraPoints"),
                                                                    MessageBoxButtons.YesNo,
                                                                    MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        blnValid = false;
                    }

                    // Check if the character has gone over on Active Skills
                    if (blnValid && CharacterObject.SkillsSection.SkillPoints > 0 && Program.ShowMessageBox(
                            this,
                            string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_ExtraPoints")
                                          , CharacterObject.SkillsSection.SkillPoints.ToString(
                                              GlobalSettings.CultureInfo)
                                          , LanguageManager.GetString("Label_SummaryActiveSkills")),
                            LanguageManager.GetString("MessageTitle_ExtraPoints"), MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        blnValid = false;
                    }

                    // Check if the character has gone over on Knowledge Skills
                    if (blnValid && CharacterObject.SkillsSection.KnowledgeSkillPointsRemain > 0
                                 && Program.ShowMessageBox(this,
                                                                    string.Format(
                                                                        GlobalSettings.CultureInfo,
                                                                        LanguageManager.GetString("Message_ExtraPoints")
                                                                        , CharacterObject.SkillsSection
                                                                            .KnowledgeSkillPointsRemain
                                                                            .ToString(GlobalSettings.CultureInfo)
                                                                        , LanguageManager.GetString("Label_SummaryKnowledgeSkills")),
                                                                    LanguageManager.GetString(
                                                                        "MessageTitle_ExtraPoints"),
                                                                    MessageBoxButtons.YesNo,
                                                                    MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        blnValid = false;
                    }
                }

                if (!blnValid && sbdMessage.Length > LanguageManager.GetString("Message_InvalidBeginning").Length)
                    Program.ShowMessageBox(this, sbdMessage.ToString(),
                                                    LanguageManager.GetString("MessageTitle_Invalid"),
                                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return blnValid;
        }

        /// <summary>
        /// Confirm that the character can move to career mode and perform final actions for karma carryover and such.
        /// </summary>
        public async ValueTask<bool> ValidateCharacter()
        {
            int intBuildPoints = CalculateBP(false);

            if (CheckCharacterValidity(true, intBuildPoints))
            {
                // See if the character has any Karma remaining.
                if (intBuildPoints > CharacterObjectSettings.KarmaCarryover)
                {
                    if (!CharacterObject.EffectiveBuildMethodUsesPriorityTables)
                    {
                        if (Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_NoExtraKarma"), intBuildPoints.ToString(GlobalSettings.CultureInfo)),
                                await LanguageManager.GetStringAsync("MessageTitle_ExtraKarma"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            return false;
                    }
                    else if (Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ExtraKarma")
                            , intBuildPoints.ToString(GlobalSettings.CultureInfo)
                            , CharacterObjectSettings.KarmaCarryover.ToString(GlobalSettings.CultureInfo)),
                        await LanguageManager.GetStringAsync("MessageTitle_ExtraKarma"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return false;
                    }
                }
                if (CharacterObject.Nuyen > 5000 && Program.ShowMessageBox(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Message_ExtraNuyen")
                        , CharacterObject.Nuyen.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo)
                        , 5000.ToString(CharacterObjectSettings.NuyenFormat, GlobalSettings.CultureInfo)),
                    await LanguageManager.GetStringAsync("MessageTitle_ExtraNuyen"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return false;
                if (GlobalSettings.CreateBackupOnCareer && chkCharacterCreated.Checked)
                {
                    // Create a pre-Career Mode backup of the character.
                    // Make sure the backup directory exists.
                    if (!Directory.Exists(Path.Combine(Utils.GetStartupPath, "saves", "backup")))
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.Combine(Utils.GetStartupPath, "saves", "backup"));
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_Insufficient_Permissions_Warning"));
                            return false;
                        }
                    }

                    string strNewName = Path.GetFileNameWithoutExtension(CharacterObject.FileName);
                    if (string.IsNullOrEmpty(strNewName))
                    {
                        strNewName = CharacterObject.Alias;
                        if (string.IsNullOrEmpty(strNewName))
                        {
                            strNewName = CharacterObject.Name;
                            if (string.IsNullOrEmpty(strNewName))
                                strNewName = Guid.NewGuid().ToString("N", GlobalSettings.InvariantCultureInfo);
                        }
                    }
                    strNewName += await LanguageManager.GetStringAsync("String_Space") + '(' + await LanguageManager.GetStringAsync("Title_CreateMode") + ").chum5";

                    strNewName = Path.Combine(Utils.GetStartupPath, "saves", "backup", strNewName);

                    using (new CursorWait(this))
                    {
                        using (LoadingBar frmProgressBar = await Program.CreateAndShowProgressBarAsync())
                        {
                            frmProgressBar.PerformStep(CharacterObject.CharacterName, LoadingBar.ProgressBarTextPatterns.Saving);
                            if (!await CharacterObject.SaveAsync(strNewName))
                                return false;
                        }
                    }
                }

                _blnSkipUpdate = true;

                // If the character does not have any Lifestyles, give them the Street Lifestyle.
                if (CharacterObject.Lifestyles.Count == 0)
                {
                    Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                    XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync("lifestyles.xml");
                    XmlNode objXmlLifestyle = objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Street\"]");

                    objLifestyle.Create(objXmlLifestyle);

                    CharacterObject.Lifestyles.Add(objLifestyle);
                }

                decimal decStartingNuyen;
                using (SelectLifestyleStartingNuyen frmStartingNuyen = new SelectLifestyleStartingNuyen(CharacterObject))
                {
                    if (await frmStartingNuyen.ShowDialogSafeAsync(this) != DialogResult.OK)
                        return false;
                    decStartingNuyen = frmStartingNuyen.StartingNuyen;
                }

                // Assign starting values and overflows.
                if (decStartingNuyen < 0)
                    decStartingNuyen = 0;
                if (CharacterObject.Nuyen > 5000)
                    CharacterObject.Nuyen = 5000;
                CharacterObject.Nuyen += decStartingNuyen;
                // See if the character has any Karma remaining.
                if (intBuildPoints > CharacterObjectSettings.KarmaCarryover)
                    CharacterObject.Karma = CharacterObject.EffectiveBuildMethodUsesPriorityTables ? CharacterObjectSettings.KarmaCarryover : 0;
                else
                    CharacterObject.Karma = intBuildPoints;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public override async Task<bool> ConfirmSaveCreatedCharacter()
        {
            return Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ConfirmCreate"),
                await LanguageManager.GetStringAsync("MessageTitle_ConfirmCreate"), MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.No && await ValidateCharacter();
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
            using (XmlNodeList xmlChildrenList = xmlSuiteNode.SelectNodes(strType + "s/" + strType))
            {
                if (xmlChildrenList?.Count > 0)
                {
                    XmlDocument objXmlDocument = CharacterObject.LoadData(strType + ".xml");
                    foreach (XmlNode objXmlChild in xmlChildrenList)
                    {
                        string strName = objXmlChild["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        XmlNode objXmlChildCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strType + "s/" + strType + "[name = " + strName.CleanXPath() + ']');
                        int intChildRating = Convert.ToInt32(objXmlChild["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                        objCyberware.Children.Add(CreateSuiteCyberware(objXmlChild, objXmlChildCyberware, objGrade, intChildRating, eSource));
                    }
                }
            }

            return objCyberware;
        }

        /// <summary>
        /// Add a PACKS Kit to the character.
        /// </summary>
        public async ValueTask<bool> AddPACKSKit()
        {
            XmlNode objXmlKit;
            bool blnAddAgain;
            using (SelectPACKSKit frmPickPACKSKit = new SelectPACKSKit(CharacterObject))
            {
                await frmPickPACKSKit.ShowDialogSafeAsync(this);

                // If the form was canceled, don't do anything.
                if (frmPickPACKSKit.DialogResult == DialogResult.Cancel)
                    return false;

                // Do not create child items for Gear if the chosen Kit is in the Custom category since these items will contain the exact plugins desired.
                //if (frmPickPACKSKit.SelectedCategory == "Custom")
                //blnCreateChildren = false;

                objXmlKit = (await CharacterObject.LoadDataAsync("packs.xml")).SelectSingleNode("/chummer/packs/pack[name = " + frmPickPACKSKit.SelectedKit.CleanXPath() + " and category = " + SelectPACKSKit.SelectedCategory.CleanXPath() + ']');
                blnAddAgain = frmPickPACKSKit.AddAgain;
            }

            if (objXmlKit == null)
                return false;
            const bool blnCreateChildren = true;
            // Update Qualities.
            XmlNode xmlQualities = objXmlKit["qualities"];
            if (xmlQualities != null)
            {
                XmlDocument xmlQualityDocument = await CharacterObject.LoadDataAsync("qualities.xml");

                // Positive and Negative Qualities.
                using (XmlNodeList xmlQualityList = xmlQualities.SelectNodes("*/quality"))
                {
                    if (xmlQualityList?.Count > 0)
                    {
                        foreach (XmlNode objXmlQuality in xmlQualityList)
                        {
                            XmlNode objXmlQualityNode = xmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlQuality.InnerText.CleanXPath() + ']');

                            if (objXmlQualityNode == null)
                                continue;
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality objQuality = new Quality(CharacterObject);
                            string strForceValue = objXmlQuality.Attributes?["select"]?.InnerText ?? string.Empty;

                            objQuality.Create(objXmlQualityNode, QualitySource.Selected, lstWeapons, strForceValue);

                            CharacterObject.Qualities.Add(objQuality);

                            // Add any created Weapons to the character.
                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                CharacterObject.Weapons.Add(objWeapon);
                            }
                        }
                    }
                }
            }

            //TODO: PACKS SKILLS?

            // Select a Martial Art.
            XmlNode xmlSelectMartialArt = objXmlKit["selectmartialart"];
            if (xmlSelectMartialArt != null)
            {
                string strForcedValue = xmlSelectMartialArt.Attributes?["select"]?.InnerText ?? string.Empty;

                using (SelectMartialArt frmPickMartialArt = new SelectMartialArt(CharacterObject)
                {
                    ForcedValue = strForcedValue
                })
                {
                    await frmPickMartialArt.ShowDialogSafeAsync(this);

                    if (frmPickMartialArt.DialogResult != DialogResult.Cancel)
                    {
                        // Open the Martial Arts XML file and locate the selected piece.
                        XmlDocument objXmlMartialArtDocument = await CharacterObject.LoadDataAsync("martialarts.xml");

                        XmlNode objXmlArt = objXmlMartialArtDocument.SelectSingleNode("/chummer/martialarts/martialart[id = " + frmPickMartialArt.SelectedMartialArt.CleanXPath() + ']');

                        MartialArt objMartialArt = new MartialArt(CharacterObject);
                        objMartialArt.Create(objXmlArt);
                        CharacterObject.MartialArts.Add(objMartialArt);
                    }
                }
            }

            // Update Martial Arts.
            XmlNode xmlMartialArts = objXmlKit["martialarts"];
            if (xmlMartialArts != null)
            {
                // Open the Martial Arts XML file and locate the selected art.
                XmlDocument objXmlMartialArtDocument = await CharacterObject.LoadDataAsync("martialarts.xml");

                using (XmlNodeList xmlMartialArtsList = xmlMartialArts.SelectNodes("martialart"))
                {
                    if (xmlMartialArtsList?.Count > 0)
                    {
                        foreach (XmlNode objXmlArt in xmlMartialArtsList)
                        {
                            MartialArt objArt = new MartialArt(CharacterObject);
                            XmlNode objXmlArtNode = objXmlMartialArtDocument.SelectSingleNode(
                                "/chummer/martialarts/martialart[(" + CharacterObjectSettings.BookXPath() +
                                ") and name = " + objXmlArt["name"]?.InnerText.CleanXPath() + ']');
                            if (objXmlArtNode == null)
                                continue;
                            objArt.Create(objXmlArtNode);
                            CharacterObject.MartialArts.Add(objArt);

                            // Check for Techniques.
                            using (XmlNodeList xmlTechniquesList = objXmlArt.SelectNodes("techniques/technique"))
                            {
                                if (xmlTechniquesList?.Count > 0)
                                {
                                    foreach (XmlNode xmlTechnique in xmlTechniquesList)
                                    {
                                        MartialArtTechnique objTechnique = new MartialArtTechnique(CharacterObject);
                                        XmlNode xmlTechniqueNode = objXmlMartialArtDocument.SelectSingleNode(
                                            "/chummer/techniques/technique[(" + CharacterObjectSettings.BookXPath() +
                                            ") and name = " + xmlTechnique["name"]?.InnerText.CleanXPath() + ']');
                                        objTechnique.Create(xmlTechniqueNode);
                                        objArt.Techniques.Add(objTechnique);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /*
            // Update Adept Powers.
            if (objXmlKit["powers"] != null)
            {
                // Open the Powers XML file and locate the selected power.
                XmlDocument objXmlPowerDocument = XmlManager.Load("powers.xml");

                foreach (XmlNode objXmlPower in objXmlKit.SelectNodes("powers/power"))
                {
                    //TODO: Fix
                }
            }
            */

            // Update Complex Forms.
            XmlNode xmlComplexForms = objXmlKit["complexforms"];
            if (xmlComplexForms != null)
            {
                // Open the Programs XML file and locate the selected program.
                XmlDocument objXmlComplexFormDocument = await CharacterObject.LoadDataAsync("complexforms.xml");
                using (XmlNodeList xmlComplexFormsList = xmlComplexForms.SelectNodes("complexform"))
                {
                    if (xmlComplexFormsList?.Count > 0)
                    {
                        foreach (XmlNode objXmlComplexForm in xmlComplexFormsList)
                        {
                            XmlNode objXmlComplexFormNode =
                                objXmlComplexFormDocument.SelectSingleNode("/chummer/complexforms/complexform[(" +
                                                                           CharacterObjectSettings.BookXPath() +
                                                                           ") and name = " +
                                                                           objXmlComplexForm["name"]?.InnerText.CleanXPath() + ']');
                            if (objXmlComplexFormNode != null)
                            {
                                ComplexForm objComplexForm = new ComplexForm(CharacterObject);
                                objComplexForm.Create(objXmlComplexFormNode);

                                CharacterObject.ComplexForms.Add(objComplexForm);
                            }
                        }
                    }
                }
            }

            // Update AI Programs.
            XmlNode xmlPrograms = objXmlKit["programs"];
            if (xmlPrograms != null)
            {
                // Open the Programs XML file and locate the selected program.
                XmlDocument objXmlProgramDocument = await CharacterObject.LoadDataAsync("programs.xml");
                using (XmlNodeList xmlProgramsList = xmlPrograms.SelectNodes("program"))
                {
                    if (xmlProgramsList?.Count > 0)
                    {
                        foreach (XmlNode objXmlProgram in xmlProgramsList)
                        {
                            XmlNode objXmlProgramNode = objXmlProgramDocument.SelectSingleNode(
                                "/chummer/programs/program[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlProgram["name"]?.InnerText.CleanXPath() + ']');
                            if (objXmlProgramNode != null)
                            {
                                AIProgram objProgram = new AIProgram(CharacterObject);
                                objProgram.Create(objXmlProgramNode);

                                CharacterObject.AIPrograms.Add(objProgram);
                            }
                        }
                    }
                }
            }

            // Update Spells.
            XmlNode xmlSpells = objXmlKit["spells"];
            if (xmlSpells != null)
            {
                XmlDocument objXmlSpellDocument = await CharacterObject.LoadDataAsync("spells.xml");
                using (XmlNodeList xmlSpellsList = xmlSpells.SelectNodes("spell"))
                {
                    if (xmlSpellsList?.Count > 0)
                    {
                        foreach (XmlNode objXmlSpell in xmlSpellsList)
                        {
                            string strCategory = objXmlSpell["category"]?.InnerText;
                            string strName = objXmlSpell["name"].InnerText;
                            // Make sure the Spell has not already been added to the character.
                            if (CharacterObject.Spells.Any(x => x.Name == strName && x.Category == strCategory))
                                continue;
                            XmlNode objXmlSpellNode = objXmlSpellDocument.SelectSingleNode(
                                "/chummer/spells/spell[(" +
                                CharacterObjectSettings.BookXPath() + ") and name = " + strName.CleanXPath() + ']');

                            if (objXmlSpellNode == null)
                                continue;

                            Spell objSpell = new Spell(CharacterObject);
                            string strForceValue = objXmlSpell.Attributes?["select"]?.InnerText ?? string.Empty;
                            objSpell.Create(objXmlSpellNode, strForceValue);
                            CharacterObject.Spells.Add(objSpell);
                        }
                    }
                }
            }

            // Update Spirits.
            XmlNode xmlSpirits = objXmlKit["spirits"];
            if (xmlSpirits != null)
            {
                using (XmlNodeList xmlSpiritsList = xmlSpirits.SelectNodes("spirit"))
                {
                    if (xmlSpiritsList?.Count > 0)
                    {
                        foreach (XmlNode objXmlSpirit in xmlSpiritsList)
                        {
                            Spirit objSpirit = new Spirit(CharacterObject)
                            {
                                EntityType = SpiritType.Spirit,
                                Name = objXmlSpirit["name"].InnerText,
                                Force =
                                    Convert.ToInt32(objXmlSpirit["force"].InnerText,
                                        GlobalSettings.InvariantCultureInfo),
                                ServicesOwed = Convert.ToInt32(objXmlSpirit["services"].InnerText,
                                    GlobalSettings.InvariantCultureInfo)
                            };
                            CharacterObject.Spirits.Add(objSpirit);
                        }
                    }
                }
            }

            // Update Lifestyles.
            XmlNode xmlLifestyles = objXmlKit["lifestyles"];
            if (xmlLifestyles != null)
            {
                XmlDocument objXmlLifestyleDocument = await CharacterObject.LoadDataAsync("lifestyles.xml");

                foreach (XmlNode objXmlLifestyle in xmlLifestyles.SelectNodes("lifestyle"))
                {
                    // Create the Lifestyle.
                    XmlNode objXmlLifestyleNode = objXmlLifestyleDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = " + objXmlLifestyle["baselifestyle"].InnerText.CleanXPath() + ']');
                    if (objXmlLifestyleNode == null)
                        continue;
                    Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                    objLifestyle.Create(objXmlLifestyleNode);
                    // This is an Advanced Lifestyle, so build it manually.
                    objLifestyle.CustomName = objXmlLifestyle["name"]?.InnerText ?? string.Empty;
                    objLifestyle.Comforts = Convert.ToInt32(objXmlLifestyle["comforts"]?.InnerText, GlobalSettings.InvariantCultureInfo);
                    objLifestyle.Security = Convert.ToInt32(objXmlLifestyle["security"]?.InnerText, GlobalSettings.InvariantCultureInfo);
                    objLifestyle.Area = Convert.ToInt32(objXmlLifestyle["area"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                    foreach (XmlNode objXmlQuality in objXmlLifestyle.SelectNodes("qualities/quality"))
                    {
                        LifestyleQuality lq = new LifestyleQuality(CharacterObject);
                        lq.Create(objXmlQuality, objLifestyle, CharacterObject, QualitySource.Selected);
                        objLifestyle.LifestyleQualities.Add(lq);
                    }

                    // Add the Lifestyle to the character and Lifestyle Tree.
                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }

            // Update NuyenBP.
            string strNuyenBP = objXmlKit["nuyenbp"]?.InnerText;
            if (!string.IsNullOrEmpty(strNuyenBP) && decimal.TryParse(strNuyenBP, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decAmount))
            {
                //if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma)
                //decAmount *= 2;

                CharacterObject.NuyenBP += decAmount;
            }

            XmlDocument objXmlGearDocument = await CharacterObject.LoadDataAsync("gear.xml");

            // Update Armor.
            XmlNode xmlArmors = objXmlKit["armors"];
            if (xmlArmors != null)
            {
                XmlDocument objXmlArmorDocument = await CharacterObject.LoadDataAsync("armor.xml");
                foreach (XmlNode objXmlArmor in xmlArmors.SelectNodes("armor"))
                {
                    XmlNode objXmlArmorNode = objXmlArmorDocument.SelectSingleNode("/chummer/armors/armor[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlArmor["name"].InnerText.CleanXPath() + ']');
                    if (objXmlArmorNode == null)
                        continue;
                    Armor objArmor = new Armor(CharacterObject);
                    List<Weapon> lstWeapons = new List<Weapon>(1);

                    objArmor.Create(objXmlArmorNode, Convert.ToInt32(objXmlArmor["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo), lstWeapons, false, blnCreateChildren);
                    CharacterObject.Armor.Add(objArmor);

                    // Look for Armor Mods.
                    foreach (XmlNode objXmlMod in objXmlArmor.SelectNodes("mods/mod"))
                    {
                        XmlNode objXmlModNode = objXmlArmorDocument.SelectSingleNode("/chummer/mods/mod[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlMod["name"].InnerText.CleanXPath() + ']');
                        if (objXmlModNode != null)
                        {
                            ArmorMod objMod = new ArmorMod(CharacterObject);
                            int intRating = 0;
                            if (objXmlMod["rating"] != null)
                                intRating = Convert.ToInt32(objXmlMod["rating"].InnerText, GlobalSettings.InvariantCultureInfo);
                            objMod.Create(objXmlModNode, intRating, lstWeapons);

                            foreach (XmlNode objXmlGear in objXmlArmor.SelectNodes("gears/gear"))
                                AddPACKSGear(objXmlGearDocument, objXmlGear, objMod, blnCreateChildren);

                            objArmor.ArmorMods.Add(objMod);
                        }
                    }

                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        CharacterObject.Weapons.Add(objWeapon);
                    }

                    foreach (XmlNode objXmlGear in objXmlArmor.SelectNodes("gears/gear"))
                        AddPACKSGear(objXmlGearDocument, objXmlGear, objArmor, blnCreateChildren);
                }
            }

            // Update Weapons.
            XmlNode xmlWeapons = objXmlKit["weapons"];
            if (xmlWeapons != null)
            {
                XmlDocument objXmlWeaponDocument = await CharacterObject.LoadDataAsync("weapons.xml");

                XmlNodeList xmlWeaponsList = xmlWeapons.SelectNodes("weapon");
                pgbProgress.Visible = true;
                pgbProgress.Value = 0;
                pgbProgress.Maximum = xmlWeaponsList.Count;
                int i = 0;
                foreach (XmlNode objXmlWeapon in xmlWeaponsList)
                {
                    i++;
                    pgbProgress.Value = i;
                    Application.DoEvents();

                    XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlWeapon["name"].InnerText.CleanXPath() + ']');
                    if (objXmlWeaponNode != null)
                    {
                        Weapon objWeapon = new Weapon(CharacterObject);
                        List<Weapon> lstWeapons = new List<Weapon>(1);
                        objWeapon.Create(objXmlWeaponNode, lstWeapons, blnCreateChildren);
                        CharacterObject.Weapons.Add(objWeapon);

                        // Look for Weapon Accessories.
                        foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                        {
                            XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlAccessory["name"].InnerText.CleanXPath() + ']');
                            if (objXmlAccessoryNode == null)
                                continue;
                            WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                            string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                            string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                            objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                            objMod.Parent = objWeapon;

                            objWeapon.WeaponAccessories.Add(objMod);

                            foreach (XmlNode objXmlGear in objXmlAccessory.SelectNodes("gears/gear"))
                                AddPACKSGear(objXmlGearDocument, objXmlGear, objMod, blnCreateChildren);
                        }

                        // Look for an Underbarrel Weapon.
                        XmlNode xmlUnderbarrelNode = objXmlWeapon["underbarrel"];
                        if (xmlUnderbarrelNode != null)
                        {
                            XmlNode objXmlUnderbarrelNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlWeapon["underbarrel"].InnerText.CleanXPath() + ']');
                            if (objXmlUnderbarrelNode == null)
                            {
                                List<Weapon> lstLoopWeapons = new List<Weapon>(1);
                                Weapon objUnderbarrelWeapon = new Weapon(CharacterObject);
                                objUnderbarrelWeapon.Create(objXmlUnderbarrelNode, lstLoopWeapons, blnCreateChildren);
                                objWeapon.UnderbarrelWeapons.Add(objUnderbarrelWeapon);
                                if (!objWeapon.AllowAccessory)
                                    objUnderbarrelWeapon.AllowAccessory = false;

                                foreach (Weapon objLoopWeapon in lstLoopWeapons)
                                {
                                    if (!objWeapon.AllowAccessory)
                                        objLoopWeapon.AllowAccessory = false;
                                    objWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                                }

                                foreach (XmlNode objXmlAccessory in xmlUnderbarrelNode.SelectNodes("accessories/accessory"))
                                {
                                    XmlNode objXmlAccessoryNode =
                                        objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlAccessory["name"].InnerText.CleanXPath() + ']');
                                    if (objXmlAccessoryNode == null)
                                        continue;
                                    WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                                    string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                                    string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                                    objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                                    objMod.Parent = objWeapon;

                                    objUnderbarrelWeapon.WeaponAccessories.Add(objMod);

                                    foreach (XmlNode objXmlGear in objXmlAccessory.SelectNodes("gears/gear"))
                                        AddPACKSGear(objXmlGearDocument, objXmlGear, objMod, blnCreateChildren);
                                }
                            }
                        }

                        foreach (Weapon objLoopWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objLoopWeapon);
                        }
                    }

                    Application.DoEvents();
                }
            }

            XmlDocument objXmlCyberwareDocument = await CharacterObject.LoadDataAsync("cyberware.xml");
            XmlDocument objXmlBiowareDocument = await CharacterObject.LoadDataAsync("bioware.xml");

            // Update Cyberware.
            XmlNode xmlCyberwares = objXmlKit["cyberwares"];
            if (xmlCyberwares != null)
            {
                XmlNodeList xmlCyberwaresList = xmlCyberwares.SelectNodes("cyberware");
                pgbProgress.Visible = true;
                pgbProgress.Value = 0;
                pgbProgress.Maximum = xmlCyberwaresList.Count;

                int i = 0;
                foreach (XmlNode objXmlCyberware in xmlCyberwaresList)
                {
                    i++;
                    pgbProgress.Value = i;
                    Application.DoEvents();

                    AddPACKSCyberware(objXmlCyberwareDocument, objXmlBiowareDocument, objXmlGearDocument, objXmlCyberware, CharacterObject, blnCreateChildren);

                    Application.DoEvents();
                }
            }

            // Update Bioware.
            XmlNode xmlBiowares = objXmlKit["biowares"];
            if (xmlBiowares != null)
            {
                XmlNodeList xmlBiowaresList = xmlBiowares.SelectNodes("bioware");
                pgbProgress.Visible = true;
                pgbProgress.Value = 0;
                pgbProgress.Maximum = xmlBiowaresList.Count;

                int i = 0;
                foreach (XmlNode objXmlBioware in xmlBiowaresList)
                {
                    i++;
                    pgbProgress.Value = i;
                    Application.DoEvents();

                    AddPACKSCyberware(objXmlCyberwareDocument, objXmlBiowareDocument, objXmlGearDocument, objXmlBioware, CharacterObject, blnCreateChildren);

                    Application.DoEvents();
                }
            }

            // Update Gear.
            XmlNode xmlGears = objXmlKit["gears"];
            if (xmlGears != null)
            {
                XmlNodeList xmlGearsList = xmlGears.SelectNodes("gear");
                pgbProgress.Visible = true;
                pgbProgress.Value = 0;
                pgbProgress.Maximum = xmlGearsList.Count;
                int i = 0;

                foreach (XmlNode objXmlGear in xmlGearsList)
                {
                    i++;
                    pgbProgress.Value = i;
                    Application.DoEvents();

                    AddPACKSGear(objXmlGearDocument, objXmlGear, CharacterObject, blnCreateChildren);

                    Application.DoEvents();
                }
            }

            // Update Vehicles.
            XmlNode xmlVehicles = objXmlKit["vehicles"];
            if (xmlVehicles != null)
            {
                XmlDocument objXmlVehicleDocument = await CharacterObject.LoadDataAsync("vehicles.xml");
                XmlNodeList xmlVehiclesList = xmlVehicles.SelectNodes("vehicle");
                pgbProgress.Visible = true;
                pgbProgress.Value = 0;
                pgbProgress.Maximum = xmlVehiclesList.Count;
                int i = 0;

                foreach (XmlNode objXmlVehicle in xmlVehiclesList)
                {
                    i++;
                    pgbProgress.Value = i;
                    Application.DoEvents();

                    Gear objDefaultSensor = null;

                    XmlNode objXmlVehicleNode = objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlVehicle["name"].InnerText.CleanXPath() + ']');
                    if (objXmlVehicleNode == null)
                        continue;
                    Vehicle objVehicle = new Vehicle(CharacterObject);
                    objVehicle.Create(objXmlVehicleNode, blnCreateChildren: blnCreateChildren);
                    CharacterObject.Vehicles.Add(objVehicle);

                    // Grab the default Sensor that comes with the Vehicle.
                    foreach (Gear objSensorGear in objVehicle.GearChildren)
                    {
                        if (objSensorGear.Category == "Sensors" && objSensorGear.Cost == "0" && objSensorGear.Rating == 0)
                        {
                            objDefaultSensor = objSensorGear;
                            break;
                        }
                    }

                    // Add any Vehicle Mods.
                    foreach (XmlNode objXmlMod in objXmlVehicle.SelectNodes("mods/mod"))
                    {
                        XmlNode objXmlModNode = objXmlVehicleDocument.SelectSingleNode("/chummer/mods/mod[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlMod["name"].InnerText.CleanXPath() + ']');
                        if (objXmlModNode == null)
                            continue;
                        int intRating = 0;
                        objXmlMod.TryGetInt32FieldQuickly("rating", ref intRating);
                        int intMarkup = 0;
                        objXmlMod.TryGetInt32FieldQuickly("markup", ref intMarkup);
                        VehicleMod objMod = new VehicleMod(CharacterObject);
                        objMod.Create(objXmlModNode, intRating, objVehicle, intMarkup);
                        objVehicle.Mods.Add(objMod);

                        foreach (XmlNode objXmlCyberware in objXmlMod.SelectNodes("cyberwares/cyberware"))
                            AddPACKSCyberware(objXmlCyberwareDocument, objXmlBiowareDocument, objXmlGearDocument, objXmlCyberware, objMod, blnCreateChildren);
                    }

                    // Add any Vehicle Gear.
                    foreach (XmlNode objXmlGear in objXmlVehicle.SelectNodes("gears/gear"))
                    {
                        Gear objGear = AddPACKSGear(objXmlGearDocument, objXmlGear, objVehicle, blnCreateChildren);
                        // If this is a Sensor, it will replace the Vehicle's base sensor, so remove it.
                        if (objGear?.Category == "Sensors" && objGear.Cost == "0" && objGear.Rating == 0)
                        {
                            objVehicle.GearChildren.Remove(objDefaultSensor);
                        }
                    }

                    // Add any Vehicle Weapons.
                    if (objXmlVehicle["weapons"] != null)
                    {
                        XmlDocument objXmlWeaponDocument = await CharacterObject.LoadDataAsync("weapons.xml");

                        foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
                        {
                            Weapon objWeapon = new Weapon(CharacterObject);

                            List<Weapon> lstSubWeapons = new List<Weapon>(1);
                            XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlWeapon["name"].InnerText.CleanXPath() + ']');
                            if (objXmlWeaponNode == null)
                                continue;
                            objWeapon.ParentVehicle = objVehicle;
                            objWeapon.Create(objXmlWeaponNode, lstSubWeapons, blnCreateChildren);

                            // Find the first Weapon Mount in the Vehicle.
                            foreach (VehicleMod objMod in objVehicle.Mods)
                            {
                                if (objMod.Name.Contains("Weapon Mount") || !string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category))
                                {
                                    objMod.Weapons.Add(objWeapon);
                                    foreach (Weapon objSubWeapon in lstSubWeapons)
                                        objMod.Weapons.Add(objSubWeapon);
                                    break;
                                }
                            }

                            // Look for Weapon Accessories.
                            foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                            {
                                XmlNode objXmlAccessoryNode =
                                    objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlAccessory["name"].InnerText.CleanXPath() + ']');
                                if (objXmlAccessoryNode == null)
                                    continue;
                                WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                                string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                                string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                                objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                                objMod.Parent = objWeapon;

                                objWeapon.WeaponAccessories.Add(objMod);
                            }

                            // Look for an Underbarrel Weapon.
                            XmlNode xmlUnderbarrelNode = objXmlWeapon["underbarrel"];
                            if (xmlUnderbarrelNode != null)
                            {
                                XmlNode objXmlUnderbarrelNode =
                                    objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlWeapon["underbarrel"].InnerText.CleanXPath() + ']');
                                if (objXmlUnderbarrelNode != null)
                                {
                                    List<Weapon> lstLoopWeapons = new List<Weapon>(1);
                                    Weapon objUnderbarrelWeapon = new Weapon(CharacterObject);
                                    objUnderbarrelWeapon.Create(objXmlUnderbarrelNode, lstLoopWeapons, blnCreateChildren);
                                    objWeapon.UnderbarrelWeapons.Add(objUnderbarrelWeapon);
                                    if (!objWeapon.AllowAccessory)
                                        objUnderbarrelWeapon.AllowAccessory = false;

                                    foreach (Weapon objLoopWeapon in lstLoopWeapons)
                                    {
                                        if (!objWeapon.AllowAccessory)
                                            objLoopWeapon.AllowAccessory = false;
                                        objWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                                    }

                                    foreach (XmlNode objXmlAccessory in xmlUnderbarrelNode.SelectNodes("accessories/accessory"))
                                    {
                                        XmlNode objXmlAccessoryNode =
                                            objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectSettings.BookXPath() + ") and name = " + objXmlAccessory["name"].InnerText.CleanXPath() + ']');
                                        if (objXmlAccessoryNode == null)
                                            continue;
                                        WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                                        string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                                        string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                                        objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                                        objMod.Parent = objWeapon;

                                        objUnderbarrelWeapon.WeaponAccessories.Add(objMod);

                                        foreach (XmlNode objXmlGear in objXmlAccessory.SelectNodes("gears/gear"))
                                            AddPACKSGear(objXmlGearDocument, objXmlGear, objMod, blnCreateChildren);
                                    }
                                }
                            }
                        }
                    }

                    Application.DoEvents();
                }
            }

            pgbProgress.Visible = false;

            return blnAddAgain;
        }

        /// <summary>
        /// Create a PACKS Kit from the character.
        /// </summary>
        public async ValueTask CreatePACKSKit()
        {
            using (CreatePACKSKit frmBuildPACKSKit = new CreatePACKSKit(CharacterObject))
                await frmBuildPACKSKit.ShowDialogSafeAsync(this);
        }

        /// <summary>
        /// Update the karma cost tooltip for Initiation/Submersion.
        /// </summary>
        private void UpdateInitiationCost()
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
        /// Change the character's Metatype or priority selection.
        /// </summary>
        public async ValueTask ChangeMetatype()
        {
            if (CharacterObject.EffectiveBuildMethodUsesPriorityTables)
            {
                using (SelectMetatypePriority frmSelectMetatype = new SelectMetatypePriority(CharacterObject))
                {
                    await frmSelectMetatype.ShowDialogSafeAsync(this);
                    if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                        return;
                }
            }
            else
            {
                using (SelectMetatypeKarma frmSelectMetatype = new SelectMetatypeKarma(CharacterObject))
                {
                    await frmSelectMetatype.ShowDialogSafeAsync(this);
                    if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                        return;
                }
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        /// <summary>
        /// Create a Cyberware Suite from the Cyberware the character currently has.
        /// </summary>
        private async ValueTask CreateCyberwareSuite(Improvement.ImprovementSource objSource)
        {
            // Make sure all of the Cyberware the character has is of the same grade.
            string strGrade = string.Empty;
            foreach (Cyberware objCyberware in CharacterObject.Cyberware)
            {
                if (objCyberware.SourceType == objSource)
                {
                    if (string.IsNullOrEmpty(strGrade))
                        strGrade = objCyberware.Grade.ToString();
                    else if (strGrade != objCyberware.Grade.ToString())
                    {
                        Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_CyberwareGradeMismatch"), await LanguageManager.GetStringAsync("MessageTitle_CyberwareGradeMismatch"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            // The character has no Cyberware!
            if (string.IsNullOrEmpty(strGrade))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_NoCyberware"), await LanguageManager.GetStringAsync("MessageTitle_NoCyberware"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            using (CreateCyberwareSuite frmBuildCyberwareSuite = new CreateCyberwareSuite(CharacterObject, objSource))
                await frmBuildCyberwareSuite.ShowDialogSafeAsync(this);
        }

        /// <summary>
        /// Set the ToolTips from the Language file.
        /// </summary>
        private void SetTooltips()
        {
            // Common Tab.
            lblAttributes.SetToolTip(LanguageManager.GetString("Tip_CommonAttributes"));
            lblAttributesBase.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesBase"));
            lblAttributesAug.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesAug"));
            lblAttributesMetatype.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesMetatypeLimits"));
            lblNuyen.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_CommonNuyen"),
                CharacterObjectSettings.ChargenKarmaToNuyenExpression
                    .Replace("{Karma}", LanguageManager.GetString("String_Karma"))
                    .Replace("{PriorityNuyen}", LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen"))));
            // Armor Tab.
            chkArmorEquipped.SetToolTip(LanguageManager.GetString("Tip_ArmorEquipped"));
            // Gear Tab.
            chkGearActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            chkCyberwareActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            // Vehicles Tab.
            chkVehicleWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled"));
            chkVehicleActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink"));
            lblVehiclePowertrainLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            lblVehicleCosmeticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            lblVehicleElectromagneticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            lblVehicleBodymodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            lblVehicleWeaponsmodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            lblVehicleProtectionLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity"));
            // Character Info Tab.
            chkCharacterCreated.SetToolTip(LanguageManager.GetString("Tip_CharacterCreated"));
            // Build Point Summary Tab.
            lblBuildPrimaryAttributes.SetToolTip(LanguageManager.GetString("Tip_CommonAttributes"));
            lblBuildPositiveQualities.SetToolTip(LanguageManager.GetString("Tip_BuildPositiveQualities"));
            lblBuildNegativeQualities.SetToolTip(LanguageManager.GetString("Tip_BuildNegativeQualities"));
            lblBuildContacts.SetToolTip(string.Format(GlobalSettings.CultureInfo,
                LanguageManager.GetString("Tip_CommonContacts"),
                CharacterObjectSettings.KarmaContact.ToString(GlobalSettings.CultureInfo)));
            lblBuildEnemies.SetToolTip(LanguageManager.GetString("Tip_CommonEnemies"));
            lblBuildNuyen.SetToolTip(string.Format(GlobalSettings.CultureInfo,
                LanguageManager.GetString("Tip_CommonNuyen"),
                CharacterObjectSettings.ChargenKarmaToNuyenExpression
                    .Replace("{Karma}", LanguageManager.GetString("String_Karma"))
                    .Replace("{PriorityNuyen}", LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen"))));
            lblBuildSkillGroups.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_SkillsSkillGroups"), CharacterObjectSettings.KarmaImproveSkillGroup.ToString(GlobalSettings.CultureInfo)));
            lblBuildActiveSkills.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_SkillsActiveSkills"), CharacterObjectSettings.KarmaImproveActiveSkill.ToString(GlobalSettings.CultureInfo), CharacterObjectSettings.KarmaSpecialization.ToString(GlobalSettings.CultureInfo)));
            lblBuildKnowledgeSkills.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_SkillsKnowledgeSkills"), CharacterObjectSettings.KarmaImproveKnowledgeSkill.ToString(GlobalSettings.CultureInfo), CharacterObjectSettings.KarmaKnowledgeSpecialization.ToString(GlobalSettings.CultureInfo)));
            lblBuildSpells.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_SpellsSelectedSpells"), CharacterObjectSettings.KarmaSpell.ToString(GlobalSettings.CultureInfo)));
            lblBuildSpirits.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_SpellsSpirits"), CharacterObjectSettings.KarmaSpirit.ToString(GlobalSettings.CultureInfo)));
            lblBuildSprites.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_TechnomancerSprites"), CharacterObjectSettings.KarmaSpirit.ToString(GlobalSettings.CultureInfo)));
            lblBuildComplexForms.SetToolTip(string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_TechnomancerComplexForms"), CharacterObjectSettings.KarmaNewComplexForm.ToString(GlobalSettings.CultureInfo)));
            // Other Info Tab.
            lblCMPhysicalLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMPhysical"));
            lblCMStunLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMStun"));
            lblINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherInitiative"));
            lblMatrixINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherMatrixInitiative"));
            lblAstralINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherAstralInitiative"));
            lblArmorLabel.SetToolTip(LanguageManager.GetString("Tip_OtherArmor"));
            lblESS.SetToolTip(LanguageManager.GetString("Tip_OtherEssence"));
            lblRemainingNuyenLabel.SetToolTip(LanguageManager.GetString("Tip_OtherNuyen"));
            lblMovementLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMovement"));
            lblSwimLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSwim"));
            lblFlyLabel.SetToolTip(LanguageManager.GetString("Tip_OtherFly"));
            lblLiftCarryLimitsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarryLimits"));
            lblComposureLabel.SetToolTip(LanguageManager.GetString("Tip_OtherComposure"));
            lblSurpriseLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSurprise"));
            lblJudgeIntentionsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherJudgeIntentions"));
            lblLiftCarryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarry"));
            lblMemoryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMemory"));
        }

        /// <summary>
        /// Recheck all mods to see if Sensor has changed.
        /// </summary>
        /// <param name="objVehicle">Vehicle to modify.</param>
        private void UpdateSensor(Vehicle objVehicle)
        {
            foreach (Gear objGear in objVehicle.GearChildren)
            {
                if (objGear.Category != "Sensors" || objGear.Name != "Sensor Array" || !objGear.IncludedInParent)
                    continue;
                // Update the name of the item in the TreeView.
                TreeNode objNode = treVehicles.FindNode(objGear.InternalId);
                if (objNode != null)
                    objNode.Text = objGear.CurrentDisplayName;
            }
        }

        /// <summary>
        /// Enable/Disable the Paste Menu and ToolStrip items as appropriate.
        /// </summary>
        private void RefreshPasteStatus(object sender, EventArgs e)
        {
            bool blnPasteEnabled = false;
            bool blnCopyEnabled = false;

            if (tabCharacterTabs.SelectedTab == tabStreetGear)
            {
                // Lifestyle Tab.
                if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                {
                    blnPasteEnabled = GlobalSettings.ClipboardContentType == ClipboardContentType.Lifestyle;
                    blnCopyEnabled = treLifestyles.SelectedNode?.Tag is Lifestyle;
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor && treArmor.SelectedNode?.Tag is IHasInternalId strSelectedId)
                {
                    blnPasteEnabled = GlobalSettings.ClipboardContentType == ClipboardContentType.Armor ||
                                      GlobalSettings.ClipboardContentType == ClipboardContentType.Gear && (CharacterObject.Armor.Any(x => x.InternalId == strSelectedId.InternalId) ||
                                          CharacterObject.Armor.FindArmorMod(strSelectedId.InternalId) != null ||
                                          CharacterObject.Armor.FindArmorGear(strSelectedId.InternalId) != null);
                    blnCopyEnabled = CharacterObject.Armor.Any(x => x.InternalId == strSelectedId.InternalId) || CharacterObject.Armor.FindArmorGear(strSelectedId.InternalId) != null;
                }

                // Weapons Tab.
                if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    string strSelectedId = treWeapons.SelectedNode?.Tag.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        switch (GlobalSettings.ClipboardContentType)
                        {
                            case ClipboardContentType.Weapon:
                                blnPasteEnabled = true;
                                break;

                            case ClipboardContentType.Gear:
                            case ClipboardContentType.WeaponAccessory:
                                blnPasteEnabled =
                                    treWeapons.SelectedNode?.Tag is ICanPaste objSelected && objSelected.AllowPasteXml;
                                break;
                        }

                        //TODO: ICanCopy interface? If weapon comes from something else == false, etc.
                        blnCopyEnabled = treWeapons.SelectedNode?.Tag is Weapon || treWeapons.SelectedNode?.Tag is Gear;
                    }
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear && treGear.SelectedNode?.Tag is IHasInternalId)
                {
                    blnPasteEnabled = GlobalSettings.ClipboardContentType == ClipboardContentType.Gear;
                    blnCopyEnabled = treGear.SelectedNode?.Tag is Gear;
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                blnPasteEnabled = treCyberware.SelectedNode?.Tag is ICanPaste selected && selected.AllowPasteXml || GlobalSettings.ClipboardContentType == ClipboardContentType.Cyberware;
                blnCopyEnabled = treCyberware.SelectedNode?.Tag is Gear || treCyberware.SelectedNode?.Tag is Cyberware;
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles && treVehicles.SelectedNode?.Tag is IHasInternalId)
            {
                switch (GlobalSettings.ClipboardContentType)
                {
                    case ClipboardContentType.Vehicle:
                        blnPasteEnabled = true;
                        break;

                    case ClipboardContentType.Gear:
                    case ClipboardContentType.Weapon:
                    case ClipboardContentType.WeaponAccessory:
                        {
                            blnPasteEnabled = treVehicles.SelectedNode?.Tag is ICanPaste selected &&
                                              selected.AllowPasteXml;
                        }
                        break;
                }

                // In theory any object that's not a generic string node is valid to copy here. Locations might go screwy?
                blnCopyEnabled = true;
            }

            mnuEditPaste.Enabled = blnPasteEnabled;
            tsbPaste.Enabled = blnPasteEnabled;
            mnuEditCopy.Enabled = blnCopyEnabled;
            tsbCopy.Enabled = blnCopyEnabled;
        }

        private async ValueTask AddCyberwareSuite(Improvement.ImprovementSource objSource)
        {
            using (SelectCyberwareSuite frmPickCyberwareSuite = new SelectCyberwareSuite(CharacterObject, objSource))
            {
                await frmPickCyberwareSuite.ShowDialogSafeAsync(this);

                if (frmPickCyberwareSuite.DialogResult == DialogResult.Cancel)
                    return;

                string strType = objSource == Improvement.ImprovementSource.Cyberware ? "cyberware" : "bioware";
                XmlDocument objXmlDocument = await CharacterObject.LoadDataAsync(strType + ".xml", string.Empty, true);
                XmlNode xmlSuite = frmPickCyberwareSuite.SelectedSuite.IsGuid()
                    ? objXmlDocument.SelectSingleNode("/chummer/suites/suite[id = " + frmPickCyberwareSuite.SelectedSuite.CleanXPath() + ']')
                    : objXmlDocument.SelectSingleNode("/chummer/suites/suite[name = " + frmPickCyberwareSuite.SelectedSuite.CleanXPath() + ']');
                if (xmlSuite == null)
                    return;
                Grade objGrade = Grade.ConvertToCyberwareGrade(xmlSuite["grade"]?.InnerText, objSource, CharacterObject);

                string strXPathPrefix = strType + "s/" + strType;
                // Run through each of the items in the Suite and add them to the character.
                using (XmlNodeList xmlItemList = xmlSuite.SelectNodes(strXPathPrefix))
                {
                    if (xmlItemList?.Count > 0)
                    {
                        foreach (XmlNode xmlItem in xmlItemList)
                        {
                            string strName = xmlItem["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XmlNode objXmlCyberware = objXmlDocument.SelectSingleNode("/chummer/" + strXPathPrefix + "[name = " + strName.CleanXPath() + ']');
                            int intRating = Convert.ToInt32(xmlItem["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

                            Cyberware objCyberware = CreateSuiteCyberware(xmlItem, objXmlCyberware, objGrade, intRating, objSource);
                            CharacterObject.Cyberware.Add(objCyberware);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a piece of Gear that was found in a PACKS Kit.
        /// </summary>
        /// <param name="objXmlGearDocument">XmlDocument that contains the Gear.</param>
        /// <param name="objXmlGear">XmlNode of the Gear to add.</param>
        /// <param name="objParentObject">Object to associate the newly-created items with.</param>
        /// <param name="blnCreateChildren">Whether or not the default plugins for the Gear should be created.</param>
        private Gear AddPACKSGear(XmlDocument objXmlGearDocument, XmlNode objXmlGear, object objParentObject, bool blnCreateChildren)
        {
            XmlNode objXmlGearNode = null;
            string strName = objXmlGear["name"]?.InnerText;
            if (!string.IsNullOrEmpty(strName))
            {
                string strCategory = objXmlGear["category"]?.InnerText;
                if (!string.IsNullOrEmpty(strCategory))
                    objXmlGearNode = objXmlGearDocument.SelectSingleNode(
                        "/chummer/gears/gear[(" + CharacterObjectSettings.BookXPath() + ") and name = " + strName.CleanXPath() +
                        " and category = " + strCategory.CleanXPath() + ']');
                else
                    objXmlGearNode = objXmlGearDocument.SelectSingleNode(
                        "/chummer/gears/gear[(" + CharacterObjectSettings.BookXPath() + ") and name = " + strName.CleanXPath() + ']');
            }

            if (objXmlGearNode == null)
                return null;

            int intRating = Convert.ToInt32(objXmlGear["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            decimal decQty = 1;
            string strQty = objXmlGear["qty"]?.InnerText;
            if (!string.IsNullOrEmpty(strQty))
                decQty = Convert.ToDecimal(strQty, GlobalSettings.InvariantCultureInfo);

            List<Weapon> lstWeapons = new List<Weapon>(1);
            string strForceValue = objXmlGear.SelectSingleNode("name/@select")?.InnerText ?? string.Empty;

            Gear objNewGear = new Gear(CharacterObject);
            objNewGear.Create(objXmlGearNode, intRating, lstWeapons, strForceValue, true, blnCreateChildren);
            objNewGear.Quantity = decQty;

            switch (objParentObject)
            {
                case Character objParentCharacter:
                    objParentCharacter.Gear.Add(objNewGear);
                    break;

                case Gear objParentGear:
                    objParentGear.Children.Add(objNewGear);
                    break;

                case Armor objParentArmor:
                    objParentArmor.GearChildren.Add(objNewGear);
                    break;

                case ArmorMod objParentArmorMod:
                    objParentArmorMod.GearChildren.Add(objNewGear);
                    break;

                case WeaponAccessory objParentWeaponAccessory:
                    objParentWeaponAccessory.GearChildren.Add(objNewGear);
                    break;

                case Cyberware objParentCyberware:
                    objParentCyberware.GearChildren.Add(objNewGear);
                    break;

                case Vehicle objParentVehicle:
                    objNewGear.Parent = objParentVehicle;
                    objParentVehicle.GearChildren.Add(objNewGear);
                    break;
            }

            // Look for child components.
            using (XmlNodeList xmlChildrenList = objXmlGear.SelectNodes("gears/gear"))
            {
                if (xmlChildrenList?.Count > 0)
                {
                    foreach (XmlNode xmlChild in xmlChildrenList)
                    {
                        AddPACKSGear(objXmlGearDocument, xmlChild, objNewGear, blnCreateChildren);
                    }
                }
            }

            // Add any Weapons created by the Gear.
            if (lstWeapons.Count > 0)
            {
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }
            }

            return objNewGear;
        }

        private void AddPACKSCyberware(XmlDocument xmlCyberwareDocument, XmlDocument xmlBiowareDocument, XmlDocument xmlGearDocument, XmlNode xmlCyberware, object objParentObject, bool blnCreateChildren)
        {
            Grade objGrade = Grade.ConvertToCyberwareGrade(xmlCyberware["grade"]?.InnerText, Improvement.ImprovementSource.Cyberware, CharacterObject);

            int intRating = Convert.ToInt32(xmlCyberware["rating"]?.InnerText, GlobalSettings.InvariantCultureInfo);

            Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Cyberware;
            string strName = xmlCyberware["name"]?.InnerText;
            if (string.IsNullOrEmpty(strName))
                return;

            XmlNode objXmlCyberwareNode = xmlCyberwareDocument.SelectSingleNode("/chummer/cyberwares/cyberware[(" + CharacterObjectSettings.BookXPath() + ") and name = " + strName.CleanXPath() + ']');
            if (objXmlCyberwareNode == null)
            {
                eSource = Improvement.ImprovementSource.Bioware;
                objXmlCyberwareNode = xmlBiowareDocument.SelectSingleNode("/chummer/biowares/bioware[(" + CharacterObjectSettings.BookXPath() + ") and name = " + strName.CleanXPath() + ']');
                if (objXmlCyberwareNode == null)
                {
                    return;
                }
            }
            List<Weapon> lstWeapons = new List<Weapon>(1);
            List<Vehicle> lstVehicles = new List<Vehicle>(1);
            Cyberware objCyberware = new Cyberware(CharacterObject);
            objCyberware.Create(objXmlCyberwareNode, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, blnCreateChildren);

            switch (objParentObject)
            {
                case Character objParentCharacter:
                    objParentCharacter.Cyberware.Add(objCyberware);
                    break;

                case Cyberware objParentCyberware:
                    objParentCyberware.Children.Add(objCyberware);
                    break;

                case VehicleMod objParentVehicleMod:
                    objParentVehicleMod.Cyberware.Add(objCyberware);
                    break;
            }

            // Add any children.
            using (XmlNodeList xmlCyberwareList = xmlCyberware.SelectNodes("cyberwares/cyberware"))
            {
                if (xmlCyberwareList?.Count > 0)
                {
                    foreach (XmlNode objXmlChild in xmlCyberwareList)
                        AddPACKSCyberware(xmlCyberwareDocument, xmlBiowareDocument, xmlGearDocument, objXmlChild, objCyberware, blnCreateChildren);
                }
            }

            using (XmlNodeList xmlGearList = xmlCyberware.SelectNodes("gears/gear"))
            {
                if (xmlGearList?.Count > 0)
                {
                    foreach (XmlNode objXmlGear in xmlGearList)
                        AddPACKSGear(xmlGearDocument, objXmlGear, objCyberware, blnCreateChildren);
                }
            }

            if (lstWeapons.Count > 0)
            {
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }
            }

            if (lstVehicles.Count > 0)
            {
                foreach (Vehicle objVehicle in lstVehicles)
                {
                    CharacterObject.Vehicles.Add(objVehicle);
                }
            }
        }

        #endregion Custom Methods

        private async void tsMetamagicAddMetamagic_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
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
            }
        }

        private async void tsMetamagicAddArt_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Art))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode objXmlArt = (await CharacterObject.LoadDataAsync("metamagic.xml")).SelectSingleNode("/chummer/arts/art[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');

                Art objArt = new Art(CharacterObject);

                objArt.Create(objXmlArt, Improvement.ImprovementSource.Metamagic);
                objArt.Grade = objGrade.Grade;
                if (objArt.InternalId.IsEmptyGuid())
                    return;

                CharacterObject.Arts.Add(objArt);
            }
        }

        private async void tsMetamagicAddEnchantment_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Enchantment))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode objXmlArt = (await CharacterObject.LoadDataAsync("spells.xml")).SelectSingleNode("/chummer/spells/spell[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');

                Spell objNewSpell = new Spell(CharacterObject);

                objNewSpell.Create(objXmlArt, string.Empty, false, false, false, Improvement.ImprovementSource.Initiation);
                objNewSpell.Grade = objGrade.Grade;
                if (objNewSpell.InternalId.IsEmptyGuid())
                {
                    objNewSpell.Dispose();
                    return;
                }

                CharacterObject.Spells.Add(objNewSpell);
            }
        }

        private async void tsMetamagicAddRitual_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Ritual))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode objXmlArt = (await CharacterObject.LoadDataAsync("spells.xml")).SelectSingleNode("/chummer/spells/spell[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');

                Spell objNewSpell = new Spell(CharacterObject);

                objNewSpell.Create(objXmlArt, string.Empty, false, false, false, Improvement.ImprovementSource.Initiation);
                objNewSpell.Grade = objGrade.Grade;
                if (objNewSpell.InternalId.IsEmptyGuid())
                {
                    objNewSpell.Dispose();
                    return;
                }

                CharacterObject.Spells.Add(objNewSpell);
            }
        }

        private async void tsInitiationNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes objNotes))
                return;
            await WriteNotes(objNotes, treMetamagic.SelectedNode);
        }

        private async void tsMetamagicAddEnhancement_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            using (SelectArt frmPickArt = new SelectArt(CharacterObject, SelectArt.Mode.Enhancement))
            {
                await frmPickArt.ShowDialogSafeAsync(this);

                // Make sure a value was selected.
                if (frmPickArt.DialogResult == DialogResult.Cancel)
                    return;

                XmlNode objXmlArt = (await CharacterObject.LoadDataAsync("powers.xml")).SelectSingleNode("/chummer/enhancements/enhancement[id = " + frmPickArt.SelectedItem.CleanXPath() + ']');
                if (objXmlArt == null)
                    return;

                Enhancement objEnhancement = new Enhancement(CharacterObject);
                objEnhancement.Create(objXmlArt, Improvement.ImprovementSource.Initiation);
                objEnhancement.Grade = objGrade.Grade;
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
            }
        }

        private void panContacts_Click(object sender, EventArgs e)
        {
            panContacts.Focus();
        }

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

        private void panEnemies_Click(object sender, EventArgs e)
        {
            panEnemies.Focus();
        }

        private async void tsAddTechniqueNotes_Click(object sender, EventArgs e)
        {
            if (!(treMartialArts.SelectedNode?.Tag is IHasNotes objNotes))
                return;
            await WriteNotes(objNotes, treMartialArts.SelectedNode);
        }

        private async void btnCreateBackstory_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                if (_objStoryBuilder == null)
                {
                    _objStoryBuilder = new StoryBuilder(CharacterObject);
                    btnCreateBackstory.Enabled = false;
                }

                CharacterObject.Background = await _objStoryBuilder.GetStory(GlobalSettings.Language);
            }
        }

        private async void mnuSpecialConfirmValidity_Click(object sender, EventArgs e)
        {
            if (CheckCharacterValidity())
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_ValidCharacter"), await LanguageManager.GetStringAsync("MessageTitle_ValidCharacter"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cboPrimaryArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsLoading || IsRefreshing || CharacterObject.Ambidextrous)
                return;
            CharacterObject.PrimaryArm = cboPrimaryArm.SelectedValue.ToString();

            IsDirty = true;
        }

        private void AttributeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshAttributes(pnlAttributes, notifyCollectionChangedEventArgs, lblAttributes, lblKarma.PreferredWidth, lblAttributesAug.PreferredWidth, lblAttributesMetatype.PreferredWidth);
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
        }

        private void QualityCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshQualities(treQualities, cmsQuality, notifyCollectionChangedEventArgs);
        }

        private void MartialArtCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshMartialArts(treMartialArts, cmsMartialArts, cmsTechnique, notifyCollectionChangedEventArgs);
        }

        private void LifestyleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle, e);
        }

        private void ContactCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshContacts(panContacts, panEnemies, panPets, notifyCollectionChangedEventArgs);
        }

        private void SpiritCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshSpirits(panSpirits, panSprites, notifyCollectionChangedEventArgs);
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

        private void DrugCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshDrugs(treCustomDrugs, notifyCollectionChangedEventArgs);
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

        private void mnuSpecialKarmaValue_Click(object sender, EventArgs e)
        {
            Program.ShowMessageBox(this, CharacterObject.CalculateKarmaValue(GlobalSettings.Language, out int _),
                LanguageManager.GetString("MessageTitle_KarmaValue"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    && lstModularMounts.All(x => !string.Equals(x.Value.ToString(), "None", StringComparison.Ordinal)))
                {
                    Program.ShowMessageBox(this,
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
                    Program.ShowMessageBox(this,
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

            CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId,
                                                          out VehicleMod objOldParentVehicleMod);
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

        private void cmdContactsExpansionToggle_Click(object sender, EventArgs e)
        {
            if (panContacts.Controls.Count <= 0)
                return;
            panContacts.SuspendLayout();
            bool toggle = ((ContactControl)panContacts.Controls[0]).Expanded;

            foreach (ContactControl c in panContacts.Controls)
            {
                c.Expanded = !toggle;
            }

            panContacts.ResumeLayout();
        }

        private void cmdSwapContactOrder_Click(object sender, EventArgs e)
        {
            panContacts.FlowDirection = panContacts.FlowDirection == FlowDirection.LeftToRight
                ? FlowDirection.TopDown
                : FlowDirection.LeftToRight;
        }

        private async void tsWeaponLocationAddWeapon_Click(object sender, EventArgs e)
        {
            if (!(treWeapons.SelectedNode?.Tag is Location objLocation))
                return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddWeapon(objLocation);
            }
            while (blnAddAgain);
        }

        private async void tsVehicleLocationAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = await AddVehicle(treVehicles.SelectedNode?.Tag is Location objSelectedNode ? objSelectedNode : null);
            }
            while (blnAddAgain);
        }

        private async void tsEditWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount))
                return;
            using (CreateWeaponMount frmCreateWeaponMount = new CreateWeaponMount(objWeaponMount.Parent, CharacterObject, objWeaponMount))
            {
                await frmCreateWeaponMount.ShowDialogSafeAsync(this);

                if (frmCreateWeaponMount.DialogResult == DialogResult.Cancel)
                    return;
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private async void btnCreateCustomDrug_Click_1(object sender, EventArgs e)
        {
            using (CreateCustomDrug form = new CreateCustomDrug(CharacterObject))
            {
                await form.ShowDialogSafeAsync(this);

                if (form.DialogResult == DialogResult.Cancel)
                    return;

                Drug objCustomDrug = form.CustomDrug;
                CharacterObject.Drugs.Add(objCustomDrug);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

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

        #region Stolen Property Changes

        private void chkDrugStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treCustomDrugs.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkDrugStolen.Checked);
        }

        private void chkCyberwareStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treCyberware.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkCyberwareStolen.Checked);
        }

        private void chkGearStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treGear.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkGearStolen.Checked);
        }

        private void chkArmorStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treArmor.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkArmorStolen.Checked);
        }

        private void chkWeaponStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treWeapons.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkWeaponStolen.Checked);
        }

        private void chkVehicleStolen_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treVehicles.SelectedNode?.Tag is IHasStolenProperty loot))
                return;
            ProcessStolenChanged(loot, chkVehicleStolen.Checked);
        }

        private void ProcessStolenChanged(IHasStolenProperty loot, bool state)
        {
            loot.Stolen = state;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        #endregion Stolen Property Changes

        private void btnDeleteCustomDrug_Click(object sender, EventArgs e)
        {
            if (!(treCustomDrugs.SelectedNode?.Tag is ICanRemove selectedObject))
                return;
            selectedObject.Remove();
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

        private void chkCyberwareBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treCyberware.SelectedNode?.Tag is ICanBlackMarketDiscount objItem))
                return;
            objItem.DiscountCost = chkCyberwareBlackMarketDiscount.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkGearBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treGear.SelectedNode?.Tag is ICanBlackMarketDiscount objItem))
                return;
            objItem.DiscountCost = chkGearBlackMarketDiscount.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkArmorBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treArmor.SelectedNode?.Tag is ICanBlackMarketDiscount objItem))
                return;
            objItem.DiscountCost = chkArmorBlackMarketDiscount.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkWeaponBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treWeapons.SelectedNode?.Tag is ICanBlackMarketDiscount objItem))
                return;
            objItem.DiscountCost = chkWeaponBlackMarketDiscount.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (!(treVehicles.SelectedNode?.Tag is ICanBlackMarketDiscount objItem))
                return;
            objItem.DiscountCost = chkVehicleBlackMarketDiscount.Checked;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private async void mnuFileExport_Click(object sender, EventArgs e)
        {
            using (ExportCharacter frmExportCharacter = new ExportCharacter(CharacterObject))
                await frmExportCharacter.ShowDialogSafeAsync(this);
        }
    }
}
