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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer.Backend.Equipment
{
    public sealed class Clip
    {
        private readonly Character _objCharacter;
        private readonly Weapon _objWeapon;

        // can be null for the internal clip
        private readonly WeaponAccessory _objAccessory;

        private Gear _objAmmoGear;

        internal Clip(Character objCharacter, WeaponAccessory objAccessory, Weapon objWeapon, Gear objGear, int intAmmoCount)
        {
            _objCharacter = objCharacter;
            _objWeapon = objWeapon;
            _objAccessory = objAccessory;
            AmmoGear = objGear;
            Ammo = intAmmoCount;
            AmmoLocation = "loaded";
        }

        /// <summary>
        /// The GUID of the weapon accessory this clip is owned by. GUID of the weapon if it's not owned by an accessory.
        /// </summary>
        internal string OwnedBy => _objAccessory?.InternalId ?? _objWeapon?.InternalId;

        internal int Ammo { get; set; }

        public string DisplayWeaponName(CultureInfo objCulture = null, string strLanguage = "")
        {
            return _objWeapon.DisplayName(objCulture, strLanguage);
        }

        public ValueTask<string> DisplayWeaponNameAsync(CultureInfo objCulture = null, string strLanguage = "", CancellationToken token = default)
        {
            return _objWeapon.DisplayNameAsync(objCulture, strLanguage, token);
        }

        public string DisplayWeaponNameShort(string strLanguage = "")
        {
            return _objWeapon.DisplayNameShort(strLanguage);
        }

        public ValueTask<string> DisplayWeaponNameShortAsync(string strLanguage = "", CancellationToken token = default)
        {
            return _objWeapon.DisplayNameShortAsync(strLanguage, token);
        }

        public string DisplayAmmoName(string strLanguage = "")
        {
            return AmmoGear?.DisplayNameShort(strLanguage)
                   ?? (Ammo > 0
                       ? LanguageManager.GetString("String_MountInternal", strLanguage)
                       : LanguageManager.GetString("String_None", strLanguage));
        }

        public Task<string> DisplayAmmoNameAsync(string strLanguage = "", CancellationToken token = default)
        {
            if (AmmoGear != null)
                return AmmoGear.DisplayNameShortAsync(strLanguage, token).AsTask();
            return Ammo > 0
                ? LanguageManager.GetStringAsync("String_MountInternal", strLanguage, token: token)
                : LanguageManager.GetStringAsync("String_None", strLanguage, token: token);
        }

        public Gear AmmoGear
        {
            get => _objAmmoGear;
            set
            {
                Gear objOldGear = Interlocked.Exchange(ref _objAmmoGear, value);
                if (objOldGear == value)
                    return;
                if (objOldGear != null)
                    objOldGear.PropertyChanged -= UpdateAmmoQuantity;
                if (value != null)
                {
                    value.LoadedIntoClip = this;
                    Ammo = Math.Min(Ammo, value.Quantity.ToInt32());
                    value.PropertyChanged += UpdateAmmoQuantity;
                }
                else
                {
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

        internal static Clip Load(XmlNode node, Character objCharacter, Weapon objWeapon, WeaponAccessory objOwnerAccessory)
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
                //Fix for older versions where ammo loaded into clips was separate from ammo lying around in the inventory
                if (objCharacter.LastSavedVersion <= new Version(5, 222, 61) && objGear != null)
                {
                    Gear objNewGear = new Gear(objCharacter);
                    objNewGear.Copy(objGear);
                    objNewGear.Quantity = intCount;
                    if (objWeapon.ParentVehicle != null)
                        objWeapon.ParentVehicle.GearChildren.Add(objNewGear);
                    else
                        objCharacter.Gear.Add(objNewGear);
                    objGear = objNewGear;
                }
                Clip objReturn = new Clip(objCharacter, objOwnerAccessory, objWeapon, objGear, intCount);
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

        internal async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (AmmoGear == null && Ammo == 0) //Don't save empty clips, we are recreating them anyway. Save those kb
                return;
            await objWriter.WriteStartElementAsync("clip", token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("name", await DisplayAmmoNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("count", Ammo.ToString(objCulture), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("location", AmmoLocation, token: token).ConfigureAwait(false);
            if (AmmoGear != null)
            {
                await objWriter.WriteElementStringAsync("id", AmmoGear.InternalId, token: token).ConfigureAwait(false);
                await objWriter.WriteStartElementAsync("ammotype", token: token).ConfigureAwait(false);

                await AmmoGear.PrintWeaponBonusEntries(objWriter, objCulture, strLanguageToPrint, true, token).ConfigureAwait(false);

                if (await AmmoGear.Children.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    // <children>
                    XmlElementWriteHelper objChildrenElement
                        = await objWriter.StartElementAsync("children", token).ConfigureAwait(false);
                    try
                    {
                        foreach (Gear objGear in await AmmoGear.Children
                                                               .DeepWhereAsync(
                                                                   x => x.Children,
                                                                   x => x.Equipped && (x.WeaponBonus != null
                                                                       || x.FlechetteWeaponBonus != null), token)
                                                               .ConfigureAwait(false))
                        {
                            await objWriter.WriteStartElementAsync("ammotype", token: token).ConfigureAwait(false);
                            await objGear
                                  .PrintWeaponBonusEntries(objWriter, objCulture, strLanguageToPrint, true, token)
                                  .ConfigureAwait(false);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // </children>
                        await objChildrenElement.DisposeAsync().ConfigureAwait(false);
                    }
                }

                // Here for Legacy reasons
                await objWriter.WriteElementStringAsync(
                    "DV", await AmmoGear.WeaponBonusDamageAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("BonusRange",
                                                        AmmoGear.WeaponBonusRange.ToString(objCulture), token: token).ConfigureAwait(false);

                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            }
            else
                await objWriter.WriteElementStringAsync(
                    "id", Guid.Empty.ToString("D", GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}
