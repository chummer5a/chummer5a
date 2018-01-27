using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// A Focus.
    /// </summary>
    public class Focus : IHasInternalId, IHasName
    {
        private Guid _guiID;
        private readonly Character _objCharacter;
        private string _strName = string.Empty;
        private Guid _guiGearId;
        private int _intRating;

        #region Constructor, Create, Save, and Load Methods
        public Focus(Character objCharacter)
        {
            // Create the GUID for the new Focus.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("focus");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("gearid", _guiGearId.ToString("D"));
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Guid.TryParse(objNode["guid"].InnerText, out _guiID);
            _strName = objNode["name"].InnerText;
            _intRating = Convert.ToInt32(objNode["rating"].InnerText);
            Guid.TryParse(objNode["gearid"].InnerText, out _guiGearId);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        public string DisplayName { get; set; }

        /// <summary>
        /// Foci's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// GUID of the linked Gear.
        /// TODO: Replace this with a pointer to the Gear instead of having to do lookups.
        /// </summary>
        public string GearId
        {
            get => _guiGearId.ToString("D");
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiGearId = guiTemp;
            }
        }

        /// <summary>
        /// Rating of the Foci.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
        }
        #endregion
    }
}
