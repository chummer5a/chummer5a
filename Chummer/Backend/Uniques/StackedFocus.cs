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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// A Stacked Focus.
    /// </summary>
    [DebuggerDisplay("{Name(GlobalSettings.DefaultLanguage)}")]
    public class StackedFocus
    {
        private Guid _guiID;
        private bool _blnBonded;
        private Guid _guiGearId;
        private readonly List<Gear> _lstGear = new List<Gear>(2);
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, and Load Methods

        public StackedFocus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("stackedfocus");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("gearid", _guiGearId.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("bonded", _blnBonded.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
                objGear.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Stacked Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetField("gearid", Guid.TryParse, out _guiGearId);
            _blnBonded = objNode["bonded"]?.InnerText == bool.TrueString;
            using (XmlNodeList nodGearList = objNode.SelectNodes("gears/gear"))
            {
                if (nodGearList == null)
                    return;
                foreach (XmlNode nodGear in nodGearList)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodGear);
                    _lstGear.Add(objGear);
                }
            }
        }

        #endregion Constructor, Create, Save, and Load Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Stacked Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// GUID of the linked Gear.
        /// </summary>
        public string GearId
        {
            get => _guiGearId.ToString("D", GlobalSettings.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiGearId = guiTemp;
            }
        }

        /// <summary>
        /// Whether or not the Stacked Focus in Bonded.
        /// </summary>
        public bool Bonded
        {
            get => _blnBonded;
            set => _blnBonded = value;
        }

        /// <summary>
        /// The Stacked Focus' total Force.
        /// </summary>
        public int TotalForce
        {
            get
            {
                int intReturn = 0;
                foreach (Gear objGear in Gear)
                    intReturn += objGear.Rating;

                return intReturn;
            }
        }

        /// <summary>
        /// The cost in Karma to bind this Stacked Focus.
        /// </summary>
        public int BindingCost
        {
            get
            {
                decimal decCost = 0;
                foreach (Gear objFocus in Gear)
                {
                    // Each Focus costs an amount of Karma equal to their Force x specific Karma cost.
                    string strFocusName = objFocus.Name;
                    string strFocusExtra = objFocus.Extra;
                    int intPosition = strFocusName.IndexOf('(');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition - 1);
                    intPosition = strFocusName.IndexOf(',');
                    if (intPosition > -1)
                        strFocusName = strFocusName.Substring(0, intPosition);
                    decimal decExtraKarmaCost = 0;
                    if (strFocusName.EndsWith(", Individualized, Complete", StringComparison.Ordinal))
                    {
                        decExtraKarmaCost = -2;
                        strFocusName = strFocusName.Replace(", Individualized, Complete", string.Empty);
                    }
                    else if (strFocusName.EndsWith(", Individualized, Partial", StringComparison.Ordinal))
                    {
                        decExtraKarmaCost = -1;
                        strFocusName = strFocusName.Replace(", Individualized, Partial", string.Empty);
                    }

                    decimal decKarmaMultiplier;
                    switch (strFocusName)
                    {
                        case "Qi Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaQiFocus;
                            break;

                        case "Sustaining Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaSustainingFocus;
                            break;

                        case "Counterspelling Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaCounterspellingFocus;
                            break;

                        case "Banishing Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaBanishingFocus;
                            break;

                        case "Binding Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaBindingFocus;
                            break;

                        case "Weapon Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaWeaponFocus;
                            break;

                        case "Spellcasting Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaSpellcastingFocus;
                            break;

                        case "Summoning Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaSummoningFocus;
                            break;

                        case "Alchemical Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaAlchemicalFocus;
                            break;

                        case "Centering Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaCenteringFocus;
                            break;

                        case "Masking Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaMaskingFocus;
                            break;

                        case "Disenchanting Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaDisenchantingFocus;
                            break;

                        case "Power Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaPowerFocus;
                            break;

                        case "Flexible Signature Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaFlexibleSignatureFocus;
                            break;

                        case "Ritual Spellcasting Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaRitualSpellcastingFocus;
                            break;

                        case "Spell Shaping Focus":
                            decKarmaMultiplier = _objCharacter.Settings.KarmaSpellShapingFocus;
                            break;

                        default:
                            decKarmaMultiplier = 1;
                            break;
                    }

                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements.Where(x => x.ImprovedName == strFocusName && (string.IsNullOrEmpty(x.Target) || strFocusExtra.Contains(x.Target)) && x.Enabled))
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.FocusBindingKarmaCost:
                                decExtraKarmaCost += objLoopImprovement.Value;
                                break;

                            case Improvement.ImprovementType.FocusBindingKarmaMultiplier:
                                decKarmaMultiplier += objLoopImprovement.Value;
                                break;
                        }
                    }
                    decCost += objFocus.Rating * decKarmaMultiplier + decExtraKarmaCost;
                }
                return decCost.StandardRound();
            }
        }

        /// <summary>
        /// Stacked Focus Name.
        /// </summary>
        public string Name(CultureInfo objCulture, string strLanguage)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                foreach (Gear objGear in Gear)
                {
                    sbdReturn.Append(objGear.DisplayName(objCulture, strLanguage));
                    sbdReturn.Append(", ");
                }

                // Remove the trailing comma.
                if (sbdReturn.Length > 0)
                    sbdReturn.Length -= 2;

                return sbdReturn.ToString();
            }
        }

        public string CurrentDisplayName => Name(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// List of Gear that make up the Stacked Focus.
        /// </summary>
        public List<Gear> Gear => _lstGear;

        #endregion Properties

        #region Methods

        public TreeNode CreateTreeNode(Gear objGear, ContextMenuStrip cmsStackedFocus)
        {
            if (objGear == null)
                throw new ArgumentNullException(nameof(objGear));
            TreeNode objNode = objGear.CreateTreeNode(cmsStackedFocus);

            objNode.Name = InternalId;
            objNode.Text = LanguageManager.GetString("String_StackedFocus")
                           + LanguageManager.GetString("String_Colon") + LanguageManager.GetString("String_Space")
                           + CurrentDisplayName;
            objNode.Tag = this;
            objNode.Checked = Bonded;

            return objNode;
        }

        #endregion Methods
    }
}
