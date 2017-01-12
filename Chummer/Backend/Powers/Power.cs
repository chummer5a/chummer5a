using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Skills;

namespace Chummer
{
	/// <summary>
	/// An Adept Power.
	/// </summary>
	public class Power
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strExtra = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strPointsPerLevel = "0";
		private string _strAction = "";
		private decimal _intRating = 1;
		private bool _blnLevelsEnabled = false;
		private int _intMaxLevel = 0;
		private bool _blnDiscountedAdeptWay = false;
		private bool _blnDiscountedGeas = false;
		private XmlNode _nodBonus;
		private XmlNode _nodAdeptWayRequirements;
		private string _strNotes = "";
		private bool _blnDoubleCost = true;
		private bool _blnFree = false;
		private int _intFreeLevels = 0;
		private string _strAdeptWayDiscount = "0";
		private string _strBonusSource = "";
		private decimal _decFreePoints = 0;
		private List<Enhancement> _lstEnhancements = new List<Enhancement>();

		private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public Power(Character objCharacter)
		{
			// Create the GUID for the new Power.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("power");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("pointsperlevel", _strPointsPerLevel);
			objWriter.WriteElementString("adeptway", _strAdeptWayDiscount);
			objWriter.WriteElementString("action", _strAction);
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
			objWriter.WriteElementString("maxlevel", _intMaxLevel.ToString());
			objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
			objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
			objWriter.WriteElementString("bonussource", _strBonusSource);
			objWriter.WriteElementString("freepoints", _decFreePoints.ToString());
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("free", _blnFree.ToString());
			objWriter.WriteElementString("freelevels", _intFreeLevels.ToString());
			objWriter.WriteElementString("doublecost", _blnDoubleCost.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
			if (_nodAdeptWayRequirements != null)
				objWriter.WriteRaw("<adeptwayrequires>" + _nodAdeptWayRequirements.InnerXml + "</adeptwayrequires>");
			else
				objWriter.WriteElementString("adeptwayrequires", "");
			objWriter.WriteStartElement("enhancements");
			foreach (Enhancement objEnhancement in _lstEnhancements)
			{
				objEnhancement.Save(objWriter);
			}
			objWriter.WriteEndElement();
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		public void Create(XmlNode objNode, ImprovementManager _objImprovementManager)
		{
			_strName = objNode["name"].InnerText;
			_strPointsPerLevel = objNode["points"].InnerText;
			_strAdeptWayDiscount = objNode["adeptway"].InnerText;
			_blnLevelsEnabled = Convert.ToBoolean(objNode["levels"].InnerText);
			_intRating = 1;
			objNode.TryGetField("maxlevels", out _intMaxLevel, 1);
			objNode.TryGetField("discounted", out _blnDiscountedAdeptWay);
			objNode.TryGetField("discountedgeas", out _blnDiscountedGeas);
			objNode.TryGetField("bonussource", out _strBonusSource);
			objNode.TryGetField("freepoints", out _decFreePoints);
			objNode.TryGetField("action", out _strAction);
			objNode.TryGetField("source", out _strSource);
			objNode.TryGetField("page", out _strPage);
			objNode.TryGetField("doublecost", out _blnDoubleCost);
			objNode.TryGetField("notes", out _strNotes);
			_nodBonus = objNode["bonus"];
			_nodAdeptWayRequirements = objNode["adeptwayrequires"];
			if (objNode.InnerXml.Contains("enhancements"))
			{
				XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
				foreach (XmlNode nodEnhancement in nodEnhancements)
				{
					Enhancement objEnhancement = new Enhancement(_objCharacter);
					objEnhancement.Load(nodEnhancement);
					objEnhancement.Parent = this;
					_lstEnhancements.Add(objEnhancement);
				}
			}
			if (_nodBonus != null && _nodBonus.HasChildNodes)
			{
				if (
					!_objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Power, InternalId, Bonus, false,
						Convert.ToInt32(Rating), DisplayNameShort))
				{
					_objCharacter.Powers.Remove(this);
					return;
				}
				Extra = _objImprovementManager.SelectedValue;
			}
		}

		/// <summary>
		/// Load the Power from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			_strPointsPerLevel = objNode["pointsperlevel"].InnerText;
			objNode.TryGetField("action", out _strAction);
			if (objNode["adeptway"] != null)
				_strAdeptWayDiscount = objNode["adeptway"].InnerText;
			else
			{
				string strPowerName = _strName;
				if (strPowerName.Contains("("))
					strPowerName = strPowerName.Substring(0, strPowerName.IndexOf("(") - 1);
				XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
				XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
				_strAdeptWayDiscount = objXmlPower["adeptway"].InnerText;
			}
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_blnLevelsEnabled = Convert.ToBoolean(objNode["levels"].InnerText);
			_blnFree = Convert.ToBoolean(objNode["free"].InnerText);
			_intFreeLevels = Convert.ToInt32(objNode["freelevels"].InnerText);
			_intMaxLevel = Convert.ToInt32(objNode["maxlevel"].InnerText);

			objNode.TryGetField("discounted", out _blnDiscountedAdeptWay);
			objNode.TryGetField("discountedgeas", out _blnDiscountedGeas);
			objNode.TryGetField("bonussource", out _strBonusSource);
			objNode.TryGetField("freepoints", out _decFreePoints);
			objNode.TryGetField("source", out _strSource);
			objNode.TryGetField("page", out _strPage);
			objNode.TryGetField("doublecost", out _blnDoubleCost);
			objNode.TryGetField("notes", out _strNotes);
			_nodBonus = objNode["bonus"];
			_nodAdeptWayRequirements = objNode["adeptwayrequires"];
			if (objNode.InnerXml.Contains("enhancements"))
			{
				XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
				foreach (XmlNode nodEnhancement in nodEnhancements)
				{
					Enhancement objEnhancement = new Enhancement(_objCharacter);
					objEnhancement.Load(nodEnhancement);
					objEnhancement.Parent = this;
					_lstEnhancements.Add(objEnhancement);
				}
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("power");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			objWriter.WriteElementString("pointsperlevel", PointsPerLevel.ToString());
			objWriter.WriteElementString("adeptway", AdeptWayDiscount.ToString());
			objWriter.WriteElementString("rating", _blnLevelsEnabled ? _intRating.ToString() : "0");
			objWriter.WriteElementString("totalpoints", PowerPoints.ToString());
			objWriter.WriteElementString("action", DisplayAction.ToString());
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteStartElement("enhancements");
			foreach (Enhancement objEnhancement in _lstEnhancements)
			{
				objEnhancement.Print(objWriter);
			}
			objWriter.WriteEndElement();
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The Character object being used by the Power.
		/// </summary>
		public Character CharacterObject
		{
			get
			{
				return _objCharacter;
			}
		}

		/// <summary>
		/// Internal identifier which will be used to identify this Power in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Power's name.
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
		/// Extra information that should be applied to the name, like a linked CharacterAttribute.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

		/// <summary>
		/// The Enhancements currently applied to the Power.
		/// </summary>
		public List<Enhancement> Enhancements
		{
			get
			{
				return _lstEnhancements;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;

				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode["translate"] != null)
						strReturn = objNode["translate"].InnerText;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The translated name of the Power (Name + any Extra text).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;
				
				if (_strExtra != "")
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					try
					{
						if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
							strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
						else
							strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
					catch
					{
						strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Power Point cost per level of the Power.
		/// </summary>
		public decimal PointsPerLevel
		{
			get
			{
				decimal decReturn = 0;
				if (_strPointsPerLevel.StartsWith("FixedValues"))
				{
					string[] strValues = _strPointsPerLevel.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					decReturn = Convert.ToDecimal(strValues[(int) (Convert.ToDecimal(_intRating) - 1)]);
				}
				else
				{
					decReturn = Convert.ToDecimal(_strPointsPerLevel);
				}
				return decReturn;
			}
			set
			{
				_strPointsPerLevel = value.ToString();
			}
		}

		/// <summary>
		/// Power Point discount for an Adept Way.
		/// </summary>
		public decimal AdeptWayDiscount
		{
			get
			{
				decimal decReturn = 0;
				if (_strAdeptWayDiscount.StartsWith("FixedValues"))
				{
					string[] strValues = _strAdeptWayDiscount.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					decReturn = Convert.ToDecimal(strValues[(int)(Convert.ToDecimal(_intRating) - 1)]);
				}
				else
				{
					decReturn = Convert.ToDecimal(_strAdeptWayDiscount);
				}
				return decReturn;
			}
			set
			{
				_strAdeptWayDiscount = value.ToString();
			}
		}

		/// <summary>
		/// Calculated Power Point cost per level of the Power (including discounts).
		/// </summary>
		public decimal CalculatedPointsPerLevel
		{
			get
			{
				return PointsPerLevel;
			}
		}

		/// <summary>
		/// Calculate the discount that is applied to the Power.
		/// </summary>
		private decimal Discount
		{
			get {
				return _blnDiscountedAdeptWay ? AdeptWayDiscount : _decFreePoints;
			}
		}

		/// <summary>
		/// The current Rating of the Power.
		/// </summary>
		public decimal Rating
		{
			get
			{
				return _intRating;
			}
			set
			{
				_intRating = value;
			}
		}

		/// <summary>
		/// Free levels of the power.
		/// </summary>
		public int FreeLevels
		{
			get
			{
				int intReturn = 0;
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.ImprovedName == _strName)
					{
						if (objImprovement.UniqueName == _strExtra)
						{
							intReturn += objImprovement.Rating;
						}
					}
				}
				return intReturn;
			}
		}

		/// <summary>
		/// Total number of Power Points the Power costs.
		/// </summary>
		public decimal PowerPoints
		{
			get
			{
				if (_blnFree)
					return 0;
				else
				{
					decimal decReturn = (Rating - FreeLevels) * PointsPerLevel;
					decReturn -= Discount;

					return Math.Max(decReturn, 0);
				}
			}
		}

		/// <summary>
		/// Bonus source.
		/// </summary>
		public string BonusSource
		{
			get
			{
				return _strBonusSource;
			}
			set
			{
				_strBonusSource = value;
			}
		}

		/// <summary>
		/// Bonus source.
		/// </summary>
		public decimal FreePoints
		{
			get
			{
				return _decFreePoints;
			}
			set
			{
				_decFreePoints = value;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;

				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode["altpage"] != null)
						strReturn = objNode["altpage"].InnerText;
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// Whether or not Levels enabled for the Power.
		/// </summary>
		public bool LevelsEnabled
		{
			get
			{
				return _blnLevelsEnabled;
			}
			set
			{
				_blnLevelsEnabled = value;
			}
		}

		/// <summary>
		/// Maximum Level for the Power.
		/// </summary>
		public int MaxLevels
		{
			get
			{
				return _intMaxLevel;
			}
			set
			{
				_intMaxLevel = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Cost is discounted by 25% from Adept Way.
		/// </summary>
		public bool DiscountedAdeptWay
		{
			get
			{
				return _blnDiscountedAdeptWay;
			}
			set
			{
				_blnDiscountedAdeptWay = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Cost is discounted by 25% from Geas.
		/// </summary>
		public bool DiscountedGeas
		{
			get
			{
				return _blnDiscountedGeas;
			}
			set
			{
				_blnDiscountedGeas = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Point cost is doubled when an CharacterAttribute exceeds its Metatype Maximum.
		/// </summary>
		public bool DoubleCost
		{
			get
			{
				return _blnDoubleCost;
			}
			set
			{
				_blnDoubleCost = value;
			}
		}

		/// <summary>
		/// Total levels of the Power.
		/// </summary>
		public decimal Levels
		{
			get
			{
				decimal actualRating = Rating - FreeLevels;
				decimal newRating = actualRating + FreeLevels;

				if (newRating < FreeLevels)
				{
					newRating = FreeLevels;
				}

				if (newRating > Convert.ToDecimal(CharacterObject.MAG.Value))
				{
					newRating = Convert.ToDecimal(CharacterObject.MAG.Value);
				}
				return newRating;
			}
		}

		/// <summary>
		/// Action.
		/// </summary>
		public string Action
		{
			get
			{
				return _strAction;
			}
			set
			{
				_strAction = value;
			}
		}

		/// <summary>
		/// Translated Action.
		/// </summary>
		public string DisplayAction
		{
			get
			{
				string strReturn = "";

				switch (_strAction)
				{
					case "Auto":
						strReturn = LanguageManager.Instance.GetString("String_ActionAutomatic");
						break;
					case "Free":
						strReturn = LanguageManager.Instance.GetString("String_ActionFree");
						break;
					case "Simple":
						strReturn = LanguageManager.Instance.GetString("String_ActionSimple");
						break;
					case "Complex":
						strReturn = LanguageManager.Instance.GetString("String_ActionComplex");
						break;
					case "Interrupt":
						strReturn = LanguageManager.Instance.GetString("String_ActionInterrupt");
						break;
					case "Special":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationSpecial");
						break;
				}

				return strReturn;
			}
		}

		#endregion

		#region Complex Properties 

		public int TotalMaximumLevels
		{
			get
			{
				int intReturn = MaxLevels;
				if (LevelsEnabled)
				{
					intReturn = Math.Max(MaxLevels, _objCharacter.MAG.TotalValue);
				}
				if (Name == "Improved Ability (skill)")
				{
					foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
					{
						if (Name == "Improved Ability (skill)" && (Extra == objSkill.Name || (objSkill.IsExoticSkill && Extra == (objSkill.DisplayName + " (" + objSkill.Specialization + ")"))))
						{
							double intImprovedAbilityMaximum = objSkill.Rating + (objSkill.Rating / 2);
							intImprovedAbilityMaximum = Convert.ToInt32(Math.Ceiling(intImprovedAbilityMaximum));
							intReturn = Convert.ToInt32(Math.Ceiling(intImprovedAbilityMaximum));
						}
					}
				}
				if (intReturn > _objCharacter.MAG.TotalValue && !_objCharacter.IgnoreRules)
				{
					MessageBox.Show(LanguageManager.Instance.GetString("Message_PowerLevel"), LanguageManager.Instance.GetString("MessageTitle_PowerLevel"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					intReturn = _objCharacter.MAG.TotalValue;
				}
				else
				{
					// If the Bonus contains "Rating", remove the existing Improvements and create new ones.
					if (Bonus != null)
					{
						if (Bonus.InnerXml.Contains("Rating"))
						{
							CharacterObject.ObjImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Power, InternalId);
							CharacterObject.ObjImprovementManager.ForcedValue = Extra;
							CharacterObject.ObjImprovementManager.CreateImprovements(Improvement.ImprovementSource.Power, InternalId, Bonus, false, Convert.ToInt32(Rating), DisplayNameShort);
						}
					}
				}
				return intReturn;
			}
		}

		/// <summary>
		/// Whether the power can be discounted due to presence of an Adept Way. 
		/// </summary>
		public bool AdeptWayDiscountEnabled
		{
			get
			{
				bool blnReturn = false;
				if (AdeptWayDiscount == 0)
				{
					return blnReturn;
				}
				if (_nodAdeptWayRequirements != null)
				{
					XmlNodeList objXmlRequiredList = _nodAdeptWayRequirements.SelectNodes("required/oneof/quality");
					foreach (XmlNode objNode in objXmlRequiredList)
					{
						if (objNode.Attributes["extra"] != null)
						{
							blnReturn = _objCharacter.Qualities.Any(objQuality => objQuality.Name == objNode.InnerText && LanguageManager.Instance.TranslateExtra(objQuality.Extra) == objNode.Attributes["extra"].InnerText);
							if (blnReturn)
							break;
						}
						else
						{
							blnReturn = _objCharacter.Qualities.Any(objQuality => objQuality.Name == objNode.InnerText); 
							if (blnReturn)
								break;
						}
					}
				}
				if (blnReturn == false && DiscountedAdeptWay)
				{
					DiscountedAdeptWay = false;
				}
				return blnReturn;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Category { get; set; }

		/// <summary>
		/// ToolTip that shows how the Power is calculating its Modified Rating.
		/// </summary>
		public string ToolTip()
		{
			string strReturn = "";
			strReturn += $"Rating ({Rating} x {PointsPerLevel})";
			string strModifier = "";
			
			foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled))
			{
				if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower && objImprovement.ImprovedName == Name &&
				    objImprovement.UniqueName == Extra)
				{
					strModifier += $" + {_objCharacter.GetObjectName(objImprovement)} ({objImprovement.Rating})";
				}
			}

			return strReturn + strModifier;
		}

		#endregion
	}
}