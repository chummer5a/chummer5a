using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chummer.Classes
{
	public class Option
	{
		private string _strName = "";
		private string _strCategory = "";
		private string _strDescriptionTag = "";
		private string _strModifierTag = "";
		private string _strSourcebook = "";
		private string _strPage = "";
		private object _objDefault;
		private object _objValue;
		#region Constructor Methods
		public string Save(XmlTextWriter writer)
		{
			object objOption = this;
			XmlSerializer xs = new XmlSerializer(objOption.GetType());
			MemoryStream buffer = new MemoryStream();
			xs.Serialize(buffer, objOption);
			return Encoding.ASCII.GetString(buffer.ToArray());
		}

		public void Load(XmlNode objNode)
		{
			objNode.TryGetField("name", out _strName);
			objNode.TryGetField("category", out _strCategory);
			objNode.TryGetField("descriptiontag", out _strDescriptionTag);
			objNode.TryGetField("modifiertag", out _strModifierTag);
			objNode.TryGetField("sourcebook", out _strModifierTag);
			if (objNode["default"] != null)
			{
				_objDefault = objNode["default"].InnerText;
			}
			if (objNode["value"] != null)
			{
				_objValue = objNode["value"].InnerText;
			}
		}
		#endregion
		#region Methods


		#endregion
		#region Properties

		/// <summary>
		/// Proper name of the Option as used in code.
		/// </summary>
		public string Name
		{
			get {return _strName;}
			set { _strName = value; }
			
		}

		/// <summary>
		/// Proper name of the category in which the Option's control shows up in the Options menu. 
		/// Only needed for items that are dynamically added to Options panels.
		/// </summary>
		public string Category
		{
			get { return _strCategory; }
			set { _strCategory = value; }
		}

		/// <summary>
		/// Default value of the Option.
		/// </summary>
		public object Default
		{
			get { return _objDefault; }
			set { _objDefault = value; }
		}

		/// <summary>
		/// Current value of the Option.
		/// </summary>
		public object Value
		{
			get {return _objValue;}
			set { _objValue = value; } 
		}

		/// <summary>
		/// Whether the option has been modified by the user. 
		/// Typically enumerated on save by comparing Value against Default.
		/// </summary>
		public bool UserModified { get; set; }

		/// <summary>
		/// Key string that will be used to fetch the display text for the description from LanguageManager.
		/// </summary>
		public string DescriptionTag
		{
			get { return _strDescriptionTag; }
			set { _strDescriptionTag = value; }
		}

		/// <summary>
		/// Key string that will be used to fetch the secondary display text for the description from LanguageManager.
		/// Used for numericupdown controls that list a multiplier on their right side.
		/// </summary>
		public string ModifierTag
		{
			get { return _strModifierTag; }
			set { _strModifierTag = value; }
		}

		/// <summary>
		/// Sourceook that the rule comes from. Mostly used for Optional Rules.
		/// </summary>
		public string Sourcebook
		{
			get { return _strSourcebook; }
			set { _strSourcebook = value; }
		}
		#endregion
	}
}