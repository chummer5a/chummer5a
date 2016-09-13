using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chummer.Classes
{
	public class Option
	{

		#region Constructor Methods
		public string Save(XmlTextWriter writer)
		{
			object objOption = this;
			XmlSerializer xs = new XmlSerializer(objOption.GetType());
			MemoryStream buffer = new MemoryStream();
			xs.Serialize(buffer, objOption);
			return Encoding.ASCII.GetString(buffer.ToArray());
		}
		#endregion
		#region Methods
		

		#endregion
		#region Properties
		
		/// <summary>
		/// Proper name of the Option as used in code.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Proper name of the category in which the Option's control shows up in the Options menu. 
		/// Only needed for items that are dynamically added to Options panels.
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// Default value of the Option.
		/// </summary>
		public object Default { get; set; }

		/// <summary>
		/// Current value of the Option.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Whether the option has been modified by the user. 
		/// Typically enumerated on save by comparing Value against Default.
		/// </summary>
		public bool UserModified { get; set; }

		/// <summary>
		/// Key string that will be used to fetch the display text for the description from LanguageManager.
		/// </summary>
		public string DescriptionTag { get; set; }

		/// <summary>
		/// Key string that will be used to fetch the secondary display text for the description from LanguageManager.
		/// Used for numericupdown controls that list a multiplier on their right side.
		/// </summary>
		public string ModifierTag { get; set; }

		/// <summary>
		/// Sourceook that the rule comes from. Mostly used for Optional Rules.
		/// </summary>
		public string Sourcebook { get; set; }
		#endregion
	}
}