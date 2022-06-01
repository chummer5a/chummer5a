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
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    public class Clip
    {
        private readonly Character _objCharacter;
        private readonly Weapon _objWeapon;
        private Gear _objAmmoGear;

        internal Clip(Character objCharacter, Weapon objWeapon, Gear objGear, int intAmmoCount)
        {
            _objCharacter = objCharacter;
            _objWeapon = objWeapon;
            AmmoGear = objGear;
            Ammo = intAmmoCount;
            AmmoLocation = "loaded";
        }

        internal int Ammo { get; set; }

        public string DisplayWeaponName(CultureInfo objCulture = null, string strLanguage = "")
        {
            return _objWeapon.DisplayName(objCulture, strLanguage);
        }

        public ValueTask<string> DisplayWeaponNameAsync(CultureInfo objCulture = null, string strLanguage = "")
        {
            return _objWeapon.DisplayNameAsync(objCulture, strLanguage);
        }

        public string DisplayWeaponNameShort(string strLanguage = "")
        {
            return _objWeapon.DisplayNameShort(strLanguage);
        }

        public ValueTask<string> DisplayWeaponNameShortAsync(string strLanguage = "")
        {
            return _objWeapon.DisplayNameShortAsync(strLanguage);
        }

        public string DisplayAmmoName(string strLanguage = "")
        {
            return AmmoGear?.DisplayNameShort(strLanguage)
                   ?? LanguageManager.GetString("String_MountInternal", strLanguage);
        }

        public Task<string> DisplayAmmoNameAsync(string strLanguage = "")
        {
            return AmmoGear != null
                ? AmmoGear.DisplayNameShortAsync(strLanguage)
                : LanguageManager.GetStringAsync("String_MountInternal", strLanguage);
        }

        public Gear AmmoGear
        {
            get => _objAmmoGear;
            set
            {
                if (_objAmmoGear == value)
                    return;
                Gear objOldGear = _objAmmoGear;
                if (objOldGear != null)
                    objOldGear.PropertyChanged -= UpdateAmmoQuantity;
                if (value != null)
                {
                    _objAmmoGear = value;
                    value.LoadedIntoClip = this;
                    Ammo = Math.Min(Ammo, value.Quantity.ToInt32());
                    value.PropertyChanged += UpdateAmmoQuantity;
                }
                else
                {
                    _objAmmoGear = null;
                    Ammo = 0;
                }
                if (objOldGear != null)
                    objOldGear.LoadedIntoClip = null;
            }
        }

        private void UpdateAmmoQuantity(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Gear.Quantity))
            {
                Ammo = AmmoGear.Quantity.ToInt32();
            }
        }

        public string AmmoLocation { get; set; }

        internal static Clip Load(XmlNode node, Character objCharacter, Weapon objWeapon)
        {
            if (node == null)
                return null;
            string strAmmoGuid = string.Empty;
            int intCount = 0;
            if (node.TryGetStringFieldQuickly("id", ref strAmmoGuid)
                && !string.IsNullOrEmpty(strAmmoGuid)
                && node.TryGetInt32FieldQuickly("count", ref intCount)
                && Guid.TryParse(strAmmoGuid, out Guid guiClipId))
            {
                Gear objGear = null;
                if (guiClipId != Guid.Empty)
                {
                    objGear = objWeapon.ParentVehicle != null
                        ? objWeapon.ParentVehicle.FindVehicleGear(strAmmoGuid)
                        : objCharacter.Gear.DeepFindById(strAmmoGuid);
                }
                Clip objReturn = new Clip(objCharacter, objWeapon, objGear, intCount);
                string strTemp = string.Empty;
                if (node.TryGetStringFieldQuickly("location", ref strTemp))
                    objReturn.AmmoLocation = strTemp;
                return objReturn;
            }
            return null;
        }

        internal void Save(XmlWriter writer)
        {
            if (AmmoGear == null && Ammo == 0) //Don't save empty clips, we are recreating them anyway. Save those kb
                return;
            writer.WriteStartElement("clip");
            writer.WriteElementString("count", Ammo.ToString(GlobalSettings.InvariantCultureInfo));
            writer.WriteElementString("location", AmmoLocation);
            writer.WriteElementString(
                "id",
                AmmoGear != null
                    ? AmmoGear.InternalId
                    : Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo));
            writer.WriteEndElement();
        }

        internal async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (AmmoGear == null && Ammo == 0) //Don't save empty clips, we are recreating them anyway. Save those kb
                return;
            await objWriter.WriteStartElementAsync("clip");
            await objWriter.WriteElementStringAsync("name", await DisplayAmmoNameAsync(strLanguageToPrint));
            await objWriter.WriteElementStringAsync("count", Ammo.ToString(objCulture));
            await objWriter.WriteElementStringAsync("location", AmmoLocation);
            if (AmmoGear != null)
            {
                await objWriter.WriteElementStringAsync("id", AmmoGear.InternalId);
                await objWriter.WriteStartElementAsync("ammotype");

                await AmmoGear.PrintWeaponBonusEntries(objWriter, objCulture, strLanguageToPrint, true);
                // Here for Legacy reasons
                await objWriter.WriteElementStringAsync(
                    "DV", await AmmoGear.WeaponBonusDamageAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("BonusRange",
                                                        AmmoGear.WeaponBonusRange.ToString(objCulture));

                await objWriter.WriteEndElementAsync();
            }
            else
                await objWriter.WriteElementStringAsync(
                    "id", Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo));

            await objWriter.WriteEndElementAsync();
        }
    }
}
