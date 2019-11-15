using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

namespace Chummer
{
    public class AdeptPower : Power
    {
        private bool _blnDiscountedAdeptWay;
        private bool _blnDiscountedGeas;
        private XmlNode _nodAdeptWayRequirements;
        #region Constructor, Save, Load and Print Methods
        public AdeptPower(Character objCharacter) : base(objCharacter)
        {
            _improvementSource = Improvement.ImprovementSource.Power;
            _freeLevelImprovementType = Improvement.ImprovementType.AdeptPowerFreeLevels;
            _freePointImprovementType = Improvement.ImprovementType.AdeptPowerFreePoints;

            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
            CharacterObject.PropertyChanged += OnCharacterChanged;
            if (CharacterObject.Options.MysAdeptSecondMAGAttribute && CharacterObject.IsMysticAdept)
            {
                MAGAttributeObject = CharacterObject.MAGAdept;
            }
            else
            {
                MAGAttributeObject = CharacterObject.MAG;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", Extra);
            objWriter.WriteElementString("pointsperlevel", _strPointsPerLevel);
            objWriter.WriteElementString("adeptway", _strAdeptWayDiscount);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("extrapointcost", _decExtraPointCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
            objWriter.WriteElementString("maxlevels", _intMaxLevels.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
            objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            if (Bonus != null)
                objWriter.WriteRaw("<bonus>" + Bonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodAdeptWayRequirements != null)
                objWriter.WriteRaw("<adeptwayrequires>" + _nodAdeptWayRequirements.InnerXml + "</adeptwayrequires>");
            else
                objWriter.WriteElementString("adeptwayrequires", string.Empty);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();

            CharacterObject.SourceProcess(_strSource);
        }

        public bool Create(XmlNode objNode, int intRating = 1, string strForcedValue = "", XmlNode objBonusNodeOverride = null, bool blnCreateImprovements = true)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
            _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("points", ref _strPointsPerLevel);
            objNode.TryGetStringFieldQuickly("adeptway", ref _strAdeptWayDiscount);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            _intRating = intRating;
            if (!objNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (!objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevels))
            {
                objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevels);
            }
            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            Bonus = objNode["bonus"];
            if (objBonusNodeOverride != null)
                Bonus = objBonusNodeOverride;
            _nodAdeptWayRequirements = objNode["adeptwayrequires"];
            XmlNode nodEnhancements = objNode["enhancements"];
            if (nodEnhancements != null)
            {
                using (XmlNodeList xmlEnhancementList = nodEnhancements.SelectNodes("enhancement"))
                {
                    if (xmlEnhancementList != null)
                    {
                        foreach (XmlNode nodEnhancement in xmlEnhancementList)
                        {
                            Enhancement objEnhancement = new Enhancement(CharacterObject);
                            objEnhancement.Load(nodEnhancement);
                            objEnhancement.Parent = this;
                            Enhancements.Add(objEnhancement);
                        }
                    }
                }
            }
            if (blnCreateImprovements && Bonus != null && Bonus.HasChildNodes)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = Extra;
                if (!ImprovementManager.CreateImprovements(CharacterObject, _improvementSource, InternalId, Bonus, false, TotalRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    ImprovementManager.ForcedValue = strOldForce;
                    DeletePower();
                    return false;
                }
                Extra = ImprovementManager.SelectedValue;
                ImprovementManager.SelectedValue = strOldSelected;
                ImprovementManager.ForcedValue = strOldForce;
            }
            if (TotalMaximumLevels < Rating)
            {
                Rating = TotalMaximumLevels;
            }
            return true;
        }
        /// <summary>
        /// Load the Power from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                if (!(node.TryGetField("id", Guid.TryParse, out _guiSourceID)))
                {
                    string strPowerName = Name;
                    int intPos = strPowerName.IndexOf('(');
                    if (intPos != -1)
                        strPowerName = strPowerName.Substring(0, intPos - 1);
                    XmlDocument objXmlDocument = XmlManager.Load("powers.xml");
                    XmlNode xmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                    if (xmlPower.TryGetField("id", Guid.TryParse, out _guiSourceID))
                    {
                        _objCachedMyXmlNode = null;
                    }
                }
            }

