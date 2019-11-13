using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #endregion
        #region Properties
        public string Category { get; set; }
        public bool CountTowardsLimit { get; set; }
        public int Karma { get; set; }
        public int Grade { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public string Type { get; set; }

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

        #endregion

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
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
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
    }
}
