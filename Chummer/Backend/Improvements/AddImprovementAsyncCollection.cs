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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using NLog;

namespace Chummer
{
    public class AddImprovementAsyncCollection
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private readonly Character _objCharacter;

        public AddImprovementAsyncCollection(Character character, Improvement.ImprovementSource objImprovementSource, string sourceName, string strUnique, string forcedValue, string limitSelection, string selectedValue, string strFriendlyName, int intRating)
        {
            _objCharacter = character;
            _objImprovementSource = objImprovementSource;
            SourceName = sourceName;
            _strUnique = strUnique;
            ForcedValue = forcedValue;
            LimitSelection = limitSelection;
            SelectedValue = selectedValue;
            _strFriendlyName = strFriendlyName;
            _intRating = intRating;
        }

        public string SourceName { get; set; }
        public string ForcedValue { get; set; }
        public string LimitSelection { get; set; }
        public string SelectedValue { get; set; }
        public string SelectedTarget { get; set; } = string.Empty;

        private readonly Improvement.ImprovementSource _objImprovementSource;
        private readonly string _strUnique;
        private readonly string _strFriendlyName;
        private readonly int _intRating;

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="strImprovedName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single highest value in the pool will be applied to the character.</param>
        /// <param name="decValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="decAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private Task<Improvement> CreateImprovementAsync(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
            string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
            decimal decValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, decimal decAugmented = 0,
            int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false, string strTarget = "", string strCondition = "", CancellationToken token = default)
        {
            return ImprovementManager.CreateImprovementAsync(_objCharacter, strImprovedName, objImprovementSource,
                strSourceName, objImprovementType, strUnique,
                decValue, intRating, intMinimum, intMaximum, decAugmented,
                intAugmentedMaximum, strExclude, blnAddToRating, strTarget, strCondition, token);
        }

        /// <summary>
        /// CreateImprovementAsync overload method that takes only ImprovedName and ImprovementType, using default properties otherwise.
        /// </summary>
        /// <param name="selectedValue">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="improvementType">Type of object the Improvement applies to.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        private Task<Improvement> CreateImprovementAsync(string selectedValue, Improvement.ImprovementType improvementType, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = selectedValue;
            else
                SelectedValue += ", " + selectedValue; // Do not use localized value for space because we expect the selected value to be in English
            return CreateImprovementAsync(selectedValue, _objImprovementSource, SourceName, improvementType, _strUnique, token: token);
        }

        /// <summary>
        /// CreateImprovement overload method that takes only ImprovedName, Target and ImprovementType, using default properties otherwise.
        /// </summary>
        /// <param name="strImprovementName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="improvementType">Type of object the Improvement applies to.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        private Task<Improvement> CreateImprovementAsync(string strImprovementName, string strTarget,
            Improvement.ImprovementType improvementType, CancellationToken token = default)
        {
            return CreateImprovementAsync(strImprovementName, _objImprovementSource, SourceName, improvementType, _strUnique, strTarget: strTarget, token: token);
        }

        #region Improvement Methods

#pragma warning disable IDE1006 // Naming Styles

