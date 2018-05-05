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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class frmCreate : CharacterShared
    {
        // Set the default culture to en-US so we work with decimals correctly.

        private bool _blnSkipRefresh;
        private bool _blnSkipUpdate;
        private bool _blnLoading = true;
        private bool _blnSkipToolStripRevert;
        private bool _blnReapplyImprovements;
        private bool _blnFreestyle;
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnDraggingGear;
        private StoryBuilder _objStoryBuilder;
        //private readonly Stopwatch PowerPropertyChanged_StopWatch = Stopwatch.StartNew();
        //private readonly Stopwatch SkillPropertyChanged_StopWatch = Stopwatch.StartNew();

        #region Form Events
        [Obsolete("This constructor is for use by form designers only.", true)]
        public frmCreate()
        {
            InitializeComponent();
        }

        public frmCreate(Character objCharacter) : base(objCharacter)
        {
            InitializeComponent();

            GlobalOptions.ClipboardChanged += RefreshPasteStatus;
            tabStreetGearTabs.MouseWheel += ShiftTabsOnMouseScroll;
            tabPeople.MouseWheel += ShiftTabsOnMouseScroll;
            tabInfo.MouseWheel += ShiftTabsOnMouseScroll;
            tabCharacterTabs.MouseWheel += ShiftTabsOnMouseScroll;

            Program.MainForm.OpenCharacterForms.Add(this);

            // Add EventHandlers for the various events MAG, RES, Qualities, etc.
            CharacterObject.PropertyChanged += OnCharacterPropertyChanged;

            tabPowerUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            tabSkillUc.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;

            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            ContextMenuStrip[] lstCMSToTranslate = {
                cmsAdvancedLifestyle,
                cmsAdvancedProgram,
                cmsArmor,
                cmsArmorGear,
                cmsArmorLocation,
                cmsArmorMod,
                cmsBioware,
                cmsComplexForm,
                cmsComplexFormPlugin,
                cmsCritterPowers,
                cmsCustomLimitModifier,
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
                cmsLimitModifier,
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

        private void frmCreate_Load(object sender, EventArgs e)
        {
            Timekeeper.Finish("load_free");
            Timekeeper.Start("load_frm_create");

            if (!CharacterObject.IsCritter && (CharacterObject.BuildMethod == CharacterBuildMethod.Karma && CharacterObject.BuildKarma == 0) || (CharacterObject.BuildMethod == CharacterBuildMethod.Priority && CharacterObject.BuildKarma == 0))
            {
                _blnFreestyle = true;
                tssBPRemain.Visible = false;
                tssBPRemainLabel.Visible = false;
            }

            // Initialize elements if we're using Priority to build.
            if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority || CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                // Load the Priority information.
                if (string.IsNullOrEmpty(CharacterObject.GameplayOption))
                {
                    CharacterObject.GameplayOption = "Standard";
                }
                XmlNode objXmlGameplayOption = XmlManager.Load("gameplayoptions.xml").SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + CharacterObject.GameplayOption + "\"]");
                if (objXmlGameplayOption != null)
                {
                    string strKarma = objXmlGameplayOption["karma"]?.InnerText;
                    string strNuyen = objXmlGameplayOption["maxnuyen"]?.InnerText;
                    if (!CharacterObjectOptions.FreeContactsMultiplierEnabled)
                    {
                        string strContactMultiplier = objXmlGameplayOption["contactmultiplier"]?.InnerText;
                        CharacterObject.ContactMultiplier = Convert.ToInt32(strContactMultiplier);
                    }
                    else
                    {
                        CharacterObject.ContactMultiplier = CharacterObjectOptions.FreeContactsMultiplier;
                    }

                    CharacterObject.GameplayOptionQualityLimit = CharacterObject.MaxKarma = Convert.ToInt32(strKarma);
                    CharacterObject.MaxNuyen = Convert.ToInt32(strNuyen);
                }
            }
            Utils.DoDatabinding(lblNuyenTotal, "Text", CharacterObject, nameof(Character.DisplayTotalStartingNuyen));
            Utils.DoDatabinding(lblAttributesBase, "Visible", CharacterObject, nameof(Character.BuildMethodHasSkillPoints));

            Utils.DoDatabinding(txtCharacterName, "Text", CharacterObject, nameof(Character.Name));
            Utils.DoDatabinding(txtSex,           "Text", CharacterObject, nameof(Character.Sex));
            Utils.DoDatabinding(txtAge,           "Text", CharacterObject, nameof(Character.Age));
            Utils.DoDatabinding(txtEyes,          "Text", CharacterObject, nameof(Character.Eyes));
            Utils.DoDatabinding(txtHeight,        "Text", CharacterObject, nameof(Character.Height));
            Utils.DoDatabinding(txtWeight,        "Text", CharacterObject, nameof(Character.Weight));
            Utils.DoDatabinding(txtSkin,          "Text", CharacterObject, nameof(Character.Skin));
            Utils.DoDatabinding(txtHair,          "Text", CharacterObject, nameof(Character.Hair));
            Utils.DoDatabinding(txtDescription,   "Text", CharacterObject, nameof(Character.Description));
            Utils.DoDatabinding(txtBackground,    "Text", CharacterObject, nameof(Character.Background));
            Utils.DoDatabinding(txtConcept,       "Text", CharacterObject, nameof(Character.Concept));
            Utils.DoDatabinding(txtNotes,         "Text", CharacterObject, nameof(Character.Notes));
            Utils.DoDatabinding(txtAlias,         "Text", CharacterObject, nameof(Character.Alias));
            Utils.DoDatabinding(txtPlayerName,    "Text", CharacterObject, nameof(Character.PlayerName));

            tssBPLabel.Text = LanguageManager.GetString("Label_Karma", GlobalOptions.Language);
            tssBPRemainLabel.Text = LanguageManager.GetString("Label_KarmaRemaining", GlobalOptions.Language);
            tabBPSummary.Text = LanguageManager.GetString("Tab_BPSummary_Karma", GlobalOptions.Language);
            lblQualityBPLabel.Text = LanguageManager.GetString("Label_Karma", GlobalOptions.Language);

            Utils.DoDatabinding(lblSpirits,   "Visible", CharacterObject, nameof(Character.MagicianEnabled));
            Utils.DoDatabinding(cmdAddSpirit, "Visible", CharacterObject, nameof(Character.MagicianEnabled));
            Utils.DoDatabinding(panSpirits,   "Visible", CharacterObject, nameof(Character.MagicianEnabled));

            // Set the visibility of the Bioware Suites menu options.
            mnuSpecialAddBiowareSuite.Visible = CharacterObjectOptions.AllowBiowareSuites;
            mnuSpecialCreateBiowareSuite.Visible = CharacterObjectOptions.AllowBiowareSuites;

            Utils.DoDatabinding(chkInitiationGroup,     "Visible", CharacterObject, nameof(Character.MAGEnabled));
            Utils.DoDatabinding(chkInitiationGroup,     "Checked", CharacterObject, nameof(Character.GroupMember));
            Utils.DoDatabinding(chkInitiationSchooling, "Visible", CharacterObject, nameof(Character.MAGEnabled));

            // Remove the Improvements Tab.
            tabCharacterTabs.TabPages.Remove(tabImprovements);
            
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
            
            OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.Ambidextrous)));

            Utils.DoDatabinding(lblFoci, "Visible", CharacterObject, nameof(Character.MAGEnabled));
            Utils.DoDatabinding(treFoci, "Visible", CharacterObject, nameof(Character.MAGEnabled));
            Utils.DoDatabinding(cmdCreateStackedFocus, "Visible", CharacterObject, nameof(Character.MAGEnabled));
            Utils.DoDatabinding(cmdAddMetamagic, "Enabled", CharacterObject, nameof(Character.AddInitiationsAllowed));

            if (CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                cmdLifeModule.Visible = true;
                btnCreateBackstory.Visible = CharacterObjectOptions.AutomaticBackstory;
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
            CharacterObject.Spirits.CollectionChanged += SpiritCollectionChanged;
            CharacterObject.Armor.CollectionChanged += ArmorCollectionChanged;
            CharacterObject.ArmorLocations.CollectionChanged += ArmorLocationCollectionChanged;
            CharacterObject.Weapons.CollectionChanged += WeaponCollectionChanged;
            CharacterObject.WeaponLocations.CollectionChanged += WeaponLocationCollectionChanged;
            CharacterObject.Gear.CollectionChanged += GearCollectionChanged;
            CharacterObject.GearLocations.CollectionChanged += GearLocationCollectionChanged;
            CharacterObject.Cyberware.CollectionChanged += CyberwareCollectionChanged;
            CharacterObject.Vehicles.CollectionChanged += VehicleCollectionChanged;
            CharacterObject.VehicleLocations.CollectionChanged += VehicleLocationCollectionChanged;

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

            Utils.DoDatabinding(lblMysticAdeptAssignment,  "Visible", CharacterObject, nameof(Character.UseMysticAdeptPPs));
            Utils.DoDatabinding(nudMysticAdeptMAGMagician, "Visible", CharacterObject, nameof(Character.UseMysticAdeptPPs));
            Utils.DoDatabinding(nudMysticAdeptMAGMagician, "Maximum", CharacterObject.MAG, nameof(CharacterAttrib.TotalValue));
            Utils.DoDatabinding(nudMysticAdeptMAGMagician, "Value",   CharacterObject, nameof(Character.MysticAdeptPowerPoints));
            
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

            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(toolStrip, "toolStrip");

            tabSkillUc.RealLoad();
            tabPowerUc.RealLoad();

            // Run through all appropriate property changers
            foreach (PropertyInfo objProperty in CharacterObject.GetType().GetProperties())
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(objProperty.Name));

            Utils.DoDatabinding(nudNuyen,           "Value", CharacterObject, nameof(Character.NuyenBP));
            Utils.DoDatabinding(nudNuyen,           "Maximum", CharacterObject, nameof(Character.TotalNuyenMaximumBP));

            Utils.DoDatabinding(lblCMPhysical,      "Text", CharacterObject, nameof(Character.PhysicalCM));
            Utils.DoDatabinding(lblCMStun,          "Text", CharacterObject, nameof(Character.StunCM));

            Utils.DoDatabinding(lblPhysical,        "Text", CharacterObject, nameof(Character.LimitPhysical));
            Utils.DoDatabinding(lblPhysical,        "ToolTipText", CharacterObject, nameof(Character.LimitPhysicalToolTip));
            Utils.DoDatabinding(lblMental,          "Text", CharacterObject, nameof(Character.LimitMental));
            Utils.DoDatabinding(lblMental,          "ToolTipText", CharacterObject, nameof(Character.LimitMentalToolTip));
            Utils.DoDatabinding(lblSocial,          "Text", CharacterObject, nameof(Character.LimitSocial));
            Utils.DoDatabinding(lblSocial,          "ToolTipText", CharacterObject, nameof(Character.LimitSocialToolTip));
            Utils.DoDatabinding(lblAstral,          "Text", CharacterObject, nameof(Character.LimitAstral));
            Utils.DoDatabinding(lblAstral,          "ToolTipText", CharacterObject, nameof(Character.LimitAstralToolTip));

            Utils.DoDatabinding(lblESSMax,          "Text", CharacterObject, nameof(Character.DisplayEssence));
            Utils.DoDatabinding(lblCyberwareESS,    "Text", CharacterObject, nameof(Character.DisplayCyberwareEssence));
            Utils.DoDatabinding(lblBiowareESS,      "Text", CharacterObject, nameof(Character.DisplayBiowareEssence));
            Utils.DoDatabinding(lblEssenceHoleESS,  "Text", CharacterObject, nameof(Character.DisplayEssenceHole));

            Utils.DoDatabinding(chkPrototypeTranshuman,         "Visible", CharacterObject, nameof(Character.IsPrototypeTranshuman));
            Utils.DoDatabinding(lblPrototypeTranshumanESSLabel, "Visible", CharacterObject, nameof(Character.IsPrototypeTranshuman));
            Utils.DoDatabinding(lblPrototypeTranshumanESS,      "Visible", CharacterObject, nameof(Character.IsPrototypeTranshuman));
            Utils.DoDatabinding(lblPrototypeTranshumanESS,      "Text", CharacterObject, nameof(Character.DisplayPrototypeTranshumanEssenceUsed));

            Utils.DoDatabinding(lblAstralINI, "Visible", CharacterObject, nameof(Character.MAGEnabled));

            Utils.DoDatabinding(lblArmor, "Text", CharacterObject, nameof(Character.TotalArmorRating));
            Utils.DoDatabinding(lblArmor, "ToolTipText", CharacterObject, nameof(Character.TotalArmorRatingToolTip));

            Utils.DoDatabinding(lblSpellDefenceIndirectDodge,       "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIndirectDodge));
            Utils.DoDatabinding(lblSpellDefenceIndirectDodge,       "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIndirectDodgeToolTip));
            Utils.DoDatabinding(lblSpellDefenceIndirectSoak,        "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIndirectSoak));
            Utils.DoDatabinding(lblSpellDefenceIndirectSoak,        "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIndirectSoakToolTip));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakMana,      "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakMana));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakMana,      "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseDirectSoakManaToolTip));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakPhysical,  "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakPhysical));
            Utils.DoDatabinding(lblSpellDefenceDirectSoakPhysical,  "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseDirectSoakPhysicalToolTip));

            Utils.DoDatabinding(lblSpellDefenceDetection,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDetection));
            Utils.DoDatabinding(lblSpellDefenceDetection,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDetectionToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttBOD,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseBOD));
            Utils.DoDatabinding(lblSpellDefenceDecAttBOD,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseBODToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttAGI,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseAGI));
            Utils.DoDatabinding(lblSpellDefenceDecAttAGI,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseAGIToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttREA,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseREA));
            Utils.DoDatabinding(lblSpellDefenceDecAttREA,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseREAToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttSTR,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseSTR));
            Utils.DoDatabinding(lblSpellDefenceDecAttSTR,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseSTRToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttCHA,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseCHA));
            Utils.DoDatabinding(lblSpellDefenceDecAttCHA,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseCHAToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttINT,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseINT));
            Utils.DoDatabinding(lblSpellDefenceDecAttINT,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseINTToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttLOG,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseLOG));
            Utils.DoDatabinding(lblSpellDefenceDecAttLOG,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseLOGToolTip));
            Utils.DoDatabinding(lblSpellDefenceDecAttWIL,           "Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseWIL));
            Utils.DoDatabinding(lblSpellDefenceDecAttWIL,           "ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseWILToolTip));

            Utils.DoDatabinding(lblSpellDefenceIllusionMana,        "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIllusionMana));
            Utils.DoDatabinding(lblSpellDefenceIllusionMana,        "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIllusionManaToolTip));
            Utils.DoDatabinding(lblSpellDefenceIllusionPhysical,    "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIllusionPhysical));
            Utils.DoDatabinding(lblSpellDefenceIllusionPhysical,    "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIllusionPhysicalToolTip));
            Utils.DoDatabinding(lblSpellDefenceManipMental,         "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseManipulationMental));
            Utils.DoDatabinding(lblSpellDefenceManipMental,         "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseManipulationMentalToolTip));
            Utils.DoDatabinding(lblSpellDefenceManipPhysical,       "Text",         CharacterObject, nameof(Character.DisplaySpellDefenseManipulationPhysical));
            Utils.DoDatabinding(lblSpellDefenceManipPhysical,       "ToolTipText",  CharacterObject, nameof(Character.SpellDefenseManipulationPhysicalToolTip));
            Utils.DoDatabinding(nudCounterspellingDice,             "Value",        CharacterObject, nameof(Character.CurrentCounterspellingDice));

            Utils.DoDatabinding(lblMovement,    "Text", CharacterObject, nameof(Character.DisplayMovement));
            Utils.DoDatabinding(lblSwim,        "Text", CharacterObject, nameof(Character.DisplaySwim));
            Utils.DoDatabinding(lblFly,         "Text", CharacterObject, nameof(Character.DisplayFly));

            Utils.DoDatabinding(lblRemainingNuyen,          "Text", CharacterObject, nameof(Character.DisplayNuyen));

            Utils.DoDatabinding(lblStreetCredTotal,         "Text", CharacterObject, nameof(Character.TotalStreetCred));
            Utils.DoDatabinding(lblStreetCredTotal,         "ToolTipText", CharacterObject, nameof(Character.StreetCredTooltip));
            Utils.DoDatabinding(lblNotorietyTotal,          "Text", CharacterObject, nameof(Character.TotalNotoriety));
            Utils.DoDatabinding(lblNotorietyTotal,          "ToolTipText", CharacterObject, nameof(Character.NotorietyTooltip));
            Utils.DoDatabinding(lblPublicAwareTotal,        "Text", CharacterObject, nameof(Character.TotalPublicAwareness));
            Utils.DoDatabinding(lblPublicAwareTotal,        "ToolTipText", CharacterObject, nameof(Character.PublicAwarenessTooltip));

            Utils.DoDatabinding(lblMentorSpiritLabel,       "Visible", CharacterObject, nameof(Character.HasMentorSpirit));
            Utils.DoDatabinding(lblMentorSpirit,            "Visible", CharacterObject, nameof(Character.HasMentorSpirit));
            Utils.DoDatabinding(lblMentorSpirit,            "Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayName));
            Utils.DoDatabinding(lblMentorSpiritInformation, "Visible", CharacterObject, nameof(Character.HasMentorSpirit));
            Utils.DoDatabinding(lblMentorSpiritInformation, "Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayInformation));

            Utils.DoDatabinding(lblComposure,       "ToolTipText", CharacterObject, nameof(Character.ComposureToolTip));
            Utils.DoDatabinding(lblComposure,       "Text", CharacterObject, nameof(Character.Composure));
            Utils.DoDatabinding(lblJudgeIntentions, "ToolTipText", CharacterObject, nameof(Character.JudgeIntentionsToolTip));
            Utils.DoDatabinding(lblJudgeIntentions, "Text", CharacterObject, nameof(Character.JudgeIntentions));
            Utils.DoDatabinding(lblLiftCarry,       "ToolTipText", CharacterObject, nameof(Character.LiftAndCarryToolTip));
            Utils.DoDatabinding(lblLiftCarry,       "Text", CharacterObject, nameof(Character.LiftAndCarry));
            Utils.DoDatabinding(lblMemory,          "ToolTipText", CharacterObject, nameof(Character.MemoryToolTip));
            Utils.DoDatabinding(lblMemory,          "Text", CharacterObject, nameof(Character.Memory));

            Utils.DoDatabinding(cmdAddCyberware, "Enabled", CharacterObject, nameof(Character.AddCyberwareEnabled));
            Utils.DoDatabinding(cmdAddBioware, "Enabled", CharacterObject, nameof(Character.AddBiowareEnabled));

            RefreshAttributes(pnlAttributes);

            PrimaryAttributes.CollectionChanged += AttributeCollectionChanged;
            SpecialAttributes.CollectionChanged += AttributeCollectionChanged;

            IsCharacterUpdateRequested = true;
            // Directly calling here so that we can properly unset the dirty flag after the update
            UpdateCharacterInfo();

            // Now we can start checking for character updates
            Application.Idle += UpdateCharacterInfo;
            Application.Idle += LiveUpdateFromCharacterFile;

            // Clear the Dirty flag which gets set when creating a new Character.
            IsDirty = false;
            RefreshPasteStatus(sender, e);
            frmCreate_Resize(sender, e);
            picMugshot_SizeChanged(sender, e);

            // Stupid hack to get the MDI icon to show up properly.
            Icon = Icon.Clone() as Icon;

            Timekeeper.Finish("load_frm_create");
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

        private void frmCreate_FormClosing(object sender, FormClosingEventArgs e)
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
                GlobalOptions.ClipboardChanged -= RefreshPasteStatus;
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
                CharacterObject.Spirits.CollectionChanged -= SpiritCollectionChanged;
                CharacterObject.Armor.CollectionChanged -= ArmorCollectionChanged;
                CharacterObject.ArmorLocations.CollectionChanged -= ArmorLocationCollectionChanged;
                CharacterObject.Weapons.CollectionChanged -= WeaponCollectionChanged;
                CharacterObject.WeaponLocations.CollectionChanged -= WeaponLocationCollectionChanged;
                CharacterObject.Gear.CollectionChanged -= GearCollectionChanged;
                CharacterObject.GearLocations.CollectionChanged -= GearLocationCollectionChanged;
                CharacterObject.Cyberware.CollectionChanged -= CyberwareCollectionChanged;
                CharacterObject.Vehicles.CollectionChanged -= VehicleCollectionChanged;
                CharacterObject.VehicleLocations.CollectionChanged -= VehicleLocationCollectionChanged;
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

                foreach (ContactControl objContactControl in panContacts.Controls.OfType<ContactControl>())
                {
                    objContactControl.ContactDetailChanged -= MakeDirtyWithCharacterUpdate;
                    objContactControl.DeleteContact -= DeleteContact;
                    objContactControl.MouseDown -= DragContactControl;
                }

                foreach (ContactControl objContactControl in panEnemies.Controls.OfType<ContactControl>())
                {
                    objContactControl.ContactDetailChanged -= EnemyChanged;
                    objContactControl.DeleteContact -= DeleteEnemy;
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
                if (!Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(CharacterObject) && x != CharacterObject))
                {
                    Program.MainForm.OpenCharacters.Remove(CharacterObject);
                    CharacterObject.DeleteCharacter();
                }
                Dispose(true);
            }
        }

        private void frmCreate_Activated(object sender, EventArgs e)
        {
            // Merge the ToolStrips.
            ToolStripManager.RevertMerge("toolStrip");
            ToolStripManager.Merge(toolStrip, "toolStrip");
        }

        private void frmCreate_Shown(object sender, EventArgs e)
        {
            frmCreate_Resize(sender, e);
        }

        private void frmCreate_Resize(object sender, EventArgs e)
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

            cmdDeleteLimitModifier.Left = cmdAddLimitModifier.Left + cmdAddLimitModifier.Width + 15;
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
                    tssNuyenRemaining.Text = CharacterObject.DisplayNuyen;
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
                            if (!tabCharacterTabs.TabPages.Contains(tabMagician))
                                tabCharacterTabs.TabPages.Insert(3, tabMagician);

                            /*
                            int intEssenceLoss = 0;
                            if (!CharacterObjectOptions.ESSLossReducesMaximumOnly)
                                intEssenceLoss = _objCharacter.EssencePenalty;
                            */
                            // If the character options permit initiation in create mode, show the Initiation page.
                            UpdateInitiationCost();

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
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_JoinedGroup", GlobalOptions.Language);

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
                            tabCharacterTabs.TabPages.Remove(tabMagician);

                            if (SpecialAttributes.Contains(CharacterObject.MAG))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAG);
                            }
                            if (SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
                            }

                            IsCharacterUpdateRequested = true;
                        }
                    }
                    break;
                case nameof(Character.RESEnabled):
                    {
                        // Change to the status of RES being enabled.
                        if (CharacterObject.RESEnabled)
                        {
                            /*
                            int intEssenceLoss = 0;
                            if (!CharacterObjectOptions.ESSLossReducesMaximumOnly)
                                intEssenceLoss = _objCharacter.EssencePenalty;
                            // If the character options permit submersion in create mode, show the Initiation page.
                            */
                            UpdateInitiationCost();

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
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_JoinedNetwork", GlobalOptions.Language);

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

                            IsCharacterUpdateRequested = true;
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
                            cmdAddSpell.Enabled = true;
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && !SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Add(CharacterObject.MAGAdept);
                            }
                        }
                        else
                        {
                            cmdAddSpell.Enabled = false;
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
                            }
                        }
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

                            if (CharacterObject.Options.MysAdeptSecondMAGAttribute && SpecialAttributes.Contains(CharacterObject.MAGAdept))
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
                                Cyberware objParent = x;
                                bool blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                                while (objParent.Parent != null && blnNoParentIsModular)
                                {
                                    objParent = objParent.Parent;
                                    blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                                }

                                return blnNoParentIsModular;
                            }
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children, funcExConIneligibleWare))
                            {
                                char chrAvail = objCyberware.TotalAvailTuple(false).Suffix;
                                if (chrAvail == 'R' || chrAvail == 'F')
                                {
                                    objCyberware.DeleteCyberware();
                                    Cyberware objParent = objCyberware.Parent;
                                    if (objParent != null)
                                        objParent.Children.Remove(objCyberware);
                                    else
                                        CharacterObject.Cyberware.Remove(objCyberware);
                                    blnDoRefresh = true;
                                }
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
                    }
                    break;
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

        private void mnuFilePrint_Click(object sender, EventArgs e)
        {
            DoPrint();
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuSpecialAddPACKSKit_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddPACKSKit();
            }
            while (blnAddAgain);
        }

        private void mnuSpecialCreatePACKSKit_Click(object sender, EventArgs e)
        {
            CreatePACKSKit();
        }

        private void mnuSpecialChangeMetatype_Click(object sender, EventArgs e)
        {
            ChangeMetatype();
        }

        private void mnuSpecialChangeOptions_Click(object sender, EventArgs e)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "settings", "default.xml");
            if (!File.Exists(strFilePath))
            {
                if (MessageBox.Show(LanguageManager.GetString("Message_CharacterOptions_OpenOptions", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CharacterOptions_OpenOptions", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    frmOptions frmOptions = new frmOptions();
                    frmOptions.ShowDialog();
                    Cursor = Cursors.Default;
                }
            }
            Cursor = Cursors.WaitCursor;
            string settingsPath = Path.Combine(Application.StartupPath, "settings");
            string[] settingsFiles = Directory.GetFiles(settingsPath, "*.xml");

            if (settingsFiles.Length > 1)
            {
                frmSelectSetting frmPickSetting = new frmSelectSetting();
                frmPickSetting.ShowDialog(this);

                if (frmPickSetting.DialogResult == DialogResult.Cancel)
                    return;

                CharacterObject.SettingsFile = frmPickSetting.SettingsFile;
            }
            else
            {
                string strSettingsFile = settingsFiles[0];
                CharacterObject.SettingsFile = Path.GetFileName(strSettingsFile);
            }

            IsCharacterUpdateRequested = true;
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

            // Get the player to roll Dice to make a WIL Test and record the result.
            frmDiceHits frmWILHits = new frmDiceHits
            {
                Text = LanguageManager.GetString("String_CyberzombieWILText", GlobalOptions.Language),
                Description = LanguageManager.GetString("String_CyberzombieWILDescription", GlobalOptions.Language)
            };
            int intDice = CharacterObject.WIL.TotalValue;
            int intThreshold = 3 + decimal.ToInt32(decimal.Floor(CharacterObject.Essence() - CharacterObject.ESS.MetatypeMaximum));
            frmWILHits.Dice = intDice;
            frmWILHits.ShowDialog(this);

            if (frmWILHits.DialogResult != DialogResult.OK)
                return;

            int intWILResult = frmWILHits.Result;

            // The character gains 10 + ((Threshold - Hits) * 10)BP worth of Negative Qualities.
            int intResult = 10;
            if (intWILResult < intThreshold)
            {
                intResult = (intThreshold - intWILResult) * 10;
            }
            ImprovementManager.CreateImprovement(CharacterObject, string.Empty, Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.FreeNegativeQualities, string.Empty, intResult * -1);
            ImprovementManager.Commit(CharacterObject);

            // Convert the character.
            // Characters lose access to Resonance.
            CharacterObject.RESEnabled = false;

            // Gain MAG that is permanently set to 1.
            CharacterObject.MAGEnabled = true;
            CharacterObject.MAG.MetatypeMinimum = 1;
            CharacterObject.MAG.MetatypeMaximum = 1;
            CharacterObject.MAG.Base = 1;

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

        private void mnuSpecialAddCyberwareSuite_Click(object sender, EventArgs e)
        {
            AddCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private void mnuSpecialAddBiowareSuite_Click(object sender, EventArgs e)
        {
            AddCyberwareSuite(Improvement.ImprovementSource.Bioware);
        }

        private void mnuSpecialCreateCyberwareSuite_Click(object sender, EventArgs e)
        {
            CreateCyberwareSuite(Improvement.ImprovementSource.Cyberware);
        }

        private void mnuSpecialCreateBiowareSuite_Click(object sender, EventArgs e)
        {
            CreateCyberwareSuite(Improvement.ImprovementSource.Bioware);
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
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.MAGEnabled)));
            if (blnRESEnabled != CharacterObject.RESEnabled)
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.RESEnabled)));
            if (blnDEPEnabled != CharacterObject.DEPEnabled)
                OnCharacterPropertyChanged(CharacterObject, new PropertyChangedEventArgs(nameof(Character.DEPEnabled)));

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

        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            if (tabCharacterTabs.SelectedTab == tabStreetGear)
            {
                // Lifestyle Tab.
                if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                {
                    // Copy the selected Lifestyle.
                    Lifestyle objCopyLifestyle = CharacterObject.Lifestyles.FindById(treLifestyles.SelectedNode?.Tag.ToString());

                    if (objCopyLifestyle != null)
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyLifestyle.Save(objWriter);

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Lifestyle;
                        //Clipboard.SetText(objCharacterXML.OuterXml);
                    }
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    // Copy the selected Armor.
                    Armor objCopyArmor = CharacterObject.Armor.FindById(treArmor.SelectedNode?.Tag.ToString());

                    if (objCopyArmor != null)
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyArmor.Save(objWriter);
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Armor;

                        if (!objCopyArmor.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyArmor.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                    }
                    else
                    {
                        // Attempt to copy Gear.
                        Gear objCopyGear = CharacterObject.Armor.FindArmorGear(treArmor.SelectedNode?.Tag.ToString());

                        if (objCopyGear != null)
                        {
                            MemoryStream objStream = new MemoryStream();
                            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 1,
                                IndentChar = '\t'
                            };

                            objWriter.WriteStartDocument();

                            // </characters>
                            objWriter.WriteStartElement("character");

                            objCopyGear.Save(objWriter);
                            GlobalOptions.ClipboardContentType = ClipboardContentType.Gear;

                            if (!objCopyGear.WeaponID.IsEmptyGuid())
                            {
                                // <weapons>
                                objWriter.WriteStartElement("weapons");
                                // Copy any Weapon that comes with the Gear.
                                foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                                {
                                    objCopyWeapon.Save(objWriter);
                                }

                                objWriter.WriteEndElement();
                            }

                            // </characters>
                            objWriter.WriteEndElement();

                            // Finish the document and flush the Writer and Stream.
                            objWriter.WriteEndDocument();
                            objWriter.Flush();

                            // Read the stream.
                            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                            objStream.Position = 0;
                            XmlDocument objCharacterXML = new XmlDocument();

                            // Put the stream into an XmlDocument.
                            string strXML = objReader.ReadToEnd();
                            objCharacterXML.LoadXml(strXML);

                            objWriter.Close();

                            GlobalOptions.Clipboard = objCharacterXML;
                        }
                    }
                }
                // Weapons Tab.
                else if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    // Copy the selected Weapon.
                    Weapon objCopyWeapon = CharacterObject.Weapons.DeepFindById(treWeapons.SelectedNode?.Tag.ToString());

                    if (objCopyWeapon != null)
                    {
                        // Do not let the user copy Gear or Cyberware Weapons.
                        if (objCopyWeapon.Category == "Gear" || objCopyWeapon.Cyberware)
                            return;

                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyWeapon.Save(objWriter);

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Weapon;
                    }
                    else
                    {
                        Gear objCopyGear = CharacterObject.Weapons.FindWeaponGear(treWeapons.SelectedNode?.Tag.ToString());

                        if (objCopyGear != null)
                        {
                            MemoryStream objStream = new MemoryStream();
                            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 1,
                                IndentChar = '\t'
                            };

                            objWriter.WriteStartDocument();

                            // </characters>
                            objWriter.WriteStartElement("character");

                            objCopyGear.Save(objWriter);
                            GlobalOptions.ClipboardContentType = ClipboardContentType.Gear;

                            if (!objCopyGear.WeaponID.IsEmptyGuid())
                            {
                                // <weapons>
                                objWriter.WriteStartElement("weapons");
                                // Copy any Weapon that comes with the Gear.
                                foreach (Weapon objCopyGearWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                                {
                                    objCopyGearWeapon.Save(objWriter);
                                }

                                objWriter.WriteEndElement();
                            }

                            // </characters>
                            objWriter.WriteEndElement();

                            // Finish the document and flush the Writer and Stream.
                            objWriter.WriteEndDocument();
                            objWriter.Flush();

                            // Read the stream.
                            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                            objStream.Position = 0;
                            XmlDocument objCharacterXML = new XmlDocument();

                            // Put the stream into an XmlDocument.
                            string strXML = objReader.ReadToEnd();
                            objCharacterXML.LoadXml(strXML);

                            objWriter.Close();

                            GlobalOptions.Clipboard = objCharacterXML;
                        }
                    }
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    // Copy the selected Gear.
                    Gear objCopyGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode?.Tag.ToString());

                    if (objCopyGear != null)
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyGear.Save(objWriter);
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Gear;

                        if (!objCopyGear.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                        //Clipboard.SetText(objCharacterXML.OuterXml);
                    }
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                string strSelectedId = treGear.SelectedNode?.Tag.ToString();
                if (string.IsNullOrEmpty(strSelectedId))
                    return;
                // Copy the selected 'ware
                Cyberware objCopyCyberware = CharacterObject.Cyberware.DeepFindById(strSelectedId);

                if (objCopyCyberware != null)
                {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };

                    objWriter.WriteStartDocument();

                    // </characters>
                    objWriter.WriteStartElement("character");

                    objCopyCyberware.Save(objWriter);
                    GlobalOptions.ClipboardContentType = ClipboardContentType.Cyberware;

                    if (!objCopyCyberware.WeaponID.IsEmptyGuid())
                    {
                        // <weapons>
                        objWriter.WriteStartElement("weapons");
                        // Copy any Weapon that comes with the Gear.
                        foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyCyberware.InternalId))
                        {
                            objCopyWeapon.Save(objWriter);
                        }

                        objWriter.WriteEndElement();
                    }
                    if (!objCopyCyberware.VehicleID.IsEmptyGuid())
                    {
                        // <weapons>
                        objWriter.WriteStartElement("vehicles");
                        // Copy any Weapon that comes with the Gear.
                        foreach (Vehicle objCopyVehicle in CharacterObject.Vehicles.Where(x => x.ParentID == objCopyCyberware.InternalId))
                        {
                            objCopyVehicle.Save(objWriter);
                        }

                        objWriter.WriteEndElement();
                    }

                    // </characters>
                    objWriter.WriteEndElement();

                    // Finish the document and flush the Writer and Stream.
                    objWriter.WriteEndDocument();
                    objWriter.Flush();

                    // Read the stream.
                    StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                    objStream.Position = 0;
                    XmlDocument objCharacterXML = new XmlDocument();

                    // Put the stream into an XmlDocument.
                    string strXML = objReader.ReadToEnd();
                    objCharacterXML.LoadXml(strXML);

                    objWriter.Close();

                    GlobalOptions.Clipboard = objCharacterXML;
                    //Clipboard.SetText(objCharacterXML.OuterXml);
                }
                else
                {
                    // Copy the selected Gear.
                    Gear objCopyGear = CharacterObject.Cyberware.FindCyberwareGear(strSelectedId);

                    if (objCopyGear != null)
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyGear.Save(objWriter);
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Gear;

                        if (!objCopyGear.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }

                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                        //Clipboard.SetText(objCharacterXML.OuterXml);
                    }
                }
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
                if (!string.IsNullOrEmpty(strSelectedId))
                    return;
                // Copy the selected Vehicle.
                Vehicle objCopyVehicle = CharacterObject.Vehicles.FindById(strSelectedId);

                if (objCopyVehicle != null)
                {
                    MemoryStream objStream = new MemoryStream();
                    XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 1,
                        IndentChar = '\t'
                    };

                    objWriter.WriteStartDocument();

                    // </characters>
                    objWriter.WriteStartElement("character");

                    objCopyVehicle.Save(objWriter);

                    // </characters>
                    objWriter.WriteEndElement();

                    // Finish the document and flush the Writer and Stream.
                    objWriter.WriteEndDocument();
                    objWriter.Flush();

                    // Read the stream.
                    StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                    objStream.Position = 0;
                    XmlDocument objCharacterXML = new XmlDocument();

                    // Put the stream into an XmlDocument.
                    string strXML = objReader.ReadToEnd();
                    objCharacterXML.LoadXml(strXML);

                    objWriter.Close();

                    GlobalOptions.Clipboard = objCharacterXML;
                    GlobalOptions.ClipboardContentType = ClipboardContentType.Vehicle;
                    //Clipboard.SetText(objCharacterXML.OuterXml);
                }
                else
                {
                    Gear objCopyGear = CharacterObject.Vehicles.FindVehicleGear(strSelectedId);

                    if (objCopyGear != null)
                    {
                        MemoryStream objStream = new MemoryStream();
                        XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 1,
                            IndentChar = '\t'
                        };

                        objWriter.WriteStartDocument();

                        // </characters>
                        objWriter.WriteStartElement("character");

                        objCopyGear.Save(objWriter);
                        GlobalOptions.ClipboardContentType = ClipboardContentType.Gear;

                        if (!objCopyGear.WeaponID.IsEmptyGuid())
                        {
                            // <weapons>
                            objWriter.WriteStartElement("weapons");
                            // Copy any Weapon that comes with the Gear.
                            foreach (Weapon objCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyGear.InternalId))
                            {
                                objCopyWeapon.Save(objWriter);
                            }
                            objWriter.WriteEndElement();
                        }

                        // </characters>
                        objWriter.WriteEndElement();

                        // Finish the document and flush the Writer and Stream.
                        objWriter.WriteEndDocument();
                        objWriter.Flush();

                        // Read the stream.
                        StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                        objStream.Position = 0;
                        XmlDocument objCharacterXML = new XmlDocument();

                        // Put the stream into an XmlDocument.
                        string strXML = objReader.ReadToEnd();
                        objCharacterXML.LoadXml(strXML);

                        objWriter.Close();

                        GlobalOptions.Clipboard = objCharacterXML;
                    }
                    else
                    {
                        Weapon objCopyWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strSelectedId);
                        if (objCopyWeapon != null)
                        {
                            // Do not let the user copy Gear or Cyberware Weapons.
                            if (objCopyWeapon.Category == "Gear" || objCopyWeapon.Cyberware)
                                return;

                            MemoryStream objStream = new MemoryStream();
                            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                            {
                                Formatting = Formatting.Indented,
                                Indentation = 1,
                                IndentChar = '\t'
                            };

                            objWriter.WriteStartDocument();

                            // </characters>
                            objWriter.WriteStartElement("character");

                            objCopyWeapon.Save(objWriter);

                            // </characters>
                            objWriter.WriteEndElement();

                            // Finish the document and flush the Writer and Stream.
                            objWriter.WriteEndDocument();
                            objWriter.Flush();

                            // Read the stream.
                            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                            objStream.Position = 0;
                            XmlDocument objCharacterXML = new XmlDocument();

                            // Put the stream into an XmlDocument.
                            string strXML = objReader.ReadToEnd();
                            objCharacterXML.LoadXml(strXML);

                            objWriter.Close();

                            GlobalOptions.Clipboard = objCharacterXML;
                            GlobalOptions.ClipboardContentType = ClipboardContentType.Weapon;
                        }
                        else
                        {
                            Cyberware objCopyCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);

                            if (objCopyCyberware != null)
                            {
                                MemoryStream objStream = new MemoryStream();
                                XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                                {
                                    Formatting = Formatting.Indented,
                                    Indentation = 1,
                                    IndentChar = '\t'
                                };

                                objWriter.WriteStartDocument();

                                // </characters>
                                objWriter.WriteStartElement("character");

                                objCopyCyberware.Save(objWriter);
                                GlobalOptions.ClipboardContentType = ClipboardContentType.Cyberware;

                                if (!objCopyCyberware.WeaponID.IsEmptyGuid())
                                {
                                    // <weapons>
                                    objWriter.WriteStartElement("weapons");
                                    // Copy any Weapon that comes with the Gear.
                                    foreach (Weapon objLoopCopyWeapon in CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objCopyCyberware.InternalId))
                                    {
                                        objLoopCopyWeapon.Save(objWriter);
                                    }

                                    objWriter.WriteEndElement();
                                }
                                if (!objCopyCyberware.VehicleID.IsEmptyGuid())
                                {
                                    // <weapons>
                                    objWriter.WriteStartElement("vehicles");
                                    // Copy any Weapon that comes with the Gear.
                                    foreach (Vehicle objLoopCopyVehicle in CharacterObject.Vehicles.Where(x => x.ParentID == objCopyCyberware.InternalId))
                                    {
                                        objLoopCopyVehicle.Save(objWriter);
                                    }

                                    objWriter.WriteEndElement();
                                }

                                // </characters>
                                objWriter.WriteEndElement();

                                // Finish the document and flush the Writer and Stream.
                                objWriter.WriteEndDocument();
                                objWriter.Flush();

                                // Read the stream.
                                StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);
                                objStream.Position = 0;
                                XmlDocument objCharacterXML = new XmlDocument();

                                // Put the stream into an XmlDocument.
                                string strXML = objReader.ReadToEnd();
                                objCharacterXML.LoadXml(strXML);

                                objWriter.Close();

                                GlobalOptions.Clipboard = objCharacterXML;
                                //Clipboard.SetText(objCharacterXML.OuterXml);
                            }
                        }
                    }
                }
            }
        }

        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            if (tabCharacterTabs.SelectedTab == tabStreetGear)
            {
                // Lifestyle Tab.
                if (tabStreetGearTabs.SelectedTab == tabLifestyle)
                {
                    // Paste Lifestyle.
                    XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/lifestyle");
                    if (objXmlNode != null)
                    {
                        Lifestyle objLifestyle = new Lifestyle(CharacterObject);
                        objLifestyle.Load(objXmlNode, true);
                        // Reset the number of months back to 1 since 0 isn't valid in Create Mode.
                        objLifestyle.Increments = 1;

                        CharacterObject.Lifestyles.Add(objLifestyle);

                        IsCharacterUpdateRequested = true;
                        IsDirty = true;
                    }
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    // Paste Armor.
                    XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/armor");
                    if (objXmlNode != null)
                    {
                        Armor objArmor = new Armor(CharacterObject);
                        objArmor.Load(objXmlNode, true);

                        CharacterObject.Armor.Add(objArmor);

                        // Add any Weapons that come with the Armor.
                        XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                        if (objXmlNodeList != null)
                        {
                            foreach (XmlNode objLoopNode in objXmlNodeList)
                            {
                                Weapon objWeapon = new Weapon(CharacterObject);
                                objWeapon.Load(objLoopNode, true);
                                CharacterObject.Weapons.Add(objWeapon);
                                objWeapon.ParentID = objArmor.InternalId;
                                objArmor.WeaponID = objWeapon.InternalId;
                            }
                        }

                        IsCharacterUpdateRequested = true;
                        IsDirty = true;
                    }
                    else
                    {
                        // Paste Gear.
                        objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");

                        if (objXmlNode != null)
                        {
                            string strSelectedId = treArmor.SelectedNode?.Tag.ToString();
                            Gear objGear = new Gear(CharacterObject);
                            objGear.Load(objXmlNode, true);

                            Armor objSelectedArmor = CharacterObject.Armor.FindById(strSelectedId);
                            if (objSelectedArmor != null)
                            {
                                objSelectedArmor.Gear.Add(objGear);
                                if (!objSelectedArmor.Equipped)
                                    objGear.ChangeEquippedStatus(false);
                            }
                            else
                            {
                                ArmorMod objSelectedArmorMod = CharacterObject.Armor.FindArmorMod(strSelectedId);
                                if (objSelectedArmorMod != null)
                                {
                                    objSelectedArmorMod.Gear.Add(objGear);
                                    if (!objSelectedArmorMod.Equipped || objSelectedArmorMod.Parent?.Equipped != true)
                                        objGear.ChangeEquippedStatus(false);
                                }
                                else
                                {
                                    Gear objNewParent = CharacterObject.Armor.FindArmorGear(strSelectedId, out objSelectedArmor, out objSelectedArmorMod);
                                    if (objNewParent != null)
                                    {
                                        XmlNodeList xmlAddonCategoryList = objNewParent.GetNode()?.SelectNodes("addoncategory");
                                        if (xmlAddonCategoryList?.Count > 0)
                                        {
                                            bool blnDoAdd = false;
                                            foreach (XmlNode xmlCategory in xmlAddonCategoryList)
                                            {
                                                if (xmlCategory.InnerText == objGear.Category)
                                                {
                                                    blnDoAdd = true;
                                                    break;
                                                }
                                            }

                                            if (!blnDoAdd)
                                            {
                                                objGear.DeleteGear();
                                                return;
                                            }
                                        }

                                        objNewParent.Children.Add(objGear);
                                        if (!objNewParent.Equipped || objSelectedArmorMod?.Equipped == false || !objSelectedArmor.Equipped)
                                            objGear.ChangeEquippedStatus(false);
                                    }
                                    else
                                        return;
                                }
                            }

                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objWeapon);
                                    objWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                    }
                }
                // Weapons Tab.
                else if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    // Paste Gear into a Weapon Accessory.
                    XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                    if (objXmlNode != null)
                    {
                        string strSelectedId = treWeapons.SelectedNode?.Tag.ToString();
                        Gear objGear = new Gear(CharacterObject);
                        objGear.Load(objXmlNode, true);

                        // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
                        WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(strSelectedId);
                        if (objAccessory != null)
                        {
                            bool blnDoAdd = false;
                            XmlNodeList xmlGearCategoryList = objAccessory.AllowGear?.SelectNodes("gearcategory");
                            if (xmlGearCategoryList?.Count > 0)
                            {
                                foreach (XmlNode objAllowed in xmlGearCategoryList)
                                {
                                    if (objAllowed.InnerText == objGear.Category)
                                    {
                                        blnDoAdd = true;
                                        break;
                                    }
                                }
                            }

                            if (!blnDoAdd)
                            {
                                objGear.DeleteGear();
                                return;
                            }

                            objAccessory.Gear.Add(objGear);

                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objGearWeapon = new Weapon(CharacterObject);
                                    objGearWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objGearWeapon);
                                    objGearWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objGearWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                        else
                        {
                            Gear objNewParent = CharacterObject.Weapons.FindWeaponGear(strSelectedId);
                            if (objNewParent != null)
                            {
                                XmlNodeList xmlAddonCategoryList = objNewParent.GetNode()?.SelectNodes("addoncategory");
                                if (xmlAddonCategoryList?.Count > 0)
                                {
                                    bool blnDoAdd = false;
                                    foreach (XmlNode xmlCategory in xmlAddonCategoryList)
                                    {
                                        if (xmlCategory.InnerText == objGear.Category)
                                        {
                                            blnDoAdd = true;
                                            break;
                                        }
                                    }

                                    if (!blnDoAdd)
                                    {
                                        objGear.DeleteGear();
                                        return;
                                    }
                                }

                                objNewParent.Children.Add(objGear);

                                IsCharacterUpdateRequested = true;
                                IsDirty = true;
                            }
                        }
                    }
                    else
                    {
                        // Paste Weapon.
                        objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/weapon");
                        if (objXmlNode != null)
                        {
                            Weapon objWeapon = new Weapon(CharacterObject) {ParentVehicle = null};
                            objWeapon.Load(objXmlNode, true);

                            CharacterObject.Weapons.Add(objWeapon);

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                    }
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    // Paste Gear.
                    XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                    if (objXmlNode != null)
                    {
                        Gear objGear = new Gear(CharacterObject);
                        objGear.Load(objXmlNode, true);

                        Gear objNewParent = CharacterObject.Gear.DeepFindById(treGear.SelectedNode?.Tag.ToString());
                        if (objNewParent != null)
                        {
                            XmlNodeList xmlAddonCategoryList = objNewParent.GetNode()?.SelectNodes("addoncategory");
                            if (xmlAddonCategoryList?.Count > 0)
                            {
                                bool blnDoAdd = false;
                                foreach (XmlNode xmlCategory in xmlAddonCategoryList)
                                {
                                    if (xmlCategory.InnerText == objGear.Category)
                                    {
                                        blnDoAdd = true;
                                        break;
                                    }
                                }

                                if (!blnDoAdd)
                                {
                                    objGear.DeleteGear();
                                    return;
                                }
                            }

                            objNewParent.Children.Add(objGear);
                            if (!objNewParent.Equipped)
                                objGear.ChangeEquippedStatus(false);

                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objWeapon);
                                    objWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                        else
                        {
                            CharacterObject.Gear.Add(objGear);

                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objWeapon);
                                    objWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                    }
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                string strSelectedId = treCyberware.SelectedNode?.Tag.ToString();
                if (string.IsNullOrEmpty(strSelectedId))
                    return;

                // Paste Cyberware.
                XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                if (objXmlNode != null)
                {
                    Cyberware objCyberware = new Cyberware(CharacterObject);
                    objCyberware.Load(objXmlNode, true);

                    // Paste Cyberware into a Cyberware.
                    Cyberware objCyberwareParent = CharacterObject.Cyberware.DeepFindById(strSelectedId);
                    if (objCyberwareParent != null && !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                    {
                        if (objCyberware.PlugsIntoModularMount != objCyberwareParent.HasModularMount || objCyberwareParent.Children.Any(x => x.PlugsIntoModularMount == objCyberware.HasModularMount))
                        {
                            objCyberwareParent = null;
                        }
                        else
                        {
                            objCyberware.Location = objCyberwareParent.Location;
                        }
                    }
                    if (objCyberwareParent != null && objCyberware.SourceType == objCyberwareParent.SourceType)
                    {
                        string strAllowedSubsystems = objCyberwareParent.AllowedSubsystems;
                        if (!string.IsNullOrEmpty(strAllowedSubsystems))
                        {
                            bool blnDoAdd = false;
                            foreach (string strSubsystem in strAllowedSubsystems.Split(','))
                            {
                                if (objCyberware.Category == strSubsystem)
                                {
                                    blnDoAdd = true;
                                    break;
                                }
                            }

                            if (!blnDoAdd)
                            {
                                objCyberware.DeleteCyberware();
                                return;
                            }
                        }

                        if (!string.IsNullOrEmpty(objCyberware.HasModularMount) || !string.IsNullOrEmpty(objCyberware.BlocksMounts))
                        {
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

                            if (!string.IsNullOrEmpty(objCyberware.HasModularMount) && setDisallowedMounts.Count > 0)
                            {
                                foreach (string strLoop in setDisallowedMounts)
                                {
                                    if (!strLoop.EndsWith("Right"))
                                    {
                                        string strCheck = strLoop;
                                        if (strCheck.EndsWith("Left"))
                                        {
                                            strCheck = strCheck.TrimEndOnce("Left", true);
                                            if (!setDisallowedMounts.Contains(strCheck + "Right"))
                                                continue;
                                        }

                                        if (strCheck == objCyberware.HasModularMount)
                                        {
                                            objCyberware.DeleteCyberware();
                                            return;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(objCyberware.BlocksMounts))
                            {
                                if (!string.IsNullOrEmpty(objCyberware.Location) || !string.IsNullOrEmpty(objCyberwareParent.Location) ||
                                    (objCyberwareParent.Children.Any(x => x.Location == "Left") && objCyberwareParent.Children.Any(x => x.Location == "Right")))
                                {
                                    string[] astrBlockedMounts = objCyberware.BlocksMounts.Split(',');
                                    foreach (string strLoop in astrBlockedMounts)
                                    {
                                        if (setHasMounts.Contains(strLoop))
                                        {
                                            objCyberware.DeleteCyberware();
                                            return;
                                        }
                                    }
                                }
                            }
                        }

                        objCyberware.Grade = objCyberwareParent.Grade;
                        objCyberwareParent.Children.Add(objCyberware);

                        // Add any Weapons that come with the Cyberware.
                        XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                        if (objXmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objLoopNode in objXmlNodeList)
                            {
                                Weapon objWeapon = new Weapon(CharacterObject);
                                objWeapon.Load(objLoopNode, true);
                                CharacterObject.Weapons.Add(objWeapon);
                                objWeapon.ParentID = objCyberware.InternalId;
                                objCyberware.WeaponID = objWeapon.InternalId;
                            }
                        }

                        // Add any Vehicles that come with the Cyberware.
                        objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                        if (objXmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objLoopNode in objXmlNodeList)
                            {
                                Vehicle objVehicle = new Vehicle(CharacterObject);
                                objVehicle.Load(objLoopNode, true);
                                CharacterObject.Vehicles.Add(objVehicle);
                                objVehicle.ParentID = objCyberware.InternalId;
                                objCyberware.WeaponID = objVehicle.InternalId;
                            }
                        }

                        IsCharacterUpdateRequested = true;
                        IsDirty = true;
                    }
                    else
                    {
                        CharacterObject.Cyberware.Add(objCyberware);

                        // Add any Weapons that come with the Cyberware.
                        XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                        if (objXmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objLoopNode in objXmlNodeList)
                            {
                                Weapon objWeapon = new Weapon(CharacterObject);
                                objWeapon.Load(objLoopNode, true);
                                CharacterObject.Weapons.Add(objWeapon);
                                objWeapon.ParentID = objCyberware.InternalId;
                                objCyberware.WeaponID = objWeapon.InternalId;
                            }
                        }

                        // Add any Vehicles that come with the Cyberware.
                        objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                        if (objXmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objLoopNode in objXmlNodeList)
                            {
                                Vehicle objVehicle = new Vehicle(CharacterObject);
                                objVehicle.Load(objLoopNode, true);
                                CharacterObject.Vehicles.Add(objVehicle);
                                objVehicle.ParentID = objCyberware.InternalId;
                                objCyberware.WeaponID = objVehicle.InternalId;
                            }
                        }

                        IsCharacterUpdateRequested = true;
                        IsDirty = true;
                    }
                }
                else
                {
                    // Paste Gear
                    objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                    if (objXmlNode != null)
                    {
                        Gear objGear = new Gear(CharacterObject);
                        objGear.Load(objXmlNode, true);

                        // Paste Gear into a Cyberware.
                        Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(strSelectedId);
                        if (objCyberware != null)
                        {
                            bool blnDoAdd = false;
                            XmlNodeList xmlGearCategoryList = objCyberware.AllowGear?.SelectNodes("gearcategory");
                            if (xmlGearCategoryList?.Count > 0)
                            {
                                foreach (XmlNode objAllowed in xmlGearCategoryList)
                                {
                                    if (objAllowed.InnerText == objGear.Category)
                                    {
                                        blnDoAdd = true;
                                        break;
                                    }
                                }
                            }

                            if (!blnDoAdd)
                            {
                                objGear.DeleteGear();
                                return;
                            }

                            objCyberware.Gear.Add(objGear);
                            if (!objCyberware.IsModularCurrentlyEquipped)
                                objGear.ChangeEquippedStatus(false);

                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objGearWeapon = new Weapon(CharacterObject);
                                    objGearWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objGearWeapon);
                                    objGearWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objGearWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                        else
                        {
                            // Paste Gear into a Gear.
                            Gear objNewParent = CharacterObject.Cyberware.FindCyberwareGear(strSelectedId, out objCyberware);
                            if (objNewParent != null)
                            {
                                XmlNodeList xmlAddonCategoryList = objNewParent.GetNode()?.SelectNodes("addoncategory");
                                if (xmlAddonCategoryList?.Count > 0)
                                {
                                    bool blnDoAdd = false;
                                    foreach (XmlNode xmlCategory in xmlAddonCategoryList)
                                    {
                                        if (xmlCategory.InnerText == objGear.Category)
                                        {
                                            blnDoAdd = true;
                                            break;
                                        }
                                    }

                                    if (!blnDoAdd)
                                    {
                                        objGear.DeleteGear();
                                        return;
                                    }
                                }

                                objNewParent.Children.Add(objGear);
                                if (!objNewParent.Equipped || !objCyberware.IsModularCurrentlyEquipped)
                                    objGear.ChangeEquippedStatus(false);

                                IsCharacterUpdateRequested = true;
                                IsDirty = true;
                            }
                        }
                    }
                }
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                // Paste Vehicle.
                XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/vehicle");
                if (objXmlNode != null)
                {
                    Vehicle objVehicle = new Vehicle(CharacterObject);
                    objVehicle.Load(objXmlNode, true);

                    CharacterObject.Vehicles.Add(objVehicle);

                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
                else
                {
                    string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
                    if (string.IsNullOrEmpty(strSelectedId))
                        return;

                    // Paste Cyberware.
                    objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                    if (objXmlNode != null)
                    {
                        Cyberware objCyberware = new Cyberware(CharacterObject);
                        objCyberware.Load(objXmlNode, true);

                        // Paste Cyberware into a Cyberware.
                        Cyberware objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);
                        if (objCyberwareParent != null && !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                        {
                            if (objCyberware.PlugsIntoModularMount != objCyberwareParent.HasModularMount || objCyberwareParent.Children.Any(x => x.PlugsIntoModularMount == objCyberware.HasModularMount))
                            {
                                objCyberwareParent = null;
                            }
                            else
                            {
                                objCyberware.Location = objCyberwareParent.Location;
                            }
                        }

                        if (objCyberwareParent != null && objCyberware.SourceType == objCyberwareParent.SourceType)
                        {
                            string strAllowedSubsystems = objCyberwareParent.AllowedSubsystems;
                            if (!string.IsNullOrEmpty(strAllowedSubsystems))
                            {
                                bool blnDoAdd = false;
                                foreach (string strSubsystem in strAllowedSubsystems.Split(','))
                                {
                                    if (objCyberware.Category == strSubsystem)
                                    {
                                        blnDoAdd = true;
                                        break;
                                    }
                                }

                                if (!blnDoAdd)
                                {
                                    objCyberware.DeleteCyberware();
                                    return;
                                }
                            }

                            if (!string.IsNullOrEmpty(objCyberware.HasModularMount) || !string.IsNullOrEmpty(objCyberware.BlocksMounts))
                            {
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

                                if (!string.IsNullOrEmpty(objCyberware.HasModularMount) && setDisallowedMounts.Count > 0)
                                {
                                    foreach (string strLoop in setDisallowedMounts)
                                    {
                                        if (!strLoop.EndsWith("Right"))
                                        {
                                            string strCheck = strLoop;
                                            if (strCheck.EndsWith("Left"))
                                            {
                                                strCheck = strCheck.TrimEndOnce("Left", true);
                                                if (!setDisallowedMounts.Contains(strCheck + "Right"))
                                                    continue;
                                            }

                                            if (strCheck == objCyberware.HasModularMount)
                                            {
                                                objCyberware.DeleteCyberware();
                                                return;
                                            }
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(objCyberware.BlocksMounts))
                                {
                                    if (!string.IsNullOrEmpty(objCyberware.Location) || !string.IsNullOrEmpty(objCyberwareParent.Location) ||
                                        (objCyberwareParent.Children.Any(x => x.Location == "Left") && objCyberwareParent.Children.Any(x => x.Location == "Right")))
                                    {
                                        string[] astrBlockedMounts = objCyberware.BlocksMounts.Split(',');
                                        foreach (string strLoop in astrBlockedMounts)
                                        {
                                            if (setHasMounts.Contains(strLoop))
                                            {
                                                objCyberware.DeleteCyberware();
                                                return;
                                            }
                                        }
                                    }
                                }
                            }

                            objCyberware.Grade = objCyberwareParent.Grade;
                            objCyberwareParent.Children.Add(objCyberware);

                            // Add any Weapons that come with the Cyberware.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList?.Count > 0)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objWeapon = new Weapon(CharacterObject);
                                    objWeapon.Load(objLoopNode, true);
                                    CharacterObject.Weapons.Add(objWeapon);
                                    objWeapon.ParentID = objCyberware.InternalId;
                                    objCyberware.WeaponID = objWeapon.InternalId;
                                }
                            }

                            // Add any Vehicles that come with the Cyberware.
                            objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList?.Count > 0)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Vehicle objVehicle = new Vehicle(CharacterObject);
                                    objVehicle.Load(objLoopNode, true);
                                    CharacterObject.Vehicles.Add(objVehicle);
                                    objVehicle.ParentID = objCyberware.InternalId;
                                    objCyberware.WeaponID = objVehicle.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                        else if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) && objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                        {
                            // Add Cyberware to vehicle mod
                            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId && x.AllowCyberware);
                            if (objMod != null)
                            {
                                string strAllowedSubsystems = objMod.Subsystems;
                                if (!string.IsNullOrEmpty(strAllowedSubsystems))
                                {
                                    bool blnDoAdd = false;
                                    foreach (string strSubsystem in strAllowedSubsystems.Split(','))
                                    {
                                        if (objCyberware.Category == strSubsystem)
                                        {
                                            blnDoAdd = true;
                                            break;
                                        }
                                    }

                                    if (!blnDoAdd)
                                    {
                                        objCyberware.DeleteCyberware();
                                        return;
                                    }
                                }

                                if (!string.IsNullOrEmpty(objCyberware.HasModularMount) || !string.IsNullOrEmpty(objCyberware.BlocksMounts))
                                {
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

                                    if (!string.IsNullOrEmpty(objCyberware.HasModularMount) && setDisallowedMounts.Count > 0)
                                    {
                                        foreach (string strLoop in setDisallowedMounts)
                                        {
                                            if (!strLoop.EndsWith("Right"))
                                            {
                                                string strCheck = strLoop;
                                                if (strCheck.EndsWith("Left"))
                                                {
                                                    strCheck = strCheck.TrimEndOnce("Left", true);
                                                    if (!setDisallowedMounts.Contains(strCheck + "Right"))
                                                        continue;
                                                }

                                                if (strCheck == objCyberware.HasModularMount)
                                                {
                                                    objCyberware.DeleteCyberware();
                                                    return;
                                                }
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(objCyberware.BlocksMounts))
                                    {
                                        if (!string.IsNullOrEmpty(objCyberware.Location) || !string.IsNullOrEmpty(objCyberwareParent.Location) ||
                                            (objCyberwareParent.Children.Any(x => x.Location == "Left") && objCyberwareParent.Children.Any(x => x.Location == "Right")))
                                        {
                                            string[] astrBlockedMounts = objCyberware.BlocksMounts.Split(',');
                                            foreach (string strLoop in astrBlockedMounts)
                                            {
                                                if (setHasMounts.Contains(strLoop))
                                                {
                                                    objCyberware.DeleteCyberware();
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (objCyberware.Grade.Name != "Standard")
                                {
                                    objCyberware.Grade = CharacterObject.GetGradeList(objCyberware.SourceType, true).FirstOrDefault(x => x.Name == "Standard");
                                }
                                objMod.Cyberware.Add(objCyberware);

                                // Add any Weapons that come with the Cyberware.
                                XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                                if (objXmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode objLoopNode in objXmlNodeList)
                                    {
                                        Weapon objWeapon = new Weapon(CharacterObject);
                                        objWeapon.Load(objLoopNode, true);
                                        CharacterObject.Weapons.Add(objWeapon);
                                        objWeapon.ParentID = objCyberware.InternalId;
                                        objCyberware.WeaponID = objWeapon.InternalId;
                                    }
                                }

                                // Add any Vehicles that come with the Cyberware.
                                objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                                if (objXmlNodeList?.Count > 0)
                                {
                                    foreach (XmlNode objLoopNode in objXmlNodeList)
                                    {
                                        Vehicle objVehicle = new Vehicle(CharacterObject);
                                        objVehicle.Load(objLoopNode, true);
                                        CharacterObject.Vehicles.Add(objVehicle);
                                        objVehicle.ParentID = objCyberware.InternalId;
                                        objCyberware.WeaponID = objVehicle.InternalId;
                                    }
                                }

                                IsCharacterUpdateRequested = true;
                                IsDirty = true;
                            }
                            else
                            {
                                objCyberware.DeleteCyberware();
                            }
                        }
                        else
                        {
                            objCyberware.DeleteCyberware();
                        }
                    }
                    else
                    {
                        // Paste Gear.
                        objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");

                        if (objXmlNode != null)
                        {
                            Gear objGear = new Gear(CharacterObject);
                            objGear.Load(objXmlNode, true);

                            // Paste the Gear into a Vehicle's Gear.
                            Gear objVehicleGear = CharacterObject.Vehicles.FindVehicleGear(strSelectedId, out Vehicle objVehicle, out WeaponAccessory objAccessory, out Cyberware objCyberware);
                            if (objVehicleGear != null)
                            {
                                XmlNodeList xmlAddonCategoryList = objVehicleGear.GetNode()?.SelectNodes("addoncategory");
                                if (xmlAddonCategoryList?.Count > 0)
                                {
                                    bool blnDoAdd = false;
                                    foreach (XmlNode xmlCategory in xmlAddonCategoryList)
                                    {
                                        if (xmlCategory.InnerText == objGear.Category)
                                        {
                                            blnDoAdd = true;
                                            break;
                                        }
                                    }

                                    if (!blnDoAdd)
                                    {
                                        objGear.DeleteGear();
                                        return;
                                    }
                                }

                                objVehicleGear.Children.Add(objGear);
                            }
                            else
                            {
                                // Paste the Gear into a Vehicle.
                                objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == strSelectedId);
                                if (objVehicle != null)
                                {
                                    objVehicle.Gear.Add(objGear);
                                }
                                else
                                {
                                    objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strSelectedId);
                                    if (objAccessory != null)
                                    {
                                        bool blnDoAdd = false;
                                        XmlNodeList xmlGearCategoryList = objAccessory.AllowGear?.SelectNodes("gearcategory");
                                        if (xmlGearCategoryList?.Count > 0)
                                        {
                                            foreach (XmlNode objAllowed in xmlGearCategoryList)
                                            {
                                                if (objAllowed.InnerText == objGear.Category)
                                                {
                                                    blnDoAdd = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!blnDoAdd)
                                        {
                                            objGear.DeleteGear();
                                            return;
                                        }

                                        objVehicle = objAccessory.Parent.ParentVehicle;
                                        objAccessory.Gear.Add(objGear);
                                    }
                                    else
                                    {
                                        objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId, out VehicleMod objVehicleMod);
                                        if (objCyberware != null)
                                        {
                                            bool blnDoAdd = false;
                                            XmlNodeList xmlGearCategoryList = objCyberware.AllowGear?.SelectNodes("gearcategory");
                                            if (xmlGearCategoryList?.Count > 0)
                                            {
                                                foreach (XmlNode objAllowed in xmlGearCategoryList)
                                                {
                                                    if (objAllowed.InnerText == objGear.Category)
                                                    {
                                                        blnDoAdd = true;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (!blnDoAdd)
                                            {
                                                objGear.DeleteGear();
                                                return;
                                            }

                                            objVehicle = objVehicleMod.Parent;
                                            objCyberware.Gear.Add(objGear);
                                        }
                                        else
                                            return;
                                    }
                                }
                            }

                            objGear.ChangeEquippedStatus(false);
                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objGearWeapon = new Weapon(CharacterObject);
                                    objGearWeapon.Load(objLoopNode, true);
                                    objVehicle.Weapons.Add(objGearWeapon);
                                    objGearWeapon.ParentID = objGear.InternalId;
                                    objGear.WeaponID = objGearWeapon.InternalId;
                                }
                            }

                            IsCharacterUpdateRequested = true;

                            IsDirty = true;
                        }
                        else
                        {
                            // Paste Weapon.
                            objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/weapon");
                            if (objXmlNode != null)
                            {
                                VehicleMod objVehicleMod = null;
                                WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out Vehicle objVehicle);
                                if (objWeaponMount == null)
                                {
                                    objVehicleMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out objVehicle, out objWeaponMount);
                                    if (objVehicleMod == null)
                                        return;
                                }

                                Weapon objWeapon = new Weapon(CharacterObject) {ParentVehicle = objVehicle};
                                objWeapon.Load(objXmlNode, true);
                                if (objVehicleMod != null)
                                {
                                    // TODO: Make this not depend on string names
                                    if (objVehicleMod.Name.StartsWith("Mechanical Arm") || objVehicleMod.Name.Contains("Drone Arm"))
                                    {
                                        objVehicleMod.Weapons.Add(objWeapon);
                                    }
                                }
                                else
                                {
                                    objWeaponMount.Weapons.Add(objWeapon);
                                }

                                IsCharacterUpdateRequested = true;
                                IsDirty = true;
                            }
                        }
                    }
                }
            }
        }

        private void tsbCopy_Click(object sender, EventArgs e)
        {
            mnuEditCopy_Click(sender, e);
        }

        private void tsbPaste_Click(object sender, EventArgs e)
        {
            mnuEditPaste_Click(sender, e);
        }

        private void mnuSpecialBPAvailLimit_Click(object sender, EventArgs e)
        {
            frmSelectBuildMethod frmPickBP = new frmSelectBuildMethod(CharacterObject, true);
            frmPickBP.ShowDialog(this);

            if (frmPickBP.DialogResult != DialogResult.Cancel)
                IsCharacterUpdateRequested = true;
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
        #endregion

        #region Martial Tab Control Events
        private void treMartialArts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _blnSkipRefresh = true;
            string strSelectedId = treMartialArts.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                MartialArt objMartialArt = CharacterObject.MartialArts.FindById(strSelectedId);
                // The Rating NUD is only enabled if a Martial Art is currently selected.
                if (objMartialArt != null)
                {
                    cmdDeleteMartialArt.Enabled = !objMartialArt.IsQuality;
                    string strPage = objMartialArt.Page(GlobalOptions.Language);
                    lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objMartialArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objMartialArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                }
                else
                {
                    // Display the Martial Art Advantage information.
                    MartialArtTechnique objTechnique = CharacterObject.MartialArts.FindMartialArtTechnique(strSelectedId, out objMartialArt);
                    if (objTechnique != null)
                    {
                        cmdDeleteMartialArt.Enabled = true;
                        string strPage = objMartialArt.Page(GlobalOptions.Language);
                        lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objMartialArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objMartialArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                    }
                    else
                    {
                        #if LEGACY
                        // Display the Maneuver information.
                        MartialArtManeuver objManeuver = CharacterObject.MartialArtManeuvers.FindById(strSelectedId);
                        if (objManeuver != null)
                        {
                            cmdDeleteMartialArt.Enabled = true;
                            string strPage = objManeuver.Page(GlobalOptions.Language);
                            lblMartialArtSource.Text = CommonFunctions.LanguageBookShort(objManeuver.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                            GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, CommonFunctions.LanguageBookLong(objManeuver.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                        }
                        else
                        #endif
                        {
                            cmdDeleteMartialArt.Enabled = false;
                            lblMartialArtSource.Text = string.Empty;
                            GlobalOptions.ToolTipProcessor.SetToolTip(lblMartialArtSource, string.Empty);
                        }
                    }
                }
            }
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

        private void cmdAddSpell_Click(object sender, EventArgs e)
        {
            // Open the Spells XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("spells.xml");

            bool blnAddAgain;

            do
            {
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
                objSpell.Create(objXmlSpell, "", frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
                if (objSpell.InternalId.IsEmptyGuid())
                {
                    frmPickSpell.Dispose();
                    continue;
                }

                objSpell.FreeBonus = frmPickSpell.FreeBonus;
                // Barehanded Adept
                if (objSpell.FreeBonus && CharacterObject.AdeptEnabled && !CharacterObject.MagicianEnabled && objSpell.Range == "T")
                {
                    objSpell.UsesUnarmed = true;
                }
                CharacterObject.Spells.Add(objSpell);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
                frmPickSpell.Dispose();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteSpell_Click(object sender, EventArgs e)
        {
            // Locate the Spell that is selected in the tree.
            Spell objSpell = CharacterObject.Spells.FindById(treSpells.SelectedNode?.Tag.ToString());

            if (objSpell != null && objSpell.Grade == 0)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteSpell", GlobalOptions.Language)))
                    return;

                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Spell, objSpell.InternalId);

                CharacterObject.Spells.Remove(objSpell);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
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
            Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode?.Tag.ToString());
            if (objCyberware != null)
            {
                if (objCyberware.Capacity == "[*]" && treCyberware.SelectedNode.Level == 2 && !CharacterObject.IgnoreRules)
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
                    objParent.Children.Remove(objCyberware);
                else
                    CharacterObject.Cyberware.Remove(objCyberware);
            }
            else
            {
                // Find and remove the selected piece of Gear.
                Gear objGear = CharacterObject.Cyberware.FindCyberwareGear(treCyberware.SelectedNode?.Tag.ToString(), out objCyberware);
                if (objGear == null)
                    return;

                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear", GlobalOptions.Language)))
                    return;

                objGear.DeleteGear();

                Gear objParent = objGear.Parent;
                if (objParent != null)
                    objParent.Children.Remove(objGear);
                else
                    objCyberware.Gear.Remove(objGear);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdAddComplexForm_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("complexforms.xml");
            bool blnAddAgain;

            do
            {
                // The number of Complex Forms cannot exceed twice the character's RES.
                if (CharacterObject.ComplexForms.Count >= ((CharacterObject.RES.Value * 2) + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.ComplexFormLimit)) && !CharacterObject.IgnoreRules)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_ComplexFormLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ComplexFormLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objXmlComplexForm["translate"]?.InnerText ?? objXmlComplexForm["name"]?.InnerText)
                    };
                    frmPickText.ShowDialog(this);
                    strExtra = frmPickText.SelectedValue;
                }

                ComplexForm objComplexForm = new ComplexForm(CharacterObject);
                objComplexForm.Create(objXmlComplexForm, strExtra);
                if (objComplexForm.InternalId.IsEmptyGuid())
                    continue;

                CharacterObject.ComplexForms.Add(objComplexForm);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdAddAIProgram_Click(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("programs.xml");
            bool blnAddAgain;
            do
            {
                // Let the user select a Program.
                frmSelectAIProgram frmPickProgram = new frmSelectAIProgram(CharacterObject);
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
                        Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"]?.InnerText)
                    };
                    frmPickText.ShowDialog(this);
                    strExtra = frmPickText.SelectedValue;
                }

                AIProgram objProgram = new AIProgram(CharacterObject);
                objProgram.Create(objXmlProgram, strExtra);
                if (objProgram.InternalId.IsEmptyGuid())
                    continue;

                CharacterObject.AIPrograms.Add(objProgram);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteArmor_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treArmor.SelectedNode;
            if (objSelectedNode == null)
                return;

            string strSelectedId = objSelectedNode.Tag.ToString();
            if (!strSelectedId.IsGuid())
            {
                if (strSelectedId == LanguageManager.GetString("Node_SelectedArmor", GlobalOptions.Language))
                    return;

                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmorLocation", GlobalOptions.Language)))
                    return;

                foreach (Armor objArmor in CharacterObject.Armor)
                {
                    if (objArmor.Location == strSelectedId)
                        objArmor.Location = string.Empty;
                }

                // Remove the Location from the character
                CharacterObject.ArmorLocations.Remove(strSelectedId);
                return;
            }

            {
                Armor objArmor = CharacterObject.Armor.FindById(strSelectedId);
                if (objArmor != null)
                {
                    objArmor.DeleteArmor();
                    CharacterObject.Armor.Remove(objArmor);
                }
                else
                {
                    ArmorMod objMod = CharacterObject.Armor.FindArmorMod(strSelectedId);
                    if (objMod != null)
                    {
                        objMod.DeleteArmorMod();
                        objMod.Parent.ArmorMods.Remove(objMod);
                    }
                    else
                    {
                        Gear objGear = CharacterObject.Armor.FindArmorGear(strSelectedId, out objArmor, out objMod);
                        if (objGear != null)
                        {
                            objGear.DeleteGear();

                            Gear objGearParent = objGear.Parent;
                            if (objGearParent != null)
                                objGearParent.Children.Remove(objGear);
                            else if (objMod != null)
                                objMod.Gear.Remove(objGear);
                            else
                                objArmor?.Gear.Remove(objGear);
                        }
                        else
                            return;
                    }
                }
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

        private void cmdAddWeapon_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddWeapon(string.Empty);
            }
            while (blnAddAgain);
        }

        private bool AddWeapon(string strLocation)
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
            objWeapon.Location = strLocation;

            if (frmPickWeapon.FreeCost)
            {
                objWeapon.Cost = "0";
            }
            CharacterObject.Weapons.Add(objWeapon);

            foreach (Weapon objExtraWeapon in lstWeapons)
            {
                CharacterObject.Weapons.Add(objExtraWeapon);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickWeapon.AddAgain;
        }

        private void cmdDeleteWeapon_Click(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode != null)
            {
                string strSelectedId = treWeapons.SelectedNode.Tag.ToString();
                // Delete the selected Weapon.
                if (treWeapons.SelectedNode.Level == 0)
                {
                    if (strSelectedId == "Node_SelectedWeapons")
                        return;

                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeaponLocation", GlobalOptions.Language)))
                        return;

                    foreach (Weapon objWeapon in CharacterObject.Weapons)
                    {
                        if (objWeapon.Location == strSelectedId)
                            objWeapon.Location = string.Empty;
                    }
                    // Remove the Weapon Location from the character, then remove the selected node.
                    CharacterObject.WeaponLocations.Remove(strSelectedId);
                }
                else
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeapon", GlobalOptions.Language)))
                        return;

                    // Locate the Weapon that is selected in the tree.
                    Weapon objWeapon = CharacterObject.Weapons.DeepFindById(strSelectedId);

                    if (objWeapon != null)
                    {
                        // Cyberweapons cannot be removed through here and must be done by removing the piece of Cyberware.
                        if (objWeapon.Cyberware)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveCyberweapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveCyberweapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        if (objWeapon.Category == "Gear")
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveGearWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveGearWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        if (objWeapon.Category.StartsWith("Quality"))
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveQualityWeapon", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveQualityWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        objWeapon.DeleteWeapon();

                        if (objWeapon.Parent != null)
                            objWeapon.Parent.Children.Remove(objWeapon);
                        else
                            CharacterObject.Weapons.Remove(objWeapon);
                    }
                    else
                    {
                        // Locate the Accessory that is selected in the tree.
                        WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(strSelectedId);
                        if (objAccessory != null)
                        {
                            objAccessory.DeleteWeaponAccessory();
                            objAccessory.Parent.WeaponAccessories.Remove(objAccessory);
                        }
                        else
                        {
                            // Find the selected Gear.
                            Gear objGear = CharacterObject.Weapons.FindWeaponGear(strSelectedId, out objAccessory);
                            if (objGear != null)
                            {
                                objGear.DeleteGear();

                                Gear objParent = objGear.Parent;
                                if (objParent != null)
                                    objParent.Children.Remove(objGear);
                                else
                                    objAccessory.Gear.Remove(objGear);
                            }
                            else
                                return;
                        }
                    }
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
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

                CharacterObject.Lifestyles.Add(objLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void cmdDeleteLifestyle_Click(object sender, EventArgs e)
        {
            // Delete the selected Lifestyle.
            if (treLifestyles.SelectedNode != null && treLifestyles.SelectedNode.Level > 0)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteLifestyle", GlobalOptions.Language)))
                    return;

                Lifestyle objLifestyle = CharacterObject.Lifestyles.FindById(treLifestyles.SelectedNode.Tag.ToString());
                if (objLifestyle == null)
                    return;

                CharacterObject.Lifestyles.Remove(objLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdAddGear_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(string.Empty);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteGear_Click(object sender, EventArgs e)
        {
            string strSelectedId = treGear.SelectedNode?.Tag.ToString();
            // Delete the selected Gear.
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                if (!strSelectedId.IsGuid())
                {
                    if (strSelectedId == "Node_SelectedGear")
                        return;

                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteGearLocation", GlobalOptions.Language)))
                        return;

                    foreach (Gear objGear in CharacterObject.Gear)
                    {
                        if (objGear.Location == strSelectedId)
                            objGear.Location = string.Empty;
                    }

                    // Remove the Location from the character.
                    CharacterObject.GearLocations.Remove(strSelectedId);
                }
                else
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear", GlobalOptions.Language)))
                        return;

                    Gear objGear = CharacterObject.Gear.DeepFindById(strSelectedId);
                    if (objGear != null)
                    {
                        objGear.DeleteGear();

                        Gear objParent = objGear.Parent;

                        // If the Parent is populated, remove the item from its Parent.
                        if (objParent != null)
                            objParent.Children.Remove(objGear);
                        else
                            CharacterObject.Gear.Remove(objGear);
                    }
                    else
                        return;
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private bool AddVehicle(TreeNode nodParentNode)
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
            if (frmPickVehicle.FreeCost)
            {
                objVehicle.Cost = "0";
            }

            objVehicle.Location = nodParentNode?.Tag.ToString() ?? string.Empty;

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

            string strSelectedId = objSelectedNode.Tag.ToString();

            // Deleting a vehicle location
            if (!strSelectedId.IsGuid())
            {
                if (strSelectedId == "Node_SelectedVehicles")
                    return;

                if (objSelectedNode.Level == 0)
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicleLocationBase", GlobalOptions.Language)))
                        return;

                    foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                    {
                        if (objVehicle.Location == strSelectedId)
                            objVehicle.Location = string.Empty;
                    }

                    // Remove the Location from the character, then remove the selected node.
                    CharacterObject.VehicleLocations.Remove(strSelectedId);
                }
                else
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicleLocation", GlobalOptions.Language)))
                        return;

                    TreeNode objVehicleNode = objSelectedNode;
                    while (objVehicleNode.Level > 1)
                        objVehicleNode = objVehicleNode.Parent;

                    Vehicle objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == objVehicleNode.Tag.ToString());

                    if (objVehicle != null)
                    {
                        foreach (Gear objGear in objVehicle.Gear)
                        {
                            if (objGear.Location == strSelectedId)
                                objGear.Location = string.Empty;
                        }

                        // Remove the Location from the vehicle, then remove the selected node.
                        objVehicle.Locations.Remove(strSelectedId);
                    }
                }
            }
            else
            {
                // Weapons that are first-level children of vehicles cannot be removed (for some reason)
                foreach (Vehicle objCharacterVehicle in CharacterObject.Vehicles)
                {
                    if (objCharacterVehicle.Weapons.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strSelectedId) != null)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_CannotRemoveGearWeaponVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotRemoveGearWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle", GlobalOptions.Language)))
                    return;

                // Locate the Vehicle that is selected in the tree.
                Vehicle objVehicle = CharacterObject.Vehicles.FindById(strSelectedId);

                // Removing a Vehicle
                if (objVehicle != null)
                {
                    objVehicle.DeleteVehicle();
                    CharacterObject.Vehicles.Remove(objVehicle);
                }
                else
                {
                    WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out objVehicle);
                    // Removing a Weapon Mount
                    if (objWeaponMount != null)
                    {
                        objWeaponMount.DeleteWeaponMount();
                        objVehicle.WeaponMounts.Remove(objWeaponMount);
                    }
                    else
                    {
                        // Locate the VehicleMod that is selected in the tree.
                        VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out objVehicle, out objWeaponMount);
                        // Removing a Vehicle Mod
                        if (objMod != null)
                        {
                            // Check for Improved Sensor bonus.
                            if (objMod.Bonus?["improvesensor"] != null || (objMod.WirelessOn && objMod.WirelessBonus?["improvesensor"] != null))
                            {
                                objVehicle.ChangeVehicleSensor(treVehicles, false, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
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
                                decimal decVehicleCost = objVehicle.OwnCost;

                                // Make sure the character has enough Nuyen for the expense.
                                decimal decCost = decVehicleCost * decPercentage / 100;

                                // Create a Vehicle Mod for the Retrofit.
                                VehicleMod objRetrofit = new VehicleMod(CharacterObject);

                                XmlDocument objVehiclesDoc = XmlManager.Load("vehicles.xml");
                                XmlNode objXmlNode = objVehiclesDoc.SelectSingleNode("/chummer/mods/mod[name = \"Retrofit\"]");
                                objRetrofit.Create(objXmlNode, 0, objVehicle);
                                objRetrofit.Cost = decCost.ToString(GlobalOptions.InvariantCultureInfo);
                                objRetrofit.IncludedInVehicle = true;
                                objVehicle.Mods.Add(objRetrofit);
                            }

                            objMod.DeleteVehicleMod();
                            if (objWeaponMount != null)
                                objWeaponMount.Mods.Remove(objMod);
                            else
                                objVehicle.Mods.Remove(objMod);
                        }
                        else
                        {
                            Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strSelectedId, out objVehicle, out objWeaponMount, out objMod);
                            // Removing a Weapon
                            if (objWeapon != null)
                            {
                                objWeapon.DeleteWeapon();
                                if (objWeapon.Parent != null)
                                    objWeapon.Parent.Children.Remove(objWeapon);
                                else if (objMod != null)
                                    objMod.Weapons.Remove(objWeapon);
                                else if (objWeaponMount != null)
                                    objWeaponMount.Weapons.Remove(objWeapon);
                                // This bit here should never be reached, but I'm adding it for future-proofing in case we want people to be able to remove weapons attached directly to vehicles
                                else
                                    objVehicle.Weapons.Remove(objWeapon);
                            }
                            else
                            {
                                WeaponAccessory objWeaponAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strSelectedId);
                                // Removing a weapon accessory
                                if (objWeaponAccessory != null)
                                {
                                    objWeaponAccessory.DeleteWeaponAccessory();
                                    objWeaponAccessory.Parent.WeaponAccessories.Remove(objWeaponAccessory);
                                }
                                else
                                {
                                    Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId, out objMod);
                                    // Removing Cyberware
                                    if (objCyberware != null)
                                    {
                                        Cyberware objParent = objCyberware.Parent;
                                        if (objParent != null)
                                            objParent.Children.Remove(objCyberware);
                                        else
                                            objMod.Cyberware.Remove(objCyberware);

                                        objCyberware.DeleteCyberware();
                                    }
                                    else
                                    {
                                        Gear objGear = CharacterObject.Vehicles.FindVehicleGear(strSelectedId, out objVehicle, out objWeaponAccessory, out objCyberware);
                                        if (objGear != null)
                                        {
                                            Gear objParent = objGear.Parent;
                                            if (objParent != null)
                                                objParent.Children.Remove(objGear);
                                            else if (objCyberware != null)
                                                objCyberware.Gear.Remove(objGear);
                                            else if (objWeaponAccessory != null)
                                                objWeaponAccessory.Gear.Remove(objGear);
                                            else
                                                objVehicle.Gear.Remove(objGear);

                                            objGear.DeleteGear();
                                        }
                                        else
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }

                IsCharacterUpdateRequested = true;
            }

            IsDirty = true;
        }

        private void cmdAddMartialArt_Click(object sender, EventArgs e)
        {
            frmSelectMartialArt frmPickMartialArt = new frmSelectMartialArt(CharacterObject);
            frmPickMartialArt.ShowDialog(this);

            if (frmPickMartialArt.DialogResult == DialogResult.Cancel)
                return;

            // Open the Martial Arts XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("martialarts.xml");

            XmlNode objXmlArt = objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[id = \"" + frmPickMartialArt.SelectedMartialArt + "\"]");

            MartialArt objMartialArt = new MartialArt(CharacterObject);
            objMartialArt.Create(objXmlArt);
            CharacterObject.MartialArts.Add(objMartialArt);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdDeleteLimitModifier_Click(object sender, EventArgs e)
        {
            if (treLimit.SelectedNode == null || treLimit.SelectedNode.Level <= 0)
                return;

            LimitModifier objLimitModifier = CharacterObject.LimitModifiers.FindById(treLimit.SelectedNode.Tag.ToString());
            if (objLimitModifier == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotDeleteLimitModifier", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotDeleteLimitModifier", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteLimitModifier", GlobalOptions.Language)))
                return;

            // Delete the selected Limit Modifier.
            CharacterObject.LimitModifiers.Remove(objLimitModifier);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdDeleteMartialArt_Click(object sender, EventArgs e)
        {
            string strSelectedId = treMartialArts.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Delete the selected Martial Art.
                MartialArt objMartialArt = CharacterObject.MartialArts.FindById(strSelectedId);
                if (objMartialArt != null && !objMartialArt.IsQuality)
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt", GlobalOptions.Language)))
                        return;

                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);
                    // Remove the Improvements for any Advantages for the Martial Art that is being removed.
                    foreach (MartialArtTechnique objAdvantage in objMartialArt.Techniques)
                    {
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.MartialArtTechnique, objAdvantage.InternalId);
                    }

                    CharacterObject.MartialArts.Remove(objMartialArt);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
                else
                {
                    // Find the selected Advantage object.
                    MartialArtTechnique objSelectedAdvantage = CharacterObject.MartialArts.FindMartialArtTechnique(strSelectedId, out objMartialArt);
                    if (objSelectedAdvantage != null)
                    {
                        if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteMartialArt", GlobalOptions.Language)))
                            return;

                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.MartialArtTechnique, objSelectedAdvantage.InternalId);

                        objMartialArt.Techniques.Remove(objSelectedAdvantage);

                        IsCharacterUpdateRequested = true;

                        IsDirty = true;
                    }
                }
            }
        }

#if LEGACY
        private void cmdAddManeuver_Click(object sender, EventArgs e)
        {
            // Characters may only have 2 Maneuvers per Martial Art Rating.
            int intTotalRating = 0;
            foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
                intTotalRating += objMartialArt.Rating * 2;

            if (CharacterObject.MartialArtManeuvers.Count >= intTotalRating && !CharacterObject.IgnoreRules)
            {
                MessageBox.Show(LanguageManager.GetString("Message_MartialArtManeuverLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_MartialArtManeuverLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.InitiateGrade + 1, CharacterObject.MAGEnabled, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);
            }
            else if (CharacterObject.RESEnabled)
            {
                tsMetamagicAddArt.Visible = false;
                tsMetamagicAddEnchantment.Visible = false;
                tsMetamagicAddEnhancement.Visible = false;
                tsMetamagicAddRitual.Visible = false;
                tsMetamagicAddMetamagic.Text = LanguageManager.GetString("Button_AddEcho", GlobalOptions.Language);

                // Make sure that the Initiate Grade is not attempting to go above the character's RES CharacterAttribute.
                if (CharacterObject.SubmersionGrade + 1 > CharacterObject.RES.TotalValue)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_CannotIncreaseSubmersionGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CannotIncreaseSubmersionGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create the Initiate Grade object.
                InitiationGrade objGrade = new InitiationGrade(CharacterObject);
                objGrade.Create(CharacterObject.SubmersionGrade + 1, CharacterObject.RESEnabled, chkInitiationGroup.Checked, chkInitiationOrdeal.Checked, chkInitiationSchooling.Checked);
                CharacterObject.InitiationGrades.AddWithSort(objGrade);
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cmdDeleteMetamagic_Click(object sender, EventArgs e)
        {
            string strSelectedId = treMetamagic.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // We're deleting an entire grade
                InitiationGrade objGrade = CharacterObject.InitiationGrades.FindById(strSelectedId);
                if (objGrade != null)
                {
                    string strMessage;
                    // Stop if this isn't the highest grade
                    if (CharacterObject.MAGEnabled)
                    {
                        if (objGrade.Grade != CharacterObject.InitiateGrade)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_DeleteGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_DeleteGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        strMessage = LanguageManager.GetString("Message_DeleteInitiateGrade", GlobalOptions.Language);
                    }
                    else if (CharacterObject.RESEnabled)
                    {
                        if (objGrade.Grade != CharacterObject.SubmersionGrade)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_DeleteGrade", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_DeleteGrade", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        strMessage = LanguageManager.GetString("Message_DeleteSubmersionGrade", GlobalOptions.Language);
                    }
                    else
                        return;

                    if (!CharacterObject.ConfirmDelete(strMessage))
                        return;
                }
                else
                {
                    // We're deleting a single bonus attached to a grade
                    Art objArt = CharacterObject.Arts.FindById(strSelectedId);
                    if (objArt != null)
                    {
                        if (objArt.Grade <= 0)
                            return;
                        string strMessage = LanguageManager.GetString("Message_DeleteArt", GlobalOptions.Language);
                        if (!CharacterObject.ConfirmDelete(strMessage))
                            return;

                        CharacterObject.Arts.Remove(objArt);
                    }
                    else
                    {
                        Metamagic objMetamagic = CharacterObject.Metamagics.FindById(strSelectedId);
                        if (objMetamagic != null)
                        {
                            if (objMetamagic.Grade <= 0)
                                return;
                            string strMessage;
                            if (CharacterObject.MAGEnabled)
                                strMessage = LanguageManager.GetString("Message_DeleteMetamagic", GlobalOptions.Language);
                            else if (CharacterObject.RESEnabled)
                                strMessage = LanguageManager.GetString("Message_DeleteEcho", GlobalOptions.Language);
                            else
                                return;
                            if (!CharacterObject.ConfirmDelete(strMessage))
                                return;

                            CharacterObject.Metamagics.Remove(objMetamagic);
                            ImprovementManager.RemoveImprovements(CharacterObject, objMetamagic.SourceType, objMetamagic.InternalId);
                        }
                        else
                        {
                            Enhancement objEnhancement = CharacterObject.FindEnhancement(strSelectedId);
                            if (objEnhancement != null)
                            {
                                if (objEnhancement.Grade <= 0)
                                    return;
                                string strMessage = LanguageManager.GetString("Message_DeleteEnhancement", GlobalOptions.Language);
                                if (!CharacterObject.ConfirmDelete(strMessage))
                                    return;

                                CharacterObject.Enhancements.Remove(objEnhancement);
                                foreach (Power objPower in CharacterObject.Powers)
                                {
                                    if (objPower.Enhancements.Contains(objEnhancement))
                                        objPower.Enhancements.Remove(objEnhancement);
                                }
                            }
                            else
                            {
                                Spell objSpell = CharacterObject.Spells.FindById(strSelectedId);
                                if (objSpell != null)
                                {
                                    if (objSpell.Grade <= 0)
                                        return;
                                    string strMessage = LanguageManager.GetString("Message_DeleteSpell", GlobalOptions.Language);
                                    if (!CharacterObject.ConfirmDelete(strMessage))
                                        return;

                                    CharacterObject.Spells.Remove(objSpell);
                                }
                                else
                                {
                                    ComplexForm objComplexForm = CharacterObject.ComplexForms.FindById(strSelectedId);
                                    if (objComplexForm != null)
                                    {
                                        if (objComplexForm.Grade <= 0)
                                            return;
                                        string strMessage = LanguageManager.GetString("Message_DeleteComplexForm", GlobalOptions.Language);
                                        if (!CharacterObject.ConfirmDelete(strMessage))
                                            return;

                                        CharacterObject.ComplexForms.Remove(objComplexForm);
                                    }
                                    else
                                        return;
                                }
                            }
                        }
                    }
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
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

                IsCharacterUpdateRequested = true;

                IsDirty = true;
                frmPickCritterPower.Dispose();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCritterPower_Click(object sender, EventArgs e)
        {
            // Locate the selected Critter Power.
            CritterPower objPower = CharacterObject.CritterPowers.FindById(treCritterPowers.SelectedNode?.Tag.ToString());

            if (objPower != null && objPower.Grade == 0)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteCritterPower", GlobalOptions.Language)))
                    return;

                // Remove any Improvements that were created by the Critter Power.
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.CritterPower, objPower.InternalId);

                CharacterObject.CritterPowers.Remove(objPower);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdDeleteComplexForm_Click(object sender, EventArgs e)
        {
            // Locate the Complex Form that is selected in the tree.
            ComplexForm objComplexForm = CharacterObject.ComplexForms.FindById(treComplexForms.SelectedNode?.Tag.ToString());

            if (objComplexForm != null && objComplexForm.Grade == 0)
            {
                if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteComplexForm", GlobalOptions.Language)))
                    return;

                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId);

                CharacterObject.ComplexForms.Remove(objComplexForm);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdDeleteAIProgram_Click(object sender, EventArgs e)
        {
            // Delete the selected AI Program.
            if (treAIPrograms.SelectedNode.Level == 1)
            {
                // Locate the Program that is selected in the tree.
                AIProgram objProgram = CharacterObject.AIPrograms.FindById(treAIPrograms.SelectedNode.Tag.ToString());

                if (objProgram != null && objProgram.CanDelete)
                {
                    if (!CharacterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteAIProgram", GlobalOptions.Language)))
                        return;

                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.AIProgram, objProgram.InternalId);

                    CharacterObject.AIPrograms.Remove(objProgram);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void cmdLifeModule_Click(object sender, EventArgs e)
        {
            XmlNode xmlStagesParentNode = XmlManager.Load("lifemodules.xml").SelectSingleNode("chummer/stages");

            bool blnAddAgain;
            do
            {
                //from 1 to second highest life module order possible (ye hardcoding is bad, but extra stage is a niche case)
                int intStage;
                for (intStage = 1; intStage < 5; ++intStage)
                {
                    XmlNode xmlStageNode = xmlStagesParentNode?.SelectSingleNode("stage[@order = \"" + intStage + "\"]");
                    if (xmlStageNode == null)
                    {
                        intStage -= 1;
                        break;
                    }
                    if (!CharacterObject.Qualities.Any(x => x.Type == QualityType.LifeModule && x.Stage == xmlStageNode.InnerText))
                    {
                        break;
                    }
                }
                //i--; //Counter last increment
                frmSelectLifeModule frmSelectLifeModule = new frmSelectLifeModule(CharacterObject, intStage);
                frmSelectLifeModule.ShowDialog(this);

                if (frmSelectLifeModule.DialogResult == DialogResult.Cancel)
                {
                    frmSelectLifeModule.Dispose();
                    break;
                }
                blnAddAgain = frmSelectLifeModule.AddAgain;

                XmlNode objXmlLifeModule = frmSelectLifeModule.SelectedNode;

                frmSelectLifeModule.Dispose();

                List<Weapon> lstWeapons = new List<Weapon>();
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

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
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

                XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmPickQuality.SelectedQuality + "\"]");
                List<Weapon> lstWeapons = new List<Weapon>();
                Quality objQuality = new Quality(CharacterObject);

                objQuality.Create(objXmlQuality, QualitySource.Selected, lstWeapons);
                if (objQuality.InternalId.IsEmptyGuid())
                {
                    // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                    frmPickQuality.Dispose();
                    continue;
                }

                if (frmPickQuality.FreeCost)
                    objQuality.BP = 0;

                // If the item being checked would cause the limit of 25 BP spent on Positive Qualities to be exceed, do not let it be checked and display a message.
                string strAmount = CharacterObject.GameplayOptionQualityLimit.ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
                int intMaxQualityAmount = CharacterObject.GameplayOptionQualityLimit;

                // Make sure that adding the Quality would not cause the character to exceed their BP limits.
                int intBP = 0;
                bool blnAddItem = true;

                // Add the cost of the Quality that is being added.
                if (objQuality.ContributeToLimit)
                    intBP += objQuality.BP;

                if (objQuality.Type == QualityType.Negative)
                {
                    // Calculate the cost of the current Negative Qualities.
                    foreach (Quality objCharacterQuality in CharacterObject.Qualities)
                    {
                        if (objCharacterQuality.Type == QualityType.Negative && objCharacterQuality.ContributeToLimit)
                            intBP += objCharacterQuality.BP;
                    }

                    // Include the BP used by Enemies.
                    if (CharacterObjectOptions.EnemyKarmaQualityLimit)
                    {
                        // Include the BP used by Enemies.
                        string strEnemiesBPText = lblEnemiesBP.Text.FastEscapeOnceFromEnd(LanguageManager.GetString("String_Karma", GlobalOptions.Language)).NormalizeWhiteSpace();
                        if (int.TryParse(strEnemiesBPText, out int intTemp))
                            intBP += intTemp;
                    }

                    // Include the amount from Free Negative Quality BP cost Improvements.
                    intBP -= (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeNegativeQualities) * CharacterObjectOptions.KarmaQuality);

                    // Check if adding this Quality would put the character over their limit.
                    if (!CharacterObjectOptions.ExceedNegativeQualities)
                    {
                        if (intBP < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language).Replace("{0}", strAmount), LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            blnAddItem = false;
                        }
                        if (CharacterObject.MetatypeBP < 0)
                        {
                            if ((intBP + CharacterObject.MetatypeBP) < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                            {
                                MessageBox.Show(LanguageManager.GetString("Message_NegativeQualityAndMetatypeLimit", GlobalOptions.Language).Replace("{0}", strAmount), LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
                        }
                    }
                }
                else
                {
                    if (objQuality.ContributeToLimit || objQuality.ContributeToBP)
                    {
                        // Calculate the cost of the current Positive Qualities.
                        foreach (Quality objCharacterQuality in CharacterObject.Qualities)
                        {
                            if (objCharacterQuality.Type == QualityType.Positive && objCharacterQuality.ContributeToLimit)
                                intBP += objCharacterQuality.BP;
                        }
                        if (CharacterObject.BuildMethod == CharacterBuildMethod.Karma)
                            intBP *= CharacterObjectOptions.KarmaQuality;

                        // Include the amount from Free Negative Quality BP cost Improvements.
                        intBP -= (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreePositiveQualities) *
                                  CharacterObjectOptions.KarmaQuality);

                        // Check if adding this Quality would put the character over their limit.
                        if (!CharacterObjectOptions.ExceedPositiveQualities)
                        {
                            if (intBP > intMaxQualityAmount && !CharacterObject.IgnoreRules)
                            {
                                MessageBox.Show(
                                    LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language)
                                        .Replace("{0}", strAmount),
                                    LanguageManager.GetString("MessageTitle_PositiveQualityLimit", GlobalOptions.Language),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
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

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
                else
                {
                    // If the Quality could not be added, remove the Improvements that were added during the Quality Creation process.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                }

                frmPickQuality.Dispose();
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
                List<Weapon> objAddWeapons = new List<Weapon>();
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
                int intBP = 0;
                if (objSelectedQuality.Type == QualityType.Negative)
                {
                    intBP = Convert.ToInt32(objXmlDeleteQuality["karma"]?.InnerText) * -1;
                }
                intBP *= CharacterObjectOptions.KarmaQuality;
                int intShowBP = intBP;
                if (blnCompleteDelete)
                    intShowBP *= objSelectedQuality.Levels;
                string strBP = intShowBP.ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);

                if (blnConfirmDelete && !CharacterObject.ConfirmDelete(blnCompleteDelete ?
                        LanguageManager.GetString("Message_DeleteMetatypeQuality", GlobalOptions.Language).Replace("{0}", strBP) :
                        LanguageManager.GetString("Message_LowerMetatypeQualityLevel", GlobalOptions.Language).Replace("{0}", strBP)))
                    return false;

                // Remove any Improvements that the Quality might have.
                XmlNode xmlDeleteQualityNoBonus = objXmlDeleteQuality.Clone();
                if (xmlDeleteQualityNoBonus["bonus"] != null)
                    xmlDeleteQualityNoBonus["bonus"].InnerText = string.Empty;

                List<Weapon> lstWeapons = new List<Weapon>();
                Quality objReplaceQuality = new Quality(CharacterObject);

                objReplaceQuality.Create(xmlDeleteQualityNoBonus, QualitySource.Selected, lstWeapons);
                objReplaceQuality.BP *= -1;
                // If a Negative Quality is being bought off, the replacement one is Positive.
                if (objSelectedQuality.Type == QualityType.Positive)
                    objSelectedQuality.Type = QualityType.Negative;
                else
                    objReplaceQuality.Type = QualityType.Positive;
                // The replacement Quality does not count towards the BP limit of the new type, nor should it be printed.
                objReplaceQuality.AllowPrint = false;
                objReplaceQuality.ContributeToLimit = false;
                CharacterObject.Qualities.Add(objReplaceQuality);
                // The replacement Quality no longer adds its weapons to the character
            }
            else
            {
                if (blnConfirmDelete && !CharacterObject.ConfirmDelete(blnCompleteDelete ? LanguageManager.GetString("Message_DeleteQuality", GlobalOptions.Language) : LanguageManager.GetString("Message_LowerQualityLevel", GlobalOptions.Language)))
                    return false;
            }

            // Remove the Improvements that were created by the Quality.
            ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objSelectedQuality.InternalId);

            if (objSelectedQuality.Type == QualityType.LifeModule)
            {
                objXmlDeleteQuality = Quality.GetNodeOverrideable(objSelectedQuality.QualityId, XmlManager.Load("lifemodules.xml", GlobalOptions.Language));
            }

            // Remove any Weapons created by the Quality if applicable.
            if (!objSelectedQuality.WeaponID.IsEmptyGuid())
            {
                List<Weapon> lstWeapons = CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objSelectedQuality.InternalId).ToList();
                foreach (Weapon objWeapon in lstWeapons)
                {
                    if (objWeapon.ParentID == objSelectedQuality.InternalId)
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

            // Fix for legacy characters with old addqualities improvements.
            RemoveAddedQualities(objXmlDeleteQuality?.SelectNodes("addqualities/addquality"));

            CharacterObject.Qualities.Remove(objSelectedQuality);
            return true;
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treQualities.SelectedNode?.Tag is Quality objSelectedQuality)
            {
                string strInternalIDToRemove = objSelectedQuality.QualityId;
                // Can't do a foreach because we're removing items, this is the next best thing
                bool blnFirstRemoval = true;
                for (int i = CharacterObject.Qualities.Count - 1; i >= 0; --i)
                {
                    Quality objLoopQuality = CharacterObject.Qualities.ElementAt(i);
                    if (objLoopQuality.QualityId == strInternalIDToRemove)
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
            CharacterObject.GearLocations.Add(strLocation);

            IsDirty = true;
        }

        private void cmdAddWeaponLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Weapons Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;

            string strLocation = frmPickText.SelectedValue;
            CharacterObject.WeaponLocations.Add(strLocation);

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
            decimal decCost = 0;
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

        private void cmdAddArmor_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddArmor(string.Empty);
            }
            while (blnAddAgain);
        }

        private bool AddArmor(string strLocation)
        {
            frmSelectArmor frmPickArmor = new frmSelectArmor(CharacterObject);
            frmPickArmor.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickArmor.DialogResult == DialogResult.Cancel)
                return false;

            // Open the Armor XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("armor.xml");

            XmlNode objXmlArmor = objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = \"" + frmPickArmor.SelectedArmor + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Armor objArmor = new Armor(CharacterObject);

            objArmor.Create(objXmlArmor, frmPickArmor.Rating, lstWeapons);
            objArmor.DiscountCost = frmPickArmor.BlackMarketDiscount;
            if (objArmor.InternalId.IsEmptyGuid())
                return frmPickArmor.AddAgain;
            objArmor.Location = strLocation;
            if (frmPickArmor.FreeCost)
            {
                objArmor.Cost = "0";
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
            CharacterObject.ArmorLocations.Add(strLocation);

            IsDirty = true;
        }

        private void cmdArmorEquipAll_Click(object sender, EventArgs e)
        {
            string strSelectedId = treArmor.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Equip all of the Armor in the Armor Bundle.
                foreach (Armor objArmor in CharacterObject.Armor)
                {
                    if (objArmor.Location == strSelectedId || (strSelectedId == "Node_SelectedArmor" && string.IsNullOrEmpty(objArmor.Location)))
                    {
                        objArmor.Equipped = true;
                    }
                }
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdArmorUnEquipAll_Click(object sender, EventArgs e)
        {
            string strSelectedId = treArmor.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // En-equip all of the Armor in the Armor Bundle.
                foreach (Armor objArmor in CharacterObject.Armor)
                {
                    if (objArmor.Location == strSelectedId || (strSelectedId == "Node_SelectedArmor" && string.IsNullOrEmpty(objArmor.Location)))
                    {
                        objArmor.Equipped = false;
                    }
                }
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdAddVehicleLocation_Click(object sender, EventArgs e)
        {
            // Make sure a Vehicle is selected.
            Vehicle objVehicle = CharacterObject.Vehicles.FindById(treVehicles.SelectedNode?.Tag.ToString());
            if (objVehicle == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleLocation", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Add a new location to the selected Vehicle.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            string strLocation = frmPickText.SelectedValue;
            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(strLocation))
                return;

            objVehicle.Locations.Add(strLocation);

            IsDirty = true;
        }
#endregion

#region ContextMenu Events
        private void tsCyberwareAddAsPlugin_Click(object sender, EventArgs e)
        {
            string strSelectedId = treCyberware.SelectedNode?.Tag.ToString();
            Cyberware objCyberware = !string.IsNullOrEmpty(strSelectedId) ? CharacterObject.Cyberware.DeepFindById(strSelectedId) : null;
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (objCyberware == null)
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
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            Cyberware objCyberware = !string.IsNullOrEmpty(strSelectedId) ? CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId) : null;
            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (objCyberware == null)
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            // Locate the Weapon that is selected in the Tree.
            Weapon objWeapon = objSelectedNode?.Level > 0 ? CharacterObject.Weapons.DeepFindById(objSelectedNode.Tag.ToString()) : null;

            if (objWeapon == null)
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
                objXmlWeapon = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/accessories/accessory[id = \"" + frmPickWeaponAccessory.SelectedAccessory + "\"]");

                WeaponAccessory objAccessory = new WeaponAccessory(CharacterObject);
                objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, Convert.ToInt32(frmPickWeaponAccessory.SelectedRating));
                objAccessory.Parent = objWeapon;

                if (frmPickWeaponAccessory.FreeCost)
                {
                    objAccessory.Cost = "0";
                }
                else if (objAccessory.Cost.StartsWith("Variable("))
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
                objWeapon.WeaponAccessories.Add(objAccessory);

                IsCharacterUpdateRequested = true;
                IsDirty = true;

                frmPickWeaponAccessory.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsAddArmorMod_Click(object sender, EventArgs e)
        {
            while (treArmor.SelectedNode != null && treArmor.SelectedNode.Level > 1)
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;

            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent item is selected, then open the Select Accessory window.
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Armor that is selected in the tree.
            Armor objArmor = CharacterObject.Armor.FindById(objSelectedNode.Tag.ToString());

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
                int intRating = Convert.ToInt32(objXmlArmor?["maxrating"]?.InnerText) > 1 ? frmPickArmorMod.SelectedRating : 0;

                objMod.Create(objXmlArmor, intRating, lstWeapons);
                if (objMod.InternalId.IsEmptyGuid())
                {
                    frmPickArmorMod.Dispose();
                    continue;
                }

                objArmor.ArmorMods.Add(objMod);

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
            string strSelectedId = treGear.SelectedNode?.Tag.ToString();
            // Make sure a parent items is selected, then open the Select Gear window.
            if (string.IsNullOrEmpty(strSelectedId) || !strSelectedId.IsGuid())
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(strSelectedId);
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            Vehicle objVehicle = CharacterObject.Vehicles.FindById(treVehicles.SelectedNode?.Tag.ToString());
            if (objVehicle == null)
                return;
            frmCreateWeaponMount frmPickVehicleMod = new frmCreateWeaponMount(objVehicle, CharacterObject);
            frmPickVehicleMod.ShowDialog(this);

            if (frmPickVehicleMod.DialogResult != DialogResult.Cancel)
            {
                WeaponMount objWeaponMount = frmPickVehicleMod.WeaponMount;
                if (frmPickVehicleMod.FreeCost)
                    objWeaponMount.Cost = "0";
                objVehicle.WeaponMounts.Add(objWeaponMount);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void tsVehicleAddMod_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            while (objSelectedNode != null && objSelectedNode.Level > 1)
                objSelectedNode = objSelectedNode.Parent;

            Vehicle objVehicle = objSelectedNode?.Level > 0 ? CharacterObject.Vehicles.FindById(objSelectedNode.Tag.ToString()) : null;
            // Make sure a parent items is selected, then open the Select Vehicle Mod window.
            if (objVehicle == null)
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
                    // Set the Vehicle properties for the window.
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
                if (frmPickVehicleMod.FreeCost)
                    objMod.Cost = "0";
                else
                {
                    // Multiply the cost if applicable.
                    decimal decOldCost = objMod.TotalCost;
                    decimal decCost = decOldCost;
                    char chrAvail = objMod.TotalAvailTuple().Suffix;
                    if (chrAvail == 'R' && CharacterObjectOptions.MultiplyRestrictedCost)
                        decCost *= CharacterObjectOptions.RestrictedCostMultiplier;
                    if (chrAvail == 'F' && CharacterObjectOptions.MultiplyForbiddenCost)
                        decCost *= CharacterObjectOptions.ForbiddenCostMultiplier;
                    decCost -= decOldCost;
                    objMod.Markup = decCost;
                }

                objVehicle.Mods.Add(objMod);

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
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
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
                frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject)
                {
                    LimitToCategories = objMod == null ? objWeaponMount.AllowedWeaponCategories : objMod.WeaponMountCategories
                };
                frmPickWeapon.ShowDialog();

                if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                    return;

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

                blnAddAgain = frmPickWeapon.AddAgain;
                frmPickWeapon.Dispose();
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddWeaponAccessory_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(treVehicles.SelectedNode?.Tag.ToString());
            if (objWeapon == null)
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

                if (frmPickWeaponAccessory.FreeCost)
                {
                    objAccessory.Cost = "0";
                }
                objWeapon.WeaponAccessories.Add(objAccessory);

                frmPickWeaponAccessory.Dispose();
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddUnderbarrelWeapon_Click(object sender, EventArgs e)
        {
            // Attempt to locate the selected VehicleWeapon.
            Weapon objSelectedWeapon = CharacterObject.Vehicles.FindVehicleWeapon(treVehicles.SelectedNode?.Tag.ToString());
            if (objSelectedWeapon == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleWeaponUnderbarrel", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_VehicleWeaponUnderbarrel", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject)
            {
                LimitToCategories = "Underbarrel Weapons",
                Mounts = objSelectedWeapon.AccessoryMounts,
                Underbarrel = true
            };

            frmPickWeapon.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                return;

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

            if (frmPickWeapon.FreeCost)
            {
                objWeapon.Cost = "0";
            }

            objWeapon.Parent = objSelectedWeapon;
            objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);
            if (objSelectedWeapon.AllowAccessory == false)
                objWeapon.AllowAccessory = false;

            foreach (Weapon objLoopWeapon in lstWeapons)
            {
                objSelectedWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                if (objSelectedWeapon.AllowAccessory == false)
                    objLoopWeapon.AllowAccessory = false;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
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
            // Select the Martial Arts node if we're currently on a child.
            while (treMartialArts.SelectedNode != null && treMartialArts.SelectedNode.Level > 1)
                treMartialArts.SelectedNode = treMartialArts.SelectedNode.Parent;

            if (treMartialArts.SelectedNode == null || treMartialArts.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectMartialArtTechnique", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectMartialArtTechnique", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MartialArt objMartialArt = CharacterObject.MartialArts.FindById(treMartialArts.SelectedNode.Tag.ToString());

            if (objMartialArt == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectMartialArtTechnique", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectMartialArtTechnique", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectMartialArtTechnique frmPickMartialArtTechnique = new frmSelectMartialArtTechnique(CharacterObject, objMartialArt);
            frmPickMartialArtTechnique.ShowDialog(this);

            if (frmPickMartialArtTechnique.DialogResult == DialogResult.Cancel)
                return;

            // Open the Martial Arts XML file and locate the selected piece.
            XmlNode xmlTechnique = XmlManager.Load("martialarts.xml").SelectSingleNode("/chummer/techniques/technique[id = \"" + frmPickMartialArtTechnique.SelectedTechnique + "\"]");

            // Create the Improvements for the Advantage if there are any.
            MartialArtTechnique objAdvantage = new MartialArtTechnique(CharacterObject);
            objAdvantage.Create(xmlTechnique);
            if (objAdvantage.InternalId.IsEmptyGuid())
                return;

            objMartialArt.Techniques.Add(objAdvantage);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsVehicleAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Locate the selected Vehicle.
            Vehicle objSelectedVehicle = objSelectedNode?.Level > 0 ? CharacterObject.Vehicles.FindById(objSelectedNode.Tag.ToString()) : null;
            if (objSelectedVehicle == null)
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

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }

                objGear.Quantity = frmPickGear.SelectedQty;
                nudVehicleRating.Increment = objGear.CostFor;
                nudVehicleRating.Minimum = nudVehicleRating.Increment;

                // Change the cost of the Sensor itself to 0.
                //if (frmPickGear.SelectedCategory == "Sensors")
                //{
                //    objGear.Cost = "0";
                //    objGear.DictionaryCostN = new Tuple<int, Dictionary<int, string>>(-1, new Dictionary<int, string>());
                //}

                frmPickGear.Dispose();

                bool blnMatchFound = false;
                // If this is Ammunition, see if the character already has it on them.
                if (objGear.Category == "Ammunition")
                {
                    foreach (Gear objVehicleGear in objSelectedVehicle.Gear)
                    {
                        if (objVehicleGear.Name == objGear.Name &&
                            objVehicleGear.Category == objGear.Category &&
                            objVehicleGear.Rating == objGear.Rating &&
                            objVehicleGear.Extra == objGear.Extra)
                        {
                            // A match was found, so increase the quantity instead.
                            objVehicleGear.Quantity += objGear.Quantity;
                            blnMatchFound = true;

                            TreeNode objGearNode = objSelectedNode.FindNode(objVehicleGear.InternalId);
                            if (objGearNode != null)
                                objGearNode.Text = objVehicleGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

                            break;
                        }
                    }
                }

                if (!blnMatchFound)
                {
                    // Add the Gear to the Vehicle.
                    objSelectedVehicle.Gear.Add(objGear);
                }

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objSelectedVehicle.Weapons.Add(objWeapon);
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
            if (objSelectedNode == null || objSelectedNode.Level < 2)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ModifyVehicleGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            Gear objSensor = CharacterObject.Vehicles.FindVehicleGear(objSelectedNode.Tag.ToString(), out Vehicle objVehicle, out WeaponAccessory _, out Cyberware _);

            // Make sure the Gear was found.
            if (objSensor == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ModifyVehicleGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
                frmPickGear.Dispose();

                _blnSkipRefresh = true;
                nudVehicleGearQty.Increment = objGear.CostFor;
                //nudVehicleGearQty.Minimum = objGear.CostFor;
                _blnSkipRefresh = false;

                objSensor.Children.Add(objGear);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objVehicle.Weapons.Add(objWeapon);
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

        private void tsVehicleGearNotes_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode == null)
                return;
            Gear objGear = CharacterObject.Vehicles.FindVehicleGear(treVehicles.SelectedNode.Tag.ToString());
            if (objGear != null)
            {
                string strOldValue = objGear.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objGear.Notes = frmItemNotes.Notes;
                    if (objGear.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treVehicles.SelectedNode.ForeColor = objGear.PreferredColor;
                        treVehicles.SelectedNode.ToolTipText = objGear.Notes.WordWrap(100);
                    }
                }
            }
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

                Lifestyle objLifestyle = frmPickLifestyle.SelectedLifestyle;
                frmPickLifestyle.Dispose();
                objLifestyle.StyleType = LifestyleType.Advanced;

                CharacterObject.Lifestyles.Add(objLifestyle);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsWeaponName_Click(object sender, EventArgs e)
        {
            while (treWeapons.SelectedNode != null && treWeapons.SelectedNode.Level > 1)
                treWeapons.SelectedNode = treWeapons.SelectedNode.Parent;

            // Make sure a parent item is selected, then open the Select Accessory window.
            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Weapon.
            Weapon objWeapon = CharacterObject.Weapons.DeepFindById(treWeapons.SelectedNode.Tag.ToString());
            if (objWeapon == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_WeaponName", GlobalOptions.Language),
                DefaultString = objWeapon.WeaponName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objWeapon.WeaponName = frmPickText.SelectedValue;
            treWeapons.SelectedNode.Text = objWeapon.DisplayName(GlobalOptions.Language);

            IsDirty = true;
        }

        private void tsGearName_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode == null || treGear.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Gear.
            Gear objGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
            if (objGear == null)
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            // Locate the Weapon that is selected in the tree.
            Weapon objSelectedWeapon = objSelectedNode?.Level > 0 ? CharacterObject.Weapons.DeepFindById(objSelectedNode.Tag.ToString()) : null;
            if (objSelectedWeapon == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponUnderbarrel", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (objSelectedWeapon.Cyberware)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberwareUnderbarrel", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_WeaponUnderbarrel", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectWeapon frmPickWeapon = new frmSelectWeapon(CharacterObject)
            {
                LimitToCategories = "Underbarrel Weapons",
                Mounts = objSelectedWeapon.AccessoryMounts,
                Underbarrel = true
            };
            frmPickWeapon.ShowDialog(this);

            // Make sure the dialogue window was not canceled.
            if (frmPickWeapon.DialogResult == DialogResult.Cancel)
                return;

            // Open the Weapons XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            XmlNode objXmlWeapon = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + frmPickWeapon.SelectedWeapon + "\"]");

            List<Weapon> lstWeapons = new List<Weapon>();
            Weapon objWeapon = new Weapon(CharacterObject);
            objWeapon.Create(objXmlWeapon, lstWeapons);
            objWeapon.DiscountCost = frmPickWeapon.BlackMarketDiscount;
            objWeapon.Parent = objSelectedWeapon;
            objWeapon.AllowAccessory = objSelectedWeapon.AllowAccessory;
            if (objSelectedWeapon.AllowAccessory == false)
                objWeapon.AllowAccessory = false;

            if (frmPickWeapon.FreeCost)
            {
                objWeapon.Cost = "0";
            }

            objSelectedWeapon.UnderbarrelWeapons.Add(objWeapon);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsGearButtonAddAccessory_Click(object sender, EventArgs e)
        {
            tsGearAddAsPlugin_Click(sender, e);
        }

        private void tsGearRename_Click(object sender, EventArgs e)
        {
            frmSelectText frmPickText = new frmSelectText();
            //frmPickText.Description = LanguageManager.GetString("String_AddLocation");
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            Gear objGear = CharacterObject.Gear.FindById(treGear.SelectedNode.Tag.ToString());
            objGear.Extra = frmPickText.SelectedValue;
            treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            IsDirty = true;
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

            CharacterObject.Gear.Add(objGear);

            IsCharacterUpdateRequested = true;
        }

        private void tsVehicleAddNexus_Click(object sender, EventArgs e)
        {
            while (treVehicles.SelectedNode != null && treVehicles.SelectedNode.Level > 1)
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;

            // Make sure a parent items is selected, then open the Select Gear window.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGearVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Attempt to locate the selected Vehicle.
            Vehicle objSelectedVehicle = CharacterObject.Vehicles.FindById(treVehicles.SelectedNode.Tag.ToString());
            if (objSelectedVehicle == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearVehicle", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGearVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectNexus frmPickNexus = new frmSelectNexus(CharacterObject);
            frmPickNexus.ShowDialog(this);

            if (frmPickNexus.DialogResult == DialogResult.Cancel)
                return;

            Gear objGear = frmPickNexus.SelectedNexus;

            treVehicles.SelectedNode.Nodes.Add(objGear.CreateTreeNode(cmsVehicleGear));
            treVehicles.SelectedNode.Expand();

            objSelectedVehicle.Gear.Add(objGear);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }
        #endif

        private void tsArmorLocationAddArmor_Click(object sender, EventArgs e)
        {
            string strSelectedLocation = treArmor.SelectedNode?.Tag.ToString() ?? string.Empty;
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddArmor(strSelectedLocation);
            }
            while (blnAddAgain);
        }

        private void tsAddArmorGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level == 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSelectedId = objSelectedNode.Tag.ToString();
            bool blnAddAgain;
            do
            {
                // Select the root Gear node then open the Select Gear window.
                blnAddAgain = PickArmorGear(strSelectedId, true);
            }
            while (blnAddAgain);
        }

        private void tsArmorGearAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treArmor.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string strSelectedId = objSelectedNode.Tag.ToString();
            // Make sure the selected item is another piece of Gear.
            ArmorMod objMod = null;
            Gear objGear = CharacterObject.Armor.FindArmorGear(strSelectedId);
            if (objGear == null)
            {
                objMod = CharacterObject.Armor.FindArmorMod(strSelectedId);
                if (objMod == null || string.IsNullOrEmpty(objMod.GearCapacity))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmorGear(strSelectedId, objMod != null);
            }
            while (blnAddAgain);
        }

        private void tsArmorNotes_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode == null)
                return;
            Armor objArmor = CharacterObject.Armor.FindById(treArmor.SelectedNode.Tag.ToString());
            if (objArmor != null)
            {
                string strOldValue = objArmor.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objArmor.Notes = frmItemNotes.Notes;
                    if (objArmor.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treArmor.SelectedNode.ForeColor = objArmor.PreferredColor;
                        treArmor.SelectedNode.ToolTipText = objArmor.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsArmorModNotes_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode == null)
                return;
            ArmorMod objArmorMod = CharacterObject.Armor.FindArmorMod(treArmor.SelectedNode.Tag.ToString());
            if (objArmorMod != null)
            {
                string strOldValue = objArmorMod.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objArmorMod.Notes = frmItemNotes.Notes;
                    if (objArmorMod.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treArmor.SelectedNode.ForeColor = objArmorMod.PreferredColor;
                        treArmor.SelectedNode.ToolTipText = objArmorMod.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tssLimitModifierNotes_Click(object sender, EventArgs e)
        {
            if (treLimit.SelectedNode == null)
                return;
            LimitModifier objLimitModifier = CharacterObject.LimitModifiers.FindById(treLimit.SelectedNode.Tag.ToString());
            if (objLimitModifier != null)
            {
                string strOldValue = objLimitModifier.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objLimitModifier.Notes = frmItemNotes.Notes;
                    if (objLimitModifier.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treLimit.SelectedNode.ForeColor = objLimitModifier.PreferredColor;
                        treLimit.SelectedNode.ToolTipText = objLimitModifier.Notes.WordWrap(100);
                    }
                }
            }
            else
            {
                // the limit modifier has a source
                string strSelectedSource = treLimit.SelectedNode.Tag.ToString();
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.SourceName == strSelectedSource)
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

        private void tsArmorGearNotes_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode == null)
                return;

            Gear objArmorGear = CharacterObject.Armor.FindArmorGear(treArmor.SelectedNode.Tag.ToString());
            if (objArmorGear != null)
            {
                string strOldValue = objArmorGear.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objArmorGear.Notes = frmItemNotes.Notes;
                    if (objArmorGear.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treArmor.SelectedNode.ForeColor = objArmorGear.PreferredColor;
                        treArmor.SelectedNode.ToolTipText = objArmorGear.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsWeaponNotes_Click(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode == null)
                return;

            Weapon objWeapon = CharacterObject.Weapons.DeepFindById(treWeapons.SelectedNode.Tag.ToString());
            if (objWeapon != null)
            {
                string strOldValue = objWeapon.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objWeapon.Notes = frmItemNotes.Notes;
                    if (objWeapon.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treWeapons.SelectedNode.ForeColor = objWeapon.PreferredColor;
                        treWeapons.SelectedNode.ToolTipText = objWeapon.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsWeaponAccessoryNotes_Click(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode == null)
                return;
            WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(treWeapons.SelectedNode.Tag.ToString());

            if (objAccessory != null)
            {
                string strOldValue = objAccessory.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objAccessory.Notes = frmItemNotes.Notes;
                    if (objAccessory.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treWeapons.SelectedNode.ForeColor = objAccessory.PreferredColor;
                        treWeapons.SelectedNode.ToolTipText = objAccessory.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsCyberwareNotes_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode == null)
                return;
            Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode.Tag.ToString());
            if (objCyberware != null)
            {
                string strOldValue = objCyberware.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objCyberware.Notes = frmItemNotes.Notes;
                    if (objCyberware.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treCyberware.SelectedNode.ForeColor = objCyberware.PreferredColor;
                        treCyberware.SelectedNode.ToolTipText = objCyberware.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsVehicleCyberwareNotes_Click(object sender, EventArgs e)
        {
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;

            Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);
            if (objCyberware != null)
            {
                string strOldValue = objCyberware.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objCyberware.Notes = frmItemNotes.Notes;
                    if (objCyberware.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treVehicles.SelectedNode.ForeColor = objCyberware.PreferredColor;
                        treVehicles.SelectedNode.ToolTipText = objCyberware.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsQualityNotes_Click(object sender, EventArgs e)
        {
            TreeNode objNode = treQualities.SelectedNode;
            if (objNode != null)
            {
                if (objNode.Tag is Quality objQuality)
                {
                    string strOldValue = objQuality.Notes;
                    frmNotes frmItemNotes = new frmNotes
                    {
                        Notes = strOldValue
                    };
                    frmItemNotes.ShowDialog(this);

                    if (frmItemNotes.DialogResult == DialogResult.OK)
                    {
                        objQuality.Notes = frmItemNotes.Notes;
                        if (objQuality.Notes != strOldValue)
                        {
                            IsDirty = true;

                            objNode.ForeColor = objQuality.PreferredColor;
                            objNode.ToolTipText = objQuality.Notes.WordWrap(100);
                        }
                    }
                }
            }
        }

        private void tsMartialArtsNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode == null)
                return;

            MartialArt objMartialArt = CharacterObject.MartialArts.FindById(treMartialArts.SelectedNode.Tag.ToString());
            if (objMartialArt != null)
            {
                string strOldValue = objMartialArt.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objMartialArt.Notes = frmItemNotes.Notes;
                    if (objMartialArt.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMartialArts.SelectedNode.ForeColor = objMartialArt.PreferredColor;
                        treMartialArts.SelectedNode.ToolTipText = objMartialArt.Notes.WordWrap(100);
                    }
                }
            }
        }

#if LEGACY
        private void tsMartialArtManeuverNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode == null)
                return;

            MartialArtManeuver objMartialArtManeuver = CharacterObject.MartialArtManeuvers.FindById(treMartialArts.SelectedNode.Tag.ToString());
            if (objMartialArtManeuver != null)
            {
                string strOldValue = objMartialArtManeuver.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objMartialArtManeuver.Notes = frmItemNotes.Notes;
                    if (objMartialArtManeuver.Notes != strOldValue)
                    {
                        IsDirty = true;

                        if (!string.IsNullOrEmpty(objMartialArtManeuver.Notes))
                            treMartialArts.SelectedNode.ForeColor = Color.SaddleBrown;
                        else
                            treMartialArts.SelectedNode.ForeColor = SystemColors.WindowText;
                        treMartialArts.SelectedNode.ToolTipText = objMartialArtManeuver.Notes.WordWrap(100);
                    }
                }
            }
        }
#endif

        private void tsSpellNotes_Click(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode == null)
                return;

            Spell objSpell = CharacterObject.Spells.FindById(treSpells.SelectedNode.Tag.ToString());
            if (objSpell != null)
            {
                string strOldValue = objSpell.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objSpell.Notes = frmItemNotes.Notes;
                    if (objSpell.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treSpells.SelectedNode.ForeColor = objSpell.PreferredColor;
                        treSpells.SelectedNode.ToolTipText = objSpell.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsComplexFormNotes_Click(object sender, EventArgs e)
        {
            if (treComplexForms.SelectedNode == null)
                return;

            ComplexForm objComplexForm = CharacterObject.ComplexForms.FindById(treComplexForms.SelectedNode.Tag.ToString());
            if (objComplexForm != null)
            {
                string strOldValue = objComplexForm.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objComplexForm.Notes = frmItemNotes.Notes;
                    if (objComplexForm.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treComplexForms.SelectedNode.ForeColor = objComplexForm.PreferredColor;
                        treComplexForms.SelectedNode.ToolTipText = objComplexForm.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsAIProgramNotes_Click(object sender, EventArgs e)
        {
            if (treAIPrograms.SelectedNode == null)
                return;

            AIProgram objAIProgram = CharacterObject.AIPrograms.FindById(treAIPrograms.SelectedNode.Tag.ToString());
            if (objAIProgram != null)
            {
                string strOldValue = objAIProgram.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objAIProgram.Notes = frmItemNotes.Notes;
                    if (objAIProgram.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treAIPrograms.SelectedNode.ForeColor = objAIProgram.PreferredColor;
                        treAIPrograms.SelectedNode.ToolTipText = objAIProgram.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsCritterPowersNotes_Click(object sender, EventArgs e)
        {
            if (treCritterPowers.SelectedNode == null)
                return;

            CritterPower objCritterPower = CharacterObject.CritterPowers.FindById(treCritterPowers.SelectedNode.Tag.ToString());
            if (objCritterPower != null)
            {
                string strOldValue = objCritterPower.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objCritterPower.Notes = frmItemNotes.Notes;
                    if (objCritterPower.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treCritterPowers.SelectedNode.ForeColor = objCritterPower.PreferredColor;
                        treCritterPowers.SelectedNode.ToolTipText = objCritterPower.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsMetamagicNotes_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode == null)
                return;

            Metamagic objMetamagic = CharacterObject.Metamagics.FindById(treMetamagic.SelectedNode.Tag.ToString());
            if (objMetamagic != null)
            {
                string strOldValue = objMetamagic.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objMetamagic.Notes = frmItemNotes.Notes;
                    if (objMetamagic.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMetamagic.SelectedNode.ForeColor = objMetamagic.PreferredColor;
                        treMetamagic.SelectedNode.ToolTipText = objMetamagic.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsGearNotes_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode == null)
                return;

            Gear objGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
            if (objGear != null)
            {
                string strOldValue = objGear.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objGear.Notes = frmItemNotes.Notes;
                    if (objGear.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treGear.SelectedNode.ForeColor = objGear.PreferredColor;
                        treGear.SelectedNode.ToolTipText = objGear.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsGearPluginNotes_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode == null)
                return;

            Gear objGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
            if (objGear != null)
            {
                string strOldValue = objGear.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objGear.Notes = frmItemNotes.Notes;
                    if (objGear.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treGear.SelectedNode.ForeColor = objGear.PreferredColor;
                        treGear.SelectedNode.ToolTipText = objGear.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            IHasNotes noteThing = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (noteThing == null)
            {
                noteThing = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId);
            }
            if (noteThing == null)
            {
                noteThing = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out Vehicle objVehicle);
            }

            if (noteThing == null) return;
            string strOldValue = noteThing.Notes;
            frmNotes frmItemNotes = new frmNotes
            {
                Notes = noteThing.Notes
            };
            frmItemNotes.ShowDialog(this);

            if (frmItemNotes.DialogResult == DialogResult.OK)
            {
                noteThing.Notes = frmItemNotes.Notes;
                if (noteThing.Notes != strOldValue)
                {
                    IsDirty = true;

                    treVehicles.SelectedNode.ForeColor = noteThing.PreferredColor;
                    treVehicles.SelectedNode.ToolTipText = noteThing.Notes.WordWrap(100);
                }
            }
        }

        private void tsLifestyleNotes_Click(object sender, EventArgs e)
        {
            if (treLifestyles.SelectedNode == null)
                return;

            Lifestyle objLifestyle = CharacterObject.Lifestyles.FindById(treLifestyles.SelectedNode.Tag.ToString());
            if (objLifestyle != null)
            {
                string strOldValue = objLifestyle.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objLifestyle.Notes = frmItemNotes.Notes;
                    if (objLifestyle.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treLifestyles.SelectedNode.ForeColor = objLifestyle.PreferredColor;
                        treLifestyles.SelectedNode.ToolTipText = objLifestyle.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsVehicleWeaponNotes_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode == null)
                return;
            Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(treVehicles.SelectedNode.Tag.ToString());
            if (objWeapon != null)
            {
                string strOldValue = objWeapon.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objWeapon.Notes = frmItemNotes.Notes;
                    if (objWeapon.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treVehicles.SelectedNode.ForeColor = objWeapon.PreferredColor;
                        treVehicles.SelectedNode.ToolTipText = objWeapon.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void tsWeaponMountLocation_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            if (objSelectedNode == null)
                return;
            WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(objSelectedNode.Tag.ToString(), out Vehicle _);
            if (objWeaponMount == null)
                return;
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_VehicleName", GlobalOptions.Language),
                DefaultString = objWeaponMount.Location
            };

            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objWeaponMount.Location = frmPickText.SelectedValue;
            objSelectedNode.Text = objWeaponMount.DisplayName(GlobalOptions.Language);
        }

        private void tsVehicleName_Click(object sender, EventArgs e)
        {
            while (treVehicles.SelectedNode != null && treVehicles.SelectedNode.Level > 1)
            {
                treVehicles.SelectedNode = treVehicles.SelectedNode.Parent;
            }

            // Make sure a parent item is selected.
            if (treVehicles.SelectedNode == null || treVehicles.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Vehicle.
            Vehicle objVehicle = CharacterObject.Vehicles.FindById(treVehicles.SelectedNode.Tag.ToString());
            if (objVehicle == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_VehicleName", GlobalOptions.Language),
                DefaultString = objVehicle.VehicleName
            };

            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objVehicle.VehicleName = frmPickText.SelectedValue;
            treVehicles.SelectedNode.Text = objVehicle.DisplayName(GlobalOptions.Language);
        }

        private void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleCyberwarePlugin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId, out objMod);

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
                    if (!objCyberwareParent.Capacity.Contains('[') || objCyberwareParent.Capacity.Contains("/["))
                    {
                        frmPickCyberware.Subsystems = objCyberwareParent.AllowedSubsystems;
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

                // Create the Cyberware object.
                Cyberware objCyberware = new Cyberware(CharacterObject);
                List<Weapon> lstWeapons = new List<Weapon>();
                List<Vehicle> lstVehicles = new List<Vehicle>();
                objCyberware.Create(objXmlCyberware, CharacterObject, frmPickCyberware.SelectedGrade, Improvement.ImprovementSource.Cyberware, frmPickCyberware.SelectedRating, lstWeapons, lstVehicles, false, true, string.Empty, null, objVehicle);
                if (objCyberware.InternalId.IsEmptyGuid())
                {
                    frmPickCyberware.Dispose();
                    continue;
                }

                if (frmPickCyberware.FreeCost)
                    objCyberware.Cost = "0";
                objCyberware.PrototypeTranshuman = frmPickCyberware.PrototypeTranshuman;
                objCyberware.DiscountCost = frmPickCyberware.BlackMarketDiscount;

                objMod.Cyberware.Add(objCyberware);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = objVehicle;
                    objMod.Weapons.Add(objWeapon);
                }

                // Create the Weapon Node if one exists.
                foreach (Vehicle objLoopVehicle in lstVehicles)
                {
                    CharacterObject.Vehicles.Add(objLoopVehicle);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;

                frmPickCyberware.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsArmorName_Click(object sender, EventArgs e)
        {
            while (treArmor.SelectedNode != null && treArmor.SelectedNode.Level > 1)
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;

            // Make sure a parent item is selected, then open the Select Accessory window.
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmorName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Armor.
            Armor objArmor = CharacterObject.Armor.FindById(treArmor.SelectedNode.Tag.ToString());
            if (objArmor == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmorName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_ArmorName", GlobalOptions.Language),
                DefaultString = objArmor.ArmorName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objArmor.ArmorName = frmPickText.SelectedValue;
            treArmor.SelectedNode.Text = objArmor.DisplayName(GlobalOptions.Language);
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
            string strSelectedId = treLifestyles.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedId) || treLifestyles.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectLifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectLifestyle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Lifestyle.
            Lifestyle objLifestyle = CharacterObject.Lifestyles.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (objLifestyle == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectLifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectLifestyle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_LifestyleName", GlobalOptions.Language),
                DefaultString = objLifestyle.Name
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            if (objLifestyle.Name != frmPickText.SelectedValue)
            {
                objLifestyle.Name = frmPickText.SelectedValue;

                treLifestyles.SelectedNode.Text = objLifestyle.DisplayName(GlobalOptions.Language);

                treLifestyles.SortCustom(strSelectedId);

                IsDirty = true;
            }
        }

        private void tsGearRenameLocation_Click(object sender, EventArgs e)
        {
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            string strNewLocation = frmPickText.SelectedValue;

            string strSelectedText = treGear.SelectedNode.Text;
            for (int i = 0; i < CharacterObject.GearLocations.Count; ++i)
            {
                string strLocation = CharacterObject.GearLocations[i];
                if (strLocation == strSelectedText)
                {
                    foreach (Gear objGear in CharacterObject.Gear)
                    {
                        if (objGear.Location == strLocation)
                            objGear.Location = strNewLocation;
                    }

                    CharacterObject.GearLocations[i] = strNewLocation;
                    break;
                }
            }

            IsDirty = true;
        }

        private void tsWeaponRenameLocation_Click(object sender, EventArgs e)
        {
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            string strNewLocation = frmPickText.SelectedValue;

            string strSelectedText = treWeapons.SelectedNode.Text;
            for (int i = 0; i < CharacterObject.WeaponLocations.Count; ++i)
            {
                string strLocation = CharacterObject.WeaponLocations[i];
                if (strLocation == strSelectedText)
                {
                    foreach (Weapon objWeapon in CharacterObject.Weapons)
                    {
                        if (objWeapon.Location == strLocation)
                            objWeapon.Location = strNewLocation;
                    }

                    CharacterObject.WeaponLocations[i] = strNewLocation;
                    break;
                }
            }

            IsDirty = true;
        }

        private void tsCreateSpell_Click(object sender, EventArgs e)
        {
            // Run through the list of Active Skills and pick out the two applicable ones.
            int intSkillValue = Math.Max(CharacterObject.SkillsSection.GetActiveSkill("Spellcasting")?.Rating ?? 0, CharacterObject.SkillsSection.GetActiveSkill("Ritual Spellcasting")?.Rating ?? 0);

            // The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
            if (CharacterObject.Spells.Count >= (2 * intSkillValue + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SpellLimit)) && !CharacterObject.IgnoreRules)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SpellLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SpellLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The character is still allowed to add Spells, so show the Create Spell window.
            frmCreateSpell frmSpell = new frmCreateSpell(CharacterObject);
            frmSpell.ShowDialog(this);

            if (frmSpell.DialogResult == DialogResult.Cancel)
                return;

            Spell objSpell = frmSpell.SelectedSpell;
            CharacterObject.Spells.Add(objSpell);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsArmorRenameLocation_Click(object sender, EventArgs e)
        {
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            string strNewLocation = frmPickText.SelectedValue;

            string strSelectedText = treArmor.SelectedNode.Text;
            for (int i = 0; i < CharacterObject.ArmorLocations.Count; ++i)
            {
                string strLocation = CharacterObject.ArmorLocations[i];
                if (strLocation == strSelectedText)
                {
                    foreach (Armor objArmor in CharacterObject.Armor)
                    {
                        if (objArmor.Location == strLocation)
                            objArmor.Location = strNewLocation;
                    }

                    CharacterObject.ArmorLocations[i] = strNewLocation;
                    break;
                }
            }

            IsDirty = true;
        }

        private void tsCyberwareAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treCyberware.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null || objSelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberware = CharacterObject.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objSelectedNode.Tag.ToString());

            // Make sure the Cyberware is allowed to accept Gear.
            if (objCyberware?.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_CyberwareGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
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

                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objNewGear = new Gear(CharacterObject);
                objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);
                objNewGear.Quantity = frmPickGear.SelectedQty;

                objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                if (objNewGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objNewGear.Cost = "(" + objNewGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objNewGear.Cost = "0";
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                objCyberware.Gear.Add(objNewGear);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleCyberwareAddGear_Click(object sender, EventArgs e)
        {
            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            Cyberware objCyberware = !string.IsNullOrEmpty(strSelectedId) ? CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId) : null;

            // Make sure a parent items is selected, then open the Select Gear window.
            if (objCyberware == null)
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

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
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

                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objNewGear = new Gear(CharacterObject);
                objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);
                objNewGear.Quantity = frmPickGear.SelectedQty;

                objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                if (objNewGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objNewGear.Cost = "(" + objNewGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objNewGear.Cost = "0";
                }
                frmPickGear.Dispose();

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                objCyberware.Gear.Add(objNewGear);

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
            Gear objSensor = CharacterObject.Cyberware.FindCyberwareGear(objSelectedNode.Tag.ToString());
            if (objSensor == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;
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

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
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
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Make sure a parent items is selected, then open the Select Gear window.
            if (objSelectedNode == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Locate the Vehicle Sensor Gear.
            Gear objSensor = CharacterObject.Cyberware.FindCyberwareGear(objSelectedNode.Tag.ToString());
            if (objSensor == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;
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

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(objSelectedNode.Tag.ToString());

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory?.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_WeaponGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;
            string strCategories = string.Empty;
            foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                strCategories += objXmlCategory.InnerText + ",";

            do
            {
                Cursor = Cursors.WaitCursor;
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
                objGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons);

                if (objGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objGear.Quantity = frmPickGear.SelectedQty;

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }
                frmPickGear.Dispose();

                objAccessory.Gear.Add(objGear);

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    CharacterObject.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            // Locate the Vehicle Sensor Gear.
            Gear objSensor = CharacterObject.Weapons.FindWeaponGear(objSelectedNode.Tag.ToString());
            if (objSensor == null)
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
            // Open the Gear XML file and locate the selected piece.
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

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                // Create any Weapons that came with this Gear.
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
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            // Determine if this is a Location.
            TreeNode objVehicleNode = treVehicles.SelectedNode;
            string strOldLocation = objVehicleNode.Tag.ToString();
            do
            {
                objVehicleNode = objVehicleNode.Parent;
            } while (objVehicleNode.Level > 1);

            // Get a reference to the affected Vehicle.
            string strSelectedId = objVehicleNode.Tag.ToString();
            Vehicle objVehicle = CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == strSelectedId);

            string strNewLocation = frmPickText.SelectedValue;

            if (objVehicle != null)
            {
                for (int i = 0; i < objVehicle.Locations.Count; ++i)
                {
                    string strLocation = objVehicle.Locations[i];
                    if (strLocation == strOldLocation)
                    {
                        foreach (Gear objGear in objVehicle.Gear)
                        {
                            if (objGear.Location == strLocation)
                                objGear.Location = strNewLocation;
                        }

                        objVehicle.Locations[i] = strNewLocation;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < CharacterObject.VehicleLocations.Count; ++i)
                {
                    string strLocation = CharacterObject.VehicleLocations[i];
                    if (strLocation == strOldLocation)
                    {
                        foreach (Vehicle objLoopVehicle in CharacterObject.Vehicles)
                        {
                            if (objLoopVehicle.Location == strLocation)
                                objLoopVehicle.Location = strNewLocation;
                        }

                        CharacterObject.VehicleLocations[i] = strNewLocation;
                        break;
                    }
                }
            }

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

        private void tsVehicleWeaponAccessoryNotes_Click(object sender, EventArgs e)
        {
            WeaponAccessory objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(treVehicles.SelectedNode.Tag.ToString());

            string strOldValue = objAccessory.Notes;
            frmNotes frmItemNotes = new frmNotes
            {
                Notes = strOldValue
            };
            frmItemNotes.ShowDialog(this);

            if (frmItemNotes.DialogResult == DialogResult.OK)
            {
                objAccessory.Notes = frmItemNotes.Notes;
                if (objAccessory.Notes != strOldValue)
                {
                    IsDirty = true;

                    treVehicles.SelectedNode.ForeColor = objAccessory.PreferredColor;
                    treVehicles.SelectedNode.ToolTipText = objAccessory.Notes.WordWrap(100);
                }
            }
        }

        private void tsVehicleWeaponAccessoryGearMenuAddAsPlugin_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            // Locate the Vehicle Sensor Gear.
            Gear objSensor = CharacterObject.Vehicles.FindVehicleGear(objSelectedNode.Tag.ToString(), out Vehicle objVehicle, out WeaponAccessory _, out Cyberware _);
            if (objSensor == null)
            // Make sure the Gear was found.
            {
                MessageBox.Show(LanguageManager.GetString("Message_ModifyVehicleGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

                objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objGear.Cost = "(" + objGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objGear.Cost = "0";
                }
                frmPickGear.Dispose();

                objSensor.Children.Add(objGear);

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                {
                    objVehicle.Weapons.Add(objWeapon);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
            while (blnAddAgain);
        }

        private void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            WeaponAccessory objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(objSelectedNode.Tag.ToString());

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (objAccessory?.AllowGear == null)
            {
                MessageBox.Show(LanguageManager.GetString("Message_WeaponGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            string strCategories = string.Empty;
            foreach (XmlNode objXmlCategory in objAccessory.AllowGear)
                strCategories += objXmlCategory.InnerText + ",";
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;
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
                XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>();

                Gear objNewGear = new Gear(CharacterObject);
                objNewGear.Create(objXmlGear, frmPickGear.SelectedRating, lstWeapons, string.Empty, false);

                if (objNewGear.InternalId.IsEmptyGuid())
                {
                    frmPickGear.Dispose();
                    continue;
                }

                objNewGear.Quantity = frmPickGear.SelectedQty;

                objNewGear.DiscountCost = frmPickGear.BlackMarketDiscount;

                // Reduce the cost for Do It Yourself components.
                if (frmPickGear.DoItYourself)
                    objNewGear.Cost = "(" + objNewGear.Cost + ") * 0.5";
                // If the item was marked as free, change its cost.
                if (frmPickGear.FreeCost)
                {
                    objNewGear.Cost = "0";
                }
                frmPickGear.Dispose();

                objAccessory.Gear.Add(objNewGear);

                // Create any Weapons that came with this Gear.
                foreach (Weapon objLoopWeapon in lstWeapons)
                {
                    objAccessory.Parent.Children.Add(objLoopWeapon);
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

        private void UpdateQualityLevelValue(Quality objSelectedQuality = null)
        {
            if (objSelectedQuality == null || objSelectedQuality.OriginSource == QualitySource.Improvement || objSelectedQuality.OriginSource == QualitySource.Metatype)
            {
                nudQualityLevel.Value = 1;
                nudQualityLevel.Enabled = false;
                return;
            }
            XmlNode objQualityNode = objSelectedQuality.GetNode();
            string strLimitString = objQualityNode != null ? (objQualityNode["chargenlimit"]?.InnerText ?? objQualityNode["limit"]?.InnerText) : string.Empty;
            if (!string.IsNullOrWhiteSpace(strLimitString) && objQualityNode?["nolevels"] == null && int.TryParse(strLimitString, out int intMaxRating))
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

                bool blnRequireUpdate = false;
                // Adding new levels
                for (; nudQualityLevel.Value > intCurrentLevels; ++intCurrentLevels)
                {
                    XmlNode objXmlSelectedQuality = objSelectedQuality.GetNode();
                    if (!objXmlSelectedQuality.RequirementsMet(CharacterObject, LanguageManager.GetString("String_Quality", GlobalOptions.Language)))
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

                    // If the item being checked would cause the limit of 25 BP spent on Positive Qualities to be exceed, do not let it be checked and display a message.
                    string strAmount = CharacterObject.GameplayOptionQualityLimit.ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
                    int intMaxQualityAmount = CharacterObject.GameplayOptionQualityLimit;

                    // Make sure that adding the Quality would not cause the character to exceed their BP limits.
                    int intBP = 0;
                    bool blnAddItem = true;

                    // Add the cost of the Quality that is being added.
                    if (objQuality.ContributeToLimit)
                        intBP += objQuality.BP;

                    if (objQuality.Type == QualityType.Negative)
                    {
                        // Calculate the cost of the current Negative Qualities.
                        foreach (Quality objCharacterQuality in CharacterObject.Qualities)
                        {
                            if (objCharacterQuality.Type == QualityType.Negative && objCharacterQuality.ContributeToLimit)
                                intBP += objCharacterQuality.BP;
                        }

                        // Include the BP used by Enemies.
                        if (CharacterObjectOptions.EnemyKarmaQualityLimit)
                        {
                            // Include the BP used by Enemies.
                            string strEnemiesBPText = lblEnemiesBP.Text.FastEscapeOnceFromEnd(LanguageManager.GetString("String_Karma", GlobalOptions.Language)).NormalizeWhiteSpace();
                            if (int.TryParse(strEnemiesBPText, out int intTemp))
                                intBP += intTemp;
                        }

                        // Include the amount from Free Negative Quality BP cost Improvements.
                        intBP -= (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeNegativeQualities) * CharacterObjectOptions.KarmaQuality);

                        // Check if adding this Quality would put the character over their limit.
                        if (!CharacterObjectOptions.ExceedNegativeQualities)
                        {
                            if (intBP < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                            {
                                MessageBox.Show(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language).Replace("{0}", strAmount), LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
                            else if (CharacterObject.MetatypeBP < 0)
                            {
                                if ((intBP + CharacterObject.MetatypeBP) < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                                {
                                    MessageBox.Show(LanguageManager.GetString("Message_NegativeQualityAndMetatypeLimit", GlobalOptions.Language).Replace("{0}", strAmount), LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    blnAddItem = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (objQuality.ContributeToLimit || objQuality.ContributeToBP)
                        {
                            // Calculate the cost of the current Positive Qualities.
                            foreach (Quality objCharacterQuality in CharacterObject.Qualities)
                            {
                                if (objCharacterQuality.Type == QualityType.Positive && objCharacterQuality.ContributeToLimit)
                                    intBP += objCharacterQuality.BP;
                            }
                            if (CharacterObject.BuildMethod == CharacterBuildMethod.Karma)
                                intBP *= CharacterObjectOptions.KarmaQuality;

                            // Include the amount from Free Negative Quality BP cost Improvements.
                            intBP -= (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreePositiveQualities) *
                                      CharacterObjectOptions.KarmaQuality);

                            // Check if adding this Quality would put the character over their limit.
                            if (!CharacterObjectOptions.ExceedPositiveQualities)
                            {
                                if (intBP > intMaxQualityAmount && !CharacterObject.IgnoreRules)
                                {
                                    MessageBox.Show(
                                        LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language)
                                            .Replace("{0}", strAmount),
                                        LanguageManager.GetString("MessageTitle_PositiveQualityLimit", GlobalOptions.Language),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    blnAddItem = false;
                                }
                            }
                        }
                    }

                    if (blnAddItem)
                    {
                        blnRequireUpdate = true;
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
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }
                }
                // Removing levels
                for (; nudQualityLevel.Value < intCurrentLevels; --intCurrentLevels)
                {
                    Quality objInvisibleQuality = CharacterObject.Qualities.FirstOrDefault(x => x.QualityId == objSelectedQuality.QualityId && x.Extra == objSelectedQuality.Extra && x.SourceName == objSelectedQuality.SourceName && x.InternalId != objSelectedQuality.InternalId);
                    if (objInvisibleQuality != null && RemoveQuality(objInvisibleQuality, false, false))
                    {
                        blnRequireUpdate = true;
                    }
                    else if (RemoveQuality(objSelectedQuality, false, false))
                    {
                        blnRequireUpdate = true;
                        break;
                    }
                    else
                    {
                        UpdateQualityLevelValue(objSelectedQuality);
                        break;
                    }
                }

                if (blnRequireUpdate)
                {
                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }
#endregion

#region Additional Cyberware Tab Control Events
        private void treCyberware_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedCyberware();
        }

        private void cboCyberwareGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_blnSkipRefresh && !_blnLoading)
            {
                string strSelectedGrade = cboCyberwareGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelectedGrade))
                {
                    // Locate the selected piece of Cyberware.
                    Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode?.Tag.ToString());
                    if (objCyberware != null)
                    {
                        Grade objNewGrade = CharacterObject.GetGradeList(objCyberware.SourceType).FirstOrDefault(x => x.Name == strSelectedGrade);
                        if (objNewGrade != null)
                        {
                            // Updated the selected Cyberware Grade.
                            objCyberware.Grade = objNewGrade;

                            IsCharacterUpdateRequested = true;

                            IsDirty = true;
                        }
                    }
                }
            }
        }

        private void chkPrototypeTranshuman_CheckedChanged(object sender, EventArgs e)
        {
            if (!_blnSkipRefresh)
            {
                // Locate the selected piece of Cyberware.
                Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode.Tag.ToString());
                if (objCyberware != null)
                {
                    // Update the selected Cyberware Rating.
                    objCyberware.PrototypeTranshuman = chkPrototypeTranshuman.Checked;

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void nudCyberwareRating_ValueChanged(object sender, EventArgs e)
        {
            if (!_blnSkipRefresh)
            {
                // Locate the selected piece of Cyberware.
                Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode.Tag.ToString());
                if (objCyberware != null)
                {
                    // Update the selected Cyberware Rating.
                    objCyberware.Rating = decimal.ToInt32(nudCyberwareRating.Value);

                    // See if a Bonus node exists.
                    if ((objCyberware.Bonus != null && objCyberware.Bonus.InnerXml.Contains("Rating")) || (objCyberware.PairBonus != null && objCyberware.PairBonus.InnerXml.Contains("Rating")) || (objCyberware.WirelessOn && objCyberware.WirelessBonus != null && objCyberware.WirelessBonus.InnerXml.Contains("Rating")))
                    {
                        // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                        ImprovementManager.RemoveImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId);
                        if (objCyberware.Bonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.Bonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                        if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, objCyberware.SourceType, objCyberware.InternalId, objCyberware.WirelessBonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));

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
                                        intNotMatchLocationCount += 1;
                                    else
                                        intMatchLocationCount += 1;
                                }
                                // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                                intCyberwaresCount = Math.Min(intNotMatchLocationCount, intMatchLocationCount) * 2;
                            }
                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                ImprovementManager.RemoveImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair");
                                // Go down the list and create pair bonuses for every second item
                                if (intCyberwaresCount > 0 && intCyberwaresCount % 2 == 0)
                                {
                                    ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                                }
                                intCyberwaresCount -= 1;
                            }
                        }

                        if (!objCyberware.IsModularCurrentlyEquipped)
                            objCyberware.ChangeModularEquip(false);
                    }

                    treCyberware.SelectedNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
                }
                else
                {
                    // Find the selected piece of Gear.
                    Gear objGear = CharacterObject.Cyberware.FindCyberwareGear(treCyberware.SelectedNode.Tag.ToString());

                    if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                    {
                        if (!RefreshSingleFocusRating(treFoci, objGear, decimal.ToInt32(nudCyberwareRating.Value)))
                        {
                            _blnSkipRefresh = true;
                            nudCyberwareRating.Value = objGear.Rating;
                            _blnSkipRefresh = false;
                            return;
                        }
                    }
                    else
                        objGear.Rating = decimal.ToInt32(nudCyberwareRating.Value);

                    // See if a Bonus node exists.
                    if (objGear.Bonus != null || (objGear.WirelessOn && objGear.WirelessBonus != null))
                    {
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                        if (!string.IsNullOrEmpty(objGear.Extra))
                        {
                            ImprovementManager.ForcedValue = objGear.Extra.TrimEndOnce(", Hacked");
                        }
                        if (objGear.Bonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                        if (objGear.WirelessOn && objGear.WirelessBonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));

                        if (!objGear.Equipped)
                            objGear.ChangeEquippedStatus(false);
                    }

                    treCyberware.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }
#endregion

#region Additional Street Gear Tab Control Events
        private void treWeapons_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedWeapon();
            RefreshPasteStatus(sender, e);
        }

        private void treWeapons_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strSelectedWeapon = treWeapons.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedWeapon) || treWeapons.SelectedNode.Level > 1 || treWeapons.SelectedNode.Level < 0)
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
            RefreshPasteStatus(sender, e);
        }

        private void treLifestyles_DoubleClick(object sender, EventArgs e)
        {
            string strSelectedLifestyle = treLifestyles.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedLifestyle) || treLifestyles.SelectedNode.Level == 0)
                return;

            // Locate the selected Lifestyle.
            Lifestyle objLifestyle = null;
            string strGuid = string.Empty;
            int intMonths = 0;
            int intPosition = 0;
            for (; intPosition < CharacterObject.Lifestyles.Count; ++intPosition)
            {
                objLifestyle = CharacterObject.Lifestyles[intPosition];
                if (objLifestyle.InternalId == strSelectedLifestyle)
                {
                    strGuid = objLifestyle.InternalId;
                    intMonths = objLifestyle.Increments;
                    break;
                }
            }

            if (objLifestyle == null || string.IsNullOrEmpty(strGuid))
                return;

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

            objLifestyle.SetInternalId(strGuid);
            objLifestyle.Increments = intMonths;
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

        private void nudLifestyleMonths_ValueChanged(object sender, EventArgs e)
        {
            if (treLifestyles.SelectedNode?.Level > 0)
            {
                _blnSkipRefresh = true;

                // Locate the selected Lifestyle.
                Lifestyle objLifestyle = CharacterObject.Lifestyles.FindById(treLifestyles.SelectedNode.Tag.ToString());
                if (objLifestyle == null)
                    return;

                objLifestyle.Increments = decimal.ToInt32(nudLifestyleMonths.Value);

                _blnSkipRefresh = false;

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void treGear_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedGear();
            RefreshPasteStatus(sender, e);
        }

        private void nudGearRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (treGear.SelectedNode.Level > 0)
            {
                Gear objGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
                if (objGear == null)
                    return;

                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                {
                    if (!RefreshSingleFocusRating(treFoci, objGear, decimal.ToInt32(nudGearRating.Value)))
                    {
                        _blnSkipRefresh = true;
                        nudGearRating.Value = objGear.Rating;
                        _blnSkipRefresh = false;
                        return;
                    }
                }
                else
                    objGear.Rating = decimal.ToInt32(nudGearRating.Value);
                if (objGear.Bonus != null || (objGear.WirelessOn && objGear.WirelessBonus != null))
                {
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                    if (!string.IsNullOrEmpty(objGear.Extra))
                    {
                        ImprovementManager.ForcedValue = objGear.Extra.TrimEndOnce(", Hacked");
                    }
                    bool blnAddBonus = true;
                    if (objGear.Name == "Qi Focus")
                    {
                        if (!objGear.Bonded)
                            blnAddBonus = false;
                    }
                    if (blnAddBonus)
                    {
                        if (objGear.Bonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                        if (objGear.WirelessOn && objGear.WirelessBonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                    }

                    if (!objGear.Equipped)
                        objGear.ChangeEquippedStatus(false);
                }

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void nudGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treGear.SelectedNode == null)
                return;
            // Attempt to locate the selected piece of Gear.
            if (treGear.SelectedNode.Level > 0)
            {
                Gear objSelectedGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());

                if (objSelectedGear != null)
                {
                    objSelectedGear.Quantity = nudGearQty.Value;

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void chkArmorEquipped_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treArmor.SelectedNode == null)
                return;

            // Locate the selected Armor or Armor Mod.
            Armor objArmor = CharacterObject.Armor.FindById(treArmor.SelectedNode.Tag.ToString());
            if (objArmor != null)
            {
                objArmor.Equipped = chkArmorEquipped.Checked;
            }
            else
            {
                ArmorMod objMod = CharacterObject.Armor.FindArmorMod(treArmor.SelectedNode.Tag.ToString());
                if (objMod != null)
                {
                    objMod.Equipped = chkArmorEquipped.Checked;
                }
                else
                {
                    Gear objGear = CharacterObject.Armor.FindArmorGear(treArmor.SelectedNode.Tag.ToString(), out objArmor, out objMod);
                    if (objGear != null)
                    {
                        objGear.Equipped = chkArmorEquipped.Checked;
                        if (chkArmorEquipped.Checked)
                        {
                            // Add the Gear's Improevments to the character.
                            if (objArmor.Equipped && objMod?.Equipped != false)
                            {
                                objGear.ChangeEquippedStatus(true);
                            }
                        }
                        else
                        {
                            objGear.ChangeEquippedStatus(false);
                        }
                    }
                    else
                        return;
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treWeapons.SelectedNode == null)
                return;
            // Locate the selected Weapon Accessory or Modification.
            WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(treWeapons.SelectedNode.Tag.ToString());
            if (objAccessory != null)
            {
                objAccessory.Installed = chkWeaponAccessoryInstalled.Checked;
            }
            else
            {
                // Determine if this is an Underbarrel Weapon.
                Weapon objWeapon = CharacterObject.Weapons.DeepFindById(treWeapons.SelectedNode.Tag.ToString());
                if (objWeapon != null)
                {
                    objWeapon.Installed = chkWeaponAccessoryInstalled.Checked;
                }
                else
                {
                    // Find the selected Gear.
                    Gear objSelectedGear = CharacterObject.Weapons.FindWeaponGear(treWeapons.SelectedNode.Tag.ToString());
                    if (objSelectedGear != null)
                    {
                        objSelectedGear.Equipped = chkWeaponAccessoryInstalled.Checked;

                        objSelectedGear.ChangeEquippedStatus(chkWeaponAccessoryInstalled.Checked);

                        IsCharacterUpdateRequested = true;
                    }
                    else
                        return;
                }
            }

            IsDirty = true;
        }

        private void chkIncludedInWeapon_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treWeapons.SelectedNode == null)
                return;
            // Locate the selected Weapon Accessory or Modification.
            WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(treWeapons.SelectedNode.Tag.ToString());
            if (objAccessory != null)
            {
                objAccessory.IncludedInWeapon = chkIncludedInWeapon.Checked;
                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void treGear_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string strSelectedId = treGear.SelectedNode?.Tag.ToString();
            if (string.IsNullOrEmpty(strSelectedId))
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

            // Do not allow the root element to be moved.
            if (strSelectedId != "Node_SelectedGear")
            {
                _intDragLevel = treGear.SelectedNode.Level;
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void treGear_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treGear_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
            TreeNode nodDestination = ((TreeView)sender).GetNodeAt(pt);

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
            Gear objSelectedGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
            if (objSelectedGear != null)
            {
                objSelectedGear.Equipped = chkGearEquipped.Checked;

                objSelectedGear.ChangeEquippedStatus(chkGearEquipped.Checked);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void chkGearHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treGear.SelectedNode == null)
                return;
            string strGuid = treGear.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objCommlink = CharacterObject.Gear.DeepFindById(strGuid);
                if (objCommlink != null)
                {
                    objCommlink.SetHomeNode(CharacterObject, chkGearHomeNode.Checked);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void chkCyberwareHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            string strGuid = treCyberware.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objCommlink = CharacterObject.Cyberware.FindCyberwareGear(strGuid) ?? (IHasMatrixAttributes) CharacterObject.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGuid);
                if (objCommlink != null)
                {
                    objCommlink.SetHomeNode(CharacterObject, chkCyberwareHomeNode.Checked);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void chkIncludedInArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            // Locate the selected Armor Modification.
            ArmorMod objMod = CharacterObject.Armor.FindArmorMod(treArmor.SelectedNode.Tag.ToString());
            if (objMod != null)
            {
                objMod.IncludedInArmor = chkIncludedInArmor.Checked;

                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void chkCommlinks_CheckedChanged(object sender, EventArgs e)
        {
            RefreshGears(treGear, cmsGearLocation, cmsGear, chkCommlinks.Checked);
        }

        private void chkGearActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || treGear.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            IHasMatrixAttributes objSelectedCommlink = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
            if (objSelectedCommlink != null)
            {
                objSelectedCommlink.SetActiveCommlink(CharacterObject, chkGearActiveCommlink.Checked);

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void chkCyberwareActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strGuid = treCyberware.SelectedNode?.Tag.ToString();
            // Attempt to locate the selected piece of Gear.
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objSelectedCommlink = CharacterObject.Cyberware.DeepFindById(strGuid) ?? (IHasMatrixAttributes) CharacterObject.Cyberware.FindCyberwareGear(strGuid);
                if (objSelectedCommlink != null)
                {
                    objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void chkVehicleActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strGuid = treVehicles.SelectedNode?.Tag.ToString();
            // Attempt to locate the selected piece of Gear.
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objSelectedCommlink = CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                           (CharacterObject.Vehicles.FirstOrDefault(x => x.InternalId == strGuid) ??
                                                            (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objSelectedCommlink != null)
                {
                    objSelectedCommlink.SetActiveCommlink(CharacterObject, chkVehicleActiveCommlink.Checked);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }

        private void cboGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboGearAttack.Enabled)
                return;

            _blnLoading = true;

            IHasMatrixAttributes objTarget = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
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

            IHasMatrixAttributes objTarget = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
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

            IHasMatrixAttributes objTarget = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
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

            IHasMatrixAttributes objTarget = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());
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

            string strGuid = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objTarget = CharacterObject.Vehicles.FindById(strGuid) ??
                                                 (CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                  (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearAttack, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            _blnLoading = false;
        }
        private void cboVehicleGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearSleaze.Enabled)
                return;

            _blnLoading = true;

            string strGuid = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objTarget = CharacterObject.Vehicles.FindById(strGuid) ??
                                                 (CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                  (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearSleaze, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            _blnLoading = false;
        }
        private void cboVehicleGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearFirewall.Enabled)
                return;

            _blnLoading = true;

            string strGuid = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objTarget = CharacterObject.Vehicles.FindById(strGuid) ??
                                                 (CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                  (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearFirewall, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            _blnLoading = false;
        }
        private void cboVehicleGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || !cboVehicleGearDataProcessing.Enabled)
                return;

            _blnLoading = true;

            string strGuid = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objTarget = CharacterObject.Vehicles.FindById(strGuid) ??
                                                 (CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                  (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleGearDataProcessing, cboVehicleGearAttack, cboVehicleGearSleaze, cboVehicleGearDataProcessing, cboVehicleGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            _blnLoading = false;
        }
#endregion

#region Additional Vehicle Tab Control Events
        private void treVehicles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshSelectedVehicle();
            RefreshPasteStatus(sender, e);
        }

        private void treVehicles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (treVehicles.SelectedNode != null && treVehicles.SelectedNode.Level > 1)
            {
                // Determine if this is a piece of Gear. If not, don't let the user drag the Node.
                Gear objGear = CharacterObject.Vehicles.FindVehicleGear(treVehicles.SelectedNode.Tag.ToString());
                if (objGear != null)
                {
                    _eDragButton = e.Button;
                    _blnDraggingGear = true;
                    _intDragLevel = treVehicles.SelectedNode.Level;
                    DoDragDrop(e.Item, DragDropEffects.Move);
                }
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
                CharacterObject.MoveVehicleNode(intNewIndex, nodDestination, treVehicles.SelectedNode);
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

        private void nudVehicleRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Locate the currently selected VehicleMod.
                VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId);
                if (objMod != null)
                {
                    objMod.Rating = decimal.ToInt32(nudVehicleRating.Value);
                    treVehicles.SelectedNode.Text = objMod.DisplayName(GlobalOptions.Language);
                }
                else
                {
                    // Locate the currently selected Vehicle Gear,.
                    Gear objGear = CharacterObject.Vehicles.FindVehicleGear(strSelectedId);
                    if (objGear != null)
                    {
                        if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                        {
                            if (!RefreshSingleFocusRating(treFoci, objGear, decimal.ToInt32(nudVehicleRating.Value)))
                            {
                                _blnSkipRefresh = true;
                                nudVehicleRating.Value = objGear.Rating;
                                _blnSkipRefresh = false;
                                return;
                            }
                        }
                        else
                            objGear.Rating = decimal.ToInt32(nudVehicleRating.Value);
                        treVehicles.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    }
                    else
                    {
                        // See if this is a piece of Cyberware.
                        Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);
                        if (objCyberware != null)
                        {
                            objCyberware.Rating = decimal.ToInt32(nudVehicleRating.Value);
                            treVehicles.SelectedNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
                        }
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                WeaponAccessory objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strSelectedId);
                if (objAccessory != null)
                    objAccessory.Installed = chkVehicleWeaponAccessoryInstalled.Checked;
                else
                {
                    VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId);
                    if (objVehicleMod != null)
                        objVehicleMod.Installed = chkVehicleWeaponAccessoryInstalled.Checked;
                    else
                    {
                        Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strSelectedId);
                        if (objWeapon != null)
                            objWeapon.Installed = chkVehicleWeaponAccessoryInstalled.Checked;
                        else
                        {
                            WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out Vehicle _);
                            if (objWeaponMount != null)
                                objWeaponMount.Installed = chkVehicleWeaponAccessoryInstalled.Checked;
                            else
                                return; // Don't mark IsDirty = true if we didn't find anything
                        }
                    }
                }
            }

            IsDirty = true;
        }

        private void nudVehicleGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            Gear objGear = CharacterObject.Vehicles.FindVehicleGear(treVehicles.SelectedNode.Tag.ToString());

            objGear.Quantity = nudVehicleGearQty.Value;
            treVehicles.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            string strGuid = treVehicles.SelectedNode?.Tag.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(strGuid))
            {
                IHasMatrixAttributes objTarget = CharacterObject.Vehicles.FindById(strGuid) ??
                                                 (CharacterObject.Vehicles.FindVehicleGear(strGuid) ??
                                                  (IHasMatrixAttributes) CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strGuid));
                if (objTarget != null)
                {
                    objTarget.SetHomeNode(CharacterObject, chkVehicleHomeNode.Checked);

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }
#endregion

#region Additional Spells and Spirits Tab Control Events
        private void treSpells_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _blnSkipRefresh = true;
            if (treSpells.SelectedNode.Level > 0)
            {
                // Locate the selected Spell.
                Spell objSpell = CharacterObject.Spells.FindById(e.Node.Tag.ToString());

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

                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
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
                        strTip = strTip.CheapReplace(objAttrib.DisplayAbbrev, () => objAttrib.DisplayAbbrev + strSpaceCharacter + '(' + objAttrib.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
                    }

                    if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.DrainResistance) != 0)
                        strTip += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Skill_DicePoolModifiers", GlobalOptions.Language) + strSpaceCharacter + '(' + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.DrainResistance).ToString(GlobalOptions.CultureInfo) + ')';
                    //if (objSpell.Limited)
                    //{
                    //    intDrain += 2;
                    //    strTip += " + " + LanguageManager.GetString("String_SpellLimited") + " (2)";
                    //}
                    lblDrainAttributesValue.Text = intDrain.ToString(GlobalOptions.CultureInfo);
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
                string strSelectedId = e.Node.Tag.ToString();
                Focus objFocus = CharacterObject.Foci.FirstOrDefault(x => x.GearObject.InternalId == strSelectedId);

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
                    StackedFocus objStack = CharacterObject.StackedFoci.FirstOrDefault(x => x.InternalId == strSelectedId);

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

            string strSelectedId = e.Node.Tag.ToString();

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
                intFociTotal = CharacterObject.StackedFoci.FirstOrDefault(x => x.InternalId == strSelectedId)?.TotalForce ?? 0;
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
                    (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && CharacterObject.InitiateGrade + 1 > CharacterObject.MAGAdept.TotalValue))
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

            if (objSelectedFocus != null)
            {
                Focus objFocus = new Focus(CharacterObject)
                {
                    GearObject = objSelectedFocus
                };

                if (objSelectedFocus.Equipped)
                {
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
                                objSelectedFocus.ChangeEquippedStatus(true);
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }
                CharacterObject.Foci.Add(objFocus);
                objSelectedFocus.Bonded = true;
            }
            else
            {
                // This is a Stacked Focus.
                StackedFocus objStack = CharacterObject.StackedFoci.FirstOrDefault(x => x.InternalId == strSelectedId);
                if (objStack != null)
                {
                    Gear objStackGear = CharacterObject.Gear.DeepFindById(objStack.GearId);
                    if (objStackGear.Equipped)
                    {
                        foreach (Gear objGear in objStack.Gear)
                        {
                            if (objGear.Bonus != null || objGear.WirelessOn && objGear.WirelessBonus != null)
                            {
                                if (!string.IsNullOrEmpty(objGear.Extra))
                                    ImprovementManager.ForcedValue = objGear.Extra;
                                if (objGear.Bonus != null)
                                {
                                    if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language)))
                                    {
                                        // Clear created improvements
                                        objStackGear.ChangeEquippedStatus(false);
                                        objStackGear.ChangeEquippedStatus(true);
                                        e.Cancel = true;
                                        return;
                                    }
                                    objGear.Extra = ImprovementManager.SelectedValue;
                                }
                                if (objGear.WirelessOn && objGear.WirelessBonus != null)
                                {
                                    if (!ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language)))
                                    {
                                        // Clear created improvements
                                        objStackGear.ChangeEquippedStatus(false);
                                        objStackGear.ChangeEquippedStatus(true);
                                        e.Cancel = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    objStack.Bonded = true;
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void nudArmorRating_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            string strSelectedArmor = treArmor.SelectedNode.Tag.ToString();

            // Locate the selected ArmorMod.
            ArmorMod objMod = CharacterObject.Armor.FindArmorMod(strSelectedArmor);
            if (objMod != null)
            {
                objMod.Rating = decimal.ToInt32(nudArmorRating.Value);
                treArmor.SelectedNode.Text = objMod.DisplayName(GlobalOptions.Language);

                // See if a Bonus node exists.
                if ((objMod.Bonus != null && objMod.Bonus.InnerXml.Contains("Rating")) || (objMod.WirelessOn && objMod.WirelessBonus != null && objMod.WirelessBonus.InnerXml.Contains("Rating")))
                {
                    // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                    ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId);
                    if (objMod.Bonus != null)
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.Bonus, false, objMod.Rating, objMod.DisplayNameShort(GlobalOptions.Language));
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.ArmorMod, objMod.InternalId, objMod.WirelessBonus, false, objMod.Rating, objMod.DisplayNameShort(GlobalOptions.Language));
                }
            }
            else
            {
                // Locate the selected Gear.
                Gear objGear = CharacterObject.Armor.FindArmorGear(strSelectedArmor);
                if (objGear != null)
                {
                    if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                    {
                        if (!RefreshSingleFocusRating(treFoci, objGear, decimal.ToInt32(nudArmorRating.Value)))
                        {
                            _blnSkipRefresh = true;
                            nudArmorRating.Value = objGear.Rating;
                            _blnSkipRefresh = false;
                            return;
                        }
                    }
                    else
                        objGear.Rating = decimal.ToInt32(nudArmorRating.Value);
                    treArmor.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

                    // See if a Bonus node exists.
                    if ((objGear.Bonus != null && objGear.Bonus.InnerXml.Contains("Rating")) || (objGear.WirelessOn && objGear.WirelessBonus != null && objGear.WirelessBonus.InnerXml.Contains("Rating")))
                    {
                        // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId);
                        if (objGear.Bonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.Bonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));
                        if (objGear.WirelessOn && objGear.WirelessBonus != null)
                            ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, objGear.InternalId, objGear.WirelessBonus, false, objGear.Rating, objGear.DisplayNameShort(GlobalOptions.Language));

                        if (!objGear.Equipped)
                            objGear.ChangeEquippedStatus(false);
                    }
                }
                else
                {
                    // Locate the selected Armor.
                    Armor objArmor = CharacterObject.Armor.FindById(strSelectedArmor);
                    if (objArmor != null)
                    {
                        objArmor.Rating = decimal.ToInt32(nudArmorRating.Value);
                        treArmor.SelectedNode.Text = objArmor.DisplayName(GlobalOptions.Language);
                    }
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void cboTradition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboTradition.IsInitalized(_blnLoading))
                return;

            XmlNode objXmlTradition = XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[name = \"" + cboTradition.SelectedValue + "\"]");

            if (objXmlTradition == null)
            {
                CharacterObject.MagicTradition = cboTradition.SelectedValue.ToString();
                CharacterObject.TraditionDrain = string.Empty;
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
                lblDrainAttributes.Visible = false;
                lblDrainAttributesValue.Visible = false;
            }
            else if (objXmlTradition["name"]?.InnerText == "Custom")
            {
                cboDrain.Visible = !CharacterObject.AdeptEnabled || CharacterObject.MagicianEnabled;
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
                lblDrainAttributes.Visible = true;
                lblDrainAttributesValue.Visible = true;

                CharacterObject.MagicTradition = string.IsNullOrEmpty(txtTraditionName.Text) ? cboTradition.SelectedValue.ToString() : txtTraditionName.Text;
            }
            else
            {
                cboDrain.Visible = false;
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
                lblDrainAttributes.Visible = true;
                lblDrainAttributesValue.Visible = true;

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
            if (_blnSkipRefresh)
                return;

            // Locate the Program that is selected in the tree.
            ComplexForm objComplexForm = CharacterObject.ComplexForms.FindById(treComplexForms.SelectedNode?.Tag.ToString());

            if (objComplexForm != null)
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

#region Additional AI Advanced Programs Tab Control Events
        private void treAIPrograms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the Program that is selected in the tree.
            AIProgram objProgram = CharacterObject.AIPrograms.FindById(treAIPrograms.SelectedNode?.Tag.ToString());

            if (objProgram != null)
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
#endregion

#region Additional Initiation Tab Control Events
        private void chkInitiationGroup_CheckedChanged(object sender, EventArgs e)
        {
            IsCharacterUpdateRequested = true;
        }

        private void chkInitiationOrdeal_CheckedChanged(object sender, EventArgs e)
        {
            IsCharacterUpdateRequested = true;
        }

        private void treMetamagic_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSelectedId = treMetamagic.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Locate the selected Metamagic.
                Metamagic objMetamagic = CharacterObject.Metamagics.FindById(treMetamagic.SelectedNode.Tag.ToString());

                if (objMetamagic != null)
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(objMetamagic.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objMetamagic.Grade >= 0;
                    string strPage = objMetamagic.Page(GlobalOptions.Language);
                    lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objMetamagic.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objMetamagic.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                }
                else
                {
                    // Locate the selected Art.
                    Art objArt = CharacterObject.Arts.FindById(treMetamagic.SelectedNode.Tag.ToString());

                    if (objArt != null)
                    {
                        cmdDeleteMetamagic.Text = LanguageManager.GetString(objArt.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                        cmdDeleteMetamagic.Enabled = objArt.Grade >= 0;
                        string strPage = objArt.Page(GlobalOptions.Language);
                        lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objArt.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                    }
                    else
                    {
                        // Locate the selected Spell.
                        Spell objSpell = CharacterObject.Spells.FindById(treMetamagic.SelectedNode.Tag.ToString());

                        if (objSpell != null)
                        {
                            cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveMetamagic", GlobalOptions.Language);
                            cmdDeleteMetamagic.Enabled = objSpell.Grade >= 0;
                            string strPage = objSpell.DisplayPage(GlobalOptions.Language);
                            lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objSpell.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                            GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objSpell.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                        }
                        else
                        {
                            // Locate the selected Complex Form.
                            ComplexForm objComplexForm = CharacterObject.ComplexForms.FindById(treMetamagic.SelectedNode.Tag.ToString());

                            if (objComplexForm != null)
                            {
                                cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveEcho", GlobalOptions.Language);
                                cmdDeleteMetamagic.Enabled = objComplexForm.Grade >= 0;
                                string strPage = objComplexForm.Page(GlobalOptions.Language);
                                lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objComplexForm.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                                GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objComplexForm.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                            }
                            else
                            {
                                // Locate the selected Enhancement.
                                Enhancement objEnhancement = CharacterObject.FindEnhancement(treMetamagic.SelectedNode.Tag.ToString());

                                if (objEnhancement != null)
                                {
                                    cmdDeleteMetamagic.Text = LanguageManager.GetString(objEnhancement.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                                    cmdDeleteMetamagic.Enabled = objEnhancement.Grade >= 0;
                                    string strPage = objEnhancement.Page(GlobalOptions.Language);
                                    lblMetamagicSource.Text = CommonFunctions.LanguageBookShort(objEnhancement.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                                    GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, CommonFunctions.LanguageBookLong(objEnhancement.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                                }
                                else
                                {
                                    cmdDeleteMetamagic.Text = LanguageManager.GetString(CharacterObject.MAGEnabled ? "Button_RemoveInitiateGrade" : "Button_RemoveSubmersionGrade", GlobalOptions.Language);
                                    cmdDeleteMetamagic.Enabled = true;
                                    lblMetamagicSource.Text = string.Empty;
                                    GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                cmdDeleteMetamagic.Text = LanguageManager.GetString(CharacterObject.MAGEnabled ? "Button_RemoveInitiateGrade" : "Button_RemoveSubmersionGrade", GlobalOptions.Language);
                cmdDeleteMetamagic.Enabled = false;
                lblMetamagicSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblMetamagicSource, string.Empty);
            }
        }
        #endregion

        #region Additional Critter Powers Tab Control Events
        private void treCritterPowers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Look for the selected Critter Power.
            CritterPower objPower = CharacterObject.CritterPowers.FindById(treCritterPowers.SelectedNode?.Tag.ToString());

            if (objPower != null)
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
            CritterPower objPower = CharacterObject.CritterPowers.FindById(treCritterPowers.SelectedNode?.Tag.ToString());

            if (objPower != null)
            {
                objPower.CountTowardsLimit = chkCritterPowerCount.Checked;

                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
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
#endregion

#region Other Control Events
        private void tabCharacterTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus(sender, e);
        }

        private void tabStreetGearTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPasteStatus(sender, e);
        }
#endregion

#region Sourcebook Label Events
        private void txtNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                ((TextBox) sender)?.SelectAll();
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

            if (CharacterObjectOptions.MetatypeCostsKarma)
            {
                lblKarmaMetatypeBP.Text = (CharacterObject.MetatypeBP * CharacterObjectOptions.MetatypeCostsKarmaMultiplier).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                                          LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            }
            else
            {
                lblKarmaMetatypeBP.Text = "0" + strSpaceCharacter + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            }

            string strToolTip = strMetatype + strSpaceCharacter + '(' + CharacterObject.MetatypeBP + ')';
            GlobalOptions.ToolTipProcessor.SetToolTip(lblKarmaMetatypeBP, strToolTip);

            mnuSpecialConvertToFreeSprite.Visible = CharacterObject.IsSprite;

            mnuSpecialCyberzombie.Visible = CharacterObject.MetatypeCategory != "Cyberzombie";
        }

        /// <summary>
        /// Calculate the BP used by Primary Attributes.
        /// </summary>
        private static int CalculateAttributeBP(IEnumerable<CharacterAttrib> attribs, IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            int intBP = 0;
            // Primary and Special Attributes are calculated separately since you can only spend a maximum of 1/2 your BP allotment on Primary Attributes.
            // Special Attributes are not subject to the 1/2 of max BP rule.
            foreach (CharacterAttrib att in attribs)
            {
                intBP += att.TotalKarmaCost;
            }
            if (extraAttribs != null)
            {
                foreach (CharacterAttrib att in extraAttribs)
                {
                    intBP += att.TotalKarmaCost;
                }
            }
            return intBP;
        }

        private int CalculateAttributePriorityPoints(IEnumerable<CharacterAttrib> attribs, IEnumerable<CharacterAttrib> extraAttribs = null)
        {
            int intAtt = 0;
            if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority ||
                CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                // Get the total of "free points" spent
                foreach (CharacterAttrib att in attribs)
                {
                    intAtt += att.SpentPriorityPoints;
                }
                if (extraAttribs != null)
                {
                    // Get the total of "free points" spent
                    foreach (CharacterAttrib att in extraAttribs)
                    {
                        intAtt += att.SpentPriorityPoints;
                    }
                }
            }
            return intAtt;
        }

        private string BuildAttributes(ICollection<CharacterAttrib> attribs, ICollection<CharacterAttrib> extraAttribs = null, bool special = false)
        {
            int bp = CalculateAttributeBP(attribs, extraAttribs);
            string s = $"{bp} {LanguageManager.GetString("String_Karma", GlobalOptions.Language)}";
            int att = CalculateAttributePriorityPoints(attribs, extraAttribs);
            int total = special ? CharacterObject.TotalSpecial : CharacterObject.TotalAttributes;
            if ((CharacterObject.BuildMethod == CharacterBuildMethod.Priority) ||
                (CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen))
            {
                if (bp > 0)
                {
                    s = string.Format(LanguageManager.GetString("String_OverPriorityPoints", GlobalOptions.Language),
                        (total - att), total, bp);
                }
                else
                {
                    s = string.Format("{0}" + LanguageManager.GetString("String_Of", GlobalOptions.Language) + "{1}",
                        (total - att), total);
                }
            }
            return s;
        }

        /// <summary>
        /// Calculate the number of Build Points the character has remaining.
        /// </summary>
        private int CalculateBP(bool blnDoUIUpdate = true)
        {
            int intKarmaPointsRemain = CharacterObject.BuildKarma;
            //int intPointsUsed = 0; // used as a running total for each section
            int intFreestyleBPMin = 0;
            int intFreestyleBP = 0;
            string strPoints = blnDoUIUpdate ? LanguageManager.GetString("String_Karma", GlobalOptions.Language) : string.Empty;

            // ------------------------------------------------------------------------------
            // Metatype/Metavariant only cost points when working with BP (or when the Metatype Costs Karma option is enabled when working with Karma).
            if ((CharacterObject.BuildMethod == CharacterBuildMethod.Karma || CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule) && CharacterObjectOptions.MetatypeCostsKarma)
            {
                // Subtract the BP used for Metatype.
                intKarmaPointsRemain -= (CharacterObject.MetatypeBP * CharacterObjectOptions.MetatypeCostsKarmaMultiplier);
            }

            if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority || CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                intKarmaPointsRemain -= (CharacterObject.MetatypeBP);
            }

            // ------------------------------------------------------------------------------
            // Calculate the points used by Contacts.
            int intPointsInContacts = 0;

            int intContactPoints = CharacterObject.ContactPoints;
            int intContactPointsLeft = intContactPoints;
            int intGroupContacts = 0;
            int intHighPlacesFriends = 0;
            foreach (Contact objContact in CharacterObject.Contacts)
            {
                // Don't care about free contacts
                if (objContact.EntityType != ContactType.Contact || objContact.Free)
                    continue;

                if (objContact.Connection >= 8 && CharacterObject.FriendsInHighPlaces)
                {
                    intHighPlacesFriends += (objContact.Connection + objContact.Loyalty);
                }
                else if (objContact.IsGroup == false)
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
                else
                {
                    //save this for later, as a group contract is counted as a positive quality
                    intGroupContacts += objContact.ContactPoints;
                }
            }

            CharacterObject.ContactPointsUsed = intContactPointsLeft;

            if (intPointsInContacts > 0 || (CharacterObject.CHA.Value * 4 < intHighPlacesFriends))
            {
                intPointsInContacts += Math.Max(0, intHighPlacesFriends - (CharacterObject.CHA.Value * 4));
            }

            intKarmaPointsRemain -= intPointsInContacts;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Enemies. These are added to the BP since they are technically
            // a Negative Quality.
            int intEnemyPoints = 0;
            foreach (Contact objLoopEnemy in CharacterObject.Contacts)
            {
                if (objLoopEnemy.EntityType == ContactType.Enemy && !objLoopEnemy.Free)
                {
                    // The Enemy's Karma cost = their (Connection + Loyalty Rating) x Karma multiplier.
                    intEnemyPoints -= (objLoopEnemy.Connection + objLoopEnemy.Loyalty) * CharacterObjectOptions.KarmaEnemy;
                }
            }

            // dont add in enemy costs here, carry it over later under qualities

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Qualities.
            int intPositiveQualities = intGroupContacts; // group contacts are positive qualities
            int intNegativeQualities = intEnemyPoints;   // enemies are negative qualities
            int intLifeModuleQualities = 0;

            intPositiveQualities += CharacterObject.Qualities.Where(q => q.Type == QualityType.Positive && q.ContributeToBP && q.ContributeToLimit).Sum(q => q.BP * CharacterObjectOptions.KarmaQuality);
            int unlimitedPositive = CharacterObject.Qualities.Where(q => q.Type == QualityType.Positive && q.ContributeToBP && !q.ContributeToLimit).Sum(q => q.BP * CharacterObjectOptions.KarmaQuality);
            intNegativeQualities += CharacterObject.Qualities.Where(q => q.Type == QualityType.Negative && q.ContributeToBP && q.ContributeToLimit).Sum(q => q.BP * CharacterObjectOptions.KarmaQuality);
            int unlimitedNegative = CharacterObject.Qualities.Where(q => q.Type == QualityType.Negative && q.ContributeToBP && !q.ContributeToLimit).Sum(q => q.BP * CharacterObjectOptions.KarmaQuality);
            intLifeModuleQualities += CharacterObject.Qualities.Where(q => q.Type == QualityType.LifeModule && q.ContributeToBP && q.ContributeToLimit).Sum(q => q.BP * CharacterObjectOptions.KarmaQuality);

            // Deduct the amounts for free Qualities.
            int intPositiveFree = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreePositiveQualities) * CharacterObjectOptions.KarmaQuality;
            int intNegativeFree = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeNegativeQualities) * CharacterObjectOptions.KarmaQuality;

            intNegativeQualities -= intNegativeFree;
            intPositiveQualities -= intPositiveFree;

            // If the character is only allowed to gain 25 BP from Negative Qualities but allowed to take as many as they'd like, limit their refunded points.
            if (CharacterObjectOptions.ExceedNegativeQualitiesLimit)
            {
                int intNegativeQualityLimit = -CharacterObject.GameplayOptionQualityLimit;
                if (intNegativeQualities < intNegativeQualityLimit)
                {
                    intNegativeQualities = intNegativeQualityLimit;
                }
            }

            // If the character is allowed to take as many Positive Qualities as they'd like but all costs in excess are doubled, add the excess to their point cost.
            if (CharacterObjectOptions.ExceedPositiveQualitiesCostDoubled)
            {
                int intPositiveQualityExcess = intPositiveQualities - CharacterObject.GameplayOptionQualityLimit;
                if (intPositiveQualityExcess > 0)
                {
                    intPositiveQualities += intPositiveQualityExcess;
                }
            }

            int intQualityPointsUsed = intLifeModuleQualities + intNegativeQualities + intPositiveQualities + unlimitedPositive + unlimitedNegative;

            intKarmaPointsRemain -= intQualityPointsUsed;
            intFreestyleBP += intQualityPointsUsed;

            // ------------------------------------------------------------------------------
            // Update Primary Attributes and Special Attributes values.
            int intAttributePointsUsed = CalculateAttributeBP(CharacterObject.AttributeSection.AttributeList);
            intAttributePointsUsed += CalculateAttributeBP(CharacterObject.AttributeSection.SpecialAttributeList);
            intKarmaPointsRemain -= intAttributePointsUsed;

            // ------------------------------------------------------------------------------
            // Include the BP used by Martial Arts.
            int intMartialArtsPoints = 0;
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            StringBuilder strMartialArtsBPToolTip = new StringBuilder();
            foreach (MartialArt objMartialArt in CharacterObject.MartialArts)
            {
                if (!objMartialArt.IsQuality)
                {
                    int intLoopCost = objMartialArt.Rating * objMartialArt.Cost * CharacterObjectOptions.KarmaQuality;
                    intMartialArtsPoints += intLoopCost;

                    if (blnDoUIUpdate)
                    {
                        if (strMartialArtsBPToolTip.Length > 0)
                            strMartialArtsBPToolTip.Append(Environment.NewLine + strSpaceCharacter + '+' + strSpaceCharacter);
                        strMartialArtsBPToolTip.Append(objMartialArt.DisplayName(GlobalOptions.Language) + strSpaceCharacter + '(' + intLoopCost.ToString(GlobalOptions.CultureInfo) + ')');

                        bool blnIsFirst = true;
                        foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                        {
                            if (blnIsFirst)
                            {
                                blnIsFirst = false;
                                continue;
                            }

                            intLoopCost = 5 * CharacterObjectOptions.KarmaQuality;
                            intMartialArtsPoints += intLoopCost;

                            strMartialArtsBPToolTip.Append(Environment.NewLine + strSpaceCharacter + '+' + strSpaceCharacter + objTechnique.DisplayName(GlobalOptions.Language) + strSpaceCharacter + '(' + intLoopCost.ToString(GlobalOptions.CultureInfo) + ')');
                        }
                    }
                    else
                        // Add in the Techniques
                        intMartialArtsPoints += (Math.Max(objMartialArt.Techniques.Count - 1, 0) * 5) * CharacterObjectOptions.KarmaQuality;
                }
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
            int intNuyenBP = decimal.ToInt32(CharacterObject.NuyenBP);

            intKarmaPointsRemain -= intNuyenBP;

            intFreestyleBP += intNuyenBP;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spells.
            int intSpellPointsUsed = 0;
            int intRitualPointsUsed = 0;
            int intPrepPointsUsed = 0;
            int spellPoints = 0;
            int ritualPoints = 0;
            int prepPoints = 0;
            if (CharacterObject.MagicianEnabled ||
                    CharacterObject.Improvements.Any(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.FreeSpells ||
                                                                     objImprovement.ImproveType == Improvement.ImprovementType.FreeSpellsATT ||
                                                                     objImprovement.ImproveType == Improvement.ImprovementType.FreeSpellsSkill) && objImprovement.Enabled))
            {
                // Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
                int spells = CharacterObject.Spells.Count(spell => spell.Grade == 0 && (!spell.Alchemical) && spell.Category != "Rituals" && !spell.FreeBonus);
                int intTouchOnlySpells = CharacterObject.Spells.Count(spell => spell.Grade == 0 && (!spell.Alchemical) && spell.Category != "Rituals" && spell.Range == "T" && !spell.FreeBonus);
                int rituals = CharacterObject.Spells.Count(spell => spell.Grade == 0 && (!spell.Alchemical) && spell.Category == "Rituals" && !spell.FreeBonus);
                int preps = CharacterObject.Spells.Count(spell => spell.Grade == 0 && spell.Alchemical && !spell.FreeBonus);

                // Each spell costs KarmaSpell.
                int spellCost = CharacterObject.SpellKarmaCost("Spells");
                int ritualCost = CharacterObject.SpellKarmaCost("Rituals");
                int prepCost = CharacterObject.SpellKarmaCost("Preparations");
                int limit = CharacterObject.SpellLimit;

                // It is only karma-efficient to use spell points for Mastery qualities if real spell karma cost is not greater than unmodified spell karma cost
                if (spellCost <= CharacterObjectOptions.KarmaSpell)
                {
                    // Assume that every [spell cost] karma spent on a Mastery quality is paid for with a priority-given spell point instead, as that is the most karma-efficient.
                    int intQualityKarmaToSpellPoints = Math.Min(limit, (CharacterObject.Qualities.Where(objQuality => objQuality.CanBuyWithSpellPoints).Sum(objQuality => objQuality.BP) * CharacterObjectOptions.KarmaQuality) / CharacterObjectOptions.KarmaSpell);
                    spells += intQualityKarmaToSpellPoints;
                    // Add the karma paid for by spell points back into the available karma pool.
                    intKarmaPointsRemain += intQualityKarmaToSpellPoints * CharacterObjectOptions.KarmaSpell;
                }

                int limitMod = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SpellLimit) +
                               ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeSpells);
                int limitModTouchOnly = 0;
                foreach (Improvement imp in CharacterObject.Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.FreeSpellsATT && i.Enabled))
                {
                    int intAttValue = CharacterObject.GetAttribute(imp.ImprovedName).TotalValue;
                    if (imp.UniqueName.Contains("half"))
                        intAttValue = (intAttValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        limitModTouchOnly += intAttValue;
                    else
                        limitMod += intAttValue;
                }
                foreach (Improvement imp in CharacterObject.Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill && i.Enabled))
                {
                    Skill skill = CharacterObject.SkillsSection.GetActiveSkill(imp.ImprovedName);
                    int intSkillValue = skill.TotalBaseRating;

                    if (imp.UniqueName.Contains("half"))
                        intSkillValue = (intSkillValue + 1) / 2;
                    if (imp.UniqueName.Contains("touchonly"))
                        limitModTouchOnly += intSkillValue;
                    else
                        limitMod += intSkillValue;
                    //TODO: I don't like this being hardcoded, even though I know full well CGL are never going to reuse this.
                    foreach (SkillSpecialization spec in skill.Specializations)
                    {
                        if (CharacterObject.Spells.Any(spell => spell.Category == spec.Name && !spell.FreeBonus))
                        {
                            spells--;
                        }
                    }
                }

                if (nudMysticAdeptMAGMagician.Value > 0)
                {
                    int intPPBought = decimal.ToInt32(nudMysticAdeptMAGMagician.Value);
                    if (CharacterObjectOptions.PrioritySpellsAsAdeptPowers)
                    {
                        spells += Math.Min(limit, intPPBought);
                        intPPBought = Math.Max(0, intPPBought - limit);
                    }
                    intAttributePointsUsed = intPPBought * CharacterObject.Options.KarmaMysticAdeptPowerPoint;
                    intKarmaPointsRemain -= intAttributePointsUsed;
                }
                spells -= intTouchOnlySpells - Math.Max(0, intTouchOnlySpells - limitModTouchOnly);

                for (int i = limit + limitMod; i > 0; i--)
                {
                    if (spells > 0)
                    {
                        spells--;
                        spellPoints++;
                    }
                    else if (rituals > 0)
                    {
                        rituals--;
                        ritualPoints++;
                    }
                    else if (preps > 0)
                    {
                        preps--;
                        prepPoints++;
                    }
                    else
                    {
                        break;
                    }
                }
                intKarmaPointsRemain -= Math.Max(0, spells) * (spellCost);
                intKarmaPointsRemain -= Math.Max(0, rituals) * (ritualCost);
                intKarmaPointsRemain -= Math.Max(0, preps) * (prepCost);

                intSpellPointsUsed += Math.Max(Math.Max(0, spells) * (spellCost), 0);
                intRitualPointsUsed += Math.Max(Math.Max(0, rituals) * (ritualCost), 0);
                intPrepPointsUsed += Math.Max(Math.Max(0, preps) * prepCost, 0);
                if (blnDoUIUpdate)
                {
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblSpellsBP, $"{spells.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildRitualsBP, $"{rituals.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildPrepsBP, $"{preps.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    if (limit + limitMod > 0)
                    {
                        lblBuildPrepsBP.Text =
                            string.Format(
                                $"{prepPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                        lblSpellsBP.Text =
                            string.Format(
                                $"{spellPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                        lblBuildRitualsBP.Text =
                            string.Format(
                                $"{ritualPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                    }
                    else
                    {
                        if (limitMod == 0)
                        {
                            lblBuildPrepsBP.Text =
                                intPrepPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                            lblSpellsBP.Text =
                                intSpellPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                            lblBuildRitualsBP.Text =
                                intRitualPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                        }
                        else
                        {
                            //TODO: Make the costs render better, currently looks wrong as hell
                            lblBuildPrepsBP.Text =
                                string.Format(
                                    $"{prepPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{limitMod.ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                            lblSpellsBP.Text =
                                string.Format(
                                    $"{spellPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{limitMod.ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                            lblBuildRitualsBP.Text =
                                string.Format(
                                    $"{ritualPoints.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{limitMod.ToString(GlobalOptions.CultureInfo)}:{strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                        }
                    }
                }
            }

            intFreestyleBP += intSpellPointsUsed + intRitualPointsUsed + intPrepPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Foci.
            int intFociPointsUsed = 0;
            StringBuilder strFociPointsTooltip = new StringBuilder();
            foreach (Focus objFocus in CharacterObject.Foci)
            {
                Gear objFocusGear =  objFocus.GearObject;
                // Each Focus costs an amount of Karma equal to their Force x speicific Karma cost.
                string strFocusName = objFocusGear.Name;
                string strFocusExtra = objFocusGear.Extra;
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
                        intKarmaMultiplier = CharacterObjectOptions.KarmaQiFocus;
                        break;
                    case "Sustaining Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSustainingFocus;
                        break;
                    case "Counterspelling Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaCounterspellingFocus;
                        break;
                    case "Banishing Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaBanishingFocus;
                        break;
                    case "Binding Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaBindingFocus;
                        break;
                    case "Weapon Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaWeaponFocus;
                        break;
                    case "Spellcasting Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSpellcastingFocus;
                        break;
                    case "Ritual Spellcasting Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaRitualSpellcastingFocus;
                        break;
                    case "Spell Shaping Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSpellShapingFocus;
                        break;
                    case "Summoning Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaSummoningFocus;
                        break;
                    case "Alchemical Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaAlchemicalFocus;
                        break;
                    case "Centering Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaCenteringFocus;
                        break;
                    case "Masking Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaMaskingFocus;
                        break;
                    case "Disenchanting Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaDisenchantingFocus;
                        break;
                    case "Power Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaPowerFocus;
                        break;
                    case "Flexible Signature Focus":
                        intKarmaMultiplier = CharacterObjectOptions.KarmaFlexibleSignatureFocus;
                        break;
                }
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements.Where(x => x.ImprovedName == strFocusName && (string.IsNullOrEmpty(x.Target) || strFocusExtra.Contains(x.Target)) && x.Enabled))
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaCost)
                        intExtraKarmaCost += objLoopImprovement.Value;
                    else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.FocusBindingKarmaMultiplier)
                        intKarmaMultiplier += objLoopImprovement.Value;
                }

                int intLoopCost = objFocus.Rating * intKarmaMultiplier + intExtraKarmaCost;
                intFociPointsUsed += intLoopCost;

                if (blnDoUIUpdate)
                {
                    if (strFociPointsTooltip.Length > 0)
                        strFociPointsTooltip.Append(Environment.NewLine + strSpaceCharacter + '+' + strSpaceCharacter);
                    strFociPointsTooltip.Append(objFocusGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language) + strSpaceCharacter + '(' + intLoopCost.ToString(GlobalOptions.CultureInfo) + ')');
                }
            }
            intKarmaPointsRemain -= intFociPointsUsed;

            // Calculate the BP used by Stacked Foci.
            foreach (StackedFocus objFocus in CharacterObject.StackedFoci)
            {
                if (objFocus.Bonded)
                {
                    int intBindingCost = objFocus.BindingCost;
                    intKarmaPointsRemain -= intBindingCost;
                    intFociPointsUsed += intBindingCost;

                    if (blnDoUIUpdate)
                    {
                        if (strFociPointsTooltip.Length > 0)
                            strFociPointsTooltip.Append(Environment.NewLine + strSpaceCharacter + '+' + strSpaceCharacter);
                        strFociPointsTooltip.Append(objFocus.Name(GlobalOptions.CultureInfo, GlobalOptions.Language) + strSpaceCharacter + '(' + intBindingCost.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }
            }

            intFreestyleBP += intFociPointsUsed;

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Spirits and Sprites.
            int intSpiritPointsUsed = 0;
            int intSpritePointsUsed = 0;
            foreach (Spirit objSpirit in CharacterObject.Spirits)
            {
                int intLoopKarma = objSpirit.ServicesOwed * CharacterObjectOptions.KarmaSpirit;
                // Each Sprite costs KarmaSpirit x Services Owed.
                intKarmaPointsRemain -= intLoopKarma;
                if (objSpirit.EntityType == SpiritType.Spirit)
                {
                    intSpiritPointsUsed += intLoopKarma;
                    // Each Fettered Spirit costs 3 x Force.
                    //TODO: Bind the 3 to an option.
                    if (objSpirit.Fettered)
                    {
                        intKarmaPointsRemain -= objSpirit.Force * 3;
                        intSpiritPointsUsed += objSpirit.Force * 3;
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
                    intFormsPointsUsed += 1;
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
                        intAIAdvancedProgramPointsUsed += 1;
                    else
                        intAINormalProgramPointsUsed += 1;
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

#if LEGACY
            // ------------------------------------------------------------------------------
            // Calculate the BP used by Martial Art Maneuvers.
            // Each Maneuver costs KarmaManeuver.
            int intManeuverPointsUsed = CharacterObject.MartialArtManeuvers.Count * CharacterObjectOptions.KarmaManeuver;
            intFreestyleBP += intManeuverPointsUsed;
            intKarmaPointsRemain -= intManeuverPointsUsed;
#endif

            // ------------------------------------------------------------------------------
            // Calculate the BP used by Initiation.
            int intInitiationPoints = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                intInitiationPoints += objGrade.KarmaCost;
                // Add the Karma cost of extra Metamagic/Echoes to the Initiation cost.
                int metamagicKarma = Math.Max(CharacterObject.Metamagics.Count(x => x.Grade == objGrade.Grade) - 1, 0);
                intInitiationPoints += CharacterObjectOptions.KarmaMetamagic * metamagicKarma;
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

            // Check to see if the character is a member of a Group.
            if (CharacterObject.GroupMember && CharacterObject.MAGEnabled)
                intInitiationPoints += CharacterObjectOptions.KarmaJoinGroup;

            intKarmaPointsRemain -= intInitiationPoints;
            intFreestyleBP += intInitiationPoints;

            // Add the Karma cost of any Critter Powers.
            foreach (CritterPower objPower in CharacterObject.CritterPowers)
            {
                intKarmaPointsRemain -= objPower.Karma;
            }

            CharacterObject.Karma = intKarmaPointsRemain;

            if (blnDoUIUpdate)
            {
                string strContactPoints = CharacterObject.ContactPointsUsed.ToString();
                if (CharacterObject.FriendsInHighPlaces)
                {
                    strContactPoints += '/' + Math.Max(0, (CharacterObject.CHA.Value * 4) - intHighPlacesFriends).ToString();
                }
                strContactPoints += LanguageManager.GetString("String_Of", GlobalOptions.Language) + intContactPoints;
                if (CharacterObject.FriendsInHighPlaces)
                {
                    strContactPoints += '/' + (CharacterObject.CHA.Value * 4).ToString();
                }
                if (intPointsInContacts > 0 || (CharacterObject.CHA.Value * 4 < intHighPlacesFriends))
                {
                    strContactPoints += strSpaceCharacter + '(' + intPointsInContacts + strSpaceCharacter + strPoints + ')';
                }

                lblContactsBP.Text = strContactPoints;
                lblContactPoints.Text = strContactPoints;
                lblEnemiesBP.Text = intEnemyPoints.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblPositiveQualitiesBP.Text = unlimitedPositive > 0
                   ? $"{intPositiveQualities}/{CharacterObject.GameplayOptionQualityLimit}{strSpaceCharacter}{strPoints}{strSpaceCharacter}({intPositiveQualities + unlimitedPositive})"
                   : $"{intPositiveQualities}/{CharacterObject.GameplayOptionQualityLimit}{strSpaceCharacter}{strPoints}";

                lblNegativeQualitiesBP.Text = unlimitedNegative > 0
                    ? $"{intNegativeQualities * -1}/{CharacterObject.GameplayOptionQualityLimit}{strSpaceCharacter}{strPoints}{strSpaceCharacter}({intNegativeQualities + unlimitedNegative})"
                    : $"{intNegativeQualities * -1}/{CharacterObject.GameplayOptionQualityLimit}{strSpaceCharacter}{strPoints}";

                lblAttributesBP.Text = BuildAttributes(CharacterObject.AttributeSection.AttributeList);
                lblPBuildSpecial.Text = BuildAttributes(CharacterObject.AttributeSection.SpecialAttributeList, null, true);

                tabSkillUc.MissingDatabindingsWorkaround();

                lblMartialArtsBP.Text = intMartialArtsPoints.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildMartialArts, strMartialArtsBPToolTip.ToString());

                lblNuyenBP.Text = intNuyenBP.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblFociBP.Text = intFociPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildFoci, strFociPointsTooltip.ToString());

                lblSpiritsBP.Text = intSpiritPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblSpritesBP.Text = intSpritePointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                string strComplexFormsBP;
                if (CharacterObject.CFPLimit > 0)
                {
                    strComplexFormsBP = $"{intFormsPointsUsed.ToString(GlobalOptions.CultureInfo)}{LanguageManager.GetString("String_Of", GlobalOptions.Language)}{CharacterObject.CFPLimit.ToString(GlobalOptions.CultureInfo)}";
                    if (intFormsPointsUsed > CharacterObject.CFPLimit)
                    {
                        strComplexFormsBP += ':' + strSpaceCharacter + ((intFormsPointsUsed - CharacterObject.CFPLimit) * CharacterObject.ComplexFormKarmaCost).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                    }
                }
                else
                {
                    strComplexFormsBP = ((intFormsPointsUsed - CharacterObject.CFPLimit) * CharacterObject.ComplexFormKarmaCost).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                }
                lblComplexFormsBP.Text = strComplexFormsBP;
                
                lblAINormalProgramsBP.Text = ((intAINormalProgramPointsUsed - CharacterObject.AINormalProgramLimit) * CharacterObject.AIProgramKarmaCost).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                lblAIAdvancedProgramsBP.Text = ((intAIAdvancedProgramPointsUsed - CharacterObject.AIAdvancedProgramLimit) * CharacterObject.AIAdvancedProgramKarmaCost).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblInitiationBP.Text = intInitiationPoints.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                // ------------------------------------------------------------------------------
                // Update the number of BP remaining in the StatusBar.
                tssBP.Text = CharacterObject.BuildKarma.ToString(GlobalOptions.CultureInfo);
                tssBPRemain.Text = intKarmaPointsRemain.ToString(GlobalOptions.CultureInfo);
                if (_blnFreestyle)
                {
                    tssBP.Text = Math.Max(intFreestyleBP, intFreestyleBPMin).ToString(GlobalOptions.CultureInfo);
                    if (intFreestyleBP < intFreestyleBPMin)
                        tssBP.ForeColor = Color.OrangeRed;
                    else
                        tssBP.ForeColor = SystemColors.ControlText;
                }
            }

            return intKarmaPointsRemain;
        }

        private void UpdateSkillRelatedInfo()
        {
            string karma = LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            string of = LanguageManager.GetString("String_Of", GlobalOptions.Language);
            string def = $"0 {karma}";
            //Update Skill Labels
            //Active skills
            string strTemp = def;
            int intActiveSkillPointsMaximum = CharacterObject.SkillsSection.SkillPointsMaximum;
            if (intActiveSkillPointsMaximum > 0)
            {
                strTemp = $"{CharacterObject.SkillsSection.SkillPoints}{of}{intActiveSkillPointsMaximum}";
            }
            int intActiveSkillsTotalCostKarma = CharacterObject.SkillsSection.Skills.TotalCostKarma();
            if (intActiveSkillsTotalCostKarma > 0)
            {
                if (strTemp != def)
                { strTemp += $": {intActiveSkillsTotalCostKarma} {karma}"; }
                else
                { strTemp = $"{intActiveSkillsTotalCostKarma} {karma}"; }

            }
            lblActiveSkillsBP.Text = strTemp;
            //Knowledge skills
            strTemp = def;
            int intKnowledgeSkillPointsMaximum = CharacterObject.SkillsSection.KnowledgeSkillPoints;
            if (intKnowledgeSkillPointsMaximum > 0)
            {
                strTemp = $"{CharacterObject.SkillsSection.KnowledgeSkillPointsRemain}{of}{intKnowledgeSkillPointsMaximum}";
            }
            int intKnowledgeSkillsTotalCostKarma = CharacterObject.SkillsSection.KnowledgeSkills.TotalCostKarma();
            if (intKnowledgeSkillsTotalCostKarma > 0)
            {
                if (strTemp != def)
                { strTemp += $": {intKnowledgeSkillsTotalCostKarma} {karma}"; }
                else
                { strTemp = $"{intKnowledgeSkillsTotalCostKarma} {karma}"; }
            }
            lblKnowledgeSkillsBP.Text = strTemp;
            //Groups
            strTemp = def;
            int intSkillGroupPointsMaximum = CharacterObject.SkillsSection.SkillGroupPointsMaximum;
            if (intSkillGroupPointsMaximum > 0)
            {
                strTemp = $"{CharacterObject.SkillsSection.SkillGroupPoints}{of}{intSkillGroupPointsMaximum}";
            }
            int intSkillGroupsTotalCostKarma = CharacterObject.SkillsSection.SkillGroups.TotalCostKarma();
            if (intSkillGroupsTotalCostKarma > 0)
            {
                if (strTemp != def)
                { strTemp += $": {intSkillGroupsTotalCostKarma} {karma}"; }
                else
                { strTemp = $"{intSkillGroupsTotalCostKarma} {karma}"; }

            }
            lblSkillGroupsBP.Text = strTemp;
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
            // TODO: Databind as much of this as possible
            if (_blnLoading || _blnSkipUpdate || !IsCharacterUpdateRequested)
                return;

            _blnSkipUpdate = true;

            // TODO: DataBind these wherever possible

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strModifiers = LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language);

            UpdateSkillRelatedInfo();
            
            // Update the Condition Monitor labels.
            bool blnIsAI = CharacterObject.IsAI;
            if (blnIsAI)
            {
                if (CharacterObject.HomeNode == null)
                {
                    lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherCoreCM", GlobalOptions.Language);
                    lblCMStunLabel.Text = string.Empty;
                    lblCMStun.Visible = false;
                    string strCM = $"8{strSpaceCharacter}+{strSpaceCharacter}({CharacterObject.DEP.DisplayAbbrev}/2)({(CharacterObject.DEP.TotalValue + 1) / 2})";

                    int intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                    if (intBonus != 0)
                        strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';

                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStun, string.Empty);
                }
                else
                {
                    lblCMStunLabel.Text = LanguageManager.GetString("Label_OtherMatrixCM", GlobalOptions.Language);
                    lblCMStun.Visible = true;

                    string strCM = $"8{strSpaceCharacter }+{strSpaceCharacter}({LanguageManager.GetString("String_DeviceRating", GlobalOptions.Language)}/2)({(CharacterObject.HomeNode.GetTotalMatrixAttribute("Device Rating") + 1) / 2})";

                    int intBonus = CharacterObject.HomeNode.TotalBonusMatrixBoxes;
                    if (intBonus != 0)
                        strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';

                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);

                    if (CharacterObject.HomeNode is Vehicle objVehicleHomeNode)
                    {
                        lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherPhysicalCM", GlobalOptions.Language);
                        strCM = $"{objVehicleHomeNode.BasePhysicalBoxes}{strSpaceCharacter}+{strSpaceCharacter}({CharacterObject.BOD.DisplayAbbrev}/2)({(objVehicleHomeNode.TotalBody + 1) / 2})";

                        intBonus = objVehicleHomeNode.Mods.Sum(objMod => objMod.ConditionMonitor);
                        if (intBonus != 0)
                            strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';

                        GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    }
                    else
                    {
                        lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherCoreCM", GlobalOptions.Language);
                        strCM = $"8{strSpaceCharacter}+{strSpaceCharacter}({CharacterObject.DEP.DisplayAbbrev}/2)({(CharacterObject.DEP.TotalValue + 1) / 2})";

                        intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                        if (intBonus != 0)
                            strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';

                        GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);
                    }
                }
            }
            else
            {
                lblCMPhysicalLabel.Text = LanguageManager.GetString("Label_OtherPhysicalCM", GlobalOptions.Language);
                lblCMStunLabel.Text = LanguageManager.GetString("Label_OtherStunCM", GlobalOptions.Language);
                lblCMStun.Visible = true;

                string strCM = $"8{strSpaceCharacter}+{strSpaceCharacter}({CharacterObject.BOD.DisplayAbbrev}/2)({(CharacterObject.BOD.TotalValue + 1) / 2})";

                int intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.PhysicalCM);
                if (intBonus != 0)
                    strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysical, strCM);

                strCM = $"8{strSpaceCharacter}+{strSpaceCharacter}({CharacterObject.WIL.DisplayAbbrev}/2)({(CharacterObject.WIL.TotalValue + 1) / 2})";
                intBonus = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.StunCM);
                if (intBonus != 0)
                    strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStun, strCM);
            }
            
            int intINTAttributeModifiers = CharacterObject.INT.AttributeModifiers;
            int intREAAttributeModifiers = CharacterObject.REA.AttributeModifiers;
            
            // Initiative.
            lblINI.Text = CharacterObject.Initiative;
            string strInitText = LanguageManager.GetString("String_Initiative", GlobalOptions.Language);
            string strMatrixInitText = LanguageManager.GetString("String_MatrixInitiativeLong", GlobalOptions.Language);
            string strInit = $"{CharacterObject.REA.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.REA.Value}){strSpaceCharacter}+{strSpaceCharacter}{CharacterObject.INT.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.INT.Value})";
            if (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Initiative) > 0 || intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter + '(' + (ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Initiative) + intINTAttributeModifiers + intREAAttributeModifiers) + ')';
            GlobalOptions.ToolTipProcessor.SetToolTip(lblINI, strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString()));

            // Astral Initiative.
            lblAstralINI.Text = CharacterObject.AstralInitiative;
            if (CharacterObject.MAGEnabled)
            {
                strInit = $"{CharacterObject.INT.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.INT.Value}){strSpaceCharacter}×{strSpaceCharacter}2";
                if (intINTAttributeModifiers > 0)
                    strInit += $"{strModifiers}{strSpaceCharacter}({intINTAttributeModifiers})";
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINI, strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.AstralInitiativeDice.ToString()));
            }
            else
                GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINI, string.Empty);

            // Matrix Initiative (AR).
            lblMatrixINI.Text = CharacterObject.MatrixInitiative;
            strInit = $"{CharacterObject.REA.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.REA.Value}){strSpaceCharacter}+{strSpaceCharacter}{CharacterObject.INT.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.INT.Value})";
            if (intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += $"{strModifiers}{strSpaceCharacter}({intREAAttributeModifiers + intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINI, strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString(GlobalOptions.CultureInfo)));

            // Matrix Initiative (Cold).
            lblMatrixINICold.Text = CharacterObject.MatrixInitiativeCold;
            strInit = strMatrixInitText.Replace("{0}", CharacterObject.INT.Value.ToString(GlobalOptions.CultureInfo)).Replace("{1}", CharacterObject.MatrixInitiativeColdDice.ToString(GlobalOptions.CultureInfo));
            if (intINTAttributeModifiers > 0)
                strInit += $"{strModifiers}{strSpaceCharacter}({intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINICold, strInit);

            // Matrix Initiative (Hot).
            lblMatrixINIHot.Text = CharacterObject.MatrixInitiativeHot;
            strInit = strMatrixInitText.Replace("{0}", CharacterObject.INT.Value.ToString(GlobalOptions.CultureInfo)).Replace("{1}", CharacterObject.MatrixInitiativeHotDice.ToString(GlobalOptions.CultureInfo));
            if (intINTAttributeModifiers > 0)
                strInit += $"{strModifiers}{strSpaceCharacter}({intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINIHot, strInit);

            // Rigger Initiative.
            lblRiggingINI.Text = CharacterObject.Initiative;
            strInit = $"{CharacterObject.REA.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.REA.Value}){strSpaceCharacter}+{strSpaceCharacter}{CharacterObject.INT.DisplayAbbrev}{strSpaceCharacter}({CharacterObject.INT.Value})";
            if (intINTAttributeModifiers > 0 || intREAAttributeModifiers > 0)
                strInit += $"{strModifiers}{strSpaceCharacter}({intREAAttributeModifiers + intINTAttributeModifiers})";
            GlobalOptions.ToolTipProcessor.SetToolTip(lblRiggingINI, strInitText.Replace("{0}", strInit).Replace("{1}", CharacterObject.InitiativeDice.ToString()));
            
            // Calculate the number of Build Points remaining.
            CalculateBP();
            CalculateNuyen();
            if ((CharacterObject.Metatype == "Free Spirit" && !CharacterObject.IsCritter) || CharacterObject.MetatypeCategory.EndsWith("Spirits"))
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

            // If the Viewer window is open for this character, call its RefreshView method which updates it asynchronously
            PrintWindow?.RefreshCharacters();
            if (Program.MainForm.PrintMultipleCharactersForm?.CharacterList?.Contains(CharacterObject) == true)
                Program.MainForm.PrintMultipleCharactersForm.PrintViewForm?.RefreshCharacters();

            UpdateInitiationCost();
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
        /// Calculate the amount of Nuyen the character has remaining.
        /// </summary>
        private decimal CalculateNuyen()
        {
            decimal decDeductions = 0;

            // Cyberware/Bioware cost.
            foreach (Cyberware objCyberware in CharacterObject.Cyberware)
                decDeductions += objCyberware.TotalCost;

            // Initiation Grade cost.
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.Schooling)
                    decDeductions += 10000;
            }

            // Armor cost.
            foreach (Armor objArmor in CharacterObject.Armor)
                decDeductions += objArmor.TotalCost;

            // Weapon cost.
            foreach (Weapon objWeapon in CharacterObject.Weapons)
                decDeductions += objWeapon.TotalCost;

            // Gear cost.
            foreach (Gear objGear in CharacterObject.Gear)
                decDeductions += objGear.TotalCost;

            // Lifestyle cost.
            foreach (Lifestyle objLifestyle in CharacterObject.Lifestyles)
                decDeductions += objLifestyle.TotalCost;

            // Vehicle cost.
            foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                decDeductions += objVehicle.TotalCost;

            return CharacterObject.Nuyen = CharacterObject.TotalStartingNuyen - decDeductions;
        }

        /// <summary>
        /// Refresh the information for the currently displayed piece of Cyberware.
        /// </summary>
        public void RefreshSelectedCyberware()
        {
            _blnSkipRefresh = true;
            lblCyberDeviceRating.Visible = false;
            lblCyberAttack.Visible = false;
            lblCyberSleaze.Visible = false;
            lblCyberDataProcessing.Visible = false;
            lblCyberFirewall.Visible = false;
            lblCyberDeviceRatingLabel.Visible = false;
            lblCyberAttackLabel.Visible = false;
            lblCyberSleazeLabel.Visible = false;
            lblCyberDataProcessingLabel.Visible = false;
            lblCyberFirewallLabel.Visible = false;
            chkPrototypeTranshuman.Enabled = false;
            cmdDeleteCyberware.Enabled = treCyberware.SelectedNode != null && treCyberware.SelectedNode.Level != 0;
            cmdCyberwareChangeMount.Visible = false;

            if (treCyberware.SelectedNode == null || treCyberware.SelectedNode.Level == 0)
            {
                nudCyberwareRating.Enabled = false;
                lblCyberwareGradeLabel.Visible = false;
                cboCyberwareGrade.Visible = false;
                lblCyberwareName.Text = string.Empty;
                lblCyberwareCategory.Text = string.Empty;
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
            Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode.Tag.ToString());
            if (objCyberware != null)
            {
                if (!string.IsNullOrEmpty(objCyberware.ParentID))
                    cmdDeleteCyberware.Enabled = false;
                cmdCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                lblCyberwareName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblCyberwareCategory.Text = objCyberware.DisplayCategory(GlobalOptions.Language);
                string strPage = objCyberware.Page(GlobalOptions.Language);
                lblCyberwareSource.Text = CommonFunctions.LanguageBookShort(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblCyberwareSource, CommonFunctions.LanguageBookLong(objCyberware.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                // Enable and set the Rating values as needed.
                if (objCyberware.MaxRating == 0)
                {
                    nudCyberwareRating.Maximum = 0;
                    nudCyberwareRating.Minimum = 0;
                    nudCyberwareRating.Value = 0;
                    nudCyberwareRating.Enabled = false;
                }
                else
                {
                    nudCyberwareRating.Maximum = Convert.ToDecimal(objCyberware.MaxRating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Minimum = Convert.ToDecimal(objCyberware.MinRating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Value = Convert.ToDecimal(objCyberware.Rating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Enabled = true;
                }

                bool blnIgnoreSecondHand = objCyberware.GetNode()?["nosecondhand"] != null;

                // Cyberware Grade is not available for Genetech items.
                // Cyberware Grade is only available on root-level items (sub-components cannot have a different Grade than the piece they belong to).
                if (objCyberware.Parent == null && !objCyberware.Suite && string.IsNullOrWhiteSpace(objCyberware.ForceGrade))
                    cboCyberwareGrade.Enabled = true;
                else
                    cboCyberwareGrade.Enabled = false;

                PopulateCyberwareGradeList(objCyberware.SourceType == Improvement.ImprovementSource.Bioware, blnIgnoreSecondHand, cboCyberwareGrade.Enabled ? string.Empty : objCyberware.Grade.Name);
                lblCyberwareGradeLabel.Visible = true;
                cboCyberwareGrade.Visible = true;

                cboCyberwareGrade.SelectedValue = objCyberware.Grade.Name;
                if (cboCyberwareGrade.SelectedIndex == -1 && cboCyberwareGrade.Items.Count > 0)
                    cboCyberwareGrade.SelectedIndex = 0;
                if (objCyberware.Category.Equals("Cyberlimb") || objCyberware.AllowedSubsystems.Contains("Cyberlimb"))
                {
                    lblCyberlimbAGI.Visible = true;
                    lblCyberlimbAGILabel.Visible = true;
                    lblCyberlimbSTR.Visible = true;
                    lblCyberlimbSTRLabel.Visible = true;

                    lblCyberlimbAGILabel.Text = CharacterObject.AGI.DisplayAbbrev + ":";
                    lblCyberlimbSTRLabel.Text = CharacterObject.STR.DisplayAbbrev + ":";
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
                    lblCyberDeviceRating.Text = objCyberware.GetTotalMatrixAttribute("Device Rating").ToString();
                    lblCyberAttack.Text = objCyberware.GetTotalMatrixAttribute("Attack").ToString();
                    lblCyberSleaze.Text = objCyberware.GetTotalMatrixAttribute("Sleaze").ToString();
                    lblCyberDataProcessing.Text = objCyberware.GetTotalMatrixAttribute("Data Processing").ToString();
                    lblCyberFirewall.Text = objCyberware.GetTotalMatrixAttribute("Firewall").ToString();

                    chkCyberwareActiveCommlink.Visible = objCyberware.IsCommlink;
                    chkCyberwareActiveCommlink.Checked = objCyberware.IsActiveCommlink(CharacterObject);
                    if (CharacterObject.Metatype == "A.I.")
                    {
                        chkCyberwareHomeNode.Visible = true;
                        chkCyberwareHomeNode.Checked = objCyberware.IsHomeNode(CharacterObject);
                        chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible && objCyberware.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objCyberware.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                    }

                    lblCyberDeviceRating.Visible = true;
                    lblCyberAttack.Visible = true;
                    lblCyberSleaze.Visible = true;
                    lblCyberDataProcessing.Visible = true;
                    lblCyberFirewall.Visible = true;
                    lblCyberDeviceRatingLabel.Visible = true;
                    lblCyberAttackLabel.Visible = true;
                    lblCyberSleazeLabel.Visible = true;
                    lblCyberDataProcessingLabel.Visible = true;
                    lblCyberFirewallLabel.Visible = true;
                }
                else
                    chkPrototypeTranshuman.Enabled = objCyberware.Parent == null;

                chkPrototypeTranshuman.Checked = objCyberware.PrototypeTranshuman;

                lblCyberwareAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblCyberwareCost.Text = objCyberware.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblCyberwareCapacity.Text =
                    $"{objCyberware.CalculatedCapacity} ({objCyberware.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo)} {LanguageManager.GetString("String_Remaining", GlobalOptions.Language)})";
                if (objCyberware.Parent == null)
                    lblCyberwareEssence.Text = objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else if (objCyberware.AddToParentESS)
                    lblCyberwareEssence.Text = '+' + objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else
                    lblCyberwareEssence.Text = (0.0m).ToString(strESSFormat, GlobalOptions.CultureInfo);
                treCyberware.SelectedNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
            }
            else
            {
                // Locate the piece of Gear.
                Gear objGear = CharacterObject.Cyberware.FindCyberwareGear(treCyberware.SelectedNode.Tag.ToString());
                if (objGear != null)
                {
                    if (objGear.IncludedInParent)
                        cmdDeleteCyberware.Enabled = false;
                    lblCyberwareName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                    lblCyberwareCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                    lblCyberwareAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    lblCyberwareCost.Text =
                        objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    lblCyberwareCapacity.Text = objGear.CalculatedCapacity + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
                                                objGear.CapacityRemaining.ToString("#,0.##",
                                                    GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                                LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                    lblCyberwareEssence.Text = (0.0m).ToString(strESSFormat, GlobalOptions.CultureInfo);
                    lblCyberwareGradeLabel.Visible = false;
                    cboCyberwareGrade.Visible = false;
                    string strPage = objGear.DisplayPage(GlobalOptions.Language);
                    lblCyberwareSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblCyberwareSource, CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                    lblCyberDeviceRating.Text = objGear.GetTotalMatrixAttribute("Device Rating").ToString();
                    lblCyberAttack.Text = objGear.GetTotalMatrixAttribute("Attack").ToString();
                    lblCyberSleaze.Text = objGear.GetTotalMatrixAttribute("Sleaze").ToString();
                    lblCyberDataProcessing.Text = objGear.GetTotalMatrixAttribute("Data Processing").ToString();
                    lblCyberFirewall.Text = objGear.GetTotalMatrixAttribute("Firewall").ToString();

                    lblCyberDeviceRating.Visible = true;
                    lblCyberAttack.Visible = true;
                    lblCyberSleaze.Visible = true;
                    lblCyberDataProcessing.Visible = true;
                    lblCyberFirewall.Visible = true;
                    lblCyberDeviceRatingLabel.Visible = true;
                    lblCyberAttackLabel.Visible = true;
                    lblCyberSleazeLabel.Visible = true;
                    lblCyberDataProcessingLabel.Visible = true;
                    lblCyberFirewallLabel.Visible = true;

                    chkCyberwareActiveCommlink.Visible = objGear.IsCommlink;
                    chkCyberwareActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                    if (CharacterObject.Metatype == "A.I.")
                    {
                        chkCyberwareHomeNode.Visible = true;
                        chkCyberwareHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                        chkCyberwareHomeNode.Enabled = chkCyberwareActiveCommlink.Visible && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objGear.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                    }

                    if (objGear.MaxRating > 0)
                    {
                        if (objGear.MinRating > 0)
                            nudCyberwareRating.Minimum = objGear.MinRating;
                        else if (objGear.MinRating == 0 && objGear.Name.Contains("Credstick,"))
                            nudCyberwareRating.Minimum = 0;
                        else
                            nudCyberwareRating.Minimum = 1;
                        nudCyberwareRating.Maximum = objGear.MaxRating;
                        nudCyberwareRating.Value = objGear.Rating;
                        nudCyberwareRating.Enabled = nudCyberwareRating.Minimum != nudCyberwareRating.Maximum;
                    }
                    else
                    {
                        nudCyberwareRating.Minimum = 0;
                        nudCyberwareRating.Maximum = 0;
                        nudCyberwareRating.Enabled = false;
                    }
                    treCyberware.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                }
            }
            _blnSkipRefresh = false;
        }
        
        /// <summary>
        /// Refresh the information for the currently displayed Weapon.
        /// </summary>
        public void RefreshSelectedWeapon()
        {
            _blnSkipRefresh = true;
            string strSelectedId = treWeapons.SelectedNode?.Tag.ToString();
            cmdDeleteWeapon.Enabled = !string.IsNullOrEmpty(strSelectedId) && strSelectedId != "Node_SelectedWeapons";

            if (treWeapons.SelectedNode == null || treWeapons.SelectedNode.Level == 0)
            {
                lblWeaponName.Text = string.Empty;
                lblWeaponCategory.Text = string.Empty;
                lblWeaponAvail.Text = string.Empty;
                lblWeaponCost.Text = string.Empty;
                lblWeaponAccuracy.Text = string.Empty;
                lblWeaponConceal.Text = string.Empty;
                lblWeaponDamage.Text = string.Empty;
                lblWeaponRC.Text = string.Empty;
                lblWeaponAP.Text = string.Empty;
                lblWeaponReach.Text = string.Empty;
                lblWeaponMode.Text = string.Empty;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponRating.Text = string.Empty;
                lblWeaponSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, null);
                chkWeaponAccessoryInstalled.Enabled = false;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = false;

                lblWeaponDeviceRatingLabel.Visible = false;
                lblWeaponDeviceRating.Visible = false;
                lblWeaponAttackLabel.Visible = false;
                lblWeaponAttack.Visible = false;
                lblWeaponSleazeLabel.Visible = false;
                lblWeaponSleaze.Visible = false;
                lblWeaponDataProcessingLabel.Visible = false;
                lblWeaponDataProcessing.Visible = false;
                lblWeaponFirewallLabel.Visible = false;
                lblWeaponFirewall.Visible = false;

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

                _blnSkipRefresh = false;
                return;
            }

            Weapon objWeapon = CharacterObject.Weapons.DeepFindById(strSelectedId);
            if (objWeapon != null)
            {
                if (objWeapon.IncludedInWeapon || objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || !string.IsNullOrEmpty(objWeapon.ParentID))
                    cmdDeleteWeapon.Enabled = false;

                lblWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                string strPage = objWeapon.DisplayPage(GlobalOptions.Language);
                lblWeaponSource.Text = CommonFunctions.LanguageBookShort(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, CommonFunctions.LanguageBookLong(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                chkWeaponAccessoryInstalled.Enabled = objWeapon.Parent != null;
                chkWeaponAccessoryInstalled.Checked = objWeapon.Installed;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

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

                lblWeaponDeviceRatingLabel.Visible = true;
                lblWeaponDeviceRating.Visible = true;
                lblWeaponAttackLabel.Visible = true;
                lblWeaponAttack.Visible = true;
                lblWeaponSleazeLabel.Visible = true;
                lblWeaponSleaze.Visible = true;
                lblWeaponDataProcessingLabel.Visible = true;
                lblWeaponDataProcessing.Visible = true;
                lblWeaponFirewallLabel.Visible = true;
                lblWeaponFirewall.Visible = true;
                lblWeaponDeviceRating.Text = objWeapon.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponAttack.Text = objWeapon.GetTotalMatrixAttribute("Attack").ToString();
                lblWeaponSleaze.Text = objWeapon.GetTotalMatrixAttribute("Sleaze").ToString();
                lblWeaponDataProcessing.Text = objWeapon.GetTotalMatrixAttribute("Data Processing").ToString();
                lblWeaponFirewall.Text = objWeapon.GetTotalMatrixAttribute("Firewall").ToString();
            }
            else
            {
                lblWeaponDicePool.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponDicePool, string.Empty);

                WeaponAccessory objSelectedAccessory = CharacterObject.Weapons.FindWeaponAccessory(strSelectedId);
                if (objSelectedAccessory != null)
                {
                    if (objSelectedAccessory.IncludedInWeapon)
                        cmdDeleteWeapon.Enabled = false;
                    objWeapon = objSelectedAccessory.Parent;
                    lblWeaponName.Text = objSelectedAccessory.DisplayNameShort(GlobalOptions.Language);
                    lblWeaponCategory.Text = LanguageManager.GetString("String_WeaponAccessory", GlobalOptions.Language);
                    lblWeaponAvail.Text = objSelectedAccessory.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    lblWeaponCost.Text = objSelectedAccessory.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    lblWeaponConceal.Text = objSelectedAccessory.TotalConcealability.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                    lblWeaponDamage.Text = string.Empty;
                    lblWeaponAccuracy.Text = objSelectedAccessory.Accuracy.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                    lblWeaponRC.Text = objSelectedAccessory.RC;
                    lblWeaponAP.Text = string.Empty;
                    lblWeaponReach.Text = string.Empty;
                    lblWeaponMode.Text = string.Empty;
                    lblWeaponAmmo.Text = string.Empty;
                    lblWeaponRating.Text = objSelectedAccessory.Rating.ToString();

                    StringBuilder strSlotsText = new StringBuilder(objSelectedAccessory.Mount);
                    if (strSlotsText.Length > 0 && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        strSlotsText.Clear();
                        foreach (string strMount in objSelectedAccessory.Mount.Split('/'))
                        {
                            strSlotsText.Append(LanguageManager.GetString("String_Mount" + strMount, GlobalOptions.Language));
                            strSlotsText.Append('/');
                        }
                        strSlotsText.Length -= 1;
                    }

                    if (!string.IsNullOrEmpty(objSelectedAccessory.ExtraMount) && (objSelectedAccessory.ExtraMount != "None"))
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
                                strSlotsText.Append(LanguageManager.GetString("String_Mount" + strCurrentExtraMount, GlobalOptions.Language));
                                strSlotsText.Append('/');
                            }
                        }
                        // Remove the trailing /
                        if (boolHaveAddedItem)
                            strSlotsText.Length -= 1;
                    }

                    lblWeaponSlots.Text = strSlotsText.ToString();
                    string strPage = objSelectedAccessory.Page(GlobalOptions.Language);
                    lblWeaponSource.Text = CommonFunctions.LanguageBookShort(objSelectedAccessory.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, CommonFunctions.LanguageBookLong(objSelectedAccessory.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                    chkWeaponAccessoryInstalled.Enabled = true;
                    chkWeaponAccessoryInstalled.Checked = objSelectedAccessory.Installed;
                    chkIncludedInWeapon.Enabled = CharacterObjectOptions.AllowEditPartOfBaseWeapon;
                    chkIncludedInWeapon.Checked = objSelectedAccessory.IncludedInWeapon;

                    lblWeaponDeviceRatingLabel.Visible = false;
                    lblWeaponDeviceRating.Visible = false;
                    lblWeaponAttackLabel.Visible = false;
                    lblWeaponAttack.Visible = false;
                    lblWeaponSleazeLabel.Visible = false;
                    lblWeaponSleaze.Visible = false;
                    lblWeaponDataProcessingLabel.Visible = false;
                    lblWeaponDataProcessing.Visible = false;
                    lblWeaponFirewallLabel.Visible = false;
                    lblWeaponFirewall.Visible = false;
                }
                else
                {
                    // Find the selected Gear.
                    Gear objGear = CharacterObject.Weapons.FindWeaponGear(strSelectedId, out objSelectedAccessory);
                    if (objGear != null)
                    {
                        if (objGear.IncludedInParent)
                            cmdDeleteWeapon.Enabled = false;
                        objWeapon = objSelectedAccessory.Parent;

                        lblWeaponName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                        lblWeaponCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                        lblWeaponAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        lblWeaponCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                        lblWeaponConceal.Text = string.Empty;
                        lblWeaponDamage.Text = string.Empty;
                        lblWeaponRC.Text = string.Empty;
                        lblWeaponAP.Text = string.Empty;
                        lblWeaponAccuracy.Text = string.Empty;
                        lblWeaponReach.Text = string.Empty;
                        lblWeaponMode.Text = string.Empty;
                        lblWeaponAmmo.Text = string.Empty;
                        lblWeaponSlots.Text = string.Empty;
                        lblWeaponRating.Text = string.Empty;
                        string strPage = objGear.DisplayPage(GlobalOptions.Language);
                        lblWeaponSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblWeaponSource, CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                        chkWeaponAccessoryInstalled.Enabled = true;
                        chkWeaponAccessoryInstalled.Checked = objGear.Equipped;
                        chkIncludedInWeapon.Enabled = false;
                        chkIncludedInWeapon.Checked = false;

                        lblWeaponDeviceRatingLabel.Visible = true;
                        lblWeaponDeviceRating.Visible = true;
                        lblWeaponAttackLabel.Visible = true;
                        lblWeaponAttack.Visible = true;
                        lblWeaponSleazeLabel.Visible = true;
                        lblWeaponSleaze.Visible = true;
                        lblWeaponDataProcessingLabel.Visible = true;
                        lblWeaponDataProcessing.Visible = true;
                        lblWeaponFirewallLabel.Visible = true;
                        lblWeaponFirewall.Visible = true;
                        lblWeaponDeviceRating.Text = objGear.GetTotalMatrixAttribute("Device Rating").ToString();
                        lblWeaponAttack.Text = objGear.GetTotalMatrixAttribute("Attack").ToString();
                        lblWeaponSleaze.Text = objGear.GetTotalMatrixAttribute("Sleaze").ToString();
                        lblWeaponDataProcessing.Text = objGear.GetTotalMatrixAttribute("Data Processing").ToString();
                        lblWeaponFirewall.Text = objGear.GetTotalMatrixAttribute("Firewall").ToString();
                    }
                }
            }
            
            if (objWeapon != null)
            {
                // Show the Weapon Ranges.
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
            }

            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Refresh the information for the currently displayed Armor.
        /// </summary>
        public void RefreshSelectedArmor()
        {
            _blnSkipRefresh = true;
            string strSelectedId = treArmor.SelectedNode?.Tag.ToString();
            bool blnNoneSelected = string.IsNullOrEmpty(strSelectedId);
            cmdDeleteArmor.Enabled = !blnNoneSelected && strSelectedId != "Node_SelectedArmor";

            if (blnNoneSelected || treArmor.SelectedNode.Level == 0)
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

                if (blnNoneSelected)
                {
                    cmdArmorEquipAll.Visible = false;
                    cmdArmorUnEquipAll.Visible = false;
                    lblArmorEquippedLabel.Visible = false;
                    lblArmorEquipped.Visible = false;
                }
                else
                {
                    cmdArmorEquipAll.Visible = true;
                    cmdArmorUnEquipAll.Visible = true;
                    lblArmorEquippedLabel.Visible = true;
                    StringBuilder strArmorEquipped = new StringBuilder();
                    foreach (Armor objLoopArmor in CharacterObject.Armor)
                    {
                        if (objLoopArmor.Equipped && (objLoopArmor.Location == strSelectedId || string.IsNullOrEmpty(objLoopArmor.Location) && strSelectedId == "Node_SelectedArmor"))
                        {
                            strArmorEquipped.Append(objLoopArmor.DisplayName(GlobalOptions.Language));
                            strArmorEquipped.Append(LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(');
                            strArmorEquipped.Append(objLoopArmor.DisplayArmorValue);
                            strArmorEquipped.AppendLine(")");
                        }
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

                chkIncludedInArmor.Enabled = false;
                chkIncludedInArmor.Checked = false;
                lblArmorValue.Text = string.Empty;
                lblArmorAvail.Text = string.Empty;
                lblArmorCost.Text = string.Empty;
                lblArmorSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, null);
                chkArmorEquipped.Enabled = false;
                nudArmorRating.Enabled = false;
                _blnSkipRefresh = false;
                return;
            }

            lblArmorEquipped.Visible = false;
            cmdArmorEquipAll.Visible = false;
            cmdArmorUnEquipAll.Visible = false;
            lblArmorEquippedLabel.Visible = false;
            lblArmorEquipped.Visible = false;

            Armor objArmor = CharacterObject.Armor.FindById(strSelectedId);
            if (objArmor != null)
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
                lblArmorCapacity.Text = objArmor.CalculatedCapacity + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + objArmor.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                if (objArmor.MaxRating == 0)
                {
                    nudArmorRating.Enabled = false;
                }
                else
                {
                    nudArmorRating.Maximum = objArmor.MaxRating;
                    nudArmorRating.Value = objArmor.Rating;
                    nudArmorRating.Enabled = true;
                }
                lblArmorCost.Text = objArmor.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                string strPage = objArmor.Page(GlobalOptions.Language);
                lblArmorSource.Text = CommonFunctions.LanguageBookShort(objArmor.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, CommonFunctions.LanguageBookLong(objArmor.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                chkArmorEquipped.Checked = objArmor.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Enabled = false;
                chkIncludedInArmor.Checked = false;
            }
            else
            {
                ArmorMod objArmorMod = CharacterObject.Armor.FindArmorMod(strSelectedId);
                if (objArmorMod != null)
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
                    lblArmorCapacity.Text = objArmor.CapacityDisplayStyle == CapacityStyle.Zero ? "[0]" : objArmorMod.CalculatedCapacity;
                    if (!string.IsNullOrEmpty(objArmorMod.GearCapacity))
                        lblArmorCapacity.Text = objArmorMod.GearCapacity + '/' + lblArmorCapacity.Text + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + objArmorMod.GearCapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                    lblArmorCost.Text = objArmorMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    string strPage = objArmorMod.DisplayPage(GlobalOptions.Language);
                    lblArmorSource.Text = CommonFunctions.LanguageBookShort(objArmorMod.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorSource, CommonFunctions.LanguageBookLong(objArmorMod.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                    chkArmorEquipped.Checked = objArmorMod.Equipped;
                    chkArmorEquipped.Enabled = true;
                    if (objArmorMod.MaximumRating > 1)
                    {
                        nudArmorRating.Maximum = objArmorMod.MaximumRating;
                        nudArmorRating.Enabled = true;
                        nudArmorRating.Value = objArmorMod.Rating;
                    }
                    else
                    {
                        nudArmorRating.Maximum = 1;
                        nudArmorRating.Enabled = false;
                        nudArmorRating.Value = 1;
                    }
                    chkIncludedInArmor.Enabled = true;
                    chkIncludedInArmor.Checked = objArmorMod.IncludedInArmor;
                }
                else
                {
                    Gear objSelectedGear = CharacterObject.Armor.FindArmorGear(strSelectedId, out objArmor, out objArmorMod);

                    if (objSelectedGear != null)
                    {
                        if (objSelectedGear.IncludedInParent)
                            cmdDeleteArmor.Enabled = false;
                        lblArmorValue.Text = string.Empty;
                        lblArmorAvail.Text = objSelectedGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);

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
                        if (objSelectedGear.MaxRating > 1)
                        {
                            nudArmorRating.Maximum = objSelectedGear.MaxRating;
                            nudArmorRating.Enabled = true;
                            nudArmorRating.Value = objSelectedGear.Rating;
                        }
                        else
                        {
                            nudArmorRating.Maximum = 1;
                            nudArmorRating.Enabled = false;
                            nudArmorRating.Value = 1;
                        }
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
                }
            }
            _blnSkipRefresh = false;
        }
        
        /// <summary>
        /// Refresh the information for the currently displayed Gear.
        /// </summary>
        public void RefreshSelectedGear()
        {
            _blnSkipRefresh = true;
            cmdDeleteGear.Enabled = treGear.SelectedNode != null && treGear.SelectedNode.Tag.ToString() != "Node_SelectedGear";
            if (treGear.SelectedNode == null || treGear.SelectedNode.Level == 0)
            {
                nudGearRating.Minimum = 0;
                nudGearRating.Maximum = 0;
                nudGearRating.Enabled = false;
                nudGearQty.Enabled = false;
                chkGearEquipped.Text = LanguageManager.GetString("Checkbox_Equipped", GlobalOptions.Language);
                chkGearEquipped.Visible = false;
                chkGearActiveCommlink.Enabled = false;
                chkGearActiveCommlink.Checked = false;
                _blnSkipRefresh = false;
                return;
            }
            chkGearHomeNode.Visible = false;

            if (treGear.SelectedNode.Level > 0)
            {
                Gear objGear = CharacterObject.Gear.DeepFindById(treGear.SelectedNode.Tag.ToString());

                if (objGear.IncludedInParent)
                    cmdDeleteGear.Enabled = false;
                lblGearName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblGearCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblGearAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                nudGearQty.Enabled = !objGear.IncludedInParent;
                nudGearQty.Increment = objGear.CostFor;
                try
                {
                    lblGearCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat + '¥', GlobalOptions.CultureInfo);
                }
                catch (FormatException)
                {
                    lblGearCost.Text = objGear.Cost + "¥";
                }
                lblGearCapacity.Text = objGear.CalculatedCapacity + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                string strPage = objGear.DisplayPage(GlobalOptions.Language);
                lblGearSource.Text = CommonFunctions.LanguageBookShort(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblGearSource, CommonFunctions.LanguageBookLong(objGear.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                objGear.RefreshMatrixAttributeCBOs(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);

                lblGearDeviceRating.Text = objGear.GetTotalMatrixAttribute("Device Rating").ToString();

                lblGearDeviceRating.Visible = true;
                lblGearDeviceRatingLabel.Visible = true;
                lblGearAttackLabel.Visible = true;
                lblGearSleazeLabel.Visible = true;
                lblGearDataProcessingLabel.Visible = true;
                lblGearFirewallLabel.Visible = true;

                chkGearActiveCommlink.Checked = objGear.IsActiveCommlink(CharacterObject);
                chkGearActiveCommlink.Enabled = objGear.IsCommlink;

                if (CharacterObject.Metatype == "A.I.")
                {
                    chkGearHomeNode.Visible = true;
                    chkGearHomeNode.Checked = objGear.IsHomeNode(CharacterObject);
                    chkGearHomeNode.Enabled = chkGearActiveCommlink.Enabled && objGear.GetTotalMatrixAttribute("Program Limit") >= (CharacterObject.DEP.TotalValue > objGear.GetTotalMatrixAttribute("Device Rating") ? 2 : 1);
                }

                if (objGear.MaxRating > 0)
                {
                    if (objGear.MinRating > 0)
                        nudGearRating.Minimum = objGear.MinRating;
                    else if (objGear.MinRating == 0 && objGear.Name.Contains("Credstick,"))
                        nudGearRating.Minimum = 0;
                    else
                        nudGearRating.Minimum = 1;
                    nudGearRating.Maximum = objGear.MaxRating;
                    nudGearRating.Value = objGear.Rating;
                    nudGearRating.Enabled = nudGearRating.Minimum != nudGearRating.Maximum;
                }
                else
                {
                    nudGearRating.Minimum = 0;
                    nudGearRating.Maximum = 0;
                    nudGearRating.Enabled = false;
                }

                //nudGearQty.Minimum = objGear.CostFor;
                nudGearQty.Increment = objGear.CostFor;
                if (objGear.Name.StartsWith("Nuyen"))
                {
                    int intDecimalPlaces = CharacterObjectOptions.NuyenDecimals;
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

                if (treGear.SelectedNode.Level == 1)
                {
                    //nudGearQty.Minimum = objGear.CostFor;
                    chkGearEquipped.Visible = true;
                    chkGearEquipped.Checked = objGear.Equipped;
                }
                else
                {
                    //nudGearQty.Enabled = false;
                    chkGearEquipped.Visible = true;
                    chkGearEquipped.Checked = objGear.Equipped;

                    // If this is a Program, determine if its parent Gear (if any) is a Commlink. If so, show the Equipped checkbox.
                    if (objGear.IsProgram && CharacterObjectOptions.CalculateCommlinkResponse)
                    {
                        if (objGear.Parent?.IsCommlink == true)
                        {
                            chkGearEquipped.Text = LanguageManager.GetString("Checkbox_SoftwareRunning", GlobalOptions.Language);
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

                treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            _blnSkipRefresh = false;
        }

        protected override string FormMode => LanguageManager.GetString("Title_CreateNewCharacter", GlobalOptions.Language);

        /// <summary>
        /// Save the Character.
        /// </summary>
        public override bool SaveCharacter(bool blnNeedConfirm = true, bool blnDoCreated = false)
        {
            return base.SaveCharacter(blnNeedConfirm, chkCharacterCreated.Checked);
        }

        /// <summary>
        /// Save the Character using the Save As dialogue box.
        /// </summary>
        public override bool SaveCharacterAs(bool blnDoCreated = false)
        {
            return base.SaveCharacterAs(chkCharacterCreated.Checked);
        }

        /// <summary>
        /// Save the character as Created and re-open it in Career Mode.
        /// </summary>
        public override void SaveCharacterAsCreated()
        {
            Cursor = Cursors.WaitCursor;
            // If the character was built with Karma, record their staring Karma amount (if any).
            if (CharacterObject.Karma > 0)
            {
                ExpenseLogEntry objKarma = new ExpenseLogEntry(CharacterObject);
                objKarma.Create(CharacterObject.Karma, "Starting Karma", ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objKarma);

                // Create an Undo entry so that the starting Karma amount can be modified if needed.
                ExpenseUndo objKarmaUndo = new ExpenseUndo();
                objKarmaUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
                objKarma.Undo = objKarmaUndo;
            }
            if (CharacterObject.MetatypeCategory == "Shapeshifter")
            {
                List<CharacterAttrib> staging = new List<CharacterAttrib>();
                XmlDocument xmlDoc = XmlManager.Load("metatypes.xml");
                string s = $"/chummer/metatypes/metatype[name = \"{CharacterObject.Metatype}\"]";
                foreach (CharacterAttrib att in CharacterObject.AttributeSection.AttributeList)
                {
                    CharacterAttrib newAtt = new CharacterAttrib(CharacterObject, att.Abbrev,
                        CharacterAttrib.AttributeCategory.Shapeshifter);
                    AttributeSection.CopyAttribute(att, newAtt, s, xmlDoc);
                    staging.Add(newAtt);
                }
                foreach (CharacterAttrib att in staging)
                {
                    CharacterObject.AttributeSection.AttributeList.Add(att);
                }
            }

            // Create an Expense Entry for Starting Nuyen.
            ExpenseLogEntry objNuyen = new ExpenseLogEntry(CharacterObject);
            objNuyen.Create(CharacterObject.Nuyen, "Starting Nuyen", ExpenseType.Nuyen, DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objNuyen);

            // Create an Undo entry so that the Starting Nuyen amount can be modified if needed.
            ExpenseUndo objNuyenUndo = new ExpenseUndo();
            objNuyenUndo.CreateNuyen(NuyenExpenseType.ManualAdd, string.Empty);
            objNuyen.Undo = objNuyenUndo;

            _blnSkipToolStripRevert = true;
            if (CharacterObject.Save())
            {
                IsDirty = false;
                Character objOpenCharacter = Program.MainForm.LoadCharacter(CharacterObject.FileName);
                Cursor = Cursors.Default;
                Program.MainForm.OpenCharacter(objOpenCharacter);
                Close();
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

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
                            decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
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
                            decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
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
                            decMultiplier -= (1.0m - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
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
                }

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
            XmlNode objXmlCyberware = objSource == Improvement.ImprovementSource.Bioware
                ? XmlManager.Load("bioware.xml").SelectSingleNode("/chummer/biowares/bioware[id = \"" + frmPickCyberware.SelectedCyberware + "\"]")
                : XmlManager.Load("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + frmPickCyberware.SelectedCyberware + "\"]");

            // Create the Cyberware object.
            Cyberware objCyberware = new Cyberware(CharacterObject);

            List<Weapon> lstWeapons = new List<Weapon>();
            List<Vehicle> lstVehicles = new List<Vehicle>();
            objCyberware.Create(objXmlCyberware, CharacterObject, frmPickCyberware.SelectedGrade, objSource, frmPickCyberware.SelectedRating, lstWeapons, lstVehicles, true, true, string.Empty, objSelectedCyberware);
            if (objCyberware.InternalId.IsEmptyGuid())
                return false;
            objCyberware.DiscountCost = frmPickCyberware.BlackMarketDiscount;
            objCyberware.PrototypeTranshuman = frmPickCyberware.PrototypeTranshuman;

            // Apply the ESS discount if applicable.
            if (CharacterObjectOptions.AllowCyberwareESSDiscounts)
                objCyberware.ESSDiscount = frmPickCyberware.SelectedESSDiscount;

            if (frmPickCyberware.FreeCost)
                objCyberware.Cost = "0";

            if (objSelectedCyberware != null)
                objSelectedCyberware.Children.Add(objCyberware);
            else
                CharacterObject.Cyberware.Add(objCyberware);

            CharacterObject.Weapons.AddRange(lstWeapons);
            CharacterObject.Vehicles.AddRange(lstVehicles);
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;
            
            return frmPickCyberware.AddAgain;
        }

        /// <summary>
        /// Select a piece of Gear to be added to the character.
        /// </summary>
        private bool PickGear(string strSelectedId)
        {
            bool blnNullParent = false;
            Gear objSelectedGear = CharacterObject.Gear.DeepFindById(strSelectedId);
            if (objSelectedGear == null)
            {
                objSelectedGear = new Gear(CharacterObject);
                blnNullParent = true;
            }

            // Open the Gear XML file and locate the selected Gear.
            XmlNode objXmlGear = blnNullParent ? null : objSelectedGear.GetNode();

            Cursor = Cursors.WaitCursor;

            string strCategories = string.Empty;

            if (!blnNullParent)
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
                    if (!string.IsNullOrEmpty(strCategories))
                        frmPickGear.ShowNegativeCapacityOnly = true;
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

            // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
            if (CharacterObject.ActiveCommlink == null && objGear.IsCommlink)
            {
                objGear.SetActiveCommlink(CharacterObject, true);
            }

            objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

            // reduce the cost for Black Market Pipeline
            objGear.DiscountCost = frmPickGear.BlackMarketDiscount;
            // Reduce the cost for Do It Yourself components.
            if (frmPickGear.DoItYourself)
                objGear.Cost = "(" + objGear.Cost + ") * 0.5";
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
            
            if (!blnNullParent)
            {
                objSelectedGear.Children.Add(objGear);
            }
            else
            {
                objGear.Location = strSelectedId;
                CharacterObject.Gear.Add(objGear);
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
            ArmorMod objSelectedMod = null;
            Armor objSelectedArmor = CharacterObject.Armor.FindById(strSelectedId);
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
                XmlNodeList xmlAddonCategoryList = objXmlParent?.SelectNodes("addoncategory");
                if (xmlAddonCategoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                        strCategories += objXmlCategory.InnerText + ",";
                    // Remove the trailing comma.
                    if (strCategories.Length > 0)
                        strCategories = strCategories.Substring(0, strCategories.Length - 1);
                }
            }

            frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objXmlParent, strCategories)
            {
                ShowArmorCapacityOnly = blnShowArmorCapacityOnly,
                CapacityDisplayStyle = objSelectedMod != null ? CapacityStyle.Standard : objSelectedArmor.CapacityDisplayStyle
            };
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // If the Gear has a Capacity with no brackets (meaning it grants Capacity), show only Subsystems (those that conume Capacity).
                if (objSelectedGear?.Capacity.Contains('[') == false)
                    frmPickGear.MaximumCapacity = objSelectedGear.CapacityRemaining;
                else if (objSelectedMod != null)
                    frmPickGear.MaximumCapacity = objSelectedMod.GearCapacityRemaining;
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
            objGear.DiscountCost = frmPickGear.BlackMarketDiscount;

            if (objSelectedGear != null)
                objGear.Parent = objSelectedGear;

            // Reduce the cost for Do It Yourself components.
            if (frmPickGear.DoItYourself)
                objGear.Cost = "(" + objGear.Cost + ") * 0.5";
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
            if (objGear.Category == "Ammunition")
            {
                foreach (Gear objCharacterGear in CharacterObject.Gear)
                {
                    if (objGear.IsIdenticalToOtherGear(objCharacterGear))
                    {
                        // A match was found, so increase the quantity instead.
                        objCharacterGear.Quantity += objGear.Quantity;
                        blnMatchFound = true;
                        break;
                    }
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
                    objSelectedMod.Gear.Add(objGear);
                }
                else
                {
                    objSelectedArmor.Gear.Add(objGear);
                }
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
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level <= 0)
            {
                cmdDeleteLifestyle.Enabled = false;
                lblLifestyleCost.Text = string.Empty;
                lblLifestyleTotalCost.Text = string.Empty;
                lblLifestyleSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblLifestyleSource, null);
                lblLifestyleQualities.Text = string.Empty;
                lblLifestyleCostLabel.Text = string.Empty;
                nudLifestyleMonths.Visible = false;
                lblLifestyleMonthsLabel.Text = string.Empty;
                _blnSkipRefresh = false;
                return;
            }

            // Locate the selected Lifestyle.
            Lifestyle objLifestyle = CharacterObject.Lifestyles.FindById(treLifestyles.SelectedNode.Tag.ToString());
            if (objLifestyle == null)
            {
                _blnSkipRefresh = false;
                return;
            }

            cmdDeleteLifestyle.Enabled = true;
            nudLifestyleMonths.Visible = true;

            lblLifestyleCost.Text = objLifestyle.TotalMonthlyCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            nudLifestyleMonths.Value = Convert.ToDecimal(objLifestyle.Increments, GlobalOptions.InvariantCultureInfo);
            lblLifestyleStartingNuyen.Text = objLifestyle.Dice + LanguageManager.GetString("String_D6", GlobalOptions.Language) + " × " + objLifestyle.Multiplier.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            string strPage = objLifestyle.DisplayPage(GlobalOptions.Language);
            lblLifestyleSource.Text = CommonFunctions.LanguageBookShort(objLifestyle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
            GlobalOptions.ToolTipProcessor.SetToolTip(lblLifestyleSource, CommonFunctions.LanguageBookLong(objLifestyle.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
            lblLifestyleTotalCost.Text = objLifestyle.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

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
            lblLifestyleTotalCost.Left = lblLifestyleMonthsLabel.Left + lblLifestyleMonthsLabel.Width + 6;

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

                if (strQualities.Length > 0)
                    strQualities += ", ";

                strQualities += string.Join(", ", objLifestyle.FreeGrids.Select(r => r.DisplayName(GlobalOptions.Language)));

                if (strQualities.EndsWith(", "))
                {
                    strQualities = strQualities.Substring(0, strQualities.Length - 2);
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
            nudVehicleGearQty.Enabled = false;

            chkVehicleHomeNode.Visible = false;
            chkVehicleActiveCommlink.Visible = false;
            lblVehicleSlotsLabel.Visible = false;
            lblVehicleSlots.Visible = false;

            if (string.IsNullOrEmpty(strSelectedId) || treVehicles.SelectedNode.Level <= 0 || !strSelectedId.IsGuid())
            {
                lblVehicleRatingLabel.Visible = false;
                nudVehicleRating.Minimum = 0;
                nudVehicleRating.Maximum = 0;
                nudVehicleRating.Enabled = false;
                nudVehicleRating.Visible = false;

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
            Vehicle objVehicle = CharacterObject.Vehicles.FindById(strSelectedId);
            if (objVehicle != null)
            {
                if (!string.IsNullOrEmpty(objVehicle.ParentID))
                    cmdDeleteVehicle.Enabled = false;
                lblVehicleRatingLabel.Visible = false;
                nudVehicleRating.Minimum = 0;
                nudVehicleRating.Maximum = 0;
                nudVehicleRating.Enabled = false;
                nudVehicleRating.Visible = false;

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

                nudVehicleGearQty.Visible = true;
                lblVehicleGearQtyLabel.Visible = true;

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
                    lblVehicleSlots.Text = objVehicle.Slots + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + (objVehicle.Slots - objVehicle.SlotsUsed) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                }
            }
            else
            {
                WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out objVehicle);
                if (objWeaponMount != null)
                {
                    lblVehicleRatingLabel.Visible = false;
                    nudVehicleRating.Minimum = 0;
                    nudVehicleRating.Maximum = 0;
                    nudVehicleRating.Enabled = false;
                    nudVehicleRating.Visible = false;

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
                    lblVehicleCost.Text = objWeaponMount.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo);

                    chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Installed;
                    chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                    chkVehicleIncludedInWeapon.Checked = false;

                    lblVehicleSlotsLabel.Visible = true;
                    lblVehicleSlots.Visible = true;
                    lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString();
                    
                    string strPage = objWeaponMount.Page(GlobalOptions.Language);
                    lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objWeaponMount.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                    GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objWeaponMount.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                }
                else
                {
                    // Locate the selected VehicleMod.
                    VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId, out objVehicle, out objWeaponMount);
                    if (objMod != null)
                    {
                        if (objMod.IncludedInVehicle)
                            cmdDeleteVehicle.Enabled = false;
                        if (objMod.MaxRating != "qty")
                        {
                            if (objMod.MaxRating == "Seats")
                            {
                                objMod.MaxRating = objVehicle.Seats.ToString();
                            }
                            if (objMod.MaxRating == "body")
                            {
                                objMod.MaxRating = objVehicle.Body.ToString();
                            }
                            if (Convert.ToInt32(objMod.MaxRating) > 0)
                            {
                                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                                lblVehicleRatingLabel.Visible = true;
                                // If the Mod is Armor, use the lower of the Mod's maximum Rating and MaxArmor value for the Vehicle instead.
                                nudVehicleRating.Maximum = objMod.Name.StartsWith("Armor,") ? Math.Min(Convert.ToInt32(objMod.MaxRating), objVehicle.MaxArmor) : Convert.ToInt32(objMod.MaxRating);
                                nudVehicleRating.Minimum = 1;
                                nudVehicleRating.Visible = true;
                                nudVehicleRating.Value = objMod.Rating;
                                nudVehicleRating.Increment = 1;
                                nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                            }
                            else
                            {
                                lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
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
                            lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                            lblVehicleRatingLabel.Visible = false;
                            nudVehicleRating.Visible = true;
                            nudVehicleRating.Minimum = 1;
                            nudVehicleRating.Maximum = 20;
                            nudVehicleRating.Value = objMod.Rating;
                            nudVehicleRating.Increment = 1;
                            nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                        }
                        DisplayVehicleStats(false);
                        DisplayVehicleWeaponStats(false);
                        DisplayVehicleCommlinkStats(false);

                        lblVehicleName.Text = objMod.DisplayNameShort(GlobalOptions.Language);
                        lblVehicleNameLabel.Visible = true;
                        lblVehicleCategoryLabel.Visible = true;
                        lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleModification", GlobalOptions.Language);
                        lblVehicleAvailLabel.Visible = true;
                        lblVehicleAvail.Text = objMod.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                        lblVehicleCostLabel.Visible = true;
                        lblVehicleCost.Text = objMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                        nudVehicleGearQty.Visible = true;
                        lblVehicleGearQtyLabel.Visible = true;

                        chkVehicleWeaponAccessoryInstalled.Checked = objMod.Installed;
                        chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                        chkVehicleIncludedInWeapon.Checked = false;

                        lblVehicleSlotsLabel.Visible = true;
                        lblVehicleSlots.Visible = true;
                        lblVehicleSlots.Text = objMod.CalculatedSlots.ToString();

                        string strPage = objMod.Page(GlobalOptions.Language);
                        lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objMod.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objMod.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);
                    }
                    else
                    {
                        // Look for the selected Vehicle Weapon.
                        Weapon objWeapon = CharacterObject.Vehicles.FindVehicleWeapon(strSelectedId, out objVehicle);
                        if (objWeapon != null)
                        {
                            if (objWeapon.Cyberware || objWeapon.Category == "Gear" || objWeapon.Category.StartsWith("Quality") || objWeapon.IncludedInWeapon || !string.IsNullOrEmpty(objWeapon.ParentID))
                                cmdDeleteVehicle.Enabled = false;
                            DisplayVehicleWeaponStats(true);
                            lblVehicleWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                            lblVehicleWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                            lblVehicleWeaponDamage.Text = objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            lblVehicleWeaponAccuracy.Text = objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                            lblVehicleCost.Text = objWeapon.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                            DisplayVehicleStats(false);
                            string strPage = objWeapon.DisplayPage(GlobalOptions.Language);
                            lblVehicleSource.Text = CommonFunctions.LanguageBookShort(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage;
                            GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleSource, CommonFunctions.LanguageBookLong(objWeapon.Source, GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Page", GlobalOptions.Language) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + strPage);

                            // Determine the Dice Pool size.
                            int intPilot = objVehicle.Pilot;
                            int intAutosoft = 0;
                            foreach (Gear objAutosoft in objVehicle.Gear)
                            {
                                if (objAutosoft.Extra == objWeapon.DisplayCategory(GlobalOptions.DefaultLanguage) && (objAutosoft.Name == "[Weapon] Targeting Autosoft" || objAutosoft.Name == "[Weapon] Melee Autosoft"))
                                {
                                    if (objAutosoft.Rating > intAutosoft)
                                    {
                                        intAutosoft = objAutosoft.Rating;
                                    }
                                }
                            }
                            if (intAutosoft == 0)
                                intPilot -= 1;
                            lblVehicleWeaponDicePool.Text = (intPilot + intAutosoft).ToString();

                            chkVehicleWeaponAccessoryInstalled.Checked = objWeapon.Installed;
                            chkVehicleWeaponAccessoryInstalled.Enabled = objWeapon.ParentID != objWeapon.Parent?.InternalId && objWeapon.ParentID != objVehicle.InternalId;
                            chkVehicleIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;
                        }
                        else
                        {
                            WeaponAccessory objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strSelectedId);
                            if (objAccessory != null)
                            {
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
                                chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Installed;
                                chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;
                            }
                            else
                            {
                                // See if this is a piece of Cyberware.
                                Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);
                                if (objCyberware != null)
                                {
                                    if (!string.IsNullOrEmpty(objCyberware.ParentID))
                                        cmdDeleteVehicle.Enabled = false;
                                    lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                                    nudVehicleRating.Minimum = objCyberware.MinRating;
                                    nudVehicleRating.Maximum = objCyberware.MaxRating;
                                    nudVehicleRating.Value = objCyberware.Rating;
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
                                else
                                {
                                    Gear objGear = CharacterObject.Vehicles.FindVehicleGear(strSelectedId);
                                    if (objGear != null)
                                    {
                                        if (objGear.IncludedInParent)
                                            cmdDeleteVehicle.Enabled = false;
                                        nudVehicleGearQty.Enabled = !objGear.IncludedInParent;
                                        if (objGear.Name.StartsWith("Nuyen"))
                                        {
                                            int intDecimalPlaces = CharacterObjectOptions.NuyenDecimals;
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

                                        if (objGear.MaxRating > 0)
                                        {
                                            lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                                            lblVehicleRatingLabel.Visible = true;
                                            nudVehicleRating.Visible = true;
                                            nudVehicleRating.Enabled = true;
                                            nudVehicleRating.Maximum = objGear.MaxRating;
                                            nudVehicleRating.Value = objGear.Rating;
                                        }
                                        else
                                        {
                                            lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                                            nudVehicleRating.Minimum = 0;
                                            nudVehicleRating.Maximum = 0;
                                            nudVehicleRating.Enabled = false;
                                        }

                                        lblVehicleName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                                        lblVehicleCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                                        lblVehicleAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                                        lblVehicleCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                                        lblVehicleSlotsLabel.Visible = true;
                                        lblVehicleSlots.Visible = true;
                                        lblVehicleSlots.Text = objGear.CalculatedCapacity + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
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
                                }
                            }
                        }
                    }
                }
            }
            _blnSkipRefresh = false;
        }

        /// <summary>
        /// Add or remove the Adapsin Cyberware Grade categories.
        /// </summary>
        public void PopulateCyberwareGradeList(bool blnBioware = false, bool blnIgnoreSecondHand = false, string strForceGrade = "")
        {
            IList<Grade> objGradeList = CharacterObject.GetGradeList(blnBioware ? Improvement.ImprovementSource.Bioware : Improvement.ImprovementSource.Cyberware);
            List<ListItem> lstCyberwareGrades = new List<ListItem>();

            foreach (Grade objWareGrade in objGradeList)
            {
                if (objWareGrade.Name == "None" && (string.IsNullOrEmpty(strForceGrade) || strForceGrade != "None"))
                    continue;
                if (CharacterObject.Improvements.Any(x => ((blnBioware && x.ImproveType == Improvement.ImprovementType.DisableBiowareGrade) || (!blnBioware && x.ImproveType == Improvement.ImprovementType.DisableCyberwareGrade))
                        && objWareGrade.Name.Contains(x.ImprovedName) && x.Enabled))
                    continue;
                if (blnIgnoreSecondHand && objWareGrade.SecondHand)
                    continue;
                if (CharacterObject.AdapsinEnabled)
                {
                    if (!objWareGrade.Adapsin && objGradeList.Any(x => objWareGrade.Name.Contains(x.Name)))
                    {
                        continue;
                    }
                }
                else if (objWareGrade.Adapsin)
                    continue;
                if (CharacterObject.BurnoutEnabled)
                {
                    if (!objWareGrade.Burnout && objGradeList.Any(x => objWareGrade.Burnout && objWareGrade.Name.Contains(x.Name)))
                    {
                        continue;
                    }
                }
                else if (objWareGrade.Burnout)
                    continue;
                if (CharacterObject.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)))
                    continue;

                lstCyberwareGrades.Add(new ListItem(objWareGrade.Name, objWareGrade.DisplayName(GlobalOptions.Language)));
            }
            cboCyberwareGrade.BeginUpdate();
            //cboCyberwareGrade.DataSource = null;
            cboCyberwareGrade.ValueMember = "Value";
            cboCyberwareGrade.DisplayMember = "Name";
            cboCyberwareGrade.DataSource = lstCyberwareGrades;
            cboCyberwareGrade.EndUpdate();
        }

        /// <summary>
        /// Check the character and determine if it has broken any of the rules.
        /// </summary>
        /// <returns></returns>
        public bool CheckCharacterValidity(bool blnUseArgBuildPoints = false, int intBuildPoints = 0)
        {
            if (CharacterObject.IgnoreRules)
                return true;
            Cursor = Cursors.WaitCursor;
            bool blnValid = true;
            string strMessage = LanguageManager.GetString("Message_InvalidBeginning", GlobalOptions.Language);

            // Number of items over the specified Availability the character is allowed to have (typically from the Restricted Gear Quality).
            int intRestrictedAllowed = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.RestrictedItemCount);
            int intRestrictedCount = 0;
            string strAvailItems = string.Empty;
            string strExConItems = string.Empty;
            string strCyberwareGrade = string.Empty;

            // Check if the character has more than 1 Martial Art, not counting qualities. TODO: Make the OTP check an optional rule. Make the Martial Arts limit an optional rule.
            int intMartialArts = CharacterObject.MartialArts.Count(objArt => !objArt.IsQuality);
            if (intMartialArts > 1)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' +
                              LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language)
                                  .Replace("{0}",
                                      ((1 - intMartialArts) * -1).ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                      LanguageManager.GetString("String_MartialArtsCount", GlobalOptions.Language));
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
                    strMessage += Environment.NewLine + '\t' +
                                  LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language)
                                      .Replace("{0}",
                                          ((5 - intTechniques) * -1).ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
                                          LanguageManager.GetString("String_TechniquesCount", GlobalOptions.Language));
                }
            }

            /*
            // Check if the character has gone over limits from optional rules
            int intContactPointsUsed = 0;
            int intGroupContacts = 0;
            int intHighPlaces = 0;
            foreach (Contact objLoopContact in CharacterObject.Contacts)
            {
                if (objLoopContact.EntityType == ContactType.Contact && !objLoopContact.Free)
                {
                    if (objLoopContact.IsGroup)
                    {
                        intGroupContacts += objLoopContact.ContactPoints;
                    }
                    else if (objLoopContact.Connection >= 8 && CharacterObject.FriendsInHighPlaces)
                    {
                        intHighPlaces += objLoopContact.Connection + objLoopContact.Loyalty;
                    }
                    else
                    {
                        intContactPointsUsed += objLoopContact.Connection + objLoopContact.Loyalty;
                    }
                }
            }

            // If the option for CHA * X free points of Contacts is enabled, deduct that amount of points (or as many points have been spent if not the full amount).
            int intFreePoints = (CharacterObject.CHA.TotalValue * CharacterObjectOptions.FreeContactsMultiplier);

            if (intContactPointsUsed >= intFreePoints)
            {
                intContactPointsUsed -= intFreePoints;
            }
            else
            {
                intContactPointsUsed = 0;
            }

            intContactPointsUsed += Math.Max(0, intHighPlaces - (CharacterObject.CHA.TotalValue * 4));

            if (intContactPointsUsed > _objCharacter.ContactPoints)
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidPointExcess").Replace("{0}", ((_objCharacter.ContactPoints - intContactPointsUsed) * -1).ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Contacts"));
            */

            // Calculate the BP used by Enemies. These are added to the BP since they are technically
            // a Negative Quality.
            int intEnemyPoints = 0;
            foreach (Contact objLoopEnemy in CharacterObject.Contacts)
            {
                if (objLoopEnemy.EntityType == ContactType.Enemy && !objLoopEnemy.Free)
                {
                    intEnemyPoints -= (objLoopEnemy.Connection + objLoopEnemy.Loyalty) * CharacterObjectOptions.KarmaEnemy;
                }
            }
            int intNegativePoints = intEnemyPoints;
            // Calculate the BP used by Positive Qualities.
            int intPointsUsed = CharacterObject.Qualities.Where(objQuality => objQuality.Type == QualityType.Positive && objQuality.ContributeToBP && objQuality.ContributeToLimit).Sum(objQuality => objQuality.BP);
            // Group contacts are counted as positive qualities
            intPointsUsed += CharacterObject.Contacts.Where(x => x.EntityType == ContactType.Contact && x.IsGroup && !x.Free).Sum(x => x.ContactPoints);

            // Deduct the amount for free Qualities.
            intPointsUsed -= ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreePositiveQualities);
            int intPositivePointsUsed = intPointsUsed;

            // Calculate the BP used for Negative Qualities.
            intPointsUsed = 0;
            foreach (Quality objQuality in CharacterObject.Qualities.Where(objQuality => objQuality.Type == QualityType.Negative && objQuality.ContributeToBP && objQuality.ContributeToLimit))
            {
                intPointsUsed += objQuality.BP;
                intNegativePoints += objQuality.BP;
            }

            // Deduct the amount for free Qualities.
            intNegativePoints -= ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.FreeNegativeQualities);

            // if positive points > 25
            if (intPositivePointsUsed > CharacterObject.GameplayOptionQualityLimit && !CharacterObjectOptions.ExceedPositiveQualities)
            {
                strMessage += Environment.NewLine + '\t' +
                              LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language)
                                  .Replace("{0}", (CharacterObject.GameplayOptionQualityLimit).ToString());
                blnValid = false;
            }
            int totalNeg = CharacterObjectOptions.EnemyKarmaQualityLimit
                ? intNegativePoints
                : (intNegativePoints - intEnemyPoints);
            // if negative points > 25
            if (totalNeg < (CharacterObject.GameplayOptionQualityLimit * -1) && !CharacterObjectOptions.ExceedNegativeQualities)
            {
                strMessage += Environment.NewLine + '\t' +
                              LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language)
                                  .Replace("{0}", (CharacterObject.GameplayOptionQualityLimit).ToString());
                blnValid = false;
            }

            if (CharacterObject.Contacts.Any(x => (!CharacterObject.FriendsInHighPlaces || x.Connection < 8) && (!CharacterObject.FriendsInHighPlaces && (Math.Max(0, x.Connection) + Math.Max(0, x.Loyalty)) > 7) && !x.Free))
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_HighContact", GlobalOptions.Language);
            }

            // Check if the character has gone over the Build Point total.
            if (!blnUseArgBuildPoints)
                intBuildPoints = CalculateBP(false);
            if (intBuildPoints < 0 && !_blnFreestyle)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language).Replace("{0}", (intBuildPoints * -1).ToString() + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language));
            }

            // if character has more than permitted Metagenetic qualities
            if (CharacterObject.MetageneticLimit > 0)
            {
                int metageneticPositiveQualities = 0;
                int metageneticNegativeQualities = 0;
                foreach (Quality objQuality in CharacterObject.Qualities.Where(objQuality => objQuality.Metagenetic && objQuality.OriginSource.ToString() != QualitySource.Metatype.ToString()))
                {
                    if (objQuality.Type == QualityType.Positive)
                    {
                        metageneticPositiveQualities = metageneticPositiveQualities + objQuality.BP;
                    }
                    else if (objQuality.Type == QualityType.Negative)
                    {
                        metageneticNegativeQualities = metageneticNegativeQualities - objQuality.BP;
                    }
                }
                if (metageneticNegativeQualities > CharacterObject.MetageneticLimit)
                {
                    strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_OverNegativeMetagenicQualities", GlobalOptions.Language).Replace("{0}", metageneticNegativeQualities.ToString()).Replace("{1}", CharacterObject.MetageneticLimit.ToString());
                    blnValid = false;
                }
                if (metageneticPositiveQualities > CharacterObject.MetageneticLimit)
                {
                    strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_OverPositiveMetagenicQualities", GlobalOptions.Language).Replace("{0}", metageneticPositiveQualities.ToString()).Replace("{1}", CharacterObject.MetageneticLimit.ToString());
                    blnValid = false;
                }

                if (metageneticNegativeQualities != metageneticPositiveQualities && metageneticNegativeQualities != (metageneticPositiveQualities - 1))
                {
                    strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_MetagenicQualitiesUnbalanced", GlobalOptions.Language).Replace("{0}", metageneticNegativeQualities.ToString()).Replace("{1}", (metageneticPositiveQualities - 1).ToString()).Replace("{2}", metageneticPositiveQualities.ToString());
                    blnValid = false;
                }
                //Subtract 1 karma to balance Metagenic Qualities
                if (metageneticNegativeQualities == (metageneticPositiveQualities - 1))
                {
                    if (CharacterObject.Karma > 0)
                    {
                        if (MessageBox.Show(LanguageManager.GetString("Message_MetagenicQualitiesSubtractingKarma", GlobalOptions.Language).Replace("{0}", intBuildPoints.ToString()), LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            CharacterObject.Karma -= 1;
                        }
                        else
                        {
                            blnValid = false;
                        }
                    }
                    else
                    {
                        strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_MetagenicQualitiesInsufficientKarma", GlobalOptions.Language).Replace("{0}", intBuildPoints.ToString());
                        blnValid = false;
                    }
                }

            }
            int i = CharacterObject.TotalAttributes - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.AttributeList);
            // Check if the character has gone over on Primary Attributes
            if (i < 0)
            {
                //TODO: ATTACH TO ATTRIBUTE SECTION
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidAttributeExcess", GlobalOptions.Language).Replace("{0}", (i * -1).ToString());
            }

            i = CharacterObject.TotalSpecial - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.SpecialAttributeList);
            // Check if the character has gone over on Special Attributes
            if (i < 0)
            {
                //TODO: ATTACH TO ATTRIBUTE SECTION
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidSpecialExcess", GlobalOptions.Language).Replace("{0}", (i * -1).ToString());
            }

            // Check if the character has gone over on Skill Groups
            if (CharacterObject.SkillsSection.SkillGroupPoints < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidSkillGroupExcess", GlobalOptions.Language).Replace("{0}", ((CharacterObject.SkillsSection.SkillGroupPoints) * -1).ToString());
            }

            // Check if the character has gone over on Active Skills
            if (CharacterObject.SkillsSection.SkillPoints < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidActiveSkillExcess", GlobalOptions.Language).Replace("{0}", ((CharacterObject.SkillsSection.SkillPoints) * -1).ToString());
            }

            // Check if the character has gone over on Knowledge Skills
            if (CharacterObject.SkillsSection.KnowledgeSkillPointsRemain < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidKnowledgeSkillExcess", GlobalOptions.Language).Replace("{0}", ((CharacterObject.SkillsSection.KnowledgeSkillPointsRemain) * -1).ToString());
            }

            // Check if the character has gone over the Nuyen limit.
            decimal decNuyen = CalculateNuyen();
            if (decNuyen < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidNuyenExcess", GlobalOptions.Language).Replace("{0}", (decNuyen * -1).ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥');
            }

            // Check if the character's Essence is above 0.
            double dblEss = decimal.ToDouble(CharacterObject.Essence());
            double dblMinEss = CharacterObjectOptions.DontRoundEssenceInternally ? 0.0 : Math.Pow(10.0, -CharacterObjectOptions.EssenceDecimals);
            if (dblEss < dblMinEss && CharacterObject.ESS.MetatypeMaximum > 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidEssenceExcess", GlobalOptions.Language).Replace("{0}", (dblMinEss - dblEss).ToString(GlobalOptions.CultureInfo));
            }

            // If the character has MAG enabled, make sure a Tradition has been selected.
            if (CharacterObject.MAGEnabled && (string.IsNullOrEmpty(CharacterObject.MagicTradition) || CharacterObject.MagicTradition == "None"))
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidNoTradition", GlobalOptions.Language);
            }

            // If the character has RES enabled, make sure a Stream has been selected.
            if (CharacterObject.RESEnabled && (string.IsNullOrEmpty(CharacterObject.TechnomancerStream) || CharacterObject.TechnomancerStream == "None"))
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidNoStream", GlobalOptions.Language);
            }

            // Check if the character has more than the permitted amount of native languages.
            int intLanguages = CharacterObject.SkillsSection.KnowledgeSkills.Count(objSkill => (objSkill.SkillCategory == "Language" && objSkill.Rating == 0));

            int intLanguageLimit = 1 + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.NativeLanguageLimit);

            if (intLanguages > intLanguageLimit)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_OverLanguageLimit", GlobalOptions.Language).Replace("{0}", intLanguages.ToString()).Replace("{1}", intLanguageLimit.ToString());
            }


            // Check the character's equipment and make sure nothing goes over their set Maximum Availability.
            bool blnRestrictedGearUsed = false;
            string strRestrictedItem = string.Empty;
            // Gear Availability.
            foreach (Gear objGear in CharacterObject.Gear)
            {
                CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
            }

            // Cyberware Availability.
            foreach (Cyberware objCyberware in CharacterObject.Cyberware.DeepWhere(x => x.Children, x => string.IsNullOrEmpty(x.ParentID)))
            {
                if (CharacterObject.BannedWareGrades.Any(s => objCyberware.Grade.Name.Contains(s)))
                    strCyberwareGrade += Environment.NewLine + "\t\t" + objCyberware.DisplayNameShort(GlobalOptions.Language);

                AvailabilityValue objTotalAvail = objCyberware.TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    if (intAvailInt > CharacterObject.MaximumAvailability)
                    {
                        if (intAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = objCyberware.DisplayName(GlobalOptions.Language);
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + objCyberware.DisplayNameShort(GlobalOptions.Language);
                        }
                    }
                }
                foreach (Gear objGear in objCyberware.Gear.Where(objGear => !objGear.IncludedInParent))
                {
                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
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
                    strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_OverPrototypeLimit", GlobalOptions.Language), decPrototypeTranshumanEssenceUsed.ToString(CharacterObjectOptions.EssenceFormat, GlobalOptions.CultureInfo), decPrototypeTranshumanEssenceMax.ToString(CharacterObjectOptions.EssenceFormat, GlobalOptions.CultureInfo));
                }
            }

            // Armor Availability.
            foreach (Armor objArmor in CharacterObject.Armor)
            {
                int intAvailInt = objArmor.TotalAvailTuple().Value;
                if (intAvailInt > CharacterObject.MaximumAvailability)
                {
                    if (intAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                    {
                        blnRestrictedGearUsed = true;
                        strRestrictedItem = objArmor.DisplayName(GlobalOptions.Language);
                    }
                    else
                    {
                        intRestrictedCount++;
                        strAvailItems += Environment.NewLine + "\t\t" + objArmor.DisplayNameShort(GlobalOptions.Language);
                    }
                }

                foreach (ArmorMod objMod in objArmor.ArmorMods.Where(objMod => !objMod.IncludedInArmor))
                {
                    AvailabilityValue objTotalAvail = objMod.TotalAvailTuple();
                    if (!objTotalAvail.AddToParent)
                    {
                        int intModAvailInt = objTotalAvail.Value;
                        if (intModAvailInt > CharacterObject.MaximumAvailability)
                        {
                            if (intModAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                            {
                                blnRestrictedGearUsed = true;
                                strRestrictedItem = objMod.DisplayName(GlobalOptions.Language);
                            }
                            else
                            {
                                intRestrictedCount++;
                                strAvailItems += Environment.NewLine + "\t\t" + objMod.DisplayNameShort(GlobalOptions.Language);
                            }
                        }
                    }
                    foreach (Gear objGear in objMod.Gear.Where(objGear => !objGear.IncludedInParent))
                    {
                        CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                    }
                }

                foreach (Gear objGear in objArmor.Gear.Where(objGear => !objGear.IncludedInParent))
                {
                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                }
            }

            // Weapon Availability.
            foreach (Weapon objWeapon in CharacterObject.Weapons.GetAllDescendants(x => x.Children))
            {
                AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                if (!objWeaponAvail.AddToParent)
                {
                    int intAvailInt = objWeaponAvail.Value;
                    if (intAvailInt > CharacterObject.MaximumAvailability)
                    {
                        if (intAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = objWeapon.DisplayName(GlobalOptions.Language);
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + objWeapon.DisplayNameShort(GlobalOptions.Language);
                        }
                    }
                }
                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories.Where(objAccessory => !objAccessory.IncludedInWeapon))
                {
                    AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                    if (!objAccessoryAvail.AddToParent)
                    {
                        int intAccessoryAvailInt = objAccessoryAvail.Value;
                        if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                        {
                            if (intAccessoryAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                            {
                                blnRestrictedGearUsed = true;
                                strRestrictedItem = objAccessory.DisplayName(GlobalOptions.Language);
                            }
                            else
                            {
                                intRestrictedCount++;
                                strAvailItems += Environment.NewLine + "\t\t" + objAccessory.DisplayNameShort(GlobalOptions.Language);
                            }
                        }
                    }

                    foreach (Gear objGear in objAccessory.Gear.Where(objGear => !objGear.IncludedInParent))
                    {
                        CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                    }
                }
            }

            // Vehicle Availability.
            foreach (Vehicle objVehicle in CharacterObject.Vehicles)
            {
                int intAvailInt = objVehicle.TotalAvailTuple().Value;
                if (intAvailInt > CharacterObject.MaximumAvailability)
                {
                    if (intAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                    {
                        blnRestrictedGearUsed = true;
                        strRestrictedItem = objVehicle.DisplayName(GlobalOptions.Language);
                    }
                    else
                    {
                        intRestrictedCount++;
                        strAvailItems += Environment.NewLine + "\t\t" + objVehicle.DisplayNameShort(GlobalOptions.Language);
                    }
                }
                foreach (VehicleMod objVehicleMod in objVehicle.Mods.Where((objVehicleMod => !objVehicleMod.IncludedInVehicle)))
                {
                    AvailabilityValue objModAvail = objVehicleMod.TotalAvailTuple();
                    if (!objModAvail.AddToParent)
                    {
                        int intModAvailInt = objModAvail.Value;
                        if (intModAvailInt > CharacterObject.MaximumAvailability)
                        {
                            if (intModAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                            {
                                blnRestrictedGearUsed = true;
                                strRestrictedItem = objVehicleMod.DisplayName(GlobalOptions.Language);
                            }
                            else
                            {
                                intRestrictedCount++;
                                strAvailItems += Environment.NewLine + "\t\t" + objVehicleMod.DisplayNameShort(GlobalOptions.Language);
                            }
                        }
                        foreach (Weapon objWeapon in objVehicleMod.Weapons.GetAllDescendants(x => x.Children))
                        {
                            AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                            if (!objWeaponAvail.AddToParent)
                            {
                                int intWeaponAvailInt = objWeaponAvail.Value;
                                if (intWeaponAvailInt > CharacterObject.MaximumAvailability)
                                {
                                    if (intWeaponAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                                    {
                                        blnRestrictedGearUsed = true;
                                        strRestrictedItem = objWeapon.DisplayName(GlobalOptions.Language);
                                    }
                                    else
                                    {
                                        intRestrictedCount++;
                                        strAvailItems += Environment.NewLine + "\t\t" + objWeapon.DisplayNameShort(GlobalOptions.Language);
                                    }
                                }
                            }
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories.Where((objAccessory => !objAccessory.IncludedInWeapon)))
                            {
                                AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                                if (!objAccessoryAvail.AddToParent)
                                {
                                    int intAccessoryAvailInt = objAccessoryAvail.Value;
                                    if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                                    {
                                        if (intAccessoryAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                                        {
                                            blnRestrictedGearUsed = true;
                                            strRestrictedItem = objAccessory.DisplayName(GlobalOptions.Language);
                                        }
                                        else
                                        {
                                            intRestrictedCount++;
                                            strAvailItems += Environment.NewLine + "\t\t" + objAccessory.DisplayName(GlobalOptions.Language);
                                        }
                                    }
                                }

                                foreach (Gear objGear in objAccessory.Gear.Where(objGear => !objGear.IncludedInParent))
                                {
                                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                                }
                            }
                        }
                    }
                    foreach (Gear objGear in objVehicle.Gear)
                    {
                        CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                    }
                }
            }

            // Make sure the character is not carrying more items over the allowed Avail than they are allowed.
            if (intRestrictedCount > intRestrictedAllowed)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidAvail", GlobalOptions.Language).Replace("{0}", (intRestrictedCount - intRestrictedAllowed).ToString()).Replace("{1}", CharacterObject.MaximumAvailability.ToString());
                strMessage += strAvailItems;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_RestrictedGearUsed", GlobalOptions.Language).Replace("{0}", strRestrictedItem);
            }

            if (!string.IsNullOrWhiteSpace(strExConItems))
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidExConWare", GlobalOptions.Language);
                strMessage += strExConItems;
            }

            if (!string.IsNullOrWhiteSpace(strCyberwareGrade))
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidCyberwareGrades", GlobalOptions.Language);
                strMessage += strCyberwareGrade;
            }

            // Check item Capacities if the option is enabled.
            List<string> lstOverCapacity = new List<string>();

            if (CharacterObjectOptions.EnforceCapacity)
            {
                bool blnOverCapacity = false;
                int intCapacityOver = 0;
                // Armor Capacity.
                foreach (Armor objArmor in CharacterObject.Armor.Where(objArmor => objArmor.CapacityRemaining < 0))
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
                    foreach (Gear objChild in objGear.Children.Where(objChild => objChild.CapacityRemaining < 0))
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
                    foreach (Cyberware objChild in objCyberware.Children.Where(objChild => objChild.CapacityRemaining < 0))
                    {
                        blnOverCapacity = true;
                        lstOverCapacity.Add(objChild.Name);
                        intCapacityOver++;
                    }
                }

                // Vehicle Capacity.
                foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                {
                    if (CharacterObjectOptions.BookEnabled("R5"))
                    {
                        if (objVehicle.IsDrone && GlobalOptions.Dronemods)
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
                    foreach (Gear objGear in objVehicle.Gear)
                    {
                        if (objGear.CapacityRemaining < 0)
                        {
                            blnOverCapacity = true;
                            lstOverCapacity.Add(objGear.Name);
                            intCapacityOver++;
                        }
                        // Check Child Gear.
                        foreach (Gear objChild in objGear.Children.Where(objChild => objChild.CapacityRemaining < 0))
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
                    strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_CapacityReachedValidate", GlobalOptions.Language).Replace("{0}", intCapacityOver.ToString());
                    foreach (string strItem in lstOverCapacity)
                    {
                        strMessage += Environment.NewLine + "\t- " + strItem;
                    }
                }
            }

            //Check Drone mods for illegalities
            if (CharacterObjectOptions.BookEnabled("R5"))
            {
                List<string> lstDronesIllegalDowngrades = new List<string>();
                bool blnIllegalDowngrades = false;
                int intIllegalDowngrades = 0;
                foreach (Vehicle objVehicle in CharacterObject.Vehicles)
                {
                    if (objVehicle.IsDrone && GlobalOptions.Dronemods)
                    {
                        foreach (VehicleMod objMod in objVehicle.Mods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Downgrade))
                        {
                            //Downgrades can't reduce a attribute to less than 1 (except Speed which can go to 0)
                            if ((objMod.Category == "Handling" && Convert.ToInt32(objVehicle.TotalHandling) < 1) ||
                                (objMod.Category == "Speed" && Convert.ToInt32(objVehicle.TotalSpeed) < 0) ||
                                (objMod.Category == "Acceleration" && Convert.ToInt32(objVehicle.TotalAccel) < 1) ||
                                (objMod.Category == "Body" && Convert.ToInt32(objVehicle.TotalBody) < 1) ||
                                (objMod.Category == "Armor" && Convert.ToInt32(objVehicle.TotalArmor) < 1) ||
                                (objMod.Category == "Sensor" && Convert.ToInt32(objVehicle.CalculatedSensor) < 1))
                            {
                                blnIllegalDowngrades = true;
                                intIllegalDowngrades++;
                                lstDronesIllegalDowngrades.Add(objVehicle.Name);
                                break;
                            }
                        }
                    }
                }
                if (blnIllegalDowngrades)
                {
                    blnValid = false;
                    strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_DroneIllegalDowngrade", GlobalOptions.Language).Replace("{0}", intIllegalDowngrades.ToString());
                    foreach (string strItem in lstDronesIllegalDowngrades)
                    {
                        strMessage += Environment.NewLine + "\t- " + strItem;
                    }
                }
            }


            i = CharacterObject.Attributes - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.AttributeList);
            // Check if the character has gone over on Primary Attributes
            if (blnValid && i > 0)
            {
                if (MessageBox.Show(
                    LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                        .Replace("{0}", i.ToString())
                        .Replace("{1}", LanguageManager.GetString("Label_SummaryPrimaryAttributes", GlobalOptions.Language)),
                    LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.No)
                {
                    blnValid = false;
                }
            }

            i = CharacterObject.Special - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.SpecialAttributeList);
            // Check if the character has gone over on Special Attributes
            if (blnValid && i > 0)
            {
                if (
                    MessageBox.Show(
                        LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            .Replace("{0}", i.ToString())
                            .Replace("{1}", LanguageManager.GetString("Label_SummarySpecialAttributes", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Skill Groups
            if (blnValid && CharacterObject.SkillsSection.SkillGroupPoints > 0)
            {
                if (
                    MessageBox.Show(
                        LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            .Replace("{0}", CharacterObject.SkillsSection.SkillGroupPoints.ToString())
                            .Replace("{1}", LanguageManager.GetString("Label_SummarySkillGroups", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Active Skills
            if (blnValid && CharacterObject.SkillsSection.SkillPoints > 0)
            {
                if (
                    MessageBox.Show(
                        LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            .Replace("{0}", CharacterObject.SkillsSection.SkillPoints.ToString())
                            .Replace("{1}", LanguageManager.GetString("Label_SummaryActiveSkills", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Knowledge Skills
            if (blnValid && CharacterObject.SkillsSection.KnowledgeSkillPointsRemain > 0)
            {
                if (
                    MessageBox.Show(
                        LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            .Replace("{0}", CharacterObject.SkillsSection.KnowledgeSkillPointsRemain.ToString())
                            .Replace("{1}", LanguageManager.GetString("Label_SummaryKnowledgeSkills", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }
            Cursor = Cursors.Default;
            if (!blnValid && strMessage.Length > LanguageManager.GetString("Message_InvalidBeginning", GlobalOptions.Language).Length)
                MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_Invalid", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return blnValid;
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="objGear">Gear to check.</param>
        /// <param name="blnRestrictedGearUsed">Whether Restricted Gear is already being used.</param>
        /// <param name="intRestrictedCount">Amount of gear that is currently over the availability limit.</param>
        /// <param name="strAvailItems">String used to list names of gear that are currently over the availability limit.</param>
        /// <param name="strRestrictedItem">Item that is being used for Restricted Gear.</param>
        /// <param name="blnOutRestrictedGearUsed">Whether Restricted Gear is already being used (tracked across gear children).</param>
        /// <param name="intOutRestrictedCount">Amount of gear that is currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutAvailItems">String used to list names of gear that are currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutRestrictedItem">Item that is being used for Restricted Gear (tracked across gear children).</param>
        private void CheckRestrictedGear(Gear objGear, bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems, string strRestrictedItem, out bool blnOutRestrictedGearUsed, out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem)
        {
            AvailabilityValue objTotalAvail = objGear.TotalAvailTuple();
            if (!objTotalAvail.AddToParent)
            {
                int intAvailInt = objTotalAvail.Value;
                //TODO: Make this dynamically update without having to validate the character.
                if (intAvailInt > CharacterObject.MaximumAvailability)
                {
                    if (intAvailInt <= 24 && CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                    {
                        blnRestrictedGearUsed = true;
                        strRestrictedItem = objGear.Parent == null ? objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language) : $"{objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language)} ({objGear.Parent})";
                    }
                    else
                    {
                        intRestrictedCount++;
                        strAvailItems += Environment.NewLine + "\t\t" + objGear.DisplayNameShort(GlobalOptions.Language);
                    }
                }
            }
            foreach (Gear objChild in objGear.Children.Where(objChild => !objChild.IncludedInParent))
            {
                CheckRestrictedGear(objChild, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
            }
            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
        }

        /// <summary>
        /// Confirm that the character can move to career mode and perform final actions for karma carryover and such.
        /// </summary>
        public bool ValidateCharacter()
        {
            int intBuildPoints = CalculateBP(false);

            if (CheckCharacterValidity(true, intBuildPoints))
            {
                // See if the character has any Karma remaining.
                if (intBuildPoints > CharacterObjectOptions.KarmaCarryover)
                {
                    if (CharacterObject.BuildMethod == CharacterBuildMethod.Karma)
                    {
                        if (MessageBox.Show(LanguageManager.GetString("Message_NoExtraKarma", GlobalOptions.Language).Replace("{0}", intBuildPoints.ToString()), LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            return false;
                    }
                    else
                    {
                        if (MessageBox.Show(LanguageManager.GetString("Message_ExtraKarma", GlobalOptions.Language).Replace("{0}", intBuildPoints.ToString()).Replace("{1}", CharacterObjectOptions.KarmaCarryover.ToString()), LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            return false;
                    }
                }
                if (CharacterObject.Nuyen > 5000)
                {
                    if (MessageBox.Show(LanguageManager.GetString("Message_ExtraNuyen", GlobalOptions.Language).Replace("{0}", CharacterObject.Nuyen.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo)).Replace("{1}", (5000).ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo)), LanguageManager.GetString("MessageTitle_ExtraNuyen", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return false;
                }
                if (CharacterObjectOptions.CreateBackupOnCareer && chkCharacterCreated.Checked)
                {
                    // Create a pre-Career Mode backup of the character.
                    // Make sure the backup directory exists.
                    if (!Directory.Exists(Path.Combine(Application.StartupPath, "saves", "backup")))
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "saves", "backup"));
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
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
                                strNewName = Guid.NewGuid().ToString("N");
                        }
                    }
                    strNewName += LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' + LanguageManager.GetString("Title_CreateMode", GlobalOptions.Language) + ").chum5";

                    strNewName = Path.Combine(Application.StartupPath, "saves", "backup", strNewName);

                    Cursor = Cursors.WaitCursor;
                    if (!CharacterObject.Save(strNewName))
                    {
                        Cursor = Cursors.Default;
                        return false;
                    }
                    Cursor = Cursors.Default;
                }

                _blnSkipUpdate = true;
                // See if the character has any Karma remaining.
                if (intBuildPoints > CharacterObjectOptions.KarmaCarryover)
                {
                    CharacterObject.Karma = CharacterObject.BuildMethod == CharacterBuildMethod.Karma ? 0 : CharacterObjectOptions.KarmaCarryover;
                }
                else
                {
                    CharacterObject.Karma = intBuildPoints;
                }
                // Determine the highest Lifestyle the character has.
                Lifestyle objLifestyle = CharacterObject.Lifestyles.FirstOrDefault();
                if (objLifestyle != null)
                {
                    foreach (Lifestyle objCharacterLifestyle in CharacterObject.Lifestyles)
                    {
                        if (objCharacterLifestyle.Multiplier > objLifestyle.Multiplier)
                            objLifestyle = objCharacterLifestyle;
                    }
                }

                // If the character does not have any Lifestyles, give them the Street Lifestyle.
                if (CharacterObject.Lifestyles.Count == 0)
                {
                    objLifestyle = new Lifestyle(CharacterObject);
                    XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                    XmlNode objXmlLifestyle = objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Street\"]");

                    objLifestyle.Create(objXmlLifestyle);

                    CharacterObject.Lifestyles.Add(objLifestyle);
                }

                if (CharacterObject.Nuyen > 5000)
                {
                    CharacterObject.Nuyen = 5000;
                }

                frmLifestyleNuyen frmStartingNuyen = new frmLifestyleNuyen(CharacterObject)
                {
                    Dice = objLifestyle?.Dice ?? 1,
                    Multiplier = objLifestyle?.Multiplier ?? 20
                };

                frmStartingNuyen.ShowDialog(this);

                // Assign the starting Nuyen amount.
                decimal decStartingNuyen = frmStartingNuyen.StartingNuyen;
                if (decStartingNuyen < 0)
                    decStartingNuyen = 0;

                CharacterObject.Nuyen += decStartingNuyen;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Verify that the user wants to save this character as Created.
        /// </summary>
        public override bool ConfirmSaveCreatedCharacter()
        {
            if (MessageBox.Show(LanguageManager.GetString("Message_ConfirmCreate", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ConfirmCreate", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return false;

            if (!ValidateCharacter())
                return false;

            // The user has confirmed that the character should be Create.
            CharacterObject.Created = true;

            return true;
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

        /// <summary>
        /// Add a PACKS Kit to the character.
        /// </summary>
        public bool AddPACKSKit()
        {
            frmSelectPACKSKit frmPickPACKSKit = new frmSelectPACKSKit(CharacterObject);
            frmPickPACKSKit.ShowDialog(this);

            bool blnCreateChildren = true;

            // If the form was canceled, don't do anything.
            if (frmPickPACKSKit.DialogResult == DialogResult.Cancel)
                return false;

            // Do not create child items for Gear if the chosen Kit is in the Custom category since these items will contain the exact plugins desired.
            //if (frmPickPACKSKit.SelectedCategory == "Custom")
            //blnCreateChildren = false;

            XmlNode objXmlKit = XmlManager.Load("packs.xml").SelectSingleNode("/chummer/packs/pack[name = \"" + frmPickPACKSKit.SelectedKit + "\" and category = \"" + frmSelectPACKSKit.SelectedCategory + "\"]");
            if (objXmlKit == null)
                return false;
            // Update Qualities.
            XmlNode xmlQualities = objXmlKit["qualities"];
            if (xmlQualities != null)
            {
                XmlDocument xmlQualityDocument = XmlManager.Load("qualities.xml");

                // Positive and Negative Qualities.
                using (XmlNodeList xmlQualityList = xmlQualities.SelectNodes("*/quality"))
                    if (xmlQualityList?.Count > 0)
                        foreach (XmlNode objXmlQuality in xmlQualityList)
                        {
                            XmlNode objXmlQualityNode = xmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlQuality.InnerText + "\"]");

                            if (objXmlQualityNode != null)
                            {
                                List<Weapon> lstWeapons = new List<Weapon>();
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

            //TODO: PACKS SKILLS?

            // Select a Martial Art.
            XmlNode xmlSelectMartialArt = objXmlKit["selectmartialart"];
            if (xmlSelectMartialArt != null)
            {
                string strForcedValue = xmlSelectMartialArt.Attributes?["select"]?.InnerText ?? string.Empty;
                int intRating = Convert.ToInt32(xmlSelectMartialArt.Attributes?["rating"]?.InnerText ?? "1");

                frmSelectMartialArt frmPickMartialArt = new frmSelectMartialArt(CharacterObject)
                {
                    ForcedValue = strForcedValue
                };
                frmPickMartialArt.ShowDialog(this);

                if (frmPickMartialArt.DialogResult != DialogResult.Cancel)
                {
                    // Open the Martial Arts XML file and locate the selected piece.
                    XmlDocument objXmlMartialArtDocument = XmlManager.Load("martialarts.xml");

                    XmlNode objXmlArt = objXmlMartialArtDocument.SelectSingleNode("/chummer/martialarts/martialart[id = \"" + frmPickMartialArt.SelectedMartialArt + "\"]");

                    MartialArt objMartialArt = new MartialArt(CharacterObject);
                    objMartialArt.Create(objXmlArt);
                    objMartialArt.Rating = intRating;
                    CharacterObject.MartialArts.Add(objMartialArt);
                }
            }

            // Update Martial Arts.
            XmlNode xmlMartialArts = objXmlKit["martialarts"];
            if (xmlMartialArts != null)
            {
                // Open the Martial Arts XML file and locate the selected art.
                XmlDocument objXmlMartialArtDocument = XmlManager.Load("martialarts.xml");

                foreach (XmlNode objXmlArt in xmlMartialArts.SelectNodes("martialart"))
                {
                    MartialArt objArt = new MartialArt(CharacterObject);
                    XmlNode objXmlArtNode = objXmlMartialArtDocument.SelectSingleNode("/chummer/martialarts/martialart[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlArt["name"].InnerText + "\"]");
                    if (objXmlArtNode != null)
                    {
                        objArt.Create(objXmlArtNode);
                        objArt.Rating = Convert.ToInt32(objXmlArt["rating"].InnerText);
                        CharacterObject.MartialArts.Add(objArt);

                        // Check for Advantages.
                        foreach (XmlNode objXmlAdvantage in objXmlArt.SelectNodes("techniques/technique"))
                        {
                            MartialArtTechnique objAdvantage = new MartialArtTechnique(CharacterObject);
                            XmlNode objXmlAdvantageNode = objXmlMartialArtDocument.SelectSingleNode("/chummer/techniques/technique[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlAdvantage["name"].InnerText + "\"]");
                            objAdvantage.Create(objXmlAdvantageNode);
                            objArt.Techniques.Add(objAdvantage);
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
                XmlDocument objXmlComplexFormDocument = XmlManager.Load("complexforms.xml");

                foreach (XmlNode objXmlComplexForm in xmlComplexForms.SelectNodes("complexform"))
                {
                    XmlNode objXmlComplexFormNode = objXmlComplexFormDocument.SelectSingleNode("/chummer/complexforms/complexform[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlComplexForm["name"].InnerText + "\"]");
                    if (objXmlComplexFormNode != null)
                    {
                        string strForceValue = objXmlComplexForm.Attributes?["select"]?.InnerText ?? string.Empty;

                        ComplexForm objComplexForm = new ComplexForm(CharacterObject);
                        objComplexForm.Create(objXmlComplexFormNode, strForceValue);

                        CharacterObject.ComplexForms.Add(objComplexForm);
                    }
                }
            }

            // Update AI Programs.
            XmlNode xmlPrograms = objXmlKit["programs"];
            if (xmlPrograms != null)
            {
                // Open the Programs XML file and locate the selected program.
                XmlDocument objXmlProgramDocument = XmlManager.Load("programs.xml");

                foreach (XmlNode objXmlProgram in xmlPrograms.SelectNodes("program"))
                {
                    XmlNode objXmlProgramNode = objXmlProgramDocument.SelectSingleNode("/chummer/programs/program[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlProgram["name"].InnerText + "\"]");
                    if (objXmlProgramNode != null)
                    {
                        AIProgram objProgram = new AIProgram(CharacterObject);
                        objProgram.Create(objXmlProgramNode);

                        CharacterObject.AIPrograms.Add(objProgram);
                    }
                }
            }

            // Update Spells.
            XmlNode xmlSpells = objXmlKit["spells"];
            if (xmlSpells != null)
            {
                XmlDocument objXmlSpellDocument = XmlManager.Load("spells.xml");

                foreach (XmlNode objXmlSpell in xmlSpells.SelectNodes("spell"))
                {
                    string strCategory = objXmlSpell["category"]?.InnerText;
                    string strName = objXmlSpell["name"].InnerText;
                    // Make sure the Spell has not already been added to the character.
                    if (!CharacterObject.Spells.Any(x => x.Name == strName && x.Category == strCategory))
                    {
                        XmlNode objXmlSpellNode = objXmlSpellDocument.SelectSingleNode("/chummer/spells/spell[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + strName + "\"]");

                        if (objXmlSpellNode == null)
                            continue;

                        Spell objSpell = new Spell(CharacterObject);
                        string strForceValue = objXmlSpell.Attributes?["select"]?.InnerText ?? string.Empty;
                        objSpell.Create(objXmlSpellNode, strForceValue);
                        CharacterObject.Spells.Add(objSpell);
                    }
                }
            }

            // Update Spirits.
            XmlNode xmlSpirits = objXmlKit["spirits"];
            if (xmlSpirits != null)
            {
                foreach (XmlNode objXmlSpirit in xmlSpirits.SelectNodes("spirit"))
                {
                    Spirit objSpirit = new Spirit(CharacterObject)
                    {
                        EntityType = SpiritType.Spirit,
                        Name = objXmlSpirit["name"].InnerText,
                        Force = Convert.ToInt32(objXmlSpirit["force"].InnerText),
                        ServicesOwed = Convert.ToInt32(objXmlSpirit["services"].InnerText)
                    };
                    CharacterObject.Spirits.Add(objSpirit);
                }
            }

            // Update Lifestyles.
            XmlNode xmlLifestyles = objXmlKit["lifestyles"];
            if (xmlLifestyles != null)
            {
                XmlDocument objXmlLifestyleDocument = XmlManager.Load("lifestyles.xml");

                foreach (XmlNode objXmlLifestyle in xmlLifestyles.SelectNodes("lifestyle"))
                {
                    string strName = objXmlLifestyle["name"].InnerText;
                    int intMonths = Convert.ToInt32(objXmlLifestyle["months"].InnerText);

                    // Create the Lifestyle.
                    Lifestyle objLifestyle = new Lifestyle(CharacterObject);

                    XmlNode objXmlLifestyleNode = objXmlLifestyleDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strName + "\"]");
                    if (objXmlLifestyleNode != null)
                    {
                        // This is a standard Lifestyle, so just use the Create method.
                        objLifestyle.Create(objXmlLifestyleNode);
                        objLifestyle.Increments = intMonths;
                    }
                    else
                    {
                        // This is an Advanced Lifestyle, so build it manually.
                        objLifestyle.Name = strName;
                        objLifestyle.Increments = intMonths;
                        objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
                        objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
                        objLifestyle.Multiplier = Convert.ToInt32(objXmlLifestyle["multiplier"].InnerText);
                        objLifestyle.BaseLifestyle = objXmlLifestyle["baselifestyle"].InnerText;
                        objLifestyle.Source = "SR5";
                        objLifestyle.Page = "373";
                        objLifestyle.Comforts = Convert.ToInt32(objXmlLifestyle["comforts"].InnerText);
                        objLifestyle.Security = Convert.ToInt32(objXmlLifestyle["security"].InnerText);
                        objLifestyle.Area = Convert.ToInt32(objXmlLifestyle["area"].InnerText);
                        objLifestyle.BaseComforts = Convert.ToInt32(objXmlLifestyle["comfortsminimum"].InnerText);
                        objLifestyle.BaseSecurity = Convert.ToInt32(objXmlLifestyle["securityminimum"].InnerText);
                        objLifestyle.BaseArea = Convert.ToInt32(objXmlLifestyle["areaminimum"].InnerText);

                        foreach (LifestyleQuality objXmlQuality in objXmlLifestyle.SelectNodes("lifestylequalities/lifestylequality"))
                            objLifestyle.LifestyleQualities.Add(objXmlQuality);
                    }

                    // Add the Lifestyle to the character and Lifestyle Tree.
                    CharacterObject.Lifestyles.Add(objLifestyle);
                }
            }

            // Update NuyenBP.
            string strNuyenBP = objXmlKit["nuyenbp"]?.InnerText;
            if (!string.IsNullOrEmpty(strNuyenBP) && decimal.TryParse(strNuyenBP, System.Globalization.NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decAmount))
            {
                //if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma)
                //decAmount *= 2;

                CharacterObject.NuyenBP += decAmount;
            }

            XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");

            // Update Armor.
            XmlNode xmlArmors = objXmlKit["armors"];
            if (xmlArmors != null)
            {
                XmlDocument objXmlArmorDocument = XmlManager.Load("armor.xml");
                foreach (XmlNode objXmlArmor in xmlArmors.SelectNodes("armor"))
                {
                    XmlNode objXmlArmorNode = objXmlArmorDocument.SelectSingleNode("/chummer/armors/armor[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlArmor["name"].InnerText + "\"]");
                    if (objXmlArmorNode != null)
                    {
                        Armor objArmor = new Armor(CharacterObject);
                        List<Weapon> lstWeapons = new List<Weapon>();

                        objArmor.Create(objXmlArmorNode, Convert.ToInt32(objXmlArmor["rating"]?.InnerText), lstWeapons, false, blnCreateChildren);
                        CharacterObject.Armor.Add(objArmor);

                        // Look for Armor Mods.
                        foreach (XmlNode objXmlMod in objXmlArmor.SelectNodes("mods/mod"))
                        {
                            XmlNode objXmlModNode = objXmlArmorDocument.SelectSingleNode("/chummer/mods/mod[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlMod["name"].InnerText + "\"]");
                            if (objXmlModNode != null)
                            {
                                ArmorMod objMod = new ArmorMod(CharacterObject);
                                int intRating = 0;
                                if (objXmlMod["rating"] != null)
                                    intRating = Convert.ToInt32(objXmlMod["rating"].InnerText);
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
            }

            // Update Weapons.
            XmlNode xmlWeapons = objXmlKit["weapons"];
            if (xmlWeapons != null)
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

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

                    XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                    if (objXmlWeaponNode != null)
                    {
                        Weapon objWeapon = new Weapon(CharacterObject);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        objWeapon.Create(objXmlWeaponNode, lstWeapons, blnCreateChildren);
                        CharacterObject.Weapons.Add(objWeapon);

                        // Look for Weapon Accessories.
                        foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                        {
                            XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                            if (objXmlAccessoryNode != null)
                            {
                                WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                                string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                                string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                                objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                                objMod.Parent = objWeapon;

                                objWeapon.WeaponAccessories.Add(objMod);

                                foreach (XmlNode objXmlGear in objXmlAccessory.SelectNodes("gears/gear"))
                                    AddPACKSGear(objXmlGearDocument, objXmlGear, objMod, blnCreateChildren);
                            }
                        }

                        // Look for an Underbarrel Weapon.
                        XmlNode xmlUnderbarrelNode = objXmlWeapon["underbarrel"];
                        if (xmlUnderbarrelNode != null)
                        {
                            XmlNode objXmlUnderbarrelNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlWeapon["underbarrel"].InnerText + "\"]");
                            if (objXmlUnderbarrelNode == null)
                            {
                                List<Weapon> lstLoopWeapons = new List<Weapon>();
                                Weapon objUnderbarrelWeapon = new Weapon(CharacterObject);
                                objUnderbarrelWeapon.Create(objXmlUnderbarrelNode, lstLoopWeapons, blnCreateChildren);
                                objWeapon.UnderbarrelWeapons.Add(objUnderbarrelWeapon);
                                if (objWeapon.AllowAccessory == false)
                                    objUnderbarrelWeapon.AllowAccessory = false;

                                foreach (Weapon objLoopWeapon in lstLoopWeapons)
                                {
                                    if (objWeapon.AllowAccessory == false)
                                        objLoopWeapon.AllowAccessory = false;
                                    objWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                                }

                                foreach (XmlNode objXmlAccessory in xmlUnderbarrelNode.SelectNodes("accessories/accessory"))
                                {
                                    XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                                    if (objXmlAccessoryNode != null)
                                    {
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

                        foreach (Weapon objLoopWeapon in lstWeapons)
                        {
                            CharacterObject.Weapons.Add(objLoopWeapon);
                        }
                    }

                    Application.DoEvents();
                }
            }

            XmlDocument objXmlCyberwareDocument = XmlManager.Load("cyberware.xml");
            XmlDocument objXmlBiowareDocument = XmlManager.Load("bioware.xml");

            // Update Cyberware.
            XmlNode xmlCyberwares = objXmlKit["cyberwares"];
            if (xmlCyberwares != null)
            {
                XmlNodeList xmlCyberwaresList = xmlWeapons.SelectNodes("cyberware");
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
                XmlDocument objXmlVehicleDocument = XmlManager.Load("vehicles.xml");
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

                    XmlNode objXmlVehicleNode = objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlVehicle["name"].InnerText + "\"]");
                    if (objXmlVehicleNode != null)
                    {
                        Vehicle objVehicle = new Vehicle(CharacterObject);
                        objVehicle.Create(objXmlVehicleNode, blnCreateChildren);
                        CharacterObject.Vehicles.Add(objVehicle);

                        // Grab the default Sensor that comes with the Vehicle.
                        foreach (Gear objSensorGear in objVehicle.Gear)
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
                            XmlNode objXmlModNode = objXmlVehicleDocument.SelectSingleNode("/chummer/mods/mod[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlMod["name"].InnerText + "\"]");
                            if (objXmlModNode != null)
                            {
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
                        }

                        // Add any Vehicle Gear.
                        foreach (XmlNode objXmlGear in objXmlVehicle.SelectNodes("gears/gear"))
                        {
                            Gear objGear = AddPACKSGear(objXmlGearDocument, objXmlGear, objVehicle, blnCreateChildren);
                            // If this is a Sensor, it will replace the Vehicle's base sensor, so remove it.
                            if (objGear != null && objGear.Category == "Sensors" && objGear.Cost == "0" && objGear.Rating == 0)
                            {
                                objVehicle.Gear.Remove(objDefaultSensor);
                            }
                        }

                        // Add any Vehicle Weapons.
                        if (objXmlVehicle["weapons"] != null)
                        {
                            XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                            foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
                            {
                                Weapon objWeapon = new Weapon(CharacterObject);

                                List<Weapon> lstSubWeapons = new List<Weapon>();
                                XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                                if (objXmlWeaponNode == null)
                                    continue;
                                objWeapon.ParentVehicle = objVehicle;
                                objWeapon.Create(objXmlWeaponNode, lstSubWeapons, blnCreateChildren);

                                // Find the first Weapon Mount in the Vehicle.
                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    if (objMod.Name.Contains("Weapon Mount") || (!string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category)))
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
                                    XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                                    if (objXmlAccessoryNode != null)
                                    {
                                        WeaponAccessory objMod = new WeaponAccessory(CharacterObject);
                                        string strMount = objXmlAccessory["mount"]?.InnerText ?? "Internal";
                                        string strExtraMount = objXmlAccessory["extramount"]?.InnerText ?? "None";
                                        objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);
                                        objMod.Parent = objWeapon;

                                        objWeapon.WeaponAccessories.Add(objMod);
                                    }
                                }

                                // Look for an Underbarrel Weapon.
                                XmlNode xmlUnderbarrelNode = objXmlWeapon["underbarrel"];
                                if (xmlUnderbarrelNode != null)
                                {
                                    XmlNode objXmlUnderbarrelNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlWeapon["underbarrel"].InnerText + "\"]");
                                    if (objXmlUnderbarrelNode != null)
                                    {
                                        List<Weapon> lstLoopWeapons = new List<Weapon>();
                                        Weapon objUnderbarrelWeapon = new Weapon(CharacterObject);
                                        objUnderbarrelWeapon.Create(objXmlUnderbarrelNode, lstLoopWeapons, blnCreateChildren);
                                        objWeapon.UnderbarrelWeapons.Add(objUnderbarrelWeapon);
                                        if (objWeapon.AllowAccessory == false)
                                            objUnderbarrelWeapon.AllowAccessory = false;

                                        foreach (Weapon objLoopWeapon in lstLoopWeapons)
                                        {
                                            if (objWeapon.AllowAccessory == false)
                                                objLoopWeapon.AllowAccessory = false;
                                            objWeapon.UnderbarrelWeapons.Add(objLoopWeapon);
                                        }

                                        foreach (XmlNode objXmlAccessory in xmlUnderbarrelNode.SelectNodes("accessories/accessory"))
                                        {
                                            XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                                            if (objXmlAccessoryNode != null)
                                            {
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
                        }

                        Application.DoEvents();
                    }
                }
            }

            pgbProgress.Visible = false;
            
            IsCharacterUpdateRequested = true;

            IsDirty = true;

            return frmPickPACKSKit.AddAgain;
        }

        /// <summary>
        /// Create a PACKS Kit from the character.
        /// </summary>
        public void CreatePACKSKit()
        {
            frmCreatePACKSKit frmBuildPACKSKit = new frmCreatePACKSKit(CharacterObject);
            frmBuildPACKSKit.ShowDialog(this);
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
                    decMultiplier -= 0.1m;
                if (chkInitiationOrdeal.Checked)
                    decMultiplier -= 0.1m;
                if (chkInitiationSchooling.Checked)
                    decMultiplier -= 0.1m;
                intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.CultureInfo) * decMultiplier));

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
                intAmount = decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(CharacterObjectOptions.KarmaInititationFlat + (CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation, GlobalOptions.CultureInfo) * decMultiplier));

                strInitTip = LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language).Replace("{0}", (CharacterObject.SubmersionGrade + 1).ToString()).Replace("{1}", intAmount.ToString());
            }

            GlobalOptions.ToolTipProcessor.SetToolTip(cmdAddMetamagic, strInitTip);
        }

        /// <summary>
        /// Change the character's Metatype or priority selection.
        /// </summary>
        public void ChangeMetatype()
        {
            // Determine if the character has any chosen Qualities that depend on their current Metatype. If so, don't let the change happen.
            string strQualities = string.Empty;
            foreach (Quality objQuality in CharacterObject.Qualities)
            {
                if (objQuality.OriginSource != QualitySource.Metatype && objQuality.OriginSource != QualitySource.MetatypeRemovable)
                {
                    XmlNode xmlQualityRequired = objQuality.GetNode()?["required"];
                    if (xmlQualityRequired != null)
                    {
                        if (xmlQualityRequired.SelectNodes("oneof/metatype[. = \"" + CharacterObject.Metatype + "\"]")?.Count > 0 ||
                            xmlQualityRequired.SelectNodes("oneof/metavariant[. = \"" + CharacterObject.Metavariant + "\"]")?.Count > 0)
                            strQualities += Environment.NewLine + '\t' + objQuality.DisplayNameShort(GlobalOptions.Language);
                        if (xmlQualityRequired.SelectNodes("allof/metatype[. = \"" + CharacterObject.Metatype + "\"]")?.Count > 0 ||
                            xmlQualityRequired.SelectNodes("allof/metavariant[. = \"" + CharacterObject.Metavariant + "\"]")?.Count > 0)
                            strQualities += Environment.NewLine + '\t' + objQuality.DisplayNameShort(GlobalOptions.Language);
                    }
                }
            }
            if (!string.IsNullOrEmpty(strQualities))
            {
                MessageBox.Show(LanguageManager.GetString("Message_CannotChangeMetatype", GlobalOptions.Language) + strQualities, LanguageManager.GetString("MessageTitle_CannotChangeMetatype", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Quality> lstRemoveQualities = new List<Quality>();
            //TODO: This shouldn't be required as of 17/10/16. Revert in case something weird shows up.
            /*Revert all Special Qualities
            foreach (Quality objQuality in _objCharacter.Qualities)
            {
                switch (objQuality.Name)
                {
                    case "Magician":
                        _objCharacter.MAGEnabled = false;
                        _objCharacter.MagicianEnabled = false;
                        lstRemoveQualities.Add(objQuality);
                        break;
                    case "Aspected Magician":
                        _objCharacter.MAGEnabled = false;
                        _objCharacter.MagicianEnabled = false;
                        lstRemoveQualities.Add(objQuality);
                        break;
                    case "Adept":
                        _objCharacter.MAGEnabled = false;
                        _objCharacter.AdeptEnabled = false;
                        lstRemoveQualities.Add(objQuality);
                        break;
                    case "Mystic Adept":
                        _objCharacter.MAGEnabled = false;
                        _objCharacter.MagicianEnabled = false;
                        _objCharacter.AdeptEnabled = false;
                        lstRemoveQualities.Add(objQuality);
                        break;
                    case "Technomancer":
                        _objCharacter.RESEnabled = false;
                        _objCharacter.TechnomancerEnabled = false;
                        lstRemoveQualities.Add(objQuality);
                        break;
                    default:
                        break;
                }
            }

            // Remove any Qualities the character received from their Metatype, then remove the Quality.
            foreach (Quality objQuality in lstRemoveQualities)
            {
                ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                _objCharacter.Qualities.Remove(objQuality);
            }
            lstRemoveQualities.Clear();

            int intEssenceLoss = 0;
            if (!CharacterObjectOptions.ESSLossReducesMaximumOnly)
                intEssenceLoss = _objCharacter.EssencePenalty;

            // Determine the number of points that have been put into Attributes.
            int intBOD = _objCharacter.BOD.Base - _objCharacter.BOD.MetatypeMinimum;
            int intAGI = _objCharacter.AGI.Base - _objCharacter.AGI.MetatypeMinimum;
            int intREA = _objCharacter.REA.Base - _objCharacter.REA.MetatypeMinimum;
            int intSTR = _objCharacter.STR.Base - _objCharacter.STR.MetatypeMinimum;
            int intCHA = _objCharacter.CHA.Base - _objCharacter.CHA.MetatypeMinimum;
            int intINT = _objCharacter.INT.Base - _objCharacter.INT.MetatypeMinimum;
            int intLOG = _objCharacter.LOG.Base - _objCharacter.LOG.MetatypeMinimum;
            int intWIL = _objCharacter.WIL.Base - _objCharacter.WIL.MetatypeMinimum;
            int intEDG = _objCharacter.EDG.Base - _objCharacter.EDG.MetatypeMinimum;
            int intDEP = _objCharacter.DEP.Base - _objCharacter.DEP.MetatypeMinimum;
            int intMAG = Math.Max(_objCharacter.MAG.Base - _objCharacter.MAG.MetatypeMinimum, 0);
            int intRES = Math.Max(_objCharacter.RES.Base - _objCharacter.RES.MetatypeMinimum, 0);
            */

            // Build a list of the current Metatype's Improvements to remove if the Metatype changes.
            List<Improvement> lstImprovement = CharacterObject.Improvements.Where(objImprovement => objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant || objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage).ToList();

            // Build a list of the current Metatype's Qualities to remove if the Metatype changes.
            lstRemoveQualities.AddRange(CharacterObject.Qualities.Where(objQuality => objQuality.OriginSource == QualitySource.Metatype || objQuality.OriginSource == QualitySource.MetatypeRemovable));

            if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority || CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(CharacterObject);
                frmSelectMetatype.ShowDialog(this);
                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                    return;
            }
            else
            {
                frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(CharacterObject);
                frmSelectMetatype.ShowDialog(this);

                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                    return;
            }

            // Remove any Improvements the character received from their Metatype.
            ImprovementManager.RemoveImprovements(CharacterObject, lstImprovement, false, true);

            // Remove any Qualities the character received from their Metatype, then remove the Quality.
            foreach (Quality objQuality in lstRemoveQualities)
            {
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                CharacterObject.Qualities.Remove(objQuality);
            }

            RefreshMetatypeFields();

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }
        
        /// <summary>
        /// Create a Cyberware Suite from the Cyberware the character currently has.
        /// </summary>
        private void CreateCyberwareSuite(Improvement.ImprovementSource objSource)
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
                        MessageBox.Show(LanguageManager.GetString("Message_CyberwareGradeMismatch", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_CyberwareGradeMismatch", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            // The character has no Cyberware!
            if (string.IsNullOrEmpty(strGrade))
            {
                MessageBox.Show(LanguageManager.GetString("Message_NoCyberware", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            frmCreateCyberwareSuite frmBuildCyberwareSuite = new frmCreateCyberwareSuite(CharacterObject, objSource);
            frmBuildCyberwareSuite.ShowDialog(this);
        }

        /// <summary>
        /// Set the ToolTips from the Language file.
        /// </summary>
        private void SetTooltips()
        {
            // Common Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAttributes, LanguageManager.GetString("Tip_CommonAttributes", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAttributesBase, LanguageManager.GetString("Tip_CommonAttributesBase", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAttributesAug, LanguageManager.GetString("Tip_CommonAttributesAug", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAttributesMetatype, LanguageManager.GetString("Tip_CommonAttributesMetatypeLimits", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblNuyen, string.Format(LanguageManager.GetString("Tip_CommonNuyen", GlobalOptions.Language), CharacterObjectOptions.KarmaNuyenPer));
            // Spells Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSelectedSpells, LanguageManager.GetString("Tip_SpellsSelectedSpells", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSpirits, LanguageManager.GetString("Tip_SpellsSpirits", GlobalOptions.Language));
            // Complex Forms Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblComplexForms, LanguageManager.GetString("Tip_TechnomancerComplexForms", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSprites, LanguageManager.GetString("Tip_TechnomancerSprites", GlobalOptions.Language));
            // Armor Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkArmorEquipped, LanguageManager.GetString("Tip_ArmorEquipped", GlobalOptions.Language));
            // Weapon Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkWeaponAccessoryInstalled, LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            // Gear Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkGearActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(chkCyberwareActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            // Vehicles Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkVehicleWeaponAccessoryInstalled, LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(chkVehicleActiveCommlink, LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            // Character Info Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(chkCharacterCreated, LanguageManager.GetString("Tip_CharacterCreated", GlobalOptions.Language));
            // Build Point Summary Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildPrimaryAttributes, LanguageManager.GetString("Tip_CommonAttributes", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildPositiveQualities, LanguageManager.GetString("Tip_BuildPositiveQualities", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildNegativeQualities, LanguageManager.GetString("Tip_BuildNegativeQualities", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildContacts, LanguageManager.GetString("Tip_CommonContacts", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaContact.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildEnemies, LanguageManager.GetString("Tip_CommonEnemies", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildNuyen, LanguageManager.GetString("Tip_CommonNuyen", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.NuyenPerBP.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥'));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildSkillGroups, LanguageManager.GetString("Tip_SkillsSkillGroups", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaImproveSkillGroup.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildActiveSkills, LanguageManager.GetString("Tip_SkillsActiveSkills", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaImproveActiveSkill.ToString()).Replace("{1}", CharacterObjectOptions.KarmaSpecialization.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildKnowledgeSkills, LanguageManager.GetString("Tip_SkillsKnowledgeSkills", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.FreeKnowledgeMultiplier.ToString()).Replace("{1}", CharacterObjectOptions.KarmaImproveKnowledgeSkill.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildSpells, LanguageManager.GetString("Tip_SpellsSelectedSpells", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaSpell.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildSpirits, LanguageManager.GetString("Tip_SpellsSpirits", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaSpirit.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildSprites, LanguageManager.GetString("Tip_TechnomancerSprites", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaSpirit.ToString()));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblBuildComplexForms, LanguageManager.GetString("Tip_TechnomancerComplexForms", GlobalOptions.Language).Replace("{0}", CharacterObjectOptions.KarmaNewComplexForm.ToString()));
            // Other Info Tab.
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMPhysicalLabel, LanguageManager.GetString("Tip_OtherCMPhysical", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblCMStunLabel, LanguageManager.GetString("Tip_OtherCMStun", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblINILabel, LanguageManager.GetString("Tip_OtherInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMatrixINILabel, LanguageManager.GetString("Tip_OtherMatrixInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblAstralINILabel, LanguageManager.GetString("Tip_OtherAstralInitiative", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblArmorLabel, LanguageManager.GetString("Tip_OtherArmor", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblESS, LanguageManager.GetString("Tip_OtherEssence", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblRemainingNuyenLabel, LanguageManager.GetString("Tip_OtherNuyen", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMovementLabel, LanguageManager.GetString("Tip_OtherMovement", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblSwimLabel, LanguageManager.GetString("Tip_OtherSwim", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblFlyLabel, LanguageManager.GetString("Tip_OtherFly", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblComposureLabel, LanguageManager.GetString("Tip_OtherComposure", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblJudgeIntentionsLabel, LanguageManager.GetString("Tip_OtherJudgeIntentions", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblLiftCarryLabel, LanguageManager.GetString("Tip_OtherLiftAndCarry", GlobalOptions.Language));
            GlobalOptions.ToolTipProcessor.SetToolTip(lblMemoryLabel, LanguageManager.GetString("Tip_OtherMemory", GlobalOptions.Language));

            // Reposition controls based on their new sizes.
            // Common Tab.
            txtAlias.Left = lblAlias.Left + lblAlias.Width + 6;
            txtAlias.Width = lblMetatypeLabel.Left - 6 - txtAlias.Left;
            cmdDeleteQuality.Left = cmdAddQuality.Left + cmdAddQuality.Width + 6;
            // Martial Arts Tab.
            cmdDeleteMartialArt.Left = cmdAddMartialArt.Left + cmdAddMartialArt.Width + 6;
            // Magician Tab.
            cmdDeleteSpell.Left = cmdAddSpell.Left + cmdAddSpell.Width + 6;
            // Technomancer Tab.
            cmdDeleteComplexForm.Left = cmdAddComplexForm.Left + cmdAddComplexForm.Width + 6;
            // Advanced Programs Tab.
            cmdDeleteAIProgram.Left = cmdAddAIProgram.Left + cmdAddAIProgram.Width + 6;
            // Critter Powers Tab.
            cmdDeleteCritterPower.Left = cmdAddCritterPower.Left + cmdAddCritterPower.Width + 6;
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
        }

        private void MoveControls()
        {
            // Common tab.
            lblAlias.Left = Math.Max(288, cmdDeleteQuality.Left + cmdDeleteQuality.Width + 6);
            txtAlias.Left = lblAlias.Left + lblAlias.Width + 6;
            txtAlias.Width = lblMetatypeLabel.Left - txtAlias.Left - 6;
            nudNuyen.Left = lblNuyen.Left + lblNuyen.Width + 6;
            lblNuyenTotal.Left = nudNuyen.Left + nudNuyen.Width + 6;
            lblQualityLevelLabel.Left = nudQualityLevel.Left - lblQualityLevelLabel.Width - 6;

            // Martial Arts tab.
            lblMartialArtSource.Left = lblMartialArtSourceLabel.Right + 6;

            // Spells and Spirits tab.
            int intWidth = Math.Max(lblSpellDescriptorsLabel.Width, lblSpellCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellRangeLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellDurationLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellSourceLabel.Width);

            lblSpellDescriptors.Left = lblSpellDescriptorsLabel.Left + intWidth + 6;
            lblSpellCategory.Left = lblSpellCategoryLabel.Left + intWidth + 6;
            lblSpellRange.Left = lblSpellRangeLabel.Left + intWidth + 6;
            lblSpellDuration.Left = lblSpellDurationLabel.Left + intWidth + 6;
            lblSpellSource.Left = lblSpellSourceLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblSpellTypeLabel.Width, lblSpellDamageLabel.Width);
            intWidth = Math.Max(intWidth, lblSpellDVLabel.Width);
            lblSpellTypeLabel.Left = lblSpellCategoryLabel.Left + 179;
            lblSpellType.Left = lblSpellTypeLabel.Left + intWidth + 6;
            lblSpellDamageLabel.Left = lblSpellRangeLabel.Left + 179;
            lblSpellDamage.Left = lblSpellDamageLabel.Left + intWidth + 6;
            lblSpellDVLabel.Left = lblSpellDurationLabel.Left + 179;
            lblSpellDV.Left = lblSpellDVLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblTraditionLabel.Width, lblDrainAttributesLabel.Width);
            intWidth = Math.Max(intWidth, lblMentorSpiritLabel.Width);
            cboTradition.Left = lblTraditionLabel.Left + intWidth + 6;
            cboDrain.Left = lblTraditionLabel.Left + intWidth + 6;
            lblDrainAttributes.Left = lblDrainAttributesLabel.Left + intWidth + 6;
            lblTraditionSource.Left = lblTraditionSourceLabel.Left + intWidth + 6;
            lblDrainAttributesValue.Left = lblDrainAttributes.Left + 91;
            lblMentorSpirit.Left = lblMentorSpiritLabel.Left + intWidth + 6;

            lblTraditionName.Left = cboTradition.Left + cboTradition.Width + 10;
            lblSpiritCombat.Left = cboTradition.Left + cboTradition.Width + 10;
            lblSpiritDetection.Left = cboTradition.Left + cboTradition.Width + 10;
            lblSpiritHealth.Left = cboTradition.Left + cboTradition.Width + 10;
            lblSpiritIllusion.Left = cboTradition.Left + cboTradition.Width + 10;
            lblSpiritManipulation.Left = cboTradition.Left + cboTradition.Width + 10;
            intWidth = Math.Max(lblTraditionName.Width, lblSpiritCombat.Width);
            intWidth = Math.Max(intWidth, lblSpiritDetection.Width);
            intWidth = Math.Max(intWidth, lblSpiritHealth.Width);
            intWidth = Math.Max(intWidth, lblSpiritIllusion.Width);
            intWidth = Math.Max(intWidth, lblSpiritManipulation.Width);
            txtTraditionName.Left = lblTraditionName.Left + intWidth + 6;
            cboSpiritCombat.Left = lblTraditionName.Left + intWidth + 6;
            cboSpiritDetection.Left = lblTraditionName.Left + intWidth + 6;
            cboSpiritHealth.Left = lblTraditionName.Left + intWidth + 6;
            cboSpiritIllusion.Left = lblTraditionName.Left + intWidth + 6;
            cboSpiritManipulation.Left = lblTraditionName.Left + intWidth + 6;

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

            // Advanced Programs tab.
            intLeft = lblAIProgramsRequiresLabel.Width;
            intLeft = Math.Max(intLeft, lblAIProgramsSourceLabel.Width);

            lblAIProgramsRequires.Left = lblAIProgramsRequiresLabel.Left + intLeft + 6;
            lblAIProgramsSource.Left = lblAIProgramsSourceLabel.Left + intLeft + 6;

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
            cboCyberwareGrade.Left = lblCyberwareGradeLabel.Left + intWidth + 6;
            lblCyberwareEssence.Left = lblCyberwareEssenceLabel.Left + intWidth + 6;
            lblCyberwareAvail.Left = lblCyberwareAvailLabel.Left + intWidth + 6;
            lblCyberwareSource.Left = lblCyberwareSourceLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblCyberwareESSLabel.Width, lblEssenceHoleESSLabel.Width);
            intWidth = Math.Max(intWidth, lblBiowareESSLabel.Width);
            intWidth = Math.Max(intWidth, lblPrototypeTranshumanESSLabel.Width);
            lblCyberwareESS.Left = lblCyberwareESSLabel.Left + intWidth + 6;
            lblBiowareESS.Left = lblBiowareESSLabel.Left + intWidth + 6;
            lblEssenceHoleESS.Left = lblEssenceHoleESSLabel.Left + intWidth + 6;
            lblPrototypeTranshumanESS.Left = lblPrototypeTranshumanESSLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblCyberwareRatingLabel.Width, lblCyberwareCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberwareCostLabel.Width);
            intWidth = Math.Max(intWidth, lblCyberlimbSTRLabel.Width);

            lblCyberAttackLabel.Left = lblCyberDeviceRating.Left + lblCyberDeviceRating.Width + 20;
            lblCyberAttack.Left = lblCyberAttackLabel.Left + lblCyberAttackLabel.Width + 6;
            lblCyberSleazeLabel.Left = lblCyberAttack.Left + lblCyberAttack.Width + 20;
            lblCyberSleaze.Left = lblCyberSleazeLabel.Left + lblCyberSleazeLabel.Width + 6;
            lblCyberDataProcessingLabel.Left = lblCyberSleaze.Left + lblCyberSleaze.Width + 20;
            lblCyberDataProcessing.Left = lblCyberDataProcessingLabel.Left + lblCyberDataProcessingLabel.Width + 6;
            lblCyberFirewallLabel.Left = lblCyberDataProcessing.Left + lblCyberDataProcessing.Width + 20;
            lblCyberFirewall.Left = lblCyberFirewallLabel.Left + lblCyberFirewallLabel.Width + 6;

            lblCyberwareRatingLabel.Left = cboCyberwareGrade.Left + cboCyberwareGrade.Width + 16;
            chkPrototypeTranshuman.Left = lblCyberwareRatingLabel.Left;
            nudCyberwareRating.Left = lblCyberwareRatingLabel.Left + intWidth + 6;
            lblCyberlimbAGILabel.Left = lblCyberwareRatingLabel.Left;
            lblCyberlimbSTRLabel.Left = lblCyberwareRatingLabel.Left;
            lblCyberlimbAGI.Left = lblCyberlimbAGILabel.Left + intWidth + 6;
            lblCyberlimbSTR.Left = lblCyberlimbSTRLabel.Left + intWidth + 6;
            lblCyberwareCapacityLabel.Left = cboCyberwareGrade.Left + cboCyberwareGrade.Width + 16;
            lblCyberwareCapacity.Left = lblCyberwareCapacityLabel.Left + intWidth + 6;
            lblCyberwareCostLabel.Left = cboCyberwareGrade.Left + cboCyberwareGrade.Width + 16;
            lblCyberwareCost.Left = lblCyberwareCostLabel.Left + intWidth + 6;

            // Street Gear tab.
            // Lifestyles tab.
            lblLifestyleCost.Left = lblLifestyleCostLabel.Left + lblLifestyleCostLabel.Width + 6;
            lblLifestyleSource.Left = lblLifestyleSourceLabel.Left + lblLifestyleSourceLabel.Width + 6;
            lblLifestyleTotalCost.Left = lblLifestyleMonthsLabel.Left + lblLifestyleMonthsLabel.Width + 6;
            lblLifestyleStartingNuyen.Left = lblLifestyleStartingNuyenLabel.Left + lblLifestyleStartingNuyenLabel.Width + 6;

            lblBaseLifestyle.Left = lblLifestyleComfortsLabel.Left + intWidth + 6;

            lblLifestyleQualitiesLabel.Left = lblBaseLifestyle.Left + 132;
            lblLifestyleQualities.Left = lblLifestyleQualitiesLabel.Left + 14;
            lblLifestyleQualities.Width = tabLifestyle.Width - lblLifestyleQualities.Left - 10;

            // Armor tab.
            intWidth = lblArmorLabel.Width;
            intWidth = Math.Max(intWidth, lblArmorRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblArmorCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblArmorSourceLabel.Width);

            lblArmor.Left = lblArmorLabel.Left + intWidth + 6;
            nudArmorRating.Left = lblArmorRatingLabel.Left + intWidth + 6;
            lblArmorCapacity.Left = lblArmorCapacityLabel.Left + intWidth + 6;
            lblArmorSource.Left = lblArmorSourceLabel.Left + intWidth + 6;

            lblArmorAvailLabel.Left = nudArmorRating.Left + Math.Max(nudArmorRating.Width, 50) + 6;
            lblArmorAvail.Left = lblArmorAvailLabel.Left + lblArmorAvailLabel.Width + 6;

            lblArmorCostLabel.Left = lblArmorAvail.Left + Math.Max(lblArmorAvail.Width, 50) + 6;
            lblArmorCost.Left = lblArmorCostLabel.Left + lblArmorCostLabel.Width + 6;

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

            lblWeaponAttackLabel.Left = lblWeaponDeviceRating.Left + lblWeaponDeviceRating.Width + 20;
            lblWeaponAttack.Left = lblWeaponAttackLabel.Left + lblWeaponAttackLabel.Width + 6;
            lblWeaponSleazeLabel.Left = lblWeaponAttack.Left + lblWeaponAttack.Width + 20;
            lblWeaponSleaze.Left = lblWeaponSleazeLabel.Left + lblWeaponSleazeLabel.Width + 6;
            lblWeaponDataProcessingLabel.Left = lblWeaponSleaze.Left + lblWeaponSleaze.Width + 20;
            lblWeaponDataProcessing.Left = lblWeaponDataProcessingLabel.Left + lblWeaponDataProcessingLabel.Width + 6;
            lblWeaponFirewallLabel.Left = lblWeaponDataProcessing.Left + lblWeaponDataProcessing.Width + 20;
            lblWeaponFirewall.Left = lblWeaponFirewallLabel.Left + lblWeaponFirewallLabel.Width + 6;

            lblWeaponAPLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponAP.Left = lblWeaponAPLabel.Left + intWidth + 6;
            lblWeaponAmmoLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponAmmo.Left = lblWeaponAmmoLabel.Left + intWidth + 6;
            lblWeaponConcealLabel.Left = lblWeaponRC.Left + 95;
            lblWeaponConceal.Left = lblWeaponConcealLabel.Left + intWidth + 6;
            chkWeaponAccessoryInstalled.Left = lblWeaponRC.Left + 95;

            lblWeaponDicePool.Left = lblWeaponDicePoolLabel.Left + intWidth + 6;

            // Gear tab.
            intWidth = Math.Max(lblGearNameLabel.Width, lblGearCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblGearRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblGearCapacityLabel.Width);
            intWidth = Math.Max(intWidth, lblGearQtyLabel.Width);

            chkCommlinks.Left = cmdAddLocation.Left + cmdAddLocation.Width + 16;

            lblGearName.Left = lblGearNameLabel.Left + intWidth + 6;
            lblGearCategory.Left = lblGearCategoryLabel.Left + intWidth + 6;
            nudGearRating.Left = lblGearRatingLabel.Left + intWidth + 6;
            lblGearCapacity.Left = lblGearCapacityLabel.Left + intWidth + 6;
            nudGearQty.Left = lblGearQtyLabel.Left + intWidth + 6;

            intWidth = Math.Max(lblGearDeviceRatingLabel.Width, lblGearDamageLabel.Width);
            lblGearDeviceRating.Left = lblGearDeviceRatingLabel.Left + intWidth + 6;
            lblGearDamage.Left = lblGearDamageLabel.Left + intWidth + 6;

            lblGearSource.Left = lblGearSourceLabel.Left + lblGearSourceLabel.Width + 6;
            chkGearHomeNode.Left = chkGearEquipped.Left + chkGearEquipped.Width + 16;

            // Vehicles and Drones tab.
            intWidth = Math.Max(lblVehicleNameLabel.Width, lblVehicleCategoryLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleHandlingLabel.Width);
            intWidth = Math.Max(intWidth, lblVehiclePilotLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleWeaponsmodLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleBodymodLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleRatingLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleGearQtyLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleSourceLabel.Width);
            intWidth = Math.Max(intWidth, lblVehicleAttackLabel.Width);

            lblVehicleName.Left = lblVehicleNameLabel.Left + intWidth + 6;
            lblVehicleCategory.Left = lblVehicleCategoryLabel.Left + intWidth + 6;
            lblVehicleHandling.Left = lblVehicleHandlingLabel.Left + intWidth + 6;
            cboVehicleGearAttack.Left = lblVehicleAttackLabel.Left + intWidth + 6;
            lblVehiclePilot.Left = lblVehiclePilotLabel.Left + intWidth + 6;
            lblVehicleAvail.Left = lblVehicleAvailLabel.Left + intWidth + 6;
            lblVehicleWeaponsmod.Left = lblVehicleWeaponsmodLabel.Left + intWidth + 6;
            lblVehicleBodymod.Left = lblVehicleBodymodLabel.Left + intWidth + 6;
            nudVehicleRating.Left = lblVehicleRatingLabel.Left + intWidth + 6;
            nudVehicleGearQty.Left = lblVehicleGearQtyLabel.Left + intWidth + 6;
            lblVehicleSource.Left = lblVehicleSourceLabel.Left + intWidth + 6;
            lblVehicleWeaponName.Left = lblVehicleWeaponNameLabel.Left + intWidth + 6;
            lblVehicleWeaponCategory.Left = lblVehicleWeaponCategoryLabel.Left + intWidth + 6;
            lblVehicleWeaponDamage.Left = lblVehicleWeaponDamageLabel.Left + intWidth + 6;
            lblVehicleWeaponAccuracy.Left = lblVehicleWeaponAccuracyLabel.Left + intWidth + 6;
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
            lblStreetCredTotal.Left = lblStreetCred.Left + intWidth + 6;
            lblNotorietyTotal.Left = lblNotoriety.Left + intWidth + 6;
            lblPublicAwareTotal.Left = lblPublicAware.Left + intWidth + 6;

            // Improvements tab.

            // It is not needed to work on those due to TableLayoutPanel
            //// Karma Summary tab.
            //MoveControlsTwoColumns(tabBPSummary);
            //// Other Info tab.
            //MoveControlsTwoColumns(tabOtherInfo);
            //// Spell Defence tab.
            //MoveControlsTwoColumns(tabDefences);

            lblCMPhysical.Left = lblCMPhysicalLabel.Left + intWidth + 6;
            lblCMStun.Left = lblCMPhysical.Left;
            lblINI.Left = lblCMPhysical.Left;
            lblMatrixINI.Left = lblCMPhysical.Left;
            lblAstralINI.Left = lblCMPhysical.Left;
            lblArmor.Left = lblCMPhysical.Left;
            lblESSMax.Left = lblCMPhysical.Left;
            lblRemainingNuyen.Left = lblCMPhysical.Left;
            lblComposure.Left = lblCMPhysical.Left;
            lblJudgeIntentions.Left = lblCMPhysical.Left;
            lblLiftCarry.Left = lblCMPhysical.Left;
            lblMemory.Left = lblCMPhysical.Left;
            lblMovement.Left = lblCMPhysical.Left;
            lblSwim.Left = lblCMPhysical.Left;
            lblFly.Left = lblCMPhysical.Left;

            // Relationships tab
            cmdContactsExpansionToggle.Left = cmdAddContact.Right + 6;
            cmdSwapContactOrder.Left = cmdContactsExpansionToggle.Right + 6;
            lblContactPoints_Label.Left = cmdSwapContactOrder.Right + 6;
            lblContactPoints.Left = lblContactPoints_Label.Right + 6;
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
                    if (objNode != null)
                        objNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                }
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
                    string strSelectedId = treLifestyles.SelectedNode?.Tag.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Lifestyle;
                        blnCopyEnabled = CharacterObject.Lifestyles.Any(x => x.InternalId == strSelectedId);
                    }
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    string strSelectedId = treArmor.SelectedNode?.Tag.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Armor ||
                                          (GlobalOptions.ClipboardContentType == ClipboardContentType.Gear && (CharacterObject.Armor.Any(x => x.InternalId == strSelectedId) ||
                                                                                                               CharacterObject.Armor.FindArmorMod(strSelectedId) != null ||
                                                                                                               CharacterObject.Armor.FindArmorGear(strSelectedId) != null));
                        blnCopyEnabled = CharacterObject.Armor.Any(x => x.InternalId == strSelectedId) || CharacterObject.Armor.FindArmorGear(strSelectedId) != null;
                    }
                }

                // Weapons Tab.
                if (tabStreetGearTabs.SelectedTab == tabWeapons)
                {
                    string strSelectedId = treWeapons.SelectedNode?.Tag.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        switch (GlobalOptions.ClipboardContentType)
                        {
                            case ClipboardContentType.Weapon:
                                blnPasteEnabled = true;
                                break;
                            case ClipboardContentType.Gear:
                                // Check if the copied Gear can be pasted into the selected Weapon Accessory.
                                XmlNode objXmlCategoryNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                                if (objXmlCategoryNode != null)
                                {
                                    // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
                                    WeaponAccessory objAccessory = CharacterObject.Weapons.FindWeaponAccessory(strSelectedId);
                                    XmlNodeList xmlGearCategoryList = objAccessory?.AllowGear?.SelectNodes("gearcategory");
                                    if (xmlGearCategoryList?.Count > 0)
                                    {
                                        foreach (XmlNode objAllowed in xmlGearCategoryList)
                                        {
                                            if (objAllowed.InnerText == objXmlCategoryNode.InnerText)
                                            {
                                                blnPasteEnabled = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                        blnCopyEnabled = CharacterObject.Weapons.Any(x => x.InternalId == strSelectedId) || CharacterObject.Weapons.FindWeaponGear(strSelectedId) != null;
                    }
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    string strSelectedId = treGear.SelectedNode?.Tag.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Gear;
                        blnCopyEnabled = CharacterObject.Gear.DeepFindById(strSelectedId) != null;
                    }
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                string strSelectedId = treCyberware.SelectedNode?.Tag.ToString();
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    switch (GlobalOptions.ClipboardContentType)
                    {
                        case ClipboardContentType.Gear:
                            XmlNode objXmlCategoryNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                            if (objXmlCategoryNode != null)
                            {
                                // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
                                Cyberware objCyberware = CharacterObject.Cyberware.DeepFindById(strSelectedId);
                                XmlNodeList xmlGearCategoryList = objCyberware?.AllowGear?.SelectNodes("gearcategory");
                                if (xmlGearCategoryList?.Count > 0)
                                {
                                    foreach (XmlNode objAllowed in xmlGearCategoryList)
                                    {
                                        if (objAllowed.InnerText == objXmlCategoryNode.InnerText)
                                        {
                                            blnPasteEnabled = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    blnCopyEnabled = CharacterObject.Cyberware.FindCyberwareGear(strSelectedId) != null;
                }
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                string strSelectedId = treVehicles.SelectedNode?.Tag.ToString();
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    switch (GlobalOptions.ClipboardContentType)
                    {
                        case ClipboardContentType.Vehicle:
                            blnPasteEnabled = true;
                            break;
                        case ClipboardContentType.Gear:
                            {
                                blnPasteEnabled = CharacterObject.Vehicles.Any(x => x.InternalId == strSelectedId) ||
                                                  CharacterObject.Vehicles.FindVehicleGear(strSelectedId) != null;
                                if (!blnPasteEnabled)
                                {
                                    WeaponAccessory objAccessory = null;
                                    Cyberware objCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId);
                                    if (objCyberware == null)
                                    {
                                        objAccessory = CharacterObject.Vehicles.FindVehicleWeaponAccessory(strSelectedId);
                                    }

                                    if (objAccessory != null || objCyberware != null)
                                    {
                                        XmlNode objXmlCategoryNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                                        if (objXmlCategoryNode != null)
                                        {
                                            XmlNodeList xmlGearCategoryList = objCyberware?.AllowGear?.SelectNodes("gearcategory");
                                            if (xmlGearCategoryList?.Count > 0)
                                            {
                                                foreach (XmlNode objAllowed in xmlGearCategoryList)
                                                {
                                                    if (objAllowed.InnerText == objXmlCategoryNode.InnerText)
                                                    {
                                                        blnPasteEnabled = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                xmlGearCategoryList = objAccessory?.AllowGear?.SelectNodes("gearcategory");
                                                if (xmlGearCategoryList?.Count > 0)
                                                {
                                                    foreach (XmlNode objAllowed in xmlGearCategoryList)
                                                    {
                                                        if (objAllowed.InnerText == objXmlCategoryNode.InnerText)
                                                        {
                                                            blnPasteEnabled = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case ClipboardContentType.Weapon:
                            WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(strSelectedId, out Vehicle _);
                            if (objWeaponMount != null)
                            {
                                blnPasteEnabled = true;
                            }
                            else
                            {
                                VehicleMod objVehicleMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strSelectedId);
                                if (objVehicleMod != null)
                                {
                                    // TODO: Make this not depend on string names
                                    if (objVehicleMod.Name.StartsWith("Mechanical Arm") || objVehicleMod.Name.Contains("Drone Arm"))
                                    {
                                        blnPasteEnabled = true;
                                    }
                                }
                            }

                            break;
                    }

                    blnCopyEnabled = CharacterObject.Vehicles.Any(x => x.InternalId == strSelectedId) ||
                                     CharacterObject.Vehicles.FindVehicleGear(strSelectedId) != null ||
                                     CharacterObject.Vehicles.FindVehicleWeapon(strSelectedId) != null;
                }
            }

            mnuEditPaste.Enabled = blnPasteEnabled;
            tsbPaste.Enabled = blnPasteEnabled;
            mnuEditCopy.Enabled = blnCopyEnabled;
            tsbCopy.Enabled = blnCopyEnabled;
        }

        private void AddCyberwareSuite(Improvement.ImprovementSource objSource)
        {
            frmSelectCyberwareSuite frmPickCyberwareSuite = new frmSelectCyberwareSuite(CharacterObject, objSource);
            frmPickCyberwareSuite.ShowDialog(this);

            if (frmPickCyberwareSuite.DialogResult == DialogResult.Cancel)
                return;

            string strType = objSource == Improvement.ImprovementSource.Cyberware ? "cyberware" : "bioware";
            XmlDocument objXmlDocument = XmlManager.Load(strType + ".xml", string.Empty, true);
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
                        "/chummer/gears/gear[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + strName +
                        "\" and category = \"" + strCategory + "\"]");
                else
                    objXmlGearNode = objXmlGearDocument.SelectSingleNode(
                        "/chummer/gears/gear[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + strName +
                        "\"]");
            }

            if (objXmlGearNode == null)
                return null;

            int intRating = Convert.ToInt32(objXmlGear["rating"]?.InnerText);
            decimal decQty = 1;
            string strQty = objXmlGear["qty"]?.InnerText;
            if (!string.IsNullOrEmpty(strQty))
                decQty = Convert.ToDecimal(strQty, GlobalOptions.InvariantCultureInfo);

            List<Weapon> lstWeapons = new List<Weapon>();
            string strForceValue = objXmlGear.SelectSingleNode("name/@select")?.InnerText ?? string.Empty;

            Gear objNewGear = new Gear(CharacterObject);
            objNewGear.Create(objXmlGearNode, intRating, lstWeapons, strForceValue, true, blnCreateChildren);
            objNewGear.Quantity = decQty;

            if (objParentObject is Character objParentCharacter)
                objParentCharacter.Gear.Add(objNewGear);
            else if (objParentObject is Gear objParentGear)
                objParentGear.Children.Add(objNewGear);
            else if (objParentObject is Armor objParentArmor)
                objParentArmor.Gear.Add(objNewGear);
            else if (objParentObject is ArmorMod objParentArmorMod)
                objParentArmorMod.Gear.Add(objNewGear);
            else if (objParentObject is WeaponAccessory objParentWeaponAccessory)
                objParentWeaponAccessory.Gear.Add(objNewGear);
            else if (objParentObject is Cyberware objParentCyberware)
                objParentCyberware.Gear.Add(objNewGear);
            else if (objParentObject is Vehicle objParentVehicle)
                objParentVehicle.Gear.Add(objNewGear);

            // Look for child components.
            using (XmlNodeList xmlChildrenList = objXmlGear.SelectNodes("gears/gear"))
                if (xmlChildrenList?.Count > 0)
                    foreach (XmlNode xmlChild in xmlChildrenList)
                    {
                        AddPACKSGear(objXmlGearDocument, xmlChild, objNewGear, blnCreateChildren);
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
            Grade objGrade = Cyberware.ConvertToCyberwareGrade(xmlCyberware["grade"]?.InnerText, Improvement.ImprovementSource.Cyberware, CharacterObject);

            int intRating = Convert.ToInt32(xmlCyberware["rating"]?.InnerText);

            Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Cyberware;
            string strName = xmlCyberware["name"]?.InnerText;
            if (string.IsNullOrEmpty(strName))
                return;

            XmlNode objXmlCyberwareNode = xmlCyberwareDocument.SelectSingleNode("/chummer/cyberwares/cyberware[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + strName + "\"]");
            if (objXmlCyberwareNode == null)
            {
                eSource = Improvement.ImprovementSource.Bioware;
                objXmlCyberwareNode = xmlBiowareDocument.SelectSingleNode("/chummer/biowares/bioware[(" + CharacterObjectOptions.BookXPath() + ") and name = \"" + strName + "\"]");
                if (objXmlCyberwareNode == null)
                {
                    return;
                }
            }
            List<Weapon> lstWeapons = new List<Weapon>();
            List<Vehicle> lstVehicles = new List<Vehicle>();
            Cyberware objCyberware = new Cyberware(CharacterObject);
            objCyberware.Create(objXmlCyberwareNode, CharacterObject, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, blnCreateChildren);

            if (objParentObject is Character objParentCharacter)
                objParentCharacter.Cyberware.Add(objCyberware);
            else if (objParentObject is Cyberware objParentCyberware)
                objParentCyberware.Children.Add(objCyberware);
            else if (objParentObject is VehicleMod objParentVehicleMod)
                objParentVehicleMod.Cyberware.Add(objCyberware);

            // Add any children.
            using (XmlNodeList xmlCyberwareList = xmlCyberware.SelectNodes("cyberwares/cyberware"))
                if (xmlCyberwareList?.Count > 0)
                    foreach (XmlNode objXmlChild in xmlCyberwareList)
                        AddPACKSCyberware(xmlCyberwareDocument, xmlBiowareDocument, xmlGearDocument, objXmlChild, objCyberware, blnCreateChildren);

            using (XmlNodeList xmlGearList = xmlCyberware.SelectNodes("gears/gear"))
                if (xmlGearList?.Count > 0)
                    foreach (XmlNode objXmlGear in xmlGearList)
                        AddPACKSGear(xmlGearDocument, objXmlGear, objCyberware, blnCreateChildren);

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
#endregion
        
        private void tsMetamagicAddMetamagic_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.InternalId == treMetamagic.SelectedNode.Tag.ToString())
                {
                    intGrade = objGrade.Grade;
                    break;
                }
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

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddArt_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.InternalId == treMetamagic.SelectedNode.Tag.ToString())
                {
                    intGrade = objGrade.Grade;
                    break;
                }
            }

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Art);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("metamagic.xml").SelectSingleNode("/chummer/arts/art[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Metamagic;

            Art objArt = new Art(CharacterObject);

            objArt.Create(objXmlArt, objSource);
            objArt.Grade = intGrade;
            if (objArt.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Arts.Add(objArt);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddEnchantment_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.InternalId == treMetamagic.SelectedNode.Tag.ToString())
                {
                    intGrade = objGrade.Grade;
                    break;
                }
            }

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Enchantment);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            Spell objNewSpell = new Spell(CharacterObject);

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddRitual_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.InternalId == treMetamagic.SelectedNode.Tag.ToString())
                {
                    intGrade = objGrade.Grade;
                    break;
                }
            }

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Ritual);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            Spell objNewSpell = new Spell(CharacterObject);

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = intGrade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsInitiationNotes_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode == null)
                return;
            // Locate the selected Metamagic.
            Metamagic objMetamagic = CharacterObject.Metamagics.FindById(treMetamagic.SelectedNode.Tag.ToString());
            if (objMetamagic != null)
            {
                string strOldValue = objMetamagic.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objMetamagic.Notes = frmItemNotes.Notes;
                    if (objMetamagic.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMetamagic.SelectedNode.ForeColor = objMetamagic.PreferredColor;
                        treMetamagic.SelectedNode.ToolTipText = objMetamagic.Notes.WordWrap(100);
                    }
                }
                return;
            }

            // Locate the selected Art.
            Art objArt = CharacterObject.Arts.FindById(treMetamagic.SelectedNode.Tag.ToString());
            if (objArt != null)
            {
                string strOldValue = objArt.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objArt.Notes = frmItemNotes.Notes;
                    if (objArt.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMetamagic.SelectedNode.ForeColor = objArt.PreferredColor;
                        treMetamagic.SelectedNode.ToolTipText = objArt.Notes.WordWrap(100);
                    }
                }
                return;
            }

            // Locate the selected Spell.
            Spell objSpell = CharacterObject.Spells.FindById(treMetamagic.SelectedNode.Tag.ToString());
            if (objSpell != null)
            {
                string strOldValue = objSpell.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objSpell.Notes = frmItemNotes.Notes;
                    if (objSpell.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMetamagic.SelectedNode.ForeColor = objSpell.PreferredColor;
                        treMetamagic.SelectedNode.ToolTipText = objSpell.Notes;

                        TreeNode nodSpell = treSpells.FindNode(treMetamagic.SelectedNode.Tag.ToString());
                        if (nodSpell != null)
                        {
                            nodSpell.ForeColor = objSpell.PreferredColor;
                            nodSpell.ToolTipText = objSpell.Notes.WordWrap(100);
                        }
                    }
                }
            }
        }

        private void tsMetamagicAddEnhancement_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode.Level != 0)
                return;

            int intGrade = 0;
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.InternalId == treMetamagic.SelectedNode.Tag.ToString())
                {
                    intGrade = objGrade.Grade;
                    break;
                }
            }
            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Enhancement);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/enhancements/enhancement[id = \"" + frmPickArt.SelectedItem + "\"]");
            if (objXmlArt == null)
                return;
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            Enhancement objEnhancement = new Enhancement(CharacterObject);
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

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void panContacts_Click(object sender, EventArgs e)
        {
            panContacts.Focus();
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
        
        private void panEnemies_Click(object sender, EventArgs e)
        {
            panEnemies.Focus();
        }

        private void tsAddTechniqueNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode == null)
                return;
            MartialArtTechnique objTechnique = CharacterObject.MartialArts.FindMartialArtTechnique(treMartialArts.SelectedNode.Tag.ToString());
            if (objTechnique != null)
            {
                string strOldValue = objTechnique.Notes;
                frmNotes frmItemNotes = new frmNotes
                {
                    Notes = strOldValue
                };
                frmItemNotes.ShowDialog(this);

                if (frmItemNotes.DialogResult == DialogResult.OK)
                {
                    objTechnique.Notes = frmItemNotes.Notes;
                    if (objTechnique.Notes != strOldValue)
                    {
                        IsDirty = true;

                        treMartialArts.SelectedNode.ForeColor = objTechnique.PreferredColor;
                        treMartialArts.SelectedNode.ToolTipText = objTechnique.Notes.WordWrap(100);
                    }
                }
            }
        }

        private void txtBackground_KeyPress(object sender, KeyPressEventArgs e)
        {
            btnCreateBackstory.Enabled = _objStoryBuilder == null;
        }

        private void btnCreateBackstory_Click(object sender, EventArgs e)
        {
            if (_objStoryBuilder == null)
                _objStoryBuilder = new StoryBuilder(CharacterObject);
            CharacterObject.Background = _objStoryBuilder.GetStory(GlobalOptions.Language);
        }
        
        private void chkInitiationSchooling_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tssLimitModifierEdit_Click(object sender, EventArgs e)
        {
            UpdateLimitModifier(treLimit);
        }

        private void mnuSpecialConfirmValidity_Click(object sender, EventArgs e)
        {
            if (CheckCharacterValidity())
            {
                MessageBox.Show(LanguageManager.GetString("Message_ValidCharacter", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_ValidCharacter", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cboPrimaryArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || CharacterObject.Ambidextrous)
                return;
            CharacterObject.PrimaryArm = cboPrimaryArm.SelectedValue.ToString();

            IsDirty = true;
        }

        private void AttributeCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshAttributes(pnlAttributes, notifyCollectionChangedEventArgs);
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

        private void LifestyleCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshLifestyles(treLifestyles, cmsLifestyleNotes, cmsAdvancedLifestyle, notifyCollectionChangedEventArgs);
        }

        private void LimitModifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            RefreshLimitModifiers(treLimit, cmsLimitModifier, notifyCollectionChangedEventArgs);
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
            if (picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width)
                picMugshot.SizeMode = PictureBoxSizeMode.CenterImage;
            else
                picMugshot.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void mnuSpecialKarmaValue_Click(object sender, EventArgs e)
        {
            MessageBox.Show(CharacterObject.CalculateKarmaValue(GlobalOptions.Language, out int _),
                LanguageManager.GetString("MessageTitle_KarmaValue", GlobalOptions.Language),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cmdCyberwareChangeMount_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode == null)
                return;
            Cyberware objModularCyberware = CharacterObject.Cyberware.DeepFindById(treCyberware.SelectedNode.Tag.ToString());
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
            string strSelectedId = treVehicles.SelectedNode.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
                return;

            Cyberware objModularCyberware = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strSelectedId, out VehicleMod objOldParentVehicleMod);
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

        private void tsWeaponLocationAddWeapon_Click(object sender, EventArgs e)
        {
            string strSelectedLocation = treWeapons.SelectedNode?.Tag.ToString() ?? string.Empty;
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddWeapon(strSelectedLocation);
            }
            while (blnAddAgain);
        }

        private void tsGearLocationAddGear_Click(object sender, EventArgs e)
        {
            string strSelectedId = treGear.SelectedNode?.Tag.ToString();
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(strSelectedId);
            }
            while (blnAddAgain);
        }

        private void tsVehicleLocationAddVehicle_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddVehicle(objSelectedNode);
            }
            while (blnAddAgain);
        }

        private void tsEditWeaponMount_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;
            WeaponMount objWeaponMount = CharacterObject.Vehicles.FindVehicleWeaponMount(objSelectedNode.Tag.ToString(), out Vehicle objVehicle);
            if (objWeaponMount == null)
                return;
            frmCreateWeaponMount frmCreateWeaponMount = new frmCreateWeaponMount(objVehicle, CharacterObject, objWeaponMount);
            frmCreateWeaponMount.ShowDialog(this);

            if (frmCreateWeaponMount.DialogResult != DialogResult.Cancel)
            {
                if (frmCreateWeaponMount.FreeCost)
                    frmCreateWeaponMount.WeaponMount.Cost = "0";

                IsCharacterUpdateRequested = true;
                
                IsDirty = true;
            }
        }
    }
}
