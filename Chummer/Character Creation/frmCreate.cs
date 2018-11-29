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
using System.Collections.ObjectModel;
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
using Chummer.Backend.Uniques;

namespace Chummer
{
    [DesignerCategory("Form")]
    public partial class frmCreate : CharacterShared
    {
        // Set the default culture to en-US so we work with decimals correctly.
        private bool _blnSkipUpdate;
        private bool _blnSkipToolStripRevert;
        private bool _blnReapplyImprovements;
        private bool _blnFreestyle;
        private int _intDragLevel;
        private MouseButtons _eDragButton = MouseButtons.None;
        private bool _blnDraggingGear;
        private StoryBuilder _objStoryBuilder;

        public TreeView FociTree => treFoci;
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
            lmtControl.MakeDirtyWithCharacterUpdate += MakeDirtyWithCharacterUpdate;
            lmtControl.MakeDirty += MakeDirty;

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

            SuspendLayout();

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
                    CharacterObject.GameplayOption = GlobalOptions.DefaultGameplayOption;
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

                mnuSpecialChangeMetatype.Tag = "Menu_SpecialChangePriorities";
                mnuSpecialChangeMetatype.Text = LanguageManager.GetString("Menu_SpecialChangePriorities");
            }
            
            lblNuyenTotal.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayTotalStartingNuyen));
            lblAttributesBase.Visible = CharacterObject.BuildMethodHasSkillPoints;

            txtGroupName.DataBindings.Add("Text", CharacterObject, nameof(Character.GroupName), false, DataSourceUpdateMode.OnPropertyChanged);
            txtGroupNotes.DataBindings.Add("Text", CharacterObject, nameof(Character.GroupNotes), false, DataSourceUpdateMode.OnPropertyChanged);
            chkJoinGroup.Checked = CharacterObject.GroupMember;

            txtCharacterName.DoDatabinding("Text", CharacterObject, nameof(Character.Name));
            txtSex.DoDatabinding("Text", CharacterObject, nameof(Character.Sex));
            txtAge.DoDatabinding("Text", CharacterObject, nameof(Character.Age));
            txtEyes.DoDatabinding("Text", CharacterObject, nameof(Character.Eyes));
            txtHeight.DoDatabinding("Text", CharacterObject, nameof(Character.Height));
            txtWeight.DoDatabinding("Text", CharacterObject, nameof(Character.Weight));
            txtSkin.DoDatabinding("Text", CharacterObject, nameof(Character.Skin));
            txtHair.DoDatabinding("Text", CharacterObject, nameof(Character.Hair));
            txtDescription.DoDatabinding("Text", CharacterObject, nameof(Character.Description));
            txtBackground.DoDatabinding("Text", CharacterObject, nameof(Character.Background));
            txtConcept.DoDatabinding("Text", CharacterObject, nameof(Character.Concept));
            txtNotes.DoDatabinding("Text", CharacterObject, nameof(Character.Notes));
            txtAlias.DoDatabinding("Text", CharacterObject, nameof(Character.Alias));
            txtPlayerName.DoDatabinding("Text", CharacterObject, nameof(Character.PlayerName));

            tssBPLabel.Text = LanguageManager.GetString("Label_Karma", GlobalOptions.Language);
            tssBPRemainLabel.Text = LanguageManager.GetString("Label_KarmaRemaining", GlobalOptions.Language);
            tabBPSummary.Text = LanguageManager.GetString("Tab_BPSummary_Karma", GlobalOptions.Language);
            lblQualityBPLabel.Text = LanguageManager.GetString("Label_Karma", GlobalOptions.Language);


            // Set the visibility of the Bioware Suites menu options.
            mnuSpecialAddBiowareSuite.Visible = CharacterObjectOptions.AllowBiowareSuites;
            mnuSpecialCreateBiowareSuite.Visible = CharacterObjectOptions.AllowBiowareSuites;

            chkJoinGroup.DoDatabinding("Checked", CharacterObject, nameof(Character.GroupMember));
            chkInitiationGroup.DoDatabinding("Enabled", CharacterObject, nameof(Character.GroupMember));

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

            cmdAddMetamagic.DoDatabinding("Enabled", CharacterObject, nameof(Character.AddInitiationsAllowed));

            if (CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                cmdLifeModule.Visible = true;
                btnCreateBackstory.Visible = CharacterObjectOptions.AutomaticBackstory;
            }

            if (!CharacterObjectOptions.BookEnabled("RF"))
            {
                cmdAddLifestyle.SplitMenuStrip = null;
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
            RefreshDrugs(treCustomDrugs);
            
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

            // Populate the Magician Traditions list.
            XPathNavigator xmlTraditionsBaseChummerNode = XmlManager.Load("traditions.xml").GetFastNavigator().SelectSingleNode("/chummer");
            List<ListItem> lstTraditions = new List<ListItem>();
            if (xmlTraditionsBaseChummerNode != null)
            {
                foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select("traditions/tradition[" + CharacterObjectOptions.BookXPath() + "]"))
                {
                    string strName = xmlTradition.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        lstTraditions.Add(new ListItem(xmlTradition.SelectSingleNode("id")?.Value ?? strName, xmlTradition.SelectSingleNode("translate")?.Value ?? strName));
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
            cboDrain.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.DrainExpression));
            cboDrain.EndUpdate();

            lblDrainAttributes.DoDatabinding("Text", CharacterObject.MagicTradition, nameof(Tradition.DisplayDrainExpression));
            lblDrainAttributesValue.DoDatabinding("Text", CharacterObject.MagicTradition, nameof(Tradition.DrainValue));
            lblDrainAttributesValue.DoDatabinding("ToolTipText", CharacterObject.MagicTradition, nameof(Tradition.DrainValueToolTip));

            lblFadingAttributes.DoDatabinding("Text", CharacterObject.MagicTradition, nameof(Tradition.DisplayDrainExpression));
            lblFadingAttributesValue.DoDatabinding("Text", CharacterObject.MagicTradition, nameof(Tradition.DrainValue));
            lblFadingAttributesValue.DoDatabinding("ToolTipText", CharacterObject.MagicTradition, nameof(Tradition.DrainValueToolTip));

            HashSet<string> limit = new HashSet<string>();
            foreach (Improvement improvement in CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LimitSpiritCategory && x.Enabled))
            {
                limit.Add(improvement.ImprovedName);
            }

            /* Populate drugs. //TODO: fix
            foreach (Drug objDrug in CharacterObj.Drugs)
            {
                treCustomDrugs.Add(objDrug);
            }*/

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
            cboSpiritCombat.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.SpiritCombat));
            lblSpiritCombat.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritCombat.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritCombat.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
            cboSpiritCombat.EndUpdate();

            List<ListItem> lstDetection = new List<ListItem>(lstSpirit);
            cboSpiritDetection.BeginUpdate();
            cboSpiritDetection.ValueMember = "Value";
            cboSpiritDetection.DisplayMember = "Name";
            cboSpiritDetection.DataSource = lstDetection;
            cboSpiritDetection.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.SpiritDetection));
            lblSpiritDetection.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritDetection.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritDetection.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
            cboSpiritDetection.EndUpdate();

            List<ListItem> lstHealth = new List<ListItem>(lstSpirit);
            cboSpiritHealth.BeginUpdate();
            cboSpiritHealth.ValueMember = "Value";
            cboSpiritHealth.DisplayMember = "Name";
            cboSpiritHealth.DataSource = lstHealth;
            cboSpiritHealth.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.SpiritHealth));
            lblSpiritHealth.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritHealth.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritHealth.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
            cboSpiritHealth.EndUpdate();

            List<ListItem> lstIllusion = new List<ListItem>(lstSpirit);
            cboSpiritIllusion.BeginUpdate();
            cboSpiritIllusion.ValueMember = "Value";
            cboSpiritIllusion.DisplayMember = "Name";
            cboSpiritIllusion.DataSource = lstIllusion;
            cboSpiritIllusion.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.SpiritIllusion));
            lblSpiritIllusion.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritIllusion.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritIllusion.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
            cboSpiritIllusion.EndUpdate();

            List<ListItem> lstManip = new List<ListItem>(lstSpirit);
            cboSpiritManipulation.BeginUpdate();
            cboSpiritManipulation.ValueMember = "Value";
            cboSpiritManipulation.DisplayMember = "Name";
            cboSpiritManipulation.DataSource = lstManip;
            cboSpiritManipulation.DoDatabinding("SelectedValue", CharacterObject.MagicTradition, nameof(Tradition.SpiritManipulation));
            lblSpiritManipulation.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritManipulation.Visible = CharacterObject.MagicTradition.Type != TraditionType.None;
            cboSpiritManipulation.Enabled = CharacterObject.MagicTradition.IsCustomTradition;
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
                        lstStreams.Add(new ListItem(xmlTradition.SelectSingleNode("id")?.Value ?? strName, xmlTradition.SelectSingleNode("translate")?.Value ?? strName));
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

            nudMysticAdeptMAGMagician.DoDatabinding("Maximum", CharacterObject.MAG, nameof(CharacterAttrib.TotalValue));
            nudMysticAdeptMAGMagician.DoDatabinding("Value",   CharacterObject, nameof(Character.MysticAdeptPowerPoints));
            
            IsLoading = false;

            // Select the Magician's Tradition.
            if (CharacterObject.MagicTradition.Type == TraditionType.MAG)
                cboTradition.SelectedValue = CharacterObject.MagicTradition.SourceID;
            else if (cboTradition.SelectedIndex == -1 && cboTradition.Items.Count > 0)
                cboTradition.SelectedIndex = 0;

            txtTraditionName.DoDatabinding("Text", CharacterObject.MagicTradition, nameof(Tradition.Name));

            // Select the Technomancer's Stream.
            if (CharacterObject.MagicTradition.Type == TraditionType.RES)
                cboStream.SelectedValue = CharacterObject.MagicTradition.SourceID;
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

            nudNuyen.DoDatabinding("Value", CharacterObject, nameof(Character.NuyenBP));
            nudNuyen.DoDatabinding("Maximum", CharacterObject, nameof(Character.TotalNuyenMaximumBP));

            lblCMPhysical.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.PhysicalCMToolTip));
            lblCMPhysical.DoDatabinding("Text", CharacterObject, nameof(Character.PhysicalCM));
            lblCMPhysicalLabel.DoDatabinding("Text", CharacterObject, nameof(Character.PhysicalCMLabelText));
            lblCMStun.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.StunCMToolTip));
            lblCMStun.DoDatabinding("Text", CharacterObject, nameof(Character.StunCM));
            lblCMStun.DoDatabinding("Visible", CharacterObject, nameof(Character.StunCMVisible));
            lblCMStunLabel.DoDatabinding("Text", CharacterObject, nameof(Character.StunCMLabelText));

            lblESSMax.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayEssence));
            lblCyberwareESS.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayCyberwareEssence));
            lblBiowareESS.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayBiowareEssence));
            lblEssenceHoleESS.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayEssenceHole));

            lblPrototypeTranshumanESS.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayPrototypeTranshumanEssenceUsed));

            lblArmor.DoDatabinding("Text", CharacterObject, nameof(Character.TotalArmorRating));
            lblArmor.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.TotalArmorRatingToolTip));

            lblSpellDefenceIndirectDodge.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIndirectDodge));
            lblSpellDefenceIndirectDodge.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIndirectDodgeToolTip));
            lblSpellDefenceIndirectSoak.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIndirectSoak));
            lblSpellDefenceIndirectSoak.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIndirectSoakToolTip));
            lblSpellDefenceDirectSoakMana.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakMana));
            lblSpellDefenceDirectSoakMana.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseDirectSoakManaToolTip));
            lblSpellDefenceDirectSoakPhysical.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseDirectSoakPhysical));
            lblSpellDefenceDirectSoakPhysical.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseDirectSoakPhysicalToolTip));

            lblSpellDefenceDetection.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDetection));
            lblSpellDefenceDetection.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDetectionToolTip));
            lblSpellDefenceDecAttBOD.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseBOD));
            lblSpellDefenceDecAttBOD.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseBODToolTip));
            lblSpellDefenceDecAttAGI.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseAGI));
            lblSpellDefenceDecAttAGI.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseAGIToolTip));
            lblSpellDefenceDecAttREA.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseREA));
            lblSpellDefenceDecAttREA.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseREAToolTip));
            lblSpellDefenceDecAttSTR.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseSTR));
            lblSpellDefenceDecAttSTR.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseSTRToolTip));
            lblSpellDefenceDecAttCHA.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseCHA));
            lblSpellDefenceDecAttCHA.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseCHAToolTip));
            lblSpellDefenceDecAttINT.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseINT));
            lblSpellDefenceDecAttINT.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseINTToolTip));
            lblSpellDefenceDecAttLOG.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseLOG));
            lblSpellDefenceDecAttLOG.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseLOGToolTip));
            lblSpellDefenceDecAttWIL.DoDatabinding("Text",           CharacterObject, nameof(Character.DisplaySpellDefenseDecreaseWIL));
            lblSpellDefenceDecAttWIL.DoDatabinding("ToolTipText",    CharacterObject, nameof(Character.SpellDefenseDecreaseWILToolTip));

            lblSpellDefenceIllusionMana.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIllusionMana));
            lblSpellDefenceIllusionMana.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIllusionManaToolTip));
            lblSpellDefenceIllusionPhysical.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseIllusionPhysical));
            lblSpellDefenceIllusionPhysical.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseIllusionPhysicalToolTip));
            lblSpellDefenceManipMental.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseManipulationMental));
            lblSpellDefenceManipMental.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseManipulationMentalToolTip));
            lblSpellDefenceManipPhysical.DoDatabinding("Text",         CharacterObject, nameof(Character.DisplaySpellDefenseManipulationPhysical));
            lblSpellDefenceManipPhysical.DoDatabinding("ToolTipText",  CharacterObject, nameof(Character.SpellDefenseManipulationPhysicalToolTip));
            nudCounterspellingDice.DoDatabinding("Value",        CharacterObject, nameof(Character.CurrentCounterspellingDice));

            lblMovement.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayMovement));
            lblSwim.DoDatabinding("Text", CharacterObject, nameof(Character.DisplaySwim));
            lblFly.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayFly));

            lblRemainingNuyen.DoDatabinding("Text", CharacterObject, nameof(Character.DisplayNuyen));

            lblStreetCredTotal.DoDatabinding("Text", CharacterObject, nameof(Character.TotalStreetCred));
            lblStreetCredTotal.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.StreetCredTooltip));
            lblNotorietyTotal.DoDatabinding("Text", CharacterObject, nameof(Character.TotalNotoriety));
            lblNotorietyTotal.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.NotorietyTooltip));
            lblPublicAwareTotal.DoDatabinding("Text", CharacterObject, nameof(Character.TotalPublicAwareness));
            lblPublicAwareTotal.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.PublicAwarenessTooltip));

            lblMentorSpirit.DoDatabinding("Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayName));
            lblMentorSpiritInformation.DoDatabinding("Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayInformation));
            lblParagon.DoDatabinding("Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayName));
            lblParagonInformation.DoDatabinding("Text", CharacterObject, nameof(Character.FirstMentorSpiritDisplayInformation));

            lblComposure.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.ComposureToolTip));
            lblComposure.DoDatabinding("Text", CharacterObject, nameof(Character.Composure));
            lblJudgeIntentions.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.JudgeIntentionsToolTip));
            lblJudgeIntentions.DoDatabinding("Text", CharacterObject, nameof(Character.JudgeIntentions));
            lblLiftCarry.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.LiftAndCarryToolTip));
            lblLiftCarry.DoDatabinding("Text", CharacterObject, nameof(Character.LiftAndCarry));
            lblMemory.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.MemoryToolTip));
            lblMemory.DoDatabinding("Text", CharacterObject, nameof(Character.Memory));

            lblINI.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.InitiativeToolTip));
            lblINI.DoDatabinding("Text", CharacterObject, nameof(Character.Initiative));
            lblAstralINI.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.AstralInitiativeToolTip));
            lblAstralINI.DoDatabinding("Text", CharacterObject, nameof(Character.AstralInitiative));
            lblMatrixINI.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.MatrixInitiativeToolTip));
            lblMatrixINI.DoDatabinding("Text", CharacterObject, nameof(Character.MatrixInitiative));
            lblMatrixINICold.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.MatrixInitiativeColdToolTip));
            lblMatrixINICold.DoDatabinding("Text", CharacterObject, nameof(Character.MatrixInitiativeCold));
            lblMatrixINIHot.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.MatrixInitiativeHotToolTip));
            lblMatrixINIHot.DoDatabinding("Text", CharacterObject, nameof(Character.MatrixInitiativeHot));
            lblRiggingINI.DoDatabinding("ToolTipText", CharacterObject, nameof(Character.InitiativeToolTip));
            lblRiggingINI.DoDatabinding("Text", CharacterObject, nameof(Character.Initiative));

            cmdAddCyberware.DoDatabinding("Enabled", CharacterObject, nameof(Character.AddCyberwareEnabled));
            cmdAddBioware.DoDatabinding("Enabled", CharacterObject, nameof(Character.AddBiowareEnabled));
            
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

            ResumeLayout();
            Cursor = Cursors.Default;
        }

        private void frmCreate_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If there are unsaved changes to the character, as the user if they would like to save their changes.
            if (IsDirty)
            {
                string strCharacterName = CharacterObject.CharacterName;
                DialogResult objResult = MessageBox.Show(string.Format(LanguageManager.GetString("Message_UnsavedChanges", GlobalOptions.Language), strCharacterName),
                    LanguageManager.GetString("MessageTitle_UnsavedChanges", GlobalOptions.Language), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
                IsLoading = true;
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
                case nameof(Character.MagicTradition):
                case nameof(Character.GroupMember):
                    IsCharacterUpdateRequested = true;
                    break;
                case nameof(Character.MAGEnabled):
                    {
                        if (CharacterObject.MAGEnabled)
                        {
                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);

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
                            gpbInitiationType.Text = LanguageManager.GetString("String_InitiationType", GlobalOptions.Language);
                            gpbInitiationGroup.Text = LanguageManager.GetString("String_InitiationGroup", GlobalOptions.Language);

                            tsMetamagicAddArt.Visible = true;
                            tsMetamagicAddEnchantment.Visible = true;
                            tsMetamagicAddEnhancement.Visible = true;
                            tsMetamagicAddRitual.Visible = true;

                            string strInitTip = string.Format(LanguageManager.GetString("Tip_ImproveInitiateGrade", GlobalOptions.Language)
                                , (CharacterObject.InitiateGrade + 1).ToString(GlobalOptions.CultureInfo)
                                , (CharacterObjectOptions.KarmaInititationFlat + ((CharacterObject.InitiateGrade + 1) * CharacterObjectOptions.KarmaInitiation)).ToString(GlobalOptions.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedGroup", GlobalOptions.Language);
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_GroupInitiation", GlobalOptions.Language);

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
                            if (!CharacterObject.RESEnabled)
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
                            if (!CharacterObjectOptions.ESSLossReducesMaximumOnly)
                                intEssenceLoss = _objCharacter.EssencePenalty;
                            // If the character options permit submersion in create mode, show the Initiation page.
                            */
                            UpdateInitiationCost();

                            if (!tabCharacterTabs.TabPages.Contains(tabInitiation))
                                tabCharacterTabs.TabPages.Insert(3, tabInitiation);

                            tabInitiation.Text = LanguageManager.GetString("Tab_Submersion", GlobalOptions.Language);
                            tsMetamagicAddMetamagic.Text = LanguageManager.GetString("Button_AddEcho", GlobalOptions.Language);
                            cmdAddMetamagic.Text = LanguageManager.GetString("Button_AddSubmersionGrade", GlobalOptions.Language);
                            chkInitiationOrdeal.Text = LanguageManager.GetString("Checkbox_SubmersionTask", GlobalOptions.Language);
                            gpbInitiationType.Text = LanguageManager.GetString("String_SubmersionType", GlobalOptions.Language);
                            gpbInitiationGroup.Text = LanguageManager.GetString("String_SubmersionNetwork", GlobalOptions.Language);

                            tsMetamagicAddArt.Visible = false;
                            tsMetamagicAddEnchantment.Visible = false;
                            tsMetamagicAddEnhancement.Visible = false;
                            tsMetamagicAddRitual.Visible = false;

                            string strInitTip = string.Format(LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language)
                                , (CharacterObject.SubmersionGrade + 1).ToString(GlobalOptions.CultureInfo)
                                , (CharacterObjectOptions.KarmaInititationFlat + ((CharacterObject.SubmersionGrade + 1) * CharacterObjectOptions.KarmaInitiation)).ToString(GlobalOptions.CultureInfo));
                            cmdAddMetamagic.SetToolTip(strInitTip);
                            chkJoinGroup.Text = LanguageManager.GetString("Checkbox_JoinedNetwork", GlobalOptions.Language);
                            chkInitiationGroup.Text = LanguageManager.GetString("Checkbox_NetworkSubmersion", GlobalOptions.Language);

                            if (!SpecialAttributes.Contains(CharacterObject.RES))
                            {
                                SpecialAttributes.Add(CharacterObject.RES);
                            }
                        }
                        else
                        {
                            if (!CharacterObject.MAGEnabled)
                                tabCharacterTabs.TabPages.Remove(tabInitiation);

                            if (SpecialAttributes.Contains(CharacterObject.RES))
                            {
                                SpecialAttributes.Remove(CharacterObject.RES);
                            }
                        }

                        IsCharacterUpdateRequested = true;
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
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Remove(CharacterObject.MAGAdept);
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
                            if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept && !SpecialAttributes.Contains(CharacterObject.MAGAdept))
                            {
                                SpecialAttributes.Add(CharacterObject.MAGAdept);
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
                                if (CharacterObjectOptions.MysAdeptSecondMAGAttribute && SpecialAttributes.Contains(CharacterObject.MAGAdept))
                                {
                                    SpecialAttributes.Remove(CharacterObject.MAGAdept);
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
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.ToList().DeepWhere(x => x.Children, x => x.SourceType == Improvement.ImprovementSource.Bioware && x.CanRemoveThroughImprovements))
                            {
                                objCyberware.DeleteCyberware();
                                Cyberware objParent = objCyberware.Parent;
                                if (objParent != null)
                                    objParent.Children.Remove(objCyberware);
                                else
                                    CharacterObject.Cyberware.Remove(objCyberware);
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
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.ToList().DeepWhere(x => x.Children, x => x.SourceType == Improvement.ImprovementSource.Cyberware && x.CanRemoveThroughImprovements))
                            {
                                objCyberware.DeleteCyberware();
                                Cyberware objParent = objCyberware.Parent;
                                if (objParent != null)
                                    objParent.Children.Remove(objCyberware);
                                else
                                    CharacterObject.Cyberware.Remove(objCyberware);
                                blnDoRefresh = true;
                            }

                            if (blnDoRefresh)
                            {
                                IsCharacterUpdateRequested = true;
                            }
                        }
                    }
                    break;
                case nameof(Character.CyberwareDisabled):
                    {
                        if (CharacterObject.CyberwareDisabled)
                        {
                            bool blnDoRefresh = false;
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.ToList().Where(x => x.CanRemoveThroughImprovements))
                            {
                                objCyberware.DeleteCyberware();
                                Cyberware objParent = objCyberware.Parent;
                                if (objParent != null)
                                    objParent.Children.Remove(objCyberware);
                                else
                                    CharacterObject.Cyberware.Remove(objCyberware);
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
                            foreach (Cyberware objCyberware in CharacterObject.Cyberware.ToList().DeepWhere(x => x.Children, x => x.CanRemoveThroughImprovements))
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
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", "default.xml");
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
            string settingsPath = Path.Combine(Utils.GetStartupPath, "settings");
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
            Cursor = Cursors.Default;
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
                Description = LanguageManager.GetString("String_CyberzombieWILDescription", GlobalOptions.Language),
                Dice = CharacterObject.WIL.TotalValue
            };
            frmWILHits.ShowDialog(this);

            if (frmWILHits.DialogResult != DialogResult.OK)
                return;

            int intWILResult = frmWILHits.Result;

            // The character gains 10 + ((Threshold - Hits) * 10)BP worth of Negative Qualities.
            int intThreshold = 3 + decimal.ToInt32(decimal.Floor(CharacterObject.Essence() - CharacterObject.ESS.MetatypeMaximum));
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
                        TreeNode objWareNode = objCyberware.SourceID == Cyberware.EssenceHoleGUID || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
                            ? treCyberware.FindNode(objCyberware.SourceID.ToString("D"))
                            : treCyberware.FindNode(objCyberware.InternalId);
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
                        if ((intCyberwaresCount & 1) == 0)
                        {
                            if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                ImprovementManager.ForcedValue = objCyberware.Forced;
                            ImprovementManager.CreateImprovements(CharacterObject, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                            TreeNode objNode = objLoopCyberware.SourceID == Cyberware.EssenceHoleGUID || objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID
                                ? treCyberware.FindNode(objCyberware.SourceID.ToString("D"))
                                : treCyberware.FindNode(objLoopCyberware.InternalId);
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
            object objSelectedObject;
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
                            // Gear can't be added directly to the armor tab, so it must be a child.
                            // Find out what the parent of the new object is going to be. 
                            objSelectedObject = treArmor.SelectedNode?.Tag;
                            Gear objGear = new Gear(CharacterObject);
                            objGear.Load(objXmlNode, true);
                            if (objSelectedObject is Armor objSelectedArmor)
                            {
                                objSelectedArmor.Gear.Add(objGear);
                                if (!objSelectedArmor.Equipped)
                                    objGear.ChangeEquippedStatus(false);
                            }
                            else if (objSelectedObject is ArmorMod objSelectedArmorMod)
                            {
                                XmlNodeList xmlAddonCategoryList = objSelectedArmorMod.GetNode()?.SelectNodes("addoncategory");
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
                                objSelectedArmorMod.Gear.Add(objGear);
                                if (!objSelectedArmorMod.Equipped || objSelectedArmorMod.Parent?.Equipped != true)
                                    objGear.ChangeEquippedStatus(false);
                            }
                            else if (objSelectedObject is Gear objSelectedGear)
                            {
                                XmlNodeList xmlAddonCategoryList = objSelectedGear.GetNode()?.SelectNodes("addoncategory");
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
                                objSelectedGear.Children.Add(objGear);
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
                    // Paste Gear into a Weapon Accessory or other Gear object.
                    XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");
                    if (objXmlNode != null)
                    {
                        objSelectedObject = treWeapons.SelectedNode?.Tag;
                        Gear objGear = new Gear(CharacterObject);
                        objGear.Load(objXmlNode, true);

                        if (objSelectedObject is WeaponAccessory objAccessory)
                        {
                            // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
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
                        else if (objSelectedObject is Gear objNewParent)
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
                    else
                    {
                        // Paste Weapon.
                        objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/weapon");
                        if (objXmlNode != null)
                        {
                            Weapon objWeapon = new Weapon(CharacterObject) { ParentVehicle = null };
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
                        objSelectedObject = treGear.SelectedNode?.Tag;
                        if (objSelectedObject is Gear objNewParent)
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
                objSelectedObject = treCyberware.SelectedNode?.Tag;
                if (objSelectedObject == null)
                    return;

                // Paste Cyberware.
                XmlNode objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/cyberware");
                if (objXmlNode != null)
                {
                    Cyberware objCyberware = new Cyberware(CharacterObject);
                    objCyberware.Load(objXmlNode, true);

                    if (objSelectedObject is Cyberware objCyberwareParent)
                    {
                        if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
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
                        if (objSelectedObject is Cyberware objCyberware)
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
                        else if (objSelectedObject is Gear objNewParent)
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

                            IsCharacterUpdateRequested = true;
                            IsDirty = true;
                        }
                    }
                }
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                objSelectedObject = treVehicles.SelectedNode?.Tag;
                if (objSelectedObject == null)
                    return;
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
                    // Paste Cyberware.
                    objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/cyberware");
                    if (objXmlNode != null)
                    {
                        Cyberware objCyberware = new Cyberware(CharacterObject);
                        objCyberware.Load(objXmlNode, true);
                        switch (objSelectedObject)
                        {
                            case Cyberware objCyberwareParent:
                                {
                                    if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
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
                                    break;
                                }
                            case VehicleMod objMod:
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
                                        /*
                                        if (!string.IsNullOrEmpty(objCyberware.BlocksMounts))
                                        {
                                            if (!string.IsNullOrEmpty(objCyberware.Location) || !string.IsNullOrEmpty(objMod.Location) ||
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
                                        }*/
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
                                    break;
                                }
                            default:
                                objCyberware.DeleteCyberware();
                                break;
                        }
                    }
                    else
                    {
                        // Paste Gear.
                        objXmlNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear");

                        if (objXmlNode != null)
                        {
                            Gear objGear = new Gear(CharacterObject);
                            Vehicle objParentVehicle = null;
                            objGear.Load(objXmlNode, true);

                            // Paste the Gear into a Vehicle's Gear.
                            if (objSelectedObject is Gear objVehicleGear)
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
                            else if (objSelectedObject is Vehicle objVehicle)
                            {
                                objVehicle.Gear.Add(objGear);
                                objParentVehicle = objVehicle;
                            }
                            else if (objSelectedObject is WeaponAccessory objAccessory)
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

                                objParentVehicle = objAccessory.Parent.ParentVehicle;
                                objAccessory.Gear.Add(objGear);
                            }
                            else if (objSelectedObject is Cyberware objCyberware)
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

                                objParentVehicle = objCyberware.Parent?.ParentVehicle ?? objCyberware.ParentVehicle;
                                objCyberware.Gear.Add(objGear);
                            }

                            objGear.ChangeEquippedStatus(false);
                            // Add any Weapons that come with the Gear.
                            XmlNodeList objXmlNodeList = GlobalOptions.Clipboard.SelectNodes("/character/weapons/weapon");
                            if (objXmlNodeList != null && objParentVehicle != null)
                            {
                                foreach (XmlNode objLoopNode in objXmlNodeList)
                                {
                                    Weapon objGearWeapon = new Weapon(CharacterObject);
                                    objGearWeapon.Load(objLoopNode, true);
                                    objParentVehicle.Weapons.Add(objGearWeapon);
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
                                if (objSelectedObject is WeaponMount objWeaponMount)
                                {

                                    Weapon objNewWeapon = new Weapon(CharacterObject) { ParentVehicle = objWeaponMount.Parent };
                                    objNewWeapon.Load(objXmlNode, true);
                                    objWeaponMount.Weapons.Add(objNewWeapon);

                                }
                                else if (objSelectedObject is VehicleMod objVehicleMod)
                                {
                                    // TODO: Make this not depend on string names
                                    if (objVehicleMod.Name.StartsWith("Mechanical Arm") ||
                                        objVehicleMod.Name.Contains("Drone Arm"))
                                    {
                                        Weapon objNewWeapon =
                                            new Weapon(CharacterObject) { ParentVehicle = objVehicleMod.Parent };
                                        objNewWeapon.Load(objXmlNode, true);
                                        objVehicleMod.Weapons.Add(objNewWeapon);
                                    }
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
            if (treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt)
            {
                cmdDeleteMartialArt.Enabled = !objMartialArt.IsQuality;
            }
            else if (treMartialArts.SelectedNode?.Tag is ICanRemove)
            {
                cmdDeleteMartialArt.Enabled = true;
            }
            else
            {
                cmdDeleteMartialArt.Enabled = false;
                lblMartialArtSource.Text = string.Empty;
                lblMartialArtSource.SetToolTip(string.Empty);
            }
            IsRefreshing = false;
        }
#endregion

#region Button Events
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
                objSpell.Create(objXmlSpell, string.Empty, frmPickSpell.Limited, frmPickSpell.Extended, frmPickSpell.Alchemical);
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
            if (!(treSpells.SelectedNode?.Tag is Spell objSpell)) return;
            // Spells that come from Initiation Grades can't be deleted normally. 
            if (objSpell.Grade != 0) return;
            if (!objSpell.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;
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
            if (!(treCyberware.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;

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
                        Description = string.Format(LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language), objXmlComplexForm["translate"]?.InnerText ?? objXmlComplexForm["name"]?.InnerText)
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
                        Description = string.Format(LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language), objXmlProgram["translate"]?.InnerText ?? objXmlProgram["name"]?.InnerText)
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
            if (!(treArmor.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;

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
            Location location = null;
            if (treWeapons.SelectedNode?.Tag is Location objLocation)
            {
                location = objLocation;
            }
            do
            {
                blnAddAgain = AddWeapon(location);
            }
            while (blnAddAgain);
        }

        private bool AddWeapon(Location objLocation = null)
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
            objWeapon.Location = objLocation;

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
            if (treWeapons.SelectedNode?.Tag is ICanRemove objselectedNode)
            {
                if (!objselectedNode.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;
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
            if (treLifestyles.SelectedNode?.Tag is ICanRemove selectedObject)
            {
                if (!selectedObject.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void cmdAddGear_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            string strSelectedId = string.Empty;
            if (treGear.SelectedNode?.Tag is Location objNode)
            {
                strSelectedId = objNode.InternalId;
            }
            do
            {
                blnAddAgain = PickGear(strSelectedId);
            }
            while (blnAddAgain);
        }

        private void cmdDeleteGear_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode?.Tag is ICanRemove objSelectedGear && objSelectedGear.Remove(CharacterObject, CharacterObjectOptions.ConfirmDelete))
            {
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
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
            if (frmPickVehicle.FreeCost)
            {
                objVehicle.Cost = "0";
            }

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
                blnAddAgain = AddVehicle();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteVehicle_Click(object sender, EventArgs e)
        {
            if (!cmdDeleteVehicle.Enabled)
                return;
            // Delete the selected Vehicle.
            object objSelectedNodeTag = treVehicles.SelectedNode?.Tag;
            // Delete the selected Vehicle.
            if (objSelectedNodeTag == null)
            {
                return;
            }

            if (objSelectedNodeTag is VehicleMod objMod)
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
                }

                objMod.DeleteVehicleMod();
                if (objMod.WeaponMountParent != null)
                    objMod.WeaponMountParent.Mods.Remove(objMod);
                else
                    objMod.Parent.Mods.Remove(objMod);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
            else if (objSelectedNodeTag is ICanRemove selectedObject)
            {
                if (selectedObject.Remove(CharacterObject, CharacterObjectOptions.ConfirmDelete))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }
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
            if (treMartialArts.SelectedNode?.Tag is ICanRemove objSelectedNode)
            {
                if (objSelectedNode.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
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
            if (!(treMetamagic.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject, CharacterObjectOptions.ConfirmDelete)) return;
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

                IsCharacterUpdateRequested = true;

                IsDirty = true;
                frmPickCritterPower.Dispose();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteCritterPower_Click(object sender, EventArgs e)
        {
            // If the selected object is not a critter or it comes from an initiate grade, we don't want to remove it.
            if (!(treCritterPowers.SelectedNode?.Tag is CritterPower objCritterPower) || objCritterPower.Grade != 0) return;
            if (!objCritterPower.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdDeleteComplexForm_Click(object sender, EventArgs e)
        {
            if (!(treComplexForms.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;
            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void cmdDeleteAIProgram_Click(object sender, EventArgs e)
        {
            // Delete the selected AI Program.
            if (!(treAIPrograms.SelectedNode?.Tag is ICanRemove selectedObject)) return;
            if (!selectedObject.Remove(CharacterObject,CharacterObjectOptions.ConfirmDelete)) return;

            IsCharacterUpdateRequested = true;
            IsDirty = true;
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
                string strAmount = CharacterObject.GameplayOptionQualityLimit.ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
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
                            MessageBox.Show(string.Format(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language), strAmount),
                                LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            blnAddItem = false;
                        }
                        if (CharacterObject.MetatypeBP < 0)
                        {
                            if ((intBP + CharacterObject.MetatypeBP) < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                            {
                                MessageBox.Show(string.Format(LanguageManager.GetString("Message_NegativeQualityAndMetatypeLimit", GlobalOptions.Language), strAmount),
                                    LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                    string.Format(LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language), strAmount),
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
                MessageBox.Show(string.Format(LanguageManager.GetString("Message_ImprovementQuality", GlobalOptions.Language), objSelectedQuality.GetSourceName(GlobalOptions.Language)),
                    LanguageManager.GetString("MessageTitle_MetavariantQuality", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string strBP = intShowBP.ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);

                if (blnConfirmDelete &&
                    !CharacterObject.ConfirmDelete(string.Format(LanguageManager.GetString(blnCompleteDelete ? "Message_DeleteMetatypeQuality" : "Message_LowerMetatypeQualityLevel", GlobalOptions.Language), strBP)))
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
                {
                    objReplaceQuality.Type = QualityType.Negative;
                    if (!string.IsNullOrEmpty(objReplaceQuality.Extra))
                        objReplaceQuality.Extra += ',' + LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    objReplaceQuality.Extra += LanguageManager.GetString("String_ExpenseRemovePositiveQuality", GlobalOptions.Language);
                }
                else
                {
                    objReplaceQuality.Type = QualityType.Positive;
                    if (!string.IsNullOrEmpty(objReplaceQuality.Extra))
                        objReplaceQuality.Extra += ',' + LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    objReplaceQuality.Extra += LanguageManager.GetString("String_ExpenseRemoveNegativeQuality", GlobalOptions.Language);
                }
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
            
            if (objSelectedQuality.Type == QualityType.LifeModule)
            {
                objXmlDeleteQuality = Quality.GetNodeOverrideable(objSelectedQuality.QualityId, XmlManager.Load("lifemodules.xml", GlobalOptions.Language));
            }
            
            // Fix for legacy characters with old addqualities improvements.
            RemoveAddedQualities(objXmlDeleteQuality?.SelectNodes("addqualities/addquality"));

            // Perform removal
            if (objSelectedQuality.Levels > 1 && blnCompleteDelete)
            {
                for (int i = CharacterObject.Qualities.Count-1; i >= 0; i--)
                {
                    Quality objLoopQuality = CharacterObject.Qualities[i];
                    if (objLoopQuality.QualityId == objSelectedQuality.QualityId && objLoopQuality.Extra == objSelectedQuality.Extra &&
                        objLoopQuality.SourceName == objSelectedQuality.SourceName && objLoopQuality.Type == objSelectedQuality.Type)
                    {
                        // Remove the Improvements that were created by the Quality.
                        ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objLoopQuality.InternalId);

                        // Remove any Weapons created by the Quality if applicable.
                        if (!objLoopQuality.WeaponID.IsEmptyGuid())
                        {
                            List<Weapon> lstWeapons = CharacterObject.Weapons.DeepWhere(x => x.Children, x => x.ParentID == objLoopQuality.InternalId).ToList();
                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                if (objWeapon.ParentID == objLoopQuality.InternalId)
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

                        CharacterObject.Qualities.RemoveAt(i);
                    }
                }
            }
            else
            {
                // Remove the Improvements that were created by the Quality.
                ImprovementManager.RemoveImprovements(CharacterObject, Improvement.ImprovementSource.Quality, objSelectedQuality.InternalId);

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

                CharacterObject.Qualities.Remove(objSelectedQuality);
            }

            return true;
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

        private void cmdAddLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Armor Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;
            Location objLocation = new Location(CharacterObject, CharacterObject.GearLocations, frmPickText.SelectedValue);
            CharacterObject.GearLocations.Add(objLocation);

            IsDirty = true;
        }

        private void cmdAddWeaponLocation_Click(object sender, EventArgs e)
        {
            // Add a new location to the Armor Tree.
            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel || string.IsNullOrEmpty(frmPickText.SelectedValue))
                return;
            Location objLocation = new Location(CharacterObject, CharacterObject.WeaponLocations, frmPickText.SelectedValue);
            CharacterObject.WeaponLocations.Add(objLocation);

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
                MinRating = string.Empty,
                MaxRating = string.Empty,
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
                blnAddAgain = AddArmor(treArmor.SelectedNode?.Tag is Location objLocation ? objLocation : null);
            }
            while (blnAddAgain);
        }

        private bool AddArmor(Location objLocation = null)
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
            objArmor.Location = objLocation;
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
            Location objLocation = new Location(CharacterObject, CharacterObject.ArmorLocations, frmPickText.SelectedValue);
            CharacterObject.ArmorLocations.Add(objLocation);

            IsDirty = true;
        }

        private void cmdArmorEquipAll_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is Location objLocation)
            {
                foreach (Armor objArmor in objLocation.Children.OfType<Armor>())
                {
                    objArmor.Equipped = true;
                }
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdArmorUnEquipAll_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode?.Tag is Location objLocation)
            {
                foreach (Armor objArmor in objLocation.Children.OfType<Armor>())
                {
                    objArmor.Equipped = true;
                }
                IsCharacterUpdateRequested = true;

                IsDirty = true;
            }
        }

        private void cmdAddVehicleLocation_Click(object sender, EventArgs e)
        {
            // Make sure a Vehicle is selected.
            Vehicle objVehicle = treVehicles.SelectedNode?.Tag as Vehicle;
            if (!(objVehicle == null || treVehicles.SelectedNode?.Tag == null))
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
            ObservableCollection<Location> lstParent = objVehicle?.Locations ?? CharacterObject.VehicleLocations;
            Location objLocation = new Location(CharacterObject, lstParent, frmPickText.SelectedValue);
            lstParent.Add(objLocation);

            IsDirty = true;
        }
#endregion

#region ContextMenu Events
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
            if (!(treWeapons.SelectedNode?.Tag is Weapon objWeapon))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectWeaponAccessory", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_SelectWeapon", GlobalOptions.Language),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, frmPickWeaponAccessory.SelectedRating);
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
                        frmPickNumber.Description = string.Format(LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language), objAccessory.DisplayNameShort(GlobalOptions.Language));
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
                XmlNode xmlAddModCategory = objXmlArmor["forcemodcategory"];
                if (xmlAddModCategory != null)
                {
                    frmPickArmorMod.AllowedCategories = xmlAddModCategory.InnerText;
                    frmPickArmorMod.ExcludeGeneralCategory = true;
                }
                else
                {
                    xmlAddModCategory = objXmlArmor["addmodcategory"];
                    if (xmlAddModCategory != null)
                    {
                        frmPickArmorMod.AllowedCategories += "," + xmlAddModCategory.InnerText;
                    }
                }

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

                IsCharacterUpdateRequested = true;

                IsDirty = true;

                frmPickArmorMod.Dispose();
            }
            while (blnAddAgain);
        }

        private void tsGearAddAsPlugin_Click(object sender, EventArgs e)
        {

            // Make sure a parent items is selected, then open the Select Cyberware window.
            if (!(treGear.SelectedNode?.Tag is Gear objGear))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGear", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                blnAddAgain = PickGear(objGear.InternalId);
            }
            while (blnAddAgain);
        }

        private void tsVehicleAddWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle)) return;
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
            // Make sure that a Weapon Mount has been selected.
            // Attempt to locate the selected VehicleMod.
            WeaponMount objWeaponMount = null;
            VehicleMod objMod = null;
            Vehicle objVehicle = null;
            if (treVehicles.SelectedNode?.Tag is WeaponMount selectedMount)
            {
                objWeaponMount = selectedMount;
                objVehicle = selectedMount.Parent;
            }
            else if (treVehicles.SelectedNode?.Tag is VehicleMod selectedMod && (selectedMod.Name.StartsWith("Mechanical Arm") || selectedMod.Name.Contains("Drone Arm")))
            {
                objMod = selectedMod;
                objVehicle = selectedMod.Parent;
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
                objAccessory.Create(objXmlWeapon, frmPickWeaponAccessory.SelectedMount, frmPickWeaponAccessory.SelectedRating);
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
            if (!(treVehicles.SelectedNode?.Tag is Weapon objSelectedWeapon))
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

            if (!(treMartialArts.SelectedNode?.Tag is MartialArt objMartialArt))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectMartialArtTechnique", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectMartialArtTechnique", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                frmSelectMartialArtTechnique frmPickMartialArtTechnique = new frmSelectMartialArtTechnique(CharacterObject, objMartialArt);
                frmPickMartialArtTechnique.ShowDialog(this);

                if (frmPickMartialArtTechnique.DialogResult == DialogResult.Cancel)
                    return;

                blnAddAgain = frmPickMartialArtTechnique.AddAgain;

                // Open the Martial Arts XML file and locate the selected piece.
                XmlNode xmlTechnique = XmlManager.Load("martialarts.xml").SelectSingleNode("/chummer/techniques/technique[id = \"" + frmPickMartialArtTechnique.SelectedTechnique + "\"]");

                // Create the Improvements for the Advantage if there are any.
                MartialArtTechnique objAdvantage = new MartialArtTechnique(CharacterObject);
                objAdvantage.Create(xmlTechnique);
                if (objAdvantage.InternalId.IsEmptyGuid())
                    return;

                objMartialArt.Techniques.Add(objAdvantage);
            } while (blnAddAgain);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSelectedVehicle);
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
                    objWeapon.ParentVehicle = objSelectedVehicle;
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

            // Make sure the Gear was found.
            if (!(objSelectedNode.Tag is Gear objSensor))
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSensor, strCategories);
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
            if (treVehicles.SelectedNode?.Tag is Gear objGear)
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
            if (treGear.SelectedNode == null || treGear.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectGearName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectGear", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Gear.
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
            TreeNode objSelectedNode = treWeapons.SelectedNode;
            // Locate the Weapon that is selected in the tree.
            if (!(objSelectedNode?.Tag is Weapon objSelectedWeapon))
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
            if (treGear.SelectedNode?.Tag is Gear objGear)
            {
                objGear.Extra = frmPickText.SelectedValue;
                treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                IsDirty = true;
            }
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
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
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
            cmdAddArmor_Click(sender, e);
        }

        private void tsAddArmorGear_Click(object sender, EventArgs e)
        {
            // Make sure a parent items is selected, then open the Select Gear window.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool blnAddAgain;
            do
            {
                // Select the root Gear node then open the Select Gear window.
                blnAddAgain = PickArmorGear(objArmor.InternalId, true);
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

            bool capacityOnly = false;
            string selectedGuid = string.Empty;
            // Make sure the selected item is another piece of Gear.
            if (objSelectedNode.Tag is ArmorMod objMod)
            {
                if (string.IsNullOrEmpty(objMod.GearCapacity))
                {
                    MessageBox.Show(LanguageManager.GetString("Message_SelectArmor", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                capacityOnly = true;
                selectedGuid = objMod.InternalId;
            }
            else if (objSelectedNode.Tag is Gear objGear)
            {
                selectedGuid = objGear.InternalId;
            }
            bool blnAddAgain;
            do
            {
                blnAddAgain = PickArmorGear(selectedGuid, capacityOnly);
            }
            while (blnAddAgain);
        }

        private void tsArmorNotes_Click(object sender, EventArgs e)
        {
            if (treArmor.SelectedNode == null)
                return;
            if (treArmor.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treArmor.SelectedNode);
            }
        }

        private void tsWeaponNotes_Click(object sender, EventArgs e)
        {
            if (treWeapons.SelectedNode == null)
                return;
            if (treWeapons.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treWeapons.SelectedNode);
            }
        }

        private void tsCyberwareNotes_Click(object sender, EventArgs e)
        {
            if (treCyberware.SelectedNode == null)
                return;
            if (treCyberware.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treCyberware.SelectedNode);
            }
        }

        private void tsVehicleNotes_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode == null)
                return;
            if (treVehicles.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treVehicles.SelectedNode);
            }
        }

        private void tsQualityNotes_Click(object sender, EventArgs e)
        {
            if (treQualities.SelectedNode == null)
                return;
            if (treQualities.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treQualities.SelectedNode);
            }
        }

        private void tsMartialArtsNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode == null)
                return;
            if (treMartialArts.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treMartialArts.SelectedNode);
            }
        }

#if LEGACY
        private void tsMartialArtManeuverNotes_Click(object sender, EventArgs e)
        {
            if (treMartialArts.SelectedNode == null)
                return;
            if (treMartialArts.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treMartialArts.SelectedNode);
            }
        }
#endif

        private void tsSpellNotes_Click(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode == null)
                return;
            if (treSpells.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treSpells.SelectedNode);
            }
        }

        private void tsComplexFormNotes_Click(object sender, EventArgs e)
        {
            if (treComplexForms.SelectedNode == null)
                return;
            if (treComplexForms.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treComplexForms.SelectedNode);
            }
        }

        private void tsAIProgramNotes_Click(object sender, EventArgs e)
        {
            if (treAIPrograms.SelectedNode == null)
                return;
            if (treAIPrograms.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treAIPrograms.SelectedNode);
            }
        }

        private void tsCritterPowersNotes_Click(object sender, EventArgs e)
        {
            if (treCritterPowers.SelectedNode == null)
                return;
            if (treCritterPowers.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treCritterPowers.SelectedNode);
            }
        }

        private void tsMetamagicNotes_Click(object sender, EventArgs e)
        {
            if (treMetamagic.SelectedNode == null)
                return;
            if (treMetamagic.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treMetamagic.SelectedNode);
            }
        }

        private void tsGearNotes_Click(object sender, EventArgs e)
        {
            if (treGear.SelectedNode == null)
                return;
            if (treGear.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treGear.SelectedNode);
            }
        }

        private void tsLifestyleNotes_Click(object sender, EventArgs e)
        {
            if (treLifestyles.SelectedNode == null)
                return;
            if (treLifestyles.SelectedNode?.Tag is IHasNotes objNotes)
            {
                WriteNotes(objNotes, treLifestyles.SelectedNode);
            }
        }

        private void tsWeaponMountLocation_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode != null && treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount)
            {
                frmSelectText frmPickText = new frmSelectText
                {
                    Description = LanguageManager.GetString("String_VehicleName", GlobalOptions.Language),
                    DefaultString = objWeaponMount.Location
                };

                frmPickText.ShowDialog(this);

                if (frmPickText.DialogResult == DialogResult.Cancel)
                    return;

                objWeaponMount.Location = frmPickText.SelectedValue;
                treVehicles.SelectedNode.Text = objWeaponMount.DisplayName(GlobalOptions.Language);
            }
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
            if (!(treVehicles.SelectedNode?.Tag is Vehicle objVehicle))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectVehicleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectVehicle", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_VehicleName", GlobalOptions.Language),
                DefaultString = objVehicle.CustomName
            };

            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objVehicle.CustomName = frmPickText.SelectedValue;
            treVehicles.SelectedNode.Text = objVehicle.DisplayName(GlobalOptions.Language);
        }

        private void tsVehicleAddCyberware_Click(object sender, EventArgs e)
        {
            if (treVehicles.SelectedNode?.Tag is string)
            {
                MessageBox.Show(LanguageManager.GetString("Message_VehicleCyberwarePlugin", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_NoCyberware", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cyberware objCyberwareParent = null;
            string strNeedleId = (treVehicles.SelectedNode?.Tag as IHasInternalId)?.InternalId;
            VehicleMod objMod = CharacterObject.Vehicles.FindVehicleMod(x => x.InternalId == strNeedleId, out Vehicle objVehicle, out WeaponMount _);
            if (objMod == null)
                objCyberwareParent = CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == strNeedleId, out objMod);

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
                frmSelectCyberware frmPickCyberware = new frmSelectCyberware(CharacterObject, Improvement.ImprovementSource.Cyberware, objCyberwareParent ?? (object)objMod);
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
                Cyberware objCyberware = new Cyberware(CharacterObject);
                if (objCyberware.Purchase(objXmlCyberware, Improvement.ImprovementSource.Cyberware, frmPickCyberware.SelectedGrade, frmPickCyberware.SelectedRating, objVehicle, objMod.Cyberware, CharacterObject.Vehicles, objMod.Weapons, frmPickCyberware.Markup, frmPickCyberware.FreeCost, frmPickCyberware.BlackMarketDiscount, true, "String_ExpensePurchaseVehicleCyberware"))
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
            while (treArmor.SelectedNode != null && treArmor.SelectedNode.Level > 1)
                treArmor.SelectedNode = treArmor.SelectedNode.Parent;

            // Make sure a parent item is selected, then open the Select Accessory window.
            if (treArmor.SelectedNode == null || treArmor.SelectedNode.Level <= 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmorName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the information for the currently selected Armor.
            if (!(treArmor.SelectedNode?.Tag is Armor objArmor))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectArmorName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectArmor", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_ArmorName", GlobalOptions.Language),
                DefaultString = objArmor.CustomName
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objArmor.CustomName = frmPickText.SelectedValue;
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
            treGear.SelectedNode.Text = objLocation.DisplayName(GlobalOptions.Language);

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
            treWeapons.SelectedNode.Text = objLocation.DisplayName(GlobalOptions.Language);

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
            treArmor.SelectedNode.Text = objLocation.DisplayName(GlobalOptions.Language);

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

            // Make sure the Cyberware is allowed to accept Gear.
            if (!(objSelectedNode.Tag is Cyberware objCyberware) ||  objCyberware.AllowGear == null)
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

                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objCyberware, strCategories);
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

            // Open the Gear XML file and locate the selected piece.
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            bool blnAddAgain;

            do
            {
                Cursor = Cursors.WaitCursor;

                string strCategories = string.Empty;
                foreach (XmlNode objXmlCategory in objCyberware.AllowGear)
                    strCategories += objXmlCategory.InnerText + ",";

                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objCyberware, strCategories);
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
            if (!(objSelectedNode.Tag is Gear objSensor))
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSensor, strCategories);
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
            if (!(objSelectedNode.Tag is Gear objSensor))
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSensor, strCategories);
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

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (!(objSelectedNode?.Tag is WeaponAccessory objAccessory) || objAccessory.AllowGear == null)
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objAccessory, strCategories);
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
            if (!(treWeapons.SelectedNode?.Tag is Gear objSensor))
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSensor, strCategories);
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
            if (!(treVehicles.SelectedNode?.Tag is Location objLocation)) return;

            frmSelectText frmPickText = new frmSelectText
            {
                Description = LanguageManager.GetString("String_AddLocation", GlobalOptions.Language)
            };
            frmPickText.ShowDialog(this);

            if (frmPickText.DialogResult == DialogResult.Cancel)
                return;

            objLocation.Name = frmPickText.SelectedValue;
            treVehicles.SelectedNode.Text = objLocation.DisplayName(GlobalOptions.Language);
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
            // Make sure the Gear was found.
            if (!(treVehicles.SelectedNode?.Tag is Gear objSensor))
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objSensor, strCategories);
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
                if (lstWeapons.Count > 0)
                {
                    CharacterObject.Vehicles.FindVehicleGear(objGear.InternalId, out Vehicle objVehicle, out WeaponAccessory _, out Cyberware _);
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

        private void tsVehicleWeaponAccessoryAddGear_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treVehicles.SelectedNode;

            // Make sure the Weapon Accessory is allowed to accept Gear.
            if (!(objSelectedNode?.Tag is WeaponAccessory objAccessory) || objAccessory.AllowGear == null)
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
                frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objAccessory, strCategories);
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
                lblQualityBP.Text = (objQuality.BP * objQuality.Levels * CharacterObjectOptions.KarmaQuality).ToString(GlobalOptions.CultureInfo) +
                                    LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language);
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
                                MessageBox.Show(string.Format(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language), strAmount),
                                    LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                blnAddItem = false;
                            }
                            else if (CharacterObject.MetatypeBP < 0)
                            {
                                if ((intBP + CharacterObject.MetatypeBP) < (intMaxQualityAmount * -1) && !CharacterObject.IgnoreRules)
                                {
                                    MessageBox.Show(string.Format(LanguageManager.GetString("Message_NegativeQualityAndMetatypeLimit", GlobalOptions.Language), strAmount),
                                        LanguageManager.GetString("MessageTitle_NegativeQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                    MessageBox.Show(string.Format(LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language), strAmount),
                                        LanguageManager.GetString("MessageTitle_PositiveQualityLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!IsRefreshing && !IsLoading)
            {
                string strSelectedGrade = cboCyberwareGrade.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(strSelectedGrade))
                {
                    // Locate the selected piece of Cyberware.
                    if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
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
            if (!IsRefreshing)
            {
                if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
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
            if (!IsRefreshing)
            {
                // Locate the selected piece of Cyberware.
                if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
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
                                if (intCyberwaresCount > 0 && (intCyberwaresCount & 1) == 0)
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
                else if (treCyberware.SelectedNode?.Tag is Gear objGear)
                {
                    // Find the selected piece of Gear.
                    if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                    {
                        if (!objGear.RefreshSingleFocusRating(treFoci, decimal.ToInt32(nudCyberwareRating.Value)))
                        {
                            IsRefreshing = true;
                            nudCyberwareRating.Value = objGear.Rating;
                            IsRefreshing = false;
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
            if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                return;

            string strGuid = objLifestyle.InternalId;
            int intMonths = objLifestyle.Increments;
            int intPosition = CharacterObject.Lifestyles.IndexOf(CharacterObject.Lifestyles.FirstOrDefault(p => p.InternalId == objLifestyle.InternalId));

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
            
            objLifestyle.SetInternalId(strGuid);
            CharacterObject.Lifestyles[intPosition] = objLifestyle;
            treLifestyles.SelectedNode.Text = objLifestyle.DisplayName(GlobalOptions.Language);
            treLifestyles.SelectedNode.Tag = objLifestyle;

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
                IsRefreshing = true;

                // Locate the selected Lifestyle.
                if (!(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
                    return;

                objLifestyle.Increments = decimal.ToInt32(nudLifestyleMonths.Value);

                IsRefreshing = false;

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
            if (IsRefreshing)
                return;

            if (treGear.SelectedNode.Level > 0)
            {
                if (!(treGear.SelectedNode?.Tag is Gear objGear))
                    return;

                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                {
                    if (!objGear.RefreshSingleFocusRating(treFoci, decimal.ToInt32(nudGearRating.Value)))
                    {
                        IsRefreshing = true;
                        nudGearRating.Value = objGear.Rating;
                        IsRefreshing = false;
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
            if (IsRefreshing || treGear.SelectedNode == null)
                return;
            // Attempt to locate the selected piece of Gear.
            if (treGear.SelectedNode.Level > 0)
            {
                if (treGear.SelectedNode?.Tag is Gear objGear)
                {
                    objGear.Quantity = nudGearQty.Value;

                    IsCharacterUpdateRequested = true;

                    IsDirty = true;
                }
            }
        }
        private void nudDrugQty_ValueChanged(object sender, EventArgs e)
        {
            // Don't attempt to do anything while the data is still being populated.
            if (IsLoading || IsRefreshing)
                return;

            if (treCustomDrugs.SelectedNode?.Tag is Drug objDrug)
            {
                objDrug.Quantity = Convert.ToInt32(nudDrugQty.Value);
                RefreshSelectedDrug();

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
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

        private void chkWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
                return;
            // Locate the selected Weapon Accessory or Modification.
            if (treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                objAccessory.Equipped = chkWeaponAccessoryInstalled.Checked;
            }
            else if (treWeapons.SelectedNode?.Tag is Weapon objWeapon)
            {
                objWeapon.Equipped = chkWeaponAccessoryInstalled.Checked;
            }
            else if (treWeapons.SelectedNode?.Tag is Gear objGear)
            {
                objGear.Equipped = chkWeaponAccessoryInstalled.Checked;
                objGear.ChangeEquippedStatus(chkWeaponAccessoryInstalled.Checked);
                IsCharacterUpdateRequested = true;
            }

            IsDirty = true;
        }

        private void chkIncludedInWeapon_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treWeapons.SelectedNode == null)
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
            if (IsRefreshing || treGear.SelectedNode == null)
                return;

            // Attempt to locate the selected piece of Gear.
            if (treGear.SelectedNode?.Tag is Gear objGear)
            {
                objGear.Equipped = chkGearEquipped.Checked;
                objGear.ChangeEquippedStatus(chkGearEquipped.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void chkGearHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || treGear.SelectedNode == null)
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
            if (IsRefreshing)
                return;
            if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objCommlink)
            {
                objCommlink.SetHomeNode(CharacterObject, chkCyberwareHomeNode.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
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
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink)
            {
                objSelectedCommlink.SetActiveCommlink(CharacterObject, chkGearActiveCommlink.Checked);

                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void chkCyberwareActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treCyberware.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink)
            {
                objSelectedCommlink.SetActiveCommlink(CharacterObject, chkCyberwareActiveCommlink.Checked);

                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void chkVehicleActiveCommlink_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objSelectedCommlink)
            {
                objSelectedCommlink.SetActiveCommlink(CharacterObject, chkVehicleActiveCommlink.Checked);

                IsDirty = true;
                IsCharacterUpdateRequested = true;
            }
        }

        private void cboGearAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearAttack.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboGearSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearSleaze.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboGearDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearDataProcessing.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboGearFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboGearFirewall.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboGearAttack, cboGearAttack,
                    cboGearSleaze, cboGearDataProcessing, cboGearFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboVehicleAttack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleAttack.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleAttack, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboVehicleSleaze_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleSleaze.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleAttack, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboVehicleFirewall_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleFirewall.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleAttack, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
            }

            IsRefreshing = false;
        }
        private void cboVehicleDataProcessing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsRefreshing || !cboVehicleDataProcessing.Enabled)
                return;

            IsRefreshing = true;
            if (treGear.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboVehicleAttack, cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall))
                {
                    IsCharacterUpdateRequested = true;
                    IsDirty = true;
                }
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

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareAttack, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareSleaze, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareDataProcessing, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
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

            if (objTarget.ProcessMatrixAttributeCBOChange(CharacterObject, cboCyberwareFirewall, cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall))
            {
                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }

            IsRefreshing = false;
        }
        #endregion

        #region Additional Drug Tab Control Events
        private void treCustomDrugs_AfterSelect(object sender, TreeViewEventArgs e)
		{
			RefreshSelectedDrug();
			RefreshPasteStatus(sender, e);
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
                if (treVehicles.SelectedNode?.Tag is Gear)
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
            if (IsRefreshing)
                return;

            if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                objMod.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objMod.DisplayName(GlobalOptions.Language);
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                {
                    if (!objGear.RefreshSingleFocusRating(treFoci, decimal.ToInt32(nudVehicleRating.Value)))
                    {
                        IsRefreshing = true;
                        nudVehicleRating.Value = objGear.Rating;
                        IsRefreshing = false;
                        return;
                    }
                }
                else
                    objGear.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                objAccessory.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objAccessory.DisplayName(GlobalOptions.Language);
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                objCyberware.Rating = decimal.ToInt32(nudVehicleRating.Value);
                treVehicles.SelectedNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
            }
            else
            {
                return;
            }

            IsCharacterUpdateRequested = true;
            IsDirty = true;
        }

        private void chkVehicleWeaponAccessoryInstalled_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                objAccessory.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
            }
            else if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                objWeapon.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
            }
            else if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                objMod.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount)
            {
                objWeaponMount.Equipped = chkVehicleWeaponAccessoryInstalled.Checked;
            }
            else
            {
                return;
            }

            IsDirty = true;
        }

        private void nudVehicleGearQty_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;

            if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                objGear.Quantity = nudVehicleGearQty.Value;
                treVehicles.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }

        private void chkVehicleHomeNode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            if (treVehicles.SelectedNode?.Tag is IHasMatrixAttributes objTarget)
            {
                objTarget.SetHomeNode(CharacterObject, chkVehicleHomeNode.Checked);

                IsCharacterUpdateRequested = true;
                IsDirty = true;
            }
        }
#endregion

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

                treFoci.SelectedNode.Text = objSelectedFocus.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                    treFoci.SelectedNode.Text = objStackGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                }
            }

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void nudArmorRating_ValueChanged(object sender, EventArgs e)
        {
            if (IsRefreshing)
                return;
            
            // Locate the selected ArmorMod.
            if (treArmor.SelectedNode?.Tag is ArmorMod objMod)
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
            else if (treArmor.SelectedNode?.Tag is Gear objGear)
            {
                if (objGear.Category == "Foci" || objGear.Category == "Metamagic Foci" || objGear.Category == "Stacked Focus")
                {
                    if (!objGear.RefreshSingleFocusRating(treFoci, decimal.ToInt32(nudArmorRating.Value)))
                    {
                        IsRefreshing = true;
                        nudArmorRating.Value = objGear.Rating;
                        IsRefreshing = false;
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
            else if (treArmor.SelectedNode?.Tag is Armor objArmor)
            {
                objArmor.Rating = decimal.ToInt32(nudArmorRating.Value);
                treArmor.SelectedNode.Text = objArmor.DisplayName(GlobalOptions.Language);
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

            XmlNode xmlTradition = XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions/tradition[id = \"" + strSelectedId + "\"]");

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
                    cboDrain.Visible = false;
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
        }
#endregion

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
            if (string.IsNullOrEmpty(strSelectedId) || strSelectedId == CharacterObject.MagicTradition.SourceID)
                return;

            XmlNode xmlNewStreamNode = XmlManager.Load("streams.xml").SelectSingleNode("/chummer/traditions/tradition[id = \"" + strSelectedId + "\"]");
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
#endregion

#region Additional AI Advanced Programs Tab Control Events
        private void treAIPrograms_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the Program that is selected in the tree.
            if (treAIPrograms.SelectedNode?.Tag is AIProgram objProgram)
            {
                lblAIProgramsRequires.Text = objProgram.DisplayRequiresProgram(GlobalOptions.Language);

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

        private void chkInitiationGroup_EnabledChanged(object sender, EventArgs e)
        {
            if (!chkInitiationGroup.Enabled)
            {
                chkInitiationGroup.Checked = false;
            }
        }

        private void chkJoinGroup_CheckedChanged(object sender, EventArgs e)
        {
            IsCharacterUpdateRequested = true;
        }

        private void treMetamagic_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch (treMetamagic.SelectedNode?.Tag)
            {
                case Metamagic objMetamagic:
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(objMetamagic.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objMetamagic.Grade >= 0;
                    objMetamagic.SetSourceDetail(lblMetamagicSource);
                    break;
                }
                case Art objArt:
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(objArt.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objArt.Grade >= 0;
                    objArt.SetSourceDetail(lblMetamagicSource);
                        break;
                }
                case Spell objSpell:
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveMetamagic", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objSpell.Grade >= 0;
                    objSpell.SetSourceDetail(lblMetamagicSource);
                        break;
                }
                case ComplexForm objComplexForm:
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString("Button_RemoveEcho", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objComplexForm.Grade >= 0;
                    objComplexForm.SetSourceDetail(lblMetamagicSource);
                        break;
                }
                case Enhancement objEnhancement:
                {
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(objEnhancement.SourceType == Improvement.ImprovementSource.Metamagic ? "Button_RemoveMetamagic" : "Button_RemoveEcho", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = objEnhancement.Grade >= 0;
                    objEnhancement.SetSourceDetail(lblMetamagicSource);
                        break;
                }
                default:
                    cmdDeleteMetamagic.Text = LanguageManager.GetString(CharacterObject.MAGEnabled ? "Button_RemoveInitiateGrade" : "Button_RemoveSubmersionGrade", GlobalOptions.Language);
                    cmdDeleteMetamagic.Enabled = true;
                    lblMetamagicSource.Text = string.Empty;
                    lblMetamagicSource.SetToolTip(string.Empty);
                    break;
            }
        }
        
        private void txtGroupNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                ((TextBox)sender)?.SelectAll();
            }
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
                objPower.SetSourceDetail(lblCritterPowerSource);

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
            lblMetatypeSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);

            if (CharacterObject.BuildMethod == CharacterBuildMethod.Karma || CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                lblKarmaMetatypeBP.Text = (CharacterObject.MetatypeBP * CharacterObjectOptions.MetatypeCostsKarmaMultiplier).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                                          LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            }
            else if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority || CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                lblKarmaMetatypeBP.Text = (CharacterObject.MetatypeBP).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                                          LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            }
            
            string strToolTip = strMetatype + strSpaceCharacter + '(' + CharacterObject.MetatypeBP + ')';
            lblKarmaMetatypeBP.SetToolTip(strToolTip);

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
                        (total - att).ToString(GlobalOptions.CultureInfo), total.ToString(GlobalOptions.CultureInfo), bp.ToString(GlobalOptions.CultureInfo));
                }
                else
                {
                    s = (total - att).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Of", GlobalOptions.Language) + total.ToString(GlobalOptions.CultureInfo);
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
            if (CharacterObject.BuildMethod == CharacterBuildMethod.Karma || CharacterObject.BuildMethod == CharacterBuildMethod.LifeModule)
            {
                // Subtract the BP used for Metatype.
                intKarmaPointsRemain -= (CharacterObject.MetatypeBP * CharacterObjectOptions.MetatypeCostsKarmaMultiplier);
            }
            else if (CharacterObject.BuildMethod == CharacterBuildMethod.Priority || CharacterObject.BuildMethod == CharacterBuildMethod.SumtoTen)
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
            int intUnlimitedPositive = 0;
            int intUnlimitedNegative = 0;
            // Used to make sure Positive Qualities that aren't doubled in Career mode don't get added into the total until after that step has been checked.
            int intPositiveQualitiesNoDoubleExcess = 0;

            foreach (Quality objLoopQuality in CharacterObject.Qualities)
            {
                if (objLoopQuality.ContributeToBP)
                {
                    if (objLoopQuality.ContributeToLimit)
                    {
                        if (objLoopQuality.Type == QualityType.Positive)
                        {
                            if (!objLoopQuality.DoubleCost)
                                intPositiveQualitiesNoDoubleExcess += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                            else
                                intPositiveQualities += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                        }
                        else if (objLoopQuality.Type == QualityType.Negative)
                        {
                            intNegativeQualities += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                        }
                        else if (objLoopQuality.Type == QualityType.LifeModule)
                        {
                            intLifeModuleQualities += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                        }
                    }
                    else if (objLoopQuality.Type == QualityType.Positive)
                    {
                        intUnlimitedPositive += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                    }
                    else if (objLoopQuality.Type == QualityType.Negative)
                    {
                        intUnlimitedNegative += objLoopQuality.BP * CharacterObjectOptions.KarmaQuality;
                    }
                }
            }
            
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
            // Now we add in the karma from qualities that are not doubled in career mode
            intPositiveQualities += intPositiveQualitiesNoDoubleExcess;

            int intQualityPointsUsed = intLifeModuleQualities + intNegativeQualities + intPositiveQualities + intUnlimitedPositive + intUnlimitedNegative;

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
            string strColon = LanguageManager.GetString("String_Colon", GlobalOptions.Language);
            string strOf = LanguageManager.GetString("String_Of", GlobalOptions.Language);
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
                int intTouchOnlySpells = CharacterObject.Spells.Count(spell => spell.Grade == 0 && (!spell.Alchemical) && spell.Category != "Rituals" && (spell.Range == "T (A)" || spell.Range == "T") && !spell.FreeBonus);
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
                    lblSpellsBP.SetToolTip($"{spells.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    lblBuildRitualsBP.SetToolTip($"{rituals.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    lblBuildPrepsBP.SetToolTip($"{preps.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}×{strSpaceCharacter}{spellCost.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}+{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}{strSpaceCharacter}={strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{LanguageManager.GetString("String_Karma", GlobalOptions.Language)}");
                    if (limit + limitMod > 0)
                    {
                        lblBuildPrepsBP.Text =
                            string.Format(
                                $"{prepPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                        lblSpellsBP.Text =
                            string.Format(
                                $"{spellPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                        lblBuildRitualsBP.Text =
                            string.Format(
                                $"{ritualPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{(limit + limitMod).ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
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
                                    $"{prepPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{limitMod.ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intPrepPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                            lblSpellsBP.Text =
                                string.Format(
                                    $"{spellPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{limitMod.ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intSpellPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
                            lblBuildRitualsBP.Text =
                                string.Format(
                                    $"{ritualPoints.ToString(GlobalOptions.CultureInfo)}{strOf}{limitMod.ToString(GlobalOptions.CultureInfo)}{strColon}{strSpaceCharacter}{intRitualPointsUsed.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}");
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
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements.Where(x => x.ImprovedName == strFocusName && (string.IsNullOrEmpty(x.Target) || x.Target.Contains(strFocusExtra)) && x.Enabled))
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

            // Joining a Network does not cost Karma for Technomancers, so this only applies to Magicians/Adepts.
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
                strContactPoints += strOf + intContactPoints;
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

                lblPositiveQualitiesBP.Text = intUnlimitedPositive > 0
                   ? $"{intPositiveQualities.ToString(GlobalOptions.CultureInfo)}/{CharacterObject.GameplayOptionQualityLimit.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}{strSpaceCharacter}({(intPositiveQualities + intUnlimitedPositive).ToString(GlobalOptions.CultureInfo)})"
                   : $"{intPositiveQualities.ToString(GlobalOptions.CultureInfo)}/{CharacterObject.GameplayOptionQualityLimit.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}";

                lblNegativeQualitiesBP.Text = intUnlimitedNegative > 0
                    ? $"{(-intNegativeQualities).ToString(GlobalOptions.CultureInfo)}/{CharacterObject.GameplayOptionQualityLimit.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}{strSpaceCharacter}({(intNegativeQualities + intUnlimitedNegative).ToString(GlobalOptions.CultureInfo)})"
                    : $"{(-intNegativeQualities).ToString(GlobalOptions.CultureInfo)}/{CharacterObject.GameplayOptionQualityLimit.ToString(GlobalOptions.CultureInfo)}{strSpaceCharacter}{strPoints}";

                lblAttributesBP.Text = BuildAttributes(CharacterObject.AttributeSection.AttributeList);
                lblPBuildSpecial.Text = BuildAttributes(CharacterObject.AttributeSection.SpecialAttributeList, null, true);

                tabSkillUc.MissingDatabindingsWorkaround();

                lblMartialArtsBP.Text = intMartialArtsPoints.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                lblBuildMartialArts.SetToolTip(strMartialArtsBPToolTip.ToString());

                lblNuyenBP.Text = intNuyenBP.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblFociBP.Text = intFociPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
                lblBuildFoci.SetToolTip(strFociPointsTooltip.ToString());

                lblSpiritsBP.Text = intSpiritPointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                lblSpritesBP.Text = intSpritePointsUsed.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;

                string strComplexFormsBP;
                if (CharacterObject.CFPLimit > 0)
                {
                    strComplexFormsBP = intFormsPointsUsed.ToString(GlobalOptions.CultureInfo) + strOf + CharacterObject.CFPLimit.ToString(GlobalOptions.CultureInfo);
                    if (intFormsPointsUsed > CharacterObject.CFPLimit)
                    {
                        strComplexFormsBP += strColon + strSpaceCharacter + ((intFormsPointsUsed - CharacterObject.CFPLimit) * CharacterObject.ComplexFormKarmaCost).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strPoints;
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
                tssBPRemain.Text = intKarmaPointsRemain.ToString(GlobalOptions.CultureInfo);
                if (_blnFreestyle)
                {
                    tssBP.Text = Math.Max(intFreestyleBP, intFreestyleBPMin).ToString(GlobalOptions.CultureInfo);
                    tssBP.ForeColor = intFreestyleBP < intFreestyleBPMin ? Color.OrangeRed : SystemColors.ControlText;
                }
                else
                {
                    tssBP.Text = CharacterObject.BuildKarma.ToString(GlobalOptions.CultureInfo);
                    tssBP.ForeColor = SystemColors.ControlText;
                }
            }

            return intKarmaPointsRemain;
        }

        private void UpdateSkillRelatedInfo()
        {
            string strKarma = LanguageManager.GetString("String_Karma", GlobalOptions.Language);
            string strOf = LanguageManager.GetString("String_Of", GlobalOptions.Language);
            string strColon = LanguageManager.GetString("String_Colon", GlobalOptions.Language);
            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strZeroKarma = 0.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
            //Update Skill Labels
            //Active skills
            string strTemp = strZeroKarma;
            int intActiveSkillPointsMaximum = CharacterObject.SkillsSection.SkillPointsMaximum;
            if (intActiveSkillPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.SkillPoints.ToString(GlobalOptions.CultureInfo) + strOf + intActiveSkillPointsMaximum.ToString(GlobalOptions.CultureInfo);
            }
            int intActiveSkillsTotalCostKarma = CharacterObject.SkillsSection.Skills.TotalCostKarma();
            if (intActiveSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intActiveSkillsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intActiveSkillsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }

            }
            lblActiveSkillsBP.Text = strTemp;
            //Knowledge skills
            strTemp = strZeroKarma;
            int intKnowledgeSkillPointsMaximum = CharacterObject.SkillsSection.KnowledgeSkillPoints;
            if (intKnowledgeSkillPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalOptions.CultureInfo) + strOf + intKnowledgeSkillPointsMaximum.ToString(GlobalOptions.CultureInfo);
            }
            int intKnowledgeSkillsTotalCostKarma = CharacterObject.SkillsSection.KnowledgeSkills.TotalCostKarma();
            if (intKnowledgeSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intKnowledgeSkillsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intKnowledgeSkillsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }
            }
            lblKnowledgeSkillsBP.Text = strTemp;
            //Groups
            strTemp = strZeroKarma;
            int intSkillGroupPointsMaximum = CharacterObject.SkillsSection.SkillGroupPointsMaximum;
            if (intSkillGroupPointsMaximum > 0)
            {
                strTemp = CharacterObject.SkillsSection.SkillGroupPoints.ToString(GlobalOptions.CultureInfo) + strOf + intSkillGroupPointsMaximum.ToString(GlobalOptions.CultureInfo);
            }
            int intSkillGroupsTotalCostKarma = CharacterObject.SkillsSection.SkillGroups.TotalCostKarma();
            if (intSkillGroupsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intSkillGroupsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }
                else
                {
                    strTemp = intSkillGroupsTotalCostKarma.ToString(GlobalOptions.CultureInfo) + strSpace + strKarma;
                }

            }
            lblSkillGroupsBP.Text = strTemp;
        }

        private void LiveUpdateFromCharacterFile(object sender, EventArgs e)
        {
            if (IsDirty || !GlobalOptions.LiveUpdateCleanCharacterFiles || IsLoading || _blnSkipUpdate || IsCharacterUpdateRequested)
                return;

            string strCharacterFile = CharacterObject.FileName;
            if (string.IsNullOrEmpty(strCharacterFile) || !File.Exists(strCharacterFile))
                return;

            if (File.GetLastWriteTimeUtc(strCharacterFile) <= CharacterObject.FileLastWriteTime)
                return;

            _blnSkipUpdate = true;

            // Character is not dirty and their savefile was updated outside of Chummer5 while it is open, so reload them
            Cursor = Cursors.WaitCursor;

            frmLoading frmLoadingForm = new frmLoading {CharacterFile = CharacterObject.FileName};
            frmLoadingForm.Reset(36);
            frmLoadingForm.Show();
            CharacterObject.Load(frmLoadingForm);
            frmLoadingForm.PerformStep(LanguageManager.GetString("String_UI"));

            // Update character information fields.
            RefreshMetatypeFields();

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
            UpdateCharacterInfo();

            IsDirty = false;

            frmLoadingForm.Close();

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
            if (IsLoading || _blnSkipUpdate || !IsCharacterUpdateRequested)
                return;

            _blnSkipUpdate = true;

            // TODO: DataBind these wherever possible

            UpdateSkillRelatedInfo();
            
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
            decDeductions += CharacterObject.Cyberware.AsParallel().Sum(x => x.TotalCost);

            // Initiation Grade cost.
            foreach (InitiationGrade objGrade in CharacterObject.InitiationGrades)
            {
                if (objGrade.Schooling)
                    decDeductions += 10000;
            }

            // Armor cost.
            decDeductions += CharacterObject.Armor.AsParallel().Sum(x => x.TotalCost);

            // Weapon cost.
            decDeductions += CharacterObject.Weapons.AsParallel().Sum(x => x.TotalCost);

            // Gear cost.
            decDeductions += CharacterObject.Gear.AsParallel().Sum(x => x.TotalCost);

            // Lifestyle cost.
            decDeductions += CharacterObject.Lifestyles.AsParallel().Sum(x => x.TotalCost);

            // Vehicle cost.
            decDeductions += CharacterObject.Vehicles.AsParallel().Sum(x => x.TotalCost);

			// Drug cost.
            decDeductions += CharacterObject.Drugs.AsParallel().Sum(x => x.TotalCost);

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
                cmdDeleteCyberware.Enabled = false;
                
                IsRefreshing = false;
                flpCyberware.ResumeLayout();
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strESSFormat = CharacterObjectOptions.EssenceFormat;
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
            // Locate the selected piece of Cyberware.
            if (treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
            {
                gpbCyberwareCommon.Visible = true;
                gpbCyberwareMatrix.Visible = objCyberware.SourceType == Improvement.ImprovementSource.Cyberware;

                // Buttons
                cmdDeleteCyberware.Enabled = string.IsNullOrEmpty(objCyberware.ParentID);

                // gpbCyberwareCommon
                lblCyberwareName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblCyberwareCategory.Text = objCyberware.DisplayCategory(GlobalOptions.Language);
                // Cyberware Grade is not available for Genetech items.
                // Cyberware Grade is only available on root-level items (sub-components cannot have a different Grade than the piece they belong to).
                if (objCyberware.Parent == null && !objCyberware.Suite && string.IsNullOrWhiteSpace(objCyberware.ForceGrade))
                    cboCyberwareGrade.Enabled = true;
                else
                    cboCyberwareGrade.Enabled = false;
                bool blnIgnoreSecondHand = objCyberware.GetNode()?["nosecondhand"] != null;
                PopulateCyberwareGradeList(objCyberware.SourceType == Improvement.ImprovementSource.Bioware, blnIgnoreSecondHand, cboCyberwareGrade.Enabled ? string.Empty : objCyberware.Grade.Name);
                lblCyberwareGradeLabel.Visible = true;
                cboCyberwareGrade.Visible = true;
                cboCyberwareGrade.SelectedValue = objCyberware.Grade.Name;
                if (cboCyberwareGrade.SelectedIndex == -1 && cboCyberwareGrade.Items.Count > 0)
                    cboCyberwareGrade.SelectedIndex = 0;
                lblCyberwareEssenceLabel.Visible = true;
                lblCyberwareEssence.Visible = true;
                if (objCyberware.Parent == null)
                    lblCyberwareEssence.Text = objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else if (objCyberware.AddToParentESS)
                    lblCyberwareEssence.Text = '+' + objCyberware.CalculatedESS().ToString(strESSFormat, GlobalOptions.CultureInfo);
                else
                    lblCyberwareEssence.Text = (0.0m).ToString(strESSFormat, GlobalOptions.CultureInfo);
                lblCyberwareAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                    nudCyberwareRating.Maximum = Convert.ToDecimal(objCyberware.MaxRating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Minimum = Convert.ToDecimal(objCyberware.MinRating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Value = Convert.ToDecimal(objCyberware.Rating, GlobalOptions.CultureInfo);
                    nudCyberwareRating.Visible = true;
                    nudCyberwareRating.Enabled = true;
                    lblCyberwareRatingLabel.Visible = true;
                }
                lblCyberwareCapacity.Text = objCyberware.CalculatedCapacity + strSpace + '(' + objCyberware.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                            strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblCyberwareCost.Text = objCyberware.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                if (objCyberware.Category.Equals("Cyberlimb") || objCyberware.AllowedSubsystems.Contains("Cyberlimb"))
                {
                    lblCyberlimbAGILabel.Visible = true;
                    lblCyberlimbAGI.Visible = true;
                    lblCyberlimbAGI.Text = objCyberware.TotalAgility.ToString();
                    lblCyberlimbSTRLabel.Visible = true;
                    lblCyberlimbSTR.Visible = true;
                    lblCyberlimbSTR.Text = objCyberware.TotalStrength.ToString();
                }
                else
                {
                    lblCyberlimbAGILabel.Visible = false;
                    lblCyberlimbAGI.Visible = false;
                    lblCyberlimbSTRLabel.Visible = false;
                    lblCyberlimbSTR.Visible = false;
                }
                chkPrototypeTranshuman.Visible = CharacterObject.PrototypeTranshuman != 0;
                chkPrototypeTranshuman.Enabled = objCyberware.Parent == null;
                chkPrototypeTranshuman.Checked = objCyberware.PrototypeTranshuman;

                // gpbCyberwareMatrix
                if (gpbCyberwareMatrix.Visible)
                {
                    int intDeviceRating = objCyberware.GetTotalMatrixAttribute("Device Rating");
                    lblCyberDeviceRating.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                    objCyberware.RefreshMatrixAttributeCBOs(cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall);

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
                
                treCyberware.SelectedNode.Text = objCyberware.DisplayName(GlobalOptions.Language);
            }
            else if (treCyberware.SelectedNode?.Tag is Gear objGear)
            {
                gpbCyberwareCommon.Visible = true;
                gpbCyberwareMatrix.Visible = true;

                // Buttons
                cmdDeleteCyberware.Enabled = !objGear.IncludedInParent;

                // gpbCyberwareCommon
                lblCyberwareName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblCyberwareCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                lblCyberwareGradeLabel.Visible = false;
                cboCyberwareGrade.Visible = false;
                lblCyberwareEssenceLabel.Visible = false;
                lblCyberwareEssence.Visible = false;
                lblCyberwareAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                    nudCyberwareRating.Enabled = nudCyberwareRating.Minimum != nudCyberwareRating.Maximum;
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
                lblCyberwareCapacity.Text = objGear.CalculatedCapacity + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                            strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblCyberwareCost.Text =
                    objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblCyberlimbAGILabel.Visible = false;
                lblCyberlimbAGI.Visible = false;
                lblCyberlimbSTRLabel.Visible = false;
                lblCyberlimbSTR.Visible = false;
                chkPrototypeTranshuman.Visible = false;

                // gpbCyberwareMatrix
                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                lblCyberDeviceRating.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                objGear.RefreshMatrixAttributeCBOs(cboCyberwareAttack, cboCyberwareSleaze, cboCyberwareDataProcessing, cboCyberwareFirewall);

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
                
                treCyberware.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                cmdDeleteWeapon.Enabled = false;

                IsRefreshing = false;
                flpWeapons.ResumeLayout();

                return;
            }
            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
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

            if (treWeapons.SelectedNode?.Tag is Weapon objWeapon)
            {
                gpbWeaponsCommon.Visible = true;
                gpbWeaponsWeapon.Visible = true;
                gpbWeaponsMatrix.Visible = true;

                // Buttons
                cmdDeleteWeapon.Enabled = !objWeapon.IncludedInWeapon &&
                                          !objWeapon.Cyberware &&
                                          objWeapon.Category != "Gear" &&
                                          !objWeapon.Category.StartsWith("Quality") &&
                                          string.IsNullOrEmpty(objWeapon.ParentID);

                // gpbWeaponsCommon
                lblWeaponName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                lblWeaponRatingLabel.Visible = false;
                lblWeaponRating.Visible = false;
                lblWeaponCapacityLabel.Visible = false;
                lblWeaponCapacity.Visible = false;
                lblWeaponAvail.Text = objWeapon.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponCost.Text = objWeapon.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblWeaponSlotsLabel.Visible = true;
                lblWeaponSlots.Visible = true;
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
                lblWeaponConcealLabel.Visible = true;
                lblWeaponConceal.Visible = true;
                lblWeaponConceal.Text = objWeapon.CalculatedConcealability(GlobalOptions.CultureInfo);
                chkWeaponAccessoryInstalled.Visible = true;
                chkWeaponAccessoryInstalled.Enabled = objWeapon.Parent != null;
                chkWeaponAccessoryInstalled.Checked = objWeapon.Equipped;
                chkIncludedInWeapon.Visible = objWeapon.Parent != null;
                chkIncludedInWeapon.Enabled = false;
                chkIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                // gpbWeaponsWeapon
                gpbWeaponsWeapon.Text = LanguageManager.GetString("String_Weapon", GlobalOptions.Language);
                lblWeaponDamageLabel.Visible = true;
                lblWeaponDamage.Visible = true;
                lblWeaponDamage.Text = objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAPLabel.Visible = true;
                lblWeaponAP.Visible = true;
                lblWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblWeaponAccuracyLabel.Visible = true;
                lblWeaponAccuracy.Visible = true;
                lblWeaponAccuracy.Text = objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponDicePoolLabel.Visible = true;
                lblWeaponDicePool.Visible = true;
                lblWeaponDicePool.Text = objWeapon.GetDicePool(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponDicePool.SetToolTip(objWeapon.DicePoolTooltip);
                if (objWeapon.WeaponType == "Ranged")
                {
                    lblWeaponReachLabel.Visible = false;
                    lblWeaponReach.Visible = false;
                    lblWeaponRCLabel.Visible = true;
                    lblWeaponRC.Visible = true;
                    lblWeaponRC.Text = objWeapon.TotalRC(GlobalOptions.CultureInfo, GlobalOptions.Language, true);
                    lblWeaponRC.SetToolTip(objWeapon.RCToolTip);
                    lblWeaponAmmoLabel.Visible = true;
                    lblWeaponAmmo.Visible = true;
                    lblWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    lblWeaponModeLabel.Visible = true;
                    lblWeaponMode.Visible = true;
                    lblWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);

                    tlpWeaponsRanges.Visible = true;
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
                    lblWeaponReachLabel.Visible = true;
                    lblWeaponReach.Visible = true;
                    lblWeaponReach.Text = objWeapon.TotalReach.ToString();
                    lblWeaponRCLabel.Visible = false;
                    lblWeaponRC.Visible = false;
                    if (objWeapon.Ammo != "0")
                    {
                        lblWeaponAmmoLabel.Visible = true;
                        lblWeaponAmmo.Visible = true;
                        lblWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                lblWeaponDeviceRating.Text = objWeapon.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponAttack.Text = objWeapon.GetTotalMatrixAttribute("Attack").ToString();
                lblWeaponSleaze.Text = objWeapon.GetTotalMatrixAttribute("Sleaze").ToString();
                lblWeaponDataProcessing.Text = objWeapon.GetTotalMatrixAttribute("Data Processing").ToString();
                lblWeaponFirewall.Text = objWeapon.GetTotalMatrixAttribute("Firewall").ToString();
            }
            else if (treWeapons.SelectedNode?.Tag is WeaponAccessory objSelectedAccessory)
            {
                gpbWeaponsCommon.Visible = true;
                gpbWeaponsWeapon.Visible = true;
                gpbWeaponsMatrix.Visible = false;

                // Buttons
                cmdDeleteWeapon.Enabled = !objSelectedAccessory.IncludedInWeapon;

                // gpbWeaponsCommon
                lblWeaponName.Text = objSelectedAccessory.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = LanguageManager.GetString("String_WeaponAccessory", GlobalOptions.Language);
                if (objSelectedAccessory.MaxRating > 0)
                {
                    lblWeaponRatingLabel.Visible = true;
                    lblWeaponRating.Visible = true;
                    lblWeaponRating.Text = objSelectedAccessory.Rating.ToString(GlobalOptions.CultureInfo);
                }
                else
                {
                    lblWeaponRatingLabel.Visible = false;
                    lblWeaponRating.Visible = false;
                }
                lblWeaponCapacityLabel.Visible = false;
                lblWeaponCapacity.Visible = false;
                lblWeaponAvail.Text = objSelectedAccessory.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponCost.Text = objSelectedAccessory.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblWeaponSlotsLabel.Visible = true;
                lblWeaponSlots.Visible = true;
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
                lblWeaponConcealLabel.Visible = objSelectedAccessory.TotalConcealability != 0;
                lblWeaponConceal.Visible = objSelectedAccessory.TotalConcealability != 0;
                lblWeaponConceal.Text = objSelectedAccessory.TotalConcealability.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                chkWeaponAccessoryInstalled.Visible = true;
                chkWeaponAccessoryInstalled.Enabled = objSelectedAccessory.Parent != null;
                chkWeaponAccessoryInstalled.Checked = objSelectedAccessory.Equipped;
                chkIncludedInWeapon.Visible = objSelectedAccessory.Parent != null;
                chkIncludedInWeapon.Enabled = CharacterObjectOptions.AllowEditPartOfBaseWeapon;
                chkIncludedInWeapon.Checked = objSelectedAccessory.IncludedInWeapon;

                // gpbWeaponsWeapon
                gpbWeaponsWeapon.Text = LanguageManager.GetString("String_WeaponAccessory", GlobalOptions.Language);
                if (string.IsNullOrEmpty(objSelectedAccessory.Damage))
                {
                    lblWeaponDamageLabel.Visible = false;
                    lblWeaponDamage.Visible = false;
                }
                else
                {
                    lblWeaponDamageLabel.Visible = !string.IsNullOrEmpty(objSelectedAccessory.Damage);
                    lblWeaponDamage.Visible = !string.IsNullOrEmpty(objSelectedAccessory.Damage);
                    lblWeaponDamage.Text = Convert.ToInt32(objSelectedAccessory.Damage).ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
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
                    lblWeaponAP.Text = Convert.ToInt32(objSelectedAccessory.AP).ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
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
                    lblWeaponAccuracy.Text = objSelectedAccessory.Accuracy.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
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
                    lblWeaponDicePool.Text = objSelectedAccessory.DicePool.ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
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
                    lblWeaponRC.Text = Convert.ToInt32(objSelectedAccessory.RC).ToString("+#,0;-#,0;0", GlobalOptions.CultureInfo);
                }
                if (objSelectedAccessory.AmmoBonus != 0 && !string.IsNullOrEmpty(objSelectedAccessory.ModifyAmmoCapacity) && objSelectedAccessory.ModifyAmmoCapacity != "0")
                {
                    lblWeaponAmmoLabel.Visible = true;
                    lblWeaponAmmo.Visible = true;
                    StringBuilder strbldAmmoBonus = new StringBuilder();
                    if (objSelectedAccessory.AmmoBonus != 0)
                        strbldAmmoBonus.Append(objSelectedAccessory.AmmoBonus.ToString("+#,0%;-#,0%;0%", GlobalOptions.CultureInfo));
                    if (!string.IsNullOrEmpty(objSelectedAccessory.ModifyAmmoCapacity) && objSelectedAccessory.ModifyAmmoCapacity != "0")
                        strbldAmmoBonus.Append(objSelectedAccessory.ModifyAmmoCapacity);
                    lblWeaponAmmo.Text = strbldAmmoBonus.ToString();
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
            else if (treWeapons.SelectedNode?.Tag is Gear objGear)
            {
                gpbWeaponsCommon.Visible = true;
                gpbWeaponsWeapon.Visible = false;
                gpbWeaponsMatrix.Visible = true;

                // Buttons
                cmdDeleteWeapon.Enabled = !objGear.IncludedInParent;

                // gpbWeaponsCommon
                lblWeaponName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblWeaponCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                int intGearMaxRatingValue = objGear.MaxRatingValue;
                if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                {
                    lblWeaponRatingLabel.Visible = true;
                    lblWeaponRating.Visible = true;
                    lblWeaponRating.Text = objGear.Rating.ToString(GlobalOptions.CultureInfo);
                }
                else
                {
                    lblWeaponRatingLabel.Visible = false;
                    lblWeaponRating.Visible = false;
                }
                lblWeaponCapacityLabel.Visible = true;
                lblWeaponCapacity.Visible = true;
                lblWeaponCapacity.Text = objGear.CalculatedCapacity + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo)
                                         + strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblWeaponAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblWeaponSlotsLabel.Visible = false;
                lblWeaponSlots.Visible = false;
                lblWeaponConcealLabel.Visible = false;
                lblWeaponConceal.Visible = false;
                chkWeaponAccessoryInstalled.Visible = true;
                chkWeaponAccessoryInstalled.Enabled = objGear.IncludedInParent;
                chkWeaponAccessoryInstalled.Checked = objGear.Equipped;
                chkIncludedInWeapon.Visible = false;

                // gpbWeaponsMatrix
                lblWeaponDeviceRating.Text = objGear.GetTotalMatrixAttribute("Device Rating").ToString();
                lblWeaponAttack.Text = objGear.GetTotalMatrixAttribute("Attack").ToString();
                lblWeaponSleaze.Text = objGear.GetTotalMatrixAttribute("Sleaze").ToString();
                lblWeaponDataProcessing.Text = objGear.GetTotalMatrixAttribute("Data Processing").ToString();
                lblWeaponFirewall.Text = objGear.GetTotalMatrixAttribute("Firewall").ToString();
            }
            else
            {
                gpbWeaponsCommon.Visible = false;
                gpbWeaponsWeapon.Visible = false;
                gpbWeaponsMatrix.Visible = false;

                // Buttons
                cmdDeleteWeapon.Enabled = false;
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
                cmdDeleteArmor.Enabled = false;

                IsRefreshing = false;
                flpArmor.ResumeLayout();
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

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);

            if (treArmor.SelectedNode?.Tag is Armor objArmor)
            {
                gpbArmorCommon.Visible = true;
                gpbArmorMatrix.Visible = false;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = true;

                // gpbArmorCommon
                lblArmorValueLabel.Visible = true;
                lblArmorValue.Visible = true;
                lblArmorValue.Text = objArmor.DisplayArmorValue;
                lblArmorAvail.Text = objArmor.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblArmorCapacity.Text = objArmor.CalculatedCapacity + strSpace + '(' + objArmor.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo)
                                        + strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                lblArmorRatingLabel.Visible = false;
                nudArmorRating.Visible = false;
                lblArmorCost.Text = objArmor.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                chkArmorEquipped.Visible = true;
                chkArmorEquipped.Checked = objArmor.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Visible = false;
            }
            else if (treArmor.SelectedNode?.Tag is ArmorMod objArmorMod)
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
                    lblArmorValue.Text = objArmorMod.Armor.ToString("+0;-0;0");
                }
                else
                {
                    lblArmorValueLabel.Visible = false;
                    lblArmorValue.Visible = false;
                }

                lblArmorAvail.Text = objArmorMod.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblArmorCapacity.Text = objArmorMod.Parent.CapacityDisplayStyle == CapacityStyle.Zero
                    ? "[0]"
                    : objArmorMod.CalculatedCapacity;
                if (!string.IsNullOrEmpty(objArmorMod.GearCapacity))
                    lblArmorCapacity.Text = objArmorMod.GearCapacity + '/' + lblArmorCapacity.Text + strSpace + '(' +
                                            objArmorMod.GearCapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                            strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                if (objArmorMod.MaximumRating > 1)
                {
                    lblArmorRatingLabel.Visible = true;
                    nudArmorRating.Visible = true;
                    nudArmorRating.Maximum = objArmorMod.MaximumRating;
                    nudArmorRating.Value = objArmorMod.Rating;
                    nudArmorRating.Enabled = true;
                }
                else
                {
                    lblArmorRatingLabel.Visible = false;
                    nudArmorRating.Visible = false;
                }
                lblArmorCost.Text = objArmorMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                chkArmorEquipped.Visible = true;
                chkArmorEquipped.Checked = objArmorMod.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Visible = true;
                chkIncludedInArmor.Checked = objArmorMod.IncludedInArmor;
            }
            else if (treArmor.SelectedNode?.Tag is Gear objSelectedGear)
            {
                gpbArmorCommon.Visible = true;
                gpbArmorMatrix.Visible = true;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = !objSelectedGear.IncludedInParent;

                // gpbArmorCommon
                lblArmorValueLabel.Visible = false;
                lblArmorValue.Visible = false;
                lblArmorAvail.Text = objSelectedGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                CharacterObject.Armor.FindArmorGear(objSelectedGear.InternalId, out objArmor, out objArmorMod);
                if (objArmorMod != null)
                    lblArmorCapacity.Text = objSelectedGear.CalculatedCapacity;
                else if (objArmor.CapacityDisplayStyle == CapacityStyle.Zero)
                    lblArmorCapacity.Text = "[0]";
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
                    nudArmorRating.Enabled = intMinRatingValue != intMaxRatingValue;
                }
                else
                {
                    lblArmorRatingLabel.Visible = false;
                    nudArmorRating.Visible = false;
                }
                lblArmorCost.Text = objSelectedGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                chkArmorEquipped.Visible = true;
                chkArmorEquipped.Checked = objSelectedGear.Equipped;
                chkArmorEquipped.Enabled = true;
                chkIncludedInArmor.Visible = true;
                chkIncludedInArmor.Checked = objSelectedGear.IncludedInParent;

                // gpbArmorMatrix
                lblArmorDeviceRating.Text = objSelectedGear.GetTotalMatrixAttribute("Device Rating").ToString();
                lblArmorAttack.Text = objSelectedGear.GetTotalMatrixAttribute("Attack").ToString();
                lblArmorSleaze.Text = objSelectedGear.GetTotalMatrixAttribute("Sleaze").ToString();
                lblArmorDataProcessing.Text = objSelectedGear.GetTotalMatrixAttribute("Data Processing").ToString();
                lblArmorFirewall.Text = objSelectedGear.GetTotalMatrixAttribute("Firewall").ToString();
            }
            else if (treArmor.SelectedNode?.Tag is Location objLocation)
            {
                gpbArmorCommon.Visible = false;
                gpbArmorMatrix.Visible = false;
                gpbArmorLocation.Visible = true;

                // Buttons
                cmdDeleteArmor.Enabled = true;

                // gpbArmorLocation
                StringBuilder strArmorEquipped = new StringBuilder();
                foreach (Armor objLoopArmor in CharacterObject.Armor.Where(objLoopArmor => objLoopArmor.Equipped && objLoopArmor.Location == objLocation))
                {
                    strArmorEquipped.Append(objLoopArmor.DisplayName(GlobalOptions.Language));
                    strArmorEquipped.Append(strSpace + '(');
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
            }
            else if (treArmor.SelectedNode?.Tag.ToString() == "Node_SelectedArmor")
            {
                gpbArmorCommon.Visible = false;
                gpbArmorMatrix.Visible = false;
                gpbArmorLocation.Visible = true;

                // Buttons
                cmdDeleteArmor.Enabled = false;

                StringBuilder strArmorEquipped = new StringBuilder();
                foreach (Armor objLoopArmor in CharacterObject.Armor.Where(objLoopArmor => objLoopArmor.Equipped && objLoopArmor.Location == null))
                {
                    strArmorEquipped.Append(objLoopArmor.DisplayName(GlobalOptions.Language));
                    strArmorEquipped.Append(strSpace + '(');
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
            }
            else
            {
                gpbArmorCommon.Visible = false;
                gpbArmorMatrix.Visible = false;
                gpbArmorLocation.Visible = false;

                // Buttons
                cmdDeleteArmor.Enabled = false;
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
                cmdDeleteGear.Enabled = false;

                IsRefreshing = false;
                flpGear.ResumeLayout();
                return;
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

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);

            if (treGear.SelectedNode?.Tag is Gear objGear)
            {
                gpbGearCommon.Visible = true;
                gpbGearMatrix.Visible = true;

                // Buttons
                cmdDeleteGear.Enabled = !objGear.IncludedInParent;

                // gpbGearCommon
                lblGearName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblGearCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
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
                    nudGearRating.Enabled = nudGearRating.Minimum != nudGearRating.Maximum;
                }
                else
                {
                    nudGearRating.Minimum = 0;
                    nudGearRating.Maximum = 0;
                    nudGearRating.Enabled = false;
                }

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
                nudGearQty.Enabled = !objGear.IncludedInParent;
                try
                {
                    lblGearCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat + '¥', GlobalOptions.CultureInfo);
                }
                catch (FormatException)
                {
                    lblGearCost.Text = objGear.Cost + "¥";
                }
                lblGearAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                try
                {
                    lblGearCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                catch (FormatException)
                {
                    lblGearCost.Text = objGear.Cost + "¥";
                }
                lblGearCapacity.Text = objGear.CalculatedCapacity + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo)
                                       + strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                chkGearEquipped.Visible = true;
                chkGearEquipped.Checked = objGear.Equipped;
                // If this is a Program, determine if its parent Gear (if any) is a Commlink. If so, show the Equipped checkbox.
                if (objGear.IsProgram && objGear.Parent is IHasMatrixAttributes objCommlink && objCommlink.IsCommlink)
                {
                    chkGearEquipped.Text = LanguageManager.GetString("Checkbox_SoftwareRunning", GlobalOptions.Language);
                }
                else
                {
                    chkGearEquipped.Text = LanguageManager.GetString("Checkbox_Equipped", GlobalOptions.Language);
                }


                // gpbGearMatrix
                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                lblGearDeviceRating.Text = intDeviceRating.ToString();
                objGear.RefreshMatrixAttributeCBOs(cboGearAttack, cboGearSleaze, cboGearDataProcessing, cboGearFirewall);
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

                treGear.SelectedNode.Text = objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                objKarma.Create(CharacterObject.Karma, LanguageManager.GetString("Label_SelectBP_StartingKarma", GlobalOptions.Language), ExpenseType.Karma, DateTime.Now);
                CharacterObject.ExpenseEntries.AddWithSort(objKarma);

                // Create an Undo entry so that the starting Karma amount can be modified if needed.
                ExpenseUndo objKarmaUndo = new ExpenseUndo();
                objKarmaUndo.CreateKarma(KarmaExpenseType.ManualAdd, string.Empty);
                objKarma.Undo = objKarmaUndo;
            }
            if (CharacterObject.MetatypeCategory == "Shapeshifter")
            {
                List<CharacterAttrib> lstAttributesToAdd = new List<CharacterAttrib>();
                XmlDocument xmlDoc = XmlManager.Load("metatypes.xml");
                string strMetavariantXPath = $"/chummer/metatypes/metatype[name = \"{CharacterObject.Metatype}\"]/metavariants/metavariant[name = \"{CharacterObject.Metavariant}\"]";
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
            objNuyen.Create(CharacterObject.Nuyen, LanguageManager.GetString("Title_LifestyleNuyen", GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
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
            frmSelectCyberware frmPickCyberware = new frmSelectCyberware(CharacterObject, objSource, objSelectedCyberware);
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
            objCyberware.Create(objXmlCyberware, frmPickCyberware.SelectedGrade, objSource, frmPickCyberware.SelectedRating, lstWeapons, lstVehicles, true, true, string.Empty, objSelectedCyberware);
            if (objCyberware.InternalId.IsEmptyGuid())
                return false;
            
            if (objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID)
            {
                CharacterObject.DecreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100));
            }
            else if (objCyberware.SourceID == Cyberware.EssenceHoleGUID)
            {
                CharacterObject.IncreaseEssenceHole((int)(objCyberware.CalculatedESS() * 100));
            }
            else
            {
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
            }

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
            Location objLocation = null;
            if (objSelectedGear == null)
            {
                objSelectedGear = new Gear(CharacterObject);
                blnNullParent = true;
                objLocation =
                    CharacterObject.GearLocations.FirstOrDefault(location => location.InternalId == strSelectedId);
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

            frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, objSelectedGear.ChildAvailModifier, objSelectedGear.ChildCostMultiplier, objSelectedGear, strCategories);
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
                objGear.Location = objLocation;
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
            object objParent = objSelectedGear ?? objSelectedMod ?? (object)objSelectedArmor;
            Cursor = Cursors.WaitCursor;

            string strCategories = string.Empty;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNodeList xmlAddonCategoryList = (objParent as IHasXmlNode)?.GetNode()?.SelectNodes("addoncategory");
                if (xmlAddonCategoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlCategory in xmlAddonCategoryList)
                        strCategories += objXmlCategory.InnerText + ",";
                    // Remove the trailing comma.
                    if (strCategories.Length > 0)
                        strCategories = strCategories.Substring(0, strCategories.Length - 1);
                }
            }

            frmSelectGear frmPickGear = new frmSelectGear(CharacterObject, 0, 1, objParent, strCategories)
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
            XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[id = \"" + frmPickGear.SelectedGear + "\"]");

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
            IsRefreshing = true;
            flpLifestyleDetails.SuspendLayout();
            if (treLifestyles.SelectedNode == null || treLifestyles.SelectedNode.Level <= 0 || !(treLifestyles.SelectedNode?.Tag is Lifestyle objLifestyle))
            {
                flpLifestyleDetails.Visible = false;
                cmdDeleteLifestyle.Enabled = false;
                IsRefreshing = false;
                flpLifestyleDetails.ResumeLayout();
                return;
            }

            flpLifestyleDetails.Visible = true;
            cmdDeleteLifestyle.Enabled = true;

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            lblLifestyleCost.Text = objLifestyle.TotalMonthlyCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            nudLifestyleMonths.Value = Convert.ToDecimal(objLifestyle.Increments, GlobalOptions.InvariantCultureInfo);
            lblLifestyleStartingNuyen.Text = objLifestyle.Dice + LanguageManager.GetString("String_D6", GlobalOptions.Language) + strSpace + '×' + strSpace + objLifestyle.Multiplier.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            objLifestyle.SetSourceDetail(lblLifestyleSource);
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
            lblLifestyleMonthsLabel.Text = strIncrementString + string.Format(LanguageManager.GetString("Label_LifestylePermanent", GlobalOptions.Language), objLifestyle.IncrementsRequiredForPermanent.ToString(GlobalOptions.CultureInfo));

            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
            {
                string strQualities = string.Join(",\n", objLifestyle.LifestyleQualities.Select(r => r.FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language)));
                
                foreach (Improvement objImprovement in CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LifestyleCost && x.Enabled))
                {
                    if (strQualities.Length > 0)
                        strQualities += ",\n";

                    strQualities += objImprovement.Value > 0
                        ? objImprovement.ImproveSource + LanguageManager.GetString("String_Space", GlobalOptions.Language) + "[+" + objImprovement.Value + "%]"
                        : objImprovement.ImproveSource + LanguageManager.GetString("String_Space", GlobalOptions.Language) + '[' + objImprovement.Value + "%]";
                }

                if (objLifestyle.FreeGrids.Count > 0)
                {
                    if (strQualities.Length > 0)
                        strQualities += ",\n";

                    strQualities += string.Join(",\n", objLifestyle.FreeGrids.Select(r => r.DisplayName(GlobalOptions.Language)));
                }
                
                lblBaseLifestyle.Text = objLifestyle.DisplayNameShort(GlobalOptions.Language);
                lblLifestyleQualities.Text = strQualities;
                lblLifestyleQualitiesLabel.Visible = true;
                lblLifestyleQualities.Visible = true;
            }
            else
            {
                lblBaseLifestyle.Text = LanguageManager.GetString("String_Error", GlobalOptions.Language);
                lblLifestyleQualitiesLabel.Visible = false;
                lblLifestyleQualities.Visible = false;
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
            
            if (string.IsNullOrEmpty(strSelectedId) || treVehicles.SelectedNode.Level <= 0 || treVehicles.SelectedNode?.Tag is Location)
            {
                gpbVehiclesCommon.Visible = false;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = false;

                IsRefreshing = false;
                flpVehicles.ResumeLayout();
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);

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
            // Locate the selected Vehicle.
            if (treVehicles.SelectedNode?.Tag is Vehicle objVehicle)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = true;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = true;

                // Buttons
                cmdDeleteVehicle.Enabled = string.IsNullOrEmpty(objVehicle.ParentID);

                // gpbVehiclesCommon
                lblVehicleName.Text = objVehicle.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objVehicle.DisplayCategory(GlobalOptions.Language);
                lblVehicleRatingLabel.Visible = false;
                nudVehicleRating.Visible = false;
                lblVehicleGearQtyLabel.Visible = false;
                nudVehicleGearQty.Visible = false;
                lblVehicleAvail.Text = objVehicle.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objVehicle.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = !CharacterObjectOptions.BookEnabled("R5");
                lblVehicleSlots.Visible = !CharacterObjectOptions.BookEnabled("R5");
                if (!CharacterObjectOptions.BookEnabled("R5"))
                    lblVehicleSlots.Text = objVehicle.Slots + strSpace + '(' + (objVehicle.Slots - objVehicle.SlotsUsed) +
                                           strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = false;
                chkVehicleIncludedInWeapon.Visible = false;
                
                // gpbVehiclesVehicle
                lblVehicleHandling.Text = objVehicle.TotalHandling;
                lblVehicleAccel.Text = objVehicle.TotalAccel;
                lblVehicleSpeed.Text = objVehicle.TotalSpeed;
                lblVehiclePilot.Text = objVehicle.Pilot.ToString(GlobalOptions.CultureInfo);
                lblVehicleBody.Text = objVehicle.TotalBody.ToString(GlobalOptions.CultureInfo);
                lblVehicleArmor.Text = objVehicle.TotalArmor.ToString(GlobalOptions.CultureInfo);
                lblVehicleSeats.Text = objVehicle.TotalSeats.ToString(GlobalOptions.CultureInfo);
                lblVehicleSensor.Text = objVehicle.CalculatedSensor.ToString(GlobalOptions.CultureInfo);
                if (CharacterObjectOptions.BookEnabled("R5"))
                {
                    if (objVehicle.IsDrone && GlobalOptions.Dronemods)
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
                        lblVehicleDroneModSlots.Text = objVehicle.DroneModSlotsUsed.ToString() + '/' + objVehicle.DroneModSlots;
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
                lblVehicleDevice.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                objVehicle.RefreshMatrixAttributeCBOs(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);
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
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = !objWeaponMount.IncludedInVehicle;

                // gpbVehiclesCommon
                lblVehicleCategory.Text = objWeaponMount.DisplayCategory(GlobalOptions.Language);
                lblVehicleName.Text = objWeaponMount.DisplayNameShort(GlobalOptions.Language);
                lblVehicleRatingLabel.Visible = false;
                nudVehicleRating.Visible = false;
                lblVehicleGearQtyLabel.Visible = false;
                nudVehicleGearQty.Visible = false;
                lblVehicleAvail.Text = objWeaponMount.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objWeaponMount.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo);
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objWeaponMount.CalculatedSlots.ToString();
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objWeaponMount.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objWeaponMount.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Visible = false;
            }
            // Locate the selected VehicleMod.
            else if (treVehicles.SelectedNode?.Tag is VehicleMod objMod)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = !objMod.IncludedInVehicle;

                // gpbVehiclesCommon
                lblVehicleName.Text = objMod.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleModification", GlobalOptions.Language);
                if (objMod.MaxRating != "qty")
                {
                    if (objMod.MaxRating == "Seats")
                    {
                        objMod.MaxRating = objMod.Parent.TotalSeats.ToString();
                    }
                    if (objMod.MaxRating == "body")
                    {
                        objMod.MaxRating = objMod.Parent.TotalBody.ToString();
                    }
                    if (Convert.ToInt32(objMod.MaxRating) > 0)
                    {
                        lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
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
                else
                {
                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                    lblVehicleRatingLabel.Visible = true;
                    nudVehicleRating.Visible = true;
                    nudVehicleRating.Minimum = 1;
                    nudVehicleRating.Maximum = 20;
                    nudVehicleRating.Value = objMod.Rating;
                    nudVehicleRating.Increment = 1;
                    nudVehicleRating.Enabled = !objMod.IncludedInVehicle;
                }
                nudVehicleGearQty.Visible = false;
                lblVehicleGearQtyLabel.Visible = false;
                lblVehicleAvail.Text = objMod.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objMod.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objMod.CalculatedSlots.ToString();
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objMod.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = !objMod.IncludedInVehicle;
                chkVehicleIncludedInWeapon.Visible = false;
            }
            else if (treVehicles.SelectedNode?.Tag is Weapon objWeapon)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = true;
                gpbVehiclesMatrix.Visible = true;

                // Buttons
                cmdDeleteVehicle.Enabled = !objWeapon.Cyberware && objWeapon.Category != "Gear" && !objWeapon.IncludedInWeapon && string.IsNullOrEmpty(objWeapon.ParentID) && !objWeapon.Category.StartsWith("Quality");

                // gpbVehiclesCommon
                lblVehicleName.Text = objWeapon.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objWeapon.DisplayCategory(GlobalOptions.Language);
                lblVehicleRatingLabel.Visible = false;
                nudVehicleRating.Visible = false;
                lblVehicleGearQtyLabel.Visible = false;
                nudVehicleGearQty.Visible = false;
                lblVehicleAvail.Text = objWeapon.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objWeapon.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
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
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objWeapon.Equipped;
                chkVehicleWeaponAccessoryInstalled.Enabled = objWeapon.ParentID != objWeapon.Parent?.InternalId && objWeapon.ParentID != objWeapon.ParentVehicle.InternalId;
                chkVehicleIncludedInWeapon.Visible = true;
                chkVehicleIncludedInWeapon.Checked = objWeapon.IncludedInWeapon;

                // gpbVehiclesWeapon
                lblVehicleWeaponDamage.Text = objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleWeaponAP.Text = objWeapon.TotalAP(GlobalOptions.Language);
                lblVehicleWeaponAccuracy.Text = objWeapon.DisplayAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);
                // Determine the Dice Pool size.
                int intPilot = objWeapon.ParentVehicle.Pilot;
                int intAutosoft = 0;
                foreach (Gear objAutosoft in objWeapon.ParentVehicle.Gear)
                {
                    if (objAutosoft.Extra == objWeapon.DisplayCategory(GlobalOptions.DefaultLanguage) &&
                        (objAutosoft.Name == "[Weapon] Targeting Autosoft" || objAutosoft.Name == "[Weapon] Melee Autosoft"))
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
                if (objWeapon.WeaponType == "Ranged")
                {
                    lblVehicleWeaponAmmoLabel.Visible = true;
                    lblVehicleWeaponAmmo.Visible = true;
                    lblVehicleWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    lblVehicleWeaponModeLabel.Visible = true;
                    lblVehicleWeaponMode.Visible = true;
                    lblVehicleWeaponMode.Text = objWeapon.CalculatedMode(GlobalOptions.Language);

                    tlpVehiclesWeaponRanges.Visible = true;
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
                }
                else
                {
                    if (objWeapon.Ammo != "0")
                    {
                        lblVehicleWeaponAmmoLabel.Visible = true;
                        lblVehicleWeaponAmmo.Visible = true;
                        lblVehicleWeaponAmmo.Text = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
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
                
                // gpbVehiclesMatrix
                int intDeviceRating = objWeapon.GetTotalMatrixAttribute("Device Rating");
                lblVehicleDevice.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                objWeapon.RefreshMatrixAttributeCBOs(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);
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
            }
            else if (treVehicles.SelectedNode?.Tag is WeaponAccessory objAccessory)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = !objAccessory.IncludedInWeapon;
                
                // gpbVehiclesCommon
                lblVehicleName.Text = objAccessory.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = LanguageManager.GetString("String_VehicleWeaponAccessory", GlobalOptions.Language);
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
                lblVehicleAvail.Text = objAccessory.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objAccessory.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = true;
                chkVehicleWeaponAccessoryInstalled.Enabled = true;
                chkVehicleWeaponAccessoryInstalled.Checked = objAccessory.Equipped;
                chkVehicleIncludedInWeapon.Visible = true;
                chkVehicleIncludedInWeapon.Checked = objAccessory.IncludedInWeapon;
            }
            else if (treVehicles.SelectedNode?.Tag is Cyberware objCyberware)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = true;

                // Buttons
                cmdDeleteVehicle.Enabled = string.IsNullOrEmpty(objCyberware.ParentID);

                // gpbVehiclesCommon
                lblVehicleName.Text = objCyberware.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objCyberware.DisplayCategory(GlobalOptions.Language);
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
                    nudVehicleRating.Maximum = Convert.ToDecimal(objCyberware.MaxRating, GlobalOptions.CultureInfo);
                    nudVehicleRating.Minimum = Convert.ToDecimal(objCyberware.MinRating, GlobalOptions.CultureInfo);
                    nudVehicleRating.Value = Convert.ToDecimal(objCyberware.Rating, GlobalOptions.CultureInfo);
                    nudVehicleRating.Visible = true;
                    lblVehicleRatingLabel.Visible = true;
                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                }
                lblVehicleGearQtyLabel.Visible = false;
                nudVehicleGearQty.Visible = false;
                lblVehicleAvail.Text = objCyberware.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objCyberware.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                cmdVehicleCyberwareChangeMount.Visible = !string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount);
                chkVehicleWeaponAccessoryInstalled.Visible = false;
                chkVehicleIncludedInWeapon.Visible = false;

                // gpbVehiclesMatrix
                int intDeviceRating = objCyberware.GetTotalMatrixAttribute("Device Rating");
                lblVehicleDevice.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                objCyberware.RefreshMatrixAttributeCBOs(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);
                
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
            }
            else if (treVehicles.SelectedNode?.Tag is Gear objGear)
            {
                gpbVehiclesCommon.Visible = true;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = true;

                // Buttons
                cmdDeleteVehicle.Enabled = !objGear.IncludedInParent;

                // gpbVehiclesCommon
                lblVehicleName.Text = objGear.DisplayNameShort(GlobalOptions.Language);
                lblVehicleCategory.Text = objGear.DisplayCategory(GlobalOptions.Language);
                int intGearMaxRatingValue = objGear.MaxRatingValue;
                if (intGearMaxRatingValue > 0 && intGearMaxRatingValue != int.MaxValue)
                {
                    lblVehicleRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                    lblVehicleRatingLabel.Visible = true;
                    nudVehicleRating.Visible = true;
                    nudVehicleRating.Enabled = true;
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
                if (objGear.Name.StartsWith("Nuyen"))
                {
                    int intDecimalPlaces = CharacterObjectOptions.NuyenDecimals;
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
                lblVehicleAvail.Text = objGear.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblVehicleCost.Text = objGear.TotalCost.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblVehicleSlotsLabel.Visible = true;
                lblVehicleSlots.Visible = true;
                lblVehicleSlots.Text = objGear.CalculatedCapacity + strSpace + '(' + objGear.CapacityRemaining.ToString("#,0.##", GlobalOptions.CultureInfo) +
                                       strSpace + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
                cmdVehicleCyberwareChangeMount.Visible = false;
                chkVehicleWeaponAccessoryInstalled.Visible = false;
                chkVehicleIncludedInWeapon.Visible = false;

                // gpbVehiclesMatrix
                int intDeviceRating = objGear.GetTotalMatrixAttribute("Device Rating");
                lblVehicleDevice.Text = intDeviceRating.ToString(GlobalOptions.CultureInfo);
                objGear.RefreshMatrixAttributeCBOs(cboVehicleAttack, cboVehicleSleaze, cboVehicleDataProcessing, cboVehicleFirewall);

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
            }
            else
            {
                gpbVehiclesCommon.Visible = false;
                gpbVehiclesVehicle.Visible = false;
                gpbVehiclesWeapon.Visible = false;
                gpbVehiclesMatrix.Visible = false;

                // Buttons
                cmdDeleteVehicle.Enabled = false;
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
                lblDrugAvail.Text = objDrug.TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblDrugGrade.Text = objDrug.Grade;
                lblDrugCost.Text = objDrug.Cost.ToString(CharacterObject.Options.NuyenFormat) + '¥';
                nudDrugQty.Value = objDrug.Quantity;
                lblDrugCategory.Text = objDrug.Category;
                lblDrugAddictionRating.Text = objDrug.AddictionRating.ToString(GlobalOptions.CultureInfo);
                lblDrugAddictionThreshold.Text = objDrug.AddictionThreshold.ToString(GlobalOptions.CultureInfo);
                lblDrugEffect.Text = objDrug.EffectDescription;
                lblDrugComponents.Text = string.Empty;
                foreach (DrugComponent objComponent in objDrug.Components)
                {
                    lblDrugComponents.Text += objComponent.CurrentDisplayName + '\n';
                }
            }
            else
            {
                flpDrugs.Visible = false;
                btnDeleteCustomDrug.Enabled = false;
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

            if (treSpells.SelectedNode != null && treSpells.SelectedNode.Level > 0 && treSpells.SelectedNode.Tag is Spell objSpell)
            {
                gpbMagicianSpell.Visible = true;
                cmdDeleteSpell.Enabled = objSpell.Grade == 0;

                lblSpellDescriptors.Text = objSpell.DisplayDescriptors(GlobalOptions.Language);
                if (string.IsNullOrEmpty(lblSpellDescriptors.Text))
                    lblSpellDescriptors.Text = LanguageManager.GetString("String_None", GlobalOptions.Language);
                lblSpellCategory.Text = objSpell.DisplayCategory(GlobalOptions.Language);
                lblSpellType.Text = objSpell.DisplayType(GlobalOptions.Language);
                lblSpellRange.Text = objSpell.DisplayRange(GlobalOptions.Language);
                lblSpellDamage.Text = objSpell.DisplayDamage(GlobalOptions.Language);
                lblSpellDuration.Text = objSpell.DisplayDuration(GlobalOptions.Language);
                lblSpellDV.Text = objSpell.DisplayDV(GlobalOptions.Language);

                objSpell.SetSourceDetail(lblSpellSource);

                // Determine the size of the Spellcasting Dice Pool.
                lblSpellDicePool.Text = objSpell.DicePool.ToString();
                lblSpellDicePool.SetToolTip(objSpell.DicePoolTooltip);

                // Build the DV tooltip.
                lblSpellDV.SetToolTip(objSpell.DVTooltip);

                // Update the Drain CharacterAttribute Value.
                if (CharacterObject.MAGEnabled && !string.IsNullOrEmpty(lblDrainAttributes.Text))
                {
                    string strSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
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
                        strTip = strTip.CheapReplace(objAttrib.DisplayAbbrev, () => objAttrib.DisplayAbbrev + strSpace + '(' + objAttrib.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
                    }

                    foreach (Improvement objImprovement in CharacterObject.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.DrainResistance && objImprovement.Enabled)
                        {
                            strTip += strSpace + '+' + strSpace + CharacterObject.GetObjectName(objImprovement, GlobalOptions.Language) + strSpace + '(' + objImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')';
                        }
                    }
                    //if (objSpell.Limited)
                    //{
                    //    intDrain += 2;
                    //    strTip += " + " + LanguageManager.GetString("String_SpellLimited") + " (2)";
                    //}
                    lblDrainAttributesValue.Text = intDrain.ToString(GlobalOptions.CultureInfo);
                    lblDrainAttributesValue.SetToolTip(strTip);
                }
            }
            else
            {
                gpbMagicianSpell.Visible = false;
                cmdDeleteSpell.Enabled = false;
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
                lblDuration.Text = objComplexForm.DisplayDuration(GlobalOptions.Language);
                lblTarget.Text = objComplexForm.DisplayTarget(GlobalOptions.Language);
                lblFV.Text = objComplexForm.DisplayFV(GlobalOptions.Language);
                lblFV.SetToolTip(objComplexForm.FVTooltip);

                // Determine the size of the Threading Dice Pool.
                lblComplexFormDicePool.Text = objComplexForm.DicePool.ToString(GlobalOptions.CultureInfo);
                lblComplexFormDicePool.SetToolTip(objComplexForm.DicePoolTooltip);

                objComplexForm.SetSourceDetail(lblComplexFormSource);
            }
            else
            {
                gpbTechnomancerComplexForm.Visible = false;
                cmdDeleteComplexForm.Enabled = false;
            }

            IsRefreshing = false;
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
                if (CharacterObject.AdapsinEnabled && !blnBioware)
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
                if (CharacterObject.BannedWareGrades.Any(s => objWareGrade.Name.Contains(s)) && !CharacterObject.IgnoreRules)
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
                              string.Format(LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language)
                                  , ((1 - intMartialArts) * -1).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
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
                                  string.Format(LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language)
                                      , ((5 - intTechniques) * -1).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
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
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidPointExcess"), ((_objCharacter.ContactPoints - intContactPointsUsed) * -1).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Contacts"));
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
                              string.Format(LanguageManager.GetString("Message_PositiveQualityLimit", GlobalOptions.Language)
                                  , (CharacterObject.GameplayOptionQualityLimit).ToString(GlobalOptions.CultureInfo));
                blnValid = false;
            }
            int totalNeg = CharacterObjectOptions.EnemyKarmaQualityLimit
                ? intNegativePoints
                : (intNegativePoints - intEnemyPoints);
            // if negative points > 25
            if (totalNeg < (CharacterObject.GameplayOptionQualityLimit * -1) && !CharacterObjectOptions.ExceedNegativeQualities)
            {
                strMessage += Environment.NewLine + '\t' +
                              string.Format(LanguageManager.GetString("Message_NegativeQualityLimit", GlobalOptions.Language)
                                  , (CharacterObject.GameplayOptionQualityLimit).ToString(GlobalOptions.CultureInfo));
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
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidPointExcess", GlobalOptions.Language)
                                  , (intBuildPoints * -1).ToString(GlobalOptions.CultureInfo) + LanguageManager.GetString("String_Space", GlobalOptions.Language) + LanguageManager.GetString("String_Karma", GlobalOptions.Language));
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
                    strMessage += Environment.NewLine + '\t' +
                                  string.Format(LanguageManager.GetString("Message_OverNegativeMetagenicQualities", GlobalOptions.Language)
                                      , metageneticNegativeQualities.ToString(GlobalOptions.CultureInfo)
                                      , CharacterObject.MetageneticLimit.ToString(GlobalOptions.CultureInfo));
                    blnValid = false;
                }
                if (metageneticPositiveQualities > CharacterObject.MetageneticLimit)
                {
                    strMessage += Environment.NewLine + '\t' +
                                  string.Format(LanguageManager.GetString("Message_OverPositiveMetagenicQualities", GlobalOptions.Language)
                                      , metageneticPositiveQualities.ToString(GlobalOptions.CultureInfo)
                                      , CharacterObject.MetageneticLimit.ToString(GlobalOptions.CultureInfo));
                    blnValid = false;
                }

                if (metageneticNegativeQualities != metageneticPositiveQualities && metageneticNegativeQualities != (metageneticPositiveQualities - 1))
                {
                    strMessage += Environment.NewLine + '\t' +
                                  string.Format(LanguageManager.GetString("Message_MetagenicQualitiesUnbalanced", GlobalOptions.Language)
                                      , metageneticNegativeQualities.ToString(GlobalOptions.CultureInfo)
                                      , (metageneticPositiveQualities - 1).ToString(GlobalOptions.CultureInfo)
                                      , metageneticPositiveQualities.ToString(GlobalOptions.CultureInfo));
                    blnValid = false;
                }
                //Subtract 1 karma to balance Metagenic Qualities
                if (metageneticNegativeQualities == (metageneticPositiveQualities - 1))
                {
                    if (CharacterObject.Karma > 0)
                    {
                        if (MessageBox.Show(string.Format(LanguageManager.GetString("Message_MetagenicQualitiesSubtractingKarma", GlobalOptions.Language), intBuildPoints.ToString(GlobalOptions.CultureInfo)),
                                LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
                        strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_MetagenicQualitiesInsufficientKarma", GlobalOptions.Language), intBuildPoints.ToString(GlobalOptions.CultureInfo));
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
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidAttributeExcess", GlobalOptions.Language), (i * -1).ToString(GlobalOptions.CultureInfo));
            }

            i = CharacterObject.TotalSpecial - CalculateAttributePriorityPoints(CharacterObject.AttributeSection.SpecialAttributeList);
            // Check if the character has gone over on Special Attributes
            if (i < 0)
            {
                //TODO: ATTACH TO ATTRIBUTE SECTION
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidSpecialExcess", GlobalOptions.Language), (i * -1).ToString(GlobalOptions.CultureInfo));
            }

            // Check if the character has gone over on Skill Groups
            if (CharacterObject.SkillsSection.SkillGroupPoints < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidSkillGroupExcess", GlobalOptions.Language), ((CharacterObject.SkillsSection.SkillGroupPoints) * -1).ToString(GlobalOptions.CultureInfo));
            }

            // Check if the character has gone over on Active Skills
            if (CharacterObject.SkillsSection.SkillPoints < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidActiveSkillExcess", GlobalOptions.Language), ((CharacterObject.SkillsSection.SkillPoints) * -1).ToString(GlobalOptions.CultureInfo));
            }

            // Check if the character has gone over on Knowledge Skills
            if (CharacterObject.SkillsSection.KnowledgeSkillPointsRemain < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidKnowledgeSkillExcess", GlobalOptions.Language), ((CharacterObject.SkillsSection.KnowledgeSkillPointsRemain) * -1).ToString(GlobalOptions.CultureInfo));
            }

            // Check if the character has gone over the Nuyen limit.
            decimal decNuyen = CalculateNuyen();
            if (decNuyen < 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidNuyenExcess", GlobalOptions.Language), (decNuyen * -1).ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥');
            }

            // Check if the character's Essence is above 0.
            double dblEss = decimal.ToDouble(CharacterObject.Essence());
            double dblMinEss = CharacterObjectOptions.DontRoundEssenceInternally ? 0.0 : Math.Pow(10.0, -CharacterObjectOptions.EssenceDecimals);
            if (dblEss < dblMinEss && CharacterObject.ESS.MetatypeMaximum > 0)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidEssenceExcess", GlobalOptions.Language), (dblMinEss - dblEss).ToString(GlobalOptions.CultureInfo));
            }

            // If the character has the Spells & Spirits Tab enabled, make sure a Tradition has been selected.
            if ((CharacterObject.MagicianEnabled || CharacterObject.AdeptEnabled) && CharacterObject.MagicTradition.Type != TraditionType.MAG)
            {
                blnValid = false;
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_InvalidNoTradition", GlobalOptions.Language);
            }

            // If the character has the Technomencer Tab enabled, make sure a Stream has been selected.
            if (CharacterObject.TechnomancerEnabled && CharacterObject.MagicTradition.Type != TraditionType.RES)
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
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_OverLanguageLimit", GlobalOptions.Language), intLanguages.ToString(GlobalOptions.CultureInfo), intLanguageLimit.ToString(GlobalOptions.CultureInfo));
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
            foreach (Cyberware objCyberware in CharacterObject.Cyberware.GetAllDescendants(x => x.Children))
            {
                if (string.IsNullOrEmpty(objCyberware.ParentID))
                {
                    if (CharacterObject.BannedWareGrades.Any(s => objCyberware.Grade.Name.Contains(s)))
                        strCyberwareGrade += Environment.NewLine + "\t\t" + objCyberware.DisplayNameShort(GlobalOptions.Language);

                    AvailabilityValue objTotalAvail = objCyberware.TotalAvailTuple();
                    if (!objTotalAvail.AddToParent)
                    {
                        int intAvailInt = objTotalAvail.Value;
                        if (intAvailInt > CharacterObject.MaximumAvailability)
                        {
                            if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                }

                foreach (Gear objGear in objCyberware.Gear)
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
                    if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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

                foreach (ArmorMod objMod in objArmor.ArmorMods)
                {
                    if (!objMod.IncludedInArmor)
                    {
                        AvailabilityValue objTotalAvail = objMod.TotalAvailTuple();
                        if (!objTotalAvail.AddToParent)
                        {
                            int intModAvailInt = objTotalAvail.Value;
                            if (intModAvailInt > CharacterObject.MaximumAvailability)
                            {
                                if (intModAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                    }

                    foreach (Gear objGear in objMod.Gear)
                    {
                        CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                    }
                }

                foreach (Gear objGear in objArmor.Gear)
                {
                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                }
            }

            // Weapon Availability.
            foreach (Weapon objWeapon in CharacterObject.Weapons.GetAllDescendants(x => x.Children))
            {
                if (!objWeapon.IncludedInWeapon)
                {
                    AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                    if (!objWeaponAvail.AddToParent)
                    {
                        int intAvailInt = objWeaponAvail.Value;
                        if (intAvailInt > CharacterObject.MaximumAvailability)
                        {
                            if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                }

                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                {
                    if (!objAccessory.IncludedInWeapon)
                    {
                        AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                        if (!objAccessoryAvail.AddToParent)
                        {
                            int intAccessoryAvailInt = objAccessoryAvail.Value;
                            if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                            {
                                if (intAccessoryAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                if (string.IsNullOrEmpty(objVehicle.ParentID))
                {
                    int intAvailInt = objVehicle.TotalAvailTuple().Value;
                    if (intAvailInt > CharacterObject.MaximumAvailability)
                    {
                        if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                }

                foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                {
                    if (!objVehicleMod.IncludedInVehicle)
                    {
                        AvailabilityValue objModAvail = objVehicleMod.TotalAvailTuple();
                        if (!objModAvail.AddToParent)
                        {
                            int intModAvailInt = objModAvail.Value;
                            if (intModAvailInt > CharacterObject.MaximumAvailability)
                            {
                                if (intModAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                        }
                    }

                    foreach (Cyberware objCyberware in objVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        if (string.IsNullOrEmpty(objCyberware.ParentID))
                        {
                            if (CharacterObject.BannedWareGrades.Any(s => objCyberware.Grade.Name.Contains(s)))
                                strCyberwareGrade += Environment.NewLine + "\t\t" + objCyberware.DisplayNameShort(GlobalOptions.Language);

                            AvailabilityValue objTotalAvail = objCyberware.TotalAvailTuple();
                            if (!objTotalAvail.AddToParent)
                            {
                                int intAvailInt = objTotalAvail.Value;
                                if (intAvailInt > CharacterObject.MaximumAvailability)
                                {
                                    if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                        }

                        foreach (Gear objGear in objCyberware.Gear)
                        {
                            CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                        }
                    }

                    foreach (Weapon objWeapon in objVehicleMod.Weapons.GetAllDescendants(x => x.Children))
                    {
                        if (!objWeapon.IncludedInWeapon)
                        {
                            AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                            if (!objWeaponAvail.AddToParent)
                            {
                                int intWeaponAvailInt = objWeaponAvail.Value;
                                if (intWeaponAvailInt > CharacterObject.MaximumAvailability)
                                {
                                    if (intWeaponAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                        }
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            if (!objAccessory.IncludedInWeapon)
                            {
                                AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                                if (!objAccessoryAvail.AddToParent)
                                {
                                    int intAccessoryAvailInt = objAccessoryAvail.Value;
                                    if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                                    {
                                        if (intAccessoryAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                            }

                            foreach (Gear objGear in objAccessory.Gear)
                            {
                                CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                            }
                        }
                    }

                    foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objWeaponMount.Weapons.GetAllDescendants(x => x.Children))
                        {
                            if (!objWeapon.IncludedInWeapon)
                            {
                                AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                                if (!objWeaponAvail.AddToParent)
                                {
                                    int intWeaponAvailInt = objWeaponAvail.Value;
                                    if (intWeaponAvailInt > CharacterObject.MaximumAvailability)
                                    {
                                        if (intWeaponAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                            }

                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                if (!objAccessory.IncludedInWeapon)
                                {
                                    AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                                    if (!objAccessoryAvail.AddToParent)
                                    {
                                        int intAccessoryAvailInt = objAccessoryAvail.Value;
                                        if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                                        {
                                            if (intAccessoryAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                                }

                                foreach (Gear objGear in objAccessory.Gear)
                                {
                                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                                }
                            }
                        }

                        foreach (VehicleMod objWeaponMountMod in objWeaponMount.Mods)
                        {
                            if (!objWeaponMountMod.IncludedInVehicle)
                            {
                                AvailabilityValue objModAvail = objWeaponMountMod.TotalAvailTuple();
                                if (!objModAvail.AddToParent)
                                {
                                    int intModAvailInt = objModAvail.Value;
                                    if (intModAvailInt > CharacterObject.MaximumAvailability)
                                    {
                                        if (intModAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                                        {
                                            blnRestrictedGearUsed = true;
                                            strRestrictedItem = objWeaponMountMod.DisplayName(GlobalOptions.Language);
                                        }
                                        else
                                        {
                                            intRestrictedCount++;
                                            strAvailItems += Environment.NewLine + "\t\t" + objWeaponMountMod.DisplayNameShort(GlobalOptions.Language);
                                        }
                                    }
                                }
                            }

                            foreach (Cyberware objCyberware in objWeaponMountMod.Cyberware.GetAllDescendants(x => x.Children))
                            {
                                if (string.IsNullOrEmpty(objCyberware.ParentID))
                                {
                                    if (CharacterObject.BannedWareGrades.Any(s => objCyberware.Grade.Name.Contains(s)))
                                        strCyberwareGrade += Environment.NewLine + "\t\t" + objCyberware.DisplayNameShort(GlobalOptions.Language);

                                    AvailabilityValue objTotalAvail = objCyberware.TotalAvailTuple();
                                    if (!objTotalAvail.AddToParent)
                                    {
                                        int intAvailInt = objTotalAvail.Value;
                                        if (intAvailInt > CharacterObject.MaximumAvailability)
                                        {
                                            if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                                }

                                foreach (Gear objGear in objCyberware.Gear)
                                {
                                    CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                                }
                            }

                            foreach (Weapon objWeapon in objWeaponMountMod.Weapons.GetAllDescendants(x => x.Children))
                            {
                                if (!objWeapon.IncludedInWeapon)
                                {
                                    AvailabilityValue objWeaponAvail = objWeapon.TotalAvailTuple();
                                    if (!objWeaponAvail.AddToParent)
                                    {
                                        int intWeaponAvailInt = objWeaponAvail.Value;
                                        if (intWeaponAvailInt > CharacterObject.MaximumAvailability)
                                        {
                                            if (intWeaponAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                                }

                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    if (!objAccessory.IncludedInWeapon)
                                    {
                                        AvailabilityValue objAccessoryAvail = objAccessory.TotalAvailTuple();
                                        if (!objAccessoryAvail.AddToParent)
                                        {
                                            int intAccessoryAvailInt = objAccessoryAvail.Value;
                                            if (intAccessoryAvailInt > CharacterObject.MaximumAvailability)
                                            {
                                                if (intAccessoryAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
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
                                    }

                                    foreach (Gear objGear in objAccessory.Gear)
                                    {
                                        CheckRestrictedGear(objGear, blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
                                    }
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
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_InvalidAvail", GlobalOptions.Language)
                                  , (intRestrictedCount - intRestrictedAllowed).ToString(GlobalOptions.CultureInfo)
                                  , CharacterObject.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
                strMessage += strAvailItems;
                strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_RestrictedGearUsed", GlobalOptions.Language), strRestrictedItem);
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
                    strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_CapacityReachedValidate", GlobalOptions.Language), intCapacityOver.ToString(GlobalOptions.CultureInfo));
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
                        foreach (VehicleMod objMod in objVehicle.Mods.Where(objMod => !objMod.IncludedInVehicle && objMod.Equipped && objMod.Downgrade))
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
                    strMessage += Environment.NewLine + '\t' + string.Format(LanguageManager.GetString("Message_DroneIllegalDowngrade", GlobalOptions.Language), intIllegalDowngrades.ToString(GlobalOptions.CultureInfo));
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
                    string.Format(LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                        , i.ToString(GlobalOptions.CultureInfo)
                        , LanguageManager.GetString("Label_SummaryPrimaryAttributes", GlobalOptions.Language)),
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
                        string.Format(LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            , i.ToString(GlobalOptions.CultureInfo)
                            , LanguageManager.GetString("Label_SummarySpecialAttributes", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Skill Groups
            if (blnValid && CharacterObject.SkillsSection.SkillGroupPoints > 0)
            {
                if (
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            , CharacterObject.SkillsSection.SkillGroupPoints.ToString(GlobalOptions.CultureInfo)
                            , LanguageManager.GetString("Label_SummarySkillGroups", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Active Skills
            if (blnValid && CharacterObject.SkillsSection.SkillPoints > 0)
            {
                if (
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            , CharacterObject.SkillsSection.SkillPoints.ToString(GlobalOptions.CultureInfo)
                            , LanguageManager.GetString("Label_SummaryActiveSkills", GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_ExtraPoints", GlobalOptions.Language), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    blnValid = false;
            }

            // Check if the character has gone over on Knowledge Skills
            if (blnValid && CharacterObject.SkillsSection.KnowledgeSkillPointsRemain > 0)
            {
                if (
                    MessageBox.Show(
                        string.Format(LanguageManager.GetString("Message_ExtraPoints", GlobalOptions.Language)
                            , CharacterObject.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalOptions.CultureInfo)
                            , LanguageManager.GetString("Label_SummaryKnowledgeSkills", GlobalOptions.Language)),
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
            if (!objGear.IncludedInParent)
            {
                AvailabilityValue objTotalAvail = objGear.TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    //TODO: Make this dynamically update without having to validate the character.
                    if (intAvailInt > CharacterObject.MaximumAvailability)
                    {
                        if (intAvailInt <= CharacterObject.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = objGear.Parent == null
                                ? objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language)
                                : $"{objGear.DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language)} ({objGear.Parent})";
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + objGear.DisplayNameShort(GlobalOptions.Language);
                        }
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
                        if (MessageBox.Show(string.Format(LanguageManager.GetString("Message_NoExtraKarma", GlobalOptions.Language), intBuildPoints.ToString(GlobalOptions.CultureInfo)),
                                LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            return false;
                    }
                    else
                    {
                        if (MessageBox.Show(string.Format(LanguageManager.GetString("Message_ExtraKarma", GlobalOptions.Language)
                                , intBuildPoints.ToString(GlobalOptions.CultureInfo)
                                , CharacterObjectOptions.KarmaCarryover.ToString(GlobalOptions.CultureInfo)),
                                LanguageManager.GetString("MessageTitle_ExtraKarma", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            return false;
                    }
                }
                if (CharacterObject.Nuyen > 5000)
                {
                    if (MessageBox.Show(string.Format(LanguageManager.GetString("Message_ExtraNuyen", GlobalOptions.Language)
                            , CharacterObject.Nuyen.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo)
                            , (5000).ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo)),
                            LanguageManager.GetString("MessageTitle_ExtraNuyen", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return false;
                }
                if (GlobalOptions.CreateBackupOnCareer && chkCharacterCreated.Checked)
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

                    strNewName = Path.Combine(Utils.GetStartupPath, "saves", "backup", strNewName);

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
                        objLifestyle.CustomName = strName;
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

                strInitTip = string.Format(LanguageManager.GetString("Tip_ImproveInitiateGrade", GlobalOptions.Language)
                    , (CharacterObject.InitiateGrade + 1).ToString(GlobalOptions.CultureInfo)
                    , intAmount.ToString(GlobalOptions.CultureInfo));
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

                strInitTip = string.Format(LanguageManager.GetString("Tip_ImproveSubmersionGrade", GlobalOptions.Language)
                    , (CharacterObject.SubmersionGrade + 1).ToString(GlobalOptions.CultureInfo)
                    , intAmount.ToString(GlobalOptions.CultureInfo));
            }

            cmdAddMetamagic.SetToolTip(strInitTip);
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
            lblAttributes.SetToolTip(LanguageManager.GetString("Tip_CommonAttributes", GlobalOptions.Language));
            lblAttributesBase.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesBase", GlobalOptions.Language));
            lblAttributesAug.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesAug", GlobalOptions.Language));
            lblAttributesMetatype.SetToolTip(LanguageManager.GetString("Tip_CommonAttributesMetatypeLimits", GlobalOptions.Language));
            lblNuyen.SetToolTip(string.Format(LanguageManager.GetString("Tip_CommonNuyen", GlobalOptions.Language), CharacterObjectOptions.KarmaNuyenPer));
            // Armor Tab.
            chkArmorEquipped.SetToolTip(LanguageManager.GetString("Tip_ArmorEquipped", GlobalOptions.Language));
            // Weapon Tab.
            chkWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            // Gear Tab.
            chkGearActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            chkCyberwareActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            // Vehicles Tab.
            chkVehicleWeaponAccessoryInstalled.SetToolTip(LanguageManager.GetString("Tip_WeaponInstalled", GlobalOptions.Language));
            chkVehicleActiveCommlink.SetToolTip(LanguageManager.GetString("Tip_ActiveCommlink", GlobalOptions.Language));
            lblVehiclePowertrainLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            lblVehicleCosmeticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            lblVehicleElectromagneticLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            lblVehicleBodymodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            lblVehicleWeaponsmodLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            lblVehicleProtectionLabel.SetToolTip(LanguageManager.GetString("Tip_TotalVehicleModCapacity", GlobalOptions.Language));
            // Character Info Tab.
            chkCharacterCreated.SetToolTip(LanguageManager.GetString("Tip_CharacterCreated", GlobalOptions.Language));
            // Build Point Summary Tab.
            lblBuildPrimaryAttributes.SetToolTip(LanguageManager.GetString("Tip_CommonAttributes", GlobalOptions.Language));
            lblBuildPositiveQualities.SetToolTip(LanguageManager.GetString("Tip_BuildPositiveQualities", GlobalOptions.Language));
            lblBuildNegativeQualities.SetToolTip(LanguageManager.GetString("Tip_BuildNegativeQualities", GlobalOptions.Language));
            lblBuildContacts.SetToolTip(string.Format(LanguageManager.GetString("Tip_CommonContacts", GlobalOptions.Language), CharacterObjectOptions.KarmaContact.ToString(GlobalOptions.CultureInfo)));
            lblBuildEnemies.SetToolTip(LanguageManager.GetString("Tip_CommonEnemies", GlobalOptions.Language));
            lblBuildNuyen.SetToolTip(string.Format(LanguageManager.GetString("Tip_CommonNuyen", GlobalOptions.Language), CharacterObjectOptions.NuyenPerBP.ToString(CharacterObjectOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥'));
            lblBuildSkillGroups.SetToolTip(string.Format(LanguageManager.GetString("Tip_SkillsSkillGroups", GlobalOptions.Language), CharacterObjectOptions.KarmaImproveSkillGroup.ToString(GlobalOptions.CultureInfo)));
            lblBuildActiveSkills.SetToolTip(string.Format(LanguageManager.GetString("Tip_SkillsActiveSkills", GlobalOptions.Language), CharacterObjectOptions.KarmaImproveActiveSkill.ToString(GlobalOptions.CultureInfo), CharacterObjectOptions.KarmaSpecialization.ToString(GlobalOptions.CultureInfo)));
            lblBuildKnowledgeSkills.SetToolTip(string.Format(LanguageManager.GetString("Tip_SkillsKnowledgeSkills", GlobalOptions.Language), CharacterObjectOptions.FreeKnowledgeMultiplier.ToString(GlobalOptions.CultureInfo), CharacterObjectOptions.KarmaImproveKnowledgeSkill.ToString(GlobalOptions.CultureInfo)));
            lblBuildSpells.SetToolTip(string.Format(LanguageManager.GetString("Tip_SpellsSelectedSpells", GlobalOptions.Language), CharacterObjectOptions.KarmaSpell.ToString(GlobalOptions.CultureInfo)));
            lblBuildSpirits.SetToolTip(string.Format(LanguageManager.GetString("Tip_SpellsSpirits", GlobalOptions.Language), CharacterObjectOptions.KarmaSpirit.ToString(GlobalOptions.CultureInfo)));
            lblBuildSprites.SetToolTip(string.Format(LanguageManager.GetString("Tip_TechnomancerSprites", GlobalOptions.Language), CharacterObjectOptions.KarmaSpirit.ToString(GlobalOptions.CultureInfo)));
            lblBuildComplexForms.SetToolTip(string.Format(LanguageManager.GetString("Tip_TechnomancerComplexForms", GlobalOptions.Language), CharacterObjectOptions.KarmaNewComplexForm.ToString(GlobalOptions.CultureInfo)));
            // Other Info Tab.
            lblCMPhysicalLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMPhysical", GlobalOptions.Language));
            lblCMStunLabel.SetToolTip(LanguageManager.GetString("Tip_OtherCMStun", GlobalOptions.Language));
            lblINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherInitiative", GlobalOptions.Language));
            lblMatrixINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherMatrixInitiative", GlobalOptions.Language));
            lblAstralINILabel.SetToolTip(LanguageManager.GetString("Tip_OtherAstralInitiative", GlobalOptions.Language));
            lblArmorLabel.SetToolTip(LanguageManager.GetString("Tip_OtherArmor", GlobalOptions.Language));
            lblESS.SetToolTip(LanguageManager.GetString("Tip_OtherEssence", GlobalOptions.Language));
            lblRemainingNuyenLabel.SetToolTip(LanguageManager.GetString("Tip_OtherNuyen", GlobalOptions.Language));
            lblMovementLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMovement", GlobalOptions.Language));
            lblSwimLabel.SetToolTip(LanguageManager.GetString("Tip_OtherSwim", GlobalOptions.Language));
            lblFlyLabel.SetToolTip(LanguageManager.GetString("Tip_OtherFly", GlobalOptions.Language));
            lblComposureLabel.SetToolTip(LanguageManager.GetString("Tip_OtherComposure", GlobalOptions.Language));
            lblJudgeIntentionsLabel.SetToolTip(LanguageManager.GetString("Tip_OtherJudgeIntentions", GlobalOptions.Language));
            lblLiftCarryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherLiftAndCarry", GlobalOptions.Language));
            lblMemoryLabel.SetToolTip(LanguageManager.GetString("Tip_OtherMemory", GlobalOptions.Language));
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
                    blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Lifestyle;
                    blnCopyEnabled = treLifestyles.SelectedNode?.Tag is Lifestyle;
                }
                // Armor Tab.
                else if (tabStreetGearTabs.SelectedTab == tabArmor)
                {
                    if (treArmor.SelectedNode?.Tag is IHasInternalId strSelectedId)
                    {
                        blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Armor ||
                                          (GlobalOptions.ClipboardContentType == ClipboardContentType.Gear && (CharacterObject.Armor.Any(x => x.InternalId == strSelectedId.InternalId) ||
                                                                                                               CharacterObject.Armor.FindArmorMod(strSelectedId.InternalId) != null ||
                                                                                                               CharacterObject.Armor.FindArmorGear(strSelectedId.InternalId) != null));
                        blnCopyEnabled = CharacterObject.Armor.Any(x => x.InternalId == strSelectedId.InternalId) || CharacterObject.Armor.FindArmorGear(strSelectedId.InternalId) != null;
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
                                if (objXmlCategoryNode != null && treWeapons.SelectedNode?.Tag is WeaponAccessory objAccessory)
                                {
                                    // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
                                    XmlNodeList xmlGearCategoryList = objAccessory.AllowGear?.SelectNodes("gearcategory");
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

                        //TODO: ICanCopy interface? If weapon comes from something else == false, etc. 
                        blnCopyEnabled = treWeapons.SelectedNode?.Tag is Weapon || treWeapons.SelectedNode?.Tag is Gear;
                    }
                }
                // Gear Tab.
                else if (tabStreetGearTabs.SelectedTab == tabGear)
                {
                    if (treGear.SelectedNode?.Tag is IHasInternalId)
                    {
                        blnPasteEnabled = GlobalOptions.ClipboardContentType == ClipboardContentType.Gear;
                        blnCopyEnabled = treGear.SelectedNode?.Tag is Gear;
                    }
                }
            }
            // Cyberware Tab.
            else if (tabCharacterTabs.SelectedTab == tabCyberware)
            {
                if (treCyberware.SelectedNode?.Tag is IHasInternalId)
                {
                    switch (GlobalOptions.ClipboardContentType)
                    {
                        case ClipboardContentType.Gear:
                            XmlNode objXmlCategoryNode = GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                            if (objXmlCategoryNode != null && treCyberware.SelectedNode?.Tag is Cyberware objCyberware)
                            {
                                // Make sure that a Weapon Accessory is selected and that it allows Gear of the item's Category.
                                XmlNodeList xmlGearCategoryList =
                                    objCyberware.AllowGear?.SelectNodes("gearcategory");
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

                    blnCopyEnabled = treCyberware.SelectedNode?.Tag is Gear;
                }
            }
            // Vehicles Tab.
            else if (tabCharacterTabs.SelectedTab == tabVehicles)
            {
                if (treVehicles.SelectedNode?.Tag is IHasInternalId)
                {
                    switch (GlobalOptions.ClipboardContentType)
                    {
                        case ClipboardContentType.Vehicle:
                            blnPasteEnabled = true;
                            break;
                        case ClipboardContentType.Gear:
                        {
                            blnPasteEnabled = treVehicles.SelectedNode?.Tag is Vehicle ||
                                              treVehicles.SelectedNode?.Tag is Gear;
                            if (!blnPasteEnabled)
                            {
                                XmlNodeList gearList = null;
                                switch (treVehicles.SelectedNode?.Tag)
                                {
                                    case Cyberware objCyberware:
                                        gearList = objCyberware.AllowGear?.SelectNodes("gearcategory");
                                        break;
                                    case WeaponAccessory objAccessory:
                                        gearList = objAccessory.AllowGear?.SelectNodes("gearcategory");
                                        break;
                                }

                                if (gearList?.Count > 0)
                                {
                                    XmlNode objXmlCategoryNode =
                                        GlobalOptions.Clipboard.SelectSingleNode("/character/gear/category");
                                    foreach (XmlNode objAllowed in gearList)
                                    {
                                        if (objAllowed.InnerText == objXmlCategoryNode?.InnerText)
                                        {
                                            blnPasteEnabled = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        case ClipboardContentType.Weapon:
                            blnPasteEnabled = ((treVehicles.SelectedNode?.Tag is WeaponMount) ||
                                                treVehicles.SelectedNode?.Tag is VehicleMod objVehicleMod &&
                                               (objVehicleMod.Name.StartsWith("Mechanical Arm") || objVehicleMod.Name.Contains("Drone Arm")));
                            break;
                    }

                    // In theory any object that's not a generic string node is valid to copy here. Locations might go screwy?
                    blnCopyEnabled = true;
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
            XmlNode xmlSuite = frmPickCyberwareSuite.SelectedSuite.IsGuid()
                ? objXmlDocument.SelectSingleNode("/chummer/suites/suite[id = \"" + frmPickCyberwareSuite.SelectedSuite + "\"]")
                : objXmlDocument.SelectSingleNode("/chummer/suites/suite[name = \"" + frmPickCyberwareSuite.SelectedSuite + "\"]");
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
            objCyberware.Create(objXmlCyberwareNode, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, blnCreateChildren);

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
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

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
            objNewMetamagic.Grade = objGrade.Grade;
            if (objNewMetamagic.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Metamagics.Add(objNewMetamagic);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddArt_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Art);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("metamagic.xml").SelectSingleNode("/chummer/arts/art[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Metamagic;

            Art objArt = new Art(CharacterObject);

            objArt.Create(objXmlArt, objSource);
            objArt.Grade = objGrade.Grade;
            if (objArt.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Arts.Add(objArt);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddEnchantment_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Enchantment);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            Spell objNewSpell = new Spell(CharacterObject);

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = objGrade.Grade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsMetamagicAddRitual_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

            frmSelectArt frmPickArt = new frmSelectArt(CharacterObject, frmSelectArt.Mode.Ritual);
            frmPickArt.ShowDialog(this);

            // Make sure a value was selected.
            if (frmPickArt.DialogResult == DialogResult.Cancel)
                return;

            XmlNode objXmlArt = XmlManager.Load("spells.xml").SelectSingleNode("/chummer/spells/spell[id = \"" + frmPickArt.SelectedItem + "\"]");
            Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Initiation;

            Spell objNewSpell = new Spell(CharacterObject);

            objNewSpell.Create(objXmlArt, string.Empty, false, false, false, objSource);
            objNewSpell.Grade = objGrade.Grade;
            if (objNewSpell.InternalId.IsEmptyGuid())
                return;

            CharacterObject.Spells.Add(objNewSpell);

            IsCharacterUpdateRequested = true;

            IsDirty = true;
        }

        private void tsInitiationNotes_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is IHasNotes objNotes))
                return;
                WriteNotes(objNotes,treMetamagic.SelectedNode);
        }

        private void tsMetamagicAddEnhancement_Click(object sender, EventArgs e)
        {
            if (!(treMetamagic.SelectedNode?.Tag is InitiationGrade objGrade))
                return;

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
            if (!(treMartialArts.SelectedNode?.Tag is IHasNotes objNotes))
                return;
            WriteNotes(objNotes, treMartialArts.SelectedNode);
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
            if (IsRefreshing)
                return;

            IsCharacterUpdateRequested = true;

            IsDirty = true;
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
            if (IsLoading || IsRefreshing || CharacterObject.Ambidextrous)
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
            picMugshot.SizeMode = picMugshot.Image != null && picMugshot.Height >= picMugshot.Image.Height && picMugshot.Width >= picMugshot.Image.Width
                ? PictureBoxSizeMode.CenterImage
                : PictureBoxSizeMode.Zoom;
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

            CharacterObject.Vehicles.FindVehicleCyberware(x => x.InternalId == objModularCyberware.InternalId,
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
            if (!(treWeapons.SelectedNode?.Tag is Location objLocation)) return;
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddWeapon(objLocation);
            }
            while (blnAddAgain);
        }

        private void tsVehicleLocationAddVehicle_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                blnAddAgain = AddVehicle(treVehicles.SelectedNode?.Tag is Location objSelectedNode ? objSelectedNode : null);
            }
            while (blnAddAgain);
        }

        private void tsEditWeaponMount_Click(object sender, EventArgs e)
        {
            if (!(treVehicles.SelectedNode?.Tag is WeaponMount objWeaponMount))
                return;
            frmCreateWeaponMount frmCreateWeaponMount = new frmCreateWeaponMount(objWeaponMount.Parent, CharacterObject, objWeaponMount);
            frmCreateWeaponMount.ShowDialog(this);

            if (frmCreateWeaponMount.DialogResult != DialogResult.Cancel)
            {
                if (frmCreateWeaponMount.FreeCost)
                    frmCreateWeaponMount.WeaponMount.Cost = "0";

                IsCharacterUpdateRequested = true;
                
                IsDirty = true;
            }
        }
        private void btnCreateCustomDrug_Click_1(object sender, EventArgs e)
        {
            frmCreateCustomDrug form = new frmCreateCustomDrug(CharacterObject);
            form.ShowDialog(this);

            if (form.DialogResult == DialogResult.Cancel)
                return;

            Drug objCustomDrug = form.CustomDrug;
            CharacterObject.Drugs.Add(objCustomDrug);
        }
		private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        private void pnlAttributes_Layout(object sender, LayoutEventArgs e)
        {
            pnlAttributes.SuspendLayout();
            foreach (Control objAttributeControl in pnlAttributes.Controls)
            {
                objAttributeControl.Width = pnlAttributes.ClientSize.Width;
            }
            pnlAttributes.ResumeLayout();
        }
    }
}