public async Task qualitylevel(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Get the level value from the bonus node
            int intLevel = await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);

            // Get the quality group from the group attribute (e.g., "SINner", "TrustFund")
            string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token) ?? string.Empty;

            // Create an improvement that tracks this quality level
            await CreateImprovementAsync(strGroup, _objImprovementSource, SourceName,
                Improvement.ImprovementType.QualityLevel, _strUnique,
                intLevel, 1, 0, 0, 0, 0, token: token).ConfigureAwait(false);
        }

        // Dummy Method for SelectText
        public static Task selecttext(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return bonusNode == null
                ? Task.FromException(new ArgumentNullException(nameof(bonusNode)))
                : Task.CompletedTask;
        }

        public async Task surprise(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Surprise, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task spellresistance(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellResistance, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task mentalmanipulationresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MentalManipulationResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task physicalmanipulationresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalManipulationResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task manaillusionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ManaIllusionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task physicalillusionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalIllusionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task detectionspellresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DetectionSpellResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task directmanaspellresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DirectManaSpellResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task directphysicalspellresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DirectPhysicalSpellResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreasebodresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseBODResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreaseagiresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseAGIResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreaserearesist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseREAResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreasestrresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseSTRResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreasecharesist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseCHAResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreaseintresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseINTResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreaselogresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseLOGResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task decreasewilresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DecreaseWILResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task enableattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            switch (bonusNode["name"]?.InnerTextViaPool(token).ToUpperInvariant())
            {
                case "MAG":
                    await CreateImprovementAsync("MAG", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0, token: token).ConfigureAwait(false);
                    await _objCharacter.SetMAGEnabledAsync(true, token).ConfigureAwait(false);
                    break;

                case "RES":
                    await CreateImprovementAsync("RES", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0, token: token).ConfigureAwait(false);
                    await _objCharacter.SetRESEnabledAsync(true, token).ConfigureAwait(false);
                    break;

                case "DEP":
                    await CreateImprovementAsync("DEP", _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                        "enableattribute", 0, 0, token: token).ConfigureAwait(false);
                    await _objCharacter.SetDEPEnabledAsync(true, token).ConfigureAwait(false);
                    break;
            }
        }

        // Add an Attribute Replacement.
        public async Task replaceattributes(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute"))
            {
                if (objXmlAttributes == null)
                    return;
                foreach (XmlNode objXmlAttribute in objXmlAttributes)
                {
                    // Record the improvement.
                    string strAttribute = string.Empty;
                    if (objXmlAttribute.TryGetStringFieldQuickly("name", ref strAttribute))
                    {
                        // Extract the modifiers.
                        int intMin = 0;
                        int intMax = 0;
                        int intAugMax = 0;
                        objXmlAttribute.TryGetInt32FieldQuickly("min", ref intMin);
                        objXmlAttribute.TryGetInt32FieldQuickly("max", ref intMax);
                        objXmlAttribute.TryGetInt32FieldQuickly("aug", ref intAugMax);

                        await CreateImprovementAsync(strAttribute, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.ReplaceAttribute,
                            _strUnique, 0, 1, intMin, intMax, 0, intAugMax, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }
            }
        }

        // Enable a special tab.
        public async Task enabletab(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            using (XmlNodeList xmlEnableList = bonusNode.SelectNodes("name"))
            {
                if (xmlEnableList?.Count > 0)
                {
                    foreach (XmlNode xmlEnable in xmlEnableList)
                    {
                        switch (xmlEnable.InnerTextViaPool(token).ToUpperInvariant())
                        {
                            case "MAGICIAN":
                                await _objCharacter.SetMagicianEnabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Magician", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;

                            case "ADEPT":
                                await _objCharacter.SetAdeptEnabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Adept", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab",
                                    0, 0, token: token).ConfigureAwait(false);
                                break;

                            case "TECHNOMANCER":
                                await _objCharacter.SetTechnomancerEnabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Technomancer", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;

                            case "ADVANCED PROGRAMS":
                                await _objCharacter.SetAdvancedProgramsEnabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Advanced Programs", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;

                            case "CRITTER":
                                await _objCharacter.SetCritterEnabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Critter", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "enabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
        }

        // Disable a  tab.
        public async Task disabletab(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList xmlDisableList = bonusNode.SelectNodes("name"))
            {
                if (xmlDisableList?.Count > 0)
                {
                    foreach (XmlNode xmlDisable in xmlDisableList)
                    {
                        switch (xmlDisable.InnerTextViaPool(token).ToUpperInvariant())
                        {
                            case "CYBERWARE":
                                await _objCharacter.SetCyberwareDisabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Cyberware", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "disabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;

                            case "INITIATION":
                                await _objCharacter.SetInitiationForceDisabledAsync(true, token).ConfigureAwait(false);
                                await CreateImprovementAsync("Initiation", _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialTab,
                                    "disabletab", 0, 0, token: token).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
        }

        // Select Restricted (select Restricted items for Fake Licenses).
        public async Task selectrestricted(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    frmPickItem.MyForm.SetRestrictedMode(_objCharacter);
                    if (!string.IsNullOrEmpty(ForcedValue))
                        frmPickItem.MyForm.ForceItem(ForcedValue);

                    frmPickItem.MyForm.AllowAutoSelect = !string.IsNullOrEmpty(ForcedValue);

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName,
                Improvement.ImprovementType.Restricted, _strUnique, token: token).ConfigureAwait(false);
        }

        public async Task selecttradition(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                // Populate the Magician Traditions list.
                XPathNavigator xmlTraditionsBaseChummerNode =
                    (await _objCharacter.LoadDataXPathAsync("traditions.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer", token);
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstTraditions))
                {
                    if (xmlTraditionsBaseChummerNode != null)
                    {
                        foreach (XPathNavigator xmlTradition in xmlTraditionsBaseChummerNode.Select(
                                     "traditions/tradition[" + await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false) + "]"))
                        {
                            string strName = xmlTradition.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                lstTraditions.Add(new ListItem(
                                                      xmlTradition.SelectSingleNodeAndCacheExpression("id", token)?.Value
                                                      ?? strName,
                                                      xmlTradition.SelectSingleNodeAndCacheExpression("translate", token)
                                                                  ?.Value ?? strName));
                        }
                    }

                    if (lstTraditions.Count > 1)
                    {
                        lstTraditions.Sort(CompareListItems.CompareNames);
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                           {
                               AllowAutoSelect = false
                           }, token).ConfigureAwait(false))
                    {
                        frmPickItem.MyForm.SetDropdownItemsMode(lstTraditions);
                        frmPickItem.MyForm.SelectedItem = await (await _objCharacter.GetMagicTraditionAsync(token).ConfigureAwait(false)).GetSourceIDStringAsync(token).ConfigureAwait(false);

                        // Make sure the dialogue window was not canceled.
                        if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                    }
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Tradition,
                _strUnique, token: token).ConfigureAwait(false);
        }

        public async Task cyberseeker(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            //Check if valid attrib
            string strBonusNodeText = bonusNode.InnerTextViaPool(token);
            if (strBonusNodeText == "BOX" || AttributeSection.AttributeStrings.Contains(strBonusNodeText))
            {
                await CreateImprovementAsync(strBonusNodeText, _objImprovementSource, SourceName, Improvement.ImprovementType.Seeker, _strUnique, 0, 0, token: token).ConfigureAwait(false);
            }
            else
            {
                Utils.BreakIfDebug();
            }
        }

        public async Task cyberlimbattributebonus(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            
            string strAttribute = bonusNode["name"]?.InnerTextViaPool(token) ?? string.Empty;
            decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);

            await CreateImprovementAsync(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberlimbAttributeBonus, _strUnique, decValue, 0, token: token).ConfigureAwait(false);
        }

        public Task blockskillcategorydefaulting(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            // Expected values are either a Skill Name or an empty string.
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName,
                              Improvement.ImprovementType.BlockSkillCategoryDefault, _strUnique, token: token);
        }

        public async Task blockskillgroupdefaulting(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strExclude = bonusNode.Attributes?["excludecategory"]?.InnerTextViaPool(token) ?? string.Empty;
            string strSelect = bonusNode.InnerTextViaPool(token);
            if (string.IsNullOrEmpty(strSelect))
            {
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("String_Improvement_SelectSkillGroupName", token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("String_Improvement_SelectSkillGroup", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectSkillGroup> frmPickSkillGroup = await ThreadSafeForm<SelectSkillGroup>.GetAsync(
                           () => new SelectSkillGroup(_objCharacter)
                           {
                               Description = strDescription
                           }, token).ConfigureAwait(false))
                {
                    if (!string.IsNullOrEmpty(ForcedValue))
                    {
                        frmPickSkillGroup.MyForm.OnlyGroup = ForcedValue;
                        frmPickSkillGroup.MyForm.Opacity = 0;
                    }

                    if (!string.IsNullOrEmpty(strExclude))
                        frmPickSkillGroup.MyForm.ExcludeCategory = strExclude;

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickSkillGroup.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelect = frmPickSkillGroup.MyForm.SelectedSkillGroup;
                }
            }

            SelectedValue = strSelect;

            await CreateImprovementAsync(strSelect, _objImprovementSource, SourceName,
                Improvement.ImprovementType.BlockSkillGroupDefault, _strUnique, 0, 0, 0, 1, 0, 0, strExclude, token: token).ConfigureAwait(false);
        }

        public async Task blockskilldefaulting(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSelectedSkill = bonusNode.InnerTextViaPool(token);
            if (string.IsNullOrEmpty(strSelectedSkill))
            {
                string strForcedValue = ForcedValue;
                strSelectedSkill = string.IsNullOrEmpty(strForcedValue)
                    ? (await ImprovementManager.DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false)).Item1
                    : strForcedValue;
            }
            // Expected values are either a Skill Name or an empty string.
            await CreateImprovementAsync(strSelectedSkill, _objImprovementSource, SourceName,
                Improvement.ImprovementType.BlockSkillDefault, _strUnique, token: token).ConfigureAwait(false);
        }

        public Task allowskilldefaulting(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            // Expected values are either a Skill Name or an empty string.
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.AllowSkillDefault, _strUnique, token: token);
        }

        // Select a Skill.
        public async Task selectskill(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            if (ForcedValue == "+2 to a Combat Skill")
                ForcedValue = string.Empty;

            (string strSelectedSkill, bool blnIsKnowledgeSkill) = await ImprovementManager.DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false);

            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;

            SelectedValue = strSelectedSkill;

            string strVal = bonusNode["val"]?.InnerTextViaPool(token);
            string strMax = bonusNode["max"]?.InnerTextViaPool(token);
            bool blnDisableSpec = false;
            bonusNode.TryGetBoolFieldQuickly("disablespecializationeffects", ref blnDisableSpec);
            // Find the selected Skill.
            if (blnIsKnowledgeSkill)
            {
                if (await _objCharacter.SkillsSection.KnowledgeSkills.AnyAsync(async k => await k.GetDictionaryKeyAsync(token).ConfigureAwait(false) == strSelectedSkill, token).ConfigureAwait(false))
                {
                    await _objCharacter.SkillsSection.KnowledgeSkills.ForEachAsync(async objKnowledgeSkill =>
                    {
                        string strName = await objKnowledgeSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                        if (strName != strSelectedSkill)
                            return;
                        // We've found the selected Skill.
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill, _strUnique,
                                await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), 1, 0, 0,
                                0, 0, string.Empty, blnAddToRating, token: token).ConfigureAwait(false);
                        }

                        if (blnDisableSpec)
                        {
                            await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.DisableSpecializationEffects, _strUnique, token: token).ConfigureAwait(false);
                        }

                        if (!string.IsNullOrEmpty(strMax))
                        {
                            await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                                await ImprovementManager.ValueToIntAsync(_objCharacter, strMax, _intRating, token).ConfigureAwait(false), 0, 0,
                                string.Empty, blnAddToRating, token: token).ConfigureAwait(false);
                        }
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    bool blnAllowUpgrade = true;
                    bonusNode.TryGetBoolFieldQuickly("disableupgrades", ref blnAllowUpgrade);
                    KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(_objCharacter, strSelectedSkill, blnAllowUpgrade);
                    try
                    {
                        await _objCharacter.SkillsSection.KnowledgeSkills.AddAsync(objKnowledgeSkill, token).ConfigureAwait(false);
                        // We've found the selected Skill.
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            await CreateImprovementAsync(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty,
                                blnAddToRating, token: token).ConfigureAwait(false);
                        }

                        if (blnDisableSpec)
                        {
                            await CreateImprovementAsync(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.DisableSpecializationEffects,
                                _strUnique, token: token).ConfigureAwait(false);
                        }

                        if (!string.IsNullOrEmpty(strMax))
                        {
                            await CreateImprovementAsync(objKnowledgeSkill.DictionaryKey, _objImprovementSource, SourceName,
                                Improvement.ImprovementType.Skill,
                                _strUnique,
                                0, 1, 0, await ImprovementManager.ValueToIntAsync(_objCharacter, strMax, _intRating, token).ConfigureAwait(false), 0, 0, string.Empty,
                                blnAddToRating, token: token).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        await objKnowledgeSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
            }
            else if (await ExoticSkill.IsExoticSkillNameAsync(_objCharacter, strSelectedSkill, token).ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    // Make sure we have the exotic skill in the list if we're altering any values
                    Skill objExistingSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync(strSelectedSkill, token).ConfigureAwait(false);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedSkill;
                        int intParenthesesIndex = strSkillName.IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex > 0)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            strSkillName = strSkillName.Substring(0, intParenthesesIndex);
                            await _objCharacter.SkillsSection.AddExoticSkillAsync(strSkillName, strSkillSpecific, token).ConfigureAwait(false);
                        }
                    }
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.Skill, _strUnique,
                        await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), 1,
                        0, 0, 0, 0, string.Empty, blnAddToRating, token: token).ConfigureAwait(false);
                }

                if (blnDisableSpec)
                {
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.DisableSpecializationEffects,
                        _strUnique, token: token).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(strMax))
                {
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource,
                        SourceName,
                        Improvement.ImprovementType.Skill, _strUnique, 0, 1, 0,
                        await ImprovementManager.ValueToIntAsync(_objCharacter, strMax, _intRating, token).ConfigureAwait(false), 0, 0, string.Empty,
                        blnAddToRating, token: token).ConfigureAwait(false);
                }
            }
            else
            {
                // Add in improvements even if we have matching skill in case the matching skill gets added later.
                if (!string.IsNullOrEmpty(strVal))
                {
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Skill,
                        _strUnique,
                        await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0,
                        string.Empty,
                        blnAddToRating, token: token).ConfigureAwait(false);
                }

                if (blnDisableSpec)
                {
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.DisableSpecializationEffects,
                        _strUnique, token: token).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(strMax))
                {
                    await CreateImprovementAsync(strSelectedSkill, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Skill,
                        _strUnique,
                        0, 1, 0, await ImprovementManager.ValueToIntAsync(_objCharacter, strMax, _intRating, token).ConfigureAwait(false), 0, 0, string.Empty,
                        blnAddToRating, token: token).ConfigureAwait(false);
                }
            }
        }

        // Select a Skill Group.
        public async Task selectskillgroup(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strExclude = string.Empty;
            if (bonusNode.Attributes?["excludecategory"] != null)
                strExclude = bonusNode.Attributes["excludecategory"].InnerTextViaPool(token);

            string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                ? string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectSkillGroupName", token: token).ConfigureAwait(false), _strFriendlyName)
                : await LanguageManager.GetStringAsync("String_Improvement_SelectSkillGroup", token: token).ConfigureAwait(false);
            using (ThreadSafeForm<SelectSkillGroup> frmPickSkillGroup = await ThreadSafeForm<SelectSkillGroup>.GetAsync(() => new SelectSkillGroup(_objCharacter)
                   {
                       Description = strDescription
                   }, token).ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickSkillGroup.MyForm.OnlyGroup = ForcedValue;
                    frmPickSkillGroup.MyForm.Opacity = 0;
                }

                if (!string.IsNullOrEmpty(strExclude))
                    frmPickSkillGroup.MyForm.ExcludeCategory = strExclude;

                // Make sure the dialogue window was not canceled.
                if (await frmPickSkillGroup.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickSkillGroup.MyForm.SelectedSkillGroup;
            }

            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;

            string strBonus = bonusNode["bonus"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, strBonus, _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, strExclude,
                    blnAddToRating, token: token).ConfigureAwait(false);
            }
            else
            {
                await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroup,
                    _strUnique, 0, 0, 0, 1, 0, 0, strExclude,
                    blnAddToRating, token: token).ConfigureAwait(false);
            }
        }

        public async Task selectattributes(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            bool blnSingleSelected = true;
            List<string> selectedValues = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlSelectAttributeList = bonusNode.SelectNodes("selectattribute"))
            {
                if (xmlSelectAttributeList != null)
                {
                    foreach (XmlNode objXmlAttribute in xmlSelectAttributeList)
                    {
                        List<string> lstAbbrevs = new List<string>(xmlSelectAttributeList.Count);
                        using (XmlNodeList xmlAttributeList = objXmlAttribute.SelectNodes("attribute"))
                        {
                            if (xmlAttributeList?.Count > 0)
                            {
                                foreach (XmlNode objSubNode in xmlAttributeList)
                                    lstAbbrevs.Add(objSubNode.InnerTextViaPool(token));
                            }
                            else
                            {
                                lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                                using (XmlNodeList xmlAttributeList2 = objXmlAttribute.SelectNodes("excludeattribute"))
                                {
                                    if (xmlAttributeList2?.Count > 0)
                                    {
                                        foreach (XmlNode objSubNode in xmlAttributeList2)
                                            lstAbbrevs.Remove(objSubNode.InnerTextViaPool(token));
                                    }
                                }
                            }
                        }

                        lstAbbrevs.Remove("ESS");
                        if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                        {
                            lstAbbrevs.Remove("MAG");
                            lstAbbrevs.Remove("MAGAdept");
                        }
                        else if (!await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                            lstAbbrevs.Remove("MAGAdept");

                        if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                            lstAbbrevs.Remove("RES");
                        if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                            lstAbbrevs.Remove("DEP");

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (!string.IsNullOrEmpty(ForcedValue))
                            LimitSelection = ForcedValue;

                        if (!string.IsNullOrEmpty(LimitSelection))
                        {
                            lstAbbrevs.RemoveAll(x => x != LimitSelection);
                        }

                        // Display the Select Attribute window and record which Skill was selected.
                        string strSelected;
                        switch (lstAbbrevs.Count)
                        {
                            case 0:
                                throw new AbortedException();
                            case 1:
                                strSelected = lstAbbrevs[0];
                                break;

                            default:
                            {
                                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                                    ? string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync(
                                            "String_Improvement_SelectAttributeNamed", token: token).ConfigureAwait(false),
                                        _strFriendlyName)
                                    : await LanguageManager.GetStringAsync("String_Improvement_SelectAttribute",
                                        token: token).ConfigureAwait(false);
                                using (ThreadSafeForm<SelectAttribute> frmPickAttribute
                                       = await ThreadSafeForm<SelectAttribute>.GetAsync(
                                           () => new SelectAttribute(lstAbbrevs.ToArray())
                                           {
                                               Description = strDescription
                                           }, token).ConfigureAwait(false))
                                {
                                    // Make sure the dialogue window was not canceled.
                                    if (await frmPickAttribute.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) ==
                                        DialogResult.Cancel)
                                    {
                                        throw new AbortedException();
                                    }

                                    strSelected = frmPickAttribute.MyForm.SelectedAttribute;

                                    break;
                                }
                            }
                        }

                        if (blnSingleSelected && selectedValues.Count > 0 && !selectedValues.Contains(strSelected))
                            blnSingleSelected = false;
                        selectedValues.Add(strSelected);

                        // Record the improvement.
                        int intMin = 0;
                        int intAug = 0;
                        int intMax = 0;
                        int intAugMax = 0;

                        // Extract the modifiers.
                        string strTemp = objXmlAttribute["min"]?.InnerTextViaPool(token);
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intMin);
                        strTemp = objXmlAttribute["val"]?.InnerTextViaPool(token);
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intAug);
                        strTemp = objXmlAttribute["max"]?.InnerTextViaPool(token);
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intMax);
                        strTemp = objXmlAttribute["aug"]?.InnerTextViaPool(token);
                        if (!string.IsNullOrEmpty(strTemp))
                            int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intAugMax);

                        string strAttribute = strSelected;

                        if (objXmlAttribute["affectbase"] != null)
                            strAttribute += "Base";

                        await CreateImprovementAsync(strAttribute, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.Attribute,
                            _strUnique,
                            0, 1, intMin, intMax, intAug, intAugMax, token: token).ConfigureAwait(false);
                    }
                }
            }

            if (blnSingleSelected)
            {
                SelectedValue = selectedValues.FirstOrDefault();
            }
            else
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sdbValue))
                {
                    foreach (string s in AttributeSection.AttributeStrings)
                    {
                        int i = selectedValues.Count(c => c == s);
                        if (i <= 0)
                            continue;
                        if (sdbValue.Length > 0)
                        {
                            sdbValue.Append(',', strSpace);
                        }

                        sdbValue.AppendFormat(GlobalSettings.CultureInfo, "{0}{1}({2})", s, strSpace, i);
                    }

                    SelectedValue = sdbValue.ToString();
                }
            }
        }

        // Select an CharacterAttribute.
        public async Task selectattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool(token));
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool(token));
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("MAGAdept");

            if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("RES");
            if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("DEP");

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                lstAbbrevs.RemoveAll(x => x != LimitSelection);
            }

            string strSelected;
            switch (lstAbbrevs.Count)
            {
                case 0:
                    throw new AbortedException();
                case 1:
                    strSelected = lstAbbrevs[0];
                    break;

                default:
                {
                    string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                        ? string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("String_Improvement_SelectAttributeNamed",
                                token: token).ConfigureAwait(false),
                            _strFriendlyName)
                        : await LanguageManager.GetStringAsync("String_Improvement_SelectAttribute", token: token).ConfigureAwait(false);
                    // Display the Select Attribute window and record which Skill was selected.
                    using (ThreadSafeForm<SelectAttribute> frmPickAttribute =
                           await ThreadSafeForm<SelectAttribute>.GetAsync(
                               () => new SelectAttribute(lstAbbrevs.ToArray())
                               {
                                   Description = strDescription
                               }, token).ConfigureAwait(false))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (await frmPickAttribute.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        strSelected = frmPickAttribute.MyForm.SelectedAttribute;
                    }

                    break;
                }
            }

            SelectedValue = strSelected;

            // Record the improvement.
            int intMin = 0;
            decimal decAug = 0;
            int intMax = 0;
            int intAugMax = 0;

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                intMin = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            strTemp = bonusNode["val"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                decAug = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            strTemp = bonusNode["max"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                intMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            strTemp = bonusNode["aug"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);

            string strAttribute = strSelected;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            await CreateImprovementAsync(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                _strUnique,
                0, 1, intMin, intMax, decAug, intAugMax, token: token).ConfigureAwait(false);
        }

        // Select a Limit.
        public async Task selectlimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> strLimits = new List<string>(4);
            using (XmlNodeList xmlDefinedLimits = bonusNode.SelectNodes("limit"))
            {
                if (xmlDefinedLimits?.Count > 0)
                {
                    foreach (XmlNode objXmlAttribute in xmlDefinedLimits)
                        strLimits.Add(objXmlAttribute.InnerTextViaPool(token));
                }
                else
                {
                    strLimits.Add("Physical");
                    strLimits.Add("Mental");
                    strLimits.Add("Social");
                }
            }

            using (XmlNodeList xmlExcludeLimits = bonusNode.SelectNodes("excludelimit"))
            {
                if (xmlExcludeLimits?.Count > 0)
                {
                    foreach (XmlNode objXmlAttribute in xmlExcludeLimits)
                    {
                        strLimits.Remove(objXmlAttribute.InnerTextViaPool(token));
                    }
                }
            }

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                strLimits.RemoveAll(x => x != LimitSelection);
            }

            // Display the Select Limit window and record which Limit was selected.
            string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                ? string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectLimitNamed",
                        token: token).ConfigureAwait(false),
                    _strFriendlyName)
                : await LanguageManager.GetStringAsync("String_Improvement_SelectLimit", token: token).ConfigureAwait(false);
            using (ThreadSafeForm<SelectLimit> frmPickLimit = await ThreadSafeForm<SelectLimit>.GetAsync(() => new SelectLimit(strLimits.ToArray())
                   {
                       Description = strDescription
                   }, token).ConfigureAwait(false))
            {
                // Make sure the dialogue window was not canceled.
                if (await frmPickLimit.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                // Record the improvement.
                int intMin = 0;
                decimal decAug = 0;
                int intMax = 0;
                int intAugMax = 0;

                // Extract the modifiers.
                string strTemp = bonusNode["min"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                    intMin = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                strTemp = bonusNode["val"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                    decAug = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                strTemp = bonusNode["max"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                    intMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                strTemp = bonusNode["aug"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                    intAugMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);

                string strLimit = frmPickLimit.MyForm.SelectedLimit;

                // string strBonus = bonusNode["value"].InnerTextViaPool(token);
                Improvement.ImprovementType eType;

                switch (strLimit.ToUpperInvariant())
                {
                    case "MENTAL":
                        {
                            eType = Improvement.ImprovementType.MentalLimit;
                            break;
                        }
                    case "SOCIAL":
                        {
                            eType = Improvement.ImprovementType.SocialLimit;
                            break;
                        }
                    case "PHYSICAL":
                        {
                            eType = Improvement.ImprovementType.PhysicalLimit;
                            break;
                        }
                    default:
                        throw new AbortedException();
                }

                SelectedValue = frmPickLimit.MyForm.SelectedDisplayLimit;

                if (bonusNode["affectbase"] != null)
                    strLimit += "Base";

                await CreateImprovementAsync(strLimit, _objImprovementSource, SourceName, eType, _strUnique, decAug, 0, intMin,
                    intMax,
                    decAug, intAugMax, token: token).ConfigureAwait(false);
            }
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public async Task swapskillattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool(token));
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool(token));
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("MAGAdept");

            if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("RES");
            if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("String_Improvement_SelectAttributeNamed",
                            token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("String_Improvement_SelectAttribute", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectAttribute> frmPickAttribute = await ThreadSafeForm<SelectAttribute>.GetAsync(() => new SelectAttribute(lstAbbrevs.ToArray())
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickAttribute.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickAttribute.MyForm.SelectedAttribute;
                }
            }

            string strLimitToSkill = bonusNode["limittoskill"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("String_Improvement_SelectSkillNamed",
                            token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("String_Improvement_SelectSkill", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectSkill> frmPickSkill = await ThreadSafeForm<SelectSkill>.GetAsync(() =>
                           new SelectSkill(_objCharacter), token).ConfigureAwait(false))
                {
                    await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    string strTemp = bonusNode["skillgroup"]?.InnerTextViaPool(token);
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                    else
                    {
                        XmlElement xmlSkillCategories = bonusNode["skillcategories"];
                        if (xmlSkillCategories != null)
                            frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                        else
                        {
                            strTemp = bonusNode["skillcategory"]?.InnerTextViaPool(token);
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.MyForm.OnlyCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode["excludecategory"]?.InnerTextViaPool(token);
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.MyForm.ExcludeCategory = strTemp;
                                else
                                {
                                    strTemp = bonusNode["limittoattribute"]?.InnerTextViaPool(token);
                                    if (!string.IsNullOrEmpty(strTemp))
                                        frmPickSkill.MyForm.LinkedAttribute = strTemp;
                                }
                            }
                        }
                    }

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickSkill.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedTarget = frmPickSkill.MyForm.SelectedSkill;
                }
            }

            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, string.Empty, false, SelectedTarget, token: token).ConfigureAwait(false);
        }

        // Select an CharacterAttribute to use instead of the default on a skill.
        public async Task swapskillspecattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
            using (XmlNodeList xmlAttributeList = bonusNode.SelectNodes("attribute"))
            {
                if (xmlAttributeList?.Count > 0)
                {
                    foreach (XmlNode objSubNode in xmlAttributeList)
                        lstAbbrevs.Add(objSubNode.InnerTextViaPool(token));
                }
                else
                {
                    lstAbbrevs.AddRange(AttributeSection.AttributeStrings);
                    using (XmlNodeList xmlAttributeList2 = bonusNode.SelectNodes("excludeattribute"))
                    {
                        if (xmlAttributeList2?.Count > 0)
                        {
                            foreach (XmlNode objSubNode in xmlAttributeList2)
                                lstAbbrevs.Remove(objSubNode.InnerTextViaPool(token));
                        }
                    }
                }
            }

            lstAbbrevs.Remove("ESS");
            if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
            {
                lstAbbrevs.Remove("MAG");
                lstAbbrevs.Remove("MAGAdept");
            }
            else if (!await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("MAGAdept");

            if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("RES");
            if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                lstAbbrevs.Remove("DEP");

            if (lstAbbrevs.Count == 1)
                LimitSelection = lstAbbrevs[0];

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (!string.IsNullOrEmpty(LimitSelection))
            {
                SelectedValue = LimitSelection;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("String_Improvement_SelectAttributeNamed",
                            token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("String_Improvement_SelectAttribute", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectAttribute> frmPickAttribute = await ThreadSafeForm<SelectAttribute>.GetAsync(() =>
                           new SelectAttribute(lstAbbrevs.ToArray())
                           {
                               Description = strDescription
                           }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickAttribute.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickAttribute.MyForm.SelectedAttribute;
                }
            }

            string strLimitToSkill = bonusNode["limittoskill"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strLimitToSkill))
            {
                SelectedTarget = strLimitToSkill;
            }
            else
            {
                // Display the Select Attribute window and record which Skill was selected.
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("String_Improvement_SelectSkillNamed",
                            token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("String_Improvement_SelectSkill", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectSkill> frmPickSkill = await ThreadSafeForm<SelectSkill>.GetAsync(() =>
                           new SelectSkill(_objCharacter), token).ConfigureAwait(false))
                {
                    await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    string strTemp = bonusNode["skillgroup"]?.InnerTextViaPool(token);
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                    else
                    {
                        XmlElement xmlSkillCategories = bonusNode["skillcategories"];
                        if (xmlSkillCategories != null)
                            frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                        else
                        {
                            strTemp = bonusNode["skillcategory"]?.InnerTextViaPool(token);
                            if (!string.IsNullOrEmpty(strTemp))
                                frmPickSkill.MyForm.OnlyCategory = strTemp;
                            else
                            {
                                strTemp = bonusNode["excludecategory"]?.InnerTextViaPool(token);
                                if (!string.IsNullOrEmpty(strTemp))
                                    frmPickSkill.MyForm.ExcludeCategory = strTemp;
                                else
                                {
                                    strTemp = bonusNode["limittoattribute"]?.InnerTextViaPool(token);
                                    if (!string.IsNullOrEmpty(strTemp))
                                        frmPickSkill.MyForm.LinkedAttribute = strTemp;
                                }
                            }
                        }
                    }

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickSkill.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedTarget = frmPickSkill.MyForm.SelectedSkill;
                }
            }

            // TODO: Allow selection of specializations through frmSelectSkillSpec
            string strSpec = bonusNode["spec"]?.InnerTextViaPool(token) ?? string.Empty;

            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.SwapSkillSpecAttribute, _strUnique,
                0, 1, 0, 0, 0, 0, strSpec, false, SelectedTarget, token: token).ConfigureAwait(false);
        }

        // Select a Spell.
        public async Task selectspell(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode node;
            // Display the Select Spell window.
            using (ThreadSafeForm<SelectSpell> frmPickSpell = await ThreadSafeForm<SelectSpell>.GetAsync(() => new SelectSpell(_objCharacter), token).ConfigureAwait(false))
            {
                string strCategory = bonusNode.Attributes?["category"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strCategory))
                    frmPickSpell.MyForm.LimitCategory = strCategory;

                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    frmPickSpell.MyForm.ForceSpellName = ForcedValue;
                    frmPickSpell.MyForm.Opacity = 0;
                }

                frmPickSpell.MyForm.IgnoreRequirements = bonusNode.Attributes?["ignorerequirements"]?.InnerTextIsTrueString() == true;

                // Make sure the dialogue window was not canceled.
                if (await frmPickSpell.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                // Open the Spells XML file and locate the selected piece.
                XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("spells.xml", token: token).ConfigureAwait(false);

                node = objXmlDocument.TryGetNodeByNameOrId("/chummer/spells/spell",
                    frmPickSpell.MyForm.SelectedSpell);
            }

            if (node == null)
                throw new AbortedException();

            SelectedValue = node["name"]?.InnerTextViaPool(token);

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = node.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext", token);
            if (xmlSelectText != null)
            {
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    node["translate"]?.InnerTextViaPool(token) ?? node["name"]?.InnerTextViaPool(token));
                using (ThreadSafeForm<SelectText> frmPickText = await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickText.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        throw new AbortedException();

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            Spell spell = new Spell(_objCharacter);
            try
            {
                await spell.CreateAsync(node, strExtra, token: token).ConfigureAwait(false);
                if (spell.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                spell.Grade = -1;
                await _objCharacter.Spells.AddAsync(spell, token).ConfigureAwait(false);

                await CreateImprovementAsync(spell.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Spell,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await spell.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Add a specific Spell to the Character.
        public async Task addspell(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlDocument objXmlSpellDocument = await _objCharacter.LoadDataAsync("spells.xml", token: token).ConfigureAwait(false);

            XmlNode node = objXmlSpellDocument.TryGetNodeByNameOrId("/chummer/spells/spell", bonusNode.InnerTextViaPool(token)) ?? throw new AbortedException();
            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = node.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext", token);
            if (xmlSelectText != null)
            {
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    node["translate"]?.InnerTextViaPool(token) ?? node["name"]?.InnerTextViaPool(token));
                using (ThreadSafeForm<SelectText> frmPickText = await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickText.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            Spell spell = new Spell(_objCharacter);
            try
            {
                await spell.CreateAsync(node, strExtra, token: token).ConfigureAwait(false);
                if (spell.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                spell.Alchemical = bonusNode.Attributes?["alchemical"]?.InnerTextIsTrueString() == true;
                spell.Extended = bonusNode.Attributes?["extended"]?.InnerTextIsTrueString() == true;
                spell.Limited = bonusNode.Attributes?["limited"]?.InnerTextIsTrueString() == true;
                spell.BarehandedAdept = bonusNode.Attributes?["barehandedadept"]?.InnerTextIsTrueString() == true || bonusNode.Attributes?["usesunarmed"]?.InnerTextIsTrueString() == true;
                spell.Grade = -1;
                await _objCharacter.Spells.AddAsync(spell, token).ConfigureAwait(false);

                await CreateImprovementAsync(spell.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Spell,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await spell.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Select a Complex Form.
        public async Task selectcomplexform(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedComplexForm = ForcedValue;

            if (string.IsNullOrEmpty(strSelectedComplexForm))
            {
                // Display the Select ComplexForm window.
                using (ThreadSafeForm<SelectComplexForm> frmPickComplexForm = await ThreadSafeForm<SelectComplexForm>.GetAsync(() => new SelectComplexForm(_objCharacter), token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickComplexForm.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelectedComplexForm = frmPickComplexForm.MyForm.SelectedComplexForm;
                }
            }

            // Open the ComplexForms XML file and locate the selected piece.
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("complexforms.xml", token: token).ConfigureAwait(false);

            XmlNode node = objXmlDocument.TryGetNodeByNameOrId("/chummer/complexforms/complexforms", strSelectedComplexForm)
                           ?? throw new AbortedException();

            SelectedValue = node["name"]?.InnerTextViaPool(token);

            ComplexForm objComplexform = new ComplexForm(_objCharacter);
            try
            {
                await objComplexform.CreateAsync(node, token: token).ConfigureAwait(false);
                if (objComplexform.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                objComplexform.Grade = -1;

                await _objCharacter.ComplexForms.AddAsync(objComplexform, token).ConfigureAwait(false);

                await CreateImprovementAsync(objComplexform.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.ComplexForm,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objComplexform.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Add a specific ComplexForm to the Character.
        public async Task addcomplexform(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlDocument objXmlComplexFormDocument = await _objCharacter.LoadDataAsync("complexforms.xml", token: token).ConfigureAwait(false);

            XmlNode node = objXmlComplexFormDocument.TryGetNodeByNameOrId("/chummer/complexforms/complexform", bonusNode.InnerTextViaPool(token)) ?? throw new AbortedException();

            ComplexForm objComplexForm = new ComplexForm(_objCharacter);
            try
            {
                await objComplexForm.CreateAsync(node, token: token).ConfigureAwait(false);
                if (objComplexForm.InternalId.IsEmptyGuid())
                    throw new AbortedException();
                objComplexForm.Grade = -1;

                await _objCharacter.ComplexForms.AddAsync(objComplexForm, token).ConfigureAwait(false);

                await CreateImprovementAsync(objComplexForm.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.ComplexForm,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objComplexForm.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Add a specific Gear to the Character.
        public async Task addgear(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Gear objNewGear = await Purchase(bonusNode).ConfigureAwait(false);
            using (XmlNodeList xmlChildren = bonusNode["children"]?.ChildNodes)
            {
                if (xmlChildren?.Count > 0)
                {
                    foreach (XmlNode xmlChildNode in xmlChildren)
                    {
                        await Purchase(xmlChildNode, objNewGear).ConfigureAwait(false);
                    }
                }
            }

            async Task<Gear> Purchase(XmlNode xmlGearNode, Gear objParent = null)
            {
                string strName = xmlGearNode["name"]?.InnerTextViaPool(token) ?? string.Empty;
                string strCategory = xmlGearNode["category"]?.InnerTextViaPool(token) ?? string.Empty;
                string strFilter = "/chummer/gears/gear";
                if (!string.IsNullOrEmpty(strName) || !string.IsNullOrEmpty(strCategory))
                {
                    strFilter += "[";
                    if (!string.IsNullOrEmpty(strName))
                    {
                        strFilter += "name = " + strName.CleanXPath();
                        if (!string.IsNullOrEmpty(strCategory))
                            strFilter += " and category = " + strCategory.CleanXPath();
                    }
                    else
                        strFilter += "category = " + strCategory.CleanXPath();
                    strFilter += "]";
                }

                XmlNode xmlGearDataNode = (await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false)).SelectSingleNode(strFilter) ?? throw new AbortedException();
                int intRating = 0;
                string strTemp = string.Empty;
                if (xmlGearNode.TryGetStringFieldQuickly("rating", ref strTemp))
                    intRating = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                decimal decQty = 1.0m;
                if (xmlGearNode["quantity"] != null)
                    decQty = await ImprovementManager.ValueToDecAsync(_objCharacter, xmlGearNode["quantity"].InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);

                // Create the new piece of Gear.
                List<Weapon> lstWeapons = new List<Weapon>(1);

                Gear objNewGearToCreate = new Gear(_objCharacter);
                try
                {
                    await objNewGearToCreate.CreateAsync(xmlGearDataNode, intRating, lstWeapons, ForcedValue, token: token).ConfigureAwait(false);

                    if (objNewGearToCreate.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    await objNewGearToCreate.SetQuantityAsync(decQty, token).ConfigureAwait(false);

                    // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                    if (await _objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) == null && await objNewGearToCreate.GetIsCommlinkAsync(token).ConfigureAwait(false))
                    {
                        await objNewGearToCreate.SetActiveCommlinkAsync(_objCharacter, true, token).ConfigureAwait(false);
                    }

                    if (xmlGearNode["fullcost"] == null)
                        objNewGearToCreate.Cost = "0";
                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in lstWeapons)
                        await _objCharacter.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);

                    objNewGearToCreate.ParentID = SourceName;
                    if (objParent != null)
                    {
                        await objParent.Children.AddAsync(objNewGearToCreate, token).ConfigureAwait(false);
                        await objNewGearToCreate.SetParentAsync(objParent, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await _objCharacter.Gear.AddAsync(objNewGearToCreate, token).ConfigureAwait(false);
                    }

                    await CreateImprovementAsync(objNewGearToCreate.InternalId, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Gear,
                        _strUnique, token: token).ConfigureAwait(false);
                    return objNewGearToCreate;
                }
                catch
                {
                    await objNewGearToCreate.DeleteGearAsync(token: CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }
        }

        // Add a specific Gear to the Character.
        public async Task addweapon(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strName = bonusNode["name"]?.InnerTextViaPool(token) ?? throw new AbortedException();
            XmlNode node = (await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/weapons/weapon", strName) ?? throw new AbortedException();

            // Create the new piece of Gear.
            List<Weapon> lstWeapons = new List<Weapon>(1);

            Weapon objNewWeapon = new Weapon(_objCharacter);
            try
            {
                await objNewWeapon.CreateAsync(node, lstWeapons, token: token).ConfigureAwait(false);

                if (objNewWeapon.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                if (await _objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) == null && await objNewWeapon.GetIsCommlinkAsync(token).ConfigureAwait(false))
                {
                    await objNewWeapon.SetActiveCommlinkAsync(_objCharacter, true, token).ConfigureAwait(false);
                }

                if (bonusNode["fullcost"] == null)
                    objNewWeapon.Cost = "0";

                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                    await _objCharacter.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);

                objNewWeapon.ParentID = SourceName;

                await _objCharacter.Weapons.AddAsync(objNewWeapon, token).ConfigureAwait(false);

                await CreateImprovementAsync(objNewWeapon.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Weapon,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objNewWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Add a specific Gear to the Character.
        public async Task naturalweapon(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Weapon objWeapon = new Weapon(_objCharacter);
            try
            {
                objWeapon.Name = bonusNode["name"]?.InnerTextViaPool(token) ?? _strFriendlyName;
                objWeapon.Category = await LanguageManager.GetStringAsync("Tab_Critter", GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false);
                objWeapon.RangeType = "Melee";
                objWeapon.Reach = bonusNode["reach"]?.InnerTextViaPool(token) ?? "0";
                objWeapon.Accuracy = bonusNode["accuracy"]?.InnerTextViaPool(token) ?? "Physical";
                objWeapon.Damage = bonusNode["damage"]?.InnerTextViaPool(token) ?? "({STR})S";
                objWeapon.AP = bonusNode["ap"]?.InnerTextViaPool(token) ?? "0";
                objWeapon.Mode = "0";
                objWeapon.RC = "0";
                objWeapon.Concealability = "0";
                objWeapon.Avail = "0";
                objWeapon.Cost = "0";
                objWeapon.Ammo = "0";
                objWeapon.UseSkill = bonusNode["useskill"]?.InnerTextViaPool(token) ?? string.Empty;
                objWeapon.Source = bonusNode["source"]?.InnerTextViaPool(token) ?? "SR5";
                objWeapon.Page = bonusNode["page"]?.InnerTextViaPool(token) ?? "0";
                objWeapon.ParentID = SourceName;
                await objWeapon.CreateClipsAsync(token).ConfigureAwait(false);

                await _objCharacter.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);

                await CreateImprovementAsync(objWeapon.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Weapon,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // Select an AI program.
        public async Task selectaiprogram(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = await _objCharacter.LoadDataAsync("programs.xml", token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", ForcedValue)
                             ?? throw new AbortedException();
            }

            if (xmlProgram == null)
            {
                // Display the Select Program window.
                using (ThreadSafeForm<SelectAIProgram> frmPickProgram = await ThreadSafeForm<SelectAIProgram>.GetAsync(() => new SelectAIProgram(_objCharacter), token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickProgram.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", frmPickProgram.MyForm.SelectedProgram)
                                 ?? throw new AbortedException();
                }
            }

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = xmlProgram.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext", token);
            if (xmlSelectText != null)
            {
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    xmlProgram["translate"]?.InnerTextViaPool(token) ?? xmlProgram["name"]?.InnerTextViaPool(token));
                using (ThreadSafeForm<SelectText> frmPickText = await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickText.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            AIProgram objProgram = new AIProgram(_objCharacter);
            await objProgram.CreateAsync(xmlProgram, strExtra, false, token).ConfigureAwait(false);
            if (objProgram.InternalId.IsEmptyGuid())
                throw new AbortedException();

            await _objCharacter.AIPrograms.AddAsync(objProgram, token).ConfigureAwait(false);

            SelectedValue = await objProgram.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);

            await CreateImprovementAsync(objProgram.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AIProgram,
                _strUnique, token: token).ConfigureAwait(false);
        }

        // Select an AI program.
        public async Task selectinherentaiprogram(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            XmlNode xmlProgram = null;
            XmlDocument xmlDocument = await _objCharacter.LoadDataAsync("programs.xml", token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", ForcedValue)
                    ?? throw new AbortedException();
            }

            if (xmlProgram == null)
            {
                // Display the Select Spell window.
                using (ThreadSafeForm<SelectAIProgram> frmPickProgram = await ThreadSafeForm<SelectAIProgram>.GetAsync(() => new SelectAIProgram(_objCharacter, false, true), token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickProgram.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    xmlProgram = xmlDocument.TryGetNodeByNameOrId("/chummer/programs/program", frmPickProgram.MyForm.SelectedProgram)
                                 ?? throw new AbortedException();
                }
            }

            // Check for SelectText.
            string strExtra = string.Empty;
            XPathNavigator xmlSelectText = xmlProgram.SelectSingleNodeAndCacheExpressionAsNavigator("bonus/selecttext", token);
            if (xmlSelectText != null)
            {
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    xmlProgram["translate"]?.InnerTextViaPool(token) ?? xmlProgram["name"]?.InnerTextViaPool(token));
                using (ThreadSafeForm<SelectText> frmPickText = await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickText.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strExtra = frmPickText.MyForm.SelectedValue;
                }
            }

            AIProgram objProgram = new AIProgram(_objCharacter);
            await objProgram.CreateAsync(xmlProgram, strExtra, false, token).ConfigureAwait(false);
            if (objProgram.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = await objProgram.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);

            await _objCharacter.AIPrograms.AddAsync(objProgram, token).ConfigureAwait(false);

            await CreateImprovementAsync(objProgram.InternalId, _objImprovementSource, SourceName,
                Improvement.ImprovementType.AIProgram,
                _strUnique, token: token).ConfigureAwait(false);
        }

        // Select a Contact
        public async Task selectcontact(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strMode = bonusNode["type"]?.InnerTextViaPool(token) ?? "all";

            List<Contact> lstSelectedContacts;
            switch (strMode.ToUpperInvariant())
            {
                case "ALL":
                    lstSelectedContacts = await (await _objCharacter.GetContactsAsync(token).ConfigureAwait(false)).ToListAsync(token).ConfigureAwait(false);
                    break;

                case "GROUP":
                case "NONGROUP":
                    {
                        bool blnGroup = strMode == "group";
                        //Select any contact where IsGroup equals blnGroup
                        //and add to a list
                        lstSelectedContacts = await _objCharacter.Contacts.ToListAsync(x => x.IsGroup == blnGroup, token: token).ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new AbortedException();
            }

            if (lstSelectedContacts.Count == 0)
            {
                await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_NoContactFound", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_NoContactFound", token: token).ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                throw new AbortedException();
            }

            using (ThreadSafeForm<SelectItem> frmSelect = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
            {
                int count = 0;
                //Black magic LINQ to cast content of list to another type
                frmSelect.MyForm.SetGeneralItemsMode(lstSelectedContacts.Select(x => new ListItem(count++.ToString(GlobalSettings.InvariantCultureInfo), x.Name)));

                if (await frmSelect.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    throw new AbortedException();

                Contact objSelectedContact = int.TryParse(await frmSelect.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false), out int intIndex)
                    ? lstSelectedContacts[intIndex]
                    : throw new AbortedException();

                string strTemp = string.Empty;
                if (bonusNode.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
                {
                    decimal decForcedLoyalty = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    await CreateImprovementAsync(await objSelectedContact.GetUniqueIdAsync(token).ConfigureAwait(false), _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, decForcedLoyalty, token: token).ConfigureAwait(false);
                }

                if (bonusNode["free"] != null)
                {
                    await CreateImprovementAsync(await objSelectedContact.GetUniqueIdAsync(token).ConfigureAwait(false), _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique, token: token).ConfigureAwait(false);
                }

                if (bonusNode["forcegroup"] != null)
                {
                    await CreateImprovementAsync(await objSelectedContact.GetUniqueIdAsync(token).ConfigureAwait(false), _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique, token: token).ConfigureAwait(false);
                }

                if (string.IsNullOrWhiteSpace(SelectedValue))
                {
                    SelectedValue = await objSelectedContact.GetNameAsync(token).ConfigureAwait(false);
                }
                else
                {
                    SelectedValue += "," + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + await objSelectedContact.GetNameAsync(token).ConfigureAwait(false);
                }
            }
        }

        public async Task addcontact(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            int intLoyalty = 1;
            int intConnection = 1;

            string strTemp = string.Empty;
            if (bonusNode.TryGetStringFieldQuickly("loyalty", ref strTemp))
                intLoyalty = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            if (bonusNode.TryGetStringFieldQuickly("connection", ref strTemp))
                intConnection = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            bool group = bonusNode["group"] != null;
            bool canwrite = bonusNode["canwrite"] != null;
            Contact contact = new Contact(_objCharacter, !canwrite);
            try
            {
                await contact.SetIsGroupAsync(group, token).ConfigureAwait(false);
                await contact.SetLoyaltyAsync(intLoyalty, token).ConfigureAwait(false);
                await contact.SetConnectionAsync(intConnection, token).ConfigureAwait(false);
                await _objCharacter.Contacts.AddAsync(contact, token).ConfigureAwait(false);

                await CreateImprovementAsync(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.AddContact, contact.UniqueId, token: token).ConfigureAwait(false);

                if (bonusNode.TryGetStringFieldQuickly("forcedloyalty", ref strTemp))
                {
                    decimal decForcedLoyalty = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    await CreateImprovementAsync(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForcedLoyalty, _strUnique, decForcedLoyalty, token: token).ConfigureAwait(false);
                }
                if (bonusNode["free"] != null)
                {
                    await CreateImprovementAsync(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactMakeFree, _strUnique, token: token).ConfigureAwait(false);
                }
                if (bonusNode["forcegroup"] != null)
                {
                    await CreateImprovementAsync(contact.UniqueId, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactForceGroup, _strUnique, token: token).ConfigureAwait(false);
                }
            }
            catch
            {
                try
                {
                    await _objCharacter.Contacts.RemoveAsync(contact, CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    //swallow this
                }
                await contact.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        // Affect a Specific CharacterAttribute.
        public async Task specificattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
            // Record the improvement.
            int intMin = 0;
            decimal decAug = 0;
            int intMax = 0;
            int intAugMax = 0;
            string strAttribute = bonusNode["name"]?.InnerTextViaPool(token);

            // Extract the modifiers.
            string strTemp = bonusNode["min"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                intMin = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            strTemp = bonusNode["val"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                decAug = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            strTemp = bonusNode["max"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.EndsWith("-natural", StringComparison.Ordinal))
                {
                    intMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp.TrimEndOnce("-natural", true), _intRating, token).ConfigureAwait(false) -
                             await (await _objCharacter.GetAttributeAsync(strAttribute, token: token).ConfigureAwait(false)).GetMetatypeMaximumAsync(token).ConfigureAwait(false);
                }
                else
                    intMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
            }
            strTemp = bonusNode["aug"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
                intAugMax = await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);

            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence", token) ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence", token);
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            if (bonusNode["affectbase"] != null)
                strAttribute += "Base";

            await CreateImprovementAsync(strAttribute, _objImprovementSource, SourceName, Improvement.ImprovementType.Attribute,
                strUseUnique, 0, 1, intMin, intMax, decAug, intAugMax, token: token).ConfigureAwait(false);
        }

        // Add a paid increase to an attribute
        public async Task attributelevel(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strAttrib = string.Empty;
            int value = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref value);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strAttrib))
            {
                await CreateImprovementAsync(strAttrib, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Attributelevel, _strUnique, value, token: token).ConfigureAwait(false);
            }
            else if (bonusNode["options"] != null)
            {
                List<string> lstAbbrevs = new List<string>(AttributeSection.AttributeStrings.Count);
                foreach (XmlNode objSubNode in bonusNode["options"])
                    lstAbbrevs.Add(objSubNode.InnerTextViaPool(token));

                lstAbbrevs.Remove("ESS");
                if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                {
                    lstAbbrevs.Remove("MAG");
                    lstAbbrevs.Remove("MAGAdept");
                }
                else if (!await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false) || !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                    lstAbbrevs.Remove("MAGAdept");

                if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                    lstAbbrevs.Remove("RES");
                if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                    lstAbbrevs.Remove("DEP");

                // Check to see if there is only one possible selection because of _strLimitSelection.
                if (!string.IsNullOrEmpty(ForcedValue))
                    LimitSelection = ForcedValue;

                if (!string.IsNullOrEmpty(LimitSelection))
                {
                    lstAbbrevs.RemoveAll(x => x != LimitSelection);
                }

                string strSelected;
                switch (lstAbbrevs.Count)
                {
                    case 0:
                        throw new AbortedException();
                    case 1:
                        strSelected = lstAbbrevs[0];
                        break;

                    default:
                        {
                            string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                                ? string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync(
                                        "String_Improvement_SelectAttributeNamed", token: token).ConfigureAwait(false),
                                    _strFriendlyName)
                                : await LanguageManager.GetStringAsync("String_Improvement_SelectAttribute", token: token).ConfigureAwait(false);
                            using (ThreadSafeForm<SelectAttribute> frmPickAttribute = await ThreadSafeForm<SelectAttribute>.GetAsync(
                                       () =>
                                           new SelectAttribute(lstAbbrevs.ToArray())
                                           {
                                               Description = strDescription
                                           }, token).ConfigureAwait(false))
                            {
                                // Make sure the dialogue window was not canceled.
                                if (await frmPickAttribute.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                {
                                    throw new AbortedException();
                                }

                                strSelected = frmPickAttribute.MyForm.SelectedAttribute;
                            }

                            break;
                        }
                }

                SelectedValue = strSelected;

                await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Attributelevel, _strUnique, value, token: token).ConfigureAwait(false);
            }
            else
            {
                Log.Error(new object[] { "attributelevel", bonusNode.OuterXmlViaPool(token) });
            }
        }

        public async Task skilllevel(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSkill = string.Empty;
            int intValue = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref intValue);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkill))
            {
                await CreateImprovementAsync(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillLevel, _strUnique, intValue, token: token).ConfigureAwait(false);
            }
            else if (bonusNode["selectskill"] != null)
            {
                strSkill = (await ImprovementManager.DoSelectSkillAsync(bonusNode["selectskill"], _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false)).Item1;
                SelectedValue = strSkill;
                await CreateImprovementAsync(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillLevel, _strUnique, intValue, token: token).ConfigureAwait(false);
            }
            else
            {
                Log.Error(new object[] { "skilllevel", bonusNode.OuterXmlViaPool(token) });
            }
        }

        public async Task pushtext(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strPush = bonusNode.InnerTextViaPool(token);
            if (!string.IsNullOrWhiteSpace(strPush))
            {
                (await _objCharacter.GetPushTextAsync(token).ConfigureAwait(false)).Push(strPush);
            }
        }

        public Task pushtextforqualitygroup(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            
            string strPush = bonusNode.InnerTextViaPool(token);
            string strQualityGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token) ?? string.Empty;
            
            if (!string.IsNullOrWhiteSpace(strPush) && !string.IsNullOrEmpty(strQualityGroup))
            {
                return _objCharacter.PushTextForQualityGroupAsync(strQualityGroup, strPush, token);
            }
            return Task.CompletedTask;
        }

        public async Task activesoft(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            bool blnKnowledgeSkill = false;
            if (string.IsNullOrEmpty(strSelectedValue))
                (strSelectedValue, blnKnowledgeSkill) = await ImprovementManager
                    .DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false);
            if (blnKnowledgeSkill)
                throw new AbortedException();
            SelectedValue = strSelectedValue;
            string strVal = bonusNode["val"]?.InnerTextViaPool(token);
            (bool blnIsExotic, string strExoticSkillName)
                = await ExoticSkill.IsExoticSkillNameTupleAsync(_objCharacter, strSelectedValue, token).ConfigureAwait(false);
            if (blnIsExotic)
            {
                if (!string.IsNullOrEmpty(strVal))
                {
                    // Make sure we have the exotic skill in the list if we're adding an activesoft
                    Skill objExistingSkill = await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(strSelectedValue, token).ConfigureAwait(false);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedValue;
                        int intParenthesesIndex = strExoticSkillName.Length - 1 + strSkillName.TrimStartOnce(strExoticSkillName, true).IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex >= strExoticSkillName.Length)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).AddExoticSkillAsync(strExoticSkillName, strSkillSpecific, token).ConfigureAwait(false);
                        }
                    }
                    await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Activesoft,
                        _strUnique,
                        await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }
            else if (!string.IsNullOrEmpty(strVal))
            {
                await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Activesoft,
                    _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }

            if (bonusNode["addknowledge"] != null)
            {
                KnowledgeSkill objKnowledgeSkill = new KnowledgeSkill(_objCharacter, strSelectedValue, false);
                try
                {
                    await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowsoftSkillsAsync(token).ConfigureAwait(false)).AddAsync(objKnowledgeSkill, token).ConfigureAwait(false);
                    if (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillsoftAccess, token: token).ConfigureAwait(false) > 0)
                    {
                        await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).AddAsync(objKnowledgeSkill, token).ConfigureAwait(false);
                    }

                    await CreateImprovementAsync(objKnowledgeSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                catch
                {
                    await objKnowledgeSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }
        }

        public async Task skillsoft(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strForcedValue = ForcedValue;

            SelectedValue = string.IsNullOrEmpty(strForcedValue)
                ? (await ImprovementManager.DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, true, token).ConfigureAwait(false)).Item1
                : strForcedValue;

            string strVal = bonusNode["val"]?.InnerTextViaPool(token);

            KnowledgeSkill objSkill = new KnowledgeSkill(_objCharacter, SelectedValue, false);
            try
            {
                await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowsoftSkillsAsync(token).ConfigureAwait(false)).AddAsync(objSkill, token).ConfigureAwait(false);
                if (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillsoftAccess, token: token).ConfigureAwait(false) > 0)
                {
                    await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).AddAsync(objSkill, token).ConfigureAwait(false);
                }

                await CreateImprovementAsync(objSkill.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillsoft, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            catch
            {
                await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        public async Task knowledgeskilllevel(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            decimal decVal = bonusNode["val"] != null ? await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"].InnerTextViaPool(token), _intRating, token).ConfigureAwait(false) : 1;
            if (bonusNode["selectskill"] != null)
            {
                string strSkill = (await ImprovementManager.DoSelectSkillAsync(bonusNode["selectskill"], _objCharacter, _intRating, _strFriendlyName, true, token).ConfigureAwait(false)).Item1;
                SelectedValue = strSkill;
                await CreateImprovementAsync(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillLevel, _strUnique, (int)decVal.StandardRound(), token: token).ConfigureAwait(false);
            }
            else
            {
                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique, decVal, token: token).ConfigureAwait(false);
            }
        }

        public async Task knowledgeskillpoints(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeKnowledgeSkills, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode.Value, _intRating, token).ConfigureAwait(false), token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task skillgrouplevel(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSkillGroup = string.Empty;
            int value = 1;
            bonusNode.TryGetInt32FieldQuickly("val", ref value);
            if (bonusNode.TryGetStringFieldQuickly("name", ref strSkillGroup))
            {
                await CreateImprovementAsync(strSkillGroup, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroupLevel, _strUnique, value, token: token).ConfigureAwait(false);
            }
            else if (bonusNode["selectskillgroup"] != null)
            {
                strSkillGroup = await ImprovementManager.DoSelectSkillGroupAsync(bonusNode["selectskillgroup"], _objCharacter, _strFriendlyName, token).ConfigureAwait(false);
                SelectedValue = strSkillGroup;
                await CreateImprovementAsync(strSkillGroup, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroupLevel, _strUnique, value, token: token).ConfigureAwait(false);
            }
            else
            {
                Log.Error(new object[] { "skillgrouplevel", bonusNode.OuterXmlViaPool(token) });
            }
        }

        // Change the maximum number of BP that can be spent on Nuyen.
        public async Task nuyenmaxbp(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NuyenMaxBP, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Apply a bonus/penalty to physical limit.
        public async Task physicallimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync("Physical", _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalLimit,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Apply a bonus/penalty to mental limit.
        public async Task mentallimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync("Mental", _objImprovementSource, SourceName, Improvement.ImprovementType.MentalLimit,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Apply a bonus/penalty to social limit.
        public async Task sociallimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync("Social", _objImprovementSource, SourceName, Improvement.ImprovementType.SocialLimit,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
        public async Task nuyenamt(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty;
            await CreateImprovementAsync(strCondition, _objImprovementSource, SourceName, Improvement.ImprovementType.Nuyen, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Improve Condition Monitors.
        public async Task conditionmonitor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strTemp = bonusNode["physical"]?.InnerTextViaPool(token);
            // Physical Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCM, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            strTemp = bonusNode["stun"]?.InnerTextViaPool(token);
            // Stun Condition.
            if (!string.IsNullOrEmpty(strTemp))
            {
                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCM, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }

            // Condition Monitor Threshold.
            XmlElement objNode = bonusNode["threshold"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, objNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }

            // Condition Monitor Threshold Offset. (Additional boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["thresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMThresholdOffset,
                    strUseUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, objNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            // Condition Monitor Threshold Offset that must be shared between the two. (Additional boxes appear before the FIRST Condition Monitor penalty)
            objNode = bonusNode["sharedthresholdoffset"];
            if (objNode != null)
            {
                string strUseUnique = _strUnique;
                string strPrecendenceString = objNode.Attributes["precedence"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strPrecendenceString))
                    strUseUnique = "precedence" + strPrecendenceString;

                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMSharedThresholdOffset,
                    strUseUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, objNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }

            // Condition Monitor Overflow.
            objNode = bonusNode["overflow"];
            if (objNode != null)
            {
                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CMOverflow, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, objNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        // Improve Living Personal Attributes.
        public async Task livingpersona(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Device Rating.
            string strBonus = bonusNode["devicerating"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDeviceRating, _strUnique, token: token).ConfigureAwait(false);
            }

            // Program Limit.
            strBonus = bonusNode["programlimit"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaProgramLimit, _strUnique, token: token).ConfigureAwait(false);
            }

            // Attack.
            strBonus = bonusNode["attack"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaAttack, _strUnique, token: token).ConfigureAwait(false);
            }

            // Sleaze.
            strBonus = bonusNode["sleaze"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaSleaze, _strUnique, token: token).ConfigureAwait(false);
            }

            // Data Processing.
            strBonus = bonusNode["dataprocessing"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaDataProcessing, _strUnique, token: token).ConfigureAwait(false);
            }

            // Firewall.
            strBonus = bonusNode["firewall"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaFirewall, _strUnique, token: token).ConfigureAwait(false);
            }

            // Matrix CM.
            strBonus = bonusNode["matrixcm"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strBonus))
            {
                strBonus = strBonus.ProcessFixedValuesString(_intRating);
                strBonus = strBonus.Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (int.TryParse(strBonus, out int intTemp) && intTemp > 0)
                    strBonus = "+" + strBonus;
                await CreateImprovementAsync(strBonus, _objImprovementSource, SourceName, Improvement.ImprovementType.LivingPersonaMatrixCM, _strUnique, token: token).ConfigureAwait(false);
            }
        }

        // The Improvement adjusts a specific Skill.
        public async Task specificskill(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
            string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;

            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;

            string strBonusNodeName = bonusNode["name"]?.InnerTextViaPool(token);
            // Record the improvement.
            string strTemp = bonusNode["bonus"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
            {
                await CreateImprovementAsync(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false), 1, 0, 0, 0,
                    0, string.Empty, blnAddToRating, string.Empty, strCondition, token).ConfigureAwait(false);
            }
            if (bonusNode["disablespecializationeffects"] != null)
            {
                await CreateImprovementAsync(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.DisableSpecializationEffects,
                    strUseUnique, 0, 1, 0, 0, 0, 0, string.Empty, false, string.Empty, strCondition, token).ConfigureAwait(false);
            }

            strTemp = bonusNode["max"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
            {
                await CreateImprovementAsync(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, await ImprovementManager.ValueToIntAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false), 0,
                    0,
                    string.Empty, blnAddToRating, string.Empty, strCondition, token).ConfigureAwait(false);
            }
            strTemp = bonusNode["misceffect"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strTemp))
            {
                await CreateImprovementAsync(strBonusNodeName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, 0, 0,
                    0, string.Empty, false, strTemp, strCondition, token).ConfigureAwait(false);
            }
        }

        public Task reflexrecorderoptimization(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ReflexRecorderOptimization, _strUnique, token: token);
        }

        public Task removeskillcategorydefaultpenalty(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            // Expected values are either a Skill Name or an empty string.
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillCategoryDefaultPenalty, _strUnique, token: token);
        }

        public Task removeskillgroupdefaultpenalty(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            // Expected values are either a Skill Name or an empty string.
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillGroupDefaultPenalty, _strUnique, token: token);
        }

        public Task removeskilldefaultpenalty(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            // Expected values are either a Skill Name or an empty string.
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.RemoveSkillDefaultPenalty, _strUnique, token: token);
        }

        // The Improvement adds a martial art
        public async Task martialart(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlArt = (await _objCharacter.LoadDataAsync("martialarts.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/martialarts/martialart", bonusNode.InnerTextViaPool(token));

            MartialArt objMartialArt = new MartialArt(_objCharacter);
            try
            {
                await objMartialArt.CreateAsync(objXmlArt, token).ConfigureAwait(false);
                objMartialArt.IsQuality = true;
                await (await _objCharacter.GetMartialArtsAsync(token).ConfigureAwait(false)).AddAsync(objMartialArt, token).ConfigureAwait(false);

                await CreateImprovementAsync(objMartialArt.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.MartialArt,
                    _strUnique, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objMartialArt.DeleteMartialArtAsync(token: CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }

        // The Improvement adds a limit modifier
        public async Task limitmodifier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strLimit = bonusNode["limit"]?.InnerTextViaPool(token);
            decimal decBonus = await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["value"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
            string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;

            LimitModifier objLimitModifier = new LimitModifier(_objCharacter);
            objLimitModifier.Create(_strFriendlyName, decBonus.StandardRound(), strLimit, strCondition, false);
            await (await _objCharacter.GetLimitModifiersAsync(token).ConfigureAwait(false)).AddAsync(objLimitModifier, token).ConfigureAwait(false);

            await CreateImprovementAsync(objLimitModifier.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.LimitModifier,
                _strUnique, decBonus, 0, 0, 0, 0, 0, string.Empty, strCondition: strCondition, token: token).ConfigureAwait(false);
        }

        // The Improvement adjusts a Skill Category.
        public async Task skillcategory(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strName = bonusNode["name"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool(token) ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;
                await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillCategory, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token),
                        _intRating, token).ConfigureAwait(false), 1, 0,
                    0,
                    0, 0, !string.IsNullOrEmpty(strExclude) ? strExclude : string.Empty, blnAddToRating, strCondition: strCondition, token: token).ConfigureAwait(false);
            }
        }

        // The Improvement adjusts a Skill Group.
        public async Task skillgroup(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strName = bonusNode["name"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool(token) ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;
                await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillGroup, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token),
                        _intRating, token).ConfigureAwait(false), 1, 0, 0, 0,
                    0, !string.IsNullOrEmpty(strExclude) ? strExclude : string.Empty, blnAddToRating, strCondition: strCondition, token: token).ConfigureAwait(false);
            }
        }

        // The Improvement adjust Skills when used with the given CharacterAttribute.
        public async Task skillattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence", token) ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence", token);
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            string strName = bonusNode["name"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool(token) ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;
                await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillAttribute, strUseUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token),
                        _intRating, token).ConfigureAwait(false), 1,
                    0, 0, 0, 0, strExclude,
                    blnAddToRating, strCondition: strCondition, token: token).ConfigureAwait(false);
            }
        }

        // The Improvement adjust Skills whose linked attribute is the given CharacterAttribute.
        public async Task skilllinkedattribute(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence", token) ?? bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("name/@precedence", token);
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            string strName = bonusNode["name"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strName))
            {
                bool blnAddToRating = bonusNode["applytorating"]?.InnerTextIsTrueString() == true;
                string strExclude = bonusNode["exclude"]?.InnerTextViaPool(token) ?? string.Empty;
                string strCondition = bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty;
                await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.SkillLinkedAttribute, strUseUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token),
                        _intRating, token).ConfigureAwait(false), 1,
                    0, 0, 0, 0, strExclude,
                    blnAddToRating, strCondition: strCondition, token: token).ConfigureAwait(false);
            }
        }

        // The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
        public async Task skillarticulation(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EnhancedArticulation,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Armor modifiers.
        public async Task armor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Armor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Fire Armor modifiers.
        public async Task firearmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FireArmor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Cold Armor modifiers.
        public async Task coldarmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ColdArmor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Electricity Armor modifiers.
        public async Task electricityarmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ElectricityArmor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Acid Armor modifiers.
        public async Task acidarmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AcidArmor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Falling Armor modifiers.
        public async Task fallingarmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FallingArmor, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Dodge modifiers.
        public async Task dodge(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
            {
                strUseUnique = "precedence" + strPrecedence;
            }
            else
            {
                string strGroup = bonusNode.Attributes?["group"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strGroup))
                {
                    strUseUnique = "group" + strGroup;
                }
            }
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Dodge, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Reach modifiers.
        public async Task reach(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strWeapon = bonusNode.Attributes?["name"]?.InnerTextViaPool(token) ?? string.Empty;
            await CreateImprovementAsync(strWeapon, _objImprovementSource, SourceName, Improvement.ImprovementType.Reach,
                _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Unarmed Damage Value modifiers.
        public async Task unarmeddv(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDV, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Unarmed Damage Value Physical.
        public Task unarmeddvphysical(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedDVPhysical, _strUnique, token: token);
        }

        // Check for Unarmed Armor Penetration.
        public async Task unarmedap(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedAP, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Unarmed Armor Penetration.
        public async Task unarmedreach(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.UnarmedReach, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Initiative modifiers.
        public async Task initiative(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Initiative, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies. Legacy method for old characters.
        public Task initiativepass(XmlNode bonusNode, CancellationToken token = default)
        {
            return initiativedice(bonusNode, token);
        }

        // Check for Initiative Pass modifiers. Only the highest one ever applies.
        public async Task initiativedice(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = bonusNode.Name;
            string strPrecedence = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecedence))
                strUseUnique = "precedence" + strPrecedence;

            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDice,
                strUseUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies. Legacy method for old characters.
        public Task initiativepassadd(XmlNode bonusNode, CancellationToken token = default)
        {
            return initiativediceadd(bonusNode, token);
        }

        // Check for Initiative Dice modifiers. Only the highest one ever applies.
        public async Task initiativediceadd(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.InitiativeDiceAdd, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Matrix Initiative modifiers.
        public async Task matrixinitiative(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiative, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public Task matrixinitiativepass(XmlNode bonusNode, CancellationToken token = default)
        {
            return matrixinitiativedice(bonusNode, token);
        }

        // Check for Matrix Initiative Pass modifiers.
        public async Task matrixinitiativedice(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                "matrixinitiativepass", await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public Task matrixinitiativepassadd(XmlNode bonusNode, CancellationToken token = default)
        {
            return matrixinitiativediceadd(bonusNode, token);
        }

        // Check for Matrix Initiative Pass modifiers. Legacy method for old characters.
        public async Task matrixinitiativediceadd(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MatrixInitiativeDice,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task availability(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strForId = bonusNode.Attributes?["id"]?.InnerTextViaPool(token) ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty;
            await CreateImprovementAsync(strForId, _objImprovementSource, SourceName, Improvement.ImprovementType.Availability, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), strCondition: strCondition, token: token).ConfigureAwait(false);
        }

        // Check for Lifestyle cost modifiers.
        public async Task lifestylecost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strBaseLifestyle = bonusNode.Attributes?["lifestyle"]?.InnerTextViaPool(token) ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty;
            await CreateImprovementAsync(strBaseLifestyle, _objImprovementSource, SourceName, Improvement.ImprovementType.LifestyleCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), strCondition: strCondition, token: token).ConfigureAwait(false);
        }

        // Check for basic Lifestyle cost modifiers.
        public async Task basiclifestylecost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the Lifestyle node is present, we restrict to a specific lifestyle type.
            string strBaseLifestyle = bonusNode.Attributes?["lifestyle"]?.InnerTextViaPool(token) ?? string.Empty;
            string strCondition = bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty;
            await CreateImprovementAsync(strBaseLifestyle, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicLifestyleCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), strCondition: strCondition, token: token).ConfigureAwait(false);
        }

        // Check for Genetech Cost modifiers.
        public async Task genetechcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechCostMultiplier,
                _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Genetech Cost modifiers.
        public async Task genetechessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.GenetechEssMultiplier,
                _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Basic Bioware Essence Cost modifiers.
        public async Task basicbiowareessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BasicBiowareEssCost,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public async Task biowareessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public async Task biowaretotalessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Cyberware Essence Cost modifiers that stack additively with base modifiers like grade.
        public async Task cyberwareessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public async Task cyberwaretotalessmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack additively with base modifiers like grade.
        public async Task biowareessmultipliernonretroactive(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareEssCostNonRetroactive, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Non-Retroactive Bioware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public async Task biowaretotalessmultipliernonretroactive(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Non-Retroactive Cyberware Essence Cost modifiers that stack additively with base modifiers like grade.
        public async Task cyberwareessmultipliernonretroactive(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareEssCostNonRetroactive, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Non-Retroactive Cyberware Essence Cost modifiers that stack multiplicatively with base modifiers like grade.
        public async Task cyberwaretotalessmultipliernonretroactive(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Prototype Transhuman modifiers.
        public async Task prototypetranshuman(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
            await _objCharacter.ModifyPrototypeTranshumanAsync(decValue, token).ConfigureAwait(false);
            await CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.PrototypeTranshuman, _strUnique, intRating: _intRating, token: token).ConfigureAwait(false);
        }

        // Check for Friends In High Places modifiers.
        public Task friendsinhighplaces(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FriendsInHighPlaces,
                _strUnique, token: token);
        }

        // Check for ExCon modifiers.
        public Task excon(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ExCon, _strUnique, token: token);
        }

        // Check for TrustFund modifiers.
        public async Task trustfund(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.TrustFund,
                _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for MadeMan modifiers.
        public Task mademan(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MadeMan, _strUnique, token: token);
        }

        // Check for Fame modifiers.
        public Task fame(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Fame, _strUnique, token: token);
        }

        // Check for Erased modifiers.
        public Task erased(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Erased, _strUnique, token: token);
        }

        // Check for Erased modifiers.
        public Task overclocker(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Overclocker, _strUnique, token: token);
        }

        // Check for Restricted Gear modifiers.
        public async Task restrictedgear(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strValue = bonusNode["availability"]?.InnerTextViaPool(token);
            string strCount = bonusNode["amount"]?.InnerTextViaPool(token);
            if (string.IsNullOrEmpty(strCount))
            {
                strCount = "1";
                // Needed for legacy purposes when re-applying improvements
                if (string.IsNullOrEmpty(strValue))
                    strValue = bonusNode.InnerTextViaPool(token);
            }

            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName,
                Improvement.ImprovementType.RestrictedGear, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, strValue, _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, strCount, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Improvements that grant bonuses to the maximum amount of Native languages a user can have.
        public async Task nativelanguagelimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NativeLanguageLimit, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Ambidextrous modifiers.
        public Task ambidextrous(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Ambidextrous, _strUnique, token: token);
        }

        // Check for Weapon Category DV modifiers.
        public Task weaponcategorydv(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponCategoryImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponCategoryDV, token);
        }

        public Task weaponcategorydice(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponCategoryImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponCategoryDice, token);
        }

        public Task weaponcategoryap(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponCategoryImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponCategoryAP, token);
        }

        public Task weaponcategoryaccuracy(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponCategoryImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponCategoryAccuracy, token);
        }

        public Task weaponcategoryreach(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponCategoryImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponCategoryReach, token);
        }

        /// <summary>
        /// Consolidated async method for creating weapon category improvements with full WeaponCategoryDV support.
        /// </summary>
        /// <param name="bonusNode">The XML node containing the improvement data</param>
        /// <param name="improvementType">The type of improvement to create</param>
        /// <param name="token">Cancellation token</param>
        private async Task CreateWeaponCategoryImprovementAsync(XmlNode bonusNode, Improvement.ImprovementType improvementType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = string.Empty;
            decimal decValue = 0;
            if (bonusNode["selectskill"] != null)
            {
                bool blnKnowledgeSkill;
                (strSelectedValue, blnKnowledgeSkill) = await ImprovementManager.DoSelectSkillAsync(bonusNode["selectskill"], _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false);

                if (blnKnowledgeSkill)
                {
                    throw new AbortedException();
                }
                decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
            }
            else if (bonusNode["selectcategory"] != null)
            {   
                // Display the Select Category window and record which Category was selected.
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstGeneralItems))
                {
                    XmlElement xmlSelectCategory = bonusNode["selectcategory"];
                    using (XmlNodeList xmlCategoryList = xmlSelectCategory.SelectNodes("category"))
                    {
                        if (xmlCategoryList?.Count > 0)
                        {
                            foreach (XmlNode objXmlCategory in xmlCategoryList)
                            {
                                string strInnerText = objXmlCategory.InnerTextViaPool(token);
                                lstGeneralItems.Add(new ListItem(strInnerText,
                                                                 await _objCharacter.TranslateExtraAsync(
                                                                     strInnerText, GlobalSettings.Language,
                                                                     "weapons.xml", token).ConfigureAwait(false)));
                            }
                        }
                    }

                    string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                        ? string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("String_Improvement_SelectSkillNamed", token: token).ConfigureAwait(false), _strFriendlyName)
                        : await LanguageManager.GetStringAsync("Title_SelectWeaponCategory", token: token).ConfigureAwait(false);
                    using (ThreadSafeForm<SelectItem> frmPickCategory = await ThreadSafeForm<SelectItem>.GetAsync(() =>
                               new SelectItem(), token).ConfigureAwait(false))
                    {
                        await frmPickCategory.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                        frmPickCategory.MyForm.SetGeneralItemsMode(lstGeneralItems);

                        if (ForcedValue.StartsWith("Adept:", StringComparison.Ordinal)
                            || ForcedValue.StartsWith("Magician:", StringComparison.Ordinal))
                            ForcedValue = string.Empty;

                        if (!string.IsNullOrEmpty(ForcedValue))
                        {
                            frmPickCategory.MyForm.Opacity = 0;
                            frmPickCategory.MyForm.ForceItem(ForcedValue);
                        }

                        // Make sure the dialogue window was not canceled.
                        if (await frmPickCategory.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        strSelectedValue = await frmPickCategory.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                        decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, xmlSelectCategory["value"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
                    }
                }
            }
            else if (bonusNode["name"] != null)
            {
                strSelectedValue = bonusNode["name"].InnerTextViaPool(token);
                decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["bonus"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
            }
            else
            {
                Utils.BreakIfDebug();
            }

            SelectedValue = strSelectedValue;
            await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName,
                improvementType, _strUnique, decValue, token: token).ConfigureAwait(false);
        }

        public Task weaponspecificdice(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponSpecificImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponSpecificDice, token);
        }

        public Task weaponspecificdv(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponSpecificImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponSpecificDV, token);
        }

        public Task weaponspecificap(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponSpecificImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponSpecificAP, token);
        }

        public Task weaponspecificaccuracy(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponSpecificImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponSpecificAccuracy, token);
        }

        public Task weaponspecificrange(XmlNode bonusNode, CancellationToken token = default)
        {
            return CreateWeaponSpecificImprovementAsync(bonusNode, Improvement.ImprovementType.WeaponSpecificRange, token);
        }

        /// <summary>
        /// Consolidated async method for creating weapon-specific improvements.
        /// </summary>
        /// <param name="bonusNode">The XML node containing the improvement data</param>
        /// <param name="improvementType">The type of improvement to create</param>
        /// <param name="token">Cancellation token</param>
        private async Task CreateWeaponSpecificImprovementAsync(XmlNode bonusNode, Improvement.ImprovementType improvementType, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstGeneralItems))
            {
                string strType = bonusNode.Attributes?["type"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strType))
                {
                    await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).ForEachAsync(async objWeapon =>
                    {
                        if (objWeapon.RangeType == strType)
                        {
                            lstGeneralItems.Add(new ListItem(objWeapon.InternalId, await objWeapon.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                        }
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).ForEachAsync(async objWeapon => lstGeneralItems.Add(new ListItem(objWeapon.InternalId, await objWeapon.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))), token).ConfigureAwait(false);
                }

                Weapon objSelectedWeapon;
                string strDescription = !string.IsNullOrEmpty(_strFriendlyName)
                    ? string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync(
                            "String_Improvement_SelectSkillNamed", token: token).ConfigureAwait(false),
                        _strFriendlyName)
                    : await LanguageManager.GetStringAsync("Title_SelectWeapon", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectItem> frmPickWeapon = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    await frmPickWeapon.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    frmPickWeapon.MyForm.SetGeneralItemsMode(lstGeneralItems);
                    if (!string.IsNullOrEmpty(ForcedValue))
                    {
                        frmPickWeapon.MyForm.Opacity = 0;
                    }

                    frmPickWeapon.MyForm.ForceItem(ForcedValue);

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickWeapon.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    string strSelected = await frmPickWeapon.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);

                    objSelectedWeapon = await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(x => x.InternalId == strSelected, token).ConfigureAwait(false);
                    if (objSelectedWeapon == null)
                    {
                        throw new AbortedException();
                    }
                }

                SelectedValue = objSelectedWeapon.Name;
                await CreateImprovementAsync(objSelectedWeapon.InternalId, _objImprovementSource, SourceName,
                    improvementType, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        // Check for Mentor Spirit bonuses.
        public async Task selectmentorspirit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (ThreadSafeForm<SelectMentorSpirit> frmPickMentorSpirit = await ThreadSafeForm<SelectMentorSpirit>.GetAsync(() => new SelectMentorSpirit(_objCharacter)
                   {
                       ForcedMentor = ForcedValue
                   }, token).ConfigureAwait(false))
            {
                // Make sure the dialogue window was not canceled.
                if (await frmPickMentorSpirit.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }
                XmlNode xmlMentor = (await _objCharacter.LoadDataAsync("mentors.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/mentors/mentor", frmPickMentorSpirit.MyForm.SelectedMentor)
                                    ?? throw new AbortedException();
                SelectedValue = xmlMentor["name"]?.InnerTextViaPool(token) ?? string.Empty;

                string strHoldValue = SelectedValue;

                string strForce = ForcedValue;
                MentorSpirit objMentor = new MentorSpirit(_objCharacter);
                try
                {
                    await objMentor.CreateAsync(xmlMentor, Improvement.ImprovementType.MentorSpirit, string.Empty,
                        frmPickMentorSpirit.MyForm.Choice1, frmPickMentorSpirit.MyForm.Choice2, token).ConfigureAwait(false);
                    if (objMentor.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    await _objCharacter.MentorSpirits.AddAsync(objMentor, token).ConfigureAwait(false);
                }
                catch
                {
                    await objMentor.DisposeAsync().ConfigureAwait(false);
                    throw;
                }

                ForcedValue = strForce;
                SelectedValue = strHoldValue;
                await CreateImprovementAsync(objMentor.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.MentorSpirit,
                    frmPickMentorSpirit.MyForm.SelectedMentor, token: token).ConfigureAwait(false);
            }
        }

        // Check for Paragon bonuses.
        public async Task selectparagon(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (ThreadSafeForm<SelectMentorSpirit> frmPickMentorSpirit = await ThreadSafeForm<SelectMentorSpirit>.GetAsync(() => new SelectMentorSpirit(_objCharacter, "paragons.xml")
                   {
                       ForcedMentor = ForcedValue
                   }, token).ConfigureAwait(false))
            {
                // Make sure the dialogue window was not canceled.
                if (await frmPickMentorSpirit.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                XmlNode xmlMentor = (await _objCharacter.LoadDataAsync("paragons.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/mentors/mentor", frmPickMentorSpirit.MyForm.SelectedMentor)
                                    ?? throw new AbortedException();
                SelectedValue = xmlMentor["name"]?.InnerTextViaPool(token) ?? string.Empty;

                string strHoldValue = SelectedValue;

                string strForce = ForcedValue;
                MentorSpirit objMentor = new MentorSpirit(_objCharacter);
                try
                {
                    await objMentor.CreateAsync(xmlMentor, Improvement.ImprovementType.Paragon, string.Empty,
                        frmPickMentorSpirit.MyForm.Choice1, frmPickMentorSpirit.MyForm.Choice2, token).ConfigureAwait(false);
                    if (objMentor.InternalId.IsEmptyGuid())
                        throw new AbortedException();
                    await _objCharacter.MentorSpirits.AddAsync(objMentor, token).ConfigureAwait(false);
                }
                catch
                {
                    await objMentor.DisposeAsync().ConfigureAwait(false);
                    throw;
                }

                ForcedValue = strForce;
                SelectedValue = strHoldValue;
                await CreateImprovementAsync(objMentor.InternalId, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Paragon, frmPickMentorSpirit.MyForm.SelectedMentor, token: token).ConfigureAwait(false);
            }
        }

        // Check for Smartlink bonus.
        public async Task smartlink(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Smartlink,
                "smartlink", await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Adapsin bonus.
        public Task adapsin(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Adapsin, "adapsin", token: token);
        }

        // Check for SoftWeave bonus.
        public Task softweave(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SoftWeave, "softweave", token: token);
        }

        // Check for bonus that removes the ability to take any bioware (e.g. Sensitive System)
        public Task disablebioware(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBioware,
                "disablebioware", token: token);
        }

        // Check for bonus that removes the ability to take any cyberware.
        public Task disablecyberware(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberware,
                "disablecyberware", token: token);
        }

        // Check for bonus that removes access to certain bioware grades (e.g. Cyber-Snob)
        public Task disablebiowaregrade(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            string strGradeName = bonusNode.InnerTextViaPool(token);
            return CreateImprovementAsync(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableBiowareGrade,
                "disablebiowaregrade", token: token);
        }

        // Check for bonus that removes access to certain cyberware grades (e.g. Regeneration critter power).
        public Task disablecyberwaregrade(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            string strGradeName = bonusNode.InnerTextViaPool(token);
            return CreateImprovementAsync(strGradeName, _objImprovementSource, SourceName, Improvement.ImprovementType.DisableCyberwareGrade,
                "disablecyberwaregrade", token: token);
        }

        // Check for increases to walk multiplier.
        public async Task walkmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplier, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
                strTemp = bonusNode["percent"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.WalkMultiplierPercent, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
            }
        }

        // Check for increases to run multiplier.
        public async Task runmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplier, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
                strTemp = bonusNode["percent"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.RunMultiplierPercent, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
            }
        }

        // Check for increases to distance sprinted per hit.
        public async Task sprintbonus(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strCategory = bonusNode["category"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strCategory))
            {
                string strTemp = bonusNode["val"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonus, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
                strTemp = bonusNode["percent"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strTemp))
                {
                    decimal decValue = await ImprovementManager.ValueToDecAsync(_objCharacter, strTemp, _intRating, token).ConfigureAwait(false);
                    if (decValue != 0.0m)
                        await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, Improvement.ImprovementType.SprintBonusPercent, _strUnique,
                            decValue, token: token).ConfigureAwait(false);
                }
            }
        }

        // Check for free Positive Qualities.
        public async Task freepositivequalities(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreePositiveQualities, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for free Negative Qualities.
        public async Task freenegativequalities(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeNegativeQualities, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Select Side.
        public async Task selectside(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strDescription = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("Label_SelectSide", token: token).ConfigureAwait(false), _strFriendlyName);
            using (ThreadSafeForm<SelectSide> frmPickSide = await ThreadSafeForm<SelectSide>.GetAsync(() => new SelectSide
                   {
                       Description = strDescription
                   }, token).ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(ForcedValue))
                    frmPickSide.MyForm.ForceValue(ForcedValue);
                // Make sure the dialogue window was not canceled.
                else if (await frmPickSide.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                SelectedValue = frmPickSide.MyForm.SelectedSide;
            }
        }

        // Check for Free Spirit Power Points.
        public async Task freespiritpowerpoints(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Adept Power Points.
        public async Task adeptpowerpoints(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AdeptPowerPoints, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Adept Powers
        public async Task specificpower(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (await _objCharacter.GetAdeptEnabledAsync(token).ConfigureAwait(false))
            {
                ForcedValue = string.Empty;

                string strPowerName = bonusNode["name"]?.InnerTextViaPool(token);

                if (!string.IsNullOrEmpty(strPowerName))
                {
                    Power objBoostedPower;
                    XmlNode objXmlPower = (await _objCharacter.LoadDataAsync("powers.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/powers/power", strPowerName);
                    // Check if the character already has this power
                    Power objNewPower = new Power(_objCharacter);
                    try
                    {
                        if (!await objNewPower.CreateAsync(objXmlPower, 0, bonusNode["bonusoverride"], token: token).ConfigureAwait(false))
                            throw new AbortedException();
                        string strNewPowerName = await objNewPower.GetNameAsync(token).ConfigureAwait(false);
                        string strNewPowerExtra = await objNewPower.GetExtraAsync(token).ConfigureAwait(false);
                        objBoostedPower = await _objCharacter.Powers.FirstOrDefaultAsync(async objPower => await objPower.GetNameAsync(token).ConfigureAwait(false) == strNewPowerName && await objPower.GetExtraAsync(token).ConfigureAwait(false) == strNewPowerExtra, token: token).ConfigureAwait(false);
                    }
                    catch
                    {
                        await objNewPower.DeletePowerAsync(token).ConfigureAwait(false);
                        throw;
                    }
                    if (objBoostedPower == null)
                    {
                        await (await _objCharacter.GetPowersAsync(token).ConfigureAwait(false)).AddAsync(objNewPower, token).ConfigureAwait(false);
                        objBoostedPower = objNewPower;
                    }

                    int.TryParse(bonusNode["val"]?.InnerTextViaPool(token), NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intLevels);
                    if (!objBoostedPower.LevelsEnabled)
                        intLevels = 1;
                    await CreateImprovementAsync(objNewPower.Name, _objImprovementSource, SourceName,
                        !string.IsNullOrWhiteSpace(bonusNode["pointsperlevel"]?.InnerTextViaPool(token))
                            ? Improvement.ImprovementType.AdeptPowerFreePoints
                            : Improvement.ImprovementType.AdeptPowerFreeLevels, objNewPower.Extra, 0,
                        intLevels, token: token).ConfigureAwait(false);

                    // fix: refund power points, if bonus would make power exceed maximum
                    int intMaximumLevels = await objBoostedPower.GetTotalMaximumLevelsAsync(token).ConfigureAwait(false);
                    if (intMaximumLevels < await objBoostedPower.GetRatingAsync(token).ConfigureAwait(false) + intLevels)
                    {
                        await objBoostedPower.SetRatingAsync(Math.Max(intMaximumLevels - intLevels, 0), token).ConfigureAwait(false);
                    }

                    await objBoostedPower.OnPropertyChangedAsync(nameof(Power.FreeLevels), token).ConfigureAwait(false);
                }
            }
        }

        // Select a Power.
        public async Task selectpowers(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // If the character isn't an adept or mystic adept, skip the rest of this.
            if (await _objCharacter.GetAdeptEnabledAsync(token).ConfigureAwait(false))
            {
                using (XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower"))
                {
                    if (objXmlPowerList != null)
                    {
                        XmlDocument xmlDocument = await _objCharacter.LoadDataAsync("powers.xml", token: token).ConfigureAwait(false);
                        foreach (XmlNode objNode in objXmlPowerList)
                        {
                            XmlNode objXmlPower;
                            int intLevels = await ImprovementManager.ValueToIntAsync(_objCharacter, objNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false);
                            string strPointsPerLevel = objNode["pointsperlevel"]?.InnerTextViaPool(token);
                            // Display the Select Power window and record which Power was selected.
                            using (ThreadSafeForm<SelectPower> frmPickPower = await ThreadSafeForm<SelectPower>.GetAsync(() => new SelectPower(_objCharacter), token).ConfigureAwait(false))
                            {
                                frmPickPower.MyForm.ForBonus = true;
                                frmPickPower.MyForm.IgnoreLimits = objNode["ignorerating"]?.InnerTextIsTrueString() == true;

                                if (!string.IsNullOrEmpty(strPointsPerLevel))
                                    frmPickPower.MyForm.PointsPerLevel = await ImprovementManager.ValueToDecAsync(_objCharacter, strPointsPerLevel, _intRating, token).ConfigureAwait(false);
                                string strLimit = objNode["limit"]?.InnerTextViaPool(token).Replace("Rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                                if (!string.IsNullOrEmpty(strLimit))
                                    frmPickPower.MyForm.LimitToRating = await ImprovementManager.ValueToIntAsync(_objCharacter, strLimit, _intRating, token).ConfigureAwait(false);
                                string strLimitToPowers = objNode.Attributes?["limittopowers"]?.InnerTextViaPool(token);
                                if (!string.IsNullOrEmpty(strLimitToPowers))
                                    frmPickPower.MyForm.LimitToPowers = strLimitToPowers;

                                // Make sure the dialogue window was not canceled.
                                if (await frmPickPower.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                {
                                    throw new AbortedException();
                                }

                                objXmlPower = xmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickPower.MyForm.SelectedPower)
                                              ?? throw new AbortedException();
                            }

                            bool blnHasPower;
                            string strName;
                            string strExtra;
                            // If no, add the power and mark it free or give it free levels
                            Power objNewPower = new Power(_objCharacter);
                            try
                            {
                                if (!await objNewPower.CreateAsync(objXmlPower, 0, bonusNode["bonusoverride"], token: token).ConfigureAwait(false))
                                    throw new AbortedException();

                                SelectedValue = await objNewPower.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                strName = await objNewPower.GetNameAsync(token).ConfigureAwait(false);
                                strExtra = await objNewPower.GetExtraAsync(token).ConfigureAwait(false);
                                blnHasPower = await (await _objCharacter.GetPowersAsync(token).ConfigureAwait(false)).AnyAsync(
                                    async objPower => await objPower.GetNameAsync(token).ConfigureAwait(false) == strName && await objPower.GetExtraAsync(token).ConfigureAwait(false) == strExtra, token: token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objNewPower.DeletePowerAsync(CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }

                            if (!blnHasPower)
                            {
                                await (await _objCharacter.GetPowersAsync(token).ConfigureAwait(false)).AddAsync(objNewPower, token).ConfigureAwait(false);
                            }
                            else
                            {
                                // Another copy of the power already exists, so we ensure that we remove any improvements created by the power because we're discarding it.
                                await objNewPower.DeletePowerAsync(token).ConfigureAwait(false);
                            }

                            await CreateImprovementAsync(strName, _objImprovementSource, SourceName,
                                !string.IsNullOrWhiteSpace(strPointsPerLevel)
                                    ? Improvement.ImprovementType.AdeptPowerFreePoints
                                    : Improvement.ImprovementType.AdeptPowerFreeLevels, strExtra, 0,
                                intLevels, token: token).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        // Check for Armor Encumbrance Penalty.
        public async Task armorencumbrancepenalty(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task addart(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlSelectedArt = (await _objCharacter.LoadDataAsync("metamagic.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/arts/art", bonusNode.InnerTextViaPool(token));

            // Makes sure we aren't over our limits for this particular metamagic from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                (objXmlSelectedArt != null && await objXmlSelectedArt.CreateNavigator().RequirementsMetAsync(_objCharacter, strLocalName: _strFriendlyName, token: token).ConfigureAwait(false)))
            {
                Art objAddArt = new Art(_objCharacter);
                await objAddArt.CreateAsync(objXmlSelectedArt, Improvement.ImprovementSource.Metamagic, token).ConfigureAwait(false);
                objAddArt.Grade = -1;
                if (objAddArt.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                await _objCharacter.Arts.AddAsync(objAddArt, token).ConfigureAwait(false);
                await CreateImprovementAsync(objAddArt.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Art, _strUnique, token: token).ConfigureAwait(false);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public async Task selectart(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("metamagic.xml", token: token).ConfigureAwait(false);
            XmlNode objXmlSelectedArt;
            using (XmlNodeList xmlArtList = bonusNode.SelectNodes("art"))
            {
                if (xmlArtList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstArts))
                    {
                        foreach (XmlNode objXmlAddArt in xmlArtList)
                        {
                            string strLoopName = objXmlAddArt.InnerTextViaPool(token);
                            XmlNode objXmlArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/arts/art", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlArt != null && await objXmlAddArt.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                            {
                                lstArts.Add(new ListItem(objXmlArt["id"]?.InnerTextViaPool(token),
                                    objXmlArt["translate"]?.InnerTextViaPool(token) ?? strLoopName));
                            }
                        }

                        if (lstArts.Count == 0)
                        {
                            await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                                SourceName), token: token).ConfigureAwait(false);
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstArts);
                            // Don't do anything else if the form was canceled.
                            if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                throw new AbortedException();

                            objXmlSelectedArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false))
                                                ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = objXmlSelectedArt["name"]?.InnerTextViaPool(token);
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                }
                else
                {
                    using (ThreadSafeForm<SelectArt> frmPickArt = await ThreadSafeForm<SelectArt>.GetAsync(() => new SelectArt(_objCharacter, SelectArt.Mode.Art), token).ConfigureAwait(false))
                    {
                        // Don't do anything else if the form was canceled.
                        if (await frmPickArt.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            throw new AbortedException();

                        objXmlSelectedArt = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", frmPickArt.MyForm.SelectedItem)
                                            ?? throw new AbortedException();
                    }
                }
            }

            Art objAddArt = new Art(_objCharacter);
            await objAddArt.CreateAsync(objXmlSelectedArt, Improvement.ImprovementSource.Metamagic, token).ConfigureAwait(false);
            objAddArt.Grade = -1;
            if (objAddArt.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = await objAddArt.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);

            await (await _objCharacter.GetArtsAsync(token).ConfigureAwait(false)).AddAsync(objAddArt, token).ConfigureAwait(false);
            await CreateImprovementAsync(objAddArt.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Art, _strUnique, token: token).ConfigureAwait(false);
        }

        public async Task addmetamagic(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode objXmlSelectedMetamagic = (await _objCharacter.LoadDataAsync("metamagic.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/metamagics/metamagic", bonusNode.InnerTextViaPool(token));
            // Makes sure we aren't over our limits for this particular metamagic from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                (objXmlSelectedMetamagic != null && await objXmlSelectedMetamagic.CreateNavigator()
                    .RequirementsMetAsync(_objCharacter, strLocalName: _strFriendlyName, token: token).ConfigureAwait(false)))
            {
                string strForcedValue = bonusNode.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;
                Metamagic objAddMetamagic = new Metamagic(_objCharacter);
                await objAddMetamagic.CreateAsync(objXmlSelectedMetamagic, Improvement.ImprovementSource.Metamagic, strForcedValue, token).ConfigureAwait(false);
                objAddMetamagic.Grade = -1;
                if (objAddMetamagic.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                await _objCharacter.Metamagics.AddAsync(objAddMetamagic, token).ConfigureAwait(false);
                await CreateImprovementAsync(objAddMetamagic.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Metamagic, _strUnique, token: token).ConfigureAwait(false);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public async Task selectmetamagic(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("metamagic.xml", token: token).ConfigureAwait(false);
            string strForceValue = string.Empty;
            XmlNode objXmlSelectedMetamagic;
            using (XmlNodeList xmlMetamagicList = bonusNode.SelectNodes("metamagic"))
            {
                if (xmlMetamagicList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstMetamagics))
                    {
                        foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                        {
                            string strLoopName = objXmlAddMetamagic.InnerTextViaPool(token);
                            XmlNode objXmlMetamagic
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlMetamagic != null && await objXmlAddMetamagic.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                            {
                                lstMetamagics.Add(new ListItem(objXmlMetamagic["id"]?.InnerTextViaPool(token),
                                    objXmlMetamagic["translate"]?.InnerTextViaPool(token)
                                    ?? strLoopName));
                            }
                        }

                        if (lstMetamagics.Count == 0)
                        {
                            await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                                SourceName), token: token).ConfigureAwait(false);
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstMetamagics);
                            // Don't do anything else if the form was canceled.
                            if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                throw new AbortedException();

                            objXmlSelectedMetamagic = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false))
                                                      ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = objXmlSelectedMetamagic["name"]?.InnerTextViaPool(token);
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                    foreach (XmlNode objXmlAddMetamagic in xmlMetamagicList)
                    {
                        if (strSelectedName == objXmlAddMetamagic.InnerTextViaPool(token))
                        {
                            strForceValue = objXmlAddMetamagic.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;
                            break;
                        }
                    }
                }
                else
                {
                    InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1 };
                    using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic = await ThreadSafeForm<SelectMetamagic>.GetAsync(() => new SelectMetamagic(_objCharacter, objGrade), token).ConfigureAwait(false))
                    {
                        // Don't do anything else if the form was canceled.
                        if (await frmPickMetamagic.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            throw new AbortedException();

                        objXmlSelectedMetamagic = objXmlDocument.TryGetNodeByNameOrId("/chummer/metamagics/metamagic", frmPickMetamagic.MyForm.SelectedMetamagic)
                                                  ?? throw new AbortedException();
                    }
                }
            }

            Metamagic objAddMetamagic = new Metamagic(_objCharacter);
            await objAddMetamagic.CreateAsync(objXmlSelectedMetamagic, Improvement.ImprovementSource.Metamagic, strForceValue, token).ConfigureAwait(false);
            objAddMetamagic.Grade = -1;
            if (objAddMetamagic.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = await objAddMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);

            await (await _objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false)).AddAsync(objAddMetamagic, token).ConfigureAwait(false);
            await CreateImprovementAsync(objAddMetamagic.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Metamagic, _strUnique, token: token).ConfigureAwait(false);
        }

        public async Task addecho(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("echoes.xml", token: token).ConfigureAwait(false);
            XmlNode objXmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", bonusNode.InnerTextViaPool(token));

            // Makes sure we aren't over our limits for this particular echo from this overall source
            if (bonusNode.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                (objXmlSelectedEcho != null && await objXmlSelectedEcho.CreateNavigator().RequirementsMetAsync(_objCharacter, strLocalName: _strFriendlyName, token: token).ConfigureAwait(false)))
            {
                string strForceValue = bonusNode.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;
                Metamagic objAddEcho = new Metamagic(_objCharacter);
                await objAddEcho.CreateAsync(objXmlSelectedEcho, Improvement.ImprovementSource.Echo, strForceValue, token).ConfigureAwait(false);
                objAddEcho.Grade = -1;
                if (objAddEcho.InternalId.IsEmptyGuid())
                    throw new AbortedException();

                await _objCharacter.Metamagics.AddAsync(objAddEcho, token).ConfigureAwait(false);
                await CreateImprovementAsync(objAddEcho.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Echo, _strUnique, token: token).ConfigureAwait(false);
            }
            else
            {
                throw new AbortedException();
            }
        }

        public async Task selectecho(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("echoes.xml", token: token).ConfigureAwait(false);
            string strForceValue = string.Empty;
            XmlNode xmlSelectedEcho;
            using (XmlNodeList xmlEchoList = bonusNode.SelectNodes("echo"))
            {
                if (xmlEchoList?.Count > 0)
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstEchoes))
                    {
                        foreach (XmlNode objXmlAddEcho in xmlEchoList)
                        {
                            string strLoopName = objXmlAddEcho.InnerTextViaPool(token);
                            XmlNode objXmlEcho = objXmlDocument.TryGetNodeByNameOrId(
                                "/chummer/metamagics/metamagic", strLoopName);
                            // Makes sure we aren't over our limits for this particular metamagic from this overall source
                            if (objXmlEcho != null && await objXmlAddEcho.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                            {
                                lstEchoes.Add(new ListItem(objXmlEcho["id"]?.InnerTextViaPool(token),
                                    objXmlEcho["translate"]?.InnerTextViaPool(token) ?? strLoopName));
                            }
                        }

                        if (lstEchoes.Count == 0)
                        {
                            await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                                SourceName), token: token).ConfigureAwait(false);
                            throw new AbortedException();
                        }

                        using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                        {
                            frmPickItem.MyForm.SetGeneralItemsMode(lstEchoes);
                            // Don't do anything else if the form was canceled.
                            if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                throw new AbortedException();

                            xmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false))
                                              ?? throw new AbortedException();
                        }
                    }

                    string strSelectedName = xmlSelectedEcho["name"]?.InnerTextViaPool(token);
                    if (string.IsNullOrEmpty(strSelectedName))
                        throw new AbortedException();
                    foreach (XmlNode objXmlAddEcho in xmlEchoList)
                    {
                        if (strSelectedName == objXmlAddEcho.InnerTextViaPool(token))
                        {
                            strForceValue = objXmlAddEcho.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;
                            break;
                        }
                    }
                }
                else
                {
                    InitiationGrade objGrade = new InitiationGrade(_objCharacter) { Grade = -1, Technomancer = true };
                    using (ThreadSafeForm<SelectMetamagic> frmPickMetamagic = await ThreadSafeForm<SelectMetamagic>.GetAsync(() => new SelectMetamagic(_objCharacter, objGrade), token).ConfigureAwait(false))
                    {
                        // Don't do anything else if the form was canceled.
                        if (await frmPickMetamagic.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            throw new AbortedException();

                        xmlSelectedEcho = objXmlDocument.TryGetNodeByNameOrId("/chummer/echoes/echo", frmPickMetamagic.MyForm.SelectedMetamagic)
                                          ?? throw new AbortedException();
                    }
                }
            }

            Metamagic objAddEcho = new Metamagic(_objCharacter);
            await objAddEcho.CreateAsync(xmlSelectedEcho, Improvement.ImprovementSource.Echo, strForceValue, token).ConfigureAwait(false);
            objAddEcho.Grade = -1;
            if (objAddEcho.InternalId.IsEmptyGuid())
                throw new AbortedException();

            SelectedValue = await objAddEcho.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);

            await _objCharacter.Metamagics.AddAsync(objAddEcho, token).ConfigureAwait(false);
            await CreateImprovementAsync(objAddEcho.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.Echo, _strUnique, token: token).ConfigureAwait(false);
        }

        // Check for Skillwires.
        public async Task skillwire(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Skillwire,
                strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Hardwires.
        public async Task hardwires(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            if (string.IsNullOrEmpty(strSelectedValue))
                strSelectedValue = (await ImprovementManager.DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false)).Item1;
            SelectedValue = strSelectedValue;
            (bool blnIsExotic, string strExoticSkillName)
                = await ExoticSkill.IsExoticSkillNameTupleAsync(_objCharacter, strSelectedValue, token).ConfigureAwait(false);
            if (blnIsExotic)
            {
                if (!bonusNode.IsNullOrInnerTextIsEmpty())
                {
                    // Make sure we have the exotic skill in the list if we're adding an activesoft
                    Skill objExistingSkill = await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(strSelectedValue, token).ConfigureAwait(false);
                    if (objExistingSkill?.IsExoticSkill != true)
                    {
                        string strSkillName = strSelectedValue;
                        int intParenthesesIndex = strExoticSkillName.Length - 1 + strSkillName.TrimStartOnce(strExoticSkillName, true).IndexOf(" (", StringComparison.OrdinalIgnoreCase);
                        if (intParenthesesIndex >= strExoticSkillName.Length)
                        {
                            string strSkillSpecific = strSkillName.Substring(intParenthesesIndex + 2, strSkillName.Length - intParenthesesIndex - 3);
                            await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).AddExoticSkillAsync(strExoticSkillName, strSkillSpecific, token).ConfigureAwait(false);
                        }
                    }
                    await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.Hardwire,
                        _strUnique,
                        await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }
            else
            {
                await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.Hardwire,
                    strSelectedValue,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        // Check for Damage Resistance.
        public async Task damageresistance(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DamageResistance, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Judge Intentions.
        public async Task judgeintentions(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentions, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Judge Intentions (offense only, i.e. doing the judging).
        public async Task judgeintentionsoffense(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsOffense, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Judge Intentions (defense only, i.e. being judged).
        public async Task judgeintentionsdefense(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.JudgeIntentionsDefense, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Composure.
        public async Task composure(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Composure, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Lift and Carry.
        public async Task liftandcarry(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.LiftAndCarry, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Memory.
        public async Task memory(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Memory, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Fatigue Resist.
        public async Task fatigueresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FatigueResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Radiation Resist.
        public async Task radiationresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.RadiationResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Sonic Attacks Resist.
        public async Task sonicresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SonicResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Contact-vector Toxins Resist.
        public async Task toxincontactresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Ingestion-vector Toxins Resist.
        public async Task toxiningestionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Inhalation-vector Toxins Resist.
        public async Task toxininhalationresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Injection-vector Toxins Resist.
        public async Task toxininjectionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Contact-vector Pathogens Resist.
        public async Task pathogencontactresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Ingestion-vector Pathogens Resist.
        public async Task pathogeningestionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Inhalation-vector Pathogens Resist.
        public async Task pathogeninhalationresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Injection-vector Pathogens Resist.
        public async Task pathogeninjectionresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionResist, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Contact-vector Toxins Immunity.
        public Task toxincontactimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinContactImmune, _strUnique, token: token);
        }

        // Check for Ingestion-vector Toxins Immunity.
        public Task toxiningestionimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinIngestionImmune, _strUnique, token: token);
        }

        // Check for Inhalation-vector Toxins Immunity.
        public Task toxininhalationimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInhalationImmune, _strUnique, token: token);
        }

        // Check for Injection-vector Toxins Immunity.
        public Task toxininjectionimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ToxinInjectionImmune, _strUnique, token: token);
        }

        // Check for Contact-vector Pathogens Immunity.
        public Task pathogencontactimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenContactImmune, _strUnique, token: token);
        }

        // Check for Ingestion-vector Pathogens Immunity.
        public Task pathogeningestionimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenIngestionImmune, _strUnique, token: token);
        }

        // Check for Inhalation-vector Pathogens Immunity.
        public Task pathogeninhalationimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInhalationImmune, _strUnique, token: token);
        }

        // Check for Injection-vector Pathogens Immunity.
        public Task pathogeninjectionimmune(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PathogenInjectionImmune, _strUnique, token: token);
        }

        // Check for Physiological Addiction Resist if you are not addicted.
        public async Task physiologicaladdictionfirsttime(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionFirstTime, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Psychological Addiction if you are not addicted.
        public async Task psychologicaladdictionfirsttime(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionFirstTime, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Physiological Addiction Resist if you are addicted.
        public async Task physiologicaladdictionalreadyaddicted(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Psychological Addiction if you are addicted.
        public async Task psychologicaladdictionalreadyaddicted(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Recovery Dice from Stun CM Damage.
        public async Task stuncmrecovery(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StunCMRecovery, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Recovery Dice from Physical CM Damage.
        public async Task physicalcmrecovery(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PhysicalCMRecovery, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Whether Essence is added to Recovery Dice from Stun CM Damage.
        public Task addesstostuncmrecovery(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoStunCMRecovery, _strUnique, token: token);
        }

        // Check for Whether Essence is added to Recovery Dice from Physical CM Damage.
        public Task addesstophysicalcmrecovery(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AddESStoPhysicalCMRecovery, _strUnique, token: token);
        }

        // Check for Concealability.
        public async Task concealability(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Concealability, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Drain Resistance.
        public async Task drainresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainResistance, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Drain Value.
        public async Task drainvalue(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode.Attributes?["specific"]?.InnerTextViaPool(token) ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.DrainValue, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Fading Resistance.
        public async Task fadingresist(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingResistance, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Fading Value.
        public async Task fadingvalue(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode.Attributes?["specific"]?.InnerTextViaPool(token) ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FadingValue, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Notoriety.
        public async Task notoriety(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.Notoriety, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Street Cred bonuses.
        public async Task streetcred(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCred, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Street Cred Multiplier bonuses.
        public async Task streetcredmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.StreetCredMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Complex Form Limit.
        public async Task complexformlimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ComplexFormLimit, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Spell Limit.
        public async Task spelllimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpellLimit, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Free Spells.
        public async Task freespells(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlAttributeCollection objNodeAttributes = bonusNode.Attributes;
            if (objNodeAttributes != null)
            {
                string strSpellTypeLimit = objNodeAttributes["limit"]?.InnerTextViaPool(token) ?? string.Empty;
                if (objNodeAttributes["attribute"] != null)
                {
                    CharacterAttrib att = await _objCharacter.GetAttributeAsync(objNodeAttributes["attribute"].InnerTextViaPool(token), token: token).ConfigureAwait(false);
                    if (att != null)
                    {
                        await CreateImprovementAsync(att.Abbrev, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsATT, strSpellTypeLimit, token: token).ConfigureAwait(false);
                    }
                }
                else if (objNodeAttributes["skill"] != null)
                {
                    string strKey = objNodeAttributes["skill"].InnerTextViaPool(token);
                    await CreateImprovementAsync(strKey, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpellsSkill, strSpellTypeLimit, token: token).ConfigureAwait(false);
                }
                else
                {
                    await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                        await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }
            else
            {
                await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.FreeSpells, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        // Check for Spell Category bonuses.
        public Task spellcategory(XmlNode bonusNode, CancellationToken token = default)
        {
            return spellcategorydicepool(bonusNode, token);
        }

        public async Task spellcategorydicepool(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategory, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for dicepool bonuses for a specific Spell.
        public async Task spelldicepool(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["id"]?.InnerTextViaPool(token) ?? bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDicePool, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Spell Category Drain bonuses.
        public async Task spellcategorydrain(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string s = bonusNode["category"]?.InnerTextViaPool(token) ?? SelectedValue;
            if (string.IsNullOrWhiteSpace(s))
                throw new AbortedException();
            await CreateImprovementAsync(s, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDrain, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Spell Category Damage bonuses.
        public async Task spellcategorydamage(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["category"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellCategoryDamage, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Spell descriptor Damage bonuses.
        public async Task spelldescriptordamage(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["descriptor"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDescriptorDamage, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Spell descriptor drain bonuses.
        public async Task spelldescriptordrain(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["descriptor"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SpellDescriptorDrain, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Throwing Range bonuses.
        public async Task throwrange(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRange, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Throwing Range bonuses.
        public async Task throwrangestr(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowRangeSTR, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Throwing STR bonuses.
        public async Task throwstr(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ThrowSTR, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Skillsoft access.
        public async Task skillsoftaccess(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strUseUnique = _strUnique;
            string strPrecendenceString = bonusNode.Attributes?["precedence"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strPrecendenceString))
                strUseUnique = "precedence" + strPrecendenceString;
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillsoftAccess, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).AddRangeAsync(await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetKnowsoftSkillsAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        // Check for Quickening Metamagic.
        public Task quickeningmetamagic(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.QuickeningMetamagic, _strUnique, token: token);
        }

        // Check for ignore Stun CM Penalty.
        public Task ignorecmpenaltystun(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, _strUnique, token: token);
        }

        // Check for ignore Physical CM Penalty.
        public Task ignorecmpenaltyphysical(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, _strUnique, token: token);
        }

        // Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
        public Task cyborgessence(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyborgEssence, _strUnique, token: token);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public async Task essencepenalty(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenalty, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public async Task essencepenaltyt100(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyT100, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting MAG rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public async Task essencepenaltymagonlyt100(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting RES rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public async Task essencepenaltyresonlyt100(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyRESOnlyT100, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value for the purposes of affecting DEP rating (input value is 100x the actual value, so essence penalty of -0.25 would be input as "25").
        public async Task essencepenaltydeponlyt100(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssencePenaltyDEPOnlyT100, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for special attribute burn modifiers that stack additively.
        public async Task specialattburnmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialAttBurn, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for special attribute burn modifiers that stack multiplicatively.
        public async Task specialatttotalburnmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialAttTotalBurnMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
        public async Task essencemax(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EssenceMax, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Check for Select Sprite.
        public async Task selectsprite(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCritters))
            {
                using (XmlNodeList objXmlNodeList = (await _objCharacter.LoadDataAsync("critters.xml", token: token).ConfigureAwait(false))
                                                                 .SelectNodes(
                                                                     "/chummer/metatypes/metatype[contains(category, \"Sprites\")]"))
                {
                    if (objXmlNodeList != null)
                    {
                        foreach (XmlNode objXmlNode in objXmlNodeList)
                        {
                            string strName = objXmlNode["name"]?.InnerTextViaPool(token);
                            lstCritters.Add(new ListItem(strName, objXmlNode["translate"]?.InnerTextViaPool(token) ?? strName));
                        }
                    }
                }

                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstCritters);

                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                }

                await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.AddSprite,
                    _strUnique, token: token).ConfigureAwait(false);
            }
        }

        // Check for Black Market Discount.
        public async Task blackmarketdiscount(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XPathNodeIterator nodeList = (await _objCharacter.LoadDataXPathAsync("options.xml", token: token).ConfigureAwait(false)).SelectAndCacheExpression("/chummer/blackmarketpipelinecategories/category", token);
            SelectedValue = string.Empty;
            if (nodeList.Count > 0)
            {
                string strDescription = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false), _strFriendlyName);
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    frmPickItem.MyForm.SetGeneralItemsMode(nodeList.OfType<XPathNavigator>().Select(objNode =>
                        new ListItem(objNode.Value, objNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? objNode.Value)));

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickItem.MyForm.ForceItem(LimitSelection);
                        frmPickItem.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.BlackMarketDiscount,
                _strUnique, token: token).ConfigureAwait(false);
        }

        // Select Armor (Mostly used for Custom Fit (Stack)).
        public async Task selectarmor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            string strFilter = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);
            XPathNavigator objXmlDocument = await _objCharacter.LoadDataXPathAsync("armor.xml", token: token).ConfigureAwait(false);
            XPathNodeIterator objXmlNodeList;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                objXmlNodeList
                    = objXmlDocument.Select(
                        "/chummer/armors/armor[starts-with(name, " + bonusNode.InnerTextViaPool(token).CleanXPath() + ") and ("
                        + strFilter + ") and mods[name = 'Custom Fit']]");
            }
            else
            {
                objXmlNodeList =
                    objXmlDocument.Select("/chummer/armors/armor[(" + strFilter + ") and mods[name = 'Custom Fit']]");
            }

            //.SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" + strFilter + ")" + SkillFilter(filter) + "]");

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstArmors))
            {
                if (objXmlNodeList.Count > 0)
                {
                    foreach (XPathNavigator objNode in objXmlNodeList)
                    {
                        string strName = objNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                            lstArmors.Add(new ListItem(
                                              strName,
                                              objNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value
                                              ?? strName));
                    }
                }

                if (lstArmors.Count > 0)
                {
                    string strDescription = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false), _strFriendlyName);
                    using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                    {
                        await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                        frmPickItem.MyForm.SetGeneralItemsMode(lstArmors);

                        if (!string.IsNullOrEmpty(LimitSelection))
                        {
                            frmPickItem.MyForm.ForceItem(LimitSelection);
                            frmPickItem.MyForm.Opacity = 0;
                        }

                        // Make sure the dialogue window was not canceled.
                        if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                    }
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique, token: token).ConfigureAwait(false);
        }

        // Select a specific piece of Cyberware.
        public async Task selectcyberware(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            // Display the Select Item window and record the value that was entered.
            string strCategory = bonusNode["category"]?.InnerTextViaPool(token);
            string strBookXPath = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);
            XPathNodeIterator objXmlNodeList = (await _objCharacter.LoadDataXPathAsync("cyberware.xml", token: token).ConfigureAwait(false)).Select(!string.IsNullOrEmpty(strCategory)
                ? "/chummer/cyberwares/cyberware[(category = " + strCategory.CleanXPath() + ") and (" + strBookXPath + ")]"
                : "/chummer/cyberwares/cyberware[(" + strBookXPath + ")]");

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> list))
            {
                if (objXmlNodeList.Count > 0)
                {
                    foreach (XPathNavigator objNode in objXmlNodeList)
                    {
                        string strName = objNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                            list.Add(new ListItem(
                                         strName,
                                         objNode.SelectSingleNodeAndCacheExpression("@translate", token)?.Value ?? strName));
                    }
                }

                if (list.Count == 0)
                    throw new AbortedException();
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    _strFriendlyName);
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    frmPickItem.MyForm.SetGeneralItemsMode(list);

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickItem.MyForm.ForceItem(LimitSelection);
                        frmPickItem.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique, token: token).ConfigureAwait(false);
        }

        // Select Weapon (custom entry for things like Spare Clip).
        public async Task selectweapon(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            if (_objCharacter == null)
            {
                // If the character is null (this is a Vehicle), the user must enter their own string.
                // Display the Select Item window and record the value that was entered.
                string strDescription = string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                    _strFriendlyName);
                using (ThreadSafeForm<SelectText> frmPickText = await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        frmPickText.MyForm.SelectedValue = LimitSelection;
                        frmPickText.MyForm.Opacity = 0;
                    }

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickText.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = frmPickText.MyForm.SelectedValue;
                }
            }
            else
            {
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstWeapons))
                {
                    bool blnIncludeUnarmed = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@includeunarmed", token)?.Value == bool.TrueString;
                    string strExclude = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory", token)?.Value ?? string.Empty;
                    string strWeaponDetails = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@weapondetails", token)?.Value ?? string.Empty;
                    foreach (Weapon objWeapon in await _objCharacter.Weapons.GetAllDescendantsAsync(x => x.Children, token).ConfigureAwait(false))
                    {
                        if (!string.IsNullOrEmpty(strExclude) && objWeapon.RangeType == strExclude)
                            continue;
                        if (!blnIncludeUnarmed && objWeapon.Name == "Unarmed Attack")
                            continue;
                        if (!string.IsNullOrEmpty(strWeaponDetails)
                            && (await objWeapon.GetNodeXPathAsync(token: token).ConfigureAwait(false))?.SelectSingleNode("self::node()[" + strWeaponDetails + "]") == null)
                            continue;
                        lstWeapons.Add(new ListItem(objWeapon.InternalId,
                                                    await objWeapon.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false)));
                    }

                    if (string.IsNullOrWhiteSpace(LimitSelection)
                        || lstWeapons.Exists(item => item.Name == LimitSelection))
                    {
                        if (lstWeapons.Count == 0)
                        {
                            await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync(
                                    "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                                SourceName), token: token).ConfigureAwait(false);
                            throw new AbortedException();
                        }

                        string strDescription = string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync("String_Improvement_SelectText", token: token).ConfigureAwait(false),
                            _strFriendlyName);
                        using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                        {
                            await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                            frmPickItem.MyForm.SetGeneralItemsMode(lstWeapons);

                            if (!string.IsNullOrEmpty(LimitSelection))
                            {
                                frmPickItem.MyForm.ForceItem(LimitSelection);
                                frmPickItem.MyForm.Opacity = 0;
                            }

                            // Make sure the dialogue window was not canceled.
                            if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            {
                                throw new AbortedException();
                            }

                            SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        SelectedValue = LimitSelection;
                    }
                }
            }

            // Create the Improvement.
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.Text, _strUnique, token: token).ConfigureAwait(false);
        }

        // Select an Optional Power.
        public async Task optionalpowers(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Check to see if there is only one possible selection because of _strLimitSelection.
            if (!string.IsNullOrEmpty(ForcedValue))
                LimitSelection = ForcedValue;

            string strForcePower = !string.IsNullOrEmpty(LimitSelection) ? LimitSelection : string.Empty;
            int powerCount = 1;
            if (string.IsNullOrEmpty(strForcePower) && bonusNode.Attributes?["count"] != null)
            {
                string strCount = bonusNode.Attributes?["count"]?.InnerTextViaPool(token);
                if (strCount.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strCount = await _objCharacter.ProcessAttributesInXPathAsync(strCount, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCount, token).ConfigureAwait(false);
                    powerCount = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                }
                else
                    powerCount = Math.Max(decValue.StandardRound(), 1);
            }

            for (int i = 0; i < powerCount; i++)
            {
                List<ValueTuple<string, string>> lstPowerExtraPairs;
                using (XmlNodeList xmlOptionalPowerList = bonusNode.SelectNodes("optionalpower"))
                {
                    lstPowerExtraPairs = new List<ValueTuple<string, string>>(xmlOptionalPowerList?.Count ?? 0);
                    if (xmlOptionalPowerList?.Count > 0)
                    {
                        foreach (XmlNode objXmlOptionalPower in xmlOptionalPowerList)
                        {
                            string strPower = objXmlOptionalPower.InnerTextViaPool(token);
                            if (string.IsNullOrEmpty(strForcePower) || strForcePower == strPower)
                            {
                                lstPowerExtraPairs.Add(new ValueTuple<string, string>(strPower,
                                    objXmlOptionalPower.Attributes?["select"]?.InnerTextViaPool(token)));
                            }
                        }
                    }
                }

                // Display the Select Critter Power window and record which power was selected.
                string strDescription =
                    await LanguageManager.GetStringAsync("String_Improvement_SelectOptionalPower", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectOptionalPower> frmPickPower = await ThreadSafeForm<SelectOptionalPower>.GetAsync(() => new SelectOptionalPower(_objCharacter, lstPowerExtraPairs.ToArray())
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickPower.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    // Record the improvement.
                    XmlNode objXmlPowerNode = (await _objCharacter.LoadDataAsync("critterpowers.xml", token: token).ConfigureAwait(false))
                        .TryGetNodeByNameOrId("/chummer/powers/power", frmPickPower.MyForm.SelectedPower);
                    CritterPower objPower = new CritterPower(_objCharacter);
                    await objPower.CreateAsync(objXmlPowerNode, 0, frmPickPower.MyForm.SelectedPowerExtra, token).ConfigureAwait(false);
                    if (objPower.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    objPower.Grade = -1;
                    await (await _objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false)).AddAsync(objPower, token).ConfigureAwait(false);
                    await CreateImprovementAsync(objPower.InternalId, _objImprovementSource, SourceName,
                        Improvement.ImprovementType.CritterPower, _strUnique, token: token).ConfigureAwait(false);
                }
            }
        }

        public async Task critterpowers(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("critterpowers.xml", token: token).ConfigureAwait(false);
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
            {
                if (xmlPowerList?.Count > 0)
                {
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        XmlNode objXmlCritterPower = objXmlDocument.TryGetNodeByNameOrId("/chummer/powers/power", objXmlPower.InnerTextViaPool(token));
                        CritterPower objPower = new CritterPower(_objCharacter);
                        string strForcedValue = string.Empty;
                        int intRating = 0;
                        if (objXmlPower.Attributes?.Count > 0)
                        {
                            string strRating = objXmlPower.Attributes["rating"]?.InnerTextViaPool(token);
                            if (!string.IsNullOrEmpty(strRating))
                                intRating = await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, _intRating, token).ConfigureAwait(false);
                            strForcedValue = objXmlPower.Attributes["select"]?.InnerTextViaPool(token);
                        }

                        await objPower.CreateAsync(objXmlCritterPower, intRating, strForcedValue, token).ConfigureAwait(false);
                        objPower.Grade = -1;
                        await (await _objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false)).AddAsync(objPower, token).ConfigureAwait(false);
                        await CreateImprovementAsync(objPower.InternalId, _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPower, _strUnique, token: token).ConfigureAwait(false);
                    }
                }
            }
        }

        // Check for Adept Power Points.
        public async Task critterpowerlevels(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (XmlNodeList xmlPowerList = bonusNode.SelectNodes("power"))
            {
                if (xmlPowerList?.Count > 0)
                {
                    foreach (XmlNode objXmlPower in xmlPowerList)
                    {
                        await CreateImprovementAsync(objXmlPower["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.CritterPowerLevel,
                            _strUnique,
                            await ImprovementManager.ValueToDecAsync(_objCharacter, objXmlPower["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task publicawareness(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.PublicAwareness, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task dealerconnection(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstItems))
            {
                using (XmlNodeList objXmlList = bonusNode.SelectNodes("category"))
                {
                    if (objXmlList?.Count > 0)
                    {
                        foreach (XmlNode objNode in objXmlList)
                        {
                            string strText = objNode.InnerTextViaPool(token);
                            if (!string.IsNullOrEmpty(strText) && (await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        Improvement.ImprovementType.DealerConnection, token: token).ConfigureAwait(false))
                                .TrueForAll(x => x.UniqueName != strText))
                            {
                                lstItems.Add(new ListItem(strText,
                                                          await LanguageManager.GetStringAsync(
                                                              "String_DealerConnection_" + strText, token: token).ConfigureAwait(false)));
                            }
                        }
                    }
                }

                if (lstItems.Count == 0)
                {
                    await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync(
                            "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                        SourceName), token: token).ConfigureAwait(false);
                    throw new AbortedException();
                }

                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                       {
                           AllowAutoSelect = false
                       }, token).ConfigureAwait(false))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstItems);
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                }

                // Create the Improvement.
                await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName,
                    Improvement.ImprovementType.DealerConnection, SelectedValue, token: token).ConfigureAwait(false);
            }
        }

        public async Task unlockskills(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            List<string> options = bonusNode.InnerTextViaPool(token).SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            string final;
            switch (options.Count)
            {
                case 0:
                    Utils.BreakIfDebug();
                    throw new AbortedException();
                case 1:
                    final = options[0];
                    break;

                default:
                    {
                        using (ThreadSafeForm<SelectItem> frmSelect = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                               {
                                   AllowAutoSelect = true
                               }, token).ConfigureAwait(false))
                        {
                            frmSelect.MyForm.SetGeneralItemsMode(options.Select(x => new ListItem(x, x)));

                            if (_objCharacter.PushText.TryPop(out string strText))
                            {
                                frmSelect.MyForm.ForceItem(strText);
                            }

                            if (await frmSelect.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            {
                                throw new AbortedException();
                            }

                            final = await frmSelect.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                        }

                        break;
                    }
            }

            if (Enum.TryParse(final, out SkillsSection.FilterOption skills))
            {
                string strName = bonusNode.Attributes?["name"]?.InnerTextViaPool(token) ?? string.Empty;
                await _objCharacter.SkillsSection.AddSkillsAsync(skills, strName, token).ConfigureAwait(false);
                await CreateImprovementAsync(skills.ToString(), _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialSkills, _strUnique, strTarget: strName, token: token).ConfigureAwait(false);
            }
            else
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Failed to parse", "specialskills", bonusNode.OuterXmlViaPool(token) });
            }
        }

        public async Task addqualities(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("qualities.xml", token: token).ConfigureAwait(false);
            using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("addquality"))
            {
                if (xmlQualityList?.Count > 0)
                {
                    foreach (XmlNode objXmlAddQuality in xmlQualityList)
                    {
                        if (objXmlAddQuality.NodeType == XmlNodeType.Comment) continue;
                        XmlNode objXmlSelectedQuality = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", objXmlAddQuality.InnerTextViaPool(token));
                        if (objXmlSelectedQuality == null)
                        {
                            Utils.BreakIfDebug();
                        }
                        string strForceValue = objXmlAddQuality.Attributes?["select"]?.InnerTextViaPool(token) ?? string.Empty;

                        string strRating = objXmlAddQuality.Attributes?["rating"]?.InnerTextViaPool(token);
                        int intCount = string.IsNullOrEmpty(strRating) ? 1 : await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, _intRating, token).ConfigureAwait(false);
                        bool blnDoesNotContributeToBP = !string.Equals(objXmlAddQuality.Attributes?["contributetobp"]?.InnerTextViaPool(token), bool.TrueString, StringComparison.OrdinalIgnoreCase);
                        bool blnForced = objXmlAddQuality.Attributes?["forced"]?.InnerTextIsTrueString() == true;
                        await AddQuality(intCount, blnForced, objXmlSelectedQuality, strForceValue, blnDoesNotContributeToBP, token: token).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task selectquality(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("qualities.xml", token: token).ConfigureAwait(false);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstQualities))
            {
                using (XmlNodeList xmlQualityList = bonusNode.SelectNodes("quality"))
                {
                    if (xmlQualityList?.Count > 0)
                    {
                        foreach (XmlNode objXmlAddQuality in xmlQualityList)
                        {
                            string strName = objXmlAddQuality.InnerTextViaPool(token);
                            XmlNode objXmlQuality
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strName);
                            // Makes sure we aren't over our limits for this particular quality from this overall source
                            if (objXmlAddQuality.Attributes?["forced"]?.InnerTextIsTrueString() == true ||
                                (objXmlQuality != null && await objXmlQuality.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false)))
                            {
                                lstQualities.Add(
                                    new ListItem(strName, objXmlQuality?["translate"]?.InnerTextViaPool(token) ?? strName));
                            }
                        }
                    }
                }

                if (lstQualities.Count == 0)
                {
                    await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync(
                            "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                        SourceName), token: token).ConfigureAwait(false);
                    throw new AbortedException();
                }

                XmlNode objXmlSelectedQuality;
                XmlNode objXmlBonusQuality;
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    frmPickItem.MyForm.SetGeneralItemsMode(lstQualities);

                    // Don't do anything else if the form was canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        throw new AbortedException();
                    string strSelected = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                    objXmlSelectedQuality = objXmlDocument.TryGetNodeByNameOrId(
                        "/chummer/qualities/quality", strSelected);
                    objXmlBonusQuality
                        = bonusNode.SelectSingleNode("quality[. = " + strSelected.CleanXPath() + "]");
                }

                int intQualityDiscount = 0;

                if (bonusNode["discountqualities"] != null)
                {
                    string strForceDiscountValue;
                    lstQualities.Clear();
                    lstQualities.Add(new ListItem("None", await LanguageManager.GetStringAsync("String_None", token: token).ConfigureAwait(false)));
                    using (XmlNodeList xmlQualityNodeList = bonusNode.SelectNodes("discountqualities/quality"))
                    {
                        if (xmlQualityNodeList?.Count > 0)
                        {
                            foreach (XmlNode objXmlAddQuality in xmlQualityNodeList)
                            {
                                strForceDiscountValue = objXmlAddQuality.SelectSingleNodeAndCacheExpressionAsNavigator("@select", token)?.Value ?? string.Empty;
                                string strName = objXmlAddQuality.InnerTextViaPool(token);

                                XmlNode objXmlQuality
                                    = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strName);
                                if (objXmlQuality != null)
                                {
                                    string strDisplayName = objXmlQuality["translate"]?.InnerTextViaPool(token) ?? strName;
                                    if (!string.IsNullOrWhiteSpace(strForceDiscountValue))
                                        strDisplayName += " (" + strForceDiscountValue + ")";
                                    lstQualities.Add(new ListItem(strName, strDisplayName));
                                }
                            }
                        }
                    }

                    if (lstQualities.Count == 0)
                    {
                        await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager.GetStringAsync(
                                "Message_Improvement_EmptySelectionListNamed", token: token).ConfigureAwait(false),
                            SourceName), token: token).ConfigureAwait(false);
                        throw new AbortedException();
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem
                           = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                    {
                        frmPickItem.MyForm.SetGeneralItemsMode(lstQualities);

                        // Don't do anything else if the form was canceled.
                        if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                            throw new AbortedException();
                        string strSelected = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                        if (strSelected != "None")
                        {
                            objXmlSelectedQuality
                                = objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality",
                                                                      strSelected);
                            objXmlBonusQuality
                                = bonusNode.SelectSingleNode("discountqualities/quality[. = "
                                                             + strSelected.CleanXPath() + "]");
                            intQualityDiscount
                                = await ImprovementManager.ValueToIntAsync(_objCharacter, objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@discount", token)?.Value, _intRating, token).ConfigureAwait(false);
                            strForceDiscountValue = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@select", token)?.Value;
                            List<Weapon> lstWeapons = new List<Weapon>(1);
                            Quality discountQuality = new Quality(_objCharacter);
                            try
                            {
                                await discountQuality.SetBPAsync(0, token).ConfigureAwait(false);
                                await discountQuality.CreateAsync(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons,
                                    strForceDiscountValue, _strFriendlyName, token).ConfigureAwait(false);
                                await CreateImprovementAsync(discountQuality.InternalId, _objImprovementSource, SourceName,
                                    Improvement.ImprovementType.SpecificQuality, _strUnique, token: token).ConfigureAwait(false);
                                await (await _objCharacter.GetQualitiesAsync(token).ConfigureAwait(false)).AddAsync(discountQuality, token).ConfigureAwait(false);
                                // Create any Weapons that came with this ware.
                                foreach (Weapon objWeapon in lstWeapons)
                                    await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).AddAsync(objWeapon, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await discountQuality.DeleteQualityAsync(token: CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }
                string strForceValue = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@select", token)?.Value;
                bool blnDoesNotContributeToBP = !string.Equals(objXmlSelectedQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@contributetobp", token)?.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
                bool blnForced = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@forced", token)?.Value == bool.TrueString;
                string strRating = objXmlBonusQuality?.SelectSingleNodeAndCacheExpressionAsNavigator("@rating", token)?.Value;
                int intCount = string.IsNullOrEmpty(strRating) ? 1 : await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, _intRating, token).ConfigureAwait(false);
                await AddQuality(intCount, blnForced, objXmlSelectedQuality, strForceValue, blnDoesNotContributeToBP,
                    intQualityDiscount, token).ConfigureAwait(false);
            }
        }

        public async Task addskillspecialization(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            string strSkill = bonusNode["skill"]?.InnerTextViaPool(token) ?? string.Empty;
            Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync(strSkill, token).ConfigureAwait(false);
            if (objSkill != null)
            {
                // Create the Improvement.
                string strSpec = bonusNode["spec"]?.InnerTextViaPool(token) ?? string.Empty;
                SkillSpecialization objSpec = new SkillSpecialization(_objCharacter, objSkill, strSpec);
                try
                {
                    await (await objSkill.GetSpecializationsAsync(token).ConfigureAwait(false)).AddAsync(objSpec, token).ConfigureAwait(false);
                    await CreateImprovementAsync(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objSpec.InternalId, token: token).ConfigureAwait(false);
                }
                catch
                {
                    await objSpec.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
            }
        }

        public async Task addskillspecializationoption(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlSkillsList = bonusNode.SelectNodes("skills/skill");
            List<Skill> lstSkills = new List<Skill>(xmlSkillsList?.Count ?? 0);
            if (xmlSkillsList?.Count > 0)
            {
                foreach (XmlNode objNode in xmlSkillsList)
                {
                    Skill objSkill = await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(objNode.InnerTextViaPool(token), token).ConfigureAwait(false);
                    if (objSkill != null)
                    {
                        lstSkills.Add(objSkill);
                    }
                }
            }
            else
            {
                Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync(bonusNode["skill"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
                if (objSkill != null)
                {
                    lstSkills.Add(objSkill);
                }
            }

            if (lstSkills.Count > 0)
            {
                foreach (Skill objSkill in lstSkills)
                {
                    // Create the Improvement.
                    string strSpec = bonusNode["spec"]?.InnerTextViaPool(token);
                    await CreateImprovementAsync(await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecializationOption, strSpec, token: token).ConfigureAwait(false);
                    if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetFreeMartialArtSpecializationAsync(token).ConfigureAwait(false) && _objImprovementSource == Improvement.ImprovementSource.MartialArt)
                    {
                        SkillSpecialization objSpec = new SkillSpecialization(_objCharacter, objSkill, strSpec);
                        try
                        {
                            await objSkill.Specializations.AddAsync(objSpec, token).ConfigureAwait(false);
                            await CreateImprovementAsync(await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objSpec.InternalId, token: token).ConfigureAwait(false);
                        }
                        catch
                        {
                            await objSpec.DisposeAsync().ConfigureAwait(false);
                            throw;
                        }
                    }
                }
            }
        }

        public Task allowspellrange(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), Improvement.ImprovementType.AllowSpellRange, token);
        }

        public async Task allowspellcategory(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                await CreateImprovementAsync(bonusNode.InnerTextViaPool(token), Improvement.ImprovementType.AllowSpellCategory, token).ConfigureAwait(false);
            }
            else
            {
                // Display the Select Spell window.
                string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpellCategory", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectSpellCategory> frmPickSpellCategory = await ThreadSafeForm<SelectSpellCategory>.GetAsync(() => new SelectSpellCategory(_objCharacter)
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickSpellCategory.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    await CreateImprovementAsync(frmPickSpellCategory.MyForm.SelectedCategory, Improvement.ImprovementType.AllowSpellCategory, token).ConfigureAwait(false);
                }
            }
        }

        public Task limitspellrange(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), Improvement.ImprovementType.LimitSpellRange, token);
        }

        public async Task limitspellcategory(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                await CreateImprovementAsync(bonusNode.InnerTextViaPool(token), Improvement.ImprovementType.LimitSpellCategory, token).ConfigureAwait(false);
            }
            else
            {
                // Display the Select Spell window.
                string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpellCategory", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectSpellCategory> frmPickSpellCategory = await ThreadSafeForm<SelectSpellCategory>.GetAsync(() => new SelectSpellCategory(_objCharacter)
                       {
                           Description = strDescription
                       }, token).ConfigureAwait(false))
                {
                    frmPickSpellCategory.MyForm.SetExcludeCategories(bonusNode.Attributes?["exclude"]?.InnerTextViaPool(token).SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));

                    // Make sure the dialogue window was not canceled.
                    if (await frmPickSpellCategory.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    await CreateImprovementAsync(frmPickSpellCategory.MyForm.SelectedCategory, Improvement.ImprovementType.LimitSpellCategory, token).ConfigureAwait(false);
                }
            }
        }

        public async Task limitspelldescriptor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Display the Select Spell window.
            string strSelected;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                strSelected = bonusNode.InnerTextViaPool(token);
            }
            else
            {
                string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpellDescriptor", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelected = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                }
            }
            await CreateImprovementAsync(strSelected, Improvement.ImprovementType.LimitSpellDescriptor, token).ConfigureAwait(false);
        }

        public async Task blockspelldescriptor(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            // Display the Select Spell window.
            string strSelected;
            if (!bonusNode.IsNullOrInnerTextIsEmpty())
            {
                strSelected = bonusNode.InnerTextViaPool(token);
            }
            else
            {
                string strDescription = await LanguageManager.GetStringAsync("Title_SelectSpellDescriptor", token: token).ConfigureAwait(false);
                using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                {
                    await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                    // Make sure the dialogue window was not canceled.
                    if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelected = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                }
            }
            await CreateImprovementAsync(strSelected, Improvement.ImprovementType.BlockSpellDescriptor, token).ConfigureAwait(false);
        }

        #region addspiritorsprite

        /// <summary>
        /// Improvement type that adds to the available sprite types a character can summon.
        /// </summary>
        public Task addsprite(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected", token)?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            return AddSpiritOrSprite("streams.xml", xmlAllowedSpirits, Improvement.ImprovementType.AddSprite, blnAddToSelected, "Sprites", token);
        }

        /// <summary>
        /// Improvement type that adds to the available spirit types a character can summon.
        /// </summary>
        public Task addspirit(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected", token)?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            return AddSpiritOrSprite("traditions.xml", xmlAllowedSpirits, Improvement.ImprovementType.AddSpirit, blnAddToSelected, "Spirits", token);
        }

        /// <summary>
        /// Improvement type that limits the spirits a character can summon to a particular category.
        /// </summary>
        public Task limitspiritcategory(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            XmlNodeList xmlAllowedSpirits = bonusNode.SelectNodes("spirit");
            bool blnAddToSelected = true;
            string strAddToSelected = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("addtoselected", token)?.Value;
            if (!string.IsNullOrEmpty(strAddToSelected))
            {
                blnAddToSelected = Convert.ToBoolean(strAddToSelected, GlobalSettings.InvariantCultureInfo);
            }
            return AddSpiritOrSprite("traditions.xml", xmlAllowedSpirits, Improvement.ImprovementType.LimitSpiritCategory, blnAddToSelected, token: token);
        }

        private async Task AddSpiritOrSprite(string strXmlDoc, XmlNodeList xmlAllowedSpirits, Improvement.ImprovementType impType, bool addToSelectedValue = true, string strCritterCategory = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlAllowedSpirits == null)
                throw new ArgumentNullException(nameof(xmlAllowedSpirits));
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setAllowed))
            {
                foreach (XmlNode n in xmlAllowedSpirits)
                {
                    setAllowed.Add(n.InnerTextViaPool(token));
                }

                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSpirits))
                {
                    foreach (XPathNavigator xmlSpirit in (await _objCharacter.LoadDataXPathAsync(strXmlDoc, token: token).ConfigureAwait(false))
                                                                      .SelectAndCacheExpression(
                                                                          "/chummer/spirits/spirit", token))
                    {
                        string strSpiritName = xmlSpirit.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                        if (setAllowed.All(l => strSpiritName != l) && setAllowed.Count != 0)
                            continue;
                        lstSpirits.Add(new ListItem(strSpiritName,
                                                    xmlSpirit.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                    ?? strSpiritName));
                    }

                    if (!string.IsNullOrEmpty(strCritterCategory))
                    {
                        foreach (XPathNavigator xmlSpirit in (await _objCharacter.LoadDataXPathAsync("critters.xml", token: token).ConfigureAwait(false))
                                                                          .Select(
                                                                              "/chummer/critters/critter[category = "
                                                                              + strCritterCategory.CleanXPath() + "]"))
                        {
                            string strSpiritName = xmlSpirit.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                            if (setAllowed.All(l => strSpiritName != l) && setAllowed.Count != 0)
                                continue;
                            lstSpirits.Add(new ListItem(strSpiritName,
                                                        xmlSpirit.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                        ?? strSpiritName));
                        }
                    }

                    using (ThreadSafeForm<SelectItem> frmSelect = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                    {
                        frmSelect.MyForm.SetGeneralItemsMode(lstSpirits);
                        frmSelect.MyForm.ForceItem(ForcedValue);
                        if (await frmSelect.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        string strSelected = await frmSelect.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                        if (addToSelectedValue)
                        {
                            if (string.IsNullOrEmpty(SelectedValue))
                                SelectedValue = strSelected;
                            else
                                SelectedValue += ", " + strSelected; // Do not use localized value for space because we expect the selected value to be in English
                        }

                        await CreateImprovementAsync(strSelected, _objImprovementSource, SourceName, impType,
                            _strUnique, token: token).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion addspiritorsprite

        public async Task movementreplace(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            Improvement.ImprovementType imp = Improvement.ImprovementType.WalkSpeed;
            string strSpeed = bonusNode["speed"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strSpeed))
            {
                switch (strSpeed.ToUpperInvariant())
                {
                    case "RUN":
                        imp = Improvement.ImprovementType.RunSpeed;
                        break;

                    case "SPRINT":
                        imp = Improvement.ImprovementType.SprintSpeed;
                        break;
                }
            }

            string strNodeValText = bonusNode["val"]?.InnerTextViaPool(token);
            string strCategory = bonusNode["category"]?.InnerTextViaPool(token);
            if (!string.IsNullOrEmpty(strCategory))
            {
                await CreateImprovementAsync(strCategory, _objImprovementSource, SourceName, imp, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strNodeValText, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
            else
            {
                await CreateImprovementAsync("Ground", _objImprovementSource, SourceName, imp, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strNodeValText, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await CreateImprovementAsync("Swim", _objImprovementSource, SourceName, imp, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strNodeValText, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await CreateImprovementAsync("Fly", _objImprovementSource, SourceName, imp, _strUnique,
                    await ImprovementManager.ValueToDecAsync(_objCharacter, strNodeValText, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            }
        }

        public async Task addlimb(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strUseUnique = _strUnique;
            XPathNavigator xmlPrecedenceNode = bonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@precedence", token);
            if (xmlPrecedenceNode != null)
                strUseUnique = "precedence" + xmlPrecedenceNode.Value;

            await CreateImprovementAsync(bonusNode["limbslot"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.AddLimb, strUseUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task attributekarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.AttributeKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task activeskillkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.ActiveSkillKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgroupkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task knowledgeskillkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.KnowledgeSkillKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task knowledgeskillkarmacostmin(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName,
                Improvement.ImprovementType.KnowledgeSkillKarmaCostMinimum, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public Task skilldisable(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillDisable, _strUnique, token: token);
        }

        public Task skillenablemovement(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillEnableMovement, _strUnique, token: token);
        }

        public Task skillgroupdisable(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupDisable, _strUnique, token: token);
        }

        public async Task skillgroupdisablechoice(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else
            {
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSkills))
                {
                    using (XmlNodeList objXmlGroups = bonusNode.SelectNodes("skillgroup"))
                    {
                        if (objXmlGroups?.Count > 0)
                        {
                            foreach (XmlNode objXmlGroup in objXmlGroups)
                            {
                                string strInnerText = objXmlGroup.InnerTextViaPool(token);
                                lstSkills.Add(new ListItem(strInnerText,
                                                           await _objCharacter.TranslateExtraAsync(
                                                               strInnerText, GlobalSettings.Language,
                                                               "skills.xml", token).ConfigureAwait(false)));
                            }
                        }
                        else
                        {
                            await (await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetSkillGroupsAsync(token).ConfigureAwait(false)).ForEachAsync(async objGroup =>
                            {
                                if (!await objGroup.GetIsDisabledAsync(token).ConfigureAwait(false))
                                    lstSkills.Add(new ListItem(objGroup.Name,
                                        await objGroup.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                            }, token).ConfigureAwait(false);
                        }
                    }

                    if (lstSkills.Count > 1)
                    {
                        lstSkills.Sort(CompareListItems.CompareNames);
                    }

                    string strDescription = await LanguageManager.GetStringAsync("String_DisableSkillGroupPrompt", token: token).ConfigureAwait(false);
                    using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                           {
                               AllowAutoSelect = false
                           }, token).ConfigureAwait(false))
                    {
                        await frmPickItem.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                        frmPickItem.MyForm.SetGeneralItemsMode(lstSkills);

                        // Make sure the dialogue window was not canceled.
                        if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                    }
                }
            }

            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupDisable, _strUnique, token: token).ConfigureAwait(false);
        }

        public Task skillgroupcategorydisable(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryDisable, _strUnique, token: token);
        }

        public async Task skillgroupcategorykarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupCategoryKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategorykarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillCategoryKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategoryspecializationkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillCategorySpecializationKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task attributepointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.AttributePointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task activeskillpointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.ActiveSkillPointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgrouppointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupPointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task knowledgeskillpointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.KnowledgeSkillPointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgroupcategorypointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupCategoryPointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategorypointcost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillCategoryPointCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newspellkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode.Attributes?["type"]?.InnerTextViaPool(token) ?? string.Empty,
                _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0,
                0, 0, 0, string.Empty, false, string.Empty,
                bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newcomplexformkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newaiprogramkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newaiadvancedprogramkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task attributekarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task activeskillkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgroupkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task knowledgeskillkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgroupcategorykarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName,
                Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1,
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategorykarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategoryspecializationkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task attributepointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributePointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task activeskillpointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.ActiveSkillPointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgrouppointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupPointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task knowledgeskillpointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillgroupcategorypointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task skillcategorypointcostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.SkillCategoryPointCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                1, await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["min"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), await ImprovementManager.ValueToIntAsync(_objCharacter, bonusNode["max"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false),
                0, 0, string.Empty, false, string.Empty, bonusNode["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newspellkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode.Attributes?["type"]?.InnerTextViaPool(token) ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewSpellKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newcomplexformkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newaiprogramkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public async Task newaiadvancedprogramkarmacostmultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, string.Empty, bonusNode.Attributes?["condition"]?.InnerTextViaPool(token) ?? string.Empty, token).ConfigureAwait(false);
        }

        public Task blockskillspecializations(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillSpecializations, _strUnique, token: token);
        }

        public Task blockskillcategoryspecializations(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.BlockSkillCategorySpecializations, _strUnique, token: token);
        }

        // Flat modifier to cost of binding a focus
        public async Task focusbindingkarmacost(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaCost, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerTextViaPool(token) ?? string.Empty, token: token).ConfigureAwait(false);
        }

        // Flat modifier to the number that is multiplied by a focus' rating to get the focus' binding karma cost
        public async Task focusbindingkarmamultiplier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.FocusBindingKarmaMultiplier, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), 1, 0, 0, 0, 0, string.Empty, false, bonusNode["extracontains"]?.InnerTextViaPool(token) ?? string.Empty, token: token).ConfigureAwait(false);
        }

        public Task magicianswaydiscount(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MagiciansWayDiscount, _strUnique, token: token);
        }

        public Task burnoutsway(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.BurnoutsWay, _strUnique, token: token);
        }

        // Add a specific Cyber/Bioware to the Character.
        public async Task addware(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNode node;
            Improvement.ImprovementSource eSource;
            string strName = bonusNode["name"]?.InnerTextViaPool(token);
            if (string.IsNullOrEmpty(strName))
                throw new AbortedException();
            if (bonusNode["type"]?.InnerTextViaPool(token) == "bioware")
            {
                node = (await _objCharacter.LoadDataAsync("bioware.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/biowares/bioware", strName);
                eSource = Improvement.ImprovementSource.Bioware;
            }
            else
            {
                node = (await _objCharacter.LoadDataAsync("cyberware.xml", token: token).ConfigureAwait(false)).TryGetNodeByNameOrId("/chummer/cyberwares/cyberware", strName);
                eSource = Improvement.ImprovementSource.Cyberware;
            }

            if (node == null)
                throw new AbortedException();
            string strRating = bonusNode["rating"]?.InnerTextViaPool(token);
            int intRating = string.IsNullOrEmpty(strRating) ? 1 : await ImprovementManager.ValueToIntAsync(_objCharacter, strRating, _intRating, token).ConfigureAwait(false);
            string strSourceId = node["id"]?.InnerTextViaPool(token);
            string strImprovedName = strSourceId;
            if (string.Equals(strSourceId, Cyberware.EssenceAntiHoleGuidString, StringComparison.OrdinalIgnoreCase))
            {
                await _objCharacter.DecreaseEssenceHoleAsync(intRating, token: token).ConfigureAwait(false);
            }
            else if (string.Equals(strSourceId, Cyberware.EssenceHoleGuidString, StringComparison.OrdinalIgnoreCase))
            {
                await _objCharacter.IncreaseEssenceHoleAsync(intRating, token: token).ConfigureAwait(false);
            }
            else
            {
                Grade objGrade = await Grade.ConvertToCyberwareGradeAsync(bonusNode["grade"]?.InnerTextViaPool(token), _objImprovementSource,
                        _objCharacter, token).ConfigureAwait(false);
                // Create the new piece of ware.
                List<Weapon> lstWeapons = new List<Weapon>(1);
                List<Vehicle> lstVehicles = new List<Vehicle>(1);
                Cyberware objCyberware = new Cyberware(_objCharacter);
                try
                {
                    await objCyberware.CreateAsync(node, objGrade, eSource, intRating, lstWeapons, lstVehicles, true, true,
                        ForcedValue, token: token).ConfigureAwait(false);

                    if (objCyberware.InternalId.IsEmptyGuid())
                        throw new AbortedException();

                    objCyberware.Cost = "0";
                    objCyberware.ParentID = SourceName;

                    await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).AddAsync(objCyberware, token).ConfigureAwait(false);
                    // Create any Weapons that came with this ware.
                    foreach (Weapon objWeapon in lstWeapons)
                        await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).AddAsync(objWeapon, token).ConfigureAwait(false);
                    // Create any Vehicles that came with this ware.
                    foreach (Vehicle objVehicle in lstVehicles)
                        await (await _objCharacter.GetVehiclesAsync(token).ConfigureAwait(false)).AddAsync(objVehicle, token).ConfigureAwait(false);
                    strImprovedName = objCyberware.InternalId;
                }
                catch
                {
                    if (lstWeapons.Count > 0)
                    {
                        foreach (Weapon objWeapon in lstWeapons)
                        {
                            await objWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                        }
                    }

                    if (lstVehicles.Count > 0)
                    {
                        foreach (Vehicle objVehicle in lstVehicles)
                        {
                            await objVehicle.DeleteVehicleAsync(token: CancellationToken.None).ConfigureAwait(false);
                        }
                    }
                    await objCyberware.DeleteCyberwareAsync(token: CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }

            await CreateImprovementAsync(strImprovedName, _objImprovementSource, SourceName,
                Improvement.ImprovementType.FreeWare,
                _strUnique, intRating: intRating, token: token).ConfigureAwait(false);
        }

        public async Task weaponaccuracy(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token) ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponAccuracy, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["value"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task weaponrangemodifier(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(bonusNode["name"]?.InnerTextViaPool(token) ?? string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponRangeModifier, _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["value"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task weaponskillaccuracy(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strSelectedValue = ForcedValue;
            if (string.IsNullOrEmpty(strSelectedValue))
            {
                XmlElement xmlSelectSkillNode = bonusNode["selectskill"];
                if (xmlSelectSkillNode != null)
                {
                    bool blnKnowledgeSkill;
                    (strSelectedValue, blnKnowledgeSkill) = await ImprovementManager.DoSelectSkillAsync(xmlSelectSkillNode, _objCharacter, _intRating,
                        _strFriendlyName, token: token).ConfigureAwait(false);
                    if (blnKnowledgeSkill)
                        throw new AbortedException();
                }
                else
                    strSelectedValue = bonusNode["name"]?.InnerTextViaPool(token) ?? string.Empty;
            }

            SelectedValue = strSelectedValue;
            string strVal = bonusNode["value"]?.InnerTextViaPool(token);

            await CreateImprovementAsync(strSelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.WeaponSkillAccuracy, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, strVal, _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task metageniclimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.MetageneticLimit, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task specialmodificationlimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.SpecialModificationLimit, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public Task cyberadeptdaemon(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.CyberadeptDaemon, _strUnique, token: token);
        }

        /// <summary>
        /// Improvement increases the Dice Pool for a specific named Action.
        /// TODO: Link to actions.xml when we implement that.
        /// </summary>
        public async Task actiondicepool(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            if (!string.IsNullOrEmpty(ForcedValue))
            {
                SelectedValue = ForcedValue;
            }
            else if (bonusNode["name"] != null)
            {
                SelectedValue = bonusNode["name"].InnerTextViaPool(token);
            }
            else
            {
                // Populate the Magician Traditions list.
                XPathNavigator xmlActionsBaseNode =
                    (await _objCharacter.LoadDataXPathAsync("actions.xml", token: token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer", token);
                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstActions))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdXPath))
                    {
                        sbdXPath.Append("actions/action[", await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false));
                        string strCategory = bonusNode.Attributes?["category"]?.InnerTextViaPool(token) ?? string.Empty;
                        if (!string.IsNullOrEmpty(strCategory))
                        {
                            sbdXPath.Append(" and category = ", strCategory.CleanXPath());
                        }
                        sbdXPath.Append(']');
                        if (xmlActionsBaseNode != null)
                        {
                            foreach (XPathNavigator xmlAction in xmlActionsBaseNode.Select(sbdXPath.ToString()))
                            {
                                string strName = xmlAction.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                                if (!string.IsNullOrEmpty(strName))
                                    lstActions.Add(new ListItem(
                                                       xmlAction.SelectSingleNodeAndCacheExpression("id", token)?.Value
                                                       ?? strName,
                                                       xmlAction.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                                       ?? strName));
                            }
                        }
                    }

                    if (lstActions.Count > 1)
                    {
                        lstActions.Sort(CompareListItems.CompareNames);
                    }

                    using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                           {
                               AllowAutoSelect = false
                           }, token).ConfigureAwait(false))
                    {
                        frmPickItem.MyForm.SetDropdownItemsMode(lstActions);

                        // Make sure the dialogue window was not canceled.
                        if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        SelectedValue = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                    }
                }
            }
            await CreateImprovementAsync(SelectedValue, _objImprovementSource, SourceName, Improvement.ImprovementType.ActionDicePool,
                _strUnique, await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode["val"]?.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task contactkarma(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactKarmaDiscount, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task contactkarmaminimum(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            await CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.ContactKarmaMinimum, _strUnique,
                await ImprovementManager.ValueToDecAsync(_objCharacter, bonusNode.InnerTextViaPool(token), _intRating, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        // Enable Sprite Fettering.
        public Task allowspritefettering(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.AllowSpriteFettering, _strUnique, token: token);
        }

        // Enable the Convert to Cyberzombie methods.
        public Task enablecyberzombie(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(string.Empty, _objImprovementSource, SourceName, Improvement.ImprovementType.EnableCyberzombie, _strUnique, token: token);
        }

        public Task allowcritterpowercategory(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.AllowCritterPowerCategory, _strUnique, token: token);
        }

        public Task limitcritterpowercategory(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.LimitCritterPowerCategory, _strUnique, token: token);
        }

        public Task attributemaxclamp(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.AttributeMaxClamp, _strUnique, token: token);
        }

        public async Task metamagiclimit(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));
            XmlNodeList xmlMetamagicsList = bonusNode.SelectNodes("metamagic");
            if (xmlMetamagicsList != null)
            {
                foreach (XmlNode child in xmlMetamagicsList)
                {
                    int intRating = -1;
                    string strGrade = child.Attributes?["grade"]?.InnerTextViaPool(token);
                    if (!string.IsNullOrEmpty(strGrade))
                        intRating = await ImprovementManager.ValueToIntAsync(_objCharacter, strGrade, _intRating, token).ConfigureAwait(false);
                    await CreateImprovementAsync(child.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.MetamagicLimit, _strUnique, 0, intRating, token: token).ConfigureAwait(false);
                }
            }
        }

        public Task disablequality(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.DisableQuality, _strUnique, token: token);
        }

        public Task freequality(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (bonusNode == null)
                return Task.FromException(new ArgumentNullException(nameof(bonusNode)));
            return CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.FreeQuality, _strUnique, token: token);
        }

        public async Task selectexpertise(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            // Select the skill to get the expertise
            string strForcedValue = ForcedValue;
            ForcedValue = string.Empty; // Temporarily clear Forced Value because the Forced Value should be for the specialization name, not the skill
            string strSkill = (await ImprovementManager.DoSelectSkillAsync(bonusNode, _objCharacter, _intRating, _strFriendlyName, token: token).ConfigureAwait(false)).Item1;
            ForcedValue = strForcedValue;
            Skill objSkill = await (await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)).GetActiveSkillAsync(strSkill, token).ConfigureAwait(false) ?? throw new AbortedException();
            // Select the actual specialization to add as an expertise
            using (ThreadSafeForm<SelectItem> frmPickItem = await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
            {
                string strLimitToSpecialization = bonusNode.Attributes?["limittospecialization"]?.InnerTextViaPool(token);
                if (!string.IsNullOrEmpty(strLimitToSpecialization))
                    frmPickItem.MyForm.SetDropdownItemsMode(strLimitToSpecialization.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                        .Where(x => objSkill.Specializations.All(y => y.Name != x, token)).Select(x => new ListItem(x, _objCharacter.TranslateExtra(x, GlobalSettings.Language, "skills.xml", token: token))));
                else
                    frmPickItem.MyForm.SetGeneralItemsMode(objSkill.CGLSpecializations);
                if (!string.IsNullOrEmpty(ForcedValue))
                    frmPickItem.MyForm.ForceItem(ForcedValue);

                frmPickItem.MyForm.AllowAutoSelect = !string.IsNullOrEmpty(ForcedValue);

                // Make sure the dialogue window was not canceled.
                string strSelected = await frmPickItem.MyForm.DoThreadSafeFuncAsync(x => x.SelectedName, token).ConfigureAwait(false);
                if (await frmPickItem.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel || string.IsNullOrEmpty(strSelected))
                {
                    throw new AbortedException();
                }

                SelectedValue = strSelected;
            }
            // Create the Improvement.
            SkillSpecialization objExpertise = new SkillSpecialization(_objCharacter, objSkill, SelectedValue, true, true);
            try
            {
                await (await objSkill.GetSpecializationsAsync(token).ConfigureAwait(false)).AddAsync(objExpertise, token).ConfigureAwait(false);
                await CreateImprovementAsync(strSkill, _objImprovementSource, SourceName, Improvement.ImprovementType.SkillSpecialization, objExpertise.InternalId, token: token).ConfigureAwait(false);
            }
            catch
            {
                await objExpertise.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task penaltyfreesustain(XmlNode bonusNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                throw new ArgumentNullException(nameof(bonusNode));

            string strDummy = string.Empty;
            int intCount = 1;
            if (bonusNode.TryGetStringFieldQuickly("count", ref strDummy))
                intCount = await ImprovementManager.ValueToIntAsync(_objCharacter, strDummy, _intRating, token).ConfigureAwait(false);
            int intMaxForce = int.MaxValue;
            if (bonusNode.TryGetStringFieldQuickly("force", ref strDummy))
                intMaxForce = await ImprovementManager.ValueToIntAsync(_objCharacter, strDummy, _intRating, token).ConfigureAwait(false);
            await CreateImprovementAsync(bonusNode.InnerTextViaPool(token), _objImprovementSource, SourceName, Improvement.ImprovementType.PenaltyFreeSustain, _strUnique, intMaxForce, intCount, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Replaces the Active Skill used for one or all spells with another. Example:
        /// <replacespellskill spell="Mana Net">Blades</replacespellskill> Replace the Spellcasting skill with Blades for the spell Mana Net.
        /// <replacespellskill>Blades</replacespellskill> Replace the Spellcasting skill with Blades for ALL spells that are not marked as Alchemical or BareHandedAdept.
        /// </summary>
        public Task replaceskillspell(XmlNode bonusNode, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return bonusNode == null
                ? Task.FromException(new ArgumentNullException(nameof(bonusNode)))
                : CreateImprovementAsync(bonusNode.Attributes?["spell"]?.InnerTextViaPool(token) ?? string.Empty, bonusNode.InnerTextViaPool(token),
                    Improvement.ImprovementType.ReplaceSkillSpell, token);
        }

#pragma warning restore IDE1006 // Naming Styles

        #endregion Improvement Methods
        #region Helper Methods

        /// <summary>
        /// Create and add a named Quality to the character.
        /// </summary>
        /// <param name="intCount">Total number of the instances of the quality to attempt to add. Used to proxy giving a quality of a given Rating.</param>
        /// <param name="blnForced">Whether to ignore the required/forbidden nodes of the quality</param>
        /// <param name="objXmlSelectedQuality">XmlNode of the selected quality</param>
        /// <param name="strForceValue">Forced Extra value for the quality</param>
        /// <param name="blnDoesNotContributeToBP">Whether the quality contributes to BP or not. If false, BP will be set to 0.</param>
        /// <param name="intQualityDiscount">Total reduced karma cost of the quality as provided by costdiscount nodes or similar. Incompatible with blnDoesNotContributeToBP. Minimum of 1.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <exception cref="AbortedException"></exception>
        private async Task AddQuality(int intCount, bool blnForced, XmlNode objXmlSelectedQuality, string strForceValue, bool blnDoesNotContributeToBP, int intQualityDiscount = 0, CancellationToken token = default)
        {
            for (int i = 0; i < intCount; ++i)
            {
                // Makes sure we aren't over our limits for this particular quality from this overall source
                if (blnForced || (objXmlSelectedQuality != null && await objXmlSelectedQuality.CreateNavigator().RequirementsMetAsync(_objCharacter, strLocalName: _strFriendlyName, token: token).ConfigureAwait(false)))
                {
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    Quality objAddQuality = new Quality(_objCharacter);
                    try
                    {
                        await objAddQuality.CreateAsync(objXmlSelectedQuality, QualitySource.Improvement, lstWeapons,
                            strForceValue, _strFriendlyName, token).ConfigureAwait(false);

                        if (blnDoesNotContributeToBP)
                        {
                            await objAddQuality.SetBPAsync(0, token).ConfigureAwait(false);
                            await objAddQuality.SetContributeToLimitAsync(false, token).ConfigureAwait(false);
                        }
                        else if (intQualityDiscount != 0)
                        {
                            await objAddQuality.SetBPAsync(Math.Max(await objAddQuality.GetBPAsync(token).ConfigureAwait(false) + intQualityDiscount, 1), token).ConfigureAwait(false);
                        }

                        await (await _objCharacter.GetQualitiesAsync(token).ConfigureAwait(false)).AddAsync(objAddQuality, token).ConfigureAwait(false);
                        foreach (Weapon objWeapon in lstWeapons)
                            await (await _objCharacter.GetWeaponsAsync(token).ConfigureAwait(false)).AddAsync(objWeapon, token).ConfigureAwait(false);
                        await CreateImprovementAsync(objAddQuality.InternalId, _objImprovementSource, SourceName,
                            Improvement.ImprovementType.SpecificQuality, _strUnique, token: token).ConfigureAwait(false);
                    }
                    catch
                    {
                        await objAddQuality.DeleteQualityAsync(token: CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
                else
                {
                    throw new AbortedException();
                }
            }
        }
        #endregion
    }
}