            Extra = objNode["extra"]?.InnerText ?? string.Empty;
            _strPointsPerLevel = objNode["pointsperlevel"]?.InnerText;
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            _strAdeptWayDiscount = objNode["adeptway"]?.InnerText;
            if (string.IsNullOrEmpty(_strAdeptWayDiscount))
            {
                string strPowerName = Name;
                int intPos = strPowerName.IndexOf('(');
                if (intPos != -1)
                    strPowerName = strPowerName.Substring(0, intPos - 1);
                _strAdeptWayDiscount = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]/adeptway")?.InnerText ?? string.Empty;
            }
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            if (!objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevels))
            {
                objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevels);
            }
            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            Bonus = objNode["bonus"];
            if (objNode["adeptway"] != null)
            {
                _nodAdeptWayRequirements = objNode["adeptwayrequires"] ?? GetNode()?["adeptwayrequires"];
            }
            if (Name != "Improved Reflexes" && Name.StartsWith("Improved Reflexes"))
            {
                XmlNode objXmlPower = XmlManager.Load("powers.xml").SelectSingleNode("/chummer/powers/power[starts-with(./name,\"Improved Reflexes\")]");
                if (objXmlPower != null)
                {
                    if (int.TryParse(Name.TrimStartOnce("Improved Reflexes", true).Trim(), out int intTemp))
                    {
                        Create(objXmlPower, intTemp, "", null, false);
                        objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
                    }
                }
            }
            else
            {
                XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
                if (nodEnhancements != null)
                {
                    foreach (XmlNode nodEnhancement in nodEnhancements)
                    {
                        Enhancement objEnhancement = new Enhancement(CharacterObject);
                        objEnhancement.Load(nodEnhancement);
                        objEnhancement.Parent = this as AdeptPower;
                        Enhancements.Add(objEnhancement);
                    }
                }
            }

            //TODO: Seems that the MysAd Second Attribute house rule gets accidentally enabled sometimes?
            if (Rating > TotalMaximumLevels)
            {
                Utils.BreakIfDebug();
                Rating = TotalMaximumLevels;
            }
            else if (Rating + FreeLevels > TotalMaximumLevels)
            {
                Utils.BreakIfDebug();
                TotalRating = TotalMaximumLevels;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("power");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName);
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("pointsperlevel", PointsPerLevel.ToString(objCulture));
            objWriter.WriteElementString("adeptway", AdeptWayDiscount.ToString(objCulture));
            objWriter.WriteElementString("rating", LevelsEnabled ? TotalRating.ToString(objCulture) : "0");
            objWriter.WriteElementString("totalpoints", PowerPoints.ToString(objCulture));
            objWriter.WriteElementString("action", DisplayActionMethod(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Print(objWriter, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }
        #endregion
        #region Properties
        /// <summary>
        /// Calculate the discount that is applied to the Power.
        /// </summary>
        public decimal Discount => DiscountedAdeptWay ? AdeptWayDiscount : 0;

        /// <summary>
        /// Whether or not the Power Cost is discounted by 50% from Adept Way.
        /// </summary>
        public bool DiscountedAdeptWay
        {
            get => _blnDiscountedAdeptWay;
            set
            {
                if (value != _blnDiscountedAdeptWay)
                {
                    _blnDiscountedAdeptWay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the Power Cost is discounted by 25% from Geas.
        /// </summary>
        public bool DiscountedGeas
        {
            get => _blnDiscountedGeas;
            set
            {
                if (value != _blnDiscountedGeas)
                {
                    _blnDiscountedGeas = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the power can be discounted due to presence of an Adept Way.
        /// </summary>
        public bool AdeptWayDiscountEnabled
        {
            get
            {
                if (AdeptWayDiscount == 0)
                {
                    return false;
                }
                bool blnReturn = false;
                //If the Adept Way Requirements node is missing OR the Adept Way Requirements node doesn't have magicianswayforbids, check for the magician's way discount.
                if (_nodAdeptWayRequirements?["magicianswayforbids"] == null)
                {
                    blnReturn = CharacterObject.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.MagiciansWayDiscount && x.Enabled);
                }
                if (!blnReturn && _nodAdeptWayRequirements?.ChildNodes.Count > 0)
                {
                    blnReturn = _nodAdeptWayRequirements.RequirementsMet(CharacterObject);
                }

                return blnReturn;
            }
        }

        public static readonly DependancyGraph<string> PowerDependencyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(DisplayPoints),
                    new DependancyGraphNode<string>(nameof(PowerPoints),
                        new DependancyGraphNode<string>(nameof(TotalRating),
                            new DependancyGraphNode<string>(nameof(Rating)),
                            new DependancyGraphNode<string>(nameof(FreeLevels),
                                new DependancyGraphNode<string>(nameof(FreePoints)),
                                new DependancyGraphNode<string>(nameof(ExtraPointCost)),
                                new DependancyGraphNode<string>(nameof(PointsPerLevel))
                            ),
                            new DependancyGraphNode<string>(nameof(TotalMaximumLevels),
                                new DependancyGraphNode<string>(nameof(LevelsEnabled)),
                                new DependancyGraphNode<string>(nameof(MaxLevels))
                            )
                        ),
                        new DependancyGraphNode<string>(nameof(Rating)),
                        new DependancyGraphNode<string>(nameof(LevelsEnabled)),
                        new DependancyGraphNode<string>(nameof(FreeLevels)),
                        new DependancyGraphNode<string>(nameof(PointsPerLevel)),
                        new DependancyGraphNode<string>(nameof(FreePoints)),
                        new DependancyGraphNode<string>(nameof(ExtraPointCost)),
                        new DependancyGraphNode<string>(nameof(Discount),
                            new DependancyGraphNode<string>(nameof(DiscountedAdeptWay)),
                            new DependancyGraphNode<string>(nameof(AdeptWayDiscount))
                        )
                    )
                ),
                new DependancyGraphNode<string>(nameof(ToolTip),
                    new DependancyGraphNode<string>(nameof(Rating)),
                    new DependancyGraphNode<string>(nameof(PointsPerLevel))
                ),
                new DependancyGraphNode<string>(nameof(DoesNotHaveFreeLevels),
                    new DependancyGraphNode<string>(nameof(FreeLevels))
                ),
                new DependancyGraphNode<string>(nameof(AdeptWayDiscountEnabled),
                    new DependancyGraphNode<string>(nameof(AdeptWayDiscount))
                )
            );
        #endregion
        #region Methods

        public void RefreshDiscountedAdeptWay(bool blnAdeptWayDiscountEnabled)
        {
            if (DiscountedAdeptWay && !blnAdeptWayDiscountEnabled)
                DiscountedAdeptWay = false;
        }
        #endregion

        #region Events
        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }
        #endregion
    }
}
