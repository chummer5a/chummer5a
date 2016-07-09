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
			foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("effects/level"))
			{
				DrugEffect objDrugEffect = new DrugEffect();
				foreach (XmlNode objXmlEffect in objXmlLevel.SelectNodes("*"))
				{
					string effectName;
					int effectValue;
					objXmlEffect.TryGetField("name", out effectName, null);
					objXmlEffect.TryGetField("value", out effectValue, 1);
					switch (objXmlEffect.Name)
					{
						case "attribute":
							if (effectName != null)
								objDrugEffect.attributes[effectName] = effectValue;
							break;
						case "limit":
							if (effectName != null)
								objDrugEffect.limits[effectName] = effectValue;
							break;
						case "quality":
							objDrugEffect.qualities.Add(objXmlEffect.InnerText);
							break;
						case "info":
							objDrugEffect.infos.Add(objXmlEffect.InnerText);
							break;
						case "initiative":
							objDrugEffect.ini = int.Parse(objXmlEffect.InnerText);
							break;
						case "initiativedice":
							objDrugEffect.iniDice = int.Parse(objXmlEffect.InnerText);
							break;
						case "crashdamage":
							objDrugEffect.crashDamage = int.Parse(objXmlEffect.InnerText);
							break;
						case "speed":
							objDrugEffect.speed = int.Parse(objXmlEffect.InnerText);
							break;
						case "duration":
							objDrugEffect.duration = int.Parse(objXmlEffect.InnerText);
							break;
						default:
							Log.Warning(info: string.Format("Unknown drug effect %s in component %s", objXmlEffect.Name, effectName));
							break;
					}
				}
				Effects.Add(objDrugEffect);
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
				objXmlWriter.WriteStartElement("drugcomponents");
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
			get
			{
				return _guiID;
			}
			set
			{
				_guiID = value;
			}
		}

		/// <summary>
		/// Internal identifier which will be used to identify this item.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Grade of the Drug.
		/// </summary>
		public string Grade
		{
			get
			{
				return _strGrade;
			}
			set
			{
				_strGrade = value;
			}
		}
		/// <summary>
		/// Internal identifier which will be used to identify this item.
		/// </summary>
		public string Description
		{
			get
			{
				return _strDescription.ToString();
			}
			set
			{
				_strDescription = value;
			}
		}

		/// <summary>
		/// Components of the Drug.
		/// </summary>
		public List<DrugComponent> Components
		{
			get
			{
				return _lstDrugComponents;
			}
			set
			{
				_lstDrugComponents = value; 
				
			}
		}

		/// <summary>
		/// Effects created by the components of the Drug.
		/// </summary>
		public List<DrugEffect> Effects
		{
			get
			{
				return _lstDrugEffects;
			}
			set
			{
				_lstDrugEffects = value;

			}
		}

		/// <summary>
		/// Name of the Drug.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Category of the Drug. 
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}

		/// <summary>
		/// Base cost of the Drug.
		/// </summary>
		public int Cost
		{
			get { return _intCost; }
			set { _intCost = value; }
		}

		/// <summary>
		/// Total cost of the Drug.
		/// </summary>
		public int TotalCost
		{
			get
			{
				return _intCost * _intQty;
			}
		}

		/// <summary>
		/// Total amount of the Drug held by the character.
		/// </summary>
		public int Quantity
		{
			get { return _intQty; }
			set { _intQty = value; }
		}

		/// <summary>
		/// Availability of the Drug.
		/// </summary>
		public int Availability
		{
			get { return _intAvailability; }
			set { _intAvailability = value; }
		}

		/// <summary>
		/// Addiction Threshold of the Drug.
		/// </summary>
		public int AddictionThreshold
		{
			get { return _intAddictionThreshold; }
			set { _intAddictionThreshold = value; }
		}

		/// <summary>
		/// Addiction Rating of the Drug.
		/// </summary>
		public int AddictionRating
		{
			get { return _intAddictionRating; }
			set { _intAddictionRating = value; }
		}

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
		#endregion
		#region Methods
		public String GenerateDescription(int level = -1)
		{
			if (level >= Effects.Count)
				return null;

			StringBuilder description = new StringBuilder();
			bool newLineFlag = false;

			description.Append(Category).Append(": ").Append(Name).AppendLine();

			if (level != -1)
			{
				var objDrugEffect = Effects.ElementAt(level);

				foreach (var objAttribute in objDrugEffect.attributes)
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

				foreach (var objLimit in objDrugEffect.limits)
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

				if (objDrugEffect.ini != 0 || objDrugEffect.iniDice != 0)
				{
					description.Append("Initiative ");
					if (objDrugEffect.ini != 0)
						description.Append(objDrugEffect.ini.ToString("+#;-#"));
					if (objDrugEffect.iniDice != 0)
						description.Append(objDrugEffect.iniDice.ToString("+#;-#"));
					description.AppendLine();
				}

				foreach (string quality in objDrugEffect.qualities)
					description.Append(quality).Append(" quality").AppendLine();
				foreach (string info in objDrugEffect.infos)
					description.Append(info).AppendLine();

				if (Category == "Custom Drug" || objDrugEffect.duration != 0)
					description.Append("Duration: 10 x ").Append(objDrugEffect.duration + 1).Append("d6 minutes").AppendLine();

				if (Category == "Custom Drug" || objDrugEffect.speed != 0)
				{
					if (3 - objDrugEffect.speed == 0)
						description.Append("Speed: Immediate").AppendLine();
					else
						description.Append("Speed: ").Append(3 - objDrugEffect.speed).Append(" combat turns").AppendLine();
				}

				if (objDrugEffect.crashDamage != 0)
					description.Append("Crash Effect: ").Append(objDrugEffect.crashDamage).Append("S damage, unresisted").AppendLine();

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
		private List<DrugEffect> _lstEffects;
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
			foreach (XmlNode objXmlLevel in objXmlData.SelectNodes("effects/level"))
			{
				DrugEffect objDrugEffect = new DrugEffect();
				foreach (XmlNode objXmlEffect in objXmlLevel.SelectNodes("*"))
				{
					string effectName;
					int effectValue;
					objXmlEffect.TryGetField("name", out effectName, null);
					objXmlEffect.TryGetField("value", out effectValue, 1);
					switch (objXmlEffect.Name)
					{
						case "attribute":
							if (effectName != null)
								objDrugEffect.attributes[effectName] = effectValue;
							break;
						case "limit":
							if (effectName != null)
								objDrugEffect.limits[effectName] = effectValue;
							break;
						case "quality":
							objDrugEffect.qualities.Add(objXmlEffect.InnerText);
							break;
						case "info":
							objDrugEffect.infos.Add(objXmlEffect.InnerText);
							break;
						case "initiative":
							objDrugEffect.ini = int.Parse(objXmlEffect.InnerText);
							break;
						case "initiativedice":
							objDrugEffect.iniDice = int.Parse(objXmlEffect.InnerText);
							break;
						case "crashdamage":
							objDrugEffect.crashDamage = int.Parse(objXmlEffect.InnerText);
							break;
						case "speed":
							objDrugEffect.speed = int.Parse(objXmlEffect.InnerText);
							break;
						case "duration":
							objDrugEffect.duration = int.Parse(objXmlEffect.InnerText);
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
				objXmlWriter.WriteStartElement("level");
				foreach (var objAttribute in objDrugEffect.attributes)
				{
					objXmlWriter.WriteStartElement("attribute");
					objXmlWriter.WriteElementString("name", objAttribute.Key);
					objXmlWriter.WriteElementString("value", objAttribute.Value.ToString());
					objXmlWriter.WriteEndElement();
				}
				foreach (var objLimit in objDrugEffect.limits)
				{
					objXmlWriter.WriteStartElement("limit");
					objXmlWriter.WriteElementString("name", objLimit.Key);
					objXmlWriter.WriteElementString("value", objLimit.Value.ToString());
					objXmlWriter.WriteEndElement();
				}
				foreach (string quality in objDrugEffect.qualities)
				{
					objXmlWriter.WriteElementString("quality", quality);
				}
				foreach (string info in objDrugEffect.infos)
				{
					objXmlWriter.WriteElementString("info", info);
				}
				if (objDrugEffect.ini != 0)
					objXmlWriter.WriteElementString("initiative", objDrugEffect.ini.ToString());
				if (objDrugEffect.iniDice != 0)
					objXmlWriter.WriteElementString("initiativedice", objDrugEffect.iniDice.ToString());
				if (objDrugEffect.duration != 0)
					objXmlWriter.WriteElementString("duration", objDrugEffect.duration.ToString());
				if (objDrugEffect.speed != 0)
					objXmlWriter.WriteElementString("speed", objDrugEffect.speed.ToString());
				if (objDrugEffect.crashDamage != 0)
					objXmlWriter.WriteElementString("crashdamage", objDrugEffect.crashDamage.ToString());
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
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
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
			set
			{
				_strName = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}
		public List<DrugEffect> DrugEffects
		{
			get { return _lstEffects; }
		}
		public int Cost
		{
			get { return _intCost; }
			set { _intCost = value; }
		}
		public int Availability
		{
			get { return _intAvailability; }
			set { _intAvailability = value; }
		}

		public int AddictionThreshold
		{
			get { return _intAddictionThreshold; }
			set { _intAddictionThreshold = value; }
		}

		public int AddictionRating
		{
			get { return _intAddictionRating; }
			set { _intAddictionRating = value; }
		}

		public int Level
		{
			get
			{
				return _intLevel;
			}
			set
			{
				_intLevel = value;
			}
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

				foreach (var objAttribute in objDrugEffect.attributes)
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

				foreach (var objLimit in objDrugEffect.limits)
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

				if (objDrugEffect.ini != 0 || objDrugEffect.iniDice != 0)
				{
					description.Append("Initiative ");
					if (objDrugEffect.ini != 0)
						description.Append(objDrugEffect.ini.ToString("+#;-#"));
					if (objDrugEffect.iniDice != 0)
						description.Append(objDrugEffect.iniDice.ToString("+#;-#"));
					description.AppendLine();
				}

				foreach (string quality in objDrugEffect.qualities)
					description.Append(quality).Append(" quality").AppendLine();
				foreach (string info in objDrugEffect.infos)
					description.Append(info).AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.duration != 0)
					description.Append("Duration: 10 x ").Append(objDrugEffect.duration + 1).Append("d6 minutes").AppendLine();

				if (_strCategory == "Custom Drug" || objDrugEffect.speed != 0)
				{
					if (3 - objDrugEffect.speed == 0)
						description.Append("Speed: Immediate").AppendLine();
					else
						description.Append("Speed: ").Append(3 - objDrugEffect.speed).Append(" combat turns").AppendLine();
				}

				if (objDrugEffect.crashDamage != 0)
					description.Append("Crash Effect: ").Append(objDrugEffect.crashDamage).Append("S damage, unresisted").AppendLine();

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
		private Dictionary<string, int> _attributes;
		private Dictionary<string, int> _limits;
		private List<string> _qualities;
		private List<string> _infos;
		public int ini = 0;
		public int iniDice = 0;
		public int crashDamage = 0;
		public int speed = 0;
		public int duration = 0;

		public DrugEffect()
		{
			_attributes = new Dictionary<string, int>();
			_limits = new Dictionary<string, int>();
			_qualities = new List<string>();
			_infos = new List<string>();
		}

		public Dictionary<string, int> attributes
		{
			get
			{
				return _attributes;
			}
		}

		public Dictionary<string, int> limits
		{
			get
			{
				return _limits;
			}
		}

		public List<string> qualities
		{
			get
			{
				return _qualities;
			}
		}

		public List<string> infos
		{
			get
			{
				return _infos;
			}
		}
	}
}