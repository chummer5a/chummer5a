using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public void RefreshDiscountedAdeptWay(bool blnAdeptWayDiscountEnabled)
        {
            if (DiscountedAdeptWay && !blnAdeptWayDiscountEnabled)
                DiscountedAdeptWay = false;
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

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }
    }
}
