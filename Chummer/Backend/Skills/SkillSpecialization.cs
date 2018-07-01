/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Xml;

namespace Chummer.Backend.Skills
{
    /// <summary>
    /// Type of Specialization
    /// </summary>
    public class SkillSpecialization : IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName;
        private readonly bool _strFree;
        private readonly Skill _objParent;

        #region Constructor, Create, Save, Load, and Print Methods
        public SkillSpecialization(string strName, bool free, Skill objParent)
        {
            _strName = LanguageManager.ReverseTranslateExtra(strName, GlobalOptions.Language);
            _guiID = Guid.NewGuid();
            _strFree = free;
            _objParent = objParent;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("spec");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("free", _strFree.ToString());
            objWriter.WriteEndElement();
        }

		static SkillSpecialization()
		{
			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument document = XmlManager.Instance.Load("skills.xml");
				XmlNodeList specList = document.SelectNodes("/chummer/*/skill/specs/spec");
				foreach (XmlNode node in specList.Cast<XmlNode>().Where(node => node.Attributes?["translate"] != null))
				{
					_translator.Add(node.InnerText, node.Attributes["translate"]?.InnerText);
				}
			}
		}

            return new SkillSpecialization(xmlNode["name"]?.InnerText, xmlNode["free"]?.InnerText == bool.TrueString, objParent)
            {
                _guiID = guiTemp
            };
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("skillspecialization");
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteEndElement();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?.Attributes?["translate"]?.InnerText ?? Name;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

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
		public string DisplayName
		{
			get { return _translator.Read(_name, ref _translated); }
		}

		/// <summary>
		/// Skill Specialization's name.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objParent?.GetNode(strLanguage)?.SelectSingleNode("specs/spec[text() = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _strName = value;
                    _objCachedMyXmlNode = null;
                }
            }
        }

        /// <summary>
        /// Is this a forced specialization or player entered
        /// </summary>
        public bool Free => _strFree;

        #endregion
    }
}

        /// <summary>
        /// Re-create a saved SkillSpecialization from an XmlNode;
        /// </summary>
        /// <param name="xmlNode">XmlNode to load.</param>
        /// <param name="objParent">Parent skill to which the specialization belongs</param>
        public static SkillSpecialization Load(XmlNode xmlNode, Skill objParent)
        {
            if (!xmlNode.TryGetField("guid",Guid.TryParse, out Guid guiTemp))
                guiTemp = Guid.NewGuid();
        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }