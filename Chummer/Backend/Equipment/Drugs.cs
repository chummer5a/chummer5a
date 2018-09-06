using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;

namespace Chummer.Backend.Equipment
{
    public class Drug : Object
    {
        private Guid _sourceID = Guid.Empty;
        private Guid _guiID = new Guid();
        private string _strName = "";
        private string _strCategory = "";
        private string _strAvailability = "0";
        private string _strDescription = string.Empty;
        private string _strEffectDescription = string.Empty;
        private ObservableCollection<DrugComponent> _lstDrugComponents = new ObservableCollection<DrugComponent>();
        private Dictionary<string, int> _dicCachedAttributes = new Dictionary<string, int>();
        private List<string> _lstCachedInfos = new List<string>();
        private Dictionary<string, int> _dicCachedLimits = new Dictionary<string, int>();
        private List<string> _lstCachedQualities = new List<string>();
        private string _strGrade = "";
        private decimal _decCost;
        private int _intAddictionThreshold;
        private int _intAddictionRating;
        private decimal _decQty;
        private string _strAltName = "";
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public Drug(Character objCharacter)
        {
            _objCharacter = objCharacter;
            // Create the GUID for the new Drug.
            _guiID = Guid.NewGuid();
            _lstDrugComponents.CollectionChanged += ComponentsChanged;
        }

        private void ComponentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _intCachedCrashDamage = int.MinValue;
            _intCachedDuration = int.MinValue;
            _intCachedInitiative = int.MinValue;
            _intCachedInitiativeDice = int.MinValue;
            _intCachedSpeed = int.MinValue;
            _blnCachedQualityFlag = false;
            _blnCachedLimitFlag = false;
            _blnCachedAttributeFlag = false;
            _strDescription = string.Empty;
        }

        public void Load(XmlNode objXmlData)
        {
            _guiID = Guid.Parse(objXmlData["guid"].InnerText);
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
            objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
            foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("drugcomponents/drugcomponent"))
            {
                DrugComponent c = new DrugComponent(_objCharacter);
                c.Load(objXmlLevel);
                Components.Add(c);
            }

            if (objXmlData["sourceid"] == null || !objXmlData.TryGetField("sourceid", Guid.TryParse, out _sourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetField("id", Guid.TryParse, out _sourceID);
            }
            objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
            objXmlData.TryGetDecFieldQuickly("cost", ref _decCost);
            objXmlData.TryGetDecFieldQuickly("quantity", ref _decQty);
            objXmlData.TryGetInt32FieldQuickly("rating", ref _intAddictionRating);
            objXmlData.TryGetInt32FieldQuickly("threshold", ref _intAddictionThreshold);
            //objXmlData.TryGetField("source", out _strSource);
            //objXmlData.TryGetField("page", out _strPage);
        }

