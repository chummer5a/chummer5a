using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Backend.Equipment
{
	public class Drug : Object
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strCategory = "";
		private string _strDescription;
		private List<DrugComponent> _lstDrugComponents = new List<DrugComponent>();
		private List<DrugEffect> _lstDrugEffects = new List<DrugEffect>();
		private string _strGrade = "";
		private int _intCost;
		private int _intAvailability;
		private int _intAddictionThreshold;
		private int _intAddictionRating;
		private int _intQty;
		private string _strAltName = "";
	    private readonly Dictionary<string, int> _limits = new Dictionary<string, int>();
	    private readonly List<string> _qualities = new List<string>();

	    #region Constructor, Create, Save, Load, and Print Methods
		public Drug()
		{
			// Create the GUID for the new Drug.
			_guiID = Guid.NewGuid();
		}
		public void Load(XmlNode objXmlData)
		{
			_guiID = Guid.Parse(objXmlData["guid"].InnerText);
			objXmlData.TryGetField("name", out _strName);
			objXmlData.TryGetField("category", out _strCategory);
			foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("drugcomponents/drugcomponent"))
			{
				DrugComponent c = new DrugComponent();
                c.Load(objXmlLevel);
				Components.Add(c);
			}
			objXmlData.TryGetField("availability", out _intAvailability);
			objXmlData.TryGetField("cost", out _intCost);
			objXmlData.TryGetField("quantity", out _intQty);
			objXmlData.TryGetField("rating", out _intAddictionRating);
			objXmlData.TryGetField("threshold", out _intAddictionThreshold);
			//objXmlData.TryGetField("source", out _strSource);
			//objXmlData.TryGetField("page", out _strPage);
		}

		public void Save(XmlWriter objXmlWriter)
		{
			objXmlWriter.WriteStartElement("drug");
			objXmlWriter.WriteElementString("guid", _guiID.ToString());
			objXmlWriter.WriteElementString("name", _strName);
			objXmlWriter.WriteElementString("category", _strCategory);
			objXmlWriter.WriteElementString("quantity", _intQty.ToString());
			objXmlWriter.WriteStartElement("drugcomponents");
			foreach (DrugComponent objDrugComponent in _lstDrugComponents)
			{
				objXmlWriter.WriteStartElement("drugcomponent");
					objDrugComponent.Save(objXmlWriter);
				objXmlWriter.WriteEndElement();
			}
			objXmlWriter.WriteEndElement();
			if (_intAvailability != 0)
				objXmlWriter.WriteElementString("availability", _intAvailability.ToString());
			if (_intCost != 0)
				objXmlWriter.WriteElementString("cost", _intCost.ToString());
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
		/// Internal identifier which will be used to identify this item.
		/// </summary>
		public string Description
		{
			get => _strDescription.ToString();
		    set => _strDescription = value;
		}

		/// <summary>
		/// Components of the Drug.
		/// </summary>
		public List<DrugComponent> Components
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

		/// <summary>
		/// Base cost of the Drug.
		/// </summary>
		public int Cost => Components.Sum(d => d.Cost);

		/// <summary>
		/// Total cost of the Drug.
		/// </summary>
		public int TotalCost => _intCost * _intQty;

	    /// <summary>
		/// Total amount of the Drug held by the character.
		/// </summary>
		public int Quantity
		{
			get => _intQty;
	        set => _intQty = value;
	    }

		/// <summary>
		/// Availability of the Drug.
		/// </summary>
		public int Availability => Components.Sum(d => d.Availability);

        /// <summary>
        /// Addiction Threshold of the Drug.
        /// </summary>
        public int AddictionThreshold => Components.Sum(d => d.AddictionThreshold);

        /// <summary>
        /// Addiction Rating of the Drug.
        /// </summary>
        public int AddictionRating => Components.Sum(d => d.AddictionRating);

        public Dictionary<string, int> Limits =>
           (Components.Where(d => d.ActiveDrugEffect.Limits.Count > 0)
                      .SelectMany(d => d.ActiveDrugEffect.Limits)
                      .GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value)));

        public List<string> Qualities
	    {
	        get
	        {
	            List<string> newList = new List<string>();
	            foreach (DrugComponent d in Components)
	            {
	                newList.AddRange(d.ActiveDrugEffect.Qualities);
                }

	            return newList.Distinct().ToList();
	        }
	    }

        public List<string> Infos
	    {
	        get
	        {
                List<string> newList = new List<string>();
	            foreach (DrugComponent d in Components)
	            {
	                newList.AddRange(d.ActiveDrugEffect.Infos);
                }

	            return newList.Distinct().ToList();
	        }
	    }

        public int Initiative => Components.Sum(d => d.ActiveDrugEffect.Initiative);

	    public int InitiativeDice => Components.Sum(d => d.ActiveDrugEffect.InitiativeDice);

	    public int Speed => Components.Sum(d => d.ActiveDrugEffect.Speed);

        public int Duration => Components.Sum(d => d.ActiveDrugEffect.Duration);

	    public int CrashDamage => Components.Sum(d => d.ActiveDrugEffect.CrashDamage);

        public string Notes { get; internal set; }

		/// <summary>
		/// The name of the object as it should appear on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				if (_intQty > 1)
					strReturn = _intQty + " " + strReturn;

				return strReturn;
			}
		}

	    public Dictionary<string, int> Attributes =>
	        (from d in Components
	            from de in d.DrugEffects.Where(de => de.Level == d.Level)
	            where de.Attributes.Count > 0
	            from attribute in de.Attributes
	            select attribute).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));

	    #endregion
        #region Methods
        public String GenerateDescription(int level = -1)
		{
            StringBuilder description = new StringBuilder();
			bool newLineFlag = false;

			description.Append(Category).Append(": ").Append(Name).AppendLine();

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

				description.Append("Addiction rating: ").Append(AddictionRating * (level + 1)).AppendLine();
				description.Append("Addiction threshold: ").Append(AddictionThreshold * (level + 1)).AppendLine();
				description.Append("Cost: ").Append(Cost * (level + 1)).Append("¥").AppendLine();
				description.Append("Availability: ").Append(Availability * (level + 1)).AppendLine();
			}
			else
			{
				description.Append("Addiction rating: ").Append(AddictionRating).Append(" per level").AppendLine();
				description.Append("Addiction threshold: ").Append(AddictionThreshold).Append(" per level").AppendLine();
				description.Append("Cost: ").Append(Cost).Append("¥ per level").AppendLine();
				description.Append("Availability: ").Append(Availability).Append(" per level").AppendLine();
			}

			return description.ToString();
		}
		#endregion
	}
	/// <summary>
	/// Drug Component.
	/// </summary>
	public class DrugComponent : object
	{
		private string _strName;
		private string _strCategory;
		private readonly List<DrugEffect> _lstEffects;
		private int availability = 0;
		private int cost = 0;
		private int addictionRating = 0;
		private int addictionThreshold = 0;
		private int _intLevel = 0;
		private string source;
		private int page = 0;
		private int _intCost;
		private int _intAvailability;
		private int _intAddictionThreshold;
		private int _intAddictionRating;

		public DrugComponent()
		{
			_lstEffects = new List<DrugEffect>();
		}

		#region Constructor, Create, Save, Load, and Print Methods
		public void Load(XmlNode objXmlData)
		{
			objXmlData.TryGetField("name", out _strName);
			objXmlData.TryGetField("category", out _strCategory);
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
			objXmlData.TryGetField("availability", out _intAvailability);
			objXmlData.TryGetField("cost", out _intCost);
			objXmlData.TryGetField("rating", out _intAddictionRating);
			objXmlData.TryGetField("threshold", out _intAddictionThreshold);
			//objXmlData.TryGetField("source", out _strSource);
			//objXmlData.TryGetField("page", out _strPage);
		}

		public void Save(XmlWriter objXmlWriter)
		{
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

			if (availability != 0)
				objXmlWriter.WriteElementString("availability", availability.ToString());
			if (cost != 0)
				objXmlWriter.WriteElementString("cost", cost.ToString());
			if (addictionRating != 0)
				objXmlWriter.WriteElementString("rating", addictionRating.ToString());
			if (addictionThreshold != 0)
				objXmlWriter.WriteElementString("threshold", addictionThreshold.ToString());
			if (source != null)
				objXmlWriter.WriteElementString("source", source);
			if (page != 0)
				objXmlWriter.WriteElementString("page", page.ToString());
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
				strReturn = "{0} (Level {1})".Replace("{0}", _strName).Replace("{1}", _intLevel.ToString());
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
		public List<DrugEffect> DrugEffects => _lstEffects;

	    public DrugEffect ActiveDrugEffect => DrugEffects.First(effect => effect.Level == Level);

	    public int Cost
		{
			get => _intCost;
	        set => _intCost = value;
	    }
		public int Availability
		{
			get => _intAvailability;
		    set => _intAvailability = value;
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
		#endregion
		#region Methods
		public String GenerateDescription(int level = -1)
		{
			if (level >= _lstEffects.Count)
				return null;

			StringBuilder description = new StringBuilder();
			bool newLineFlag = false;

			description.Append(_strCategory).Append(": ").Append(_strName).AppendLine();

			if (level != -1)
			{
				var objDrugEffect = _lstEffects.ElementAt(level);

				foreach (var objAttribute in objDrugEffect.Attributes)
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

				foreach (var objLimit in objDrugEffect.Limits)
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

				if (objDrugEffect.Initiative != 0 || objDrugEffect.InitiativeDice != 0)
				{
					description.Append("Initiative ");
					if (objDrugEffect.Initiative != 0)
						description.Append(objDrugEffect.Initiative.ToString("+#;-#"));
					if (objDrugEffect.InitiativeDice != 0)
						description.Append(objDrugEffect.InitiativeDice.ToString("+#;-#"));
					description.AppendLine();
				}

				foreach (string quality in objDrugEffect.Qualities)
					description.Append(quality).Append(" quality").AppendLine();
				foreach (string info in objDrugEffect.Infos)
					description.Append(info).AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.Duration != 0)
					description.Append("Duration: 10 x ").Append(objDrugEffect.Duration + 1).Append("d6 minutes").AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.Speed != 0)
				{
					if (3 - objDrugEffect.Speed == 0)
						description.Append("Speed: Immediate").AppendLine();
					else
						description.Append("Speed: ").Append(3 - objDrugEffect.Speed).Append(" combat turns").AppendLine();
				}

				if (objDrugEffect.CrashDamage != 0)
					description.Append("Crash Effect: ").Append(objDrugEffect.CrashDamage).Append("S damage, unresisted").AppendLine();

				description.Append("Addiction rating: ").Append(addictionRating * (level + 1)).AppendLine();
				description.Append("Addiction threshold: ").Append(addictionThreshold * (level + 1)).AppendLine();
				description.Append("Cost: ").Append(cost * (level + 1)).Append("¥").AppendLine();
				description.Append("Availability: ").Append(availability * (level + 1)).AppendLine();
			}
			else
			{
				description.Append("Addiction rating: ").Append(addictionRating).Append(" per level").AppendLine();
				description.Append("Addiction threshold: ").Append(addictionThreshold).Append(" per level").AppendLine();
				description.Append("Cost: ").Append(cost).Append("¥ per level").AppendLine();
				description.Append("Availability: ").Append(availability).Append(" per level").AppendLine();
			}

			return description.ToString();
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
