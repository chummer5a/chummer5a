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
using System.Linq;
using System.Xml;
using Chummer.Datastructures;

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

        /// <summary>
        /// Re-create a saved SkillSpecialization from an XmlNode;
        /// </summary>
        /// <param name = "objNode" > XmlNode to load.</param>
        public static SkillSpecialization Load(XmlNode objNode, Skill objParent)
        {
            return new SkillSpecialization(objNode["name"]?.InnerText, objNode["free"]?.InnerText == bool.TrueString, objParent)
            {
                _guiID = Guid.Parse(objNode["guid"].InnerText)
            };
        }

        /// Print the object's XML to the XmlWriter.        /// <summary>

        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
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
        public string InternalId
        {
            get { return _guiID.ToString("D"); }
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?.Attributes?["translate"]?.InnerText ?? Name;
        }

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
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
            get { return _strName; }
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
        public bool Free
        {
            get { return _strFree; }
        }

        #endregion
    }
}