        public void Save(XmlWriter objXmlWriter)
        {
            objXmlWriter.WriteStartElement("drug");
            objXmlWriter.WriteElementString("id", _sourceID.ToString("D"));
            objXmlWriter.WriteElementString("guid", _guiID.ToString());
            objXmlWriter.WriteElementString("name", _strName);
            objXmlWriter.WriteElementString("category", _strCategory);
            objXmlWriter.WriteElementString("quantity", _decQty.ToString(GlobalOptions.InvariantCultureInfo));
            objXmlWriter.WriteStartElement("drugcomponents");
            foreach (DrugComponent objDrugComponent in _lstDrugComponents)
            {
                objXmlWriter.WriteStartElement("drugcomponent");
                objDrugComponent.Save(objXmlWriter);
                objXmlWriter.WriteEndElement();
            }

            objXmlWriter.WriteEndElement();
            objXmlWriter.WriteElementString("availability", _strAvailability);
            if (_decCost != 0)
                objXmlWriter.WriteElementString("cost", _decCost.ToString(GlobalOptions.InvariantCultureInfo));
            if (_intAddictionRating != 0)
                objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString());
            if (_intAddictionThreshold != 0)
                objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString());
            /*if (source != null)
                objXmlWriter.WriteElementString("source", source);
            if (page != 0)
                objXmlWriter.WriteElementString("page", page.ToString());*/
            objXmlWriter.WriteEndElement();
        }

        #endregion

        #region Properties

        public Guid GUID
        {
            get => _guiID;
            set => _guiID = value;
        }

        /// <summary>
        /// Internal identifier which will be used to identify this item.
        /// </summary>
        public string InternalId => _guiID.ToString();

        /// <summary>
        /// Grade of the Drug.
        /// </summary>
        public string Grade
        {
            get => _strGrade;
            set => _strGrade = value;
        }

        /// <summary>
        /// Compiled description of the drug.
        /// </summary>
        public string Description
        {
            get
            {
                if (_strDescription == string.Empty)
                    _strDescription = GenerateDescription(0);
                return _strDescription;
            }
            set => _strDescription = value;
        }

        /// <summary>
        /// Compiled description of the drug's Effects.
        /// </summary>
        public string EffectDescription
        {
            get
            {
                if (_strEffectDescription == string.Empty)
                    _strEffectDescription = GenerateDescription(0, true);
                return _strEffectDescription;
            }
            set => _strEffectDescription = value;
        }

        /// <summary>
        /// Components of the Drug.
        /// </summary>
        public ObservableCollection<DrugComponent> Components
        {
            get => _lstDrugComponents;
            set => _lstDrugComponents = value;
        }

        /// <summary>
        /// Name of the Drug.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Category of the Drug. 
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        private decimal _decCachedCost = decimal.MinValue;

        /// <summary>
        /// Base cost of the Drug.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (_decCachedCost != decimal.MinValue) return _decCachedCost;
                _decCachedCost = Components.Sum(d => d.CostPerLevel);
                return _decCachedCost;
            }
        }

        /// <summary>
		/// Total cost of the Drug.
		/// </summary>
		public decimal TotalCost => Cost * Quantity;

	    /// <summary>
		/// Total amount of the Drug held by the character.
		/// </summary>
		public decimal Quantity
		{
			get => _decQty;
	        set => _decQty = value;
	    }
        
        /// <summary>
        /// Availability of the Drug.
        /// </summary>
        public string Availability => _strAvailability;

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                /*
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }*/

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }
            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (DrugComponent objComponent in Components)
                {
                    AvailabilityValue objLoopAvail = objComponent.TotalAvailTuple();
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        private int _intCachedAddictionThreshold = int.MinValue;

        /// <summary>
        /// Addiction Threshold of the Drug.
        /// </summary>
        public int AddictionThreshold
        {
            get
            {
                if (_intCachedAddictionThreshold != int.MinValue) return _intCachedAddictionThreshold;
                _intCachedAddictionThreshold = Components.Sum(d => d.AddictionThreshold);
                return _intCachedAddictionThreshold;
            }
        }

        private int _intCachedAddictionRating = int.MinValue;
        /// <summary>
        /// Addiction Rating of the Drug.
        /// </summary>
        public int AddictionRating
        {
            get
            {
                if (_intCachedAddictionRating != int.MinValue) return _intCachedAddictionRating;
                _intCachedAddictionRating = Components.Sum(d => d.AddictionRating);
                return _intCachedAddictionRating;
            }
        }

        private bool _blnCachedLimitFlag = false;
        public Dictionary<string, int> Limits
        {
            get
            {
                if (_blnCachedLimitFlag) return _dicCachedLimits;
                _dicCachedLimits = Components.Where(d => d.ActiveDrugEffect.Limits.Count > 0)
                    .SelectMany(d => d.ActiveDrugEffect.Limits)
                    .GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
                _blnCachedLimitFlag = true;
                return _dicCachedLimits;

            }
        }

        private bool _blnCachedQualityFlag = false;
        public List<string> Qualities
	    {
	        get
	        {
	            if (_blnCachedQualityFlag) return _lstCachedQualities;
	            foreach (DrugComponent d in Components)
	            {
	                _lstCachedQualities.AddRange(d.ActiveDrugEffect.Qualities);
                }

	            _lstCachedQualities = _lstCachedQualities.Distinct().ToList();
	            _blnCachedQualityFlag = true;
                return _lstCachedQualities;
	        }
	    }

        private bool _cachedInfoFlag = false;
        public List<string> Infos
	    {
	        get
	        {
	            if (_cachedInfoFlag) return _lstCachedInfos;
	            foreach (DrugComponent d in Components)
	            {
	                _lstCachedInfos.AddRange(d.ActiveDrugEffect.Infos);
                }

	            _lstCachedInfos = _lstCachedInfos.Distinct().ToList();
	            _cachedInfoFlag = true;
                return _lstCachedInfos;
	        }
	    }

        private int _intCachedInitiative = int.MinValue;

        public int Initiative
        {
            get
            {
                if (_intCachedInitiative != int.MinValue) return _intCachedInitiative;
                _intCachedInitiative = Components.Sum(d => d.ActiveDrugEffect.Initiative);
                return _intCachedInitiative;
            }
        }

        private int _intCachedInitiativeDice = int.MinValue;
        public int InitiativeDice
        {
            get
            {
                if (_intCachedInitiativeDice != int.MinValue) return _intCachedInitiativeDice;
                _intCachedInitiativeDice = Components.Sum(d => d.ActiveDrugEffect.InitiativeDice);
                return _intCachedInitiativeDice;
            }
        }

        private int _intCachedSpeed = int.MinValue;
        public int Speed
        {
            get
            {
                if (_intCachedSpeed != int.MinValue) return _intCachedSpeed;
                _intCachedSpeed = Components.Sum(d => d.ActiveDrugEffect.Speed);
                return _intCachedSpeed;
            }
        }

        private int _intCachedDuration = int.MinValue;
        public int Duration
        {
            get
            {
                if (_intCachedDuration != int.MinValue) return _intCachedDuration;
                _intCachedDuration = Components.Sum(d => d.ActiveDrugEffect.Duration);
                return _intCachedDuration;
            }
        }

        private int _intCachedCrashDamage = int.MinValue;
        public int CrashDamage
        {
            get
            {
                if (_intCachedCrashDamage != int.MinValue) return _intCachedCrashDamage;
                _intCachedCrashDamage = Components.Sum(d => d.ActiveDrugEffect.Duration);
                return _intCachedCrashDamage;
            }
        }

        public string Notes { get; internal set; }

		/// <summary>
		/// The name of the object as it should appear on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort => _strAltName != string.Empty ? _strAltName : _strName;


        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort;
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (Quantity != 1)
                strReturn = Quantity.ToString("#,0.##", objCulture) + strSpaceCharacter + strReturn;
            return strReturn;
        }

        private bool _blnCachedAttributeFlag = false;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage;

        public Dictionary<string, int> Attributes
        {
            get
            {
                if (_blnCachedAttributeFlag) return _dicCachedAttributes;
                _dicCachedAttributes =
                    (from d in Components
                        from de in d.DrugEffects.Where(de => de.Level == d.Level)
                        where de.Attributes.Count > 0
                        from attribute in de.Attributes
                        select attribute).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
                _blnCachedAttributeFlag = true;
                return _dicCachedAttributes;
            }
        }
        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Color.SaddleBrown;
                }

                return SystemColors.WindowText;
            }
        }
        public Guid SourceID => _sourceID;

        #endregion


        #region UI Methods
        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public TreeNode CreateTreeNode()
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
                Tag = this,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }
        #endregion
        #region Methods

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("armor.xml", strLanguage).SelectSingleNode("/chummer/armors/armor[id = \"" + SourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public String GenerateDescription(int level = -1, bool effectsOnly = false)
        {
            StringBuilder description = new StringBuilder();
			bool newLineFlag = false;
            if (!effectsOnly) description.Append(Category).Append(": ").Append(Name).AppendLine();

			if (level != -1)
			{
                foreach (var objAttribute in Attributes)
				{
					if (objAttribute.Value != 0)
					{
						description.Append(objAttribute.Key).Append(objAttribute.Value.ToString("+#;-#")).Append("; ");
						newLineFlag = true;
					}
				}
				if (newLineFlag)
				{
					newLineFlag = false;
					description.AppendLine();
				}

				foreach (var objLimit in Limits)
				{
					if (objLimit.Value != 0)
					{
						description.Append(objLimit.Key).Append(" limit ").Append(objLimit.Value.ToString("+#;-#")).Append("; ");
						newLineFlag = true;
					}
				}
				if (newLineFlag)
				{
					newLineFlag = false;
					description.AppendLine();
				}

				if (Initiative != 0 || InitiativeDice != 0)
				{
					description.Append("Initiative ");
					if (Initiative != 0)
						description.Append(Initiative.ToString("+#;-#"));
					if (InitiativeDice != 0)
						description.Append(InitiativeDice.ToString("+#;-#"));
					description.AppendLine();
				}

				foreach (string quality in Qualities)
					description.Append(quality).Append(" quality").AppendLine();
				foreach (string info in Infos)
					description.Append(info).AppendLine();

				if (Category == "Custom Drug" || Duration != 0)
					description.Append("Duration: 10 x ").Append(Duration + 1).Append("d6 minutes").AppendLine();

				if (Category == "Custom Drug" || Speed != 0)
				{
					if (3 - Speed == 0)
						description.Append("Speed: Immediate").AppendLine();
					else
						description.Append("Speed: ").Append(3 - Speed).Append(" combat turns").AppendLine();
				}

				if (CrashDamage != 0)
					description.Append("Crash Effect: ").Append(CrashDamage).Append("S damage, unresisted").AppendLine();
			    if (!effectsOnly)
			    {
			        description.Append("Addiction rating: ").Append(AddictionRating * (level + 1)).AppendLine();
			        description.Append("Addiction threshold: ").Append(AddictionThreshold * (level + 1)).AppendLine();
			        description.Append("Cost: ").Append((Cost * (level + 1)).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)).Append("¥").AppendLine();
			        description.Append($"Availability: {TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language)}")
			            .AppendLine();
			    }
			}
			else if (!effectsOnly)
            {
				description.Append("Addiction rating: ").Append(AddictionRating).Append(" per level").AppendLine();
				description.Append("Addiction threshold: ").Append(AddictionThreshold).Append(" per level").AppendLine();
				description.Append("Cost: ").Append(Cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)).Append("¥ per level").AppendLine();
				description.Append("Availability: ").Append(TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language)).Append(" per level").AppendLine();
			}

            _strDescription = description.ToString();
			return _strDescription;
		}
		#endregion
	}
	/// <summary>
	/// Drug Component.
	/// </summary>
	public class DrugComponent : object
	{
	    private Guid _sourceID;
        private string _strName;
		private string _strCategory;
	    private string _strAvailability = "0";
        private readonly List<DrugEffect> _lstEffects;
		private int _intLevel = 0;
		private string _strSource;
		private string _strPage;
		private string _strCost;
		private int _intAddictionThreshold;
		private int _intAddictionRating;
	    private XmlNode _objCachedMyXmlNode;
	    private string _strCachedXmlNodeLanguage;
	    private readonly Character _objCharacter;

	    public DrugComponent(Character objCharacter)
	    {
	        _objCharacter = objCharacter;
            _lstEffects = new List<DrugEffect>();
		}

		#region Constructor, Create, Save, Load, and Print Methods
		public void Load(XmlNode objXmlData)
		{
		    objXmlData.TryGetField("id", Guid.TryParse, out _sourceID);
            objXmlData.TryGetStringFieldQuickly("name", ref _strName);
			objXmlData.TryGetStringFieldQuickly("category", ref _strCategory);
			foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("effects/effect"))
			{
				DrugEffect objDrugEffect = new DrugEffect();
			    objXmlLevel.TryGetField("level", out int effectLevel);
			    objDrugEffect.Level = effectLevel;

                foreach (XmlNode objXmlEffect in objXmlLevel.SelectNodes("*"))
				{
				    objXmlEffect.TryGetField("name", out string effectName, null);
				    objXmlEffect.TryGetField("value", out var effectValue, 1);
                    switch (objXmlEffect.Name)
					{
						case "attribute":
							if (effectName != null)
								objDrugEffect.Attributes[effectName] = effectValue;
							break;
						case "limit":
							if (effectName != null)
								objDrugEffect.Limits[effectName] = effectValue;
							break;
						case "quality":
							objDrugEffect.Qualities.Add(objXmlEffect.InnerText);
							break;
						case "info":
							objDrugEffect.Infos.Add(objXmlEffect.InnerText);
							break;
						case "initiative":
							objDrugEffect.Initiative = int.Parse(objXmlEffect.InnerText);
							break;
						case "initiativedice":
							objDrugEffect.InitiativeDice = int.Parse(objXmlEffect.InnerText);
							break;
						case "crashdamage":
							objDrugEffect.CrashDamage = int.Parse(objXmlEffect.InnerText);
							break;
						case "speed":
							objDrugEffect.Speed = int.Parse(objXmlEffect.InnerText);
							break;
						case "duration":
							objDrugEffect.Duration = int.Parse(objXmlEffect.InnerText);
							break;
						default:
							Log.Warning(info: string.Format("Unknown drug effect %s in component %s", objXmlEffect.Name, effectName));
							break;
					}
				}
				_lstEffects.Add(objDrugEffect);
			}
			objXmlData.TryGetStringFieldQuickly("availability", ref _strAvailability);
			objXmlData.TryGetStringFieldQuickly("cost", ref _strCost);
			objXmlData.TryGetField("rating", out _intAddictionRating);
			objXmlData.TryGetField("threshold", out _intAddictionThreshold);
			objXmlData.TryGetStringFieldQuickly("source", ref _strSource);
			objXmlData.TryGetStringFieldQuickly("page", ref _strPage);
		}

		public void Save(XmlWriter objXmlWriter)
        {
            objXmlWriter.WriteElementString("id", _sourceID.ToString("D"));
            objXmlWriter.WriteElementString("name", _strName);
			objXmlWriter.WriteElementString("category", _strCategory);

			objXmlWriter.WriteStartElement("effects");
			foreach (var objDrugEffect in _lstEffects)
			{
				objXmlWriter.WriteStartElement("effect");
				foreach (var objAttribute in objDrugEffect.Attributes)
				{
					objXmlWriter.WriteStartElement("attribute");
					objXmlWriter.WriteElementString("name", objAttribute.Key);
					objXmlWriter.WriteElementString("value", objAttribute.Value.ToString());
					objXmlWriter.WriteEndElement();
				}
				foreach (var objLimit in objDrugEffect.Limits)
				{
					objXmlWriter.WriteStartElement("limit");
					objXmlWriter.WriteElementString("name", objLimit.Key);
					objXmlWriter.WriteElementString("value", objLimit.Value.ToString());
					objXmlWriter.WriteEndElement();
				}
				foreach (string quality in objDrugEffect.Qualities)
				{
					objXmlWriter.WriteElementString("quality", quality);
				}
				foreach (string info in objDrugEffect.Infos)
				{
					objXmlWriter.WriteElementString("info", info);
				}
				if (objDrugEffect.Initiative != 0)
					objXmlWriter.WriteElementString("initiative", objDrugEffect.Initiative.ToString());
				if (objDrugEffect.InitiativeDice != 0)
					objXmlWriter.WriteElementString("initiativedice", objDrugEffect.InitiativeDice.ToString());
				if (objDrugEffect.Duration != 0)
					objXmlWriter.WriteElementString("duration", objDrugEffect.Duration.ToString());
				if (objDrugEffect.Speed != 0)
					objXmlWriter.WriteElementString("speed", objDrugEffect.Speed.ToString());
				if (objDrugEffect.CrashDamage != 0)
					objXmlWriter.WriteElementString("crashdamage", objDrugEffect.CrashDamage.ToString());
				objXmlWriter.WriteEndElement();
			}
			objXmlWriter.WriteEndElement();

		    objXmlWriter.WriteElementString("availability", _strAvailability);
            objXmlWriter.WriteElementString("cost", _strCost);
            if (_intAddictionRating != 0)
				objXmlWriter.WriteElementString("rating", _intAddictionRating.ToString());
			if (_intAddictionThreshold != 0)
				objXmlWriter.WriteElementString("threshold", _intAddictionThreshold.ToString());
            objXmlWriter.WriteElementString("source", _strSource);
            objXmlWriter.WriteElementString("page", _strPage);
        }
		#endregion
		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get => _strName;
		    set => _strName = value;
		}
		/// <summary>
		/// 
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = "";
				if (_intLevel == 0)
				{
					return _strName;
				}
				strReturn = $"{_strName} (Level {_intLevel})";
				return strReturn;
			}
			set => _strName = value;
		}
		/// <summary>
		/// 
		/// </summary>
		public string Category
		{
			get => _strCategory;
		    set => _strCategory = value;
		}
	    /// <summary>
	    /// Sourcebook.
	    /// </summary>
	    public string Source
	    {
	        get => _strSource;
	        set => _strSource = value;
	    }
	    /// <summary>
	    /// Sourcebook Page Number.
	    /// </summary>
	    public string Page(string strLanguage)
	    {
	        if (strLanguage == GlobalOptions.DefaultLanguage)
	            return _strPage;

	        return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
	    }

        public List<DrugEffect> DrugEffects => _lstEffects;

	    public DrugEffect ActiveDrugEffect => DrugEffects.FirstOrDefault(effect => effect.Level == Level);

	    public string Cost
		{
	        get => _strCost;
	        set => _strCost = value;
        }

        /// <summary>
        /// Cost of the drug component per level
        /// </summary>
        public decimal CostPerLevel
        {
            get
            {
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCostExpression = strValues[Math.Max(Math.Min(Level, strValues.Length) - 1, 0)].Trim('[', ']');
                }
                
                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));
                objCost.Replace("Level", Level.ToString());
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
            }
        }

        public string Availability
		{
			get => _strAvailability;
		    set => _strAvailability = value;
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Availability;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }
                
                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                /*
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }*/

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        public int AddictionThreshold
		{
			get => _intAddictionThreshold;
		    set => _intAddictionThreshold = value;
		}

		public int AddictionRating
		{
			get => _intAddictionRating;
		    set => _intAddictionRating = value;
		}

		public int Level
		{
			get => _intLevel;
		    set => _intLevel = value;
	    }

	    public Guid SourceID => _sourceID;
        #endregion
        #region Methods

        public XmlNode GetNode(string strLanguage)
	    {
	        if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
	        {
	            _objCachedMyXmlNode = XmlManager.Load("armor.xml", strLanguage).SelectSingleNode("/chummer/armors/armor[id = \"" + SourceID.ToString("D") + "\"]");
	            _strCachedXmlNodeLanguage = strLanguage;
	        }
	        return _objCachedMyXmlNode;
	    }

	    public string GenerateDescription(int intLevel = -1)
		{
			if (intLevel >= _lstEffects.Count)
				return null;

			StringBuilder strbldDescription = new StringBuilder();
			bool blnNewLineFlag = false;

			strbldDescription.Append(_strCategory).Append(": ").Append(_strName).AppendLine();

			if (intLevel != -1)
			{
				DrugEffect objDrugEffect = _lstEffects.ElementAt(intLevel);

				foreach (KeyValuePair<string, int> objAttribute in objDrugEffect.Attributes)
				{
					if (objAttribute.Value != 0)
					{
						strbldDescription.Append(objAttribute.Key).Append(objAttribute.Value.ToString("+#;-#")).Append("; ");
						blnNewLineFlag = true;
					}
				}
				if (blnNewLineFlag)
				{
					blnNewLineFlag = false;
					strbldDescription.AppendLine();
				}

				foreach (KeyValuePair<string, int> objLimit in objDrugEffect.Limits)
				{
					if (objLimit.Value != 0)
					{
						strbldDescription.Append(objLimit.Key).Append(" limit ").Append(objLimit.Value.ToString("+#;-#")).Append("; ");
						blnNewLineFlag = true;
					}
				}
				if (blnNewLineFlag)
				{
					blnNewLineFlag = false;
					strbldDescription.AppendLine();
				}

				if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
				{
					strbldDescription.Append("Initiative ");
					if (objDrugEffect.Initiative != 0)
						strbldDescription.Append(objDrugEffect.Initiative.ToString("+#;-#"));
					if (objDrugEffect.InitiativeDice != 0)
						strbldDescription.Append(objDrugEffect.InitiativeDice.ToString("+#;-#"));
					strbldDescription.AppendLine();
				}

				foreach (string quality in objDrugEffect.Qualities)
					strbldDescription.Append(quality).Append(" quality").AppendLine();
				foreach (string info in objDrugEffect.Infos)
					strbldDescription.Append(info).AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.Duration != 0)
					strbldDescription.Append("Duration: 10 x ").Append(objDrugEffect.Duration + 1).Append("d6 minutes").AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.Speed != 0)
				{
					if (3 - objDrugEffect.Speed == 0)
						strbldDescription.Append("Speed: Immediate").AppendLine();
					else
						strbldDescription.Append("Speed: ").Append(3 - objDrugEffect.Speed).Append(" combat turns").AppendLine();
				}

				if (objDrugEffect.CrashDamage != 0)
					strbldDescription.Append("Crash Effect: ").Append(objDrugEffect.CrashDamage).Append("S damage, unresisted").AppendLine();

				strbldDescription.Append("Addiction rating: ").Append(AddictionRating * (intLevel + 1)).AppendLine();
				strbldDescription.Append("Addiction threshold: ").Append(AddictionThreshold * (intLevel + 1)).AppendLine();
				strbldDescription.Append("Cost: ").Append((CostPerLevel * (intLevel + 1)).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)).Append("¥").AppendLine();
				strbldDescription.Append("Availability: ").Append(Availability).AppendLine();
			}
			else
			{
				strbldDescription.Append("Addiction rating: ").Append(AddictionRating).Append(" per level").AppendLine();
				strbldDescription.Append("Addiction threshold: ").Append(AddictionThreshold).Append(" per level").AppendLine();
				strbldDescription.Append("Cost: ").Append(CostPerLevel.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)).Append("¥ per level").AppendLine();
				strbldDescription.Append("Availability: ").Append(Availability).Append(" per level").AppendLine();
			}

			return strbldDescription.ToString();
		}
		#endregion
	}
	/// <summary>
	/// Drug Effect
	/// </summary>
	public class DrugEffect : Object
	{
	    public DrugEffect()
		{
			Attributes = new Dictionary<string, int>();
			Limits = new Dictionary<string, int>();
			Qualities = new List<string>();
			Infos = new List<string>();
		}

		public Dictionary<string, int> Attributes { get; }

	    public Dictionary<string, int> Limits { get; }

	    public List<string> Qualities { get; }

	    public List<string> Infos { get; }

	    public int Initiative { get; set; } = 0;

	    public int InitiativeDice { get; set; } = 0;

	    public int CrashDamage { get; set; } = 0;

	    public int Speed { get; set; } = 0;

	    public int Duration { get; set; } = 0;

        public int Level { get; set; }
	}
}
