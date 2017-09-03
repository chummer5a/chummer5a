using System;
using System.Linq;
using System.Xml;
using Chummer.Datastructures;

namespace Chummer
{
    /// <summary>
    /// Type of Specialization
    /// </summary>
    public class SkillSpecialization
    {
        private static TranslatedField<string> _translator = new TranslatedField<string>();

        private Guid _guiID;
        private string _name;
        private string _translated;
        private readonly bool _free;

        #region Constructor, Create, Save, Load, and Print Methods

        static SkillSpecialization()
        {
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument document = XmlManager.Instance.Load("skills.xml");
                XmlNodeList specList = document.SelectNodes("/chummer/*/skill/specs/spec");
                foreach (XmlNode node in specList.Cast<XmlNode>().Where(node => node.Attributes?["translate"] != null))
                {
                    _translator.Add(node.InnerText, node.Attributes?["translate"]?.InnerText);
                }
            }
        }

        public SkillSpecialization(string strName, bool free)
        {
            _translator.Write(strName, ref _name, ref _translated);
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
            objWriter.WriteElementString("name", _name);
            if(_translated != null) objWriter.WriteElementString(GlobalOptions.Instance.Language, _translated);
            if(_free) objWriter.WriteElementString("free", string.Empty);
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
                _guiID = Guid.Parse(objNode["guid"].InnerText),
                _translated = objNode[GlobalOptions.Instance.Language]?.InnerText
            };
        }

        /// Print the object's XML to the XmlWriter.        /// <summary>

        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {

            objWriter.WriteStartElement("skillspecialization");
            objWriter.WriteElementString("name", DisplayName);
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
