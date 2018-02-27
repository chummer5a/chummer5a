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
using System.Diagnostics;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    /// <summary>
    /// A Focus.
    /// </summary>
    [DebuggerDisplay("{GearObject?.DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Focus : IHasInternalId
    {
        private Guid _guiID;
        private readonly Character _objCharacter;

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
            objWriter.WriteElementString("gearid", GearObject?.InternalId);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Focus from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            string strGearId = string.Empty;
            if (objNode.TryGetStringFieldQuickly("gearid", ref strGearId))
            {
                GearObject = _objCharacter.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGearId) ??
                             (_objCharacter.Armor.FindArmorGear(strGearId) ?? (_objCharacter.Weapons.FindWeaponGear(strGearId) ??
                                                                               (_objCharacter.Cyberware.FindCyberwareGear(strGearId) ??
                                                                                _objCharacter.Vehicles.FindVehicleGear(strGearId))));
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Focus in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Foci's name.
        /// </summary>
        public Gear GearObject { get; set; }

        /// <summary>
        /// Rating of the Foci.
        /// </summary>
        public int Rating => GearObject?.Rating ?? 0;

        #endregion
    }
}
