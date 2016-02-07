using System;
using System.Xml;

namespace Chummer
{
	/// <summary>
	/// Type of Specialization
	/// </summary>
	public class SkillSpecialization
	{
		private Guid _guiID;
		private string _strName = "";
		private readonly bool _free;

		#region Constructor, Create, Save, Load, and Print Methods

		public SkillSpecialization(string strName, bool free)
		{
			_strName = strName;
			_guiID = Guid.NewGuid();
			_free = free;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("spec");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			if(_free) objWriter.WriteElementString("free", "");
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Re-create a saved SkillSpecialization from an XmlNode;
		/// </summary>
		/// <param name = "objNode" > XmlNode to load.</param>
		public static SkillSpecialization Load(XmlNode objNode)
		{
			return new SkillSpecialization(objNode["name"].InnerText, objNode["free"] != null)
			{
				_guiID = Guid.Parse(objNode["guid"].InnerText)
			};
		}

		/// Print the object's XML to the XmlWriter.		/// <summary>

		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{

			objWriter.WriteStartElement("skillspecialization");
			objWriter.WriteElementString("name", Name);
			objWriter.WriteEndElement();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Internal identifier which will be used to identify this Spell in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get { return _guiID.ToString(); }
		}

		/// <summary>
		/// Skill Specialization's name.
		/// </summary>
		public string Name
		{
			get { return _strName; }
		}

		/// <summary>
		/// Is this a forced specialization or player entered
		/// </summary>
		public bool Free
		{
			get { return _free; } 
		}

		#endregion
	}
}