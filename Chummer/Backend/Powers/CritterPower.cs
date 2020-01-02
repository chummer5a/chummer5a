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

namespace Chummer
{
    public class CritterPower : Power, INotifyMultiplePropertyChanged
    {
        private int    _intKarma;
        private string _strCategory;

        #region Constructor, Print, Save and Load Methods
        public CritterPower(Character objCharacter) : base(objCharacter)
        {
            _improvementSource = Improvement.ImprovementSource.CritterPower;
            _freeLevelImprovementType = Improvement.ImprovementType.CritterPowerLevel;
            _freePointImprovementType = Improvement.ImprovementType.AdeptPowerFreePoints;

            // Create the GUID for the new Power.
            _guiID = Guid.NewGuid();
            CharacterObject = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("critterpower");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", Extra);
            objWriter.WriteElementString("pointsperlevel", _strPointsPerLevel);
            objWriter.WriteElementString("action", _strAction);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("range", _strDuration);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extrapointcost", _decExtraPointCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
            objWriter.WriteElementString("maxlevels", _intMaxLevels.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            if (Bonus != null)
                objWriter.WriteRaw("<bonus>" + Bonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();

            CharacterObject.SourceProcess(_strSource);
        }

        public bool Create(XmlNode objNode, int intRating = 1, string strForcedValue = "", XmlNode objBonusNodeOverride = null, bool blnCreateImprovements = true)
        {
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
            _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("points", ref _strPointsPerLevel);
            objNode.TryGetInt32FieldQuickly("karma", ref _intKarma);
            objNode.TryGetBoolFieldQuickly("rating", ref _blnLevelsEnabled);
            _intRating = intRating;
            if (!objNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (!objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevels))
            {
                objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevels);
            }
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
            if (blnCreateImprovements && Bonus != null && Bonus.HasChildNodes)
            {
                string strOldForce = ImprovementManager.ForcedValue;
                string strOldSelected = ImprovementManager.SelectedValue;
                ImprovementManager.ForcedValue = strForcedValue;
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
                    XmlDocument objXmlDocument = XmlManager.Load("critterpowers.xml");
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
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            if (!objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevels))
            {
                objNode.TryGetInt32FieldQuickly("maxlevels", ref _intMaxLevels);
            }
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetDecFieldQuickly("extrapointcost", ref _decExtraPointCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            Bonus = objNode["bonus"];

            if (Rating > TotalMaximumLevels)
            {
                Utils.BreakIfDebug();
                Rating = TotalMaximumLevels;
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
            objWriter.WriteElementString("rating", LevelsEnabled ? TotalRating.ToString(objCulture) : "0");
            objWriter.WriteElementString("totalpoints", PowerPoints.ToString(objCulture));
            objWriter.WriteElementString("action", DisplayActionMethod(strLanguageToPrint));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            if (CharacterObject.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion
        #region Properties
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Whether or not the Critter Power counts towards their total number of Critter Powers.
        /// </summary>
        public bool CountTowardsLimit { get; set; }

        /// <summary>
        /// Karma cost of the power. 
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set => _intKarma = value;
        }

        public int Grade { get; set; }
        /// <summary>
        /// Type.
        /// </summary>
        public string Type { get; set; }



        /// <summary>
        /// The current 'paid' Rating of the Power.
        /// </summary>
        public new int Rating
        {
            get => _intRating;
            set
            {
                if (_intRating != value)
                {
                    _intRating = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The current Rating of the Power, including any Free Levels.
        /// </summary>
        public new int TotalRating => Rating + FreeLevels;

        /// <summary>
        /// Total maximum number of levels the power can have. Unlike Adept powers, Free Levels are applied on top of the maximum. 
        /// </summary>
        public new int TotalMaximumLevels
        {
            get
            {
                if (!LevelsEnabled)
                    return 1;
                int intReturn = MaxLevels + FreeLevels;
                if (intReturn == 0)
                {
                    intReturn = int.MaxValue;
                }
                return intReturn;
            }
        }

        /// <summary>
        /// Translated Type.
        /// </summary>
        public string DisplayType(string strLanguage)
        {
            string strReturn = Type;

            switch (strReturn)
            {
                case "M":
                    strReturn = LanguageManager.GetString("String_SpellTypeMana", strLanguage);
                    break;
                case "P":
                    strReturn = LanguageManager.GetString("String_SpellTypePhysical", strLanguage);
                    break;
                default:
                    strReturn = LanguageManager.GetString("String_None", strLanguage);
                    break;
            }

            return strReturn;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("critterpowers.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Whether the Power is allowed to be upgraded by the user. 
        /// </summary>
        public bool AllowUpgrade => LevelsEnabled && Karma > 0 ||
                                    (PointsPerLevel > 0 && CharacterObject.CritterPowerPointsTotal > 0);

        /// <summary>
        /// Free levels of the power.
        /// </summary>
        public new int FreeLevels
        {
            get
            {
                if (_intCachedFreeLevels != int.MinValue)
                    return _intCachedFreeLevels;
                //TODO: This does much less than the FreeLevels for adept powers. If this turns out to be needed, reintegrate with the base Power class.
                int intReturn = CharacterObject.Improvements.Where(objImprovement =>
                        objImprovement.ImproveType == _freeLevelImprovementType &&
                        objImprovement.ImprovedName == Name &&
                        objImprovement.UniqueName == Extra && objImprovement.Enabled)
                    .Sum(objImprovement => objImprovement.Rating);
                
                return _intCachedFreeLevels = intReturn;
            }
        }

        #endregion
        #region PropertyChanged
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
                        new DependancyGraphNode<string>(nameof(ExtraPointCost))
                    )
                ),
                new DependancyGraphNode<string>(nameof(ToolTip),
                    new DependancyGraphNode<string>(nameof(Rating)),
                    new DependancyGraphNode<string>(nameof(PointsPerLevel))
                ),
                new DependancyGraphNode<string>(nameof(DoesNotHaveFreeLevels),
                    new DependancyGraphNode<string>(nameof(FreeLevels))
                )
            );
        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public new void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public new void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = PowerDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in PowerDependencyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(DisplayPoints)))
                _strCachedPowerPoints = string.Empty;
            if (lstNamesOfChangedProperties.Contains(nameof(FreeLevels)))
                _intCachedFreeLevels = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(PowerPoints)))
                _decCachedPowerPoints = decimal.MinValue;

            // If the Bonus contains "Rating", remove the existing Improvements and create new ones.
            if (lstNamesOfChangedProperties.Contains(nameof(TotalRating)))
            {
                if (Bonus?.InnerXml.Contains("Rating") == true)
                {
                    ImprovementManager.RemoveImprovements(CharacterObject, _improvementSource, InternalId);
                    int intTotalRating = TotalRating;
                    if (intTotalRating > 0)
                    {
                        ImprovementManager.ForcedValue = Extra;
                        ImprovementManager.CreateImprovements(CharacterObject, _improvementSource, InternalId, Bonus, false, intTotalRating, DisplayNameShort(GlobalOptions.Language));
                    }
                }
            }

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }
        #endregion
        #region Methods
        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteCritterPower",
                    GlobalOptions.Language)))
                    return false;
            }

            ImprovementManager.RemoveImprovements(characterObject, Improvement.ImprovementSource.CritterPower, InternalId);

            return characterObject.CritterPowers.Remove(this);
        }
        #endregion
    }
}
