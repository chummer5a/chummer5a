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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class CreatePACKSKit : Form
    {
        [NotNull]
        private readonly Character _objCharacter;

        #region Control Events

        public CreatePACKSKit(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            // Make sure the kit and file name fields are populated.
            string strName = await txtName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strName))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_KitName").ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_KitName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strFileName = await txtFileName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strFileName))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_CreatePACKSKit_FileName").ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_FileName").ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Make sure the file name starts with custom and ends with _packs.xml.
            if (!strFileName.StartsWith("custom_", StringComparison.OrdinalIgnoreCase))
            {
                strFileName = "custom_" + strFileName;
            }
            if (!strFileName.EndsWith("_packs.xml", StringComparison.OrdinalIgnoreCase))
            {
                strFileName += "_packs.xml";
            }

            // See if a Kit with this name already exists for the Custom category.
            // This was originally done without the XmlManager, but because amends and overrides and toggling custom data directories can change names, we need to use it.
            if ((await _objCharacter.LoadDataXPathAsync("packs.xml").ConfigureAwait(false))
                .TryGetNodeByNameOrId("/chummer/packs/pack", strName, "category = \"Custom\"]")
                != null)
            {
                Program.ShowScrollableMessageBox(
                    this,
                    string.Format(GlobalSettings.CultureInfo,
                                  await LanguageManager.GetStringAsync("Message_CreatePACKSKit_DuplicateName")
                                                       .ConfigureAwait(false), strName),
                    await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_DuplicateName")
                                         .ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!Directory.Exists(Utils.GetPacksFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(Utils.GetPacksFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    Program.ShowScrollableMessageBox(await LanguageManager
                                                           .GetStringAsync("Message_Insufficient_Permissions_Warning")
                                                           .ConfigureAwait(false));
                    return;
                }
            }

            string strPath = Path.Combine(Utils.GetPacksFolderPath, strFileName);

            // If this is not a new file, read in the existing contents.
            XmlDocument objXmlCurrentDocument = null;
            if (File.Exists(strPath))
            {
                try
                {
                    objXmlCurrentDocument = new XmlDocument { XmlResolver = null };
                    await objXmlCurrentDocument.LoadStandardAsync(strPath).ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    Program.ShowScrollableMessageBox(this, ex.ToString());
                    return;
                }
                catch (XmlException ex)
                {
                    Program.ShowScrollableMessageBox(this, ex.ToString());
                    return;
                }
            }

            using (FileStream objStream = new FileStream(strPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                {
                    await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                    // <chummer>
                    await objWriter.WriteStartElementAsync("chummer").ConfigureAwait(false);
                    // <packs>
                    await objWriter.WriteStartElementAsync("packs").ConfigureAwait(false);

                    // If this is not a new file, write out the current contents.
                    if (objXmlCurrentDocument != null)
                    {
                        XmlNode xmlExistingPacksNode = objXmlCurrentDocument.SelectSingleNode("/chummer/packs");
                        xmlExistingPacksNode?.WriteContentTo(objWriter);
                    }

                    // <pack>
                    await objWriter.WriteStartElementAsync("pack").ConfigureAwait(false);
                    // <name />
                    await objWriter.WriteElementStringAsync("name", txtName.Text).ConfigureAwait(false);
                    // <category />
                    await objWriter.WriteElementStringAsync("category", "Custom").ConfigureAwait(false);

                    // Export Attributes.
                    if (await chkAttributes.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        int intBOD = await _objCharacter.BOD.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.BOD.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intAGI = await _objCharacter.AGI.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.AGI.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intREA = await _objCharacter.REA.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.REA.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intSTR = await _objCharacter.STR.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.STR.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intCHA = await _objCharacter.CHA.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.CHA.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intINT = await _objCharacter.INT.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.INT.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intLOG = await _objCharacter.LOG.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.LOG.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intWIL = await _objCharacter.WIL.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.WIL.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intEDG = await _objCharacter.EDG.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.EDG.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intMAG = await _objCharacter.MAG.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.MAG.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intMAGAdept = await _objCharacter.MAGAdept.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.MAGAdept.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intDEP = await _objCharacter.DEP.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.DEP.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        int intRES = await _objCharacter.RES.GetValueAsync().ConfigureAwait(false) - (await _objCharacter.RES.GetMetatypeMinimumAsync().ConfigureAwait(false) - 1);
                        // <attributes>
                        await objWriter.WriteStartElementAsync("attributes").ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("bod", intBOD.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("agi", intAGI.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("rea", intREA.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("str", intSTR.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("cha", intCHA.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("int", intINT.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("log", intLOG.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("wil", intWIL.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("edg", intEDG.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        if (_objCharacter.MAGEnabled)
                        {
                            await objWriter.WriteElementStringAsync("mag", intMAG.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                            if (_objCharacter.Settings.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                                await objWriter.WriteElementStringAsync("magadept", intMAGAdept.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        }

                        if (_objCharacter.RESEnabled)
                            await objWriter.WriteElementStringAsync("res", intRES.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        if (_objCharacter.DEPEnabled)
                            await objWriter.WriteElementStringAsync("dep", intDEP.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                        // </attributes>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Qualities.
                    if (await chkQualities.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        bool blnPositive = false;
                        bool blnNegative = false;
                        // Determine if Positive or Negative Qualities exist.
                        await _objCharacter.Qualities.ForEachWithBreakAsync(objQuality =>
                        {
                            switch (objQuality.Type)
                            {
                                case QualityType.Positive:
                                    blnPositive = true;
                                    break;

                                case QualityType.Negative:
                                    blnNegative = true;
                                    break;
                            }

                            return !blnPositive || !blnNegative;
                        }).ConfigureAwait(false);

                        // <qualities>
                        await objWriter.WriteStartElementAsync("qualities").ConfigureAwait(false);

                        // Positive Qualities.
                        if (blnPositive)
                        {
                            // <positive>
                            await objWriter.WriteStartElementAsync("positive").ConfigureAwait(false);
                            await _objCharacter.Qualities.ForEachAsync(async objQuality =>
                            {
                                if (objQuality.Type == QualityType.Positive)
                                {
                                    // ReSharper disable AccessToDisposedClosure
                                    await objWriter.WriteStartElementAsync("quality").ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        await objWriter.WriteAttributeStringAsync("select", objQuality.Extra)
                                                       .ConfigureAwait(false);
                                    objWriter.WriteValue(objQuality.Name);
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    // ReSharper restore AccessToDisposedClosure
                                }
                            }).ConfigureAwait(false);

                            // </positive>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }

                        // Negative Qualities.
                        if (blnPositive)
                        {
                            // <negative>
                            await objWriter.WriteStartElementAsync("negative").ConfigureAwait(false);
                            await _objCharacter.Qualities.ForEachAsync(async objQuality =>
                            {
                                if (objQuality.Type == QualityType.Negative)
                                {
                                    // ReSharper disable AccessToDisposedClosure
                                    await objWriter.WriteStartElementAsync("quality").ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(objQuality.Extra))
                                        await objWriter.WriteAttributeStringAsync("select", objQuality.Extra)
                                                       .ConfigureAwait(false);
                                    objWriter.WriteValue(objQuality.Name);
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    // ReSharper restore AccessToDisposedClosure
                                }
                            }).ConfigureAwait(false);

                            // </negative>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }

                        // </qualities>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Starting Nuyen.
                    if (await chkStartingNuyen.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        decimal decNuyenBP = _objCharacter.NuyenBP;
                        if (!_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                            decNuyenBP /= 2.0m;
                        await objWriter.WriteElementStringAsync("nuyenbp", decNuyenBP.ToString(GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                    }

                    /* TODO: Add support for active and knowledge skills and skill groups
                    // Export Active Skills.
                    if (await chkActiveSkills.DoThreadSafeFuncAsync(x => x.Checked))
                    {
                        // <skills>
                        await objWriter.WriteStartElementAsync("skills");

                        // Active Skills.
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (!objSkill.IsKnowledgeSkill && objSkill.Rating > 0)
                            {
                                // <skill>
                                await objWriter.WriteStartElementAsync("skill");
                                await objWriter.WriteElementStringAsync("name", objSkill.Name);
                                await objWriter.WriteElementStringAsync("rating", objSkill.Rating.ToString());
                                if (!string.IsNullOrEmpty(objSkill.Specialization))
                                    await objWriter.WriteElementStringAsync("spec", objSkill.Specialization);
                                // </skill>
                                await objWriter.WriteEndElementAsync();
                            }
                        }

                        // Skill Groups.
                        foreach (SkillGroup objSkillGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objSkillGroup.BaseUnbroken && objSkillGroup.Rating > 0)
                            {
                                // <skillgroup>
                                await objWriter.WriteStartElementAsync("skillgroup");
                                await objWriter.WriteElementStringAsync("name", objSkillGroup.Name);
                                await objWriter.WriteElementStringAsync("rating", objSkillGroup.Rating.ToString());
                                // </skillgroup>
                                await objWriter.WriteEndElementAsync();
                            }
                        }
                        // </skills>
                        await objWriter.WriteEndElementAsync();
                    }

                    // Export Knowledge Skills.
                    if (await chkKnowledgeSkills.DoThreadSafeFuncAsync(x => x.Checked))
                    {
                        // <knowledgeskills>
                        await objWriter.WriteStartElementAsync("knowledgeskills");
                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.Skills.OfType<KnowledgeSkill>())
                        {
                            // <skill>
                            await objWriter.WriteStartElementAsync("skill");
                            await objWriter.WriteElementStringAsync("name", objSkill.Name);
                            await objWriter.WriteElementStringAsync("rating", objSkill.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!string.IsNullOrEmpty(objSkill.Specialization))
                                await objWriter.WriteElementStringAsync("spec", objSkill.Specialization);
                            await objWriter.WriteElementStringAsync("category", objSkill.SkillCategory);
                            // </skill>
                            await objWriter.WriteEndElementAsync();
                        }

                        // </knowledgeskills>
                        await objWriter.WriteEndElementAsync();
                    }
                    */

                    // Export Martial Arts.
                    if (await chkMartialArts.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <martialarts>
                        await objWriter.WriteStartElementAsync("martialarts").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.MartialArts.ForEachAsync(async objArt =>
                        {
                            // <martialart>
                            await objWriter.WriteStartElementAsync("martialart").ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name", objArt.Name).ConfigureAwait(false);
                            if (await objArt.Techniques.GetCountAsync().ConfigureAwait(false) > 0)
                            {
                                // <techniques>
                                await objWriter.WriteStartElementAsync("techniques").ConfigureAwait(false);
                                await objArt.Techniques
                                            .ForEachAsync(objTechnique =>
                                                              objWriter.WriteElementStringAsync(
                                                                  "technique", objTechnique.Name))
                                            .ConfigureAwait(false);
                                // </techniques>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }

                            // </martialart>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure
                        // </martialarts>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Spells.
                    if (await chkSpells.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <spells>
                        await objWriter.WriteStartElementAsync("spells").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.Spells.ForEachAsync(async objSpell =>
                        {
                            await objWriter.WriteStartElementAsync("spell").ConfigureAwait(false);
                            await objWriter.WriteStartElementAsync("name").ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(objSpell.Extra))
                                await objWriter.WriteAttributeStringAsync("select", objSpell.Extra)
                                               .ConfigureAwait(false);
                            objWriter.WriteValue(objSpell.Name);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("category", objSpell.Category)
                                           .ConfigureAwait(false);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </spells>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Complex Forms.
                    if (await chkComplexForms.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <programs>
                        await objWriter.WriteStartElementAsync("complexforms").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.ComplexForms.ForEachAsync(async objComplexForm =>
                        {
                            // <program>
                            await objWriter.WriteStartElementAsync("complexform").ConfigureAwait(false);
                            await objWriter.WriteStartElementAsync("name").ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(objComplexForm.Extra))
                                await objWriter.WriteAttributeStringAsync("select", objComplexForm.Extra)
                                               .ConfigureAwait(false);
                            objWriter.WriteValue(objComplexForm.Name);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            // </program>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </programs>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Cyberware/Bioware.
                    if (await chkCyberware.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        bool blnCyberware = false;
                        bool blnBioware = false;
                        await _objCharacter.Cyberware.ForEachWithBreakAsync(objCharacterCyberware =>
                        {
                            switch (objCharacterCyberware.SourceType)
                            {
                                case Improvement.ImprovementSource.Bioware:
                                    blnBioware = true;
                                    break;

                                case Improvement.ImprovementSource.Cyberware:
                                    blnCyberware = true;
                                    break;
                            }

                            return !blnCyberware || !blnBioware;
                        }).ConfigureAwait(false);

                        if (blnCyberware)
                        {
                            // <cyberwares>
                            await objWriter.WriteStartElementAsync("cyberwares").ConfigureAwait(false);
                            // ReSharper disable AccessToDisposedClosure
                            await _objCharacter.Cyberware.ForEachAsync(async objCyberware =>
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                                {
                                    // <cyberware>
                                    await objWriter.WriteStartElementAsync("cyberware").ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("name", objCyberware.Name)
                                                   .ConfigureAwait(false);
                                    int intRating = await objCyberware.GetRatingAsync()
                                                                      .ConfigureAwait(false);
                                    if (intRating > 0)
                                        await objWriter
                                              .WriteElementStringAsync(
                                                  "rating",
                                                  intRating.ToString(GlobalSettings.InvariantCultureInfo))
                                              .ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("grade", objCyberware.Grade.Name)
                                                   .ConfigureAwait(false);
                                    if (await objCyberware.Children.GetCountAsync().ConfigureAwait(false) > 0)
                                    {
                                        // <cyberwares>
                                        await objWriter.WriteStartElementAsync("cyberwares").ConfigureAwait(false);
                                        await objCyberware.Children.ForEachAsync(async objChildCyberware =>
                                        {
                                            if (objChildCyberware.Capacity != "[*]")
                                            {
                                                // <cyberware/bioware>
                                                await objWriter
                                                      .WriteStartElementAsync(
                                                          objChildCyberware.SourceType
                                                          == Improvement.ImprovementSource.Cyberware
                                                              ? "cyberware"
                                                              : "bioware")
                                                      .ConfigureAwait(false);
                                                await objWriter.WriteElementStringAsync("name", objChildCyberware.Name)
                                                               .ConfigureAwait(false);
                                                int intChildRating = await objChildCyberware.GetRatingAsync()
                                                    .ConfigureAwait(false);
                                                if (intChildRating > 0)
                                                    await objWriter
                                                          .WriteElementStringAsync(
                                                              "rating",
                                                              intChildRating.ToString(
                                                                  GlobalSettings.InvariantCultureInfo))
                                                          .ConfigureAwait(false);

                                                if (await objChildCyberware.GearChildren.GetCountAsync()
                                                                           .ConfigureAwait(false) > 0)
                                                    await WriteGear(objWriter, objChildCyberware.GearChildren)
                                                        .ConfigureAwait(false);
                                                // </cyberware>
                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                            }
                                        }).ConfigureAwait(false);

                                        // </cyberwares>
                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    if (await objCyberware.GearChildren.GetCountAsync().ConfigureAwait(false) > 0)
                                        await WriteGear(objWriter, objCyberware.GearChildren).ConfigureAwait(false);

                                    // </cyberware>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }
                            }).ConfigureAwait(false);
                            // ReSharper restore AccessToDisposedClosure

                            // </cyberwares>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }

                        if (blnBioware)
                        {
                            // <biowares>
                            await objWriter.WriteStartElementAsync("biowares").ConfigureAwait(false);
                            // ReSharper disable AccessToDisposedClosure
                            await _objCharacter.Cyberware.ForEachAsync(async objCyberware =>
                            {
                                if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                                {
                                    // <bioware>
                                    await objWriter.WriteStartElementAsync("bioware").ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("name", objCyberware.Name)
                                                   .ConfigureAwait(false);
                                    int intRating = await objCyberware.GetRatingAsync()
                                                                      .ConfigureAwait(false);
                                    if (intRating > 0)
                                        await objWriter
                                              .WriteElementStringAsync(
                                                  "rating",
                                                  intRating.ToString(GlobalSettings.InvariantCultureInfo))
                                              .ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("grade", objCyberware.Grade.Name)
                                                   .ConfigureAwait(false);
                                    if (await objCyberware.Children.GetCountAsync().ConfigureAwait(false) > 0)
                                    {
                                        // <cyberwares>
                                        await objWriter.WriteStartElementAsync("cyberwares").ConfigureAwait(false);
                                        await objCyberware.Children.ForEachAsync(async objChildCyberware =>
                                        {
                                            if (objChildCyberware.Capacity != "[*]")
                                            {
                                                // <cyberware>
                                                // <cyberware/bioware>
                                                await objWriter
                                                      .WriteStartElementAsync(
                                                          objChildCyberware.SourceType
                                                          == Improvement.ImprovementSource.Cyberware
                                                              ? "cyberware"
                                                              : "bioware")
                                                      .ConfigureAwait(false);
                                                await objWriter.WriteElementStringAsync("name", objChildCyberware.Name)
                                                               .ConfigureAwait(false);
                                                int intChildRating = await objChildCyberware.GetRatingAsync()
                                                    .ConfigureAwait(false);
                                                if (intChildRating > 0)
                                                    await objWriter
                                                          .WriteElementStringAsync(
                                                              "rating",
                                                              intChildRating.ToString(
                                                                  GlobalSettings.InvariantCultureInfo))
                                                          .ConfigureAwait(false);

                                                if (await objChildCyberware.GearChildren.GetCountAsync()
                                                                           .ConfigureAwait(false) > 0)
                                                    await WriteGear(objWriter, objChildCyberware.GearChildren)
                                                        .ConfigureAwait(false);
                                                // </cyberware>
                                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                            }
                                        }).ConfigureAwait(false);

                                        // </cyberwares>
                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }

                                    if (await objCyberware.GearChildren.GetCountAsync().ConfigureAwait(false) > 0)
                                        await WriteGear(objWriter, objCyberware.GearChildren).ConfigureAwait(false);

                                    // </bioware>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }
                            }).ConfigureAwait(false);
                            // ReSharper restore AccessToDisposedClosure

                            // </biowares>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }
                    }

                    // Export Lifestyle.
                    if (await chkLifestyle.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <lifestyles>
                        await objWriter.WriteStartElementAsync("lifestyles").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.Lifestyles.ForEachAsync(async objLifestyle =>
                        {
                            // <lifestyle>
                            await objWriter.WriteStartElementAsync("lifestyle").ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name", objLifestyle.Name).ConfigureAwait(false);
                            await objWriter
                                  .WriteElementStringAsync(
                                      "months", objLifestyle.Increments.ToString(GlobalSettings.InvariantCultureInfo))
                                  .ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(objLifestyle.BaseLifestyle))
                            {
                                // This is an Advanced Lifestyle, so write out its properties.
                                await objWriter
                                      .WriteElementStringAsync(
                                          "cost",
                                          objLifestyle.Cost.ToString(_objCharacter.Settings.NuyenFormat,
                                                                     GlobalSettings.CultureInfo)).ConfigureAwait(false);
                                await objWriter
                                      .WriteElementStringAsync(
                                          "dice", objLifestyle.Dice.ToString(GlobalSettings.InvariantCultureInfo))
                                      .ConfigureAwait(false);
                                await objWriter
                                      .WriteElementStringAsync("multiplier",
                                                               objLifestyle.Multiplier.ToString(
                                                                   _objCharacter.Settings.NuyenFormat,
                                                                   GlobalSettings.CultureInfo)).ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync("baselifestyle", objLifestyle.BaseLifestyle)
                                               .ConfigureAwait(false);
                                if (objLifestyle.LifestyleQualities.Count > 0)
                                {
                                    // <qualities>
                                    await objWriter.WriteStartElementAsync("qualities").ConfigureAwait(false);
                                    foreach (LifestyleQuality objQuality in objLifestyle.LifestyleQualities)
                                        await objWriter.WriteElementStringAsync("quality", objQuality.Name)
                                                       .ConfigureAwait(false);
                                    // </qualities>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }
                            }

                            // </lifestyle>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </lifestyles>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Armor.
                    if (await chkArmor.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <armors>
                        await objWriter.WriteStartElementAsync("armors").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.Armor.ForEachAsync(async objArmor =>
                        {
                            // <armor>
                            await objWriter.WriteStartElementAsync("armor").ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name", objArmor.Name).ConfigureAwait(false);
                            if (await objArmor.ArmorMods.GetCountAsync().ConfigureAwait(false) > 0)
                            {
                                // <mods>
                                await objWriter.WriteStartElementAsync("mods").ConfigureAwait(false);
                                await objArmor.ArmorMods.ForEachAsync(async objMod =>
                                {
                                    // <mod>
                                    await objWriter.WriteStartElementAsync("mod").ConfigureAwait(false);
                                    await objWriter.WriteElementStringAsync("name", objMod.Name).ConfigureAwait(false);
                                    if (objMod.Rating > 0)
                                        await objWriter
                                              .WriteElementStringAsync(
                                                  "rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                              .ConfigureAwait(false);
                                    // </mod>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }).ConfigureAwait(false);

                                // </mods>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }

                            if (objArmor.GearChildren.Count > 0)
                                await WriteGear(objWriter, objArmor.GearChildren).ConfigureAwait(false);

                            // </armor>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </armors>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Weapons.
                    if (await chkWeapons.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <weapons>
                        await objWriter.WriteStartElementAsync("weapons").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.Weapons.ForEachAsync(async objWeapon =>
                        {
                            // Don't attempt to export Cyberware and Gear Weapons since those are handled by those object types. The default Unarmed Attack Weapon should also not be exported.
                            if (objWeapon.Category != "Cyberware" && objWeapon.Category != "Gear"
                                                                  && objWeapon.Name != "Unarmed Attack")
                            {
                                // <weapon>
                                await objWriter.WriteStartElementAsync("weapon").ConfigureAwait(false);
                                await objWriter.WriteElementStringAsync("name", objWeapon.Name).ConfigureAwait(false);

                                // Weapon Accessories.
                                if (objWeapon.WeaponAccessories.Count > 0)
                                {
                                    // <accessories>
                                    await objWriter.WriteStartElementAsync("accessories").ConfigureAwait(false);
                                    await objWeapon.WeaponAccessories.ForEachAsync(async objAccessory =>
                                    {
                                        // Don't attempt to export items included in the Weapon.
                                        if (!objAccessory.IncludedInWeapon)
                                        {
                                            // <accessory>
                                            await objWriter.WriteStartElementAsync("accessory").ConfigureAwait(false);
                                            await objWriter.WriteElementStringAsync("name", objAccessory.Name)
                                                           .ConfigureAwait(false);
                                            await objWriter.WriteElementStringAsync("mount", objAccessory.Mount)
                                                           .ConfigureAwait(false);
                                            await objWriter
                                                  .WriteElementStringAsync("extramount", objAccessory.ExtraMount)
                                                  .ConfigureAwait(false);

                                            if (await objAccessory.GearChildren.GetCountAsync().ConfigureAwait(false) > 0)
                                                await WriteGear(objWriter, objAccessory.GearChildren)
                                                    .ConfigureAwait(false);

                                            // </accessory>
                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                        }
                                    }).ConfigureAwait(false);

                                    // </accessories>
                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                }

                                // Underbarrel Weapon.
                                if (await objWeapon.UnderbarrelWeapons.GetCountAsync().ConfigureAwait(false) > 0)
                                {
                                    await objWeapon.UnderbarrelWeapons.ForEachAsync(async objUnderbarrelWeapon =>
                                    {
                                        if (!objUnderbarrelWeapon.IncludedInWeapon)
                                            await objWriter
                                                  .WriteElementStringAsync("underbarrel", objUnderbarrelWeapon.Name)
                                                  .ConfigureAwait(false);
                                    }).ConfigureAwait(false);
                                }

                                // </weapon>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </weapons>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // Export Gear.
                    if (await chkGear.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        await WriteGear(objWriter, _objCharacter.Gear).ConfigureAwait(false);
                    }

                    // Export Vehicles.
                    if (await chkVehicles.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                    {
                        // <vehicles>
                        await objWriter.WriteStartElementAsync("vehicles").ConfigureAwait(false);
                        // ReSharper disable AccessToDisposedClosure
                        await _objCharacter.Vehicles.ForEachAsync(async objVehicle =>
                        {
                            bool blnWeapons = false;
                            // <vehicle>
                            await objWriter.WriteStartElementAsync("vehicle").ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("name", objVehicle.Name).ConfigureAwait(false);
                            if (objVehicle.Mods.Count > 0)
                            {
                                // <mods>
                                await objWriter.WriteStartElementAsync("mods").ConfigureAwait(false);
                                await objVehicle.Mods.ForEachAsync(async objVehicleMod =>
                                {
                                    // Only write out the Mods that are not part of the base vehicle.
                                    if (!objVehicleMod.IncludedInVehicle)
                                    {
                                        // <mod>
                                        await objWriter.WriteStartElementAsync("mod").ConfigureAwait(false);
                                        await objWriter.WriteElementStringAsync("name", objVehicleMod.Name)
                                                       .ConfigureAwait(false);
                                        if (objVehicleMod.Rating > 0)
                                            await objWriter
                                                  .WriteElementStringAsync(
                                                      "rating",
                                                      objVehicleMod.Rating.ToString(
                                                          GlobalSettings.InvariantCultureInfo)).ConfigureAwait(false);
                                        // </mod>
                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                                        // See if this is a Weapon Mount with Weapons.
                                        if (objVehicleMod.Weapons.Count > 0)
                                            blnWeapons = true;
                                    }
                                    else
                                    {
                                        // See if this is a Weapon Mount with Weapons.
                                        if (objVehicleMod.Weapons.Count > 0)
                                            blnWeapons = true;
                                    }
                                }).ConfigureAwait(false);

                                // </mods>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }

                            // If there are Weapons, add them.
                            if (blnWeapons)
                            {
                                // <weapons>
                                await objWriter.WriteStartElementAsync("weapons").ConfigureAwait(false);
                                await objVehicle.Mods.ForEachAsync(async objVehicleMod =>
                                {
                                    await objVehicleMod.Weapons.ForEachAsync(async objWeapon =>
                                    {
                                        // <weapon>
                                        await objWriter.WriteStartElementAsync("weapon").ConfigureAwait(false);
                                        await objWriter.WriteElementStringAsync("name", objWeapon.Name)
                                                       .ConfigureAwait(false);

                                        // Weapon Accessories.
                                        if (await objWeapon.WeaponAccessories.GetCountAsync().ConfigureAwait(false) > 0)
                                        {
                                            // <accessories>
                                            await objWriter.WriteStartElementAsync("accessories").ConfigureAwait(false);
                                            await objWeapon.WeaponAccessories.ForEachAsync(async objAccessory =>
                                            {
                                                // Don't attempt to export items included in the Weapon.
                                                if (!objAccessory.IncludedInWeapon)
                                                {
                                                    // <accessory>
                                                    await objWriter.WriteStartElementAsync("accessory")
                                                                   .ConfigureAwait(false);
                                                    await objWriter.WriteElementStringAsync("name", objAccessory.Name)
                                                                   .ConfigureAwait(false);
                                                    await objWriter.WriteElementStringAsync("mount", objAccessory.Mount)
                                                                   .ConfigureAwait(false);
                                                    await objWriter
                                                          .WriteElementStringAsync(
                                                              "extramount", objAccessory.ExtraMount)
                                                          .ConfigureAwait(false);
                                                    // </accessory>
                                                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                                }
                                            }).ConfigureAwait(false);

                                            // </accessories>
                                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                        }

                                        // Underbarrel Weapon.
                                        if (await objWeapon.UnderbarrelWeapons.GetCountAsync().ConfigureAwait(false) > 0)
                                        {
                                            await objWeapon.UnderbarrelWeapons.ForEachAsync(objUnderbarrelWeapon =>
                                                objWriter
                                                    .WriteElementStringAsync(
                                                        "underbarrel", objUnderbarrelWeapon.Name)).ConfigureAwait(false);
                                        }

                                        // </weapon>
                                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                                    }).ConfigureAwait(false);
                                }).ConfigureAwait(false);

                                // </weapons>
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            }

                            // Gear.
                            if (await objVehicle.GearChildren.GetCountAsync().ConfigureAwait(false) > 0)
                            {
                                await WriteGear(objWriter, objVehicle.GearChildren).ConfigureAwait(false);
                            }

                            // </vehicle>
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }).ConfigureAwait(false);
                        // ReSharper restore AccessToDisposedClosure

                        // </vehicles>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    }

                    // </pack>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    // </packs>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                    // </chummer>
                    await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                    await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                }
            }

            Program.ShowScrollableMessageBox(
                this,
                string.Format(GlobalSettings.CultureInfo,
                              await LanguageManager.GetStringAsync("Message_CreatePACKSKit_SuiteCreated")
                                                   .ConfigureAwait(false), strName),
                await LanguageManager.GetStringAsync("MessageTitle_CreatePACKSKit_SuiteCreated").ConfigureAwait(false),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Recursively write out all Gear information since these can be nested pretty deep.
        /// </summary>
        /// <param name="objWriter">XmlWriter to use.</param>
        /// <param name="lstGear">List of Gear to write.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static async Task WriteGear(XmlWriter objWriter, IEnumerable<Gear> lstGear, CancellationToken token = default)
        {
            // <gears>
            await objWriter.WriteStartElementAsync("gears", token: token).ConfigureAwait(false);
            foreach (Gear objGear in lstGear)
            {
                if (objGear.IncludedInParent)
                    continue;
                // <gear>
                await objWriter.WriteStartElementAsync("gear", token: token).ConfigureAwait(false);
                await objWriter.WriteStartElementAsync("name", token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(objGear.Extra))
                    await objWriter.WriteAttributeStringAsync("select", objGear.Extra, token: token).ConfigureAwait(false);
                objWriter.WriteValue(objGear.Name);
                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", objGear.Category, token: token).ConfigureAwait(false);
                if (objGear.Rating > 0)
                    await objWriter.WriteElementStringAsync("rating", objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                if (objGear.Quantity != 1)
                    await objWriter.WriteElementStringAsync("qty", objGear.Quantity.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                if (objGear.Children.Count > 0)
                    await WriteGear(objWriter, objGear.Children, token).ConfigureAwait(false);
                // </gear>
                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            }
            // </gears>
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }

        #endregion Methods
    }
}
