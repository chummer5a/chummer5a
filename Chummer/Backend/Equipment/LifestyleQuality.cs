using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer.Backend.Equipment
{
	public class LifestyleQuality
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strExtra = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strNotes = "";
		private bool _blnContributeToLimit = true;
		private bool _blnPrint = true;
		private int _intLP = 0;
		private int _intCost = 0;
		private int _intMultiplier = 0;
		private QualityType _objLifestyleQualityType = QualityType.Positive;
		private QualitySource _objLifestyleQualitySource = QualitySource.Selected;
		private XmlNode _nodBonus;
		private readonly Character _objCharacter;
		private string _strAltName = "";
		private string _strAltPage = "";

		#region Helper Methods
		/// <summary>
		/// Convert a string to a LifestyleQualityType.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public QualityType ConvertToLifestyleQualityType(string strValue)
		{
			switch (strValue)
			{
				case "Negative":
					return QualityType.Negative;
				case "Positive":
					return QualityType.Positive;
				default:
					return QualityType.Entertainment;
			}
		}

		/// <summary>
		/// Convert a string to a LifestyleQualitySource.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public QualitySource ConvertToLifestyleQualitySource(string strValue)
		{
			switch (strValue)
			{
				default:
					return QualitySource.Selected;
			}
		}
		#endregion

		#region Constructor, Create, Save, Load, and Print Methods
		public LifestyleQuality(Character objCharacter)
		{
			// Create the GUID for the new LifestyleQuality.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Create a LifestyleQuality from an XmlNode and return the TreeNodes for it.
		/// </summary>
		/// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
		/// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		public void Create(XmlNode objXmlLifestyleQuality, Character objCharacter, QualitySource objLifestyleQualitySource, TreeNode objNode)
		{
			_strName = objXmlLifestyleQuality["name"].InnerText;
			if (objXmlLifestyleQuality["lp"] != null)
			{
				_intLP = Convert.ToInt32(objXmlLifestyleQuality["lp"].InnerText);
			}
			else
			{
				_intLP = 0;
			}
			if (objXmlLifestyleQuality["cost"] != null)
			{
				_intCost = Convert.ToInt32(objXmlLifestyleQuality["cost"].InnerText);
			}
			_objLifestyleQualityType = ConvertToLifestyleQualityType(objXmlLifestyleQuality["category"].InnerText);
			_objLifestyleQualitySource = objLifestyleQualitySource;
			if (objXmlLifestyleQuality["print"] != null)
			{
				if (objXmlLifestyleQuality["print"].InnerText == "no")
					_blnPrint = false;
			}
			if (objXmlLifestyleQuality["contributetolimit"] != null)
			{
				if (objXmlLifestyleQuality["contributetolimit"].InnerText == "no")
					_blnContributeToLimit = false;
			}
			_strSource = objXmlLifestyleQuality["source"].InnerText;
			_strPage = objXmlLifestyleQuality["page"].InnerText;
			if (objNode.Text.Contains('('))
			{
				_strExtra = objNode.Text.Split('(')[1].TrimEnd(')');
			}
			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
				XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/LifestyleQuality[name = \"" + _strName + "\"]");
				if (objLifestyleQualityNode != null)
				{
					if (objLifestyleQualityNode["translate"] != null)
						_strAltName = objLifestyleQualityNode["translate"].InnerText;
					if (objLifestyleQualityNode["altpage"] != null)
						_strAltPage = objLifestyleQualityNode["altpage"].InnerText;
				}
			}

			// If the item grants a bonus, pass the information to the Improvement Manager.
			if (objXmlLifestyleQuality.InnerXml.Contains("<bonus>"))
			{
				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Quality, _guiID.ToString(), objXmlLifestyleQuality["bonus"], false, 1, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (objImprovementManager.SelectedValue != "")
				{
					_strExtra = objImprovementManager.SelectedValue;
					//objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
				}
			}

			// Built-In Qualities appear as grey text to show that they cannot be removed.
			if (objLifestyleQualitySource == QualitySource.BuiltIn)
				objNode.ForeColor = SystemColors.GrayText;
			objNode.Name = Name;
			objNode.Text = DisplayName;
			objNode.Tag = InternalId;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("lifestylequality");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("cost", _intCost.ToString());
			objWriter.WriteElementString("multiplier", _intMultiplier.ToString());
			objWriter.WriteElementString("lp", _intLP.ToString());
			objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
			objWriter.WriteElementString("print", _blnPrint.ToString());
			objWriter.WriteElementString("lifestylequalitytype", _objLifestyleQualityType.ToString());
			objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the CharacterAttribute from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			_intLP = Convert.ToInt32(objNode["lp"].InnerText);
			if (objNode["cost"].InnerText != "")
			{
				_intCost = Convert.ToInt32(objNode["cost"].InnerText);
			}
			if (objNode["multiplier"] != null)
			{
				_intMultiplier = Convert.ToInt32(objNode["multiplier"].InnerText);
			}
			_blnContributeToLimit = Convert.ToBoolean(objNode["contributetolimit"].InnerText);
			_blnPrint = Convert.ToBoolean(objNode["print"].InnerText);
			_objLifestyleQualityType = ConvertToLifestyleQualityType(objNode["lifestylequalitytype"].InnerText);
			_objLifestyleQualitySource = ConvertToLifestyleQualitySource(objNode["lifestylequalitysource"].InnerText);
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			_nodBonus = objNode["bonus"];
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
				XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/lifestylequalities/lifestylequality[name = \"" + _strName + "\"]");
				if (objLifestyleQualityNode != null)
				{
					if (objLifestyleQualityNode["translate"] != null)
						_strAltName = objLifestyleQualityNode["translate"].InnerText;
					if (objLifestyleQualityNode["altpage"] != null)
						_strAltPage = objLifestyleQualityNode["altpage"].InnerText;
				}
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			if (_blnPrint)
			{
				objWriter.WriteStartElement("lifestylequality");
				objWriter.WriteElementString("name", DisplayNameShort);
				objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
				objWriter.WriteElementString("lp", _intLP.ToString());
				objWriter.WriteElementString("cost", _intCost.ToString());
				string strLifestyleQualityType = _objLifestyleQualityType.ToString();
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strLifestyleQualityType + "\"]");
					if (objNode != null)
					{
						if (objNode.Attributes["translate"] != null)
							strLifestyleQualityType = objNode.Attributes["translate"].InnerText;
					}
				}
				objWriter.WriteElementString("lifestylequalitytype", strLifestyleQualityType);
				objWriter.WriteElementString("lifestylequalitytype_english", _objLifestyleQualityType.ToString());
				objWriter.WriteElementString("lifestylequalitysource", _objLifestyleQualitySource.ToString());
				objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
				objWriter.WriteElementString("page", Page);
				if (_objCharacter.Options.PrintNotes)
					objWriter.WriteElementString("notes", _strNotes);
				objWriter.WriteEndElement();
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// LifestyleQuality's name.
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
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

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
		/// LifestyleQuality Type.
		/// </summary>
		public QualityType Type
		{
			get
			{
				return _objLifestyleQualityType;
			}
			set
			{
				_objLifestyleQualityType = value;
			}
		}

		/// <summary>
		/// Source of the LifestyleQuality.
		/// </summary>
		public QualitySource OriginSource
		{
			get
			{
				return _objLifestyleQualitySource;
			}
			set
			{
				_objLifestyleQualitySource = value;
			}
		}

		/// <summary>
		/// Number of Build Points the LifestyleQuality costs.
		/// </summary>
		public int LP
		{
			get
			{
				return _intLP;
			}
			set
			{
				_intLP = value;
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
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
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
		/// Whether or not the LifestyleQuality appears on the printouts.
		/// </summary>
		public bool AllowPrint
		{
			get
			{
				return _blnPrint;
			}
			set
			{
				_blnPrint = value;
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
		#endregion
	}
}